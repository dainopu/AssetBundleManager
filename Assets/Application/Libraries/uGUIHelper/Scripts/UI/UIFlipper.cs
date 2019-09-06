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
	/// Flipper コンポーネントクラス
	/// </summary>
	[ ExecuteInEditMode ]
	// ＵＩの簡易アニメーション用のコンポーネント
	public class UIFlipper : UIBehaviour
	{
		// 識別名
		public string identity ;

		// 開始までにかかる時間
		public float delay = 0 ;

		//------------------------------------------------------

		[SerializeField][HideInInspector]
		private UISpriteAnimation	m_SpriteAnimation ;	// スプライトアニメーション
		public  UISpriteAnimation     spriteAnimation
		{
			get
			{
				return m_SpriteAnimation ;
			}
			set
			{
				if( m_SpriteAnimation != value )
				{
					m_SpriteAnimation  = value ;

					Image tImage = GetComponent<Image>() ;

					if( m_SpriteAnimation != null )
					{
						if( tImage != null )
						{
							tImage.sprite = m_SpriteAnimation.GetSpriteOfFrame( 0 ) ;
						}

						m_Begin = 0 ;
						m_End   = m_SpriteAnimation.length - 1 ;
					}
					else
					{
						if( tImage != null )
						{
							tImage.sprite = null ;
						}

						m_Begin = 0 ;
						m_End   = 0 ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_Begin = 0 ;			// 開始フレームのインデックス番号
		public  int   begin
		{
			get
			{
				return m_Begin ;
			}
			set
			{
				if( m_Begin != value && m_End != value && m_SpriteAnimation != null )
				{
					if( m_Begin >= 0 && m_Begin <  m_SpriteAnimation.length )
					{
						m_Begin = value ;
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private int m_End = 0 ;				// 終了フレームのインデックス番号
		public  int   end
		{
			get
			{
				return m_End ;
			}
			set
			{
				if( m_End != value && m_Begin != value && m_SpriteAnimation != null )
				{
					if( m_End >= 0 && m_End <  m_SpriteAnimation.length )
					{
						m_End = value ;
					}
				}
			}
		}

		private int m_Current = 0 ;				// カレントフレーム
		public  int   current
		{
			get
			{
				return m_Current ;
			}
			set
			{
				if( m_Current != value && m_SpriteAnimation != null )
				{
					if( m_Current >= 0 && m_Current <  m_SpriteAnimation.length )
					{
						m_Current = value ;
					}
				}
			}
		}
		
		/// <summary>
		/// 逆再生(しない)
		/// </summary>
		public bool back = false ;			// 逆方向再生

		/// <summary>
		/// ループ(しない)
		/// </summary>
		public bool loop = false ;			// ループの有無

		/// <summary>
		/// ループ後の反転(しない)
		/// </summary>
		public bool reverse = false ;		// ループの際に再生方向を反転するか

		private int m_Count = 0 ;

		/// <summary>
		/// スピード倍率
		/// </summary>
		public float speed = 1.0f ;


		/// <summary>
		/// タイムスケールを無視するかどうか（しない）
		/// </summary>
		public bool ignoreTimeScale = false ;

		/// <summary>
		/// 起動と同時に実行するかどうか（する）
		/// </summary>
		public bool playOnAwake = true ;

		/// <summary>
		/// 終了と同時にゲームオブジェクトを破棄するかどうか
		/// </summary>
		public bool destroyAtEnd = false ;

		/// <summary>
		/// イメージの大きさを各フレームのサイズに合わせるかどうか
		/// </summary>
		public bool autoResize = false ;

		//------------------------------------------------------

		private Image			m_Image ;
		private RectTransform	m_RectTransform ;

		private UIView			m_View ;
		public  UIView			  View
		{
			get
			{
				return m_View ;
			}
		}
		
		private float   m_Time ;
		private float	m_BaseTime ;

		private enum State
		{
			Delay = 0,
			Play  = 1,
			Finished = 2,
		}

		private State m_State = State.Delay ;

		// 実行状況
		private bool m_Running = false ;
		public  bool isRunning{ get{ return m_Running ; } }

		// 再生状況
		private bool m_Playing = false ;
		public  bool isPlaying{ get{ return m_Playing ; } }
	
		//--------------------------------------------------------

		public UnityEvent onFinished = new UnityEvent() ;

		private Action<string, UIFlipper> m_OnFinishedAction ;
		public  Action<string, UIFlipper>   onFinishedAction
		{
			get
			{
				return m_OnFinishedAction ;
			}
			set
			{
				m_OnFinishedAction = value ;
			}
		}
		public void SetOnFinished( Action<string, UIFlipper> tOnFinishAction )
		{
			m_OnFinishedAction = tOnFinishAction ;
		}


		//--------------------------------------------------------

		//--------------------------------------------------------

		override protected void  OnDisable()
		{
			base.OnDisable() ;

			// 無効化した際にチェッカーが開いていれば強制的に閉じる
			isChecker = false ;
		}

		override protected void Awake()
		{
			base.Awake() ;

			if( Application.isPlaying == true && isChecker == true )
			{
				LoadState() ;
			}
		}
	
		override protected void Start()
		{
			base.Start() ;

			if( Application.isPlaying == true && playOnAwake == true )
			{
				// カスタムリスナー登録
				onFinished.AddListener( OnFinishedInner ) ;
			
				Play() ;
			}
		}
	
		//---------------------------------------------

		// 内部リスナー
		private void OnFinishedInner()
		{
			if( m_OnFinishedAction != null )
			{
				m_OnFinishedAction( identity, this ) ;
			}
		}
	
		/// <summary>
		/// 再生終了時に呼ばれるリスナーを登録する
		/// </summary>
		/// <param name="tOnFinished"></param>
		public void AddOnFinishedListener( UnityEngine.Events.UnityAction tOnFinished )
		{
			onFinished.AddListener( tOnFinished ) ;
		}
	
		/// <summary>
		/// 再生終了時に呼ばれリスナーを全て削除する
		/// </summary>
		public void RemoveOnFinishedAllListeners()
		{
			onFinished.RemoveAllListeners() ;
		}
	
		//---------------------------------------------
	
		/// <summary>
		/// 再生する(コルーチン)
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator Play_Coroutine( bool tDestroyAtEnd = false, float tSpeed = 0, float tDelay = -1 )
		{
			Play( tDestroyAtEnd, tSpeed, tDelay ) ;

			while( m_Playing == true )
			{
				yield return null ;
			}
		}

		/// <summary>
		/// 再生する
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tSpeed"></param>
		public void Play( bool tDestroyAtEnd = false, float tSpeed = 0, float tDelay = -1, Action<string,UIFlipper> onFinishedAction = null )
		{
			if( spriteAnimation == null )
			{
				return ;	// 再生出来ない
			}

			enabled = true ;

			if( onFinishedAction != null )
			{
				// 既に設定されている可能性がある
				m_OnFinishedAction = onFinishedAction ;
			}

			if( m_Running == true )
			{
				// 既に再生中
				if( m_Playing == true )
				{
					return ;
				}
				else
				{
					// リスタート
					m_BaseTime = Time.realtimeSinceStartup ;

					m_Playing = true ;
					return ;
				}
			}

			//-------------------------------------------


			if( tDelay >= 0 )
			{
				delay = tDelay ;
			}

			if( tSpeed >  0 )
			{
				speed = tSpeed ;
			}

			if( tDestroyAtEnd == true )
			{
				destroyAtEnd = true ;
			}

			m_Image	= gameObject.GetComponent<Image>() ;

			if( m_Image != null && delay >  0 )
			{
				m_Image.enabled = false ;
			}

			m_RectTransform = gameObject.GetComponent<RectTransform>() ;

			//-------------------------------

			m_View = gameObject.GetComponent<UIView>() ;

			//-------------------------------

			m_State = State.Delay ;

			m_Current = m_Begin ;

			m_Time = 0 ;
			m_Count = 0 ;



			Modify( m_Current ) ;	// 初期位置へ移動させる

			m_BaseTime = Time.realtimeSinceStartup ;

			m_Running = true ;
			m_Playing = true ;
		}

		/// <summary>
		/// 一時停止する
		/// </summary>
		public void Pause()
		{
			if( m_Running == true )
			{
				m_Playing = false ;
			}
		}

		/// <summary>
		/// 完全停止する
		/// </summary>
		public void Stop()
		{
			if( m_Running == true )
			{
				m_Playing = false ;
				m_Running = false ;
			}
		}

		void Update()
		{
			if( m_Running == true && m_Playing == true )
			{
				m_Time = m_Time + GetDeltaTime() ;
			
				if( m_State == State.Delay )
				{
					if( m_Image != null && m_Image.enabled == true )
					{
						m_Image.enabled = false ;
					}

					// ディレイはタイムスケール値を無視する(ただし Time.timeScale は除く)
					float tDelay = delay ;

					if( m_Time <  tDelay )
					{
						return ;
					}
					else
					{
						m_State = State.Play ;
//						m_Time = m_Time - tDelay ;
						m_Time = 0 ;
					}
				}

				if( m_State == State.Play )
				{
					if( m_Image != null && m_Image.enabled == false )
					{
						m_Image.enabled = true ;
					}

					if( m_Time <  m_SpriteAnimation[ m_Current ].duration )
					{
						return ;
					}
				
				
					while( true )
					{
						m_Time = m_Time -  m_SpriteAnimation[ m_Current ].duration ;

						bool tBack = back ;
						if( loop == true && reverse == true )
						{
							if( ( m_Count % 1 ) == 1 )
							{
								tBack = ! tBack ;
							}
						}

						if( tBack == false )
						{
							m_Current = ( m_Current + 1 ) % m_SpriteAnimation.length ;
						}
						else
						{
							m_Current = ( m_Current - 1 + m_SpriteAnimation.length ) % m_SpriteAnimation.length ;
						}

						if( m_Current == m_End )
						{
							// 終了
							if( loop == false )
							{
								m_State = State.Finished ;
								break ;
							}
							else
							{
								m_Count ++ ;
							}
						}

						if( m_Time <  m_SpriteAnimation[ m_Current ].duration )
						{
							break ;
						}
					}

					Modify( m_Current ) ;
				}
				else
				if( m_State == State.Finished )
				{
					// 最終フレーム

					if( m_Time <  m_SpriteAnimation[ m_Current ].duration )
					{
						return ;
					}
				
					// 完全終了
					m_Playing = false ;
					m_Running = false ;

					// コールバック呼び出し
					onFinished.Invoke() ;

					if( destroyAtEnd == true )
					{
						Destroy( gameObject ) ;
					}
				}
			}
		}

		private void Modify( int tFrameIndex )
		{
			if( spriteAnimation != null && m_Image != null )
			{
				// 指定したフレームのスプライトを表示する
				m_Image.sprite = spriteAnimation.GetSpriteOfFrame( tFrameIndex ) ;

				if( autoResize == true )
				{
					if( m_Image.sprite != null && m_RectTransform != null )
					{
						m_RectTransform.sizeDelta = new Vector2( m_Image.sprite.rect.width, m_Image.sprite.rect.height ) ;
					}
				}
			}
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			float tBaseTime = Time.realtimeSinceStartup ;
			float tDeltaTime = tBaseTime - m_BaseTime ;

			m_BaseTime = tBaseTime ;

			// 独自のスピードを乗算する
			tDeltaTime = tDeltaTime * speed * m_SpriteAnimation.timeScale ;

			if( ignoreTimeScale == false )
			{
				// タイムスケールを無視しない
				tDeltaTime = tDeltaTime * Time.timeScale ;
			}

			return tDeltaTime ;
		}

		//--------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Sprite m_InitialSprite ;

		//-------------------------------------------------

		[SerializeField][HideInInspector]
		private bool m_Checker = false ;
		public  bool isChecker
		{
			get
			{
				return m_Checker ;
			}
			set
			{
				if( m_Checker != value )
				{
					m_Checker = value ;
					if( m_Checker == true )
					{
						SaveState() ;
						SetCheckFactor( m_CheckFactor ) ;
					}
					else
					{
						LoadState() ;
					}
				}
			}
		}

		private void RefreshChecker()
		{
			if( Application.isPlaying == true )
			{
				return ;	// 実行中は無効
			}

			if( m_Checker == true )
			{
				LoadState() ;
				SetCheckFactor( m_CheckFactor ) ;
			}
		}
	
		// ただの位置表示用
		[SerializeField][HideInInspector]
		private int m_CheckFactor = 0 ;
		public  int   checkFactor
		{
			get
			{
				return m_CheckFactor ;
			}
			set
			{
				if( m_CheckFactor != value )
				{
					m_CheckFactor = value ;
					if( m_Checker == true )
					{
						SetCheckFactor( m_CheckFactor ) ;
					}
				}
			}
		}

		// モーションチェック専用で現在の状態を退避する
		public void SaveState()
		{
			Image tImage = gameObject.GetComponent<Image>() ;
			if( tImage != null )
			{
				m_InitialSprite = tImage.sprite ;
			}
		}

		// モーションチェック専用で現在の状態を復帰する
		private void LoadState()
		{
			Image tImage = gameObject.GetComponent<Image>() ;
			if( tImage != null )
			{
				tImage.sprite = m_InitialSprite ;
			}
		}

		// デバッグ用
		private void SetCheckFactor( int tCheckFactor )
		{
			m_Image	= gameObject.GetComponent<Image>() ;

			//-----------------

			Modify( tCheckFactor ) ;

			m_Image.GraphicUpdateComplete() ;
		}
	}
}

