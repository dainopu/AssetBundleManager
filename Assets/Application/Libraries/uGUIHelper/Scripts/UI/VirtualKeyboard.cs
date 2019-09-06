using UnityEngine ;
using System ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// バーチャルキーボード制御クラス
	/// </summary>
	public class VirtualKeyboard : MonoBehaviour
	{
		// シングルトンインスタンス
		private static VirtualKeyboard m_Instance = null ; 
		public  static VirtualKeyboard   instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		//---------------------------------------------------------
	
		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <param name="tRunInbackground"></param>
		/// <returns></returns>
		public static VirtualKeyboard Create()
		{
			if( m_Instance != null )
			{
	//			Debug.Log( "生成済み:AudioManager" ) ;
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( VirtualKeyboard ) ) as VirtualKeyboard ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "VirtualKeyboard" ) ;
				tGameObject.AddComponent<VirtualKeyboard>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// インスタンスを取得する
		/// </summary>
		/// <returns></returns>
//		public static VirtualKeyboard Get()
//		{
//			return m_Instance ;
//		}
	
		/// <summary>
		/// インスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{			
				if( Application.isPlaying == false )
				{
					DestroyImmediate( m_Instance.gameObject ) ;
				}
				else
				{
					Destroy( m_Instance.gameObject ) ;
				}
			
				m_Instance = null ;
			}
		}
	
		//-----------------------------------------------------------------
	
		void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			VirtualKeyboard tInstanceOther = GameObject.FindObjectOfType( typeof( VirtualKeyboard ) ) as VirtualKeyboard ;
			if( tInstanceOther != null )
			{
				if( tInstanceOther != this )
				{
					GameObject.DestroyImmediate( gameObject ) ;
					return ;
				}
			}
		
			//-----------------------------
			// Awake 内でマニュアルで実行した場合とスクリプトで実行した場合の共通の処理を行う必要がある
		
			m_Instance = this ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------
		}

		void Start()
		{
		}
	
		void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}

		//-------------------------------------------------------------------------------------

		public enum KeyboardType
		{
			Default					= 0,	// デフォルト
			ASCIICapable			= 1,	// ASCII配列のキーボード
			NumbersAndPunctuation	= 2,	// 数字と句読点が含まれるキーボード
			URL						= 3,	// URL入力を行うキーボード
			NumberPad				= 4,	// テンキーキーボード
			PhonePad				= 5,	// 電話番号入力を行うキーボード
			NamePhonePad			= 6,	// 電話番号と文字入力を行うキーボード
			EmailAddress			= 7,	// @を含んだメールアドレスを入力するキーボード
		}

		public enum State
		{
			Unknown					=  0,
			ValueChanged			=  1,	// 文字列が変化した
			Done					=  2,	// 文字列が決定された
			Cancel					=  3,	// 入力がキャンセルさせれた
		}


		/// <summary>
		/// バーチャルキーボードを開く
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="tPlaceholder"></param>
		/// <param name="tKeyboardType"></param>
		/// <param name="tIsMultiLine"></param>
		/// <param name="tIsSecureCode"></param>
		/// <param name="tOnValueChaned"></param>
		/// <returns></returns>
		public static bool Open( string tText, string tTextPlaceholder, KeyboardType tKeyboardType, bool tIsMultiLine, bool tIsSecure, bool tHideInput, Action<State,string> tOnValueChaned )
		{
			VirtualKeyboard tInstane = Create() ;
			if( tInstane == null )
			{
				return false ;
			}

			tInstane.Open_Private( tText, tTextPlaceholder, tKeyboardType, tIsMultiLine, tIsSecure, tHideInput, tOnValueChaned ) ;

			return true ;
		}

		private Action<State,string>	m_OnValueChanged = null ;
		private string					m_InitialText = "" ;
		private string					m_ActiveText = "" ;



#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

		private TouchScreenKeyboard m_VirtualKeyboard = null ;
		
#else
		private UIImage					m_Base = null ;
#endif

		private void Open_Private( string tText, string tTextPlaceholder, KeyboardType tKeyboardType, bool tIsMultiLine, bool tIsSecure, bool tHideInput, Action<State,string> tOnValueChaned )
		{

			// コールバックを登録する
			m_OnValueChanged = tOnValueChaned ;

			// 初期の文字列を保存する
			m_InitialText = tText ;

			// 現在の文字列を保存する
			m_ActiveText = tText ;

			//-------------------------------------------------

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

			// 実機
			TouchScreenKeyboardType	tMobileKeyboardType = TouchScreenKeyboardType.Default ;
			if( tKeyboardType == KeyboardType.Default )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.Default ;
			}
			else
			if( tKeyboardType == KeyboardType.ASCIICapable )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.ASCIICapable ;
			}
			else
			if( tKeyboardType == KeyboardType.NumbersAndPunctuation )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.NumbersAndPunctuation ;
			}
			else
			if( tKeyboardType == KeyboardType.URL )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.URL ;
			}
			else
			if( tKeyboardType == KeyboardType.NumberPad )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.NumberPad ;
			}
			else
			if( tKeyboardType == KeyboardType.PhonePad )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.PhonePad ;
			}
			else
			if( tKeyboardType == KeyboardType.NamePhonePad )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.NamePhonePad ;
			}
			else
			if( tKeyboardType == KeyboardType.EmailAddress )
			{
				tMobileKeyboardType = TouchScreenKeyboardType.EmailAddress ;
			}

			TouchScreenKeyboard.hideInput = tHideInput ;

			m_VirtualKeyboard = TouchScreenKeyboard.Open
			(
				tText,
				tMobileKeyboardType,
				true,
				tIsMultiLine,
				tIsSecure,
				false,
				tTextPlaceholder
			) ;


