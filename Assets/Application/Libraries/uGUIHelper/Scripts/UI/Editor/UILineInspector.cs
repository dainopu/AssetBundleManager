using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UILine のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UILine ) ) ]
	public class UILineInspector : UIViewInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UILine tTarget = target as UILine ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			bool tTrailEnabled = EditorGUILayout.Toggle( "Trail Enabled", tTarget.trailEnabled ) ;
			if( tTrailEnabled != tTarget.trailEnabled )
			{
				Undo.RecordObject( tTarget, "UILine : Trail Enabled Change" ) ;	// アンドウバッファに登録
				tTarget.trailEnabled = tTrailEnabled ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.trailEnabled == true )
			{
				// 頂点が消えるまでの時間
				float tTrailKeepTime = EditorGUILayout.FloatField( " Trail Keep Time", tTarget.trailKeepTime ) ;
				if( tTrailKeepTime != tTarget.trailKeepTime )
				{
					Undo.RecordObject( tTarget, "UILine : Trail Keep Time Change" ) ;	// アンドウバッファに登録
					tTarget.trailKeepTime = tTrailKeepTime ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}

/*			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UIImageNumber : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
			}*/
		}
	}
}

