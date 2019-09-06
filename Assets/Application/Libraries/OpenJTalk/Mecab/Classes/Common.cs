using System ;
using System.Text ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;


namespace MecabForOpenJTalk.Classes
{
	public class Common
	{
		// Common
		public const string COPYRIGHT	= "MeCab: Yet Another Part-of-Speech and Morphological Analyzer\n\nCopyright(C) 2001-2012 Taku Kudo \nCopyright(C) 2004-2008 Nippon Telegraph and Telephone Corporation\n" ;
		
		public const string PACKAGE		= "open_jtalk" ;
		public const string	VERSION		= "1.01" ;

		public const uint	DIC_VERSION	= 102 ;

		public const string	SYS_DIC_FILE				= "sys.dic" ;
//		public const string	UNK_DEF_FILE				= "unk.def" ;
		public const string	UNK_DIC_FILE				= "unk.dic" ;
//		public const string	MATRIX_DEF_FILE				= "matrix.def" ;
		public const string	MATRIX_FILE					= "matrix.bin" ;
//		public const string	CHAR_PROPERTY_DEF_FILE		= "char.def" ;
		public const string	CHAR_PROPERTY_FILE			= "char.bin" ;
//		public const string	FEATURE_FILE				= "feature.def" ;
//		public const string	REWRITE_FILE				= "rewrite.def" ;
//		public const string	LEFT_ID_FILE				= "left-id.def" ;
//		public const string	RIGHT_ID_FILE				= "right-id.def" ;
//		public const string	POS_ID_FILE					= "pos-id.def" ;
//		public const string	MODEL_DEF_FILE				= "model.def" ;
//		public const string	MODEL_FILE					= "model.bin" ;
//		public const string	DICRC						= "dicrc" ;

		public const string	BOS_KEY						= "BOS/EOS" ;



		public const int	NBEST_MAX				= 512 ;
		public const int	NODE_FREELIST_SIZE		= 512 ;
		public const int	PATH_FREELIST_SIZE		= 2048 ;
		public const int	MIN_INPUT_BUFFER_SIZE	= 8192 ;
		public const int	MAX_INPUT_BUFFER_SIZE	= ( 8192 * 640 ) ;
		public const int	BUF_SIZE				= 8192 ;

		public const int	MECAB_NOR_NODE			= 0 ;
		public const int	MECAB_UNK_NODE			= 1 ;
		public const int	MECAB_BOS_NODE			= 2 ;
		public const int	MECAB_EOS_NODE			= 3 ;
		public const int	MECAB_EON_NODE			= 4 ;


		public const int	MECAB_ANY_BOUNDARY		= 0 ;
		public const int	MECAB_TOKEN_BOUNDARY	= 1 ;
		public const int	MECAB_INSIDE_TOKEN		= 2 ;


		protected	const	string	BOS_FEATURE					= "BOS/EOS,*,*,*,*,*,*,*,*" ;
		protected	const	int		DEFAULT_MAX_GROUPING_SIZE	= 24 ;

		// 文字コード
		public enum CharsetCode
		{
			EUC_JP	= 0,
			CP932	= 1,
			UTF8	= 2,
			UTF16	= 3,
			UTF16LE	= 4,
			UTF16BE	= 5,
			ASCII	= 6,
		}
		
		protected CharsetCode GetCharsetCode( string tCharset )
		{
			string tmp = tCharset ;
			tmp = tmp.ToLower() ;

			if( tmp == "sjis"  || tmp == "shift-jis" || tmp == "shift_jis" || tmp == "cp932" )
			{
				return CharsetCode.CP932 ;
			}
			else
			if( tmp == "euc"   || tmp == "euc_jp" || tmp == "euc-jp" )
			{
				return CharsetCode.EUC_JP ;
			}
			else
			if( tmp == "utf8" || tmp == "utf_8" || tmp == "utf-8" )
			{
				return CharsetCode.UTF8 ;
			}
			else
			if( tmp == "utf16" || tmp == "utf_16" || tmp == "utf-16" )
			{
				return CharsetCode.UTF16 ;
			}
			else
			if( tmp == "utf16be" || tmp == "utf_16be" || tmp == "utf-16be" )
			{
				return CharsetCode.UTF16BE ;
			}
			else
			if( tmp == "utf16le" || tmp == "utf_16le" || tmp == "utf-16le" )
			{
				return CharsetCode.UTF16LE ;
			}
			else
			if( tmp == "ascii" )
			{
				return CharsetCode.ASCII ;
			}

			return CharsetCode.UTF8 ;  // default is UTF8
		}

		protected ushort GetUnicode( CharsetCode tCode, byte[] tSentence, int tBegin, int tEnd, ref int rLength )
		{
			ushort tUnicode = 0 ;

    		switch( tCode )
			{
//					case EUC_JP		:	t = euc_to_ucs2(		begin, end, mblen ) ; break ;
				case CharsetCode.CP932	: tUnicode = CP932ToUnicode(	tSentence, tBegin, tEnd, ref rLength ) ; break ;
				case CharsetCode.UTF8	: tUnicode = UTF8ToUnicode(		tSentence, tBegin, tEnd, ref rLength ) ; break ;
//					case UTF16		:	t = utf16_to_ucs2(		begin, end, mblen ) ; break ;
//					case UTF16LE	:	t = utf16le_to_ucs2(	begin, end, mblen ) ; break ;
//					case UTF16BE	:	t = utf16be_to_ucs2(	begin, end, mblen ) ; break ;
//					case ASCII		:	t = ascii_to_ucs2(		begin, end, mblen ) ; break ;
//					default			:	t = utf8_to_ucs2(		begin, end, mblen ) ; break ;
			}

			return tUnicode ;
		}

