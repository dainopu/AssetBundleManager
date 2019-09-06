using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGraphic のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( MaskableGraphicWrapper ) ) ]
	public class MaskableGraphicWrapperInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
/*		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			UIGraphic tTarget = target as UIGraphic ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;
		}*/

		// Graphic の基本情報を描画する
		protected void DrawBasis( MaskableGraphicWrapper tTarget )
		{
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// マテリアル
			Material tMaterial = EditorGUILayout.ObjectField( "Material", tTarget.material, typeof( Material ), false ) as Material ;
			if( tMaterial != tTarget.material )
			{
				Undo.RecordObject( tTarget, "UIGraphic : Material Change" ) ;	// アンドウバッファに登録
				tTarget.material = tMaterial ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// カラー
			Color tColor = new Color( tTarget.color.r, tTarget.color.g, tTarget.color.b, tTarget.color.a ) ;
			tColor = EditorGUILayout.ColorField( "Color", tColor ) ;
			if( tColor.r != tTarget.color.r || tColor.g != tTarget.color.g || tColor.b != tTarget.color.b || tColor.a != tTarget.color.a )
			{
				Undo.RecordObject( tTarget, "UIGraphic : Color Change" ) ;	// アンドウバッファに登録
				tTarget.color = tColor ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// レイキャストターゲット
			bool tRaycastTarget = EditorGUILayout.Toggle( "Raycast Taget", tTarget.raycastTarget ) ;
			if( tRaycastTarget != tTarget.raycastTarget )
			{
				Undo.RecordObject( tTarget, "UIGraphic : Raycast Target Change" ) ;	// アンドウバッファに登録
				tTarget.raycastTarget = tRaycastTarget ;
				EditorUtility.SetDirty( tTarget ) ;
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		}
	}
}
