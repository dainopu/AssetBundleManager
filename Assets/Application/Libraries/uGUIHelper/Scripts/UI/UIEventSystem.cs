using System ;
using System.Collections ;
using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.XR ;

namespace uGUIHelper
{
	/// <summary>
	/// uGUI:EventSystem の機能拡張コンポーネントクラス
	/// </summary>
	[ ExecuteInEditMode ]	
	public class UIEventSystem : MonoBehaviour
	{
		// シングルトンインスタンス
		private static UIEventSystem m_Instance ;

		// 入力無効化状態
		private static uint	m_Lock = 0 ;

		private static Stack m_LockStack = new Stack() ;

		// ＶＲ用の反応ボタン押下判定用のコールバック
		private static Func<GameObject,bool>	m_OnPressAction = null ;


		//-----------------------------------------------------------

		/// <summary>
		/// ＶＲ用の反応ボタン押下判定用のコールバックを設定する
		/// </summary>
		/// <param name="tOnPressAction"></param>
		public static void SetOnPressAction( Func<GameObject,bool> tOnPressAction )
		{
			m_OnPressAction = tOnPressAction ;

			if( m_Instance != null && m_Instance.m_StandaloneInputModule != null )
			{
				m_Instance.m_StandaloneInputModule.SetOnPressAction( tOnPressAction ) ;
			}
		}

		/// <summary>
		/// 注目しているオブジェクトを返す
		/// </summary>
		/// <returns></returns>
		public static GameObject GetLookingObject()
		{
			if( m_Instance != null && m_Instance.m_StandaloneInputModule != null )
			{
				return m_Instance.m_StandaloneInputModule.GetLookingObject() ;
			}
			return null ;
		}

		//---------------------------------------------------------
		
		// インプットモジュールのインスタンス
		private StandaloneInputModuleWrapper	m_StandaloneInputModule = null ;

		/// <summary>
		/// インプットモジュールの動作タイプ
		/// </summary>
		public static StandaloneInputModuleWrapper.ProcessType	ProcessType = StandaloneInputModuleWrapper.ProcessType.Default ;

		//---------------------------------------------------------

		/// <summary>
		/// インスタンス生成（スクリプトから生成する場合）
		/// </summary>
		/// <returns></returns>
		public static UIEventSystem Create()
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
#if UNITY_EDITOR
			if( GameObject.FindObjectOfType<EventSystem>() != null )
			{
				Debug.LogWarning( "既にシーン内に EventSystem が存在しています。UIEventSystm への差し替えを推奨します。" ) ;
			}
#endif

			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( UIEventSystem ) ) as UIEventSystem ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "EventSystem" ) ;
				tGameObject.AddComponent<UIEventSystem>() ;
			
				tGameObject.transform.localPosition = Vector3.zero ;
				tGameObject.transform.localRotation = Quaternion.identity ;
				tGameObject.transform.localScale = Vector3.one ;
			}
		
			return m_Instance ;
		}
	
		// インスタンス取得
		public static UIEventSystem Get()
		{
			return m_Instance ;
		}
	
		// インスタンス破棄
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
			}
		}
	
		//-----------------------------------------------------------------
	
		void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する（シーンを加えようとした場合）
			if( m_Instance != null )
			{
				Debug.LogWarning( "Destroy Myself !!:" + name ) ;
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
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

			EventSystem eventSystem = GetComponent<EventSystem>() ;
			if( eventSystem == null )
			{
				eventSystem = gameObject.AddComponent<EventSystem>() ;
			}

			eventSystem.enabled = ( m_Lock == 0 ) ;
			
			// カスタマイズ版のインプットモジュールを使用する
			if( GetComponent<StandaloneInputModuleWrapper>() == null )
			{
				m_StandaloneInputModule = gameObject.AddComponent<StandaloneInputModuleWrapper>() ;
				m_StandaloneInputModule.SetProcessType( ProcessType ) ;
				m_StandaloneInputModule.SetOnPressAction( m_OnPressAction ) ;
			}
		}
	
		void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}

		/// <summary>
		/// イベントシステムを有効化する
		/// </summary>
		/// <returns></returns>
		public static bool Enable( int tLevel = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetState( tLevel, true ) ;
		}

		/// <summary>
		/// イベントシステムを無効化する
		/// </summary>
		/// <returns></returns>
		public static bool Disable( int tLevel = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetState( tLevel, false ) ;
		}

		/// <summary>
		/// ロック状態を退避する
		/// </summary>
		/// <returns></returns>
		public static bool Push()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_LockStack.Push( m_Lock ) ;

			return true ;
		}

		/// <summary>
		/// ロック状態を復帰する
		/// </summary>
		/// <returns></returns>
		public static bool Pop()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Lock = ( uint )m_LockStack.Pop() ;

			return true ;
		}

		/// <summary>
		/// スタックをクリアする
		/// </summary>
		/// <returns></returns>
		public static bool Clear()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_LockStack.Clear() ;

			return true ;
		}


		/// <summary>
		/// イベントシステムの状態を取得する
		/// </summary>
		/// <returns></returns>
		public static bool state
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
	
				EventSystem tEventSystem = m_Instance.gameObject.GetComponent<EventSystem>() ;
				if( tEventSystem == null )
				{
					return false ;
				}
	
				return tEventSystem.enabled ;
			}
		}


		private bool SetState( int level, bool state )
		{
			EventSystem eventSystem = gameObject.GetComponent<EventSystem>() ;
			if( eventSystem == null )
			{
				return false ;
			}

			//----------------------------------------------------------

			if( state == true )
			{
				// 有効化
				if( level >= 0 && level <= 31 )
				{
					m_Lock = ( uint )( m_Lock & ( 0xFFFFFFFF ^ ( ( uint )1 << level ) ) ) ;
				}
				else
				{
					m_Lock = 0 ;
				}

				m_StandaloneInputModule.Enable() ;
			}
			else
			{
				// 無効化
				m_Lock = ( uint )( m_Lock |                ( ( uint )1 << level )   ) ;

				// 現時点で Hover や Press 扱いになっているものを解放する
				m_StandaloneInputModule.Disable() ;
			}
			
			eventSystem.enabled = ( m_Lock == 0 ) ;

			return eventSystem.enabled ;
		}

		//---------------------------------------------------------------------------
	}
}

