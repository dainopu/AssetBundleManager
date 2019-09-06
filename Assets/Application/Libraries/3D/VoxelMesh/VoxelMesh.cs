using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

[ExecuteInEditMode]



public class VoxelMesh : MonoBehaviour
{
	[SerializeField][HideInInspector]
	private MeshRenderer	mMeshRenderer ;
	private MeshRenderer	_MeshRenderer
	{
		get
		{
			if( mMeshRenderer != null )
			{
				return mMeshRenderer ;
			}
			mMeshRenderer = GetComponent<MeshRenderer>() ;
			return mMeshRenderer ;
		}
	}

	[SerializeField][HideInInspector]
	private MeshFilter		mMeshFilter ;
	private MeshFilter		_MeshFilter
	{
		get
		{
			if( mMeshFilter != null )
			{
				return mMeshFilter ;
			}
			mMeshFilter = GetComponent<MeshFilter>() ;
			return mMeshFilter ;
		}
	}


	[SerializeField][HideInInspector]
	private Vector3 mOffset = Vector3.zero ;
	public  Vector3  offset
	{
		get
		{
			return mOffset ;
		}
		set
		{
			if( mOffset.Equals( value ) == false )
			{
				mOffset = value ;
				UpdateMesh() ;
			}
		}
	}

	[SerializeField][HideInInspector]
	private float mVoxelScale = 1.0f ;
	public  float  voxelScale
	{
		get
		{
			return mVoxelScale ;
		}
		set
		{
			if( mVoxelScale != value )
			{
				mVoxelScale = value ;
				UpdateMesh() ;
			}
		}
	}	




	[SerializeField][HideInInspector]
	private Material mMaterial = null ;
	public  Material  material
	{
		get
		{
			return mMaterial ;
		}
		set
		{
			if( mMaterial != value )
			{
				if( mMeshRenderer != null )
				{
					Texture tTexture = null ;

					if( mMeshRenderer.sharedMaterial != null )
					{
						tTexture = mMeshRenderer.sharedMaterial.mainTexture ;
					}

					mMaterial = value ;
					mMeshRenderer.sharedMaterial = mMaterial ;

					if( mMeshRenderer.sharedMaterial == null )
					{
						return ;
					}

					if( mMeshRenderer.sharedMaterial != null && tTexture != null && tTexture == mTexture )
					{
						mMeshRenderer.sharedMaterial.mainTexture = tTexture ;
					}
				}
			}
		}
	}

	[SerializeField][HideInInspector]
	private Texture mTexture = null ;
	public  Texture	 texture
	{
		get
		{
			return mTexture ;
		}
		set
		{
			if( mTexture != value )
			{
				mTexture = value ;

				if( mMeshRenderer == null || mMeshRenderer.sharedMaterial == null )
				{
					return ;
				}
	
				if( mMeshRenderer.sharedMaterial.mainTexture != mTexture )
				{
					mMeshRenderer.sharedMaterial.mainTexture  = mTexture ;
				}
			}
		}
	}




	[SerializeField][HideInInspector]
	private Texture2D mVoxelTexture = null ;
	public  Texture2D  voxelTexture
	{
		get
		{
			return mVoxelTexture ;
		}
		set
		{
			if( mVoxelTexture != value )
			{
				mVoxelTexture = value ;
				UpdateMesh() ;
			}
		}
	}


	[SerializeField][HideInInspector]
	private bool mVoxelTextureReadable = false ;
	public  bool  voxelTextureReadable
	{
		get
		{
			return mVoxelTextureReadable ;
		}
		set
		{
			if( mVoxelTextureReadable != value )
			{
				mVoxelTextureReadable  = value ;
//				UpdateMesh() ;
			}
		}
	}



	void Awake()
	{
		mMeshRenderer	= GetComponent<MeshRenderer>() ;
		if( mMeshRenderer != null )
		{
			if( mMeshRenderer.sharedMaterial == null )
			{
				if( mMaterial == null )
				{
					mMaterial = new Material( Shader.Find( "Standard" ) ) ;
				}

				mMeshRenderer.sharedMaterial = mMaterial ;

				if( mTexture != null )
				{
					mMeshRenderer.sharedMaterial.mainTexture  = mTexture ;
				}
			}
		}

		mMeshFilter		= GetComponent<MeshFilter>() ;
		if( mMeshFilter != null )
		{
			if( mMeshFilter.sharedMesh == null )
			{
				mMeshFilter.sharedMesh = new Mesh() ;	
			}
			else
			{
				mMeshFilter.sharedMesh.Clear() ;
			}
		}

		UpdateMesh() ;
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

		if( tMeshFilter.sharedMesh == null )
		{
			return ;
		}

		tMeshFilter.sharedMesh.Clear() ;
	}

