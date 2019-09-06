using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:InputField クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
	[RequireComponent(typeof(UnityEngine.UI.InputFieldPlus))]	
	public class UIInputField : UIImage
	{
		/// <summary>
		/// interactable(ショーシカット)
		/// </summary>
		public bool interactable
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return false ;
				}
				return tInputField.interactable ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.interactable = value ;
			}
		}

		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string text
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return "" ;
				}

				return tInputField.text ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.text = value ;
			}
		}
		
		/// <summary>
		/// キャラクターリミット(ショートカット)
		/// </summary>
		public int characterLimit
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return 0 ;
				}
				return tInputField.characterLimit ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.characterLimit = value ;
			}
		}
	
		/// <summary>
		/// コンテントタイプ(ショートカット)
		/// </summary>
		public InputFieldPlus.ContentType contentType
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return InputFieldPlus.ContentType.Standard ;
				}
				return tInputField.contentType ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}

				if( tInputField.contentType != value )
				{
					tInputField.contentType = value ;

					string tText = tInputField.text ;
					tInputField.text = "" ;
					tInputField.text = tText ;
				}
			}
		}

		/// <summary>
		/// ラインタイプ(ショートカット)
		/// </summary>
		public InputFieldPlus.LineType lineType
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return InputFieldPlus.LineType.SingleLine ;
				}
				return tInputField.lineType ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.lineType = value ;
			}
		}

		/// <summary>
		/// テキストコンポーネント(ショートカット)
		/// </summary>
		public UIText textComponent
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return null ;
				}
				return tInputField.textComponent.GetComponent<UIText>() ;
			}
		}

		/// <summary>
		/// プレースホルダー(ショートカット)
		/// </summary>
		public UIText placeholder
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return null ;
				}
				return tInputField.placeholder.GetComponent<UIText>() ;
			}
		}

		/// <summary>
		/// キャレットブリンクレート(ショートカット)
		/// </summary>
		public float caretBlinkRate
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return 0 ;
				}
				return tInputField.caretBlinkRate ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.caretBlinkRate = value ;
			}
		}

		/// <summary>
		/// キャレットウィドス(ショートカット)
		/// </summary>
		public int caretWidth
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return 0 ;
				}
				return tInputField.caretWidth ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.caretWidth = value ;
			}
		}

		/// <summary>
		/// カスタムキャレットカラー(ショートカット)
		/// </summary>
		public bool customCaretColor
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return false ;
				}
				return tInputField.customCaretColor ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.customCaretColor = value ;
			}
		}

		/// <summary>
		/// キャレットカラー(ショートカット)
		/// </summary>
		public Color caretColor
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return Color.black ;
				}
				return tInputField.caretColor ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.caretColor = value ;
			}
		}

		/// <summary>
		/// セレクションカラー(ショートカット)
		/// </summary>
		public Color selectionColor
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return Color.black ;
				}
				return tInputField.selectionColor;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.selectionColor = value ;
			}
		}

		/// <summary>
		/// ハイドモバイルインプット(ショートカット)
		/// </summary>
		public bool hideMobileInput
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return false ;
				}
				return tInputField.shouldHideMobileInput ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.shouldHideMobileInput = value ;
			}
		}


		/// <summary>
		/// リードオンリー(ショートカット)
		/// </summary>
		public bool readOnly
		{
			get
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return false ;
				}
				return tInputField.readOnly ;
			}
			set
			{
				InputFieldPlus tInputField = _inputField ;
				if( tInputField == null )
				{
					return ;
				}
				tInputField.readOnly = value ;
			}
		}

		/// <summary>
		/// フォントサイズを設定する
		/// </summary>
		/// <param name="tFontSize"></param>
		public void SetFontSize( int tFontSize )
		{
			if( textComponent != null )
			{
				textComponent.fontSize = tFontSize ;
			}

			if( placeholder != null )
			{
				placeholder.fontSize = tFontSize ;
			}
		}

		public FontFilter	fontFilter = null ;
		public char			fontAlternateCode = ( char )0 ;

		//-----------------------------------

		/// <summary>
		/// フォーカスを持たせる
		/// </summary>
		public bool Activate()
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField == null )
			{
				return false ;
			}
			tInputField.ActivateInputField() ;

			return true ;
		}
		
		//-------------------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			InputFieldPlus tInputField = _inputField ;

			if( tInputField == null )
			{
				tInputField = gameObject.AddComponent<InputFieldPlus>() ;
			}
			if( tInputField == null )
			{
				// 異常
				return ;
			}

			Image tImage = _image ;
			if( tImage != null )
			{
				tInputField.targetGraphic = tImage ;
			}

			//-------------------------------
			
			bool tIsMultiLine = false ;
			if( string.IsNullOrEmpty( tOption ) == false && tOption.ToLower() == "multiline" )
			{
				// マルチ
				tIsMultiLine = true ;
			}


			Vector2 tSize = GetCanvasSize() ;

			int tFontSize = 16 ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				if( tIsMultiLine == false )
				{
					// シングル
					SetSize( tSize.y * 0.5f, tSize.y * 0.1f ) ;
				}
				else
				{
					// マルチ
					SetSize( tSize.y * 0.5f, tSize.y * 0.5f ) ;
				}

				tFontSize = ( int )( tSize.y * 0.1f * 0.6f ) ;
			}
				
			// Image
			tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultFrame" ) ;
			tImage.type = Image.Type.Sliced ;
				
			if( isCanvasOverlay == true )
			{
				tImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
				
			// Text
			UIText tTextComponent = AddView<UIText>( "Text", "SIMPLE" ) ;
			tTextComponent.isContentSizeFitter = false ;
			tTextComponent.fontSize = tFontSize ;
			tTextComponent.supportRichText = false ;
			tTextComponent.color = new Color32(  50,  50,  50,  255 ) ;
			tTextComponent.SetAnchorToStretch() ;
			tTextComponent.SetMargin( 12, 12, 12, 12 ) ;
//			tText.position = new Vector2( 0, -2 ) ;
//			tText.SetSize( -24, -28 ) ;
//			tText.resizeTextForBestFit = true ;
			tInputField.textComponent = tTextComponent._text ;
			if( tIsMultiLine == false )
			{
				tTextComponent.alignment = TextAnchor.MiddleLeft ;
			}
			else
			{
				tTextComponent.alignment = TextAnchor.UpperLeft ;
			}
				
			if( isCanvasOverlay == true )
			{
				tTextComponent.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			// TextColorModifier
			tTextComponent.AddComponent<TextColorModifier>() ;

			// Placeholder
			UIText tPlaceholder = AddView<UIText>( "Placeholder", "SIMPLE" ) ;
			tPlaceholder.fontSize = tFontSize ;
			tPlaceholder.fontStyle = FontStyle.Italic ;
			tPlaceholder.text = "Enter text..." ;
			tPlaceholder.color = new Color32(  50,  50,  50, 128 ) ;
			tPlaceholder.SetAnchorToStretch() ;
			tPlaceholder.SetMargin( 12, 12, 12, 12 ) ;
//			tPlaceholder.position = new Vector2( 0, -2 ) ;
//			tPlaceholder.SetSize( -24, -28 ) ;
//			tPlaceholder.resizeTextForBestFit = true ;
			tInputField.placeholder = tPlaceholder._text ;
			if( tIsMultiLine == false )
			{
				tPlaceholder.alignment = TextAnchor.MiddleLeft ;
			}
			else
			{
				tPlaceholder.alignment = TextAnchor.UpperLeft ;
			}

			if( isCanvasOverlay == true )
			{
				tPlaceholder.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}


			if( tIsMultiLine == true )
			{
				// マルチラインで生成する
				tInputField.lineType = InputFieldPlus.LineType.MultiLineNewline ;
				tInputField.textComponent.horizontalOverflow = HorizontalWrapMode.Wrap ;
			}

			tInputField.caretWidth = 4 ;
			tInputField.customCaretColor = true ;
			tInputField.caretColor = Color.blue ;

			//----------------------------------------------------------

			FontFilter	tFontFilter = null ;
			char		tFontAlternateCode = '？' ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tFontFilter			= tDS.fontFilter ;
					tFontAlternateCode	= tDS.fontAlternateCode ;
				}
			}
			
