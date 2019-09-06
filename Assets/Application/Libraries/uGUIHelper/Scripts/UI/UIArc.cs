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
	[ RequireComponent( typeof( Arc ) ) ]
	public class UIArc : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Color.white ;
				}
				return tArc.color ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return null ;
				}
				return tArc.material ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.material = value ;
			}
		}


		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  sprite
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return null ;
				}
				return tArc.sprite ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.sprite = value ;
			}
		}


		/// <summary>
		/// 内側のカラー(ショートカット)
		/// </summary>
		public    Color  innerColor
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Color.white ;
				}
				return tArc.innerColor ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.innerColor = value ;
			}
		}
		
		/// <summary>
		/// 外側のカラー(ショートカット)
		/// </summary>
		public    Color  outerColor
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Color.white ;
				}
				return tArc.outerColor ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.outerColor = value ;
			}
		}
	
		/// <summary>
		/// 円弧開始角度(ショートカット)
		/// </summary>
		public    float  startAngle
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return 0 ;
				}
				return tArc.startAngle ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.startAngle = value ;
			}
		}
	
		/// <summary>
		/// 円弧終了角度(ショートカット)
		/// </summary>
		public    float  endAngle
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return 0 ;
				}
				return tArc.endAngle ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.endAngle = value ;
			}
		}
	
		/// <summary>
		/// 円弧方向(ショートカット)
		/// </summary>
		public    Arc.Direction  direction
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Arc.Direction.Right ;
				}
				return tArc.direction ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.direction = value ;
			}
		}

		/// <summary>
		/// 形状(ショートカット)
		/// </summary>
		public    Arc.ShapeType  shapeType
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Arc.ShapeType.Circle ;
				}
				return tArc.shapeType ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.shapeType = value ;
			}
		}


		/// <summary>
		/// 分割数(ショートカット)
		/// </summary>
		public    int  split
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return 0 ;
				}
				return tArc.split ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.split = value ;
			}
		}

		/// <summary>
		/// テクスチャの張り方(形状が円限定)
		/// </summary>
		public    Arc.DecalType  decalType
		{
			get
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return Arc.DecalType.Normal ;
				}
				return tArc.decalType ;
			}
			set
			{
				Arc tArc = _arc ;
				if( tArc == null )
				{
					return ;
				}
				tArc.decalType = value ;
			}
		}
	
		//--------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			Arc tArc = _arc ;

			if( tArc == null )
			{
				tArc = gameObject.AddComponent<Arc>() ;
			}
			if( tArc == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			tArc.color = Color.white ;

			ResetRectTransform() ;
		}
	}
}

