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
	/// uGUI:RawImage クラスの機能拡張コンポーネントクラス
	/// </summary>
	[ RequireComponent( typeof( GridMap ) ) ]
	public class UIGridMap : UIView
	{
		/// <summary>
		/// テクスチャ(ショートカット)
		/// </summary>
		public Texture2D texture
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return null ;
				}
				return tGridMap.texture ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.texture = value ;
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return Color.white ;
				}
				return tGridMap.color ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return null ;
				}
				return tGridMap.material ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.material = value ;
			}
		}
	
		/// <summary>
		/// ＵＩの横方向の分割数(ショートカット)
		/// </summary>
		public int vertexHorizontalGrid
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.vertexHorizontalGrid ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.vertexHorizontalGrid = value ;
			}
		}

		/// <summary>
		/// ＵＩの横方向の分割数(ショートカット)
		/// </summary>
		public int vertexVerticalGrid
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.vertexVerticalGrid ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.vertexVerticalGrid = value ;
			}
		}

		/// <summary>
		/// メッシュの分割密度(ショートカット)
		/// </summary>
		public GridMap.VertexDensity vertexDensity
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.vertexDensity ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.vertexDensity = value ;
			}
		}

		/// <summary>
		/// テクスチャの横方向の分割数(ショートカット)
		/// </summary>
		public int textureHorizontalGrid
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.textureHorizontalGrid ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.textureHorizontalGrid = value ;
			}
		}

		/// <summary>
		/// テクスチャの横方向の分割数(ショートカット)
		/// </summary>
		public int textureVerticalGrid
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.textureVerticalGrid ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.textureVerticalGrid = value ;
			}
		}

		/// <summary>
		/// テクスチャのパディング(ショートカット)
		/// </summary>
		public int textureGridPadding
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.textureGridPadding ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.textureGridPadding = value ;
			}
		}

		/// <summary>
		/// マップデータの設定(ショートカット)
		/// </summary>
		/// <param name="tMap"></param>
		public void SetData( int[,] tData )
		{
			GridMap tGridMap = _gridMap ;
			if( tGridMap == null )
			{
				return ;
			}
			tGridMap.SetData( tData ) ;
		}

		/// <summary>
		/// トランジションの方向(ショートカット)
		/// </summary>
		public GridMap.TransitionType transitionType
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.transitionType ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.transitionType = value ;
			}
		}

		/// <summary>
		/// トランジションの状態値(ショートカット)
		/// </summary>
		public float transitionFactor
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.transitionFactor ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.transitionFactor = value ;
			}
		}

		/// <summary>
		/// トランジションの強度値(ショートカット)
		/// </summary>
		public float transitionIntensity
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return 0 ;
				}
				return tGridMap.transitionIntensity ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.transitionIntensity = value ;
			}
		}

		/// <summary>
		/// トランジションの状態反転(ショートカット)
		/// </summary>
		public bool transitionReverse
		{
			get
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return false ;
				}
				return tGridMap.transitionReverse ;
			}
			set
			{
				GridMap tGridMap = _gridMap ;
				if( tGridMap == null )
				{
					return ;
				}
				tGridMap.transitionReverse = value ;
			}
		}

		//----------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			GridMap tGridMap = _gridMap ;

			if( tGridMap == null )
			{
				tGridMap = gameObject.AddComponent<GridMap>() ;
			}
			if( tGridMap == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			tGridMap.color = Color.white ;

			vertexHorizontalGrid	= 16 ;
			vertexVerticalGrid		= 16 ;
			vertexDensity			= GridMap.VertexDensity.Low ;


			ResetRectTransform() ;
		}
	}
}