#endif
			if( tFontFilter == null )
			{

			}
			else
			{
				fontFilter = tFontFilter ;
			}

			if( tFontAlternateCode == 0 )
			{
				fontAlternateCode = '？' ;
			}
			else
			{
				fontAlternateCode = tFontAlternateCode ;
			}
		}

		private UITextColor m_TextColor = null ;

		// 派生クラスの Start
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _inputField != null )
				{
					_inputField.onValueChanged.AddListener( OnValueChangedInner ) ;
					_inputField.onEndEdit.AddListener( OnEndEditInner ) ;

					if( _inputField.textComponent != null )
					{
						m_TextColor = _inputField.textComponent.GetComponent<UITextColor>() ;
						if( m_TextColor == null )
						{
							m_TextColor = _inputField.textComponent.gameObject.AddComponent<UITextColor>() ;
						}
					}

				}
			}
		}

		// キャレットのＶＲモード対応
		private LayoutElement m_LE = null ;

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			if( Application.isPlaying == true )
			{
				// キャレットが表示されなくなるバグ対策
				if( _inputField != null && m_LE == null )
				{
					m_LE =	_inputField.GetComponentInChildren<LayoutElement>() ;
					if( m_LE != null )
					{
						CanvasRenderer tCR0 = m_LE.GetComponent<CanvasRenderer>() ;
						CanvasRenderer tCR1 = _inputField.GetComponent<CanvasRenderer>() ;
						if( tCR0 != null && tCR1 != null )
						{
							tCR0.SetMaterial( tCR1.GetMaterial(), 0 ) ;
						}
					}
				}
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> onValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDelegate( string tIdentity, UIView tView, string tValue ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDelegate onValueChangedDelegate ;
		
		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIInputField, string> tOnValueChangedAction )
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
		private void OnValueChangedInner( string tValue )
		{
//			Debug.LogWarning( "状態変化:" + tValue + " : " + Input.compositionString + " : " + Input.inputString ) ;

			if( fontFilter != null && fontAlternateCode != 0 && string.IsNullOrEmpty( tValue ) == false )
			{
				// 内部の文字を検査して表示できない文字が入っていたら強制的に補正する
				int i, l = tValue.Length ;
				char[] s = new char[ l ] ;
				int c ;
				bool tDirty = false ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = tValue[ i ] ;

					if( _inputField.lineType == InputFieldPlus.LineType.MultiLineNewline && ( c == 0x0D || c == 0x0A ) )
					{
						// 改行コードは許容する
						s[ i ] = tValue[ i ] ;
						tDirty = true ;
					}
					else
					{
						if( ( fontFilter.flag[ c >> 3 ] & ( 1 << ( c & 0x0007 ) ) ) == 0 )
						{
							// この文字は置き換える必要がある
							s[ i ] = fontAlternateCode ;
						}
						else
						{
							s[ i ] = tValue[ i ] ;
							tDirty = true ;
						}
					}
				}

				if( tDirty == true )
				{
					_inputField.text = new string( s ) ;
				}
			}

			//----------------------------------------------------------

			//----------------------------------------------------------

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
	
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangeListener( UnityEngine.Events.UnityAction<string> tOnValueChanged )
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onValueChanged.AddListener( tOnValueChanged ) ;
			}
		}

		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangeListener( UnityEngine.Events.UnityAction<string> tOnValueChanged )
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onValueChanged.RemoveListener( tOnValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangeAllListeners()
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onValueChanged.RemoveAllListeners() ;
			}
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> onEndEditAction ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnEndEditDelegate( string tIdentity, UIInputField tView, string tValue ) ;

		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲート
		/// </summary>
		public OnEndEditDelegate onEndEditDelegate ;

		/// <summary>
		/// 入力が終了した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnEndEditAction">アクションメソッド</param>
		public void SetOnEndEdit( Action<string, UIInputField, string> tOnEndEditAction )
		{
			onEndEditAction = tOnEndEditAction ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnEndEditDelegate">デリゲートメソッド</param>
		public void AddOnEndEdit( OnEndEditDelegate tOnEndEditDelegate )
		{
			onEndEditDelegate += tOnEndEditDelegate ;
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnEndEditDelegate">デリゲートメソッド</param>
		public void RemoveOnEndEdit( OnEndEditDelegate tOnEndEditDelegate )
		{
			onEndEditDelegate -= tOnEndEditDelegate ;
		}
		
		//---------------------------

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクション
		/// </summary>
		public Action<string, UIInputField, string> onEnterKeyPressedAction ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnEnterKeyPressedDelegate( string tIdentity, UIInputField tView, string tValue ) ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるデリゲート
		/// </summary>
		public OnEnterKeyPressedDelegate onEnterKeyPressedDelegate ;

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを設定する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyAction">アクションメソッド</param>
		public void SetOnEnterKeyPressed( Action<string, UIInputField, string> tOnEnterKeyPressedAction )
		{
			onEnterKeyPressedAction = tOnEnterKeyPressedAction ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを追加する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyDelegate">デリゲートメソッド</param>
		public void AddOnEnterKeyPressed( OnEnterKeyPressedDelegate tOnEnterKeyPressedDelegate )
		{
			onEnterKeyPressedDelegate += tOnEnterKeyPressedDelegate ;
		}

		/// <summary>
		/// エンターキーが押された際に呼び出されるアクションを削除する(ただしフォーカルを持っている必要がある)
		/// </summary>
		/// <param name="tOnPressEnterKeyDelegate">デリゲートメソッド</param>
		public void RemoveOnEnterKeyPressed( OnEnterKeyPressedDelegate tOnEnterKeyPressedDelegate )
		{
			onEnterKeyPressedDelegate -= tOnEnterKeyPressedDelegate ;
		}

		//---------------------------
		
		// 内部リスナー
		private void OnEndEditInner( string tValue )
		{
			if( onEndEditAction != null || onEndEditDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( onEndEditAction != null )
				{
					onEndEditAction( identity, this, tValue ) ;
				}

				if( onEndEditDelegate != null )
				{
					onEndEditDelegate( identity, this, tValue ) ;
				}
			}

			if( Input.GetKey( KeyCode.Return ) == true )
			{
				if( onEnterKeyPressedAction != null || onEnterKeyPressedDelegate != null )
				{
					string identity = Identity ;
					if( string.IsNullOrEmpty( identity ) == true )
					{
						identity = name ;
					}

					if( onEnterKeyPressedAction != null )
					{
						onEnterKeyPressedAction( identity, this, tValue ) ;
					}

					if( onEnterKeyPressedDelegate != null )
					{
						onEnterKeyPressedDelegate( identity, this, tValue ) ;
					}
				}
			}
		}

		//---------------------------------------------
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnEndEdit">リスナーメソッド</param>
		public void AddOnEndEditListener( UnityEngine.Events.UnityAction<string> tOnEndEdit )
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onEndEdit.AddListener( tOnEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnEndEdit">リスナーメソッド</param>
		public void RemoveOnEndEditListener( UnityEngine.Events.UnityAction<string> tOnEndEdit )
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onEndEdit.RemoveListener( tOnEndEdit ) ;
			}
		}
		
		/// <summary>
		/// 入力が終了した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnEndEditAllListeners()
		{
			InputFieldPlus tInputField = _inputField ;
			if( tInputField != null )
			{
				tInputField.onEndEdit.RemoveAllListeners() ;
			}
		}
	}
}

