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
	/// uGUI:Image クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( ImageNumber ) ) ]
	public class UIImageNumber : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return Color.white ;
				}
				return tImageNumber.color ;
			}
			set
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return ;
				}
				tImageNumber.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return null ;
				}
				return tImageNumber.material ;
			}
			set
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return ;
				}
				tImageNumber.material = value ;
			}
		}

		/// <summary>
		/// 値(ショートカット)
		/// </summary>
		public int value
		{
			get
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return 0 ;
				}
				return tImageNumber.value ;
			}
			set
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return ;
				}
				tImageNumber.value = value ;

				if( autoSizeFitting == true )
				{
					SetSize( tImageNumber.preferredWidth, tImageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字のアンカー(ショートカット)
		/// </summary>
		public TextAnchor alignment
		{
			get
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return 0 ;
				}
				return tImageNumber.alignment ;
			}
			set
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return ;
				}
				tImageNumber.alignment = value ;

				if( autoSizeFitting == true )
				{
					SetSize( tImageNumber.preferredWidth, tImageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置
		/// </summary>
		public Vector3[] codeOffset
		{
			get
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return null ;
				}
				return tImageNumber.codeOffset ;
			}
			set
			{
				ImageNumber tImageNumber = _imageNumber ;
				if( tImageNumber == null )
				{
					return ;
				}
				tImageNumber.codeOffset = value ;

				if( autoSizeFitting == true )
				{
					SetSize( tImageNumber.preferredWidth, tImageNumber.preferredHeight ) ;
				}
			}
		}

		/// <summary>
		/// 文字単位のオフセット位置の設定
		/// </summary>
		/// <param name="tCodeOffset"></param>
		public void SetCodeOffset( Vector3[] tCodeOffset )
		{
			ImageNumber tImageNumber = _imageNumber ;
			if( tImageNumber == null )
			{
				return ;
			}
			tImageNumber.SetCodeOffset( tCodeOffset ) ;
		}

		/// <summary>
		/// ＵＩのサイズを文字のサイズに自動調整するかどうか
		/// </summary>
		public bool autoSizeFitting = true ;


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			ImageNumber tImageNumber = _imageNumber ;

			if( tImageNumber == null )
			{
				tImageNumber = gameObject.AddComponent<ImageNumber>() ;
			}
			if( tImageNumber == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			tImageNumber.color = Color.white ;
			tImageNumber.atlasSprite = UIAtlasSprite.Create( "uGUIHelper/Textures/UIDefaultImageNumber" ) ;

			if( isCanvasOverlay == true )
			{
				tImageNumber.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			ResetRectTransform() ;
		}

		override protected void OnLateUpdate()
		{
			if( autoSizeFitting == true )
			{
				ImageNumber t = _imageNumber ;
				RectTransform r = _rectTransform ;
				if( r != null && t != null )
				{
					Vector2 tSize = r.sizeDelta ;

					if( r.anchorMin.x == r.anchorMax.x )
					{
						tSize.x = t.preferredWidth ;
					}
					if( r.anchorMin.y == r.anchorMax.y )
					{
						tSize.y = t.preferredHeight ;
					}

					r.sizeDelta = tSize ;
				}
			}
		}	
	}
}

