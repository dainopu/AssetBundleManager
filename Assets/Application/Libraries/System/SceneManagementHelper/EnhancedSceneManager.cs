using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

/// <summary>
/// シーンマネージメントヘルパーパッケージ
/// </summary>
namespace SceneManagementHelper
{
	/// <summary>
	/// エンハンスドシーンマネージャクラス Version 2017/11/20 0
	/// </summary>
	public class EnhancedSceneManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// EnhancedSceneManager を生成
		/// </summary>
//		[MenuItem("AudioHelper/Create a EnhancedSceneManager")]
		[MenuItem("GameObject/Helper/SceneManagementHelper/EnhancedSceneManager", false, 24)]
		public static void CreateEnhancedSceneManager()
		{
			GameObject tGameObject = new GameObject( "EnhancedSceneManager" ) ;
		
			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;
		
			tGameObject.AddComponent<EnhancedSceneManager>() ;
			Selection.activeGameObject = tGameObject ;
		}
#endif

		// シングルトンインスタンス
		private static EnhancedSceneManager m_Instance = null ;

		/// <summary>
		/// エンハンスドシーンマネージャのインスタンス
		/// </summary>
		public  static EnhancedSceneManager   instance
		{
			get
			{
				return m_Instance ;
			}
		}
		
		//---------------------------------------------------------
	
		//---------------------------------------------------------
	
		/// <summary>
		/// エンハンスドシーンマネージャのインスタンスを生成する
		/// </summary>
		/// <returns>エンハンスドシーンマネージャのインスタンス</returns>
		public static EnhancedSceneManager Create( Transform tParent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType<EnhancedSceneManager>() ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "EnhancedSceneManager" ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}

				tGameObject.AddComponent<EnhancedSceneManager>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// エンハンスドシーンマネージャのインスタンスを破棄する
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
		
			EnhancedSceneManager tInstanceOther = GameObject.FindObjectOfType<EnhancedSceneManager>() ;
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
			
			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------

