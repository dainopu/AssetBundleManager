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
		/// <summary>
		/// マニフェスト情報クラス
		/// </summary>
		[System.Serializable]
		public class ManifestInfo
		{
			/// <summary>
			/// リモート側のマニフェストのパス
			/// </summary>
			public string	FilePath = "" ;

			/// <summary>
			/// レガシータイプのアセットバンドルであるかどうかを示す
			/// </summary>
			public bool		LegacyType = false ;

			/// <summary>
			/// マニフェストごとのキャッシュサイズ(0で無制限)
			/// </summary>
			public long		CacheSize = 0L ;	// 0 は無制限

			/// <summary>
			/// マニフェストのロード進行度
			/// </summary>
			public float	Progress { get ; private set ; } = 0 ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error { get ; private set ; } = "" ;

			/// <summary>
			/// 非同期版のロードを行う際に通信以外処理を全て同期で行う(時間は短縮させるが別のコルーチンの呼び出し頻度が下がる)
			/// </summary>
			public bool		FastLoadEnabled = true ;

			/// <summary>
			/// アセットバンドルを扱う準備が完了しているかどうかを示す
			/// </summary>
			public bool		Completed { get ; private set ; } = false ;	// 本来は　private にして、アクセサで readonly にすべきだが、Editor を作成を省略するため、あえて public にする。


			// 展開中のマニフェストのインスタンス
			private AssetBundleManifest	m_Manifest = null ;

			/// <summary>
			/// アセットバンドルキャッシュ
			/// </summary>
			public class AssetBundleCacheElement
			{
				public	AssetBundle	AssetBundle ;
				public	float		LastAccessTime ;

				public AssetBundleCacheElement( AssetBundle assetBundle, float lastAccessTime )
				{
					AssetBundle		= assetBundle ;
					LastAccessTime	= lastAccessTime ;
				}
			}

			/// <summary>
			/// アセットバンドルのキャッシュ
			/// </summary>
			private readonly Dictionary<string,AssetBundleCacheElement> m_AssetBundleCache = new Dictionary<string, AssetBundleCacheElement>() ;

			/// <summary>
			/// アセットバンドルキャッシュに格納可能な最大数
			/// </summary>
			public int	AssetBundleCacheLimit = 128 ;

			/// <summary>
			/// キャッシュにアセットバンドルを追加する
			/// </summary>
			/// <param name="tName"></param>
			/// <param name="tAssetBundle"></param>
			public void AddAssetBundleCache( string assetBundleName, AssetBundle assetBundle, AssetBundleManager instance )
			{
				// キャッシュに追加されるアセットバンドルは自動破棄対象にはしない
				instance.RemoveAutoCleaningTarget( assetBundle ) ;

				if( m_AssetBundleCache.ContainsKey( assetBundleName ) == true )
				{
					// 既に登録済みなので最終アクセス時間を更新して戻る
					m_AssetBundleCache[ assetBundleName ].LastAccessTime = Time.realtimeSinceStartup ;
					return ;
				}

				if( m_AssetBundleCache.Count >= AssetBundleCacheLimit )
				{
					// 最もアクセス時間が古いものをキャッシュから削除する
					var removeTarget = m_AssetBundleCache.OrderBy( _ => _.Value.LastAccessTime ).First() ;

					removeTarget.Value.AssetBundle.Unload( false ) ;
					m_AssetBundleCache.Remove( removeTarget.Key ) ;
				}

				// キャッシュを追加する
				m_AssetBundleCache.Add( assetBundleName, new AssetBundleCacheElement( assetBundle, Time.realtimeSinceStartup ) ) ;
			}

			// キャッシュからアセットバンドルを削除する
			public void RemoveAssetBundleCache( string assetBundleName )
			{
				if( m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					return ;	// 元々キャッシュには存在しない
				}

				m_AssetBundleCache[ assetBundleName ].AssetBundle.Unload( false ) ;
				m_AssetBundleCache.Remove( assetBundleName ) ;
			}

			/// <summary>
			/// アセットバンドルキャッシュをクリアする
			/// </summary>
			public void ClearAssetBundleCache()
			{
#if UNITY_EDITOR
				Debug.Log( "[AssetBundleManager] キャッシュからクリア対象となる展開済みアセットバンドル数 = " + m_AssetBundleCache.Count + " / " + ManifestName ) ;
#endif
				if( m_AssetBundleCache.Count == 0 )
				{
					return ;
				}

				foreach( var removeTarget in m_AssetBundleCache )
				{
					removeTarget.Value.AssetBundle.Unload( false ) ;
				}

				m_AssetBundleCache.Clear() ;
			}

			//--------------------------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public ManifestInfo()
			{
				m_AssetBundleInfo = new List<AssetBundleInfo>() ;
				m_AssetBundleHash = new Dictionary<string, AssetBundleInfo>() ;	// Dictionary は、Inspector 上でインスタンスを生成しても、自動では生成してくれないため、明示的に生成する必要がある。
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="filePath">リモート側のマニフェストのパス</param>
			/// <param name="cacheSize">マニフェストごとのキャッシュサイズ(0で無制限)</param>
			public ManifestInfo( string filePath, long cacheSize )
			{
				m_AssetBundleInfo = new List<AssetBundleInfo>() ;
				m_AssetBundleHash = new Dictionary<string, AssetBundleInfo>() ;	// Dictionary は、Inspector 上でインスタンスを生成しても、自動では生成してくれないため、明示的に生成する必要がある。

				FilePath	= filePath ;
				CacheSize	= cacheSize ;
			}

			//-----------------------------------------------------------------------------------

			// AssetBundleInfo
			
			/// <summary>
			/// アセットバンドル情報クラス
			/// </summary>
			[System.Serializable]
			public class AssetBundleInfo
			{
				/// <summary>
				/// マニフェスト内での相対パス
				/// </summary>
				public string	Path = String.Empty ;

				/// <summary>
				/// ハッシュ値
				/// </summary>
				public string	Hash = String.Empty ;

				/// <summary>
				/// アセットバンドルファイルのサイズ
				/// </summary>
				public int		Size = 0 ;			// サイズ(処理の高速化のためにここに保持しておく)※キャッシュオーバーなどの際の処理に使用する

				/// <summary>
				/// ＣＲＣ値(０で使用しない)
				/// </summary>
				public uint		Crc = 0 ;

				/// <summary>
				/// 最終更新日時
				/// </summary>
				public long		LastUpdateTime = 0 ;

				/// <summary>
				/// キャッシュオーバーする際に破棄可能にするかどうかを示す
				/// </summary>
				public bool		Keep = false ;

				/// <summary>
				/// 更新が必要がどうかを示す
				/// </summary>
				public bool		UpdateRequired = true ;		// 更新が必要かどうか

				/// <summary>
				/// 非同期アクセス時の排他ロックフラグ
				/// </summary>
				public bool		Busy = false ;

				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="tPath">マニフェスト内での相対パス</param>
				/// <param name="tHash">ハッシュ値</param>
				/// <param name="tTime">最終更新日時</param>
				public AssetBundleInfo( string path, string hash, int size, uint crc, long lastUpdateTime )
				{
					Path			= path ;
					Hash			= hash ;
					Size			= size ;
					Crc				= crc ;
					LastUpdateTime	= lastUpdateTime ;
				}


				// 対象がプレハブの場合にシェーダーを付け直す(Unity Editor で Platform が Android iOS の場合の固有バグ対策コード)
				private UnityEngine.Object ReplaceShader( UnityEngine.Object asset, Type type )
				{
#if( UNITY_EDITOR && UNITY_ANDROID ) || ( UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE ) )
					if( asset is GameObject )
					{
						GameObject go = asset as GameObject ;

						Renderer[] renderers = go.GetComponentsInChildren<Renderer>( true ) ;
						if( renderers != null && renderers.Length >  0 )
						{
							foreach( var renderer in renderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

						MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>( true ) ;
						if( meshRenderers != null && meshRenderers.Length >  0 )
						{
							foreach( var renderer in meshRenderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

						SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>( true ) ;
						if( skinnedMeshRenderers != null && skinnedMeshRenderers.Length >  0 )
						{
							foreach( var renderer in skinnedMeshRenderers )
							{
								if( renderer.sharedMaterials != null && renderer.sharedMaterials.Length >  0 )
								{
									foreach( var material in renderer.sharedMaterials )
									{
										if( material != null )
										{
											string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											material.shader = Shader.Find( shaderName ) ;
										}
									}
								}
							}
						}

					}
					else
					if( asset is Material )
					{
						Material material = asset as Material ;
						string shaderName = material.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
						material.shader = Shader.Find( shaderName ) ;
					}
#endif
					return asset ;
				}

				//---------------------------------------------------------

				// AssetBundleInfo :: Asset
				
				/// <summary>
				/// アセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tName">アセット名</param>
				/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadAsset( AssetBundle assetBundle, string assetBundleName, string assetName, Type type, AssetBundleManager instance )
				{
					string path ;

					// ひとまず assetName は空想定でやってみる
					if( string.IsNullOrEmpty( assetName ) == true )
					{
						// アセットバンドル＝単一アセットのケース
						path = ( instance.m_LocalAssetBundleRootPath + assetBundleName ).ToLower() ;
					}
					else
					{
						// アセットバンドル＝複合アセットのケース
						path = ( instance.m_LocalAssetBundleRootPath + assetBundleName + "/" + assetName ).ToLower() ;
					}

					// まずはそのままロードしてみる
					UnityEngine.Object asset = assetBundle.LoadAsset( path, type ) ;
					if( asset == null )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								asset = assetBundle.LoadAsset( path + extension, type ) ;
								if( asset != null )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}
					
					if( asset != null )
					{
						asset = ReplaceShader( asset, type ) ;
					}

					return asset ;
				}
#if false
				/// <summary>
				/// アセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tName">アセット名</param>
				/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, UnityEngine.Object[] rAsset, Request tRequest, AssetBundleManager tInstance )
				{
					if( rAsset == null || rAsset.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						yield break ;
					}

					if( string.IsNullOrEmpty( tAssetName ) == true )
					{
						// mainAsset は Unity5 より非推奨になりました(Unity5 以降の BuildPipeline.BuildAssetBundles から設定項目が消失している)
//						if( tAB.mainAsset != null )
//						{
//							if( rAsset != null && rAsset.Length >  0 )
//							{
//								rAsset[ 0 ] =  tAB.mainAsset as T ;
//							}
//							yield break ;
//						}

						int p = tAssetBundleName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							tAssetName = tAssetBundleName.Substring( p + 1, tAssetBundleName.Length - ( p + 1 ) ) ;
						}
						else
						{
							tAssetName = tAssetBundleName ;
						}
					}

					tAssetName = tAssetName.ToLower() ;

					if( m_Index.ContainsKey( tAssetName ) == false )
					{
						yield break ;
					}

					AssetBundleRequest tR = null ;
					UnityEngine.Object tAsset = null ;

					int n, c = m_Index[ tAssetName ].Count ;

					yield return new WaitForSeconds( 1 ) ;

					for( n  = 0 ; n <  c ; n ++ )
					{
						// tR に対してアクセスすると Assertion failed: Assertion failed on expression: 'Thread::CurrentThreadIsMainThread()' が発生するので現状使用できない
						tR = tAssetBundle.LoadAssetAsync( m_Index[ tAssetName ][ n ], tType ) ;

						if( tRequest == null )
						{
							yield return tR ;
						}
						else
						{
							while( true )
							{
								tRequest.progress =	tR.progress ;
								if( tR.isDone == true )
								{
									break ;
								}
								yield return null ;
							}
						}

						if( tR.isDone == true && tR.asset != null )
						{
							tAsset = tR.asset ;
							tAsset = ReplaceShader( tAsset, tType ) ;
							rAsset[ 0 ] = tAsset ;
							break ;
						}
					}
				}
#endif
				//-----------------------
				// AssetBundleInfo :: AllAssets

				/// <summary>
				/// 全てのアセットを取得する
				/// </summary>
				/// <param name="assetBundle"></param>
				/// <param name="type"></param>
				/// <param name="instance"></param>
				/// <param name="resourcePath"></param>
				/// <returns></returns>
				internal protected UnityEngine.Object[] LoadAllAssets( AssetBundle assetBundle, Type type, AssetBundleManager instance, string resourcePath )
				{
					UnityEngine.Object[] assets = assetBundle.LoadAllAssets() ;
					if( type != null && assets != null && assets.Length >  0 )
					{
						// タイプ指定があるならタイプで絞る
						List<UnityEngine.Object> temporaryAssets = new List<UnityEngine.Object>() ;
						foreach( var asset in assets )
						{
							if( asset.GetType() == type )
							{
								temporaryAssets.Add( asset ) ;
							}
						}

						if( temporaryAssets.Count == 0 )
						{
							return null ;
						}

						assets = temporaryAssets.ToArray() ;
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = resourcePath + "/" + assets[ i ].name + ":" + assets[ i ].GetType().ToString() ;
							if( instance.resourceCache != null && instance.resourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								assets[ i ] = instance.resourceCache[ resourceCachePath ] ;
							}
							else
							{
								assets[ i ] = ReplaceShader( assets[ i ], assets[ i ].GetType() ) ;
							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
				
				//-----------------------

				// AssetBundleInfo :: SubAsset

				/// <summary>
				/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tSubName">サブアセット名</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object LoadSubAsset( AssetBundle assetBundle, string assetBundleName, string assetName, string subAssetName, Type type, AssetBundleManager instance, string resourcePath )
				{
					UnityEngine.Object[] subAssets = LoadAllSubAssets( assetBundle, assetBundleName, assetName, type, instance, resourcePath ) ;
					if( subAssets == null || subAssets.Length == 0 )
					{
						return null ;
					}

					UnityEngine.Object asset = null ;

					foreach( var subAsset in subAssets )
					{
						if( subAsset.name == subAssetName && subAsset.GetType() == type )
						{
							asset = subAsset ;
							break ;
						}
					}

					if( asset != null )
					{
						asset = ReplaceShader( asset, type ) ;
					}

					return asset ;
				}
#if false
				/// <summary>
				/// アセットに含まれるサブアセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tSubAssetName">サブアセット名</param>
				/// <param name="rSubAsset">サブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadSubAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, UnityEngine.Object[] rSubAsset, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null || rSubAsset == null || rSubAsset.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( string.IsNullOrEmpty( tSubAssetName ) == true )
					{
						yield break ;
					}

					List<UnityEngine.Object> tAllSubAssets = null ;

					List<UnityEngine.Object>[] rAllSubAssets = { null } ;
					yield return tInstance.StartCoroutine( LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssets, tRequest, tInstance, tResourcePath ) ) ;
					tAllSubAssets = rAllSubAssets[ 0 ] ;

					if( tAllSubAssets == null || tAllSubAssets.Count == 0 )
					{
						yield break ;
					}

					UnityEngine.Object tAsset = null ;

					int i, l = tAllSubAssets.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( tAllSubAssets[ i ].name == tSubAssetName && tAllSubAssets[ i ].GetType() == tType )
						{
							tAsset = tAllSubAssets[ i ] ;
							break ;
						}
					}

					if( tAsset != null )
					{
						tAsset = ReplaceShader( tAsset, tType ) ;
					}

					rSubAsset[ 0 ] = tAsset ;
				}
#endif
				//-----------------------

				// AssetBundleInfo :: AllSubAssets

				/// <summary>
				/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
				internal protected UnityEngine.Object[] LoadAllSubAssets( AssetBundle assetBundle, string assetBundleName, string assetName, Type type, AssetBundleManager instance, string resourcePath )
				{
					string path ;

					if( string.IsNullOrEmpty( assetName ) == true )
					{
						// アセットバンドル＝単一アセットのケース
						path = ( instance.m_LocalAssetBundleRootPath + assetBundleName ).ToLower() ;
					}
					else
					{
						// アセットバンドル＝複合アセットのケース
						path = ( instance.m_LocalAssetBundleRootPath + assetBundleName + "/" + assetName ).ToLower() ;
					}

					UnityEngine.Object[] assets = assetBundle.LoadAssetWithSubAssets( path, type ) ;	// 注意：該当が無くても配列数０のインスタンスが返る
					if( assets == null || assets.Length == 0 )
					{
						int i0 = path.LastIndexOf( '/' ) ;
						int i1 = path.LastIndexOf( '.' ) ;
						if( i1 <= i0 )
						{
							// 拡張子なし
							// だめなら拡張子を加えてロードしてみる
							List<string> extensions ;
							if( instance.m_TypeToExtension.ContainsKey( type ) == true )
							{
								// 一般タイプ
								extensions = instance.m_TypeToExtension[ type ] ;
							}
							else
							{
								// 不明タイプ
								extensions = instance.m_UnknownTypeToExtension ;
							}

							foreach( string extension in extensions )
							{
								assets = assetBundle.LoadAssetWithSubAssets( path + extension, type ) ;
								if( assets != null && assets.Length >  0 )
								{
									// 成功したら終了
									break ;
								}
							}
						}
					}

					if( assets != null && assets.Length >  0 )
					{
						string resourceCachePath ;

						// 最終的なものを生成する
						for( int i  = 0 ; i <  assets.Length ; i ++ )
						{
							resourceCachePath = resourcePath + "/" + assets[ i ].name + ":" + type.ToString() ;
							if( instance.resourceCache != null && instance.resourceCache.ContainsKey( resourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								assets[ i ] = instance.resourceCache[ resourceCachePath ] ;
							}
							else
							{
								assets[ i ] = ReplaceShader( assets[ i ], type ) ;
							}
						}
					}
					else
					{
						assets = null ;
					}

					return assets ;
				}
#if false               
				/// <summary>
				/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
				/// </summary>
				/// <typeparam name="T">任意のコンポーネント型</typeparam>
				/// <param name="tAssetBundle">アセットバンドルのインスタンス</param>
				/// <param name="tAssetBundleName">アセットバンドル名</param>
				/// <param name="tAssetName">アセット名</param>
				/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
				/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
				/// <param name="tResourcePath">アセットのリソースパス</param>
				/// <returns>列挙子</returns>
				internal protected IEnumerator LoadAllSubAssets_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, List<UnityEngine.Object>[] rAllSubAssets, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null || rAllSubAssets == null || rAllSubAssets.Length == 0 )
					{
						yield break ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						yield break ;
					}

					if( string.IsNullOrEmpty( tAssetName ) == true )
					{
						// 名前が指定されていない場合はメインアセットとみなす
						int p = tAssetBundleName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							tAssetName = tAssetBundleName.Substring( p + 1, tAssetBundleName.Length - ( p + 1 ) ) ;
						}
						else
						{
							tAssetName = tAssetBundleName ;
						}
					}

					tAssetName = tAssetName.ToLower() ;

					List<UnityEngine.Object> tAllSubAssets = null ;
					string tResourceCachePath ;
					UnityEngine.Object tAsset ;

					if( m_Index.ContainsKey( tAssetName ) == true )
					{
						// 単体ファイルとして合致するものが有る
						AssetBundleRequest tR = null ;
						UnityEngine.Object[] t = null ;

						int i, l ;
						int n, c = m_Index[ tAssetName ].Count ;

						for( n  = 0 ; n <  c ; n ++ )
						{
							tR = tAssetBundle.LoadAssetWithSubAssetsAsync( m_Index[ tAssetName ][ n ], tType ) ;
							yield return tR ;

							if( tRequest != null )
							{
								tRequest.progress = ( float )n / ( float )c ;
							}

							if( tR.isDone == true )
							{
								t = tR.allAssets as UnityEngine.Object[] ;
								if( t != null && t.Length >  0 )
								{
									tAllSubAssets = new List<UnityEngine.Object>() ;
	
									l = t.Length ;
									for( i  = 0 ; i <  l ; i ++ )
									{
										tResourceCachePath = tResourcePath + "/" + t[ i ].name + ":" + tType.ToString() ;
										if( tInstance.resourceCache != null && tInstance.resourceCache.ContainsKey( tResourceCachePath ) == true )
										{
											// キャッシュにあればそれを返す
											tAllSubAssets.Add( tInstance.resourceCache[ tResourceCachePath ] ) ;
										}
										else
										{
											tAsset = t[ i ] ;
											tAsset = ReplaceShader( tAsset, tType ) ;
											tAllSubAssets.Add( tAsset ) ;
										}
									}
									
									if( tAllSubAssets.Count >  0 )
									{
										rAllSubAssets[ 0 ] = tAllSubAssets ;
									}

									yield break ;
								}
							}
						}
					}
					else
					{
						// 単体ファイルとして合致するものが無い

						// フォルダ指定の可能性があるので上位フォルダが合致するものをまとめて取得する
						int i, l ;
						int n, c ;

						l = m_Index.Count ;
						string[] tKey = new string[ l ] ;
						m_Index.Keys.CopyTo( tKey, 0 ) ;

						List<string> tTarget = new List<string>() ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							if( tKey[ i ].IndexOf( tAssetName ) == 0 )
							{
								// 発見した
								tTarget.Add( tKey[ i ] ) ;
							}
						}

						if( tTarget.Count == 0 )
						{
							// 合致するものが存在しない
							yield break ;
						}

						tAllSubAssets = new List<UnityEngine.Object>() ;

						string tName ;
						AssetBundleRequest tR ;

						l = tTarget.Count ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tName = tTarget[ i ].Replace( tAssetName, "" ) ;

							tResourceCachePath = tResourcePath + "/" + tAssetName + "/" + tName + ":" + tType.ToString() ;
							if( tInstance.resourceCache != null && tInstance.resourceCache.ContainsKey( tResourceCachePath ) == true )
							{
								// キャッシュにあればそれを返す
								tAllSubAssets.Add( tInstance.resourceCache[ tResourceCachePath ] ) ;
							}
							else
							{
								c = m_Index[ tTarget[ i ] ].Count ;
								for( n  = 0 ; n <  c ; n ++ )
								{
									tR = tAssetBundle.LoadAssetAsync( m_Index[ tTarget[ i ] ][ n ], tType ) ;
									yield return tR ;

									if( tRequest != null )
									{
										tRequest.progress = ( float )n / ( float )c ;
									}

									if( tR.isDone == true )
									{
										tAsset = tR.asset ;
										tAsset = ReplaceShader( tAsset, tType ) ;
										tAllSubAssets.Add( tAsset ) ;
										break ;
									}
								}
							}
						}

						if( tAllSubAssets.Count >  0 )
						{
							rAllSubAssets[ 0 ] = tAllSubAssets ;
						}

						yield break ;
					}
				}
