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
		/// キャッシュタイプ
		/// </summary>
		public enum CachingType
		{
			None			= 0,	// キャッシュしない
			ResourceOnly	= 1,	// リソースのみキャッシャする
			AssetBundleOnly	= 2,	// アセットバンドルのみキャッシュする
			Same			= 3,	// リソース・アセットバンドルともにキャッシュする
		}

		// 注意：アセットバンドルから展開させるリソースのインスタンスについて
		//
		// パスは同じでも異なるアセットバンドルのインスタンスから展開された同じパスのリソースは、
		// 別のリソースとして扱われる(別のインスタンスとなる)
		// よってアセットバンドルの展開そのものと、
		// 展開されたアセットバンドルからのリソースの展開は、重々注意する必要がある。
		// (重複展開され、無駄にメモリを消費する事になる。その他にもバグの要因となる。)

		// None：展開されたリソースのインスタンスは別個
		// 　動作をきちんと理解していないと危険なタイプである。
		// 　同じ動作を２度行った際の展開されたリソースのインスタンスは別物になる。
		// 　そのシーンで１度しか使わないようなものに対してのみ使用すること。
		//
		// ResourceOnly：展開されたリソースのインスタンスは同一
		// 　展開されたリソースそのもののインスタンスは同一になるが、
		// 　リソース群が同じアセットバンドルに内包されている場合、
		// 　何度も無駄にアセットバンドルの展開が発生する事になる。
		//
		// AssetBundleOnly：展開されたリソースのインスタンスは同一
		// 　展開されたアセットバンドルのインスタンス内に、
		// 　展開されたリソースのインスタンス(実体はシステムリソースキャッシュにある)を
		// 　保持しているため、同じリソースであればリソースの再展開は行われず、
		// 　同一のリソースのリソースのインスタンスが返される。
		//
		// Same：展開されたリソースのインスタンスは同一
		// 　ResourceOnly とと同じく、リソースキャッシュに保存された、
		// 　展開されたリソースのインスタンスを返すので、
		// 　展開されたリソースのインスタンスは同一になる。

		//-----------------------------------------------------------

		// リソースキャッシュ
		private Dictionary<string,UnityEngine.Object> m_ResourceCache ;

		/// <summary>
		/// リソースキャッシュ
		/// </summary>
		internal protected Dictionary<string,UnityEngine.Object> resourceCache
		{
			get
			{
				return m_ResourceCache ;
			}
		}

		/// <summary>
		/// リソースキャッシュを有効にするかどうか(デフォルトは有効)
		/// </summary>
		public static bool resourceCacheEnabled
		{
			get
			{
				if( m_Instance == null )
				{
					return false ;
				}
				return m_Instance.resourceCacheEnabled_Private ;
			}
			set
			{
				if( m_Instance == null )
				{
					return ;
				}
				m_Instance.resourceCacheEnabled_Private = value ;
			}
		}
		
		private bool resourceCacheEnabled_Private
		{
			get
			{
				if( m_ResourceCache == null )
				{
					return false ;
				}
				return true ;
			}
			set
			{
				if( value == true )
				{
					if( m_ResourceCache == null )
					{
						m_ResourceCache = new Dictionary<string, UnityEngine.Object>() ;
					}
				}
				else
				{
					if( m_ResourceCache != null )
					{
						m_ResourceCache.Clear() ;
						m_ResourceCache  = null ;
					}
				}
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// リソースキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearResourceCache( bool tUnloadUnusedAssets )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return false ;
			}

			return m_Instance.ClearResourceCache_Private( tUnloadUnusedAssets ) ;
		}

		// リソースキャッシュをクリアする
		private bool ClearResourceCache_Private( bool tUnloadUnusedAssets )
		{
			// 各マニフェストのアセットバンドルキャッシュもクリアする
			int i, l = m_ManifestInfo.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m_ManifestInfo[ i ].ClearAssetBundleCache() ;
			}

			if( m_ResourceCache != null )
			{
#if UNITY_EDITOR
				l = m_ResourceCache.Count ;
				Debug.Log( "[AssetBundleManager] キャッシュからクリア対象となる展開済みリソース数 = " + l ) ;
#endif
				m_ResourceCache.Clear() ;
			}

			if( tUnloadUnusedAssets == true )
			{
				Resources.UnloadUnusedAssets() ;
				System.GC.Collect() ;
			}

			return true ;
		}

		/// <summary>
		/// ローカルストレージ内のアセットバンドルキャッシュをクリアする
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Cleanup()
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return false ;
			}

			return m_Instance.Cleanup_Private() ;
		}

		// ローカルストレージ内のアセットバンドルキャッシュをクリアする
		private  bool Cleanup_Private()
		{
			int i, j, l = m_ManifestInfo.Count, m ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m = m_ManifestInfo[ i ].assetBundleInfo.Count ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					// 全てのアセットバンドルのダウンロードし直しが必要になる
					m_ManifestInfo[ i ].assetBundleInfo[ j ].update = true ;
				}
			}

			return StorageAccessor_Remove( "", true ) ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 指定のアセットバンドルのキャッシュ内での動作を設定する
		/// </summary>
		/// <param name="tPath">アセットバンドルのパス</param>
		/// <param name="tKeep">キャッシュオーバー時の動作(true=キャッシュオーバー時に保持する・false=キャッシュオーバー時に破棄する)</param>
		/// <returns>結果(true=成功・失敗)</returns>
		public static bool SetKeepFlag( string tPath, bool tKeep )
		{
			if( m_Instance == null )
			{
				// インスタンスが生成されていない
				return false ;
			}

			return m_Instance.SetKeepFlag_Private( tPath, tKeep ) ;
		}

		// 指定のアセットバンドルのキャッシュ内での動作を設定する
		private bool SetKeepFlag_Private( string tPath, bool tKeep )
		{
			string tManifestName = "" ;
			string tAssetBundleName = "" ;
			string tAssetName = "" ;

			if( GetManifestNameAndAssetBundleName( tPath, out tManifestName, out tAssetBundleName, out tAssetName ) == false )
			{
				return false ;
			}

			//------------------------------------------------

			if( string.IsNullOrEmpty( tManifestName ) == false && string.IsNullOrEmpty( tAssetBundleName ) == false )
			{
				// マニフェスト名は絶対に必要
				if( m_ManifestHash.ContainsKey( tManifestName ) == true )
				{
					// 指定の名前のマニフェストインフォは存在する

					// マニフェストインフォを取得する
					ManifestInfo tManifestInfo = m_ManifestHash[ tManifestName ] ;

					// 以下はサブクラス内に移動しても良い
					return tManifestInfo.SetKeepFlag( tAssetBundleName, tKeep ) ;
				}
			}

			return false ;
		}
	}
}
