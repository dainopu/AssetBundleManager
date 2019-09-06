using System ;
using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.Rendering ;

//#if TextMeshPro
using TMPro ;
//#endif

namespace uGUIHelper
{
	public class UIAlphaMaskTarget : UIBehaviour, IMaterialModifier
	{
		/// <summary>
		/// カスタムマテリアル
		/// </summary>
		public Material		alphaMaskMaterial = null ;

		//-----------------------------------------------------------

		private Material	m_AlphaMaskMaterial = null ;

		private bool		m_ShouldRecalculateStencil = true ;

		private Material	m_UsingMaskMaterial = null ;

		private Graphic		m_Graphic = null ;

//#if TextMeshPro
		private TextMeshProUGUI	m_TextMeshProUGUI = null ;
//#endif

		//-------------------------------------------------------------------------------------------

		//		override protected void Awake()
		//		{
		//			base.Awake() ;
		//		}

		/// <summary>
		/// マテリアルの設定が必要な際に呼び出される
		/// </summary>
		/// <param name="tBaseMaterial"></param>
		/// <returns></returns>
		public virtual Material GetModifiedMaterial( Material tBaseMaterial )
        {
//			if( name == "1" )
//			{
//				Debug.LogWarning( "-------> GetModifiedMaterial:" + name ) ;
//			}

			Material tActiveMaterial = ChoiceMaterial( tBaseMaterial ) ;

			int tStencilValue = 0 ;

			if( m_ShouldRecalculateStencil == true )
			{
				Transform tRootCanvas = MaskUtilities.FindRootSortOverrideCanvas( transform ) ;
				tStencilValue =  MaskUtilities.GetStencilDepth( transform, tRootCanvas ) ;
                m_ShouldRecalculateStencil = false ;
            }

			if( tStencilValue >  0 )
			{
				Material tMargedMaskMaterial = StencilMaterial.Add( tActiveMaterial, ( 1 << tStencilValue ) - 1, StencilOp.Keep, CompareFunction.Equal, ColorWriteMask.All, ( 1 << tStencilValue ) - 1, 0 ) ;

				if( m_UsingMaskMaterial != null )
				{
	                StencilMaterial.Remove( m_UsingMaskMaterial ) ;
				}

				m_UsingMaskMaterial = tMargedMaskMaterial ;

                tActiveMaterial = m_UsingMaskMaterial ;
            }

			return tActiveMaterial ;
		}

		protected override void OnEnable()
		{
			base.OnEnable() ;

			// マテリアルが独自に設定されている場合は複製する
			if( alphaMaskMaterial != null )
			{
				m_AlphaMaskMaterial = GameObject.Instantiate<Material>( alphaMaskMaterial ) ;
			}

			m_ShouldRecalculateStencil = true ;
		}

		protected override void OnDisable()
		{
			base.OnDisable() ;

			m_ShouldRecalculateStencil = true ;

			if( m_UsingMaskMaterial != null )
			{
				StencilMaterial.Remove( m_UsingMaskMaterial ) ;
				m_UsingMaskMaterial = null ;
			}

			if( m_AlphaMaskMaterial != null )
			{
				DestroyImmediate( m_AlphaMaskMaterial ) ;
				m_AlphaMaskMaterial = null ;
			}
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate() ;

			m_ShouldRecalculateStencil = true ;
		}
#endif

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged() ;

			m_ShouldRecalculateStencil = true ;
		}

		protected override void OnCanvasHierarchyChanged()
		{
			base.OnCanvasHierarchyChanged() ;

			m_ShouldRecalculateStencil = true ;
		}

		public virtual void RecalculateMasking()
		{
			m_ShouldRecalculateStencil = true ;
		}

		/// <summary>
		/// アルファマスクマテリアルを更新する
		/// </summary>
		public void RefreshAlphaMask()
		{
			m_ShouldRecalculateStencil = true ;

			if( m_Graphic == null )
			{
				m_Graphic = GetComponent<Graphic>() ;
			}

			if( m_Graphic != null )
			{
				m_Graphic.SetMaterialDirty() ;
			}
		}

		// 使用するマテリアルを選別する
		private Material ChoiceMaterial( Material tBasisMaterial )
		{
			if( enabled == false )
			{
				return tBasisMaterial ;
			}

			if( m_AlphaMaskMaterial != null )
			{
				return m_AlphaMaskMaterial ;
			}

			Material tCloneMaterial ;

			//----------------------------------------------------------
			// TextMesh

//#if TextMeshPro
			if( m_TextMeshProUGUI == null )
			{
				m_TextMeshProUGUI = GetComponent<TextMeshProUGUI>() ;
			}

			if( m_TextMeshProUGUI != null )
			{
				tCloneMaterial = GetAlphaMaskMaterialForTextMesh( tBasisMaterial ) ;
				if( tCloneMaterial != null )
				{
					return tCloneMaterial ;
				}
				else
				{
					return tBasisMaterial ;
				}
			}
//#endif

			//----------------------------------------------------------
			// Text

			// AlphaMaskWindow のマテリアルを取得する
			tCloneMaterial = GetAlphaMaskMaterial() ;
			if( tCloneMaterial != null )
			{
				return tCloneMaterial ;
			}
			else
			{
				return tBasisMaterial ;
			}
		}


		// AlphaMaskWindow のマテリアルを取得する
		private Material GetAlphaMaskMaterial()
		{
			Transform tTransform = transform ;

			UIAlphaMaskWindow tWindow = null ;

			int i, l = 64 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTransform.GetComponent<Canvas>() != null )
				{
					break ;
				}
				
				tWindow = tTransform.GetComponent<UIAlphaMaskWindow>() ;
				if( tWindow != null )
				{
					break ;
				}

				if( tTransform.parent == null )
				{
					break ;
				}

				tTransform = tTransform.parent ;
			}

			if( tWindow == null )
			{
				return null ;
			}

			return tWindow.GetAlphaMaskMaterial() ;
		}

//#if TextMeshPro
		// AlphaMaskWindow のマテリアルを取得する
		private Material GetAlphaMaskMaterialForTextMesh( Material tBasisMaterial )
		{
			Transform tTransform = transform ;

			UIAlphaMaskWindow tWindow = null ;

			int i, l = 64 ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTransform.GetComponent<Canvas>() != null )
				{
					break ;
				}
				
				tWindow = tTransform.GetComponent<UIAlphaMaskWindow>() ;
				if( tWindow != null )
				{
					break ;
				}

				if( tTransform.parent == null )
				{
					break ;
				}

				tTransform = tTransform.parent ;
			}

			if( tWindow == null )
			{
				return null ;
			}

			return tWindow.GetAlphaMaskMaterialForTextMesh( tBasisMaterial ) ;
		}
//#endif

	}
}
