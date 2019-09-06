using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

/// <summary>
/// ウェーブノートピッカーパッケージ
/// </summary>
namespace SimpleWaveNotePicker
{
	/// <summary>
	/// ウェーブノートピッカークラス(エディター用) Version 2017/10/20 0
	/// </summary>
	public class SimpleWaveNotePicker : EditorWindow
	{
		[ MenuItem( "Tools/Simple WaveNote Picker" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleWaveNotePicker>( false, "Wave Note Picker", true ) ;
		}

		//----------------------------------------------------------

		// 有音無音状態
		internal protected class Note
		{
			internal protected int		state ;
			internal protected float	value ;

			internal protected Note( int tState, float tValue )
			{
				state	= tState ;
				value	= tValue ;

//				if( state == 1 )
//				{
//					Debug.LogWarning( "●有音期間開始位置:" + tValue ) ;
//				}
//				else
//				{
//					Debug.LogWarning( "○無音期間開始位置:" + tValue ) ;
//				}
			}
		}
		

		private string	m_OutputPath = "" ;

		private float	m_ThresholdVolume = 0.05f ;
		private float	m_ThresholdLength = 0.5f ;

		private Vector2 m_ScrollPosition ;

		//----------------------------------------------------------

		// レイアウトを描画する
		private void OnGUI()
		{
			string tPath ;

			bool tExecute  = false ;

			//----------------------------------------------------------

			// 保存先のパスの設定
			EditorGUILayout.HelpBox( GetMessage( "SelectOutputPath" ), MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Output Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_OutputPath = "Assets/" ;
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
							m_OutputPath = tPath + "/" ;
						}
						else
						{
							// ファイルを指定しています
							tPath = tPath.Replace( "\\", "/" ) ;
							m_OutputPath = tPath ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int tIndex = m_OutputPath.LastIndexOf( '/' ) ;
							if( tIndex >= 0 )
							{
								m_OutputPath = m_OutputPath.Substring( 0, tIndex ) + "/" ;
							}
						}
					}
				}
			
				// 保存パス
				m_OutputPath = EditorGUILayout.TextField( m_OutputPath ) ;
			}
			GUILayout.EndHorizontal() ;

			// 無音判定音量(0.01 ～ 0.25)
			m_ThresholdVolume = EditorGUILayout.Slider( "Threshold Volume", m_ThresholdVolume, 0.01f, 0.25f ) ;

			// 無音判定時間(0.25 ～ 1.5)
			m_ThresholdLength = EditorGUILayout.Slider( "Threshold Length", m_ThresholdLength, 0.25f, 1.25f ) ;


