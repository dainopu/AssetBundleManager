using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Slider クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent(typeof(UnityEngine.UI.Slider))]
	public class UISlider : UIView
	{
		/// <summary>
		/// 全体領域(ショートカット)
		/// </summary>
		public RectTransform fillRect
		{
			get
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return null ;
				}
				return tSlider.fillRect ;
			}
			set
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return ;
				}
				tSlider.fillRect = value ;
			}
		}
		
		/// <summary>
		/// 下地のイメージ(ショートカット)
		/// </summary>
		public Graphic targetGraphic
		{
			get
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return null ;
				}
				return tSlider.targetGraphic ;
			}
			set
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return ;
				}
				tSlider.targetGraphic = value ;
			}
		}
		
		/// <summary>
		/// 移動領域(ショートカット)
		/// </summary>
		public RectTransform handleRect
		{
			get
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return null ;
				}
				return tSlider.fillRect ;
			}
			set
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return ;
				}
				tSlider.handleRect = value ;
			}
		}
		
		/// <summary>
		/// 値(0～1)(ショートカット)
		/// </summary>
		public float value
		{
			get
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return 0f ;
				}
				return tSlider.value ;
			}
			set
			{
				Slider tSlider = _slider ;
				if( tSlider == null )
				{
					return ;
				}

				if( tSlider.value != value )
				{
					tSlider.value = value ;
				}
			}
		}

		private bool m_CallbackDisable = false ;

		/// <summary>
		/// 値(0～1)を設定する(コールバックの発生も指定可能)
		/// </summary>
		/// <param name="tValue"></param>
		/// <param name="tCallback"></param>
		public bool SetValue( float tValue, bool tCallback = true )
		{
			Slider tSlider = _slider ;
			if( tSlider == null )
			{
				return false ;
			}

			if( tSlider.value != tValue )
			{
				m_CallbackDisable = ! tCallback ;

				tSlider.value  = tValue ;

				m_CallbackDisable = false ;
			}

			return true ;
		}

		//-------------------------------------------------------------
		
		// 方向
		private enum Direction
		{
			Unknown    = 0,
			Horizontal = 1,
			Vertical   = 2,
		}
		

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Slider tSlider = _slider ;
		
			if( tSlider == null )
			{
				tSlider = gameObject.AddComponent<Slider>() ;
			}
			if( tSlider == null )
			{
				// 異常
				return ;
			}

			//---------------------------------

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
					SetSize( s * 0.25f, s * 0.05f ) ;
					tSlider.direction = Slider.Direction.LeftToRight ;
				}
				else
				if( tDirection == Direction.Vertical )
				{
					SetSize( s * 0.05f, s * 0.25f ) ;
					tSlider.direction = Slider.Direction.BottomToTop ;
				}
			}
				
			ResetRectTransform() ;
			
			UIImage tBackground = AddView<UIImage>( "Background" ) ;
			if( tDirection == Direction.Horizontal )
			{
				tBackground.SetAnchorMinAndMax( 0.00f, 0.25f, 1.00f, 0.75f ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tBackground.SetAnchorMinAndMax( 0.25f, 0.00f, 0.75f, 1.00f ) ;
			}
			tBackground.SetMargin( 0, 0, 0, 0 ) ;
			tBackground.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			tBackground.type = Image.Type.Sliced ;
			tBackground.fillCenter = true ;
			tBackground.SetSize( 0, 0 ) ;
			
			UIView tFillArea = AddView<UIView>( "Fill Area" ) ;
			if( tDirection == Direction.Horizontal )
			{
				tFillArea.SetAnchorMinAndMax( 0.00f, 0.25f, 1.00f, 0.75f ) ;
				tFillArea.SetMargin(  5, 15,  0,  0 ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tFillArea.SetAnchorMinAndMax( 0.25f, 0.00f, 0.75f, 1.00f ) ;
				tFillArea.SetMargin(  0,  0,   5, 15 ) ;
			}

			UIImage tFill = tFillArea.AddView<UIImage>( "Fill" ) ;
			tFill.SetAnchorMinAndMax( 0.00f, 0.00f, 1.00f, 1.00f ) ;
			tFill.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			tFill.type = Image.Type.Sliced ;
			tFill.fillCenter = true ;
			if( tDirection == Direction.Horizontal )
			{
				tFill.SetMargin( -5, -5,  0,  0 ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tFill.SetMargin(  0,  0, -5, -5 ) ;
			}

			tSlider.fillRect = tFill._rectTransform ;
			
			
			UIView tHandleSlideArea = AddView<UIView>( "Handle Slide Area" ) ;
			tHandleSlideArea.SetAnchorToStretch() ;
			if( tDirection == Direction.Horizontal )
			{
				tHandleSlideArea.SetMargin( 10, 10,  0,  0 ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tHandleSlideArea.SetMargin(  0,  0, 10, 10 ) ;
			}

			UIImage tHandle = tHandleSlideArea.AddView<UIImage>( "Handle" ) ;
			if( tDirection == Direction.Horizontal )
			{
				tHandle.SetAnchorToRightStretch() ;
				tHandle._x =  0 ;
				tHandle._w = _h * 1.0f ;
				tHandle.SetMarginY( 0, 0 ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tHandle.SetAnchorToStretchTop() ;
				tHandle._y =  0 ;
				tHandle._h = _w * 1.0f ;
				tHandle.SetMarginX( 0, 0 ) ;
			}

			tHandle.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;

			tSlider.targetGraphic = tHandle._image ;
			tSlider.handleRect = tHandle._rectTransform ;
		}

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;
		
			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _slider != null )
				{
					_slider.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------
	
		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		private Action<string, UISlider, float> onValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDelegate( string tIdentity, UISlider tView, float tValue ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDelegate onValueChangedDelegate ;
		
		/// <summary>
		/// 状態変化時に呼ばれるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UISlider, float> tOnValueChangedAction )
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
		
		// 内部リスナー登録
		private void OnValueChangedInner( float tValue )
		{
			if( m_CallbackDisable == true )
			{
				return ;
			}

			if( onValueChangedAction != null || onValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				onValueChangedAction?.Invoke( identity, this, tValue ) ;
				onValueChangedDelegate?.Invoke( identity, this, tValue ) ;
			}
		}
	
		//---------------------------------------------
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<float> tOnValueChanged )
		{
			Slider tSlider = _slider ;
			if( tSlider != null )
			{
				tSlider.onValueChanged.AddListener( tOnValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<float> tOnValueChanged )
		{
			Slider tSlider = _slider ;
			if( tSlider != null )
			{
				tSlider.onValueChanged.RemoveListener( tOnValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Slider tSlider = _slider ;
			if( tSlider != null )
			{
				tSlider.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}
