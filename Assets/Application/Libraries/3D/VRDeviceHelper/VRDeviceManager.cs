using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.XR ;
using UnityEngine.EventSystems ;
using UnityEngine.SceneManagement ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


using uGUIHelper ;
using AudioHelper ;
using TransformHelper ;

namespace VRDeviceHelper
{
	public class VRDeviceManager : MonoBehaviour
	{
#if UNITY_EDITOR
		/// <summary>
		/// AudioManager を生成
		/// </summary>
//		[MenuItem("AudioHelper/Create a AudioManager")]
		[MenuItem("GameObject/Helper/VRDeviceHelper/VRDeviceManager", false, 24)]
		public static void CreateVRCameraManager()
		{
			GameObject tGameObject = new GameObject( "VRDeviceManager" ) ;
		
			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;
		
			tGameObject.AddComponent<VRDeviceManager>() ;
			Selection.activeGameObject = tGameObject ;
		}
#endif

		// オーディオマネージャのインスタンス(シングルトン)
		private static VRDeviceManager m_Instance = null ; 

		/// <summary>
		/// オーディオマネージャのインスタンス
		/// </summary>
		public  static VRDeviceManager   instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		//---------------------------------------------------------
	



		private GameObject		m_VRCameraBase							= null ;
		private Camera			m_VRCamera								= null ;

		private GameObject		m_VRCameraBase_Distance					= null ;

		private GameObject		m_VRCameraBase_Source					= null ;	// 監視対象
		private Camera			m_VRCamera_Source						= null ;


		//-----------------------------------

		// ＵＩ

		private UICanvas		m_VRCameraUI_Mask						= null ;
		private UIImage			m_VRCameraUI_Mask_Fade					= null ;
		
		private UICanvas		m_VRCameraUI							= null ;

		// プログレス
		
		private UIImage			m_VRCameraUI_Progress_Fade				= null ;
		private UIImage			m_VRCameraUI_Progress					= null ;
		private Sprite[]		m_VRCameraUI_Progress_Sprite			= null ;


		[SerializeField]
		private float			m_VRCameraUI_DefaultDistance	= 5f ;      // デフォルトのカメラからの距離


		// レティクル

		private bool			m_VRCameraUI_Reticle_Visible			= false ;
		
//		[SerializeField]
		private bool			m_VRCameraUI_Reticle_UseNormal			= true ;			// レティクルがヒットした物体の法線の向きに合わせるかどうか

//		private Quaternion		m_VRCameraUI_Reticle_OriginalRotation ;	// 初期の回転
//		private Vector3			m_VRCameraUI_Reticle_OriginalScale ;	// 初期の縮尺

		// フェード

		private UIImage			m_VRCameraUI_Fade						= null ;
		private UIImage			m_VRCameraUI_Reticle					= null ;
		private UIImage			m_VRCameraUI_Reticle_ProgressFrame		= null ;
		private UIImage			m_VRCameraUI_Reticle_ProgressGauge		= null ;

		//---------------------------------------------------------------------------

		/// <summary>
		/// ＶＲカメラを取得する
		/// </summary>
		/// <returns></returns>
		public static Camera GetVRCamera()
		{
			if( m_Instance == null )
			{
				return null ;
			}
			return m_Instance.m_VRCamera ;
		}
		
		//---------------------------------------------------------
	