#endif   
			}

			//-----------------------------------------------------------------------------------

			/// <summary>
			/// 保持している情報をクリアする
			/// </summary>
			public void Clear()
			{
				m_AssetBundleInfo.Clear() ;
				m_AssetBundleHash.Clear() ;
			}

			/// <summary>
			/// 全てのアセットバンドルを更新が必要な対象とする
			/// </summary>
			public void SetAllUpdateRequired()
			{
				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					assetBundleInfo.UpdateRequired = true ;	// 更新が必要扱いにする
				}
			}

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報
			/// </summary>
			[SerializeField]
			private readonly List<AssetBundleInfo> m_AssetBundleInfo ;	// インスペクターで確認可能とするため実体はリストで保持する

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報の高速アクセス用のハッシュリスト
			/// </summary>
			private readonly Dictionary<string,AssetBundleInfo> m_AssetBundleHash ;	// ショートカットアクセスのためディクショナリも用意する

			// マニフェストが展開中かを示す
			private bool m_Busy = false ;

			/// <summary>
			/// マニフェストの名前
			/// </summary>
			public string ManifestName
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return string.Empty ;
					}

					// 名前を取り出す
					string manifestName = FilePath ;

					int i ;

					i = manifestName.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						manifestName = FilePath.Substring( i + 1, manifestName.Length - ( i + 1 ) ) ;
					}

					i = manifestName.IndexOf( '.' ) ;
					if( i >= 0 )
					{
						manifestName = manifestName.Substring( 0, i ) ;
					}

					return manifestName ;
				}
			}

			/// <summary>
			/// マニフェストのファイル名
			/// </summary>
			public string FileName
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return string.Empty ;
					}

					// 名前を取り出す
					int i ;

					i = FilePath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						return FilePath.Substring( i + 1, FilePath.Length - ( i + 1 ) ) ;
					}
					else
					{
						return FilePath ;
					}
				}
			}

			/// <summary>
			/// マニフェストのパス
			/// </summary>
			public string Path
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return string.Empty ;
					}

					// パスを取り出す
					string path = FilePath ;

					int i  ;

					//--------------------------------

					i = path.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						path = path.Substring( 0, i ) ;
					}

					//--------------------------------

					return path ;
				}
			}

			/// <summary>
			/// ストリーミングアセット内想定のマニフェストのパス
			/// </summary>
			public string StreamingAssetsPath
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return string.Empty ;
					}

					string path = FilePath ;

					int i ;

					//--------------------------------

					i = path.IndexOf( "://" ) ;
					if( i >= 0 )
					{
//						string scheme  = path.Substring( 0, i ).ToLower() ;
//						if( scheme  == "http" || scheme == "https" )
//						{
							i += 3 ;
							path = path.Substring( i, path.Length - i ) ;

							// ドメイン名も削除する

							i = path.IndexOf( '/' ) ;
							if( i >= 0 )
							{
								i ++ ;
								path = path.Substring( i, path.Length - i ) ;
							}
//						}
					}

					//--------------------------------

					i = path.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						path = path.Substring( 0, i ) ;
					}

					//--------------------------------

					return path ;
				}
			}

			/// <summary>
			/// アセットバンドルのサイズとＣＲＣ
			/// </summary>
			public class AssetBundleCrcDescriptor
			{
				public int		Size ;
				public uint		Crc ;

				public AssetBundleCrcDescriptor( int size, uint crc )
				{
					Size	= size ;
					Crc		= crc ;
				}
			}


			/// <summary>
			/// マニフェストを展開する(非同期)
			/// </summary>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tStatus">結果を格納する要素数１以上の配列</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAsync( Request result, AssetBundleManager instance )
			{
				while( m_Busy )
				{
					yield return null ;	// 同じマニフェストに対し同時に処理を行おうとした場合は排他処理を行う
				}

				m_Busy = true ;

				//------------------------------------
	
				Progress = 0 ;
				Error = string.Empty ;

				//------------------------------------
	
				int i, l ;

				byte[] data = null ;
				string text ;

				//------------------------------------

				for( int t  = 0 ; t <  2 ; t ++ )
				{
					if( data == null )
					{
						if( ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							// 最初に StreamingAssets からロードを試みる
							if( instance.m_UseStreamingAssets == true )
							{
								// ストリーミングアセットから読み出してみる
								yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssets( StreamingAssetsPath + "/" + FileName, ( _ ) => { data = _ ; } ) ) ;
							}
						}
						else
						if( ( t == 1 &&instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( instance.m_UseDownload == true )
							{
								// DownloadHandler を自作しないならスタティックメソッドを使うこと(.downloadHandler が null になってしまうため)
								UnityWebRequest www = UnityWebRequest.Get( FilePath + "?time=" + GetClientTime() ) ;
	//							www.downloadHandler = new DownloadHandlerBuffer() ;
								www.SendWebRequest() ;

								while( true )
								{
									Progress = www.downloadProgress ;

									if( www.isHttpError == true || www.isNetworkError == true || string.IsNullOrEmpty( www.error ) == false )
									{
										Error = www.error ;
										break ;
									}

									if( www.isDone == true )
									{
										break ;
									}
	
									yield return null ;
								}

								if( string.IsNullOrEmpty( www.error ) == true )
								{
									// 成功
									data = www.downloadHandler.data ;
								}

								www.Dispose() ;
							}
						}
					}
					if( data != null )
					{
						break ;
					}
				}

				if( data == null )
				{
					// データが取得出来ない
					Error = "Could not load data" ;
					result.Error = Error ;
					m_Busy = false ;
					yield break ;
				}

				//------------------------------------

				string[] assetBundleName ;
				string[] assetBundleHash ;
				
				if( LegacyType == false )
				{
					// ノーマル版

					// バイナリからアセットバンドルを生成する
					AssetBundle assetBundle = AssetBundle.LoadFromMemory( data ) ;
					if( assetBundle == null )
					{
						// アセットバンドルが生成出来ない
						Error = "Could not create AssetBundle" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					m_Manifest = assetBundle.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" ) ;
					if( m_Manifest == null )
					{
						// マニフェストが取得出来ない
						assetBundle.Unload( true ) ;

						Error = "Could not get Manifest" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					assetBundleName = m_Manifest.GetAllAssetBundles() ;
					if( assetBundleName == null || assetBundleName.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						assetBundle.Unload( true ) ;

						Error = "No AssetBundles" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}
					
					// ハッシュを取得
					l = assetBundleName.Length ;
					assetBundleHash = new string[ l ] ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						assetBundleHash[ i ] = m_Manifest.GetAssetBundleHash( assetBundleName[ i ] ).ToString() ;
					}

					assetBundle.Unload( false ) ;
				}
				else
				{
					// レガシー版(アセットは重複する可能性がある)

					text = Encoding.UTF8.GetString( data ) ;

					if( string.IsNullOrEmpty( text ) == true )
					{
						Error = "Could not get Legacy list file. 1" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}
					
					string[] line = text.Split( '\n' ) ;
					if( line == null || line.Length == 0 )
					{
						Error = "Could not get Legacy list file. 2 " ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					List<string> row = new List<string>() ;

					// 空行がある可能性があるので除外する
					foreach( var code in line )
					{
						if( string.IsNullOrEmpty( code ) == false )
						{
							row.Add( code ) ;
						}
					}
					
					l = row.Count ;
					if( l == 0 )
					{
						Error = "Could not get Legacy list file. 2 " ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					assetBundleName = new string[ l ] ;
					assetBundleHash = new string[ l ] ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						string[] column = row[ i ].Split( ',' ) ;
						if( column == null || column.Length != 2 )
						{
							break ;
						}

						assetBundleName[ i ] = column[ 0 ] ;
						assetBundleHash[ i ] = column[ 1 ] ;
					}

					if( i <  l )
					{
						Error = "Could not get Legacy list file. 3" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}
				}

				//-------------

				// ＣＲＣファイルのロードを試みる
				text = "" ;

				for( int t  = 0 ; t <  2 ; t ++ )
				{
					if( string.IsNullOrEmpty( text ) == true )
					{
						if( ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( instance.m_UseStreamingAssets == true )
							{
								// ストリーミングアセットから読み出す
								yield return instance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssets( StreamingAssetsPath + "/" + FileName + ".crc", ( _ ) => { text = _ ; } ) ) ;
							}
						}
						else
						if( ( t == 1 && instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( instance.m_UseDownload == true )
							{
								// DownloadHandler を自作しないならスタティックメソッドを使うこと(.downloadHandler が null になってしまうため)
								UnityWebRequest	www = UnityWebRequest.Get( FilePath + ".crc" + "?time=" + GetClientTime() ) ;
	//							www.downloadHandler = new DownloadHandlerBuffer() ;
								www.SendWebRequest() ;

								while( true )
								{
									Progress = www.downloadProgress ;

									if( www.isHttpError == true || www.isNetworkError == true || string.IsNullOrEmpty( www.error ) == false )
									{
										Error = www.error ;
										break ;
									}

									if( www.isDone == true )
									{
										break ;
									}
	
									yield return null ;
								}

								if( string.IsNullOrEmpty( www.error ) == true )
								{
									// 成功
									text = UTF8Encoding.UTF8.GetString( www.downloadHandler.data ) ;
								}

								www.Dispose() ;
							}
						}
					}
					if( string.IsNullOrEmpty( text ) == false )
					{
						break ;
					}
				}

				Dictionary<string,AssetBundleCrcDescriptor> crcHash = null ;
				int size ;
				uint crc ;
				if( string.IsNullOrEmpty( text ) == false )
				{
					crcHash = new Dictionary<string,AssetBundleCrcDescriptor>() ;

					// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する
					string[] line = text.Split( '\n' ) ;
					l = line.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( line[ i ] ) == false )
						{
							string[] keyAndValue = line[ i ].Split( ',' ) ;
	
							if( keyAndValue.Length >  0  && string.IsNullOrEmpty( keyAndValue[ 0 ] ) == false )
							{
								size = 0 ;
								if( keyAndValue.Length >  1 && string.IsNullOrEmpty( keyAndValue[ 1 ] ) == false )
								{
									int.TryParse( keyAndValue[ 1 ], out size ) ;
								}
								crc = 0 ;
								if( keyAndValue.Length >  2 && string.IsNullOrEmpty( keyAndValue[ 2 ] ) == false )
								{
									uint.TryParse( keyAndValue[ 2 ], out crc ) ;
								}

								crcHash.Add( keyAndValue[ 0 ].ToLower(), new AssetBundleCrcDescriptor( size,crc ) ) ;
							}
						}
					}
				}

				//-------------

				// 一旦、パスとハッシュを突っ込む
				m_AssetBundleInfo.Clear() ;
				m_AssetBundleHash.Clear() ;

				ManifestInfo.AssetBundleInfo node ;

				l = assetBundleName.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					size	= 0 ;
					crc		= 0 ;
					if( crcHash != null && crcHash.ContainsKey( assetBundleName[ i ] ) == true )
					{
						size	= crcHash[ assetBundleName[ i ] ].Size ;                                                                                                                                                                                                                  
						crc		= crcHash[ assetBundleName[ i ] ].Crc ;
					}

					node = new ManifestInfo.AssetBundleInfo( assetBundleName[ i ], assetBundleHash[ i ], size,crc, 0L ) ;
					m_AssetBundleInfo.Add( node ) ;
					if( m_AssetBundleHash.ContainsKey( assetBundleName[ i ] ) == false )
					{
						m_AssetBundleHash.Add( assetBundleName[ i ], node ) ;
					}
				}

				//--------------------------------------------------------

				// ローカルの情報をマージし更新すべきファイルのフラグを立てる
				Marge( instance ) ;

				// ここでセーブしておく
				Save() ;

				//------------------------------------------------------------
				
				m_Busy = false ;	// 処理終了

				//------------------------------------------------------------

				// 使用可能状態となった
				Completed = true ;
			}
			
			// ローカルの情報をマージし更新すべきファイルのフラグを立てる
			private void Marge( AssetBundleManager instance )
			{
				string manifestName = ManifestName ;
				
				string text = StorageAccessor_LoadText( manifestName + "/" + manifestName + ".manifest" ) ;
				if( string.IsNullOrEmpty( text ) == true )
				{
					// 一度も保存されていない
					return ;
				}

				int i, l = text.Length ;
				if( text[ l - 1 ] == '\n' )
				{
					// 最後の改行をカット
					text = text.Substring( 0, l - 1 ) ;
				}

				string[] line = text.Split( '\n' ) ;
				if( line == null || line.Length == 0 )
				{
					return ;
				}

				//---------------------------------------------------------

				AssetBundleInfo node ;
				List<AssetBundleInfo> info = new List<AssetBundleInfo>() ;
				Dictionary<string,AssetBundleInfo> hash = new Dictionary<string, AssetBundleInfo>() ;

				for( i  = 0 ; i <   line.Length ; i ++ )
				{
					l = line[ i ].Length ;
					if( line[ i ][ l - 1 ] == '\t' )
					{
						// 最後のタブをカット
						line[ i ] = line[ i ].Substring( 0, l - 1 ) ;
					}

					string[] code = line[ i ].Split( '\t' ) ;

					if( code.Length == 5 )
					{
						node =  new AssetBundleInfo( code[ 0 ], code[ 1 ], int.Parse( code[ 2 ] ), uint.Parse( code[ 3 ] ), long.Parse( code[ 4 ] ) ) ;
						info.Add( node ) ;
						if( hash.ContainsKey( code[ 0 ] ) == false )
						{
							hash.Add( code[ 0 ], node ) ;
						}
					}
				}

				//----------------------------------------------
					
				// まずはリモートから見て更新の必要の無いものをチェックする
				string assetBundleName ;
				int size ;

				for( i  = 0 ; i <  m_AssetBundleInfo.Count ; i ++ )
				{
					assetBundleName = m_AssetBundleInfo[ i ].Path ;
					if( hash.ContainsKey( assetBundleName ) == true )
					{
						// 既に記録した事がある
						node =  hash[ assetBundleName ] ;

						if( m_AssetBundleInfo[ i ].Hash == node.Hash && m_AssetBundleInfo[ i ].Size == node.Size )
						{
							// ハッシュは同じである

							// 実体の存在する場所
							size = StorageAccessor_GetSize( manifestName + "/" + assetBundleName ) ;
							if( size >  0 )
							{
								// このファイルは更新しない
								m_AssetBundleInfo[ i ].LastUpdateTime	= node.LastUpdateTime ;
								m_AssetBundleInfo[ i ].Size				= size ;		// ＣＲＣファイルでのサイズと同じになるはず(ＣＲＣファイルにもサイズを持たせる事：サイズ部分にＣＲＣ値がずれて入り半日潰すバグを発生させた事があった事を忘れるな)
								m_AssetBundleInfo[ i ].UpdateRequired	= false ;
							}
						}
					}
				}

				// つぎに参照が無くなったファイルを削除する
				for( i  = 0 ; i <  info.Count ; i ++ )
				{
					assetBundleName = info[ i ].Path ;
					if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
					{
						// このファイルは参照が無くなる
						StorageAccessor_Remove( manifestName + "/" + assetBundleName ) ;
					}
				}

				// ファイルが存在しなくなったフォルダも削除する
				if( instance.m_SecretPathEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( manifestName + "/" ) ;
				}
			}

			/// <summary>
			/// 最新のマニフェスト情報をローカルストレージに保存する
			/// </summary>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			public bool Save()
			{
				string manifestName = ManifestName ;

				if( string.IsNullOrEmpty( manifestName ) == true )
				{
					// File Path に異常がある
					return false ;
				}

				string path = manifestName + "/" + manifestName + ".manifest" ;

				string text = string.Empty ;

				int i, l = m_AssetBundleInfo.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					text += m_AssetBundleInfo[ i ].Path + "\t" ;
					text += m_AssetBundleInfo[ i ].Hash + "\t" ;
					text += m_AssetBundleInfo[ i ].Size + "\t" ;
					text += m_AssetBundleInfo[ i ].Crc  + "\t" ;
					text += m_AssetBundleInfo[ i ].LastUpdateTime + "\n" ;
				}

				return StorageAccessor_SaveText( path, text, true ) ;
			}

			//----------------------------------------------------------------------

			// ローカルにアセットバンドルが存在しない場合にリモートから取得しローカルに保存する
			private IEnumerator LoadAssetBundleFromRemote_Coroutine( AssetBundleInfo assetBundleInfo, string assetBundleLocalPath, bool keepChange, bool keep, Request request, AssetBundleManager instance )
			{
				string assetBundleRemotePath ;
				byte[] data = null ;

				for( int t  = 0 ; t <  2 ; t ++ )
				{
					if( data == null || data.Length == 0 )
					{ 
						if( ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( instance.m_UseStreamingAssets == true )
							{
								// StreamingAssets からダウンロードを試みる
								assetBundleRemotePath = StreamingAssetsPath + "/" + assetBundleInfo.Path ;

								// ストリーミングアセットから読み出してみる
								yield return instance.StartCoroutine( StorageAccessor.LoadFromStreamingAssets( assetBundleRemotePath, ( _ ) => { data = _ ; } ) ) ;
								if( data != null && data.Length >  0 )
								{
									request.Progress = 1.0f ;
								}
							}
						}
						else
						if( ( t == 1 && instance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && instance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( instance.m_UseDownload == true )
							{
								// ネットワークからダウンロードを試みる
								assetBundleRemotePath = Path + "/" + assetBundleInfo.Path + "?time=" + GetClientTime() ;

								// DownloadHandler を自作しないならスタティックメソッドを使うこと(.downloadHandler が null になってしまうため)
								UnityWebRequest www = UnityWebRequest.Get( assetBundleRemotePath ) ; ;
	//							www.downloadHandler = new DownloadHandlerBuffer() ;
								www.SendWebRequest() ;

								while( true )
								{
									Progress = www.downloadProgress ;

									request.Progress = Progress ;

									if( www.isHttpError == true || www.isNetworkError == true || string.IsNullOrEmpty( www.error ) == false )
									{
										Error = www.error ;
										break ;
									}

									if( www.isDone == true )
									{
										break ;
									}

									yield return null ;
								}

								if( string.IsNullOrEmpty( www.error ) == true )
								{
									// 成功
									data = www.downloadHandler.data ;
								}

								www.Dispose() ;
							}
						}
					}
					if( data != null && data.Length >  0 )
					{
						break ;
					}
				}

				//-------------------------------------------------

				if( data == null || data.Length == 0 )
				{
					// 失敗
					Error = "Could not load data" ;
					request.Error	= Error ;    // 失敗

					yield break ;
				}

				if( assetBundleInfo.Crc != 0 )
				{
					// ＣＲＣのチェックが必要
					uint crc = GetCRC32( data ) ;
					if( crc != assetBundleInfo.Crc )
					{
						Error = "Bad CRC" ;
						request.Error = Error ;    // 失敗

						yield break ;
					}
				}

				//---------------------------------------------------------

				// 成功
				while( m_Busy )
				{
					yield return null ;	// 同じマニフェストに対し同時に処理を行おうとした場合は排他処理を行う
				}
						
				m_Busy = true ;

				// キャッシュに空きが出来るまで古いものから順に削除していく
				if( CacheSize <= 0 || ( CacheSize >  0 && Cleanup( data.Length ) == true ) )
				{
					// 空き領域が確保出来た
					// ファイルを保存しアセットバンドルインフォを更新する
					if( StorageAccessor_Save( assetBundleLocalPath, data, true ) == true )
					{
						assetBundleInfo.LastUpdateTime = GetClientTime() ;	// タイムスタンプ更新
						assetBundleInfo.Size = data.Length ;		// ＣＲＣファイルが存在しない場合はここで初めてサイズが書き込まれる

						if( keepChange == true )
						{
							assetBundleInfo.Keep = keep ;
						}

						assetBundleInfo.UpdateRequired = false ;

						// マニフェストをセーブする
						Save() ;
					}
					else
					{
						// 保存出来ない
						Error = "Could not save" ;
						request.Error	= Error ;	// 失敗
					}
				}
				else
				{
					// 空き領域が確保出来ない
					Error = "Could not alocate space" ;
					request.Error	= Error ;    // 失敗
				}

				m_Busy = false ;
			}

			//---------------------------------------------------------

			// ManifestInfo :: Asset
						
			/// <summary>
			/// アセットを取得する(同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object LoadAsset( string assetBundleName, string assetName, Type type, bool assetBundleCaching, AssetBundleManager instance )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundleName, assetBundleCaching, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットのロード
				UnityEngine.Object asset = assetBundleInfo.LoadAsset( assetBundle, assetBundleName, assetName, type, instance ) ;

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAsset アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
#endif

					// どうもアセットのロードは非同期で行われているらしく、その最中に破棄を実行するとエラーになってしまう。基本的に同期ロードは使えないということか。
//					tAssetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}

				return asset ;
			}
			
			/// <summary>
			/// アセットを取得する(非同期版)　※直接呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAsset_Coroutine( string assetBundleName, string assetName, Type type, Action<UnityEngine.Object> onLoaded, bool keep, Request request, bool assetBundleCaching, AssetBundleManager instance )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundleName, ( _ ) => { assetBundle = _ ; }, keep, request, assetBundleCaching, instance ) ) ;

				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期(現状使用できない)
