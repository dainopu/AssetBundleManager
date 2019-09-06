using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIScrollView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIScrollView ) ) ]
	public class UIScrollViewInspector : UIImageInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIScrollView tTarget = target as UIScrollView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// スクロールバーの有無
			DrawScrollbar( tTarget ) ;
		}

		// スクロールバーの有無を表示
		protected void DrawScrollbar( UIScrollView tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// 横スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsHorizontalScrollbar = EditorGUILayout.Toggle( tTarget.isHorizontalScrollber, GUILayout.Width( 16f ) ) ;
				if( tIsHorizontalScrollbar != tTarget.isHorizontalScrollber )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					tTarget.isHorizontalScrollber = tIsHorizontalScrollbar ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Horizontal Scrollbar", GUILayout.Width( 120f ) ) ;

				// 横位置
				UIScrollView.HorizontalScrollbarPosition tHorizontalScrollbarPosition = ( UIScrollView.HorizontalScrollbarPosition )EditorGUILayout.EnumPopup( "",  tTarget.horizontalScrollbarPosition, GUILayout.Width( 80f ) ) ;
				if( tHorizontalScrollbarPosition != tTarget.horizontalScrollbarPosition )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Horizontal Scrollbar Position Change" ) ;	// アンドウバッファに登録
					tTarget.horizontalScrollbarPosition = tHorizontalScrollbarPosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			UIScrollbar tHorizontalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", tTarget.horizontalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( tHorizontalScrollbarElastic != tTarget.horizontalScrollbarElastic )
			{
				Undo.RecordObject( tTarget, "UIScrollView : Horizontal Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				tTarget.horizontalScrollbarElastic = tHorizontalScrollbarElastic ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 縦スクロールバー
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsVerticalScrollbar = EditorGUILayout.Toggle( tTarget.isVerticalScrollber, GUILayout.Width( 16f ) ) ;
				if( tIsVerticalScrollbar != tTarget.isVerticalScrollber )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Is Horizontal Scrollbar Change" ) ;	// アンドウバッファに登録
					tTarget.isVerticalScrollber = tIsVerticalScrollbar ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Vertical Scrollbar", GUILayout.Width( 120f ) ) ;

				// 縦位置
				UIScrollView.VerticalScrollbarPosition tVerticalScrollbarPosition = ( UIScrollView.VerticalScrollbarPosition )EditorGUILayout.EnumPopup( "",  tTarget.verticalScrollbarPosition, GUILayout.Width( 80f ) ) ;
				if( tVerticalScrollbarPosition != tTarget.verticalScrollbarPosition )
				{
					Undo.RecordObject( tTarget, "UIScrollView : Vertical Scrollbar Position Change" ) ;	// アンドウバッファに登録
					tTarget.verticalScrollbarPosition = tVerticalScrollbarPosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			UIScrollbar tVerticalScrollbarElastic = EditorGUILayout.ObjectField( "Elastic", tTarget.verticalScrollbarElastic, typeof( UIScrollbar ), true ) as UIScrollbar ;
			if( tVerticalScrollbarElastic != tTarget.verticalScrollbarElastic )
			{
				Undo.RecordObject( tTarget, "UIScrollView : Vertical Scrollbar Elastic Change" ) ;	// アンドウバッファに登録
				tTarget.verticalScrollbarElastic = tVerticalScrollbarElastic ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			//----------------------------------------------------------

			bool tWarning = false ;

			tWarning = false ;
			if( tTarget.horizontalScrollbarElastic != null && tTarget.horizontalScrollbarElastic.scrollViewElastic == null )
			{
				tWarning = true ;
			}
			if( tTarget.verticalScrollbarElastic != null && tTarget.verticalScrollbarElastic.scrollViewElastic == null )
			{
					tWarning = true ;
			}
			if( tWarning == true )
			{
				EditorGUILayout.HelpBox( GetMessage( "SetScrollView" ), MessageType.Warning, true ) ;
			}

			ScrollRectWrapper tScroll = tTarget.GetComponent<ScrollRectWrapper>() ;
			if( tScroll != null )
			{
				tWarning = false ;
				if( tTarget.horizontalScrollbarElastic != null && tScroll.horizontalScrollbar!= null )
				{
					tWarning = true ;
				}
				if( tTarget.verticalScrollbarElastic != null && tScroll.verticalScrollbar != null )
				{
					tWarning = true ;
				}
				if( tWarning == true )
				{
					EditorGUILayout.HelpBox( GetMessage( "ClearScrollBar" ), MessageType.Warning, true ) ;
				}
			}

			//----------------------------------------------------------


			if( tTarget.isHorizontalScrollber == true || tTarget.horizontalScrollbarElastic != null || tTarget.isVerticalScrollber == true || tTarget.verticalScrollbarElastic != null )
			{
				// スクロールバーのフェード処理
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tScrollbarFadeEnebled = EditorGUILayout.Toggle( tTarget.scrollbarFadeEnabled, GUILayout.Width( 16f ) ) ;
					if( tScrollbarFadeEnebled != tTarget.scrollbarFadeEnabled )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Enabled Change" ) ;	// アンドウバッファに登録
						tTarget.scrollbarFadeEnabled = tScrollbarFadeEnebled ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
					GUILayout.Label( "Scrollbar Fade Enabled" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				if( tTarget.scrollbarFadeEnabled == true )
				{
					if( tTarget.isInteraction == false && tTarget.isInteractionForScrollView == false )
					{
						EditorGUILayout.HelpBox( GetMessage( "InteractionNone" ), MessageType.Warning, true ) ;
					}

//					if( tTarget._scrollRect.verticalScrollbarVisibility != UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHide )
//					{
//						EditorGUILayout.HelpBox( GetMessage( "SetAutoHide" ), MessageType.Warning, true ) ;
//					}

	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０

					float tScrollbarFadeInDuration = EditorGUILayout.FloatField( " Duration In", tTarget.scrollbarFadeInDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeInDuration != tTarget.scrollbarFadeInDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade In Duration" ) ;	// アンドウバッファに登録
						tTarget.scrollbarFadeInDuration = tScrollbarFadeInDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					float tScrollbarFadeHoldDuration = EditorGUILayout.FloatField( " Duration Hold", tTarget.scrollbarFadeHoldDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeHoldDuration != tTarget.scrollbarFadeHoldDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Hold Duration" ) ;	// アンドウバッファに登録
						tTarget.scrollbarFadeHoldDuration = tScrollbarFadeHoldDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					float tScrollbarFadeOutDuration = EditorGUILayout.FloatField( " Duration Out", tTarget.scrollbarFadeOutDuration /*, GUILayout.Width( 120f ) */ ) ;
					if( tScrollbarFadeOutDuration != tTarget.scrollbarFadeOutDuration )
					{
						Undo.RecordObject( tTarget, "UIScrollView : Scrollbar Fade Out Duration" ) ;	// アンドウバッファに登録
						tTarget.scrollbarFadeOutDuration = tScrollbarFadeOutDuration ;
						EditorUtility.SetDirty( tTarget ) ;
					}



	//				EditorGUIUtility.LookLikeControls( 120f, 40f ) ;	// デフォルトの見た目で横幅８０


					if( tTarget.horizontalScrollbar != null )
					{
						if( tTarget.horizontalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "HSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
					if( tTarget.verticalScrollbar != null )
					{
						if( tTarget.verticalScrollbar.GetComponent<CanvasGroup>() == null )
						{
							EditorGUILayout.HelpBox( GetMessage( "VSB_CanvasGroupNone" ), MessageType.Warning, true ) ;
						}
					}
				}

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool tHidingScrollbarIfContentFew = EditorGUILayout.Toggle( tTarget.hidingScrollbarIfContentFew, GUILayout.Width( 16f ) ) ;
						if( tHidingScrollbarIfContentFew != tTarget.hidingScrollbarIfContentFew )
						{
							Undo.RecordObject( tTarget, "UIScrollView : Hiding Scrollbar If Content Few" ) ;	// アンドウバッファに登録
							tTarget.hidingScrollbarIfContentFew = tHidingScrollbarIfContentFew ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( " Hiding Scrollbar If Content Few" ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了

					GUILayout.BeginHorizontal() ;	// 横並び
					{
						bool tInvalidateScrollIfContentFew = EditorGUILayout.Toggle( tTarget.invalidateScrollIfContentFew, GUILayout.Width( 16f ) ) ;
						if( tInvalidateScrollIfContentFew != tTarget.invalidateScrollIfContentFew )
						{
							Undo.RecordObject( tTarget, "UIScrollView : Invalidate Scroll If Content Few" ) ;	// アンドウバッファに登録
							tTarget.invalidateScrollIfContentFew = tInvalidateScrollIfContentFew ;
							EditorUtility.SetDirty( tTarget ) ;
						}
						GUILayout.Label( " Invalidate Scroll If Content Few" ) ;
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SetScrollView",			"Elastic を 完全に有効にするには Scrollbar に ScrollView を設定する必要があります" },
			{ "ClearScrollBar",			"Elastic を 完全に有効にするには ScrollRect の Scrollbar を消去する必要があります" },
			{ "SetAutoHide",			"Visibility を 'Auto Hide' に設定する事をお勧めします" },
			{ "InteractionNone",		"UIInteraction クラスまたは UIInteractionForScrollView クラスが必要です" },
			{ "HSB_CanvasGroupNone",	"Horizontal Scrollbar に CanvasGroup クラスが必要です" },
			{ "VSB_CanvasGroupNone",	"Vertical Scrollbar に CanvasGroup クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SetScrollView",			"In order to fully enable Elastic you need to set ScrollView on Scrollbar" },
			{ "ClearScrollBar",			"In order to fully enable Elastic, you need to clear the Scrollbar of ScrollRect" },
			{ "SetAutoHide",			"Recommend to set Visibility to 'Auto Hide'" },
			{ "InteractionNone",		"'UIInteraction' or 'UIInteractionForScrollView' is necessary" },
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

