using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Security.Cryptography ;
using System.Runtime.Serialization.Formatters.Binary ;

using UnityEngine ;
using UnityEngine.Networking ;

/// <summary>
/// ストレージヘルパーパッケージ
/// </summary>
namespace OJT
{
	/// <summary>
	/// ストレージアクセサクラス Version 2018/06/18 0
	/// </summary>
	public class OpenJTalk_StorageAccessor
	{
		public enum Target
		{
			Unknown	= -1,
			None	=  0,
			File	=  1,
			Folder	=  2,
		}


#if UNITY_EDITOR || UNITY_STANDALONE
	
		// デバッグ用のテンポラリデータフォルダ
		public const string temporaryDataFoler = "/TemporaryDataFolder" ;
	
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
	
		private static string CreateTemporaryDataFolder()
		{
			string tPath = Directory.GetCurrentDirectory().Replace( "\\", "/" ) + temporaryDataFoler ;
		
			if( Directory.Exists( tPath ) == false )
			{
				// テンポラリが無いので生成する
				Directory.CreateDirectory( tPath ) ;
			}

			return tPath ;
		}
	
#endif


		/// <summary>
		/// データフォルダのパス
		/// </summary>
		public static string Path
		{
			get
			{
				// あえてメソッド化しているのは、引数で挙動を変える可能性もあるため。
				string tPath = "" ;
		
#if UNITY_EDITOR || UNITY_STANDALONE
		
				tPath = CreateTemporaryDataFolder() ;
		
#else
		
				tPath = Application.persistentDataPath ;
		
#endif
			
				return tPath + "/" ;
			}
		}
		
		public static void Setup()
		{
#if UNITY_EDITOR || UNITY_STANDALONE

			CreateTemporaryDataFolder() ;
#endif
		}

		/// <summary>
		/// ２つのパスを結合したものを返す
		/// </summary>
		/// <param name="tPath_0"></param>
		/// <param name="tPath_1"></param>
		/// <returns></returns>
		public static string Combine( string tPath_0, string tPath_1 )
		{
			if( string.IsNullOrEmpty( tPath_1 ) == false )
			{
				if( tPath_1[ 0 ] == '!' || tPath_1[ 0 ] == '@' )
				{
					// 絶対パス
					return tPath_1.Substring( 1, tPath_1.Length - 1 ) ;
				}
			}
			
			if( string.IsNullOrEmpty( tPath_0 ) == true )
			{
				return tPath_1 ;
			}

			tPath_0 = tPath_0.Replace( "\\", "/" ) ;

			if( string.IsNullOrEmpty( tPath_1 ) == true )
			{
				return tPath_0 ;
			}

			tPath_1 = tPath_1.Replace( "\\", "/" ) ;

			int l0 = tPath_0.Length ;
			if( tPath_0[ l0 - 1 ] == '/' )
			{
				tPath_0 = tPath_0.Substring( 0, l0 - 1 ) ;
			}

			int l1 = tPath_1.Length ;
			if( tPath_1[ 0 ] == '/' )
			{
				tPath_1 = tPath_1.Substring( 1, l1 - 1 ) ;
			}

			return tPath_0 + "/" + tPath_1 ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージにバイト配列を書き込む
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tData">バイト配列</param>
		/// <param name="tMakeFolder">フォルダが存在しない場合に生成するかどうか(生成しない場合はエラーとなる)</param>
		/// <param name="tKey">暗号化キー(null で暗号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で暗号化は行わない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Save( string tName, byte[] tData, bool tMakeFolder = false, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tName ) == true || tData == null )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Save Error : Name = " + tName + " Data = " + tData ) ;
				#endif
				return false ;
			}

			if( string.IsNullOrEmpty( tKey ) == false && string.IsNullOrEmpty( tVector ) == false )
			{
				// 暗号化する
				tData = Encrypt( tData, tKey, tVector ) ;
			}

			string tPath = Combine( Path, tName ) ;

			// 名前にフォルダが含まれているかチェックする
			int i = tPath.LastIndexOf( '/' ) ;
			if( i >= 0 )
			{
				// フォルダが含まれている
				string tFolderName = tPath.Substring( 0, i ) ;

				if( Directory.Exists( tFolderName ) == false )
				{
					if( tMakeFolder == false )
					{
						// セーブ出来ません
						return false ;
					}
					else
					{
						// フォルダを生成する(多階層をまとめて生成出来る)
						Directory.CreateDirectory( tFolderName ) ;
						
						// Apple 審査のリジェクト回避用コード
						#if !UNITY_EDITOR &&( UNITY_IOS || UNITY_IPHONE )
							UnityEngine.iOS.Device.SetNoBackupFlag( tFolderName ) ;
						#endif
					}
				}
			}

