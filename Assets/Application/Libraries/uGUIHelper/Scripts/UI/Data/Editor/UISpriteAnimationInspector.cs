using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;


namespace uGUIHelper
{
	/// <summary>
	/// UISpriteAnimation のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UISpriteAnimation ) ) ]
	public class UISpriteAnimationInspector : Editor
	{
		private Texture m_TemporaryTexture = null ;
		
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;

			//-----------------------------------------------------

			// ターゲットのインスタンス
			UISpriteAnimation tTarget = target as UISpriteAnimation ;
		
			//-----------------------------------------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		

			Texture tTexture = EditorGUILayout.ObjectField( "Atlas Sprite", tTarget.texture, typeof( Texture ), false ) as Texture ;
			if( m_TemporaryTexture == null )
			{
				if( tTexture != tTarget.texture )
				{
					m_TemporaryTexture = tTexture ;
				}
			}
			else
			{
				bool tChange = false ;
				if( tTarget.texture != null )
				{
					string tMessage = GetMessage( "ChangeOK?" ) ;
					GUILayout.Label( tMessage ) ;
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						GUI.backgroundColor = Color.red ;
						if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
						{
							tChange = true ;
						}
						GUI.backgroundColor = Color.white ;
						if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
						{
							m_TemporaryTexture = null ;
						}
					}
					GUILayout.EndHorizontal() ;     // 横並び終了
				}
				else
				{
					tChange = true ;
				}
			
				if( tChange == true )
				{
					// 本当に変更する
					Undo.RecordObject( tTarget, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録

					tTarget.texture = m_TemporaryTexture ;

					List<Sprite> tList = new List<Sprite>() ;

					if( tTarget.texture != null )
					{
						string tPath = AssetDatabase.GetAssetPath( tTarget.texture.GetInstanceID() ) ;

						// テクスチャからパスを取得してマルチタイプスプライトとしてロードする
						UnityEngine.Object[] tSpriteAll = AssetDatabase.LoadAllAssetsAtPath( tPath ) ;

						if( tSpriteAll != null )
						{
							int i, l = tSpriteAll.Length ;

							for( i  = 0 ; i <  l ; i ++ )
							{
								if( tSpriteAll[ i ] is UnityEngine.Sprite )
								{
									tList.Add( tSpriteAll[ i ] as UnityEngine.Sprite ) ;
								}
							}
						}
					}

					if( tList.Count >  0 )
					{
						// 存在するので更新する
						tTarget.SetSprite( tList.ToArray() ) ;
					}
					else
					{
						// 存在しないのでクリアする
						tTarget.ClearSprite() ;
					}

					EditorUtility.SetDirty( tTarget ) ;

					m_TemporaryTexture = null ;
				}
			}

			//-----------------------------------------------------
		
