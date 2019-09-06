using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace TransformHelper
{
	[ ExecuteInEditMode ]
	public class SoftTransformTween : MonoBehaviour
	{
		/// <summary>
		/// 識別名
		/// </summary>
		public string identity ;

		/// <summary>
		/// 遅延時間(秒)
		/// </summary>
		public float delay = 0 ;

		/// <summary>
		/// 実行時間(秒)
		/// </summary>
		public float duration = 1 ;

		/// <summary>
		/// 動作対象種別の定義
		/// </summary>
		public enum MotionType
		{
			Position = 0,
			Rotation = 1,
			Scale    = 2,
			alpha    = 3,
		}

		[SerializeField][HideInInspector]
		private MotionType m_MotionType = MotionType.Position ;

		/// <summary>
		/// 動作対象種別
		/// </summary>
		public  MotionType  motionType
		{
			get
			{
				return m_MotionType ;
			}
			set
			{
				if( m_MotionType != value )
				{
					m_MotionType  = value ;
					RefreshChecker() ;
				}
			}
		}
		
		/// <summary>
		/// カーブの処理方法種別の定義
		/// </summary>
		public enum ProcessType
		{
			Ease = 0,
			AnimationCurve = 1,
		}

		/// <summary>
		/// イーズの種別の定義
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
		
		//-----------------------------------------------------------

		// ポジション

		[SerializeField][HideInInspector]
		private bool m_PositionEnabled = false ;

		/// <summary>
		/// 位置の変化を有効にするかどうか
		/// </summary>
		public  bool  positionEnabled
		{
			get
			{
				return m_PositionEnabled ;
			}
			set
			{
				if( m_PositionEnabled != value )
				{
					m_PositionEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionFrom = Vector3.zero ;

		/// <summary>
		/// 開始位置
		/// </summary>
		public  Vector3  positionFrom
		{
			get
			{
				return m_PositionFrom ;
			}
			set
			{
				if( m_PositionFrom != value )
				{
					m_PositionFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始位置Ｘ
		/// </summary>
		public float positionFromX
		{
			get
			{
				return m_PositionFrom.x ;
			}
			set
			{
				positionFrom = new Vector2( value, m_PositionFrom.y ) ;
			}
		}

		/// <summary>
		/// 開始位置Ｙ
		/// </summary>
		public float positionFromY
		{
			get
			{
				return m_PositionFrom.y ;
			}
			set
			{
				positionFrom = new Vector2( m_PositionFrom.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_PositionTo   = Vector3.zero ;

		/// <summary>
		/// 終了位置
		/// </summary>
		public  Vector3  positionTo
		{
			get
			{
				return m_PositionTo ;
			}
			set
			{
				if( m_PositionTo != value )
				{
					m_PositionTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了位置Ｘ
		/// </summary>
		public float positionToX
		{
			get
			{
				return m_PositionTo.x ;
			}
			set
			{
				positionTo = new Vector2( value, m_PositionTo.y ) ;
			}
		}

		/// <summary>
		/// 終了位置Ｙ
		/// </summary>
		public float positionToY
		{
			get
			{
				return m_PositionTo.y ;
			}
			set
			{
				positionTo = new Vector2( m_PositionTo.x, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessType m_PositionProcessType = ProcessType.Ease ;

		/// <summary>
		/// 位置のカーブの処理方法種別
		/// </summary>
		public  ProcessType  positionProcessType
		{
			get
			{
				return m_PositionProcessType ;
			}
			set
			{
				if( m_PositionProcessType != value )
				{
					m_PositionProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseType m_PositionEaseType = EaseType.linear ;

		/// <summary>
		/// 位置のイーズの種別
		/// </summary>
		public  EaseType  positionEaseType
		{
			get
			{
				return m_PositionEaseType ;
			}
			set
			{
				if( m_PositionEaseType != value )
				{
					m_PositionEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_PositionAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 位置のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  positionAnimationCurve
		{
			get
			{
				return m_PositionAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_PositionAnimationCurve, value ) == true )
				{
					m_PositionAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_PositionFoldOut = true ;

		/// <summary>
		/// 位置のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  positionFoldOut
		{
			get
			{
				return m_PositionFoldOut ;
			}
			set
			{
				m_PositionFoldOut = value ;
			}
		}

		//-----------------------------------------------------------

		// ローテーション

		[SerializeField][HideInInspector]
		private bool m_RotationEnabled = false ;

		/// <summary>
		/// 角度の変化を有効にするかどうか
		/// </summary>
		public  bool  rotationEnabled
		{
			get
			{
				return m_RotationEnabled ;
			}
			set
			{
				if( m_RotationEnabled != value )
				{
					m_RotationEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationFrom = Vector3.zero ;

		/// <summary>
		/// 開始角度
		/// </summary>
		public  Vector3  rotationFrom
		{
			get
			{
				return m_RotationFrom ;
			}
			set
			{
				if( m_RotationFrom != value )
				{
					m_RotationFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始角度Ｘ
		/// </summary>
		public float rotationFromX
		{
			get
			{
				return m_RotationFrom.x ;
			}
			set
			{
				rotationFrom = new Vector3( value, m_RotationFrom.y, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｙ
		/// </summary>
		public float rotationFromY
		{
			get
			{
				return m_RotationFrom.y ;
			}
			set
			{
				rotationFrom = new Vector3( m_RotationFrom.x, value, m_RotationFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始角度Ｚ
		/// </summary>
		public float rotationFromZ
		{
			get
			{
				return m_RotationFrom.z ;
			}
			set
			{
				rotationFrom = new Vector3( m_RotationFrom.x, m_RotationFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_RotationTo   = Vector3.zero ;

		/// <summary>
		/// 終了角度
		/// </summary>
		public  Vector3  rotationTo
		{
			get
			{
				return m_RotationTo ;
			}
			set
			{
				if( m_RotationTo != value )
				{
					m_RotationTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了角度Ｘ
		/// </summary>
		public float rotationToX
		{
			get
			{
				return m_RotationTo.x ;
			}
			set
			{
				rotationTo = new Vector3( value, m_RotationTo.y, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｙ
		/// </summary>
		public float rotationToY
		{
			get
			{
				return m_RotationTo.y ;
			}
			set
			{
				rotationTo = new Vector3( m_RotationTo.x, value, m_RotationTo.z ) ;
			}
		}

		/// <summary>
		/// 終了角度Ｚ
		/// </summary>
		public float rotationToZ
		{
			get
			{
				return m_RotationTo.z ;
			}
			set
			{
				rotationFrom = new Vector3( m_RotationTo.x, m_RotationTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessType m_RotationProcessType = ProcessType.Ease ;

		/// <summary>
		/// 角度のカーブの処理種別
		/// </summary>
		public  ProcessType  rotationProcessType
		{
			get
			{
				return m_RotationProcessType ;
			}
			set
			{
				if( m_RotationProcessType != value )
				{
					m_RotationProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseType m_RotationEaseType = EaseType.linear ;

		/// <summary>
		/// 角度のイーズの種別
		/// </summary>
		public  EaseType  rotationEaseType
		{
			get
			{
				return m_RotationEaseType ;
			}
			set
			{
				if( m_RotationEaseType != value )
				{
					m_RotationEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_RotationAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 角度のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  rotationAnimationCurve
		{
			get
			{
				return m_RotationAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_RotationAnimationCurve, value ) == true )
				{
					m_RotationAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_RotationFoldOut = true ;

		/// <summary>
		/// 角度のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  rotationFoldOut
		{
			get
			{
				return m_RotationFoldOut ;
			}
			set
			{
				m_RotationFoldOut = value ;
			}
		}

		//-------------------------------------------

		// スケール

		[SerializeField][HideInInspector]
		private bool m_ScaleEnabled = false ;

		/// <summary>
		/// 縮尺の変化を有効にするかどうか
		/// </summary>
		public  bool  scaleEnabled
		{
			get
			{
				return m_ScaleEnabled ;
			}
			set
			{
				if( m_ScaleEnabled != value )
				{
					m_ScaleEnabled = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleFrom    = Vector3.one ;

		/// <summary>
		/// 開始縮尺
		/// </summary>
		public  Vector3  scaleFrom
		{
			get
			{
				return m_ScaleFrom ;
			}
			set
			{
				if( m_ScaleFrom != value )
				{
					m_ScaleFrom = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 開始縮尺Ｘ
		/// </summary>
		public float scaleFromX
		{
			get
			{
				return m_ScaleFrom.x ;
			}
			set
			{
				scaleFrom = new Vector3( value, m_ScaleFrom.y, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｙ
		/// </summary>
		public float scaleFromY
		{
			get
			{
				return m_ScaleFrom.y ;
			}
			set
			{
				scaleFrom = new Vector3( m_ScaleFrom.x, value, m_ScaleFrom.z ) ;
			}
		}

		/// <summary>
		/// 開始縮尺Ｚ
		/// </summary>
		public float scaleFromZ
		{
			get
			{
				return m_ScaleFrom.z ;
			}
			set
			{
				scaleFrom = new Vector3( m_ScaleFrom.x, m_ScaleFrom.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private Vector3 m_ScaleTo      = Vector3.one ;

		/// <summary>
		/// 終了縮尺
		/// </summary>
		public  Vector3  scaleTo
		{
			get
			{
				return m_ScaleTo ;
			}
			set
			{
				if( m_ScaleTo != value )
				{
					m_ScaleTo = value ;
					RefreshChecker() ;
				}
			}
		}

		/// <summary>
		/// 終了縮尺Ｘ
		/// </summary>
		public float scaleToX
		{
			get
			{
				return m_ScaleTo.x ;
			}
			set
			{
				scaleTo = new Vector3( value, m_ScaleTo.y, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｙ
		/// </summary>
		public float scaleToY
		{
			get
			{
				return m_ScaleTo.y ;
			}
			set
			{
				scaleTo = new Vector3( m_ScaleTo.x, value, m_ScaleTo.z ) ;
			}
		}

		/// <summary>
		/// 終了縮尺Ｚ
		/// </summary>
		public float scaleToZ
		{
			get
			{
				return m_ScaleTo.z ;
			}
			set
			{
				scaleTo = new Vector3( m_ScaleTo.x, m_ScaleTo.y, value ) ;
			}
		}

		[SerializeField][HideInInspector]
		private ProcessType m_ScaleProcessType = ProcessType.Ease ;

		/// <summary>
		/// 縮尺のカーブの処理種別
		/// </summary>
		public  ProcessType  scaleProcessType
		{
			get
			{
				return m_ScaleProcessType ;
			}
			set
			{
				if( m_ScaleProcessType != value )
				{
					m_ScaleProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private EaseType m_ScaleEaseType = EaseType.linear ;

		/// <summary>
		/// 縮尺のイーズの種別
		/// </summary>
		public  EaseType  scaleEaseType
		{
			get
			{
				return m_ScaleEaseType ;
			}
			set
			{
				if( m_ScaleEaseType != value )
				{
					m_ScaleEaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private AnimationCurve m_ScaleAnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;

		/// <summary>
		/// 縮尺のアニメーションカーブ
		/// </summary>
		public  AnimationCurve  scaleAnimationCurve
		{
			get
			{
				return m_ScaleAnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_ScaleAnimationCurve, value ) == true )
				{
					m_ScaleAnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool m_ScaleFoldOut = true ;

		/// <summary>
		/// 縮尺のインスペクターでの表示領域操作(エディタのみ使用)
		/// </summary>
		public  bool  scaleFoldOut
		{
			get
			{
				return m_ScaleFoldOut ;
			}
			set
			{
				m_ScaleFoldOut = value ;
			}
		}

		//-------------------------------------------

		[SerializeField][HideInInspector]
		private ProcessType m_ProcessType = ProcessType.Ease ;
		public  ProcessType   processType
		{
			get
			{
				return m_ProcessType ;
			}
			set
			{
				if( m_ProcessType != value )
				{
					m_ProcessType  = value ;
					RefreshChecker() ;
				}
			}
		}


		[SerializeField][HideInInspector]
		private EaseType m_EaseType = EaseType.linear ;
		public  EaseType   easeType
		{
			get
			{
				return m_EaseType ;
			}
			set
			{
				if( m_EaseType != value )
				{
					m_EaseType  = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブバージョン
		[SerializeField][HideInInspector]
		private AnimationCurve m_AnimationCurve = AnimationCurve.Linear(  0, 0, 1.0f, 1.0f ) ;
		public  AnimationCurve   animationCurve
		{
			get
			{
				return m_AnimationCurve ;
			}
			set
			{
				if( IsModifyAnamationCurve( m_AnimationCurve, value ) == true )
				{
					m_AnimationCurve = value ;
					RefreshChecker() ;
				}
			}
		}

		// アニメーションカーブに変化があったか確認する
		private bool IsModifyAnamationCurve( AnimationCurve a1, AnimationCurve a2 )
		{
			if( a1.length != a2.length || a1.postWrapMode != a2.postWrapMode || a1.preWrapMode != a2.preWrapMode )
			{
				return true ;
			}

			int i, l = a1.length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Keyframe k1 = a1[ i ] ;	// 最終キー
				Keyframe k2 = a2[ i ] ;	// 最終キー

				if( k1.time != k2.time || k1.value != k2.value || k1.inTangent != k2.inTangent || k1.outTangent != k2.outTangent )
				{
					return true ;
				}
			}

			return false ;
		}



		// 相対位置か絶対位置か（相対位置）
		public enum ValueType
		{
			Relative = 0,
			Absolute = 1,
			RelativeAndUpdate = 2,
		}

		[SerializeField][HideInInspector]
		private ValueType m_ValueType = ValueType.Relative ;
		public  ValueType   valueType
		{
			get
			{
				return m_ValueType ;
			}
			set
			{
				if( m_ValueType != value )
				{
					m_ValueType  = value ;
					RefreshChecker() ;
				}
			}
		}

		//------------------------------------------------------

		/// <summary>
		/// ループさせるか
		/// </summary>
		public bool loop = false ;

		/// <summary>
		/// ループ場合に終了時に逆再生するか
		/// </summary>
		public bool reverse = true ;

		// タイムスケールを無視するかどうか（する）
		public bool ignoreTimeScale = true ;

		/// <summary>
		/// インスタンス生成と同時に実行されるようにするか
		/// </summary>
		public bool playOnAwake = true ;

		/// <summary>
		/// 処理中は入力を受け付けないようにするか
		/// </summary>
//		public bool interactionDisableInPlaying = true ;

		//--------------------------------------------------------

		private Transform		m_Transform ;

		private Vector3			m_BasePosition ;
		private Vector3			m_BaseRotation ;
		private Vector3			m_BaseScale ;

		private SoftTransform	m_Base ;

		private float			m_Time ;
		private float			m_BaseTime ;

		// 実行状況
		private bool			m_Running = false ;
		public  bool			isRunning{ get{ return m_Running ; } }

		// 再生状況
		private bool			m_Playing = false ;
		public  bool			isPlaying{ get{ return m_Playing ; } }
	

		private bool			m_Busy = false ;

		//--------------------------------------------------------

		public UnityEvent onFinished = new UnityEvent() ;

		private Action<string, SoftTransformTween> m_OnFinishedAction ;
		public  Action<string, SoftTransformTween>   onFinishedAction
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
		public void SetOnFinished( Action<string, SoftTransformTween> tOnFinishAction )
		{
			m_OnFinishedAction = tOnFinishAction ;
		}


		//--------------------------------------------------------

		void OnDisable()
		{
			// 無効化した際にチェッカーが開いていれば強制的に閉じる
			isChecker = false ;
		}

		void Awake()
		{
			if( Application.isPlaying == true && isChecker == true )
			{
				LoadState() ;
			}		
		}
	
		private bool	 m_IsStarting = false ;

		private float	m_DelayStack	= -1 ;
		private float	m_DurationStack	= -1 ;
		private float	m_OffsetStack	=  0 ; 

		void Start()
		{
			// カスタムリスナー登録
			onFinished.AddListener( OnFinishedInner ) ;

			m_IsStarting = true ;	// スタートが実行された

			if( Application.isPlaying == true && ( playOnAwake == true || m_Running == true ) )
			{
				// Start() の前に　Play が実行されている可能性もある
				Play( m_DelayStack, m_DurationStack, m_OffsetStack ) ;
			}
		}
	
		//---------------------------------------------

		// 内部リスナー登録
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
	
//		private bool m_KeepInteractionDisableProcass = false ; 

//		private void DisableInteraction()
//		{
//			// 自身が処理を担当する
//			m_KeepInteractionDisableProcass = true ;
//		}

//		private void EnableInteraction()
//		{
//			// 自身は処理の担当ではない
//			if( m_KeepInteractionDisableProcass == false )
//			{
//				return ;
//			}
//
//			// 処理終了
//			m_KeepInteractionDisableProcass = false ;
//		}


		//---------------------------------------------

		/// <summary>
		/// 再生する(コルーチン)
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator Play_Coroutine( float tDelay = -1, float tDuration = -1 )
		{
			Play( tDelay, tDuration ) ;

			while( m_Running == true || m_Playing == true )
			{
				yield return null ;
			}
		}


		/// <summary>
		/// 再生する
		/// </summary>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		public void Play( float tDelay = -1, float tDuration = -1, float tOffset = 0  )
		{
			enabled = true ;

			if( m_IsStarting == false )
			{
				// スタート前なのでためるだけ
				m_DelayStack	= tDelay ;
				m_DurationStack	= tDuration ;
				m_OffsetStack	= tOffset ;

				m_Running = true ;

				return ;
			}

			//-------------------------------------------

			if( tDelay >= 0 )
			{
				delay = tDelay ;
			}

			if( tDuration >= 0 )
			{
				duration = tDuration ;
			}

			m_Transform	= gameObject.GetComponent<Transform>() ;
			if( m_Transform != null )
			{
				m_BasePosition		= m_Transform.localPosition ;
				m_BaseRotation		= m_Transform.localEulerAngles ;
				m_BaseScale			= m_Transform.localScale ;
			}

//			if( interactionDisableInPlaying == true )
//			{
//				DisableInteraction() ;
//			}

			//--------------------------------------------------

			m_Base = gameObject.GetComponent<SoftTransform>() ;
			if( m_Base != null )
			{
				m_BasePosition = m_Base.localPosition ;
				m_BaseRotation = m_Base.localRotation ;
				m_BaseScale    = m_Base.localScale ;
			}

			//-------------------------------

			m_Time = tOffset ;
			SetState( m_Time ) ;	// 初期位置へ移動させる

			m_BaseTime = Time.realtimeSinceStartup ;

			m_Running = true ;
			m_Playing = true ;

			m_Busy = true ;
		}

		/// <summary>
		/// 状態を再生前に戻す
		/// </summary>
		public void Revert()
		{
			if( m_Busy == false )
			{
				return ;
			}

			m_Running = false ;
			m_Playing = false ;

			m_Transform	= gameObject.GetComponent<Transform>() ;
			if( m_Transform != null )
			{
				m_Transform.localPosition		= m_BasePosition ;
				m_Transform.localEulerAngles	= m_BaseRotation ;
				m_Transform.localScale			= m_BaseScale ;
			}

			m_Busy = false ;
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
		/// 再開する
		/// </summary>
		public void Continue()
		{
			if( m_Running == true )
			{
				m_Playing = true ;
			}
		}

		/// <summary>
		/// 完全停止する
		/// </summary>
		public void Stop()
		{
			if( m_Running == true )
			{
//				if( interactionDisableInPlaying == true )
//				{
//					EnableInteraction() ;
//				}

				m_Playing = false ;
				m_Running = false ;
			}
		}

		/// <summary>
		/// 完全に停止させ状態を元に戻す
		/// </summary>
		public void StopAndReset()
		{
			if( m_Running == true )
			{
//				if( interactionDisableInPlaying == true )
//				{
//					EnableInteraction() ;
//				}

				m_Playing = false ;
				m_Running = false ;
			}

			Revert() ;
		}

		/// <summary>
		/// 強制的に最後の状態にする
		/// </summary>
		public void Finish()
		{
			// 強制的に最後の状態にする(ただしループになっているものは無効)
			if( loop == true )
			{
				return ;
			}

			if( m_Running == true && m_Playing == true )
			{
				Modify( 1, true ) ;

				// 終了

				m_Playing = false ;
				m_Running = false ;

				// コールバック呼び出し
				onFinished.Invoke() ;
			}
			else
			{
				Modify( 1, true ) ;
			}
		}

		void Update()
		{
			if( m_Running == true && m_Playing == true )
			{
				m_Time = m_Time + GetDeltaTime() ;

				if( SetState( m_Time ) == true )
				{
					// 終了
//					if( interactionDisableInPlaying == true )
//					{
//						EnableInteraction() ;
//					}

					m_Playing = false ;
					m_Running = false ;

					// コールバック呼び出し
					onFinished.Invoke() ;
				}
			}
		}

		// 現在の経過時間から状態を設定する
		private bool SetState( float tTime )
		{
			float tDuration = 0 ;
			if( m_ProcessType == ProcessType.Ease )
			{
				tDuration = duration ;
			}
			else
			if( m_ProcessType == ProcessType.AnimationCurve )
			{
				int l = m_AnimationCurve.length ;
				Keyframe tKeyFrame = m_AnimationCurve[ l - 1 ] ;	// 最終キー
				tDuration = tKeyFrame.time ;
			}

			if( tDuration <= 0 )
			{
				return false ;
			}

			//---------------------------------------------

			if( tTime <  delay )
			{
				Modify( 0, false ) ;
				return false ;
			}

			tTime = m_Time - delay ;

			if( m_Base != null )
			{
				m_BasePosition = m_Base.localPosition ;
				m_BaseRotation = m_Base.localRotation ;
				m_BaseScale    = m_Base.localScale ;
			}

			int tCount ;
			float tFactor ;

			if( loop == false )
			{
				// ループ無し

				tFactor  = tTime / tDuration ;
				if( tFactor >= 1 )
				{
					tFactor  = 1 ;
				}

				Modify( tFactor, tFactor >= 1 ) ;

				if( tFactor >= 1 )
				{
					return true ;
				}
			}
			else
			{
				// ループ有り
				
				tCount  = ( int )( tTime / tDuration ) ;
				tFactor = ( tTime % tDuration ) / tDuration ;

				if( reverse == true && ( tCount & 1 ) == 1 )
				{
					tFactor = 1.0f - tFactor ;
				}

				Modify( tFactor, false ) ;
			}

			return false ;
		}


		/// <summary>
		/// 実行開始からの経過時間
		/// </summary>
		public float processTime
		{
			get
			{
				return m_Time ;
			}
			set
			{
				m_Time = value ;

				// ベースタイムをリセットしておかないと次のフレームで大きく飛んでしまう
				m_BaseTime = Time.realtimeSinceStartup ;

				if( m_Running == true )
				{
					SetState( m_Time ) ;
				}
			}
		}

		private void Modify( float tFactor, bool tFinished )
		{
			if( m_Transform != null )
			{
				if( m_PositionEnabled == true )
				{
					Vector3 tDelta = GetValue( m_PositionFrom, m_PositionTo, tFactor, m_PositionProcessType, m_PositionEaseType, m_PositionAnimationCurve ) ;

					if( m_ValueType == ValueType.Relative || m_ValueType == ValueType.RelativeAndUpdate )
					{
						m_Transform.localPosition = m_BasePosition + tDelta ;

						if( tFinished == true && m_ValueType == ValueType.RelativeAndUpdate && m_Base != null )
						{
							m_Base.localPosition = m_BasePosition + tDelta ;
						}
					}
					else
					{
						m_Transform.localPosition = tDelta ;
					}
				}

				if( m_RotationEnabled == true )
				{
					Vector3 tDelta = GetValue( m_RotationFrom, m_RotationTo, tFactor, m_RotationProcessType, m_RotationEaseType, m_RotationAnimationCurve ) ;
			
					if( m_ValueType == ValueType.Relative || m_ValueType == ValueType.RelativeAndUpdate )
					{
						m_Transform.localEulerAngles = m_BaseRotation + tDelta ;

						if( tFinished == true && m_ValueType == ValueType.RelativeAndUpdate && m_Base != null )
						{
							m_Base.localRotation = m_BaseRotation + tDelta ;
						}
					}
					else
					{
						m_Transform.localEulerAngles = tDelta ;
					}
				}

				if( m_ScaleEnabled == true )
				{
					Vector3 tDelta = GetValue( m_ScaleFrom, m_ScaleTo, tFactor, m_ScaleProcessType, m_ScaleEaseType, m_ScaleAnimationCurve ) ;

					if( m_ValueType == ValueType.Relative || m_ValueType == ValueType.RelativeAndUpdate  )
					{
						m_Transform.localScale = new Vector3( m_BaseScale.x * tDelta.x, m_BaseScale.y * tDelta.y, m_BaseScale.z * tDelta.z ) ;

						if( tFinished == true && m_ValueType == ValueType.RelativeAndUpdate && m_Base != null )
						{
							m_Base.localScale = new Vector3( m_BaseScale.x * tDelta.x, m_BaseScale.y * tDelta.y, m_BaseScale.z * tDelta.z ) ;
						}
					}
					else
					{
						m_Transform.localScale = tDelta ;
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

			if( ignoreTimeScale == false )
			{
				// タイムスケールを無視しない
				tDeltaTime = tDeltaTime * Time.timeScale ;
			}

			return tDeltaTime ;
		}


		//--------------------------------------------------------

		// Vector3 の変化中の値を取得
		public static Vector3 GetValue( Vector3 tStart, Vector3 tEnd, float tFactor, ProcessType tProcessType, EaseType tEaseType, AnimationCurve tAnimationCurve = null )
		{
			float x = GetValue( tStart.x, tEnd.x, tFactor, tProcessType, tEaseType, tAnimationCurve ) ;
			float y = GetValue( tStart.y, tEnd.y, tFactor, tProcessType, tEaseType, tAnimationCurve ) ;
			float z = GetValue( tStart.z, tEnd.z, tFactor, tProcessType, tEaseType, tAnimationCurve ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public static float GetValue( float tStart, float tEnd, float tFactor, ProcessType tProcessType, EaseType tEaseType, AnimationCurve tAnimationCurve = null )
		{
			float tValue = 0 ;

			if( tProcessType == ProcessType.Ease )
			{
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
			}
			else
			if( tProcessType == ProcessType.AnimationCurve )
			{
				if( tAnimationCurve != null )
				{
					int l = tAnimationCurve.length ;
					Keyframe tKeyFrame = tAnimationCurve[ l - 1 ] ;	// 最終キー
					float tDuration = tKeyFrame.time ;
	
					tValue = tAnimationCurve.Evaluate( tDuration * tFactor ) ;
				}

				tValue = Mathf.Lerp( tStart, tEnd, tValue ) ;
			}

			return tValue ;
		}


		//------------------------

		private static float easeInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private static float easeOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private static float easeInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private static float easeInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private static float easeOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private static float easeInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private static float easeInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private static float easeOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private static float easeInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private static float easeInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private static float easeOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private static float easeInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private static float easeInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private static float easeOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private static float easeInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private static float easeInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private static float easeOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private static float easeInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private static float easeInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private static float easeOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private static float easeInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private static float linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private static float spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private static float easeInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - easeOutBounce( 0, end, d - value ) + start ;
		}
	
		private static float easeOutBounce( float start, float end, float value )
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

		private static float easeInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return easeInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return easeOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private static float easeInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private static float easeOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float easeInOutBack( float start, float end, float value )
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

		private static float easeInElastic( float start, float end, float value )
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

		private static float easeOutElastic( float start, float end, float value )
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

		private static float easeInOutElastic( float start, float end, float value )
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

		private static float punch( float amplitude, float value )
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

		private static float clerp( float start, float end, float value )
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

		//--------------------------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3	m_InitialPosition ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialRotation ;

		[SerializeField][HideInInspector]
		private Vector3	m_InitialScale ;

		//-------------------------------------------------

		[SerializeField][HideInInspector]
		private bool mIsChecker = false ;
		public  bool  isChecker
		{
			get
			{
				return mIsChecker ;
			}
			set
			{
				if( mIsChecker != value )
				{
					mIsChecker = value ;
					if( mIsChecker == true )
					{
						SaveState() ;
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
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

			if( mIsChecker == true )
			{
				LoadState() ;
				SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
			}
		}
	
		// ただの位置表示用
		[SerializeField][HideInInspector]
		private float m_CheckFactor = 0 ;
		public  float   checkFactor
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
					if( mIsChecker == true )
					{
						SetCheckFactor( m_CheckFactor, m_InitialPosition, m_InitialRotation, m_InitialScale ) ;
					}
				}
			}
		}

		// モーションチェック専用で現在の状態を退避する
		public void SaveState()
		{
			Transform tTransform = gameObject.GetComponent<Transform>() ;
			if( tTransform != null )
			{
				m_InitialPosition = tTransform.localPosition ;
				m_InitialRotation = tTransform.localEulerAngles ;
				m_InitialScale    = tTransform.localScale ;
			}
		}

		// モーションチェック専用で現在の状態を復帰する
		private void LoadState()
		{
			Transform tTransform = gameObject.GetComponent<Transform>() ;
			if( tTransform != null )
			{
				tTransform.localPosition = m_InitialPosition ;
				tTransform.localEulerAngles = m_InitialRotation ;
				tTransform.localScale       = m_InitialScale ;
			}
		}

		// デバッグ用
		private void SetCheckFactor( float tCheckFactor, Vector3 tInitialPosition, Vector3 tInitialRotation, Vector3 tInitialScale )
		{
			m_Transform	= gameObject.GetComponent<Transform>() ;

			//-----------------

			m_BasePosition		= tInitialPosition ;
			m_BaseRotation		= tInitialRotation ;
			m_BaseScale			= tInitialScale ;

			//-----------------

			Modify( tCheckFactor, false ) ;
		}
	}
}
