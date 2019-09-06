using System ;
using System.Text ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine ;

using OJT ;

namespace HTS_Engine_API
{
	public class HTS_File
	{
		private	byte[]	m_Data ;
		private	int		m_Size ;
		private	int		m_Pointer ;

		//-----------------------------------------------------------

		public const int SEEK_SET = 0 ;
		public const int SEEK_CUR = 1 ;
		public const int SEEK_END = 2 ;

		//---------------------------------------------------------

		// HTS_fopen_from_fn: wrapper for fopen
		public static HTS_File Open( string tPath )
		{
			if( OpenJTalk_StorageAccessor.Exists( tPath ) != OpenJTalk_StorageAccessor.Target.File )
			{
				return null ;
			}

			//----------------------------------

			HTS_File tNewFp = new HTS_File() ;
			
			// ここにファイルから取得したデータを格納する
			tNewFp.m_Data = OpenJTalk_StorageAccessor.Load( tPath ) ;
			tNewFp.m_Size = tNewFp.m_Data.Length ;
			tNewFp.m_Pointer = 0 ;

		   return tNewFp ;
		}

		public static HTS_File Create( string tWord )
		{
			if( string.IsNullOrEmpty( tWord ) == true )
			{
				return null ;
			}

			byte[] tData = Encoding.UTF8.GetBytes( tWord ) ;

			return Create( tData ) ;
		}

		// HTS_fopen_from_data: wrapper for fopen
		public static HTS_File Create( byte[] tData )
		{
			if( tData == null || tData.Length <= 0 )
			{
				return null ;
			}

			//----------------------------------

			HTS_File tNewFp = new HTS_File() ;
			
			// ここにファイルから取得したデータを格納する
			tNewFp.m_Data = tData ;
			tNewFp.m_Size = tNewFp.m_Data.Length ;
			tNewFp.m_Pointer = 0 ;

		   return tNewFp ;
		}

		// HTS_fopen_from_fp: wrapper for fopen
		public static HTS_File Create( HTS_File tFp, int tSize )
		{
			if( tFp == null || tSize <= 0 )
			{
				return null ;
			}
			
			//----------------------------------

			if( ( tFp.m_Size - tFp.m_Pointer ) <  tSize )
			{
				// 足りない(失敗)
				tFp.m_Pointer = tFp.m_Size ;
				return null ;
			}
				
			HTS_File tNewFp = new HTS_File() ;
			tNewFp.m_Data = new byte[ tSize ] ;
			tNewFp.m_Size = tNewFp.m_Data.Length ;
			tNewFp.m_Pointer = 0 ;

			Array.Copy( tFp.m_Data, tFp.m_Pointer, tNewFp.m_Data, 0, tSize ) ;

			tFp.m_Pointer += tSize ;

			return tNewFp ;
		}

		private void Free()
		{
			m_Data		= null ;
			m_Size		= 0 ;
			m_Pointer	= 0 ;
		}

		public void Close()
		{
			Free() ;
		}

		//---------------------------------------------------------------------------

		// HTS_get_token: get token from file pointer (separators are space, tab, and line break)
		public string GetToken()
		{
			byte c ;
			
			if( Eof() == true )
			{
				return null ;
			}

			c = Getc() ;

			// 0x0D の可能性もある
			while( c == ' ' || c == 0x0A || c == '\t' )
			{
				if( Eof() == true )
				{
					return null ;
				}

				c = Getc() ;
			}
			
			List<byte> tToken = new List<byte>() ;

			// 0x0D の可能性もある
			while( c != ' ' && c != 0x0A && c != '\t' )
			{
				tToken.Add( c ) ;
				if( Eof() == true )
				{
					break ;
				}

				c = Getc() ;
			}

			if( tToken.Count == 0 )
			{
				return "" ;
			}
			
			return Encoding.UTF8.GetString( tToken.ToArray() ) ;
		}


		// 区切りが見つかるまでの文字列を取得する
		public string GetTokenFromFpWithSeparator( byte tSeparator )
		{
			if( m_Data == null || m_Data.Length <= 0 || m_Pointer >= m_Data.Length )
			{
				return null ;
			}

			int p = m_Pointer ;

			int i, l = m_Data.Length - p ;
			for( i  = p ; i <  l ; i ++ )
			{
				if( m_Data[ i ] == tSeparator )
				{
					// 区切り記号発見
					break ;
				}
			}

			if( i >= l )
			{
				// 区切り記号を発見できず
				return null ;
			}

			string s = Encoding.UTF8.GetString( m_Data, p, i - p ) ;

			m_Pointer = i + 1 ;

			return s ;
		}

