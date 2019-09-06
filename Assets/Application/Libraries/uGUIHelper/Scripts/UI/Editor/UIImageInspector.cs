using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImage のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIImage ) ) ]
	public class UIImageInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIImage tTarget = target as UIImage ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlas( tTarget ) ;

			// Flipper の追加と削除
			DrawFlipper( tTarget ) ;

			// マテリアル選択
			DrawMaterial( tTarget ) ;
		}
	}
}

