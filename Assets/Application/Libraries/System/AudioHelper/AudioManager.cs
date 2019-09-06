// #define USE_MICROPHONE

using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.SceneManagement ;

#if UNITY_EDITOR
using UnityEditor ;
#endif



// シングルトンはエディタモードに対応させてはダメ
// なぜならシングルトンを判定するスタティックなインスタンス変数がシリアライズ化出来ないため


/// <summary>
/// オーディオヘルパーハッケージ
/// </summary>
namespace AudioHelper
{
	/// <summary>
	/// オーディオ全般の管理クラス Version 2018/10/17 0
	/// </summary>
	public class AudioManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AudioManager を生成
		/// </summary>
//		[MenuItem("AudioHelper/Create a AudioManager")]
		[MenuItem("GameObject/Helper/AudioHelper/AudioManager", false, 24)]
		public static void CreateAudioManager()
		{
			GameObject tGameObject = new GameObject( "AudioManager" ) ;
		
			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;
		
			tGameObject.AddComponent<AudioManager>() ;
			Selection.activeGameObject = tGameObject ;
		}
#endif

		// オーディオマネージャのインスタンス(シングルトン)
		private static AudioManager m_Instance = null ; 

		/// <summary>
		/// オーディオマネージャのインスタンス
		/// </summary>
		public  static AudioManager   instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		//---------------------------------------------------------
	
		// リスナー（Awake 時にクリアされるのでシリアライズ化して保持する意味が無い）
		private AudioListener m_Listener = null ;

		/// <summary>
		/// オーディオリスナーのインスタンス
		/// </summary>
		public  AudioListener   listener{ get{ return m_Listener ; } }
	
		/// <summary>
		/// チャンネル情報
		/// </summary>
		public List<AudioChannel> channels = new List<AudioChannel>() ;

		/// <summary>
		/// 最大チャンネル数
		/// </summary>
		public int max = 32 ;
		
		//-----------------------------------------------------------

		// 発音毎に割り振られるユニークな識別子
		private int m_PlayId = 0 ;

		// １増加した発音毎に割り振られるユニークな識別子
		private int GetPlayId()
		{
			m_PlayId = ( m_PlayId + 1 ) & 0x7FFFFFFF ;
			return m_PlayId ;
		}


		/// <summary>
		/// スタートフェードとストップフェードのエフェクト
		/// </summary>
		internal protected class FadeEffect
		{
			/// <summary>
			/// 基本基準時間
			/// </summary>
			internal protected float baseTime ;

			/// <summary>
			/// 基本音量
			/// </summary>
			internal protected float baseVolume ;

			/// <summary>
			/// フェード時間
			/// </summary>
			internal protected float duration ;

			/// <summary>
			/// フェード中のアクティブな音量
			/// </summary>
			internal protected float volume ;
		}
	
	
		// プレイフェード用
		private Dictionary<AudioChannel,FadeEffect> m_FadePlayList = new Dictionary<AudioChannel, FadeEffect>() ;
		private Dictionary<AudioChannel,FadeEffect> m_FadeStopList = new Dictionary<AudioChannel, FadeEffect>() ;
	
		/// <summary>
		/// マスターボリューム
		/// </summary>
		public float masterVolume = 1.0f ;
	
		// ミュート
		private bool m_Mute = false ;
	
		// ミュートリスト
		private List<string> m_MuteList = new List<string>() ;
	
		/// <summary>
		/// バックグラウンド再生を有効にするかどうか
		/// </summary>
		public bool runInBackground = false ;


		//-----------------------------------------------------

#if USE_MICROPHONE
		// マイク関係
		private AudioSource			m_MicrophoneRecordingAudioSource	= null ;
		private AudioClip			m_MicrophoneRecordingAudioClip		= null ;
		private	int					m_MicrophoneRecordingFrequency		= 44100 ;
		private int					m_MicrophoneRecordingTime			= 1 ; 
		private float[]				m_MicrophoneRecordingBuffer			= null ;
		private int					m_MicrophoneRecordingPosition		= 0 ;
		private Action<float[],int>	m_MicrophoneCallback				= null ;
		private bool				m_MicrophoneCallbackDataSkip		= false ;
		private float[]				m_MicrophoneCallbackBuffer			= null ;
#endif

		//-----------------------------------------------------

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		public delegate void OnStopped( int tPlayId ) ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲート
		/// </summary>
		public OnStopped onStopped ;

		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnStopped">追加するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool AddOnStopped( OnStopped tOnStopped )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onStopped += tOnStopped ;

