using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine.Assertions ;
using System.Linq ;

#if UNITY_EDITOR
using UnityEditor ;
#endif


/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace InputHelper
{
	/// <summary>
	/// 入力操作クラス Version 2018/06/12 0
	/// </summary>
	public class InputManager : MonoBehaviour
	{
		// 注意：パッド系の操作をした場合、一度パッド系の操作を完全解除するまで、実際のバッド系入力ができないようにする事も可能になっている。→modeEnabled


#if UNITY_EDITOR
		/// <summary>
		/// InputManager を生成
		/// </summary>
//		[ MenuItem( "InputHelper/Create a InputManager") ]
		[ MenuItem( "GameObject/Helper/InputHelper/InputManager", false, 24 ) ]
		public static void CreateInputManager()
		{
			GameObject tGameObject = new GameObject( "InputManager" ) ;
		
			Transform tTransform = tGameObject.transform ;
			tTransform.SetParent( null ) ;
			tTransform.localPosition = Vector3.zero ;
			tTransform.localRotation = Quaternion.identity ;
			tTransform.localScale = Vector3.one ;
		
			tGameObject.AddComponent<InputManager>() ;
			Selection.activeGameObject = tGameObject ;
		}
#endif

		// インプットマネージャのインスタンス(シングルトン)
		private static InputManager m_Instance = null ; 

		/// <summary>
		/// インプットマネージャのインスタンス
		/// </summary>
		public  static InputManager   instance
		{
			get
			{
				return m_Instance ;
			}
		}
	
		/// <summary>
		/// インプットマネージャのインスタンスを生成する
		/// </summary>
		/// <param name="tRunInbackground">バックグラウンドで再生させるようにするかどうか</param>
		/// <returns>インプットマネージャのインスタンス</returns>
		public static InputManager Create( Transform tParent = null )
		{
			if( m_Instance != null )
			{
				return m_Instance ;
			}
			
#if UNITY_EDITOR
			if( InputManagerSettings.Check() == false )
			{
				Debug.LogWarning( "InputManager[Edit->Project Settings->Input]に必要なパラメータが設定されていません\n[Tools->Initialize InputManager]を実行してください" ) ;
				return null ;
			}
#endif
			// オブジェクトが非アクティブだと検出されないのでオブジェクトを非アクティブにしてはならない
			// この判定は必須で mInstance は static であるためシーンの最初はオブジェクトが存在しても null になっている
			m_Instance = GameObject.FindObjectOfType( typeof( InputManager ) ) as InputManager ;
			if( m_Instance == null )
			{
				GameObject tGameObject = new GameObject( "InputManager" ) ;
				if( tParent != null )
				{
					tGameObject.transform.SetParent( tParent, false ) ;
				}

				tGameObject.AddComponent<InputManager>() ;
			}

			return m_Instance ;
		}
	
		/// <summary>
		/// インプットマネージャのインスタンスを破棄する
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
		
			InputManager tInstanceOther = GameObject.FindObjectOfType( typeof( InputManager ) ) as InputManager ;
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
		
	//		gameObject.hideFlags = HideFlags.HideInHierarchy ;
		
			//-----------------------------
		
			// 原点じゃないと気持ち悪い
			gameObject.transform.localPosition = Vector3.zero ;
			gameObject.transform.localRotation = Quaternion.identity ;
			gameObject.transform.localScale = Vector3.one ;
		}

		void OnDestroy()
		{
			if( m_Instance == this )
			{
				m_Instance  = null ;
			}
		}
	
		//---------------------------------------------------------------------------

		public enum Mode
		{
			Unknown = -1,
			Pointer	=  0,
			GamePad	=  1,
		}

		private Mode m_Mode = Mode.Pointer ;	// デフォルトはタッチモード
		private bool m_Hold = false ;

		private Vector3 m_Position = Vector3.zero ;

		private Action<Mode>	m_OnMode = null ;

		public delegate void OnModeDelegate( Mode tMode ) ;

		private OnModeDelegate	m_OnModeDelegate ;


		/// <summary>
		/// 縦軸の符号反転
		/// </summary>
		public static bool invert = false ;	

		//---------------------------------------------------------------------------

		/// <summary>
		/// 現在のモードが実際に有効かどうか
		/// </summary>
		public static bool modeEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}

				return ! m_Instance.m_Hold ;
			}
		}

		public static Mode mode
		{
			get
			{
				if( m_Instance == null )
				{
					return Mode.Unknown ;
				}

				return m_Instance.m_Mode ;
			}
		}

		//---------------------------------------------------------------------------

		private bool m_Enabled = true ;

		/// <summary>
		/// 有効にする
		/// </summary>
		public static void Enable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_Enabled = true ;
		}

		/// <summary>
		/// 無効にする
		/// </summary>
		public static void Disable()
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.m_Enabled = false ;
		}

		/// <summary>
		/// 有効無効状態
		/// </summary>
		public static bool isEnabled
		{
			get
			{
				if(  m_Instance == null )
				{
					return false ;
				}

				return m_Instance.m_Enabled ;
			}
		}
		
		//---------------------------------------------------------------------------

		public class Updater
		{
			public Action<System.Object>	action = null ;
			public System.Object			option = null ;

			public Updater( Action<System.Object> tAction, System.Object tOption )
			{
				action = tAction ;
				option = tOption ;
			}
		}

		private Stack<Updater>	m_Updater = new Stack<Updater>() ;


		/// <summary>
		/// GamePadUpdater を登録する
		/// </summary>
		/// <param name="tAction"></param>
		/// <param name="tOption"></param>
		/// <returns></returns>
		public static bool AddUpdater( Action<System.Object> tAction, System.Object tOption = null )
		{
			if( InputManager.instance == null )
			{
				return false ;
			}

			return InputManager.instance.AddUpdater_Private( tAction, tOption ) ;
		}
		
		private bool AddUpdater_Private( Action<System.Object> tAction, System.Object tOption )
		{
			if( tAction == null )
			{
				return false ;
			}

			m_Updater.Push( new Updater( tAction, tOption ) ) ;

			return true ;
		}

		/// <summary>
		/// GamePadUpdater を除去する
		/// </summary>
		/// <param name="tAction"></param>
		/// <returns></returns>
		public static bool RemoveUpdater( Action<System.Object> tAction = null )
		{
			if( InputManager.instance == null )
			{
				return false ;
			}

			return InputManager.instance.RemoveUpdater_Private( tAction ) ;
		}

		private bool RemoveUpdater_Private( Action<System.Object> tAction )
		{
			if( m_Updater.Count == 0 )
			{
				return false ;	// キューには何も積まれていない
			}

			if( tAction != null )
			{
				// 間違った GamePadUpdater を除去しないように確認する

				Updater tStateUpdater = m_Updater.Peek() ;
				if( tStateUpdater.action != tAction )
				{
					// 違う！
					return false ;
				}
			}
			
			m_Updater.Pop() ;

			return true ;
		}

		//---------------------------------------------------------------------------

		void LateUpdate()
		{
			if( m_Enabled == false )
			{
				return ;
			}

			//----------------------------------------------------------

			if( m_Mode == Mode.Pointer )
			{
				// Pointer モード
				if( m_Hold == true )
				{
					// 切り替えた直後は一度全開放しないと入力できない
					if( Input.GetMouseButton( 0 ) == false && Input.GetMouseButton( 1 ) == false && Input.GetMouseButton( 2 ) == false )
					{
						m_Hold = false ;
//						Debug.LogWarning( "タッチモードが有効になった" ) ;
					}
				}
				else
				{
					// Pointer モード有効中

					Mouse.Update() ;

//					Debug.LogWarning( "タッチモード中" ) ;
					Vector2 tAxis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 tAxis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 tAxis_2 = GamePad.GetAxis( 2 ) ;
					if( GamePad.GetButtonAll() != 0 || tAxis_0.x != 0 || tAxis_0.y != 0 || tAxis_1.x != 0 || tAxis_1.y != 0 || tAxis_2.x != 0 || tAxis_2.y != 0 )
					{
						// パッドのいずれかが押された
						Cursor.visible = false ;

						m_Mode = Mode.GamePad ;
						m_Hold = true ;
//						Debug.LogWarning( "パッドモードに移行する" ) ;

						if( m_OnMode != null )
						{
							m_OnMode( m_Mode ) ;
						}

						if( m_OnModeDelegate != null )
						{
							m_OnModeDelegate( m_Mode ) ;
						}
					}
				}
			}
			else
			{
				// GamePad モード
				if( m_Hold == true )
				{
					// 切り替えた直後は一度全開放しないと入力できない
					Vector2 tAxis_0 = GamePad.GetAxis( 0 ) ;
					Vector2 tAxis_1 = GamePad.GetAxis( 1 ) ;
					Vector2 tAxis_2 = GamePad.GetAxis( 2 ) ;
					if( GamePad.GetButtonAll() == 0 && tAxis_0.x == 0 && tAxis_0.y == 0 && tAxis_1.x == 0 && tAxis_1.y == 0 && tAxis_2.x == 0 && tAxis_2.y == 0 )
					{
						m_Hold = false ;
//						Debug.LogWarning( "パッドモードになった" ) ;

						m_Position = Input.mousePosition ;
					}
				}
				else
				{
					// GamePad モード有効中

					GamePad.Update() ;	// SmartMethod 用

//					Debug.LogWarning( "パッドモード中" ) ;
					if( m_Position.Equals( Input.mousePosition ) == false || Input.GetMouseButton( 0 ) == true || Input.GetMouseButton( 1 ) == true || Input.GetMouseButton( 2 ) == true )
					{
						Cursor.visible = true ;

						m_Mode = Mode.Pointer ;
						m_Hold = true ;
//						Debug.LogWarning( "タッチモードに移行する" ) ;

						if( m_OnMode != null )
						{
							m_OnMode( m_Mode ) ;
						}

						if( m_OnModeDelegate != null )
						{
							m_OnModeDelegate( m_Mode ) ;
						}
					}
				}
			}

			//----------------------------------------------------------

			if( m_Updater.Count >  0 )
			{
				Updater tUpdater = m_Updater.Peek() ;
				tUpdater.action( tUpdater.option ) ;
			}	
		}

		//---------------------------------------------------------------------------

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="tOnMode"></param>
		public static void SetOnMode( Action<Mode> tOnMode, bool tCall = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.SetOnMode_Private( tOnMode, tCall ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void SetOnMode_Private( Action<Mode> tOnMode, bool tCall )
		{
			m_OnMode = tOnMode ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( tCall == true && m_OnMode != null )
			{
				m_OnMode( m_Mode ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを設定する
		/// </summary>
		/// <param name="tOnMode"></param>
		public static void AddOnMode( OnModeDelegate tOnModeDelegate, bool tCall = false )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.AddOnMode_Private( tOnModeDelegate, tCall ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void AddOnMode_Private( OnModeDelegate tOnModeDelegate, bool tCall )
		{
			// 念のため既存の登録があったら削除する(おそらく必要は無いと思うが)
			m_OnModeDelegate -= tOnModeDelegate ;

			m_OnModeDelegate += tOnModeDelegate ;
			
			// セットした直後に現在のモードでコールバックを呼ぶ
			if( tCall == true )
			{
				m_OnModeDelegate( m_Mode ) ;
			}
		}

		/// <summary>
		/// モード切替を通知するコールバックを解除する
		/// </summary>
		/// <param name="tOnMode"></param>
		public static void RemoveOnMode( OnModeDelegate tOnModeDelegate )
		{
			if( m_Instance == null )
			{
				return ;
			}

			m_Instance.RemoveOnMode_Private( tOnModeDelegate ) ;
		}

		// モード切替を通知するコールバックを設定する
		private void RemoveOnMode_Private( OnModeDelegate tOnModeDelegate )
		{
			m_OnModeDelegate -= tOnModeDelegate ;
		}
	}

	//--------------------------------------------------------------------------------------------

#if UNITY_EDITOR

	/// <summary>
	/// InputManager を設定するクラス(Editor専用)
	/// </summary>
	public class InputManagerSettings
	{
		public enum AxisType	
		{
			KeyOrMouseButton	= 0,
			MouseMovement		= 1,
			JoystickAxis		= 2,
		} ;

		public class Axis
		{
			public string	name					= "" ;
			public string	descriptiveName			= "" ;
			public string	descriptiveNegativeName	= "" ;
			public string	negativeButton			= "" ;
			public string	positiveButton			= "" ;
			public string	altNegativeButton		= "" ;
			public string	altPositiveButton		= "" ;
	
			public float	gravity					= 0 ;
			public float	dead					= 0 ;
			public float	sensitivity				= 0 ;
	
			public bool		snap					= false ;
			public bool		invert					= false ;
		
			public AxisType	type					= AxisType.KeyOrMouseButton ;
	
			public int		axisNum					= 1 ;
			public int		joyNum					= 0 ;
		}

		//-------------------------------------------------------------------------------------------

		[ MenuItem( "Tools/Initialize InputManager" ) ]
		public static void Initialize()
		{
			// 設定をクリアする
			Clear() ;
			
			int i, p ;

//			for( i  =  0 ; i <=15 ; i ++ )
//			{
//				AddAxis( CreateButton( "Button_" + i.ToString( "D02" ), "joystick button " + i.ToString() ) ) ;
//			}
//			for( i  =  1 ; i <= 12 ; i ++ )
//			{
//				AddAxis( CreatePadAxis( "Axis_" + i.ToString( "D02" ), 0, i ) ) ;
//			}

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <=15 ; i ++ )
				{
					AddAxis( CreateButton( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ), "joystick " + p.ToString() + " button " + i.ToString() ) ) ;
				}
				for( i  =  1 ; i <= 12 ; i ++ )
				{
					AddAxis( CreatePadAxis( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ), p, i ) ) ;
				}
			}
		}

		/// <summary>
		/// 設定をクリアする
		/// </summary>
		private static void Clear()
		{
			SerializedObject	tSerializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			SerializedProperty	tAxesProperty = tSerializedObject.FindProperty( "m_Axes" ) ;

			int i, j, l, m, p ;

			//--------------

			List<string> tKey = new List<string>() ;

//			for( i  =  0 ; i <= 15 ; i ++ )
//			{
//				tKey.Add( "Button_" + i.ToString( "D02" ) ) ;
//			}
//			for( i  =  1 ; i <= 12 ; i ++ )
//			{
//				tKey.Add( "Axis_" + i.ToString( "D02" ) ) ;
//			}

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					tKey.Add( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ) ) ;
				}

				for( i  =  1 ; i <= 12 ; i ++ )
				{
					tKey.Add( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ) ) ;
				}
			}

			m = tKey.Count ;

			//--------------

			SerializedProperty tAxisPropertyElement ;
			string tAxisName ;

			l = tAxesProperty.arraySize ;
			i = 0 ;

			while( true )
			{
				tAxisPropertyElement = tAxesProperty.GetArrayElementAtIndex( i ) ;
				tAxisName = GetChildProperty( tAxisPropertyElement, "m_Name" ).stringValue ;
				
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( tAxisName == tKey[ j ] )
					{
						// 削除対象発見
						break ;
					}
				}

				if( j <  m )
				{
					// 削除対象
					tAxesProperty.DeleteArrayElementAtIndex( i ) ;
					l -- ;
				}
				else
				{
					i ++ ;
				}

				if( i >= l )
				{
					// 終了
					break ;
				}
			}

