using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIButton のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIButton ) ) ]
	public class UIButtonInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIButton tTarget = target as UIButton ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------

			// アトラススプライトの表示
			DrawAtlas( tTarget ) ;

			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			UIText tLabel = EditorGUILayout.ObjectField( "Label", tTarget.label, typeof( UIText ), true ) as UIText ;
			if( tLabel != tTarget.label )
			{
				Undo.RecordObject( tTarget, "UIButton : Label Change" ) ;	// アンドウバッファに登録
				tTarget.label = tLabel ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			UIRichText tRichLabel = EditorGUILayout.ObjectField( "RichLabel", tTarget.richLabel, typeof( UIRichText ), true ) as UIRichText ;
			if( tRichLabel != tTarget.richLabel )
			{
				Undo.RecordObject( tTarget, "UIButton : Rich Label Change" ) ;	// アンドウバッファに登録
				tTarget.richLabel = tRichLabel ;
				EditorUtility.SetDirty( tTarget ) ;
			}

//#if TextMeshPro
			UITextMesh tLabelMesh = EditorGUILayout.ObjectField( "LabelMesh", tTarget.labelMesh, typeof( UITextMesh ), true ) as UITextMesh ;
			if( tLabelMesh != tTarget.labelMesh )
			{
				Undo.RecordObject( tTarget, "UIButton : Label Mesh Change" ) ;	// アンドウバッファに登録
				tTarget.labelMesh = tLabelMesh ;
				EditorUtility.SetDirty( tTarget ) ;
			}
//#endif

			UIImage tDisableMask = EditorGUILayout.ObjectField( "DisableMask", tTarget.disableMask, typeof( UIImage), true ) as UIImage ;
			if( tDisableMask != tTarget.disableMask )
			{
				Undo.RecordObject( tTarget, "UIButton : Disable Mask Change" ) ;	// アンドウバッファに登録
				tTarget.disableMask = tDisableMask ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tClickTransitionEnabled = EditorGUILayout.Toggle( "Click Transition Enabled", tTarget.clickTransitionEnabled ) ;
			if( tClickTransitionEnabled != tTarget.clickTransitionEnabled )
			{
				Undo.RecordObject( tTarget, "UIButton : Click Transition Enabled Change" ) ;	// アンドウバッファに登録
				tTarget.clickTransitionEnabled = tClickTransitionEnabled ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tWaitForTransition = EditorGUILayout.Toggle( "Wait For Transition", tTarget.waitForTransition ) ;
			if( tWaitForTransition != tTarget.waitForTransition )
			{
				Undo.RecordObject( tTarget, "UIButton : Wait For Transition Change" ) ;	// アンドウバッファに登録
				tTarget.waitForTransition = tWaitForTransition ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			bool tColorTransmission = EditorGUILayout.Toggle( "Color Transmission", tTarget.colorTransmission ) ;
			if( tColorTransmission != tTarget.colorTransmission )
			{
				Undo.RecordObject( tTarget, "UIButton : Color Transmission Change" ) ;	// アンドウバッファに登録
				tTarget.colorTransmission = tColorTransmission ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			bool tSetPivotToCenter = EditorGUILayout.Toggle( "Set Pivot To Center", tTarget.setPivotToCenter ) ;
			if( tSetPivotToCenter != tTarget.setPivotToCenter )
			{
				Undo.RecordObject( tTarget, "UIButton : Set Pivot To Center Change" ) ;	// アンドウバッファに登録
				tTarget.setPivotToCenter = tSetPivotToCenter ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