			return true ;
		}
		
		/// <summary>
		/// いずれかのチャンネルが停止された際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnStopped">削除するデリゲートメソッド</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveOnStopped( OnStopped tOnStopped )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.onStopped -= tOnStopped ;

			return true ;
		}

		//---------------------------------------------------------
	
		/// <summary>
		/// オーディオマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="tRunInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>オーディオマネージャのインスタンス</returns>
		public static AudioManager Create( Transform tParent = null, bool tIsListener = true, bool tRunInbackground = false )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( AudioManager ) ) as AudioManager ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "AudioManager" ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}

				tGameObject.AddComponent<AudioManager>() ;
			}

			m_Instance.runInBackground	= tRunInbackground ;

			// リスナーの有効無効の設定
			m_Instance.m_Listener.enabled = tIsListener ;

			return m_Instance ;
		}
	
		/// <summary>
		/// オーディオマネージャのインスタンスを破棄する
		/// </summary>
		public static void Delete()
		{	
			if( m_Instance != null )
			{
				ClearAll() ;
			
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
		
			AudioManager tInstanceOther = GameObject.FindObjectOfType( typeof( AudioManager ) ) as AudioManager ;
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
			
			// シーンがロードされた際に呼び出されるデリゲートを登録する
			SceneManager.sceneLoaded += OnLevelWasLoaded_Private ;	// MonoBehaviour の OnLevelWasLoaded メソッドは廃止予定

			// シーン切り替え時に破棄されないようにする(ただし自身がルートである場合のみ有効)
			if( transform.parent == null )
			{
				DontDestroyOnLoad( gameObject ) ;
			}
		
	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		
			//-----------------------------
			
			// リスナーを張り付けておく
			CreateAudioListener( true ) ;
		
			// チャンネルリストをクリアする
			channels.Clear() ;
		
			// ミュートリストをクリアする
			m_MuteList.Clear() ;
		
			// フェードプレイリストをクリアする
			m_FadePlayList.Clear() ;
		
			// フェードストップリストをクリアする
			m_FadeStopList.Clear() ;
		}

		// 何等かのシーンがロードされた際に呼び出される
		private void OnLevelWasLoaded_Private( Scene tSceneName, LoadSceneMode tSceneMode )
		{
//			Debug.Log( "Scene Loaded : " + tSceneName + " : " + tSceneMode.ToString() + " )" ) ;
			if( tSceneMode == LoadSceneMode.Single )
			{
				bool tEnable = true ;
				if( m_Listener != null )
				{
					tEnable = m_Listener.enabled ;
				}

				CreateAudioListener( tEnable ) ;
			}
		}

		// オーディオリスナーを生成する
		private bool CreateAudioListener( bool tEnable )
		{
			if( m_Listener != null )
			{
				m_Listener.enabled = tEnable ;
				return true ;	// 既に自身のオーディオリスナーを生成済みになっている
			}

			AudioListener tListener = GameObject.FindObjectOfType( typeof( AudioListener ) ) as AudioListener ;

			GameObject tGameObject = new GameObject( "Listener" ) ;
			tGameObject.transform.SetParent( transform, false ) ;

			// 存在しない(自身のオーディオリスナーを生成する)
			m_Listener = tGameObject.AddComponent<AudioListener>() ;
			AudioListener.volume = 1.0f ;

			if( tListener == null )
			{
				m_Listener.enabled = tEnable ;

				return true ;
			}
			else
			{
				// 存在する
#if UNITY_EDITOR
				Debug.LogWarning( "[AudioManager] 既に AudioListener が存在します" ) ;
#endif
				m_Listener.enabled = false ;

				return false ;
			}
		}

		void Update()
		{
			// ソースを監視して停止している（予定）
		
			// 一定時間ごとにコールされる
			Process() ;

#if USE_MICROPHONE
			// マイクの録音
			ProcessMicrophoneRecording() ;
#endif
		}
	
		void OnDestroy()
		{
			if( m_Instance == this )
			{
				// シーンがロードされた際に呼び出されるデリゲートを削除する
				SceneManager.sceneLoaded -= OnLevelWasLoaded_Private ;	// MonoBehaviour の OnLevelWasLoaded メソッドは廃止予定

				m_Listener = null ;
				m_Instance  = null ;
			}
		}

		//-----------------------------------------------------------------
#if USE_MICROPHONE
		/// <summary>
		/// マイク入力を開始する
		/// </summary>
		/// <param name="rMicrophone"></param>
		/// <returns></returns>
		public static IEnumerator StartMicrophone( int tRecordingTime, int tRecordingFrequency, int tBufferSize = 0, bool tDataSkip = false, Action<float[],int> tCallback = null )
		{
			if( m_Instance == null )
			{
				yield break ;
			}

			yield return m_Instance.StartCoroutine( m_Instance.StartMicrophone_Private( tRecordingTime, tRecordingFrequency, tBufferSize, tDataSkip, tCallback ) ) ;
		}

		// マイク入力を開始する
		private IEnumerator StartMicrophone_Private( int tRecordingTime, int tRecordingFrequency, int tBufferSize, bool tDataSkip, Action<float[],int> tCallback )
		{
			if( Microphone.devices == null || Microphone.devices.Length == 0 )
			{
				// マイクが接続されていない模様
				yield break ;
			}

			//----------------------------------------------------------

			if( m_MicrophoneRecordingAudioSource == null )
			{
				GameObject tGameObject = new GameObject( "Microphone" ) ; 
				tGameObject.transform.SetParent( transform, false ) ;

				// AudioManager に直で AudioSource を Add してはダメ
				m_MicrophoneRecordingAudioSource = tGameObject.AddComponent<AudioSource>() ;
				m_MicrophoneRecordingAudioSource.playOnAwake = false ;
			}

			m_MicrophoneRecordingAudioSource.enabled = true ;

			m_MicrophoneRecordingTime		= tRecordingTime ;
			m_MicrophoneRecordingFrequency	= tRecordingFrequency ;

			AudioClip tAudioClip = Microphone.Start( null, true, m_MicrophoneRecordingTime, m_MicrophoneRecordingFrequency ) ;

			m_MicrophoneCallback				= tCallback ;
			if( tCallback != null && tBufferSize >  0 )
			{
				m_MicrophoneRecordingAudioClip	= tAudioClip ;
				m_MicrophoneRecordingBuffer		= new float[ m_MicrophoneRecordingTime * m_MicrophoneRecordingFrequency ] ; 

				m_MicrophoneCallbackBuffer		= new float[ tBufferSize ] ;

				m_MicrophoneCallbackDataSkip	= tDataSkip ;
				m_MicrophoneRecordingPosition	= 0 ;
			}

			m_MicrophoneRecordingAudioSource.clip = tAudioClip ;

			// 準備が整うまで待つ
			yield return new WaitWhile( () => Microphone.GetPosition( null ) <= 0 ) ;

			m_MicrophoneRecordingAudioSource.timeSamples = Microphone.GetPosition( null ) ;
			m_MicrophoneRecordingAudioSource.loop = true ;
			m_MicrophoneRecordingAudioSource.Play() ;
		}

		/// <summary>
		/// マイク入力を停止する
		/// </summary>
		/// <param name="tMicrophone"></param>
		/// <returns></returns>
		public static bool StopMicrophone()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.StopMicrophone_Private() ;
		}

		private bool StopMicrophone_Private()
		{
			if( m_MicrophoneRecordingAudioSource != null )
			{
				m_MicrophoneRecordingAudioSource.Stop() ;
				m_MicrophoneRecordingAudioSource.enabled = false ;
			}

			Microphone.End( null ) ;

			if( m_MicrophoneCallback != null )
			{
				m_MicrophoneCallbackBuffer		= null ;
				m_MicrophoneRecordingBuffer		= null ;
				m_MicrophoneRecordingAudioClip	= null ;

				m_MicrophoneCallback			= null ;
			}


			return true ;
		}

		/// <summary>
		/// マイクのオーディオソースを取得する
		/// </summary>
		/// <returns></returns>
		public static AudioSource GetMicrophone()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			return m_Instance.m_MicrophoneRecordingAudioSource ;
		}

		// マイクの録音処理
		private void ProcessMicrophoneRecording()
		{
			if( m_MicrophoneRecordingAudioClip == null || m_MicrophoneRecordingBuffer == null || m_MicrophoneCallback == null || m_MicrophoneCallbackBuffer == null )
			{
				return ;
			}

			//----------------------------------------------------------

			int tPosition = Microphone.GetPosition( null ) ;
			if( tPosition <  0 || tPosition == m_MicrophoneRecordingPosition )
			{
				return ;
			}

			int tCB_Size = m_MicrophoneCallbackBuffer.Length ; 
			int tRB_Size = m_MicrophoneRecordingBuffer.Length ;

			int tSize ;

			// サンプルデータをコピーする
			m_MicrophoneRecordingAudioClip.GetData( m_MicrophoneRecordingBuffer, 0 ) ;

			while( true )
			{
				if( m_MicrophoneRecordingPosition <  tPosition )
				{
					// そのまま引き算で録音サイズ数が計算できる
					tSize = tPosition - m_MicrophoneRecordingPosition ;
				}
				else
				{
					// バッファの反対側に回っているので若干計算が複雑になる
					tSize = ( tRB_Size - m_MicrophoneRecordingPosition ) + tPosition ;
				}
	
				if( tSize <  tCB_Size )
				{
					// 必要量に足りない
					return ;
				}

				// レコーディングバッファのサンプルデータをコルーバックバッファにコピーする
				if( ( m_MicrophoneRecordingPosition + tCB_Size ) <= tRB_Size )
				{
					// １回でコピー可能
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, tCB_Size ) ;
				}
				else
				{
					// ２回でコピー可能
					int o = tRB_Size - m_MicrophoneRecordingPosition ;
					Array.Copy( m_MicrophoneRecordingBuffer, m_MicrophoneRecordingPosition, m_MicrophoneCallbackBuffer, 0, o ) ;
					Array.Copy( m_MicrophoneRecordingBuffer, 0,                             m_MicrophoneCallbackBuffer, 0, tCB_Size - o ) ;
				}
				
				// コールバック実行
				m_MicrophoneCallback( m_MicrophoneCallbackBuffer, m_MicrophoneRecordingFrequency ) ;

				// 読み取り位置を変化させる
				if( m_MicrophoneCallbackDataSkip == false )
				{
					// データスキップ無し
					m_MicrophoneRecordingPosition = ( m_MicrophoneRecordingPosition + tCB_Size ) % tRB_Size ;
				}
				else
				{
					// データスキップ有り
					m_MicrophoneRecordingPosition = tPosition ;
					return ;	// 終了
				}
			}
		}
