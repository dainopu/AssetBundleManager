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
	/// Transition コンポーネントクラス
	/// </summary>
	public class UITransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>
		/// ボタン状態
		/// </summary>
		public enum State
		{
			Normal		= 0,
			Highlighted	= 1,
			Pressed		= 2,
			Disabled	= 3,
			Clicked		= 4,
			Finished	= 5,
		}
	
		/// <summary>
		/// カーブの種別
		/// </summary>
		public enum ProcessType
		{
			Ease = 0,
			AnimationCurve = 1,
		}

		/// <summary>
		/// イーズの種別
		/// </summary>
		public enum EaseType
		{
			easeInQuad,
			easeOutQuad,
			easeInOutQuad,
			easeInCubic,
			easeOutCubic,
			easeInOutCubic,
			easeInQuart,
			easeOutQuart,
			easeInOutQuart,
			easeInQuint,
			easeOutQuint,
			easeInOutQuint,
			easeInSine,
			easeOutSine,
			easeInOutSine,
			easeInExpo,
			easeOutExpo,
			easeInOutExpo,
			easeInCirc,
			easeOutCirc,
			easeInOutCirc,
			linear,
			spring,
			easeInBounce,
			easeOutBounce,
			easeInOutBounce,
			easeInBack,
			easeOutBack,
			easeInOutBack,
			easeInElastic,
			easeOutElastic,
			easeInOutElastic,
	//		punch
		}

		/// <summary>
		/// トランジョン情報クラス
		/// </summary>
		[System.Serializable]
		public class Transition
		{
			public Sprite	sprite = null ;
			public ProcessType processType = ProcessType.Ease ;

	//		public Vector3 fadePosition = Vector3.zero ;
			public Vector3 fadeRotation = Vector3.zero ;
			public Vector3 fadeScale    = Vector3.one ;

			public EaseType fadeEaseType = EaseType.linear ;
			public float fadeDuration = 0.2f ;

			public AnimationCurve fadeAnimationCurve = AnimationCurve.Linear(  0, 0, 0.5f, 1 ) ;

			public Transition( int tState )
			{
				if( tState == 0 )
				{
					fadeEaseType = EaseType.easeOutBack ;
				}
				else
				if( tState == 1 )
				{
					fadeScale = new Vector3( 1.05f, 1.05f, 1.05f ) ;
				}
				else
				if( tState == 2 )
				{
					fadeScale = new Vector3( 0.95f, 0.95f, 0.95f ) ;
				}
				else
				if( tState == 4 )
				{
					fadeScale = new Vector3( 1.25f, 1.25f, 1.0f ) ;
					fadeEaseType = EaseType.easeOutBounce ;
					fadeDuration = 0.25f ;
				}
				else
				if( tState == 5 )
				{
					fadeScale = new Vector3( 1.0f, 1.0f, 1.0f ) ;
					fadeEaseType = EaseType.linear ;
					fadeDuration = 0.1f ;
				}
			}
		}



