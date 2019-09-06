using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 文字列にグラデーションを付与するコンポーネント
	/// </summary>
	public class UITextColor : BaseMeshEffect
	{
		public Color[] Color = null ;

		public void SetColor( Color[] color )
		{
			Color = color ;
		}

		public void SetColor( Color32[] color32 )
		{
			if( color32 == null || color32.Length == 0 )
			{
				return ;
			}

			int i, l = color32.Length ;

			Color = new Color[ l  ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				Color[ i ] = color32[ i ] ;
			}
		}

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
			if( IsActive() == false || tList == null || tList.Count == 0 || Color == null || Color.Length == 0 )
			{
				return ;
			}
			
			UIVertex v ;

			// テキストのケース

			int N = tList.Count / 6 ;	// １文字あたり６頂点
			int o, i ;

			if( N >  Color.Length )
			{
				N  = Color.Length ;
			}

			Color c ;


			for( int n  = 0 ; n <  N ; n ++ )
			{
				c = Color[ n ] ;

				o = n * 6 ;
					
				for( i  = 0 ; i <  6 ; i ++ )
				{
					v = tList[ o + i ] ;
					v.color = c ;
					tList[ o + i ] = v ;
				}
			}
		}
	
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}
	}
}

