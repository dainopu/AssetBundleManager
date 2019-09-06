using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections.Generic ;
using System.Security.Cryptography ;
using System.Text ;

/// <summary>
/// シンプルアセットバンドルビルダーパッケージ
/// </summary>
namespace SimpleAssetBundleBuilder
{
	/// <summary>
	/// アセットバンドルビルダークラス(エディター用) Version 2018/06/25 0
	/// </summary>
	public class SimpleAssetBundleBuilder : EditorWindow
	{
		[ MenuItem( "Tools/Simple AssetBundle Builder" ) ]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleAssetBundleBuilder>( false, "AB Builder", true ) ;
		}

		//------------------------------------------------------------

		// アセットバンドル化するファイル単位の情報
		[System.Serializable]
		class AssetBundleFile
		{
			[System.Serializable]
			public class AssetFile
			{
				public string	path ;
				public int		type ;	// 0だと直接含まれるアセットを表す

				public AssetFile( string tPath, int tType )
				{
					path = tPath ;
					type = tType ;
				}
			}

			public string	name ;											// 書き出すアセットバンドルのパス
			public List<AssetFile>	assetFile = new List<AssetFile>() ;		// 含めるアセットのパスのリスト

			public int		sourceType ;									// 0=ファイル　1=フォルダ

			public void AddAssetFile( string tPath, int tType )
			{
				assetFile.Add( new AssetFile( tPath, tType ) ) ;
			}

			// 依存関係のあるアセットも対象に追加する
			public int CollectDependencies()
			{
				// 注意：必ず基本アセットを全て追加した後にこのメソッドを呼び出す事

				if( assetFile.Count == 0 )
				{
					return 0 ;
				}

				List<string> tAssetPath = new List<string>() ;

				string[] tCheckPath ;

				string s ;
				int i, j, k, l, p ;
				bool f ;

				l = assetFile.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
	//				Debug.LogWarning( "基準アセット:" + assetFile[ i ].path ) ;

					// 依存関係にあるアセットを検出する
					tCheckPath = AssetDatabase.GetDependencies( assetFile[ i ].path ) ;
					if( tCheckPath!= null && tCheckPath.Length >  0 )
					{
						for( j  = 0 ; j <  tCheckPath.Length ; j ++ )
						{
							// 同アセットバンドルに含まれるアセットは除外する
							for( k  = 0 ; k <  assetFile.Count ; k ++ )
							{
								if( tCheckPath[ j ] == assetFile[ k ].path )
								{
									break ;
								}
							}

							if( k >= assetFile.Count )
							{
								// 候補にはなる
							
								f = true ;

								// 拡張子を確認する
								p = tCheckPath[ j ].LastIndexOf( '.' ) ;
								if( p >= 0 )
								{
									s = tCheckPath[ j ].Substring( p + 1, ( tCheckPath[ j ].Length - ( p + 1 ) ) ) ;
									if( s == "cs" || s == "js" )
									{
										// ソースコードは除外
										f = false ;
									}
								}

								if( f == true )
								{
									// さらに自身の依存を辿る
	//								Debug.LogWarning( "  依存アセット:" + tCheckPath[ j ] ) ;
									tAssetPath.Add( tCheckPath[ j ] ) ;
								}
							}
						}
					}
				}
			
				l = tAssetPath.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					AddAssetFile( tAssetPath[ i ], 1 ) ;	// 依存系のアセット
				}

