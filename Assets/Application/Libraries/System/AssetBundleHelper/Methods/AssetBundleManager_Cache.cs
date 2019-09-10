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
		public static bool ResourceCacheEnabled
		{
			get
			{
				return m_Instance == null ? false : m_Instance.ResourceCacheEnabled_Private ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.ResourceCacheEnabled_Private = value ;
				}
			}
		}
		
		private bool ResourceCacheEnabled_Private
		{
			get
			{
				return !( m_ResourceCache == null ) ;
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
		public static bool ClearResourceCache( bool unloadUnusedAssets )
		{
			return m_Instance == null ? false : m_Instance.ClearResourceCache_Private( unloadUnusedAssets ) ;
		}

		// リソースキャッシュをクリアする
		private bool ClearResourceCache_Private( bool unloadUnusedAssets )
		{
			if( m_ManifestInfo == null || m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					// 各マニフェストのアセットバンドルキャッシュもクリアする
					manifestInfo.ClearAssetBundleCache() ;
				}
			}

			if( m_ResourceCache != null )
			{
#if UNITY_EDITOR
				Debug.Log( "[AssetBundleManager] キャッシュからクリア対象となる展開済みリソース数 = " + m_ResourceCache.Count ) ;
#endif
				m_ResourceCache.Clear() ;
			}

			if( unloadUnusedAssets == true )
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
			return m_Instance == null ? false : m_Instance.Cleanup_Private() ;
		}

		// ローカルストレージ内のアセットバンドルキャッシュをクリアする
		private  bool Cleanup_Private()
		{
			if( m_ManifestInfo != null && m_ManifestInfo.Count >  0 )
			{
				foreach( var manifestInfo in m_ManifestInfo )
				{
					manifestInfo.SetAllUpdateRequired() ;	// 更新が必要扱いにする
				}
			}
			return StorageAccessor_Remove( string.Empty, true ) ;
		}

		//-------------------------------------------------------------------

	}
}
