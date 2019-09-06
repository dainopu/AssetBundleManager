using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;


// Version 2016/12/15 0 

namespace TransformHelper
{
	[ ExecuteInEditMode ]
	[RequireComponent(typeof(Transform))]

	/// <summary>
	/// トランスフォームをもっと楽に扱うためのコンポーネント(メソッド拡張は他人とかち合う可能性があるので使わない)
	/// </summary>
	public class SoftTransform : MonoBehaviour
	{
		/// <summary>
		/// 識別子
		/// </summary>
		public string	identity ;

		void Awake()
		{
			// Tween などで使用する基準情報を保存する(コンポーネントの実行順が不確定なので Awake で必ず実行すること)
			SetLocalState() ;
		}

		/// <summary>
		/// 回転をリセットする
		/// </summary>
		public void ResetRotation()
		{
			transform.localRotation = new Quaternion() ;
			localRotation =  transform.localEulerAngles ;
		}

		/// <summary>
		/// 縮尺をリセットする
		/// </summary>
		public void ResetScale()
		{
			transform.localScale = Vector3.one ;
			localScale    =  transform.localScale ;
		}

		// Tween などで使用する基準情報を保存する
		protected void SetLocalState()
		{
			localPosition =  transform.localPosition ;
			localRotation =  transform.localEulerAngles ;
			localScale    =  transform.localScale ;
		}
		
		[SerializeField][HideInInspector]
		private Vector3 m_LocalPosition = Vector3.zero ;
		public  Vector3   localPosition
		{
			get
			{
				return m_LocalPosition ;
			}
			set
			{
				m_LocalPosition = value ;
			}
		}