				return l ;
			}
		}

		//------------------------------------------------------------


		private string						m_ResourceListFilePath			= "" ;
		private string						m_ResourceRootFolderPath		= "" ;						// リソースルートパス
		private string						m_AssetBundleRootFolderPath		= "" ;						// アセットバンドルルートパス

		private BuildTarget					m_BuildTarget					= BuildTarget.Android ;		// ターゲットプラットフォーム
		private bool						m_ChunkBasedCompression			= true ;					// チャンクベースのフォーマットにするか
		private bool						m_ForceRebuildAssetBundle		= false ;					// 強制的に再生成するか
		private bool						m_IgnoreTypeTreeChanges			= false ;					// タイプツリーが変わっても無視(ソースコードを変更しても変化したとはみなされない)
		private bool						m_DisableWriteTypeTree			= false ;					// タイプツリー自体を削除(サイズ削減用途として)

		private bool						m_CollectDependencies			= false ;					// 全てのアセットバンドルでそのアセットバンドルで使用するアセットを全て含ませる

		private bool						m_GenerateCRCFile				= true ;					// アセットバンドルファイルのＣＲＣファイルを出力する

		//--------------------------------------------------
	
	
		private AssetBundleFile[]			m_AssetBundleFileList = null ;
	
		//--------------------------------------------------
	
		private bool m_Clean				= true ;
	
		private Vector2 m_Scroll			= Vector2.zero ;
	
		private bool m_Refresh				= true ;
	
		private bool m_ShowResourceElements	= false ;
	
	
		//-----------------------------------------------------------------
		
		// 描画
		void OnGUI()
		{
			GUILayout.Space( 6f ) ;
		
			string tPath ;
		
			int i, l ;
			
			// リスト更新フラグ
			m_Refresh = false ;

			//-------------------------------------------------------------
		
			EditorGUILayout.HelpBox( GetMessage( "SelectResourcePath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 0, 1, 1, 1 ) ;
				if( GUILayout.Button( "Resource List File Path", GUILayout.Width( 200f ) ) == true )
				{
					m_Refresh = true ;

					m_ResourceListFilePath = "" ;
					m_ResourceRootFolderPath = "" ;
					m_AssetBundleFileList = null ;

					if( Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						// １つだけ選択（複数選択には対応していない：フォルダかファイル）
						tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( File.Exists( tPath ) == true )
						{
							// ファイルを指定
								
							TextAsset tTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( tPath ) ;
							if( tTextAsset != null && string.IsNullOrEmpty( tTextAsset.text ) == false )
							{
								m_ResourceListFilePath = tPath ;

								tPath = tPath.Replace( "\\", "/" ) ;
							
								// 最後のフォルダ区切り位置を取得する
								int s = tPath.LastIndexOf( '.' ) ;
								if( s >= 0 )
								{
									tPath = tPath.Substring( 0, s ) ;
								}
							
								// 最後のフォルダ区切り位置を取得する
								s = tPath.LastIndexOf( '/' ) ;
								if( s >= 0 )
								{
									tPath = tPath.Substring( 0, s ) ;
								}
								
								// ファイルかどうか判別するには System.IO.File.Exists
								m_ResourceRootFolderPath = tPath + "/" ;
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;
			
			
				//---------------------------------------------------------
			
				// ルートフォルダ
				EditorGUILayout.TextField( m_ResourceListFilePath ) ;
			}
			GUILayout.EndHorizontal() ;
			
			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false )
			{
				GUILayout.BeginHorizontal() ;
				{
					// ルートフォルダも表示する
					GUILayout.Label( "     Resource Root Folder Path     ", GUILayout.Width( 200f ) ) ;
					GUI.color = Color.yellow ;
					GUILayout.Label( m_ResourceRootFolderPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;
			}

			if( string.IsNullOrEmpty( m_ResourceListFilePath ) == false && string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false )
			{
				// 更新
				GUI.backgroundColor = new Color( 1, 0, 1, 1 ) ;
				if( GUILayout.Button( "Refresh" ) == true )
				{
					m_Refresh = true ;	// 対象更新
				}
				GUI.backgroundColor = Color.white ;
			}

			//-------------------------------------------------------------
				


			//-------------------------------------------------------------

			GUILayout.Space( 12f ) ;
		
			//-------------------------------------------------------------
		
			EditorGUILayout.HelpBox( GetMessage( "SelectAssetBundlePath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				GUI.backgroundColor = new Color( 1, 0.5f, 0, 1 ) ;
				if( GUILayout.Button( "AssetBundle Root Folder Path", GUILayout.Width( 220f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AssetBundleRootFolderPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( System.IO.Directory.Exists( tPath ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							tPath = tPath.Replace( "\\", "/" ) ;
						}
						else
						{
							// ファイルを指定しています
							tPath = tPath.Replace( "\\", "/" ) ;
						
	//						Object tObject = Selection.objects[ 0 ] ;
	//						Debug.Log( "Type:"+tObject.GetType().ToString() ) ;
						
							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int s = tPath.LastIndexOf( '.' ) ;
							if( s >= 0 )
							{
								tPath = tPath.Substring( 0, s ) ;
							}
						
							// 最後のフォルダ区切り位置を取得する
							s = tPath.LastIndexOf( '/' ) ;
							if( s >= 0 )
							{
								tPath = tPath.Substring( 0, s ) ;
							}
						}
					
						m_AssetBundleRootFolderPath = tPath + "/" ;
					}

					// プラットフォーム自動設定
					tPath = m_AssetBundleRootFolderPath ;
					if( string.IsNullOrEmpty( tPath ) == false )
					{
						string[] tFolderName = tPath.Split( '/' ) ;
						string tSmallFolderName ;
						if( tFolderName != null && tFolderName.Length >= 2 )
						{
							l = tFolderName.Length - 1 ;

							for( i  = 0 ; i <  l ; i ++ )
							{
								if( string.IsNullOrEmpty( tFolderName[ i ] ) == false )
								{
									tSmallFolderName = tFolderName[ i ].ToLower() ;
									if( tSmallFolderName == "windows" )
									{
										m_BuildTarget = BuildTarget.StandaloneWindows ;
										break ;
									}
									else
									if( tSmallFolderName == "android" )
									{
										m_BuildTarget = BuildTarget.Android ;
										break ;
									}
									else
									if( tSmallFolderName == "ios" || tSmallFolderName == "iphone" )
									{
										m_BuildTarget = BuildTarget.iOS ;
									}
								}
							}
						}
					}
				}
				GUI.backgroundColor = Color.white ;
			
				// 保存パス
				if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
				{
					GUI.color = Color.yellow ;
					GUILayout.Label( "Select AssetBundle Root Folder Path." ) ;
					GUI.color = Color.white ;
				}
				else
				{
					m_AssetBundleRootFolderPath = EditorGUILayout.TextField( m_AssetBundleRootFolderPath ) ;
				}
			}
			GUILayout.EndHorizontal() ;
		
			//-----------------------------------------------------
		
			// ターゲットプラットフォームと圧縮指定
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				GUILayout.Label( "Build Target", GUILayout.Width( 80 ) ) ;	// null でないなら 74
			
				BuildTarget tBuildTarget = ( BuildTarget )EditorGUILayout.EnumPopup( m_BuildTarget ) ;
				if( tBuildTarget != m_BuildTarget )
				{
					m_BuildTarget  = tBuildTarget ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool tChunkBasedCompression = EditorGUILayout.Toggle( m_ChunkBasedCompression, GUILayout.Width( 10f ) ) ;
				if( tChunkBasedCompression != m_ChunkBasedCompression )
				{
					m_ChunkBasedCompression  = tChunkBasedCompression ;
				}
				GUILayout.Label( "Chunk Based Compression", GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool tForceRebuildAssetBundle = EditorGUILayout.Toggle( m_ForceRebuildAssetBundle, GUILayout.Width( 10f ) ) ;
				if( tForceRebuildAssetBundle != m_ForceRebuildAssetBundle )
				{
					m_ForceRebuildAssetBundle  = tForceRebuildAssetBundle ;
				}
				GUILayout.Label( "Force Rebuild AssetBundle", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool tIgnoreTypeTreeChanges = EditorGUILayout.Toggle( m_IgnoreTypeTreeChanges, GUILayout.Width( 10f ) ) ;
				if( tIgnoreTypeTreeChanges != m_IgnoreTypeTreeChanges )
				{
					m_IgnoreTypeTreeChanges  = tIgnoreTypeTreeChanges ;
					if( m_IgnoreTypeTreeChanges == true )
					{
						m_DisableWriteTypeTree = false ;
					}
				}
				GUILayout.Label( "Ignore Type Tree Changes",  GUILayout.Width( 160f ) ) ;

				GUILayout.Label( " " ) ;

				bool tDisableWriteTypeTree = EditorGUILayout.Toggle( m_DisableWriteTypeTree, GUILayout.Width( 10f ) ) ;
				if( tDisableWriteTypeTree != m_DisableWriteTypeTree )
				{
					m_DisableWriteTypeTree  = tDisableWriteTypeTree ;
					if( m_DisableWriteTypeTree == true )
					{
						m_IgnoreTypeTreeChanges = false ;
					}
				}
				GUILayout.Label( "Disable Write TypeTree", GUILayout.Width( 160f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Space(  6f ) ;
		
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool tCollectDependencies = EditorGUILayout.Toggle( m_CollectDependencies, GUILayout.Width( 10f ) ) ;
				if( tCollectDependencies != m_CollectDependencies )
				{
					m_CollectDependencies  = tCollectDependencies ;
					m_Refresh = true ;	// リスト更新
				}
				GUI.color = Color.yellow ;
				GUILayout.Label( "Collect Dependencies ( Legacy Type )", GUILayout.Width( 240f ) ) ;
				GUI.color = Color.white ;

				GUILayout.Label( " " ) ;

				bool tGenerateCRCFile = EditorGUILayout.Toggle( m_GenerateCRCFile, GUILayout.Width( 10f ) ) ;
				if( tGenerateCRCFile != m_GenerateCRCFile )
				{
					m_GenerateCRCFile  = tGenerateCRCFile ;
				}
				GUILayout.Label( "Generate CRC File", GUILayout.Width( 160f ) ) ;

			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			//-----------------------------------------------------
		
			GUILayout.Space( 24f ) ;

			//-------------------------------------------------------------
		
			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == false && m_ResourceRootFolderPath == m_AssetBundleRootFolderPath )
			{
				// 同じパスを指定するのはダメ
				EditorGUILayout.HelpBox( GetMessage( "SamePath" ), MessageType.Warning ) ;
			
				return ;
			}
		
			//-------------------------------------------------------------
		
			// ここからが重要

	//		Debug.LogWarning( "R:" + tButtonR + " A:" + tButtonA + " R:" + mRefresh ) ;
		
			if( string.IsNullOrEmpty( m_ResourceRootFolderPath ) == true )
			{
				return ;
			}
		
			//----------------------------------

			//　アップデートフラグを更新する
			if( m_Refresh == true )
			{
				m_Refresh = false ;
			
				// アセットバンドル情報を読み出す
				m_AssetBundleFileList = GetAssetBundleFileList() ;
			}
		
			// アセットバンドル化対象リストを表示する
			if( m_AssetBundleFileList == null || m_AssetBundleFileList.Length == 0 )
			{
				return ;
			}

			// トータルのアセットバンドル数とリソース数を計算する
			int ta = m_AssetBundleFileList.Length ;
			int tr = 0 ;
			for( i  = 0 ; i <  ta ; i ++ )
			{
				tr = tr + m_AssetBundleFileList[ i ].assetFile.Count ;
			}
			
			//---------------------------------------------------------
			
			// 生成ボタン（Create , Create And Replace , Replace
			bool tExecute = false ;
			
			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == false && Directory.Exists( m_AssetBundleRootFolderPath ) == true )
			{
				// 更新または新規作成対象が存在する
				GUILayout.BeginHorizontal() ;
				{
					// 生成
					GUI.backgroundColor = new Color( 0, 1, 0, 1 ) ;
					if( GUILayout.Button( "Create Or Update" ) == true )
					{
						tExecute = true ;
					}
					GUI.backgroundColor = Color.white ;
				
					// 同時にリソースリスト外のファイル・フォルダを削除する
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
					m_Clean = EditorGUILayout.Toggle( m_Clean, GUILayout.Width( 10f ) ) ;
					GUILayout.Label( "Clean", GUILayout.Width( 40 ) ) ;
					GUILayout.Label( "", GUILayout.Width( 10 ) ) ;
				}
				GUILayout.EndHorizontal() ;
			}

			//---------------------------------------------------------
			
			// リストを表示する
			
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				m_ShowResourceElements = EditorGUILayout.Toggle( m_ShowResourceElements, GUILayout.Width( 10f ) ) ;
				GUILayout.Label( "Show Resource Elements" ) ;	// null でないなら
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
				
			GUILayout.Space(  6f ) ;
			
			GUILayout.Label( "Asset Bundle : " + ta + "  from Resource : " + tr ) ;
			
			string tFA = "{0,0:d" + ta.ToString ().Length +"}" ;
			string tFR = "{0,0:d" + tr.ToString ().Length +"}" ;
			// 0 無しは "{0," + tNumber.Length + "}"
			
			//-------------------------------------------------
		
			int j ;
			
			Color c0 ;
			Color c1 ;
			
			// スクロールビューで表示する
			m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
			{
				// 表示が必要な箇所だけ表示する
				for( i  = 0 ; i <  ta ; i ++ )
				{
					// アセット情報
					
					// 更新の必要がある
					c0 = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;
					c1 = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;
					
					GUILayout.BeginHorizontal() ;
					{
						// 横一列
						GUI.color = c0 ;

						GUILayout.Label( string.Format( tFA, i ) + " : ", GUILayout.Width( 40f ) ) ;
						string ac = "" ;
					
						if( m_CollectDependencies == false )
						{
							ac = " [ " + m_AssetBundleFileList[ i ].assetFile.Count +" ]" ;
						}
						else
						{
							int[] st = { 0, 0 } ;

							int t ;
							for( t  = 0 ; t <  m_AssetBundleFileList[ i ].assetFile.Count ; t ++ )
							{
								st[ m_AssetBundleFileList[ i ].assetFile[ t ].type ] ++ ;
							}

							ac = " [ " + st[ 0 ] + " + " + st[ 1 ] +" ]" ;
						}

						string tName = m_AssetBundleFileList[ i ].name ;
						if( m_AssetBundleFileList[ i ].sourceType == 1 )
						{
							tName = tName + "/" ;
						}
						
						GUILayout.Label( tName + ac ) ;

						GUI.color = Color.white ;
					}
					GUILayout.EndHorizontal() ;
						
					if( m_ShowResourceElements == true )
					{
						GUI.color = c1 ;
						for( j  = 0 ; j <  m_AssetBundleFileList[ i ].assetFile.Count ; j ++ )
						{
							GUILayout.BeginHorizontal() ;
							{
								// 横一列
								if( m_CollectDependencies == true )
								{
									if( m_AssetBundleFileList[ i ].assetFile[ j ].type == 0 )
									{
										GUI.color = Color.white ;
									}
									else
									{
										GUI.color = Color.yellow ;
									}
								}

								GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
								GUILayout.Label( string.Format( tFR, j ) + " : ", GUILayout.Width( 40f ) ) ;
								GUILayout.Label( m_AssetBundleFileList[ i ].assetFile[ j ].path ) ;
							}
							GUILayout.EndHorizontal() ;
						}
						GUI.color = Color.white ;
					}
				}
			}
			GUILayout.EndScrollView() ;
			
			//-------------------------------------------------
			
			if( tExecute == true )
			{
				// アセットバンドル群を生成する
				if( m_AssetBundleRootFolderPath == "Assets/" )
				{
					tExecute = EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "RootPath" ).Replace( "%1", m_AssetBundleRootFolderPath ), GetMessage( "Yes" ), GetMessage( "No" ) ) ;
				}
				
				if( tExecute == true )
				{
//					CreateAssetBundleAll( m_AssetBundleFileList ) ;
					CreateAssetBundleAll() ;	// 表示と状態が変わっている可能性があるのでリストは作り直す
					
					// 表示を更新
					m_Refresh = true ;
					Repaint() ;
					
					// 結果のアラートダイアログを表示する
					EditorUtility.DisplayDialog( "Build Asset Bundle", GetMessage( "Succeed" ), GetMessage( "OK" ) ) ;
				}
			}

			// EditorUserBuildSettings.activeBuildTarget
		}

		// 選択中のファイルが変更された際に呼び出される
		void OnSelectionChange()
		{
			Repaint() ;
		}

		//-----------------------------------------------------------------------------------------------------


		// アセットバンドルの生成リストを取得する
		private AssetBundleFile[] GetAssetBundleFileList()
		{
			//-------------------------------------------------------------
			
			if( File.Exists( m_ResourceListFilePath ) == false )
			{
				Debug.Log( "[Log]Error : File not found !! : " + m_ResourceListFilePath ) ;
				return null ;
			}

			// リストファイルを読み出す
			TextAsset tTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( m_ResourceListFilePath ) ;
			string tText = tTextAsset.text ;

			if( string.IsNullOrEmpty( tText ) == true )
			{
				Debug.Log( "[Log]Error : Bad list file !! : " + m_ResourceListFilePath ) ;
				return null ;
			}
			
			
			string[] tFile = tText.Split( '\n', ( char )0x0D, ( char )0x0A ) ;
			if( tFile == null || tFile.Length == 0 )
			{
				return null ;
			}
			
			//-------------------------------------------------------------

			List<AssetBundleFile> tList = new List<AssetBundleFile>() ;
			
			string tPath ;

			string[] tWildPath ;

			bool tWildCard ;
			bool tFolderOnly ;

			int i, l = tFile.Length, j, m ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tPath = GetLowerPath( tFile[ i ], out tWildCard, out tFolderOnly ) ;
				if( tPath != null )
				{
					// 有効なパス指定

					tWildPath = GetUpperPath( tPath ) ;
					if( tWildPath != null && tWildPath.Length >  0 )
					{
						m = tWildPath.Length ;
						for( j  = 0 ; j <  m ; j ++ )
						{
							// 生成するアセットバンドル情報を追加する
							AddAssetBundleFile( tWildPath[ j ], m_ResourceRootFolderPath + tWildPath[ j ], tWildCard, tFolderOnly, ref tList ) ;
						}
					}
				}
			}

			//-------------------------------------------------------------

			if( tList.Count == 0 )
			{
				return null ;
			}
		
			//-----------------------------------------------------
		
			return tList.ToArray() ;
		}

		// 生成するアセットバンドル情報を追加する
		private void AddAssetBundleFile( string tPath, string tResourcePath, bool tWildCard, bool tFolderOnly, ref List<AssetBundleFile> tList )
		{
			int i, l, p ;

			string tParentPath, tName ;
			string[] tTargetPath ;

			if( tWildCard == false )
			{
				// 単体

				// １つ親のフォルダを取得する
				p = tResourcePath.LastIndexOf( '/' ) ;
				if( p <  0 )
				{
					// ありえない
					return ;
				}

				tParentPath = tResourcePath.Substring( 0, p ) ;
				if( tParentPath.Length <  0 )
				{
					// ありえない
					return ;
				}

				// 親フォルダ内の全てのフォルダまたはファイルのパスを取得する
				if( tFolderOnly == false && Directory.Exists( tParentPath ) == true )
				{
					tTargetPath = Directory.GetFiles( tParentPath ) ;
					if( tTargetPath != null && tTargetPath.Length >  0 )
					{
						l = tTargetPath.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							// 拡張子は関係無く最初にヒットしたものを対象とする(基本的に同名のフォルダとファイルを同一フォルダ内に置いてはならない)

							tTargetPath[ i ] = tTargetPath[ i ].Replace( "\\", "/" ) ;
							if( tTargetPath[ i ].Contains( tPath ) == true )
							{
								// 対象はフォルダまたはファイル
								if( CheckType( tTargetPath[ i ] ) == true )
								{
									// 決定(単独ファイル)

									AssetBundleFile tABF = new AssetBundleFile() ;
															
									tABF.name	= tPath ;	// 出力パス(相対)
						
									// コードで対象指定：単独ファイルのケース
									tABF.AddAssetFile( tTargetPath[ i ], 0 ) ;
									
									tABF.sourceType = 0 ;	// 単独ファイル

									if( m_CollectDependencies == true )
									{
										// 依存対象のアセットも内包対象に追加する
										tABF.CollectDependencies() ;
									}

									// リストに加える
									tList.Add( tABF ) ;

//									Debug.LogWarning( "AB0:" + tABF.name + " " + tABF.assetFile.Count ) ;

									// 終了
									return ;
								}
							}
						}
					}
				}

				// フォルダ
				if( Directory.Exists( tResourcePath ) == true )
				{
					AssetBundleFile tABF = new AssetBundleFile() ;
															
					tABF.name	= tPath ;	// 出力パス
							
					// 再帰的に素材ファイルを加える
					AddAssetBundleFile( tABF, tResourcePath ) ;
					
					if( tABF.assetFile.Count >  0 )
					{
						tABF.sourceType = 1 ;	// 複数ファイル

						if( m_CollectDependencies == true )
						{
							// 依存対象のアセットも内包対象に追加する
							tABF.CollectDependencies() ;
						}

//						Debug.LogWarning( "AB1:" + tABF.name + " " + tABF.assetFile.Count ) ;

						// リストに加える
						tList.Add( tABF ) ;

						// 終了
						return ;
					}
				}
			}
			else
			{
				// 複数

				if( Directory.Exists( tResourcePath ) == false )
				{
					return ;
				}

				if( tFolderOnly == false )
				{
					// ファイル
					tTargetPath = Directory.GetFiles( tResourcePath ) ;
					if( tTargetPath != null && tTargetPath.Length >  0 )
					{
						l = tTargetPath.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							tTargetPath[ i ] = tTargetPath[ i ].Replace( "\\", "/" ) ;

							// 対象はファイル
							if( CheckType( tTargetPath[ i ] ) == true )
							{
								// 決定(単独ファイル)

								AssetBundleFile tABF = new AssetBundleFile() ;
								
								tName = tTargetPath[ i ] ;
								p = tName.LastIndexOf( '/' ) ;
								if( p >= 0 )
								{
									p ++ ;
									tName = tName.Substring( p, tName.Length - p ) ;
								}
								p = tName.IndexOf( '.' ) ;
								if( p >= 0 )
								{
									tName = tName.Substring( 0, p ) ;
								}

								tABF.name	= tPath + "/" + tName ;	// 出力パス(相対)
						
								// コードで対象指定：単独ファイルのケース
								tABF.AddAssetFile( tTargetPath[ i ], 0 ) ;
						
								tABF.sourceType = 0 ;	// 単独ファイル

								if( m_CollectDependencies == true )
								{
									// 依存対象のアセットも内包対象に追加する
									tABF.CollectDependencies() ;
								}

//								Debug.LogWarning( "AB0:" + tABF.name + " " + tABF.assetFile.Count ) ;

								// リストに加える
								tList.Add( tABF ) ;
							}
						}
					}
				}

				// フォルダ
				tTargetPath = Directory.GetDirectories( tResourcePath ) ;
				if( tTargetPath != null && tTargetPath.Length >  0 )
				{
					l = tTargetPath.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tTargetPath[ i ] = tTargetPath[ i ].Replace( "\\", "/" ) ;

						AssetBundleFile tABF = new AssetBundleFile() ;
															
						tName = tTargetPath[ i ] ;
						p = tName.LastIndexOf( '/' ) ;
						if( p >= 0 )
						{
							p ++ ;
							tName = tName.Substring( p, tName.Length - p ) ;
						}

						tABF.name = tPath + "/" + tName ;	// 出力パス
							
						// 再帰的に素材ファイルを加える
						AddAssetBundleFile( tABF, tTargetPath[ i ] ) ;
					
						if( tABF.assetFile.Count >  0 )
						{
							tABF.sourceType = 1 ;	// 複数ファイル

							if( m_CollectDependencies == true )
							{
								// 依存対象のアセットも内包対象に追加する
								tABF.CollectDependencies() ;
							}

//							Debug.LogWarning( "AB1:" + tABF.name + " " + tABF.assetFile.Count ) ;

							// リストに加える
							tList.Add( tABF ) ;
						}
					}
				}
			}
		}

		// アセットバンドルの要素をリストに追加していく（再帰版）
		private void AddAssetBundleFile( AssetBundleFile tABF, string tCurrentPath )
		{
			int L = tCurrentPath.Length ;
		
			int i, l ;
		
			//-----------------------------------------------------
		
			if( Directory.Exists( tCurrentPath ) == false )
			{
				return ;
			}
		
			//-----------------------------------------------------
		
			// フォルダ
			string[] tDA = Directory.GetDirectories( tCurrentPath ) ;
		
			if( tDA != null && tDA.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				l = tDA.Length ;
				for( i  = 0 ; i <  tDA.Length ; i ++ )
				{
					// サブフォルダを検査
					tDA[ i ] = tDA[ i ].Replace( "\\", "/" ) ;
					AddAssetBundleFile( tABF, tDA[ i ] + "/" ) ;	// 再帰版
				}
			}
		
			// ファイル
			string[] tFA = Directory.GetFiles( tCurrentPath ) ;
		
			if( tFA != null && tFA.Length >  0 )
			{
				l = tFA.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// 対象化コードで反転無効化（は止める）

					tFA[ i ] = tFA[ i ].Replace( "\\", "/" ) ;
					if( CheckType( tFA[ i ] ) == true )
					{
						// コードで対象指定：複数ファイルのケース
						tABF.AddAssetFile( tFA[ i ], 0 ) ;
					}
				}
			}
		}

		// パスを解析して最終的なターゲットパスを取得する
		private string GetLowerPath( string tPath, out bool oWildCard, out bool oFolderOnly )
		{
			oWildCard = false ;
			oFolderOnly = false ;

			tPath = tPath.TrimEnd( '\n', ( char )0x0D, ( char )0x0A ) ;
			tPath = tPath.Trim( ' ' ) ;	// 前後のスペースを削除する

			tPath = tPath.Replace( "**", "*" ) ;
			tPath = tPath.Replace( "**", "*" ) ;

			if( tPath.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 先頭がビックリマークなら除外する
			if( tPath[ 0 ] == '!' )
			{
				// 不可
				return null ;
			}

			// 先頭にスラッシュが付いていれば外す
			tPath = tPath.TrimStart( '/' ) ;
			if( tPath.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にスラッシュが付いていればフォルダ限定
			if( tPath[ tPath.Length - 1 ] == '/' )
			{
				oFolderOnly = true ;	// フォルダ限定
				tPath = tPath.TrimEnd( '/' ) ;
			}

			if( tPath.Length == 0 )
			{
				// 不可
				return null ;
			}

			// 最後にアスタリスクが付いていれば複数対象
			if( tPath[ tPath.Length - 1 ] == '*' )
			{
				oWildCard = true ;	// 対象は親フォルダ内の全て
				tPath = tPath.TrimEnd( '*' ) ;
			}

			// 最後にスラッシュになってしまうようなら除外する
			if( tPath.Length >= 1 )
			{
				if( tPath[ tPath.Length - 1 ] == '/' )
				{
					tPath = tPath.Trim( '/' ) ;
				}
			}

			// パスが空文字の場合もありえる
			return tPath ;
		}

		private string[] GetUpperPath( string tPath )
		{
			if( string.IsNullOrEmpty( tPath ) == true || tPath.Contains( "*" ) == false )
			{
				return new string[]{ tPath } ;
			}

			// ワイルドカード部分を展開して全て個別のパスにする

			List<string> tStackedPath = new List<string>() ;

			// 一時的に最後にスラッシュを付ける
			tPath = tPath + "/" ;

			string tCurrentPath = "" ;

			// 再帰メソッドを呼ぶ
			GetUpperPath( tPath, tCurrentPath, ref tStackedPath ) ;

			if( tStackedPath.Count == 0 )
			{
				// ワイルドカード内で有効なパスは存在しない
				return null ;
			}

			return tStackedPath.ToArray() ;
		}

		// 再帰処理側
		private void GetUpperPath( string tPath, string tCurrentPath, ref List<string> tStackedPath )
		{
			int i, l, p ;
			string tName,  tFixedPath ;

			string[]		tWorks ;
			List<string>	tNames = new List<string>() ;

			//----------------------------------

			p = tPath.IndexOf( "/" ) ;

			// 最初のスラッシュまで切り出し(絶対に０より大きい値になる
			tName = tPath.Substring( 0, p ) ;

			p ++ ;
			tPath = tPath.Substring( p, tPath.Length - p ) ;

			if( tName != "*" )
			{
				// 固定
				tFixedPath = tCurrentPath ;
				if( string.IsNullOrEmpty( tFixedPath ) == false )
				{
					tFixedPath = tFixedPath + "/" ;
				}
				tFixedPath = tFixedPath + tName ;

				if( string.IsNullOrEmpty( tPath ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					GetUpperPath( tPath, tFixedPath, ref tStackedPath ) ;
				}
				else
				{
					// 最終的なパスが決定した
					tStackedPath.Add( tFixedPath ) ;
				}
			}
			else
			{
				// 可変

				if( Directory.Exists( m_ResourceRootFolderPath + tCurrentPath ) == false )
				{
					// フォルダが存在しない
					return ;
				}

				tNames.Clear() ;
				tWorks = Directory.GetDirectories( m_ResourceRootFolderPath + tCurrentPath ) ;
				if( tWorks != null && tWorks.Length >  0 )
				{
					l = tWorks.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tWorks[ i ] = tWorks[ i ].Replace( "\\", "/" ) ;
						if( Directory.Exists( tWorks[ i ] ) == true )
						{
							// フォルダ
							tNames.Add( tWorks[ i ].Replace( m_ResourceRootFolderPath + tCurrentPath + "/", "" ) ) ;
						}
					}
				}

				if( tNames.Count == 0 )
				{
					// この枝は打ち止め
					return ;
				}

				if( string.IsNullOrEmpty( tPath ) == false )
				{
					// まだ続きがある

					// 再帰的に処理する
					l = tNames.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tFixedPath = tCurrentPath ;
						if( string.IsNullOrEmpty( tFixedPath ) == false )
						{
							tFixedPath = tFixedPath + "/" ;
						}
						tFixedPath = tFixedPath + tNames[ i ] ;

						GetUpperPath( tPath, tFixedPath, ref tStackedPath ) ;
					}
				}
				else
				{
					// 最終的なパスが決定した
					l = tNames.Count ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tFixedPath = tCurrentPath ;
						if( string.IsNullOrEmpty( tFixedPath ) == false )
						{
							tFixedPath = tFixedPath + "/" ;
						}
						tFixedPath = tFixedPath + tNames[ i ] ;

						// 追加
						tStackedPath.Add( tFixedPath ) ;
					}
				}
			}
		}


		//-----------------------------------------------------------
	
		// 拡張子をチェックして有効なファイルかどうか判別する
		private bool CheckType( string tPath )
		{
			int s = tPath.LastIndexOf( '.' ) ;
			if( s >= 0 )
			{
				// 拡張子あり
				string tType = tPath.Substring( s + 1, tPath.Length - ( s + 1 ) ) ;
			
				if( string.IsNullOrEmpty( tType ) == false )
				{
	//				Debug.Log ( "Type:" + tType ) ;
			
					if( tType != "meta" )
					{
						return true ;	// meta 以外はＯＫ
					}
				}
			}
		
			return false ;
		}
	
		//-----------------------------------------------------------------

		// 必要なアセットバンドルを全て生成する
		private void CreateAssetBundleAll()
		{
			// コンソールから呼ばれた場合
			AssetBundleFile[] tAssetBundleFileList = GetAssetBundleFileList() ;

			if( tAssetBundleFileList != null && tAssetBundleFileList.Length >  0 )
			{
				CreateAssetBundleAll( tAssetBundleFileList ) ;
			}
		}

		// 必要なアセットバンドルを全て生成する
		private void CreateAssetBundleAll( AssetBundleFile[] tAssetBundleFileList )
		{
			if( tAssetBundleFileList == null || tAssetBundleFileList.Length == 0 )
			{
				return ;
			}
	
			int i, l ;
			bool tResult = false ;
			string[] tAssetBundleName = null ;
			
			//-----------------------------------------------------------------------------
			
			// アセットバンドルファイルの階層が浅い方から順にビルドするようにソートする(小さい値の方が先)

			l = tAssetBundleFileList.Length ;
			
			//-----------------------------------------------------------------------------
		
			// 保存先ルートフォルダ
			string tAssetBundleRootFolderPath = m_AssetBundleRootFolderPath ;
			l = tAssetBundleRootFolderPath.Length ;
			if( tAssetBundleRootFolderPath[ l - 1 ] == '/' )
			{
				tAssetBundleRootFolderPath = tAssetBundleRootFolderPath.Substring( 0, l - 1 ) ;
			}
			
			if( Directory.Exists( tAssetBundleRootFolderPath ) == false )
			{
				Debug.Log( "[Log]Output folder is not found :" + tAssetBundleRootFolderPath ) ;
				return ;
			}


			//----------------------------------

			BuildAssetBundleOptions tOptions = BuildAssetBundleOptions.DeterministicAssetBundle ;

			if( m_ChunkBasedCompression == true )
			{
				tOptions = tOptions | BuildAssetBundleOptions.ChunkBasedCompression ;
			}

			if( m_ForceRebuildAssetBundle == true )
			{
				tOptions = tOptions | BuildAssetBundleOptions.ForceRebuildAssetBundle ;
			}

			if( m_IgnoreTypeTreeChanges == true && m_DisableWriteTypeTree == false )
			{
				tOptions = tOptions | BuildAssetBundleOptions.IgnoreTypeTreeChanges ;
			}

			if( m_DisableWriteTypeTree == true && m_IgnoreTypeTreeChanges == false  )
			{
				tOptions = tOptions | BuildAssetBundleOptions.DisableWriteTypeTree ;
			}

			//---------------------------------------------------------

			if( m_CollectDependencies == false )
			{
				// 新版(依存アセット除外あり)
				l = tAssetBundleFileList.Length ;

				// ここからが新版のメイン生成処理
				AssetBundleBuild[] tMap = new AssetBundleBuild[ l ] ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					PostAssetBundleFile( tAssetBundleFileList[ i ], ref tMap[ i ] ) ;
				}

				//--------------------

				AssetBundleManifest tManifest = null ;

				// アセットバンドルの生成
				tManifest = BuildPipeline.BuildAssetBundles
				(
					tAssetBundleRootFolderPath,
					tMap,
					tOptions,
					m_BuildTarget
				) ;
	
				if( tManifest != null )
				{
					tResult = true ;
					tAssetBundleName = tManifest.GetAllAssetBundles() ;
				}
			}
			else
			{
				// 旧版(依存アセット除外なし)
				l = tAssetBundleFileList.Length ;

				// ここからが新版のメイン生成処理
				AssetBundleBuild[] tMap = new AssetBundleBuild[ 1 ] ;
			
				AssetBundleManifest tManifest = null ;

				string[] tNA ;
				List<string> tNL = new List<string>() ;
				List<string> tHC = new List<string>() ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					PostAssetBundleFile( tAssetBundleFileList[ i ], ref tMap[ 0 ] ) ;

					// アセットバンドルの生成
					tManifest = BuildPipeline.BuildAssetBundles
					(
						tAssetBundleRootFolderPath,
						tMap,
						tOptions,
						m_BuildTarget
					) ;
	
					if( tManifest == null )
					{
						break ;
					}

					tNA = tManifest.GetAllAssetBundles() ;
					if( tNA != null && tNA.Length >  0 )
					{
						tNL.Add( tNA[ 0 ] ) ;	// 常に１つのはず
						tHC.Add( tManifest.GetAssetBundleHash( tNA[ 0 ] ).ToString() ) ; 
					}
				}

				if( i >= l )
				{
					tResult = true ;
					tAssetBundleName = tNL.ToArray() ;

					// ハッシュリストを生成して保存する
					l = tNL.Count ;
					string tText = "" ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tText = tText + tNL[ i ] + "," + tHC[ i ] + "\n" ;
					}

					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".list", tText ) ;
				}
			}

			//-------------------------------------------------------------------------------

			// ＣＲＣファイルを出力する
			if( m_GenerateCRCFile == true )
			{
//				Debug.LogWarning( "保存先ルートフォルダ:" + m_AssetBundleRootFolderPath ) ;
//				Debug.LogWarning( "保存マニフェスト名:" + GetAssetBundleRootName() ) ;

				l = tAssetBundleFileList.Length ;
				
				byte[] tData ;
				int tSize ;
				uint tCRC ;
				string tText = "" ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					tData = File.ReadAllBytes( m_AssetBundleRootFolderPath + tAssetBundleFileList[ i ].name ) ;
					if( tData != null && tData.Length >  0 )
					{
						tSize = tData.Length ;
						tCRC = GetCRC32( tData ) ;
						tText = tText + tAssetBundleFileList[ i ].name + "," + tSize + "," + tCRC + "\n" ;
					}

//					Debug.LogWarning( "アセットバンドルファイルのパス:" + tAssetBundleFileList[ i ].name ) ;
				}

				if( string.IsNullOrEmpty( tText ) == false )
				{
					File.WriteAllText( m_AssetBundleRootFolderPath + GetAssetBundleRootName() + ".crc", tText ) ;
				}
			}

			//-------------------------------------------------------------------------------

			if( m_Clean == true && tResult == true && tAssetBundleName != null )
			{
				// 余計なファイルを削除する
				CleanAssetBundle( tAssetBundleName ) ;
			}
		
			AssetDatabase.Refresh() ;
		}

		// アセットバンドルの情報を格納する
		private void PostAssetBundleFile( AssetBundleFile tAssetBundleFile, ref AssetBundleBuild tMap )
		{
			string tName = tAssetBundleFile.name ;

			tMap.assetBundleName = tName ;
			tMap.assetBundleVariant = "" ;
		
//			Debug.LogWarning( "Map assetBundleName:" + tMap.assetBundleName ) ;
			
			List<string> tAssetPath = new List<string>() ;
		
			int i, l = tAssetBundleFile.assetFile.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				// このアセット自体は確実にアセットバンドルに含まれる
				if( tAssetBundleFile.assetFile[ i ].type == 0 )
				{
					tAssetPath.Add( tAssetBundleFile.assetFile[ i ].path ) ;
//					Debug.LogWarning( "含まれるリソース:" + tAssetBundleFile.assetFile[ i ].path ) ;

					// 依存関係にあるアセットを表示する
//					DebugPrintDependencies( tAssetBundleFile.assetFile[ i ].path ) ;
				}
			}
		
			tMap.assetNames = tAssetPath.ToArray() ;
		}

		private const uint CRC32_MASK = 0xffffffff ;
	
		// ＣＲＣ値を取得する
		public static uint GetCRC32( byte[] tData )
		{
			uint tValue = CRC32_MASK  ;
			
			int i, l = tData.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tValue = m_CRC32_Table[ ( tValue ^ tData[ i ] ) & 0xFF ] ^ ( tValue >> 8 ) ;
			}
		
			return tValue ^ CRC32_MASK ;
		}
	
		private readonly static uint[] m_CRC32_Table = new uint[]
		{
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
			0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
			0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
			0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
			0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
			0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
			0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
			0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
			0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
			0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
			0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
			0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
			0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
			0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
			0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
			0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
			0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
			0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
			0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
			0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
			0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
			0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
			0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
			0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
			0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
			0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
			0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
			0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
			0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
			0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
			0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
			0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
			0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
			0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
			0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
			0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
			0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
			0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
			0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
			0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
			0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
			0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
			0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
			0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
			0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
			0x2d02ef8d
		} ;

		//-----------------------------------------------------------------

		// 出力パスのフォルダ名を取得する
		private string GetAssetBundleRootName()
		{
			if( string.IsNullOrEmpty( m_AssetBundleRootFolderPath ) == true )
			{
				return "" ;
			}

			// 出力パスのアセットバンドル
			string tPath = m_AssetBundleRootFolderPath ;
			if( tPath[ tPath.Length - 1 ] == '/' )
			{
				tPath = tPath.Substring( 0, tPath.Length - 1 ) ;
			}
			int s = tPath.LastIndexOf( '/' ) ;
			if( s >= 0 )
			{
				tPath = tPath.Substring( s + 1, tPath.Length - ( s + 1 ) ) ;
			}

			return tPath ;
		}

		// 不要になったアセットバンドルファイルを削除する
		private void CleanAssetBundle( string[] tName )
		{
			List<string> tList = new List<string>() ;

			//-------------------------------------------------

			// 削除対象から除外するファイル名を登録する

			// 出力パスのアセットバンドル
			string tPath = GetAssetBundleRootName() ;

			// シングルマニフェストアセットバンドル
			if( m_CollectDependencies == false )
			{
				// 依存ありタイプ
				tList.Add( m_AssetBundleRootFolderPath + tPath ) ;
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".meta" ) ;
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".manifest" ) ;
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".manifest.meta" ) ;
			}
			else
			{
				// 依存なしタイプ
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".list" ) ;
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".list.meta" ) ;
			}

			if( m_GenerateCRCFile == true )
			{
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".crc" ) ;
				tList.Add( m_AssetBundleRootFolderPath + tPath + ".crc.meta" ) ;
			}

			// 各アセットバンドル
			if( tName != null && tName.Length >  0 )
			{
				int i, l = tName.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
	//				Debug.LogWarning( "生成されたAB:" + tName[ i ] ) ;

					tList.Add( m_AssetBundleRootFolderPath + tName[ i ] ) ;
					tList.Add( m_AssetBundleRootFolderPath + tName[ i ] + ".meta" ) ;
					tList.Add( m_AssetBundleRootFolderPath + tName[ i ] + ".manifest" ) ;
					tList.Add( m_AssetBundleRootFolderPath + tName[ i ] + ".manifest.meta" ) ;
				}
			}

			//---------------------------------------------------------

			// 再帰を使って不要になったアセットバンドルファイルを全て削除する
			CleanAssetBundle( tList, m_AssetBundleRootFolderPath ) ;
		}

		// 不要になったアセットバンドルファイルを削除(再帰)
		private int CleanAssetBundle( List<string> tList, string tCurrentPath )
		{
			int i ;
		
			string tPath ;
		
			//-----------------------------------------------------
		
			if( Directory.Exists( tCurrentPath ) == false )
			{
				return 0 ;
			}
		
			//-----------------------------------------------------
		
			int c = 0 ;	// フォルダ・ファイルを残すカウント
			int d = 0 ;	// フォルダ・ファイルを消すカウント
		
			// フォルダ
			string[] tDA = Directory.GetDirectories( tCurrentPath ) ;
			if( tDA != null && tDA.Length >  0 )
			{
				// サブフォルダがあるのでさらに検査していく
				for( i  = 0 ; i <  tDA.Length ; i ++ )
				{
					// サブフォルダを検査
					tPath = tDA[ i ] + "/" ;
					if( CleanAssetBundle( tList, tPath ) == 0 )
					{
						// このサブフォルダは残す
						Debug.LogWarning( "削除対象:" + tPath ) ;
						System.IO.Directory.Delete( tPath, true ) ;

						d ++ ;
					}
					else
					{
						// フォルダのメタファイルを削除対象から除外する
						if( tPath[ tPath.Length - 1 ] == '/' )
						{
							tPath = tPath.Substring( 0, tPath.Length - 1 ) ;
						}
						tPath = tPath + ".meta" ;

						if( tList.Contains( tPath ) == false )
						{
							tList.Add( tPath ) ;
						}

						c ++ ;
					}
				}
			}
		
			// ファイル
			string[] tFA = Directory.GetFiles( tCurrentPath ) ;
			if( tFA != null && tFA.Length >  0 )
			{
				for( i  = 0 ; i <  tFA.Length ; i ++ )
				{
					if( tList.Contains( tFA[ i ] ) == false )
					{
						// 削除対象

//						Debug.LogWarning( "削除対象:" + tFA[ i ] ) ;
						File.Delete( tFA[ i ] ) ;

						d ++ ;
					}
					else
					{
						c ++ ;
					}
				}
			}
		
			Debug.Log( "Deleted Count : " + tCurrentPath + " = " + d ) ;
		
			return c ;
		}
	
		// 依存するアセットをコンソールに表示する
		private void DebugPrintDependencies( string tPath )
		{
			// 依存関係にあるアセットを検出する
			string[] tCheckPath = AssetDatabase.GetDependencies( tPath ) ;
			if( tCheckPath!= null && tCheckPath.Length >  0 )
			{
				int j ;
				for( j  = 0 ; j <  tCheckPath.Length ; j ++ )
				{
					Debug.LogWarning( "依存:" + tCheckPath[ j ] ) ;
				}
			}
		}


		public void OnPostprocessAssetbundleNameChanged( string assetPath, string previousAssetBundleName, string newAssetBundleName )
		{
			Debug.Log( "Asset " + assetPath + " has been moved from assetBundle " + previousAssetBundleName + " to assetBundle " + newAssetBundleName + "." ) ;
		}


		//----------------------------------------------------------------------------------------------


		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SelectResourcePath",		"AssetBundle化したいファイル一覧が記述されたリストファイルを設定してください" },
			{ "SelectAssetBundlePath",	"生成したAssetBundleを格納するフォルダを設定してください" },
			{ "SelectAllResource",		"AssetBundle化対象はプロジェクト全体のAssetLabel入力済みファイルとなります" },
			{ "SamePath",				"ResourceフォルダとAssetBundleフォルダに同じものは指定できません" },
			{ "RootPath",				"プロジェクトのルートフォルダ\n\n%1\n\nにAssetBundleを生成します\n\n本当によろしいですか？" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"はい" },
			{ "No",						"いいえ" },
			{ "OK",						"閉じる" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SelectResourcePath",		"Please set up a list file that lists the files you want AssetBundle." },
			{ "SelectAssetBundlePath",	"Please set the folder in which to store the generated AssetBundle." },
			{ "SelectAllResource",		"AssetBundle target will be AssetLabel entered file of the entire project." },
			{ "SamePath",				"The same thing can not be specified in the Resource folder and AssetBundle folder." },
			{ "RootPath",				"Asset Bundle Root Path is \n\n '%1'\n\nReally ?" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"All Succeed !!" },
			{ "No",						"No" },
			{ "OK",						"OK" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( mJapanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return mJapanese_Message[ tLabel ] ;
			}
			else
			{
				if( mEnglish_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return mEnglish_Message[ tLabel ] ;
			}
		}

		//----------------------------------------------------------------------------

		// コマンドラインからの実行可能版
		public static bool BatchBuild()
		{
			string tPath = GetConfigurationFilePtah() ;
			if( string.IsNullOrEmpty( tPath ) == true )
			{
				Debug.Log( "Error : Bad Configuration File !!" ) ;
			
				return false ;
			}
	
			return BatchBuild( tPath ) ;
		}
	
		public static bool BatchBuild( string tPath )
		{
			SimpleAssetBundleBuilder tSABB = ScriptableObject.CreateInstance<SimpleAssetBundleBuilder>() ;
		
			if( tSABB.LoadConfiguration( tPath ) == false )
			{
				DestroyImmediate( tSABB ) ;
			
				Debug.Log( "Error : Bad Configuration File !!" ) ;
			
				return false ;
			}
		
			//-----------------------------------------
			
			// アセットバンドルをビルドする
			tSABB.CreateAssetBundleAll() ;
		
			// 最後にオブジェクトを破棄する
			DestroyImmediate( tSABB ) ;
			
			return true ;
		}
	
	
		// コンフィギュレーションファイルのパスを取得する
		private static string GetConfigurationFilePtah()
		{
			string[] tArgs = System.Environment.GetCommandLineArgs() ;
			if( tArgs == null || tArgs.Length == 0 )
			{
				return null ;
			}
		
			string tPath = "" ;
		
			int i, l = tArgs.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tArgs[ i ].ToLower() == "-setting" && ( i + 1 ) <  l )
				{
					// 発見した
					tPath = tArgs[ i + 1 ] ;
					break ;
				}
			}
		
			if( i >= l || string.IsNullOrEmpty( tPath ) == true )
			{
				return null ;
			}
		
	//		if( tPath[ 0 ] == '/' )
	//		{
	//			l = tPath.Length ;
	//			tPath = tPath.Substring( 1, l - 1 ) ;
	//		}
	//		
	//		tPath = "Assets/" + tPath ;
		
		
			return tPath ;
		}
	
	
		// コンフィグ情報を読み出す
		private bool LoadConfiguration( string tPath )
		{
	//		Debug.Log( "ConfigPath:" + tPath ) ;
		
			if( File.Exists( tPath ) == false )
			{
				return false ;
			}
		
			string tCode = File.ReadAllText( tPath ) ;
		
			if( string.IsNullOrEmpty( tCode ) == true )
			{
				return false ;
			}
		
	//		TextAsset tText = AssetDatabase.LoadAssetAtPath( tPath, typeof( TextAsset ) ) as TextAsset ;
	//		if( tText == null )
	//		{
	//			return false ;
	//		}
	//		
	//		if( string.IsNullOrEmpty( tText.text ) == true )
	//		{
	//			return true ;
	//		}
	//		
	//		string tCode = tText.text ;
		
			//-------------------------------------------------
		
			string[] tLine = tCode.Split( '\n' ) ;
			int i, l = tLine.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tLine[ i ] = tLine[ i ].Replace( " ", "" ) ;
				tLine[ i ] = tLine[ i ].Replace( "\n", "" ) ;
				tLine[ i ] = tLine[ i ].Replace( "\t", "" ) ;
				tLine[ i ] = tLine[ i ].Replace( "\r", "" ) ;	// 超重要
				string[] tData = tLine[ i ].Split( '=' ) ;
			
				if( tData.Length == 2 )
				{
					string tLabel = tData[ 0 ].ToLower() ;
					string tValue = tData[ 1 ] ;
				
					if( tLabel == "ResourceListFilePath".ToLower() )
					{
						m_ResourceListFilePath = CollectFilePath( tValue ) ;
						Debug.Log( "[Log]ResourceListFilePath:" + m_ResourceListFilePath ) ;
					}

					if( tLabel == "ResourceRootFolderPath".ToLower() )
					{
						m_ResourceRootFolderPath = CollectFolderPath( tValue ) ;
						Debug.Log( "[Log]ResourceRootFolderPath:" + m_ResourceRootFolderPath ) ;
					}

					if( tLabel == "AssetBundleRootFolderPath".ToLower() )
					{
						m_AssetBundleRootFolderPath = CollectFolderPath( tValue ) ;
						Debug.Log( "[Log]m_AssetBundleRootFolderPath:" + m_AssetBundleRootFolderPath ) ;
					}

					if( tLabel == "BuildTarget".ToLower() )
					{
						m_BuildTarget = GetBuildTarget( tValue ) ;
					}

					if( tLabel == "ChunkBasedCompression".ToLower() )
					{
						m_ChunkBasedCompression = GetBoolean( tValue ) ;
					}

					if( tLabel == "ForceRebuildAssetBundle".ToLower() )
					{
						m_ForceRebuildAssetBundle = GetBoolean( tValue ) ;
					}

					if( tLabel == "IgnoreTypeTreeChanges".ToLower() )
					{
						m_IgnoreTypeTreeChanges = GetBoolean( tValue ) ;
					}

					if( tLabel == "DisableWriteTypeTree".ToLower() )
					{
						m_DisableWriteTypeTree = GetBoolean( tValue ) ;
					}

					if( tLabel == "CollectDependencies".ToLower() )
					{
						m_CollectDependencies = GetBoolean( tValue ) ;
					}
				}
			}
		
			return true ;
		}
	
		// パスの整形を行う
		private string CollectFolderPath( string tPath )
		{
			if( string.IsNullOrEmpty( tPath ) == true )
			{
				return tPath ;
			}
		
			tPath = tPath.Replace( "\\", "/" ) ;
		
			if( tPath[ 0 ] == '/' )
			{
				tPath = tPath.Substring( 1, tPath.Length - 1 ) ;
			}
		
			if( tPath.Length <  1 )
			{
				return tPath ;
			}
		
			if( tPath[ tPath.Length - 1 ] != '/' )
			{
				tPath = tPath + "/" ;
			}
		
			return tPath ;
		}
	
		// パスの整形を行う
		private string CollectFilePath( string tPath )
		{
			if( string.IsNullOrEmpty( tPath ) == true )
			{
				return tPath ;
			}
		
			tPath = tPath.Replace( "\\", "/" ) ;
		
			if( tPath[ 0 ] == '/' )
			{
				tPath = tPath.Substring( 1, tPath.Length - 1 ) ;
			}
		
			return tPath ;
		}

		// ブーリアン結果を取得する
		private bool GetBoolean( string tValue )
		{
			tValue = tValue.ToLower() ;
		
			bool tBoolean = false ;
		
			if( tValue == "false".ToLower() )
			{
				tBoolean = false ;
			}
			else
			if( tValue == "true".ToLower() )
			{
				tBoolean = true ;
			}
		
			return tBoolean ;
		}


		// ビルドターゲットを取得する
		private BuildTarget GetBuildTarget( string tValue )
		{
			tValue = tValue.ToLower() ;
		
			BuildTarget tBuildTarget = BuildTarget.Android ;
		
			if( tValue == "StandaloneOSXUniversal".ToLower() )
			{
				tBuildTarget = BuildTarget.StandaloneOSX ;
			}
			else
			if( tValue == "StandaloneWindows".ToLower() || tValue == "Windows".ToLower() )
			{
				tBuildTarget = BuildTarget.StandaloneWindows ;
			}
			else
			if( tValue == "iPhone".ToLower() || tValue == "iOS".ToLower() )
			{
				tBuildTarget = BuildTarget.iOS ;
			}
			else
			if( tValue == "Android".ToLower() )
			{
				tBuildTarget = BuildTarget.Android ;
			}
			else
			if( tValue == "StandaloneWindows64".ToLower() || tValue == "Windows64".ToLower() )
			{
				tBuildTarget = BuildTarget.StandaloneWindows64 ;
			}
			else
			if( tValue == "StandaloneLinux64".ToLower() )
			{
				tBuildTarget = BuildTarget.StandaloneLinux64 ;
			}
		
			return tBuildTarget ;
		}
	}
}

