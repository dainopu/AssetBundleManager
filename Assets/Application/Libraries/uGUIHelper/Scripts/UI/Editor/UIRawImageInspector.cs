using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIRawImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIRawImage ) ) ]
	public class UIRawImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIRawImage tTarget = target as UIRawImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// マテリアル選択
			DrawMaterial( tTarget ) ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tAutoCreateDrawableTexture = EditorGUILayout.Toggle( tTarget.autoCreateDrawableTexture, GUILayout.Width( 16f ) ) ;
				if( tAutoCreateDrawableTexture != tTarget.autoCreateDrawableTexture )
				{
					Undo.RecordObject( tTarget, "UIRawImage : Auto Create DrawableTexture Change" ) ;	// アンドウバッファに登録
					tTarget.autoCreateDrawableTexture = tAutoCreateDrawableTexture ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Auto Create Drawable Texture" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				bool tIsFlipVertical = EditorGUILayout.Toggle( tTarget.isFlipVertical, GUILayout.Width( 16f ) ) ;
				if( tIsFlipVertical != tTarget.isFlipVertical )
				{
					Undo.RecordObject( tTarget, "UIRawImage : Is Flip Vertical Change" ) ;	// アンドウバッファに登録
					tTarget.isFlipVertical = tIsFlipVertical ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Is Flip Vertical" ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}
	}
}

