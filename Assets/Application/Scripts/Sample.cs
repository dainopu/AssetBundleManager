using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;
using UnityEngine.AddressableAssets ;

using AssetBundleHelper ;

#if UNITY_EDITOR
using UnityEditor ;
#endif

using uGUIHelper ;

namespace AssetableExperiment
{
	public class Sample : MonoBehaviour
	{
		[SerializeField]
		protected UIImage[] m_Image = new UIImage[ 2 ] ;

		private bool m_FE ;
		private int m_FC ;

		void Awake()
		{
			// 描画フレームレートの設定（３０）
			Application.targetFrameRate = 30 ;

			// 処理フレームレートの設定（３０）
			Time.fixedDeltaTime = 0.0333f ;
		}

		IEnumerator Start()
		{
			// フレームレート指定
			yield return StartCoroutine( AssetBundleStartup() ) ;
		}
		
		/// <summary>
		/// アセットバンドルのテスト
		/// </summary>
		/// <returns></returns>
		IEnumerator AssetBundleStartup()
		{
			// 汎用アセットバンドルマネージャの生成
			if( AssetBundleManager.Instance == null )
			{
				// マネージャのインスタンスを生成する
				AssetBundleManager.Create( transform ) ;

				// マニフェストをダウンロードする
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				AssetBundleManager.SecretPathEnabled = false ;  // アセットバンドルをローカルストレージに保存いる際にファイル名を暗号化する	
#else
				AssetBundleManager.SecretPathEnabled = true ;  // アセットバンドルをローカルストレージに保存いる際にファイル名を暗号化する	
#endif
	
//				AssetBundleManager.localPriority = AssetBundleManager.LocalPriority.High ; // 優先 Resources > StreamingAssets > AssetBundle
				AssetBundleManager.LoadPriorityType = AssetBundleManager.LoadPriority.Local ;	// 優先 StreamingAssets > Resources > AssetBundle
				Debug.LogWarning( "[注意]各種アセットは StreamingAssets を優先的に使用します" ) ;
				AssetBundleManager.UseStreamingAssets = true ;	// ネットワーク上のアセットバンドルが見つからない場合は StreamingAssets から探す

				AssetBundleManager.FastLoadEnabled = false ;	// 一部同期化で高速化読み出し

				// 実際はマスターの通信が完了してからそちらから取得する
//				string tDomainName = "http://vms010.ibrains.co.jp/ibrains/moe/" ;
				string domainName = "http://localhost/Sample/" ;


				// マニフェストを登録
				AssetBundleManager.AddManifest( domainName + Define.AssetBundlePlatformName + "/" + Define.AssetBundlePlatformName ) ;

				// デフォルトマニフェスト名を登録する
				AssetBundleManager.DefaultManifestName = Define.AssetBundlePlatformName ;

				// 登録された全てのマニフェストのダウンロード
				yield return AssetBundleManager.LoadAllManifestsAsync() ;

				// 全ての Manifest がダウンロードされるのを待つ
				while( AssetBundleManager.IsAllManifestsCompleted == false )
				{
					if( AssetBundleManager.GetAnyManifestError( out string manifestName, out string manifestError ) == true )
					{
#if UNITY_EDITOR
						Debug.LogError( "マニフェストのロードでエラーが発生しました:" + manifestName + " -> " + manifestError );
#endif
						break;
					}

					yield return null ;
				}
				
				// 各マニフェストのキャッシュサイズを設定するサンプル
				AssetBundleManager.ManifestInfo m = AssetBundleManager.GetManifest( Define.AssetBundlePlatformName ) ;
				if( m != null )
				{
					m.CacheSize = 1024 * 1024 * 1024 ;	// キャッシュサイズを１ＧＢに設定

					string sizeName = "" ;
					long size = m.CacheSize ;
					if( size <  1024L )
					{
						sizeName = size + " byte" ;
					}
					else
					if( size <  ( 1024L * 1024L ) )
					{
						sizeName = ( size / 1024L ) + " KB" ;
					}
					else
					if( size <  ( 1024L * 1024L * 1024L ) )
					{
						sizeName = ( size / ( 1024L * 1024L ) ) + " MB" ;
					}
					else
					if( size <  ( 1024L * 1024L * 1024L * 1024L ) )
					{
						sizeName = ( size / ( 1024L * 1024L * 1024L ) ) + "GB" ;
					}
					Debug.LogWarning( "マニフェスト " + m.ManifestName +" のキャッシュサイズを " + sizeName + " に制限しました。" ) ;
				}
			}

			//-------------------------------------------------

			m_FE = true ;
			m_FC = 0 ;
			float ft = Time.realtimeSinceStartup ;

			//-----------------

			string assetBundlePath = "Textures/button" ;
			string[] subAssetName = { "button_0", "button_1" } ;
			
			string atlasAssetBundlePath = "Textures/Atlas" ;

			Sprite[] sprite = new Sprite[ 2 ] ;

			int category = 3 ;
			int type = 1 ;

			AssetBundleManager.UseResources = AssetBundleManager.UserResources.None ;       // ネットワーク上のアセットバンドルが見つからない場合は Resources から探す
			AssetBundleManager.UseLocalAsset = false ;

			if( category == 0 )
			{
				// Asset
				if( type == 0 )
				{
					// Async(? ms)
	//				var request = AssetBundleManager.LoadAssetAsync<Sprite>( path ) ;
	//				yield return request ;
	//				sprite = request.Asset as Sprite ;

					yield return AssetBundleManager.LoadAssetAsync<Sprite>( assetBundlePath, ( _ ) => { sprite[ 0 ] = _ ; } ) ;
				}
				else
				if( type == 1 )
				{
					// Sync(仮 : 1.5～2.0 ms|実 : 6.5～7.0 ms)
	//				if( AssetBundleManager.Exists( path ) == false )
	//				{
						sprite[ 0 ] = AssetBundleManager.LoadAsset<Sprite>( assetBundlePath ) ;
	//				}
				}
				else
				if( type == 2 )
				{
					// Local Asset - Simple(1.5～2.0 ms)
					sprite[ 0 ] = Asset.Load<Sprite>( assetBundlePath, "png" ) ;
				}
				else
				if( type == 3 )
				{
					// Resources(0.5 ms)
					sprite[ 0 ] = Resources.Load<Sprite>( assetBundlePath ) ;
				}
			}
			else
			if( category == 1 )
			{
				// SubAsset
				if( type == 0 )
				{
					// Async(? ms)
	//				var request = AssetBundleManager.LoadAssetAsync<Sprite>( path ) ;
	//				yield return request ;
	//				sprite = request.Asset as Sprite ;
					
					yield return AssetBundleManager.LoadSubAssetAsync<Sprite>( assetBundlePath, subAssetName[ 0 ], ( _ ) => { sprite[ 0 ] = _ ; } ) ;
				}
				else
				if( type == 1 )
				{
					// Sync(仮 : 1.5～2.0 ms|実 : 6.5～7.0 ms)
	//				if( AssetBundleManager.Exists( path ) == false )
	//				{
						sprite[ 0 ] = AssetBundleManager.LoadSubAsset<Sprite>( assetBundlePath, subAssetName[ 0 ] ) ;
	//				}
				}
			}
			else
			if( category == 2 )
			{
				// AllSubAssets
				if( type == 0 )
				{
					// Async(? ms)
	//				var request = AssetBundleManager.LoadAssetAsync<Sprite>( path ) ;
	//				yield return request ;
	//				sprite = request.Asset as Sprite ;
					
					yield return AssetBundleManager.LoadSubAssetAsync<Sprite>( assetBundlePath, subAssetName[ 0 ], ( _ ) => { sprite[ 0 ] = _ ; }, AssetBundleManager.CachingType.Same ) ;
					yield return AssetBundleManager.LoadSubAssetAsync<Sprite>( assetBundlePath, subAssetName[ 1 ], ( _ ) => { sprite[ 1 ] = _ ; }, AssetBundleManager.CachingType.Same ) ;
				}
				else
				if( type == 1 )
				{
					// Sync(仮 : 1.5～2.0 ms|実 : 6.5～7.0 ms)
	//				if( AssetBundleManager.Exists( path ) == false )
	//				{
						sprite[ 0 ] = AssetBundleManager.LoadSubAsset<Sprite>( assetBundlePath, subAssetName[ 0 ], AssetBundleManager.CachingType.Same ) ;
						sprite[ 1 ] = AssetBundleManager.LoadSubAsset<Sprite>( assetBundlePath, subAssetName[ 1 ], AssetBundleManager.CachingType.Same ) ;
	//				}
				}
			}
			else
			if( category == 3 )
			{
				if( type == 0 )
				{
					// Async(? ms)
	//				var request = AssetBundleManager.LoadAssetAsync<Sprite>( path ) ;
	//				yield return request ;
	//				sprite = request.Asset as Sprite ;
					Sprite[] sprites = null ;
					yield return AssetBundleManager.LoadAllAssetsAsync<Sprite>( atlasAssetBundlePath, ( _ ) => { sprites = _ ; } ) ;

					if( sprites != null )
					{
						sprite[ 0 ] = sprites[ 0 ] ;
						sprite[ 1 ] = sprites[ 1 ] ;
					}
				}
				else
				if( type == 1 )
				{
					// Sync(仮 : 1.5～2.0 ms|実 : 6.5～7.0 ms)
	//				if( AssetBundleManager.Exists( path ) == false )
	//				{
						Sprite[] sprites = AssetBundleManager.LoadAllAssets<Sprite>( atlasAssetBundlePath ) ;
	//				}

					sprite[ 0 ] = sprites[ 0 ] ;
					sprite[ 1 ] = sprites[ 1 ] ;
				}

			}
			else
			if( category == 4 )
			{
				if( type == 0 )
				{
					// Async(? ms)
	//				var request = AssetBundleManager.LoadAssetAsync<Sprite>( path ) ;
	//				yield return request ;
	//				sprite = request.Asset as Sprite ;
					Sprite[] sprites = null ;
					yield return AssetBundleManager.LoadAllSubAssetsAsync<Sprite>( assetBundlePath, ( _ ) => { sprites = _ ; } ) ;

					if( sprites != null )
					{
						sprite[ 0 ] = sprites[ 0 ] ;
						sprite[ 1 ] = sprites[ 1 ] ;
					}
				}
				else
				if( type == 1 )
				{
					// Sync(仮 : 1.5～2.0 ms|実 : 6.5～7.0 ms)
	//				if( AssetBundleManager.Exists( path ) == false )
	//				{
						Sprite[] sprites = AssetBundleManager.LoadAllSubAssets<Sprite>( assetBundlePath ) ;
	//				}

					sprite[ 0 ] = sprites[ 0 ] ;
					sprite[ 1 ] = sprites[ 1 ] ;
				}

			}
			else
			if( category == 5 )
			{
				if( type == 0 )
				{
					yield return AssetBundleManager.AddSceneAsync( "scenes/OverlayScene" ) ;
				}
			}

			ft = Time.realtimeSinceStartup - ft ;
			Debug.LogWarning( "経過フレーム数:" + m_FC + " 経過タイム: " + ft ) ;

			if( sprite != null && m_Image[ 0 ] != null )
			{
				m_Image[ 0 ].sprite = sprite[ 0 ] ;
			}
			
			if( sprite[ 1 ] != null && m_Image[ 1 ] != null )
			{
				m_Image[ 1 ].sprite = sprite[ 1 ] ;
			}
			

			//-----------------

			//-------------------------------------------------

			yield break ;
		}