		// HTS_get_pattern_token: get pattern token (single/double quote can be used)
		public string GetPatternToken()
		{
			byte c ;
			bool tSQuote = false, tDQuote = false ;
			
			if( m_Data == null || m_Data.Length <= 0 )
			{
				return "" ;
			}

			if( Eof() == true )
			{
				return "" ;
			}

			c = Getc() ;
			
			// 空白または改行の場合は空読みする
			while( c == ' ' || c == 0x0A )
			{
				if( Eof() == true )
				{
					return "" ;
				}
				
				c = Getc() ;
			}
			
			// 空白または改行以外が見つかった

			if( c == '\'' )
			{
				// シングルクォートが見つかった
				if( Eof() == true )
				{
					return "" ;
				}

				c = Getc() ;	// シングルクォートの次の文字

				tSQuote = true ;
			}
			
			if( c == '\"' )
			{
				// ダブルクォートが見つかった
				if( Eof() == true )
				{
					return "" ;
				}

				c = Getc() ;	// ダブルクォートの次の文字

				tDQuote = true ;
			}
			
			if( c == ',' )
			{
				// カンマ区切りが見つかった
				return "," ;
			}

			List<byte> tWord = new List<byte>() ;

			while( true )
			{
				tWord.Add( c ) ;

				if( Eof() == true )
				{
					if( tSQuote == false && tDQuote == false )
					{
						// 終端で終了
						break ;
					}
					else
					{
						// エラー
						return "" ;
					}
				}

				c = Getc() ;

				if( tSQuote == true && c == '\'' )
				{
					// シングルクォート終了
					break ;
				}

				if( tDQuote == true && c == '\"' )
				{
					// ダブルクォート終了
					break ;
				}

				if( tSQuote == false && tDQuote == false )
				{
					if( c == ' ' || c == 0x0A )
					{
						// 空白・改行で終了
						break ;
					}
				}
			}
			
			if( tWord.Count == 0 )
			{
				return "" ;
			}

			return Encoding.UTF8.GetString( tWord.ToArray() ) ;
		}

		// HTS_fgetc: wrapper for fgetc
		public byte Getc()
		{
			// 後でファイルから直読みにした場合は修正する
			byte c = m_Data[ m_Pointer ] ;
			m_Pointer ++ ;
			return c ;
		}

		// HTS_feof: wrapper for feof
		public bool Eof()
		{
			// 後でファイルから直読みにした場合は修正する
			if( m_Pointer >= m_Size )
			{
				return true ;
			}

			if( m_Pointer == ( m_Size - 1 ) )
			{
				// 一番最後の場合
				byte c = m_Data[ m_Pointer ] ;
				if( c == ' ' || c == 0x0A || c == '\t' || c == 0 )
				{
					m_Pointer ++ ;
					return true ;	// 最後が終端記号でもＥｏｆとみなす
				}
			}

			return false ;
		}

		// HTS_ftell: rapper for ftell
		public int Tell()
		{
			return m_Pointer ;
		}

		// HTS_fseek: wrapper for fseek
		public int Seek( int tOffset, int tOrigin )
		{
			if( tOrigin == SEEK_SET )
			{
				m_Pointer = tOffset ;
			}
			else
			if( tOrigin == SEEK_CUR )
			{
				m_Pointer += tOffset ;
			}
			else
			if( tOrigin == SEEK_END )
			{
				m_Pointer = m_Size + tOffset ;
			}
			else
			{
				return 1 ;
			}

			return 0 ;
		}

		// HTS_fread_little_endian: fread with byteswap
		public int ReadLittleEndian( int[] array, int size, int n )
		{
			if( size == 0 || n == 0 )
			{
				return 0 ;
			}

			int i, b, p ;
			int v ;

			byte[] d = m_Data ;
			p = m_Pointer ;

			for( i  = 0 ; i <  n ; i ++ )
			{
				v = 0 ;
				for( b  = 0 ; b <  size ; b ++ )
				{
					v = v | ( ( ( int )d[ p + b ] & 0xFF ) << ( b * 8 ) ) ;
				}
				p = p + size ;
				array[ i ] = v ;
			}

			m_Pointer = p ;

			return n ;
		}

		// HTS_fread_little_endian: fread with byteswap
		public int ReadLittleEndian( float[] array, int size, int n )
		{
			if( size != sizeof( float ) || n == 0 )
			{
				return 0 ;
			}

			int i, b, p ;
			byte[] w = new byte[ size ] ;

			byte[] d = m_Data ;
			p = m_Pointer ;

			if( BitConverter.IsLittleEndian == true )
			{
				for( i  = 0 ; i <  n ; i ++ )
				{
					array[ i ] = BitConverter.ToSingle( d, p ) ;
					p = p + size ;
				}
			}
			else
			{
				for( i  = 0 ; i <  n ; i ++ )
				{
					for( b  = 0 ; b <  size ; b ++ )
					{
						w[ size - 1 - b ] = d[ p + b ] ;
					}
					array[ i ] = BitConverter.ToSingle( w, 0 ) ;
					p = p + size ;
				}
			}

			m_Pointer = p ;

			return n ;
		}

	}
}

