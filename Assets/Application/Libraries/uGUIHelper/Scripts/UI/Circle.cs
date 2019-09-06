using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:サークル
	/// </summary>
	public class Circle : MaskableGraphicWrapper
	{
		/// <summary>
		/// スプライト
		/// </summary>
		[SerializeField][HideInInspector]
		private Sprite m_Sprite = null ;

		private Texture2D m_BlankTexture = null ;

		public  Sprite  sprite
		{
			get
			{
				return m_Sprite ;
			}
			set
			{
				if( m_Sprite != value )
				{
					m_Sprite  = value ;

					if( m_Sprite != null )
					{
						_CanvasRenderer.SetTexture( m_Sprite.texture ) ;
					}
					else
					{
						if( m_BlankTexture == null )
						{
							m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
						}
						_CanvasRenderer.SetTexture( m_BlankTexture ) ;
					}

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 内側のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_InnerColor = Color.white ;

		public    Color  innerColor
		{
			get
			{
				return m_InnerColor ;
			}
			set
			{
				if( m_InnerColor.Equals( value ) == false )
				{
					m_InnerColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 外側のカラー
		/// </summary>
		[HideInInspector][SerializeField]
		protected Color m_OuterColor = Color.white ;

		public    Color  outerColor
		{
			get
			{
				return m_OuterColor ;
			}
			set
			{
				if( m_OuterColor.Equals( value ) == false )
				{
					m_OuterColor = value ;

					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// 分割数
		/// </summary>
		[HideInInspector][SerializeField]
		protected int m_Split = 36 ;

		public    int  split
		{
			get
			{
				return m_Split ;
			}
			set
			{
				if( m_Split != value && value >= 3 )
				{
					m_Split = value ;

					SetVerticesDirty() ;
				}
			}
		}
	


		/// <summary>
		/// 内側の塗りつぶしの有無
		/// </summary>
		[HideInInspector][SerializeField]
		protected bool m_FillInner  = true ;

		public    bool  fillInner
		{
			get
			{
				return m_FillInner ;
			}
			set
			{
				if( m_FillInner != value )
				{
					m_FillInner = value ;

					SetVerticesDirty() ;
				}
			}
		}


		/// <summary>
		/// 外周の太さ(塗りつぶし無し限定)
		/// </summary>
		[HideInInspector][SerializeField]
		protected float m_LineWidth =  1 ;

		public    float  lineWidth
		{
			get
			{
				return m_LineWidth ;
			}
			set
			{
				if( m_LineWidth != value && value >  0 )
				{
					m_LineWidth = value ;

					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// テクスチャの張り方(塗りつぶし有り限定)
		/// </summary>
		public enum DecalType
		{
			Normal = 0,
			Effect = 1,
		}

		[HideInInspector][SerializeField]
		protected DecalType m_DecalType = DecalType.Normal ;

		public    DecalType  decalType
		{
			get
			{
				return m_DecalType ;
			}
			set
			{
				if( m_DecalType != value )
				{
					m_DecalType  = value ;

					SetVerticesDirty() ;
				}
			}
		}


		//----------------------------------------------------------

//		protected override void OnValidate()
//		{
//			base.OnValidate() ;
//		}

		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			tHelper.Clear() ;

			//-----------------------------------------------------------

			// 実際に頂点バッファを生成する

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;


			Vector2 tSize = size ;
			Vector2 tPivot = _RectTransform.pivot ;

			UIVertex v ;

			int i, j ;
			float a ;

			Vector2 p = Vector2.zero ;

			float vrw = tSize.x * 0.5f ;
			float vrh = tSize.y * 0.5f ;
			float vcx = tSize.x * ( 0.5f - tPivot.x ) ;
			float vcy = tSize.y * ( 0.5f - tPivot.y ) ;
			
			float tTw = 0 ;
			float tTh = 0 ;

			float trw = 0 ;
			float trh = 0 ;
			float tcx = 0 ;
			float tcy = 0 ;

			if( m_Sprite != null )
			{
				tTw = m_Sprite.texture.width ;
				tTh = m_Sprite.texture.height ;

				trw = m_Sprite.rect.width  * 0.5f ;
				trh = m_Sprite.rect.height * 0.5f ;

				tcx = m_Sprite.rect.x + trw ;
				tcy = m_Sprite.rect.y + trh ;
			}

			Vector3 tNormalVector = new Vector3(  0,  0, -1 ) ;

			//-----------------------------------------

			if( m_FillInner == false )
			{
				// 中心の塗りつぶしは無し
			
				float r ;
			
				Vector2 po = Vector2.zero ;
				Vector2 pi = Vector2.zero ;
				
				Vector2[] tVO = new Vector2[ m_Split ] ;
				Vector2[] tVI = new Vector2[ m_Split ] ;
						
				Vector2[] tTO = null ;
				Vector2[] tTI = null ;

				if( m_Sprite != null )
				{
					tTO = new Vector2[ m_Split ] ;
					tTI = new Vector2[ m_Split ] ;
				}
				

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )m_Split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;

					po.x = p.x * vrw ;
					po.y = p.y * vrh ;
					
					pi = po - ( p.normalized * m_LineWidth ) ;

					tVO[ i ] = new Vector2( po.x + vcx, po.y + vcy ) ;	// ９０度回転させてＹ符号反転
					tVI[ i ] = new Vector2( pi.x + vcx, pi.y + vcy ) ;

					if( m_Sprite != null )
					{
						r = pi.magnitude / po.magnitude ;
						
						tTO[ i ] = new Vector2( p.x * 1 * trw + tcx, p.y * 1 * trh + tcy ) ;
						tTI[ i ] = new Vector2( p.x * r * trw + tcx, p.y * r * trh + tcy ) ;
					}
				}
				
				//-------------------------------------------------
				
				// 実際に頂点データを生成する

				int vi ;
			
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					vi = aV.Count ;
					
					v = new UIVertex() ;

					j = ( i + 1 ) % m_Split ;

					// １点目
					v.position	= new Vector3( tVO[ i ].x, tVO[ i ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tTO[ i ].x / tTw, tTO[ i ].y / tTh ) ;
					}

					aV.Add( v ) ;

					// ２点目
					v.position	= new Vector3( tVO[ j ].x, tVO[ j ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_OuterColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tTO[ j ].x / tTw, tTO[ j ].y / tTh ) ;
					}

					aV.Add( v ) ;

					// ３点目
					v.position	= new Vector3( tVI[ i ].x, tVI[ i ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tTI[ i ].x / tTw, tTI[ i ].y / tTh ) ;
					}

					aV.Add( v ) ;

					// ４点目
					v.position	= new Vector3( tVI[ j ].x, tVI[ j ].y, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0	= new Vector2( tTI[ j ].x / tTw, tTI[ j ].y / tTh ) ;
					}

					aV.Add( v ) ;

					//インデックス
				
					aI.Add( vi + 0 ) ;
					aI.Add( vi + 1 ) ;
					aI.Add( vi + 2 ) ;

					aI.Add( vi + 1 ) ;
					aI.Add( vi + 3 ) ;
					aI.Add( vi + 2 ) ;
				}
			}
			else
			{
				// 中心の塗りつぶしは有り
			
				Vector2[] tV = new Vector2[ m_Split ] ;
			
				Vector2[] tT = null ;

				if( m_Sprite != null && m_DecalType == DecalType.Normal )
				{
					tT = new Vector2[ m_Split ] ;
				}

				// 頂点・ＵＶ情報を整理する
				for( i  = 0 ; i <  m_Split ; i ++ )
				{
					a = 2.0f * Mathf.PI * ( float )i / ( float )m_Split ;
				
					// 上を頂点開始地点にする(時計回り)
					p.x = Mathf.Sin( a ) ;
					p.y = Mathf.Cos( a ) ;


					tV[ i ] = new Vector2( p.x * vrw + vcx, p.y * vrh + vcy ) ;	// ９０度回転させてＹ符号反転

					if( m_Sprite != null && m_DecalType == DecalType.Normal )
					{
						tT[ i ] = new Vector2( p.x * trw + tcx, p.y * trh + tcy ) ;
					}
				}
			
				//-------------------------------------------------
				
				// 実際に頂点データを生成する(２タイプから選択可能)

				int vi ;
			
				if( m_DecalType == DecalType.Normal )
				{
					// テクスチャの張り方：通常

					// 中心
					v = new UIVertex() ;

					v.position	= new Vector3( vcx, vcy, 0 ) ;
					v.normal	= tNormalVector ;
					v.color		= m_InnerColor ;

					if( m_Sprite != null )
					{
						v.uv0		= new Vector2( tcx / tTw, tcy / tTh ) ;
					}

					aV.Add( v ) ;

					for( i  = 0 ; i <  m_Split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % m_Split ;

						// １点目
						v = new UIVertex() ;

						v.position	= new Vector3( tV[ i ].x, tV[ i ].y, 0 ) ;
						v.normal	= tNormalVector ;
						v.color		= m_OuterColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tT[ i ].x / tTw, tT[ i ].y / tTh ) ;
						}

						aV.Add( v ) ;

						// ２点目
						v.position	= new Vector3( tV[ j ].x, tV[ j ].y, 0 ) ;
						v.normal	= tNormalVector ;
						v.color		= m_OuterColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tT[ j ].x / tTw, tT[ j ].y / tTh ) ;
						}

						aV.Add( v ) ;

						// インデックス
						aI.Add( 0 ) ;
						aI.Add( vi + 0 ) ;
						aI.Add( vi + 1 ) ;
					}
				}
				else
				if( m_DecalType == DecalType.Effect )
				{
					// テクスチャの張り方：効果

					float tx0 = 0 ;
					float ty0 = 0 ;

					float tx1 = 0 ;
					float ty1 = 0 ;

					float tx2 = 0 ;
					float ty2 = 0 ;

					if( m_Sprite != null )
					{
						tx0 = tcx				/ tTw ;
						ty0 = m_Sprite.rect.yMax	/ tTh ;

						tx1 = m_Sprite.rect.xMin	/ tTw ;
						ty1 = m_Sprite.rect.yMin	/ tTh ;

						tx2 = m_Sprite.rect.xMax	/ tTw ;
						ty2 = m_Sprite.rect.yMin	/ tTh ;
					}

					for( i  = 0 ; i <  m_Split ; i ++ )
					{
						vi = aV.Count ;
				
						j = ( i + 1 ) % m_Split ;

						// １点目
						v = new UIVertex() ;

						v.position	= new Vector3( vcx, vcy, 0 ) ;
						v.normal	= tNormalVector ;
						v.color		= m_InnerColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tx0, ty0 ) ;
						}

						aV.Add( v ) ;

						// ２点目
						v = new UIVertex() ;

						v.position	= new Vector3( tV[ i ].x, tV[ i ].y, 0 ) ;
						v.normal	= tNormalVector ;
						v.color		= m_OuterColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tx1, ty1 ) ;
						}

						aV.Add( v ) ;

						// ３点目
						v.position	= new Vector3( tV[ j ].x, tV[ j ].y, 0 ) ;
						v.normal	= tNormalVector ;
						v.color		= m_OuterColor ;

						if( m_Sprite != null )
						{
							v.uv0	= new Vector2( tx2, ty2 ) ;
						}

						aV.Add( v ) ;

						// インデックス
						aI.Add( vi + 0 ) ;
						aI.Add( vi + 1 ) ;
						aI.Add( vi + 2 ) ;
					}

				}
			}


			if( aV.Count >  0 && aI.Count >  0 )
			{
				tHelper.AddUIVertexStream( aV, aI ) ;
			}
		}
		
		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			if( m_Sprite != null )
			{
				_CanvasRenderer.SetTexture( m_Sprite.texture ) ;
			}
			else
			{
				if( m_BlankTexture == null )
				{
					m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
				}
				_CanvasRenderer.SetTexture( m_BlankTexture ) ;
			}
		}
	}
}
