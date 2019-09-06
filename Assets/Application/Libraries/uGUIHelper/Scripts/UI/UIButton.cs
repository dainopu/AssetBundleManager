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
	/// uGUI:Button クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Button ) ) ]
	public class UIButton : UIImage
	{
		/// <summary>
		/// ラベルのビューのインスタンス
		/// </summary>
		public UIText label ;
		
		/// <summary>
		/// リッチラベルのビューのインスタンス
		/// </summary>
		public UIRichText	richLabel ;

//#if TextMeshPro
		/// <summary>
		/// リッチラベルのビューのインスタンス
		/// </summary>
		public UITextMesh	labelMesh ;
//#endif

		public UIImage		disableMask ;


		/// <summary>
		/// クリック時のトランジションを有効にするかどうか
		/// </summary>
		public bool clickTransitionEnabled = false ;

		/// <summary>
		/// クリック時のトランジションを待ってからクリックのコールバックを呼び出す
		/// </summary>
		public bool waitForTransition = false ;

		/// <summary>
		/// 子オブジェクトに対しても色変化を伝搬させる
		/// </summary>
		public bool colorTransmission = false ;

		/// <summary>
		/// ピボットを自動的に実行時に中心にする
		/// </summary>
		public bool setPivotToCenter = true ;

		//-----------------------------------------------------
	
		private Color	m_ActiveColor = new Color() ;

		protected bool m_ButtonClick = false ;

		public bool isButtonClick
		{
			get
			{
				return m_ButtonClick ;
			}
		}

		protected int m_ButtonClickCountTime = 0 ;

		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Button tButton = _button ;

			if( tButton == null )
			{
				tButton = gameObject.AddComponent<Button>() ;
			}
			if( tButton == null )
			{
				// 異常
				return ;
			}

			Image tImage = _image ;

			//---------------------------------

			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				SetSize( tSize.y * 0.25f, tSize.y * 0.075f ) ;
			}
				
			ColorBlock tColorBlock = tButton.colors ;
			tColorBlock.fadeDuration = 0.2f ;
			tButton.colors = tColorBlock ;
				
			// Image

			Sprite tDefaultSprite = null ;
			Color	tDefaultColor = tButton.colors.disabledColor ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultSprite		= tDS.buttonFrame ;
					tDefaultColor		= tDS.buttonDisabledColor ;
				}
			}
			
#endif

			if( tDefaultSprite == null )
			{
				tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			}
			else
			{
				tImage.sprite = tDefaultSprite ;
			}
			ColorBlock tCB = tButton.colors ;
			tCB.disabledColor = tDefaultColor ;
			tButton.colors = tCB ;

			tImage.color = Color.white ;
			tImage.type = Image.Type.Sliced ;

			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------

			// トランジションを追加
			isTransition = true ;

			// イベントトリガーは不要
