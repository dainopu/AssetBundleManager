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
	public class ScrollRectWrapper : ScrollRect, IPointerDownHandler, IPointerUpHandler
	{
		private bool m_Press = false ;
		public  bool isPress
		{
			get
			{
				return m_Press ;
			}
		}

		private bool m_Drag = false ;
		public  bool isDrag
		{
			get
			{
				return m_Drag ;
			}
		}

		// 反応可能距離
		public float interactionThresholdRatio = 40.0f / 960.0f ;
		private Vector2	m_InteractionLimitDistance_Base ;

		// 反応可能時間
		public float interactionLimitHoldTime =	 0.5f ;
		private float   m_InteractionLimitHoldTime_Base ;

	    public void OnPointerDown( PointerEventData tPointer )
		{
//			Debug.LogWarning( "プレス開始" ) ;
			if( tPointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Press = true ;

			m_InteractionLimitDistance_Base = tPointer.position ;
			m_InteractionLimitHoldTime_Base = Time.realtimeSinceStartup ;
		}

	    public void OnPointerUp( PointerEventData tPointer )
		{
//			Debug.LogWarning( "プレス終了" ) ;
			if( tPointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Press = false ;
		}

	    public override void OnBeginDrag( PointerEventData tPointer )
		{
//			Debug.LogWarning( "ドラッグ開始" ) ;
			base.OnBeginDrag( tPointer ) ;

			if( tPointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			m_Drag = true ;		// 外からの参照用
			
			m_InteractionLimitDistance_Base = tPointer.position ;
		}

		public override void OnDrag( PointerEventData tPointer )
		{
			base.OnDrag( tPointer ) ;

			if( tPointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}
		}


		public override void OnEndDrag( PointerEventData tPointer )
		{
//			Debug.LogWarning( "ドラッグ終了" ) ;

			base.OnEndDrag( tPointer ) ;

			m_Drag = false ;	// 外からの参照用

			if( tPointer.pointerPressRaycast.gameObject == null )
			{
				return ;	// ゲームオブジェクトがドラッグ中に消失する可能性もある
			}

			//----------------------------------------------------------

//			Debug.LogWarning( "判定に来た") ;

			float tHoldTime = Time.realtimeSinceStartup - m_InteractionLimitHoldTime_Base ;
			if( tHoldTime <= interactionLimitHoldTime )
			{
				UIView tView = tPointer.pointerPressRaycast.gameObject.GetComponent<UIView>() ;
				if( tView != null )
				{
					float tDistance = Vector2.Distance( m_InteractionLimitDistance_Base, tPointer.position ) ;

					if( tDistance <= ( interactionThresholdRatio * tView.GetCanvasLength() ) )
					{
						if( tView is UIButton )
						{
							// ボタン
							UIButton tButton = tView as UIButton ;
							if( tButton.enabled == true && tButton.interactable == true && tButton.onButtonClickAction != null )
							{
								tButton.onButtonClickAction( tButton.Identity, tButton ) ;
							}
						}
					}
				}
			}
		}
	}
}