#endif
		//-----------------------------------------------------------------

		// 以下スクリプトから利用するメソッド群

		/// <summary>
		/// リスナーの状態
		/// </summary>
		public static bool listenerEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.listenerEnabled_Private ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.listenerEnabled_Private = value ;
			}
		}
		
		// リスナーの状態
		private bool listenerEnabled_Private
		{
			get
			{
				if( m_Listener == null )
				{
					return false ;
				}

				return m_Listener.enabled ;
			}
			set
			{
				if( m_Listener == null )
				{
					return ;
				}

				m_Listener.enabled = value ;
			}
		}
		

		/// <summary>
		/// いずれかの空いているオーディオチャンネルを１つ取得する
		/// </summary>
		/// <returns>オーディオチャンネルのインスタンス</returns>
		public static AudioChannel GetChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetChannel_Private() ;
		}
		
		// いずれかの空いているオーディオチャンネルを１つ取得する
		private AudioChannel GetChannel_Private()
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null && channels[ i ].IsPlaying() == false && channels[ i ].IsPausing() == false && channels[ i ].busy == false )
				{
					// 空いているチャンネルを発見した
					return channels[ i ] ;
				}
			}
		
			if( l >= max )
			{
				return null ;	// 最大チャンネルまで使用済み
			}
		
			// 空いているソースが無いので追加する
		
			AudioChannel tAudioChannel = new AudioChannel( gameObject.AddComponent<AudioSource>() ) ;
			channels.Add( tAudioChannel ) ;
		
			return tAudioChannel ;
		}
	
		/// <summary>
		/// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		/// </summary>
		/// <returns>使用中の全てのチャンネルのインスタンス</returns>
		public static string[] GetUsedChannel()
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetUsedChannel_Private() ;
		}
	
		// 使用中の全てのチャンネルの名前(再生中のオーディオクリップ名)を取得する
		private string[] GetUsedChannel_Private()
		{
			List<string> tList = new List<string>() ;
		
			int i, l =channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPlaying() == true || channels[ i ].IsPausing() == true || channels[ i ].busy == true )
					{
						// 使用中のチャンネルを発見した
						tList.Add( channels[ i ].name ) ;
					}
				}
			}
		
			if( tList.Count == 0 )
			{
				return null ;
			}
		
			return tList.ToArray() ;
		}
		
		// 指定した識別子に該当する再生中のオーディオチャンネルを取得する
		private AudioChannel GetChannelByPlayId( int tPlayId )
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( ( channels[ i ].IsPlaying() == true || channels[ i ].IsPausing() == true || channels[ i ].busy == true ) && channels[ i ].playId == tPlayId )
					{
						// 該当するチャンネルを発見した
						return channels[ i ] ;
					}
				}
			}
			
			// 該当するチャンネルが存在しない1
			return null ;
		}

		//---------------------------------------------------------
	
		/// <summary>
		/// サウンドを再生する(リソースパスから)
		/// </summary>
		/// <param name="tPath">リソースパス</param>
		/// <param name="tLoop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tPitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tDelay">再生までの遅延時間(秒)</param>
		/// <param name="tTag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play( string tPath, bool tLoop = false, float tVolume = 1.0f, float tPan = 0.0f, float tPitch = 0.0f, float tDelay = 0.0f, string tTag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			AudioClip tClip = Resources.Load( tPath ) as AudioClip ;
			if( tClip == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play_Private( tClip, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		}
		
		/// <summary>
		/// サウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="tClip">オーディオクリップのインスタンス</param>
		/// <param name="tLoop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tPitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上)</param>
		/// <param name="tDelay">再生までの遅延時間(秒)</param>
		/// <param name="tTag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int Play( AudioClip tClip, bool tLoop = false, float tVolume = 1.0f, float tPan = 0.0f, float tPitch = 0.0f, float tDelay = 0.0f, string tTag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.Play_Private( tClip, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		}
	
		//----------------
	
		// サウンドを再生する(オーディオクリップから)
		private int Play_Private( AudioClip tClip, bool tLoop, float tVolume, float tPan, float tPitch, float tDelay, string tTag )
		{
			AudioChannel tAudioChannel = GetChannel_Private() ;
			if( tAudioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
			
			int tPlayId = GetPlayId() ;

			tAudioChannel.Play( tPlayId, tClip, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		
			if( string.IsNullOrEmpty( tTag ) == false && m_MuteList.Contains( tTag ) == true )
			{
				// ミュート対象のタグ
				tAudioChannel.mute = true ;
			}
			
			// 成功したらソースのインスタンスを返す
			return tPlayId ;
		}
	
		//---------------------------------
	
		/// <summary>
		/// フェード付きでサウンドを再生する(リソースパスから)
		/// </summary>
		/// <param name="tPath">リソースパス</param>
		/// <param name="tDuration">フェードの時間(秒)</param>
		/// <param name="tLoop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tPan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="tPitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tDelay">再生までの遅延時間(秒)</param>
		/// <param name="tTag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade( string tPath, float tDuration = 1.0f, bool tLoop = false, float tVolume = 1.0f, float tPan = 0.0f, float tPitch = 0.0f, float tDelay = 0.0f, string tTag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			AudioClip tClip = Resources.Load( tPath ) as AudioClip ;
			if( tClip == null )
			{
				return -1 ;
			}
		
			return m_Instance.PlayFade_Private( tClip, tDuration, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		}
	
		/// <summary>
		/// フェード付きでサウンドを再生する(オーディオクリップから)
		/// </summary>
		/// <param name="tClip">オーディオクリップのインスタンス</param>
		/// <param name="tDuration">フェードの時間(秒)</param>
		/// <param name="tLoop">ルーブの有無(true=有効・false=無効)</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tPan">パンの位置(-1=左～0=中～+1=右)</param>
		/// <param name="tPitch">ピッチ(-1=1オクターブ下～0=通常～+1=１オクターブ上</param>
		/// <param name="tDelay">再生までの遅延時間(秒)</param>
		/// <param name="tTag">複数のチャンネルを同時に操作するための区別用のタグ名</param>
		/// <returns>発音毎に割り振られるユニークな識別子(-1で失敗)</returns>
		public static int PlayFade( AudioClip tClip, float tDuration = 1.0f, bool tLoop = false, float tVolume = 1.0f, float tPan = 0.0f, float tPitch = 0.0f, float tDelay = 0.0f, string tTag = "" )
		{
			if( m_Instance == null )
			{
				return -1 ;
			}
		
			return m_Instance.PlayFade_Private( tClip, tDuration, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		}
	
		//--------------------
	
		// フェード付きでサウンドを再生する(オーディオクリップから)
		private int PlayFade_Private( AudioClip tClip, float tDuration, bool tLoop, float tVolume, float tPan, float tPitch, float tDelay, string tTag )
		{
			AudioChannel tAudioChannel = GetChannel_Private() ;
			if( tAudioChannel == null )
			{
				// 空きがありません（古いやつから停止させるようにするかどうか）
				return -1 ;
			}
		
			//-----------------------------------------------------
		
			// プレイフェードに登録する
		
			// 一旦破棄
			RemoveFadeList( tAudioChannel ) ;
		
			FadeEffect tEffect = new FadeEffect() ;
		
			tEffect.baseTime   = Time.realtimeSinceStartup ;
			tEffect.baseVolume = 0 ;
			tEffect.duration   = tDuration ;
			tEffect.volume     = tVolume ;
		
			m_FadePlayList.Add( tAudioChannel, tEffect ) ;
		
			// フェード処理中であるというロックをかける
			tAudioChannel.busy = true ;
		
			//-----------------------------------------------------
			
			int tPlayId = GetPlayId() ;

			// 再生する
			tAudioChannel.Play( tPlayId, tClip, tLoop, tVolume, tPan, tPitch, tDelay, tTag ) ;
		
			if( string.IsNullOrEmpty( tTag ) == false && m_MuteList.Contains( tTag ) == true )
			{
				// ミュート対象のタグ
				tAudioChannel.mute = true ;
			}
		
			// 成功したらソースのインスタンスを返す
			return tPlayId ;
		}


		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(リソースパスから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="tPath">リソースパス</param>
		/// <param name="tPosition">音源のワールド空間座標</param>
		/// <param name="tListener">リスナーのトランスフォーム</param>
		/// <param name="tScale">距離の係数</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Play3D( string tPath, Vector3 tPosition, Transform tListener = null, float tScale = 1, float tVolume = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			AudioClip tAudioClip = Resources.Load( tPath ) as AudioClip ;
			if( tAudioClip == null )
			{
				return false ;
			}
		
			return m_Instance.Play3D_Private( tAudioClip, tPosition, tListener, tScale, tVolume ) ;
		}

		/// <summary>
		/// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		/// </summary>
		/// <param name="tAudioClip">オーディオクリップのインスタンス</param>
		/// <param name="tPosition">音源のワールド空間座標</param>
		/// <param name="tListener">リスナーのトランスフォーム</param>
		/// <param name="tScale">距離の係数</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Play3D( AudioClip tAudioClip, Vector3 tPosition, Transform tListener = null, float tScale = 1, float tVolume = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Play3D_Private( tAudioClip, tPosition, tListener, tScale, tVolume ) ;
		}

		// ３Ｄ空間想定でサウンドをワンショット再生する(オーディオクリップから)　※３Ｄ効果音用
		private bool Play3D_Private( AudioClip tAudioClip, Vector3 tPosition, Transform tListener, float tScale, float tVolume )
		{
			if( tListener != null )
			{
				// リスナーのローカル座標系に変換する
				tPosition = tListener.InverseTransformPoint( tPosition ) ;
			}

			tPosition = tPosition * tScale ;

			AudioSource.PlayClipAtPoint( tAudioClip, tPosition, tVolume ) ;

			return true ;
		}

		//-----------------------------------------------------------



		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音のオーディオクリップ名を取得する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static string GetName( int tPlayId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetName_Private( tPlayId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private string GetName_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//----------------------------------

			return tAudioChannel.name ;
		}

		/// <summary>
		/// 再生中のオーディオクリップを取得する
		/// </summary>
		/// <param name="tPlayId"></param>
		/// <returns></returns>
		public static AudioClip GetClip( int tPlayId )
		{
			if( m_Instance == null )
			{
				return null ;
			}
		
			return m_Instance.GetClip_Private( tPlayId ) ;
		}
	
		//----------------
	
		// 再生中のオーディオクリップを取得する
		private AudioClip GetClip_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return null ;
			}

			//-----------------------------------------------------
			
			return tAudioChannel.clip ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsRunning( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsRunning_Private( tPlayId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsRunning_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return tAudioChannel.IsRunning() ;
		}
		

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPlaying( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPlaying_Private( tPlayId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPlaying_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return tAudioChannel.IsPlaying() ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsPausing( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsPausing_Private( tPlayId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsPausing_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			return tAudioChannel.IsPausing() ;
		}

		/// <summary>
		/// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns></returns>
		public static bool IsUsing( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.IsUsing_Private( tPlayId ) ;

		}

		// 指定された発音毎に割り振られるユニークな識別子で示される発音が継続しているかを取得する
		private bool IsUsing_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			if( tAudioChannel.IsPlaying() == true || tAudioChannel.IsPausing() == true || tAudioChannel.busy == true )
			{
				return true ;
			}

			return false ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとのミュート状態を設定する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="tState">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Mute( int tPlayId, bool tState )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Mute_Private( tPlayId, tState ) ;
		}
		
		// オーディオチャンネルごとのミュート状態を設定する
		private bool Mute_Private( int tPlayId, bool tState )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			tAudioChannel.mute = tState ;	// 完全停止
				
			return true ;	// 発見
		}
		
		/// <summary>
		/// オーディオチャンネルごとにサウンドを完全停止させる
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Stop( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Stop_Private( tPlayId ) ;
		}
		
		// オーディオチャンネルごとにサウンドを完全停止させる
		private bool Stop_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			tAudioChannel.Stop() ;	// 完全停止
				
			// フェードリストから除外する
			RemoveFadeList( tAudioChannel ) ;
					
			// 即時呼び出す
			if( tAudioChannel.callback == true )
			{
				if( onStopped != null )
				{
					onStopped( tPlayId ) ;
				}
				tAudioChannel.callback = false ;
			}

			return true ;	// 成功
		}

		/// <summary>
		/// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <param name="tDuration">フェード時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopFade( int tPlayId, float tDuration = 1.0f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return  m_Instance.StopFade_Private( tPlayId, tDuration ) ;
		}
	
		// オーディオチャンネルごとにフェードでサウンドを完全停止させる
		private bool StopFade_Private( int tPlayId, float tDuration )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のチャンネルを発見した（ただし既にフェード対象に対する多重の効果はかけられない）
				
			if( tAudioChannel.IsPlaying() == true && m_FadeStopList.ContainsKey( tAudioChannel ) == false )
			{
				// 一旦破棄
				RemoveFadeList( tAudioChannel ) ;	// 多重実行は禁止したが保険
					
				FadeEffect tEffect = new FadeEffect() ;
					
				tEffect.baseTime   = Time.realtimeSinceStartup ;
				tEffect.baseVolume = tAudioChannel.volume ;
				tEffect.duration   = tDuration ;
				tEffect.volume     = 0 ;
					
				m_FadeStopList.Add( tAudioChannel, tEffect ) ;
					
				// フェード処理中なのでロックする
				tAudioChannel.busy = true ;
					
				return true ;   // 発見
			}
			else
			{
				// ポーズ中であれば即時停止させる
				tAudioChannel.Stop() ;
					
				// リストから除外する
				RemoveFadeList( tAudioChannel ) ;

				// 即時呼び出す
				if( tAudioChannel.callback == true )
				{
					if( onStopped != null )
					{
						onStopped( tPlayId ) ;
					}
					tAudioChannel.callback = false ;
				}
			}
		
			return false ;
		}
	
	
		/// <summary>
		/// オーディオチャンネルごとにサウンドを一時停止させる
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Pause( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Pause_Private( tPlayId ) ;
		}
		
		// オーディオチャンネルごとにでサウンドを一時停止させる
		private bool Pause_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			if( tAudioChannel.IsPlaying() == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				PauseFadeList( tAudioChannel ) ;
					
				tAudioChannel.Pause() ;	// 一時停止
			}
			
			return true ;
		}
	
		/// <summary>
		/// オーディオチャンネルごとに一時停止を解除させる
		/// </summary>
		/// <param name="tPlayId">発音毎に割り振られるユニークな識別子</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Unpause( int tPlayId )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.Unpause_Private( tPlayId ) ;
		}
		
		// オーディオチャンネルごとに一時停止を解除させる
		private bool Unpause_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return false ;
			}

			//----------------------------------

			// 再生中のソースを発見した
				
			// 注意：AudioChannel では、終了中なのか中断中なのか区別がつかない
			if( tAudioChannel.IsPausing() == true )
			{
				// ポーズ状態になるのでベースタイムとベースボリュームを更新する
				UnpauseFadeList( tAudioChannel ) ;
					
				tAudioChannel.Unpause() ;	// 再開
			}
			else
			{
				// ポーズ中でなく普通に終わっているだけなら再生する
				tAudioChannel.Unpause() ;
			}
				
			return true ;	// 発見
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 再生中の位置(秒)を取得する
		/// </summary>
		/// <param name="tPlayId"></param>
		/// <returns></returns>
		public static float GetTime( int tPlayId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTime_Private( tPlayId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private float GetTime_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return tAudioChannel.time ;
		}

		/// <summary>
		/// 再生中の位置(サンプル数)を取得する
		/// </summary>
		/// <param name="tPlayId"></param>
		/// <returns></returns>
		public static int GetTimeSamples( int tPlayId )
		{
			if( m_Instance == null )
			{
				return 0 ;
			}
		
			return m_Instance.GetTimeSamples_Private( tPlayId ) ;
		}
	
		// 再生中の位置(秒)を取得する
		private int GetTimeSamples_Private( int tPlayId )
		{
			AudioChannel tAudioChannel = GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return 0 ;
			}

			//-----------------------------------------------------
			
			return tAudioChannel.timeSamples ;
		}
		
		//-----------------------------------------------------------
	
		/// <summary>
		/// オーディオチャンネルごとの現在の状態を取得する
		/// </summary>
		/// <param name="tAudioChannel">オーディオチャンネルのインスタンス</param>
		/// <returns>現在の状態(-1=不正な引数・0=一時停止か完全停止・1=再生</returns>
