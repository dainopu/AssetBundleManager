using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;


namespace uGUIHelper
{
	/// <summary>
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( UnityEngine.UI.Image ) ) ]
	public class UIImage : UIView
	{
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

					Sprite tSprite = null ;
					if( m_AtlasSprite != null )
					{
						if( m_AtlasSprite.length >  0 )
						{
							tSprite = m_AtlasSprite[ m_AtlasSprite.GetNameList()[ 0 ] ] ;
						}
					}
					sprite = tSprite ;
				}

				// インスタンスは保持しない(本来は private メソッドでやるべきではない)
				m_AtlasSprite = null ;
			}
		}

		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string tName, bool tResize = false )
		{
			if( m_AtlasSprite == null )
			{
				return false ;	// 基本的にありえない
			}

			if( m_AtlasSprite.texture == null && string.IsNullOrEmpty( m_AtlasSprite.path ) == false )
			{
				m_AtlasSprite.Load() ;
			}

			if( m_AtlasSprite[ tName ] == null )
			{
				return false ;
			}

			sprite = m_AtlasSprite[ tName ] ;

			if( tResize == true )
			{
				SetNativeSize() ;
			}

			return true ;
		}

		/// <summary>
		/// アトラススプライト内のスプライトを表示する
		/// </summary>
		/// <param name="tName">スプライト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool SetSpriteInAtlas( string tPath, string tName, bool tResize = false )
		{
			if( m_AtlasSprite == null )
			{
				return false ;	// 基本的にありえない
			}

			if( m_AtlasSprite.texture == null && string.IsNullOrEmpty( tPath ) == false )
			{
				m_AtlasSprite.Load( tPath ) ;
			}

			if( m_AtlasSprite[ tName ] == null )
			{
				return false ;
			}

			sprite = m_AtlasSprite[ tName ] ;

			if( tResize == true )
			{
				SetNativeSize() ;
			}

			return true ;
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

		/// <summary>
		/// １６進数値で色を設定する
		/// </summary>
		/// <param name="tColor"></param>
		public void SetColor( uint tColor )
		{
			Image tImage = _image ;
			if( tImage == null )
			{
				return ;
			}

			byte tR = ( byte )( ( tColor >> 16 ) & 0xFF ) ;
			byte tG = ( byte )( ( tColor >>  8 ) & 0xFF ) ;
			byte tB = ( byte )( ( tColor       ) & 0xFF ) ;
			byte tA = ( byte )( ( tColor >> 24 ) & 0xFF ) ;

			tImage.color = new Color32( tR, tG, tB, tA ) ;
		}

		/// <summary>
		/// 画像の向きを設定する
		/// </summary>
		public UIInversion.Direction inversion
		{
			get
			{
				if( _inversion == null )
				{
					return UIInversion.Direction.None ;
				}
				return _inversion.direction ;
			}
			set
			{
				if( value == UIInversion.Direction.None )
				{
					isInversion = false ;
				}
				else
				{
					if( _inversion == null )
					{
						isInversion = true ;
					}
					_inversion.direction = value ;
				}
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public Sprite sprite
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return null ;
				}
				return tImage.sprite ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.sprite = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return Color.white ;
				}
				return tImage.color ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return null ;
				}
				return tImage.material ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.material = value ;
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public bool raycastTarget
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return false ;
				}
				return tImage.raycastTarget ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.raycastTarget = value ;
			}
		}
		
		/// <summary>
		/// タイプ(ショートカット)
		/// </summary>
		public Image.Type type
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return Image.Type.Simple ;
				}
				return tImage.type ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.type = value ;
			}
		}
	
		/// <summary>
		/// フィルセンター(ショートカット)
		/// </summary>
		public bool fillCenter
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return false ;
				}
				return tImage.fillCenter ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.fillCenter = value ;
			}
		}

		/// <summary>
		/// フィルメソッド(ショートカット)
		/// </summary>
		public Image.FillMethod fillMethod
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return  0 ;
				}
				return tImage.fillMethod ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.fillMethod = value ;
			}
		}

		public enum FillOrigin
		{
			Bottom	= 0,
			Right	= 1,
			Top		= 2,
			Left	= 3,
		}

		/// <summary>
		/// フィルオリジン(ショートカット)
		/// </summary>
		public FillOrigin fillOrigin
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return  0 ;
				}
				return ( FillOrigin )tImage.fillOrigin ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.fillOrigin = ( int )value ;
			}
		}

		/// <summary>
		/// フィルアマウント(ショートカット)
		/// </summary>
		public float fillAmount
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return  0 ;
				}
				return tImage.fillAmount ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.fillAmount = value ;
			}
		}

		/// <summary>
		/// クロックワイズ(ショートカット)
		/// </summary>
		public bool fillClockwise
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return false ;
				}
				return tImage.fillClockwise ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.fillClockwise = value ;
			}
		}

		/// <summary>
		/// プリサーブアスペクト(ショートカット)
		/// </summary>
		public bool preserveAspect
		{
			get
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return false ;
				}
				return tImage.preserveAspect ;
			}
			set
			{
				Image tImage = _image ;
				if( tImage == null )
				{
					return ;
				}
				tImage.preserveAspect = value ;
			}
		}

		/// <summary>
		/// マスクのグラフィック表示の有無(ショートカット)
		/// </summary>
		public bool showMaskGraphic
		{
			get
			{
				Mask tMask = _mask ;
				if( tMask == null )
				{
					return false ;
				}
				return tMask.showMaskGraphic ;
			}
			set
			{
				Mask tMask = _mask ;
				if( tMask == null )
				{
					return ;
				}
				tMask.showMaskGraphic = value ;
			}
		}

		/// <summary>
		/// RectRransform のサイズをスプライトのサイズに合わせる
		/// </summary>
		public void SetNativeSize()
		{
			if( sprite == null )
			{
				return ;
			}

			SetSize( sprite.rect.width, sprite.rect.height ) ;
		}



		//-----------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Image tImage = _image ;

			if( tImage == null )
			{
				tImage = gameObject.AddComponent<Image>() ;
			}
			if( tImage == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			if( tOption.ToLower() == "panel" )
			{
				// Panel
				tImage.color = new Color32( 255, 255, 255, 100 ) ;
				tImage.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			
				SetAnchorToStretch() ;
//				SetSize( 0, 0 ) ;
			}
			else
			{
				// Default
//				tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
				tImage.color = Color.white ;
				tImage.type = Image.Type.Sliced ;

				ResetRectTransform() ;
			}

			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			tImage.raycastTarget = false ;
		}

		/// <summary>
		/// リソースからスプライトをロードする
		/// </summary>
		/// <param name="tPath">リソースのパス</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public bool LoadSpriteFromResources( string tPath )
		{
			Sprite tSprite = Resources.Load<Sprite>( tPath ) ;
			if( tSprite == null )
			{
				return false ;
			}

			sprite = tSprite ;

			return true ;
		}

		/// <summary>
		/// スプライトを設定しスプライトのサイズでリサイズする
		/// </summary>
		/// <param name="tSprite">スプライトのインスタンス</param>
		public void SetSpriteAndResize( Sprite tSprite )
		{
			sprite = tSprite ;

			if( tSprite != null )
			{
				size = tSprite.rect.size ;
			}
		}

		/// <summary>
		/// スプライトを設定し任意のサイズにリサイズする
		/// </summary>
		/// <param name="tSprite">スプライトのインスタンス</param>
		/// <param name="tSize">リサイズ後のサイズ</param>
		public void SetSpriteAndResize( Sprite tSprite, Vector2 tSize )
		{
			sprite = tSprite ;
			size   = tSize ;
		}
	}
}
