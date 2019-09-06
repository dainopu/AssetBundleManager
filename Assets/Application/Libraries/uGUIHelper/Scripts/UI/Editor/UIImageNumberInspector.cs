using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageNumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIImageNumber ) ) ]
	public class UIImageNumberInspector : UIViewInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UIImageNumber tTarget = target as UIImageNumber ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			int tValue = EditorGUILayout.IntField( "Value", tTarget.value, GUILayout.Width( 200f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( tValue != tTarget.value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( tTarget, "ImageNumber : Value Change" ) ;	// アンドウバッファに登録
				tTarget.value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UIImageNumber : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}
	}
}

