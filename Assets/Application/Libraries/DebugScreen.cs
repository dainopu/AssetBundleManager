using UnityEngine;
using System.Collections;

// スタティックにしたい場合
[ ExecuteInEditMode ]

// 画面へのデバッグ表示用
public class DebugScreen : MonoBehaviour
{
	// マルチスレッド用
	private static DebugScreen m_Instance = null ;

	public GUISkin Skin ;
	public ArrayList TextArray = new ArrayList() ;
	public string Text = "" ;
	public bool Enable = true ;
	private bool View = true ;
	public int Max = 30 ; 
	
	public static uint textColor = 0xFFFFFFFF ;
	public static int fontSize = 16 ;
	public static bool wordWarp = true ;
	
	public bool quitOnAndroid = false ;
	
	
	void Start()
	{
		Max = ( int )( ( Screen.height * 0.5f ) / 16 ) + 30 ;
	}
	
	void Awake()
	{
		DontDestroyOnLoad( this ) ;

		if( TextArray != null )
		{
			TextArray.Clear() ;
		}
	}
	
	void Update()
	{	
		// Androidで戻るキーでアプリを終了させる
		if( quitOnAndroid == true )
		{
			if( Application.platform == RuntimePlatform.Android && Input.GetKey( KeyCode.Escape ) )
			{
				Application.Quit() ;
			}
		}
	}
	
	// ＧＵＩ描画
	void OnGUI()
	{
		if( Enable == true )
		{
			if( Skin != null )
			{
				if( SystemInfo.operatingSystem.Contains( "Android" ) == true )
				{
					GUI.skin = Skin ;
				}
			}
			
			int sw = Screen.width ;
			int sh = Screen.height ;
			int fb = 0 ;
			if( sw >  sh )
			{
				fb = sw ;
			}
			else
			{
				fb = sh ;
			}
			
			
			GUIStyle style = new GUIStyle() ;
			style.fontSize = ( int )( fontSize * fb / 640 ) ;
			style.normal.textColor = new Color
			(
				( float )( ( textColor >> 16 ) & 0xFF ) / 255.0f,
				( float )( ( textColor >>  8 ) & 0xFF ) / 255.0f,
				( float )( ( textColor >>  0 ) & 0xFF ) / 255.0f,
				( float )( ( textColor >> 24 ) & 0xFF ) / 255.0f
			) ;
			style.wordWrap = wordWarp ;
			
			if( View == true )
			{
				GUI.Label( new Rect(   0,   0, Screen.width, Screen.height * 0.5f ), Text, style ) ;
				
//				GUI.TextArea(  new Rect(   0,   0, Screen.width, Screen.height ), Text ) ; 
				
				
				
//	   			GUI.BeginScrollView( new Rect ( 0, 0, 160, 320 ), new Vector2(), new Rect(   0,   0, 160, 320 ) ) ;
//				GUI.TextArea( new Rect(   0,   0, 320, 640 ), Text ) ;
//				GUI.EndScrollView() ;
			}
			
			int w = Screen.width ;
			int h = Screen.height ;
			int s = w ;
			if( h <  w )
			{
				s  = h ;
			}
			
			w = s / 4 ;
			h = w / 2 ;
			
			GUIStyle tButtonStyle = new GUIStyle( "button" ) ;
			tButtonStyle.fontSize = ( int )( fontSize * fb / 640 ) / 1 ;
			
			
			if( GUI.Button( new Rect( Screen.width - w,   0, w, h ), "CLEAR", tButtonStyle ) == true )
			{
				TextArray.Clear() ;
				Text = "" ;
			}
			
			string ln ;
			if( View == true )
			{
				ln = "HIDE" ;
			}
			else
			{
				ln = "SHOW" ;
			}
			
			if( GUI.Button( new Rect( Screen.width - w,  h * 1.5f, w, h ), ln, tButtonStyle ) == true )
			{
				if( View == true )
				{
					View  = false ;
				}
				else
				{
					View  = true ;
				}
			}
			
			
			int mu ;
			int ll ;
			string hs ;
			
/*			mu = ( int )Profiler.usedHeapSize ;
			hs = "" + ( mu / 1048576 ) ;
			ll = 4 - hs.Length ;
			hs = hs + "."+( ( mu % 1048576 ) * 10 / 1048576 ) + " / " + SystemInfo.systemMemorySize + " MB" ;
			GUI.Label( new Rect( Screen.width * 0.5f + ll * 7, Screen.height - fontSize * 2, Screen.width * 0.5f, 12 ), hs, style ) ;
*/		
//			System.GC.Collect() ;

			
			mu = ( int )System.GC.GetTotalMemory( false ) ;
			hs = "" + ( mu / 1048576 ) ;
			ll = 4 - hs.Length ;
			hs = hs + "."+( ( mu % 1048576 ) * 10 / 1048576 ) + " / " + SystemInfo.systemMemorySize + " MB" ;
			GUI.Label( new Rect( Screen.width * 0.5f + ll * 7, Screen.height - fontSize * 1, Screen.width * 0.5f, 12 ), hs, style ) ;
		}
	}
	
	public void addText( string s )
	{
		TextArray.Add( s + "\n" ) ;
		
		int i ;
		
		int i1 = TextArray.Count - 1 ;
		int i0 = i1 - Max ;
		if( i0 <  0 )
		{
			i0  = 0 ;
		}
		
		Text = "" ;
		for( i  = i0 ; i <= i1 ; i ++  )
		{
			Text = Text + ( TextArray[ i ] as string ) ;
		}
		
		if( i1 >  Max )
		{
			TextArray.Remove( 0 ) ;
		}
	}
	
	// デバッグスクリーンに文字列を追加する
	public static void Out( string s )
	{
//		if( mVisible == false )
//		{
//			return ;
//		}
		
		DebugScreen tDebug ;
		
		if( m_Instance == null )
		{
			tDebug = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
			if( tDebug == null )
			{
//				GameObject tGameObject = new GameObject( "DebugOut Screen" ) ;
//				tDebug = tGameObject.AddComponent<DebugScreen>() ;
				return ;
			}
		}
		else
		{
			tDebug = m_Instance ;
		}

		tDebug.addText( s ) ;
	}
	
	public static void SetTextColor( uint tTextColor )
	{
		textColor = tTextColor ;
	}
	
	public static void SetFontSize( int tFontSize )
	{
		fontSize = tFontSize ;
	}
	
	public static void Create()
	{
		Create( textColor, fontSize ) ;
	}
	
	public static void Create( uint tTextColor )
	{
		Create( tTextColor, fontSize ) ;
	}
	
	public static void Create( uint tTextColor, int tFontSize )
	{
		textColor = tTextColor ;
		fontSize  = tFontSize ;
		
//		if( mVisible == false )
//		{
//			return ;
//		}
		
		DebugScreen tDebug = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
		if( tDebug == null )
		{
			GameObject tGameObject = new GameObject( "DebugScreen" ) ;
			tDebug = tGameObject.AddComponent<DebugScreen>() ;
		}

		m_Instance = tDebug ;
	}
	
	public static void Terminate()
	{
		DebugScreen tDebug = ( DebugScreen )GameObject.FindObjectOfType( typeof( DebugScreen ) ) ;
		if( tDebug != null )
		{
			Destroy( tDebug.gameObject ) ;
		}
	}

	void OnDestroy()
	{
		if( this == m_Instance )
		{
			m_Instance = null ;
		}		
	}

	//	private static bool mVisible = false ;

	public static void SetVisible( bool tVisible )
	{
//		mVisible = tVisible ;
//		if( tVisible == false )
//		{
//			Terminate() ;
//		}
	}
}
