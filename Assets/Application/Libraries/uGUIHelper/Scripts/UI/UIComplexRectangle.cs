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
	/// uGUI:ComplxRectangle クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( ComplexRectangle ) ) ]
	public class UIComplexRectangle : UIView
	{
		/// <summary>
		/// オフセット(ショートカット)
		/// </summary>
		public Vector2 offset
		{
			get
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return Vector2.zero ;
				}
				return tComplexRectangle.offset ;
			}
			set
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return ;
				}
				tComplexRectangle.offset = value ;
			}
		}
	



		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
//		public bool autoSizeFitting = true ;

		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture2D texture
		{
			get
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return null ;
				}
				return tComplexRectangle.texture ;
			}
			set
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return ;
				}
				tComplexRectangle.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return Color.white ;
				}
				return tComplexRectangle.color ;
			}
			set
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return ;
				}
				tComplexRectangle.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return null ;
				}
				return tComplexRectangle.material ;
			}
			set
			{
				ComplexRectangle tComplexRectangle = _complexRectangle ;
				if( tComplexRectangle == null )
				{
					return ;
				}
				tComplexRectangle.material = value ;
			}
		}
	
		//-----------------------------------------------------------

		[SerializeField][HideInInspector]
		private UIAtlasSprite m_AtlasSprite = null ;

		/// <summary>
		/// アトラススプライトのインスタンス
		/// </summary>
		public  UIAtlasSprite  atlasSprite
		{
			get
			{
				return m_AtlasSprite ;
			}
			set
			{
				if( m_AtlasSprite != value )
				{
					m_AtlasSprite  = value ;
				}

				m_AtlasSprite = null ;
			}
		}


		/// <summary>
		/// アトラススプライト内のスプライトを取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>スプライトのインスタンス</returns>
		public Sprite GetSpriteInAtlas( string tName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return null ;
			}

			return m_AtlasSprite[ tName ] ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの横幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>横幅</returns>
		public int GetWidthOfSpriteInAtlas( string tName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ tName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ tName ].rect.width ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトの縦幅を取得する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>縦幅</returns>
		public int GetHeightOfSpriteInAtlas( string tName )
		{
			if( m_AtlasSprite == null || m_AtlasSprite.length == 0 )
			{
				return 0 ;
			}

			if( m_AtlasSprite[ tName ] == null )
			{
				return 0 ;
			}

			return ( int )m_AtlasSprite[ tName ].rect.height ;
		}
		
		//-----------------------------------------------------------

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string tName, Vector2 tOffset, Vector2 tPivot, Vector2 tSize, float tRotation, Color tColor, Rect tUV, int tPriority )
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			if( tComplexRectangle == null )
			{
				return ;
			}

			tComplexRectangle.AddRectangle( tName, tOffset, tPivot, tSize, tRotation, tColor, tUV, tPriority ) ;
		}

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string tName, Vector2 tOffset, Vector2 tPivot, Vector2 tSize, float tRotation, Color tColor, string tSpriteName, int tPriority )
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			if( tComplexRectangle == null || m_AtlasSprite == null || string.IsNullOrEmpty( tSpriteName ) == true || m_AtlasSprite[ tSpriteName ] == null )
			{
				return ;
			}

			Rect tUV = m_AtlasSprite[ tSpriteName ].textureRect ;

			tComplexRectangle.AddRectangle( tName, tOffset, tPivot, tSize, tRotation, tColor, tUV, tPriority ) ;
		}

		/// <summary>
		/// 矩形を追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <param name="tOffset"></param>
		/// <param name="tPivot"></param>
		/// <param name="tSize"></param>
		/// <param name="tRotation"></param>
		/// <param name="tColor"></param>
		/// <param name="tUV"></param>
		/// <param name="tPriority"></param>
		public void AddRectangle( string tName, float tOffsetX, float tOffsetY, float tPivotX, float tPivotY, float tSizeX, float tSizeY, Color tColor, string tSpriteName, float tPadding, int tPriority )
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			if( tComplexRectangle == null || m_AtlasSprite == null || string.IsNullOrEmpty( tSpriteName ) == true || m_AtlasSprite[ tSpriteName ] == null )
			{
				return ;
			}

			Vector2 tOffset = new Vector2( tOffsetX, tOffsetY ) ;
			Vector2 tPivot = new Vector2( tPivotX, tPivotY ) ;

			Rect tUV = m_AtlasSprite[ tSpriteName ].textureRect ;

//			Vector2 tSize = new Vector2( tUV.width, tUV.height ) ;
			Vector2 tSize = new Vector2( tSizeX, tSizeY ) ;

			// 座標をテクスチャ系に変換する
			float tTW = m_AtlasSprite.texture.width ;
			float tTH = m_AtlasSprite.texture.height ;

			tUV.xMin	= ( tUV.xMin	+ tPadding )	/ tTW ;
			tUV.yMin	= ( tUV.yMin	+ tPadding )	/ tTH ;
			tUV.xMax	= ( tUV.xMax	- tPadding )	/ tTW ;
			tUV.yMax	= ( tUV.yMax	- tPadding )	/ tTH ;

//			Debug.LogWarning( "UV:" + tUV ) ;

			tComplexRectangle.AddRectangle( tName, tOffset, tPivot, tSize, 0, tColor, tUV, tPriority ) ;
		}



		/// <summary>
		/// 矩形を削除する
		/// </summary>
		/// <param name="tName"></param>
		public void RemoveRectangle( string tName )
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			if( tComplexRectangle == null )
			{
				return ;
			}

			tComplexRectangle.RemoveRectangle( tName ) ;
		}

		/// <summary>
		/// 矩形を全て削除する
		/// </summary>
		public void ClearRectangle()
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			if( tComplexRectangle == null )
			{
				return ;
			}

			tComplexRectangle.ClearRectangle() ;
		}

		//----------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;

			if( tComplexRectangle == null )
			{
				tComplexRectangle = gameObject.AddComponent<ComplexRectangle>() ;
			}
			if( tComplexRectangle == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			tComplexRectangle.rectangle.Add( new ComplexRectangle.Rectangle() ) ;

			// Default
			tComplexRectangle.color = Color.white ;

			ResetRectTransform() ;
		}

/*		override protected void OnLateUpdate()
		{
			if( autoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			ComplexRectangle tComplexRectangle = _complexRectangle ;
			RectTransform r = _rectTransform ;
			if( r != null && tComplexRectangle != null )
			{
				Vector2 tSize = r.sizeDelta ;

				tSize.x = tComplexRectangle.preferredWidth ;
				tSize.y = tComplexRectangle.preferredHeight ;

				r.sizeDelta = tSize ;
			}
		}*/

	}
}
