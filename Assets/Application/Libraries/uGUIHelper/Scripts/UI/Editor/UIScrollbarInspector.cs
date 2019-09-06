using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIScrollbar のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIScrollbar ) ) ]
	public class UIScrollbarInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIScrollbar tTarget = target as UIScrollbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			UIScrollView tScrollViewElastic = EditorGUILayout.ObjectField( "ScrollView Elastic", tTarget.scrollViewElastic, typeof( UIScrollView ), true ) as UIScrollView ;
			if( tScrollViewElastic != tTarget.scrollViewElastic )
			{
				Undo.RecordObject( tTarget, "UIScrollbar : ScrollbarViewElastic Change" ) ;	// アンドウバッファに登録
				tTarget.scrollViewElastic = tScrollViewElastic ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.scrollViewElastic != null )
			{
				if( tTarget.isInteraction == false )
				{
					EditorGUILayout.HelpBox( GetMessage( "InteractionNone" ), MessageType.Warning, true ) ;
				}
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' is necessary" },
			{ "HSB_CanvasGroupNone",	"'CanvasGroup' is necessary to horizontal scrollbar" },
			{ "VSB_CanvasGroupNone",	"'CanvasGroup' is necessary to vertical scrollbar" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}

	}
}
