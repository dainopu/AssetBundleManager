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
	[ RequireComponent( typeof( Line ) ) ]
	public class UILine : UIView
	{
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return Color.white ;
				}
				return tLine.color ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.color = value ;
			}
		}
	
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return null ;
				}
				return tLine.material ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.material = value ;
			}
		}


		/// <summary>
		/// スプライト(ショートカット)
		/// </summary>
		public  Sprite  sprite
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return null ;
				}
				return tLine.sprite ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.sprite = value ;
			}
		}


		/// <summary>
		/// 最初のカラー(ショートカット)
		/// </summary>
		public    Color  startColor
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return Color.white ;
				}
				return tLine.startColor ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.startColor = value ;
			}
		}
		
		/// <summary>
		/// 最後のカラー(ショートカット)
		/// </summary>
		public    Color  endColor
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return Color.white ;
				}
				return tLine.endColor ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.endColor = value ;
			}
		}
	
		/// <summary>
		/// 最初の太さ(ショートカット)
		/// </summary>
		public    float  startWidth
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return 0 ;
				}
				return tLine.startWidth ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.startWidth = value ;
			}
		}
	
		/// <summary>
		/// 最後の太さ(ショートカット)
		/// </summary>
		public    float  endWidth
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return 0 ;
				}
				return tLine.endWidth ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.endWidth = value ;
			}
		}
		
		/// <summary>
		/// オフセット(ショートカット)
		/// </summary>
		public  Vector2 offset
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return Vector2.zero ;
				}
				return tLine.offset ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.offset = value ;
			}
		}

		/// <summary>
		/// 頂点配列(ショートカット)
		/// </summary>
		public Vector2[] vertices
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return null ;
				}
				return tLine.vertices ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.vertices = value ;
				tLine.SetAllDirty() ;
			}
		}

		/// <summary>
		/// 座標の位置タイプ(ショートカット)
		public  Line.PositionType  positionType
		{
			get
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return Line.PositionType.Relative ;
				}
				return tLine.positionType ;
			}
			set
			{
				Line tLine = _line ;
				if( tLine == null )
				{
					return ;
				}
				tLine.positionType = value ;
			}
		}

		
		/// <summary>
		/// トレイルモード
		/// </summary>
		[SerializeField][HideInInspector]
		private bool m_TrailEnabled = false ;
		public  bool   trailEnabled
		{
			get
			{
				return m_TrailEnabled ;
			}
			set
			{
				if( m_TrailEnabled != value )
				{
					m_TrailEnabled  = value ;
					if( m_TrailEnabled == true )
					{
						vertices = null ;
					}
				}
			}
		}

		/// <summary>
		/// トレイル用の頂点が消えるまでの時間
		/// </summary>
		public float trailKeepTime = 0.25f ;


		public class TrailData
		{
			public Vector2	position ;
			public float	time ;

			public TrailData( Vector2 tPosition, float tTime )
			{
				position	= tPosition ;
				time		= tTime ;
			}
		}

		private List<TrailData> m_TrailData = new List<TrailData>() ;



		//---------------------------------------------------------------------

		/// <summary>
		/// ＵＩのサイズを文字のサイズに自動調整するかどうか
		/// </summary>
//		public bool autoSizeFitting = true ;


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			Line tLine = _line ;

			if( tLine == null )
			{
				tLine = gameObject.AddComponent<Line>() ;
			}
			if( tLine == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			// Default
			tLine.color = Color.white ;

			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tLine.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			//----------------------------------------------------------

			ResetRectTransform() ;
		}

		protected override void OnStart()
		{
			base.OnStart() ;

			if( m_TrailEnabled == true )
			{
				// トレイルが有効である場合は消しておく
				vertices = null ;
			}
		}

		protected override void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			if( m_TrailEnabled == true )
			{
				ProcessTrail() ;
			}
		}

		/// <summary>
		/// トレイルの頂点を追加する
		/// </summary>
		/// <param name="tMove"></param>
		public void AddTrailPosition( Vector2 tMove )
		{
			if( m_TrailEnabled == false )
			{
				return ;
			}

			float t = Time.realtimeSinceStartup ;

			int l = m_TrailData.Count ;
			if( l == 0 )
			{
				m_TrailData.Add( new TrailData( tMove, t ) ) ;
			}
			else
			{
				if( m_TrailData[ l - 1 ].position != tMove )
				{
					m_TrailData.Add( new TrailData( tMove, t ) ) ;
				}
			}
		}

		/// <summary>
		/// トレイルを処理する
		/// </summary>
		private void ProcessTrail()
		{
			int i, l  ;
		
			if( m_TrailData.Count == 0 )
			{
				vertices = null ;
				return ;
			}

			l = m_TrailData.Count ;

			float t = Time.realtimeSinceStartup ;
		
			// 経過時間で頂点を消していく
			for( i  =    0 ; i < l ; i ++ )
			{
				if( ( t - m_TrailData[ 0 ].time ) >  trailKeepTime )
				{
					m_TrailData.RemoveAt( 0 ) ;
				}
				else
				{
					break ;
				}
			}

			if( m_TrailData.Count >= 2 )
			{
				List<Vector2> tLineArray = new List<Vector2>() ;
				l = m_TrailData.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tLineArray.Add( m_TrailData[ i ].position ) ;
				}

				vertices = tLineArray.ToArray() ;
			}
			else
			{
				vertices = null ;
			}
		}




		/*		override protected void OnLateUpdate()
				{
					if( autoSizeFitting == true )
					{
						Line t = _line ;
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
				}*/
	}
}

