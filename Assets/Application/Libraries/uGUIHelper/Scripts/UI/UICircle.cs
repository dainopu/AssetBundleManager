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
	[ RequireComponent( typeof( Circle ) ) ]
	public class UICircle : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return Color.white ;
				}
				return tCircle.color ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return null ;
				}
				return tCircle.material ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.material = value ;
			}
		}


		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  sprite
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return null ;
				}
				return tCircle.sprite ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.sprite = value ;
			}
		}


		/// <summary>
		/// 内側のカラー
		/// </summary>
		public    Color  innerColor
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return Color.white ;
				}
				return tCircle.innerColor ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.innerColor = value ;
			}
		}
		
		/// <summary>
		/// 外側のカラー
		/// </summary>
		public    Color  outerColor
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return Color.white ;
				}
				return tCircle.outerColor ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.outerColor = value ;
			}
		}
	
		/// <summary>
		/// 分割数
		/// </summary>
		public    int  split
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return 0 ;
				}
				return tCircle.split ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.split = value ;
			}
		}
	


		/// <summary>
		/// 内側の塗りつぶしの有無
		/// </summary>
		public    bool  fillInner
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return false ;
				}
				return tCircle.fillInner ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.fillInner = value ;
			}
		}


		/// <summary>
		/// 外周の太さ(塗りつぶし無し限定)
		/// </summary>
		public    float  lineWidth
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return 0 ;
				}
				return tCircle.lineWidth ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.lineWidth = value ;
			}
		}

		/// <summary>
		/// テクスチャの張り方(塗りつぶし有り限定)
		/// </summary>
		public    Circle.DecalType  decalType
		{
			get
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return Circle.DecalType.Normal ;
				}
				return tCircle.decalType ;
			}
			set
			{
				Circle tCircle = _circle ;
				if( tCircle == null )
				{
					return ;
				}
				tCircle.decalType = value ;
			}
		}
	
		//--------------------------------------------------------------------------------

		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			Circle tCircle = _circle ;

			if( tCircle == null )
			{
				tCircle = gameObject.AddComponent<Circle>() ;
			}
			if( tCircle == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			tCircle.color = Color.white ;
			
			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tCircle.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------------------------------

			ResetRectTransform() ;
		}
	}
}

