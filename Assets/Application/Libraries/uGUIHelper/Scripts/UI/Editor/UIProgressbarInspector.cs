using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIProgressbar のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIProgressbar ) ) ]
	public class UIProgressbarInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIProgressbar tTarget = target as UIProgressbar ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// スライダーでアルファをコントロール出来るようにする
			float tValue = EditorGUILayout.Slider( "Value", tTarget.value, 0.0f, 1.0f ) ;
			if( tValue != tTarget.value )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Value Change" ) ;	// アンドウバッファに登録
				tTarget.value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			// 即値
			float tNumber = EditorGUILayout.FloatField( "Number",  tTarget.number ) ;
			if( tNumber != tTarget.number )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Number Change" ) ;	// アンドウバッファに登録
				tTarget.number = tNumber ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 頂点密度
			UIProgressbar.DisplayType tDisplayType = ( UIProgressbar.DisplayType )EditorGUILayout.EnumPopup( "Display Type",  tTarget.displayType ) ;
			if( tDisplayType != tTarget.displayType )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Display Type Change" ) ;	// アンドウバッファに登録
				tTarget.displayType = tDisplayType ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}



			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIImage tScope= EditorGUILayout.ObjectField( "Scope", tTarget.scope, typeof( UIImage ), true ) as UIImage ;
			if( tScope != tTarget.scope )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Scope Change" ) ;	// アンドウバッファに登録
				tTarget.scope = tScope ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UIImage tThumb = EditorGUILayout.ObjectField( "Thumb", tTarget.thumb, typeof( UIImage ), true ) as UIImage ;
			if( tThumb != tTarget.thumb )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Thumb Change" ) ;	// アンドウバッファに登録
				tTarget.thumb = tThumb ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			UINumber tLabel = EditorGUILayout.ObjectField( "Label", tTarget.label, typeof( UINumber ), true ) as UINumber ;
			if( tLabel != tTarget.label )
			{
				Undo.RecordObject( tTarget, "UIProgressbar : Label Change" ) ;	// アンドウバッファに登録
				tTarget.label = tLabel ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}
	}
}