//					Debug.LogWarning( "非同期" ) ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAssetHolder, tRequest, tInstance ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadAsset( assetBundle, assetBundleName, assetName, type, instance ) ;
//				}

				if( asset != null )
				{
					onLoaded?.Invoke( asset ) ;
				}

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAsset_Coroutine アセットバンドルの自動破棄予約: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllAssets

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object[] LoadAllAssets( string assetBundleName, Type type, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundleName, assetBundleCaching, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットのロード
				UnityEngine.Object[] assets = assetBundleInfo.LoadAllAssets( assetBundle, type, instance, resourcePath ) ;

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + type + " )" ) ;
#endif
//					assetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}

				return assets ;
			}

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAllAssets_Coroutine( string assetBundleName, Type type, Action<UnityEngine.Object[]> onLoaded, bool keep, Request request, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundleName, ( _ ) => { assetBundle = _ ; }, keep, request, assetBundleCaching, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssetsHolder, tRequest, tInstance, tResourcePath ) ) ;
//					tAllSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllAssets( assetBundle, type, instance, resourcePath ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets ) ;
				}

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets_Coroutine アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + type + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					assetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: SubAsset

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tSubAssetName">サブアセット名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>サブアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object LoadSubAsset( string assetBundleName, string assetName, string subAssetName, Type type, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundleName, assetBundleCaching, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットのロード
				UnityEngine.Object asset = assetBundleInfo.LoadSubAsset( assetBundle, assetBundleName, assetName, subAssetName, type, instance, resourcePath ) ;

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadSubAsset アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
#endif
//					tAssetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}

				return asset ;
			}

			/// <summary>
			/// アセットに含まれるサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tSubAssetName">サブアセット名</param>
			/// <param name="rSubAsset">サブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadSubAsset_Coroutine( string assetBundleName, string assetName, string subAssetName, Type type, Action<UnityEngine.Object> onLoaded, bool keep, Request request, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundleName, ( _ ) => { assetBundle = _ ; }, keep, request, assetBundleCaching, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				UnityEngine.Object asset ;

				// アセットバンドルが読み出せた(あと一息)
