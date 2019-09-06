using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Toggle クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent(typeof(UnityEngine.UI.Toggle))]
	public class UIToggle : UIView
	{
		/// <summary>
		/// ベースのビューのインスタンス
		/// </summary>
		public UIImage	background ;

		/// <summary>
		/// チェックのビューのインスタンス
		/// </summary>
		public UIImage	checkmark ;

		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		public UIText	label ;
	
		//-------------------------------------------
		
		/// <summary>
		/// チェック状態(ショートカット)
		/// </summary>
		public bool isOn
		{
			get
			{
				Toggle toggle = _toggle ;
				if( toggle == null )
				{
					return false ;
				}
			
				return toggle.isOn ;
			}
			set
			{
				Toggle toggle = _toggle ;
				if( toggle == null )
				{
					return ;
				}
			
				toggle.isOn = value ;
			}
		}
	
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool interactable
		{
			get
			{
				Toggle toggle = _toggle ;
				if( toggle == null )
				{
					return false ;
				}
				return toggle.interactable ;
			}
			set
			{
				Toggle toggle = _toggle ;
				if( toggle == null )
				{
					return ;
				}
				toggle.interactable = value ;
			}
		}

		//---------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string option = "" )
		{
			Toggle toggle = _toggle ;
		
			if( toggle == null )
			{
				toggle = gameObject.AddComponent<Toggle>() ;
			}
			if( toggle == null )
			{
				// 異常
				return ;
			}

			//-------------------------

			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				SetSize( tSize.y * 0.25f, tSize.y * 0.05f ) ;
			}
			
			// Background	
			background = AddView<UIImage>( "Background" ) ;
			background.SetAnchorToLeftMiddle() ;
			background.SetPosition( _h * 0.5f, 0 ) ;
			background.SetSize( _h, _h ) ;
			background.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			background.type = Image.Type.Sliced ;
			background.fillCenter = true ;
			
			background.raycastTarget = true ;	// 必須

			if( isCanvasOverlay == true )
			{
				background.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			toggle.targetGraphic = background._image ;
			
			// Checkmark	
			checkmark = background.AddView<UIImage>( "Checkmark" ) ;
			checkmark.SetAnchorToCenter() ;
			checkmark.SetSize( _h, _h ) ;
			checkmark.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultCheck" ) ;
			
			if( isCanvasOverlay == true )
			{
				checkmark.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			toggle.graphic = checkmark._image ;
			
			// Label
			label = AddView<UIText>( "Label" ) ;
			label.SetAnchorToLeftMiddle() ;
			label.SetPosition( _h * 1.2f, 0 ) ;
			label.SetPivot( 0, 0.5f ) ;
	//		label.text = "Toggle" ;
			label.fontSize = ( int )( _h * 0.75f ) ;
			
			if( isCanvasOverlay == true )
			{
				label.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------------

			// 親にグループがアタッチされていればグループとみなす
			ToggleGroup toggleGroup = GetComponentInParent<ToggleGroup>() ;
			if( toggleGroup != null )
			{
				toggle.group = toggleGroup ;
			}

			//----------------------------------------

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
				if( _toggle != null )
				{
					_toggle.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIToggle, bool> OnValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDefinition( string identity, UIToggle view, bool value ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDefinition OnValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIToggle, bool> onValueChangedAction )
		{
			OnValueChangedAction = onValueChangedAction ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChangedDefinition onValueChangedDelegate )
		{
			OnValueChangedDelegate += onValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChangedDefinition onValueChangedDelegate )
		{
			OnValueChangedDelegate -= onValueChangedDelegate ;
		}

		// 内部リスナー
		private void OnValueChangedInner( bool tValue )
		{
			if( OnValueChangedAction != null || OnValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				OnValueChangedAction?.Invoke( identity, this, tValue ) ;
				OnValueChangedDelegate?.Invoke( identity, this, tValue ) ;
			}
		}
	
		//-----------------------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<bool> onValueChanged )
		{
			Toggle toggle = _toggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.AddListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<bool> onValueChanged )
		{
			Toggle toggle = _toggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.RemoveListener( onValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Toggle toggle = _toggle ;
			if( toggle != null )
			{
				toggle.onValueChanged.RemoveAllListeners() ;
			}
		}
	}
}