//			tAxesProperty.ClearArray() ;
			tSerializedObject.ApplyModifiedProperties() ;
		}

		/// <summary>
		/// InputManager に必要な設定が追加されているか確認する
		/// </summary>
		public static bool Check()
		{
			SerializedObject	tSerializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			SerializedProperty	tAxesProperty = tSerializedObject.FindProperty( "m_Axes" ) ;

			int i, l, m, p ;

			//--------------

			Dictionary<string,bool> tKeyValue = new Dictionary<string, bool>() ;

//			for( i  =  0 ; i <= 15 ; i ++ )
//			{
//				tKey.Add( "Button_" + i.ToString( "D02" ) ) ;
//			}
//			for( i  =  1 ; i <= 12 ; i ++ )
//			{
//				tKey.Add( "Axis_" + i.ToString( "D02" ) ) ;
//			}

			for( p  = 1 ; p <= 4 ; p ++ )
			{
				for( i  =  0 ; i <= 15 ; i ++ )
				{
					tKeyValue.Add( "Player_" + p.ToString() + "_Button_" + i.ToString( "D02" ), false ) ;
				}

				for( i  =  1 ; i <= 12 ; i ++ )
				{
					tKeyValue.Add( "Player_" + p.ToString() + "_Axis_" + i.ToString( "D02" ), false ) ;
				}
			}

			m = tKeyValue.Count ;

			//--------------

			SerializedProperty tAxisPropertyElement ;
			string tAxisName ;

			l = tAxesProperty.arraySize ;
			i = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tAxisPropertyElement = tAxesProperty.GetArrayElementAtIndex( i ) ;
				tAxisName = GetChildProperty( tAxisPropertyElement, "m_Name" ).stringValue ;
				
				if( tKeyValue.ContainsKey( tAxisName ) == true )
				{
					tKeyValue[ tAxisName ] = true ;
				}
			}

			string[] tKey = new string[ m ] ;
			tKeyValue.Keys.CopyTo( tKey, 0 ) ;

			for( i  = 0 ; i <  m ; i ++ )
			{
				if( tKeyValue[ tKey[ i ] ] == false )
				{
					return false ;	// 設定されていないものがある
				}
			}

			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ボタンを生成する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tPositiveButton"></param>
		/// <param name="tAltPositiveButton"></param>
		/// <returns></returns>
		private static Axis CreateButton( string tName, string tPositiveButton )
		{
			Axis tAxis = new Axis() ;

			tAxis.name				= tName;
			tAxis.positiveButton	= tPositiveButton ;
			tAxis.gravity			= 1000 ;
			tAxis.dead				= 0.001f ;
			tAxis.sensitivity		= 1000 ;
			tAxis.type				= AxisType.KeyOrMouseButton ;
			
			return tAxis ;
		}

		public static Axis CreateKeyAxis( string tName, string tNegativeButton, string tPositiveButton )
		{
			Axis tAxis = new Axis() ;

			tAxis.name				= tName ;
			tAxis.negativeButton	= tNegativeButton ;
			tAxis.positiveButton	= tPositiveButton ;
			tAxis.gravity			= 3 ;
			tAxis.sensitivity		= 3 ;
			tAxis.dead				= 0.001f ;
			tAxis.type				= AxisType.KeyOrMouseButton ;
 
			return tAxis ;
		}

		/// <summary>
		/// ジョイスティックを生成する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tJoyNum"></param>
		/// <param name="tAxisNum"></param>
		/// <returns></returns>
		public static Axis CreatePadAxis( string tName, int tJoyNum, int tAxisNum )
		{
			Axis tAxis = new Axis() ;

			tAxis.name				= tName;
			tAxis.dead				= 0.2f ;
			tAxis.sensitivity		= 1 ;
			tAxis.type				= AxisType.JoystickAxis ;
			tAxis.axisNum			= tAxisNum ;
			tAxis.joyNum			= tJoyNum ;
 
			return tAxis ;
		}

		/// <summary>
		/// 設定を追加する
		/// </summary>
		/// <param name="tAxis"></param>
		private static void AddAxis( Axis tAxis )
		{
			SerializedObject	tSerializedObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/InputManager.asset" )[ 0 ] );
			SerializedProperty	tAxesProperty = tSerializedObject.FindProperty( "m_Axes" ) ;

			tAxesProperty.arraySize ++ ;
			tSerializedObject.ApplyModifiedProperties() ;
	
			SerializedProperty tAxisProperty = tAxesProperty.GetArrayElementAtIndex( tAxesProperty.arraySize - 1 ) ;
		
			GetChildProperty( tAxisProperty, "m_Name"					).stringValue	= tAxis.name ;
			GetChildProperty( tAxisProperty, "descriptiveName"			).stringValue	= tAxis.descriptiveName ;
			GetChildProperty( tAxisProperty, "descriptiveNegativeName"	).stringValue	= tAxis.descriptiveNegativeName ;
			GetChildProperty( tAxisProperty, "negativeButton"			).stringValue	= tAxis.negativeButton ;
			GetChildProperty( tAxisProperty, "positiveButton"			).stringValue	= tAxis.positiveButton ;
			GetChildProperty( tAxisProperty, "altNegativeButton"		).stringValue	= tAxis.altNegativeButton ;
			GetChildProperty( tAxisProperty, "altPositiveButton"		).stringValue	= tAxis.altPositiveButton ;
			GetChildProperty( tAxisProperty, "gravity"					).floatValue	= tAxis.gravity ;
			GetChildProperty( tAxisProperty, "dead"						).floatValue	= tAxis.dead ;
			GetChildProperty( tAxisProperty, "sensitivity"				).floatValue	= tAxis.sensitivity ;
			GetChildProperty( tAxisProperty, "snap"						).boolValue		= tAxis.snap ;
			GetChildProperty( tAxisProperty, "invert"					).boolValue		= tAxis.invert ;
			GetChildProperty( tAxisProperty, "type"						).intValue		= ( int )tAxis.type ;
			GetChildProperty( tAxisProperty, "axis"						).intValue		= tAxis.axisNum - 1 ;
			GetChildProperty( tAxisProperty, "joyNum"					).intValue		= tAxis.joyNum ;
 
			tSerializedObject.ApplyModifiedProperties() ;
		}
 
		private static SerializedProperty GetChildProperty( SerializedProperty tParent, string tName )
		{
			SerializedProperty tChild = tParent.Copy() ;
			tChild.Next( true ) ;

			do
			{
				if( tChild.name == tName )
				{
					return tChild ;
				}
			}
			while( tChild.Next( false ) ) ;

			return null ;
		}
	}
 
