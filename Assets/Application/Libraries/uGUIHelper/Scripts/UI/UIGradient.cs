using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 文字列にグラデーションを付与するコンポーネント
	/// </summary>
	public class UIGradient : BaseMeshEffect
	{
//		[SerializeField][HideInInspector]
//		private   RectTransform	m_RectTransform ;
//		protected RectTransform	 _RectTransform
//		{
//			get
//			{
//				if( m_RectTransform != null )
//				{
//					return m_RectTransform ;
//				}
//				m_RectTransform = GetComponent<RectTransform>() ;
//				return m_RectTransform ;
//			}
//		}
		
		public enum Geometory
		{
			Image,
			Text,
		}

		public enum Direction
		{
			Vertical,
			Horizontal,
			Both,
		}
	
		public Geometory geometory	= Geometory.Image ;
		public Direction direction	= Direction.Vertical ;

		public Color top	= Color.white ;
		public Color middle	= Color.gray ;
		public Color bottom	= Color.black ;
		public float pivotMiddle = 0.5f ;

		public Color left	= Color.red ;
		public Color center	= Color.green ;
		public Color right	= Color.blue ;
		public float pivotCenter = 0.5f ;
		
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
			if( IsActive() == false || tList == null || tList.Count == 0 )
			{
				return ;
			}
			
			UIVertex v ;

			if( geometory == Geometory.Image )
			{
				// イメージのケース

				// 頂点の最少値と最大値を抽出する
				float tMaxX = - Mathf.Infinity, tMaxY = - Mathf.Infinity, tMinX = Mathf.Infinity, tMinY = Mathf.Infinity ;
				
				for( int i  = 0 ; i <  tList.Count ; i ++ )
				{
					v = tList[ i ] ;
					tMinX = Mathf.Min( tMinX, v.position.x ) ;
					tMinY = Mathf.Min( tMinY, v.position.y ) ;
					tMaxX = Mathf.Max( tMaxX, v.position.x ) ;
					tMaxY = Mathf.Max( tMaxY, v.position.y ) ;
				}
		
				float w = tMaxX - tMinX ;
				float h = tMaxY - tMinY ;
		
				// 頂点ごとの色を調整する
				Color tColorO ;

				Color tColorH ;
				Color tColorV ;

				Color tColorM = Color.white ;

				float xa, ya ;

				for( int i  = 0 ; i <  tList.Count ; i ++ )
				{
					v = tList[ i ] ;

					tColorO = v.color ;	// 指定のテキストカラー

					xa = ( v.position.x - tMinX ) / w ;	// 横位置
					if( xa <  pivotCenter )
					{
						tColorH = Color.Lerp( left,		center,	xa / pivotCenter ) ;
					}
					else
					if( xa >  pivotCenter )
					{
						tColorH = Color.Lerp( center,	right,	( xa - pivotCenter ) / ( 1.0f - pivotCenter ) ) ;
					}
					else
					{
						tColorH = center ;
					}

					ya = ( v.position.y - tMinY ) / h ;	// 縦位置
					if( ya <  pivotMiddle )
					{
						tColorV = Color.Lerp( bottom,	middle,	ya / pivotMiddle ) ;
					}
					else
					if( ya >  pivotMiddle )
					{
						tColorV = Color.Lerp( middle,	top,	( ya - pivotMiddle ) / ( 1.0f - pivotMiddle ) ) ;
					}
					else
					{
						tColorV = middle ;
					}

					switch( direction )
					{
						case Direction.Horizontal :
							tColorM = tColorH ;
						break ;

						case Direction.Vertical :
							tColorM = tColorV ;
						break ;

						case Direction.Both :
							tColorM = tColorV * tColorH ;
						break ;
					}

					v.color = tColorO * tColorM ;
	
					tList[ i ] = v ;
				}
			}
			else
			if( geometory == Geometory.Text )
			{
				// テキストのケース

				int N = tList.Count / 6 ;	// １文字あたり６頂点
				int o ;

				for( int n  = 0 ; n <  N ; n ++ )
				{
					o = n * 6 ;
					
					if( direction == Direction.Horizontal )
					{
						v = tList[ o + 0 ] ;
						v.color *= left ;
						tList[ o + 0 ] = v ;

						v = tList[ o + 1 ] ;
						v.color *= right ;
						tList[ o + 1 ] = v ;

						v = tList[ o + 2 ] ;
						v.color *= right ;
						tList[ o + 2 ] = v ;

						v = tList[ o + 3 ] ;
						v.color *= right ;
						tList[ o + 3 ] = v ;

						v = tList[ o + 4 ] ;
						v.color *= left ;
						tList[ o + 4 ] = v ;

						v = tList[ o + 5 ] ;
						v.color *= left ;
						tList[ o + 5 ] = v ;

					}
					else
					if( direction == Direction.Vertical )
					{
						v = tList[ o + 0 ] ;
						v.color *= top ;
						tList[ o + 0 ] = v ;

						v = tList[ o + 1 ] ;
						v.color *= top ;
						tList[ o + 1 ] = v ;

						v = tList[ o + 2 ] ;
						v.color *= bottom ;
						tList[ o + 2 ] = v ;

						v = tList[ o + 3 ] ;
						v.color *= bottom ;
						tList[ o + 3 ] = v ;

						v = tList[ o + 4 ] ;
						v.color *= bottom ;
						tList[ o + 4 ] = v ;

						v = tList[ o + 5 ] ;
						v.color *= top ;
						tList[ o + 5 ] = v ;
					}
					else
					if( direction == Direction.Both )
					{
						v = tList[ o + 0 ] ;
						v.color *= ( left *top ) ;
						tList[ o + 0 ] = v ;

						v = tList[ o + 1 ] ;
						v.color *= ( right * top ) ;
						tList[ o + 1 ] = v ;

						v = tList[ o + 2 ] ;
						v.color *= ( right * bottom ) ;
						tList[ o + 2 ] = v ;

						v = tList[ o + 3 ] ;
						v.color *= ( right * bottom ) ;
						tList[ o + 3 ] = v ;

						v = tList[ o + 4 ] ;
						v.color *= ( left * bottom ) ;
						tList[ o + 4 ] = v ;

						v = tList[ o + 5 ] ;
						v.color *= ( left * top ) ;
						tList[ o + 5 ] = v ;
					}
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
