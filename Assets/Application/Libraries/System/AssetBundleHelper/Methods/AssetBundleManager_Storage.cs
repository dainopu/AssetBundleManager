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
		// ハッシュ生成インスタンス
		//		private static MD5CryptoServiceProvider mHashGenerator = new MD5CryptoServiceProvider() ;
		private static HMACSHA256 m_HashGenerator = new HMACSHA256( new byte[]{ 0, 1, 2, 3 } ) ;	// コンストラクタに適当なキー値を入れる事(でないと毎回ランダムになってしまう)

		// ハッシュコードを計算する
		private static string GetHash( string tName )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return "" ;
			}

			byte[] tData = System.Text.Encoding.UTF8.GetBytes( tName ) ;
			return GetHash( tData ) ;
		}

		// ハッシュコードを計算する
		private static string GetHash( byte[] tData )
		{
			byte[] tHash = m_HashGenerator.ComputeHash( tData ) ;

			int i, l = tHash.Length ;
			string r = "" ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				r = r + tHash[ i ].ToString( "x2" ) ;
			}

			return r ;
		}

		private static string GetFullPath( string tName )
		{
			string tPath = m_Instance.m_DataPath ;
			if( m_Instance.m_SecretPathEnabled == true )
			{
				tName = GetHash( tName ) ;
			}
			if( string.IsNullOrEmpty( tName ) == false )
			{
				tPath = tPath + "/" + tName ;
			}
			return tPath ;
		}

		private const string m_Key    = "lkirwf897+22#bbtrm8814z5qq=498j5" ;	// RM  用 32 byte
	
		// 初期化ベクタ
		private const string m_Vector = "741952hheeyy66#cs!9hjv887mxx7@8y" ;	// 16 byte

		// ローカルストレージからのテキストの読み出し
		private static string StorageAccessor_LoadText( string tName, string tKey = null, string tVector = null )
		{
			if( m_Instance != null )
			{
				if( m_Instance.m_SecretPathEnabled == true )
				{
					if( string.IsNullOrEmpty( tKey ) == true )
					{
						tKey	= m_Key ;
					}
					if( string.IsNullOrEmpty( tVector ) == true )
					{
						tVector	= m_Vector ;
					}
				}
			}

			return StorageAccessor.LoadText( GetFullPath( tName ), tKey, tVector ) ;
		}

		// ローカルストレージへテキストの書き込み
		private static bool StorageAccessor_SaveText( string tName, string tText, bool tMakeFolder = false, string tKey = null, string tVector = null )
		{
			if( m_Instance != null )
			{
				if( m_Instance.m_SecretPathEnabled == true )
				{
					if( string.IsNullOrEmpty( tKey ) == true )
					{
						tKey	= m_Key ;
					}
					if( string.IsNullOrEmpty( tVector ) == true )
					{
						tVector	= m_Vector ;
					}
				}
			}

			return StorageAccessor.SaveText( GetFullPath( tName ), tText, tMakeFolder, tKey, tVector ) ;
		}

		// ローカルストレージへのバイナリの書き込み
		private static bool StorageAccessor_Save( string tName, byte[] tData, bool tMakeFolder = false, string tKey = null, string tVector = null )
		{
			return StorageAccessor.Save( GetFullPath( tName ), tData, tMakeFolder, tKey, tVector ) ;
		}

		// ローカルストレージへのファイルの存在確認
		private static StorageAccessor.Target StorageAccessor_Exists( string tName )
		{
			return StorageAccessor.Exists( GetFullPath( tName ) ) ;
		}

		// ローカルストレージからのファイルサイズの取得
		private static int StorageAccessor_GetSize( string tName )
		{
			return StorageAccessor.GetSize( GetFullPath( tName ) ) ;
		}

		// ローカルストレージへのファイルの削除
		private static bool StorageAccessor_Remove( string tName, bool tAbsolute = false )
		{
			return StorageAccessor.Remove( GetFullPath( tName ), tAbsolute ) ;
		}

		// ローカルストレージでの空フォルダの削除
		private static void StorageAccessor_RemoveAllEmptyFolders( string tName = "" )
		{
			StorageAccessor.RemoveAllEmptyFolders( GetFullPath( tName ) ) ;
		}

		// ローカルストレージからのアセットバンドルの取得(同期版)
		private static AssetBundle StorageAccessor_LoadAssetBundle( string tName, string tKey = null, string tVector = null )
		{
			return StorageAccessor.LoadAssetBundle( GetFullPath( tName ), tKey, tVector ) ;
		}

		// ローカルストレージからのアセットバンドルの取得(非同期版)
		private static IEnumerator StorageAccessor_LoadAssetBundleAsync( string tName, AssetBundle[] rAssetBundle, string tKey = null, string tVector = null )
		{
			return StorageAccessor.LoadAssetBundle( GetFullPath( tName ), rAssetBundle, tKey, tVector ) ;
		}
	}
}