//			isEventTrigger = false ;

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
				if( _button != null )
				{
					_button.onClick.AddListener( OnButtonClickInner ) ;

					if( setPivotToCenter == true )
					{
						SetPivot( 0.5f, 0.5f, true ) ;	
					}
				}

				if( _image != null )
				{
					Image tImage = _image ;
					m_ActiveColor.r = tImage.color.r ;
					m_ActiveColor.g = tImage.color.g ;
					m_ActiveColor.b = tImage.color.b ;
					m_ActiveColor.a = tImage.color.a ;
				}
			}
		}

		//---------------------------------------------
	
		/// <summary>
		/// ボタンをクリックした際に呼び出されるアクション
		/// </summary>
		public Action<string, UIButton> onButtonClickAction ;
		
		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		public delegate void OnButtonClickDelegate( string tIdentity, UIButton tView ) ;

		/// <summary>
		/// ボタンをクリックした際に呼び出されるデリゲート
		/// </summary>
		public OnButtonClickDelegate onButtonClickDelegate ;

		/// <summary>
		/// ボタンクリックを強制的に実行する
		/// </summary>
		public void ExecuteButtonClick()
		{
			OnButtonClickInner() ;
		}

		// 内部リスナー
		private void OnButtonClickInner()
		{
			if( clickTransitionEnabled == false || ( clickTransitionEnabled == true && waitForTransition == false ) )
			{
				m_ButtonClick = true ;
				m_ButtonClickCountTime = Time.frameCount ;

				if( onButtonClickAction != null || onButtonClickDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}
	
					if( onButtonClickAction != null )
					{
						onButtonClickAction( identity, this ) ;
					}
	
					if( onButtonClickDelegate != null )
					{
						onButtonClickDelegate( identity, this ) ;
					}
				}
			}

			if( clickTransitionEnabled == true )
			{
				UITransition tTransition = _transition ;
				if( tTransition != null )
				{
					tTransition.OnClicked( waitForTransition ) ;
				}
			}
		}
		
		internal protected void OnButtonClickFromTransition()
		{
			if( waitForTransition == true )
			{
				m_ButtonClick = true ;
				m_ButtonClickCountTime = Time.frameCount ;

				if( onButtonClickAction != null || onButtonClickDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}
	
					if( onButtonClickAction != null )
					{
						onButtonClickAction( identity, this ) ;
					}
	
					if( onButtonClickDelegate != null )
					{
						onButtonClickDelegate( identity, this ) ;
					}
				}
			}
		}


		/// <summary>
		/// ボタンをクリックされた際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnButtonClickAction">アクションメソッド</param>
		public void SetOnButtonClick( Action<string, UIButton> tOnButtonClickAction )
		{
			onButtonClickAction = tOnButtonClickAction ;
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnButtonClickDelegate">デリゲートメソッド</param>
		public void AddOnButtonClick( OnButtonClickDelegate tOnButtonClickDelegate )
		{
			onButtonClickDelegate += tOnButtonClickDelegate ;
		}

		/// <summary>
		/// ボタンをクリックされた際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnButtonClickDelegate">デリゲートメソッド</param>
		public void RemoveOnButtonClick( OnButtonClickDelegate tOnButtonClickDelegate )
		{
			onButtonClickDelegate -= tOnButtonClickDelegate ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを追加する
		/// </summary>
		/// <param name="tOnClick">リスナーメソッド</param>
		public void AddOnClickListener( UnityEngine.Events.UnityAction tOnClick )
		{
			Button tButton = _button ;
			if( tButton != null )
			{
				tButton.onClick.AddListener( tOnClick ) ;
			}
		}
		
		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを削除する
		/// </summary>
		/// <param name="tOnClick">リスナーメソッド</param>
		public void RemoveOnClickListener( UnityEngine.Events.UnityAction tOnClick )
		{
			Button tButton = _button ;
			if( tButton != null )
			{
				tButton.onClick.RemoveListener( tOnClick ) ;
			}
		}

		/// <summary>
		/// ボタンをクリックした際に呼ばれるリスナーを全て削除する
		/// </summary>
		public void RemoveOnClickAllListeners()
		{
			Button tButton = _button ;
			if( tButton != null )
			{
				tButton.onClick.RemoveAllListeners() ;
			}
		}
	
		//---------------------------------------------
		
		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIText AddLabel( string tText, uint tColor = 0xFFFFFFFF, int tFontSize = 0 )
		{
			if( label == null )
			{
				label = AddView<UIText>() ;
			}

			UIText tLabel = label ;

			//----------------------------------

			if( tFontSize <= 0 )
			{
				tFontSize  = ( int )( size.y * 0.6f ) ;
			}

			tLabel.alignment = TextAnchor.MiddleCenter ;
		
			tLabel.text = tText ;

			Font	tDefaultFont = null ;
			int		tDefaultFontSize = 0 ;
			Color	tDefaultColor = ARGB( tColor ) ;
			bool	tDefaultShadow = false ;
			bool	tDefaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultFont		= tDS.font ;
					tDefaultFontSize	= tDS.buttonLabelFontSize ;
					tDefaultColor		= tDS.buttonLabelColor ;
					tDefaultShadow		= tDS.buttonLabelShadow ;
					tDefaultOutline		= tDS.buttonLabelOutline ;
				}
			}
			
#endif

			if( tDefaultFont == null )
			{
				tLabel.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				tLabel.font = tDefaultFont ;
			}
			
			if( tDefaultFontSize == 0 )
			{
				tLabel.fontSize = tFontSize ;
			}
			else
			{
				tLabel.fontSize = tDefaultFontSize ;
			}

			tLabel.color = tDefaultColor ;

			tLabel.isShadow		= tDefaultShadow ;
			tLabel.isOutline	= tDefaultOutline ;

			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tLabel.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return tLabel ;
		}

		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UIRichText AddRichLabel( string tText, uint tColor = 0xFFFFFFFF, int tFontSize = 0 )
		{
			if( richLabel == null )
			{
				richLabel = AddView<UIRichText>() ;
			}

			UIRichText tLabel = richLabel ;

			//----------------------------------

			if( tFontSize <= 0 )
			{
				tFontSize  = ( int )( size.y * 0.6f ) ;
			}

			tLabel.alignment = TextAnchor.MiddleCenter ;
		
			tLabel.text = tText ;

			Font	tDefaultFont = null ;
			int		tDefaultFontSize = 0 ;
			Color	tDefaultColor = ARGB( tColor ) ;
			bool	tDefaultShadow = false ;
			bool	tDefaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultFont		= tDS.font ;
					tDefaultFontSize	= tDS.buttonLabelFontSize ;
					tDefaultColor		= tDS.buttonLabelColor ;
					tDefaultShadow		= tDS.buttonLabelShadow ;
					tDefaultOutline		= tDS.buttonLabelOutline ;
				}
			}
			
