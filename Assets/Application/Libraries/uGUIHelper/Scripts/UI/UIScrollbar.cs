using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Scrollbar クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent(typeof(ScrollbarWrapper))]
	public class UIScrollbar : UIImage
	{
		public float offset
		{
			get
			{
				if( _scrollbar == null )
				{
					return 0 ;
				}
				return _scrollbar.value ;
			}
			set
			{
				if( _scrollbar == null )
				{
					return ;
				}
				_scrollbar.value = value ;
			}
		}

		public float length
		{
			get
			{
				if( _scrollbar == null )
				{
					return 0 ;
				}
				return _scrollbar.size ;
			}
			set
			{
				if( _scrollbar == null )
				{
					return ;
				}
				_scrollbar.size = value ;
			}
		}

		public int step
		{
			get
			{
				if( _scrollbar == null )
				{
					return 0 ;
				}
				return _scrollbar.numberOfSteps ;
			}
			set
			{
				if( _scrollbar == null )
				{
					return ;
				}
				_scrollbar.numberOfSteps = value ;
			}
		}

		[SerializeField][HideInInspector]
		private UIScrollView	m_ScrollViewElastic = null ;
		public  UIScrollView	  scrollViewElastic
		{
			get
			{
				return m_ScrollViewElastic ;
			}
			set
			{
				m_ScrollViewElastic = value ;
			}
		}

		// 方向
		private enum Direction
		{
			Unknown    = 0,
			Horizontal = 1,
			Vertical   = 2,
		}
		
		//-----------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Scrollbar tScrollbar = _scrollbar ;

			if( tScrollbar == null )
			{
				tScrollbar = gameObject.AddComponent<Scrollbar>() ;
			}
			if( tScrollbar == null )
			{
				// 異常
				return ;
			}

			//----------------------------------

			Direction tDirection = Direction.Unknown ;
			if( tOption.ToLower() == "h" )
			{
				tDirection = Direction.Horizontal ;
			}
			else
			if( tOption.ToLower() == "v" )
			{
				tDirection = Direction.Vertical ;
			}

			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				float s ;
				if( tSize.x <= tSize.y )
				{
					s = tSize.x ;
				}
				else
				{
					s = tSize.y ;
				}
				
				if( tDirection == Direction.Horizontal )
				{
					SetSize( s * 0.4f, s * 0.075f ) ;
					tScrollbar.direction = Scrollbar.Direction.LeftToRight ;
				}
				else
				if( tDirection == Direction.Vertical )
				{
					SetSize( s * 0.075f, s * 0.4f ) ;
					tScrollbar.direction = Scrollbar.Direction.BottomToTop ;
				}
			}
				
			ColorBlock tColorBlock = tScrollbar.colors ;
			tColorBlock.fadeDuration = 0.2f ;
			tScrollbar.colors = tColorBlock ;
				
			Image tImage = _image ;
			tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			tImage.color = Color.white ;
			tImage.type = Image.Type.Sliced ;
				
			
//			UIImage tFrame = AddView<UIImage>( "Frame" ) ;
//			tFrame.SetAnchorToStretch() ;
//			tFrame.SetMargin(  0,  0,  0,  0 ) ;
//			tFrame.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
//			tFrame.color = Color.white ;
//			tFrame.type = Image.Type.Sliced ;

				
			UIView tSlidingArea = AddView<UIView>( "Sliding Area" ) ;
			tSlidingArea.SetAnchorToStretch() ;
			tSlidingArea.SetMargin( 10, 10, 10, 10 ) ;
				
			UIImage tHandle = tSlidingArea.AddView<UIImage>( "Handle" ) ;
			if( tDirection == Direction.Horizontal )
			{
				tHandle.SetAnchorToStretch() ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tHandle.SetAnchorToStretch() ;
			}
			tHandle.SetMargin( -10, -10, -10, -10 ) ;
				
			tHandle.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			tHandle.type = Image.Type.Sliced ;
			tHandle.fillCenter = true ;
				
			tScrollbar.targetGraphic = tHandle._image ;
			tScrollbar.handleRect = tHandle._rectTransform ;
			
			ResetRectTransform() ;
		}
		
		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _scrollbar != null )
				{
					_scrollbar.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------
	
		// Down
		override protected void OnPointerDownBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			base.OnPointerDownBasic( tPointer, tFromScrollView ) ;

			if( m_ScrollViewElastic != null )
			{
				if( _scrollbar.direction == Scrollbar.Direction.LeftToRight || _scrollbar.direction == Scrollbar.Direction.RightToLeft )
				{
					// 横
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.Direction.Horizontal, offset ) ;
				}
				if( _scrollbar.direction == Scrollbar.Direction.TopToBottom || _scrollbar.direction == Scrollbar.Direction.BottomToTop )
				{
					// 縦
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.Direction.Vertical,   offset ) ;
				}
			}
		}

		override protected void OnDragBasic( PointerEventData tPointer, bool tFromScrollView )
		{
			base.OnPointerDownBasic( tPointer, tFromScrollView ) ;

			if( m_ScrollViewElastic != null )
			{
				if( _scrollbar.direction == Scrollbar.Direction.LeftToRight || _scrollbar.direction == Scrollbar.Direction.RightToLeft )
				{
					// 横
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.Direction.Horizontal, offset ) ;
				}
				if( _scrollbar.direction == Scrollbar.Direction.TopToBottom || _scrollbar.direction == Scrollbar.Direction.BottomToTop )
				{
					// 縦
					m_ScrollViewElastic.SetPositionFromScrollbar( UIScrollView.Direction.Vertical,   offset ) ;
				}
			}
		}


		// Up
/*		override protected void OnPointerUpBasic( PointerEventData tPointer )
		{
			base.OnPointerUpBasic( tPointer ) ;

			// → Enter 状態であれば Highlight へ遷移
			// → Exit  状態であれば Normal へ遷移

//			ChangeScrollbarFadeState( false ) ;

			Debug.LogWarning( "離されたよ" ) ;
		}*/

		//---------------------------------------------
	
		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIScrollbar, float> onValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDelegate( string tIdentity, UIScrollbar tView, float tValue ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDelegate onValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIScrollbar, float> tOnValueChangedAction )
		{
			onValueChangedAction = tOnValueChangedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate += tOnValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate -= tOnValueChangedDelegate ;
		}

		// 内部リスナー
		private void OnValueChangedInner( float tValue )
		{
			if( onValueChangedAction != null || onValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( onValueChangedAction != null )
				{
					onValueChangedAction( identity, this, tValue ) ;
				}

				if( onValueChangedDelegate != null )
				{
					onValueChangedDelegate( identity, this, tValue ) ;
				}
			}
		}
		
		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<float> tOnValueChanged )
		{
			Scrollbar tScrollbar = _scrollbar ;
			if( tScrollbar != null )
			{
				tScrollbar.onValueChanged.AddListener( tOnValueChanged ) ;
			}
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<float> tOnValueChanged )
		{
			Scrollbar tScrollbar = _scrollbar ;
			if( tScrollbar != null )
			{
				tScrollbar.onValueChanged.RemoveListener( tOnValueChanged ) ;
			}
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Scrollbar tScrollbar = _scrollbar ;
			if( tScrollbar != null )
			{
				tScrollbar.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}

