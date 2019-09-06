using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIJoystick のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIJoystick ) ) ]
	public class UIJoystickInspector : UIViewInspector
	{
		override protected void DrawInspectorGUI()
		{
			UIJoystick tTarget = target as UIJoystick ;

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
		
			// キャンバスグループを有効にするかどうか
			DrawCanvasGroup( tTarget ) ;
		}
	}
}
