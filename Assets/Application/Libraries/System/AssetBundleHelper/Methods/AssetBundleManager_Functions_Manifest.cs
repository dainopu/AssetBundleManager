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
		/// 全てのマニフェストが使用可能になったかどうかを示す
		/// </summary>
		public static bool IsAllManifestsCompleted
		{
			get
			{
				return m_Instance == null ? false : m_Instance.IsAllManifestsCompleted_Private ;
			}
		}

		// 全てのマニフェストが使用可能になったかどうかを示す
		private bool IsAllManifestsCompleted_Private
		{
			get
			{
				return ! m_ManifestInfo.Any( _ => _.Completed == false ) ;
			}
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 全マニフェスト情報の高速アクセス用のハッシュリスト
		/// </summary>
		internal protected Dictionary<string,ManifestInfo> m_ManifestHash = new Dictionary<string, ManifestInfo>() ;

		// マニフェストハッシュを更新する
		private void UpdateManifestHash()
		{
			if( m_ManifestHash == null )
			{
				m_ManifestHash = new Dictionary<string, ManifestInfo>() ;
			}
			else
			{
				m_ManifestHash.Clear() ;
			}

			if( m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return ;
			}

			foreach( var manifestInfo in m_ManifestInfo )
			{
				if( m_ManifestHash.ContainsKey( manifestInfo.name ) == false )
				{
					m_ManifestHash.Add( manifestInfo.name, manifestInfo ) ;
				}
			}
		}

		//-------------------------------------------------------------------
		
		/// <summary>
		/// マニフェストがローディング中かどうかを示す
		/// </summary>
		public static bool IsAnyManifestLoading
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_IsAnyManifestLoading ;
			}
		}

		// マニフェストがローディング中かどうかを示す
		private bool m_IsAnyManifestLoading = false ;

		//-------------------------------------------------------------------

		// マニフェスト全体情報の更新時の排他制御用フラグ
		private bool m_Busy = false ;

		/// <summary>
		/// 指定のマニフェストが登録されているか確認する
		/// </summary>
		/// <param name="manifestName">マニフェスト名</param>
		/// <returns>マニフェストの登録リスト上でのインデックス番号(-1で未登録)</returns>
		public static int IsManifest( string manifestName )
		{
			return m_Instance == null ? -1 : m_Instance.IsManifest_Private( manifestName ) ;
		}

		// 指定のマニフェストが登録されているか確認する
		private int IsManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return -1 ;
			}

			return m_ManifestInfo.FindIndex( _ => _.name == manifestName ) ;
		}
		
		/// <summary>
		/// マニフェストを登録する
		/// </summary>
		/// <param name="tPath">マニフェストのパス</param>
		/// <param name="tSize">キャッシュサイズ(0で無制限)</param>
		/// <returns>マニフェストの登録リスト上でのインデックス番号(-1で登録失敗)</returns>
		public static ManifestInfo AddManifest( string filePath, long cacheSize = 0L )
		{
			return m_Instance == null ? null : m_Instance.AddManifest_Private( filePath, cacheSize ) ;
		}

		// マニフェストを登録する
		private ManifestInfo AddManifest_Private( string filePath, long cacheSize )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( filePath ) == true )
			{
				return null ;
			}

			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.FilePath == filePath ) ;
			if( manifestInfo == null )
			{
				// 未登録(新規に登録する)
				m_ManifestInfo.Add( new ManifestInfo( filePath, cacheSize ) ) ;
				UpdateManifestHash() ;
			}
			else
			{
				// 登録済(キャッシュサイズを更新する)
				manifestInfo.CacheSize	= cacheSize ;
			}

			return manifestInfo ;
		}
		
		/// <summary>
		/// マニフェストを取得する
		/// </summary>
		/// <param name="tName">マニフェスト名</param>
		/// <returns>マニフェストのインスタンス</returns>
		public static ManifestInfo GetManifest( string manifestName )
		{
			return m_Instance == null ? null : m_Instance.GetManifest_Private( manifestName ) ;
		}

		// マニフェストを取得する
		private ManifestInfo GetManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return null ;
			}

			// 大文字小文字の区別無しで取得する
			manifestName = manifestName.ToLower() ;
			return m_ManifestInfo.FirstOrDefault( _ => _.name.ToLower() == manifestName ) ;
		}		
		
		/// <summary>
		/// マニフェストを削除する
		/// </summary>
		/// <param name="tName">マニフェスト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool RemoveManifest( string manifestName )
		{
			return m_Instance == null ? false : m_Instance.RemoveManifest_Private( manifestName ) ;
		}

		// マニフェストを削除する
		private bool RemoveManifest_Private( string manifestName )
		{
			if( m_ManifestInfo == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return false ;
			}

			// 大文字小文字の区別無しで取得する
			// 大文字小文字の区別無しで取得する
			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.name.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			m_ManifestInfo.Remove( manifestInfo ) ;
			UpdateManifestHash() ;

			return true ;
		}

		/// <summary>
		/// 全マニフェストを削除する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool ClearManifest()
		{
			return m_Instance == null ? false : m_Instance.ClearManifest_Private() ;
		}

		// 全マニフェストを削除する
		private bool ClearManifest_Private()
		{
			if( m_ManifestInfo == null )
			{
				return false ;
			}

			foreach( var manifestInfo in m_ManifestInfo )
			{
				manifestInfo.ClearAssetBundleCache() ;
			}

			m_ManifestInfo.Clear() ;
			UpdateManifestHash() ;

			return true ;
		}

		/// <summary>
		/// マニフェストの展開でエラーが発生していれば取得する
		/// </summary>
		/// <param name="rName">エラーが発生したマニフェスト名</param>
		/// <param name="rError">エラーメッセージ</param>
		/// <returns>結果(true=エラーが発生した・false=エラーは発生していない)</returns>
		public static bool GetAnyManifestError( out string oManifestName, out string oError )
		{
			oManifestName	= null ;
			oError	= null ;

			return m_Instance == null ? false : m_Instance.GetAnyManifestError_Private( out oManifestName, out oError ) ;
		}
		
		// マニフェストの展開でエラーが発生していれば取得する
		private bool GetAnyManifestError_Private( out string oManifestName, out string oError )
		{
			oManifestName	= null ;
			oError			= null ;

			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => string.IsNullOrEmpty( _.Error ) == false ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			oManifestName	= manifestInfo.name ;
			oError			= manifestInfo.Error ;

			return true ;
		}

		//------------------------------------

		/// <summary>
		/// 登録されたマニフェストを全て展開する(同期版)
		/// </summary>
		public static Request LoadAllManifestsAsync()
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadAllManifestsAsync_Private( request ) ) ;
			return request ;
		}

		// 登録されたマニフェストを全て展開する(非同期版)
		private IEnumerator LoadAllManifestsAsync_Private( Request request )
		{
			m_IsAnyManifestLoading = true ;

			foreach( var manifestInfo in m_ManifestInfo )
			{
				Request subRequest = new Request() ;
				yield return StartCoroutine( LoadManifestAsync_Private( manifestInfo, subRequest, true ) ) ;
				if( string.IsNullOrEmpty( subRequest.Error ) == false )
				{
					request.Error = subRequest.Error ;
					break ;
				}
			}

			request.IsDone = true ;

			m_IsAnyManifestLoading = false ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// マニフェストを展開する(登録されていなければ登録する)(非同期版)
		/// </summary>
		/// <param name="tPath">マニフェストのパス</param>
		/// <param name="tSize">キャッシュサイズ(0で無制限)</param>
		/// <param name="rStatus">結果を格納するための要素数１以上の配列</param>
		/// <returns>列挙子</returns>
		public static Request LoadManifestAsync( string filePath, long cacheSize = 0L )
		{
			if( m_Instance == null )
			{
				return null ;
			}

			Request request = new Request() ;
			m_Instance.StartCoroutine( m_Instance.LoadManifestAsync_Private( filePath, cacheSize, request ) ) ;
			return request ;
		}

		// マニフェストを展開する(登録されていなければ登録する)(非同期版)
		private IEnumerator LoadManifestAsync_Private( string filePath, long cacheSize, Request request )
		{
			var manifestInfo = AddManifest_Private( filePath, cacheSize ) ;
			if( manifestInfo == null )
			{
				// 何故か失敗した(基本的にありえない)
				request.Error = "Could not load" ;
				request.IsDone = true ;
				yield break ;
			}

			yield return StartCoroutine( LoadManifestAsync_Private( manifestInfo, request, false ) ) ;
		}
		
		//-----------------------------------------------------------

		// マニフェストのダウンロードと展開を行う　※共通処理
		private IEnumerator LoadManifestAsync_Private( ManifestInfo manifestInfo, Request request, bool isAll )
		{
			if( isAll == false )
			{
				m_IsAnyManifestLoading = true ;
			}

			// 実際のダウンロードと展開を行う
			yield return StartCoroutine( manifestInfo.LoadAsync( request, this ) ) ;
			if( string.IsNullOrEmpty( request.Error ) == false )
			{
				if( isAll == false )
				{
					m_IsAnyManifestLoading = false ;
				}

				request.IsDone = true ;
				yield break ;
			}

			//----------------------------------------------------------

			// マニフェストリストのタイムスタンプを更新する(排他的にする)

			yield return new WaitWhile( () => m_Busy == true ) ;

			m_Busy = true ;

			AddOrUpdateManifestToSystemFile( manifestInfo.name, GetClientTime() ) ;
			SaveSystemFile() ;
			UpdateManifestHash() ;

			m_Busy = false ;

			if( isAll == false )
			{
				m_IsAnyManifestLoading = false ;
			}

			//----------------------------------------------------------

			request.IsDone = true ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="tManifestName">マニフェスト名</param>
		/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
		/// <returns>マニフェストのアセットバンドルのパス一覧</returns>
		public static string[] GetAllAssetBundlePaths( bool needUpdateOnly = false )
		{
			return m_Instance == null ? null : m_Instance.GetAllAssetBundlePaths_Private( m_Instance.m_DefaultManifestName, needUpdateOnly ) ;
		}
		
		/// <summary>
		/// マニフェストのアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="tManifestName">マニフェスト名</param>
		/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
		/// <returns>マニフェストのアセットバンドルのパス一覧</returns>
		public static string[] GetAllAssetBundlePaths( string manifestName, bool needUpdateOnly = false )
		{
			return m_Instance == null ? null : m_Instance.GetAllAssetBundlePaths_Private( manifestName, needUpdateOnly ) ;
		}

		// マニフェストのアセットパンドルのパス一覧を取得する
		private string[] GetAllAssetBundlePaths_Private( string manifestName, bool needUpdateOnly )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return null ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.name.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return null ;
			}

			// アセットバンドルのパス一覧を取得する
			return manifestInfo.GetAllAssetBundlePaths( needUpdateOnly ) ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 指定のマニフェストをローカルストレージに明示的に保存する
		/// </summary>
		/// <param name="tManifestName">マニフェスト名</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveManifestInfo( string manifestName )
		{
			return m_Instance == null ? false : m_Instance.SaveManifestInfo_Private( manifestName ) ;
		}

		// 指定のマニフェストをローカルストレージに明示的に保存する
		private bool SaveManifestInfo_Private( string manifestName )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return false ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.name.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return false ;
			}

			return manifestInfo.Save( this ) ;
		}

		/// <summary>
		/// 全てのマニフェストをローカルストレージに明示的に保存する
		/// </summary>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveAllManifestInfo()
		{
			return m_Instance == null ? false : m_Instance.SaveAllManifestInfo_Private() ;
		}

		// 全てのマニフェストをローカルストレージに明示的に保存する
		private bool SaveAllManifestInfo_Private()
		{
			if( m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return false ;
			}

			bool result = true ;
			foreach( var manifestInfo in m_ManifestInfo )
			{
				if( manifestInfo.Save( this ) == false )
				{
					result = false ;
				}
			}

			return result ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// 依存関係にあるアセットバンドルのパス一覧を取得する
		/// </summary>
		/// <param name="tManifestName">マニフェスト名</param>
		/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
		/// <returns>マニフェストのアセットバンドルのパス一覧</returns>
		public static string[] GetAllDependentAssetBundlePaths( string assetBundleName, bool needUpdateOnly = false )
		{
			return m_Instance == null ? null : m_Instance.GetAllDependentAssetBundlePaths_Private( m_Instance.m_DefaultManifestName, assetBundleName, needUpdateOnly ) ;
		}
		
		/// <summary>
		/// 依存関係にあるアセットパンドルのパス一覧を取得する
		/// </summary>
		/// <param name="tManifestName">マニフェスト名</param>
		/// <param name="tNeedUpdateOnly">更新が必要なものみに対象を限定するかどうか</param>
		/// <returns>マニフェストのアセットバンドルのパス一覧</returns>
		public static string[] GetAllDependentAssetBundlePaths( string manifestName, string assetBundleName, bool needUpdateOnly = false )
		{
			return m_Instance == null ? null : m_Instance.GetAllDependentAssetBundlePaths_Private( manifestName, assetBundleName, needUpdateOnly ) ;
		}

		// マニフェストのアセットパンドルのパス一覧を取得する
		private string[] GetAllDependentAssetBundlePaths_Private( string manifestName, string assetBundleName, bool needUpdateOnly )
		{
			if( string.IsNullOrEmpty( manifestName ) == true || m_ManifestInfo == null || m_ManifestInfo.Count == 0 )
			{
				return null ;
			}

			manifestName = manifestName.ToLower() ;
			var manifestInfo = m_ManifestInfo.FirstOrDefault( _ => _.name.ToLower() == manifestName ) ;
			if( manifestInfo == null )
			{
				return null ;
			}

			// アセットバンドルのパス一覧を取得する
			return manifestInfo.GetAllDependentAssetBundlePaths( assetBundleName, needUpdateOnly ) ;
		}
	}
}