			File.WriteAllBytes( tPath, tData ) ;

			// Apple 審査のリジェクト回避用コード
			#if !UNITY_EDITOR &&( UNITY_IOS || UNITY_IPHONE )
				UnityEngine.iOS.Device.SetNoBackupFlag( tPath ) ;
			#endif

			return true ;
		}

		/// <summary>
		/// ローカルストレージに文字列を書き込む
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tText">文字列</param>
		/// <param name="tMakeFolder">フォルダが存在しない場合に生成するかどうか(生成しない場合はエラーとなる)</param>
		/// <param name="tKey">暗号化キー(null で暗号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で暗号化は行わない)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveText( string tName, string tText, bool tMakeFolder = false, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tName ) == true || string.IsNullOrEmpty( tText ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Text File Save Error : Name = " + tName + " Text = " + tText ) ;
				#endif
				return false ;
			}
		
			byte[] tData = Encoding.UTF8.GetBytes( tText ) ;

			return Save( tName, tData, tMakeFolder, tKey, tVector ) ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージからバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で複合化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で複合化は行わない)</param>
		/// <returns>バイト配列(null で失敗)</returns>
		public static byte[] Load( string tName, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			string tPath = Combine( Path, tName ) ;

			if( File.Exists( tPath ) == false )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			byte[] tData = File.ReadAllBytes( tPath ) ;
			if( tData == null )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			if( string.IsNullOrEmpty( tKey ) == false && string.IsNullOrEmpty( tVector ) == false )
			{
				// 復号化する
				tData = Decrypt( tData, tKey, tVector ) ;
			}

			return tData ;
		}

		/// <summary>
		/// ローカルストレージからバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で複合化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で複合化は行わない)</param>
		/// <returns>バイト配列(null で失敗)</returns>
		public static byte[] Load( string tName, int tOffset, int tLength )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			string tPath = Combine( Path, tName ) ;

			if( File.Exists( tPath ) == false )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			int tSize = GetSize( tName ) ;

			if( tSize <= 0 || tOffset <  0 || tOffset >= tSize )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			if( ( tOffset + tLength ) >  tSize )
			{
				tLength = tSize - tOffset ;
			}

			FileStream tStream = File.OpenRead( tPath ) ;
			if( tStream == null )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			tStream.Seek( tOffset, SeekOrigin.Begin ) ;

			byte[] tData = new byte[ tLength ] ;
			tSize = tStream.Read( tData, 0, tLength ) ;

			if( tSize != tLength )
			{
				tStream.Close() ;

				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				return null ;
			}

			tStream.Close() ;

			return tData ;
		}

		/// <summary>
		/// ローカルストレージから文字列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>文字列(null で失敗)</returns>
		public static string LoadText( string tName, string tKey = null, string tVector = null )
		{
			byte[] tData = Load( tName, tKey, tVector ) ;
			if( tData == null )
			{
				return null ;
			}

			if( tData.Length >= 3 )
			{
				if( tData[ 0 ] == 0xEF && tData[ 1 ] == 0xBB && tData[ 2 ] == 0xBF )
				{
					// BOM 突き
					return Encoding.UTF8.GetString( tData, 3, tData.Length - 3 ) ;
				}
			}

			return Encoding.UTF8.GetString( tData ) ;
		}

		//-----------------------------------------------------------
