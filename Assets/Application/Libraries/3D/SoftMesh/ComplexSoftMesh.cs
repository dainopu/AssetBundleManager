using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

[ RequireComponent( typeof( MeshRenderer ) ) ]
[ RequireComponent( typeof( MeshFilter ) ) ]

[ ExecuteInEditMode ]


// Last Update 2018/04/10 0

/// <summary>
/// 任意のメッシュ形状を選択出来るメッシュ生成クラス
/// </summary>
public class ComplexSoftMesh : MonoBehaviour
{
	//------------------------------------------------------------

	// private

	// メッシュレンダラー
	[ SerializeField  ][ HideInInspector ]
	private MeshRenderer	m_MeshRenderer ;
	private MeshRenderer	 _MeshRenderer
	{
		get
		{
			if( m_MeshRenderer != null )
			{
				return m_MeshRenderer ;
			}
			m_MeshRenderer = GetComponent<MeshRenderer>() ;
			return m_MeshRenderer ;
		}
	}


	// メッシュフィルター
	[ SerializeField ][ HideInInspector ]
	private MeshFilter		m_MeshFilter ;
	private MeshFilter		 _MeshFilter
	{
		get
		{
			if( m_MeshFilter != null )
			{
				return m_MeshFilter ;
			}
			m_MeshFilter = GetComponent<MeshFilter>() ;
			return m_MeshFilter ;
		}
	}

	private Mesh m_Mesh = null ;

	//------------------------------------------------------------

	// protected & public

	/// <summary>
	/// メッシュ形状
	/// </summary>
	public enum ShapeType
	{
		Cube		= 1,
		Sphere		= 2,
		Capsule		= 3,
		Cylinder	= 4,
		Plane		= 5,
		Box2D		= 6,
		Circle2D	= 7,
//		Polygon2D	= 8,
	}

	/// <summary>
	/// メッシュ形状がカプセル・シリンダーの場合の方向
	/// </summary>
	public enum Direction
	{
		X_Axis = 0,
		Y_Axis = 1,
		Z_Axis = 2,
	}

	/// <summary>
	/// メッシュの形状がプレーンの場合の配置
	/// </summary>
	public enum PlaneDirection
	{
		Front  = 1,
		Back   = 2,
		Left   = 3,
		Right  = 4,
		Top    = 5,
		Bottom = 6,
	}

	//--------------------------------------------------------------------------------------------

	[Serializable]
	public class ShapeData
	{
		public	string			name			= "" ;

		public	bool			visible			= true ;

		public	ShapeType		shapeType		= ShapeType.Cube ;
		public	Direction		direction		= Direction.Y_Axis ;
		public	PlaneDirection	planeDirection	= PlaneDirection.Front ;
		public	Rect[]			uv				= new Rect[]{ new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ), new Rect( 0, 0, 1, 1 ) } ;

		public	Vector3			offset			= Vector3.zero ;
		public	Vector3			size			= Vector3.one ;
		public	Color			vertexColor		= Color.white ;
		public	int				split			= 0 ;
		public	bool			tiling			= false ;

		public ShapeData( string tName )
		{
			name	= tName ;
		}

