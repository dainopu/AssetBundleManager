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
		/// マニフェストのリスト情報クラス
		/// </summary>
		[System.Serializable]
		public class ManifestDescriptor
		{
			/// <summary>
			/// マニフェスト名
			/// </summary>
			public string	ManifestName ;	// マニフェスト名 

			/// <summary>
			/// 最終更新日時(ＵＴＣ)
			/// </summary>
			public long		TimeStamp ;		// タイムスタンプ

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="tName"></param>
			/// <param name="tTIme"></param>
			public ManifestDescriptor( string manifestName, long timeStamp )
			{
				ManifestName	= manifestName ;
				TimeStamp		= timeStamp ;
			}
		}

		/// <summary>
		/// 全マニフェストの情報
		/// </summary>
		private List<ManifestDescriptor> m_ManifestDescriptors ;

		//-------------------------------------------------------------------

		/// <summary>
		/// システムファイルを読み出す
		/// </summary>
		/// <returns></returns>
		private bool LoadSystemFile()
		{
			if( m_ManifestDescriptors == null )
			{
				m_ManifestDescriptors = new List<ManifestDescriptor>() ;
			}
			else
			{
				m_ManifestDescriptors.Clear() ;
			}

			//----------------------------------------------------------

			string text = StorageAccessor_LoadText( m_SystemFileName ) ;
			if( string.IsNullOrEmpty( text ) == true )
			{
				return false ;	// ファイルが読み出せない
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
				return false ;
			}

			//----------------------------------------------------------

			for( i  = 0 ; i <  line.Length ; i ++ )
			{
				l = line[ i ].Length ;
				if( line[ i ][ l - 1 ] == '\t' )
				{
					// 最後の改行をカット
					line[ i ] = line[ i ].Substring( 0, l - 1 ) ;
				}

				string[] code = line[ i ].Split( '\t' ) ;

				if( code.Length == 2 )
				{
					// 既に実体を失っているものはリストから除外する

					if( StorageAccessor_Exists( code[ 0 ] ) == StorageAccessor.Target.Folder )
					{
						m_ManifestDescriptors.Add( new ManifestDescriptor( code[ 0 ], long.Parse( code[ 1 ] ) ) ) ;
					}
				}
			}

			if( m_ManifestKeepTime >  0L )
			{
				// 指定の時間アクセスの無いマニフェストは自動削除する

				long timeStamp = GetClientTime() ;

				List<ManifestDescriptor> manifestDescriptors = new List<ManifestDescriptor>() ;

				foreach( var manifestDescriptor in m_ManifestDescriptors )
				{
					if( ( manifestDescriptor.TimeStamp - timeStamp ) <  m_ManifestKeepTime )
					{
						// 維持する
						manifestDescriptors.Add( manifestDescriptor ) ;
					}
					else
					{
						// 削除する
						StorageAccessor_Remove( manifestDescriptor.ManifestName, true ) ;
					}
				}

				if( m_ManifestDescriptors.Count >  manifestDescriptors.Count )
				{
					// 無効化時間を超えたものがあるのでマニフェストリストをこの場で保存する
					m_ManifestDescriptors = manifestDescriptors ;

					SaveSystemFile() ;
				}
			}

			return true ;
		}

		/// <summary>
		/// システムファイルを書き込む
		/// </summary>
		/// <returns></returns>
		private bool SaveSystemFile()
		{
			if( m_ManifestDescriptors == null || m_ManifestDescriptors.Count == 0 )
			{
				return false ;
			}

			//----------------------------------

			string text = "" ;

			foreach( var manifestDescriptor in m_ManifestDescriptors )
			{
				text += manifestDescriptor.ManifestName ;
				text += "\t" ;
				text += manifestDescriptor.TimeStamp.ToString() ;
				text += "\n" ;
			}

			return StorageAccessor_SaveText( m_SystemFileName, text, true ) ;
		}

		//-------------------------------------------------------------------

		/// <summary>
		/// システムファイルにマニフェスト名を追加する
		/// </summary>
		/// <param name="manifestName"></param>
		/// <param name="timeStamp"></param>
		/// <returns></returns>
		private bool AddOrUpdateManifestToSystemFile( string manifestName, long timeStamp )
		{
			if( m_ManifestDescriptors == null || string.IsNullOrEmpty( manifestName ) == true )
			{
				return false ;
			}

			foreach( var manifestDescriptor in m_ManifestDescriptors )
			{
				if( manifestDescriptor.ManifestName.Equals( manifestName ) == true )
				{
					// 既に存在する
					manifestDescriptor.TimeStamp = timeStamp ;
					return true ;
				}
			}

			// 新規に追加する
			m_ManifestDescriptors.Add( new ManifestDescriptor( manifestName, timeStamp ) ) ;

			return true ;
		}
	}
}
