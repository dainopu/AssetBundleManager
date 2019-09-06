using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using HTS_Engine_API ;


namespace MecabForOpenJTalk.Classes
{
	public class WordDictionary : Common
	{
		private Mmap<byte>		m_DMmap = new Mmap<Byte>() ;

		private Token[]			m_Token = null ;

		private byte[]			m_Feature_s ;	// string にはしない
		private int				m_Feature_o ;

		private string			m_Charset ;	// 最大32バイトの文字列

		private uint			m_Version ;
		private uint			m_Type ;
		private uint			m_Size ;

		private uint			m_LSize ;
		private uint			m_RSize ;

		private DoubleArray		m_DoubleArray  = new DoubleArray() ;


		private const uint DictionaryMagicID = 0xef718f77u ;

		//---------------------------------------------------------------------------

		public string Charset
		{
			get
			{
				return m_Charset ;
			}
		}

		public ushort Version
		{
			get
			{
				return ( ushort )m_Version ;
			}
		}

		public int Size
		{
			get
			{
				return ( int )m_Size ;
			}
		}

		public int Type
		{
			get
			{
				return ( int )m_Type ;
			}
		}

		public int LSize
		{
			get
			{
				return ( int )m_LSize ;
			}
		}

		public int RSize
		{
			get
			{
				return ( int )m_RSize ;
			}
		}


		//---------------------------------------------------------------------------

		public bool Open( string tPath )
		{
			Close() ;

			Debug.LogWarning( "=======> 辞書ファイルオープン : " + tPath ) ;

			if( m_DMmap.Open( tPath ) == false )
			{
				Debug.LogWarning( "no such file or directory: " + tPath ) ;
				return false ;
			}

			if( m_DMmap.Size <  100 )
			{
				Debug.LogWarning( "dictionary file is broken: " + tPath ) ;
				return false ;
			}

			//----------------------------------------------------------

			// 先頭のポインタを取得する
			byte[] tBuffer = m_DMmap.Data ;
			int tOffset = 0 ;

			uint tDSize ;
			uint tTSize ;
			uint tFSize ;
			uint tMagic ;
//			uint dummy ;

			tMagic = GetUintL( tBuffer, ref tOffset ) ;

			if( ( ( tMagic ^ DictionaryMagicID ) == m_DMmap.Size ) == false )
			{
				Debug.LogWarning( "dictionary file is broken: " + tPath ) ;
				return false ;
			}

			Debug.LogWarning( "辞書ファイルはリトルエンディアン:" + tMagic ) ;

			m_Version = GetUintL( tBuffer, ref tOffset ) ;
			if( ( m_Version == DIC_VERSION ) == false )
			{
				Debug.LogWarning( "incompatible version: " + m_Version ) ;
				return false ;
			}

			Debug.LogWarning( "辞書ファイルのバージョン:" + m_Version ) ;


			m_Type		= GetUintL( tBuffer, ref tOffset ) ;
			m_Size		= GetUintL( tBuffer, ref tOffset ) ;
			m_LSize		= GetUintL( tBuffer, ref tOffset ) ;
			m_RSize		= GetUintL( tBuffer, ref tOffset ) ;
			tDSize		= GetUintL( tBuffer, ref tOffset ) ;
			tTSize		= GetUintL( tBuffer, ref tOffset ) ;
			tFSize		= GetUintL( tBuffer, ref tOffset ) ;
//			dummy		= GetUintL( b, ref o ) ;
			tOffset += 4 ;	// dummy 分


//			Debug.LogWarning( "type_ :" + type_ ) ;
//			Debug.LogWarning( "lexsize_ :" + lexsize_ ) ;
//			Debug.LogWarning( "lsize_:" + lsize_ ) ;
//			Debug.LogWarning( "rsize_:" + rsize_ ) ;
//			Debug.LogWarning( "dsize :" + dsize ) ;
//			Debug.LogWarning( "tsize :" + tsize ) ;
//			Debug.LogWarning( "fsize :" + fsize ) ;
//			Debug.LogWarning( "dummy :" + dummy ) ;


			// オフセットに変更
			m_Charset = GetString( tBuffer, tOffset, 32 ) ;	// 最大３２バイト(SHIFT-JIS)
			Debug.LogWarning( "Charset : " + m_Charset ) ;

			// オフセットに変更
			tOffset += 32 ;

			// dsize は 3688
			m_DoubleArray.SetArray( tBuffer, tOffset, ( int )tDSize ) ;

			// オフセットに変更
			tOffset += ( int )tDSize ;

			// token_ 展開
			// tsize は 640
			// Token は 1 つ 16 バイト(2+2+2+2+4+4)になるはず 640 / 16 = 40
			int i, l = ( int )( tTSize / 16 ), o = tOffset ;
			m_Token = new Token[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_Token[ i ] = new Token() ;
				m_Token[ i ].Open( tBuffer, ref o ) ;
			}

			// オフセットに変更
			tOffset += ( int )tTSize ;

			// feature 展開
			m_Feature_s = tBuffer ;
			m_Feature_o = tOffset ;

			// オフセットに変更
			tOffset += ( int )tFSize ;

			if( tOffset != m_DMmap.Size )
			{
				Debug.LogWarning( "dictionary file is broken: " + tPath ) ;
				return false ;
			}

			Debug.Log( "WordDictionary.open OK:" + o ) ;

			return true ;
		}


		public void Close()
		{
			m_DMmap.Close() ;
		}

		// 文字コード非依存
		private string GetString( byte[] b, int o, int l )
		{
			int i, c = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( b[ o + i ] == 0 )
				{
					break ;
				}
				c ++ ;
			}

			if( c == 0 )
			{
				return "" ;
			}

			// 全て ASCII なので文字コードはなんでも良い
			return Encoding.UTF8.GetString( b, o, c ) ;
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

		//---------------------------------------------------------------------------

		public Token GetToken( DoubleArray.Word n, int o = 0 )
		{
			return m_Token[ ( n.value >> 8 ) + o ] ;
		}

		public int GetSize( DoubleArray.Word n )
		{
			return n.value & 0xFF ;
		}

		// 特殊：基準となるトークンから相対位置にあるトークンを取得する
		public Token GetRelativeToken( Token tToken, int tPosition )
		{
			if( tToken == null || m_Token == null || m_Token.Length == 0 )
			{
				return null ;
			}

			int i, l = m_Token.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Token[ i ] == tToken )
				{
					break ;	// 発見
				}
			}

			if( i >= l )
			{
				return null ;
			}

			i = i + tPosition ;
			if( i <  0 || i >= l )
			{
				return null ;
			}

			return m_Token[ i ] ;
		}


		public DoubleArray.Word ExactMatchSearch( byte[] tKey )
		{
			return m_DoubleArray.ExactMatchSearch( tKey ) ;
		}

		public int CommonPrefixSearch( byte[] tSentence, int tKey, int len, DoubleArray.Word[] result, int rlen )
		{
    		return m_DoubleArray.CommonPrefixSearch( tSentence, tKey, result, rlen, len ) ;
		}
		
		public byte[] GetFeature( Token tToken, out int oOffset )
		{
			// 辞書から文字列を取り出す
			oOffset = ( int )( m_Feature_o + tToken.feature ) ;
			return m_Feature_s  ;
		}
	}
}