/*		public static int GetStatus( AudioChannel tAudioChannel )
		{
			if( tAudioChannel == null )
			{
				return -1 ;
			}
		
			if( tAudioChannel.IsPlaying() == true )
			{
				return 1 ;	// 再生中
			}
		
			return 0 ;	// 停止中か一時停止中
		}*/
	
		//---------------------------------------------------------
	
		// 個別に停止したい場合は再生時に取得したオーディオソースに対して Stop() を実行する
	
		// オーディオに関しては実装が不完全と言える
		// 音自体の再生停止は runInBackground とは別に管理しなくてはならない（別に管理出来るようにする）
		// （あまり使い道は無いかもしれないがゲームが止まっても音は流し続ける事も可能）
		// しかしフェード効果は Update で処理しているためバックグウンド時はフェード効果が正しく処理されなくなってしまう
	
		// サスペンド・レジューム
		void OnApplicationPause( bool tState )
		{
			if( tState == true )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}	
	
		// フォーカス
		void OnApplicationFocus( bool tState )
		{
			if( tState == false )
			{
				// サスペンド
				SuspendAll_Private() ;
			}
			else
			{
				// レジューム
				ResumeAll_Private() ;
			}
		}
	
		// 全サスペンド
		private bool SuspendAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( runInBackground == true )
			{
				return true ;
			}
		
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPlaying() == true )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						PauseFadeList( channels[ i ] ) ;
					
						channels[ i ].Pause() ;	// 一時停止
					
						channels[ i ].sleep = true ;
					}
				}
			}
		
			return true ;
		}
	
		// 全レジューム
		private bool ResumeAll_Private()
		{
			// マネージャを生成するのは一ヶ所だけなのでひとまず Application.runInBackground とは独立管理出来るようにしておく
			if( runInBackground == true )
			{
				return true ;
			}
		
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPausing() == true && channels[ i ].sleep == true )
					{
						// リプレイ状態になるのでベースタイムとベースボリュームを更新する
						UnpauseFadeList( channels[ i ] ) ;
					
						channels[ i ].Unpause() ;	// 一時停止
					
						channels[ i ].sleep = false ;
					}
				}
			}
		
			return true ;
		}
		
		//---------------------------------------------------------------------------
	
		/// <summary>
		/// 全オーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="tState">ミュート状態(true=オン・false=オフ)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteAll( bool tState )
		{
			return MuteWithTag( tState, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのミュート状態を設定する
		/// </summary>
		/// <param name="tState">ミュート状態(true=オン・false=オフ)</param>
		/// <param name="tTag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool MuteWithTag( bool tState, string tTag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.MuteWithTag_Private( tState, tTag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのミュート状態を設定する
		private bool MuteWithTag_Private( bool tState, string tTag )
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPlaying() == true )
					{
						if( string.IsNullOrEmpty( tTag ) == true || ( string.IsNullOrEmpty( tTag ) == false && channels[ i ].tag == tTag ) )
						{
							channels[ i ].mute = tState ;	// ミュート設定
						}
					}
				}
			}
		
			// タグ指定でミュートリストに保存する
			if( string.IsNullOrEmpty( tTag ) == false )
			{
				if( tState == true )
				{
					// オン
					if( m_MuteList.Contains( tTag ) == false )
					{
						m_MuteList.Add( tTag ) ;
					}
				}
				else
				{
					// オフ
					if( m_MuteList.Contains( tTag ) == true )
					{
						m_MuteList.Remove( tTag ) ;
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルを完全停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll()
		{
			return StopAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを完全停止させる
		/// </summary>
		/// <param name="tTag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool StopAll( string tTag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.StopAll_Private( tTag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを完全停止させる
		private bool StopAll_Private( string tTag )
		{
			AudioChannel tAudioChannel ;
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					tAudioChannel = channels[ i ] ;
					if( string.IsNullOrEmpty( tTag ) == true || ( string.IsNullOrEmpty( tTag ) == false && tAudioChannel.tag == tTag ) )
					{
						// 再生中のチャンネルを発見した
						tAudioChannel.Stop() ;
					
						// フェードリストから除外する
						RemoveFadeList( tAudioChannel ) ;

						// 即時呼び出す
						if( tAudioChannel.callback == true )
						{
							if( onStopped != null )
							{
								onStopped( tAudioChannel.playId ) ;
							}
							tAudioChannel.callback = false ;
						}
					}
				}
			}
		
			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを一時停止させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll()
		{
			return PauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルを一時停止させる
		/// </summary>
		/// <param name="tTag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PauseAll( string tTag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.PauseAll_Private( tTag ) ;
		}
		
		// タグで指定されたオーディオチャンネルを一時停止させる
		private bool PauseAll_Private( string tTag )
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPlaying() == true )
					{
						if( string.IsNullOrEmpty( tTag ) == true || ( string.IsNullOrEmpty( tTag ) == false && channels[ i ].tag == tTag ) )
						{
							// ポーズ状態になるのでベースタイムとベースボリュームを更新する
							PauseFadeList( channels[ i ] ) ;
						
							channels[ i ].Pause() ;	// 一時停止
						}
					}
				}
			}
		
			return true ;
		}
		
		/// <summary>
		/// 全オーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll()
		{
			return UnpauseAll( null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルの一時停止を解除させる
		/// </summary>
		/// <param name="tTag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool UnpauseAll( string tTag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.UnpauseAll_Private( tTag ) ;
		}
		
		// タグで指定されたオーディオチャンネルの一時停止を解除させる
		private bool UnpauseAll_Private( string tTag )
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( channels[ i ].IsPlaying() == false && channels[ i ].IsRunning() == true )
					{
						if( string.IsNullOrEmpty( tTag ) == true || ( string.IsNullOrEmpty( tTag ) == false && channels[ i ].tag == tTag ) )
						{
							// ポーズ解除状態になるのでベースタイムとベースボリュームを更新する
							UnpauseFadeList( channels[ i ] ) ;
							
							channels[ i ].Unpause() ;	// 再開
						}
					}
				}
			}
		
			return true ;
		}

		/// <summary>
		/// 全オーディオチャンネルを完全にクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearAll()
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.ClearAll_Private() ;
		}
		
		// 全オーディオチャンネルを完全にクリアする
		private bool ClearAll_Private()
		{
			StopAll_Private( null ) ;
		
			int i, l = channels.Count ;
			for( i  = l - 1 ; i >= 0 ; i -- )
			{
				if( channels[ i ] != null )
				{
					channels[ i ].Destroy() ;
				}
			}
		
			m_FadeStopList.Clear() ;
		
			m_FadePlayList.Clear() ;
		
			m_MuteList.Clear() ;
		
			channels.Clear() ;
		
			return true ;
		}
	
		// --------------------------------------------------------
	
		/// <summary>
		/// 全体共通のミュート状態を設定する
		/// </summary>
		/// <param name="tState">全体共通のミュート状態(true=オン・false=オフ)</param>
		public static void Mute( bool tState )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			m_Instance.m_Mute = tState ;
		
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.masterVolume ;
			}
			else
			{
				AudioListener.volume = 0 ;
			}
		}
	
		/// <summary>
		/// マスターボリュームを設定する
		/// </summary>
		/// <param name="tVolume">マスターボリューム(0～1)</param>
		public static void SetMasterVolume( float tVolume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( tVolume >  1 )
			{
				tVolume  = 1 ;
			}
			else
			if( tVolume <  0 )
			{
				tVolume  = 0 ;
			}
		
			m_Instance.masterVolume = tVolume ;
		
			if( m_Instance.m_Mute == false )
			{
				AudioListener.volume = m_Instance.masterVolume ;
			}
		}
	
		/// <summary>
		/// 特定チャンネルのボリュームを設定する
		/// </summary>
		/// <param name="tVolume">マスターボリューム(0～1)</param>
		public static void SetVolume( int tPlayId, float tVolume )
		{
			if( m_Instance == null )
			{
				return ;
			}
		
			if( tVolume >  1 )
			{
				tVolume  = 1 ;
			}
			else
			if( tVolume <  0 )
			{
				tVolume  = 0 ;
			}
			
			AudioChannel tAudioChannel = m_Instance.GetChannelByPlayId( tPlayId ) ;
			if( tAudioChannel == null )
			{
				// 失敗(元々存在しないか既に停止している)
				return ;
			}

			tAudioChannel.volume = tVolume ;
		}

		/// <summary>
		/// 全オーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float tVolume )
		{
			return SetAllVolumes( tVolume, null ) ;
		}

		/// <summary>
		/// タグで指定されたオーディオチャンネルのボリュームを設定する
		/// </summary>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tTag">タグ名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SetAllVolumes( float tVolume, string tTag )
		{
			if( m_Instance == null )
			{
				return false ;
			}
		
			return m_Instance.SetAllVolumes_Private( tVolume, tTag ) ;
		}
		
		// タグで指定されたオーディオチャンネルのボリュームを設定する
		private bool SetAllVolumes_Private( float tVolume, string tTag )
		{
			int i, l = channels.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( channels[ i ] != null )
				{
					if( string.IsNullOrEmpty( tTag ) == true || ( string.IsNullOrEmpty( tTag ) == false && channels[ i ].tag == tTag ) )
					{
						// ポーズ状態になるのでベースタイムとベースボリュームを更新する
						channels[ i ].volume = tVolume ;	// 一時停止
					}
				}
			}
		
			return true ;
		}
	
		//---------------------------------------------------------
		
		// フェードを処理する
		private void Process()
		{
			// 現在の時間を取得する
			float tCurrentTime = Time.realtimeSinceStartup ;
		
			//-----------------------------------------------------
		
			// プレイフェードを処理する
		
			if( m_FadePlayList.Count >  0 )
			{
				int i, l = m_FadePlayList.Count ;
				AudioChannel[] tAudioChannelList = new AudioChannel[ l ] ;
			
				m_FadePlayList.Keys.CopyTo( tAudioChannelList, 0 ) ;
			
				AudioChannel tAudioChannel ;
				float tDeltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					tAudioChannel = tAudioChannelList[ i ] ;
				
					if( tAudioChannel.IsPlaying() == true )
					{
						// 再生中のみ処理する
					
						tDeltaTime = tCurrentTime - m_FadePlayList[ tAudioChannel ].baseTime ;
						if( tDeltaTime <  m_FadePlayList[ tAudioChannel ].duration )
						{
							// まだ続く
							tAudioChannel.volume = m_FadePlayList[ tAudioChannel ].baseVolume + ( ( m_FadePlayList[ tAudioChannel ].volume - m_FadePlayList[ tAudioChannel ].baseVolume ) * tDeltaTime / m_FadePlayList[ tAudioChannel ].duration ) ;
						}
						else
						{
							// 終了
							tAudioChannel.volume = m_FadePlayList[ tAudioChannel ].volume ;
						
							// リストから除外する
							m_FadePlayList.Remove( tAudioChannel ) ;
						
				//			Debug.Log( "フェードプレイが終わったので破棄:" + mFadePlayList.Count ) ;
						}
					}
				}
			}
		
			//-----------------------------
		
			// ストップフェードを処理する
		
			if( m_FadeStopList.Count >  0 )
			{
				int i, l = m_FadeStopList.Count ;
				AudioChannel[] tAudioChannelList = new AudioChannel[ l ] ;
			
				m_FadeStopList.Keys.CopyTo( tAudioChannelList, 0 ) ;
			
				AudioChannel tAudioChannel ;
				float tDeltaTime ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					tAudioChannel = tAudioChannelList[ i ] ;
				
					if( tAudioChannel.IsPlaying() == true )
					{
						// 再生中のみ処理する
					
						tDeltaTime = tCurrentTime - m_FadeStopList[ tAudioChannel ].baseTime ;
						if( tDeltaTime <  m_FadeStopList[ tAudioChannel ].duration )
						{
							// まだ続く
							tAudioChannel.volume = m_FadeStopList[ tAudioChannel ].baseVolume + ( ( m_FadeStopList[ tAudioChannel ].volume - m_FadeStopList[ tAudioChannel ].baseVolume ) * tDeltaTime / m_FadeStopList[ tAudioChannel ].duration ) ;
						}
						else
						{
							// 終了
						
							// 再生も停止する
							tAudioChannel.Stop() ;
						
							tAudioChannel.volume = m_FadeStopList[ tAudioChannel ].volume ;
						
							// リストから除外する
							m_FadeStopList.Remove( tAudioChannel ) ;
						
				//			Debug.Log( "フェードストップが終わったので破棄:" + mFadeStopList.Count ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 終了したチャンネルを監視してコールバックを発生させる
			if( onStopped != null )
			{
				AudioChannel tAudioChannel ;
				int i, l = channels.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( channels[ i ] != null )
					{
						tAudioChannel = channels[ i ] ;
						if( tAudioChannel.callback == true && tAudioChannel.IsPlaying() == false && tAudioChannel.IsPausing() == false )
						{
							onStopped( tAudioChannel.playId ) ;
							tAudioChannel.callback = false ;
						}
					}
				}
			}
		}
	
		// ポーズ状態になるのでベースタイムとベースボリュームを更新する
		private void PauseFadeList( AudioChannel tAudioChannel )
		{
			float tCurrentTime = Time.realtimeSinceStartup ;
			float tDeltaTime ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( tAudioChannel ) == true )
			{
				// 対象のソース
				tDeltaTime = tCurrentTime - m_FadePlayList[ tAudioChannel ].baseTime ;
			
				if( tDeltaTime <  m_FadePlayList[ tAudioChannel ].duration )
				{
					// まだ続く
					m_FadePlayList[ tAudioChannel ].duration   = m_FadePlayList[ tAudioChannel ].duration - tDeltaTime ;
				
					m_FadePlayList[ tAudioChannel ].baseTime   = tCurrentTime ;
					m_FadePlayList[ tAudioChannel ].baseVolume = tAudioChannel.volume ;
				}
				else
				{
					// 終了
					tAudioChannel.volume = m_FadePlayList[ tAudioChannel ].volume ;	// 最終的なボリューム
					m_FadePlayList.Remove( tAudioChannel ) ;	// リストから破棄
				}
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( tAudioChannel ) == true )
			{
				// 対象のソース
				tDeltaTime = tCurrentTime - m_FadeStopList[ tAudioChannel ].baseTime ;
			
				if( tDeltaTime <  m_FadeStopList[ tAudioChannel ].duration )
				{
					// まだ続く
					m_FadeStopList[ tAudioChannel ].duration   = m_FadeStopList[ tAudioChannel ].duration - tDeltaTime ;
				
					m_FadeStopList[ tAudioChannel ].baseTime   = tCurrentTime ;
					m_FadeStopList[ tAudioChannel ].baseVolume = tAudioChannel.volume ;
				}
				else
				{
					// 終了
					tAudioChannel.volume = m_FadeStopList[ tAudioChannel ].volume ;	// 最終的なボリューム（ストップの場合は０）
				
					m_FadeStopList.Remove( tAudioChannel ) ;
				
					// ここに来る事はあまり考えられないが念のため
					tAudioChannel.Stop() ;
				}
			}
		}
	
		// リプレイ状態になるのでベースタイムとベースボリュームを更新する
		private void UnpauseFadeList( AudioChannel tAudioChannel )
		{
			float tCurrentTime = Time.realtimeSinceStartup ;
		
			// プレイリスト
			if( m_FadePlayList.ContainsKey( tAudioChannel ) == true )
			{
				// 対象のソース
				m_FadePlayList[ tAudioChannel ].baseTime   = tCurrentTime ;
			}
		
			// ストップリスト
			if( m_FadeStopList.ContainsKey( tAudioChannel ) == true )
			{
				// 対象のソース
				m_FadeStopList[ tAudioChannel ].baseTime   = tCurrentTime ;
			}
		}
		
		// フェードリストから除外する
		private void RemoveFadeList( AudioChannel tAudioChannel )
		{
			if( m_FadePlayList.ContainsKey( tAudioChannel ) == true )
			{
				m_FadePlayList.Remove( tAudioChannel ) ;
			}
						
			if( m_FadeStopList.ContainsKey( tAudioChannel ) == true )
			{
				m_FadeStopList.Remove( tAudioChannel ) ;
			}
		}
	}

	//--------------------------------------------------------------------------------------------

	// AudioChannel

	/// <summary>
	/// オーディオチャンネルクラス(オーディオソースの機能拡張クラス)
	/// </summary>
	[Serializable]
	public class AudioChannel
	{
		/// <summary>
		/// オーディオクリップの名称
		/// </summary>
		public string name ;
		
		/// <summary>
		/// 発音中の音のユニークな識別子
		/// </summary>
		public int playId ;

		/// <summary>
		/// 処理中かどうか(フェードアウトの際に使用中にする)
		/// </summary>
		public bool busy ;
		
		/// <summary>
		/// 処理保留中かどうか
		/// </summary>
		public bool sleep ;
	
		/// <summary>
		/// タグ名
		/// </summary>
		public string tag ;	// タグ
	
		//---------------------------------
		
		// オーディオソースのインスタンス
		private AudioSource m_AudioSource ;
		
		/// <summary>
		/// ポーズ状態
		/// </summary>
		public bool pause ;
		
		/// <summary>
		/// このチャンネル再生終了時にコールバックを呼び出すかどうか
		/// </summary>
		internal protected bool callback = true ;

		//---------------------------------------------------------
	
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="tAudioSource">オーディオソースのインスタンス</param>
		internal protected AudioChannel( AudioSource tAudioSource )
		{
			m_AudioSource = tAudioSource ;
		}
		
		/// <summary>
		/// オーディオチャンネルの破棄を行う
		/// </summary>
		internal protected void Destroy()
		{
			if( m_AudioSource != null )
			{
				UnityEngine.GameObject.Destroy( m_AudioSource ) ;
				m_AudioSource = null ;
			}
		}
	
		/// <summary>
		/// オーディオクリップ
		/// </summary>
		internal protected AudioClip clip
		{
			get
			{
				if( m_AudioSource == null )
				{
					return null ;
				}
			
				return m_AudioSource.clip ;
			}
		}

		/// <summary>
		/// ミュート状態
		/// </summary>
		internal protected bool mute
		{
			get
			{
				if( m_AudioSource == null )
				{
					return false ;
				}
			
				return m_AudioSource.mute ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}
			
				m_AudioSource.mute = mute ;
			}
		}
	
		/// <summary>
		/// ボリューム(0～1)
		/// </summary>
		internal protected float volume
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.volume ;
			}
			set
			{
				if( m_AudioSource == null )
				{
					return ;
				}
			
				m_AudioSource.volume = value ;
			}
		}

		/// <summary>
		/// 再生位置(秒)
		/// </summary>
		internal protected float time
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.time ;
			}
		}

		/// <summary>
		/// 再生位置(サンプル)
		/// </summary>
		internal protected int timeSamples
		{
			get
			{
				if( m_AudioSource == null )
				{
					return 0 ;
				}
			
				return m_AudioSource.timeSamples ;
			}
		}

		//-----------------------------------------------------------
	
		/// <summary>
		/// サウンドを再生する
		/// </summary>
		/// <param name="tPlayId">発音毎に割り当てられるユニークな識別子</param>
		/// <param name="tAudioClip">オーディオクリップのインスタンス</param>
		/// <param name="tLoop">ループの有無(true=する・false=しない)</param>
		/// <param name="tVolume">ボリューム(0～1)</param>
		/// <param name="tPan">パン(-1=左～0=中～+1=右)</param>
		/// <param name="tPitch">ピッチ(-1=1オクターブ下～0=通常～+1=1オクターブ上)</param>
		/// <param name="tDelay">再生までの遅延時間(秒)</param>
		/// <param name="tTag">タグ名</param>
		internal protected void Play( int tPlayId, AudioClip tAudioClip, bool tLoop, float tVolume, float tPan, float tPitch, float tDelay, string tTag )
		{
			if( m_AudioSource == null || tAudioClip == null )
			{
				return ;
			}

			playId = tPlayId ;

			name = tAudioClip.name ;
			tag = tTag ;

			m_AudioSource.spatialBlend = 0 ;	// 2D

			m_AudioSource.pitch		= Mathf.Pow( 2.0f, tPitch ) ;
		
			m_AudioSource.loop		= tLoop ;

			m_AudioSource.clip		= null ;

			m_AudioSource.clip		= tAudioClip ;

			if( tDelay >  0 )
			{
				m_AudioSource.PlayDelayed( tDelay ) ;
			}
			else
			{
				m_AudioSource.Play() ;
			}

			if( tPan >   1 )
			{
				tPan  =  1 ;
			}
			else
			if( tPan <  -1 )
			{
				tPan  = -1 ;
			}
		
			// ボリューム
			m_AudioSource.volume	= tVolume ;

			// パン
			m_AudioSource.panStereo = tPan ;

	//		mAudioSource.bypassEffects = true ;
	//		mAudioSource.bypassListenerEffects = true ;
		
			//-----------------------------------------------------
		
			pause = false ;

			callback = true ;
		}
	
		/// <summary>
		/// サウンドを完全停止する
		/// </summary>
		internal protected void Stop()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
			
			m_AudioSource.Stop() ;
			
			if( m_AudioSource.clip != null )
			{
				// 他のチャンネルからの参照があるかもしれないので破棄はしない
//				Resources.UnloadAsset( m_AudioSource.clip ) ;	// アセットバンドルから展開したリソースの場合は再度同じリソースを異なるインスタンスで展開しようとすると以前のインスタンスが誤動作を引き起こす事があるので以前のインスタンスは完全にシステムリソースキャッシュから破棄する必要がある。
				m_AudioSource.clip = null ; // 参照を完全になくすため(これをしないと AudioClip がアセットバンドルから生成されたものの際にバグるので超重要)
			}

			// ストップでボーズもビジィも解除される
			pause	= false ;
			busy	= false ;
		
			tag = null ;
		}
	
		/// <summary>
		/// サウンドを一時停止する
		/// </summary>
		internal protected void Pause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
		
			m_AudioSource.Pause() ;
		
			pause	= true ;
		}
	
		/// <summary>
		/// サウンドの一時停止を解除する
		/// </summary>
		internal protected void Unpause()
		{
			if( m_AudioSource == null )
			{
				return ;
			}
		
			m_AudioSource.Play() ;
		
			pause	= false ;
		}
	
		/// <summary>
		/// 実際に再生中かの判定を行う（ポーズ中は true とみなされる）
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsRunning()
		{
			if( m_AudioSource == null )
			{
				return false ;
			}
			
			if( m_AudioSource.isPlaying == false && pause == false )
			{
				return false ;
			}

			return true ;
		}
		
		/// <summary>
		///  実際に再生中かどうか（ポーズ中は true とみなされる）
		/// </summary>
		internal protected bool isRunning
		{
			get
			{
				return IsRunning() ;
			}
		}

		/// <summary>
		/// 実際に再生中かの判定を行う（ポーズ中は false とみなされる）
		/// </summary>
		/// <returns>結果(true=再生中・false=再生中ではない)</returns>
		internal protected bool IsPlaying()
		{
			if( m_AudioSource == null )
			{
				return false ;
			}
			
			return m_AudioSource.isPlaying ;
		}
		
		/// <summary>
		///  実際に再生中かどうか（ポーズ中は false とみなされる）
		/// </summary>
		internal protected bool isPlaying
		{
			get
			{
				return IsPlaying() ;
			}
		}

		/// <summary>
		/// 一時停止中か判定を行う
		/// </summary>
		/// <returns>結果(true=一時停止中・false=一時停止中ではない)</returns>
		internal protected bool IsPausing()
		{
			return pause ;
		}

		/// <summary>
		/// 一時停止中かどうか
		/// </summary>
		internal protected bool isPausing
		{
			get
			{
				return IsPausing() ;
			}
		}
	}
}

