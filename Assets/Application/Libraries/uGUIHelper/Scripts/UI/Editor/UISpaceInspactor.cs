using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UICanvas のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISpace ) ) ]
	public class UISpaceInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UISpace tTarget = target as UISpace ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( tTarget ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUI.backgroundColor = Color.magenta ;
			Camera tTargetCamera = EditorGUILayout.ObjectField( "Target Camera", tTarget.targetCamera, typeof( Camera ), true ) as Camera ;
			GUI.backgroundColor = Color.white ;
			if( tTargetCamera != tTarget.targetCamera )
			{
				Undo.RecordObject( tTarget, "UISpace : Target Camera Change" ) ;	// アンドウバッファに登録
				tTarget.targetCamera = tTargetCamera ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.targetCamera != null )
			{
				Vector2 tCameraOffset = EditorGUILayout.Vector2Field( "Camera Offset", tTarget.cameraOffset ) ;
				if( tCameraOffset.Equals( tTarget.cameraOffset ) == false )
				{
					Undo.RecordObject( tTarget, "UISpace : Camera Offset Change" ) ;	// アンドウバッファに登録
					tTarget.cameraOffset = tCameraOffset ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tFlexibleFieldOfView = EditorGUILayout.Toggle( tTarget.flexibleFieldOfView, GUILayout.Width( 16f ) ) ;
				if( tFlexibleFieldOfView != tTarget.flexibleFieldOfView )
				{
					Undo.RecordObject( tTarget, "UISpace : Flexible Field Of View Change" ) ;	// アンドウバッファに登録
					tTarget.flexibleFieldOfView = tFlexibleFieldOfView ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Flexible Field Of View" ) ;

				if( tTarget.flexibleFieldOfView == true )
				{
					float tBasisHeight = EditorGUILayout.FloatField( "Basis Height", tTarget.basisHeight ) ;
					if( tBasisHeight != tTarget.basisHeight )
					{
						Undo.RecordObject( tTarget, "UISpace : Basis Height Change" ) ;	// アンドウバッファに登録
						tTarget.basisHeight = tBasisHeight ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			//----------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tRenderTextureEnabled = EditorGUILayout.Toggle( tTarget.renderTextureEnabled, GUILayout.Width( 16f ) ) ;
				if( tRenderTextureEnabled != tTarget.renderTextureEnabled )
				{
					Undo.RecordObject( tTarget, "UISpace : Render Texture Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.renderTextureEnabled = tRenderTextureEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Render Texture Enabled" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			//----------------------------------------------------------

			if( tTarget.renderTextureEnabled == true )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				EditorGUIUtility.labelWidth =  60f ;
				EditorGUIUtility.fieldWidth =  40f ;

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsMask = EditorGUILayout.Toggle( tTarget.imageMask, GUILayout.Width( 16f ) ) ;
					if( tIsMask != tTarget.imageMask )
					{
						Undo.RecordObject( tTarget, "UISpace : Mask Change" ) ;	// アンドウバッファに登録
						tTarget.imageMask = tIsMask ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Mask" ) ;

					bool tIsInversion = EditorGUILayout.Toggle( tTarget.imageInversion, GUILayout.Width( 16f ) ) ;
					if( tIsInversion != tTarget.imageInversion )
					{
						Undo.RecordObject( tTarget, "UISpace : Image Inversion Change" ) ;	// アンドウバッファに登録
						tTarget.imageInversion = tIsInversion ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Inversion" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了


				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsShadow = EditorGUILayout.Toggle( tTarget.imageShadow, GUILayout.Width( 16f ) ) ;
					if( tIsShadow != tTarget.isShadow )
					{
						Undo.RecordObject( tTarget, "UISpace : Shadow Change" ) ;	// アンドウバッファに登録
						tTarget.imageShadow = tIsShadow ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Shadow" ) ;

					bool tIsOutline = EditorGUILayout.Toggle( tTarget.imageOutline, GUILayout.Width( 16f ) ) ;
					if( tIsOutline != tTarget.imageOutline )
					{
						Undo.RecordObject( tTarget, "UISpace : Outline Change" ) ;	// アンドウバッファに登録
						tTarget.imageOutline = tIsOutline ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Outline" ) ;

					bool tIsGradient = EditorGUILayout.Toggle( tTarget.imageGradient, GUILayout.Width( 16f ) ) ;
					if( tIsGradient != tTarget.imageGradient )
					{
						Undo.RecordObject( tTarget, "UISpace : Gradient Change" ) ;	// アンドウバッファに登録
						tTarget.imageGradient = tIsGradient ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Gradient" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				EditorGUIUtility.labelWidth = 116f ;
				EditorGUIUtility.fieldWidth =  40f ;
			}

		}
	}
}

