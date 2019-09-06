using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

/// <summary>
/// アニメーションコントローラーオプティマイザーパッケージ
/// </summary>
namespace AnimationControllerOptimizer
{
	/// <summary>
	/// アニメーションコントローラーオプティマイザークラス(エディター用) Version 2017/10/20 0
	/// </summary>
	public class AnimationControllerOptimizer : EditorWindow
	{
		[ MenuItem( "Tools/AnimationController Optimizer" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<AnimationControllerOptimizer>( false, "AnimationController Optimizer", true ) ;
		}

		//----------------------------------------------------------
		
		private string	m_OutputPath = "" ;

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

			if( string.IsNullOrEmpty( m_OutputPath ) == false && Directory.Exists( m_OutputPath ) == true )
			{
				EditorGUILayout.HelpBox( GetMessage( "SelectSourceFile" ), MessageType.Info ) ;


				string[] tSourcePath = GetSourcePath() ;

				if( tSourcePath != null && tSourcePath.Length >  0 )
				{
					// 実行ボタン

					GUILayout.BeginHorizontal() ;
					{
						GUI.backgroundColor = Color.green ;
						tExecute = GUILayout.Button( "Execute" ) ;
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

						if( IsExtension( tPath, "controller" ) == true )
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

					if( IsExtension( tPath, "controller" ) == true )
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

		// 実行する
		private void Execute( string tOutputPath, string[] tSourcePath )
		{
			int i, j, l, m ;

			string[] tData ;

			l = tOutputPath.Length ;
			if( tOutputPath[ l - 1 ] != '/' )
			{
				tOutputPath =tOutputPath + "/" ;
			}

			string tName ;

			l = tSourcePath.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tData = LoadData( tSourcePath[ i ] ) ;

				if( tData != null && tData.Length >  0 )
				{
					tData = MakeData( tData ) ;

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

							tName = tName + ".controller" ;

							if( string.IsNullOrEmpty( tName ) == false )
							{
								SaveData( tOutputPath + tName, tData ) ;

								AssetDatabase.SaveAssets() ;
								AssetDatabase.Refresh() ;
							}
						}
					}

					Resources.UnloadUnusedAssets() ;
				}
			}
		}
		
		//-------------------------------------------------------------------------------------------

		// 読み出す
		private string[] LoadData( string tSourcePath )
		{
//			Debug.LogWarning( "パス:" + tSourcePath ) ;

			// PreloadAudioData にチェックが入っているとサンプルデータがロード出来なくなるためチェックを外す必要がある
			// ※ただし、Selection.objects で対象が選択されていた場合は PreloadAudioData にチェック゛入っていもサウプルデータは読めてしまうので注意すること

			string[] tData = File.ReadAllLines( tSourcePath ) ;

			//----------------------------------------------------------

			return tData ;
		}

		//-------------------------------------------------------------------------------------------

		public class Node
		{
			public	string	identity ;
			public	string	type ;

			public	int		start ;
			public	int		end ;

			public	bool	enable = false ;
		}

		public class AnimatorControllerNode : Node
		{
			public string			stateMachine		= "" ;
			public List<string>		behaviours			= new List<string>() ;
		}

		public class AnimatorStateMachineNode : Node
		{
			public List<string>	childStates				= new List<string>() ;
			public List<string>	stateMachineBehaviours	= new List<string>() ;
		}

		public class AnimatorStateNode : Node
		{
			public List<string> transitions				= new List<string>() ;
			public List<string>	stateMachineBehaviours	= new List<string>() ;
		}

		// 処理する
		private string[] MakeData( string[] tData )
		{
			List<string> tWork = new List<string>() ;

			int i, l = tData.Length, p = 0 ;

			// 最初の識別子が見つかるまではそのまま格納する
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tData[ i ].IndexOf( "---" ) != 0 )
				{
					tWork.Add( tData[ i ] ) ;
				}
				else
				{
					// 最初を発見した
					p = i ;
					break ;
				}
			}

			if( p == 0 )
			{
				return tWork.ToArray() ;	// 終了
			}

			//----------------------------------------------------------

			List<AnimatorControllerNode>				tAnimatorControllers			= new List<AnimatorControllerNode>() ;
			List<AnimatorStateMachineNode>				tAnimatorStateMachines			= new List<AnimatorStateMachineNode>() ;
			List<AnimatorStateNode>						tAnimatorStates					= new List<AnimatorStateNode>() ;
			List<Node>									tAnimatorStateTransitions		= new List<Node>() ;
			List<Node>									tMonoBehaviours					= new List<Node>() ;