#endif

			if( tDefaultFont == null )
			{
				tLabel.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				tLabel.font = tDefaultFont ;
			}
			
			if( tDefaultFontSize == 0 )
			{
				tLabel.fontSize = tFontSize ;
			}
			else
			{
				tLabel.fontSize = tDefaultFontSize ;
			}

			tLabel.color = tDefaultColor ;

			tLabel.isShadow		= tDefaultShadow ;
			tLabel.isOutline	= tDefaultOutline ;


			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tLabel.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return tLabel ;
		}

		/// <summary>
		/// ラベルを追加する
		/// </summary>
		/// <param name="tText">ラベルの文字列</param>
		/// <param name="tColor">ラベルのカラー</param>
		/// <returns>UIText のインスタンス</returns>
		public UITextMesh AddLabelMesh( string tText, uint tColor = 0xFFFFFFFF, int tFontSize = 0 )
		{
			if( labelMesh == null )
			{
				labelMesh = AddView<UITextMesh>() ;
			}

			UITextMesh tLabel = labelMesh ;

			//----------------------------------
			
			if( tFontSize <= 0 )
			{
				tFontSize  = ( int )( size.y * 0.6f ) ;
			}

			tLabel.alignment = TMPro.TextAlignmentOptions.Center ;
		
			tLabel.text = tText ;

//			Font	tDefaultFont = null ;
			int		tDefaultFontSize = 0 ;
			Color	tDefaultColor = ARGB( tColor ) ;
			bool	tDefaultShadow = false ;
			bool	tDefaultOutline = true ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
//					tDefaultFont		= tDS.font ;
					tDefaultFontSize	= tDS.buttonLabelFontSize ;
					tDefaultColor		= tDS.buttonLabelColor ;
					tDefaultShadow		= tDS.buttonLabelShadow ;
					tDefaultOutline		= tDS.buttonLabelOutline ;
				}
			}
			
#endif

			// TextMeshPro ではフォントは設定できない
