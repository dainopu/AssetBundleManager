using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;

//#if TextMeshPro
using TMPro ;
//#endif

namespace uGUIHelper
{
	public class UIAlphaMaskWindow : MonoBehaviour
	{
		/// <summary>
		/// アルファマスク対象に使うマテリアル(シェーダー)
		/// </summary>
		public Material		alphaMaskMaterial = null ;

		/// <summary>
		/// アルファマスクのソフトネス
		/// </summary>
		public Vector2		alphaMaskSoftness = new Vector2( 16, 16 ) ;

		/// <summary>
		/// アルファマスクを全ての子に影響させるかどうか
		/// </summary>
		public bool			alphaMaskEffectToAllItem = true ;

		/// <summary>
		/// アルファマスクを自身に影響させるかどうか
		/// </summary>
		public bool			alphaMaskEffectToMyself = false ;

		//-----------------------------------------------------------

		private Material	m_AlphaMaskMaterial = null ;

//#if TextMeshPro
		private Dictionary<Material,Material>	m_AlphaMaskMaterialForTextMesh = null ;
//#endif
		//-------------------------------------------------------------------------------------------

		void Awake()
		{
			if( alphaMaskMaterial == null )
			{
				bool tOverlay = false ;
				UIView tView = GetComponent<UIView>() ;
				if( tView.isCanvasOverlay == true )
				{
					tOverlay = true ;
				}

				if( tOverlay == false )
				{
					m_AlphaMaskMaterial = GameObject.Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-AlphaMask" ) ) ;
				}
				else
				{
					m_AlphaMaskMaterial = GameObject.Instantiate( Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-AlphaMask" ) ) ;
				}
			}
			else
			{
				m_AlphaMaskMaterial = alphaMaskMaterial ;
			}

//#if TextMeshPro
			m_AlphaMaskMaterialForTextMesh = new Dictionary<Material, Material>() ;
//#endif

			UpdateSoftness() ;
		}

		void Start()
		{
			Prepare() ;
			Refresh() ;
		}

		public void Prepare()
		{
			if( Application.isPlaying == true &&  alphaMaskEffectToAllItem == true )
			{
				// アルファマスクを有効にしているのでテンプレートアイテムに適用する

				UIView[] tElement = GetComponentsInChildren<UIView>( true ) ;

				if( tElement != null )
				{
					int i, l = tElement.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( alphaMaskEffectToMyself == false && gameObject != tElement[ i ].gameObject )
						{
							if( tElement[ i ].isGraphic == true && tElement[ i ].isMask == false && tElement[ i ].isRectMask2D == false && tElement[ i ].isAlphaMaskTarget == false )
							{
								tElement[ i ].isAlphaMaskTarget  = true ;
							}
						}
					}
				}
			}
		}

		public void Refresh()
		{
			if( Application.isPlaying == true && alphaMaskEffectToAllItem == true )
			{
				// アルファマスクを有効にしているのでテンプレートアイテムに適用する

				UIAlphaMaskTarget[] tElement = GetComponentsInChildren<UIAlphaMaskTarget>( true ) ;

				if( tElement != null )
				{
					int i, l = tElement.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tElement[ i ].RefreshAlphaMask() ;
					}
				}
			}
		}

//		void OnTransformChildrenChanged()
//		{
//			Debug.LogWarning( "-------- 子に変化がありました" ) ;
//			Prepare() ;
//			Refresh() ;
//		}

