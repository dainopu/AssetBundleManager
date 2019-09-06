using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.EventSystems ;

namespace uGUIHelper
{
	/// <summary>
	/// ドラッグ中かどうかを取得するためにのみ用意した ScrollRect の継承クラス
	/// </summary>
	public class ScrollbarWrapper : Scrollbar
	{
		private bool m_Press ;
		public bool isPress
		{
			get
			{
				return m_Press ;
			}
		}

	    public override void OnPointerDown( PointerEventData tPointer )
		{
			base.OnPointerDown( tPointer ) ;
			m_Press = true ;
		}

	    public override void OnPointerUp( PointerEventData tPointer )
		{
			base.OnPointerUp( tPointer ) ;
			m_Press = false ;
		}
	}
}
