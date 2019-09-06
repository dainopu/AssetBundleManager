using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	/// <summary>
	/// プログレスバークラス(複合UI)
	/// </summary>
	public class UIProgressbar : UIImage
	{
		/// <summary>
		/// 領域部のインスタンス
		/// </summary>
		public UIImage		scope ;

		/// <summary>
		/// 画像部のインスタンス
		/// </summary>
		public UIImage		thumb ;

		/// <summary>
		/// 数値部のインスタンス
		/// </summary>
		public UINumber		label ;

		/// <summary>
		/// バーの表示タイプ
		/// </summary>
		public enum DisplayType
		{
			Stretch = 0,
			Mask = 1,
		}

		[SerializeField][HideInInspector]
		private DisplayType m_DisplayType = DisplayType.Stretch ; 
		public  DisplayType   displayType
		{
			get
			{
				return m_DisplayType ;
			}
			set
			{
				if( m_DisplayType != value )
				{
					m_DisplayType  = value ;
					UpdateThumb() ;
				}
			}
		}


		/// <summary>
		/// 値(係数)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Value = 1 ;
		public  float   value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				if( m_Value != value )
				{
					m_Value = value ;
					UpdateThumb() ;
					UpdateLabel() ;
				}
			}
		}

		/// <summary>
		/// 値(即値)
		/// </summary>
		[SerializeField][HideInInspector]
		private float m_Number = 100.0f ;
		public  float   number
		{
			get
			{
				return m_Number ;
			}
			set
			{
				if( m_Number != value )
				{
					m_Number = value ;
					UpdateThumb() ;
					UpdateLabel() ;
				}
			}
		}



		//-----------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				float s ;
				if( tSize.x <= tSize.y )
				{
					s = tSize.x ;
				}
				else
				{
					s = tSize.y ;
				}
				SetSize( s * 0.5f, s * 0.05f ) ;
			}


			Sprite tDefaultFrameSprite = null ;
			Sprite tDefaultThumbSprite = null ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultFrameSprite		= tDS.progressbarFrame ;
					tDefaultThumbSprite		= tDS.progressbarThumb ;
				}
			}
			
#endif

			UIAtlasSprite tAtlas = UIAtlasSprite.Create( "uGUIHelper/Textures/UIProgressbar" ) ;

			// Frame
			Image tFrame = _image ;

			if( tDefaultFrameSprite == null )
			{
				tFrame.sprite = tAtlas[ "UIProgressbar_Frame" ] ;
			}
			else
			{
				tFrame.sprite = tDefaultFrameSprite ;
			}
			tFrame.type = Image.Type.Sliced ;
			tFrame.fillCenter = true ;

			if( isCanvasOverlay == true )
			{
				tFrame.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			UIView tFillArea = AddView<UIView>( "Fill Area" ) ;
			tFillArea.SetAnchorToStretch() ;

			// Mask
			scope = tFillArea.AddView<UIImage>( "Scope" ) ;
			scope.SetAnchorToStretch() ;
			scope.SetMargin( 0, 0, 0, 0 ) ;

			scope.isMask = true ;
			scope.showMaskGraphic = false ;

			if( isCanvasOverlay == true )
			{
				scope.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			// Thumb
			thumb = scope.AddView<UIImage>( "Thumb" ) ;
			thumb.SetAnchorToStretch() ;
			thumb.SetMargin( 0, 0, 0, 0 ) ;

			if( tDefaultThumbSprite == null )
			{
				thumb.sprite = tAtlas[ "UIProgressbar_Thumb" ] ;
			}
			else
			{
				thumb.sprite = tDefaultThumbSprite ;
			}
			thumb.type = Image.Type.Sliced ;
			thumb.fillCenter = true ;

			if( isCanvasOverlay == true )
			{
				thumb.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			UpdateThumb() ;

			// Label
			label = AddView<UINumber>( "Label" ) ;
			label.fontSize = ( int )( _h * 0.6f ) ;
			label.isOutline = true ;
			label.percent = true ;

			if( isCanvasOverlay == true )
			{
				label.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			UpdateLabel() ;

//			DestroyImmediate( tAtlas ) ;
		}

		//----------------------------------------------------

		/// <summary>
		/// Thumb 更新
		/// </summary>
		private void UpdateThumb()
		{
			if( scope == null || thumb == null )
			{
				return ;
			}

			if( m_DisplayType == DisplayType.Stretch)
			{
				if( m_Value <= 0 )
				{
					scope.SetActive( false ) ;
				}
				else
				{
					scope.SetActive( true ) ;
	
					scope.SetAnchorToStretch() ;
					scope.SetAnchorMin(       0, 0 ) ;
					scope.SetAnchorMax( m_Value, 1 ) ;

					thumb.SetAnchorToStretch() ;
					thumb.SetMargin(   0,   0,   0,   0 ) ;
				}
			}
			else
			if( m_DisplayType == DisplayType.Mask )
			{
				if( m_Value <= 0 )
				{
					scope.SetActive( false ) ;
				}
				else
				{
					scope.SetActive( true ) ;
					scope.SetAnchorToStretch() ;

					float d = scope._w * ( 1.0f - m_Value ) ;

					scope.SetMargin( 0, d, 0, 0 ) ;
	
					thumb.SetAnchorToStretch() ;
					thumb.SetMargin(   0, - d,   0,   0 ) ;
				}
			}
		}

		/// <summary>
		/// Label 更新
		/// </summary>
		private void UpdateLabel()
		{
			if( label == null )
			{
				return ;
			}

			label.value = m_Value * m_Number ;
		}
	}
}