			if( tTarget.exist == true )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				DrawFrame( tTarget ) ;
			}
		}


		private int		m_InsertFrameIndex = -1 ;
		private int		m_InsertSpriteIndex = 0 ;
		private float	m_InsertDuration = 0.1f ;

		private bool[]	m_RemoveFrameIndex = null ;
		private bool    m_ExecuteRemove = false ;

		private Vector2 m_Scroll = Vector2.zero ;
	
		private bool	m_Thumbnail = false ;


		protected void DrawFrame( UISpriteAnimation tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			float tTimeScale = EditorGUILayout.FloatField( "Time Scale", tTarget.timeScale ) ;
			if( tTimeScale >  0 && tTimeScale != tTarget.timeScale )
			{
				Undo.RecordObject( tTarget, "UISpriteAnimation : Time Scale Change" ) ;	// アンドウバッファに登録
				tTarget.timeScale = tTimeScale ;
				EditorUtility.SetDirty( tTarget ) ;	// アセットデータベースに変更を通知
			}


			int i, l ;

			l = tTarget.frame.Count + 1 ;
			string[] tFrameNumber = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tFrameNumber[ i ] = i.ToString() ;
			}
			if( m_InsertFrameIndex <  0 )
			{
				m_InsertFrameIndex = l - 1 ;	// 最終フレーム指定
			}
			else
			if( m_InsertFrameIndex >= tFrameNumber.Length )
			{
				m_InsertFrameIndex  = tFrameNumber.Length - 1 ;
			}

			l = tTarget.GetSpriteCount() ;

			string[] tSpriteName = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tTarget.GetSprite( i ) != null )
				{
					tSpriteName[ i ] = tTarget.GetSprite( i ).name ;
				}
				else
				{
					tSpriteName[ i ] = "<Missing>" ;
				}
			}
			if( m_InsertSpriteIndex <  0 )
			{
				m_InsertSpriteIndex = l - 1 ;	// 最終フレーム指定
			}
			else
			if( m_InsertSpriteIndex >= tSpriteName.Length )
			{
				m_InsertSpriteIndex  = tSpriteName.Length - 1 ;
			}

			//----------------------------------------------------

			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool tInsert = false ;

				// 挿入ボタン
				GUI.backgroundColor = Color.cyan ;
				if( GUILayout.Button( "Insert", GUILayout.Width( 60f ) ) == true )
				{
					tInsert = true ;
				}
				GUI.backgroundColor = Color.white ;

				// フレーム番号
				m_InsertFrameIndex = EditorGUILayout.Popup( "", m_InsertFrameIndex, tFrameNumber, GUILayout.Width( 50f ) ) ;

				// スプライト番号
				m_InsertSpriteIndex = EditorGUILayout.Popup( "", m_InsertSpriteIndex, tSpriteName, GUILayout.Width( 120f ) ) ;

				// 表示時間
				m_InsertDuration = EditorGUILayout.FloatField( "", m_InsertDuration, GUILayout.Width( 50f ) ) ;

				if( tInsert == true && tSpriteName[ m_InsertSpriteIndex ] != "<Missing>" && m_InsertDuration >  0 )
				{
					// Frame を追加する
					Undo.RecordObject( tTarget, "UISpriteAnimation : Insert" ) ;	// アンドウバッファに登録
					tTarget.Insert( m_InsertFrameIndex, m_InsertSpriteIndex, m_InsertDuration ) ;
					EditorUtility.SetDirty( tTarget ) ;

					m_InsertFrameIndex ++ ;
					m_InsertSpriteIndex ++ ;

					m_RemoveFrameIndex = null ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
			

			// 以下１つ以上フレームが登録されている場合のみ表示する
			l = tTarget.frame.Count ;
			if( l >  0 )
			{
				// 削除用フラグ
				if( m_RemoveFrameIndex == null )
				{
					m_RemoveFrameIndex = new bool[ l ] ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						m_RemoveFrameIndex[ i ] = false ;
					}
				}
			
				//------------------------------------------------

				// 削除確認
				if( m_RemoveFrameIndex != null )
				{
					if( m_ExecuteRemove == false )
					{
						// 削除ではない

						bool tRemove = false ;
						for( i  = 0 ; i <  l ; i ++  )
						{
							if( m_RemoveFrameIndex[ i ] == true )
							{
								tRemove = true ;
								break ;
							}
						}

						if( tRemove == true )
						{
							GUI.backgroundColor = Color.red ;
							if( GUILayout.Button( "Execute Remove",  GUILayout.Height( 36f ) ) == true )
							{
								m_ExecuteRemove = true ;
							}
							GUI.backgroundColor = Color.white ;
						}
						else
						{
							GUILayout.Label( "",  GUILayout.Height( 38f ) ) ;
						}
					}
					else
					{
						// 削除の最終確認

						string tMessage = GetMessage( "RemoveOK?" ) ;
						GUILayout.Label( tMessage ) ;
						GUILayout.BeginHorizontal() ;	// 横並び開始
						{
							GUI.backgroundColor = Color.red ;
							if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
							{
								// 本当に削除する
								Undo.RecordObject( tTarget, "UISpriteAnimation : Remove" ) ;	// アンドウバッファに登録

								List<int> tRemoveFrameIndex = new List<int>() ;
								for( i  = 0 ; i <  l ; i ++ )
								{
									if( m_RemoveFrameIndex[ i ] == true )
									{
										tRemoveFrameIndex.Add( i ) ;
									}
								}

								if( tRemoveFrameIndex.Count >  0 )
								{
									tTarget.Remove( tRemoveFrameIndex.ToArray() ) ;
								}

								EditorUtility.SetDirty( tTarget ) ;
							
								m_RemoveFrameIndex = null ;

								m_ExecuteRemove = false ;
							}
							GUI.backgroundColor = Color.white ;
							if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
							{
								m_ExecuteRemove = false ;
							}
						}
						GUILayout.EndHorizontal() ;		// 横並び終了

						if( m_RemoveFrameIndex == null )
						{
							// フレームの数が変動しているので以下のリストを表示してはならない
							return ;
						}
					}
				}
					
				//---------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					GUILayout.Label( "Frame" ) ;

					m_Thumbnail = EditorGUILayout.Toggle( m_Thumbnail, GUILayout.Width( 16f ) ) ;
					GUILayout.Label( "Thumbnail" ) ;
				}
				GUILayout.EndHorizontal() ;     // 横並び終了

				EditorGUILayout.Separator() ;	// 少し区切りスペース


				// 登録されているフレーム情報を表示する
				m_Scroll = GUILayout.BeginScrollView( m_Scroll ) ;
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						GUILayout.BeginHorizontal() ;
						{
							// サムネイルアイコン

							// フレーム番号（右寄せ）
							GUILayout.Label( i.ToString(), GUILayout.Width( 30f ) ) ;


							// スプライト選択
							int tSpriteIndex = EditorGUILayout.Popup( "", tTarget.frame[ i ].spriteIndex, tSpriteName, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
							if( tSpriteIndex != tTarget.frame[ i ].spriteIndex )
							{
								if( tSpriteName[ tSpriteIndex ] != "<Missing>" )
								{
									Undo.RecordObject( tTarget, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録
									tTarget.frame[ i ].spriteIndex = tSpriteIndex ;
									EditorUtility.SetDirty( tTarget ) ;
								}
							}


							// 表示時間
							float tDuration = EditorGUILayout.FloatField( "", tTarget.frame[ i ].duration, GUILayout.Width( 50f ) ) ;
							if( tDuration != tTarget.frame[ i ].duration && tDuration >  0 )
							{
								Undo.RecordObject( tTarget, "UISpriteAnimation : Change" ) ;	// アンドウバッファに登録
								tTarget.frame[ i ].duration = tDuration ;
								EditorUtility.SetDirty( tTarget ) ;
							}


							// 削除用ボタン
							if( m_RemoveFrameIndex[ i ] == false )
							{
								GUI.backgroundColor = Color.white ;
							}
							else
							{
								GUI.backgroundColor = Color.red ;
							}
							if( GUILayout.Button( "Remove", GUILayout.Width( 60f ) ) )
							{
								m_RemoveFrameIndex[ i ] = ! m_RemoveFrameIndex[ i ] ;
							}
							GUI.backgroundColor = Color.white ;
						}
						GUILayout.EndHorizontal() ;
						
						// サムネイル
						if( m_Thumbnail == true )
						{
							GUILayout.BeginHorizontal() ;
							{
								GUILayout.Label( "", GUILayout.Width( 30f ) ) ;
								Sprite tSprite = EditorGUILayout.ObjectField( "", tTarget.GetSprite( i ), typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) as Sprite ;
								if( tSprite != tTarget.GetSprite( i ) )
								{
								}
							}
							GUILayout.EndHorizontal() ;
						}
					}
				}
				GUILayout.EndScrollView() ;
			}
		}


		//--------------------------------------------------------------------------

		private Dictionary<string,string> m_Japanese_Message = new Dictionary<string, string>()
		{
			{ "ChangeOK?", "本当に変更してもよろしいですか？\n※フレーム情報が全て消去されます" },
			{ "RemoveOK?", "本当に削除してもよろしいですか？" },
		} ;
		private Dictionary<string,string> m_English_Message = new Dictionary<string, string>()
		{
			{ "ChangeOK?", "It does really may be to change ?\n※Frame information all will be erased." },
			{ "RemoveOK?", "It does really may be to remove ?" },
		} ;

		private string GetMessage( string tLabel )
		{
			if( Application.systemLanguage == SystemLanguage.Japanese )
			{
				if( m_Japanese_Message.ContainsKey( tLabel ) == false )
				{
					return "指定のラベル名が見つかりません" ;
				}
				return m_Japanese_Message[ tLabel ].Replace( "\\n", "\n" ) ;
			}
			else
			{
				if( m_English_Message.ContainsKey( tLabel ) == false )
				{
					return "Specifying the label name can not be found" ;
				}
				return m_English_Message[ tLabel ].Replace( "\\n", "\n" )  ;
			}
		}
	}
}