		// １文字分の文字コードを取得する(Shift-JIS を Unicode に変換する)
//		public static ushort cp932_to_ucs2( byte[] begin, byte[] end, ref int mblen )
		protected ushort CP932ToUnicode( byte[] tSentence, int tBegin, int tEnd, ref int rLength )
		{
			int l = tEnd - tBegin ;

			// オフセット操作
			if( tSentence[ tBegin ] >= 0xA1 && tSentence[ tBegin ] <= 0xDF )
			{
				// 半角
				rLength = 1 ;
			}
			else
			if( ( tSentence[ tBegin ] & 0x80 ) != 0 && l >= 2 )
			{
				// 全角
				rLength = 2 ;
			}
			else
			{
				// 半角
				rLength = 1 ;

				return ( ushort )tSentence[ tBegin ] ;
			}

			// Unicode に変換
			string s = Encoding.GetEncoding( "shift_jis" ).GetString( tSentence, tBegin, rLength ) ;

			return s[ 0 ] ;
		}

		protected ushort UTF8ToUnicode( byte[] tSentence, int tBegin, int tEnd, ref int rLength )
		{
			int l = tEnd - tBegin ;

			// オフセット操作
			if( tSentence[ tBegin ] <  0x80 )
			{
				// 半角
				rLength = 1 ;

				return ( ushort )tSentence[ tBegin ] ;
			}
			else
			if( ( tSentence[ tBegin ] & 0xE0 ) == 0xC0 && l >= 2 )
			{
				// 全角
				rLength = 2 ;
			}
			else
			if( ( tSentence[ tBegin ] & 0xF0 ) == 0xE0 && l >= 3 )
			{
				// 全角
				rLength = 3 ;
			}
			else
			if( ( tSentence[ tBegin ] & 0xF8 ) == 0xF0 && l >= 4 )
			{
				// 全角
				rLength = 4 ;

				return 0 ;
			}
			else
			if( ( tSentence[ tBegin ] & 0xFC ) == 0xF8 && l >= 5 )
			{
				// 全角
				rLength = 5 ;

				return 0 ;
			}
			else
			if( ( tSentence[ tBegin ] & 0xFE ) == 0xFC && l >= 6 )
			{
				// 全角
				rLength = 5 ;

				return 0 ;
			}
			else
			{
				// 半角
				rLength = 1 ;

				return 0 ;
			}

			// Unicode に変換
			string s = Encoding.UTF8.GetString( tSentence, tBegin, rLength ) ;

			return s[ 0 ] ;
		}

		// 文字コードに従って文字列をバイト配列に変換する(基本的に終端記号が必要)
		protected byte[] StringToBytes( CharsetCode tCode, string s, bool tEOS = true )
		{
			byte[] b = null ;

			if( string.IsNullOrEmpty( s ) == true )
			{
				if( tEOS == false )
				{
					return new byte[ 0 ] ;
				}
				else
				{
					return new byte[]{ 0 } ;
				}
			}

			switch( tCode )
			{
				case CharsetCode.CP932 :
					b = Encoding.GetEncoding( "shift_jis" ).GetBytes( s ) ;
				break ;

				case CharsetCode.UTF8 :
					b = Encoding.UTF8.GetBytes( s ) ;
				break ;
			}

			if( tEOS == true )
			{
				Array.Resize( ref b, b.Length + 1 ) ;
				b[ b.Length - 1 ] = 0 ;
			}

			return b ;
		}

		protected int GetLength( byte[] b )
		{
			int i, l = b.Length ;
			for( i = 0 ; i <  b.Length ; i ++ )
			{
				if( b[ i ] == 0 )
				{
					break ;
				}
			}

			return i ;
		}

		// 文字コードに従ってバイト配列を文字列に変換する
		protected string BytesToString( CharsetCode tCode, byte[] b, int o = 0, int l = 0 )
		{
			string s = null ;

			if( l <= 0 )
			{
				int i ;
				for( i  = o ; i <  b.Length ; i ++ )
				{
					if( b[ i ] == 0 )
					{
						break ;
					}
					l ++ ;
				}
			}
			else
			{
				int i, c = 0 ;
				for( i  = o ; i <  ( o + l ) ; i ++ )
				{
					if( b[ i ] == 0 )
					{
						break ;
					}
					c ++ ;
				}

				if( c <  l )
				{
					l  = c ;
				}
			}

			switch( tCode )
			{
				case CharsetCode.CP932 :
					s = Encoding.GetEncoding( "shift_jis" ).GetString( b, o, l ) ;
				break ;

				case CharsetCode.UTF8 :
					s = Encoding.UTF8.GetString( b, o, l ) ;
				break ;					
			}

			return s ;
		}

	}
}

