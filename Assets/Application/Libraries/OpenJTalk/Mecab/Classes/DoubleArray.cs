using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;


using OJT ;

namespace MecabForOpenJTalk.Classes
{
	// 文字コード依存クラス
	public class DoubleArray
	{
		// サイズは８バイト
		public class Unit
		{
			public int		basis ;		// array_type_ 
			public uint		check ;		// array_u_type_
		}

		public class Word
		{
			public int	value ;
			public int	length ;

			public Word()
			{
				value	= 0 ;
				length	= 0 ;
			}

			public Word( int tValue, int tLength )
			{
				value	= tValue ;
				length	= tLength ;
			}

			public void Set( int tValue, int tLength )
			{
				value	= tValue ;
				length	= tLength ;
			}
		}


		private Unit[]	m_Array ;


		public void Clear()
		{
		}

		/// <summary>
		/// ここのデータは各文字コードであるため解析文字コードもそれに合わせる必要がある
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="ptr_o"></param>
		/// <param name="count"></param>
		/// <param name="size"></param>
		public void SetArray( byte[] tBuffer, int tOffset, int tCount, int tSize = 0 )
		{
			Clear() ;

//				array_ = reinterpret_cast<unit_t *>(ptr);
				
			// １つあたり８バイト
			int i, l = tCount / 8, o = tOffset ;
			m_Array = new Unit[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Array[ i ] = new Unit() ;

				m_Array[ i ].basis = ( int )GetUintL( tBuffer, ref o ) ;
				m_Array[ i ].check = GetUintL( tBuffer, ref o ) ;
			}
			Debug.LogWarning( "array_数:" + l ) ;

			// ↑フル展開は重いが仕方が無いか？
				

			//---------------------------------------------------------
			// ここはデバッグ表示
//			int d, lc = 0 ;
//			for( d  = 0 ; d <  l ; d ++ )
//			{
//				if( array_[ d ].base_ != 0 )
//				{
//					Debug.LogWarning( "arra_[ " + d + " ].base : " + array_[ d ].base_ ) ;
//					Debug.LogWarning( "arra_[ " + d + " ].check : " + array_[ d ].check ) ;
//
//					lc ++ ;
//				}
//
//				if( lc >= 4 )
//				{
//					break ;
//				}
//			}
			//---------------------------------------------------------

//			no_delete_ = true ;
		}


		private uint GetUintL( byte[] b, ref int p )
		{
			int i ;
			uint v = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				v = v | ( uint )( b[ p + i ] << ( i * 8 ) ) ;
			}

			p = p + 4 ;

			return v ;
		}

		/// <summary>
		/// ここも文字コードに応じたものでなければダメ
		/// </summary>
		/// <param name="key"></param>
		/// <param name="len"></param>
		/// <param name="node_pos"></param>
		/// <returns></returns>
		public Word ExactMatchSearch( byte[] tKey, int len = 0, int node_pos = 0 )
		{
			if( len == 0 )
			{
				for( int i  = 0 ; i <  tKey.Length ; i ++ )
				{
					if( tKey[ i ] == 0 )
					{
						break ;
					}
					len ++ ;
				}
			}

			// おそらく初期化
			Word result = new Word( -1, 0 ) ;

			int  b = m_Array[ node_pos ].basis ;
			uint p ;

			for( int i  = 0 ; i <  len ; ++ i )
			{
//					p = b + ( node_u_type_ )( key[ i ] ) + 1 ;
				p = ( uint )b + ( ( uint )tKey[ i ] ) + 1 ;

//					if( static_cast<array_u_type_>( b ) == array_[ p ].check )
				if( ( uint )b == m_Array[ p ].check )
				{
					b = m_Array[ p ].basis ;
				}
				else
				{
					return result ;
				}
			}

			p = ( uint )b ;
			int n = m_Array[ p ].basis ;

//				if( static_cast<uint>( b ) == array_[ p ].check && n <  0 )
			if( ( uint )b == m_Array[ p ].check && n <  0 )
			{
				result.Set( - n - 1, len ) ;
			}
				
			return result ;
		}

		public int CommonPrefixSearch( byte[] tSentence, int tKey, DoubleArray.Word[] result, int result_len, int len = 0, int node_pos = 0 )
		{
			if( len == 0 )
			{
//					len = length_func_()(key);
				for( int i  = tKey ; i <  tSentence.Length ; i ++ )
				{
					if( tSentence[ i ] == 0 )
					{
						break ;
					}
					len ++ ;
				}
			}

			// array_type   -> int
			// array_u_type -> uint

			// node_type    -> byte
			// node_u_type  -> byte

			int	b   = m_Array[ node_pos ].basis ;
			int	num	= 0 ;
			int	n ;
			uint p ;

			for( int i  = 0 ; i <  len ; ++ i )
			{
				p = ( uint )b ;  // + 0;
				n = m_Array[ p ].basis ;
				if( ( uint )b == m_Array[ p ].check && n <  0 )
				{
					// result[num] = -n-1;
					if( num <  result_len )
					{
						result[ num ].Set( - n - 1, i ) ;
					}

					++ num ;
				}

				// オフセット操作
//					p = b + ( byte )( key[ i ] ) + 1 ;
				p = ( uint )( b + ( byte )( tSentence[ tKey + i ] ) + 1 ) ;

				if( ( uint )b == m_Array[ p ].check )
				{
					b = m_Array[ p ].basis ;
				}
				else
				{
					return num ;
				}
			}

			p = ( uint )b ;
			n = m_Array[ p ].basis ;

			if( ( uint )b == m_Array[ p ].check && n <  0 )
			{
				if( num <  result_len )
				{
					result[ num ].Set( - n - 1, len ) ;
				}

				++ num ;
			}

			return num ;
		}
	}
}