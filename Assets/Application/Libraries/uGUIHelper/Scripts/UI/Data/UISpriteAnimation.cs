using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// パラパラアニメの情報を保持する
	/// </summary>
	public class UISpriteAnimation : ScriptableObject
	{
		[HideInInspector][SerializeField]
		private Texture m_Texture ;
		public  Texture   texture
		{
			get
			{
				return m_Texture ;
			}
			set
			{
				m_Texture = value ;
			}
		}

		[HideInInspector][SerializeField]
		private List<Sprite> m_SpriteList = null ;
		
		/// <summary>
		/// スプライト情報の展開の有無
		/// </summary>
		public bool exist
		{
			get
			{
				if( m_SpriteList != null && m_SpriteList.Count >  0 )
				{
					return true ;
				}

				return false ;
			}
		}

		/// <summary>
		/// タイムスケール
		/// </summary>
		public float timeScale = 1.0f ;

		/// <summary>
		/// アトラスタイプのスプライトをセットする
		/// </summary>
		/// <param name="tSpriteList"></param>
		public void SetSprite( Sprite[] tSpriteList )
		{
			m_SpriteList = new List<Sprite>() ;

			int i, l = tSpriteList.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_SpriteList.Add( tSpriteList[ i ] ) ;
			}

			// テクスチャはどこも共通
			texture = tSpriteList[ 0 ].texture ;
		}

		/// <summary>
		/// スプライトを取得する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Sprite GetSprite( string tName )
		{
			if( m_SpriteList == null )
			{
				return null ;
			}

			int i, l = m_SpriteList.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SpriteList[ i ].name == tName )
				{
					return m_SpriteList[ i ] ;
				}
			}

			return null ;
		}

		/// <summary>
		/// スプライトを取得する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Sprite GetSprite( int tIndex )
		{
			if( m_SpriteList == null )
			{
				return null ;
			}

			if( tIndex <  0 || tIndex >= m_SpriteList.Count )
			{
				return null ;
			}

			return m_SpriteList[ tIndex ] ;
		}


		/// <summary>
		/// 指定のフレームのスプライトインデックス番号を取得する
		/// </summary>
		public Sprite GetSpriteOfFrame( int tFrameIndex )
		{
			if( m_SpriteList == null )
			{
				return null ;
			}

			if( tFrameIndex <  0 || tFrameIndex >= m_Frame.Count )
			{
				return null ;
			}

			int tSpriteIndex = m_Frame[ tFrameIndex ].spriteIndex ;

			if( tSpriteIndex <  0 || tSpriteIndex >= m_SpriteList.Count )
			{
				return null ;
			}

			return m_SpriteList[ tSpriteIndex ] ;
		}

		/// <summary>
		/// 全てのスプライト情報を取得する
		/// </summary>
		/// <returns></returns>
		public Sprite[] GetSpriteAll()
		{
			if( m_SpriteList == null )
			{
				return null ;
			}

			return m_SpriteList.ToArray() ;
		}

		/// <summary>
		/// スプライトの数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetSpriteCount()
		{
			if( m_SpriteList == null )
			{
				return 0 ;
			}

			return m_SpriteList.Count ;
		}

		/// <summary>
		/// アトラスタイプのスプライト情報をクリアする
		/// </summary>
		public void ClearSprite()
		{
			m_SpriteList = null ;
		}

		/// <summary>
		/// アトラスタイプのスプライトの名前リストを取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetSpriteNameList()
		{
			if( m_SpriteList == null || m_SpriteList.Count == 0 )
			{
				return null ;
			}

			int i, l = m_SpriteList.Count ;
			string[] tName = new string[ l ] ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tName[ i ] = m_SpriteList[ i ].name ;
			}

			return tName ;
		}

		// フレーム管理クラス
		[System.Serializable]
		public class Frame
		{
			public int		spriteIndex ;
			public float	duration ;

			public Frame( int tSpriteIndex, float tDuration )
			{
				spriteIndex = tSpriteIndex ;
				duration = tDuration ;
			}
		}

		[HideInInspector][SerializeField]
		private List<Frame> m_Frame = new List<Frame>() ;
		public  List<Frame>   frame
		{
			get
			{
				return m_Frame ;
			}
		}

		// インデクサを使って連想配列っぽいことをやる
		public Frame this[ int tFrameIndex ]
		{
			get
			{
				if( m_Frame == null )
				{
					return null ;
				}

				if( tFrameIndex <  0 || tFrameIndex >= m_Frame.Count )
				{
					return null ;
				}

				return m_Frame[ tFrameIndex ] ;
			}
			set
			{
				if( m_Frame == null )
				{
					return ;
				}

				if( tFrameIndex <  0 || tFrameIndex >= m_Frame.Count )
				{
					return ;
				}

				m_Frame[ tFrameIndex ] = value ;
			}
		}




		/// <summary>
		/// フレームを挿入および追加
		/// </summary>
		/// <param name="tFrameIndex"></param>
		/// <param name="tSpriteIndex"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public bool Insert( int tFrameIndex, int tSpriteIndex, float tDuration )
		{
			if( tFrameIndex <  0 || m_SpriteList == null || tSpriteIndex <  0 || tSpriteIndex >= m_SpriteList.Count || tDuration <  0 )
			{
				return false ;
			}

			if( tFrameIndex <  m_Frame.Count )
			{
				m_Frame.Insert( tFrameIndex, new Frame( tSpriteIndex, tDuration ) ) ;
			}
			else
			{
				m_Frame.Add( new Frame( tSpriteIndex, tDuration ) ) ;
			}

			return true ;
		}

		/// <summary>
		/// フレームをまとめて削除
		/// </summary>
		/// <param name="tFrameIndex"></param>
		public void Remove( params int[] tFrameIndex )
		{
			if( tFrameIndex == null || tFrameIndex.Length == 0 )
			{
				return ;
			}

			int i, l = tFrameIndex.Length ;
			List<Frame> tList = new List<Frame>() ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tList.Add( m_Frame[ tFrameIndex[ i ] ] ) ;
			}

			if( tList.Count >  0 )
			{
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_Frame.Contains( tList[ i ] ) == true )
					{
						m_Frame.Remove( tList[ i ] ) ;
					}
				}
			}
		}

		/// <summary>
		/// フレーム数を返す
		/// </summary>
		public int length
		{
			get
			{
				return m_Frame.Count ;
			}
		}
	}
}