//				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadSubAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tSubAssetName, tType, rSubAssetHolder, tRequest, tInstance, tResourcePath ) ) ;
//				}
//				else
//				{
					// 同期
					asset = assetBundleInfo.LoadSubAsset( assetBundle, assetBundleName, assetName, subAssetName, type, instance, resourcePath ) ;
//				}

				if( asset != null )
				{
					onLoaded?.Invoke( asset ) ;
				}

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadSubAsset_Coroutine アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}
			}

			//-----------------------

			// ManifestInfo :: AllSubAssets

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>全てのサブアセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object[] LoadAllSubAssets( string assetBundleName, string assetName, Type type, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = LoadAssetBundle( assetBundleName, assetBundleCaching, instance ) ;
				if( assetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットのロード
				UnityEngine.Object[] assets = assetBundleInfo.LoadAllSubAssets( assetBundle, assetBundleName, assetName, type, instance, resourcePath ) ;

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
#endif
//					assetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}

				return assets ;
			}

			/// <summary>
			/// アセットに含まれる全てのサブアセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="rAllSubAssets">全てのサブアセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tResourcePath">アセットのリソースパス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAllSubAssets_Coroutine( string assetBundleName, string assetName, Type type, Action<UnityEngine.Object[]> onLoaded, bool keep, Request request, bool assetBundleCaching, AssetBundleManager instance, string resourcePath )
			{
				AssetBundle assetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				yield return instance.StartCoroutine( LoadAssetBundle_Coroutine( assetBundleName, ( _ ) => { assetBundle = _ ; }, keep, request, assetBundleCaching, instance ) ) ;
				if( assetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// アセットバンドルが読み出せた(あと一息)
				UnityEngine.Object[] assets = null ;

//				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//				{
//					// 非同期
//					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
//					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssetsHolder, tRequest, tInstance, tResourcePath ) ) ;
//					tAllSubAssets = rAllSubAssetsHolder[ 0 ] ;
//				}
//				else
//				{
					// 同期
					assets = assetBundleInfo.LoadAllSubAssets( assetBundle, assetBundleName, assetName, type, instance, resourcePath ) ;
//				}

				if( assets != null && assets.Length >  0 )
				{
					onLoaded?.Invoke( assets ) ;
				}

				if( assetBundleCaching == false && m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets_Coroutine アセットバンドルを破棄する: [ " + assetBundleName + " ] ( " + assetName + " : " + type + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					instance.AddAutoCleaningTarget( assetBundle ) ;
				}
			}

			//--------------------------------------

			// ManifestInfo :: AssetBundle

			/// <summary>
			/// アセットバンドルを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットバンドルのインスタンス</returns>
			internal protected AssetBundle LoadAssetBundle( string assetBundleName, bool assetBundleCaching, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return null ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// キャッシュから削除する
					RemoveAssetBundleCache( assetBundleName ) ;

					// 更新対象になっているので取得不可
					return null ;
				}
				
				//-------------------------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundleNames = m_Manifest.GetAllDependencies( assetBundleName ) ;
					if( dependentAssetBundleNames != null && dependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

						Debug.LogWarning( "同期:依存するアセットバンドルが存在する: [ " + dependentAssetBundleNames.Length + " ] <- " + assetBundleName ) ;

						string			dependentAssetBundleName ;
						AssetBundleInfo	dependentAssetBundleInfo ;
						AssetBundle		dependentAssetBundle ;

						int i, l = dependentAssetBundleNames.Length ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							dependentAssetBundleName = dependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

							Debug.LogWarning( "同期:依存するアセットバンドル名:" + dependentAssetBundleName ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundleName ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCache( dependentAssetBundleName ) ;

//								Debug.LogWarning( "同期:依存するアセットバンドルに更新が必要なものがある:" + tDependentAssetBundleName ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return null ;
							}
						}

						//-------------------------------

						for( i  = 0 ; i <  l ; i ++ )
						{
							dependentAssetBundleName = dependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

//							Debug.LogWarning( "同期:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundleName ] ;

							// 全て更新対象ではないはず

							// キャッシュに存在するか確認する
							if( m_AssetBundleCache.ContainsKey( dependentAssetBundleName ) == false )
							{
								// キャッシュに存在しない
								dependentAssetBundle = StorageAccessor_LoadAssetBundle( ManifestName + "/" + dependentAssetBundleInfo.Path ) ;
								if( dependentAssetBundle != null )
								{
									// キャッシュにためる
									AddAssetBundleCache( dependentAssetBundleName, dependentAssetBundle, instance ) ;
								}
							}
						}
					}
				}

				//-------------------------------------------------------------
				
				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				AssetBundle assetBundle ;
				
				if( m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュには存在しない
					assetBundle = StorageAccessor_LoadAssetBundle( ManifestName + "/" + assetBundleInfo.Path ) ;
					if( assetBundle == null )
					{
						return null ;	// アセットバンドルが展開出来ない
					}

					// キャッシュにためる
					if( assetBundleCaching == true )
					{
//						Debug.LogWarning( "アセットバンドルをキャッシュにためました:" + tAssetBundleName ) ;
						AddAssetBundleCache( assetBundleName, assetBundle, instance ) ;
					}
				}
				else
				{
					// キャッシュに存在する
					assetBundle = m_AssetBundleCache[ assetBundleName ].AssetBundle ;
				}

				// アセットバンドルのインスタンスを返す
				return assetBundle ;
			}

			/// <summary>
			/// アセットバンドルを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAssetBundle_Coroutine( string assetBundleName, Action<AssetBundle> onLoaded, bool keep, Request request, bool assetBundleCaching, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックをかける
				if( assetBundleInfo.Busy == true )
				{
					yield return new WaitWhile( () => assetBundleInfo.Busy == true ) ;
				}

				assetBundleInfo.Busy = true ;

				//------------------------------------------

				// ローカルのパス
				string assetBundleLocalPath = ManifestName + "/" + assetBundleInfo.Path ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// キャッシュから削除する
					RemoveAssetBundleCache( assetBundleName ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( assetBundleInfo, assetBundleLocalPath, true, keep, request, instance ) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// 失敗
						yield break ;
					}
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundleNames = m_Manifest.GetAllDependencies( assetBundleName ) ;
					if( dependentAssetBundleNames != null && dependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

						Debug.LogWarning( "非同期:依存するアセットバンドルが存在する: [ " + dependentAssetBundleNames.Length + " ] <- " + assetBundleName ) ;

						string			dependentAssetBundleName ;
						AssetBundleInfo	dependentAssetBundleInfo ;
						string			dependentAssetBundleLocalPath ;
						AssetBundle		dependentAssetBundle ;

						int i, l = dependentAssetBundleNames.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							dependentAssetBundleName = dependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字化

							Debug.LogWarning( "非同期:依存するアセットバンドル名:" + dependentAssetBundleName ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundleName ] ;

//							Debug.LogWarning( "非同期:ロック状態:" + tDependentAssetBundleName + " " + tDependentAssetBundleInfo.busy ) ;

							//------------------------------

							// 非同期で同じアセットバンドルにアクセスする場合は排他ロックがかかる
							if( dependentAssetBundleInfo.Busy == true )
							{
								yield return new WaitWhile( () => dependentAssetBundleInfo.Busy == true ) ;
							}
							
							dependentAssetBundleInfo.Busy = true ;

							//------------------------------

							dependentAssetBundleLocalPath = ManifestName + "/" + dependentAssetBundleInfo.Path ;

//							Debug.LogWarning( "非同期:更新が必要か:" + tDependentAssetBundleName + " " + tDependentAssetBundleInfo.update ) ;

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// キャッシュから削除する
								RemoveAssetBundleCache( dependentAssetBundleName ) ;

								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードを試みる:" + dependentAssetBundleName ) ;

								// 更新対象になっているのでダウンロードを試みる
								yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( dependentAssetBundleInfo, dependentAssetBundleLocalPath, true, keep, request, instance ) ) ;
							}

							if( dependentAssetBundleInfo.UpdateRequired == false )
							{
								// 依存アセットバンドルのダウンロードに成功
								if( m_AssetBundleCache.ContainsKey( dependentAssetBundleName ) == false )
								{
									// キャッシュには存在しないのでロードする
//									if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//									{
//										// 非同期(低速)
//										yield return tInstance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( tDependentAssetBundleLocalPath, rDependentAssetBundleHolder ) ) ;
//									}
//									else
//									{
										// 同期(高速)
										dependentAssetBundle = StorageAccessor_LoadAssetBundle( dependentAssetBundleLocalPath ) ;
//									}

									if( dependentAssetBundle != null )
									{
										// キャッシュにためる
//										Debug.LogWarning( "キャッシュにためます:" + tDependentAssetBundleName ) ;
										AddAssetBundleCache( dependentAssetBundleName, dependentAssetBundle, instance ) ;
									}
								}
							}

							// 排他ロック解除
							dependentAssetBundleInfo.Busy = false ;

							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードに失敗した:" + dependentAssetBundleName ) ;

								// 依存アセットバンドルのダウンロードに失敗
								request.Error = "Could not dependent load" ;
								yield break ;
							}
						}
					}
				}

				//----------------------------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する

				AssetBundle assetBundle ;

				if( m_AssetBundleCache.ContainsKey( assetBundleName ) == false )
				{
					// キャッシュには存在しない
//					if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
//					{
//						// 非同期(低速)
//						yield return tInstance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( tAssetBundleLocalPath, rAssetBundleHolder ) ) ;
//					}
//					else
//					{
						// 同期(高速)
						assetBundle = StorageAccessor_LoadAssetBundle( assetBundleLocalPath ) ;
//					}

					// キャッシュにためる
					if( assetBundleCaching == true )
					{
						AddAssetBundleCache( assetBundleName, assetBundle, instance ) ;
					}
				}
				else
				{
					// キャッシュに存在する
					assetBundle = m_AssetBundleCache[ assetBundleName ].AssetBundle ;
				}
				
				//---------------------------------------------------------

				// 保存する
				if( assetBundle != null )
				{
					onLoaded?.Invoke( assetBundle ) ;
				}

				//---------------------------------------------------------

				// 排他ロック解除
				assetBundleInfo.Busy = false ;
			}
			
			/// <summary>
			/// アセットバンドルのダウンロードを行う　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundle_Coroutine( string assetBundleName, Action<bool> onLoaded, bool keep, Request request, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// ローカルのパス
				string assetBundleLocalPath = ManifestName + "/" + assetBundleInfo.Path ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					RemoveAssetBundleCache( assetBundleName ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return instance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( assetBundleInfo, assetBundleLocalPath, true, keep, request, instance ) ) ;

					if( assetBundleInfo.UpdateRequired == true )
					{
						// ダウンロード失敗
						request.Error = "Could not load." ;
						yield break ;
					}
				}

				// 成功
				onLoaded?.Invoke( true ) ;
			}

			//----------------------------------------------------------

			/// <summary>
			/// アセットバンドルをキャッシュから削除する　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected bool RemoveAssetBundle( string assetBundleName, AssetBundleManager instance )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// ローカルのパス
				string assetBundleLocalPath = ManifestName + "/" + assetBundleInfo.Path ;
				
				// 削除した
				StorageAccessor_Remove( assetBundleLocalPath ) ;

				// ファイルが存在しなくなったフォルダも削除する
				if( instance.m_SecretPathEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( ManifestName + "/" ) ;
				}

				// 更新必要フラグをオンにする
				assetBundleInfo.UpdateRequired = true ;

				return true ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットバンドルの保有を確認する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Contains( string assetBundleName )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return true ;
			}

			/// <summary>
			/// アセットバンドルの存在を確認する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Exists( string assetBundleName )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( assetBundleInfo.UpdateRequired == true )
				{
					// 更新対象になっているので取得不可
					return false ;
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] dependentAssetBundleNames = m_Manifest.GetAllDependencies( assetBundleName ) ;
					if( dependentAssetBundleNames != null && dependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

//						Debug.LogWarning( "存在確認:依存するアセットバンドルが存在する: [ " + tDependentAssetBundleNames.Length + " ] <- " + tAssetBundleName ) ;

						string			dependentAssetBundleName ;
						AssetBundleInfo	dependentAssetBundleInfo ;

						for( int i  = 0 ; i <  dependentAssetBundleNames.Length ; i ++ )
						{
							dependentAssetBundleName = dependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字化

//							Debug.LogWarning( "存在確認:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundleName ] ;
							if( dependentAssetBundleInfo.UpdateRequired == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCache( dependentAssetBundleName ) ;

								Debug.LogWarning( "----- ※存在:依存するアセットバンドルに更新が必要なものがある:" + dependentAssetBundleName + " <- " + assetBundleName ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return false ;
							}
						}
					}
				}

				// 依存も含めて全て問題の無い状態になっている
				return true ;
			}
			
			/// <summary>
			/// アセットバンドルのサイズを取得する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected int GetSize( string assetBundleName )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return 0 ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo assetBundleInfo = m_AssetBundleHash[ assetBundleName ] ;

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return assetBundleInfo.Size ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePaths( bool updateRequiredOnly = true )
			{
				List<string> path = new List<string>() ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( updateRequiredOnly == false || ( updateRequiredOnly == true && assetBundleInfo.UpdateRequired == true ) )
					{
						path.Add( assetBundleInfo.Path ) ;
					}
				}

				return path.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 依存関係にあるアセットバンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllDependentAssetBundlePaths( string assetBundleName, bool updateRequiredOnly = true )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				string[] dependentAssetBundleNames = m_Manifest.GetAllDependencies( assetBundleName ) ;

				if( dependentAssetBundleNames == null || dependentAssetBundleNames.Length == 0 )
				{
					return null ;	// 依存するアセットバンドル存在しない
				}

				if( updateRequiredOnly == false )
				{
					// そのまま返す
					return dependentAssetBundleNames ;
				}

				//---------------------------------------------------------

				// 更新が必要なもののみ返す

				List<string> path = new List<string>() ;

				string			dependentAssetBundleName ;
				AssetBundleInfo	dependentAssetBundleInfo ;

				for( int i  = 0 ; i <  dependentAssetBundleNames.Length ; i ++ )
				{
					dependentAssetBundleName = dependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字化

//					Debug.LogWarning( "依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

					dependentAssetBundleInfo = m_AssetBundleHash[ dependentAssetBundleName ] ;
					if( dependentAssetBundleInfo.UpdateRequired == true )
					{
						// 更新対象であるため取得不可
						path.Add( dependentAssetBundleNames[ i ] ) ;
					}
				}

				if( path.Count == 0 )
				{
					return null ;
				}

				return path.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 指定のアセットバンドルのキャッシュ内での動作を設定する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <returns>結果(true=成功・失敗)</returns>
			public bool SetKeepFlag( string assetBundleName, bool keep )
			{
				// 全て小文字化
				assetBundleName = assetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( m_AssetBundleHash.ContainsKey( assetBundleName ) == true )
				{
					m_AssetBundleHash[ assetBundleName ].Keep = keep ;
					return true ;
				}

				return false ;
			}

			/// <summary>
			/// 破棄可能なアセットバンドルをタイムスタンプの古い順に破棄してキャッシュの空き容量を確保する
			/// </summary>
			/// <param name="size">必要なキャッシュサイズ</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			private bool Cleanup( int requireSize )
			{
				// キープ対象全てとキープ非対象でタイムスタンプの新しい順にサイズを足していきキャッシュの容量をオーバーするまで検査する
				List<AssetBundleInfo> freeAssetBundleInfo = new List<AssetBundleInfo>() ;

				long freeCacheSize = CacheSize ;

				foreach( var assetBundleInfo in m_AssetBundleInfo )
				{
					if( assetBundleInfo.UpdateRequired == false )
					{
						// 更新の必要の無い最新の状態のアセットバンドル
						if( assetBundleInfo.Keep == true )
						{
							// 常時保持する必要のあるアセットパンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								freeCacheSize -= assetBundleInfo.Size ;
//							}
						}
						else
						{
							// 空き容量が足りなくなったら破棄してもよいアセットバンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								// 破棄可能なアセットバンドルの情報を追加する
								freeAssetBundleInfo.Add( assetBundleInfo ) ;
//							}
						}
					}
				}

				if( freeCacheSize <  requireSize )
				{
					// 破棄しないアセットバンドルだけで既に空き容量が足りない
					return false ;
				}
				
				if( freeAssetBundleInfo.Count == 0 )
				{
					// 空き容量は足りる
					return true ;
				}

				//--------------------------------

				// 破棄できるアセットバンドルで既にに実体を持っているものをタイムスタンプの新しいものの順にソートする
				freeAssetBundleInfo.Sort( ( a, b ) => ( int )( a.LastUpdateTime - b.LastUpdateTime ) ) ;

				int i, l = freeAssetBundleInfo.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					freeCacheSize -= freeAssetBundleInfo[ i ].Size ;
					if( freeCacheSize <  requireSize )
					{
						// ここから容量が足りない
						break ;
					}
				}

				if( i >= l )
				{
					// 空き容量は足りる
					return true ;
				}

				int s = i ;

				string manifestName = ManifestName ;

				// ここからアセットバンドルを破棄する(古いものを優先的に破棄)
				for( i  = s ; i <  l ; i ++ )
				{
					// 一時的に削除するだけなので空になったフォルダまで削除する事はしない
					StorageAccessor_Remove( manifestName + "/" + freeAssetBundleInfo[ i ].Path ) ;
					
					freeAssetBundleInfo[ i ].LastUpdateTime = 0L ;
					freeAssetBundleInfo[ i ].UpdateRequired = true ;
				}

				// マニフェスト情報を保存しておく
//				Save( tInstance ) ;	// この後の追加保存でマニフェスト情報を保存するのでここでは保存しない

				return true ;
			}
		}
	}
}