//		public Transition[] transition = new Transition[ 5 ]{ new Transition( 0 ), new Transition( 1 ), new Transition( 2 ), new Transition( 3 ),  new Transition( 4 ) } ;
		
		[SerializeField][HideInInspector]
		private List<Transition> m_Transition = new List<Transition>()
		{
			new Transition( 0 ),	// Normal
			new Transition( 1 ),	// Hilighted
			new Transition( 2 ),	// Pressed
			new Transition( 3 ),	// Disabled
			new Transition( 4 ),	// Clicked
			new Transition( 5 ),	// Finished
			new Transition( 6 ),
			new Transition( 7 ),
		} ;
		
		public List<Transition> transition
		{
			get
			{
				return m_Transition ;
			}
		}


		/// <summary>
		/// スプライトをアトラス画像で動的に変更する
		/// </summary>
		public bool spriteOverwriteEnabled = false ;

		// 選択中のタイプ
		[SerializeField][HideInInspector]
		private State m_EditingState = State.Pressed ;
		public  State   editingState
		{
			get
			{
				return m_EditingState ;
			}
			set
			{
				m_EditingState = value ;
			}
		}

		[SerializeField][HideInInspector]
		private bool m_TransitionFoldOut = true ;

		/// <summary>
		/// トランジションのホールド時間
		/// </summary>
		public  bool  transitionFoldOut
		{
			get
			{
				return m_TransitionFoldOut ;
			}
			set
			{
				m_TransitionFoldOut = value ;
			}
		}

		// トランジションを有効にするかどうか
		[SerializeField][HideInInspector]
		private bool m_TransitionEnabled = true ;

		/// <summary>
		/// トランジションの有効無効の設定
		/// </summary>
		public  bool  transitionEnabled
		{
			get
			{
				return m_TransitionEnabled ;
			}
			set
			{
				m_TransitionEnabled = value ;
			}
		}

		[SerializeField][HideInInspector]
		private bool m_PauseAfterFinished = true ;

		/// <summary>
		/// フィニッシュの後に動作をポーズするかどうか
		/// </summary>
		public bool pauseAfterFinished
		{
			get
			{
				return m_PauseAfterFinished ;
			}
			set
			{
				m_PauseAfterFinished = value ;
			}
		}


		public bool colorTransmission = false ;


		// ロック状態
		private bool m_Pausing = false ;

		public bool isPauseing
		{
			get
			{
				return m_Pausing ;
			}
			set
			{
				m_Pausing = value ;
			}
		}



		private bool m_Hover = false ;

		/// <summary>
		/// ホバー状態
		/// </summary>
		public  bool  isHover
		{
			get
			{
				return m_Hover ;
			}
		}

		private bool m_Press = false ;

		/// <summary>
		/// プレス状態
		/// </summary>
		public  bool  isPress
		{
			get
			{
				return m_Press ;
			}
		}

		private State m_State = State.Normal ;
		public  State   state
		{
			get
			{
				return m_State ;
			}
		}

		private State	m_EaseState ;

		private float	m_BaseTime = 0 ;

	//	private Vector3 m_BasePosition ;
		private Vector3 m_BaseRotation ;
		private Vector3 m_BaseScale ;

	//	private Vector3 m_MovePosition ;
		private Vector3 m_MoveRotation ;
		private Vector3 m_MoveScale ;


		private bool m_Processing = false ;

		//--------------------------------

		private RectTransform m_RectTransform ;

		/// <summary>
		/// RectTransform の有無を返す
		/// </summary>
		public bool isRectTransform
		{
			get
			{
				if( m_RectTransform == null )
				{
					m_RectTransform = GetComponent<RectTransform>() ;
				}
				if( m_RectTransform == null )
				{
					return false ;
				}
				return true ;
			}
		}


		private Button m_Button ;

		/// <summary>
		/// Button の有無を返す
		/// </summary>
		public bool isButton
		{
			get
			{
				if( m_Button == null )
				{
					m_Button = GetComponent<Button>() ;
				}
				if( m_Button == null )
				{
					return false ;
				}
				return true ;
			}
		}

		private UIButton m_UIButton ;

		//-----------------------------------------------------------------


		void Start()
		{
			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				m_State = State.Normal ;
				Transition tData = transition[ ( int )m_State ] ;
//				m_BasePosition = tData.fadePosition ;
				m_BaseRotation = tData.fadeRotation ;
				m_BaseScale    = tData.fadeScale ;
//				m_MovePosition = tData.fadePosition ;
				m_MoveRotation = tData.fadeRotation ;
				m_MoveScale    = tData.fadeScale ;
				m_Processing = false ;

				if( m_Button == null )
				{
					m_Button = GetComponent<Button>() ;
				}

				if( m_UIButton == null )
				{
					m_UIButton = GetComponent<UIButton>() ;
				}
			}
		}
	
		void Update()
		{
			if( Application.isPlaying == true )
			{
				if( m_TransitionEnabled == true && m_Pausing == false )
				{
					if( m_State != State.Clicked && m_State != State.Finished )
					{
						bool tDisable = false ;
						if( m_Button != null )
						{
							if( m_Button.IsInteractable() == false )
							{
								tDisable = true ;
							}
						}

						if( m_State != State.Disabled && tDisable == true )
						{
							// 無効状態
							ChangeTransitionState( State.Disabled, State.Disabled ) ;
						}
						else
						if( m_State == State.Disabled && tDisable == false )
						{
							// 無効状態
							ChangeTransitionState( State.Normal, State.Disabled ) ;
						}

						if( m_State == State.Highlighted && isHover == false )
						{
							// 無効状態
							ChangeTransitionState( State.Normal, State.Highlighted ) ;
						}
					}

					//--------------------------------------------------------

					// 実行する
					if( m_Processing == true )
					{
						ProcessTransition() ;
					}
				}
			}
		}

		//---------------------------------------------
	
		// Enter
		public void OnPointerEnter( PointerEventData tPointer )
		{
			// → Release 状態であれば Highlight へ遷移
			if( m_State != State.Clicked && m_State != State.Finished )
			{
				if( m_Press == false && m_Processing == false )
				{
					ChangeTransitionState( State.Highlighted, State.Highlighted ) ;
				}
				m_Hover = true ;
			}
		}

		// Exit
		public void OnPointerExit( PointerEventData tPointer )
		{
			// → Release 状態であれば Normal へ遷移
			if( m_State != State.Clicked && m_State != State.Finished )
			{
				if( m_Press == false && m_Processing == false )
				{
					ChangeTransitionState( State.Normal, State.Highlighted ) ;
				}
				m_Hover = false ;
			}
		}

		// Down
		public void OnPointerDown( PointerEventData tPointer )
		{
			// → Press 状態へ遷移
			if( m_State != State.Clicked && m_State != State.Finished )
			{
				ChangeTransitionState( State.Pressed, State.Pressed ) ;
				m_Press = true ;
			}
		}

		// Up
		public void OnPointerUp( PointerEventData tPointer )
		{
			// → Enter 状態であれば Highlight へ遷移
			// → Exit  状態であれば Normal へ遷移

			if( m_State != State.Clicked && m_State != State.Finished )
			{
				if( m_Hover == true )
				{
					ChangeTransitionState( State.Highlighted, State.Normal ) ;
				}
				else
				{
					ChangeTransitionState( State.Normal, State.Normal ) ;
				}
				m_Press = false ;
			}
		}

		// Clicked(UIButton から呼び出される)
		internal protected void OnClicked( bool tWaitForTransition )
		{
			if( m_State != State.Clicked && m_State != State.Finished )
			{
				m_WaitForTransition = tWaitForTransition ;
				if( m_WaitForTransition == true )
				{
//					Debug.LogWarning( "------- クリックされたのでクリック後のトランジション発動" ) ;
					UIEventSystem.Disable( 31 ) ;
				}

				// 押されたボタンを同階層で最も手前に表示する
				if( transform.parent != null )
				{
					UIView tParent = transform.parent.GetComponent<UIView>() ;
					if( tParent != null )
					{
						Vector3 p =	transform.localPosition ;
						p.z = 10 ;
						transform.localPosition = p ;
						tParent.SortChildByZ() ;
					}
				}

				ChangeTransitionState( State.Clicked, State.Clicked ) ;
				m_Press = false ;
			}
		}

		//-------------------------------------------------------------------

		private bool m_WaitForTransition = false ;

		// トランジジョンの状態を変える
		private bool ChangeTransitionState( State tState, State tEaseState )
		{
			if( m_State == tState )
			{
				// 状態が同じなので処理しない
				return true ;
			}

			if( m_Button != null && m_UIButton != null )
			{
				if( colorTransmission == true )
				{
					ColorBlock tCB = m_Button.colors ;
				
					if( tState == State.Normal )
					{
						m_UIButton.SetColorOfChildren( tCB.normalColor ) ;
					}
					else
					if( tState == State.Highlighted )
					{
						m_UIButton.SetColorOfChildren( tCB.highlightedColor ) ;
					}
					else
					if( tState == State.Pressed )
					{
						m_UIButton.SetColorOfChildren( tCB.pressedColor ) ;
					}
					else
					if( tState == State.Disabled )
					{
						m_UIButton.SetColorOfChildren( tCB.disabledColor ) ;
					}
					else
					{
						m_UIButton.SetColorOfChildren( tCB.normalColor ) ;
					}
				}
			}

			// 現在の RectTransform の状態を退避する

			if( m_RectTransform == null )
			{
				m_RectTransform = GetComponent<RectTransform>() ;
			}
			if( m_RectTransform == null )
			{
				// RectTransform がアタッチされていない
				return false ;
			}


			// 現在変化中の状態を変化前の状態とする
	//		m_BasePosition	= m_MovePosition ;
			m_BaseRotation	= m_MoveRotation ;
			m_BaseScale		= m_MoveScale ;

			m_State			= tState ;
			m_EaseState		= tEaseState ;
			m_BaseTime		= Time.realtimeSinceStartup ;

			m_Processing	= true ;	// 処理する

			if( spriteOverwriteEnabled == true )
			{
				UIImage tImage = GetComponent<UIImage>() ;
				if( tImage != null && tImage.atlasSprite != null )
				{
					// 画像を変更する
					
					Transition tData = transition[ ( int )m_State ] ;
	
					if( tData.sprite != null )
					{
						tImage.SetSpriteInAtlas( tData.sprite.name ) ;
					}
				}
			}

			return true ;
		}

		// トランジションの状態を反映させる
		private bool ProcessTransition()
		{
			if( m_RectTransform == null )
			{
				m_RectTransform = GetComponent<RectTransform>() ;
			}
			if( m_RectTransform == null )
			{
				// RectTransform がアタッチされていない
				return false ;
			}

			float tTime = Time.realtimeSinceStartup - m_BaseTime ;

			Transition tData = transition[ ( int )m_State ] ;
			Transition tEaseData = transition[ ( int )m_EaseState ] ;
			if( tData.processType == ProcessType.Ease )
			{
				if( tData.fadeDuration >  0 )
				{
					float tFactor = tTime / tData.fadeDuration ;
					if( tFactor >  1 )
					{
						tFactor  = 1 ;
						m_Processing = false ;
					}
				
	//				m_MovePosition = GetValue( m_BasePosition, tData.fadePosition, tFactor, tEaseData.fadeEaseType ) ;
					m_MoveRotation = GetValue( m_BaseRotation, tData.fadeRotation, tFactor, tEaseData.fadeEaseType ) ;
					m_MoveScale    = GetValue( m_BaseScale,    tData.fadeScale,    tFactor, tEaseData.fadeEaseType ) ;

	//				tRectTransform.anchoredPosition  = m_InitialPosition + m_MovePosition ;
	//				tRectTransform.localEulerAngles  = m_InitialRotation + m_MoveRotation ;
	//				tRectTransform.localScale        = new Vector3( m_InitialScale.x * m_MoveScale.x, m_InitialScale.y * m_MoveScale.y, m_InitialScale.z * m_MoveScale.z ) ;
	//				tRectTransform.anchoredPosition  = m_InitialPosition + m_MovePosition ;
					m_RectTransform.localEulerAngles = m_MoveRotation ;
					m_RectTransform.localScale       = m_MoveScale ;
				}
			}
			else
			if( tData.processType == ProcessType.AnimationCurve )
			{
				int l = tData.fadeAnimationCurve.length ;
				Keyframe tKeyFrame = tData.fadeAnimationCurve[ l - 1 ] ;	// 最終キー
				float tFadeDuration = tKeyFrame.time ;
			
				if( tFadeDuration >  0 )
				{
					if( tTime >  tFadeDuration )
					{
						tTime  = tFadeDuration ;
						m_Processing = false ;
					}

					float tValue = tEaseData.fadeAnimationCurve.Evaluate( tTime ) ;
	//				m_MovePosition = Vector3.Lerp( m_BasePosition, tData.fadePosition, tValue ) ;
					m_MoveRotation = Vector3.Lerp( m_BaseRotation, tData.fadeRotation, tValue ) ;
					m_MoveScale    = Vector3.Lerp( m_BaseScale,    tData.fadeScale,    tValue ) ;

	//				tRectTransform.anchoredPosition  = m_InitialPosition + m_MovePosition ;
	//				tRectTransform.localEulerAngles  = m_InitialRotation + m_MoveRotation ;
	//				tRectTransform.localScale        = new Vector3( m_InitialScale.x * m_MoveScale.x, m_InitialScale.y * m_MoveScale.y, m_InitialScale.z * m_MoveScale.z ) ;
					m_RectTransform.localEulerAngles = m_MoveRotation ;
					m_RectTransform.localScale       = m_MoveScale ;
				}
			}


			if( m_Processing == false )
			{
				// 終了
				if( m_State == State.Clicked )
				{
					// 元の見た目に戻す
					ChangeTransitionState( State.Finished, State.Finished ) ;
				}
				else
				if( m_State == State.Finished )
				{
					if( m_WaitForTransition == true )
					{
//						Debug.LogWarning( "------- クリック後のトランジションが終了した" ) ;
						UIEventSystem.Enable( 31 ) ;
	
						if( m_UIButton != null )
						{
							m_UIButton.OnButtonClickFromTransition() ;
						}
						m_WaitForTransition = false ;
					}

					if( m_PauseAfterFinished == true )
					{
						m_Pausing = true ;	// 動作をロックする
					}
				}
			}


			return true ;
		}

		/// <summary>
		/// 指定の状態で表示する画像を設定する
		/// </summary>
		/// <param name="tState"></param>
		/// <param name="tName"></param>
		/// <returns></returns>
		public bool ReplaceImage( State tState, string tName )
		{
			UIImage tImage = GetComponent<UIImage>() ;
			if( tImage == null || tImage.atlasSprite == null )
			{
				return false ;
			}

			Sprite tSprite = tImage.GetSpriteInAtlas( tName ) ;
			if( tSprite == null )
			{
				return false ;
			}

			int i = ( int )tState ;

			if( transition[ i ] != null )
			{
				transition[ i ].sprite = tSprite ;
			}

			if( m_State == tState )
			{
				tImage.SetSpriteInAtlas( tName ) ;
			}

			return true ;
		}


		/// <summary>
		/// 全ての状態で表示する画像を設定する
		/// </summary>
		/// <param name="tName"></param>
		public bool ReplaceImage( string tName )
		{
			UIImage tImage = GetComponent<UIImage>() ;
			if( tImage == null || tImage.atlasSprite == null )
			{
				return false ;
			}

			Sprite tSprite = tImage.GetSpriteInAtlas( tName ) ;
			if( tSprite == null )
			{
				return false ;
			}

			int i, l = transition.Count ;
				
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( transition[ i ] != null )
				{
					transition[ i ].sprite = tSprite ;
				}
			}

			tImage.SetSpriteInAtlas( tName ) ;

			return true ;
		}


		//---------------------------------------------
	
		// Vector3 の変化中の値を取得
		private Vector3 GetValue( Vector3 tStart, Vector3 tEnd, float tFactor, EaseType tEaseType  )
		{
			float x = GetValue( tStart.x, tEnd.x, tFactor, tEaseType ) ;
			float y = GetValue( tStart.y, tEnd.y, tFactor, tEaseType ) ;
			float z = GetValue( tStart.z, tEnd.z, tFactor, tEaseType ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public float GetValue( float tStart, float tEnd, float tFactor, EaseType tEaseType )
		{
			float tValue = 0 ;
			switch( tEaseType )
			{
				case EaseType.easeInQuad		: tValue = easeInQuad(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuad		: tValue = easeOutQuad(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuad		: tValue = easeInOutQuad(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInCubic		: tValue = easeInCubic(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutCubic		: tValue = easeOutCubic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutCubic	: tValue = easeInOutCubic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInQuart		: tValue = easeInQuart(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuart		: tValue = easeOutQuart(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuart	: tValue = easeInOutQuart(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInQuint		: tValue = easeInQuint(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuint		: tValue = easeOutQuint(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuint	: tValue = easeInOutQuint(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInSine		: tValue = easeInSine(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutSine		: tValue = easeOutSine(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutSine		: tValue = easeInOutSine(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInExpo		: tValue = easeInExpo(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutExpo		: tValue = easeOutExpo(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutExpo		: tValue = easeInOutExpo(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInCirc		: tValue = easeInCirc(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutCirc		: tValue = easeOutCirc(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutCirc		: tValue = easeInOutCirc(		tStart, tEnd, tFactor )	; break ;
				case EaseType.linear			: tValue = linear(				tStart, tEnd, tFactor )	; break ;
				case EaseType.spring			: tValue = spring(				tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInBounce		: tValue = easeInBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutBounce		: tValue = easeOutBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutBounce	: tValue = easeInOutBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInBack		: tValue = easeInBack(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutBack		: tValue = easeOutBack(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutBack		: tValue = easeInOutBack(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInElastic		: tValue = easeInElastic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutElastic	: tValue = easeOutElastic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutElastic	: tValue = easeInOutElastic(	tStart, tEnd, tFactor )	; break ;
			}
			return tValue ;
		}

		//------------------------

		private float easeInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private float easeOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private float easeInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private float easeInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private float easeOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private float easeInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private float easeInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private float easeOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private float easeInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private float easeInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private float easeOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private float easeInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private float easeInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private float easeOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private float easeInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private float easeInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private float easeOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private float easeInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private float easeInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private float easeOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private float easeInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private float linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private float spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private float easeInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - easeOutBounce( 0, end, d - value ) + start ;
		}
	
		private float easeOutBounce( float start, float end, float value )
		{
			value /= 1f ;
			end -= start ;
			if( value <  ( 1 / 2.75f ) )
			{
				return end * ( 7.5625f * value * value ) + start ;
			}
			else
			if( value <  ( 2 / 2.75f ) )
			{
				value -= ( 1.5f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .75f ) + start ;
			}
			else
			if( value <  (  2.5  / 2.75 ) )
			{
				value -= ( 2.25f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .9375f ) + start ;
			}
			else
			{
				value -= ( 2.625f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .984375f ) + start ;
			}
		}

		private float easeInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return easeInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return easeOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private float easeInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private float easeOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private float easeInOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value /= 0.5f ;
			if( ( value ) <  1 )
			{
				s *= ( 1.525f ) ;
				return end * 0.5f * ( value * value * ( ( ( s ) + 1 ) * value - s ) ) + start ;
			}
			value -= 2 ;
			s *= ( 1.525f ) ;
			return end * 0.5f * ( ( value ) * value * ( ( ( s ) + 1 ) * value + s ) + 2 ) + start ;
		}

		private float easeInElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return - ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
		}		

		private float easeOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p * 0.25f ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return ( a * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start ) ;
		}		

		private float easeInOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d * 0.5f ) == 2 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			if( value <  1 ) return - 0.5f * ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
			return a * Mathf.Pow( 2, - 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start ;
		}		

		private float punch( float amplitude, float value )
		{
			float s = 9 ;
			if( value == 0 )
			{
				return 0 ;
			}
			else
			if( value == 1 )
			{
				return 0 ;
			}
			float period = 1 * 0.3f ;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 ) ;
			return ( amplitude * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * 1 - s ) * ( 2 * Mathf.PI ) / period ) ) ;
		}

		private float clerp( float start, float end, float value )
		{
			float min = 0.0f ;
			float max = 360.0f ;
			float half = Mathf.Abs( ( max - min ) * 0.5f ) ;
			float retval = 0.0f ;
			float diff = 0.0f ;
			if( ( end - start ) <  - half )
			{
				diff =   ( ( max - start ) + end   ) * value ;
				retval = start + diff ;
			}
			else
			if( ( end - start ) >    half )
			{
				diff = - ( ( max - end   ) + start ) * value ;
				retval = start + diff ;
			}
			else retval = start + ( end - start ) * value ;
			return retval ;
		}
	}

}