#else
			// ＰＣ

			// フルスクリーンのキャンバスを生成する
			UICanvas tCanvas = UICanvas.CreateWithCamera( transform, 0, 0 ) ;


			// キャンバスのカメラを取得する
			Camera tCamera = tCanvas.GetCanvasCamera() ;

			// キャンバスのカメラをかなり手前に設定する
			tCamera.depth = 32767 ;

			// カメラを塗りつぶさないようにする
			tCamera.clearFlags = CameraClearFlags.Nothing ;

			// 全画面用のイメージを追加する
			UIImage tScreen = tCanvas.AddView<UIImage>( "Virtaul Keyboard Screen" ) ;
			tScreen.SetAnchorToStretch() ;	// 全画面表示にする
			tScreen.isMask = true ;
			tScreen._mask.showMaskGraphic = false ;
				
			Vector2 tCanvasSize = tScreen.GetCanvasSize() ;

			float cw = tCanvasSize.x ; // tCanvas._w ;
			float ch = tCanvasSize.y ; // tCanvas._h ;

			// 最も基本的な縦の長さ
			float bw = cw ;
			float bh = ch * 0.1f ;

			int tFontSize = ( int )( bh * 0.4f ) ;

			//---------------------------------------------------------

			// 土台部分を作る
			UIImage tBase = tScreen.AddView<UIImage>( "Base" ) ;
			tBase.SetAnchorToStretchBottom() ;
			tBase.SetPivotToCenterBottom() ;

			if( tIsMultiLine == false )
			{
				// シングル
				tBase._h = bh * 2 ;
			}
			else
			{
				// マルチ
				tBase._h = bh * 6 ;
			}

			// テキスト入力部を作る
			string tOption = "" ;
			if( tIsMultiLine == true )
			{
				tOption = "MultiLine" ;
			}
			UIInputField tInputField = tBase.AddView<UIInputField>( "Input", tOption ) ;
			tInputField.SetAnchorToStretchTop() ;
			tInputField.SetPivotToCenterTop() ;
			tInputField.SetMarginX( 2, 2 ) ;
			tInputField._y = -2 ;

			tInputField.text = tText ;
			tInputField.placeholder.text = tTextPlaceholder ;

			if( tIsMultiLine == false )
			{
				tInputField._h = ( bh * 1 ) - 4 ;
			}
			else
			{
				tInputField._h = ( bh * 5 ) - 4 ;

				// デフォルトでニューラインになっているのでサブミットに変える
				tInputField.lineType = UnityEngine.UI.InputFieldPlus.LineType.MultiLineSubmit ;
			}
			tInputField.SetFontSize( tFontSize ) ;

			if( tKeyboardType == KeyboardType.Default )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Standard ;
			}
			else
			if( tKeyboardType == KeyboardType.ASCIICapable )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Alphanumeric ;
			}
			else
			if( tKeyboardType == KeyboardType.NumbersAndPunctuation )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.IntegerNumber ;
			}
			else
			if( tKeyboardType == KeyboardType.URL )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Autocorrected ;
			}
			else
			if( tKeyboardType == KeyboardType.NumberPad )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.DecimalNumber ;
			}
			else
			if( tKeyboardType == KeyboardType.PhonePad )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Pin ;
			}
			else
			if( tKeyboardType == KeyboardType.NamePhonePad )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Name ;
			}
			else
			if( tKeyboardType == KeyboardType.EmailAddress )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.EmailAddress ;
			}

			if( tIsSecure == true )
			{
				tInputField.contentType = UnityEngine.UI.InputFieldPlus.ContentType.Password ;
			}

			// 状態が変化した際に呼び出される
			tInputField.SetOnValueChanged( OnValueChanged ) ;

			// リターンキーによる決定時に呼び出される
		//	tInputField.SetOnEndEditDelegate( OnEndEdit ) ;
			tInputField.SetOnEnterKeyPressed( OnEndEdit ) ;


			// ＯＫボタンを作る
			UIButton tButton_OK = tBase.AddView<UIButton>( "OK" ) ;
			tButton_OK.SetAnchorToLeftBottom() ;
			tButton_OK.SetSize( bw * 0.5f - 4, bh - 4 ) ;
			tButton_OK.SetPosition( bw * 0.25f, bh * 0.5f ) ;
			tButton_OK.AddLabel( "OK", 0xFFFFFFFF, tFontSize ) ;
			tButton_OK.SetOnButtonClick( OnClick ) ;

			// ＣＡＮＣＥＬボタンを作る
			UIButton tButton_Cancel = tBase.AddView<UIButton>( "Cancel" ) ;
			tButton_Cancel.SetAnchorToRightBottom() ;
			tButton_Cancel.SetSize( bw * 0.5f - 4, bh - 4 ) ;
			tButton_Cancel.SetPosition( - bw * 0.25f, bh * 0.5f ) ;
			tButton_Cancel.AddLabel( "CANCEL", 0xFFFFFFFF, tFontSize ) ;
			tButton_Cancel.SetOnButtonClick( OnClick ) ;

			float tH = tBase._h ;

			UITween tTweenUp = tBase.AddTween( "Up" ) ;
			tTweenUp.delay = 0 ;
			tTweenUp.duration = 0.25f ;
			tTweenUp.positionEnabled = true ;
			tTweenUp.positionEaseType = UITween.EaseType.easeOutQuad ;
			tTweenUp.positionFromY = - tH ;
			tTweenUp.playOnAwake = false ;

			UITween tTweenDown = tBase.AddTween( "Down" ) ;
			tTweenDown.delay = 0 ;
			tTweenDown.duration = 0.25f ;
			tTweenDown.positionEnabled = true ;
			tTweenDown.positionEaseType = UITween.EaseType.easeOutQuad ;
			tTweenDown.positionToY = - tH ;
			tTweenDown.playOnAwake = false ;

			tTweenDown.SetOnFinished( ( string tIdentity, UITween tTween ) =>
			{
				Destroy( gameObject ) ;
			} ) ;

			tBase.PlayTween( "Up" ) ;

			m_Base = tBase ;
