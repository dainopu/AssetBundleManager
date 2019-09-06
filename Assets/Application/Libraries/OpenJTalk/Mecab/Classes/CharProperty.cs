using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace MecabForOpenJTalk.Classes
{
	public class CharProperty : Common
	{
		private Mmap<byte>					m_CMmap = new Mmap<byte>() ;
		private List<byte[]>				m_Clist = new List<byte[]>() ;	// 文字コード依存

		// 全展開(65535個)するとめちゃくちゃ重い(固まる)ので逐次生成するようにする
//			private CharInfo[]					map_ = null ;
		private byte[]						m_MapBuffer = null ;
		private int							m_MapOffset = 0 ;

		private	CharsetCode					m_CharsetCode ;
		
		//---------------------------------------------------------------------------

		public CharProperty( CharsetCode tCharsetCode )
		{
			m_CharsetCode = tCharsetCode ;
		}

		public bool Open( string tDirectory )
		{
			string tPath = Path.Combine( tDirectory, CHAR_PROPERTY_FILE ).Replace( "\\", "/" ) ;

			Debug.LogWarning( "==================== CharProperty.open" ) ;

			if( m_CMmap.Open( tPath ) == false )
			{
				return false ;
			}

			// オフセット操作
			byte[] tMapBuffer = m_CMmap.Data ;
			int tMapOffset = 0 ;
				
			uint tCSize ;
			tCSize = GetUintL( tMapBuffer, ref tMapOffset ) ;

			// Unityでは別環境で作られたデータを使用するので sizeof を使うのは間違い→固定値4を使用する
			uint tFSize = 4 + ( 32 * tCSize ) + 4 * 0xffff ;

			if( tFSize != m_CMmap.Size )
			{
				return false ;
			}
			
//			Debug.LogWarning( "=======>csize = " + tCSize ) ;

			m_Clist.Clear() ;
			for( uint i  = 0 ; i <  tCSize ; ++ i )
			{
				byte[] s = GetStringBytes( tMapBuffer, ref tMapOffset, 32 ) ;
				m_Clist.Add( s ) ;

//				Debug.LogWarning( "clist:[" + i +"] = " + BytesToString( m_CharsetCode, s ) ) ;
			}

			// ビットデータである事に注意する
			int ml = ( int )( ( tFSize - tMapOffset ) / 4 ) ;
			Debug.LogWarning( "==========マップデータ数:" + ml ) ;
/*				
			// 65535 個もの数があり、ここで展開するのは無理。よって必要に応じて逐次値を取るようにする。
			map_ = new CharInfo[ ml ] ;
			for( mi  = 0 ; mi <  ml ; mi ++ )
			{
				map_[ mi ] = new CharInfo() ;
				map_[ mi ].open( b, ref o ) ;
			}*/
//				map_ = reinterpret_cast<const CharInfo *>(ptr);
/*				map_.open( b, ref o ) ;*/

			m_MapBuffer = tMapBuffer ;
			m_MapOffset = tMapOffset ;

			return true ;
		}

		public void Close()
		{
			m_CMmap.Close() ;
		}

		//--------------------------------------------------------------------------

		public int Size
		{
			get
			{
				return m_Clist.Count ;
			}
		}

		public byte[] GetName( int i )
		{
			return m_Clist[ i ] ;
		}

		//--------------------------------------------------------------------------

		// オフセット操作
//			public byte[] seekToOtherType( byte[] begin, byte[] end, CharInfo c, ref CharInfo fail, ref int mblen, ref int clen )
  		public int SeekToOtherType( byte[] tSentence, int tBegin, int tEnd, CharInfo c, ref CharInfo fail, ref int mblen, ref int clen )
		{
			// オフセット操作
//				byte[] p =  begin ;
    		int p =  tBegin ;
			clen = 0 ;

			while( p != tEnd && c.isKindOf( fail = GetCharInfo( tSentence, p, tEnd, ref mblen ) ) )
			{
				p += mblen ;
				++ clen  ;
				c = fail ;
			}

			return p ;
		}

		// オフセット操作
		public CharInfo GetCharInfo( byte[] tSentence, int tBegin, int tEnd, ref int rLength )
		{
			ushort tUnicode = GetUnicode( m_CharsetCode, tSentence, tBegin, tEnd, ref rLength ) ;

			return GetCharInfo( tUnicode ) ;
		}
		/// <summary>
		/// １文字の情報を取得する(引数は Unicode）
		/// </summary>
		/// <param name="tId"></param>
		/// <returns></returns>
		public CharInfo GetCharInfo( int tId )
		{
//			Debug.LogWarning( "文字 = " + ( char )tId ) ;

			// 逐次取得するようにする
			CharInfo tMap = new CharInfo() ;

			int tMapOffset = m_MapOffset + tId * 4 ;
			tMap.Open( m_MapBuffer, ref tMapOffset ) ;

			return tMap ;
		}

		//----------------------------------------------------------

		// 文字コード非依存
		private byte[] GetStringBytes( byte[] b, ref int o, int l )
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
				o = o + l ;
				return new byte[ 1 ]{ 0 } ;
			}

			byte[] s = new byte[ c + 1 ] ;
			Array.Copy( b, o, s, 0, c ) ;
			
			o = o + l ;

			return s ;
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


	}
}
