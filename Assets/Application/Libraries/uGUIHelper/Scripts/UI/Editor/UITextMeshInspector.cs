//#if TextMeshPro

using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIText のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITextMesh ) ) ]
	public class UITextMeshInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UITextMesh tTarget = target as UITextMesh ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UITextMesh : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
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

		}
	}
}

//#endif
