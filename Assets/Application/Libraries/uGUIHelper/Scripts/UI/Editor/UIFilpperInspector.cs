using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIFlipper のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIFlipper ) ) ]
	public class UIFlipperInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIFlipper tTarget = target as UIFlipper ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity",  tTarget.identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity != tTarget.identity )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
		
			// ディレイ
			float tDelay = EditorGUILayout.FloatField( "Delay",  tTarget.delay ) ;
			if( tDelay != tTarget.delay )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Delay Change" ) ;	// アンドウバッファに登録
				tTarget.delay = tDelay ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			if( tTarget.GetComponent<Image>() == null )
			{
				EditorGUILayout.HelpBox( GetMessage( "ImageNone" ), MessageType.Warning, true ) ;		
			}

			// ワイドモードを有効にする
	//		bool tWideMode = EditorGUIUtility.wideMode ;
	//		EditorGUIUtility.wideMode = true ;


			//--------------------------------------------------------------------

			// 一番肝心なスプライトアニメーションファイル
			UISpriteAnimation tSpriteAnimation = EditorGUILayout.ObjectField( "Sprite Animation", tTarget.spriteAnimation, typeof( UISpriteAnimation ), false ) as UISpriteAnimation ;
			if( tSpriteAnimation != tTarget.spriteAnimation )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Sprite Animation Change " ) ;	// アンドウバッファに登録
				tTarget.spriteAnimation = tSpriteAnimation ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			if( tTarget.enabled == true && tTarget.spriteAnimation != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// チェック
					GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

					bool tIsChecker = EditorGUILayout.Toggle( tTarget.isChecker ) ;
					if( tIsChecker != tTarget.isChecker )
					{
						if( tIsChecker == true )
						{
							UIFlipper[] tFlipperList = tTarget.gameObject.GetComponents<UIFlipper>() ;
							if( tFlipperList != null && tFlipperList.Length >  0 )
							{
								for( int i  = 0 ; i <  tFlipperList.Length ; i ++ )
								{
									if( tFlipperList[ i ] != tTarget )
									{
										if( tFlipperList[ i ].isChecker == true )
										{
											tFlipperList[ i ].isChecker  = false ;
										}
									}
								}
							}
						}


						Undo.RecordObject( tTarget, "UIFlipper : Checker Change" ) ;	// アンドウバッファに登録
						tTarget.isChecker = tIsChecker ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTarget.isChecker == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						int tCheckFactor = EditorGUILayout.IntSlider( tTarget.checkFactor, 0, tTarget.spriteAnimation.length - 1 ) ;
						if( tCheckFactor != tTarget.checkFactor )
						{
							Undo.RecordObject( tTarget, "UIFlipper : Check Factor Change " ) ;	// アンドウバッファに登録
							tTarget.checkFactor = tCheckFactor ;
							EditorUtility.SetDirty( tTarget ) ;
//							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

	/*						Image tImage = tTarget.GetComponent<Image>() ;
							if( tImage != null )
							{
								EditorUtility.SetDirty( tImage ) ;
							}*/
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}


				EditorGUILayout.Separator() ;	// 少し区切りスペース

				// Begin
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Begin", GUILayout.Width( 60f ) ) ;
					int tBegin = EditorGUILayout.IntSlider( tTarget.begin, 0, tTarget.spriteAnimation.length - 1 ) ;
					if( tBegin != tTarget.begin )
					{
						Undo.RecordObject( tTarget, "UIFlipper : Begin Change " ) ;	// アンドウバッファに登録
						tTarget.begin = tBegin ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// end
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "End", GUILayout.Width( 60f ) ) ;
					int tEnd = EditorGUILayout.IntSlider( tTarget.end, 0, tTarget.spriteAnimation.length - 1 ) ;
					if( tEnd != tTarget.end )
					{
						Undo.RecordObject( tTarget, "UIFlipper : End Change " ) ;	// アンドウバッファに登録
						tTarget.end = tEnd ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// バック
				GUILayout.Label( "Back", GUILayout.Width( 116f ) ) ;

				bool tBack = EditorGUILayout.Toggle( tTarget.back ) ;
				if( tBack != tTarget.back )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Back Change" ) ;	// アンドウバッファに登録
					tTarget.back = tBack ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool tLoop = EditorGUILayout.Toggle( tTarget.loop ) ;
				if( tLoop != tTarget.loop )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Loop Change" ) ;	// アンドウバッファに登録
					tTarget.loop = tLoop ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.loop == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// リバース
					GUILayout.Label( "Reverse", GUILayout.Width( 116f ) ) ;

					bool tReverse = EditorGUILayout.Toggle( tTarget.reverse ) ;
					if( tReverse != tTarget.reverse )
					{
						Undo.RecordObject( tTarget, "UIFlipper : Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.reverse = tReverse ;
						EditorUtility.SetDirty( tTarget ) ;
//						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			// スピード
			float tSpeed = EditorGUILayout.FloatField( "Speed",  tTarget.speed ) ;
			if( tSpeed != tTarget.speed )
			{
				Undo.RecordObject( tTarget, "UIFlipper : Speed Change" ) ;	// アンドウバッファに登録
				tTarget.speed = tSpeed ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool tIgnoreTimeScale = EditorGUILayout.Toggle( tTarget.ignoreTimeScale ) ;
				if( tIgnoreTimeScale != tTarget.ignoreTimeScale )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					tTarget.ignoreTimeScale = tIgnoreTimeScale ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// プレイオンアウェイク
				GUILayout.Label( "Play On Awake", GUILayout.Width( 116f ) ) ;

				bool tPlayOnAwake = EditorGUILayout.Toggle( tTarget.playOnAwake ) ;
				if( tPlayOnAwake != tTarget.playOnAwake )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Play On Awake Change" ) ;	// アンドウバッファに登録
					tTarget.playOnAwake = tPlayOnAwake ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;     // 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Destroy At End", GUILayout.Width( 116f ) ) ;

				bool tDestroyAtEnd = EditorGUILayout.Toggle( tTarget.destroyAtEnd ) ;
				if( tDestroyAtEnd != tTarget.destroyAtEnd )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Destroy At End Change" ) ;	// アンドウバッファに登録
					tTarget.destroyAtEnd = tDestroyAtEnd ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// デストロイアットエンド
				GUILayout.Label( "Auto Resize", GUILayout.Width( 116f ) ) ;

				bool tAutoResize = EditorGUILayout.Toggle( tTarget.autoResize ) ;
				if( tAutoResize != tTarget.autoResize )
				{
					Undo.RecordObject( tTarget, "UIFlipper : Auto Resize Change" ) ;	// アンドウバッファに登録
					tTarget.autoResize = tAutoResize ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イズプレイング
				GUILayout.Label( "Is Playing", GUILayout.Width( 116f ) ) ;

				EditorGUILayout.Toggle( tTarget.isPlaying ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			EditorGUILayout.Separator() ;   // 少し区切りスペース


			// デリゲートの設定状況
			SerializedObject tSO = new SerializedObject( tTarget ) ;

			SerializedProperty tSP = tSO.FindProperty( "onFinished" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP ) ;
			}
			tSO.ApplyModifiedProperties() ;
		}


		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "ImageNone", "Image クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "ImageNone", "'Image' is necessary." },
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
	}
}
