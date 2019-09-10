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
		public static string LocalAssetBundleRootPath
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_LocalAssetBundleRootPath ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LocalAssetBundleRootPath = value ;
				}
			}
		}

		[SerializeField,Header("アセットバンドルのルートパス")]
		private string m_LocalAssetBundleRootPath = "Assets/Application/AssetBundle/" ;


		/// <summary>
		/// ダウンロードしたアセットバンドルファイルを保存するルートフォルダ名
		/// </summary>
		public static string DataPath
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DataPath ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DataPath = value ;
				}
			}
		}

		[SerializeField,Header("ストレージ内の保存対象パス")]
		private string m_DataPath = "AssetBundleCache" ;

		/// <summary>
		/// マニフェスト全体管理リスト名
		/// </summary>
		public static string SystemFileName
		{
			get
			{
				return m_Instance == null ? String.Empty : m_Instance.m_SystemFileName ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_SystemFileName = value ;
				}
			}
		}

		[SerializeField,Header("システムファイル名")]
		private string m_SystemFileName = "SystemFile" ;

		/// <summary>
		/// マニフェストにアクセスが無い場合にどれくらいの期間保持するかの時間(秒)
		/// </summary>
		public static long ManifestKeepTime
		{
			get
			{
				return m_Instance == null ? 0L : m_Instance.m_ManifestKeepTime ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_ManifestKeepTime = value ;
				}
			}
		}

		[SerializeField,Header("マニフェストの保存時間(秒)")]
		private long m_ManifestKeepTime = 30L * 24L * 60L * 60L ;

		/// <summary>
		/// 保存されたアセットバンドルファイルのパスをハッシュ化して隠蔽するかどうか
		/// </summary>
		public static bool SecretPathEnabled
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_SecretPathEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_SecretPathEnabled = value ;
				}
			}
		}

		[SerializeField,Header("保存されたアセットバンドルのパスを暗号化するかどうか")]
		private bool m_SecretPathEnabled = false ;

		/// <summary>
		/// 非同期版のロードを行う際に通信以外処理を全て同期で行うかどうか(展開速度は上がるが別のコルーチンの呼び出し頻度が下がる)
		/// </summary>
		public static bool FastLoadEnabled
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_FastLoadEnabled ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_FastLoadEnabled = value ;
				}
			}
		}
		
		[SerializeField,Header("非同期ロード実行時に通信以外を全て同期で行うかどうか")]
		private bool m_FastLoadEnabled = true ;

		/// <summary>
		/// アセットバンドルマネージャ起動と同時にマニフェストをロードするかどうかを示す
		/// </summary>
		public static bool LoadManifestOnAwake
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_LoadManifestOnAwake ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LoadManifestOnAwake = value ;
				}
			}
		}
		
		[SerializeField,Header("AssetBundleManager起動と同時に自動的にマニフェストのダウンロードを行うかどうか")]
		private bool m_LoadManifestOnAwake = false ;

		/// <summary>
		/// ダウンロードを行うどうか(デバッグ用)
		/// </summary>
		public static bool UseLocalAsset
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_UseLocalAsset ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseLocalAsset = value ;
				}
			}
		}
		
		[SerializeField,Header("ローカルアセットを使用するかどうか(Eitor専用)")]
		private bool m_UseLocalAsset = true ;


		/// <summary>
		/// ダウンロードを行うどうか(デバッグ用)
		/// </summary>
		public static bool UseDownload
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_UseDownload ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseDownload = value ;
				}
			}
		}
		
		[SerializeField,Header("ダウンロードを実際に行うかどうか")]
		private bool m_UseDownload = true ;


		/// <summary>
		/// StreamingAssets を参照するかどうかを示す(デバッグ用)
		/// </summary>
		public static bool UseStreamingAssets
		{
			get
			{
				return m_Instance == null ? false : m_Instance.m_UseStreamingAssets ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseStreamingAssets = value ;
				}
			}
		}
		
		[SerializeField,Header("StreamingAssetsを参照するかどうか")]
		private bool m_UseStreamingAssets = true ;

		/// <summary>
		/// デバッグとしてのリソースの使用方法
		/// </summary>
		public enum UserResources
		{
			None = 0,
			SyncOnly = 1,
			AsyncOnly = 2,
			Same = 3,
		}

		/// <summary>
		/// Resources を参照するかどうかを示す(デバッグ用)
		/// </summary>
		public static UserResources UseResources
		{
			get
			{
				return m_Instance == null ?  UserResources.None : m_Instance.m_UseResources ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_UseResources = value ;
				}
			}
		}
		
		[SerializeField,Header("Resourcesを参照するかどうか")]
		private UserResources m_UseResources = UserResources.Same ;

		/// <summary>
		/// StreamingAssets および Resources のアセットバンドルに対する参照の優先順位
		/// </summary>
		public enum LoadPriority
		{
			Local			= -1,	// (Resources)Local -> (StreamingAssets)Remote
			Resources		= 0,	// Resources -> StreamingAssets -> Remote
			Remote			= 1,	// (StreamingAssets)Remote -> (Resources)Local
			StreamingAssets	= 2,	// StreamingAssets -> Remote -> Resources
		}

		/// <summary>
		/// StreamingAssets および Resources のアセットバンドルに対する参照の優先順位を示す
		/// </summary>
		public static LoadPriority LoadPriorityType
		{
			get
			{
				return m_Instance == null ? LoadPriority.Local : m_Instance.m_LoadPriorityType ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_LoadPriorityType = value ;
				}
			}
		}
		
		[SerializeField,Header("参照の優先順位")]
		private LoadPriority m_LoadPriorityType = LoadPriority.Resources ;

		/// <summary>
		/// デフォルトのマニフェスト(無指定の場合はパスの最初のフォルダ名がマニフェスト名とみなされる)
		/// </summary>
		public static string DefaultManifestName
		{
			get
			{
				return m_Instance == null ? string.Empty : m_Instance.m_DefaultManifestName ;
			}
			set
			{
				if( m_Instance != null )
				{
					m_Instance.m_DefaultManifestName = value ;
				}
			}
		}
		
		[SerializeField,Header("デフォルトのマニフェスト名(マニフェスト名省略時の対象)")]
		private string m_DefaultManifestName = "" ;

		//-----------------------------------------------------------------
		
		/// <summary>
		/// 全マニフェスト情報
		/// </summary>
		[SerializeField,Header("マニフェスト情報")]
		private List<ManifestInfo> m_ManifestInfo = new List<ManifestInfo>() ;

		//-----------------------------------------------------------
	}
}
