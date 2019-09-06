using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[RequireComponent( typeof( UnityEngine.UI.Text ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UIText : UIView
	{
		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
		public bool autoSizeFitting = true ;

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 textSize
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return Vector2.zero ;
				}
				return new Vector2( tText.preferredWidth, tText.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int fontSize
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return 0 ;
				}
				return tText.fontSize ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.fontSize = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string text
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return null ;
				}
				return tText.text ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.text = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public Font font
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return null ;
				}
				return tText.font ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.font = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return Color.white ;
				}
				return tText.color ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return null ;
				}
				return tText.material ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.material = value ;
			}
		}

		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyle fontStyle
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return FontStyle.Normal ;
				}
				return tText.fontStyle ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.fontStyle = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// リッチテキスト(ショートカット)
		/// </summary>
		public bool supportRichText
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return false ;
				}
				return tText.supportRichText ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.supportRichText = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAnchor alignment
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return TextAnchor.MiddleCenter ;
				}
				return tText.alignment ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.alignment = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// リサイズ(ショートカット)
		/// </summary>
		public bool resizeTextForBestFit
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return false ;
				}
				return tText.resizeTextForBestFit ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.resizeTextForBestFit = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  HorizontalWrapMode horizontalOverflow
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return HorizontalWrapMode.Overflow ;
				}
				return tText.horizontalOverflow ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.horizontalOverflow = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  VerticalWrapMode verticalOverflow
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return VerticalWrapMode.Overflow ;
				}
				return tText.verticalOverflow ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.verticalOverflow = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// 改行時の下方向への移動量係数(ショートカット)
		/// </summary>
		public  float lineSpacing
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return 1.0f ;
				}
				return tText.lineSpacing ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.lineSpacing = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public bool raycastTarget
		{
			get
			{
				Text tText = _text ;
				if( tText == null )
				{
					return false ;
				}
				return tText.raycastTarget ;
			}
			set
			{
				Text tText = _text ;
				if( tText == null )
				{
					return ;
				}
				tText.raycastTarget = value ;
			}
		}

		//--------------------------------------------------

		public Shadow	shadow
		{
			get
			{
				return _shadow ;
			}
		}

		public Outline	outline
		{
			get
			{
				return _outline ;
			}
		}

		public UIGradient gradient
		{
			get
			{
				return _gradient ;
			}
		}

		//--------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Text tText = _text ;
			if( tText == null )
			{
				tText = gameObject.AddComponent<Text>() ;
			}
			if( tText == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	tDefaultTextColor = Color.white ;

			Font	tDefaultFont = null ;
			int		tDefaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultTextColor	= tDS.textColor ;

					tDefaultFont		= tDS.font ;
					tDefaultFontSize	= tDS.fontSize ;
				}
			}
			
#endif

			tText.color = tDefaultTextColor ;

			if( tDefaultFont == null )
			{
				tText.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				tText.font = tDefaultFont ;
			}

			if( tDefaultFontSize <= 0 )
			{
				tText.fontSize = 32 ;
			}
			else
			{
				tText.fontSize = tDefaultFontSize ;
			}

			tText.alignment = TextAnchor.MiddleLeft ;

			tText.horizontalOverflow = HorizontalWrapMode.Overflow ;
			tText.verticalOverflow   = VerticalWrapMode.Overflow ;
			
			ResetRectTransform() ;


			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tText.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			tText.raycastTarget = false ;
		}

		override protected void OnLateUpdate()
		{
			if( autoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			Text t = _text ;
			RectTransform r = _rectTransform ;
			if( r != null && t != null )
			{
				Vector2 tSize = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.horizontalOverflow == HorizontalWrapMode.Overflow )
					{
						tSize.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.verticalOverflow == VerticalWrapMode.Overflow )
					{
						tSize.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = tSize ;
			}
		}
	}
}