			Dictionary<string,AnimatorStateMachineNode>	tAnimatorStateMachines_Hash		= new Dictionary<string, AnimatorStateMachineNode>() ;
			Dictionary<string,AnimatorStateNode>		tAnimatorStates_Hash			= new Dictionary<string, AnimatorStateNode>() ;
			Dictionary<string,Node>						tAnimatorStateTransitions_Hash	= new Dictionary<string, Node>() ;
			Dictionary<string,Node>						tMonoBehaviours_Hash			= new Dictionary<string, Node>() ; 

			Node							tNode = null ;

			AnimatorControllerNode			tAC_Node ;
			AnimatorStateMachineNode		tASM_Node ;
			AnimatorStateNode				tAS_Node ;


			string[] t ;
			string tType ;
			string tIdentity ;

			for( i  = p ; i <  l ; i ++ )
			{
				if( tData[ i ].IndexOf( "---" ) == 0 )
				{
					// 発見

					// １つ前を保存
					if( tNode != null )
					{
						tNode.end = i - 1 ;
						tNode = null ;
					}

					//--------------------------------

					t = tData[ i ].Split( '&' ) ;

					if( ( i + 1 ) <  ( l - 1 ) )
					{
						// タイプ必須
						tType = tData[ i + 1 ].ToLower() ;

						if( tType.Contains( "AnimatorController:".ToLower() ) == true )
						{
							tAC_Node = new AnimatorControllerNode() ;
							tAnimatorControllers.Add( tAC_Node ) ;

							tNode = tAC_Node ;

							tNode.enable = true ;	// 必須
						}
						else
						if( tType.Contains( "AnimatorStateMachine:".ToLower() ) == true )
						{
							tASM_Node = new AnimatorStateMachineNode() ;
							tAnimatorStateMachines.Add( tASM_Node ) ;

							tNode = tASM_Node ;

							tNode.enable = false ;
						}
						else
						if( tType.Contains( "AnimatorState:".ToLower() ) == true )
						{
							tAS_Node = new AnimatorStateNode() ;
							tAnimatorStates.Add( tAS_Node ) ;

							tNode = tAS_Node ;

							tNode.enable = false ;
						}
						else
						if( tType.Contains( "AnimatorStateTransition:".ToLower() ) == true )
						{
							tNode = new Node() ;
							tAnimatorStateTransitions.Add( tNode ) ;

							tNode.enable = false ;
						}
						else
						if( tType.Contains( "MonoBehaviour:".ToLower() ) == true )
						{
							tNode = new Node() ;
							tMonoBehaviours.Add( tNode ) ;

							tNode.enable = false ;
						}

						if( tNode != null )
						{
							tNode.type = tType ;
							tNode.identity = t[ 1 ] ;

							tNode.start = i ;
						}
					}
				}
			}

			if( tNode != null )
			{
				tNode.end = i - 1 ;
				tNode = null ;
			}
			
			//----------------------------------------------------------

			// 各ノードを種別に応じて解析する

			int j, m ;
			string s ;

			//----------------------------------
			// AnimatorController

			l = tAnimatorControllers.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAC_Node = tAnimatorControllers[ i ] ;

				//---------------------------------
				// m_StateMachine

				p = -1 ;
				for( j  = tAC_Node.start ; j <= tAC_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_StateMachine:" ) == true )
					{
						p = j ;
						break ;
					}
				}

				if( p >= 0 )
				{
					s = tData[ j ].Replace( " ", "" ) ;
					s = s.Replace( "m_StateMachine:", "" ) ;
					s = s.Replace( "fileID:", "" ) ;
					s = s.Replace( "{", "" ) ;
					s = s.Replace( "}", "" ) ;

					tAC_Node.stateMachine = s ;
				}

				//---------------------------------
				// Behaviours

				p = -1 ;
				for( j  = tAC_Node.start ; j <= tAC_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_Behaviours:" ) == true )
					{
						p = j + 1 ;
						break ;
					}
				}

				if( p >= 0 )
				{
					for( j  = p ; j <= tAC_Node.end ; j ++ )
					{
						s = tData[ j ].Replace( " ", "" ) ;
						if( s.IndexOf( "-" ) == 0 )
						{
							s = s.Replace( "fileID:", "" ) ;
							s = s.Replace( "{", "" ) ;
							s = s.Replace( "}", "" ) ;

							tAC_Node.behaviours.Add( s ) ;
						}
						else
						{
							break ;	// 終了
						}
					}
				}
			}

			//----------------------------------
			// AnimatorStateMachine

			l = tAnimatorStateMachines.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tASM_Node = tAnimatorStateMachines[ i ] ;

