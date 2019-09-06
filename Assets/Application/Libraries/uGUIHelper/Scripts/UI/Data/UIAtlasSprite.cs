using UnityEngine ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// アトラスタイプのププライト管理用のクラス
	/// </summary>
	[System.Serializable]
	public class UIAtlasSprite /*: ScriptableObject*/
	{
		[HideInInspector][SerializeField]
		private Texture2D m_Texture ;
		public  Texture2D   texture
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
		private string m_Path ;
		public  string   path
		{
			get
			{
				return m_Path ;
			}
			set
			{
				m_Path = value ;
			}
		}

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<Sprite> m_SpriteList ;

		// 個々のスプライト情報を保持する
		[SerializeField][HideInInspector]
		private List<string> m_NameList ;


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
		/// アトラスタイプのスプライトを生成する
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public static UIAtlasSprite Create( string tPath = null )
		{
//			UIAtlasSprite tAtlasSprite = ScriptableObject.CreateInstance<UIAtlasSprite>() ;
			UIAtlasSprite tAtlasSprite = new UIAtlasSprite() ;

			if( string.IsNullOrEmpty( tPath ) == false )
			{
				if( tAtlasSprite.Load( tPath ) == false )
				{
//					DestroyImmediate( tAtlasSprite ) ;
					return null ;
				}
			}

			return tAtlasSprite ;
		}

		/// <summary>
		/// 指定したパスからアトラスタイプのスプライトをロードする
		/// </summary>
		/// <param name="tPath"></param>
		/// <returns></returns>
		public bool Load( string tPath = "" )
		{
			if( string.IsNullOrEmpty( tPath ) == true )
			{
				tPath = path ;
			}

			Sprite[] tSprite = Resources.LoadAll<Sprite>( tPath ) ;
			if( tSprite == null )
			{
				return false ;
			}

			if( tSprite.Length == 0 )
			{
				return false ;
			}

			Set( tSprite ) ;

			return true ;
		}

		/// <summary>
		/// アトラスタイプのスプライトをセットする
		/// </summary>
		/// <param name="tSpriteList"></param>
		public void Set( Sprite[] tSpriteList )
		{
			m_SpriteList = new List<Sprite>() ;
			m_NameList = new List<string>() ;

			int i, l = tSpriteList.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_SpriteList.Add( tSpriteList[ i ] ) ;
				m_NameList.Add( tSpriteList[ i ].name ) ;
			}

			// テクスチャはどこも共通
			texture = tSpriteList[ 0 ].texture ;
		}

		/// <summary>
		/// アトラスタイプのスプライト情報をクリアする
		/// </summary>
		public void Clear()
		{
			m_NameList = null ;
			m_SpriteList = null ;
			m_Texture = null ;
		}

		/// <summary>
		/// インデクサを使ってアクセスを簡易化
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public Sprite this[ string tName ]
		{
			get
			{
				if( m_SpriteList == null )
				{
					return null ;
				}

				int i, l = m_SpriteList.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_SpriteList[ i ] != null && m_SpriteList[ i ].name == tName )
					{
						return m_SpriteList[ i ] ;
					}
				}

				return null ;
			}
		}

		/// <summary>
		/// アトラスタイプのスプライトの名前リストを取得する
		/// </summary>
		/// <returns></returns>
		public string[] GetNameList()
		{
			if( m_SpriteList == null || m_SpriteList.Count == 0 )
			{
				return null ;
			}

			int i, l = m_SpriteList.Count ;

			List<string> tNameList = new List<string>() ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_SpriteList[ i ] != null )
				{
					tNameList.Add( m_SpriteList[ i ].name ) ;
				}
				else
				{
					// 異常発生
				}
			}

			return tNameList.ToArray() ;
		}

		/// <summary>
		/// 要素の数を返す
		/// </summary>
		/// <returns></returns>
		public int length
		{
			get
			{
				if( m_SpriteList == null )
				{
					return 0 ;
				}
	
				return m_SpriteList.Count ;
			}
		}

		/// <summary>
		/// データに異常が無く使用可能な状態にあるか
		/// </summary>
		public bool isAvailable
		{
			get
			{
				if( m_SpriteList == null || m_NameList == null )
				{
					return false ;
				}

				if( m_SpriteList.Count != m_NameList.Count )
				{
					return false ;
				}

				int i, l = m_SpriteList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_SpriteList[ i ] == null || m_NameList[ i ] == null )
					{
						return false ;
					}

					if( m_SpriteList[ i ].name != m_NameList[ i ] )
					{
						return false ;
					}
				}

				return true ;
			}
		}
	}
}
