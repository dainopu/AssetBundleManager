using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Security.Cryptography ;
using System.Linq ;

using UnityEngine ;
using UnityEngine.Networking ;

#if UNITY_EDITOR
using UnityEditor ;
using UnityEditor.SceneManagement ;
#endif

using StorageHelper ;

/// <summary>
/// アセットバンドルヘルパーパッケージ
/// </summary>
namespace AssetBundleHelper
{
	/// <summary>
	/// アセットバンドルマネージャクラス(メソッド)
	/// </summary>
	public partial class AssetBundleManager : MonoBehaviour
	{
		// AssetBundleManager :: Asset

#if UNITY_EDITOR

		/// <summary>
		/// ローカルアセットバンドルパスからアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private UnityEngine.Object LoadLocalAsset( string path, Type type )
		{
			path = m_LocalAssetBundleRootPath + path ;
			
			// 最初はそのままロードを試みる
			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath( path, type ) ;
			if( asset != null )
			{
				// 成功したら終了
				return asset ;
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般的なタイプ
					foreach( string extension in m_TypeToExtension[ type ] )
					{
						asset = AssetDatabase.LoadAssetAtPath( path + extension, type ) ;
						if( asset != null )
						{
							return asset ;
						}
					}

					Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
					return null ;
				}
				else
				{
					// 不明なタイプ
					return AssetDatabase.LoadAssetAtPath( path + ".asset", type ) ;
				}
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスからサブアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <param name="path"></param>
		/// <param name="subAssetName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private UnityEngine.Object LoadLocalSubAsset( string path, string subAssetName, Type type )
		{
			path = m_LocalAssetBundleRootPath + path ;
			
			// 最初はそのままロードを試みる
			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path ) ;
			if( assets != null && assets.Length >  0 )
			{
				// 成功したら終了
				UnityEngine.Object asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
				return ( asset != null && asset.GetType() == type ) ? asset : null ;
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般的なタイプ
					foreach( string extension in m_TypeToExtension[ type ] )
					{
						assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + extension ) ;
						if( assets != null && assets.Length >  0 )
						{
							// 成功したら終了
							UnityEngine.Object asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
							return ( asset != null && asset.GetType() == type ) ? asset : null ;
						}
					}

					Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
					return null ;
				}
				else
				{
					// 不明なタイプ
					assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + ".asset" ) ;
					if( assets != null && assets.Length >  0 )
					{
						// 成功したら終了
						UnityEngine.Object asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
						return ( asset != null && asset.GetType() == type ) ? asset : null ;
					}

					Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
					return null ;
				}
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットバンドルパスから全てのサブアセットの取得を行う(同期)　※非同期は存在しない
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		private UnityEngine.Object[] LoadLocalAllSubAssets( string path, Type type )
		{
			path = m_LocalAssetBundleRootPath + path ;
			
			// 最初はそのままロードを試みる
			UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path ) ;
			if( assets != null && assets.Length >  0 )
			{
				// 成功したら終了
				return assets.Where( _ => _.GetType() == type ).ToArray() ;
			}
			
			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				if( m_TypeToExtension.ContainsKey( type ) == true )
				{
					// 一般的なタイプ
					foreach( string extension in m_TypeToExtension[ type ] )
					{
						assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + extension ) ;
						if( assets != null && assets.Length >  0 )
						{
							// 成功したら終了
							return assets.Where( _ => _.GetType() == type ).ToArray() ;
						}
					}

					Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
					return null ;
				}
				else
				{
					// 不明なタイプ
					assets = AssetDatabase.LoadAllAssetRepresentationsAtPath( path + ".asset" ) ;
					if( assets != null && assets.Length >  0 )
					{
						// 成功したら終了
						return assets.Where( _ => _.GetType() == type ).ToArray() ;
					}

					Debug.LogWarning( "Unknown Extension : " + path + " " + type.ToString() ) ;
					return null ;
				}
			}
			else
			{
				// 拡張子あり(失敗)
				return null ;
			}
		}

		/// <summary>
		/// ローカルアセットパスからシーンをロードする
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private IEnumerator OpenLocalSceneAsync( string path, string sceneName, Type type, UnityEngine.SceneManagement.LoadSceneMode mode, Request request )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				request.Error = "Bad scene name" ;
				yield break ;
			}
			
			//----------------------------------------------------------

			if( type != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindObjectOfType( type ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------

			path = m_LocalAssetBundleRootPath + path ;

			// 拡張子が無い場合はタイプ検索を行う
			int i0 = path.LastIndexOf( '/' ) ;
			int i1 = path.LastIndexOf( '.' ) ;
			if( i1 <= i0 )
			{
				// 拡張子なし
				path += ".unity" ;
			}

			EditorSceneManager.LoadSceneInPlayMode( path, new UnityEngine.SceneManagement.LoadSceneParameters( mode ) ) ;
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			if( scene.IsValid() == false )
			{
				request.Error = "Could not load." ;
				yield break ;
			}
		}

		//---------------

		// タイプに対する拡張子
		private static Dictionary<Type,List<string>> m_TypeToExtension = new Dictionary<Type, List<string>>()
		{
			{ typeof( Sprite ),		new List<string>{ ".png", ".jpg", ".bmp", ".tiff",		} },
			{ typeof( GameObject ), new List<string>{ ".prefab",							} },
			{ typeof( AudioClip ),	new List<string>{ ".wav", ".ogg", ".mp3",				} },
			{ typeof( TextAsset ),	new List<string>{ ".txt", ".bytes", ".xml", ".json",	} },
			{ typeof( Texture2D ),	new List<string>{ ".png", ".jpg", ".bmp", ".tiff",		} },
			{ typeof( Texture ),	new List<string>{ ".png", ".jpg", ".bmp", ".tiff",		} },
			{ typeof( Material ),	new List<string>{ ".mat",								} },
		} ;
#endif

		//-------------------------------------------------------------------

		/// <summary>
		/// アセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static T LoadAsset<T>( string path, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			return m_Instance == null ? null : m_Instance.LoadAsset_Private( path, typeof( T ), cachingType ) as T ;
		}

		/// <summary>
		/// アセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static UnityEngine.Object LoadAsset( string path, Type type, CachingType cachingType = CachingType.None )
		{
			return m_Instance == null ? null : m_Instance.LoadAsset_Private( path, type, cachingType ) ;
		}

		// アセットを取得する(同期版)
		private UnityEngine.Object LoadAsset_Private( string path, Type type, CachingType cachingType )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = resourcePath + ":" + type.ToString() ;

			//--------------

			// キャッシュにあればそれを返す
			if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				return ( m_ResourceCache[ resourceCachePath ] ) ;
			}

			//------------------------------------------------

			UnityEngine.Object asset = null ;

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( asset == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.SyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							asset = Resources.Load( resourcePath, type ) ;
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && asset == null )
						{
							// ローカルアセットからロードを試みる
							asset = LoadLocalAsset( resourcePath, type ) ;
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									asset = m_ManifestHash[ manifestName ].LoadAsset( assetBundleName, assetName, type, assetBundleCaching, this ) ;
								}
							}
						}
					}
				}
				if( asset != null )
				{
					break ;
				}
			}

			if( asset == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == false )
			{
				m_ResourceCache.Add( resourceCachePath, asset ) ;
			}

			//------------------------------------------------

			return asset ;
		}

		//---------------

		/// <summary>
		/// アセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAssetAsync<T>( string path, Action<T> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetAsync_Private( path, typeof( T ), ( UnityEngine.Object asset ) => { onLoaded?.Invoke( asset as T ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		/// <summary>
		/// アセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAssetAsync( string path, Type type, Action<UnityEngine.Object> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetAsync_Private( path, type, onLoaded, cachingType, keep, request ) ) ;
			return request ;
		}
		
		// アセットを取得する(非同期版)
		private IEnumerator LoadAssetAsync_Private( string path, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType, bool keep, Request request )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = resourcePath + ":" + type.ToString() ;

			//------------------------------------------------

			UnityEngine.Object asset = null ;

			// キャッシュにあればそれを返す
			if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				asset = m_ResourceCache[ resourceCachePath ] ;
				onLoaded?.Invoke( asset ) ;
				request.Asset = asset ;
				request.IsDone = true ;
				yield break ;
			}

			//------------------------------------------------

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( asset == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.AsyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							ResourceRequest resourceRequest ;
							yield return resourceRequest = Resources.LoadAsync( resourcePath, type ) ;
							if( resourceRequest.isDone == true )
							{
								asset = resourceRequest.asset ;
							}
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && asset == null )
						{
							// ローカルアセットからロードを試みる
							asset = LoadLocalAsset( resourcePath, type ) ;
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									UnityEngine.Object[] rAssetHolder = { null } ;
									yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAsset_Coroutine( assetBundleName, assetName, type, rAssetHolder, keep, null, request, assetBundleCaching, this ) ) ;
									asset = rAssetHolder[ 0 ] ;
								}
							}
						}
					}
				}
				if( asset != null )
				{
					break ;
				}
			}

			if( asset == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Could not load" ;
				}
				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == false )
			{
				m_ResourceCache.Add( resourceCachePath, asset ) ;
			}

			//------------------------------------------------

			onLoaded?.Invoke( asset ) ;
			request.Asset = asset ;
			request.IsDone = true ;
		}
		
		//---------------------------------------------------------------------------

		// AssetBundleManager :: SubAsset

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセット名</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static T LoadSubAsset<T>( string path, string subAssetName, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			return m_Instance == null ? null : m_Instance.LoadSubAsset_Private( path, subAssetName, typeof( T ), cachingType ) as T ;
		}

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセット名</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static UnityEngine.Object LoadSubAsset( string path, string subAssetName, Type type, CachingType cachingType = CachingType.None )
		{
			return m_Instance == null ? null : m_Instance.LoadSubAsset_Private( path, subAssetName, type, cachingType ) ;
		}

		// アセットに含まれるサブアセットを取得する(同期版)
		private UnityEngine.Object LoadSubAsset_Private( string path, string subAssetName, Type type, CachingType cachingType )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = resourcePath + "/" + subAssetName + ":" + type.ToString() ;

			//--------------

			// キャッシュにあればそれを返す
			if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				return ( m_ResourceCache[ resourceCachePath ] ) ;
			}

			//------------------------------------------------

			UnityEngine.Object asset = null ;

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( asset == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.SyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							UnityEngine.Object[] assets = Resources.LoadAll( resourcePath, type ) ;
							if( assets != null && assets.Length >  0 )
							{
								asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
							}
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && asset == null )
						{
							// ローカルアセットからロードを試みる
							asset = LoadLocalSubAsset( resourcePath, subAssetName, type ) ;
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									asset = m_ManifestHash[ manifestName ].LoadSubAsset( assetBundleName, assetName, subAssetName, type, assetBundleCaching, this, resourcePath ) ;
								}
							}
						}
					}
				}
				if( asset != null )
				{
					break ;
				}
			}

			if( asset == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == false )
			{
				m_ResourceCache.Add( resourceCachePath, asset ) ;
			}

			//------------------------------------------------

			return asset ;
		}

		//---------------

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセット名</param>
		/// <param name="rAsset">サブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadSubAssetAsync<T>( string path, string subAssetName, Action<T> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadSubAssetAsync_Private( path, subAssetName, typeof( T ), ( UnityEngine.Object asset ) => { onLoaded?.Invoke( asset as T ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		/// <summary>
		/// アセットに含まれるサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tSubAssetName">サブアセット名</param>
		/// <param name="rAsset">サブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadSubAssetAsync( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadSubAssetAsync_Private( path, subAssetName, type, onLoaded, cachingType, keep, request ) ) ;
			return request ;
		}
		
		// アセットに含まれるサブアセットを取得する(非同期版)
		private IEnumerator LoadSubAssetAsync_Private( string path, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded, CachingType cachingType, bool keep, Request request )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			// 同名型違いが存在するため型名を最後に付与する
			string resourceCachePath = resourcePath + "/" + subAssetName + ":" + type.ToString() ;

			//------------------------------------------------

			UnityEngine.Object asset = null ;

			// キャッシュにあればそれを返す
			if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == true )
			{
				asset = m_ResourceCache[ resourceCachePath ] ;
				onLoaded?.Invoke( asset ) ;
				request.Asset = asset ;
				request.IsDone = true ;
				yield break ;
			}

			//------------------------------------------------

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( asset == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.AsyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる(LoadAllに関しては非同期版が存在しない)
							UnityEngine.Object[] assets = Resources.LoadAll( resourcePath, type ) ;
							if( assets != null && assets.Length >  0 )
							{
								asset = assets.FirstOrDefault( _ => _.name == subAssetName ) ;
							}
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && asset == null )
						{
							// ローカルアセットからロードを試みる
							asset = LoadLocalSubAsset( resourcePath, subAssetName, type ) ;
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									UnityEngine.Object[] rSubAssetHolder = { null } ;
									yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadSubAsset_Coroutine( assetBundleName, assetName, subAssetName, type, rSubAssetHolder, keep, null, request, assetBundleCaching, this, resourcePath ) ) ;
									asset = rSubAssetHolder[ 0 ] ;
								}
							}
						}
					}
				}
				if( asset != null )
				{
					break ;
				}
			}

			if( asset == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Could not load" ;
				}
				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null && m_ResourceCache.ContainsKey( resourceCachePath ) == false )
			{
				m_ResourceCache.Add( resourceCachePath, asset ) ;
			}

			//------------------------------------------------

			onLoaded?.Invoke( asset ) ;
			request.Asset = asset ;
			request.IsDone = true ;
		}

		//---------------------------------------------------------------------------

		// AssetBundleManager :: AllSubAssets

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static T[] LoadAllSubAssets<T>( string path, CachingType cachingType = CachingType.None ) where T : UnityEngine.Object
		{
			return m_Instance == null ? null : m_Instance.LoadAllSubAssets_Private( path, typeof( T ), cachingType ) as T[] ;
		}

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
		public static UnityEngine.Object[] LoadAllSubAssets( string path, Type type, CachingType cachingType = CachingType.None )
		{
			return m_Instance == null ? null : m_Instance.LoadAllSubAssets_Private( path, type, cachingType ) ;
		}

		// アセットバンドル内の指定の型の全てのサブアセットを直接取得する(同期版)
		private UnityEngine.Object[] LoadAllSubAssets_Private( string path, Type type, CachingType cachingType )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			string resourceCachePath ;
			
			//------------------------------------------------

			List<UnityEngine.Object> assets = null ;
			UnityEngine.Object[] temporaryAssets ;

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( assets == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.SyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							temporaryAssets = Resources.LoadAll( resourcePath, type ) ;
							if( temporaryAssets != null && temporaryAssets.Length >  0 )
							{
								assets = new List<UnityEngine.Object>() ;
								foreach( var asset in temporaryAssets )
								{
									resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
									if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourcePath ) == true )
									{
										// キャッシュにあればそれを返す
										assets.Add( m_ResourceCache[ resourceCachePath ] ) ;
									}
									else
									{
										assets.Add( asset ) ;
									}
								}
							}
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && assets == null && assets.Count == 0 )
						{
							// ローカルアセットバンドルパスからロードを試みる
							temporaryAssets = LoadLocalAllSubAssets( resourcePath, type ) ;
							if( temporaryAssets != null && temporaryAssets.Length >  0 )
							{
								assets = new List<UnityEngine.Object>() ;
								foreach( var asset in temporaryAssets )
								{
									resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
									if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourcePath ) == true )
									{
										// キャッシュにあればそれを返す
										assets.Add( m_ResourceCache[ resourceCachePath ] ) ;
									}
									else
									{
										assets.Add( asset ) ;
									}
								}
							}
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									assets = m_ManifestHash[ manifestName ].LoadAllSubAssets( assetBundleName, assetName, type, assetBundleCaching, this, resourcePath ) ;
								}
							}
						}
					}
				}
				if( assets != null )
				{
					break ;
				}
			}

			if( assets == null )
			{
				// 失敗
				return null ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null )
			{
				foreach( var asset in assets )
				{
					resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						m_ResourceCache.Add( resourceCachePath, asset ) ;
					}
				}
			}

			//------------------------------------------------

			return assets.ToArray() ;
		}

		//---------------

		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllSubAssetsAsync<T>( string path, Action<T[]> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAllSubAssetsAsync_Private( path, typeof( T ), ( UnityEngine.Object[] assets ) => { onLoaded?.Invoke( assets as T[] ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// アセットに含まれる全てのサブアセットを取得する(非同期版)
		/// </summary>
		/// <typeparam name="T">任意のコンポーネント型</typeparam>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tCaching">キャッシュするかどうか(true=する・false=しない)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAllSubAssetsAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, CachingType cachingType = CachingType.None, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAllSubAssetsAsync_Private( path, type, ( UnityEngine.Object[] assets ) => { onLoaded?.Invoke( assets ) ; }, cachingType, keep, request ) ) ;
			return request ;
		}

		// アセットに含まれる全てのサブアセットを取得する(非同期版)
		private IEnumerator LoadAllSubAssetsAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, CachingType cachingType, bool keep, Request request )
		{
			bool resourceCaching = false ;
			bool assetBundleCaching = false ;

			if( cachingType == CachingType.ResourceOnly || cachingType == CachingType.Same )
			{
				resourceCaching = true ;
			}
			if( cachingType == CachingType.AssetBundleOnly || cachingType == CachingType.Same )
			{
				assetBundleCaching = true ;
			}

			//------------------------------------------------

			string resourcePath = path ;
			resourcePath = resourcePath.Replace( "//", "/" ) ;

			string resourceCachePath ;
			
			//------------------------------------------------

			List<UnityEngine.Object> assets = null ;
			UnityEngine.Object[] temporaryAssets ;

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( assets == null )
				{
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.AsyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							temporaryAssets = Resources.LoadAll( resourcePath, type ) ;
							if( temporaryAssets != null && temporaryAssets.Length >  0 )
							{
								assets = new List<UnityEngine.Object>() ;
								foreach( var asset in temporaryAssets )
								{
									resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
									if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourcePath ) == true )
									{
										// キャッシュにあればそれを返す
										assets.Add( m_ResourceCache[ resourceCachePath ] ) ;
									}
									else
									{
										assets.Add( asset ) ;
									}
								}
							}
						}
#if UNITY_EDITOR
						if( m_UseLocalAsset == true && assets == null && assets.Count == 0 )
						{
							// ローカルアセットバンドルパスからロードを試みる
							temporaryAssets = LoadLocalAllSubAssets( resourcePath, type ) ;
							if( temporaryAssets != null && temporaryAssets.Length >  0 )
							{
								assets = new List<UnityEngine.Object>() ;
								foreach( var asset in temporaryAssets )
								{
									resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
									if( m_ResourceCache != null && m_ResourceCache.ContainsKey( resourcePath ) == true )
									{
										// キャッシュにあればそれを返す
										assets.Add( m_ResourceCache[ resourceCachePath ] ) ;
									}
									else
									{
										assets.Add( asset ) ;
									}
								}
							}
						}
#endif
					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
									yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAllSubAssets_Coroutine( assetBundleName, assetName, type, rAllSubAssetsHolder, keep, null, request, assetBundleCaching, this, resourcePath ) ) ;
									assets = rAllSubAssetsHolder[ 0 ] ;
								}
							}
						}
					}
				}
				if( assets != null )
				{
					break ;
				}
			}

			if( assets == null )
			{
				// 失敗
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Could not load" ;
				}
				yield break ;
			}

			//------------------------------------------------

			// 必要であればここでキャッシュに貯める
			if( resourceCaching == true && m_ResourceCache != null )
			{
				foreach( var asset in assets )
				{
					resourceCachePath = resourcePath + "/" + asset.name + ":" + type.ToString() ;
					if( m_ResourceCache.ContainsKey( resourceCachePath ) == false )
					{
						m_ResourceCache.Add( resourceCachePath, asset ) ;
					}
				}
			}

			//------------------------------------------------

			temporaryAssets = assets.ToArray() ;
			onLoaded?.Invoke( temporaryAssets ) ;
			request.Assets = temporaryAssets ;
			request.IsDone = true ;
		}
		
		//---------------------------------------------------------------------------

		// AssetBundleManager :: Scene

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync( string path, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <param name="rTarget"></param>
		/// <param name="tTargetName"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync<T>( string path, Action<T[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, typeof( T ), ( UnityEngine.Object[] targets ) => { onLoaded?.Invoke( targets as T[] ) ; }, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを展開する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <param name="rTarget"></param>
		/// <param name="tTargetName"></param>
		/// <returns></returns>
		public static Request LoadSceneAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Single, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <returns></returns>
		public static Request AddSceneAsync( string path, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, null, null, null, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}

		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <param name="rTarget"></param>
		/// <param name="tTargetName"></param>
		/// <returns></returns>
		public static Request AddSceneAsync<T>( string path, Action<T[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false ) where T : UnityEngine.Object
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, typeof( T ), ( UnityEngine.Object[] targets ) => { onLoaded?.Invoke( targets as T[] ) ; }, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}
		
		/// <summary>
		/// シーンを加算する(非同期版)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="tPath"></param>
		/// <param name="tName"></param>
		/// <param name="rTarget"></param>
		/// <param name="tTargetName"></param>
		/// <returns></returns>
		public static Request AddSceneAsync( string path, Type type, Action<UnityEngine.Object[]> onLoaded = null, string targetName = null, string sceneName = null, bool keep = false )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}
			
			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadOrAddSceneAsync_Private( path, type, onLoaded, targetName, sceneName, keep, UnityEngine.SceneManagement.LoadSceneMode.Additive, request ) ) ;
			return request ;
		}
		
		//-----------------------------------------------------------

		// シーンを展開または加算する(非同期版)
		private IEnumerator LoadOrAddSceneAsync_Private( string path, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, string sceneName, bool keep, UnityEngine.SceneManagement.LoadSceneMode mode, Request request )
		{
			// 名前の指定が無ければファイル名から生成する
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				int p = path.LastIndexOf( "/" ) ;
				if( p <  0 )
				{
					sceneName = path ;
				}
				else
				{
					p ++ ;
					sceneName = path.Substring( p, path.Length - p ) ;
					if( string.IsNullOrEmpty( sceneName ) == true )
					{
						request.Error = "Bad scene name" ;
						yield break ;	// シーン名が不明
					}
				}
			}

			//------------------------------------------------

			bool			result = false ;
			AssetBundle		assetBundle = null ;
			AssetBundle[]	rAssetBundleHolder = { null } ;

			UnityEngine.Object[] targets = null ;

			for( int t  = 0 ; t <  2 ; t ++ )
			{
				if( result == false )
				{
					request.Error = null ;
					if( ( t == 0 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						if( m_UseResources == UserResources.AsyncOnly || m_UseResources == UserResources.Same )
						{
							// リソースからロードを試みる
							yield return StartCoroutine( OpenSceneAsync_Private( sceneName, type, mode, request ) ) ;
							result = string.IsNullOrEmpty( request.Error ) ;
							if( result == true )
							{
								yield return StartCoroutine( WaitSceneAsync_Private( sceneName, type, ( _ ) => { targets = _ ; }, targetName, request ) ) ;
								result = string.IsNullOrEmpty( request.Error ) ;
							}
						}

#if UNITY_EDITOR
						if( m_UseLocalAsset == true && result == false )
						{
							// ローカルアセットからロードを試みる
							yield return StartCoroutine( OpenLocalSceneAsync( path, sceneName, type, mode, request ) ) ;
							result = string.IsNullOrEmpty( request.Error ) ;
							if( result == true )
							{
								yield return StartCoroutine( WaitSceneAsync_Private( sceneName, type, ( _ ) => { targets = _ ; }, targetName, request ) ) ;
								result = string.IsNullOrEmpty( request.Error ) ;
							}
						}
#endif

					}
					else
					if( ( t == 1 && m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && m_LoadPriorityType == LoadPriority.Remote ) )
					{
						// アセットバンドルからロードを試みる
						if( GetManifestNameAndAssetBundleName( path, out string manifestName, out string assetBundleName, out string assetName ) == true )
						{
							if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
							{
								if( m_ManifestHash.ContainsKey( manifestName ) == true )
								{
									yield return StartCoroutine( m_ManifestHash[ manifestName ].LoadAssetBundle_Coroutine( assetBundleName, rAssetBundleHolder, keep, null, request, false, this ) ) ;
									assetBundle = rAssetBundleHolder[ 0 ] ;

									if( assetBundle != null )
									{
										if( assetBundle.isStreamedSceneAssetBundle == true )
										{
											// SceneのAssetBundle
											yield return StartCoroutine( OpenSceneAsync_Private( sceneName, type, mode, request ) ) ;
											result = string.IsNullOrEmpty( request.Error ) ;
											if( result == true )
											{
												yield return StartCoroutine( WaitSceneAsync_Private( sceneName, type, ( _ ) => { targets = _ ; }, targetName, request ) ) ;
												result = string.IsNullOrEmpty( request.Error ) ;
											}
										}

										if( result == true )
										{
											// 成功の場合は自動破棄リストに追加する(Unload(false))
											AddAutoCleaningTarget( assetBundle ) ;
										}
										else
										{
											// 失敗(isStreamedSceneAssetBundle==trueでなければSceneのAssetBundleではない)
											assetBundle.Unload( true ) ;
										}
									}
								}
							}
						}
					}
				}
				if( result == true )
				{
					break ;
				}
			}

			if( result == false )
			{
				// 失敗
				if( string.IsNullOrEmpty( request.Error ) == true )
				{
					request.Error = "Could not load" ;
				}
				yield break ;
			}

			//------------------------------------------------

			onLoaded?.Invoke( targets ) ;
			request.IsDone = true ;
		}

		private IEnumerator OpenSceneAsync_Private( string sceneName, Type type, UnityEngine.SceneManagement.LoadSceneMode mode, Request request )
		{
			if( string.IsNullOrEmpty( sceneName ) == true )
			{
				request.Error = "Bad scene name" ;
				yield break ;
			}
			
			//----------------------------------------------------------

			if( type != null )
			{
				// 指定の型のコンポーネントが存在する場合はそれが完全に消滅するまで待つ
				while( true )
				{
					if( GameObject.FindObjectOfType( type ) == null )
					{
						break ;
					}
					yield return null ;
				}
			}

			//----------------------------------------------------------
			
			// リモート
//			if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//			{
//				// 非同期(低速)
//				yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( tName, tMode ) ;
//			}
//			else
//			{
				// 同期(高速)　※同期メソッドを使っても実質非同期
				UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName, mode ) ;
