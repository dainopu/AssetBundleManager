//#if TextMeshPro

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UINumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UINumberMesh ) ) ]
	public class UINumberMeshInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UINumberMesh tTarget = target as UINumberMesh ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UINumberMesh : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}


			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			bool tIsCustomized = EditorGUILayout.Toggle( "Customize", tTarget.isCustomized ) ;
			if( tIsCustomized != tTarget.isCustomized )
			{
				Undo.RecordObject( tTarget, "UITextMesh : Customize Change" ) ;	// アンドウバッファに登録
				tTarget.isCustomized = tIsCustomized ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.isCustomized == true )
			{
				Color tOutlineColor_Old = CloneColor( tTarget.outlineColor ) ;
				Color tOutlineColor_New = EditorGUILayout.ColorField( "Outline Color", tOutlineColor_Old ) ;
				if( CheckColor( tOutlineColor_Old, tOutlineColor_New ) == false )
				{
					Undo.RecordObject( tTarget, "UITextMesh : Outline Color Change" ) ;	// アンドウバッファに登録
					tTarget.outlineColor = tOutlineColor_New ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}

			bool tRaycastTarget = EditorGUILayout.Toggle( "Raycast Target", tTarget.raycastTarget ) ;
			if( tRaycastTarget != tTarget.raycastTarget )
			{
				Undo.RecordObject( tTarget, "UITextMesh : RaycastTarget Change" ) ;	// アンドウバッファに登録
				tTarget.raycastTarget = tRaycastTarget ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			double tValue = EditorGUILayout.DoubleField( "Value", tTarget.value, GUILayout.Width( 200f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( tValue != tTarget.value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( tTarget, "UINumberMesh : Value Change" ) ;	// アンドウバッファに登録
				tTarget.value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-----------------------------------------------------
			
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 縦方向揃え
				GUILayout.Label( "Digit", GUILayout.Width( 40.0f ) ) ;	// null でないなら 74
				int tDigitInteger = EditorGUILayout.IntField( tTarget.digitInteger, GUILayout.Width( 40f ) ) ;
				if( tDigitInteger != tTarget.digitInteger )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Digit Integer Change" ) ;	// アンドウバッファに登録
					tTarget.digitInteger = tDigitInteger ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			
				GUILayout.Label( ".", GUILayout.Width( 10.0f ) ) ;	// null でないなら 74
			
				int tDigitDecimal = EditorGUILayout.IntField( tTarget.digitDecimal, GUILayout.Width( 40f ) ) ;
				if( tDigitDecimal != tTarget.digitDecimal )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Digit Decimal Change" ) ;	// アンドウバッファに登録
					tTarget.digitDecimal = tDigitDecimal ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// 適当なスペース
				GUILayout.Label( "", GUILayout.Width( 5f ) ) ;

				// カンマ
				GUILayout.Label( "Comma", GUILayout.Width( 50.0f ) ) ;	// null でないなら 74
				int tComma = EditorGUILayout.IntField( tTarget.comma, GUILayout.Width( 40f ) ) ;
				if( tComma != tTarget.comma )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "UINumberMesh : Comma Change" ) ;	// アンドウバッファに登録
					tTarget.comma = tComma ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;
		
			// 符号を表示するか否か
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPlusSign = EditorGUILayout.Toggle( tTarget.plusSign, GUILayout.Width( 16f ) ) ;
				if( tPlusSign != tTarget.plusSign )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Plus Sign Change" ) ;	// アンドウバッファに登録
					tTarget.plusSign = tPlusSign ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Plus Sign", GUILayout.Width( 80f ) ) ;
	//		}
	//		GUILayout.EndHorizontal() ;		// 横並び終了
			
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// 符号を表示するか否か
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroSign = EditorGUILayout.Toggle( tTarget.zeroSign, GUILayout.Width( 16f ) ) ;
				if( tZeroSign != tTarget.zeroSign )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zero Sign Change" ) ;	// アンドウバッファに登録
					tTarget.zeroSign = tZeroSign ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Sign", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

//			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// ０埋め
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroPadding = EditorGUILayout.Toggle( tTarget.zeroPadding, GUILayout.Width( 16f ) ) ;
				if( tZeroPadding != tTarget.zeroPadding )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zero Padding Change" ) ;	// アンドウバッファに登録
					tTarget.zeroPadding = tZeroPadding ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Padding", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
		
			// パーセント
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPercent = EditorGUILayout.Toggle( tTarget.percent, GUILayout.Width( 16f ) ) ;
				if( tPercent != tTarget.percent )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Percent Change" ) ;	// アンドウバッファに登録
					tTarget.percent = tPercent ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Percent", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
		
			// 全角
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZenkaku = EditorGUILayout.Toggle( tTarget.zenkaku, GUILayout.Width( 16f ) ) ;
				if( tZenkaku != tTarget.zenkaku )
				{
					Undo.RecordObject( tTarget, "UINumberMesh : Zenkaku Change" ) ;	// アンドウバッファに登録
					tTarget.zenkaku = tZenkaku ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Zenkaku", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

//#endif