			// 現在のシーン名を保存する
			history.Add( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ) ;
		}

		void Update()
		{
			// ソースを監視して停止している（予定）
		}
	
		void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}
	
		//-----------------------------------------------------------------

		// シーン間の受け渡し用のパラメータ
		private Dictionary<string,System.Object> m_Parameter = new Dictionary<string,System.Object>() ;

		/// <summary>
		/// シーンロードの履歴
		/// </summary>
		public List<string> history = new List<string>() ;

		/// <summary>
		/// １つ前のシーン名
		/// </summary>
		public string previousName ;

		/// <summary>
		/// シーン間の受け渡しパラメータを設定する
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータをの値</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetParameter( string tName, System.Object tObject )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetParameter_Private( tName, tObject ) ;
		}

		// シーン間の受け渡しパラメータを保存する
		private bool SetParameter_Private( string tLabel, System.Object tValue )
		{
			if( m_Parameter.ContainsKey( tLabel ) == true )
			{
				m_Parameter[ tLabel ] = tValue ;
				return true ;
			}
			else
			{
				m_Parameter.Add( tLabel, tValue ) ;
				return false ;
			}
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを取得する(任意の型にキャスト)
		/// </summary>
		/// <typeparam name="T">受け渡しパラメータの型</typeparam>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tClear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか</param>
		/// <returns>受け渡しパラメータのインスタンス(任意の型にキャスト)</returns>
		public static T GetParameter<T>( string tName, bool tClear = true ) where T : class
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetParameter_Private<T>( tName, tClear ) ;
		}

		// シーン間の受け渡しパラメータを取得する(任意の型にキャスト)
		private T GetParameter_Private<T>( string tName, bool tClear ) where T : class
		{
			if( m_Parameter.ContainsKey( tName ) == false )
			{
				return null ;
			}

			T tValue = m_Parameter[ tName ] as T ;

			if( tClear == true )
			{
				m_Parameter.Remove( tName ) ;
			}

			return tValue ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを取得する
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tClear">受け渡しパラメータを取得した後に受け渡しパラメータを消去するかどうか</param>
		/// <returns>パラメータの値</returns>
		public static System.Object GetParameter( string tName, bool tClear = true )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetParameter_Private( tName, tClear ) ;
		}

		// シーン間の受け渡しパラメータを取得する
		private System.Object GetParameter_Private( string tName, bool tClear )
		{
			if( m_Parameter.ContainsKey( tName ) == false )
			{
				return null ;
			}

			System.Object tValue = m_Parameter[ tName ] ;

			if( tClear == true )
			{
				m_Parameter.Remove( tName ) ;
			}

			return tValue ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータの存在を確認するする
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter( string tName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ContainsParameter_Private( tName ) ;
		}

		// シーン間の受け渡しパラメータの存在を確認する
		private bool ContainsParameter_Private( string tName )
		{
			if( m_Parameter.ContainsKey( tName ) == false )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータの存在を確認するする
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <returns>結果(true=存在する・false=存在しない)</returns>
		public static bool ContainsParameter<T>( string tName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ContainsParameter_Private<T>( tName ) ;
		}

		// シーン間の受け渡しパラメータの存在を確認する
		private bool ContainsParameter_Private<T>( string tName  )
		{
			if( m_Parameter.ContainsKey( tName ) == false )
			{
				return false ;
			}

			if( m_Parameter[ tName ] is T )
			{
				return true ;
			}

			return false ;
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを削除する
		/// </summary>
		/// <param name="tName">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータをの値</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveParameter( string tName )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.RemoveParameter_Private( tName ) ;
		}

		// シーン間の受け渡しパラメータを削除する
		private bool RemoveParameter_Private( string tLabel )
		{
			if( m_Parameter.ContainsKey( tLabel ) == true )
			{
				m_Parameter.Remove( tLabel ) ;
				return true ;
			}
			else
			{
				return false ;
			}
		}

		/// <summary>
		/// シーン間の受け渡しパラメータを全て消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearParameter()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearParameter_Private() ;

		}

		// シーン間の受け渡しパラメータを全て消去する
		private bool ClearParameter_Private()
		{
			m_Parameter.Clear() ;

			return true ;
		}

		/// <summary>
		/// シーンの遷移の履歴を消去する
		/// </summary>
		/// <returns></returns>
		public static bool ClearHistory()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.ClearHistory_Private() ;
		}

		// シーンの遷移の履歴を消去する
		private bool ClearHistory_Private()
		{
			history.Clear() ;

			history.Add( UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ) ;

			return true ;
		}

		//-----------------------------------------------------------------

		/// <summary>
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( isDone == false && string.IsNullOrEmpty( error ) == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool isDone = false ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	error = "" ;

			/// <summary>
			/// 
			/// </summary>
			public UnityEngine.Object[]		instances = null ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// シーンをロードする(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果((true=成功・false=失敗)</returns>
		public static bool Load( string tName, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, true ) ;
		}

		/// <summary>
		/// シーンをロードする(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果((true=成功・false=失敗)</returns>
		public static bool Load<T>( string tName, T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, true ) ;
		}

		/// <summary>
		/// シーンをロードする(非同期版)　※このメソッドは常駐済みのコンポーネントで呼び出すこと
		/// </summary>
		/// <typeparam name="T">シーン内の特定のコンポーネント型</typeparam>
		/// <param name="tName">シーン名</param>
		/// <param name="rTarget">シーン内の特定のコンポーネントのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tTargetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync( string tName, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, true, tRequest ) ) ;
			return tRequest ;
		}

		/// <summary>
		/// シーンをロードする(非同期版)　※このメソッドは常駐済みのコンポーネントで呼び出すこと
		/// </summary>
		/// <typeparam name="T">シーン内の特定のコンポーネント型</typeparam>
		/// <param name="tName">シーン名</param>
		/// <param name="rTarget">シーン内の特定のコンポーネントのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tTargetName">該当のコンポーネントをさらに名前により絞り込む</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request LoadAsync<T>( string tName, T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, true, tRequest ) ) ;
			return tRequest ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// シーンを加算する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add( string tName, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Additive, false ) ;
		}

		/// <summary>
		/// シーンを加算する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Add<T>( string tName, T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.LoadOrAdd_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Additive, false ) ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync( string tName, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Additive, false, tRequest ) ) ;
			return tRequest ;
		}
		
		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request AddAsync<T>( string tName, T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Additive, false, tRequest ) ) ;
			return tRequest ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back( string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			if( m_Instance.history.Count <= 1 )
			{
				return true ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.history.Count ;
			string tName = m_Instance.history[ c - 2 ] ;
			m_Instance.history.RemoveAt( c - 1 ) ;
			
			return m_Instance.LoadOrAdd_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, false ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Back<T>( T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			if( m_Instance.history.Count <= 1 )
			{
				return true ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.history.Count ;
			string tName = m_Instance.history[ c - 2 ] ;
			m_Instance.history.RemoveAt( c - 1 ) ;
			
			return m_Instance.LoadOrAdd_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, false ) ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(非同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request BackAsync( string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;

			if( m_Instance.history.Count <= 1 )
			{
				tRequest.isDone = true ;
				return tRequest ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.history.Count ;
			string tName = m_Instance.history[ c - 2 ] ;
			m_Instance.history.RemoveAt( c - 1 ) ;
			
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, null, null, null, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, false, tRequest ) ) ;

			return tRequest ;
		}

		/// <summary>
		/// 現在のシーンの１つ前にロードされていてシーンをロードする(非同期版)
		/// </summary>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>列挙子</returns>
		public static Request BackAsync<T>( T[] rTarget, string tTargetName = null, string tLabel = null, System.Object tValue = null ) where T : UnityEngine.Component
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;

			if( m_Instance.history.Count <= 1 )
			{
				tRequest.isDone = true ;
				return tRequest ;	// １つ前のシーンは存在しない
			}

			int c = m_Instance.history.Count ;
			string tName = m_Instance.history[ c - 2 ] ;
			m_Instance.history.RemoveAt( c - 1 ) ;
			
			m_Instance.StartCoroutine( m_Instance.LoadOrAddAsync_Private( tName, typeof( T ), rTarget, tTargetName, tLabel, tValue, UnityEngine.SceneManagement.LoadSceneMode.Single, false, tRequest ) ) ;

			return tRequest ;
		}

		//-----------------------------------------------------------

		// シーンをロードまたは加算する(同期版)
		private bool LoadOrAdd_Private( string tName, Type tType, UnityEngine.Object[] rTarget, string tTargetName, string tLabel, System.Object tValue, UnityEngine.SceneManagement.LoadSceneMode tMode, bool tHistory )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return false ;
			}
			
			if( string.IsNullOrEmpty( tLabel ) == false )
			{
				SetParameter_Private( tLabel, tValue ) ;
			}

			if( tMode == UnityEngine.SceneManagement.LoadSceneMode.Single )
			{
				// ロードの場合は履歴に追加する
				previousName = GetActiveName() ;

				if( tHistory == true )
				{
					history.Add( tName ) ;
				}
			}
			
			UnityEngine.SceneManagement.SceneManager.LoadScene( tName, tMode ) ;

			//------------------------------------------------------------------------------------------

			UnityEngine.SceneManagement.Scene tScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( tName ) ;
			if( tScene.IsValid() == false || tScene.isLoaded == false )
			{
				return false ;
			}

			if( tType != null && rTarget != null && rTarget.Length >  0 )
			{
				GetInstance_Private( tScene, tType, rTarget, tTargetName, null ) ;
			}

			return true ;
		}

		// シーンをロードまたは加算する(非同期版)
		private IEnumerator LoadOrAddAsync_Private( string tName, Type tType, UnityEngine.Object[] rTarget, string tTargetName, string tLabel, System.Object tValue, UnityEngine.SceneManagement.LoadSceneMode tMode, bool tHistory, Request tRequest )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				yield break ;
			}
			
			//----------------------------------------------------------

			if( tType != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindObjectOfType( tType ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------

			if( string.IsNullOrEmpty( tLabel ) == false )
			{
				SetParameter_Private( tLabel, tValue ) ;
			}

			if( tMode == UnityEngine.SceneManagement.LoadSceneMode.Single )
			{
				// ロードの場合は履歴に追加する
				previousName = GetActiveName() ;

				if( tHistory == true )
				{
					history.Add( tName ) ;
				}
			}

			yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( tName, tMode ) ;
			
			//------------------------------------------------------------------------------------------

			UnityEngine.SceneManagement.Scene tScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( tName ) ;
			
			if( tScene.IsValid() == false )
			{
				if( tRequest != null )
				{
					tRequest.error = "Could not load" ;
				}
				yield break ;
			}

			// シーンの展開が完了するのを待つ
			while( tScene.isLoaded == false )
			{
				yield return null ;
			}

			if( tType != null && rTarget != null && rTarget.Length >  0 )
			{
				GetInstance_Private( tScene, tType, rTarget, tTargetName, tRequest ) ;
			}

			if( tRequest != null )
			{
				tRequest.isDone = true ;
			}
		}

		//-----------------------------------

		private void GetInstance_Private( UnityEngine.SceneManagement.Scene tScene, Type tType, UnityEngine.Object[] rTarget, string tTargetName, Request tRequest )
		{
			if( ( rTarget != null && rTarget.Length >  0 ) || tRequest != null )
			{
				// 指定の型のコンポーネントを探してインスタンスを取得する
				List<UnityEngine.Object> tList = new List<UnityEngine.Object>() ;
				UnityEngine.Object[] tArray ;

				int i, j, l, m, p ;
				GameObject[] tGameObject = tScene.GetRootGameObjects() ;
				if( tGameObject != null && tGameObject.Length >  0 )
				{
					l = tGameObject.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tArray = tGameObject[ i ].GetComponentsInChildren( tType, true ) ;
						if( tArray != null && tArray.Length >  0 )
						{
							m = tArray.Length ;
							for( j  = 0 ; j <  m ; j ++ )
							{
								tList.Add( tArray[ j ] ) ;
							}
						}
					}
				}

				if( tList.Count >  0 )
				{
					// 該当のコンポーネントが見つかった
					l  = tList.Count ;

					if( tRequest != null )
					{
						if( string.IsNullOrEmpty( tTargetName ) == false )
						{
							// 名前によるフィルタ有り
							p = 0 ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tList[ i ].name == tTargetName )
								{
									p ++ ;
								}
							}

							if( p >  0 )
							{
								tRequest.instances = new UnityEngine.Object[ p ] ;

								p = 0 ;
								for( i  = 0 ; i <  l ; i ++ )
								{
									if( tList[ i ].name == tTargetName )
									{
										tRequest.instances[ p ] = tList[ i ] ;
										p ++ ;
									}
								}
							} 
						}
						else
						{
							// 名前によるフィルタ無し
							tRequest.instances = new UnityEngine.Object[ l ] ;

							for( i  = 0 ; i <  l ; i ++ )
							{
								tRequest.instances[ i ] = tList[ i ] ;
							} 
						}
					}


					if( rTarget != null && rTarget.Length >  0 )
					{
						m  = rTarget.Length ;

						if( string.IsNullOrEmpty( tTargetName ) == false )
						{
							// 名前によるフィルタ有り
							p = 0 ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tList[ i ].name == tTargetName )
								{
									rTarget[ p ] = tList[ i ] ;
									p ++ ;

									if( p >= m )
									{
										break ;
									}
								}
							} 
						}
						else
						{
							// 名前によるフィルタ無し
							if( l >  m )
							{
								l  = m ;
							}

							for( i  = 0 ; i <  l ; i ++ )
							{
								rTarget[ i ] = tList[ i ] ;
							} 
						}
					}
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定の名前のシーンがロード中か確認する
		/// </summary>
		/// <param name="tSceneName"></param>
		/// <returns></returns>
		public static bool IsLoaded( string tName )
		{
			UnityEngine.SceneManagement.Scene tScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( tName ) ;
			
			return tScene.isLoaded ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// ロードまたは加算されたシーンを破棄する(同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <returns>結果(true=成功・false=失敗</returns>
		public static bool Remove( string tName, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			return m_Instance.Remove_Private( tName, tLabel, tValue ) ;
		}

		// ロードまたは加算されたシーンを破棄する(実際は非同期になってしまう)
		private bool Remove_Private( string tName, string tLabel, System.Object tValue )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return false ;
			}
			
			UnityEngine.SceneManagement.Scene tScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( tName ) ;
			if( tScene.isLoaded == false )
			{
				// そのようなシーンは実際は存在しない
				return false ;
			}

			if( string.IsNullOrEmpty( tLabel ) == false )
			{
				SetParameter_Private( tLabel, tValue ) ;
			}

			// 同期メソッドが廃止されてしまった
			// 最後にロードしたシーンを破棄しようとすると警告が出る
			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( tName ) ;
			return true ;
		}

		//-----------------------------------

		/// <summary>
		/// ロードまたは加算されたシーンを破棄する(非同期版)
		/// </summary>
		/// <param name="tName">シーン名</param>
		/// <param name="tLabel">受け渡しパラメータの識別名</param>
		/// <param name="tValue">受け渡しパラメータのインスタンス</param>
		/// <param name="rResult">結果を格納する要素数１以上のブルー型の配列</param>
		/// <returns>列挙子</returns>
		public static Request RemoveAsync( string tName, bool[] rResult = null, string tLabel = null, System.Object tValue = null )
		{
			if( m_Instance == null )
			{
				Create() ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.RemoveAsync_Private( tName, rResult, tLabel, tValue, tRequest ) ) ;
			return tRequest ;
		}

		// ロードまたは加算されたシーンを破棄する(非同期版)
		private IEnumerator RemoveAsync_Private( string tName, bool[] rResult, string tLabel, System.Object tValue, Request tRequest )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				if( tRequest != null ){ tRequest.error = "could not remove" ; }
				if( rResult != null && rResult.Length >  0 ){ rResult[ 0 ] = false ; }
				yield break ;
			}
			
			UnityEngine.SceneManagement.Scene tScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( tName ) ;
			if( tScene.isLoaded == false )
			{
				// そのようなシーンは実際は存在しない
				if( tRequest != null ){ tRequest.error = "could not remove" ; }
				if( rResult != null && rResult.Length >  0 ){ rResult[ 0 ] = false ; }
				yield break ;
			}

			if( string.IsNullOrEmpty( tLabel ) == false )
			{
				SetParameter_Private( tLabel, tValue ) ;
			}

			// 最後にロードしたシーンを破棄しようとすると警告が出る
			AsyncOperation tAsyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( tName ) ;
			yield return tAsyncOperation ;

			if( tAsyncOperation != null && tAsyncOperation.isDone == true )
			{
				if( tRequest != null ){ tRequest.isDone = true ; }
				if( rResult != null && rResult.Length >  0 ){ rResult[ 0 ] = true ; }
			}
			else
			{
				if( tRequest != null ){ tRequest.error = "could not remove" ; }
				if( rResult != null && rResult.Length >  0 ){ rResult[ 0 ] = false ; }
			}
		}
		
		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// １つ前のシーンの名前を取得する
		/// </summary>
		/// <returns>１つ前のシーンの名前(nullで存在しない)</returns>
		public static string GetPreviousName()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetPreviousName_Private() ;
		}

		// １つ前のシーンの名前を取得する
		private string GetPreviousName_Private()
		{
//			if( history.Count <= 1 )
//			{
//				return null ;
//			}
//
//			int c = history.Count ;
//			return history[ c - 2 ] ;
			return previousName ;
		}
		
		/// <summary>
		/// 現在のシーン名を取得する
		/// </summary>
		/// <returns></returns>
		public static string GetActiveName()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.GetActiveName_Private() ;
		}

		// 現在のシーンの名前を取得する
		private string GetActiveName_Private()
		{
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name ;
		}
		
		//-----------------------------------------------------------------
	}
}


