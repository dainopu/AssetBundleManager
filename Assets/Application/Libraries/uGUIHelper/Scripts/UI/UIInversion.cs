using UnityEngine ;
using System.Collections.Generic ;
using UnityEngine.UI ;

namespace uGUIHelper
{
	/// <summary>
	/// 画像を上下左右三転するコンポーネント
	/// </summary>
	public class UIInversion : BaseMeshEffect
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

		public enum Direction
		{
			None,
			Horizontal,
			Vertical,
			Both,
		}
		
		[SerializeField]
		private Direction m_Direction = Direction.None ;

		public Direction direction
		{
			get
			{
				return m_Direction ;
			}
			set
			{
				if( m_Direction != value )
				{
					m_Direction  = value ;
					Refresh() ;
				}
			}
		}

		public override void ModifyMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}
		
			List<UIVertex> tList = new List<UIVertex>() ;
			tHelper.GetUIVertexStream( tList ) ;
			
			if( m_Direction != Direction.None && _RectTransform != null )
			{
				ModifyVertices( tList ) ;
			}
				
			tHelper.Clear() ;
			tHelper.AddUIVertexTriangleStream( tList ) ;
		}
	
		private void ModifyVertices( List<UIVertex> tList )
		{
			if( IsActive() == false || tList == null || tList.Count == 0 )
			{
				return ;
			}
		
			UIVertex v ;

			float dx = _RectTransform.sizeDelta.x * ( ( _RectTransform.pivot.x - 0.5f ) * 2.0f ) ;
			float dy = _RectTransform.sizeDelta.y * ( ( _RectTransform.pivot.y - 0.5f ) * 2.0f ) ;

			v = tList[ 0 ] ;
			for( int i  = 0 ; i <  tList.Count ; i ++ )
			{
				v = tList[ i ] ;

				switch( m_Direction )
				{
					// 左右のみ
					case Direction.Horizontal :
						v.position.x = - v.position.x - dx ;
					break ;

					// 上下のみ
					case Direction.Vertical :
						v.position.y = - v.position.y - dy ;
					break ;

					// 左右上下
					case Direction.Both :
						v.position.x = - v.position.x - dx ;
						v.position.y = - v.position.y - dy ;
					break ;
				}

				tList[ i ] = v ;
			}
		}
	
		public void Refresh()
		{
			if( graphic != null )
			{
				graphic.SetVerticesDirty() ;
			}
		}
	}
}