	private void UpdateMesh()
	{
		CreateVoxelMesh() ;
	}



	/// <summary>
	/// メッシュを更新する
	/// </summary>
	/// <param name="aVD"></param>
	/// <param name="aCD"></param>
	/// <param name="aTD"></param>
	/// <param name="aID"></param>
	public void Build( string tName, Vector3[] aV, Vector3[] aN, Color[] aC, Vector2[] aT, int[] aI )
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
		if( aN != null )
		{
			tMeshFilter.sharedMesh.normals   = aN ;
		}
		tMeshFilter.sharedMesh.colors    = aC ;
		tMeshFilter.sharedMesh.uv        = aT ;
		tMeshFilter.sharedMesh.triangles = aI ;
		
		if( aN == null )
		{
			tMeshFilter.sharedMesh.RecalculateNormals() ;
		}
		tMeshFilter.sharedMesh.RecalculateBounds() ;
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
	public void CreateVoxelMesh()
	{
		if( mVoxelTexture == null || mVoxelTextureReadable == false )
		{
			Clear() ;
			return ;
		}

		//----------------------------------------------

		int w = mVoxelTexture.width ;
		int h = mVoxelTexture.height ;

		Color32[] cd = mVoxelTexture.GetPixels32( 0 ) ;

		List<Vector3> aV = new List<Vector3>() ;
		List<Color>   aC = new List<Color>() ;
		List<Vector2> aT = new List<Vector2>() ;
		List<int>     aI = new List<int>() ;
		
		float sx = - ( float )w * 0.5f + 0.5f ;
		float sy =   ( float )h        + 0.5f ;

		Color32 c ;
		int x, y, i, o = 0 ;
		float x0, y0, x1, y1,z0, z1 ;
		float vr = 0.5f * mVoxelScale ;

		for( y  = 0 ; y <  h ; y ++ )
		{
			for( x   = 0 ; x <  w ; x ++ )
			{
				i = ( h - 1 - y ) * w + x ;
				c = cd[ i ] ;
				if( c.a != 0 )
				{
					x0 = sx + x - vr + mOffset.x ;
					x1 = sx + x + vr + mOffset.x ;
					y0 = sy - y + vr + mOffset.y ;
					y1 = sy - y - vr + mOffset.y ;
					z0 =        - vr + mOffset.z ;
					z1 =          vr + mOffset.z ;

					// 表
					aV.Add( new Vector3( x0, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x1, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 1 ) ) ;

					aV.Add( new Vector3( x0, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aV.Add( new Vector3( x1, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;

					// 裏
					aV.Add( new Vector3( x1, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aV.Add( new Vector3( x0, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x1, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aV.Add( new Vector3( x0, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;

					// 左
					aV.Add( new Vector3( x0, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x0, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 1 ) ) ;

					aV.Add( new Vector3( x0, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aV.Add( new Vector3( x0, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;

					// 右
					aV.Add( new Vector3( x1, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x1, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 1 ) ) ;

					aV.Add( new Vector3( x1, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aV.Add( new Vector3( x1, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;

					// 上
					aV.Add( new Vector3( x0, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x1, y0, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 1 ) ) ;

					aV.Add( new Vector3( x0, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aV.Add( new Vector3( x1, y0, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;

					// 下
					aV.Add( new Vector3( x0, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 1 ) ) ;

					aV.Add( new Vector3( x1, y1, z0 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 1 ) ) ;

					aV.Add( new Vector3( x0, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 0, 0 ) ) ;

					aV.Add( new Vector3( x1, y1, z1 ) ) ;
					aC.Add( new Color32( c.r, c.g, c.b, c.a ) ) ;
					aT.Add( new Vector2( 1, 0 ) ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;
					aI.Add( o + 2 ) ;

					o = o + 4 ;
				}
			}
		}


		Build( "VoxelMesh", aV.ToArray(), null, aC.ToArray(), aT.ToArray(), aI.ToArray() ) ;
	}



	void Start()
	{
	}
	
	void Update()
	{
	}
}