			if( string.IsNullOrEmpty( m_OutputPath ) == false && Directory.Exists( m_OutputPath ) == true )
			{
				EditorGUILayout.HelpBox( GetMessage( "SelectSourceFile" ), MessageType.Info ) ;


				string[] tSourcePath = GetSourcePath() ;

				if( tSourcePath != null && tSourcePath.Length >  0 )
				{
					// 生成ボタン

					GUILayout.BeginHorizontal() ;
					{
						GUI.backgroundColor = Color.green ;
						tExecute = GUILayout.Button( "Create" ) ;
						GUI.backgroundColor = Color.white ;
					}
					GUILayout.EndHorizontal() ;

					//--------------------------------------------------------

					int i, l = tSourcePath.Length ;

					// リストを表示する

					EditorGUILayout.Separator() ;
			
					EditorGUILayout.LabelField( "Target (" + l + ")" ) ;

					GUILayout.BeginVertical() ;
					{
						m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
						{
							for( i  = 0 ; i <  l ; i ++ )
							{
								tPath = tSourcePath [ i ] ;
						
								GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
								{
									GUI.color = Color.cyan ;
									GUILayout.Label( tPath, GUILayout.Height( 20f ) ) ;
									GUI.color = Color.white ;
								}
								GUILayout.EndHorizontal() ;		// 横一列終了
							}
						}
						GUILayout.EndScrollView() ;
					}
					GUILayout.EndVertical() ;
				}

				//-------------------------------------------------------------------------

				if( tExecute == true )
				{
					// 生成
					Execute( m_OutputPath, tSourcePath ) ;
				}
			}
		}

		//---------------------------------------------------------------

		// 選択しているものが変化したら再描画する
		private void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//----------------------------------------------------------------------------------------------

		// ソースパス情報一覧を取得する
		private string[] GetSourcePath()
		{
			List<string> tSourcePath = new List<string>() ;
			string tPath ;

			// 選択中の素材を追加する
			if( Selection.objects != null && Selection.objects.Length >  0 )
			{
				foreach( UnityEngine.Object tObject in Selection.objects )
				{
					tPath = AssetDatabase.GetAssetPath( tObject.GetInstanceID() ) ;
					tPath = tPath.Replace( "\\", "/" ) ;

					if( File.Exists( tPath ) == true )
					{
						// このパスはファイル

						if( IsExtension( tPath, "wav", "ogg", "mp3" ) == true )
						{
							if( tSourcePath.Contains( tPath ) == false )
							{
								// パスを追加
								tSourcePath.Add( tPath ) ;
							}
						}
					}
					else
					if( Directory.Exists( tPath ) == true )
					{
						// このパスはフォルダ

						// このパスを起点として再帰的にファイル情報を取得する
						AddSourcePath( tPath, ref tSourcePath ) ;
					}
				}
			}

			return tSourcePath.ToArray() ;
		}

		// 再帰的にファイル情報を取得する
		private void AddSourcePath( string tRootPath, ref List<string> rSourcePath )
		{
			int i, l ;
			string tPath ;

			// フォルダ
			string[] tFolder = Directory.GetDirectories( tRootPath ) ;
			if( tFolder != null && tFolder.Length >  0 )
			{
				// サブフォルダが存在する
				l = tFolder.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tPath = tFolder[ i ] ;
					tPath = tPath.Replace( "\\", "/" ) ;

					AddSourcePath( tPath, ref rSourcePath ) ;
				}
			}

			// ファイル
			string[] tFile = Directory.GetFiles( tRootPath ) ;
			if( tFile != null && tFile.Length >  0 )
			{
				// ファイルが存在する
				l= tFile.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tPath = tFile[ i ] ;
					tPath = tPath.Replace( "\\", "/" ) ;

					if( IsExtension( tPath, "wav", "ogg", "mp3" ) == true )
					{
						if( rSourcePath.Contains( tPath ) == false )
						{
							rSourcePath.Add( tPath ) ;
						}
					}
				}
			}
		}

		// 拡張子を確認する
		private bool IsExtension( string tPath, params string[] tExtension )
		{
			if( string.IsNullOrEmpty( tPath ) == true || tExtension == null || tExtension.Length == 0 )
			{
				return false ;
			}

			tPath = tPath.ToLower() ;

			int i = tPath.LastIndexOf( '.' ) ;
			if( i <  0 )
			{
				return false ;
			}

			int l = tPath.Length ;

			tPath = tPath.Substring( i + 1, l - ( i + 1 ) ) ;

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				return false ;
			}

			l = tExtension.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tPath == tExtension[ i ].ToLower() )
				{
					// 該当の拡張子です
					return true ;
				}
			}

			return false ;
		}

		//----------------------------------------------------------------------------------------------

		// 出力する
		private void Execute( string tOutputPath, string[] tSourcePath )
		{
			int i, j, l, m ;

			Note[] tNote ;

			l = tOutputPath.Length ;
			if( tOutputPath[ l - 1 ] != '/' )
			{
				tOutputPath =tOutputPath + "/" ;
			}

			string tName ;

			l = tSourcePath.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tNote = LoadNote( tSourcePath[ i ] ) ;

				if( tNote != null && tNote.Length >  0 )
				{
					tName = tSourcePath[ i ] ;
					m = tName.Length ;
					j = tName.LastIndexOf( '/' ) ;
					if( j >= 0 )
					{
						tName = tName.Substring( j + 1, m - ( j + 1 ) ) ;
						if( string.IsNullOrEmpty( tName ) == false )
						{
							j = tName.LastIndexOf( '.' ) ;
							if( j >= 0 )
							{
								// 拡張子あり
								tName = tName.Substring( 0, j ) ;
							}

							tName = tName + ".txt" ;

							if( string.IsNullOrEmpty( tName ) == false )
							{
								SaveNote( tOutputPath + tName, tNote ) ;

								AssetDatabase.SaveAssets() ;
								AssetDatabase.Refresh() ;
							}
						}
					}

					Resources.UnloadUnusedAssets() ;
				}
			}
		}
		
		// 有音期間と無音期間を時間で取得する
		private Note[] LoadNote( string tSourcePath )
		{
//			Debug.LogWarning( "パス:" + tSourcePath ) ;

			// PreloadAudioData にチェックが入っているとサンプルデータがロード出来なくなるためチェックを外す必要がある
			// ※ただし、Selection.objects で対象が選択されていた場合は PreloadAudioData にチェック゛入っていもサウプルデータは読めてしまうので注意すること

			bool tPreloadAudioData ;

			AudioImporter tAudioImporter = AssetImporter.GetAtPath( tSourcePath ) as AudioImporter ;
			tPreloadAudioData = tAudioImporter.preloadAudioData ;
			if( tPreloadAudioData == true )
			{
				tAudioImporter.preloadAudioData = false ;
				AssetDatabase.ImportAsset( tSourcePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			//----------------------------------------------------------

			AudioClip tAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>( tSourcePath ) ;

			// オーディオクリップのサンプルデータを読み出せるようにする
			tAudioClip.LoadAudioData() ;

//			Debug.LogWarning( "ロード:" + tAudioClip.LoadAudioData() ) ;

//			Debug.LogWarning( "プリロード:" + tAudioClip.preloadAudioData ) ;

//			Debug.LogWarning( "名前:" + tAudioClip.name ) ;
//			Debug.LogWarning( "サンプリングレート:" + tAudioClip.frequency ) ;
//			Debug.LogWarning( "サンプル数:" + tAudioClip.samples ) ;
//			Debug.LogWarning( "チャンネル数:" + tAudioClip.channels ) ;
//			Debug.LogWarning( "再生時間:" + tAudioClip.length ) ;

//			Debug.LogWarning( "計算結果:" + ( ( float )tAudioClip.samples / ( float )tAudioClip.frequency ) ) ;


//			Debug.LogWarning( "========================" ) ;

			List<Note> tNote = new List<Note>() ;

			int p, q, r ;
			int f = tAudioClip.frequency ;
			int s = tAudioClip.samples ;
			int c = tAudioClip.channels ;
			int w, t ;
			float v ;

			int o0, o1 ;
			o0 = 0 ;
			o1 = 0 ;
			int n = 0 ;

			t = 0 ;

			float tThresholdVolume = m_ThresholdVolume ;
			if( tThresholdVolume <  0.01f )
			{
				tThresholdVolume  = 0.01f ;
			}
			else
			if( tThresholdVolume >  0.25f )
			{
				tThresholdVolume  = 0.25f ;
			}

			float tThresholdLength = m_ThresholdLength ;
			if( tThresholdLength <  0.25f )
			{
				tThresholdLength  = 0.25f ;
			}
			else
			if( tThresholdLength >  1.25f )
			{
				tThresholdLength  = 1.25f ;
			}

//			Debug.LogWarning( "ThresholdVolume:" + tThresholdVolume ) ;
//			Debug.LogWarning( "ThresholdLength:" + tThresholdLength ) ;

			int d = ( int )( tAudioClip.frequency * tThresholdLength ) ;	// ０．５以上秒閾値を下回っていたら無音期間と判定する

			bool b ;

//			Debug.LogWarning( "Buffer:" + f * c ) ;

//			Debug.LogWarning( "=======" ) ;

			float[] tData = new float[ f * c ] ;
			for( p  = 0 ; p <  s ; p = p + f )
			{
				w = s - p ;
				if( w >  f )
				{
					w  = f ;
				}
				w = w * c ;

				tAudioClip.GetData( tData, p * c ) ;

				for( q  = 0 ; q <  w ; q = q + c )
				{
					b = false ;
					for( r  = 0 ; r <  c ; r ++ )
					{
						v = tData[ q + r ] ;
						if( v <  0 )
						{
							v  = - v ;
						}

						if( v >= tThresholdVolume )
						{
							b = true ;	// ２つのチャンネルのうちのどちらかかが閾値を上回ったら有音とみなす
							break ;
						}
					}

					if( n == 1 )
					{
						// 有音期間チェック中
						if( b == false )
						{
							// 無音の可能性がある期間発見
							o1 = t ;

							n = 0 ;
						}
					}
					else
					{
						// 無音期間チェック中
						if( b == true )
						{
							// 有音期間発見
							if( ( t - o1 ) >  d )
							{
								// 無音期間とみなす

								if( o1 >  o0 )
								{
									// 最初に有音期間開始位置を記録する
									tNote.Add( new Note( 1, ( float )o0 / ( float )f ) ) ;
								}

								// 次に無音期間開始位置を記録する
								tNote.Add( new Note( 0, ( float )o1 / ( float )f ) ) ;

								o0 = t ;	// ここからが新しい有音期間開始位置
								o1 = t ;	// ここからが新しい無音期間開始位置(本当はこの１行は必要ない)
							}

							n = 1 ;	// 有音期間になる
						}
					}

					// サンプル数増加
					t ++ ;
				}
			}
			
//			Debug.LogWarning( "===== ループを抜けた:" + n ) ;

			if( n == 1 )
			{
				// 有音期間の状態で終わった
				if( t >  o0 )
				{
					// 最初に有音期間開始位置を記録する
					tNote.Add( new Note( 1, ( float )o0 / ( float )f ) ) ;
				}
			}
			else
			{
				// 無音期間の状態で終わった

				if( ( t - o1 ) >  d )
				{
					// 無音期間とみなす
					if( o1 >  o0 )
					{
						// 最初に有音期間開始位置を記録する
						tNote.Add( new Note( 1, ( float )o0 / ( float )f ) ) ;
					}

					// 次に無音期間開始位置を記録する
					tNote.Add( new Note( 0, ( float )o1 / ( float )f ) ) ;
				}
				else
				{
					// 最後まで有音期間とみなす
					if( t >  o0 )
					{
						// 最初に有音期間開始位置を記録する
						tNote.Add( new Note( 1, ( float )o0 / ( float )f ) ) ;
					}
				}
			}

			// 最後に無音期間で閉める(必要無いかもしれない
			tNote.Add( new Note( 0, ( float )t / ( float )f ) ) ;
			
			//----------------------------------------------------------
			
			// オーディオクリップのサンプルデータを破棄する
			tAudioClip.UnloadAudioData() ;

			if( tPreloadAudioData == true )
			{
				// PreloadAudioData を変更していた場合は元に戻す
				tAudioImporter.preloadAudioData = true ;
				AssetDatabase.ImportAsset( tSourcePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			//----------------------------------------------------------

			return tNote.ToArray() ;
		}

		// 保存する
		private void SaveNote( string tPath, Note[] tNote )
		{
			string tText = "" ;

			int i, l = tNote.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tText = tText + tNote[ i ].state.ToString() + "," ;
				tText = tText + tNote[ i ].value.ToString() + "\n" ;
			}

			File.WriteAllText( tPath, tText ) ;
		}


		//----------------------------------------------------------------------------------------------



		private Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SelectSourceFile",		"対象となるサウンドファイルを選択して下さい(複数選択可)" },
			{ "SelectOutputPath",		"ウェーブノートを格納するフォルダを選択して下さい" },
			{ "SelectAllResource",		"AssetBundle化対象はプロジェクト全体のAssetLabel入力済みファイルとなります" },
			{ "SamePath",				"ResourceフォルダとAssetBundleフォルダに同じものは指定できません" },
			{ "RootPath",				"プロジェクトのルートフォルダ\n\n%1\n\nにAssetBundleを生成します\n\n本当によろしいですか？" },
			{ "Succeed",				"成功しました" },
			{ "Yes",					"はい" },
			{ "No",						"いいえ" },
			{ "OK",						"閉じる" },
		} ;
		private Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "SelectSourceFile",		"Please select a sound file to be." },
			{ "SelectOutputPath",		"Please select the folder in which to store the wave notes" },
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
				if( m_Japanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ tLabel ] ;
			}
			else
			{
				if( m_English_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ tLabel ] ;
			}
		}
	}
}