		public Vector3 position
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalPosition ;
				}
				else
				{
					return transform.localPosition ;
				}
			}
			set
			{
				m_LocalPosition = value ;
				transform.localPosition = value ;
			}
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( Vector2 tPosition )
		{
			position = new Vector3( tPosition.x, tPosition.y, position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( Vector3 tPosition )
		{
			position = new Vector3( tPosition.x, tPosition.y, tPosition.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( float x, float y )
		{
			position = new Vector3( x, y, position.z ) ;
		}

		/// <summary>
		/// ロケーションを設定
		/// </summary>
		/// <param name="tPosition"></param>
		public void SetPosition( float x, float y, float z )
		{
			position = new Vector3( x, y, z ) ;
		}
		/// <summary>
		/// Ｘ座標(ショートカット)
		/// </summary>
		public float _x
		{
			get
			{
				return position.x ;
			}
			set
			{
				if( position.x != value )
				{
					position = new Vector3( value, position.y, position.z ) ;
				}
			}
		}

		/// <summary>
		/// Ｘ座標(ショートカット)
		/// </summary>
		public float _X
		{
			get
			{
				return transform.localPosition.x ;
			}
			set
			{
				transform.localPosition = new Vector3( value, transform.localPosition.y, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}
		
		/// <summary>
		/// Ｙ座標(ショートカット)
		/// </summary>
		public float _y
		{
			get
			{
				return position.y ;
			}
			set
			{
				if( position.y != value )
				{
					position = new Vector3( position.x, value, position.z ) ;
				}
			}
		}

		/// <summary>
		/// Ｙ座標(ショートカット)
		/// </summary>
		public float _Y
		{
			get
			{
				return transform.localPosition.y ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, value, transform.localPosition.z ) ;
				m_LocalPosition = transform.localPosition;
			}
		}

		/// <summary>
		/// Ｚ座標(ショートカット)
		/// </summary>
		public float _z
		{
			get
			{
				return position.z ;
			}
			set
			{
				if( position.z != value )
				{
					position = new Vector3( position.x, position.y, value ) ;
				}
			}
		}

		public float _Z
		{
			get
			{
				return transform.localPosition.z ;
			}
			set
			{
				transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, value ) ;
				m_LocalPosition = transform.localPosition ;
			}
		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalRotation = Vector3.zero ;
		public  Vector3   localRotation
		{
			get
			{
				return m_LocalRotation ;
			}
			set
			{
				m_LocalRotation = value ;
			}
		}

/*		public Quaternion rotation
		{
			get
			{
				return transform.localRotation ;
			}
			set
			{
				transform.localRotation = value ;
			}
		}*/

		public Vector3 rotation
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					return transform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;
				transform.localEulerAngles = value ;
			}
		}

		public Vector3 eulerAngles
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalRotation ;
				}
				else
				{
					return transform.localEulerAngles ;
				}
			}
			set
			{
				m_LocalRotation = value ;
				transform.localEulerAngles = value ;
			}
		}

		public float _pitch
		{
			get
			{
				return eulerAngles.x ;
			}
			set
			{
				if( eulerAngles.x != value )
				{
					eulerAngles = new Vector3( value, eulerAngles.y, eulerAngles.z ) ;
				}
			}
		}


		public float _yaw
		{
			get
			{
				return eulerAngles.y ;
			}
			set
			{
				if( eulerAngles.y != value )
				{
					eulerAngles = new Vector3( eulerAngles.x, value, eulerAngles.z ) ;
				}
			}
		}

		public float _roll
		{
			get
			{
				return eulerAngles.z ;
			}
			set
			{
				if( eulerAngles.z != value )
				{
					eulerAngles = new Vector3( eulerAngles.x, eulerAngles.y, value ) ;
				}
			}
		}

		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private Vector3 m_LocalScale = Vector3.one ;
		public  Vector3   localScale
		{
			get
			{
				return m_LocalScale ;
			}
			set
			{
				m_LocalScale = value ;
			}
		}

		public Vector3 scale
		{
			get
			{
				if( Application.isPlaying == true )
				{
					return m_LocalScale ;
				}
				else
				{
					return transform.localScale ;
				}
			}
			set
			{
				m_LocalScale = value ;
				transform.localScale = value ;
			}
		}

		/// <summary>
		/// スケールをＸＹＺ一括で設定する
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( float tScale )
		{
			scale = new Vector3( tScale, tScale, tScale ) ;
		}
		
		/// <summary>
		/// スケールをＸＹＺ一括で設定する
		/// </summary>
		/// <param name="tScale"></param>
		public void SetScale( Vector3 tScale )
		{
			scale = tScale ;
		}
		
		//-------------------------------------------------------------------------------------------

		private Camera mChildCamera ;
		public Camera childCamera
		{
			get
			{
				if( mChildCamera != null )
				{
					return mChildCamera ;
				}
				mChildCamera = GetComponentInChildren<Camera>() ;
				return mChildCamera ;
			}
		}

		public void SetActive( bool tState )
		{
			gameObject.SetActive( tState ) ;
		}

		private Camera mCamera = null ;
		public  Camera _camera
		{
			get
			{
				if( mCamera != null )
				{
					return mCamera ;
				}
				mCamera = GetComponent<Camera>() ;
				return mCamera ;
			}
		}

		public bool isActive
		{
			get
			{
				return gameObject.activeSelf ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定の親からの相対的な位置を取得する
		/// </summary>
		/// <param name="tParent"></param>
		/// <returns></returns>
		public Vector3 GetRelativePosition( SoftTransform tParent )
		{
			return transform.position - tParent.transform.position ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 空の GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public GameObject AddObject( string tName, Transform tTransform = null, int tLayer = -1 )
		{
			GameObject tGameObject = new GameObject( tName ) ;
			tGameObject.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			tGameObject.transform.localRotation = Quaternion.identity ;
			tGameObject.transform.localScale = Vector3.one ;

			if( tTransform == null )
			{
				tTransform = transform ;
			}

			tGameObject.transform.SetParent( tTransform, false ) ;

			if( tLayer >= -1 && tLayer <= 31 )
			{
				if( tLayer == -1 )
				{
					tLayer = tTransform.gameObject.layer ;
				}
				SetLayer( tGameObject, tLayer ) ;
			}

			return tGameObject ;
		}

		/// <summary>
		/// 指定のコンポーネントをアタッチしたの GameObject を追加する
		/// </summary>
		/// <param name="tTransform"></param>
		/// <param name="tLayer"></param>
		/// <returns></returns>
		public T AddObject<T>( string tName, Transform tTransform = null, int tLayer = -1 ) where T : UnityEngine.Component
		{
			GameObject tGameObject = new GameObject( tName ) ;
			tGameObject.transform.localPosition = new Vector3(   0,  0,   0 ) ;
			tGameObject.transform.localRotation = Quaternion.identity ;
			tGameObject.transform.localScale = Vector3.one ;

			if( tTransform == null )
			{
				tTransform = transform ;
			}

			tGameObject.transform.SetParent( tTransform, false ) ;

			if( tLayer >= -1 && tLayer <= 31 )
			{
				if( tLayer == -1 )
				{
					tLayer = tTransform.gameObject.layer ;
				}
				SetLayer( tGameObject, tLayer ) ;
			}

			T tComponent = tGameObject.AddComponent<T>() ;
			return tComponent ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public GameObject AddPrefab( string tPath, Transform tTransform = null, int tLayer = -1 )
		{
			GameObject tGameObject = Resources.Load( tPath, typeof( GameObject ) ) as GameObject ;
			if( tGameObject == null )
			{
				return null ;
			}

			tGameObject = Instantiate( tGameObject ) ;
		
			AddPrefab( tGameObject, tTransform, tLayer ) ;
		
			return tGameObject ;
		}
	
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public GameObject AddPrefab( GameObject tPrefab, Transform tTransform = null, int tLayer = -1 )
		{
			if( tPrefab == null )
			{
				return null ;
			}
			
			if( tTransform == null )
			{
				tTransform = transform ;
			}

			GameObject tGameObject = ( GameObject )GameObject.Instantiate( tPrefab ) ;
			if( tGameObject == null )
			{
				return null ;
			}
		
			tGameObject.transform.SetParent( tTransform, false ) ;

			if( tLayer >= -1 && tLayer <= 31 )
			{
				if( tLayer == -1 )
				{
					tLayer = tTransform.gameObject.layer ;
				}
				SetLayer( tGameObject, tLayer ) ;
			}

			return tGameObject ;
		}
		
		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public T AddPrefab<T>( string tPath, Transform tTransform = null, int tLayer = -1 ) where T : UnityEngine.Component
		{
			GameObject tPrefab = Resources.Load( tPath, typeof( GameObject ) ) as GameObject ;
			if( tPrefab == null )
			{
				return null ;
			}

			return AddPrefab<T>( tPrefab, tTransform, tLayer ) ;
		}
		

		/// <summary>
		/// プレハブからインスタンスを生成し自身の子とする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParentName"></param>
		/// <returns></returns>
		public T AddPrefabOnChild<T>( GameObject tPrefab, string tParentName = null, int tLayer = -1 ) where T : UnityEngine.Component
		{
			Transform tTransform = null ;
			if( string.IsNullOrEmpty( tParentName ) == false )
			{
				if( transform.name.ToLower() == tParentName.ToLower() )
				{
					tTransform = transform ;
				}
				else
				{
					tTransform = GetTransformByName( transform, tParentName ) ;
				}
			}

			return AddPrefab<T>( tPrefab, tTransform, tLayer ) ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 自身に含まれる指定した名前のトランスフォームを検索する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Transform GetTransformByName( string tName, bool tContain = false )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return null ;
			}

			return GetTransformByName( transform, tName, tContain ) ;
		}

		// 自身に含まれる指定した名前のトランスフォームを検索する
		private Transform GetTransformByName( Transform tTransform, string tName, bool tContain = false )
		{
			tName = tName.ToLower() ;

			Transform tChild ;
			string tChildName ;
			bool tResult ;

			int i, l = tTransform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tChild = tTransform.GetChild( i ) ;
				tChildName = tChild.name.ToLower() ;
//				Debug.LogWarning( "n:[" + tChildName + "] : [" + tName +"]" ) ;
				tResult = false ;
				if( tContain == false && tChildName == tName )
				{
					tResult = true ;
				}
				else
				if( tContain == true && tChildName.Contains( tName ) == true )
				{
					tResult = true ;
				}

				if( tResult == true )
				{
					// 発見
					return tChild ;
				}

				if( tChild.childCount >  0 )
				{
					tChild = GetTransformByName( tChild, tName ) ;
					if( tChild != null )
					{
						// 発見
						return tChild ;
					}
				}
			}

			// 発見出来ず
			return null ;
		}

		/// <summary>
		/// Prefab を追加する
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tTransform"></param>
		/// <returns></returns>
		public T AddPrefab<T>( GameObject tPrefab, Transform tTransform = null, int tLayer = -1 ) where T : UnityEngine.Component
		{
			if( tPrefab == null )
			{
				return default( T ) ;
			}
			
			if( tTransform == null )
			{
				tTransform = transform ;
			}

			GameObject tGameObject = ( GameObject )GameObject.Instantiate( tPrefab ) ;
			if( tGameObject == null )
			{
				return null ;
			}
		
			tGameObject.transform.SetParent( tTransform, false ) ;

			if( tLayer >= -1 && tLayer <= 31 )
			{
				if( tLayer == -1 )
				{
					tLayer = tTransform.gameObject.layer ;
				}
				SetLayer( tGameObject, tLayer ) ;
			}

			T tComponent = tGameObject.GetComponent<T>() ;
			return tComponent ;
		}

	

		/// <summary>
		/// 指定したゲームオブジェクトを自身の子にする
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPrefab"></param>
		/// <param name="tParent"></param>
		/// <returns></returns>
		public GameObject SetPrefab( GameObject tPrefab, Transform tParent = null )
		{
			if( tPrefab == null )
			{
				return null ;
			}
		
			GameObject tGameObject = tPrefab ;
			if( tGameObject == null )
			{
				return null ;
			}
		
			if( tParent == null )
			{
				tParent = transform ;
			}
			tGameObject.transform.SetParent( tParent, false ) ;
			SetLayer( tGameObject, gameObject.layer ) ;

			return tGameObject ;
		}
	

		/// <summary>
		/// 指定のゲームオブジェクトを子も含めて指定のレイヤーに設定する
		/// </summary>
		/// <param name="tLayer"></param>
		public void SetLayer( int tLayer )
		{
			SetLayer( gameObject, tLayer ) ;
		}


		/// <summary>
		/// 指定のゲームオブジェクトを子も含めて指定のレイヤーに設定する
		/// </summary>
		/// <param name="tGameObject"></param>
		/// <param name="tLayer"></param>
		public static void SetLayer( GameObject tGameObject, int tLayer )
		{
			tGameObject.layer = tLayer ;

			int i, l = tGameObject.transform.childCount ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				SetLayer( tGameObject.transform.GetChild( i ).gameObject, tLayer ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// MonoBehaviour 直下には AddComponent が存在しないためバイパスメソッドを追加
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T AddComponent<T>() where T: UnityEngine.Component
		{
			return gameObject.AddComponent<T>() ;
		}

		/// <summary>
		/// ARGB 32 ビットから Color を返す
		/// </summary>
		/// <param name="tColor"></param>
		/// <returns></returns>
		public static Color32 ARGB( uint tColor )
		{
			return new Color32( ( byte )( ( tColor >> 16 ) & 0xFF ), ( byte )( ( tColor >>  8 ) & 0xFF ), ( byte )( ( tColor & 0xFF ) ), ( byte )( ( tColor >> 24 ) & 0xFF ) ) ;
		}
	
		//-------------------------------------------------------------------------------------------

		void Update()
		{
			#if UNITY_EDITOR
	
			if( Application.isPlaying == false )
			{
				bool tTweenChecker = false ;
				SoftTransformTween[] tTweenList = GetComponents<SoftTransformTween>() ;
				if( tTweenList != null && tTweenList.Length >  0 )
				{
					for( int i  = 0 ; i <  tTweenList.Length ; i++ )
					{
						if( tTweenList[ i ].isChecker == true )
						{
							tTweenChecker = true ;
							break ;
						}
					}
				}

				if( tTweenChecker == false )
				{
					// ３つの値が異なっていれば更新する
					if( transform != null )
					{
						if( m_LocalPosition != transform.localPosition )
						{
							m_LocalPosition  = transform.localPosition ;
						}
						if( m_LocalRotation != transform.localEulerAngles )
						{
							m_LocalRotation  = transform.localEulerAngles ;
						}
						if( m_LocalScale != transform.localScale )
						{
							m_LocalScale  = transform.localScale ;
						}
					}
				}
			}

			RemoveComponents() ;

			#endif
		}

		#if UNITY_EDITOR
		// コンポーネントの削除
		private void RemoveComponents()
		{
			if( string.IsNullOrEmpty( m_RemoveTweenIdentity ) == false && m_RemoveTweenInstance != 0 )
			{
				RemoveTween( m_RemoveTweenIdentity, m_RemoveTweenInstance ) ;
				m_RemoveTweenIdentity = null ;
				m_RemoveTweenInstance = 0 ;
			}
		}
		#endif

		#if UNITY_EDITOR
		
		private string m_RemoveTweenIdentity = null ;

		public  string  removeTweenIdentity
		{
			set
			{
				m_RemoveTweenIdentity = value ;
			}
		}

		private int    m_RemoveTweenInstance = 0 ;

		public  int     removeTweenInstance
		{
			set
			{
				m_RemoveTweenInstance = value ;
			}
		}

		#endif
		
		/// <summary>
		/// Tween の追加
		/// </summary>
		/// <param name="tIdentity"></param>
		public SoftTransformTween AddTween( string tIdentity )
		{
			SoftTransformTween tTween = gameObject.AddComponent<SoftTransformTween>() ;
			tTween.identity = tIdentity ;

			return tTween ;
		}
		
		/// <summary>
		/// Tween の削除
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tInstance"></param>
		public void RemoveTween( string tIdentity, int tInstance = 0 )
		{
			SoftTransformTween[] tTweenList = GetComponents<SoftTransformTween>() ;
			if( tTweenList == null || tTweenList.Length == 0 )
			{
				return ;
			}
			int i, l = tTweenList.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( ( tInstance == 0 && tTweenList[ i ].identity == tIdentity ) || ( tInstance != 0 && tTweenList[ i ].identity == tIdentity && tTweenList[ i ].GetInstanceID() == tInstance ) )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				DestroyImmediate( tTweenList[ i ] ) ;
			}
			else
			{
				Destroy( tTweenList[ i ] ) ;
			}
		}

		/// <summary>
		/// Tween の取得
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public SoftTransformTween GetTween( string tIdentity )
		{
			SoftTransformTween[] tTweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tTweenArray == null || tTweenArray.Length == 0 )
			{
				return null ;
			}

			int i, l = tTweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTweenArray[ i ].identity == tIdentity )
				{
					return tTweenArray[ i ] ;
				}
			}

			return null ;
		}

		/// <summary>
		/// 全ての Tween を取得
		/// </summary>
		/// <returns></returns>
		public Dictionary<string,SoftTransformTween> GetTweenAll()
		{
			SoftTransformTween[] tTweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tTweenArray == null || tTweenArray.Length == 0 )
			{
				return null ;
			}

			Dictionary<string,SoftTransformTween> tList = new Dictionary<string, SoftTransformTween>() ;

			int i, l = tTweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( string.IsNullOrEmpty( tTweenArray[ i ].identity ) == false )
				{
					if( tList.ContainsKey( tTweenArray[ i ].identity ) == false )
					{
						tList.Add( tTweenArray[ i ].identity, tTweenArray[ i ] ) ;
					}
				}
			}

			if( tList.Count == 0 )
			{
				return null ;
			}

			return tList ;
		}

		/// <summary>
		/// Tween の Delay と Duration を設定
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public bool SetTweenTime( string tIdentity, float tDelay = -1, float tDuration = -1 )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.delay = tDelay ;
			tTween.duration = tDuration ;
			return true ;
		}

		/// <summary>
		/// Tween の再生
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public bool PlayTween( string tIdentity, float tDelay = -1, float tDuration = -1 )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			if( tTween.gameObject.activeSelf == false )
			{
				tTween.gameObject.SetActive( true ) ;
			}

			tTween.Play( tDelay, tDuration ) ;
			return true ;
		}

		/// <summary>
		/// Tween の再生(コルーチン)
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tDelay"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public IEnumerator PlayTween_Coroutine( string tIdentity, float tDelay = -1, float tDuration = -1 )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				yield break ;
			}

			if( tTween.gameObject.activeSelf == false )
			{
				tTween.gameObject.SetActive( true ) ;
			}

			yield return StartCoroutine( tTween.Play_Coroutine( tDelay, tDuration ) ) ;
		}

		/// <summary>
		/// いずれかの Tween の実行中の有無 
		/// </summary>
		public bool isAnyTweenPlaying
		{
			get
			{
				SoftTransformTween[] tTweenArray = gameObject.GetComponents<SoftTransformTween>() ;
				if( tTweenArray == null || tTweenArray.Length == 0 )
				{
					return false ;
				}

				int i, l = tTweenArray.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tTweenArray[ i ].enabled == true && ( tTweenArray[ i ].isRunning == true || tTweenArray[ i ].isPlaying == true ) )
					{
						return true ;
					}
				}
			
				return false ;
			}
		}

		/// <summary>
		/// Tween の一時停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool PauseTween( string tIdentity )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.Pause() ;
			return true ;
		}

		/// <summary>
		/// Tween の再開
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool ContinueTween( string tIdentity )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.Continue() ;
			return true ;
		}


		/// <summary>
		/// Tween の完全停止
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool StopTween( string tIdentity )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.Stop() ;
			return true ;
		}

		/// <summary>
		/// Tween の完全停止と状態のリセット
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public bool StopAndResetTween( string tIdentity )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.StopAndReset() ;
			return true ;
		}

		/// <summary>
		/// 全ての Tween の停止
		/// </summary>
		public bool StopTweenAll()
		{
			SoftTransformTween[] tTweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tTweenArray == null || tTweenArray.Length == 0 )
			{
				return false ;
			}

			int i, l = tTweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTweenArray[ i ].enabled == true && ( tTweenArray[ i ].isRunning == true || tTweenArray[ i ].isPlaying == true ) )
				{
					tTweenArray[ i ].Stop() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 全ての Tween の停止とリセット
		/// </summary>
		public bool StopAndResetTweenAll()
		{
			SoftTransformTween[] tTweenArray = gameObject.GetComponents<SoftTransformTween>() ;
			if( tTweenArray == null || tTweenArray.Length == 0 )
			{
				return false ;
			}

			int i, l = tTweenArray.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTweenArray[ i ].enabled == true )
				{
					tTweenArray[ i ].StopAndReset() ;
				}
			}
			
			return false ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を取得する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <returns></returns>
		public float GetTweenProcessTime( string tIdentity )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return 0 ;
			}

			return tTween.processTime ;
		}

		/// <summary>
		/// 実行中の Tween の実行開始からの経過時間を設定する
		/// </summary>
		/// <param name="tIdentity"></param>
		/// <param name="tTime"></param>
		public bool SetTweenProcessTime( string tIdentity, float tTime )
		{
			SoftTransformTween tTween = GetTween( tIdentity ) ;
			if( tTween == null )
			{
				return false ;
			}

			tTween.processTime = tTime ;

			return true ;
		}

		//-------------------------------------------

	}
}

