using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 文字列にグラデーションを付与するコンポーネント
	/// </summary>
	public class UISelectableColor : BaseMeshEffect
	{
		public Selectable	target ;
		private bool		m_Interactable = false ;

		public override void ModifyMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			List<UIVertex> tList = new List<UIVertex>() ;
			tHelper.GetUIVertexStream( tList ) ;
		
			ModifyVertices( tList ) ;
		
			tHelper.Clear() ;
			tHelper.AddUIVertexTriangleStream( tList ) ;
		}
	
		private void ModifyVertices( List<UIVertex> tList )
		{
			if( IsActive() == false || tList == null || tList.Count == 0 || target == null )
			{
				return ;
			}
			
			UIVertex v ;

			Color tColor ;

			if( target.interactable == true )
			{
				tColor = target.colors.normalColor ;
			}
			else
			{
				tColor = target.colors.disabledColor ;
			}

			// 全頂点の色を補正する
			for( int i  = 0 ; i <  tList.Count ; i ++ )
			{
				v = tList[ i ] ;
				v.color = v.color * tColor ;	// 指定のテキストカラー
				tList[ i ] = v ;
			}
		}
	
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}

		override protected void Start()
		{
			base.Start() ;

			Refresh() ;
			m_Interactable = target.interactable ;
		}

		public void Update()
		{
			if( target != null )
			{
				if( m_Interactable != target.interactable )
				{
					Refresh() ;
					m_Interactable  = target.interactable ;
				}
			}
		}
	}
}