		void OnDestroy()
		{
//#if TextMeshPro
			if( m_AlphaMaskMaterialForTextMesh != null )
			{
				int i, l = m_AlphaMaskMaterialForTextMesh.Count ;
				if( l >  0 )
				{
					Material[] tBasisMaterials = new Material[ l ] ;
					m_AlphaMaskMaterialForTextMesh.Keys.CopyTo( tBasisMaterials, 0 ) ;

					Material tCloneMaterial ;
					for( i  = ( l - 1 ) ; i >= 0 ; i -- )
					{
						tCloneMaterial = m_AlphaMaskMaterialForTextMesh[ tBasisMaterials[ i ] ] ;
						if( tCloneMaterial != null )
						{
							DestroyImmediate( tCloneMaterial ) ;
						}
						m_AlphaMaskMaterialForTextMesh[ tBasisMaterials[ i ] ] = null ;
					}
				}
				m_AlphaMaskMaterialForTextMesh.Clear() ;
				m_AlphaMaskMaterialForTextMesh = null ;
			}
//#endif
			if( alphaMaskMaterial == null && m_AlphaMaskMaterial != null )
			{
				DestroyImmediate( m_AlphaMaskMaterial ) ;
				m_AlphaMaskMaterial = null ;
			}
		}

		/// <summary>
		/// 子がアルファマテリアルを取得する際に呼び出すメソッド
		/// </summary>
		/// <returns></returns>
		public Material GetAlphaMaskMaterial()
		{
			if( enabled == false )
			{
				return null ;
			}

			return m_AlphaMaskMaterial ;
		}

//#if TextMeshPro
		/// <summary>
		/// 子がアルファマテリアルを取得する際に呼び出すメソッド(TextMesh用)
		/// </summary>
		/// <param name="tBasisMaterial"></param>
		/// <returns></returns>
		public Material GetAlphaMaskMaterialForTextMesh( Material tBasisMaterial )
		{
			if( m_AlphaMaskMaterialForTextMesh != null )
			{
				if( m_AlphaMaskMaterialForTextMesh.ContainsKey( tBasisMaterial ) == false )
				{
					// これに対応するマテリアルは未作成
					Material tCloneMaterial = GameObject.Instantiate<Material>( tBasisMaterial ) ;

					m_AlphaMaskMaterialForTextMesh.Add( tBasisMaterial, tCloneMaterial ) ;

					UpdateSoftness() ;

					return tCloneMaterial ;
				}
				else
				{
					// これに対応するマテリアルは生成済
					return m_AlphaMaskMaterialForTextMesh[ tBasisMaterial ] ;
				}
			}
			else
			{
				return null ;
			}
		}
//#endif

		//-------------------------------------------------------------------------------------------

		// 毎フレーム値を更新する
		void Update()
		{
			UpdateSoftness() ;
		}

		private void UpdateSoftness()
		{
			if( m_AlphaMaskMaterial != null )
			{
				if( m_AlphaMaskMaterial.HasProperty( "_MaskSoftnessX" ) == true )
				{
					m_AlphaMaskMaterial.SetFloat(	"_MaskSoftnessX",	alphaMaskSoftness.x ) ;
				}
				if( m_AlphaMaskMaterial.HasProperty( "_MaskSoftnessY" ) == true )
				{
					m_AlphaMaskMaterial.SetFloat(	"_MaskSoftnessY",	alphaMaskSoftness.y ) ;
				}
			}

//#if TextMeshPro
			if( m_AlphaMaskMaterialForTextMesh != null )
			{
				int i, l = m_AlphaMaskMaterialForTextMesh.Count ;
				if( l >  0 )
				{
					Material[] tBasisMaterials = new Material[ l ] ;
					m_AlphaMaskMaterialForTextMesh.Keys.CopyTo( tBasisMaterials, 0 ) ;

					Material tCloneMaterial ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tCloneMaterial = m_AlphaMaskMaterialForTextMesh[ tBasisMaterials[ i ] ] ;
						if( tCloneMaterial != null )
						{
							if( tCloneMaterial.HasProperty( "_MaskSoftnessX" ) == true )
							{
								tCloneMaterial.SetFloat(	"_MaskSoftnessX",	alphaMaskSoftness.x ) ;
							}
							if( tCloneMaterial.HasProperty( "_MaskSoftnessY" ) == true )
							{
								tCloneMaterial.SetFloat(	"_MaskSoftnessY",	alphaMaskSoftness.y ) ;
							}
						}
					}
				}
			}
//#endif
		}
	}
}