		void Update()
		{
			if( m_FE == true )
			{
				m_FC ++ ;
			}
		}
	}


	public class Asset
	{
		public const string BasePath = "Assets/Application/AssetBundle/" ;


		public static T Load<T>( string path, string extension ) where T : UnityEngine.Object
		{
#if UNITY_EDITOR
			return AssetDatabase.LoadAssetAtPath<T>( CombineExtension( CombinePath( BasePath, path ), extension ) ) as T ;
#else

			return default ;
#endif
		}

		private static string CombinePath( string a, string b )
		{
			if( string.IsNullOrEmpty( a ) == true )
			{
				return b ;
			}

			if( string.IsNullOrEmpty( b ) == true )
			{
				return a ;
			}

			if( a[ a.Length - 1 ] == '/' )
			{
				a = a.Substring( 0, a.Length - 1 ) ;
			}
			if( b[ 0 ] == '/' )
			{
				b = b.Substring( 1, b.Length - 1 ) ;
			}

			return a + "/" + b ;
		}

		private static string CombineExtension( string a, string b )
		{
			if( string.IsNullOrEmpty( a ) == true )
			{
				return b ;
			}

			if( string.IsNullOrEmpty( b ) == true )
			{
				return a ;
			}

			if( a[ a.Length - 1 ] == '.' )
			{
				a = a.Substring( 0, a.Length - 1 ) ;
			}
			if( b[ 0 ] == '.' )
			{
				b = b.Substring( 1, b.Length - 1 ) ;
			}

			return a + "." + b ;
		}
	}
}