/*
		/// <summary>
		/// オブジェクトをバイナリデータ化して返す
		/// </summary>
		/// <param name="tObject"></param>
		/// <returns></returns>
		public static byte[] GetBinary<T>( T tObject ) where T : class
		{
			// iOSでは下記設定を行わないとエラーになる
#if UNITY_IOS || UNITY_IPHONE
			Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" ) ;
#endif

			BinaryFormatter tBF = new BinaryFormatter() ;
			MemoryStream tMS = new MemoryStream() ;

			tBF.Serialize( tMS, tObject ) ;
			return tMS.GetBuffer() ;
		}

		public static T SetBinary<T>( T tOverwriteObject, byte[] tData ) where T : class 
		{
			// iOSでは下記設定を行わないとエラーになる
#if UNITY_IOS || UNITY_IPHONE
			Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" ) ;
#endif

			BinaryFormatter tBF = new BinaryFormatter() ;
			MemoryStream tMS = new MemoryStream( tData ) ;

			T tObject = tBF.Deserialize( tMS ) as T ;

			if( tOverwriteObject != null && tObject != null )
			{
				tOverwriteObject  = tObject ;
			}

			return tObject ;
		}
*/

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージからテクスチャを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>テクスチャのインスタンス(null で失敗)</returns>
		public static Texture2D LoadTexture( string tName, string tKey = null, string tVector = null )
		{
			byte[] tData = Load( tName, tKey, tVector ) ;
			if( tData == null )
			{
				return null ;
			}
		
			// イメージデータは取得出来た
			Texture2D tTexture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;	// MipMap を事前に切るのがミソ
			tTexture.LoadImage( tData ) ;
		
	//		Debug.Log( "ミップマップカウント:" + tTexture.mipmapCount ) ;	// これが１になってなっていればＯＫ
		
			return tTexture ;
		}

		/// <summary>
		/// ローカルストレージからオーディオクリップを読み出す(これは使い物にはならない)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tOutput">オーディオクリップのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tStream">ストリーミングを有効にするかどうか</param>
		/// <param name="tAudioType">ファイルの種別(wav ogg mp3)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAudioClip( string name, AudioClip[] output, bool stream = false, AudioType audioType = AudioType.WAV, string key = null, string vector = null )
		{
			if( output == null || output.Length == 0 )
			{
				yield break ;
			}

			if( Exists( name ) != Target.File )
			{
				yield break ;	// ファイルが存在しない
			}

			string path = Combine( Path, name ) ;

			name = name.ToLower() ;
			if( name.IndexOf( "wav" ) >= 0 )
			{
				audioType = AudioType.WAV ;
			}
			else
			if( name.IndexOf( "ogg" ) >= 0 )
			{
				audioType = AudioType.OGGVORBIS ;
			}
			else
			if( name.IndexOf( "mp3" ) >= 0 )
			{
				audioType = AudioType.MPEG ;
			}

			UnityWebRequest	www = UnityWebRequestMultimedia.GetAudioClip( "file://" + path, audioType ) ;
			yield return www.SendWebRequest() ;
			if( www.isHttpError || www.isNetworkError )
			{
				yield break ;	// エラー
			}
			
			AudioClip audioClip = DownloadHandlerAudioClip.GetContent( www ) ;
			www.Dispose() ;
			www = null ;

			output[ 0 ] = audioClip ;
		}

		/// <summary>
		/// ローカルストレージからアセットバンドルを読み出す(同期版)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>アセットバンドルのインスタンス(null で失敗)</returns>
		public static AssetBundle LoadAssetBundle( string tName, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tKey ) == true && string.IsNullOrEmpty( tVector ) == true )
			{
				if( Exists( tName ) != Target.File )
				{
					return null ;	// ファイルが存在しない
				}

				string tPath = Combine( Path, tName ) ;

				// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
//				Debug.LogWarning( "Path:" + tPath ) ;
				return AssetBundle.LoadFromFile( tPath ) ;
			}
			
			byte[] tData = Load( tName, tKey, tVector ) ;
			if( tData == null )
			{
				return null ;
			}
			
			// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
			return AssetBundle.LoadFromMemory( tData ) ;
		}

		/// <summary>
		/// ローカルストレージからアセットバンドルを読み出す(非同期版)
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAssetBundle( string tName, AssetBundle[] rAssetBundle, string tKey = null, string tVector = null )
		{
			AssetBundleCreateRequest tR ;

			if( string.IsNullOrEmpty( tKey ) == true && string.IsNullOrEmpty( tVector ) == true )
			{
				if( Exists( tName ) != Target.File )
				{
					yield break ;	// ファイルが存在しない
				}

				string tPath = Combine( Path, tName ) ;

				// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
				tR = AssetBundle.LoadFromFileAsync( tPath ) ;
				yield return tR ;

				if( tR.isDone == false )
				{
					yield break ;
				}

				if( rAssetBundle != null && rAssetBundle.Length >  0 )
				{
					rAssetBundle[ 0 ] = tR.assetBundle ;
				}
				else
				{
					if( tR.assetBundle != null )
					{
						tR.assetBundle.Unload( true ) ;
					}
				}		

				yield break ;
			}


			byte[] tData = Load( tName, tKey, tVector ) ;
			if( tData == null )
			{
				yield break ;
			}
		
			// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
			tR = AssetBundle.LoadFromMemoryAsync( tData ) ;
			yield return tR ;

			if( tR.isDone == false )
			{
				yield break ;
			}

			if( rAssetBundle != null && rAssetBundle.Length >  0 )
			{
				rAssetBundle[ 0 ] = tR.assetBundle ;
			}
			else
			{
				if( tR.assetBundle != null )
				{
					tR.assetBundle.Unload( true ) ;
				}
			}		
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ローカルストレージに保存されたムービーファイルをネイティブプレイヤーで再生する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tIsCancelOnInput">タップで再生を中止させられるようにするかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMovie( string tName, bool tIsCancelOnInput )
		{
			if( Exists( tName ) != Target.File )
			{
				return false ;	// ファイルが存在しない
			}

			bool tResult = true ;

			string tPath = Combine( Path, tName ) ;

#if UNITY_EDITOR || UNITY_STANDALONE
			Application.OpenURL( "file://" + tPath ) ;
#elif !UNITY_EDITOR && UNITY_ANDROID
			tResult = Handheld.PlayFullScreenMovie( tPath, Color.black, ( tIsCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;
#elif !UNITY_EDITOR && ( UNITY_IOS || UNITY_IPHONE )
			tResult = Handheld.PlayFullScreenMovie( "file://" + tPath, Color.black, ( tIsCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;
#endif
		
			return tResult ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 指定したファイルのサイズを取得する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <returns>ファイルのサイズ(-1 でファイルが存在しない)</returns>
		public static int GetSize( string tName )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Exist Error : Name = " + tName ) ;
				#endif
				return -1 ;
			}

			string tPath = Combine( Path, tName ) ;
		
			if( File.Exists( tPath ) == false )
			{
				return -1 ;
			}
		
			FileInfo tInfo = new FileInfo( tPath ) ;
		
			return ( int )tInfo.Length ;
		}
	
		/// <summary>
		/// 指定したフォルダに内包されるファイル名の一覧を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>ファイル名の一覧が格納された文字列の配列</returns>
		public static string[] GetFiles( string tName )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return null ;
			}

			string tPath = Combine( Path, tName ) ;

			if( Directory.Exists( tPath ) == false )
			{
				// 対象はフォルダではない
				return null ;
			}

			return Directory.GetFiles( tPath ) ;
		}

		/// <summary>
		/// 指定したフォルダに内包されるフォルダ名の一覧を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>フォルダ名の一覧が格納された文字列の配列</returns>
		public static string[] GetFolders( string tName )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				return null ;
			}

			string tPath = Combine( Path, tName ) ;

			if( Directory.Exists( tPath ) == false )
			{
				// 対象はフォルダではない
				return null ;
			}

			return Directory.GetDirectories( tPath ) ;
		}

		/// <summary>
		/// 指定したフォルダに内包されるファイルおよびフォルダの数を取得する
		/// </summary>
		/// <param name="tName">フォルダ名(相対パス)</param>
		/// <returns>ファイルおよびフォルダの数</returns>
		public static int GetCount( string tName )
		{
			string[] tFileList   = GetFiles( tName ) ;
			string[] tFolderList = GetFolders( tName ) ;

			int tCount = 0 ;

			if( tFileList != null )
			{
				tCount = tCount + tFileList.Length ;
			}

			if( tFolderList != null )
			{
				tCount = tCount + tFolderList.Length ;
			}

			return tCount ;
		}

		/// <summary>
		/// 指定のファイルまたはフォルダが存在するか確認する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <returns>結果(-1=名前が不正・0=存在しない・1=ファイルが存在する・2=フォルダが存在する)</returns>
		public static Target Exists( string tName )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Exist Error : Name = " + tName ) ;
				#endif
				return Target.None ;
			}

			string tPath = Combine( Path, tName ) ;

			if( File.Exists( tPath ) == true )
			{
				return Target.File ;
			}
			else
			if( Directory.Exists( tPath ) == true )
			{
				return Target.Folder ;
			}

			return Target.None ;
		}

		/// <summary>
		/// 指定のファイルまたはフォルダを移動する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <param name="tAbsolute">削除対象がフォルダの場合に内部にファイルまたはフォルダが存在していても強制的に削除するかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Move( string tNameOld, string tNameNew )
		{
			if( string.IsNullOrEmpty( tNameOld ) == true || string.IsNullOrEmpty( tNameNew ) )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Move Error" ) ;
				#endif
				return false ;
			}

			if( tNameOld.Equals( tNameNew ) == true )
			{
				// パスが同じ
				return true ;
			}

			string tPathOld = Combine( Path, tNameOld ) ;
			string tPathNew = Combine( Path, tNameNew ) ;
			
			if( File.Exists( tPathOld ) == false && Directory.Exists( tPathOld ) == false )
			{
				// 移動元のファイルかフォルダ存在しない
				return false ;
			}

			if( File.Exists( tPathNew ) == true || Directory.Exists( tPathNew ) == true )
			{
				// 移動先にファイルかフォルダが存在する
				return false ;
			}

			if( File.Exists( tPathOld ) == true )
			{
				// 移動元はファイル
				File.Move( tPathOld, tPathNew ) ;
			}
			else
			if( Directory.Exists( tPathOld ) == true )
			{
				// 移動元はフォルダ
				Directory.Move( tPathOld, tPathNew ) ;
			}
			
			// Apple 審査のリジェクト回避用コード
			#if !UNITY_EDITOR &&( UNITY_IOS || UNITY_IPHONE )
				UnityEngine.iOS.Device.SetNoBackupFlag( tPathNew ) ;
			#endif

			return true ;
		}



		/// <summary>
		/// 指定のファイルまたはフォルダを削除する
		/// </summary>
		/// <param name="tName">ファイルまたはフォルダの名前(相対パス)</param>
		/// <param name="tAbsolute">削除対象がフォルダの場合に内部にファイルまたはフォルダが存在していても強制的に削除するかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Remove( string tName, bool tAbsolute = false )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Remove Error : Name = " + tName ) ;
				#endif
				return false ;
			}

			string tPath = Combine( Path, tName ) ;
		
			if( File.Exists( tPath ) == true )
			{
				// ファイルが存在する
				File.Delete( tPath ) ;

				return true ;
			}
			else
			if( Directory.Exists( tPath ) == true )
			{
				// ディレクトリが存在する
				
				if( tAbsolute == false )
				{
					// 内包するファイルとフォルダの数を確認する
					if( GetCount( tName ) >  0 )
					{
						return false ;	// 削除は出来ない
					}
					Directory.Delete( tPath ) ;

					return true ;
				}
				else
				{
					// 強制削除可能

					// 再帰的に内包するファイルとフォルダを全て削除する
					Directory.Delete( tPath, true ) ;

					return true ;
				}
			}

			return false ;
		}

		/// <summary>
		/// 指定したフォルダ以下の内包されるファイルとフォルダが存在しないフォルダを全て削除する
		/// </summary>
		/// <param name="tName">フォルダの名前(相対パス)</param>
		public static void RemoveAllEmptyFolders( string tName = "" )
		{
			if( tName == null )
			{
				tName = "" ;
			}

			string tPath = Combine( Path, tName ) ;

			RemoveEmptyFolderAllLoop( tPath ) ;	// ルートフォルダは残す
		}

		// 指定したフォルダ以下の内包されるファイルとフォルダが存在しないフォルダを全て削除する
		private static bool RemoveEmptyFolderAllLoop( string tCurrentPath )
		{
			int i ;
		
			string tPath ;
		
			//-----------------------------------------------------
		
			if( Directory.Exists( tCurrentPath ) == false )
			{
				return false ;
			}
		
			//-----------------------------------------------------
		
			// ファイル
			int f = 0 ;
			string[] tFA = Directory.GetFiles( tCurrentPath ) ;
			if( tFA != null && tFA.Length >  0 )
			{
				f = tFA.Length ;
			}

			// フォルダ
			int d = 0 ;
			string[] tDA = Directory.GetDirectories( tCurrentPath ) ;
			if( tDA != null && tDA.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( i  = 0 ; i <  tDA.Length ; i ++ )
				{
					tPath = tDA[ i ] + "/" ;
					if( RemoveEmptyFolderAllLoop( tPath ) == false )
					{
						// このフォルダは削除してはいけない

						d ++ ;	// 残っているフォルダ増加
					}
					else
					{
						// このフォルダは削除して良い
						Directory.Delete( tPath, true ) ;
					}
				}
			}

			if( f >  0 || d >  0 )
			{
				// このフォルダは削除してはいけない
				return false ;
			}
			else
			{
				// このフォルダは削除して良い
				return true ;
			}
		}

		//------------------------------------------------------------------------------

		/// <summary>
		/// バイト配列を暗号化する
		/// </summary>
		/// <param name="tOriginalData">暗号化前のバイト配列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>暗号化後のバイト配列</returns>
		public static byte[] Encrypt( byte[] tOriginalData, string tKey, string tVector )
		{
			// オリジナルのサイズがわからなくなるので保存する
			byte[] tData = new byte[ 4 + tOriginalData.Length ] ;
			long tSize = tOriginalData.Length ;
		
			tData[ 0 ] = ( byte )( ( tSize >>  0 ) & 0xFF ) ;
			tData[ 1 ] = ( byte )( ( tSize >>  8 ) & 0xFF ) ;
			tData[ 2 ] = ( byte )( ( tSize >> 16 ) & 0xFF ) ;
			tData[ 3 ] = ( byte )( ( tSize >> 24 ) & 0xFF ) ;
	
			System.Array.Copy( tOriginalData, 0, tData, 4, tSize ) ;
		
			//-----------------------------------------------------
		
			// 暗号化用の種別オブジェクト生成
	//		TripleDESCryptoServiceProvider tKind = new TripleDESCryptoServiceProvider() ;

			RijndaelManaged tKind = new RijndaelManaged() ;
			tKind.Padding = PaddingMode.Zeros ;
			tKind.Mode = CipherMode.CBC ;
			tKind.KeySize   = 256 ;
			tKind.BlockSize = 256 ;
		
			//-----------------------------------------------------
		
			// 暗号用のキー情報をセットする
			byte[] aKey    = Encoding.UTF8.GetBytes( tKey    ) ;
			byte[] aVector = Encoding.UTF8.GetBytes( tVector ) ;
		
			ICryptoTransform tEncryptor = tKind.CreateEncryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			MemoryStream tMemoryStream = new MemoryStream() ;
		
			// 暗号化
			CryptoStream tCryptoStream = new CryptoStream( tMemoryStream, tEncryptor, CryptoStreamMode.Write ) ;
		
 			tCryptoStream.Write( tData, 0, tData.Length ) ;
			tCryptoStream.FlushFinalBlock() ;
		
			tCryptoStream.Close() ;
		
			byte[] tCryptoData = tMemoryStream.ToArray() ;
		
 			tMemoryStream.Close() ;
		
			//-----------------------------------------------------
		
			tEncryptor.Dispose() ;
		
			tKind.Clear() ;
		
			//-----------------------------------------------------
		
			return tCryptoData ;
		}

		/// <summary>
		/// 文字列を暗号化する
		/// </summary>
		/// <param name="tText">暗号化前の文字列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>暗号化後の文字列</returns>
		public static string Encrypt( string tText, string tKey, string tVector )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				return null ;
			}
		
			byte[] tOriginalData = Encoding.UTF8.GetBytes( tText ) ;
			byte[] tCryptoData = Encrypt( tOriginalData, tKey, tVector ) ;
		
			return Convert.ToBase64String( tCryptoData ) ;
		}

		/// <summary>
		/// バイト配列を復号化する
		/// </summary>
		/// <param name="tCryptoData">暗号化されたバイト配列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>復号化されたバイト配列</returns>
		public static byte[] Decrypt( byte[] tCryptoData, string tKey, string tVector )
		{
			// 暗号化用の種別オブジェクト生成
	//		TripleDESCryptoServiceProvider tKind = new TripleDESCryptoServiceProvider() ;
		
			RijndaelManaged tKind = new RijndaelManaged() ;
			tKind.Padding = PaddingMode.Zeros ;
			tKind.Mode = CipherMode.CBC ;
			tKind.KeySize   = 256 ;
			tKind.BlockSize = 256 ;
		
			//-----------------------------------------------------
		
			// 暗号用のキー情報をセットする
			byte[] aKey    = Encoding.UTF8.GetBytes( tKey    ) ;
			byte[] aVector = Encoding.UTF8.GetBytes( tVector ) ;
		
			ICryptoTransform tDecryptor = tKind.CreateDecryptor( aKey, aVector ) ;
		
			//-----------------------------------------------------
		
			byte[] tData = new byte[ tCryptoData.Length ] ;
		
			//-----------------------------------------------------
		
			MemoryStream tMemoryStream = new MemoryStream( tCryptoData ) ;
		
			// 復号化
			CryptoStream tCryptoStream = new CryptoStream( tMemoryStream, tDecryptor, CryptoStreamMode.Read ) ;
		
		
			tCryptoStream.Read( tData, 0, tData.Length ) ;
			tCryptoStream.Close() ;
		
			tMemoryStream.Close() ;
 		
			//-----------------------------------------------------
		
			tDecryptor.Dispose() ;
		
			tKind.Clear() ;
		
			//-----------------------------------------------------
		
			long tSize = ( ( long )tData[ 0 ] <<  0 ) | ( ( long )tData[ 1 ] <<  8 ) | ( ( long )tData[ 2 ] << 16 ) | ( ( long )tData[ 3 ] ) ;
		
			byte[] tOriginalData = new byte[ tSize ] ;
			System.Array.Copy( tData, 4, tOriginalData, 0, tSize ) ;
		
			return tOriginalData ;
		}
	
		/// <summary>
		/// 文字列を復号化する
		/// </summary>
		/// <param name="tText">暗号化された文字列</param>
		/// <param name="tKey">暗号化キー</param>
		/// <param name="tVector">暗号化ベクター</param>
		/// <returns>復号化された文字列</returns>
		public static string Decrypt( string tText, string tKey, string tVector )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				return null ;
			}

			byte[] tCryptoData ;
			try
			{
				tCryptoData = Convert.FromBase64String( tText ) ;
			}
			catch( System.FormatException )
			{
				Debug.LogWarning( "データが壊れています" ) ;
				return "";
			}
			byte[] tOriginalData = Decrypt( tCryptoData, tKey, tVector ) ;
		
			return Encoding.UTF8.GetString( tOriginalData ) ;
		}

		//-------------------------------------------------------
		
		/// <summary>
		/// バイト配列からＭＤ５のハッシュコード文字列を取得する
		/// </summary>
		/// <param name="tData">ハッシュコードを取得する対象のバイト配列</param>
		/// <returns>ハッシュコード文字列</returns>
		public static string GetMD5Hash( byte[] tData )
		{
			// MD5CryptoServiceProviderオブジェクトを作成
			System.Security.Cryptography.MD5CryptoServiceProvider tMD5 = new System.Security.Cryptography.MD5CryptoServiceProvider() ;
		
			// ハッシュ値を計算する
			byte[] tBytes = tMD5.ComputeHash( tData ) ;
		
			// リソースを解放する
			tMD5.Clear() ;
		
			// byte型配列を16進数の文字列に変換
			System.Text.StringBuilder tResult = new System.Text.StringBuilder() ;
			foreach( byte tByte in tBytes )
			{
				tResult.Append( tByte.ToString( "x2" ) ) ;
			}
		
			return tResult.ToString() ;
		}

		/// <summary>
		/// 文字列からＭＤ５のハッシュコード文字列を取得する
		/// </summary>
		/// <param name="tText">ハッシュコードを取得する対象の文字列</param>
		/// <returns>ハッシュコード文字列</returns>
		public static string GetMD5Hash( string tText )
		{
			// 文字列をbyte型配列に変換する
			return GetMD5Hash( System.Text.Encoding.UTF8.GetBytes( tText ) ) ;
		}
		
		//-------------------------------------------------------

		/// <summary>
		/// StreamingAssets からバイト配列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rData">バイト配列を格納する要素数１以上のバイト配列の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadFromStreamingAssets( string tName, byte[][] rData, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tName ) == true )
			{
				#if UNITY_EDITOR
				Debug.LogError( "Storage Data File Load Error : Name = " + tName + " Data = " + rData ) ;
				#endif
				yield break ;
			}


			byte[] tData = null ;

			#if UNITY_ANDROID && !UNITY_EDITOR

			string path = Application.streamingAssetsPath + "/" + tName ;
			UnityWebRequest request = UnityWebRequest.Get(path);
			request.SendWebRequest();
			yield return request;

			if( request.isDone == true && string.IsNullOrEmpty( request.error ) == true )
			{
				tData = request.downloadHandler.data ;
			}

			request.Dispose() ;

			#else
		
			string tPath = Application.streamingAssetsPath + "/" + tName ;

			if( File.Exists( tPath ) == true )
			{
				tData = File.ReadAllBytes( tPath ) ;
			}

			#endif

			if( tData == null )
			{
				#if UNITY_EDITOR
				Debug.LogWarning( "Storage Data File Load Error : Name = " + tName ) ;
				#endif
				yield break ;
			}

			if( string.IsNullOrEmpty( tKey ) == false && string.IsNullOrEmpty( tVector ) == false )
			{
				// 復号化する
				tData = Encrypt( tData, tKey, tVector ) ;
			}

			if( rData != null && rData.Length >  0 )
			{
				rData[ 0 ] = tData ;
			}
		}

		/// <summary>
		/// StreamingAssets から文字列を読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rText">文字列を格納する要素数１以上の文字列の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadTextFromStreamingAssets( string tName, string[] rText, string tKey = null, string tVector = null  )
		{
			byte[][] rData = new byte[ 1 ][] ;

			yield return LoadFromStreamingAssets( tName, rData, tKey, tVector ) ;

			if( rData[ 0 ] != null && rText != null && rText.Length >  0 )
			{
				rText[ 0 ] = Encoding.UTF8.GetString( rData[ 0 ] ) ;
			}
		}

		/// <summary>
		/// StreamingAssets からテクスチャを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rTexture">テクスチャのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadTextureFromStreamingAssets( string tName, Texture2D[] rTexture, string tKey = null, string tVector = null )
		{
			byte[][] rData = new byte[ 1 ][] ;

			yield return LoadFromStreamingAssets( tName, rData, tKey, tVector ) ;

			if( rData[ 0 ] != null && rTexture != null && rTexture.Length >  0 )
			{
				// イメージデータは取得出来た
				Texture2D tTexture = new Texture2D( 4, 4, TextureFormat.ARGB32, false, true ) ;	// MipMap を事前に切るのがミソ
				tTexture.LoadImage( rData[ 0 ] ) ;

				rTexture[ 0 ] = tTexture ;
			}
		}

		/// <summary>
		/// StreamingAssets からアセットバンドルを読み出す
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="rAssetBundle">アセットバンドルのインスタンスを格納する要素数１以上の配列</param>
		/// <param name="tKey">暗号化キー(null で復号化は行わない)</param>
		/// <param name="tVector">暗号化ベクター(null で復号化は行わない)</param>
		/// <returns>列挙子</returns>
		public static IEnumerator LoadAssetBundleFromStreamingAssets( string tName, AssetBundle[] rAssetBundle, string tKey = null, string tVector = null )
		{
			if( string.IsNullOrEmpty( tKey ) == true && string.IsNullOrEmpty( tVector ) == true )
			{
				AssetBundle tAssetBundle = null ;

				#if UNITY_ANDROID && !UNITY_EDITOR

				string path = Application.streamingAssetsPath + "/" + tName ;
				UnityWebRequest request = UnityWebRequest.Get(path);
				request.SendWebRequest();
				yield return request;

				if( request.isDone == true && string.IsNullOrEmpty( request.error ) == true )
				{
					if( request.downloadHandler.data != null && request.downloadHandler.data.Length >  0 )
					{
						tAssetBundle = AssetBundle.LoadFromMemory( request.downloadHandler.data ) ;
					}
				}

				request.Dispose() ;

				#else

				string tPath = Application.streamingAssetsPath + "/" + tName ;
				if( File.Exists( tPath ) == true )
				{
					// 暗号化がされていなければファイルから直接アセットバンドル化を行う(メモリ消費量が少ない)
					tAssetBundle = AssetBundle.LoadFromFile( tPath ) ;
				}

				#endif

				if( rAssetBundle != null && rAssetBundle.Length >  0 )
				{
					rAssetBundle[ 0 ] = tAssetBundle ;
				}

				yield break ;
			}


			byte[][] rData = new byte[ 1 ][] ;

			yield return LoadFromStreamingAssets( tName, rData, tKey, tVector ) ;
		
			if( rData[ 0 ] != null && rAssetBundle != null && rAssetBundle.Length >  0 )
			{
				// 暗号化がされている場合は一旦メモリに展開してからアセットバンドル化を行う(メモリ消費量が大きい)
				rAssetBundle[ 0 ] = AssetBundle.LoadFromMemory( rData[ 0 ] ) ;
			}
		}


		/// <summary>
		/// StreamingAssets に存在するムービーファイルをネイティブプレイヤーで再生する
		/// </summary>
		/// <param name="tName">ファイル名(相対パス)</param>
		/// <param name="tIsCancelOnInput">タップで再生を中止させられるようにするかどうか</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool PlayMovieFromStreamingAssets( string tName, bool tIsCancelOnInput )
		{
			bool tResult = true ;
		
			#if !UNITY_EDITOR && UNITY_ANDROID

			tResult = Handheld.PlayFullScreenMovie( tName, Color.black, ( tIsCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;

			#elif!UNITY_EDITOR &&  ( UNITY_IOS || UNITY_IPHONE )

			tResult = Handheld.PlayFullScreenMovie( tName, Color.black, ( tIsCancelOnInput ) ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit ) ;

			#endif

			return tResult ;
		}


		//---------------------------------------------------------------------------

		/// <summary>
		/// 固有識別子を取得する(取得毎に値が変わる可能性があるため最初に保存したものを使いまわす)
		/// </summary>
		/// <returns></returns>
		public static string GetUUID( string tPath = "UUID" )
		{
			string tKey		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;
			string tVector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;

			string tUUID = LoadText( tPath, tKey, tVector ) ;
			if( string.IsNullOrEmpty( tUUID ) == true )
			{
				// 生成する
				tUUID = SystemInfo.deviceUniqueIdentifier ;
				SaveText( tPath, tUUID, false, tKey, tVector ) ;
			}

			return tUUID ;
		}
	}
}