				tAnimatorStateMachines_Hash.Add( tASM_Node.identity, tASM_Node ) ;

				//---------------------------------
				// m_State

				p = -1 ;
				for( j  = tASM_Node.start ; j <= tASM_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_ChildStates:" ) == true )
					{
						p = j + 1 ;
						break ;
					}
				}

				if( p >= 0 )
				{
					for( j  = p ; j <= tASM_Node.end ; j ++ )
					{
						s = tData[ j ].Replace( " ", "" ) ;
						if( s.IndexOf( "-" ) == 0 )
						{
							s = tData[ j + 1 ].Replace( " ", "" ) ;
							if( s.IndexOf( "m_State:" ) == 0 )
							{
								s = s.Replace( "m_State:{fileID:", "" ) ;
								s = s.Replace( "}", "" ) ;

								tASM_Node.childStates.Add( s ) ;
							}

							j = j + 2 ;
						}
						else
						{
							break ;	// 終了
						}
					}
				}

				//---------------------------------
				// StateMachineBehaviour

				p = -1 ;
				for( j  = tASM_Node.start ; j <= tASM_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_StateMachineBehaviours:" ) == true )
					{
						p = j + 1 ;
						break ;
					}
				}

				if( p >= 0 )
				{
					for( j  = p ; j <= tASM_Node.end ; j ++ )
					{
						s = tData[ j ].Replace( " ", "" ) ;
						if( s.IndexOf( "-" ) == 0 )
						{
							s = s.Replace( "-{fileID:", "" ) ;
							s = s.Replace( "}", "" ) ;

							tASM_Node.stateMachineBehaviours.Add( s ) ;
						}
						else
						{
							break ;	// 終了
						}
					}
				}
			}

			//----------------------------------
			// AnimatorState

			l = tAnimatorStates.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAS_Node = tAnimatorStates[ i ] ;

				tAnimatorStates_Hash.Add( tAS_Node.identity, tAS_Node ) ;

				//---------------------------------
				// m_Transition

				p = -1 ;
				for( j  = tAS_Node.start ; j <= tAS_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_Transitions:" ) == true )
					{
						p = j + 1 ;
						break ;
					}
				}

				if( p >= 0 )
				{
					for( j  = p ; j <= tAS_Node.end ; j ++ )
					{
						s = tData[ j ].Replace( " ", "" ) ;
						if( s.IndexOf( "-" ) == 0 )
						{
							s = s.Replace( "-{fileID:", "" ) ;
							s = s.Replace( "}", "" ) ;

							tAS_Node.transitions.Add( s ) ;
						}
						else
						{
							break ;	// 終了
						}
					}
				}

				//---------------------------------
				// StateMachineBehaviour

				p = -1 ;
				for( j  = tAS_Node.start ; j <= tAS_Node.end ; j ++ )
				{
					if( tData[ j ].Contains( "m_StateMachineBehaviours:" ) == true )
					{
						p = j + 1 ;
						break ;
					}
				}

				if( p >= 0 )
				{
					for( j  = p ; j <= tAS_Node.end ; j ++ )
					{
						s = tData[ j ].Replace( " ", "" ) ;
						if( s.IndexOf( "-" ) == 0 )
						{
							s = s.Replace( "-{fileID:", "" ) ;
							s = s.Replace( "}", "" ) ;

							tAS_Node.stateMachineBehaviours.Add( s ) ;
						}
						else
						{
							break ;	// 終了
						}
					}
				}
			}

			//----------------------------------
			// AnimatorStateTransition

			l = tAnimatorStateTransitions.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tNode = tAnimatorStateTransitions[ i ] ;

				tAnimatorStateTransitions_Hash.Add( tNode.identity, tNode ) ;
			}

			//----------------------------------
			// MonoBehaviour

			l = tMonoBehaviours.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tNode = tMonoBehaviours[ i ] ;

				tMonoBehaviours_Hash.Add( tNode.identity, tNode ) ;
			}

			//------------------------------------------------------------------------------------------

			// 必要な AnimatorStateMachine にチェックを入れる

			l = tAnimatorControllers.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAC_Node = tAnimatorControllers[ i ] ;

				if( string.IsNullOrEmpty( tAC_Node.stateMachine ) == false )
				{
					if( tAnimatorStateMachines_Hash.ContainsKey( tAC_Node.stateMachine ) == true )
					{
						 tAnimatorStateMachines_Hash[ tAC_Node.stateMachine ].enable = true ;
					}
				}
			}

			//----------------------------------------------------------

			// 必要な AnimatorState にチェックを入れる

			l = tAnimatorStateMachines.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tASM_Node = tAnimatorStateMachines[ i ] ;
				if( tASM_Node.enable == true )
				{
					m = tASM_Node.childStates.Count ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						tIdentity = tASM_Node.childStates[ j ] ;
						if( tAnimatorStates_Hash.ContainsKey( tIdentity ) == true )
						{
							tAnimatorStates_Hash[ tIdentity ].enable = true ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 必要な AnimatorStateTransition にチェックを入れる

			l = tAnimatorStates.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAS_Node = tAnimatorStates[ i ] ;
				if( tAS_Node.enable == true )
				{
					m = tAS_Node.transitions.Count ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						tIdentity = tAS_Node.transitions[ j ] ;
						if( tAnimatorStateTransitions_Hash.ContainsKey( tIdentity ) == true )
						{
							tAnimatorStateTransitions_Hash[ tIdentity ].enable = true ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 必要な MonoBehaviour にチェックを入れる

			l = tAnimatorControllers.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAC_Node = tAnimatorControllers[ i ] ;

				m = tAC_Node.behaviours.Count ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					tIdentity = tAC_Node.behaviours[ j ] ;
					if( tMonoBehaviours_Hash.ContainsKey( tIdentity ) == true )
					{
						 tMonoBehaviours_Hash[ tIdentity ].enable = true ;
					}
				}
			}

			l = tAnimatorStateMachines.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tASM_Node = tAnimatorStateMachines[ i ] ;
				if( tASM_Node.enable == true )
				{
					m = tASM_Node.stateMachineBehaviours.Count ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						tIdentity = tASM_Node.stateMachineBehaviours[ j ] ;
						if( tMonoBehaviours_Hash.ContainsKey( tIdentity ) == true )
						{
							 tMonoBehaviours_Hash[ tIdentity ].enable = true ;
						}
					}
				}
			}

			l = tAnimatorStates.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAS_Node = tAnimatorStates[ i ] ;
				if( tAS_Node.enable == true )
				{
					m = tAS_Node.stateMachineBehaviours.Count ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						tIdentity = tAS_Node.stateMachineBehaviours[ j ] ;
						if( tMonoBehaviours_Hash.ContainsKey( tIdentity ) == true )
						{
							 tMonoBehaviours_Hash[ tIdentity ].enable = true ;
						}
					}
				}
			}

			//------------------------------------------------------------------------------------------

			// 有効なデータを出力する

			//----------------------------------
			// AnimatorController

			l = tAnimatorControllers.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAC_Node = tAnimatorControllers[ i ] ;

				for( p  = tAC_Node.start ; p <= tAC_Node.end ; p ++ )
				{
					tWork.Add( tData[ p ] ) ;
				}
			}

			//----------------------------------
			// AnimatorStateMachine

			l = tAnimatorStateMachines.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tASM_Node = tAnimatorStateMachines[ i ] ;
				
				if( tASM_Node.enable == true )
				{
					for( p  = tASM_Node.start ; p <= tASM_Node.end ; p ++ )
					{
						tWork.Add( tData[ p ] ) ;
					}
				}
			}

			//----------------------------------
			// AnimatorState

			l = tAnimatorStates.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tAS_Node = tAnimatorStates[ i ] ;
				
				if( tAS_Node.enable == true )
				{
					for( p  = tAS_Node.start ; p <= tAS_Node.end ; p ++ )
					{
						tWork.Add( tData[ p ] ) ;
					}
				}
			}

			//----------------------------------
			// AnimatorStateTransition

			l = tAnimatorStateTransitions.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tNode = tAnimatorStateTransitions[ i ] ;
				
				if( tNode.enable == true )
				{
					for( p  = tNode.start ; p <= tNode.end ; p ++ )
					{
						tWork.Add( tData[ p ] ) ;
					}
				}
			}

			//----------------------------------
			// MonoBehaviour

			l = tMonoBehaviours.Count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tNode = tMonoBehaviours[ i ] ;
				
				if( tNode.enable == true )
				{
					for( p  = tNode.start ; p <= tNode.end ; p ++ )
					{
						tWork.Add( tData[ p ] ) ;
					}
				}
			}

			//------------------------------------------------------------------------------------------

			return tWork.ToArray() ;
		}

		//-------------------------------------------------------------------------------------------

		// 書き込む
		private void SaveData( string tPath, string[] tData )
		{
			File.WriteAllLines( tPath, tData ) ;
		}

		//----------------------------------------------------------------------------------------------



		private Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "SelectSourceFile",		"対象となるアニメーションコントローラーを選択して下さい(複数選択可)" },
			{ "SelectOutputPath",		"アニメーションコントローラーを保存するフォルダを選択して下さい" },
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
