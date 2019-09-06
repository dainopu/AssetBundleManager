using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using System.Security.Cryptography ;

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
			public class AssetBundleCache
			{
				public	AssetBundle	assetBundle ;
				public	float		lastAccessTime ;

				public AssetBundleCache( AssetBundle tAssetBundle, float tLastAccessTime )
				{
					assetBundle		= tAssetBundle ;
					lastAccessTime	= tLastAccessTime ;
				}
			}

			/// <summary>
			/// アセットバンドルのキャッシュ
			/// </summary>
			public Dictionary<string,AssetBundleCache>	assetBundleCache = new Dictionary<string, AssetBundleCache>() ;

			/// <summary>
			/// アセットバンドルキャッシュに格納可能な最大数
			/// </summary>
			public int	assetBundleCacheLimit = 128 ;

			/// <summary>
			/// キャッシュにアセットバンドルを追加する
			/// </summary>
			/// <param name="tName"></param>
			/// <param name="tAssetBundle"></param>
			public void AddAssetBundleCache( string tAssetBundleName, AssetBundle tAssetBundle, AssetBundleManager tInstance )
			{
				// キャッシュに追加されるアセットバンドルは自動破棄対象にはしない
				tInstance.RemoveAutoCleaningTarget( tAssetBundle ) ;

				if( assetBundleCache.ContainsKey( tAssetBundleName ) == true )
				{
					// 既に登録済みなので最終アクセス時間を更新して戻る
					assetBundleCache[ tAssetBundleName ].lastAccessTime = Time.realtimeSinceStartup ;
					return ;
				}

				int i, l = assetBundleCache.Count ;
				if( l >= assetBundleCacheLimit )
				{
					// 最もアクセス時間が古いものをキャッシュから削除する
					string[] tKeys = new string[ l ] ;
					assetBundleCache.Keys.CopyTo( tKeys, 0 ) ;

					string tKey = "" ;
					float tTime = Mathf.Infinity ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( assetBundleCache[ tKeys[ i ] ].lastAccessTime <  tTime )
						{
							tKey = tKeys[ i ] ;
							tTime = assetBundleCache[ tKeys[ i ] ].lastAccessTime ;
						}
					}

					assetBundleCache[ tKey ].assetBundle.Unload( false ) ;
					assetBundleCache[ tKey ] = null ;
					assetBundleCache.Remove( tKey ) ;
				}

				// キャッシュを追加する
				assetBundleCache.Add( tAssetBundleName, new AssetBundleCache( tAssetBundle, Time.realtimeSinceStartup ) ) ;
			}

			// キャッシュからアセットバンドルを削除する
			public void RemoveAssetBundleCache( string tAssetBundleName )
			{
				if( assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					return ;	// 元々キャッシュには存在しない
				}

				assetBundleCache[ tAssetBundleName ].assetBundle.Unload( false ) ;
				assetBundleCache[ tAssetBundleName ] = null ;
				assetBundleCache.Remove( tAssetBundleName ) ;
			}

			/// <summary>
			/// アセットバンドルキャッシュをクリアする
			/// </summary>
			public void ClearAssetBundleCache()
			{
				int i, l = assetBundleCache.Count ;

#if UNITY_EDITOR
				Debug.Log( "[AssetBundleManager] キャッシュからクリア対象となる展開済みアセットバンドル数 = " + l + " / " + name ) ;
#endif
				if( l == 0 )
				{
					return ;
				}

				string[] tKeys = new string[ l ] ;

				assetBundleCache.Keys.CopyTo( tKeys, 0 ) ;

				for( i  = 0 ; i <  l ; i ++ )
				{
//					Debug.LogWarning( "破棄するアセットバンドル:" + tKeys[ i ] ) ;
					assetBundleCache[ tKeys[ i ] ].assetBundle.Unload( false ) ;
					assetBundleCache[ tKeys[ i ] ] = null ;
					assetBundleCache.Remove( tKeys[ i ] ) ;
				}

				assetBundleCache.Clear() ;
			}

			//--------------------------------------------------------------------------

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public ManifestInfo()
			{
				assetBundleInfo = new List<AssetBundleInfo>() ;
				assetBundleLink = new Dictionary<string, AssetBundleInfo>() ;	// Dictionary は、Inspector 上でインスタンスを生成しても、自動では生成してくれないため、明示的に生成する必要がある。
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="tFilePath">リモート側のマニフェストのパス</param>
			/// <param name="tCacheSize">マニフェストごとのキャッシュサイズ(0で無制限)</param>
			public ManifestInfo( string filePath, long cacheSize )
			{
				assetBundleInfo = new List<AssetBundleInfo>() ;
				assetBundleLink = new Dictionary<string, AssetBundleInfo>() ;	// Dictionary は、Inspector 上でインスタンスを生成しても、自動では生成してくれないため、明示的に生成する必要がある。

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
				public string	path = "" ;

				/// <summary>
				/// ハッシュ値
				/// </summary>
				public string	hash = "" ;

				/// <summary>
				/// アセットバンドルファイルのサイズ
				/// </summary>
				public int		size = 0 ;			// サイズ(処理の高速化のためにここに保持しておく)※キャッシュオーバーなどの際の処理に使用する

				/// <summary>
				/// ＣＲＣ値(０で使用しない)
				/// </summary>
				public uint		crc = 0 ;

				/// <summary>
				/// 最終更新日時
				/// </summary>
				public long		time = 0 ;

				/// <summary>
				/// キャッシュオーバーする際に破棄可能にするかどうかを示す
				/// </summary>
				public bool		keep = false ;

				/// <summary>
				/// 更新が必要がどうかを示す
				/// </summary>
				public bool		update = true ;		// 更新が必要かどうか

				/// <summary>
				/// 非同期アクセス時の排他ロックフラグ
				/// </summary>
				public bool		busy = false ;

				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="tPath">マニフェスト内での相対パス</param>
				/// <param name="tHash">ハッシュ値</param>
				/// <param name="tTime">最終更新日時</param>
				public AssetBundleInfo( string tPath, string tHash, int tSize, uint tCrc, long tTime )
				{
					path	= tPath ;
					hash	= tHash ;
					size	= tSize ;
					crc		= tCrc ;
					time	= tTime ;
				}


				// 対象がプレハブの場合にシェーダーを付け直す(Unity Editor で Platform が Android iOS の場合の固有バグ対策コード)
				private UnityEngine.Object ReplaceShader( UnityEngine.Object tAsset, Type tType )
				{
#if( UNITY_EDITOR && UNITY_ANDROID ) || ( UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE ) )
					if( tAsset is GameObject )
					{
						GameObject tGO = tAsset as GameObject ;

						Renderer[] tRenderer = tGO.GetComponentsInChildren<Renderer>( true ) ;
						if( tRenderer != null && tRenderer.Length >  0 )
						{
							int i, l = tRenderer.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tRenderer[ i ].sharedMaterials != null && tRenderer[ i ].sharedMaterials.Length >  0 )
								{
									int j, m = tRenderer[ i ].sharedMaterials.Length ;
									for( j  = 0 ; j <  m ; j ++ )
									{
										if( tRenderer[ i ].sharedMaterials[ j ] != null )
										{
											string tSN = tRenderer[ i ].sharedMaterials[ j ].shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											tRenderer[ i ].sharedMaterials[ j ].shader = Shader.Find( tSN ) ;
										}
									}
								}
							}
						}

						MeshRenderer[] tMeshRenderer = tGO.GetComponentsInChildren<MeshRenderer>( true ) ;
						if( tMeshRenderer != null && tMeshRenderer.Length >  0 )
						{
							int i, l = tMeshRenderer.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tMeshRenderer[ i ].sharedMaterials != null && tMeshRenderer[ i ].sharedMaterials.Length >  0 )
								{
									int j, m = tMeshRenderer[ i ].sharedMaterials.Length ;
									for( j  = 0 ; j <  m ; j ++ )
									{
										if( tMeshRenderer[ i ].sharedMaterials[ j ] != null )
										{
											string tSN = tMeshRenderer[ i ].sharedMaterials[ j ].shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											tMeshRenderer[ i ].sharedMaterials[ j ].shader = Shader.Find( tSN ) ;
										}
									}
								}
							}
						}

						SkinnedMeshRenderer[] tSkinnedMeshRenderer = tGO.GetComponentsInChildren<SkinnedMeshRenderer>( true ) ;
						if( tSkinnedMeshRenderer != null && tSkinnedMeshRenderer.Length >  0 )
						{
							int i, l = tSkinnedMeshRenderer.Length ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tSkinnedMeshRenderer[ i ].sharedMaterials != null && tSkinnedMeshRenderer[ i ].sharedMaterials.Length >  0 )
								{
									int j, m = tSkinnedMeshRenderer[ i ].sharedMaterials.Length ;
									for( j  = 0 ; j <  m ; j ++ )
									{
										if( tSkinnedMeshRenderer[ i ].sharedMaterials[ j ] != null )
										{
											string tSN = tSkinnedMeshRenderer[ i ].sharedMaterials[ j ].shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
											tSkinnedMeshRenderer[ i ].sharedMaterials[ j ].shader = Shader.Find( tSN ) ;
										}
									}
								}
							}
						}

					}
					else
					if( tAsset is Material )
					{
						Material tMaterial = tAsset as Material ;
						string tSN = tMaterial.shader.ToString().Replace( " (UnityEngine.Shader)", "" ) ;
						tMaterial.shader = Shader.Find( tSN ) ;
					}
