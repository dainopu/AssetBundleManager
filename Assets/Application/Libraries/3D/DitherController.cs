using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DitherHelper
{
	public class DitherController : MonoBehaviour
	{
		private Texture[]				m_DitherPattern = null ;
		private SkinnedMeshRenderer[]	m_Renderer = null ;

		[SerializeField][Range(0, 1)]
		private float m_Alpha = 1 ;

		private const int m_Pattern = 16 ;

		public float alpha
		{
			get
			{
				return m_Alpha ;
			}
			set
			{
				if( m_Alpha != value )
				{
					UpdateDitherPattern( value ) ;
					m_Alpha  = value ;
				}
			}
		}

		void Awake()
		{
			Prepare() ;
		}

		private void Prepare()
		{
			int i, l = m_Pattern + 1 ;

			m_DitherPattern = new Texture[ l ] ;

			string tPath ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tPath = "Textures/DitherPattern/DitherPattern" + i.ToString( "D2" ) ;
				m_DitherPattern[ i ] = Resources.Load<Texture>( tPath ) ;
			}

			m_Renderer = GetComponentsInChildren<SkinnedMeshRenderer>() ;
		}

	#if UNITY_EDITOR
		// インスペクターからの変更テスト
		void OnValidate()
		{
//			Prepare() ;
//

			if( Application.isPlaying == true )
			{
				UpdateDitherPattern( m_Alpha ) ;
			}
		}
	#endif

		private void UpdateDitherPattern( float tAlpha )
		{
			if( m_DitherPattern == null || m_Renderer == null )
			{
				return ;
			}

			int tIndex = ( int )( tAlpha * m_Pattern + 0.5f ) ;

			Material tMaterial ;

			int i, l = m_Renderer.Length ;
			int j, m ;

/*			if( Application.isPlaying == false )
			{
				// 停止中
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Renderer[ i ].sharedMaterials != null && m_Renderer[ i ].sharedMaterials.Length >  0 )
					{
						m = m_Renderer[ i ].sharedMaterials.Length ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							if( m_Renderer[ i ].sharedMaterials[ j ] != null )
							{
								tMaterial = m_Renderer[ i ].sharedMaterials[ j ] ;
								if( tMaterial.HasProperty( "_MaskTex" ) == true )
								{
									tMaterial.SetTexture( "_MaskTex", m_DitherPattern[ tIndex ] ) ;
								}
							}
						}
					}
				}
			}
			else
			{*/
				// 実行中
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Renderer[ i ].materials != null && m_Renderer[ i ].materials.Length >  0 )
					{
						m = m_Renderer[ i ].materials.Length ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							if( m_Renderer[ i ].materials[ j ] != null )
							{
								tMaterial = m_Renderer[ i ].materials[ j ] ;
								if( tMaterial.HasProperty( "_MaskTex" ) == true )
								{
									tMaterial.SetTexture( "_MaskTex", m_DitherPattern[ tIndex ] ) ;
								}
							}
						}
					}
				}
/*			}*/
		}
	}
}

