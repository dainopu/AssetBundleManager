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
	/// ジョイスティッククラス(複合UI)
	/// </summary>
	public class UIJoystick : UIImage
	{
		/// <summary>
		/// 枠
		/// </summary>
		public UIImage frame ;

		/// <summary>
		/// 棒
		/// </summary>
		public UIImage thumb ;

		/// <summary>
		/// 枠の幅
		/// </summary>
		public float frameWidth = 128.0f ;

		/// <summary>
		/// 棒の比率
		/// </summary>
		public float thumbScale = 0.6f ;


		private Action<string, UIJoystick, float, Vector2> m_OnValueChangedAction ;
		public  Action<string, UIJoystick, float, Vector2>   onvalueChangedAction
		{
			get
			{
				return m_OnValueChangedAction ;
			}
			set
			{
				m_OnValueChangedAction = value ;
			}
		}

		public void SetOnValueChanged( Action<string, UIJoystick, float, Vector2> tOnValueChangedAction )
		{
			m_OnValueChangedAction = tOnValueChangedAction ;
		}
		
		//-----------------------------------------------------------
		
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			SetAnchorToLeftStretch() ;
			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				_w = tSize.x * 0.5f ;
			}
			SetPivot( 0, 0 ) ;

			Image tImage = _image ;
			tImage.color = new Color32( 255, 255, 255, 32 ) ;

			isCanvasGroup = true ;
			isEventTrigger = true ;

			UIAtlasSprite tAtlas = UIAtlasSprite.Create( "uGUIHelper/Textures/UISimpleJoystick" ) ;

			UIImage tFrame = AddView<UIImage>( "Frame" ) ;
			tFrame.SetAnchorToLeftBottom() ;
			tFrame.sprite = tAtlas[ "UISimpleJoystick_Frame_Type_0" ] ;
			tFrame.SetSize( frameWidth, frameWidth ) ;
			frame = tFrame ;

			float thumbWidth = frameWidth * thumbScale ;

			UIImage tThumb = tFrame.AddView<UIImage>( "Thumb" ) ;
			tThumb.SetAnchorToCenter() ;
			tThumb.sprite = tAtlas[ "UISimpleJoystick_Thumb_Type_0" ] ;
			tThumb.SetSize( thumbWidth, thumbWidth ) ;
			thumb = tThumb ;

//			DestroyImmediate( tAtlas ) ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				frame.SetActive( false ) ;
			}
		}

		/// <summary>
		/// 派生クラスの Update
		/// </summary>
		override protected void OnUpdate()
		{
			base.OnUpdate() ;
		}

		//---------------------------------------------

		// タッチポイントのＩＤ
		private int		m_PointerId = -1 ;
		private Vector2	m_BasePosition ;

		private Vector2 m_DeltaPosition ;
		public  Vector2   deltaPosition
		{
			get
			{
				return m_DeltaPosition ;
			}
		} 

		private float m_Volume = 0 ;
		public  float   volume
		{
			get
			{
				float kx = Input.GetAxis( "Horizontal" ) ;
				float ky = Input.GetAxis( "Vertical" ) ;
				if( kx != 0 || ky != 0 )
				{
					return 1 ;
				}

				return m_Volume ;
			}
		}

		private Vector2 m_Direction ;
		public  Vector2   direction
		{
			get
			{
				return m_Direction ;
			}
		}

		public Vector2 directionVolume
		{
			get
			{
				return m_Direction * m_Volume ;
			}
		}

		// デジタル系の値で取得する
		public Vector2 axis
		{
			get
			{
				Vector2 tAxis = Vector2.zero ;

				float kx = Input.GetAxis( "Horizontal" ) ;
				float ky = Input.GetAxis( "Vertical" ) ;
				if( kx != 0 || ky != 0 )
				{
					if( kx != 0 )
					{
						tAxis.x = kx ;
					}
					if( ky != 0 )
					{
						tAxis.y = ky ;
					}
					return tAxis ;
				}

				//---------------------------------------

				if( m_Volume == 0 || ( m_Direction.x == 0 && m_Direction.y == 0 ) )
				{
					return Vector2.zero ;
				}

				Vector2 tDirection = m_Direction.normalized ;

				float tLimit = 0.4226f ;	// 60 = 0.5f  45 = 0.7071f 

				if( tDirection.x <  ( - tLimit ) )
				{
					tAxis.x = -1 ;
				}
				else
				if( tDirection.x >  (   tLimit ) )
				{
					tAxis.x =  1 ;
				}

				if( tDirection.y <  ( - tLimit ) )
				{
					tAxis.y = -1 ;
				}
				else
				if( tDirection.y >  (   tLimit ) )
				{
					tAxis.y =  1 ;
				}

				return tAxis ;
			}
		}

		// Down
		override protected void OnPointerDownBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			base.OnPointerDownBasic( tPointer, tFromScrollView ) ;

			m_PointerId = tPointer.pointerId ;

			m_BasePosition = GetLocalPosition( tPointer ) ;

			frame.SetPosition( m_BasePosition ) ;
			frame.SetSize( frameWidth, frameWidth ) ;

			m_Volume = 0 ;
			m_Direction = Vector2.zero ;
			m_DeltaPosition = Vector2.zero ;

			float thumbWidth = frameWidth * thumbScale ;

			thumb.SetPosition( 0, 0 ) ;
			thumb.SetSize( thumbWidth, thumbWidth ) ;

			frame.SetActive( true ) ;

			if( m_OnValueChangedAction != null )
			{
				m_OnValueChangedAction( Identity, this, m_Volume, m_Direction ) ;
			}
		}

		// Up
		override protected void OnPointerUpBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			base.OnPointerUpBasic( tPointer, tFromScrollView ) ;

			frame.SetActive( false ) ;

			m_PointerId = -1 ;

			m_Volume = 0 ;
			m_Direction = Vector2.zero ;

			if( m_OnValueChangedAction != null )
			{
				m_OnValueChangedAction( Identity, this, m_Volume, m_Direction ) ;
			}
		}

		// Move
		override protected void OnDragBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			base.OnDragBasic( tPointer, tFromScrollView ) ;

			if( tPointer.pointerId == m_PointerId )
			{
				Vector2 tPosition = GetLocalPosition( tPointer ) ;

				Vector2 tDeltaPosition = tPosition - m_BasePosition ;
				Vector2 tDirection = tDeltaPosition.normalized ;

				float tThumbWidth = frameWidth * thumbScale ;
				float tLimit = ( frameWidth - tThumbWidth ) * 0.5f ;

				float tVolume = tDeltaPosition.magnitude ;
				if( tVolume >  tLimit )
				{
					tVolume  = tLimit ;
				}

				thumb.SetPosition( tDirection * tVolume ) ;

				m_Direction = tDirection ;
				m_Volume = tVolume / tLimit ;

				if( m_OnValueChangedAction != null )
				{
					m_OnValueChangedAction( Identity, this, m_Volume, m_Direction ) ;
				}
			}
		}
	}
}

