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
		/// リクエスト待ちクラス
		/// </summary>
		public class Request : CustomYieldInstruction
		{
			public Request()
			{
			}

			public override bool keepWaiting
			{
				get
				{
					if( IsDone == false && Asset == null && AssetBundle == null && string.IsNullOrEmpty( Error ) == true )
					{
						return true ;    // 継続
					}
					return false ;   // 終了
				}
			}

			/// <summary>
			/// 通信が終了したかどうか
			/// </summary>
			public bool		IsDone = false ;

			//----------------------------------------------------------

			/// <summary>
			/// アセット
			/// </summary>
			public System.Object	Asset = null ;

			/// <summary>
			/// 指定の型でアセットを取得する(直接 .asset でも良い)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public T GetAsset<T>() where T : class
			{
				if( Asset == null )
				{
					return null ;
				}

				return Asset as T ;
			}

			public UnityEngine.Object[]	Assets = null ;

			/// <summary>
			/// 指定の型でインスタンスを取得する(直接 .instances でも良い)
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public T[] GetAssets<T>() where T : UnityEngine.Object
			{
				if( Assets == null )
				{
					return null ;
				}

				return Assets as T[] ;
			}

			/// <summary>
			/// アセットバンドル
			/// </summary>
			public AssetBundle	AssetBundle = null ;
			
			//----------------------------------------------------------

			/// <summary>
			/// リザルトコード
			/// </summary>
			public int		ResultCode = 0 ;

			/// <summary>
			/// エラーメッセージ
			/// </summary>
			public string	Error = "" ;

			/// <summary>
			/// ダウンロード状況
			/// </summary>
			public float	Progress = 0 ;

			/// <summary>
			/// ダウンロード済み対象サイズ
			/// </summary>
			public int		StoredDataSize = 0 ;

			/// <summary>
			/// ダウンロード対象サイズ
			/// </summary>
			public int		EntireDataSize = 0 ;

			/// <summary>
			/// ダウンロード済み対象数
			/// </summary>
			public int		StoredFileCount = 0 ;

			/// <summary>
			/// ダウンロード対象数
			/// </summary>
			public int		EntireFileCount = 0 ;


		}
	}
}