		/// <summary>
		/// 複数のＵＶをまとめて設定する
		/// </summary>
		/// <param name="tUVs"></param>
		public void SetUV( params Rect[] tUVs )
		{
			if( tUVs == null || tUVs.Length == 0 )
			{
				return ;
			}

			int i, l = tUVs.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				SetUV( i, tUVs[ i ] ) ;
			}
		}

		/// <summary>
		/// インデックスを指定してＵＶを設定する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <param name="tUV"></param>
		/// <returns></returns>
		public bool SetUV( int tIndex, Rect tUV )
		{
			if( uv == null || uv.Length == 0 )
			{
				return false ;
			}

			if( tIndex <  0 || tIndex >= uv.Length )
			{
				return false ;
			}

			if( uv[ tIndex ].Equals( tUV ) == false )
			{
				uv[ tIndex ].x		= tUV.x ;
				uv[ tIndex ].y		= tUV.y ;
				uv[ tIndex ].width	= tUV.width ;
				uv[ tIndex ].height	= tUV.height ;
			}

			return true ;
		}
	}

	public List<ShapeData>	shapeData = new List<ShapeData>() ;

	/// <summary>
	/// 形状情報を追加する
	/// </summary>
	/// <param name="tName"></param>
	/// <returns></returns>
	public ShapeData AddShapeData( string tName )
	{
		ShapeData tShapeData = new ShapeData( tName ) ;
		shapeData.Add( tShapeData ) ;

		return tShapeData ;
	}

	/// <summary>
	/// 形状情報を削除する
	/// </summary>
	/// <param name="tName"></param>
	/// <returns></returns>
	public bool RemoveShapeData( string tName )
	{
		int i, l = shapeData.Count ;

		for( i  = 0 ; i <  l ; i ++ )
		{
			if( shapeData[ i ].name == tName )
			{
				// 発見
				shapeData.RemoveAt( i ) ;
				return true ;
			}
		}

		return false ;
	}

	//--------------------------------------------------------------------------------------------

	/// <summary>
	/// マテリアル(ショートカット)
	/// </summary>
	public    Material  material
	{
		get
		{
			if(  m_MeshRenderer == null )
			{
				return null ;
			}

			if( Application.isPlaying == false )
			{
				return m_MeshRenderer.sharedMaterial ;
			}
			else
			{
				return m_MeshRenderer.material ;
			}
		}
		set
		{
			if( m_MeshRenderer == null )
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial != value )
				{
					if( m_MeshRenderer.sharedMaterial != null )
					{
						// テクスチャの受け渡し
						Texture tTexture = m_MeshRenderer.sharedMaterial.mainTexture ;
	
						m_MeshRenderer.sharedMaterial = value ;
	
						if( m_MeshRenderer.sharedMaterial != null && m_MeshRenderer.sharedMaterial.mainTexture == null )
						{
							m_MeshRenderer.sharedMaterial.mainTexture = tTexture ;
						}
					}
					else
					{
						m_MeshRenderer.sharedMaterial = value ;
					}
				}
			}
			else
			{
				if( m_MeshRenderer.material != value )
				{
					if( m_MeshRenderer.material != null )
					{
						// テクスチャの受け渡し
						Texture tTexture = m_MeshRenderer.material.mainTexture ;
	
						m_MeshRenderer.material = value ;
	
						if( m_MeshRenderer.material != null && m_MeshRenderer.material.mainTexture == null )
						{
							m_MeshRenderer.material.mainTexture = tTexture ;
						}
					}
					else
					{
						m_MeshRenderer.sharedMaterial = value ;
					}
				}
			}
		}
	}


	[SerializeField][HideInInspector]
	private Texture	m_Texture = null ;

	/// <summary>
	/// テクスチャ(ショートカット)
	/// </summary>
	public    Texture	 texture
	{
		get
		{
			return m_Texture ;
		}
		set
		{
			m_Texture = value ;

			if( m_MeshRenderer == null ) 
			{
				return ;
			}

			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.sharedMaterial = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.sharedMaterial != null )
				{
					if( m_MeshRenderer.sharedMaterial.mainTexture != m_Texture )
					{
						m_MeshRenderer.sharedMaterial.mainTexture  = m_Texture ;
					}
				}
			}
			else
			{
				if( m_MeshRenderer.material == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.material = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.material != null )
				{
					if( m_MeshRenderer.material.mainTexture != m_Texture )
					{
						m_MeshRenderer.material.mainTexture  = m_Texture ;
					}
				}
			}
		}
	}

	//---------------------------------------

	public Vector3 totalOffset	= Vector3.zero ;
	public Vector3 totalSize	= Vector3.zero ;

	// キャッシュ
	private Collider	m_Collider = null ;

	/// <summary>
	/// Collider(ショートカット)
	/// </summary>
	public  Collider _collider
	{
		get
		{
			if( m_Collider != null )
			{
				return m_Collider ;
			}
			m_Collider = GetComponent<Collider>() ;

			return m_Collider ;
		}
	}

	/// <summary>
	/// コライダーが存在するかの判定を行う
	/// </summary>
	public bool isCollider
	{
		get
		{
			if( _collider == null )
			{
				return false ;
			}
			else
			{
				return true ;
			}
		}
		set
		{
			if( value == true )
			{
				AddCollider() ;
			}
			else
			{
				#if UNITY_EDITOR

				if( Application.isPlaying == true )
				{
					RemoveCollider() ;
				}
				else
				{
					m_RemoveCollider = true ;
				}

				#else

				RemoveCollider() ;

				#endif
			}
		}
	}

	/// <summary>
	/// Collider の追加
	/// </summary>
	public void AddCollider()
	{
		if( _collider != null )
		{
			return ;
		}
			
		m_Collider = gameObject.AddComponent<BoxCollider>() ;

		AdjustCollider() ;
	}

	#if UNITY_EDITOR
	private bool m_RemoveCollider	= false ;
	private bool m_AddCollider		= false ;
	#endif

	/// <summary>
	/// Collider の削除
	/// </summary>
	public void RemoveCollider()
	{
		Collider tCollider = _collider ;
		if( tCollider != null )
		{
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tCollider ) ;
			}
			else
			{
				Destroy( tCollider ) ;
			}
	
			m_Collider = null ;
		}
	}

	/// <summary>
	/// コライダーの自動調整
	/// </summary>
	[ SerializeField ][ HideInInspector ]
	protected bool m_ColliderAdjustment = true ;

	public    bool	colliderAdjustment
	{
		get
		{
			return m_ColliderAdjustment ;
		}
		set
		{
			if( m_ColliderAdjustment != value )
			{
				m_ColliderAdjustment  = value ;

				if( m_ColliderAdjustment == true )
				{
					AdjustCollider() ;
				}
			}
		}
	}

	/// <summary>
	/// コライダーの位置と大きさをメッシュと同じに合わせる
	/// </summary>
	public void AdjustCollider()
	{
		if( _collider == null )
		{
			return ;
		}

		BoxCollider tCollider = _collider as BoxCollider ;
		tCollider.center	= totalOffset ;

		tCollider.size		= totalSize ; 
	}

	//---------------------------------------------------------------

	// キャッシュ
	private Rigidbody	m_Rigidbody		= null ;

	/// <summary>
	/// Rigidbody(ショートカット)
	/// </summary>
	public  Rigidbody _rigidbody
	{
		get
		{
			if( m_Rigidbody != null )
			{
				return m_Rigidbody ;
			}
			m_Rigidbody = GetComponent<Rigidbody>() ;

			return m_Rigidbody ;
		}
	}

	/// <summary>
	/// Rigidbody が存在するかの判定を行う
	/// </summary>
	public bool isRigidbody
	{
		get
		{
			if( _rigidbody == null )
			{
				return false ;
			}
			else
			{
				return true ;
			}
		}
		set
		{
			if( value == true )
			{
				AddRigidbody() ;
			}
			else
			{
				#if UNITY_EDITOR

				if( Application.isPlaying == true )
				{
					RemoveRigidbody() ;
				}
				else
				{
					m_RemoveRigidbody = true ;
				}

				#else

				RemoveRigidbody() ;

				#endif
			}
		}
	}

	/// <summary>
	/// Rigidbody の追加
	/// </summary>
	public void AddRigidbody()
	{
		m_Rigidbody = gameObject.AddComponent<Rigidbody>() ;
	}

	#if UNITY_EDITOR
	private bool m_RemoveRigidbody	= false ;
	private bool m_AddRigidbody		= false ;
	#endif

	/// <summary>
	/// Rigidbody の削除
	/// </summary>
	public void RemoveRigidbody()
	{
		Rigidbody tRigidbody = _rigidbody ;
		if( tRigidbody != null )
		{
			if( Application.isPlaying == false )
			{
				DestroyImmediate( tRigidbody ) ;
			}
			else
			{
				Destroy( tRigidbody ) ;
			}
	
			m_Rigidbody = null ;
		}
	}

	//--------------------------------------------------------------------------------------------

	private List<Vector3>	m_aV = new List<Vector3>() ;
	private List<Vector3>	m_aN = new List<Vector3>() ;
	private List<Color>		m_aC = new List<Color>() ;
	private List<Vector2>	m_aT = new List<Vector2>() ;
	private List<int>		m_aI = new List<int>() ;

	//--------------------------------------------------------------------------------------------

	void Awake()
	{
		m_MeshRenderer	= GetComponent<MeshRenderer>() ;

		UpdateTexture() ;

		m_MeshFilter		= GetComponent<MeshFilter>() ;
		if( m_MeshFilter != null )
		{
			m_Mesh = new Mesh() ;
			if( Application.isPlaying == false )
			{
				m_MeshFilter.sharedMesh = m_Mesh ;
			}
			else
			{
				m_MeshFilter.mesh = m_Mesh ;
			}
		}

		Refresh() ;
	}

	void Start()
	{
		UpdateTexture() ;
	}

	private void UpdateTexture()
	{
		if( m_MeshRenderer != null )
		{
			if( Application.isPlaying == false )
			{
				if( m_MeshRenderer.sharedMaterial == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.sharedMaterial = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.sharedMaterial != null && m_Texture != null )
				{
					m_MeshRenderer.sharedMaterial.mainTexture = m_Texture ;
				}
			}
			else
			{
				if( m_MeshRenderer.material == null )
				{
#if UNITY_EDITOR
					m_MeshRenderer.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>( "Default-Diffuse.mat" ) ;
#else
					m_MeshRenderer.material = Resources.GetBuiltinResource<Material>( "Default-Diffuse.mat" ) ;
#endif
				}

				if( m_MeshRenderer.material != null && m_Texture != null )
				{
					m_MeshRenderer.material.mainTexture = m_Texture ;
				}
			}
		}
	}

	void Update()
	{
		#if UNITY_EDITOR

		// 生成と削除の順番が重要
		// 必ず
		// Ｒ削除→Ｃ削除→Ｃ生成→Ｒ生成
		// とすること
		
		if( m_RemoveRigidbody == true )
		{
			RemoveRigidbody() ;
			m_RemoveRigidbody  = false ;
		}

		if( m_RemoveCollider == true )
		{
			RemoveCollider() ;
			m_RemoveCollider  = false ;
		}

		if( m_AddCollider == true )
		{
			AddCollider() ;
			m_AddCollider  = false ;
		}

		if( m_AddRigidbody == true )
		{
			AddRigidbody() ;
			m_AddRigidbody  = false ;
		}

		#endif
	}

	void OnDestroy()
	{
		if( m_MeshFilter != null )
		{
			if( Application.isPlaying == false )
			{
				m_MeshFilter.sharedMesh = null ;
			}
			else
			{
				m_MeshFilter.mesh = null ;
			}
			m_MeshFilter = null ;
		}

		if( m_Mesh != null )
		{
			DestroyImmediate( m_Mesh ) ;
			m_Mesh = null ;
		}

		if( m_MeshRenderer != null )
		{
			if( Application.isPlaying == false )
			{
				m_MeshRenderer.sharedMaterial = null ;
			}
			else
			{
				m_MeshRenderer.material = null ;
			}
		}
	}

	//-------------------------------------------------------------------------

	/// <summary>
	/// メッシュをクリアする
	/// </summary>
	public void Clear()
	{
		MeshFilter tMeshFilter = _MeshFilter ;
		if( tMeshFilter == null )
		{
			return ;
		}

		if( Application.isPlaying == false )
		{
			if( tMeshFilter.sharedMesh == null )
			{
				return ;
			}

			tMeshFilter.sharedMesh.Clear() ;
		}
		else
		{
			if( tMeshFilter.mesh == null )
			{
				return ;
			}

			tMeshFilter.mesh.Clear() ;
		}
	}


	// メッシュを更新する
	public void Refresh()
	{
		int i, l ;

		m_aV.Clear() ;
		m_aN.Clear() ;
		m_aC.Clear() ;
		m_aT.Clear() ;
		m_aI.Clear() ;

		//-----------------------------------------------------------
		
		if( shapeData != null && shapeData.Count >  0 )
		{
			l = shapeData.Count ;
			for( i  = 0 ; i <  l ;i ++ )
			{
				if( shapeData[ i ].visible == true )
				{
					switch(	shapeData[ i ].shapeType )
					{
						case ShapeType.Cube		: CreateCube( shapeData[ i ] )		; break ;
						case ShapeType.Sphere	: CreateSphere( shapeData[ i ] )	; break ;
						case ShapeType.Capsule	: CreateCapsule( shapeData[ i ] )	; break ;
						case ShapeType.Cylinder	: CreateCylinder( shapeData[ i ] )	; break ;
						case ShapeType.Plane	: CreatePlane( shapeData[ i ] )		; break ;
						case ShapeType.Box2D	: CreateBox2D( shapeData[ i ] )		; break ;
						case ShapeType.Circle2D	: CreateCircle2D( shapeData[ i ])	; break ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		float xMin = Mathf.Infinity, xMax = - Mathf.Infinity ;
		float yMin = Mathf.Infinity, yMax = - Mathf.Infinity ;
		float zMin = Mathf.Infinity, zMax = - Mathf.Infinity ;

		float x, y, z ;

		if( m_aV.Count >  0 )
		{
			l = m_aV.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				x = m_aV[ i ].x ;
				y = m_aV[ i ].y ;
				z = m_aV[ i ].z ;

				if( x >  xMax )
				{
					xMax  = x ;
				}
				else
				if( x <  xMin )
				{
					xMin  = x ;
				}

				if( y >  yMax )
				{
					yMax  = y ;
				}
				else
				if( y <  yMin )
				{
					yMin  = y ;
				}

				if( z >  zMax )
				{
					zMax  = z ;
				}
				else
				if( z <  zMin )
				{
					zMin  = z ;
				}
			}

			totalOffset	= new Vector3( ( xMin + xMax ) * 0.5f, ( yMin + yMax ) * 0.5f, ( zMin + zMax ) * 0.5f ) ;
			totalSize	= new Vector3( Mathf.Abs( xMax - yMin ), Mathf.Abs( yMax - yMin ), Mathf.Abs( zMax - zMin ) ) ;
		}

		Build( "Complex", m_aV.ToArray(), m_aN.ToArray(), m_aC.ToArray(), m_aT.ToArray(), m_aI.ToArray() ) ;

		//-----------------------------------------------------------

		m_aV.Clear() ;
		m_aN.Clear() ;
		m_aC.Clear() ;
		m_aT.Clear() ;
		m_aI.Clear() ;
	}

	private void Add( Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI )
	{
		int tOffset = m_aV.Count ;

		int i, l = aI.Length ;
		for( i  = 0 ; i <  l ; i ++ )
		{
			aI[ i ] = tOffset + aI[ i ] ;
		}

		m_aV.AddRange( aV ) ;
		m_aN.AddRange( aN ) ;
		m_aC.AddRange( aC ) ;
		m_aT.AddRange( aT ) ;
		m_aI.AddRange( aI ) ;
	}

	/// <summary>
	/// メッシュを更新する
	/// </summary>
	/// <param name="aVD"></param>
	/// <param name="aCD"></param>
	/// <param name="aTD"></param>
	/// <param name="aID"></param>
	private void Build( string tName, Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI )
	{
		MeshFilter tMeshFilter = _MeshFilter ;
		if( tMeshFilter == null )
		{
			return ;
		}

		if( tMeshFilter.sharedMesh == null )
		{
			return ;
		}

		tMeshFilter.sharedMesh.name = tName ;

		tMeshFilter.sharedMesh.Clear() ;	// 新たに設定する場合は必ずクリアが必要（クリアしないと設定中の要素数の不整合がおきてエラーになる）
		tMeshFilter.sharedMesh.vertices  = aV ;
		tMeshFilter.sharedMesh.normals   = aN ;
		tMeshFilter.sharedMesh.colors    = aC ;
		tMeshFilter.sharedMesh.uv        = aT ;
		tMeshFilter.sharedMesh.triangles = aI ;
		
		tMeshFilter.sharedMesh.RecalculateNormals() ;
		tMeshFilter.sharedMesh.RecalculateBounds() ;
	}

	//--------------------------------------------------------------------------------------------

	private void CreateCube( ShapeData tShapeData )
	{
		CreateCube
		(
			tShapeData.offset.x,	tShapeData.offset.y,	tShapeData.offset.z,
			tShapeData.size.x,		tShapeData.size.y,		tShapeData.size.z,
			tShapeData.vertexColor,
			tShapeData.uv,
			tShapeData.split
		) ;
	}

	/// <summary>
	/// キューブを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="uv"></param>
	public void CreateCube( float px, float py, float pz, float sx, float sy, float sz, Color c, Rect[] tUV = null, int tSplit = 0 )
	{
		// 値を更新しておく
//		m_Offset      = new Vector3( px, py, pz ) ;
//		m_Size        = new Vector3( sx, sy, sz ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float xMin = - sx * 0.5f, xMax = sx * 0.5f, yMin = - sy * 0.5f, yMax = sy * 0.5f, zMin = - sz * 0.5f, zMax = sz * 0.5f ;
		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;

		Vector3[,] v = new Vector3[6,4]
		{
			{ new Vector3( xMin, yMin, zMin ), new Vector3( xMin, yMax, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMin, zMin ) },
            { new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMax, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMax, zMin ) },
			{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMax, zMin ), new Vector3( xMin, yMin, zMin ) },
			{ new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMax, zMin ), new Vector3( xMax, yMax, zMax ), new Vector3( xMax, yMin, zMax ) },
			{ new Vector3( xMin, yMin, zMax ), new Vector3( xMin, yMin, zMin ), new Vector3( xMax, yMin, zMin ), new Vector3( xMax, yMin, zMax ) },
			{ new Vector3( xMax, yMin, zMax ), new Vector3( xMax, yMax, zMax ), new Vector3( xMin, yMax, zMax ), new Vector3( xMin, yMin, zMax ) },
		} ;

		Vector3[] n = new Vector3[]
		{
			new Vector3(  0,  0, -1 ),
			new Vector3(  0,  1,  0 ),
			new Vector3( -1,  0,  0 ),
			new Vector3(  1,  0,  0 ),
			new Vector3(  0, -1,  0 ),
			new Vector3(  0,  0,  1 ),
		} ;

		int l = v.GetLength( 0 ) ;

		int ly, lx ;

		int s, s1 = 1 << tSplit, s2 = 2 << tSplit ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}

		int f ;
		for( f  = 0 ; f <  l ; f ++ )
		{
			float vx0 = v[ f, 0 ].x ;
			float vy0 = v[ f, 0 ].y ;
			float vz0 = v[ f, 0 ].z ;

			float vx1 = v[ f, 1 ].x ;
			float vy1 = v[ f, 1 ].y ;
			float vz1 = v[ f, 1 ].z ;

			float vx2 = v[ f, 2 ].x ;
			float vy2 = v[ f, 2 ].y ;
			float vz2 = v[ f, 2 ].z ;

			float vx3 = v[ f, 3 ].x ;
			float vy3 = v[ f, 3 ].y ;
			float vz3 = v[ f, 3 ].z ;


			float tx0, ty0 ;
			float tx1, ty1 ;
			float tx2, ty2 ;
			float tx3, ty3 ;

			if( tUV != null && tUV.Length >= 6 )
			{
				tx0 = tUV[ f ].x ;
				ty0 = tUV[ f ].y ;
				tx1 = tUV[ f ].x ;
				ty1 = tUV[ f ].y + tUV[ f ].height ;
				tx2 = tUV[ f ].x + tUV[ f ].width ;
				ty2 = tUV[ f ].y + tUV[ f ].height ;
				tx3 = tUV[ f ].x + tUV[ f ].width ;
				ty3 = tUV[ f ].y ;
			}
			else
			{
				tx0 = 0 ;
				ty0 = 0 ;
				tx1 = 0 ;
				ty1 = 1 ;
				tx2 = 1 ;
				ty2 = 1 ;
				tx3 = 1 ;
				ty3 = 0 ;
			}

			for( lx  = 0 ; lx <  s2 ; lx ++ )
			{
				// V
				float vlx0_dx = vx3 - vx0 ;
				float vlx0_dy = vy3 - vy0 ;
				float vlx0_dz = vz3 - vz0 ;

				float vlx1_dx = vx2 - vx1 ;
				float vlx1_dy = vy2 - vy1 ;
				float vlx1_dz = vz2 - vz1 ;

				float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
				float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
				float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

				float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
				float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
				float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

				float vly_dx = vlx1_x - vlx0_x ;
				float vly_dy = vlx1_y - vlx0_y ;
				float vly_dz = vlx1_z - vlx0_z ;

				for( ly  = 0 ; ly <  s2 ; ly ++  )
				{
					float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
					float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
					float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

					tV[ lx ][ ly ] = new Vector3( vx, vy, vz ) ;
				}

				// T
				float tlx0_dx = tx3 - tx0 ;
				float tlx0_dy = ty3 - ty0 ;

				float tlx1_dx = tx2 - tx1 ;
				float tlx1_dy = ty2 - ty1 ;

				float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
				float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

				float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
				float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

				float tly_dx = tlx1_x - tlx0_x ;
				float tly_dy = tlx1_y - tlx0_y ;

				for( ly  = 0 ; ly <  s2 ; ly ++  )
				{
					float tx = tlx0_x + ( tly_dx * ly / s1 ) ;
					float ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
					tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
				}
			}

			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( n[ f ] ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateSphere( ShapeData tShapeData )
	{
		CreateSphere
		(
			tShapeData.offset.x,	tShapeData.offset.y,	tShapeData.offset.z,
			tShapeData.size.x,		tShapeData.size.y,		tShapeData.size.z,
			tShapeData.vertexColor,
			tShapeData.split
		) ;
	}

	/// <summary>
	/// スフィアを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateSphere( float px, float py, float pz, float sx, float sy, float sz, Color c, int tSplit = 0 )
	{
		// 値を更新しておく
//		m_Offset      = new Vector3( px, py, pz ) ;
//		m_Size        = new Vector3( sx, sy, sz ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int l ;
		int s1, s2, s4 ;
		int ly ;
		float a, r ;
		
		s1 = 1 << tSplit ;
		s2 = 2 << tSplit ;
		s4 = 4 << tSplit ;


		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;

		float vx, vy, vz ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s2 ) ;
		int yp = s2 ;


		// 一番上の頂点
		vx = 0 ;
		vy =   1.0f ;
		vz = 0 ;
		ty = tdh * yp ; yp -- ;
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * i + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o + i          ) ;
			aI.Add( o + i + 1 + s4 ) ;
			aI.Add( o + i     + s4 ) ;
		}
		o = o + s4 ;

		// 赤道から上の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  = Mathf.Sin( a ) ;
			vy = Mathf.Cos( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				vx = - Mathf.Cos( a ) * r ;
//				vz = - Mathf.Sin( a ) * r ;
				vx = - Mathf.Sin( a ) * r ;
				vz =   Mathf.Cos( a ) * r ;

				aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
				aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道の頂点
		vy = 0 ;
		ty = tdh * yp ; yp -- ;
		for( i  = 0 ; i <= s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
//			vx = - Mathf.Cos( a ) ;
//			vz = - Mathf.Sin( a ) ;
			vx = - Mathf.Sin( a ) ;
			vz =   Mathf.Cos( a ) ;

			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cb, cg, ca ) ) ;

			tx = tdw * i ;
			aT.Add( new Vector2( tx, ty ) ) ;

			if( i <  s4 )
			{
				if( tSplit == 0 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
				else
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
		}
		o = o + s4 + 1 ;

		// 赤道から下の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  =   Mathf.Cos( a ) ;
			vy = - Mathf.Sin( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				vx = - Mathf.Cos( a ) * r ;
//				vz = - Mathf.Sin( a ) * r ;
				vx = - Mathf.Sin( a ) * r ;
				vz =   Mathf.Cos( a ) * r ;

				aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
				aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  ( s1 - 1 ) )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}
		
		// 一番下の頂点
		vx = 0 ;
		vy = - 1.0f ;
		vz = 0 ;
		ty = tdh * yp ; yp -- ;
		for( l  = 0 ; l <  s4 ; l ++ )
		{
			aV.Add( new Vector3( hsx * vx + px, hsy * vy + py, hsz * vz + pz ) ) ;
			aN.Add( new Vector3( vx, vy, vz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * l + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;
		}
		
		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateCapsule( ShapeData tShapeData )
	{
		CreateCapsule
		(
			tShapeData.direction,
			tShapeData.offset.x,	tShapeData.offset.y,	tShapeData.offset.z,
			tShapeData.size.x,		tShapeData.size.y,		tShapeData.size.z,
			tShapeData.vertexColor,
			tShapeData.split
		) ;
	}

	/// <summary>
	/// カプセルを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCapsule( Direction tDirection, float px, float py, float pz, float sx, float sy, float sz, Color c, int tSplit = 0 )
	{
		// 値を更新しておく
//		m_Offset      = new Vector3( px, py, pz ) ;
//		m_Size        = new Vector3( sx, sy, sz ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int l ;
		int s1, s2, s4 ;
		int ly ;
		float a, r ;

		float vx, vy, vz, vs ;
		float nx, ny, nz, ns ;

		s1 = 1 << tSplit ;
		s2 = 2 << tSplit ;
		s4 = 4 << tSplit ;


		float hsx = sx * 0.50f ;
		float hsy = sy * 0.50f ;
		float hsz = sz * 0.50f ;

		float qsy = sy * 0.25f ;

		float x, y, z ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s4 ) ;
		int yp = s4 ;


		// 一番上の頂点
		x = 0 ;
		y =   1.0f ;
		z = 0 ;
		ty = tdh * yp ; yp -- ;

		for( i  = 0 ; i <  s4 ; i ++ )
		{
			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx = x ;
			ny = y ;
			nz = z ;

			if( tDirection == Direction.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( tDirection == Direction.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			tx = tdw * i + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o + i          ) ;
			aI.Add( o + i + 1 + s4 ) ;
			aI.Add( o + i     + s4 ) ;
		}
		o = o + s4 ;

		// 赤道から上の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  = Mathf.Sin( a ) ;
			y = Mathf.Cos( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) * r ;
//				z = - Mathf.Sin( a ) * r ;
				x = - Mathf.Sin( a ) * r ;
				z =   Mathf.Cos( a ) * r ;

				vx = hsx * x ;
				vy = qsy * y + qsy ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( tDirection == Direction.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( tDirection == Direction.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					aI.Add( o + i              ) ;
					aI.Add( o + i + 1          ) ;
					aI.Add( o + i     + s4 + 1 ) ;

					aI.Add( o + i + 1          ) ;
					aI.Add( o + i + 1 + s4 + 1 ) ;
					aI.Add( o + i     + s4 + 1 ) ;
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道の頂点
		for( ly  = 0 ; ly <= s2 ; ly ++ )
		{
			y = 0.25f - ( 0.5f * ly / s2 ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) ;
//				z = - Mathf.Sin( a ) ;
				x = - Mathf.Sin( a ) ;
				z =   Mathf.Cos( a ) ;

				vx = hsx * x ;
				vy = sy  * y ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( tDirection == Direction.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( tDirection == Direction.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cb, cg, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  s2 )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						if( tSplit == 0 )
						{
							aI.Add( o + i              ) ;
							aI.Add( o + i + 1          ) ;
							aI.Add( o + i     + s4 + 1 ) ;
						}
						else
						{
							aI.Add( o + i              ) ;
							aI.Add( o + i + 1          ) ;
							aI.Add( o + i     + s4 + 1 ) ;

							aI.Add( o + i + 1          ) ;
							aI.Add( o + i + 1 + s4 + 1 ) ;
							aI.Add( o + i     + s4 + 1 ) ;
						}
					}
				}
			}
			o = o + s4 + 1 ;
		}

		// 赤道から下の頂点
		for( ly  = 1 ; ly <  s1 ; ly ++ )
		{
			a  = ( 2.0f * Mathf.PI * ly *  90f ) / ( s1 * 360f ) ;
			r  =   Mathf.Cos( a ) ;
			y  = - Mathf.Sin( a ) ;
			ty = tdh * yp ; yp -- ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
//				x = - Mathf.Cos( a ) * r ;
//				z = - Mathf.Sin( a ) * r ;
				x = - Mathf.Sin( a ) * r ;
				z =   Mathf.Cos( a ) * r ;

				vx = hsx * x ;
				vy = qsy * y - qsy ;
				vz = hsz * z ;

				nx = x ;
				ny = y ;
				nz = z ;

				if( tDirection == Direction.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( tDirection == Direction.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;

				tx = tdw * i ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  ( s1 - 1 ) )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
					else
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}
		
		// 一番下の頂点
		x = 0 ;
		y = - 1.0f ;
		z = 0 ;
		ty = tdh * yp ; yp -- ;
		for( l  = 0 ; l <  s4 ; l ++ )
		{
			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx = x ;
			ny = y ;
			nz = z ;

			if( tDirection == Direction.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( tDirection == Direction.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = tdw * l + ( tdw * 0.5f ) ;
			aT.Add( new Vector2( tx, ty ) ) ;
		}
		
		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateCylinder( ShapeData tShapeData )
	{
		CreateCylinder
		(
			tShapeData.direction,
			tShapeData.offset.x,	tShapeData.offset.y,	tShapeData.offset.z,
			tShapeData.size.x,		tShapeData.size.y,		tShapeData.size.z,
			tShapeData.vertexColor,
			tShapeData.split
		) ;
	}

	/// <summary>
	/// シリンダーを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCylinder( Direction tDirection, float px, float py, float pz, float sx, float sy, float sz, Color c, int tSplit = 0 )
	{
		// 値を更新しておく
//		m_Offset      = new Vector3( px, py, pz ) ;
//		m_Size        = new Vector3( sx, sy, sz ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0, i ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;


		int s1, s4 ;
		int ly ;
		float a ;
		
		float vx, vy, vz, vs ;
		float nx, ny, nz, ns ;
		
		s1 = 1 << tSplit ;
		s4 = 4 << tSplit ;


		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;

		float x, y, z ;
		float tx, ty ;


		float tdw = ( 1 / ( float )s4 ) ;
		float tdh = ( 1 / ( float )s1 ) ;

		// 一番上の頂点
		x =  0 ;
		y =  1 ;
		z =  0 ;

		// 上の中心

		vx = hsx * x ;
		vy = hsy * y ;
		vz = hsz * z ;

		nx =  0 ;
		ny =  1 ;
		nz =  0 ;

		if( tDirection == Direction.X_Axis )
		{
			// ＸとＹを入れ替え
			vs = - vx ;
			vx =   vy ;
			vy =   vs ;

			ns = - nx ;
			nx =   ny ;
			ny =   ns ;
		}
		else
		if( tDirection == Direction.Z_Axis )
		{
			// ＺとＹを入れ替え
			vs = - vz ;
			vz =   vy ;
			vy =   vs ;

			ns = - nz ;
			nz =   ny ;
			ny =   ns ;
		}

		aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
		aN.Add( new Vector3( nx, ny, nz ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 上の外周
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
			x = - Mathf.Sin( a ) ;
			z =   Mathf.Cos( a ) ;

			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx =  0 ;
			ny =  1 ;
			nz =  0 ;

			if( tDirection == Direction.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( tDirection == Direction.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = (   x * 0.5f ) + 0.5f ;
			ty = ( - z * 0.5f ) + 0.5f ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o                      ) ;
			aI.Add( o + 1 + ( i + 1 ) % s4 ) ;
			aI.Add( o + 1 +   i            ) ;
		}
		o = o + 1 + s4 ;

		// 中
		for( ly  = 0 ; ly <= s1 ; ly ++ )
		{
			y = 1.0f - ( 2.0f / ( float )s1 ) * ly ;

			for( i  = 0 ; i <= s4 ; i ++ )
			{
				a = 2.0f * Mathf.PI * i / s4 ;
				x = - Mathf.Sin( a ) ;
				z =   Mathf.Cos( a ) ;

				vx = hsx * x ;
				vy = hsy * y ;
				vz = hsz * z ;

				nx =  x ;
				ny =  0 ;
				nz =  z ;

				if( tDirection == Direction.X_Axis )
				{
					// ＸとＹを入れ替え
					vs = - vx ;
					vx =   vy ;
					vy =   vs ;

					ns = - nx ;
					nx =   ny ;
					ny =   ns ;
				}
				else
				if( tDirection == Direction.Z_Axis )
				{
					// ＺとＹを入れ替え
					vs = - vz ;
					vz =   vy ;
					vy =   vs ;

					ns = - nz ;
					nz =   ny ;
					ny =   ns ;
				}

				aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
				aN.Add( new Vector3( nx, ny, nz ).normalized ) ;
				aC.Add( new Color( cr, cg, cb, ca ) ) ;
	
				tx = tdw * i ;
				ty = 1.0f - ( tdh * ly ) ;
				aT.Add( new Vector2( tx, ty ) ) ;

				if( i <  s4 )
				{
					if( ly <  s1 )
					{
						aI.Add( o + i              ) ;
						aI.Add( o + i + 1          ) ;
						aI.Add( o + i     + s4 + 1 ) ;

						aI.Add( o + i + 1          ) ;
						aI.Add( o + i + 1 + s4 + 1 ) ;
						aI.Add( o + i     + s4 + 1 ) ;
					}
				}
			}
			o = o + s4 + 1 ;
		}

		// 一番上の頂点
		x =  0 ;
		y = -1 ;
		z =  0 ;
		
		// 下の中心
		vx = hsx * x ;
		vy = hsy * y ;
		vz = hsz * z ;

		nx =  0 ;
		ny = -1 ;
		nz =  0 ;

		if( tDirection == Direction.X_Axis )
		{
			// ＸとＹを入れ替え
			vs = - vx ;
			vx =   vy ;
			vy =   vs ;

			ns = - nx ;
			nx =   ny ;
			ny =   ns ;
		}
		else
		if( tDirection == Direction.Z_Axis )
		{
			// ＺとＹを入れ替え
			vs = - vz ;
			vz =   vy ;
			vy =   vs ;

			ns = - nz ;
			nz =   ny ;
			ny =   ns ;
		}

		aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
		aN.Add( new Vector3( nx, ny, nz ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 上の外周
		for( i  = 0 ; i <  s4 ; i ++ )
		{
			a = 2.0f * Mathf.PI * i / s4 ;
			x = - Mathf.Sin( a ) ;
			z =   Mathf.Cos( a ) ;

			vx = hsx * x ;
			vy = hsy * y ;
			vz = hsz * z ;

			nx =  0 ;
			ny = -1 ;
			nz =  0 ;

			if( tDirection == Direction.X_Axis )
			{
				// ＸとＹを入れ替え
				vs = - vx ;
				vx =   vy ;
				vy =   vs ;

				ns = - nx ;
				nx =   ny ;
				ny =   ns ;
			}
			else
			if( tDirection == Direction.Z_Axis )
			{
				// ＺとＹを入れ替え
				vs = - vz ;
				vz =   vy ;
				vy =   vs ;

				ns = - nz ;
				nz =   ny ;
				ny =   ns ;
			}

			aV.Add( new Vector3( vx + px, vy + py, vz + pz ) ) ;
			aN.Add( new Vector3( nx, ny, nz ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;

			tx = (   x * 0.5f ) + 0.5f ;
			ty = ( - z * 0.5f ) + 0.5f ;
			aT.Add( new Vector2( tx, ty ) ) ;

			aI.Add( o                      ) ;
			aI.Add( o + 1 +   i            ) ;
			aI.Add( o + 1 + ( i + 1 ) % s4 ) ;
		}
		o = o + 1 + s4 ;


		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreatePlane( ShapeData tShapeData )
	{
		Rect tUV = new Rect( 0, 0, 1, 1 ) ;

		if( tShapeData.uv != null && tShapeData.uv.Length >  0 )
		{
			tUV = tShapeData.uv[ 0 ] ;
		}

		CreatePlane
		(
			tShapeData.planeDirection,
			tShapeData.offset.x,	tShapeData.offset.y,	tShapeData.offset.z,
			tShapeData.size.x,		tShapeData.size.y,		tShapeData.size.z,
			tShapeData.vertexColor,
			tUV,
			tShapeData.split,
			tShapeData.tiling
		) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreatePlane( PlaneDirection tPlaneDirection, float px, float py, float pz, float sx, float sy, float sz, Color c, Rect tUV, int tSplit = 0, bool tTiling = false )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
//		m_Offset      = new Vector3( px, py, pz ) ;
//		m_Size        = new Vector3( sx, sy, sz ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;
		
		// ４つの頂点
		float vx0, vy0, vz0 ;
		float vx1, vy1, vz1 ;
		float vx2, vy2, vz2 ;
		float vx3, vy3, vz3 ;

		float nx, ny, nz ;

		float tx0, ty0 ;
		float tx1, ty1 ;
		float tx2, ty2 ;
		float tx3, ty3 ;

		float tx, ty ;

		int lx, ly ;

		int s, s1 = 1 << tSplit, s2 = 2 << tSplit ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}

		//-----------------------------------------------------------

		if( tPlaneDirection == PlaneDirection.Front )
		{
			// 前
			vx0 = -1 ; vy0 = -1 ; vz0 =  0 ;
			vx1 = -1 ; vy1 =  1 ; vz1 =  0 ;
			vx2 =  1 ; vy2 =  1 ; vz2 =  0 ;
			vx3 =  1 ; vy3 = -1 ; vz3 =  0 ;

			nx =  0 ;
			ny =  0 ;
			nz = -1 ;
		}
		else
		if( tPlaneDirection == PlaneDirection.Back )
		{
			// 後
			vx0 =  1 ; vy0 = -1 ; vz0 =  0 ;
			vx1 =  1 ; vy1 =  1 ; vz1 =  0 ;
			vx2 = -1 ; vy2 =  1 ; vz2 =  0 ;
			vx3 = -1 ; vy3 = -1 ; vz3 =  0 ;

			nx =  0 ;
			ny =  0 ;
			nz =  1 ;
		}
		else
		if( tPlaneDirection == PlaneDirection.Left )
		{
			// 左
			vx0 =  0 ; vy0 = -1 ; vz0 = -1 ;
			vx1 =  0 ; vy1 =  1 ; vz1 = -1 ;
			vx2 =  0 ; vy2 =  1 ; vz2 =  1 ;
			vx3 =  0 ; vy3 = -1 ; vz3 =  1 ;

			nx =  1 ;
			ny =  0 ;
			nz =  0 ;
		}
		else
		if( tPlaneDirection == PlaneDirection.Right )
		{
			// 右
			vx0 =  0 ; vy0 = -1 ; vz0 =  1 ;
			vx1 =  0 ; vy1 =  1 ; vz1 =  1 ;
			vx2 =  0 ; vy2 =  1 ; vz2 = -1 ;
			vx3 =  0 ; vy3 = -1 ; vz3 = -1 ;

			nx = -1 ;
			ny =  0 ;
			nz =  0 ;
		}
		else
		if( tPlaneDirection == PlaneDirection.Top )
		{
			// 上
			vx0 = -1 ; vy0 =  0 ; vz0 =  1 ;
			vx1 = -1 ; vy1 =  0 ; vz1 = -1 ;
			vx2 =  1 ; vy2 =  0 ; vz2 = -1 ;
			vx3 =  1 ; vy3 =  0 ; vz3 =  1 ;

			nx =  0 ;
			ny = -1 ;
			nz =  0 ;
		}
		else
		if( tPlaneDirection == PlaneDirection.Bottom )
		{
			// 下
			vx0 = -1 ; vy0 =  0 ; vz0 = -1 ;
			vx1 = -1 ; vy1 =  0 ; vz1 =  1 ;
			vx2 =  1 ; vy2 =  0 ; vz2 =  1 ;
			vx3 =  1 ; vy3 =  0 ; vz3 = -1 ;

			nx =  0 ;
			ny =  1 ;
			nz =  0 ;
		}
		else
		{
			return ;
		}

//		if( tUV != null )
//		{
			tx0 = tUV.x ;
			ty0 = tUV.y ;
			tx1 = tUV.x ;
			ty1 = tUV.y + tUV.height ;
			tx2 = tUV.x + tUV.width ;
			ty2 = tUV.y + tUV.height ;
			tx3 = tUV.x + tUV.width ;
			ty3 = tUV.y ;
//		}
//		else
//		{
//			tx0 = 0 ;
//			ty0 = 0 ;
//			tx1 = 0 ;
//			ty1 = 1 ;
//			tx2 = 1 ;
//			ty2 = 1 ;
//			tx3 = 1 ;
//			ty3 = 0 ;
//		}
		
		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		float hsz = sz * 0.5f ;
		
		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// V
			float vlx0_dx = vx3 - vx0 ;
			float vlx0_dy = vy3 - vy0 ;
			float vlx0_dz = vz3 - vz0 ;

			float vlx1_dx = vx2 - vx1 ;
			float vlx1_dy = vy2 - vy1 ;
			float vlx1_dz = vz2 - vz1 ;

			float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
			float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
			float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

			float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
			float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
			float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

			float vly_dx = vlx1_x - vlx0_x ;
			float vly_dy = vlx1_y - vlx0_y ;
			float vly_dz = vlx1_z - vlx0_z ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
				float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
				float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

				tV[ lx ][ ly ] = new Vector3( vx * hsx, vy * hsy, vz * hsz ) ;
			}
		}

		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// T
			float tlx0_dx = tx3 - tx0 ;
			float tlx0_dy = ty3 - ty0 ;

			float tlx1_dx = tx2 - tx1 ;
			float tlx1_dy = ty2 - ty1 ;

			float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
			float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

			float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
			float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

			float tly_dx = tlx1_x - tlx0_x ;
			float tly_dy = tlx1_y - tlx0_y ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				tx = tlx0_x + ( tly_dx * ly / s1 ) ;
				ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
				tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
			}
		}

		if( tTiling == false )
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}
		else
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx0 ;
					y = tx0 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx1 ;
					y = ty1 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx2 ;
					y = ty2 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx3 ;
					y = ty3 ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}
	
	//------------------------------------------------------------

	private void CreateBox2D( ShapeData tShapeData )
	{
		Rect tUV = new Rect( 0, 0, 1, 1 ) ;

		if( tShapeData.uv != null && tShapeData.uv.Length >  0 )
		{
			tUV = tShapeData.uv[ 0 ] ;
		}

		CreateBox2D
		(
			tShapeData.offset.x,	tShapeData.offset.y,
			tShapeData.size.x,		tShapeData.size.y,
			tShapeData.vertexColor,
			tUV,
			tShapeData.split,
			tShapeData.tiling
		) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateBox2D( float px, float py, float sx, float sy, Color c, Rect tUV, int tSplit = 0, bool tTiling = false )
	{
		// 値を更新しておく(プログラムから更新生成される場合用)
//		m_Offset      = new Vector3( px, py, m_Offset.z ) ;
//		m_Size        = new Vector3( sx, sy, m_Size.z ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float pz = 0 ;

		float x, y, z ;
		float r = 1, g = 1, b = 1, a = 1 ;
		int i = 0 ;

		r = c.r ;
		g = c.g ;
		b = c.b ;
		a = c.a ;
		
		// ４つの頂点
		float vx0, vy0, vz0 ;
		float vx1, vy1, vz1 ;
		float vx2, vy2, vz2 ;
		float vx3, vy3, vz3 ;

		float nx, ny, nz ;

		float tx0, ty0 ;
		float tx1, ty1 ;
		float tx2, ty2 ;
		float tx3, ty3 ;

		float tx, ty ;

		int lx, ly ;

		int s, s1 = 1 << tSplit, s2 = 2 << tSplit ;

		Vector3[][] tV = new Vector3[ s2 ][] ;
		for( s  = 0 ; s <  tV.Length ; s ++ )
		{
			tV[ s ] = new Vector3[ s2 ] ;
		}

		Vector2[][] tT = new Vector2[ s2 ][] ;
		for( s  = 0 ; s <  tT.Length ; s ++ )
		{
			tT[ s ] = new Vector2[ s2 ] ;
		}

		//-----------------------------------------------------------

		// 前
		vx0 = -1 ; vy0 = -1 ; vz0 =  0 ;
		vx1 = -1 ; vy1 =  1 ; vz1 =  0 ;
		vx2 =  1 ; vy2 =  1 ; vz2 =  0 ;
		vx3 =  1 ; vy3 = -1 ; vz3 =  0 ;

		nx =  0 ;
		ny =  0 ;
		nz = -1 ;

//		if( tUV != null )
//		{
			tx0 = tUV.x ;
			ty0 = tUV.y ;
			tx1 = tUV.x ;
			ty1 = tUV.y + tUV.height ;
			tx2 = tUV.x + tUV.width ;
			ty2 = tUV.y + tUV.height ;
			tx3 = tUV.x + tUV.width ;
			ty3 = tUV.y ;
//		}
//		else
//		{
//			tx0 = 0 ;
//			ty0 = 0 ;
//			tx1 = 0 ;
//			ty1 = 1 ;
//			tx2 = 1 ;
//			ty2 = 1 ;
//			tx3 = 1 ;
//			ty3 = 0 ;
//		}
		
		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;
		
		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// V
			float vlx0_dx = vx3 - vx0 ;
			float vlx0_dy = vy3 - vy0 ;
			float vlx0_dz = vz3 - vz0 ;

			float vlx1_dx = vx2 - vx1 ;
			float vlx1_dy = vy2 - vy1 ;
			float vlx1_dz = vz2 - vz1 ;

			float vlx0_x = vx0 + ( vlx0_dx * lx / s1 ) ;
			float vlx0_y = vy0 + ( vlx0_dy * lx / s1 ) ;
			float vlx0_z = vz0 + ( vlx0_dz * lx / s1 ) ;

			float vlx1_x = vx1 + ( vlx1_dx * lx / s1 ) ;
			float vlx1_y = vy1 + ( vlx1_dy * lx / s1 ) ;
			float vlx1_z = vz1 + ( vlx1_dz * lx / s1 ) ;

			float vly_dx = vlx1_x - vlx0_x ;
			float vly_dy = vlx1_y - vlx0_y ;
			float vly_dz = vlx1_z - vlx0_z ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				float vx = vlx0_x + ( vly_dx * ly / s1 ) ;
				float vy = vlx0_y + ( vly_dy * ly / s1 ) ;
				float vz = vlx0_z + ( vly_dz * ly / s1 ) ;

				tV[ lx ][ ly ] = new Vector3( vx * hsx, vy * hsy, vz ) ;
			}
		}

		for( lx  = 0 ; lx <  s2 ; lx ++ )
		{
			// T
			float tlx0_dx = tx3 - tx0 ;
			float tlx0_dy = ty3 - ty0 ;

			float tlx1_dx = tx2 - tx1 ;
			float tlx1_dy = ty2 - ty1 ;

			float tlx0_x = tx0 + ( tlx0_dx * lx / s1 ) ;
			float tlx0_y = ty0 + ( tlx0_dy * lx / s1 ) ;

			float tlx1_x = tx1 + ( tlx1_dx * lx / s1 ) ;
			float tlx1_y = ty1 + ( tlx1_dy * lx / s1 ) ;

			float tly_dx = tlx1_x - tlx0_x ;
			float tly_dy = tlx1_y - tlx0_y ;

			for( ly  = 0 ; ly <  s2 ; ly ++  )
			{
				tx = tlx0_x + ( tly_dx * ly / s1 ) ;
				ty = tlx0_y + ( tly_dy * ly / s1 ) ;
	
				tT[ lx ][ ly ] = new Vector2( tx, ty ) ;
			}
		}

		if( tTiling == false )
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly ].x ;
					y = tT[ lx ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx ][ ly + 1 ].x ;
					y = tT[ lx ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly + 1 ].x ;
					y = tT[ lx + 1 ][ ly + 1 ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tT[ lx + 1 ][ ly ].x ;
					y = tT[ lx + 1 ][ ly ].y ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}
		else
		{
			for( lx  = 0 ; lx <  s1 ; lx ++ )
			{
				for( ly  = 0 ; ly <  s1 ; ly ++ )
				{
					// 0
					x = tV[ lx ][ ly ].x + px ;
					y = tV[ lx ][ ly ].y + py ;
					z = tV[ lx ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx0 ;
					y = tx0 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 1
					x = tV[ lx ][ ly + 1 ].x + px ;
					y = tV[ lx ][ ly + 1 ].y + py ;
					z = tV[ lx ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx1 ;
					y = ty1 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 2
					x = tV[ lx + 1 ][ ly + 1 ].x + px ;
					y = tV[ lx + 1 ][ ly + 1 ].y + py ;
					z = tV[ lx + 1 ][ ly + 1 ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx2 ;
					y = ty2 ;
					aT.Add( new Vector2( x, y ) ) ;

					// 3
					x = tV[ lx + 1 ][ ly ].x + px ;
					y = tV[ lx + 1 ][ ly ].y + py ;
					z = tV[ lx + 1 ][ ly ].z + pz ;
					aV.Add( new Vector3( x, y, z ) ) ;
					aN.Add( new Vector3( nx, ny, nz ) ) ; 
					aC.Add( new Color( r, g, b, a ) ) ;
					x = tx3 ;
					y = ty3 ;
					aT.Add( new Vector2( x, y ) ) ;

					aI.Add( i + 0 ) ;
					aI.Add( i + 1 ) ;
					aI.Add( i + 2 ) ;
				
					aI.Add( i + 0 ) ;
					aI.Add( i + 2 ) ;
					aI.Add( i + 3 ) ;

					i = i + 4 ;
				}
			}
		}

		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}

	//------------------------------------------------------------

	private void CreateCircle2D( ShapeData tShapeData )
	{
		CreateCircle2D
		(
			tShapeData.offset.x,	tShapeData.offset.y,
			tShapeData.size.x,		tShapeData.size.y,
			tShapeData.vertexColor,
			tShapeData.split
		) ;
	}

	/// <summary>
	/// プレーンを生成する
	/// </summary>
	/// <param name="px"></param>
	/// <param name="py"></param>
	/// <param name="pz"></param>
	/// <param name="sx"></param>
	/// <param name="sy"></param>
	/// <param name="sz"></param>
	/// <param name="c"></param>
	/// <param name="tSplit"></param>
	public void CreateCircle2D( float px, float py, float sx, float sy, Color c, int tSplit = 0 )
	{
		// 値を更新しておく
//		m_Offset      = new Vector3( px, py, m_Offset.z ) ;
//		m_Size        = new Vector3( sx, sy, m_Size.z ) ;
//		m_VertexColor = c ;
//		m_Split       = tSplit ;
		
		List<Vector3> aV = new List<Vector3>() ;
		List<Vector3> aN = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float cr = 1, cg = 1, cb = 1, ca = 1 ;
		int o = 0 ;

		cr = c.r ;
		cg = c.g ;
		cb = c.b ;
		ca = c.a ;

		// 中心頂点
		aV.Add( new Vector3( px, py,  0 ) ) ;
		aN.Add( new Vector3(  0,  0, -1 ) ) ;
		aC.Add( new Color( cr, cg, cb, ca ) ) ;
		aT.Add( new Vector2( 0.5f, 0.5f ) ) ;

		// 分割数
		int s = 4 << tSplit ;

		float hsx = sx * 0.5f ;
		float hsy = sy * 0.5f ;

		float rx, ry, ra ;

		int i ;

		for( i  = 0 ; i <= s ; i ++ )
		{
			ra = 2.0f * Mathf.PI * ( float )i / ( float )s ;
			rx = Mathf.Cos( ra ) ;
			ry = Mathf.Sin( ra ) ;

			aV.Add( new Vector3( rx * hsx + px, ry * hsy + py,  0 ) ) ;
			aN.Add( new Vector3(  0,  0, -1 ) ) ;
			aC.Add( new Color( cr, cg, cb, ca ) ) ;
			aT.Add( new Vector2( 0.5f + rx * 0.5f, 0.5f + ry * 0.5f ) ) ;
		}

		o = 1 ;
		for( i  = 0 ; i <  s ; i ++ )
		{
			aI.Add( 0 ) ;
			aI.Add( o + 1 ) ;
			aI.Add( o ) ;

			o ++ ;
		}

		//-----------------------------------------------------------

		Add( aV.ToArray(), aN.ToArray(), aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}
}
