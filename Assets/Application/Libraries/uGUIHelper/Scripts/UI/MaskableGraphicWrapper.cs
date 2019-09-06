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
	/// 拡張ＵＩのベースとなる基底クラス
	/// </summary>
	[ RequireComponent( typeof( RectTransform ) ) ]
	[ RequireComponent( typeof( CanvasRenderer ) ) ]
	public class MaskableGraphicWrapper : MaskableGraphic
	{
		[SerializeField][HideInInspector]
		private   RectTransform	m_RectTransform ;
		protected RectTransform	 _RectTransform
		{
			get
			{
				if( m_RectTransform != null )
				{
					return m_RectTransform ;
				}
				m_RectTransform = GetComponent<RectTransform>() ;
				return m_RectTransform ;
			}
		}


		protected Vector2 size
		{
			get
			{
				RectTransform tRectTransform = _RectTransform ;
				if( tRectTransform == null )
				{
					return Vector2.zero ;
				}
				Vector2 tSize = tRectTransform.sizeDelta ;

				// Ｘのチェック
				if( _RectTransform.anchorMin.x != _RectTransform.anchorMax.x )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> tList = new List<RectTransform>() ;
					tList.Add( _RectTransform ) ;

					Transform tParent = _RectTransform.parent ;
					RectTransform tTarget ;
					float tDelta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( tParent != null )
						{
							tTarget = tParent.GetComponent<RectTransform>() ;
							if( tTarget != null )
							{
								if( tTarget.anchorMin.x == tTarget.anchorMax.x  )
								{
									// 発見
									tDelta = tTarget.sizeDelta.x ;
									break ;
								}
								else
								{
									tList.Add( tTarget ) ;
								}
							}
							tParent = tParent.parent ;
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( tDelta >   0 )
					{
						// マージン分を引く
						for( int i  = tList.Count - 1 ; i >= 0 ; i -- )
						{
							tDelta = tDelta * ( tList[ i ].anchorMax.x - tList[ i ].anchorMin.x ) ;
							tDelta = tDelta + tList[ i ].sizeDelta.x ;
						}

						tSize.x = tDelta ;
					}
				}

				// Ｙのチェック
				if( _RectTransform.anchorMin.y != _RectTransform.anchorMax.y )
				{
					// Stretch なので Stretch じゃなくなるまで親をたどる
					List<RectTransform> tList = new List<RectTransform>() ;
					tList.Add( _RectTransform ) ;

					Transform tParent = _RectTransform.parent ;
					RectTransform tTarget ;
					float tDelta = 0 ;
					for( int i  = 0 ; i <  64 ; i ++ )
					{
						if( tParent != null )
						{
							tTarget = tParent.GetComponent<RectTransform>() ;
							if( tTarget != null )
							{
								if( tTarget.anchorMin.y == tTarget.anchorMax.y )
								{
									// 発見
									tDelta = tTarget.sizeDelta.y ;
									break ;
								}
								else
								{
									tList.Add( tTarget ) ;
								}
							}
							tParent = tParent.parent ;
						}
						else
						{
							// 検索終了
							break ;
						}
					}

					if( tDelta >   0 )
					{
						// マージン分を引く
						for( int i  = tList.Count - 1 ; i >= 0 ; i -- )
						{
							tDelta = tDelta * ( tList[ i ].anchorMax.y - tList[ i ].anchorMin.y ) ;
							tDelta = tDelta + tList[ i ].sizeDelta.y ;
						}

						tSize.y = tDelta ;
					}
				}

				return tSize ;
			}
		}


		[SerializeField][HideInInspector]
		private   CanvasRenderer	m_CanvasRenderer ;
		protected CanvasRenderer	 _CanvasRenderer
		{
			get
			{
				if( m_CanvasRenderer != null )
				{
					return m_CanvasRenderer ;
				}
				m_CanvasRenderer = GetComponent<CanvasRenderer>() ;
				return m_CanvasRenderer ;
			}
		}

		public new Color color
		{
			get
			{
				return base.color ;
			}
			set
			{
				if( base.color.r != value.r || base.color.g != value.g || base.color.b != value.b || base.color.a != value.a )
				{
					base.color = value ;
					_CanvasRenderer.SetColor( value ) ;
				}
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			_CanvasRenderer.SetColor( base.color ) ;
		}
	}
}
