using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using MecabForOpenJTalk.Classes ;


namespace MecabForOpenJTalk
{
	public class Mecab : Common
	{
		private	Model		m_Model ;

		//---------------------------------------------------------------------------

		public Mecab()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_Model		= null ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 準備
		/// </summary>
		/// <param name="dn_dict"></param>
		/// <returns></returns>
		public bool Load( string tDirectory )
		{
			if( string.IsNullOrEmpty( tDirectory ) == true )
			{
				return false ;
			}
			
			Initialize() ;
			
			//----------------------------------------------------------

			m_Model = new Model() ;
			if( m_Model.Open( tDirectory ) == false )
			{
				m_Model = null ;
			}
			
			return true ;
		}

		/// <summary>
		/// 解析
		/// </summary>
		/// <param name="tText"></param>
		/// <returns></returns>
		public string[] Analyze( string tText )
		{
			if( string.IsNullOrEmpty( tText ) == true || m_Model == null )
			{
				return null ;
			}

			// 文字列の半角を全角に変換する
			tText = SmallToLarge( tText ) ;

			// 語句解析を行い結果を返す
			return m_Model.Analyze( tText ) ;
		}

		//-------------------------------------------------------------------------------------------

		// 半角を全角に統一する
		private string SmallToLarge( string tText )
		{
			int i, l = m_SmallToLarge.Length ;

			// 半角を全角に統一する
			for( i  = 0 ; i <  l ; i = i + 2 )
			{
				if( tText.Contains( m_SmallToLarge[ i ] ) == true )
				{
					// ヒットしたので変換する
					tText = tText.Replace( m_SmallToLarge[ i + 0 ], m_SmallToLarge[ i + 1 ] ) ;
				}
			}

			// コントロールコードを除外する
			List<char> tWork = new List<char>() ;

			l = tText.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 半角はありえない
				if( tText[ i ] >  0x0100 )
				{
					tWork.Add( tText[ i ] ) ;
				}
			}

			// ここで日本語(漢字)以外の仕様できない全角文字を除外したいのだが一旦保留とする


			return new string( tWork.ToArray() ) ;
		}

