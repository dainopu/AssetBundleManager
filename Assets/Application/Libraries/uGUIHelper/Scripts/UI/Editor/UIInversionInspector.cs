using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIImageInversion のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIInversion ) ) ]
	public class UIInversionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIInversion tTarget = target as UIInversion ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// バリュータイプ
			UIInversion.Direction tDirection = ( UIInversion.Direction )EditorGUILayout.EnumPopup( "Direction",  tTarget.direction ) ;
			if( tDirection != tTarget.direction )
			{
				Undo.RecordObject( tTarget, "UIInversion : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.direction = tDirection ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}