		/// <summary>
		/// オーディオマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="tRunInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>オーディオマネージャのインスタンス</returns>
		public static VRDeviceManager Create( Transform tParent = null, bool tIsListener = true, bool tRunInbackground = false )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
		
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( VRDeviceManager ) ) as VRDeviceManager ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "VRDeviceManager" ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}

				tGameObject.AddComponent<VRDeviceManager>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// オーディオマネージャのインスタンスを破棄する
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
	
		//---------------------------------------------------------------------------

		void Awake()
		{
			// 既に存在し重複になる場合は自身を削除する
			if( m_Instance != null )
			{
				GameObject.DestroyImmediate( gameObject ) ;
				return ;
			}
		
			VRDeviceManager tInstanceOther = GameObject.FindObjectOfType( typeof( VRDeviceManager ) ) as VRDeviceManager ;
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
//			SceneManager.sceneLoaded += OnLevelWasLoaded_Private ;	// MonoBehaviour の OnLevelWasLoaded メソッドは廃止予定

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
		
			//----------------------------------------------------------

			// 初期設定
			Initialize() ;
		}

		private void Initialize()
		{
			UITween tTween ;
			int i, l ;

			//----------------------------------

			// 土台を生成する
			m_VRCameraBase = new GameObject( "VR" ) ;

			// 原点じゃないと気持ち悪い
			m_VRCameraBase.transform.localPosition = Vector3.zero ;
			m_VRCameraBase.transform.localRotation = Quaternion.identity ;
			m_VRCameraBase.transform.localScale = Vector3.one ;

			m_VRCameraBase_Distance = new GameObject( "VR Distance" ) ;
			m_VRCameraBase_Distance.transform.localPosition = Vector3.zero ;
			m_VRCameraBase_Distance.transform.localRotation = Quaternion.identity ;
			m_VRCameraBase_Distance.transform.localScale = Vector3.one ;
			m_VRCameraBase_Distance.transform.SetParent( m_VRCameraBase.transform, false ) ;
			m_VRCameraBase_Distance.hideFlags = HideFlags.HideInHierarchy ;

			// カメラを生成する
			GameObject tCameraObject = new GameObject( "VR Camera" ) ;

			// 原点じゃないと気持ち悪い
			tCameraObject.transform.localPosition = Vector3.zero ;
			tCameraObject.transform.localRotation = Quaternion.identity ;
			tCameraObject.transform.localScale = Vector3.one ;

			tCameraObject.transform.SetParent( m_VRCameraBase.transform ) ;

			m_VRCamera = tCameraObject.AddComponent<Camera>() ;
			m_VRCamera.tag = "Untagged" ;
			m_VRCamera.stereoTargetEye = StereoTargetEyeMask.None ;

			m_VRCamera.clearFlags = CameraClearFlags.SolidColor ;
			m_VRCamera.backgroundColor = new Color( 0, 0, 0, 1 ) ;


			tCameraObject.AddComponent<FlareLayer>() ;
			AudioListener tAudioListener = tCameraObject.AddComponent<AudioListener>() ;
			AudioListener.volume = 1 ;
			tAudioListener.enabled = false ;

//			m_VRCamera.enabled = false ;

			//----------------------------------------------------------

			// ＶＲ専用のＵＩ
			SoftTransform t = m_VRCamera.gameObject.AddComponent<SoftTransform>() ;

			//----------------------------------

			m_VRCameraUI_Mask = t.AddObject<UICanvas>( "VRCameraUI_Mask" ) ;
			m_VRCameraUI_Mask.width		= 16 ;	// ここは後で変えられるようにする
			m_VRCameraUI_Mask.height	=  9 ;	// ここは後で変えられるようにする
			m_VRCameraUI_Mask.depth		= 55 ;
			m_VRCameraUI_Mask.isOverlay = true ;

			m_VRCameraUI_Mask._canvas.renderMode = RenderMode.WorldSpace ;
			m_VRCameraUI_Mask._canvas.worldCamera = null ;

			m_VRCameraUI_Mask._canvasScaler.dynamicPixelsPerUnit = 15 ;
			m_VRCameraUI_Mask._canvasScaler.referencePixelsPerUnit = 1 ;

			m_VRCameraUI_Mask._canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;

			m_VRCameraUI_Mask._w = 16 ;
			m_VRCameraUI_Mask._h =  9 ;
			m_VRCameraUI_Mask._x =  0 ;
			m_VRCameraUI_Mask._y =  0 ;
			m_VRCameraUI_Mask._z =  0 ;

			m_VRCameraUI_Mask_Fade = m_VRCameraUI_Mask.AddView<UIImage>( "Mask" ) ;
			m_VRCameraUI_Mask_Fade.SetAnchorToStretch() ;
			m_VRCameraUI_Mask_Fade.SetMargin( 0, 0, 0, 0 ) ;
			m_VRCameraUI_Mask_Fade._x = 0 ;
			m_VRCameraUI_Mask_Fade._y = 0 ;
			m_VRCameraUI_Mask_Fade._z = 0.75f ;
			m_VRCameraUI_Mask_Fade.isCanvasGroup = true ;
			m_VRCameraUI_Mask_Fade.color = new Color32(   0,   0,  0, 127 ) ;
			m_VRCameraUI_Mask_Fade.raycastTarget = true ;
			m_VRCameraUI_Mask_Fade.SetActive( false ) ;

			tTween = m_VRCameraUI_Mask_Fade.AddTween( "FadeIn" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 0 ;
			tTween.alphaTo		= 1 ;
			tTween.playOnAwake	= false ;

			tTween = m_VRCameraUI_Mask_Fade.AddTween( "FadeOut" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 1 ;
			tTween.alphaTo		= 0 ;
			tTween.playOnAwake	= false ;

			//----------------------------------

			m_VRCameraUI = t.AddObject<UICanvas>( "VRCameraUI" ) ;
			m_VRCameraUI.width	= 16 ;	// ここは後で変えられるようにする
			m_VRCameraUI.height	=  9 ;	// ここは後で変えられるようにする
			m_VRCameraUI.depth	= 1000000 ;
			m_VRCameraUI.isOverlay = true ;

			m_VRCameraUI._canvas.renderMode = RenderMode.WorldSpace ;
			m_VRCameraUI._canvas.worldCamera = null ;

			m_VRCameraUI._canvasScaler.dynamicPixelsPerUnit = 15 ;
			m_VRCameraUI._canvasScaler.referencePixelsPerUnit = 1 ;

			m_VRCameraUI._canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;

			m_VRCameraUI._w = 16 ;
			m_VRCameraUI._h =  9 ;
			m_VRCameraUI._x =  0 ;
			m_VRCameraUI._y =  0 ;
			m_VRCameraUI._z =  0 ;

//			m_VRCameraUI.SetActive( false ) ;


			//----------------------------------

			// プログレス関係

			m_VRCameraUI_Progress_Fade = m_VRCameraUI.AddView<UIImage>( "Progress Fade" ) ;
			m_VRCameraUI_Progress_Fade.SetAnchorToStretch() ;
			m_VRCameraUI_Progress_Fade.SetMargin( 0, 0, 0, 0 ) ;
			m_VRCameraUI_Progress_Fade._x = 0 ;
			m_VRCameraUI_Progress_Fade._y = 0 ;
			m_VRCameraUI_Progress_Fade._z = 0 ;
			m_VRCameraUI_Progress_Fade.isCanvasGroup = true ;
			m_VRCameraUI_Progress_Fade.color = new Color32(   0,   0,  0, 127 ) ;
			m_VRCameraUI_Progress_Fade.raycastTarget = true ;
			m_VRCameraUI_Progress_Fade.SetActive( false ) ;

			l = 8 ;

			m_VRCameraUI_Progress_Sprite = new Sprite[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_VRCameraUI_Progress_Sprite[ i ] = Resources.Load<Sprite>( "VRDeviceHelper/Progress/Progress" + i.ToString( "D2" ) ) ;
			}

			m_VRCameraUI_Progress = m_VRCameraUI_Progress_Fade.AddView<UIImage>( "Progress" ) ;
			m_VRCameraUI_Progress.SetAnchorToCenter() ;
			m_VRCameraUI_Progress.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Progress.sprite = m_VRCameraUI_Progress_Sprite[ 0 ] ;
			m_VRCameraUI_Progress._w = 0.5f ;
			m_VRCameraUI_Progress._h = 0.5f ;
			m_VRCameraUI_Progress._x = 0 ;
			m_VRCameraUI_Progress._y = 0 ;
			m_VRCameraUI_Progress._z = m_VRCameraUI_DefaultDistance ;
			m_VRCameraUI_Progress.SetScale( 1f, 1f, 1f ) ;
			m_VRCameraUI_Progress.color = new Color32( 255,   0, 191, 255 ) ;
			m_VRCameraUI_Progress.raycastTarget = false ;
			m_VRCameraUI_Progress.type = Image.Type.Simple ;

			tTween = m_VRCameraUI_Progress_Fade.AddTween( "FadeIn" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 0 ;
			tTween.alphaTo		= 1 ;
			tTween.playOnAwake	= false ;

			tTween = m_VRCameraUI_Progress_Fade.AddTween( "FadeOut" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 1 ;
			tTween.alphaTo		= 0 ;
			tTween.playOnAwake	= false ;

			//----------------------------------

			// レティクル関係

			m_VRCameraUI_Reticle = m_VRCameraUI.AddView<UIImage>( "Reticle" ) ;
			m_VRCameraUI_Reticle.SetAnchorToCenter() ;
			m_VRCameraUI_Reticle.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle.sprite = Resources.Load<Sprite>( "VRDeviceHelper/Reticle/Point" ) ;
			m_VRCameraUI_Reticle._w = 0.05f ;
			m_VRCameraUI_Reticle._h = 0.05f ;
			m_VRCameraUI_Reticle._x = 0 ;
			m_VRCameraUI_Reticle._y = 0 ;
			m_VRCameraUI_Reticle._z = m_VRCameraUI_DefaultDistance ;
			m_VRCameraUI_Reticle.SetScale( 1, 1, 1 ) ;
			m_VRCameraUI_Reticle.color = new Color32( 255,   0, 191, 255 ) ;
			m_VRCameraUI_Reticle.raycastTarget = false ;
			m_VRCameraUI_Reticle.type = Image.Type.Simple ;
			m_VRCameraUI_Reticle.SetActive( false ) ;

			m_VRCameraUI_Reticle_ProgressFrame = m_VRCameraUI_Reticle.AddView<UIImage>( "Frame" ) ;
			m_VRCameraUI_Reticle_ProgressFrame.SetAnchorToStretch() ;
			m_VRCameraUI_Reticle_ProgressFrame.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle_ProgressFrame.sprite = Resources.Load<Sprite>( "VRDeviceHelper/Reticle/Gauge" ) ;
			m_VRCameraUI_Reticle_ProgressFrame._z = 0 ;
			m_VRCameraUI_Reticle_ProgressFrame.SetScale( 1, 1, 1 ) ;
			m_VRCameraUI_Reticle_ProgressFrame.color = new Color32( 255,   0, 191,  79 ) ;
			m_VRCameraUI_Reticle_ProgressFrame.raycastTarget = false ;
			m_VRCameraUI_Reticle_ProgressFrame.type = Image.Type.Simple ;

			m_VRCameraUI_Reticle_ProgressGauge = m_VRCameraUI_Reticle_ProgressFrame.AddView<UIImage>( "Gauge" ) ;
			m_VRCameraUI_Reticle_ProgressGauge.SetAnchorToStretch() ;
			m_VRCameraUI_Reticle_ProgressGauge.SetPivot( 0.5f, 0.5f ) ;
			m_VRCameraUI_Reticle_ProgressGauge.sprite = Resources.Load<Sprite>( "VRDeviceHelper/Reticle/Gauge" ) ;
			m_VRCameraUI_Reticle_ProgressGauge._z = 0 ;
			m_VRCameraUI_Reticle_ProgressGauge.SetScale( 1, 1, 1 ) ;
			m_VRCameraUI_Reticle_ProgressGauge.color = new Color32( 255,   0, 191, 255 ) ;
			m_VRCameraUI_Reticle_ProgressGauge.raycastTarget = false ;
			m_VRCameraUI_Reticle_ProgressGauge.type = Image.Type.Filled ;
			m_VRCameraUI_Reticle_ProgressGauge.fillMethod = Image.FillMethod.Radial360 ;
			m_VRCameraUI_Reticle_ProgressGauge.fillOrigin = UIImage.FillOrigin.Bottom ;
			m_VRCameraUI_Reticle_ProgressGauge.fillAmount = 0 ;
			m_VRCameraUI_Reticle_ProgressGauge.fillClockwise = true ;
			m_VRCameraUI_Reticle_ProgressGauge.preserveAspect = false ;

//			m_VRCameraUI_Reticle_OriginalRotation	= m_VRCameraUI_Reticle.transform.localRotation ;
//			m_VRCameraUI_Reticle_OriginalScale		= m_VRCameraUI_Reticle.localScale ;

			//----------------------------------

			// フェード関係(キャンバス内の描画優先順位はWorldSpaceであろうともＺ値は関係しないようなのでレティクルの後ら生成配置する)

			m_VRCameraUI_Fade = m_VRCameraUI.AddView<UIImage>( "Fade" ) ;
			m_VRCameraUI_Fade.SetAnchorToStretch() ;
			m_VRCameraUI_Fade.SetMargin( 0, 0, 0, 0 ) ;
			m_VRCameraUI_Fade._x = 0 ;
			m_VRCameraUI_Fade._y = 0 ;
			m_VRCameraUI_Fade._z = 0.75f ;
			m_VRCameraUI_Fade.isCanvasGroup = true ;
			m_VRCameraUI_Fade.color = new Color32(   0,   0,  0, 255 ) ;
			m_VRCameraUI_Fade.raycastTarget = true ;
			m_VRCameraUI_Fade.SetActive( false ) ;

			tTween = m_VRCameraUI_Fade.AddTween( "FadeIn" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 1 ;
			tTween.alphaTo		= 0 ;
			tTween.playOnAwake	= false ;

			tTween = m_VRCameraUI_Fade.AddTween( "FadeOut" ) ;
			tTween.delay		= 0 ;
			tTween.duration		= 0.5f ;
			tTween.alphaEnabled	= true ;
			tTween.alphaFrom	= 0 ;
			tTween.alphaTo		= 1 ;
			tTween.playOnAwake	= false ;

			//----------------------------------------------------------

			// レイヤーをＵＩにする
			SetLayer( m_VRCameraUI_Mask.gameObject, 5 ) ;
			SetLayer( m_VRCameraUI.gameObject, 5 ) ;

			// 常駐
			DontDestroyOnLoad( m_VRCameraBase ) ;
		}

		// レイヤーを設定する
		private void SetLayer( GameObject tGameObject, int tLayer )
		{
			tGameObject.layer = tLayer ;

			int i, l = tGameObject.transform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				SetLayer( tGameObject.transform.GetChild( i ).gameObject, tLayer ) ;
			}
		}


		void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_VRCamera_Source		= null ;
				m_VRCameraBase_Source	= null ;

				// 生成したオブジェクトを破棄する
				m_VRCamera = null ;

				if( m_VRCameraBase != null )
				{
					DestroyImmediate( m_VRCameraBase ) ;
					m_VRCameraBase = null ;
				}
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// シーンのＶＲ用カメラに同期させる
		/// </summary>
		/// <param name="tVRCameraBase"></param>
		/// <returns></returns>
		public static bool SetMainCamera( Camera tMainCamera )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetMainCamera_Private( tMainCamera ) ;
		}

		private bool SetMainCamera_Private( Camera tMainCamera )
		{
			if( tMainCamera == null )
			{
				return false ;
			}

			m_VRCamera_Source = tMainCamera ;

			//----------------------------------
			// 有効なカメラが見つかったのでいくつかの設定をコピーする


			//----------------------------------

			// このカメラは無効化する
			m_VRCamera_Source.gameObject.SetActive( false ) ;
			m_VRCamera_Source.tag = "Untagged" ;
			m_VRCamera_Source.stereoTargetEye = StereoTargetEyeMask.None ;

			m_VRCamera.tag = "MainCamera" ;
			m_VRCamera.stereoTargetEye = StereoTargetEyeMask.Both ;
//			m_VRCamera.gameObject.SetActive( true ) ;
			AudioListener tAudioListener = m_VRCamera.gameObject.GetComponent<AudioListener>() ;
			if( tAudioListener != null )
			{
				if( AudioManager.instance != null )
				{
					AudioManager.listenerEnabled = false ;
				}

				tAudioListener.enabled = true ;
			}

			m_VRCamera.transform.localPosition	= tMainCamera.transform.localPosition ;
			m_VRCamera.transform.localRotation	= tMainCamera.transform.localRotation ;
			m_VRCamera.transform.localScale		= tMainCamera.transform.localScale ;

			ReplaceCamera( m_VRCamera_Source, m_VRCamera ) ;

			// 土台部分の状態をコピーする
			if( tMainCamera.transform.parent != null )
			{
				m_VRCameraBase_Source = tMainCamera.transform.parent.gameObject ;
			}
			else
			{
				m_VRCameraBase_Source = null ;
			}

			ReplaceTransform( m_VRCameraBase_Source, m_VRCameraBase ) ;

			return true ;
		}

		//---------------------------------------------------------------------------

		void Update()
		{
		}

		void LateUpdate()
		{
			float tDistance ;

			// フレームの最後にカメラの状態を同期させる
			if( m_VRCameraBase != null && m_VRCameraBase_Source != null )
			{
				ReplaceTransform( m_VRCameraBase_Source, m_VRCameraBase ) ;
			}

			if( m_VRCamera != null && m_VRCamera_Source != null )
			{
				ReplaceCamera( m_VRCamera_Source, m_VRCamera ) ;
			}

			//---------------------------------------------------------

			if( m_VRCameraBase != null && m_VRCamera != null )
			{
//				Debug.LogWarning( "カメラの向いている方向:" + m_VRCamera.transform.forward ) ;

				if( m_AdjustCanvas != null && m_AdjustCanvas.Count >  0 )
				{
					int i, l = m_AdjustCanvas.Count ;

					UICanvas	tCanvas ;
					float		tFieldOfView = 60 ;
					float		tScale ;
					int			tLost = 0 ;

					if( m_VRCamera_Source != null )
					{
						// タイミングによってはまだ m_VRCamera_Source が設定されていない事がありえる
						tFieldOfView = m_VRCamera_Source.fieldOfView ;
					}

					for( i  = 0 ; i <  l ; i ++ )
					{
						tCanvas			= m_AdjustCanvas[ i ] ;
						if( tCanvas != null )
						{
							if( XRSettings.enabled == true )
							{
								// ＶＲ有効
								tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;

								float tAngle = Mathf.PI * tFieldOfView / 360.0f ;

								// ベースに追従する
								tDistance = tCanvas.vrDistance ;

								// 計算誤差が出るので位置算出用のオブジェクトを使って座標を取得する
								m_VRCameraBase_Distance.transform.localPosition = new Vector3( 0, 0, tDistance ) ;
								tCanvas.transform.position = m_VRCameraBase_Distance.transform.position ;

								tCanvas.transform.rotation = m_VRCameraBase.transform.rotation ;

								tScale = ( tCanvas.vrDistance * Mathf.Tan( tAngle ) / ( tCanvas.height * 0.5f ) ) * tCanvas.vrScale ;
								tCanvas.transform.localScale = new Vector3( tScale, tScale, tScale ) ;	// 回転を考慮してＺもスケーリングすること
							}
							else
							{
								// ＶＲ無効
//								tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceCamera ) ;
								tCanvas.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceOverlay ) ;
							}
						}
						else
						{
							m_AdjustCanvas[ i ] = null ;
							tLost ++ ;
						}
					}

					// キャンバスが消失したものは除外する(行儀が悪いので本来は明示的に除去すべき)
					if( l >  0 && tLost >  0 )
					{
						List<UICanvas> tAdjustCanvas = new List<UICanvas>() ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( m_AdjustCanvas[ i ] != null )
							{
								tAdjustCanvas.Add( m_AdjustCanvas[ i ] ) ;
							}
						}
						m_AdjustCanvas = tAdjustCanvas ;
					}
				}

				//---------------------------------------------------------

				GameObject tUI = null ;

				if( XRSettings.enabled == true )
				{
					// ＶＲモード
//					m_VRCamera.enabled = true ;

					m_VRCameraUI_Mask.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;
					m_VRCameraUI_Mask.SetScale( 1 ) ;	// ScreenSpaceOverlay になった際にスケールが変化しているので元に戻す必要がある
					m_VRCameraUI.SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;
					m_VRCameraUI.SetScale( 1 ) ;		// ScreenSpaceOverlay になった際にスケールが変化しているので元に戻す必要がある

					float tNearClipPlane = m_VRCamera.nearClipPlane ;
					tNearClipPlane = tNearClipPlane + tNearClipPlane * 0.1f ;

					if( m_VRCameraUI_Fade._z <  tNearClipPlane )
					{
						m_VRCameraUI_Fade._z  = tNearClipPlane ;
					}

					if( m_VRCameraUI_Progress_Fade._z <  tNearClipPlane )
					{
						m_VRCameraUI_Progress_Fade._z  = tNearClipPlane ;
					}

					//--------------------------------

				}
				else
				{
//					m_VRCamera.enabled = false ;

					m_VRCameraUI_Mask.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceOverlay ) ;
					m_VRCameraUI.SetRenderMode( UICanvas.AutomaticRenderMode.ScreenSpaceOverlay ) ;

//					m_VRCameraUI_Reticle.SetActive( false ) ;
				}

				//---------------------------------------------------------

				// レティクルを処理する

				bool tProgress = false ;
				if( m_VRCameraUI_Progress_Fade != null && m_VRCameraUI_Progress_Fade.ActiveSelf == true )
				{
					// レティクルはプログレスに当たってもチラチラしてしまうのでプログレス表示中も消す
					tProgress = true ;
				}

				if( m_VRCameraUI_Reticle_Visible == true && m_FadeShowing == false && m_FadePlaying == false && tProgress == false )
				{
					// フェード中はレイがフェードに当たってレティクルの位置が飛びまくるのでレティクルの表示をしないようにする
					m_VRCameraUI_Reticle.SetActive( true ) ;
					
					if( XRSettings.enabled == true )
					{
						tUI = UIEventSystem.GetLookingObject() ;
						if( tUI == null )
						{
							Ray tRay = new Ray( m_VRCamera.transform.position, m_VRCamera.transform.forward ) ;
							RaycastHit tHit ;
			
							if( Physics.Raycast( tRay, out tHit, Mathf.Infinity ) == true )
							{
								SetReticleView( m_VRCamera, tHit ) ;
							}
							else
							{
								SetReticleView( m_VRCamera ) ;
							}
						}
						else
						{
							// ＵＩの無限平面とレイキャストの交点を求める
							Vector3 tPlane_N = - tUI.transform.forward ;
							Vector3 tPlane_P = tUI.transform.position ;

							Vector3 tRay_P = m_VRCamera.transform.position ;
							Vector3 tRay_D = m_VRCamera.transform.forward ;

							float h = Vector3.Dot( tPlane_N, tPlane_P ) ;
							Vector3 tCross_P = tRay_P + tRay_D * ( ( h - Vector3.Dot( tPlane_N, tRay_P ) ) / Vector3.Dot( tPlane_N, tRay_D ) ) ;

							tDistance = ( tCross_P - tRay_P ).magnitude ;

							SetReticleView( tCross_P, tPlane_N, tDistance ) ;
						}
					}
					else
					{
						SetReticleView( m_VRCamera, 10 ) ;
					}
				}
				else
				{
					m_VRCameraUI_Reticle.SetActive( false ) ;
				}

				//----------------------------------

				// プログレス関連

				if( m_VRCameraUI_Progress_Fade != null && m_VRCameraUI_Progress_Fade.ActiveSelf == true )
				{
					// プログレスの状態を更新する
					m_ProgressTotalTime += Time.unscaledDeltaTime ;
					int l = m_VRCameraUI_Progress_Sprite.Length ;
					int i = ( int )( m_ProgressTotalTime / m_ProgressDeltaTime ) ;
					i = i % l ;
	
					m_VRCameraUI_Progress.sprite = m_VRCameraUI_Progress_Sprite[ i ] ;

					if( XRSettings.enabled == true )
					{
						m_VRCameraUI_Progress.SetScale( 1 ) ;
					}
					else
					{
						// 距離５の時２倍(と言っても平行投影時のサイズはＺの距離には関係無くＷＨサイズにしか依存しない：たまたま距離５の時に２倍で丁度同じ見ためになるというだけ)
						m_VRCameraUI_Progress.SetScale( 2 ) ;
					}
				}
			}
		}

		private void ReplaceTransform( GameObject g1, GameObject g2 )
		{
			if( g1 == null || g2 == null )
			{
				return ;
			}

			g2.transform.position		= g1.transform.position ;
			g2.transform.rotation		= g1.transform.rotation ;
			g2.transform.localScale		= g2.transform.localScale ;
		}

		private void ReplaceCamera( Camera c1, Camera c2 )
		{
			if( XRSettings.enabled == false )
			{
				c2.fieldOfView	= c1.fieldOfView ;
			}


			c2.clearFlags		= c1.clearFlags ;
			c2.backgroundColor	= c1.backgroundColor ;

			c2.nearClipPlane	= c1.nearClipPlane ;

			c2.cullingMask		= c1.cullingMask ;
		}

		/// <summary>
		/// レティクルの位置を設定する(視線の方向に何も無いケース)
		/// </summary>
		private void SetReticleView( Camera tCamera )
		{
			float tNearClipPlane = tCamera.nearClipPlane ;
//			tNearClipPlane = tNearClipPlane + tNearClipPlane * 1 ;

			float tDistance = m_VRCameraUI_DefaultDistance ;
			if( tDistance <  tNearClipPlane )
			{
				tDistance  = tNearClipPlane ;
			}

//			Debug.LogWarning( "CP:" + tCamera.transform.position + " CF:" + tCamera.transform.forward + " D:" + tDistance ) ;
//			Debug.LogWarning( "--R:" + ( tCamera.transform.position + tCamera.transform.forward * tDistance ) ) ;
			// レティクルの位置をカメラの前方に設定する
			m_VRCameraUI_Reticle.transform.position = tCamera.transform.position + tCamera.transform.forward * tDistance ;
	
			// 回転は初期状態のものを使う
			m_VRCameraUI_Reticle.transform.localRotation	= Quaternion.identity ;

			// 縮尺は初期状態のものに距離を乗算したものにする
			m_VRCameraUI_Reticle.transform.localScale		= new Vector3( tDistance, tDistance, tDistance ) ;
		}
		
		/// <summary>
		/// レティクルの位置を設定する(視線の方向に物体が存在するケース)
		/// </summary>
		/// <param name="hit"></param>
		private void SetReticleView( Camera tCamera, RaycastHit tHit )
        {
			m_VRCameraUI_Reticle.transform.position		= tHit.point ;	// Ｚ値がおおきくなると表示も小さくなるか

			// 重要なポイントとして位置が３Ｄ系なのでキャンバススケールは１にし個々のＵＩのスケールを調整(極小)にする必要がある
			if( m_VRCameraUI_Reticle_UseNormal == true && m_VRCameraUI_Reticle_ProgressFrame.ActiveSelf == false )
			{
				// レティクルの法線方向を物体の法線方向に合わせる
				m_VRCameraUI_Reticle.transform.rotation			= Quaternion.FromToRotation( Vector3.forward, tHit.normal ) ;
			}
			else
			{
				// レティクルの法線方向を物体の法線方向に合わせない
				m_VRCameraUI_Reticle.transform.localRotation	= Quaternion.identity ;
			}

			// 最大距離より近いとレティクルが大きく表示される
			m_VRCameraUI_Reticle.transform.localScale			= new Vector3( tHit.distance, tHit.distance, tHit.distance ) ;	// 距離を掛け合わせることで遠くいっても見た目上の大きさを一定以上に保つ事ができる
		}

		/// <summary>
		/// レティクルの位置を設定する(視線の方向に何も無いケース)
		/// </summary>
		private void SetReticleView( Camera tCamera, float tScale )
		{
			float tNearClipPlane = tCamera.nearClipPlane ;
			// レティクルの位置をカメラの前方に設定する

			m_VRCameraUI_Reticle.transform.localPosition = new Vector3( 0, 0, tNearClipPlane * 2 ) ;
	
			// 回転は初期状態のものを使う
			m_VRCameraUI_Reticle.transform.localRotation	= Quaternion.identity ;

			// 縮尺は初期状態のものに距離を乗算したものにする
			m_VRCameraUI_Reticle.transform.localScale		= new Vector3( tScale, tScale, tScale ) ;
		}


		/// <summary>
		/// レティクルの位置を設定する(視線の方向に物体が存在するケース)
		/// </summary>
		/// <param name="hit"></param>
		private void SetReticleView( Vector3 tPoint, Vector3 tNormal, float tDistance )
        {
			m_VRCameraUI_Reticle.transform.position		= tPoint ;	// Ｚ値がおおきくなると表示も小さくなるか

			// 重要なポイントとして位置が３Ｄ系なのでキャンバススケールは１にし個々のＵＩのスケールを調整(極小)にする必要がある
			if( m_VRCameraUI_Reticle_UseNormal == true )
			{
				// レティクルの法線方向を物体の法線方向に合わせる
				m_VRCameraUI_Reticle.transform.rotation			= Quaternion.FromToRotation( Vector3.forward, tNormal ) ;
			}
			else
			{
				// レティクルの法線方向を物体の法線方向に合わせない
				m_VRCameraUI_Reticle.transform.localRotation	= Quaternion.identity ;
			}

			// 最大距離より近いとレティクルが大きく表示される
			m_VRCameraUI_Reticle.transform.localScale			= new Vector3( tDistance, tDistance, tDistance ) ;	// 距離を掛け合わせることで遠くいっても見た目上の大きさを一定以上に保つ事ができる
		}


		//-----------------------------------------------------------

		private List<UICanvas> m_AdjustCanvas = new List<UICanvas>() ;
		
		/// <summary>
		/// キャンバスの登録を追加する
		/// </summary>
		/// <param name="tCanvas"></param>
		/// <returns></returns>
		public static bool AddAdjustCanvas( params UICanvas[] tCanvas )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.AddAdjustCanvas_Private( tCanvas ) ;
		}

		private bool AddAdjustCanvas_Private( params UICanvas[] tCanvas )
		{
			if( tCanvas == null || tCanvas.Length == 0 )
			{
				return false ;
			}

			int j, m ;

			int i, l = tCanvas.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m = m_AdjustCanvas.Count ;	// 数が変動するので毎回数を取得すること
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( m_AdjustCanvas[ j ] == tCanvas[ i ] )
					{
						// 既に登録済み
						break ;
					}
				}

				if( j >= m )
				{
					// 新規登録
					if( tCanvas[ i ] != null )
					{
						tCanvas[ i ].SetRenderMode( UICanvas.AutomaticRenderMode.WorldSpace ) ;
						m_AdjustCanvas.Add( tCanvas[ i ] ) ;
					}
				}
			}