//			}
		}

		// シーンをロードまたは加算する(非同期版)
		private IEnumerator WaitSceneAsync_Private( string sceneName, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, Request request )
		{
			UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneName ) ;
			
			if( scene.IsValid() == false )
			{
				request.Error = "Scene is invalid" ;
				yield break ;
			}

			// シーンの展開が完了するのを待つ
			yield return new WaitWhile( () => scene.isLoaded == false ) ;

			if( type != null && onLoaded != null )
			{
				GetInstance_Private( scene, type, onLoaded, targetName, request ) ;
			}

			// 念のため保険
			request.Error = null ;
		}

		//---------------------------

		private void GetInstance_Private( UnityEngine.SceneManagement.Scene scene, Type type, Action<UnityEngine.Object[]> onLoaded, string targetName, Request request )
		{
			if( onLoaded != null || request != null )
			{
				// 指定の型のコンポーネントを探してインスタンスを取得する
				List<UnityEngine.Object> fullTargets = new List<UnityEngine.Object>() ;

				GameObject[] gos = scene.GetRootGameObjects() ;
				if( gos != null && gos.Length >  0 )
				{
					UnityEngine.Object[] components ;
					foreach( var go in gos )
					{
						components = go.GetComponentsInChildren( type, true ) ;
						if( components != null && components.Length >  0 )
						{
							foreach( var component in components )
							{
								fullTargets.Add( component ) ;
							}
						}
					}
				}

				if( fullTargets.Count >  0 )
				{
					UnityEngine.Object[] temporaryTargets = null ;

					// 該当のコンポーネントが見つかった
					if( string.IsNullOrEmpty( targetName ) == false )
					{
						// 名前によるフィルタ有り
						List<UnityEngine.Object> filteredTargets = new List<UnityEngine.Object>() ;
						foreach( var target in fullTargets )
						{
							if( target.name == targetName )
							{
								filteredTargets.Add( target ) ;
							}
						}

						if( filteredTargets.Count >  0 )
						{
							temporaryTargets = filteredTargets.ToArray() ;
						} 
					}
					else
					{
						// 名前によるフィルタ無し
						temporaryTargets = fullTargets.ToArray() ;
					}

					if( temporaryTargets != null && temporaryTargets.Length >  0 )
					{
						onLoaded?.Invoke( temporaryTargets ) ;
						request.Assets = temporaryTargets ;
					}
				}
			}
		}

		//---------------------------------------------------------------------------

		// AssetBundleManager :: AssetBundle
		
		/// <summary>
		/// アセットバンドルを取得する(同期版)
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <returns>アセットバンドルのインスタンス</returns>
		public static AssetBundle LoadAssetBundle( string tPath )
		{
			// 必ず自前で Unload を行わなければならない
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			return m_Instance.LoadAssetBundle_Private( tPath ) ;
		}

		// アセットバンドルを取得する(同期版)
		private AssetBundle LoadAssetBundle_Private( string tPath )
		{
			string tManifestName = "" ;
			string tAssetBundleName = "" ;
			string tAssetName = "" ;

			if( GetManifestNameAndAssetBundleName( tPath, out tManifestName, out tAssetBundleName, out tAssetName ) == false )
			{
				return null ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( tManifestName ) == true || string.IsNullOrEmpty( tAssetBundleName ) == true )
			{
				// マニフェスト名とアセットバンドル名は絶対に必要(この処理は実際は必要無いかもしれない)
				return null ;
			}
			
			if( m_ManifestHash.ContainsKey( tManifestName ) == false )
			{
				// マニフェスト名が存在しない
				return null ;
			}

			// マニフェストインフォを取得する
			ManifestInfo tManifestInfo = m_ManifestHash[ tManifestName ] ;

			// アセットバンドルを取得する
			return tManifestInfo.LoadAssetBundle( tAssetBundleName, false, this ) ;
		}

		//-----------------------------------

		/// <summary>
		/// アセットバンドルを取得する(非同期版)
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納するための要素数１以上の配列</param>
		/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>列挙子</returns>
		public static Request LoadAssetBundleAysnc( string tPath, AssetBundle[] rAssetBundle, bool tKeep = false, int[] rResultCode = null )
		{
			// 必ず自前で Unload を行わなければならない
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAssetBundleAsync_Private( tPath, rAssetBundle, tKeep, rResultCode, tRequest ) ) ;
			return tRequest ;
		}

		// アセットバンドルを取得する(非同期版)
		private IEnumerator LoadAssetBundleAsync_Private( string tPath, AssetBundle[] rAssetBundle, bool tKeep, int[] rResultCode, Request tRequest )
		{
			// リザルトコードを成功で一旦初期化しておく
			if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 0 ; }

			string tManifestName = "" ;
			string tAssetBundleName = "" ;
			string tAssetName = "" ;

			if( GetManifestNameAndAssetBundleName( tPath, out tManifestName, out tAssetBundleName, out tAssetName ) == false )
			{
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( tManifestName ) == true || string.IsNullOrEmpty( tAssetBundleName ) == true )
			{
				// マニフェスト名とアセットバンドル名は絶対に必要(この処理は実際は必要無いかもしれない)
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}

			if( m_ManifestHash.ContainsKey( tManifestName ) == false )
			{
				// マニフェスト名が存在しない
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}

			// マニフェストインフォを取得する
			ManifestInfo tManifestInfo = m_ManifestHash[ tManifestName ] ;

			// アセットバンドルを取得する
			AssetBundle[] tAssetBundleHolder = { null } ;
			yield return StartCoroutine( tManifestInfo.LoadAssetBundle_Coroutine( tAssetBundleName, tAssetBundleHolder, tKeep, rResultCode, tRequest, false, this ) ) ;

			if( tAssetBundleHolder[ 0 ] == null )
			{
				if( rResultCode != null && rResultCode.Length >  0 )
				{
					if( rResultCode[ 0 ] == 0 )
					{
						rResultCode[ 0 ] = 2 ;
					}
				}
				if( tRequest != null )
				{
					if( string.IsNullOrEmpty( tRequest.Error ) == true )
					{
						tRequest.Error = "Could not load" ;
						tRequest.ResultCode = 2 ;
					}
				}
				yield break ;
			}

			if( rAssetBundle != null && rAssetBundle.Length >  0 )
			{
				rAssetBundle[ 0 ] = tAssetBundleHolder[ 0 ] ;
			}
			if( tRequest != null )
			{
				tRequest.AssetBundle = tAssetBundleHolder[ 0 ] ;
				tRequest.IsDone = true ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// アセットバンドルのダウンロードを行う(非同期)
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static Request DownloadAssetBundleAsync( string tPath, bool tKeep = false, int[] rResultCode = null )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return null ;
			}

			Request tRequest = new Request() ;
			m_Instance.StartCoroutine( m_Instance.DownloadAssetBundleAsync_Private( tPath, tKeep, rResultCode, tRequest ) ) ;
			return tRequest ;
		}

		// アセットバンドルのダウンロードを行う
		private IEnumerator DownloadAssetBundleAsync_Private( string tPath, bool tKeep, int[] rResultCode, Request tRequest )
		{
			// リザルトコードを成功で一旦初期化しておく
			if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 0 ; }

			string tManifestName = "" ;
			string tAssetBundleName = "" ;
			string tAssetName = "" ;

			if( GetManifestNameAndAssetBundleName( tPath, out tManifestName, out tAssetBundleName, out tAssetName ) == false )
			{
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( tManifestName ) == true || string.IsNullOrEmpty( tAssetBundleName ) == true )
			{
				// マニフェスト名とアセットバンドル名は絶対に必要(この処理は実際は必要無いかもしれない)
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}
			
			if( m_ManifestHash.ContainsKey( tManifestName ) == false )
			{
				// マニフェスト名が存在しない
				if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
				if( tRequest != null ){ tRequest.Error = "Could not load" ; tRequest.ResultCode = 1 ; }
				yield break ;
			}

			// マニフェストインフォを取得する
			ManifestInfo tManifestInfo = m_ManifestHash[ tManifestName ] ;

			// アセットバンドルを取得する
			bool[] tResultHolder = { false } ;
			yield return StartCoroutine( tManifestInfo.DownloadAssetBundle_Coroutine( tAssetBundleName, tResultHolder, tKeep, rResultCode, tRequest, this ) ) ;

			if( tResultHolder[ 0 ] == false )
			{
				if( rResultCode != null && rResultCode.Length >  0 )
				{
					if( rResultCode[ 0 ] == 0 )
					{
						rResultCode[ 0 ] = 2 ;
					}
				}
				if( tRequest != null )
				{
					if( string.IsNullOrEmpty( tRequest.Error ) == true )
					{
						tRequest.Error = "Could not load" ;
						tRequest.ResultCode = 2 ;
					}
				}
				yield break ;
			}

			if( tRequest != null )
			{
				tRequest.IsDone = true ;
			}
		}
		
		//-----------------------------------

		/// <summary>
		/// アセットバンドルをストレージキャッシュから削除する
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>アセットバンドルのダウンロードリクエストクラスのインスタンス</returns>
		public static bool RemoveAssetBundle( string path )
		{
			return m_Instance == null ? false : m_Instance.RemoveAssetBundle_Private( path ) ;
		}

		// アセットバンドルをキャッシュから削除する
		private bool RemoveAssetBundle_Private( string path )
		{
			string manifestName ;
			string assetBundleName ;
			string assetName ;

			if( GetManifestNameAndAssetBundleName( path, out manifestName, out assetBundleName, out assetName ) == false )
			{
				return false ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
			{
				if( m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					return m_ManifestHash[ manifestName ].RemoveAssetBundle( assetBundleName, this ) ;
				}
			}

			return false ;
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// アセットバンドルが管理対象に含まれているか確認する
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <returns></returns>
		public static bool Contains( string path )
		{
			return m_Instance == null ? false : m_Instance.Contains_Private( path ) ;
		}
		
		// アセットバンドルが管理対象に含まれているか確認する
		private bool Contains_Private( string path )
		{
			string manifestName ;
			string assetBundleName ;
			string assetName ;

			if( GetManifestNameAndAssetBundleName( path, out manifestName, out assetBundleName, out assetName ) == false )
			{
				return false ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
			{
				if( m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					return m_ManifestHash[ manifestName ].Contains( assetBundleName, this ) ;
				}
			}

			return false ;
		}
		
		/// <summary>
		/// アセットバンドルの存在を確認する
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <returns></returns>
		public static bool Exists( string path )
		{
			return m_Instance == null ? false : m_Instance.Exists_Private( path ) ;
		}
		
		// アセットバンドルの存在を確認する
		private bool Exists_Private( string path )
		{
			if( m_UseLocalAsset == true )
			{
				return true ;	// いわゆるデバッグモードなので常に成功扱いにする
			}

			string manifestName ;
			string assetBundleName ;
			string assetName ;

			if( GetManifestNameAndAssetBundleName( path, out manifestName, out assetBundleName, out assetName ) == false )
			{
				return false ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
			{
				if( m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					return m_ManifestHash[ manifestName ].Exists( assetBundleName, this ) ;
				}
			}

			return false ;
		}

		/// <summary>
		/// アセットバンドルのサイズを取得する
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <returns></returns>
		public static int GetSize( string path )
		{
			return m_Instance == null ? -1 : m_Instance.GetSize_Private( path ) ;

		}
		
		// アセットバンドルのサイズを取得する
		private int GetSize_Private( string path )
		{
			string manifestName ;
			string assetBundleName ;
			string assetName ;

			if( GetManifestNameAndAssetBundleName( path, out manifestName, out assetBundleName, out assetName ) == false )
			{
				return -1 ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( manifestName ) == false && string.IsNullOrEmpty( assetBundleName ) == false )
			{
				// マニフェスト名は絶対に必要

				if( m_ManifestHash.ContainsKey( manifestName ) == true )
				{
					return m_ManifestHash[ manifestName ].GetSize( assetBundleName, this ) ;
				}
			}

			return -1 ;
		}

		//-------------------------------------------------------------------

		// 破棄対象のアセットバンドル
		Dictionary<AssetBundle,int>	m_AutoCleaningAssetBundle = new Dictionary<AssetBundle,int>() ;

		// 破棄対象のアセットバンドルを追加する
		private void AddAutoCleaningTarget( AssetBundle assetBundle )
		{
			if( assetBundle == null )
			{
				return ;
			}

			if( m_AutoCleaningAssetBundle.ContainsKey( assetBundle ) == true )
			{
				// 既に破棄対象になっている
				return ;
			}

			m_AutoCleaningAssetBundle.Add( assetBundle, Time.frameCount ) ;
		}

		// 破棄対象のアセットバンドルを除去する
		private void RemoveAutoCleaningTarget( AssetBundle assetBundle )
		{
			if( assetBundle == null )
			{
				return ;
			}

			if( m_AutoCleaningAssetBundle.ContainsKey( assetBundle ) == false )
			{
				// 破棄対象には含まれていない
				return ;
			}

			m_AutoCleaningAssetBundle.Remove( assetBundle ) ;
		}

		// 自動破棄対象のアセットバンドルを破棄する
		private void AutoCleaning()
		{
			if( m_AutoCleaningAssetBundle.Count == 0 )
			{
				return ;
			}

			int frameCount = Time.frameCount ;

			int i, l = m_AutoCleaningAssetBundle.Count ;

			AssetBundle[] assetBundles = new AssetBundle[ l ] ;
			m_AutoCleaningAssetBundle.Keys.CopyTo( assetBundles, 0 ) ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_AutoCleaningAssetBundle[ assetBundles[ i ] ] <  frameCount )
				{
					// 破棄実行対象
					Debug.LogWarning( "------->アセットバンドルの自動破棄実行:" + assetBundles[ i ].name ) ;
					assetBundles[ i ].Unload( false ) ;
					m_AutoCleaningAssetBundle.Remove( assetBundles[ i ] ) ;
				}
			}
		}
	}
}