#endif

		}



		void Update()
		{
			// ソースを監視して停止している（予定）
#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )

			if( m_VirtualKeyboard != null )
			{
				State tState = State.Unknown ;

				string tActiveText = m_VirtualKeyboard.text ;

				if( m_VirtualKeyboard.status != TouchScreenKeyboard.Status.Done )
				{
					if( m_ActiveText != tActiveText )
					{
						m_ActiveText  = tActiveText ;
						tState = State.ValueChanged ;
					}
				}

				if( m_VirtualKeyboard.status == TouchScreenKeyboard.Status.Done )
				{
					if( m_VirtualKeyboard.status != TouchScreenKeyboard.Status.Canceled )
					{
						// キャンセルではない
						m_ActiveText = tActiveText ;
						tState = State.Done ;
					}
					else
					{
						// キャンセルされた
						m_ActiveText = m_InitialText ;
						tState = State.Cancel ;
					}
				}
				else
				if( m_VirtualKeyboard.status == TouchScreenKeyboard.Status.Canceled || m_VirtualKeyboard.active == false )
				{
					// キャンセルされた
					m_ActiveText = m_InitialText ;
					tState = State.Cancel ;
				}

				if( tState != State.Unknown )
				{
					// コールバック呼び出し
					if( m_OnValueChanged != null )
					{
						m_OnValueChanged( tState, m_ActiveText ) ;
					}

					if( tState != State.ValueChanged )
					{
						// バーチャルキーボードが閉じられた
						m_VirtualKeyboard = null ;

						Destroy( gameObject ) ;
					}
				}
			}
#endif		
		}

		//-------------------------------------------------------------------------------------

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS )
#else
		// 状態が変化した
		private void OnValueChanged( string tIdentity, UIView View, string tValue )
		{
			m_ActiveText = tValue ;
			CallHandler( State.ValueChanged ) ;
		}

		// リターンキーが押された
		private void OnEndEdit( string tIdentity, UIInputField tInputField, string tValue )
		{
			m_ActiveText = tValue ;
			CallHandler( State.Done ) ;
		}

		// ボタンが押された
		private void OnClick( string tIdentity, UIButton tButton )
		{
			if( tIdentity == "OK" )
			{
				CallHandler( State.Done ) ;
			}
			else
			if( tIdentity == "Cancel" )
			{
				CallHandler( State.Cancel ) ;
			}
		}

		// コールバック呼び出し
		private void CallHandler( State tState )
		{
			if( tState == State.Cancel )
			{
				m_ActiveText = m_InitialText ;
			}

			if( m_OnValueChanged != null )
			{
				m_OnValueChanged( tState, m_ActiveText ) ;
			}

			if( tState != State.ValueChanged )
			{
				m_Base.PlayTween( "Down" ) ;
			}
		}
#endif

	}
}