#endif
					return tAsset ;
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
				internal protected UnityEngine.Object LoadAsset( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType )
				{
					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						return null ;
					}

					if( string.IsNullOrEmpty( tAssetName ) == true )
					{
						// mainAsset は Unity5 より非推奨になりました(Unity5 以降の BuildPipeline.BuildAssetBundles から設定項目が消失している)
//						if( tAB.mainAsset != null )
//						{
//							return tAB.mainAsset as T ;
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
						return null ;
					}

					UnityEngine.Object tAsset = null ;
					
					int n, c = m_Index[ tAssetName ].Count ;
					for( n  = 0 ; n <  c ; n ++ )
					{
						tAsset = tAssetBundle.LoadAsset( m_Index[ tAssetName ][ n ], tType ) ;
						if( tAsset != null )
						{
							break ;
						}
					}

					if( tAsset != null )
					{
						tAsset = ReplaceShader( tAsset, tType ) ;
					}

					return tAsset ;
				}

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
/*				internal protected IEnumerator LoadAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, UnityEngine.Object[] rAsset, Request tRequest, AssetBundleManager tInstance )
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
				}*/

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
				internal protected UnityEngine.Object LoadSubAsset( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null )
					{
						return null ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( string.IsNullOrEmpty( tSubAssetName ) == true )
					{
						return null ;
					}

					List<UnityEngine.Object> tAllSubAssets = LoadAllSubAssets( tAssetBundle, tAssetBundleName, tAssetName, tType, tInstance, tResourcePath ) ;
					if( tAllSubAssets == null || tAllSubAssets.Count == 0 )
					{
						return null ;
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

					return tAsset ;
				}

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
/*				internal protected IEnumerator LoadSubAsset_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, UnityEngine.Object[] rSubAsset, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
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
				}*/

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
				internal protected List<UnityEngine.Object> LoadAllSubAssets( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, AssetBundleManager tInstance, string tResourcePath )
				{
					if( tInstance == null )
					{
						return null ;
					}

					CreateIndex( tAssetBundle, tAssetBundleName ) ;

					if( m_Index == null )
					{
						return null ;
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

						UnityEngine.Object[] t = null ;
						int i, l ;
						int n, c = m_Index[ tAssetName ].Count ;

						for( n  = 0 ; n <  c ; n ++ )
						{
							t = tAssetBundle.LoadAssetWithSubAssets( m_Index[ tAssetName ][ n ], tType ) ;
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
								
								if( tAllSubAssets.Count == 0 )
								{
									return null ;
								}

								return tAllSubAssets ;
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
							return null ;
						}

						tAllSubAssets = new List<UnityEngine.Object>() ;

						string tName ;

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
									tAsset = tAssetBundle.LoadAsset( m_Index[ tTarget[ i ] ][ n ], tType ) ;
									if( tAsset != null )
									{
										tAsset = ReplaceShader( tAsset, tType ) ;
										tAllSubAssets.Add( tAsset ) ;
										break ;
									}
								}
							}
						}

						if( tAllSubAssets.Count == 0 )
						{
							return null ;
						}

						return tAllSubAssets ;
					}

					return null ;
				}
				
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
/*				internal protected IEnumerator LoadAllSubAssets_Coroutine( AssetBundle tAssetBundle, string tAssetBundleName, string tAssetName, Type tType, List<UnityEngine.Object>[] rAllSubAssets, Request tRequest, AssetBundleManager tInstance, string tResourcePath )
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
				}*/
	
				//--------------------------------------------------------

				// アセットへのアクセス高速化用のインデックス情報
				private Dictionary<string,List<string>> m_Index = null ;

				// 最初に名称のインデックスを生成する
				private void CreateIndex( AssetBundle tAssetBundle, string tAssetBundleName )
				{
					if( m_Index != null )
					{
						return ;
					}

					m_Index = new Dictionary<string,List<string>>() ;

					// アセットバンドル名も小文字になっている
					tAssetBundleName = tAssetBundleName.ToLower() ;

					string[] tFullPath = tAssetBundle.GetAllAssetNames() ;
					int i, l = tFullPath.Length, p, w = tAssetBundleName.Length ;
					string tName ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						p = tFullPath[ i ].IndexOf( tAssetBundleName ) ;
						if( p >= 0 )
						{
							// 該当しない事は基本的にありえない
							p ++ ;
							tName = tFullPath[ i ].Substring( p + w, tFullPath[ i ].Length - ( p + w ) ) ;
							if( string.IsNullOrEmpty( tName ) == false )
							{
								// 拡張子があれば切り取る
								p = tName.IndexOf( '.' ) ;
								if( p >  0 )
								{
									tName = tName.Substring( 0, p ) ;
								}
								else
								{
									// 拡張子のみになってしまうのでこれはメインアセット
									tName = tFullPath[ i ] ;
									p = tName.LastIndexOf( '/' ) ;
									if( p >= 0 )
									{
										tName = tName.Substring( p + 1, tName.Length - ( p + 1 ) ) ;
									}

									// 拡張子があれば切る
									p = tName.IndexOf( '.' ) ;
									if( p >  0 )
									{
										tName = tName.Substring( 0, p ) ;
									}
								}

								if( m_Index.ContainsKey( tName ) == false )
								{
									m_Index.Add( tName, new List<string>() ) ;
								}
								
								m_Index[ tName ].Add( tFullPath[ i ] ) ;
							}
						}
					}
				}
			}

			//-----------------------------------------------------------------------------------

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報
			/// </summary>
			public List<AssetBundleInfo> assetBundleInfo ;	// インスペクターで確認可能とするため実体はリストで保持する

			/// <summary>
			/// マニフェスト内の全アセットバンドル情報の高速アクセス用のハッシュリスト
			/// </summary>
			internal protected Dictionary<string,AssetBundleInfo> assetBundleLink ;	// ショートカットアクセスのためディクショナリも用意する

			// マニフェストが展開中かを示す
			private bool m_Busy = false ;

			/// <summary>
			/// マニフェストの名前
			/// </summary>
			public string name
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return "" ;
					}

					// 名前を取り出す
					string tName = "" ;

					int i, l ;

					l = FilePath.Length ;
					i = FilePath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						tName = FilePath.Substring( i + 1, l - ( i + 1 ) ) ;
					}
					else
					{
						tName = FilePath ;
					}

					l = tName.Length ;
					i = tName.IndexOf( '.' ) ;
					if( i >= 0 )
					{
						tName = tName.Substring( 0, i ) ;
					}

					return tName ;
				}
			}

			/// <summary>
			/// マニフェストのファイル名
			/// </summary>
			public string fileName
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return "" ;
					}

					// 名前を取り出す
					string tName = "" ;

					int i, l ;

					l = FilePath.Length ;
					i = FilePath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						tName = FilePath.Substring( i + 1, l - ( i + 1 ) ) ;
					}
					else
					{
						tName = FilePath ;
					}

					return tName ;
				}
			}

			/// <summary>
			/// マニフェストのパス
			/// </summary>
			public string path
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return "" ;
					}

					// パスを取り出す
					string tPath = FilePath ;

					int i  ;

					//--------------------------------

					i = tPath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						tPath = tPath.Substring( 0, i ) ;
					}

					//--------------------------------

					return tPath ;
				}
			}

			/// <summary>
			/// ストリーミングアセット内想定のマニフェストのパス
			/// </summary>
			public string streamingAssetsPath
			{
				get
				{
					if( string.IsNullOrEmpty( FilePath ) == true )
					{
						return "" ;
					}

					string tPath = FilePath ;

					int i ;

					//--------------------------------

					i = tPath.IndexOf( "://" ) ;
					if( i >= 0 )
					{
//						string tScheme  = tPath.Substring( 0, i ).ToLower() ;
//						if( tScheme  == "http" || tScheme == "https" )
//						{
							i += 3 ;
							tPath = tPath.Substring( i, tPath.Length - i ) ;

							// ドメイン名も削除する

							i = tPath.IndexOf( '/' ) ;
							if( i >= 0 )
							{
								i ++ ;
								tPath = tPath.Substring( i, tPath.Length - i ) ;
							}
//						}
					}

					//--------------------------------

					i = tPath.LastIndexOf( '/' ) ;
					if( i >= 0 )
					{
						tPath = tPath.Substring( 0, i ) ;
					}

					//--------------------------------

					return tPath ;
				}
			}

			public class AssetBundle_CRC_File
			{
				public int		size ;
				public uint		crc ;

				public AssetBundle_CRC_File( int tSize, uint tCRC )
				{
					size	= tSize ;
					crc		= tCRC ;
				}
			}


			/// <summary>
			/// マニフェストを展開する(非同期)
			/// </summary>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <param name="tStatus">結果を格納する要素数１以上の配列</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAsync( Request result, AssetBundleManager tInstance )
			{
				while( m_Busy )
				{
					yield return null ;	// 同じマニフェストに対し同時に処理を行おうとした場合は排他処理を行う
				}

				m_Busy = true ;

				//------------------------------------
	
				Progress = 0 ;
				Error = "" ;

				//------------------------------------
	
				int i, l ;

				byte[] tData = null ;
				string tText = "" ;

				//------------------------------------

				int t ;

				for( t  = 0 ; t <  2 ; t ++ )
				{
					if( tData == null )
					{
						if( ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							// 最初に StreamingAssets からロードを試みる
							if( tInstance.m_UseStreamingAssets == true )
							{
								// ストリーミングアセットから読み出してみる
								byte[][] rData = { null } ;
								yield return tInstance.StartCoroutine( StorageAccessor.LoadFromStreamingAssets( streamingAssetsPath + "/" + fileName, rData ) ) ;
								if( rData[ 0 ] != null && rData[ 0 ].Length >  0 )
								{
									tData = rData[ 0 ] ;
								}
							}
						}
						else
						if( ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( tInstance.m_UseDownload == true )
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
									tData = www.downloadHandler.data ;
								}

								www.Dispose() ;
								www = null ;
							}
						}
					}
					if( tData != null )
					{
						break ;
					}
				}

				if( tData == null )
				{
					// データが取得出来ない
					Error = "Could not load data" ;
					result.Error = Error ;
					m_Busy = false ;
					yield break ;
				}

				//------------------------------------

				string[] tAssetBundleName = null ;
				string[] tAssetBundleHash = null ;
				
				if( LegacyType == false )
				{
					// ノーマル版

					// バイナリからアセットバンドルを生成する
					AssetBundle tAB = AssetBundle.LoadFromMemory( tData ) ;
					if( tAB == null )
					{
						// アセットバンドルが生成出来ない
						Error = "Could not create AssetBundle" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					m_Manifest = tAB.LoadAsset<AssetBundleManifest>( "AssetBundleManifest" ) ;
					if( m_Manifest == null )
					{
						// マニフェストが取得出来ない
						tAB.Unload( true ) ;

						Error = "Could not get Manifest" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					tAssetBundleName = m_Manifest.GetAllAssetBundles() ;
					if( tAssetBundleName == null || tAssetBundleName.Length == 0 )
					{
						// 内包されるアセットバンドルが存在しない
						tAB.Unload( true ) ;

						Error = "No AssetBundles" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}
					
					// ハッシュを取得
					l = tAssetBundleName.Length ;
					tAssetBundleHash = new string[ l ] ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						tAssetBundleHash[ i ] = m_Manifest.GetAssetBundleHash( tAssetBundleName[ i ] ).ToString() ;
					}

					tAB.Unload( false ) ;
				}
				else
				{
					// レガシー版

					tText = Encoding.UTF8.GetString( tData ) ;

					if( string.IsNullOrEmpty( tText ) == true )
					{
						Error = "Could not get Legacy list file. 1" ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}
					
					string[] tLine = tText.Split( '\n' ) ;
					if( tLine == null || tLine.Length == 0 )
					{
						Error = "Could not get Legacy list file. 2 " ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					List<string> tR = new List<string>() ;

					// 空行がある可能性があるので除外する
					l = tLine.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( tLine[ i ] ) == false )
						{
							tR.Add( tLine[ i ] ) ;
						}
					}
					
					l = tR.Count ;
					if( l == 0 )
					{
						Error = "Could not get Legacy list file. 2 " ;
						result.Error = Error ;
						m_Busy = false ;
						yield break ;
					}

					tAssetBundleName = new string[ l ] ;
					tAssetBundleHash = new string[ l ] ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						string[] tC = tR[ i ].Split( ',' ) ;
						if( tC == null || tC.Length != 2 )
						{
							break ;
						}

						tAssetBundleName[ i ] = tC[ 0 ] ;
						tAssetBundleHash[ i ] = tC[ 1 ] ;
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
				tText = "" ;

				for( t  = 0 ; t <  2 ; t ++ )
				{
					if( string.IsNullOrEmpty( tText ) == true )
					{
						if( ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( tInstance.m_UseStreamingAssets == true )
							{
								// ストリーミングアセットから読み出す
								string[] rText = { null } ;
								yield return tInstance.StartCoroutine( StorageAccessor.LoadTextFromStreamingAssets( streamingAssetsPath + "/" + fileName + ".crc", rText ) ) ;
								if( string.IsNullOrEmpty( rText[ 0 ] ) == false )
								{
									tText = rText[ 0 ] ;
								}
							}
						}
						else
						if( ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( tInstance.m_UseDownload == true )
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
									tText = UTF8Encoding.UTF8.GetString( www.downloadHandler.data ) ;
								}

								www.Dispose() ;
								www = null ;
							}
						}
					}
					if( string.IsNullOrEmpty( tText ) == false )
					{
						break ;
					}
				}

				Dictionary<string,AssetBundle_CRC_File> tCRC_Hash = null ;
				int tSize ;
				uint tCRC ;
				if( string.IsNullOrEmpty( tText ) == false )
				{
					tCRC_Hash = new Dictionary<string, AssetBundle_CRC_File>() ;

					// ＣＲＣデータが取得出来た場合のみアセットバンドル名をキー・ＣＲＣ値をバリューとしたディクショナリを生成する
					string[] tLine = tText.Split( '\n' ) ;
					l = tLine.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( string.IsNullOrEmpty( tLine[ i ] ) == false )
						{
							string[] tKeyAndValue = tLine[ i ].Split( ',' ) ;
	
							if( tKeyAndValue.Length >  0  && string.IsNullOrEmpty( tKeyAndValue[ 0 ] ) == false )
							{
								tSize = 0 ;
								if( tKeyAndValue.Length >  1 && string.IsNullOrEmpty( tKeyAndValue[ 1 ] ) == false )
								{
									int.TryParse( tKeyAndValue[ 1 ], out tSize ) ;
								}
								tCRC = 0 ;
								if( tKeyAndValue.Length >  2 && string.IsNullOrEmpty( tKeyAndValue[ 2 ] ) == false )
								{
									uint.TryParse( tKeyAndValue[ 2 ], out tCRC ) ;
								}

								tCRC_Hash.Add( tKeyAndValue[ 0 ].ToLower(), new AssetBundle_CRC_File( tSize, tCRC ) ) ;
							}
						}
					}
				}

				//-------------

				// 一旦、パスとハッシュを突っ込む
				assetBundleInfo.Clear() ;
				assetBundleLink.Clear() ;

				ManifestInfo.AssetBundleInfo tNode ;

				l = tAssetBundleName.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tSize	= 0 ;
					tCRC	= 0 ;
					if( tCRC_Hash != null && tCRC_Hash.ContainsKey( tAssetBundleName[ i ] ) == true )
					{
						tSize   = tCRC_Hash[ tAssetBundleName[ i ] ].size ;                                                                                                                                                                                                                  
						tCRC	= tCRC_Hash[ tAssetBundleName[ i ] ].crc ;
					}

					tNode = new ManifestInfo.AssetBundleInfo( tAssetBundleName[ i ], tAssetBundleHash[ i ], tSize, tCRC, 0L ) ;
					assetBundleInfo.Add( tNode ) ;
					if( assetBundleLink.ContainsKey( tAssetBundleName[ i ] ) == false )
					{
						assetBundleLink.Add( tAssetBundleName[ i ], tNode ) ;
					}
				}

				//--------------------------------------------------------

				// ローカルの情報をマージし更新すべきファイルのフラグを立てる
				Marge( tInstance ) ;

				// ここでセーブしておく
				Save( tInstance ) ;

				//------------------------------------------------------------
				
				m_Busy = false ;	// 処理終了

				//------------------------------------------------------------

				// 使用可能状態となった
				Completed = true ;
			}
			
			// ローカルの情報をマージし更新すべきファイルのフラグを立てる
			private void Marge( AssetBundleManager tInstance )
			{
				string tName = name ;
				
				string tText = StorageAccessor_LoadText( tName + "/" + tName + ".manifest" ) ;
				if( string.IsNullOrEmpty( tText ) == true )
				{
					// 一度も保存されていない
					return ;
				}

				int i, l = tText.Length ;
				if( tText[ l - 1 ] == '\n' )
				{
					// 最後の改行をカット
					tText = tText.Substring( 0, l - 1 ) ;
				}

				string[] tLine = tText.Split( '\n' ) ;
				if( tLine == null || tLine.Length == 0 )
				{
					return ;
				}

				//---------------------------------------------------------

				AssetBundleInfo tNode ;
				List<AssetBundleInfo> tInfo = new List<AssetBundleInfo>() ;
				Dictionary<string,AssetBundleInfo> tLink = new Dictionary<string, AssetBundleInfo>() ;

				for( i  = 0 ; i <   tLine.Length ; i ++ )
				{
					l = tLine[ i ].Length ;
					if( tLine[ i ][ l - 1 ] == '\t' )
					{
						// 最後のタブをカット
						tLine[ i ] = tLine[ i ].Substring( 0, l - 1 ) ;
					}

					string[] tCode = tLine[ i ].Split( '\t' ) ;

					if( tCode.Length == 5 )
					{
						tNode =  new AssetBundleInfo( tCode[ 0 ], tCode[ 1 ], int.Parse( tCode[ 2 ] ), uint.Parse( tCode[ 3 ] ), long.Parse( tCode[ 4 ] ) ) ;
						tInfo.Add( tNode ) ;
						if( tLink.ContainsKey( tCode[ 0 ] ) == false )
						{
							tLink.Add( tCode[ 0 ], tNode ) ;
						}
					}
				}

				//----------------------------------------------
					
				// まずはリモートから見て更新の必要の無いものをチェックする
				string tAssetBundleName ;
				int tSize ;

				for( i  = 0 ; i <  assetBundleInfo.Count ; i ++ )
				{
					tAssetBundleName = assetBundleInfo[ i ].path ;
					if( tLink.ContainsKey( tAssetBundleName ) == true )
					{
						// 既に記録した事がある
						tNode =  tLink[ tAssetBundleName ] ;

						if( assetBundleInfo[ i ].hash == tNode.hash && assetBundleInfo[ i ].size == tNode.size )
						{
							// ハッシュは同じである

							// 実体の存在する場所
							tSize = StorageAccessor_GetSize( tName + "/" + tAssetBundleName ) ;
							if( tSize >  0 )
							{
								// このファイルは更新しない
								assetBundleInfo[ i ].time = tNode.time ;
								assetBundleInfo[ i ].size = tSize ;		// ＣＲＣファイルでのサイズと同じになるはず(ＣＲＣファイルにもサイズを持たせる事：サイズ部分にＣＲＣ値がずれて入り半日潰すバグを発生させた事があった事を忘れるな)
								assetBundleInfo[ i ].update = false ;
							}
						}
					}
				}

				// つぎに参照が無くなったファイルを削除する
				for( i  = 0 ; i <  tInfo.Count ; i ++ )
				{
					tAssetBundleName = tInfo[ i ].path ;
					if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
					{
						// このファイルは参照が無くなる
						StorageAccessor_Remove( tName + "/" + tAssetBundleName ) ;
					}
				}

				// ファイルが存在しなくなったフォルダも削除する
				if( tInstance.m_SecretPathEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( tName + "/" ) ;
				}
			}

			/// <summary>
			/// 最新のマニフェスト情報をローカルストレージに保存する
			/// </summary>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			public bool Save( AssetBundleManager tInstance )
			{
				string tName = name ;

				if( string.IsNullOrEmpty( tName ) == true )
				{
					// File Path に異常がある
					return false ;
				}

				string tPath = tName + "/" + tName + ".manifest" ;

				string tText = "" ;

				int i, l = assetBundleInfo.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tText = tText + assetBundleInfo[ i ].path + "\t" ;
					tText = tText + assetBundleInfo[ i ].hash + "\t" ;
					tText = tText + assetBundleInfo[ i ].size + "\t" ;
					tText = tText + assetBundleInfo[ i ].crc  + "\t" ;
					tText = tText + assetBundleInfo[ i ].time + "\n" ;
				}

				return StorageAccessor_SaveText( tPath, tText, true ) ;
			}

			//----------------------------------------------------------------------

			// ローカルに存在しない場合にリモートから取得しローカルに保存する
			private IEnumerator LoadAssetBundleFromRemote_Coroutine( AssetBundleInfo tAssetBundleInfo, string tAssetBundleLocalPath, bool tKeepChange, bool tKeep, int[] rResultCode, Request tRequest, AssetBundleManager tInstance )
			{
				string tAssetBundleRemotePath ;
				byte[] tData = null ;

				int t ;
				for( t  = 0 ; t <  2 ; t ++ )
				{
					if( tData == null )
					{ 
						if( ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( tInstance.m_UseStreamingAssets == true )
							{
								// StreamingAssets からダウンロードを試みる
								tAssetBundleRemotePath = streamingAssetsPath + "/" + tAssetBundleInfo.path ;

								byte[][] rData = { null } ;

								// ストリーミングアセットから読み出してみる
								yield return tInstance.StartCoroutine( StorageAccessor.LoadFromStreamingAssets( tAssetBundleRemotePath, rData ) ) ;

								if( rData[ 0 ] != null && rData[ 0 ].Length >  0 )
								{
									tData = rData[ 0 ] ;

									if( tRequest != null )
									{
										tRequest.Progress = 1.0f ;
									}
								}
							}
						}
						else
						if( ( t == 1 && tInstance.m_LoadPriorityType == LoadPriority.Local ) || ( t == 0 && tInstance.m_LoadPriorityType == LoadPriority.Remote ) )
						{
							if( tInstance.m_UseDownload == true )
							{
								// ネットワークからダウンロードを試みる
								tAssetBundleRemotePath = path + "/" +  tAssetBundleInfo.path + "?time=" + GetClientTime() ;

								// DownloadHandler を自作しないならスタティックメソッドを使うこと(.downloadHandler が null になってしまうため)
								UnityWebRequest www = UnityWebRequest.Get( tAssetBundleRemotePath ) ; ;
	//							www.downloadHandler = new DownloadHandlerBuffer() ;
								www.SendWebRequest() ;

								while( true )
								{
									Progress = www.downloadProgress ;

									if( tRequest != null )
									{
										tRequest.Progress = Progress ;
									}

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
									tData = www.downloadHandler.data ;
								}

								www.Dispose() ;
								www = null ;
							}
						}
					}
					if( tData != null )
					{
						break ;
					}
				}

				//-------------------------------------------------

				if( tData == null )
				{
					// 失敗
					Error = "Could not load data" ;

					if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
					if( tRequest != null )
					{
						tRequest.ResultCode	= 1 ;
						tRequest.Error	= Error ;    // 失敗
					}

					yield break ;
				}

				if( tAssetBundleInfo.crc != 0 )
				{
					// ＣＲＣのチェックが必要
					uint tCRC = GetCRC32( tData ) ;
					if( tCRC != tAssetBundleInfo.crc )
					{
						Error = "Bad CRC" ;

						if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 1 ; }
						if( tRequest != null )
						{
							tRequest.ResultCode	= 1 ;
							tRequest.Error = Error ;    // 失敗
						}

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
				if( CacheSize <= 0 || ( CacheSize >  0 && Cleanup( tData.Length, tInstance ) == true ) )
				{
					// 空き領域が確保出来た
					// ファイルを保存しアセットバンドルインフォを更新する
					if( StorageAccessor_Save( tAssetBundleLocalPath, tData, true ) == true )
					{
						tAssetBundleInfo.time = GetClientTime() ;	// タイムスタンプ更新
						tAssetBundleInfo.size = tData.Length ;		// ＣＲＣファイルが存在しない場合はここで初めてサイズが書き込まれる

						if( tKeepChange == true )
						{
							tAssetBundleInfo.keep = tKeep ;
						}

						tAssetBundleInfo.update = false ;

						// マニフェストをセーブする
						Save( tInstance ) ;
					}
					else
					{
						// 保存出来ない
						Error = "Could not save" ;
	
						if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 3 ; }
						if( tRequest != null )
						{
							tRequest.ResultCode	= 3 ;
							tRequest.Error	= Error ;	// 失敗
						}
					}
				}
				else
				{
					// 空き領域が確保出来ない
					Error = "Could not alocate space" ;

					if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 3 ; }
					if( tRequest != null )
					{
						tRequest.ResultCode	= 3 ;
						tRequest.Error	= Error ;    // 失敗
					}
				}

				m_Busy = false ;
			}

			//---------------------------------------------------------

			// ManifestInfo :: Asset
						
			/// <summary>
			/// アセットを取得する(同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>アセットに含まれる任意のコンポーネントのインスタンス</returns>
			internal protected UnityEngine.Object LoadAsset( string tAssetBundleName, string tAssetName, Type tType, bool tAssetBundleCaching, AssetBundleManager tInstance )
			{
				AssetBundle tAssetBundle = LoadAssetBundle( tAssetBundleName, tAssetBundleCaching, tInstance ) ;
				if( tAssetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// アセットのロード
				UnityEngine.Object tAsset = tAssetBundleInfo.LoadAsset( tAssetBundle, tAssetBundleName, tAssetName, tType ) ;

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAsset アセットバンドルを破棄する: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
#endif

					// どうもアセットのロードは非同期で行われているらしく、その最中に破棄を実行するとエラーになってしまう。基本的に同期ロードは使えないということか。
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
				}


				return tAsset ;
			}
			
			/// <summary>
			/// アセットを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <typeparam name="T">任意のコンポーネント型</typeparam>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tAssetName">アセット名</param>
			/// <param name="rAsset">アセットに含まれる任意のコンポーネントのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAsset_Coroutine( string tAssetBundleName, string tAssetName, Type tType, UnityEngine.Object[] rAsset, bool tKeep, int[] rResultCode, Request tRequest, bool tAssetBundleCaching, AssetBundleManager tInstance )
			{
				AssetBundle tAssetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				AssetBundle[] rAssetBundle = { null } ;
				yield return tInstance.StartCoroutine( LoadAssetBundle_Coroutine( tAssetBundleName, rAssetBundle, tKeep, rResultCode, tRequest, tAssetBundleCaching, tInstance ) ) ;
				tAssetBundle = rAssetBundle[ 0 ] ;

				if( tAssetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				UnityEngine.Object[] rAssetHolder = { null } ;

				// アセットバンドルが読み出せた(あと一息)
/*				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
				{
					// 非同期(現状使用できない)
					Debug.LogWarning( "非同期" ) ;
					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAssetHolder, tRequest, tInstance ) ) ;
				}
				else
				{*/
					// 同期
					rAssetHolder[ 0 ] = tAssetBundleInfo.LoadAsset( tAssetBundle, tAssetBundleName, tAssetName, tType ) ;
/*				}*/

				if( rAsset != null && rAsset.Length >  0 )
				{
					rAsset[ 0 ] = rAssetHolder[ 0 ] ;
				}

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAsset_Coroutine アセットバンドルの自動破棄予約: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
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
			internal protected UnityEngine.Object LoadSubAsset( string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, bool tAssetBundleCaching, AssetBundleManager tInstance, string tResourcePath )
			{
				AssetBundle tAssetBundle = LoadAssetBundle( tAssetBundleName, tAssetBundleCaching, tInstance ) ;
				if( tAssetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// アセットのロード
				UnityEngine.Object tAsset = tAssetBundleInfo.LoadSubAsset( tAssetBundle, tAssetBundleName, tAssetName, tSubAssetName, tType, tInstance, tResourcePath ) ;

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadSubAsset アセットバンドルを破棄する: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
#endif
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
				}

				return tAsset ;
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
			internal protected IEnumerator LoadSubAsset_Coroutine( string tAssetBundleName, string tAssetName, string tSubAssetName, Type tType, UnityEngine.Object[] rSubAsset, bool tKeep, int[] rResultCode, Request tRequest, bool tAssetBundleCaching, AssetBundleManager tInstance, string tResourcePath )
			{
				AssetBundle tAssetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				AssetBundle[] rAssetBundle = { null } ;
				yield return tInstance.StartCoroutine( LoadAssetBundle_Coroutine( tAssetBundleName, rAssetBundle, tKeep, rResultCode, tRequest, tAssetBundleCaching, tInstance  ) ) ;
				tAssetBundle = rAssetBundle[ 0 ] ;

				if( tAssetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				UnityEngine.Object[] rSubAssetHolder = { null } ;

				// アセットバンドルが読み出せた(あと一息)
/*				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
				{
					// 非同期
					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadSubAsset_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tSubAssetName, tType, rSubAssetHolder, tRequest, tInstance, tResourcePath ) ) ;
				}
				else
				{*/
					// 同期
					rSubAssetHolder[ 0 ] = tAssetBundleInfo.LoadSubAsset( tAssetBundle, tAssetBundleName, tAssetName, tSubAssetName, tType, tInstance, tResourcePath ) ;
/*				}*/

				if( rSubAsset != null && rSubAsset.Length >  0 )
				{
					rSubAsset[ 0 ] = rSubAssetHolder[ 0 ] ;
				}

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadSubAsset_Coroutine アセットバンドルを破棄する: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
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
			internal protected List<UnityEngine.Object> LoadAllSubAssets( string tAssetBundleName, string tAssetName, Type tType, bool tAssetBundleCaching, AssetBundleManager tInstance, string tResourcePath )
			{
				AssetBundle tAssetBundle = LoadAssetBundle( tAssetBundleName, tAssetBundleCaching, tInstance ) ;
				if( tAssetBundle == null )
				{
					return null ;	// 失敗
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// アセットのロード
				List<UnityEngine.Object> tAllSubAssets = tAssetBundleInfo.LoadAllSubAssets( tAssetBundle, tAssetBundleName, tAssetName, tType, tInstance, tResourcePath ) ;

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets アセットバンドルを破棄する: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
#endif
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
				}

				return tAllSubAssets ;
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
			internal protected IEnumerator LoadAllSubAssets_Coroutine( string tAssetBundleName, string tAssetName, Type tType, List<UnityEngine.Object>[] rAllSubAssets, bool tKeep, int[] rResultCode, Request tRequest, bool tAssetBundleCaching, AssetBundleManager tInstance, string tResourcePath )
			{
				AssetBundle tAssetBundle = null ;
				
				// 必ず非同期ダウンロードを試みる(依存関係にあるアセットバンドルのロードも行う必要があるため)
				AssetBundle[] rAssetBundle = { null } ;
				yield return tInstance.StartCoroutine( LoadAssetBundle_Coroutine( tAssetBundleName, rAssetBundle, tKeep, rResultCode, tRequest, tAssetBundleCaching, tInstance ) ) ;
				tAssetBundle = rAssetBundle[ 0 ] ;

				if( tAssetBundle == null )
				{
					yield break ;	// アセットバンドルが展開出来ない
				}

				//---------------------------------------------------------

				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// アセットバンドルが読み出せた(あと一息)
				List<UnityEngine.Object> tAllSubAssets = null ;

/*				if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
				{
					// 非同期
					List<UnityEngine.Object>[] rAllSubAssetsHolder = { null } ;
					yield return tInstance.StartCoroutine( tAssetBundleInfo.LoadAllSubAssets_Coroutine( tAssetBundle, tAssetBundleName, tAssetName, tType, rAllSubAssetsHolder, tRequest, tInstance, tResourcePath ) ) ;
					tAllSubAssets = rAllSubAssetsHolder[ 0 ] ;
				}
				else
				{*/
					// 同期
					tAllSubAssets = tAssetBundleInfo.LoadAllSubAssets( tAssetBundle, tAssetBundleName, tAssetName,tType, tInstance, tResourcePath ) ;
/*				}*/

				if( rAllSubAssets != null && rAllSubAssets.Length >  0 )
				{
					rAllSubAssets[ 0 ] = tAllSubAssets ;
				}

				if( tAssetBundleCaching == false && assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュにためない場合はアセットパンドルのインスタンスは破棄する
#if UNITY_EDITOR
					// デバッグログを出力するとなぜか UnityEditor 上でのエラーが出なくなる
					Debug.LogWarning( "----- LoadAllSubAssets_Coroutine アセットバンドルを破棄する: [ " + tAssetBundleName + " ] ( " + tAssetName + " : " + tType + " )" ) ;
//					yield return null ;	// １フレームおくとUnityEditor上でのエラーが出なくなる
#endif
//					tAssetBundle.Unload( false ) ;
					tInstance.AddAutoCleaningTarget( tAssetBundle ) ;
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
			internal protected AssetBundle LoadAssetBundle( string tAssetBundleName, bool tAssetBundleCaching, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return null ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( tAssetBundleInfo.update == true )
				{
					// キャッシュから削除する
					RemoveAssetBundleCache( tAssetBundleName ) ;

					// 更新対象になっているので取得不可
					return null ;
				}
				
				//-------------------------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] tDependentAssetBundleNames = m_Manifest.GetAllDependencies( tAssetBundleName ) ;
					if( tDependentAssetBundleNames != null && tDependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

						Debug.LogWarning( "同期:依存するアセットバンドルが存在する: [ " + tDependentAssetBundleNames.Length + " ] <- " + tAssetBundleName ) ;

						string			tDependentAssetBundleName ;
						AssetBundleInfo	tDependentAssetBundleInfo = null ;
						AssetBundle		tDependentAssetBundle = null ;

						int i, l = tDependentAssetBundleNames.Length ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							tDependentAssetBundleName = tDependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

							Debug.LogWarning( "同期:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							tDependentAssetBundleInfo = assetBundleLink[ tDependentAssetBundleName ] ;
							if( tDependentAssetBundleInfo.update == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCache( tDependentAssetBundleName ) ;

//								Debug.LogWarning( "同期:依存するアセットバンドルに更新が必要なものがある:" + tDependentAssetBundleName ) ;

								// １つでも依存アセットバンドルが欠けていたら対象のアセットバンドルも取得出来ない(非同期で取得せよ)
								return null ;
							}
						}

						//-------------------------------

						for( i  = 0 ; i <  l ; i ++ )
						{
							tDependentAssetBundleName = tDependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

//							Debug.LogWarning( "同期:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							tDependentAssetBundleInfo = assetBundleLink[ tDependentAssetBundleName ] ;

							// 全て更新対象ではないはず

							// キャッシュに存在するか確認する
							if( assetBundleCache.ContainsKey( tDependentAssetBundleName ) == false )
							{
								// キャッシュに存在しない
								tDependentAssetBundle = StorageAccessor_LoadAssetBundle( name + "/" + tDependentAssetBundleInfo.path ) ;
								if( tDependentAssetBundle != null )
								{
									// キャッシュにためる
									AddAssetBundleCache( tDependentAssetBundleName, tDependentAssetBundle, tInstance ) ;
								}
							}
						}
					}
				}

				//-------------------------------------------------------------
				
				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				AssetBundle tAssetBundle = null ;
				
				if( assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュには存在しない
					tAssetBundle = StorageAccessor_LoadAssetBundle( name + "/" + tAssetBundleInfo.path ) ;
					if( tAssetBundle == null )
					{
						return null ;	// アセットバンドルが展開出来ない
					}

					// キャッシュにためる
					if( tAssetBundleCaching == true )
					{
//						Debug.LogWarning( "アセットバンドルをキャッシュにためました:" + tAssetBundleName ) ;
						AddAssetBundleCache( tAssetBundleName, tAssetBundle, tInstance ) ;
					}
				}
				else
				{
					// キャッシュに存在する
					tAssetBundle = assetBundleCache[ tAssetBundleName ].assetBundle ;
				}

				// アセットバンドルのインスタンスを返す
				return tAssetBundle ;
			}

			/// <summary>
			/// アセットバンドルを取得する(非同期版)　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納するための要素数１以上の配列</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator LoadAssetBundle_Coroutine( string tAssetBundleName, AssetBundle[] rAssetBundle, bool tKeep, int[] rResultCode, Request tRequest, bool tAssetBundleCaching, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// 非同期で同じアセットバンドルにアクセスする場合は排他ロックがかかる
				if( tAssetBundleInfo.busy == true )
				{
					yield return new WaitWhile( () => tAssetBundleInfo.busy == true ) ;
				}

				tAssetBundleInfo.busy = true ;

				//------------------------------------------

				// ローカルのパス
				string tAssetBundleLocalPath = name + "/" + tAssetBundleInfo.path ;

				// このアセットバンドルが更新対象になっているか確認する
				if( tAssetBundleInfo.update == true )
				{
					// キャッシュから削除する
					RemoveAssetBundleCache( tAssetBundleName ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return tInstance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( tAssetBundleInfo, tAssetBundleLocalPath, true, tKeep, rResultCode, tRequest, tInstance ) ) ;

					if( tAssetBundleInfo.update == true )
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
					string[] tDependentAssetBundleNames = m_Manifest.GetAllDependencies( tAssetBundleName ) ;
					if( tDependentAssetBundleNames != null && tDependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

						Debug.LogWarning( "非同期:依存するアセットバンドルが存在する: [ " + tDependentAssetBundleNames.Length + " ] <- " + tAssetBundleName ) ;

						string			tDependentAssetBundleName ;
						AssetBundleInfo	tDependentAssetBundleInfo = null ;
						string			tDependentAssetBundleLocalPath ;
						AssetBundle[]	rDependentAssetBundleHolder = { null } ;

						int i, l = tDependentAssetBundleNames.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tDependentAssetBundleName = tDependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字化

							Debug.LogWarning( "非同期:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							tDependentAssetBundleInfo = assetBundleLink[ tDependentAssetBundleName ] ;

//							Debug.LogWarning( "非同期:ロック状態:" + tDependentAssetBundleName + " " + tDependentAssetBundleInfo.busy ) ;

							//------------------------------

							// 非同期で同じアセットバンドルにアクセスする場合は排他ロックがかかる
							if( tDependentAssetBundleInfo.busy == true )
							{
								yield return new WaitWhile( () => tDependentAssetBundleInfo.busy == true ) ;
							}
							
							tDependentAssetBundleInfo.busy = true ;

							//------------------------------

							tDependentAssetBundleLocalPath = name + "/" + tDependentAssetBundleInfo.path ;

//							Debug.LogWarning( "非同期:更新が必要か:" + tDependentAssetBundleName + " " + tDependentAssetBundleInfo.update ) ;

							if( tDependentAssetBundleInfo.update == true )
							{
								// キャッシュから削除する
								RemoveAssetBundleCache( tDependentAssetBundleName ) ;

								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードを試みる:" + tDependentAssetBundleName ) ;

								// 更新対象になっているのでダウンロードを試みる
								yield return tInstance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( tDependentAssetBundleInfo, tDependentAssetBundleLocalPath, true, tKeep, null, null, tInstance ) ) ;
							}

							if( tDependentAssetBundleInfo.update == false )
							{
								// 依存アセットバンドルのダウンロードに成功
								if( assetBundleCache.ContainsKey( tDependentAssetBundleName ) == false )
								{
									// キャッシュには存在しないのでロードする
/*									if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
									{
										// 非同期(低速)
										yield return tInstance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( tDependentAssetBundleLocalPath, rDependentAssetBundleHolder ) ) ;
									}
									else
									{*/
										// 同期(高速)
										rDependentAssetBundleHolder[ 0 ] = StorageAccessor_LoadAssetBundle( tDependentAssetBundleLocalPath ) ;
/*									}*/

									if( rDependentAssetBundleHolder[ 0 ] != null )
									{
										// キャッシュにためる
//										Debug.LogWarning( "キャッシュにためます:" + tDependentAssetBundleName ) ;
										AddAssetBundleCache( tDependentAssetBundleName, rDependentAssetBundleHolder[ 0 ], tInstance ) ;
									}
								}
							}

							// 排他ロック解除
							tDependentAssetBundleInfo.busy = false ;

							if( tDependentAssetBundleInfo.update == true )
							{
								Debug.LogWarning( "非同期:依存アセットバンドルのダウンロードに失敗した:" + tDependentAssetBundleName ) ;

								// 依存アセットバンドルのダウンロードに失敗
								if( rResultCode != null && rResultCode.Length >  0 ){ rResultCode[ 0 ] = 4 ; }
								if( tRequest != null ){ tRequest.Error = "Could not dependent load" ; tRequest.ResultCode = 4 ; }
								yield break ;
							}
						}
					}
				}

				//----------------------------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する

				AssetBundle[] rAssetBundleHolder = { null } ;

				if( assetBundleCache.ContainsKey( tAssetBundleName ) == false )
				{
					// キャッシュには存在しない
/*					if( tInstance.m_FastLoadEnabled == false || fastLoadEnabled == false )
					{
						// 非同期(低速)
						yield return tInstance.StartCoroutine( StorageAccessor_LoadAssetBundleAsync( tAssetBundleLocalPath, rAssetBundleHolder ) ) ;
					}
					else
					{*/
						// 同期(高速)
						rAssetBundleHolder[ 0 ] = StorageAccessor_LoadAssetBundle( tAssetBundleLocalPath ) ;
/*					}*/

					// キャッシュにためる
					if( tAssetBundleCaching == true )
					{
						AddAssetBundleCache( tAssetBundleName, rAssetBundleHolder[ 0 ], tInstance ) ;
					}
				}
				else
				{
					// キャッシュに存在する
					rAssetBundleHolder[ 0 ] = assetBundleCache[ tAssetBundleName ].assetBundle ;
				}
				
				//---------------------------------------------------------

				// 保存する
				if( rAssetBundle != null && rAssetBundle.Length >  0 )
				{
					rAssetBundle[ 0 ] = rAssetBundleHolder[ 0  ] ;
				}

				//---------------------------------------------------------

				// 排他ロック解除
				tAssetBundleInfo.busy = false ;
			}
			
			/// <summary>
			/// アセットバンドルのダウンロードを行う　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected IEnumerator DownloadAssetBundle_Coroutine( string tAssetBundleName, bool[] rResult, bool tKeep, int[] rResultCode, Request tRequest, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					yield break ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// ローカルのパス
				string tAssetBundleLocalPath = name + "/" + tAssetBundleInfo.path ;

				// このアセットバンドルが更新対象になっているか確認する
				if( tAssetBundleInfo.update == true )
				{
					RemoveAssetBundleCache( tAssetBundleName ) ;

					// 更新対象になっているのでダウンロードを試みる
					yield return tInstance.StartCoroutine( LoadAssetBundleFromRemote_Coroutine( tAssetBundleInfo, tAssetBundleLocalPath, true, tKeep, rResultCode, tRequest, tInstance ) ) ;

					if( tAssetBundleInfo.update == true )
					{
						// ダウンロード失敗
						yield break ;
					}
				}

				// 成功
				if( rResult != null && rResult.Length >  0 )
				{
					rResult[ 0 ] = true ;
				}
			}

			//----------------------------------------------------------

			/// <summary>
			/// アセットバンドルをキャッシュから削除する　※呼び出し非推奨
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>列挙子</returns>
			internal protected bool RemoveAssetBundle( string tAssetBundleName, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// ローカルのパス
				string tAssetBundleLocalPath = name + "/" + tAssetBundleInfo.path ;
				
				// 削除した
				StorageAccessor_Remove( tAssetBundleLocalPath ) ;

				// ファイルが存在しなくなったフォルダも削除する
				if( tInstance.m_SecretPathEnabled == false )
				{
					StorageAccessor_RemoveAllEmptyFolders( name + "/" ) ;
				}

				// 更新必要フラグをオンにする
				tAssetBundleInfo.update = true ;

				return true ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットバンドルの保有を確認する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=存在する・false=存在しない</returns>
			internal protected bool Contains( string tAssetBundleName, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
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
			internal protected bool Exists( string tAssetBundleName, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				// このアセットバンドルが更新対象になっているか確認する
				if( tAssetBundleInfo.update == true )
				{
					// 更新対象になっているので取得不可
					return false ;
				}
				
				//------------------------------------------

				// このアセットバンドルが依存している他のアセットバンドルの情報を取得する

				if( m_Manifest != null )
				{
					// レガシータイプの場合はマニフェストが存在しないので null チェックはきちんと行う必要がある
					string[] tDependentAssetBundleNames = m_Manifest.GetAllDependencies( tAssetBundleName ) ;
					if( tDependentAssetBundleNames != null && tDependentAssetBundleNames.Length >  0 )
					{
						// 依存するものが存在する

//						Debug.LogWarning( "存在確認:依存するアセットバンドルが存在する: [ " + tDependentAssetBundleNames.Length + " ] <- " + tAssetBundleName ) ;

						string			tDependentAssetBundleName ;
						AssetBundleInfo	tDependentAssetBundleInfo = null ;

						int i, l = tDependentAssetBundleNames.Length ;

						for( i  = 0 ; i <  l ; i ++ )
						{
							tDependentAssetBundleName = tDependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

//							Debug.LogWarning( "存在確認:依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

							tDependentAssetBundleInfo = assetBundleLink[ tDependentAssetBundleName ] ;
							if( tDependentAssetBundleInfo.update == true )
							{
								// 更新対象であるため取得不可
								RemoveAssetBundleCache( tDependentAssetBundleName ) ;

								Debug.LogWarning( "----- ※存在:依存するアセットバンドルに更新が必要なものがある:" + tDependentAssetBundleName + " <- " + tAssetBundleName ) ;

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
			internal protected int GetSize( string tAssetBundleName, AssetBundleManager tInstance )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return 0 ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				//------------------------------------------

				// ここに来るということは既にローカルに最新のアセットバンドルが保存されている事を意味する
				return tAssetBundleInfo.size ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// アセットパンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllAssetBundlePaths( bool tNeedUpdateOnly = true )
			{
				List<string> tPath = new List<string>() ;

				int i, l = assetBundleInfo.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tNeedUpdateOnly == false || ( tNeedUpdateOnly == true && assetBundleInfo[ i ].update == true ) )
					{
						tPath.Add( assetBundleInfo[ i ].path ) ;
					}
				}

				return tPath.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 依存関係にあるアセットバンドルのパス一覧を取得する
			/// </summary>
			/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
			/// <returns>アセットバンドルのパス一覧</returns>
			internal protected string[] GetAllDependentAssetBundlePaths( string tAssetBundleName, bool tNeedUpdateOnly = true )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				string[] tDependentAssetBundleNames = m_Manifest.GetAllDependencies( tAssetBundleName ) ;

				if( tDependentAssetBundleNames == null || tDependentAssetBundleNames.Length == 0 )
				{
					return null ;	// 依存するアセットバンドル存在しない
				}

				if( tNeedUpdateOnly == false )
				{
					// そのまま返す
					return tDependentAssetBundleNames ;
				}

				//---------------------------------------------------------

				// 更新が必要なもののみ返す

				List<string> tList = new List<string>() ;

				string			tDependentAssetBundleName ;
				AssetBundleInfo	tDependentAssetBundleInfo = null ;

				int i, l = tDependentAssetBundleNames.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tDependentAssetBundleName = tDependentAssetBundleNames[ i ].ToLower() ;	// 保険で小文字か化

//					Debug.LogWarning( "依存するアセットバンドル名:" + tDependentAssetBundleName ) ;

					tDependentAssetBundleInfo = assetBundleLink[ tDependentAssetBundleName ] ;
					if( tDependentAssetBundleInfo.update == true )
					{
						// 更新対象であるため取得不可
						tList.Add( tDependentAssetBundleNames[ i ] ) ;
					}
				}

				if( tList.Count == 0 )
				{
					return null ;
				}

				return tList.ToArray() ;
			}

			//--------------------------------------------------------------

			/// <summary>
			/// 指定のアセットバンドルのキャッシュ内での動作を設定する
			/// </summary>
			/// <param name="tAssetBundleName">アセットバンドル名</param>
			/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
			/// <returns>結果(true=成功・失敗)</returns>
			public bool SetKeepFlag( string tAssetBundleName, bool tKeep )
			{
				// 全て小文字化
				tAssetBundleName = tAssetBundleName.ToLower() ;

				// そのファイルが更新対象か確認する
				if( assetBundleLink.ContainsKey( tAssetBundleName ) == false )
				{
					// 指定の名前のアセットバンドルは存在しない
					return false ;
				}

				//------------------------------------------

				// 指定の名前のアセットバンドルインフォは存在する

				// アセットバンドルインフォを取得する
				AssetBundleInfo tAssetBundleInfo = assetBundleLink[ tAssetBundleName ] ;

				tAssetBundleInfo.keep = tKeep ;

				return true ;
			}

			/// <summary>
			/// 破棄可能なアセットバンドルをタイムスタンプの古い順に破棄してキャッシュの空き容量を確保する
			/// </summary>
			/// <param name="tSize">キャッシュサイズ</param>
			/// <param name="tInstance">アセットバンドルマネージャのインスタンス</param>
			/// <returns>結果(true=成功・false=失敗)</returns>
			private bool Cleanup( int tSize, AssetBundleManager tInstance )
			{
				// キープ対象全てとキープ非対象でタイムスタンプの新しい順にサイズを足していきキャッシュの容量をオーバーするまで検査する
				List<AssetBundleInfo> tAssetBundleInfo = new List<AssetBundleInfo>() ;

				long tAvailableSize = CacheSize ;

				int i, l = assetBundleInfo.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( assetBundleInfo[ i ].update == false )
					{
						// 更新の必要の無い最新の状態のアセットバンドル
						if( assetBundleInfo[ i ].keep == true )
						{
							// 常時保持する必要のあるアセットパンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								tAvailableSize = tAvailableSize - assetBundleInfo[ i ].size ;
//							}
						}
						else
						{
							// 空き容量が足りなくなったら破棄してもよいアセットバンドル
//							if( assetBundleInfo[ i ].size >  0 )
//							{
								// 破棄可能なアセットバンドルの情報を追加する
								tAssetBundleInfo.Add( assetBundleInfo[ i ] ) ;
//							}
						}
					}
				}

				if( tAvailableSize <  tSize )
				{
					// 破棄しないアセットバンドルだけで既に空き容量が足りない
					return false ;
				}
				
				if( tAssetBundleInfo.Count == 0 )
				{
					// 空き容量は足りる
					return true ;
				}

				//--------------------------------

				// 破棄できるアセットバンドルで既にに実体を持っているものをタイムスタンプの新しいものの順にソートする
				tAssetBundleInfo.Sort( ( a, b ) => ( int )( a.time - b.time ) ) ;

				l = tAssetBundleInfo.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tAvailableSize = tAvailableSize - tAssetBundleInfo[ i ].size ;
					if( tAvailableSize <  tSize )
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

				string tManifestName = name ;

				// ここからアセットバンドルを破棄する
				for( i  = s ; i <  l ; i ++ )
				{
					// 一時的に削除するだけなので空になったフォルダまで削除する事はしない
					StorageAccessor_Remove( tManifestName + "/" + tAssetBundleInfo[ i ].path ) ;
					
					tAssetBundleInfo[ i ].time = 0L ;
//					tAssetBundleInfo[ i ].size = 0 ;

					tAssetBundleInfo[ i ].update = true ;
				}

				// マニフェスト情報を保存しておく
//				Save( tInstance ) ;	// この後の追加保存でマニフェスト情報を保存するのでここでは保存しない

				return true ;
			}
		}
	}
}