//			if( tDefaultFont == null )
//			{
//				tLabel.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
//			}
//			else
//			{
//				tLabel.font = tDefaultFont ;
//			}
			
			if( tDefaultFontSize == 0 )
			{
				tLabel.fontSize = tFontSize ;
			}
			else
			{
				tLabel.fontSize = tDefaultFontSize ;
			}

			tLabel.color = tDefaultColor ;

			tLabel.isShadow		= tDefaultShadow ;
			tLabel.isOutline	= tDefaultOutline ;

			return tLabel ;
		}

		/// <summary>
		/// いずれかのラベルが存在するか確認する
		/// </summary>
		/// <returns></returns>
		public bool HasAnyLabels()
		{
			if( label != null )
			{
				return true ;
			}

			if( richLabel != null )
			{
				return true ;
			}

			if( labelMesh != null )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// ラベルのテキストを設定する
		/// </summary>
		/// <param name="tLabelText"></param>
		public void SetAnyLabelsText( string tLabelText )
		{
			if( label != null )
			{
				label.text = tLabelText ;
			}

			if( richLabel != null )
			{
				richLabel.text = tLabelText ;
			}

			if( labelMesh != null )
			{
				labelMesh.text = tLabelText ;
			}
		}


		//---------------------------------------------------------------------------
		
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool interactable
		{
			get
			{
				Button tButton = _button ;
				if( tButton == null )
				{
					return false ;
				}
				return tButton.interactable ;
			}
			set
			{
				Button tButton = _button ;
				if( tButton == null )
				{
					return ;
				}
				tButton.interactable = value ;

				if( disableMask != null )
				{
					disableMask.SetActive( ! value ) ;
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="tARGB">カラー値(AARRGGBB)</param>
		public void On( uint tARGB = 0xFFFFFFFF )
		{
			interactable = true ;
			if( ( tARGB & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( tARGB >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( tARGB >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   tARGB         & 0xFF ) ;
				byte a = ( byte )( ( tARGB >> 24 ) & 0xFF ) ;

				Image tImage = _image ;
				if( tImage != null )
				{
					tImage.color = new Color32( r, g, b, a ) ;
					if( colorTransmission == true )
					{
						SetColorOfChildren( tImage.color ) ;
					}
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション有効化
		/// </summary>
		/// <param name="tARGB">カラー値(AARRGGBB)</param>
		public void Off( uint tARGB = 0xC0C0C0C0 )
		{
			interactable = false ;
			if( ( tARGB & 0xFF000000 ) != 0 )
			{
				byte r = ( byte )( ( tARGB >> 16 ) & 0xFF ) ;
				byte g = ( byte )( ( tARGB >>  8 ) & 0xFF ) ;
				byte b = ( byte )(   tARGB         & 0xFF ) ;
				byte a = ( byte )( ( tARGB >> 24 ) & 0xFF ) ;

				Image tImage = _image ;
				if( tImage != null )
				{
					tImage.color = new Color32( r, g, b, a ) ;
					if( colorTransmission == true )
					{
						SetColorOfChildren( tImage.color ) ;
					}
				}
			}
		}

		/// <summary>
		/// 特殊なインタラクション設定
		/// </summary>
		/// <param name="tState"></param>
		public void SetInteractable( bool tState, uint tARGB = 0 )
		{
			if( tState == true )
			{
				if( tARGB == 0 )
				{
					tARGB = 0xFFFFFFFF ;
				}
				On( tARGB ) ;
			}
			else
			{
				if( tARGB == 0 )
				{
					tARGB = 0xC0C0C0C0 ;
				}
				Off( tARGB ) ;
			}
		}

		public void SetColorOfChildren( Color tColor )
		{
			int i, l ;

			Image[] tImage = transform.GetComponentsInChildren<Image>( true ) ;
			if( tImage != null && tImage.Length >  0 )
			{
				l = tImage.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tImage[ i ].transform != transform )
					{
						tImage[ i ].color = tColor;
					}
				}
			}

/*			Text[] tText = transform.GetComponentsInChildren<Text>( true ) ;
			if( tText != null && tText.Length >  0 )
			{
				l = tText.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tText[ i ].transform != transform )
					{
						tText[ i ].color = tColor ;
					}
				}
			}

			RichText[] tRichText = transform.GetComponentsInChildren<RichText>( true ) ;
			if( tRichText != null && tRichText.Length >  0 )
			{
				l = tRichText.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tRichText[ i ].transform != transform )
					{
						tRichText[ i ].color = tColor ;
					}
				}
			}*/
		}

		override protected void OnUpdate()
		{
			if( m_ButtonClick == true && m_ButtonClickCountTime != Time.frameCount )
			{
				m_ButtonClick = false ;
			}

			if( colorTransmission == true )
			{
				if( _image != null )
				{
					Image tImage = _image ;
					if( tImage.color.Equals( m_ActiveColor ) == false )
					{
						m_ActiveColor.r = tImage.color.r ;
						m_ActiveColor.g = tImage.color.g ;
						m_ActiveColor.b = tImage.color.b ;
						m_ActiveColor.a = tImage.color.a ;

						SetColorOfChildren( m_ActiveColor ) ;
					}
				}
			}
		}
	}
}
