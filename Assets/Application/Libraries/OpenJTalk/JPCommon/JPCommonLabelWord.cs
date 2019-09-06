using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace OJT
{
	public class JPCommonLabelWord
	{
		public	string						pron ;
		public	string						pos ;
		public	string						ctype ;
		public	string						cform ;

		public	JPCommonLabelMora			head ;
		public	JPCommonLabelMora			tail ;
		public	JPCommonLabelWord			prev ;
		public	JPCommonLabelWord			next ;
		public	JPCommonLabelAccentPhrase	up ;

		public void Initialize( string pron, string pos, string ctype, string cform, JPCommonLabelMora head, JPCommonLabelMora tail, JPCommonLabelWord prev, JPCommonLabelWord next )
		{
			int i, find ;
			
			this.pron = pron ;
			for( i  = 0, find  = 0 ; jpcommon_pos_list[ i ] != null ; i += 2 )
			{
				if( jpcommon_pos_list[ i ] == pos )
				{
					find = 1 ;
					break ;
				}
			}

			if( find == 0 )
			{
				Debug.LogError( "WARNING: JPCommonLabelWord_initializel() in jpcommon_label.c: " + pos + " is unknown POS" ) ;
				i = 0 ;
			}

			this.pos =  jpcommon_pos_list[ i + 1 ] ;

			for( i  = 0, find  = 0 ; jpcommon_ctype_list[ i ] != null ; i += 2 )
			{
				if( jpcommon_ctype_list[ i ] == ctype )
				{
					find = 1 ;
					break ;
				}
			}

			if( find == 0 )
			{
				Debug.LogError( "WARNING: JPCommonLabelWord_initializel() in jpcommon_label.c: " + ctype + " is unknown conjugation type." ) ;
				i = 0 ;
			}

			this.ctype = jpcommon_ctype_list[ i + 1 ] ;

			for( i  = 0, find  = 0 ; jpcommon_cform_list[ i ] != null ; i += 2 )
			{
				if( jpcommon_cform_list[ i ] == cform )
				{
					find = 1 ;
					break ;
				}
			}

			if( find == 0 )
			{
				Debug.LogError( "WARNING: JPCommonLabelWord_initializel() in jpcommon_label.c: " + cform + " is unknown conjugation form." ) ;
				i = 0 ;
			}

			this.cform = jpcommon_cform_list[ i + 1 ] ;

			this.head = head ;
			this.tail = tail ;
			this.prev = prev ;
			this.next = next ;
		}

		public void Clear()
		{
			this.pron	= null ;
			this.pos	= null ;
			this.ctype	= null ;
			this.cform	= null ;
		}

		//---------------------------------------------------------------------------

		private static string[] jpcommon_pos_list =
		{
			"その他", "xx",
			"感動詞", "09",
			"記号", "xx",
			"形状詞", "19",
			"形容詞", "01",
			"助詞-その他", "23",
			"助詞-格助詞", "13",
			"助詞-係助詞", "24",
			"助詞-終助詞", "14",
			"助詞-接続助詞", "12",
			"助詞-副助詞", "11",
			"助動詞", "10",
			"接続詞", "08",
			"接頭辞", "16",
			"接頭辞-形状詞的", "16",
			"接頭辞-形容詞的", "16",
			"接頭辞-動詞的", "16",
			"接頭辞-名詞的", "16",
			"接尾辞-形状詞的", "15",
			"接尾辞-形容詞的", "15",
			"接尾辞-動詞的", "15",
			"接尾辞-名詞的", "15",
			"代名詞", "04",
			"動詞", "20",
			"動詞-非自立", "17",
			"副詞", "06",
			"名詞-サ変接続", "03",
			"名詞-固有名詞", "18",
			"名詞-数詞", "05",
			"名詞-非自立", "22",
			"名詞-普通名詞", "02",
			"連体詞", "07",
			"フィラー", "25",
			null, null
		} ;

		private static string[] jpcommon_cform_list =
		{
		   "*", "xx",
		   "その他", "6",
		   "仮定形", "4",
		   "基本形", "2",
		   "未然形", "0",
		   "命令形", "5",
		   "連体形", "3",
		   "連用形", "1",
		   null, null
		} ;

		private static string[] jpcommon_ctype_list =
		{
			"*", "xx",
			"カ行変格", "5",
			"サ行変格", "4",
			"ラ行変格", "6",
			"一段", "3",
			"形容詞", "7",
			"五段", "1",
			"四段", "6",
			"助動詞", "7",
			"二段", "6",
			"不変化", "6",
			"文語助動詞", "6",
			null, null
		} ;
	}
}
