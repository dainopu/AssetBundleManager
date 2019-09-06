using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIListView のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIListView ) ) ]
	public class UIListViewInspector : UIScrollViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIListView tTarget = target as UIListView ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// スクロールバーの有無
			DrawScrollbar( tTarget ) ;
		
			// アイテムの追加と削除
			DrawItem( tTarget ) ;
		}

		// Item 関係
		protected void DrawItem( UIListView tTarget )
		{
			if( tTarget._scrollRect == null || tTarget._scrollRect.content == null )
			{
				return ;
			}


			EditorGUILayout.Separator() ;	// 少し区切りスペース

//			GUILayout.Label( "Type " + tTarget.buildType, GUILayout.Width( 120f ) ) ;

			EditorGUIUtility.labelWidth = 100f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// テンプレート
			UIView tContentItem = EditorGUILayout.ObjectField( "Item(Template)", tTarget.item, typeof( UIView ), true ) as UIView ;
			if( tContentItem != tTarget.item )
			{
				Undo.RecordObject( tTarget, "UIListView Item(Template) : Change" ) ;	// アンドウバッファに登録
				tTarget.item = tContentItem ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			//------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				int tWorkingItemCount = EditorGUILayout.IntField( "Working Item Count", tTarget.workingItemCount ) ;
				if( tWorkingItemCount!= tTarget.workingItemCount )
				{
					Undo.RecordObject( tTarget, "UIListView : Working Item Count Change" ) ;	// アンドウバッファに登録
					tTarget.workingItemCount = tWorkingItemCount ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			float tWorkingMargin = EditorGUILayout.FloatField( "Working Margin", tTarget.workingMargin ) ;
			if( tWorkingMargin!= tTarget.workingMargin )
			{
				Undo.RecordObject( tTarget, "UIListView : Working Margin Change" ) ;	// アンドウバッファに登録
				tTarget.workingMargin = tWorkingMargin ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tInfinity = EditorGUILayout.Toggle( tTarget.infinity, GUILayout.Width( 16f ) ) ;
				if( tInfinity != tTarget.infinity )
				{
					Undo.RecordObject( tTarget, "UIListView : Infinity Change" ) ;	// アンドウバッファに登録
					tTarget.infinity = tInfinity ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Infinity", GUILayout.Width( 80f ) ) ;


				if( tTarget.infinity == false )
				{
//					GUILayout.Label( "Max", GUILayout.Width( 40f ) ) ;

					EditorGUIUtility.labelWidth =  80f ;
					EditorGUIUtility.fieldWidth =  40f ;

					int tItemCount = EditorGUILayout.IntField( "Item Count", tTarget.itemCount ) ;
					if( tItemCount!= tTarget.itemCount )
					{
						Undo.RecordObject( tTarget, "UIListView : Item Count Change" ) ;	// アンドウバッファに登録
						tTarget.itemCount = tItemCount ;
						EditorUtility.SetDirty( tTarget ) ;
//						if( Application.isPlaying == false )
//						{
//							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
//						}
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tSnap = EditorGUILayout.Toggle( tTarget.snap, GUILayout.Width( 16f ) ) ;
				if( tSnap != tTarget.snap )
				{
					Undo.RecordObject( tTarget, "UIListView : Snap Change" ) ;	// アンドウバッファに登録
					tTarget.snap = tSnap ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Snap", GUILayout.Width( 80f ) ) ;

				if( tTarget.snap == true )
				{
					EditorGUIUtility.labelWidth =  80f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float tSnapThreshold = EditorGUILayout.FloatField( "Threshold", tTarget.snapThreshold ) ;
					if( tSnapThreshold != tTarget.snapThreshold )
					{
						Undo.RecordObject( tTarget, "UIListView : Snap Threshold Change" ) ;	// アンドウバッファに登録
						tTarget.snapThreshold = tSnapThreshold ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					EditorGUIUtility.labelWidth =  40f ;
					EditorGUIUtility.fieldWidth =  40f ;

					float tSnapTime = EditorGUILayout.FloatField( "Time", tTarget.snapTime ) ;
					if( tSnapTime != tTarget.snapTime )
					{
						Undo.RecordObject( tTarget, "UIListView : Snap Time Change" ) ;	// アンドウバッファに登録
						tTarget.snapTime = tSnapTime ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					EditorGUIUtility.labelWidth = 116f ;
					EditorGUIUtility.fieldWidth =  40f ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.snap == true )
			{
				UIListView.SnapAnchor tSnapPosition = ( UIListView.SnapAnchor )EditorGUILayout.EnumPopup( "Snap Anchor", tTarget.snapAnchor ) ;
				if( tSnapPosition != tTarget.snapAnchor )
				{
					Undo.RecordObject( tTarget, "UIListView : Snap Anchor Change" ) ;	// アンドウバッファに登録
					tTarget.snapAnchor = tSnapPosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}


			// アイテムの更新が必要な時に呼び出されるイベント

			// デリゲートの設定状況
			SerializedObject tSO = new SerializedObject( tTarget ) ;

			SerializedProperty tSP = tSO.FindProperty( "onUpdateItem" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP ) ;
			}
			tSO.ApplyModifiedProperties() ;
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

