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
	public class UIInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public delegate void InteractionDelegate( PointerEventData tPointer, bool tFromScrollView ) ;

		public InteractionDelegate onPointerEnter	= null ;
		public InteractionDelegate onPointerExit	= null ;
		public InteractionDelegate onPointerDown	= null ;
		public InteractionDelegate onPointerUp		= null ;
		public InteractionDelegate onPointerClick	= null ;
		public InteractionDelegate onBeginDrag		= null ;
		public InteractionDelegate onDrag			= null ;
		public InteractionDelegate onEndDrag		= null ;

		public void OnPointerEnter( PointerEventData tPointer )
		{
			if( onPointerEnter != null )
			{
				onPointerEnter( tPointer, false ) ;
			}
		}

		public void OnPointerExit( PointerEventData tPointer )
		{
			if( onPointerExit != null )
			{
				onPointerExit( tPointer, false ) ;
			}
		}

		public void OnPointerDown( PointerEventData tPointer )
		{
			if( onPointerDown != null )
			{
				onPointerDown( tPointer, false ) ;
			}
		}

		public void OnPointerUp( PointerEventData tPointer )
		{
			if( onPointerUp != null )
			{
				onPointerUp( tPointer, false ) ;
			}
		}

		public void OnPointerClick( PointerEventData tPointer )
		{
			if( onPointerClick != null )
			{
				onPointerClick( tPointer, true ) ;
			}
		}

		public void OnBeginDrag( PointerEventData tPointer )
		{
			if( onBeginDrag != null )
			{
				onBeginDrag( tPointer, false ) ;
			}
		}

		public void OnDrag( PointerEventData tPointer )
		{
			if( onDrag != null )
			{
				onDrag( tPointer, false ) ;
			}
		}

		public void OnEndDrag( PointerEventData tPointer )
		{
			if( onEndDrag != null )
			{
				onEndDrag( tPointer, false ) ;
			}
		}
	}
}
