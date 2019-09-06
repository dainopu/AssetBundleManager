using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommon
	{
		private	JPCommonNode	m_Head ;
		private	JPCommonNode	m_Tail ;
		private	JPCommonLabel	m_Label ;

		public JPCommon()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_Head	= null ;
			m_Tail	= null ;
			m_Label	= null ;
		}

		public void Refresh()
		{
			Initialize() ;
		}

		//-------------------------------------------------------------------------------------------

		public string[] Analyze( NJD njd )
		{
			NJDNode tINode ;
			JPCommonNode tJNode ;
			string tToken ;
			
			for( tINode  = njd.head ; tINode != null ; tINode = tINode.next )
			{
//				Debug.LogWarning( "Node:" + inode.Word ) ;
//				Debug.LogWarning( "Pron:" + inode.Pron ) ;


				tJNode = new JPCommonNode() ;

				tJNode.Pron = tINode.Pron ;

				ConvertPos( out tToken, tINode.Pos, tINode.PosGroup1, tINode.PosGroup2, tINode.PosGroup3 ) ;
				tJNode.Pos = tToken ;

				ConvertCType( out tToken, tINode.CType ) ;
				tJNode.CType = tToken ;

				ConvertCForm( out tToken, tINode.CForm ) ;
				tJNode.CForm = tToken ;

				tJNode.Acc = tINode.Acc ;
				tJNode.ChainFlag = tINode.ChainFlag ;

				Push( tJNode ) ;
			}

			MakeLabel() ;

			//----------------------------------------------------------

			if( GetLabelSize() <= 2 )
			{
				return null ;
			}

			return GetLabelFeature() ;
		}

		private void ConvertPos( out string buff, string pos, string pos_group1, string pos_group2, string pos_group3 )
		{
			int i ;
			
			for( i  = 0 ; njd2jpcommon_pos_list[ i ] != null ; i += 5 )
			{
				if
				(
					njd2jpcommon_pos_list[ i     ] == pos        &&
					njd2jpcommon_pos_list[ i + 1 ] == pos_group1 &&
					njd2jpcommon_pos_list[ i + 2 ] == pos_group2 &&
					njd2jpcommon_pos_list[ i + 3 ] == pos_group3
				)
				{
					buff = njd2jpcommon_pos_list[ i + 4 ] ;
					return ;
				}
			}

			Debug.LogError( "WARING: convert_pos() in njd2jpcommon.c: " + pos + " " + pos_group1 + " " + pos_group2 + " " + pos_group3 + " are not appropriate POS." ) ;

			buff = njd2jpcommon_pos_list[ 4 ] ;
		}

		private void ConvertCType( out string buff, string ctype )
		{
			int i ;
			
			for( i  = 0 ; njd2jpcommon_ctype_list[ i ] != null ; i += 2 )
			{
				if( njd2jpcommon_ctype_list[ i ] == ctype )
				{
					buff = njd2jpcommon_ctype_list[ i + 1 ] ;
					return ;
				}
			}

			Debug.LogError( "WARING: convert_ctype() in njd2jpcommon.c: " + ctype + " is not appropriate conjugation type." ) ;

			buff = njd2jpcommon_ctype_list[ 1 ] ;
		}

		private void ConvertCForm( out string buff, string cform )
		{
			int i ;
			
			for( i  = 0 ; njd2jpcommon_cform_list[ i ] != null ; i += 2 )
			{
				if( njd2jpcommon_cform_list[ i ] == cform )
				{
					buff = njd2jpcommon_cform_list[ i + 1 ] ;
					return ;
				}
			}

			Debug.LogError( "WARING: convert_cform() in njd2jpcommon.c: " + cform +" is not appropriate conjugation form." ) ;

			buff = njd2jpcommon_cform_list[ 1 ] ;
		}

		//---------------------------------------------------------------------------

		private void Push( JPCommonNode tNode )
		{
			if( m_Head == null )
			{
				m_Head  = tNode ;
			}
			else
			{
				m_Tail.next = tNode ;
				tNode.prev = m_Tail ;
			}
			m_Tail = tNode ;
		}

		private void MakeLabel()
		{
			JPCommonNode tNode = m_Head ;
			
			// initialize
			if( m_Label != null )
			{
				m_Label.Clear() ;
			}
			else
			{
				m_Label = new JPCommonLabel() ;
			}
			
			m_Label.Initialize() ;
			
			// push word
			for( tNode = m_Head ; tNode != null ; tNode = tNode.next )
			{
				m_Label.Push
				(
					tNode.Pron,
					tNode.Pos,
					tNode.CType,
					tNode.CForm,
					tNode.Acc,
					tNode.ChainFlag
				) ;
			}
			
			// make label
			m_Label.make() ;
		}

		public int GetLabelSize()
		{
			if( m_Label == null )
			{
				return 0 ;
			}

			return m_Label.GetSize() ;
		}

		public string[] GetLabelFeature()
		{
			if( m_Label == null )
			{ 
				return null ;
			}

			return m_Label.GetFeature() ;
		}

		//---------------------------------------------------------------------------

		private static string[] njd2jpcommon_pos_list =
		{
			"その他", "間投", "*", "*", "その他",
			"フィラー", "*", "*", "*", "感動詞",
			"感動詞", "*", "*", "*", "感動詞",
			"記号", "*", "*", "*", "記号",
			"記号", "アルファベット", "*", "*", "記号",
			"記号", "一般", "*", "*", "記号",
			"記号", "括弧開", "*", "*", "記号",
			"記号", "括弧閉", "*", "*", "記号",
			"記号", "句点", "*", "*", "記号",
			"記号", "空白", "*", "*", "記号",
			"記号", "読点", "*", "*", "記号",
			"形容詞", "自立", "*", "*", "形容詞",
			"形容詞", "接尾", "*", "*", "接尾辞-形容詞的",
			"形容詞", "非自立", "*", "*", "形容詞",
			"助詞", "格助詞", "一般", "*", "助詞-格助詞",
			"助詞", "格助詞", "引用", "*", "助詞-格助詞",
			"助詞", "格助詞", "連語", "*", "助詞-格助詞",
			"助詞", "係助詞", "*", "*", "助詞-係助詞",
			"助詞", "終助詞", "*", "*", "助詞-終助詞",
			"助詞", "接続助詞", "*", "*", "助詞-接続助詞",
			"助詞", "特殊", "*", "*", "助詞-その他",
			"助詞", "副詞化", "*", "*", "助詞-その他",
			"助詞", "副助詞", "*", "*", "助詞-副助詞",
			"助詞", "副助詞／並立助詞／終助詞", "*", "*", "助詞-その他",
			"助詞", "並立助詞", "*", "*", "助詞-その他",
			"助詞", "連体化", "*", "*", "助詞-その他",
			"助動詞", "*", "*", "*", "助動詞",
			"接続詞", "*", "*", "*", "接続詞",
			"接頭詞", "形容詞接続", "*", "*", "接頭辞",
			"接頭詞", "数接続", "*", "*", "接頭辞",
			"接頭詞", "動詞接続", "*", "*", "接頭辞",
			"接頭詞", "名詞接続", "*", "*", "接頭辞",
			"動詞", "自立", "*", "*", "動詞",
			"動詞", "接尾", "*", "*", "接尾辞-動詞的",
			"動詞", "非自立", "*", "*", "動詞-非自立",
			"副詞", "*", "*", "*", "副詞",
			"副詞", "一般", "*", "*", "副詞",
			"副詞", "助詞類接続", "*", "*", "副詞",
			"名詞", "サ変接続", "*", "*", "名詞-サ変接続",
			"名詞", "ナイ形容詞語幹", "*", "*", "名詞-普通名詞",
			"名詞", "一般", "*", "*", "名詞-普通名詞",
			"名詞", "引用文字列", "*", "*", "名詞-普通名詞",
			"名詞", "形容動詞語幹", "*", "*", "形状詞",
			"名詞", "固有名詞", "一般", "*", "名詞-固有名詞",
			"名詞", "固有名詞", "人名", "一般", "名詞-固有名詞",
			"名詞", "固有名詞", "人名", "姓", "名詞-固有名詞",
			"名詞", "固有名詞", "人名", "名", "名詞-固有名詞",
			"名詞", "固有名詞", "組織", "*", "名詞-固有名詞",
			"名詞", "固有名詞", "地域", "一般", "名詞-固有名詞",
			"名詞", "固有名詞", "地域", "国", "名詞-固有名詞",
			"名詞", "数", "*", "*", "名詞-数詞",
			"名詞", "接続詞的", "*", "*", "名詞-普通名詞",
			"名詞", "接尾", "サ変接続", "*", "接尾辞-名詞的",
			"名詞", "接尾", "一般", "*", "接尾辞-名詞的",
			"名詞", "接尾", "形容動詞語幹", "*", "接尾辞-形状詞的",
			"名詞", "接尾", "助数詞", "*", "接尾辞-名詞的",
			"名詞", "接尾", "助動詞語幹", "*", "接尾辞-名詞的",
			"名詞", "接尾", "人名", "*", "接尾辞-名詞的",
			"名詞", "接尾", "地域", "*", "接尾辞-名詞的",
			"名詞", "接尾", "特殊", "*", "接尾辞-名詞的",
			"名詞", "接尾", "副詞可能", "*", "接尾辞-名詞的",
			"名詞", "代名詞", "一般", "*", "代名詞",
			"名詞", "代名詞", "縮約", "*", "代名詞",
			"名詞", "動詞非自立的", "*", "*", "名詞-普通名詞",
			"名詞", "特殊", "助動詞語幹", "*", "名詞-普通名詞",
			"名詞", "非自立", "一般", "*", "名詞-非自立",
			"名詞", "非自立", "形容動詞語幹", "*", "名詞-非自立",
			"名詞", "非自立", "助動詞語幹", "*", "名詞-非自立",
			"名詞", "非自立", "副詞可能", "*", "名詞-非自立",
			"名詞", "非自立", "*", "*", "名詞-非自立",
			"名詞", "副詞可能", "*", "*", "名詞-普通名詞",
			"連体詞", "*", "*", "*", "連体詞",
			null, null, null, null, null
		} ;

		private static string[] njd2jpcommon_cform_list =
		{
			"*", "*",
			"ガル接続", "その他",
			"音便基本形", "基本形",
			"仮定形", "仮定形",
			"仮定縮約１", "仮定形",
			"仮定縮約２", "仮定形",
			"基本形", "基本形",
			"基本形-促音便", "基本形",
			"現代基本形", "基本形",
			"体言接続", "連体形",
			"体言接続特殊", "連体形",
			"体言接続特殊２", "連体形",
			"文語基本形", "基本形",
			"未然ウ接続", "未然形",
			"未然ヌ接続", "未然形",
			"未然レル接続", "未然形",
			"未然形", "未然形",
			"未然特殊", "未然形",
			"命令ｅ", "命令形",
			"命令ｉ", "命令形",
			"命令ｒｏ", "命令形",
			"命令ｙｏ", "命令形",
			"連用ゴザイ接続", "連用形",
			"連用タ接続", "連用形",
			"連用テ接続", "連用形",
			"連用デ接続", "連用形",
			"連用ニ接続", "連用形",
			"連用形", "連用形",
			null, null
		} ;
		
		private static string[] njd2jpcommon_ctype_list =
		{
			"*", "*",
			"カ変・クル", "カ行変格",
			"カ変・来ル", "カ行変格",
			"サ変・－スル", "サ行変格",
			"サ変・－ズル", "サ行変格",
			"サ変・スル", "サ行変格",
			"ラ変", "ラ行変格",
			"一段", "一段",
			"一段・クレル", "一段",
			"一段・得ル", "一段",
			"下二・カ行", "二段",
			"下二・ガ行", "二段",
			"下二・タ行", "二段",
			"下二・ダ行", "二段",
			"下二・ハ行", "二段",
			"下二・マ行", "二段",
			"下二・得", "二段",
			"形容詞・アウオ段", "形容詞",
			"形容詞・イイ", "形容詞",
			"形容詞・イ段", "形容詞",
			"五段・カ行イ音便", "五段",
			"五段・カ行促音便", "五段",
			"五段・カ行促音便ユク", "五段",
			"五段・ガ行", "五段",
			"五段・サ行", "五段",
			"五段・タ行", "五段",
			"五段・ナ行", "五段",
			"五段・バ行", "五段",
			"五段・マ行", "五段",
			"五段・ラ行", "五段",
			"五段・ラ行アル", "五段",
			"五段・ラ行特殊", "五段",
			"五段・ワ行ウ音便", "五段",
			"五段・ワ行促音便", "五段",
			"四段・サ行", "四段",
			"四段・タ行", "四段",
			"四段・ハ行", "四段",
			"四段・バ行", "四段",
			"上二・ダ行", "二段",
			"上二・ハ行", "二段",
			"特殊・ジャ", "助動詞",
			"特殊・タ", "助動詞",
			"特殊・タイ", "助動詞",
			"特殊・ダ", "助動詞",
			"特殊・デス", "助動詞",
			"特殊・ナイ", "助動詞",
			"特殊・ヌ", "助動詞",
			"特殊・マス", "助動詞",
			"特殊・ヤ", "助動詞",
			"不変化型", "不変化",
			"文語・キ", "文語助動詞",
			"文語・ケリ", "文語助動詞",
			"文語・ゴトシ", "文語助動詞",
			"文語・ナリ", "文語助動詞",
			"文語・ベシ", "文語助動詞",
			"文語・マジ", "文語助動詞",
			"文語・リ", "文語助動詞",
			"文語・ル", "文語助動詞",
			null, null
		} ;		
	}
}