		private static string[] m_SmallToLarge =
		{
		   " ", "　",
		   "!", "！",
		   "\"", "”",
		   "#", "＃",
		   "$", "＄",
		   "%", "％",
		   "&", "＆",
		   "'", "’",
		   "(", "（",
		   ")", "）",
		   "*", "＊",
		   "+", "＋",
		   ",", "，",
		   "-", "－",
		   ".", "．",
		   "/", "／",
		   "0", "０",
		   "1", "１",
		   "2", "２",
		   "3", "３",
		   "4", "４",
		   "5", "５",
		   "6", "６",
		   "7", "７",
		   "8", "８",
		   "9", "９",
		   ":", "：",
		   ";", "；",
		   "<", "＜",
		   "=", "＝",
		   ">", "＞",
		   "?", "？",
		   "@", "＠",
		   "A", "Ａ",
		   "B", "Ｂ",
		   "C", "Ｃ",
		   "D", "Ｄ",
		   "E", "Ｅ",
		   "F", "Ｆ",
		   "G", "Ｇ",
		   "H", "Ｈ",
		   "I", "Ｉ",
		   "J", "Ｊ",
		   "K", "Ｋ",
		   "L", "Ｌ",
		   "M", "Ｍ",
		   "N", "Ｎ",
		   "O", "Ｏ",
		   "P", "Ｐ",
		   "Q", "Ｑ",
		   "R", "Ｒ",
		   "S", "Ｓ",
		   "T", "Ｔ",
		   "U", "Ｕ",
		   "V", "Ｖ",
		   "W", "Ｗ",
		   "X", "Ｘ",
		   "Y", "Ｙ",
		   "Z", "Ｚ",
		   "[", "［",
		   "\\", "￥",
		   "]", "］",
		   "^", "＾",
		   "_", "＿",
		   "`", "‘",
		   "a", "ａ",
		   "b", "ｂ",
		   "c", "ｃ",
		   "d", "ｄ",
		   "e", "ｅ",
		   "f", "ｆ",
		   "g", "ｇ",
		   "h", "ｈ",
		   "i", "ｉ",
		   "j", "ｊ",
		   "k", "ｋ",
		   "l", "ｌ",
		   "m", "ｍ",
		   "n", "ｎ",
		   "o", "ｏ",
		   "p", "ｐ",
		   "q", "ｑ",
		   "r", "ｒ",
		   "s", "ｓ",
		   "t", "ｔ",
		   "u", "ｕ",
		   "v", "ｖ",
		   "w", "ｗ",
		   "x", "ｘ",
		   "y", "ｙ",
		   "z", "ｚ",
		   "{", "｛",
		   "|", "｜",
		   "}", "｝",
		   "~", "～",
		   "ｳﾞ", "ヴ",
		   "ｶﾞ", "ガ",
		   "ｷﾞ", "ギ",
		   "ｸﾞ", "グ",
		   "ｹﾞ", "ゲ",
		   "ｺﾞ", "ゴ",
		   "ｻﾞ", "ザ",
		   "ｼﾞ", "ジ",
		   "ｽﾞ", "ズ",
		   "ｾﾞ", "ゼ",
		   "ｿﾞ", "ゾ",
		   "ﾀﾞ", "ダ",
		   "ﾁﾞ", "ヂ",
		   "ﾂﾞ", "ヅ",
		   "ﾃﾞ", "デ",
		   "ﾄﾞ", "ド",
		   "ﾊﾞ", "バ",
		   "ﾋﾞ", "ビ",
		   "ﾌﾞ", "ブ",
		   "ﾍﾞ", "ベ",
		   "ﾎﾞ", "ボ",
		   "ﾊﾟ", "パ",
		   "ﾋﾟ", "ピ",
		   "ﾌﾟ", "プ",
		   "ﾍﾟ", "ペ",
		   "ﾎﾟ", "ポ",
		   "｡", "。",
		   "｢", "「",
		   "｣", "」",
		   "､", "、",
		   "･", "・",
		   "ｦ", "ヲ",
		   "ｧ", "ァ",
		   "ｨ", "ィ",
		   "ｩ", "ゥ",
		   "ｪ", "ェ",
		   "ｫ", "ォ",
		   "ｬ", "ャ",
		   "ｭ", "ュ",
		   "ｮ", "ョ",
		   "ｯ", "ッ",
		   "ｰ", "ー",
		   "ｱ", "ア",
		   "ｲ", "イ",
		   "ｳ", "ウ",
		   "ｴ", "エ",
		   "ｵ", "オ",
		   "ｶ", "カ",
		   "ｷ", "キ",
		   "ｸ", "ク",
		   "ｹ", "ケ",
		   "ｺ", "コ",
		   "ｻ", "サ",
		   "ｼ", "シ",
		   "ｽ", "ス",
		   "ｾ", "セ",
		   "ｿ", "ソ",
		   "ﾀ", "タ",
		   "ﾁ", "チ",
		   "ﾂ", "ツ",
		   "ﾃ", "テ",
		   "ﾄ", "ト",
		   "ﾅ", "ナ",
		   "ﾆ", "ニ",
		   "ﾇ", "ヌ",
		   "ﾈ", "ネ",
		   "ﾉ", "ノ",
		   "ﾊ", "ハ",
		   "ﾋ", "ヒ",
		   "ﾌ", "フ",
		   "ﾍ", "ヘ",
		   "ﾎ", "ホ",
		   "ﾏ", "マ",
		   "ﾐ", "ミ",
		   "ﾑ", "ム",
		   "ﾒ", "メ",
		   "ﾓ", "モ",
		   "ﾔ", "ヤ",
		   "ﾕ", "ユ",
		   "ﾖ", "ヨ",
		   "ﾗ", "ラ",
		   "ﾘ", "リ",
		   "ﾙ", "ル",
		   "ﾚ", "レ",
		   "ﾛ", "ロ",
		   "ﾜ", "ワ",
		   "ﾝ", "ン",
		   "ﾞ", "",
		   "ﾟ", "",
		} ;
	}
}
