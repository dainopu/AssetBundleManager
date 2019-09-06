using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Interaction コンポーネントクラス
	/// </summary>
	public class UIInteractionForScrollView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		public delegate void InteractionDelegate( PointerEventData tPointer, bool tFromScrollView ) ;

		public InteractionDelegate onPointerEnter	= null ;
		public InteractionDelegate onPointerExit	= null ;
		public InteractionDelegate onPointerDown	= null ;
		public InteractionDelegate onPointerUp		= null ;
		public InteractionDelegate onPointerClick	= null ;

		public void OnPointerEnter( PointerEventData tPointer )
		{
			if( onPointerEnter != null )
			{
				onPointerEnter( tPointer, true ) ;
			}
		}

		public void OnPointerExit( PointerEventData tPointer )
		{
			if( onPointerExit != null )
			{
				onPointerExit( tPointer, true ) ;
			}
		}

		public void OnPointerDown( PointerEventData tPointer )
		{
			if( onPointerDown != null )
			{
				onPointerDown( tPointer, true ) ;
			}
		}

		public void OnPointerUp( PointerEventData tPointer )
		{
			if( onPointerUp != null )
			{
				onPointerUp( tPointer, true ) ;
			}
		}

		public void OnPointerClick( PointerEventData tPointer )
		{
			if( onPointerClick != null )
			{
				onPointerClick( tPointer, true ) ;
			}
		}

	}
}