#endif

	//--------------------------------------------------------------------------------------------

	public class GamePad
	{
		public const int B1	= 0x0001 ;
		public const int B2	= 0x0002 ;
		public const int B3	= 0x0004 ;
		public const int B4	= 0x0008 ;

		public const int R1	= 0x0010 ;
		public const int L1	= 0x0020 ;
		public const int R2	= 0x0040 ;
		public const int L2	= 0x0080 ;
		public const int R3	= 0x0100 ;
		public const int L3	= 0x0200 ;

		public const int O1	= 0x0400 ;
		public const int O2	= 0x0800 ;
		public const int O3	= 0x1000 ;
		public const int O4	= 0x2000 ;

		private static Dictionary<int,int> m_ButtonNumberIndex = new Dictionary<int, int>()
		{
			{ B1,  0 },
			{ B2,  1 },
			{ B3,  2 },
			{ B4,  3 },

			{ R1,  4 },
			{ L1,  5 },
			{ R2,  6 },
			{ L2,  7 },
			{ R3,  8 },
			{ L3,  9 },

			{ O1, 10 },
			{ O2, 11 },
			{ O3, 12 },
			{ O4, 13 },
		} ;

		public class Profile
		{
			public int[]	buttonNumber = null ;
			public int[]	axisNumber = null ;

			public bool		analogButtonCorrection = false ;
			public float	analogButtonThreshold = 0.2f ;

			public Profile( int[] tButtonNumber, int[] tAxisNumber, bool tAnalogButtonCorrection, float tAnalogButtonThreshold )
			{
				buttonNumber			= tButtonNumber ;
				axisNumber				= tAxisNumber ;
				analogButtonCorrection	= tAnalogButtonCorrection ;
				analogButtonThreshold	= tAnalogButtonThreshold ;
			}
		}

		public static Profile profile_Xbox = new Profile
		(
			new int[]{  1,  0,  3,  2,  5,  4, -1, -1,  9,  8,  7,  6,	11, 10 },
			new int[]{  6,  7,  1,  2,  4,  5, 10,  9 },
			false, 0.4f
		) ;

		public static Profile profile_PlayStation = new Profile
		(
			new int[]{  2,  1,  3,  0,  5,  4,  7,  6, 11, 10,  9,  8,	13, 12 },
			new int[]{  7,  8,  1,  2,  3,  6,  5,  4 },
			true,  0.4f
		) ;

		// プロファイル情報(0～7)
		private static Profile[] m_Profile =
		{
			// Xbox
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
			profile_Xbox,
		} ;

		/// <summary>
		/// プロファィル情報を設定する
		/// </summary>
		/// <param name="tNumber"></param>
		/// <param name="tButtonNumber"></param>
		/// <param name="tAxisNumber"></param>
		/// <param name="tAnalogButtonCorrection"></param>
		/// <param name="tAnalogButtonThreshold"></param>
		/// <returns></returns>
		public static bool SetProfile( int tNumber, int[] tButtonNumber, int[] tAxisNumber, bool tAnalogButtonCorrection, float tAnalogButtonThreshold )
		{
			if( tNumber <  0 || tNumber >= m_Profile.Length )
			{
				return false ;
			}

			m_Profile[ tNumber ] = new Profile( tButtonNumber, tAxisNumber, tAnalogButtonCorrection, tAnalogButtonThreshold ) ;
			return true ;
		}

		/// <summary>
		/// プロファイル情報を設定する
		/// </summary>
		/// <param name="tNumber"></param>
		/// <param name="tProfile"></param>
		/// <returns></returns>
		public static bool SetProfile( int tNumber, Profile tProfile )
		{
			if( tNumber <  0 || tNumber >= m_Profile.Length )
			{
				return false ;
			}

			m_Profile[ tNumber ] = tProfile ;
			return true ;

		}

		// プレイヤー番号に対するデフォルトのプロフィール番号
		private static int[] m_DefaultProfileNumber =
		{
			-1, -1, -1, -1
		} ;

		/// <summary>
		/// プレイヤー番号に対するデフォルトのプロフィール番号を設定する
		/// </summary>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool SetDefaultProfileNumber( int tPlayerNumber, int tProfileNumber )
		{
			if( tPlayerNumber <  0 || tPlayerNumber >= m_DefaultProfileNumber.Length || tProfileNumber <  0 || tProfileNumber >= m_Profile.Length )
			{
				return false ;
			}

			m_DefaultProfileNumber[ tPlayerNumber ] = tProfileNumber ;

			return true ;
		}

		//-------------------------------------------------------------------------------------------

		private static string[][]	m_ButtonName =
		{
//			new string[]
//			{
//				"Button_00", "Button_01", "Button_02","Button_03", "Button_04", "Button_05", "Button_06", "Button_07", "Button_08","Button_09", "Button_10", "Button_11", "Button_12", "Button_13", "Button_14","Button_15"
//			},
			new string[]
			{
				"Player_1_Button_00", "Player_1_Button_01", "Player_1_Button_02","Player_1_Button_03", "Player_1_Button_04", "Player_1_Button_05", "Player_1_Button_06", "Player_1_Button_07", "Player_1_Button_08","Player_1_Button_09", "Player_1_Button_10", "Player_1_Button_11", "Player_1_Button_12", "Player_1_Button_13", "Player_1_Button_14","Player_1_Button_15"
			},
			new string[]
			{
				"Player_2_Button_00", "Player_2_Button_01", "Player_2_Button_02","Player_2_Button_03", "Player_2_Button_04", "Player_2_Button_05", "Player_2_Button_06", "Player_2_Button_07", "Player_2_Button_08","Player_2_Button_09", "Player_2_Button_10", "Player_2_Button_11", "Player_2_Button_12", "Player_2_Button_13", "Player_2_Button_14","Player_2_Button_15"
			},
			new string[]
			{
				"Player_3_Button_00", "Player_3_Button_01", "Player_3_Button_02","Player_3_Button_03", "Player_3_Button_04", "Player_3_Button_05", "Player_3_Button_06", "Player_3_Button_07", "Player_3_Button_08","Player_3_Button_09", "Player_3_Button_10", "Player_3_Button_11", "Player_3_Button_12", "Player_3_Button_13", "Player_3_Button_14","Player_3_Button_15"
			},
			new string[]
			{
				"Player_4_Button_00", "Player_4_Button_01", "Player_4_Button_02","Player_4_Button_03", "Player_4_Button_04", "Player_4_Button_05", "Player_4_Button_06", "Player_4_Button_07", "Player_4_Button_08","Player_4_Button_09", "Player_4_Button_10", "Player_4_Button_11", "Player_4_Button_12", "Player_4_Button_13", "Player_4_Button_14","Player_4_Button_15"
			},
		} ;

		private static string[][]	m_AxisName =
		{
//			new string[]
//			{
//				"", "Axis_01", "Axis_02","Axis_03", "Axis_04", "Axis_05", "Axis_06", "Axis_07", "Axis_08","Axis_09", "Axis_10", "Axis_11", "Axis_12"
//			},
			new string[]
			{
				"", "Player_1_Axis_01", "Player_1_Axis_02","Player_1_Axis_03", "Player_1_Axis_04", "Player_1_Axis_05", "Player_1_Axis_06", "Player_1_Axis_07", "Player_1_Axis_08","Player_1_Axis_09", "Player_1_Axis_10", "Player_1_Axis_11", "Player_1_Axis_12"
			},
			new string[]
			{
				"", "Player_2_Axis_01", "Player_2_Axis_02","Player_2_Axis_03", "Player_2_Axis_04", "Player_2_Axis_05", "Player_2_Axis_06", "Player_2_Axis_07", "Player_2_Axis_08","Player_2_Axis_09", "Player_2_Axis_10", "Player_2_Axis_11", "Player_2_Axis_12"
			},
			new string[]
			{
				"", "Player_3_Axis_01", "Player_3_Axis_02","Player_3_Axis_03", "Player_3_Axis_04", "Player_3_Axis_05", "Player_3_Axis_06", "Player_3_Axis_07", "Player_3_Axis_08","Player_3_Axis_09", "Player_3_Axis_10", "Player_3_Axis_11", "Player_3_Axis_12"
			},
			new string[]
			{
				"", "Player_4_Axis_01", "Player_4_Axis_02","Player_4_Axis_03", "Player_4_Axis_04", "Player_4_Axis_05", "Player_4_Axis_06", "Player_4_Axis_07", "Player_4_Axis_08","Player_4_Axis_09", "Player_4_Axis_10", "Player_4_Axis_11", "Player_4_Axis_12"
			},
		} ;

		public static float RepeatStartingTime = 0.5f ;
		public static float RepeatIntervalTime = 0.05f ;
		public static float EnableThreshold = 0.75f ;
		
		public static int	layer ;

		public static void SetLayer( int tLayer )
		{
			if( tLayer <  0 )
			{
				tLayer  = 0 ;
			}

			layer = tLayer ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定したレイヤーの全てのボタンの状態を取得する
		/// </summary>
		/// <param name="tLayer"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static int GetButtonAllOfLayer( int tLayer, int tPlayerNumber = 1, int tProfileNumber = 1 )
		{
			if( tLayer >= 0 && tLayer <  layer )
			{
				// 無効
				return 0 ;
			}

			return GetButtonAll( tPlayerNumber, tProfileNumber ) ;
		}

		/// <summary>
		/// 全てのボタンの状態を取得する
		/// </summary>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static int GetButtonAll( int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return 0 ;
			}

			//----------------------------------

			int tButton = 0 ;

			//----------------------------------

			if( tPlayerNumber <  -1 || tPlayerNumber >= m_DefaultProfileNumber.Length )
			{
				tPlayerNumber  = -1 ;
			}

			if( tPlayerNumber <  0 )
			{
				// Key
				if( Input.GetKey( KeyCode.Z ) == true )
				{
					tButton |= B1 ;
				}
				if( Input.GetKey( KeyCode.X ) == true )
				{
					tButton |= B2 ;
				}
				if( Input.GetKey( KeyCode.C ) == true )
				{
					tButton |= B3 ;
				}
				if( Input.GetKey( KeyCode.V ) == true )
				{
					tButton |= B4 ;
				}

				if( Input.GetKey( KeyCode.E ) == true )
				{
					tButton |= R1 ;
				}
				if( Input.GetKey( KeyCode.Q ) == true )
				{
					tButton |= L1 ;
				}

				if( Input.GetKey( KeyCode.RightShift ) == true )
				{
					tButton |= R2 ;
				}
				if( Input.GetKey( KeyCode.LeftShift ) == true )
				{
					tButton |= L2 ;
				}

				if( Input.GetKey( KeyCode.RightControl ) == true )
				{
					tButton |= R3 ;
				}
				if( Input.GetKey( KeyCode.LeftControl ) == true )
				{
					tButton |= L3 ;
				}
				if( Input.GetKey( KeyCode.Return ) == true )
				{
					tButton |= O1 ;
				}
				if( Input.GetKey( KeyCode.Escape ) == true )
				{
					tButton |= O2 ;
				}
				if( Input.GetKey( KeyCode.Space ) == true )
				{
					tButton |= O3 ;
				}
				if( Input.GetKey( KeyCode.Backspace ) == true )
				{
					tButton |= O4 ;
				}
			}
			
			int p, ps, pe, q ;

			if( tPlayerNumber <  0 )
			{
				ps = 0 ;
				pe = 3 ;
			}
			else
			{
				ps = tPlayerNumber ;
				pe = tPlayerNumber ;
			}

			for( p  = ps ; p <= pe ; p ++ )
			{
				// 0～
				if( tProfileNumber >= 0 && tProfileNumber <  m_Profile.Length )
				{
					q = tProfileNumber ;
				}
				else
				{
					q = m_DefaultProfileNumber[ p ] ;
				}

				if( q >= 0 && q <  m_Profile.Length )
				{
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  0 ] ] ) == true )
					{
						tButton |= B1 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  1 ] ] ) == true )
					{
						tButton |= B2 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  2 ] ] ) == true )
					{
						tButton |= B3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  3 ] ] ) == true )
					{
						tButton |= B4 ;
					}

					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  4 ] ] ) == true )
					{
						tButton |= R1 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  5 ] ] ) == true )
					{
						tButton |= L1 ;
					}

					if( m_Profile[ q ].buttonNumber[  6 ] >= 0 )
					{
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  6 ] ] ) == true )
						{
							tButton |= R2 ;
						}
					}
					else
					{
						float tAxis = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  6 ] ] ) ;
						if( m_Profile[ q ].analogButtonCorrection == true )
						{
							tAxis = tAxis * 0.5f + 0.5f ;
						}
						if( tAxis >= m_Profile[ q ].analogButtonThreshold )
						{
							tButton |= R2 ;
						}
					}

					if( m_Profile[ q ].buttonNumber[  7 ] >= 0 )
					{
						if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  7 ] ] ) == true )
						{
							tButton |= L2 ;
						}
					}
					else
					{
						float tAxis = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  7 ] ] ) ;
						if( m_Profile[ q ].analogButtonCorrection == true )
						{
							tAxis = tAxis * 0.5f + 0.5f ;
						}
						if( tAxis >= m_Profile[ q ].analogButtonThreshold )
						{
							tButton |= L2 ;
						}
					}

					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  8 ] ] ) == true )
					{
						tButton |= R3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[  9 ] ] ) == true )
					{
						tButton |= L3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[ 10 ] ] ) == true )
					{
						tButton |= O1 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[ 11 ] ] ) == true )
					{
						tButton |= O2 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[ 12 ] ] ) == true )
					{
						tButton |= O3 ;
					}
					if( Input.GetButton( m_ButtonName[ p ][ m_Profile[ q ].buttonNumber[ 13 ] ] ) == true )
					{
						tButton |= O4 ;
					}
				}
			}

			return tButton ;
		}

		/// <summary>
		/// 指定したレイヤーのボタンの状態を取得する
		/// </summary>
		/// <param name="tLayer"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool GetButtonOfLayer( int tButtonNumber, int tLayer, int tPlayerNumber = 1, int tProfileNumber = 1 )
		{
			if( tLayer >= 0 && tLayer <  layer )
			{
				// 無効
				return false ;
			}

			return GetButton( tButtonNumber, tPlayerNumber, tProfileNumber ) ;
		}

		/// <summary>
		/// ボタンの状態を取得する
		/// </summary>
		/// <param name="tButtonNumber"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool GetButton( int tButtonNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int tButton = GetButtonAll( tPlayerNumber, tProfileNumber ) ;

			if( ( tButton & tButtonNumber ) == 0 )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// 指定したレイヤーのアクシズの状態を取得する
		/// </summary>
		/// <param name="tLayer"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static Vector2 GetAxisOfLayer( int tAxisNumber, int tLayer, int tPlayerNumber = 1, int tProfileNumber = 1 )
		{
			if( tLayer >= 0 && tLayer <  layer )
			{
				// 無効
				return Vector2.zero ;
			}

			return GetAxis( tAxisNumber, tPlayerNumber, tProfileNumber ) ;
		}

		/// <summary>
		/// アクシズの状態を取得する
		/// </summary>
		/// <param name="tAxisNumber"></param>
		/// <param name="oAxisX"></param>
		/// <param name="oAxisY"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		public static Vector2 GetAxis( int tAxisNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			float oAxisX = 0 ;
			float oAxisY = 0 ;

			if( tPlayerNumber <  -1 || tPlayerNumber >= m_DefaultProfileNumber.Length )
			{
				tPlayerNumber  = -1 ;
			}
			
			if( tPlayerNumber <  0 )
			{
				// Key

				if( tAxisNumber == 0 )
				{
					// SCX
					if( Input.GetKey( KeyCode.D ) == true || Input.GetKey( KeyCode.RightArrow ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.A ) == true || Input.GetKey( KeyCode.LeftArrow ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.W ) == true || Input.GetKey( KeyCode.UpArrow ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.S ) == true || Input.GetKey( KeyCode.DownArrow ) == true )
					{
						oAxisY += 1 ;
					}
				}

				if( tAxisNumber == 1 )
				{
					// SCX
					if( Input.GetKey( KeyCode.D ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.A ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.W ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.S ) == true )
					{
						oAxisY += 1 ;
					}
				}

				if( tAxisNumber == 2 )
				{
					// SCX
					if( Input.GetKey( KeyCode.RightArrow ) == true )
					{
						oAxisX += 1 ;
					}
					if( Input.GetKey( KeyCode.LeftArrow ) == true )
					{
						oAxisX -= 1 ;
					}

					// SCY
					if( Input.GetKey( KeyCode.UpArrow ) == true )
					{
						oAxisY -= 1 ;
					}
					if( Input.GetKey( KeyCode.DownArrow ) == true )
					{
						oAxisY += 1 ;
					}
				}
			}
			
			int p, ps, pe, q ;

			if( tPlayerNumber <  0 )
			{
				ps = 0 ;
				pe = 3 ;
			}
			else
			{
				ps = tPlayerNumber ;
				pe = tPlayerNumber ;
			}

			for( p  = ps ; p <= pe ; p ++ )
			{
				// 0～
				if( tProfileNumber >= 0 && tProfileNumber <  m_Profile.Length )
				{
					q = tProfileNumber ;
				}
				else
				{
					q = m_DefaultProfileNumber[ p ] ;
				}

				if( q >= 0 && q <  m_Profile.Length )
				{
					if( tAxisNumber == 0 )
					{
						// SCX
						float tAxisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  0 ] ] ) ;
						if( tAxisX != 0 )
						{
							oAxisX = tAxisX ;
						}

						// SCY
						float tAxisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  1 ] ] ) ;
						if( tAxisY != 0 )
						{
							oAxisY = - tAxisY ;
						}
					}
					else
					if( tAxisNumber == 1 )
					{
						// SLX
						float tAxisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  2 ] ] ) ;
						if( tAxisX != 0 )
						{
							oAxisX = tAxisX ;
						}

						// SLY
						float tAxisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  3 ] ] ) ;
						if( tAxisY != 0 )
						{
							oAxisY = tAxisY ;
						}
					}
					else
					if( tAxisNumber == 2 )
					{
						// SRX
						float tAxisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  4 ] ] ) ;
						if( tAxisX != 0 )
						{
							oAxisX = tAxisX ;
						}

						// SRY
						float tAxisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  5 ] ] ) ;
						if( tAxisY != 0 )
						{
							oAxisY = tAxisY ;
						}
					}
					else
					if( tAxisNumber == 3 )
					{
						// R2
						float tAxisX = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  6 ] ] ) ;
						if( m_Profile[ q ].analogButtonCorrection == true )
						{
							tAxisX = tAxisX * 0.5f + 0.5f ;
						}
						if( tAxisX != 0 )
						{
							oAxisX = tAxisX ;
						}

						// L2
						float tAxisY = Input.GetAxis( m_AxisName[ p ][ m_Profile[ q ].axisNumber[  7 ] ] ) ;
						if( m_Profile[ q ].analogButtonCorrection == true )
						{
							tAxisY = tAxisY * 0.5f + 0.5f ;
						}
						if( tAxisY != 0 )
						{
							oAxisY = tAxisY ;
						}
					}
				}
			}

			// 縦軸の符号反転
			if( InputManager.invert == true )
			{
				oAxisY = - oAxisY ;
			}

			return new Vector2( oAxisX, oAxisY ) ;
		}

		/// <summary>
		/// 接続中のゲームパッドの名前を取得する
		/// </summary>
		/// <returns></returns>
		public static string[] GetNames()
		{
			return Input.GetJoystickNames() ;
		}




		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// Smart 系の状態更新
		/// </summary>
		public static void Update()
		{
			int i, l, j, m, k ;

			int[] tPlayerNumber  = {  0,  1,  2,  3, -1 } ;
			int[] tProfileNumber = {  0,  1,  2,  3, -1 } ;

			int[] tButtonNumber =
			{
				B1,
				B2,
				B3,
				B4,

				R1,
				L1,
				R2,
				L2,
				R3,
				L3,

				O1,
				O2,
				O3,
				O4,
			} ;

			int[] tAxisNumber =
			{
				0,
				1,
				2,
				3,
			} ;


			l = tPlayerNumber.Length ;

			int tButton ;
			Vector2 tAxis ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				// Button

				tButton = GetButtonAll( tPlayerNumber[ i ], tProfileNumber[ i ] ) ;

				m = tButtonNumber.Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					m_ButtonHoldState[ i, j ] = false ;
					m_ButtonOnceState[ i, j ] = false ;

					k = m_ButtonNumberIndex[ tButtonNumber[ j ] ] ;
						
					if( ( tButton & tButtonNumber[ j ] ) != 0 )
					{
						if( m_ButtonHoldKeepFlag[ i, k ] == false )
						{
							// ホールド開始
							m_ButtonHoldState[ i, k ] = true ;

							m_ButtonHoldKeepFlag[ i, k ] = true ;
							m_ButtonHoldWakeTime[ i, k ] = Time.realtimeSinceStartup ;
							m_ButtonHoldLoopTime[ i, k ] = Time.realtimeSinceStartup ;

							m_ButtonOnceState[ i, k ] = true ;
						}
						else
						{
							// ホールド最中
							if( ( Time.realtimeSinceStartup - m_ButtonHoldWakeTime[ i, k ] ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( Time.realtimeSinceStartup - m_ButtonHoldLoopTime[ i, k ] ) >= RepeatIntervalTime )
								{
									m_ButtonHoldState[ i, k ] = true ;

									m_ButtonHoldLoopTime[ i, k ] = Time.realtimeSinceStartup ;
								}
							}
						}
					}
					else
					{
						// ホールド解除
						m_ButtonHoldKeepFlag[ i, k ] = false ;
					}
				}

				// Axis
				m = tAxisNumber.Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					tAxis = GetAxis( tAxisNumber[ j ], tPlayerNumber[ i ], tProfileNumber[ i ] ) ;

					k = tAxisNumber[ j ] ;

					if( tAxis.x >=     EnableThreshold   )
					{
						tAxis.x  =  1 ;
					}
					else
					if( tAxis.x <= ( - EnableThreshold ) )
					{
						tAxis.x  = -1 ;
					}
					else
					{
						tAxis.x  =  0 ;
					}

					if( tAxis.y >=     EnableThreshold   )
					{
						tAxis.y  =  1 ;
					}
					else
					if( tAxis.y <= ( - EnableThreshold ) )
					{
						tAxis.y  = -1 ;
					}
					else
					{
						tAxis.y  =  0 ;
					}

					m_AxisHoldState[ i, k ] = Vector2.zero ;
					m_AxisOnceState[ i, k ] = Vector2.zero ;

					if( tAxis.x != 0 || tAxis.y != 0 )
					{
						if( m_AxisHoldKeepFlag[ i, k ] == false )
						{
							// ホールド開始
							m_AxisHoldState[ i, k ] = tAxis ;

							m_AxisHoldKeepFlag[ i, k ] = true ;
							m_AxisHoldWakeTime[ i, k ] = Time.realtimeSinceStartup ;
							m_AxisHoldLoopTime[ i, k ] = Time.realtimeSinceStartup ;

							m_AxisOnceState[ i, k ] = tAxis ;
						}
						else
						{
							// ホールド最中
							if( ( Time.realtimeSinceStartup - m_AxisHoldWakeTime[ i, k ] ) >= RepeatStartingTime )
							{
								// リピート中
								if( ( Time.realtimeSinceStartup - m_AxisHoldLoopTime[ i, k ] ) >= RepeatIntervalTime )
								{
									m_AxisHoldState[ i, k ] = tAxis ;
	
									m_AxisHoldLoopTime[ i, k ] = Time.realtimeSinceStartup ;
								}
							}
						}
					}
					else
					{
						// ホールド解除
						m_AxisHoldKeepFlag[ i, k ] = false ;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		
		private static bool[,]	m_ButtonHoldKeepFlag	= new  bool[ 5, 16 ] ;
		private static float[,]	m_ButtonHoldWakeTime	= new float[ 5, 16 ] ;
		private static float[,]	m_ButtonHoldLoopTime	= new float[ 5, 16 ] ;
		private static bool[,]	m_ButtonHoldState		= new  bool[ 5, 16 ] ;
		private static bool[,]	m_ButtonOnceState		= new  bool[ 5, 16 ] ;
		

		/// <summary>
		/// 指定したレイヤーのリピート機能付きでボタンの押下状態を取得する
		/// </summary>
		/// <param name="tButtonNumber"></param>
		/// <param name="tLayer"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool GetSmartButtonOfLayer( int tButtonNumber, int tLayer, int tPlayerNumber = 1, int tProfileNumber = 1 )
		{
			if( tLayer >= 0 && tLayer <  layer )
			{
				// 無効
				return false ;
			}

			return GetSmartButton( tButtonNumber, tPlayerNumber, tProfileNumber ) ;
		}
		
		/// <summary>
		/// リピート機能付きでボタンの押下状態を取得する
		/// </summary>
		/// <param name="tButtonNumber"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool GetSmartButton( int tButtonNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int i, k ;

			if( tPlayerNumber >= 0 && tPlayerNumber <  m_DefaultProfileNumber.Length )
			{
				i = tPlayerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = m_ButtonNumberIndex[ tButtonNumber ] ;

			return m_ButtonHoldState[ i, k ] ;
		}

		/// <summary>
		/// 一度だけ反応するボタンの押下状態を取得する
		/// </summary>
		/// <param name="tButtonNumber"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static bool GetOnceButton( int tButtonNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return false ;
			}

			//----------------------------------

			int i, k ;

			if( tPlayerNumber >= 0 && tPlayerNumber <  m_DefaultProfileNumber.Length )
			{
				i = tPlayerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = m_ButtonNumberIndex[ tButtonNumber ] ;

			return m_ButtonOnceState[ i, k ] ;
		}

		//-------------------------------------------------------------------------------------------

		private static bool[,]		m_AxisHoldKeepFlag = new  bool[ 5, 4 ] ;
		private static float[,]		m_AxisHoldWakeTime = new float[ 5, 4 ] ;
		private static float[,]		m_AxisHoldLoopTime = new float[ 5, 4 ] ;
		private static Vector2[,]	m_AxisHoldState = new Vector2[ 5, 4 ] ;
		private static Vector2[,]	m_AxisOnceState = new Vector2[ 5, 4 ] ;

		/// <summary>
		/// 指定したレイヤーのリピート機能付きでアクシズの状態を取得する
		/// </summary>
		/// <param name="tButtonNumber"></param>
		/// <param name="tLayer"></param>
		/// <param name="tPlayerNumber"></param>
		/// <param name="tProfileNumber"></param>
		/// <returns></returns>
		public static Vector2 GetSmartAxisOfLayer( int tAxisNumber, int tLayer, int tPlayerNumber = 1, int tProfileNumber = 1 )
		{
			if( tLayer >= 0 && tLayer <  layer )
			{
				// 無効
				return Vector2.zero ;
			}

			return GetSmartAxis( tAxisNumber, tPlayerNumber, tProfileNumber ) ;
		}

		/// <summary>
		/// リピート機能付きでアクシズの状態を取得する
		/// </summary>
		/// <param name="tAxisX"></param>
		/// <param name="tAxisY"></param>
		/// <param name="tPlayerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetSmartAxis( int tAxisNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			int i, k ;

			if( tPlayerNumber >= 0 && tPlayerNumber <  m_DefaultProfileNumber.Length )
			{
				i = tPlayerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = tAxisNumber ;

			return m_AxisHoldState[ i, k ] ;
		}

		/// <summary>
		/// 一度だけ反応するアクシズの状態を取得する
		/// </summary>
		/// <param name="tAxisX"></param>
		/// <param name="tAxisY"></param>
		/// <param name="tPlayerNumber"></param>
		/// <returns></returns>
		public static Vector2 GetOnceAxis( int tAxisNumber, int tPlayerNumber = -1, int tProfileNumber = -1 )
		{
			// modeEnabled を判定条件に入れないのは、マウスとキーボードを同時入力するケースを考慮するため
			if( InputManager.instance == null || InputManager.isEnabled == false )
			{
				return Vector2.zero ;
			}

			//----------------------------------

			int i, k ;

			if( tPlayerNumber >= 0 && tPlayerNumber <  m_DefaultProfileNumber.Length )
			{
				i = tPlayerNumber ;
			}
			else
			{
				i  = 4 ;
			}

			k = tAxisNumber ;

			return m_AxisOnceState[ i, k ] ;
		}

	}

	public static class Mouse
	{
		private static bool[]	m_Press = new bool[ 3 ] ;
		private static int[]	m_State = new int[ 3 ] ;
		private static float[]	m_Timer = new float[ 3 ] ;

		public static float		RepeatStartingTime = 0.5f ;
		public static float		RepeatIntervalTime = 0.05f ;

		public static void Update()
		{
			int i, l = 3 ;
			float t ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( Input.GetMouseButton( i ) == true )
				{
					if( m_State[ i ] == 0 )
					{
						m_Press[ i ] = true ;
						m_State[ i ] = 1 ;
						m_Timer[ i ] = Time.realtimeSinceStartup ;
					}
					else
					if( m_State[ i ] == 1 )
					{
						t = Time.realtimeSinceStartup - m_Timer[ i ] ;

						if( t <  RepeatStartingTime )
						{
							m_Press[ i ] = false ;
						}
						else
						{
							m_Press[ i ] = true ;
							m_State[ i ] = 2 ;
							m_Timer[ i ] = Time.realtimeSinceStartup ;
						}
					}
					else
					if( m_State[ i ] == 2 )
					{
						t = Time.realtimeSinceStartup - m_Timer[ i ] ;

						if( t <  RepeatIntervalTime )
						{
							m_Press[ i ] = false ;
						}
						else
						{
							m_Press[ i ] = true ;
							m_State[ i ] = 2 ;
							m_Timer[ i ] = Time.realtimeSinceStartup ;
						}
					}
				}
				else
				{
					m_Press[ i ] = false ;
					m_State[ i ] = 0 ;
					m_Timer[ i ] = 0 ;
				}
			}
		}

		/// <summary>
		/// リピート付きでマウスボタンの状態を取得する
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetSmartButton( int index )
		{
			if( index <  0 || index >= m_Press.Length )
			{
				return false ;
			}

			return m_Press[ index ] ;
		}
	}

}