//			tCanvas.worldCamera = m_VRCamera ;
			
			return true ;
		}

		/// <summary>
		/// キャンバスの登録を削除する
		/// </summary>
		/// <param name="tCanvas"></param>
		/// <returns></returns>
		public static bool RemoveAdjustCanvas( params UICanvas[] tCanvas )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.RemoveAdjustCanvas_Private( tCanvas ) ;
		}

		private bool RemoveAdjustCanvas_Private( params UICanvas[] tCanvas )
		{
			if( tCanvas == null || tCanvas.Length == 0 )
			{
				return false ;
			}

			int j, m ;

			int i, l = tCanvas.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m = m_AdjustCanvas.Count ;	// 数が変動するので毎回数を取得すること
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( m_AdjustCanvas[ j ] == tCanvas[ i ] )
					{
						// 既に登録済み
						break ;
					}
				}

				if( j <  m )
				{
					// 新規登録
					m_AdjustCanvas.RemoveAt( j ) ;
				}
			}

			return false ;
		}

		//---------------------------------------------------------------------------

		private Func<bool> m_DecideExecutor = null ;

		/// <summary>
		/// 決定判定用コールバックを設定する
		/// </summary>
		/// <param name="tDecideExecutor"></param>
		/// <returns></returns>
		public static bool SetDecideExecutor( Func<bool> tDecideExecutor )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_DecideExecutor = tDecideExecutor ;
			return true ;
		}

		/// <summary>
		/// 決定判定
		/// </summary>
		public static bool isDecide
		{
			get
			{
				if( m_Instance == null || m_Instance.m_DecideExecutor == null )
				{
					return false ;
				}

				return m_Instance.m_DecideExecutor() ;
			}
		}

		//-----------------------------------

		private Func<bool> m_CancelExecutor = null ;

		/// <summary>
		/// 決定判定用コールバックを設定する
		/// </summary>
		/// <param name="tDecideExecutor"></param>
		/// <returns></returns>
		public static bool SetCancelExecutor( Func<bool> tCancelExecutor )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			m_Instance.m_CancelExecutor = tCancelExecutor ;
			return true ;
		}

		/// <summary>
		/// 否定判定
		/// </summary>
		public static bool isCancel
		{
			get
			{
				if( m_Instance == null || m_Instance.m_CancelExecutor == null )
				{
					return false ;
				}

				return m_Instance.m_CancelExecutor() ;
			}
		}

		//---------------------------------------------------------------------------

		// レティクル操作関係

		/// <summary>
		/// レティクルの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticle( bool tVisible, bool tProgress = false, float tGauge = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetReticle_Private( tVisible, tProgress, tGauge ) ;
		}

		private bool SetReticle_Private( bool tVisible, bool tProgress, float tGauge )
		{
			if( m_VRCameraUI_Reticle == null || m_VRCameraUI_Reticle_ProgressFrame == null || m_VRCameraUI_Reticle_ProgressGauge == null )
			{
				return false ;
			}

			m_VRCameraUI_Reticle_Visible = tVisible ;
			if( tVisible == true && XRSettings.enabled == true )
			{
				m_VRCameraUI_Reticle.SetActive( true ) ;
			}
			else
			{
				m_VRCameraUI_Reticle.SetActive( false ) ;
			}

			m_VRCameraUI_Reticle_ProgressFrame.SetActive( tProgress ) ;
			m_VRCameraUI_Reticle_ProgressGauge.fillAmount = tGauge ;

			return true ;
		}

		/// <summary>
		/// レティクルのプログレスの表示を設定する
		/// </summary>
		/// <param name="tVisible"></param>
		/// <param name="tProgress"></param>
		/// <param name="tGauge"></param>
		/// <returns></returns>
		public static bool SetReticleProgress( bool tProgress, float tGauge = 0 )
		{
			if( m_Instance == null )
			{
				return false ;
			}

			return m_Instance.SetReticleProgress_Private( tProgress, tGauge ) ;
		}

		private bool SetReticleProgress_Private( bool tProgress, float tGauge )
		{
			if( m_VRCameraUI_Reticle == null || m_VRCameraUI_Reticle_ProgressFrame == null || m_VRCameraUI_Reticle_ProgressGauge == null )
			{
				return false ;
			}

			m_VRCameraUI_Reticle_ProgressFrame.SetActive( tProgress ) ;
			m_VRCameraUI_Reticle_ProgressGauge.fillAmount = tGauge ;

			return true ;
		}


		//---------------------------------------------------------------------------

		// フェード操作関係

		/// <summary>
		/// デフォルトの色
		/// </summary>
		public Color fadeDefaultColor = new Color( 0, 0, 0, 1 ) ;

		/// <summary>
		/// デフォルトの遅延時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float fadeDefaultDelay = 0 ;

		/// <summary>
		/// デフォルトの実行時間(秒)　※0 未満でインスタペクターに設定された値を使用する
		/// </summary>
		public float fadeDefaultDuration = -1 ;

		//-----------------------------------

		/// <summary>
		/// フェードの状態クラス
		/// </summary>
		public class Status : CustomYieldInstruction
		{
			public Status()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( isDone == false )
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
		}

		//-----------------------------------

		private bool m_FadeShowing = false ;

		/// <summary>
		/// 表示状態
		/// </summary>
		public static bool isFadeShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_FadeShowing ;
			}
		}

		// 実行状態
		private bool m_FadePlaying = false ;

		/// <summary>
		/// 実行状態
		/// </summary>
		public static bool isFadePlaying
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_FadePlaying ;
			}
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void ShowFade( uint tAARRGGBB = 0x00000000 )
		{
			Color32 tColor= new Color32
			(
				( byte )( ( tAARRGGBB >> 16 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  8 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  0 ) & 0xFF ),
				( byte )( ( tAARRGGBB >> 24 ) & 0xFF )
			) ;

			ShowFade( tColor ) ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void ShowFade( Color tColor )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.ShowFade_Private( tColor ) ;
		}

		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void ShowFade_Private( Color tColor )
		{
			if( m_VRCameraUI_Fade == null )
			{
				return ;
			}

			UITween tTween = m_VRCameraUI_Fade.GetTween( "FadeOut" ) ;
			if( tTween != null && ( tTween.isRunning == true || tTween.isPlaying == true ) )
			{
				return ;	// 放っておいても表示になるので無視する
			}

			m_VRCameraUI_Fade.StopTweenAll() ;

			if( tColor.a == 0 )
			{
				tColor = fadeDefaultColor ;
			}

			m_VRCameraUI_Fade.color = tColor ;

			m_VRCameraUI_Fade._a = 1 ;

			gameObject.SetActive( true ) ;

			m_FadeShowing = true ;
		}

		/// <summary>
		/// 単色のフェードフェクトを表示する(フェードイン前の準備)
		/// </summary>
		/// <param name="tColor">色</param>
		public static void HideFade()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.HideFade_Private() ;
		}


		// 単色のフェードフェクトを表示する(フェードイン前の準備)
		private void HideFade_Private()
		{
			if( m_VRCameraUI_Fade == null )
			{
				return ;
			}

			UITween tTween = m_VRCameraUI_Fade.GetTween( "FadeIn" ) ;
			if( tTween != null && ( tTween.isRunning == true || tTween.isPlaying == true ) )
			{
				return ;	// 放っておいても非表示になるので無視する
			}

			m_VRCameraUI_Fade.StopTweenAll() ;
			
			gameObject.SetActive( false ) ;

			m_FadeShowing = false ;
		}

		//---------------

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		public static Status FadeIn( float tDelay = 0, float tDuration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Status tStatus = new Status() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.FadeIn_Private( tDelay, tDuration, tStatus ) ) ;

			return tStatus ;
		}

		/// <summary>
		/// フェードインを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator FadeIn_Private( float tDelay, float tDuration, Status tStatus )
		{
			if( m_VRCameraUI_Fade == null )
			{
				tStatus.isDone = true ;
				yield break ;
			}

			m_FadePlaying = true ;

			m_VRCameraUI_Fade.color = fadeDefaultColor ;

			if( tDelay <  0 )
			{
				tDelay  = fadeDefaultDelay ;
			}

			if( tDuration <  0 )
			{
				tDuration = fadeDefaultDuration ;
			}

			m_VRCameraUI_Fade.PlayTweenDirect( "FadeIn", tDelay, tDuration ) ;

			yield return new WaitWhile( () => m_VRCameraUI_Fade.isAnyTweenPlaying == true ) ;

//			gameObject.SetActive( false ) ;
			m_VRCameraUI_Fade.SetActive( false ) ;

			m_FadePlaying = false ;

			tStatus.isDone = true ;

			m_FadeShowing = false ;
		}

		//---------------

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static Status FadeOut( float tDelay = 0, float tDuration = -1 )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Status tStatus = new Status() ;

			m_Instance.gameObject.SetActive( true ) ;
			m_Instance.StartCoroutine( m_Instance.FadeOut_Private( tDelay, tDuration, tStatus ) ) ;

			return tStatus ;
		}

		/// <summary>
		/// フェードアウトを実行する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		/// <param name="tDuration">実行時間(秒)</param>
		/// <returns>列挙子</returns>
		private IEnumerator FadeOut_Private( float tDelay, float tDuration, Status tStatus )
		{
			if( m_VRCameraUI_Fade == null )
			{
				tStatus.isDone = true ;
				yield break ;
			}

			m_FadePlaying = true ;

			m_VRCameraUI_Fade.color = fadeDefaultColor ;

			if( tDelay <  0 )
			{
				tDelay  = fadeDefaultDelay ;
			}

			if( tDuration <  0 )
			{
				tDuration = fadeDefaultDuration ;
			}

			m_VRCameraUI_Fade.PlayTweenDirect( "FadeOut", tDelay, tDuration ) ;

			yield return new WaitWhile( () => m_VRCameraUI_Fade.isAnyTweenPlaying == true ) ;

			m_FadePlaying = false ;

			tStatus.isDone = true ;

			m_FadeShowing = true ;
		}
		
		//---------------

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tAARRGGBB">色値(ＡＡＲＲＧＧＢＢ)</param>
		public static void SetFadeDefaultColor( uint tAARRGGBB )
		{
			Color32 tColor = new Color32
			(
				( byte )( ( tAARRGGBB >> 16 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  8 ) & 0xFF ),
				( byte )( ( tAARRGGBB >>  0 ) & 0xFF ),
				( byte )( ( tAARRGGBB >> 24 ) & 0xFF )
			) ;

			SetFadeDefaultColor( tColor ) ;
		}

		/// <summary>
		/// フェードのデフォルトの色を設定する
		/// </summary>
		/// <param name="tColor">色</param>
		public static void SetFadeDefaultColor( Color tColor )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultColor = tColor ;
		}

		/// <summary>
		/// フェードのデフォルトの遅延時間を設定する
		/// </summary>
		/// <param name="tDelay">遅延時間(秒)</param>
		public static void SetFadeDefaultDelay( float tDelay )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultDelay = tDelay ;
		}

		/// <summary>
		/// フェードのデフォルトの実行時間を設定する
		/// </summary>
		/// <param name="tDuration">実行時間(秒)</param>
		public static void SetFadeDefaultDuration( float tDuration )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.fadeDefaultDuration = tDuration ;
		}

		//---------------------------------------------------------------------------

		// プログレス操作関係

		private float m_ProgressTotalTime = 0 ;
		private float m_ProgressDeltaTime = 0.1f ;
		
		// プログレス継続中のフラグ
		private bool m_ProgressOn = false ;

		/// <summary>
		/// プログレス継続中のフラグ
		/// </summary>
		public static bool isProgressOn
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_ProgressOn ;
			}

			set
			{
				if( m_Instance == null )
				{
					return ;
				}

				m_Instance.m_ProgressOn = value ;
			}
		}

		// プログレスを消去中かどうか(コルーチンを使用しているので連続実行を抑制する必要がある)
		private IEnumerator m_ProgressHidingCoroutine = null ;

		/// <summary>
		/// プログレスを表示中かどうか
		/// </summary>
		/// <returns></returns>
		public static bool isProgressShowing
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				if( m_Instance.m_VRCameraUI_Progress_Fade.isAnyTweenPlaying == true || m_Instance.m_ProgressHidingCoroutine != null )
				{
					return true ;	// 表示中
				}

				return false ;
			}
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		public static bool ShowProgress()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_VRCameraUI_Progress_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.ShowProgress_Private() ;

			return true ;
		}

		// プログレスを表示する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void ShowProgress_Private()
		{
			UITween tTween ;

			if( m_ProgressHidingCoroutine != null )
			{
				StopCoroutine( m_ProgressHidingCoroutine ) ;
				m_ProgressHidingCoroutine = null ;

				if( m_VRCameraUI_Progress_Fade.ActiveSelf == true )
				{
					tTween = m_VRCameraUI_Progress_Fade.GetTween( "FadeOut" ) ;
					if( tTween != null && ( tTween.isRunning == true || tTween.isPlaying == true ) )
					{
						// 非表示中なので終了させる
						tTween.Finish() ;
					}
				}
			}

			if( m_VRCameraUI_Progress_Fade.ActiveSelf == false )
			{
				m_VRCameraUI_Progress_Fade.SetActive( true ) ;
			}

			tTween = m_VRCameraUI_Progress_Fade.GetTween( "FadeIn" ) ;
			if( tTween != null && tTween.isPlaying == true )
			{
				return ;	// 既に表示最中
			}

			// 改めてフェードイン再生
			m_VRCameraUI_Progress_Fade.PlayTweenDirect( "FadeIn" ) ;

			m_ProgressTotalTime = 0 ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		public static bool HideProgress()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_VRCameraUI_Progress_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.HideProgress_Private() ;

			return true ;
		}

		// プログレスを消去する(実装部)　※ここに別のタイプを実装する(表示するタイプを切り替えられるようにする)
		private void HideProgress_Private()
		{
			UITween tTween ;

			if( m_VRCameraUI_Progress_Fade.ActiveSelf == false || m_ProgressHidingCoroutine != null )
			{
				// 既に非表示中
				return ;
			}
			
			tTween = m_VRCameraUI_Progress_Fade.GetTween( "FadeIn" ) ;
			if( tTween != null && ( tTween.isRunning == true || tTween.isPlaying == true ) )
			{
				// 表示中なので終了させる
				tTween.Finish() ;
			}

			m_ProgressHidingCoroutine = ProgressHidingCoroutine_Private() ;
			StartCoroutine( m_ProgressHidingCoroutine ) ;
		}

		// プログレスを消去する(実装部・コルーチン)
		private IEnumerator ProgressHidingCoroutine_Private()
		{
			m_VRCameraUI_Progress_Fade.PlayTweenDirect( "FadeOut" ) ;
			yield return new WaitWhile( () => m_Instance.m_VRCameraUI_Progress_Fade.isAnyTweenPlaying == true ) ;	// フェードアウトが完了するのを待つ

			m_VRCameraUI_Progress_Fade.SetActive( false ) ;
			m_ProgressHidingCoroutine = null ;	// 非表示終了
		}

		/// <summary>
		/// プログレスを表示する
		/// </summary>
		/// <returns></returns>
		public static bool ProgressOn()
		{
			if( m_Instance == null )
			{
				return false ;
			}

			if( m_Instance.m_VRCameraUI_Progress_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.m_ProgressOn = true ;

			m_Instance.ShowProgress_Private() ;

			return true ;
		}

		/// <summary>
		/// プログレスを消去する
		/// </summary>
		/// <returns></returns>
		public static bool ProgressOff()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			
			if( m_Instance.m_VRCameraUI_Progress_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.HideProgress_Private() ;

			m_Instance.m_ProgressOn = false ;

			return true ;
		}

		//-----------------------------------------------------------

		// ダイアログ用のマスク関係

		// 表示
		public static bool FadeInDialogMask( float tDuration = 0.5f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			
			if( m_Instance.m_VRCameraUI_Mask_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			m_Instance.m_VRCameraUI_Mask_Fade.SetActive( true ) ;
			m_Instance.m_VRCameraUI_Mask_Fade.PlayTweenDirect( "FadeIn", 0, tDuration ) ;

			return true ;
		}

		// 消去
		public static bool FadeOutDialogMask( float tDuration = 0.5f )
		{
			if( m_Instance == null )
			{
				return false ;
			}
			
			if( m_Instance.m_VRCameraUI_Mask_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			if( m_Instance.m_VRCameraUI_Mask_Fade.ActiveSelf == true )
			{
				m_Instance.m_VRCameraUI_Mask_Fade.PlayTweenDirect( "FadeOut", 0, tDuration ) ;
			}

			return true ;
		}

		// 消去
		public static bool IsDialogMaskFading()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			
			if( m_Instance.m_VRCameraUI_Mask_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			if( m_Instance.m_VRCameraUI_Mask_Fade.ActiveSelf == true )
			{
				return m_Instance.m_VRCameraUI_Mask_Fade.isAnyTweenPlaying ;
			}

			return false ;
		}

		// 消去
		public static bool HideDialogMask()
		{
			if( m_Instance == null )
			{
				return false ;
			}
			
			if( m_Instance.m_VRCameraUI_Mask_Fade == null )
			{
				return false ;
			}
			
			//----------------------------------------------------------

			if( m_Instance.m_VRCameraUI_Mask_Fade.ActiveSelf == true )
			{
				m_Instance.m_VRCameraUI_Mask_Fade.SetActive( false ) ;
			}

			return true ;
		}

		
	}
}
