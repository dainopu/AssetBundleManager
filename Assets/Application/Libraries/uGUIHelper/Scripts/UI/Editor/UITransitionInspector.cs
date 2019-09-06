using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UITransition のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UITransition ) ) ]
	public class UITransitionInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UITransition tTarget = target as UITransition ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.transitionEnabled == false )
				{
					GUILayout.Label( "Transition Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.transitionFoldOut = EditorGUILayout.Foldout( tTarget.transitionFoldOut, "Transition Enabled" ) ;
				}

				bool tTransitionEnabled = EditorGUILayout.Toggle( tTarget.transitionEnabled, GUILayout.Width( 24f ) ) ;
				if( tTransitionEnabled != tTarget.transitionEnabled )
				{
					Undo.RecordObject( tTarget, "UITransition : Transition Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.transitionEnabled = tTransitionEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.transitionEnabled == true && tTarget.transitionFoldOut == true )
			{
				// ワイドモードを有効にする
				bool tWideMode = EditorGUIUtility.wideMode ;
				EditorGUIUtility.wideMode = true ;

				//--------------------------------------------

				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tSpriteOverwriteEnabled = EditorGUILayout.Toggle( tTarget.spriteOverwriteEnabled, GUILayout.Width( 24f ) ) ;
					if( tSpriteOverwriteEnabled != tTarget.spriteOverwriteEnabled )
					{
						Undo.RecordObject( tTarget, "UITransition : Sprite Overwrite Enabled Change" ) ;	// アンドウバッファに登録
						tTarget.spriteOverwriteEnabled = tSpriteOverwriteEnabled ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Sprite Overwrite Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// 選択中のトランジションタイプ
				UITransition.State tEditingState = ( UITransition.State )EditorGUILayout.EnumPopup( " State",  tTarget.editingState ) ;
				if( tEditingState != tTarget.editingState )
				{
					Undo.RecordObject( tTarget, "UITransition : Editing State Change" ) ;	// アンドウバッファに登録
					tTarget.editingState = tEditingState ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}


				// ステートに応じた値を描画する
				int si = ( int )tTarget.editingState ;

				if( tTarget.spriteOverwriteEnabled == true )
				{
					UIImage tImage = tTarget.GetComponent<UIImage>() ;
					if( tImage != null )
					{
						if( tImage.atlasSprite != null )
						{
							string[] tName = tImage.atlasSprite.GetNameList() ;
							if( tName != null && tName.Length >  0 )
							{
								if( tTarget.transition[ si ].sprite == null && tImage.sprite != null )
								{
									Undo.RecordObject( tTarget, "UITransition : Editing State Change" ) ;	// アンドウバッファに登録
									tTarget.transition[ si ].sprite = tImage.sprite ;
									EditorUtility.SetDirty( tTarget ) ;
								}

								string tCurrentName = tTarget.transition[ si ].sprite.name ;

								int tIndexBase = -1 ;

								int i, l = tName.Length ;
								for( i  = 0 ; i <  l ; i ++ )
								{
									if( tName[ i ] == tCurrentName )
									{
										tIndexBase = i ;
										break ;
									}
								}

								if( tIndexBase <  0 )
								{
									List<string> tList = new List<string>() ;
									tList.Add( "Unknown" ) ;

									for( i  = 0 ; i <  l ; i ++ )
									{
										tList.Add( tName[ i ] ) ;
									}

									tName = tList.ToArray() ;
							
									tIndexBase = 0 ;
								}

								// フレーム番号
								int tIndex = EditorGUILayout.Popup( "  Selected Sprite", tIndexBase, tName ) ;
								if( tIndex != tIndexBase )
								{
									Undo.RecordObject( tTarget, "UIImage Sprite : Change" ) ;	// アンドウバッファに登録
									tTarget.transition[ si ].sprite = tImage.GetSpriteInAtlas( tName[ tIndex ] ) ;
									EditorUtility.SetDirty( tTarget ) ;
	//								UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
								}

								// 確認用
								Sprite tSprite = EditorGUILayout.ObjectField( "", tImage.GetSpriteInAtlas( tName[ tIndex ] ), typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) as Sprite ;
								if( tSprite != tImage.GetSpriteInAtlas( tName[ tIndex ] ) )
								{
								}
							}
						}
					}
				}

				UITransition.ProcessType tProcessType = ( UITransition.ProcessType )EditorGUILayout.EnumPopup( "  Process Type",  tTarget.transition[ si ].processType ) ;
				if( tProcessType != tTarget.transition[ si ].processType )
				{
					Undo.RecordObject( tTarget, "UIButton : Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.transition[ si ].processType = tProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

	/*			Vector3 tFadePosition = EditorGUILayout.Vector3Field( "   Fade Position",  tTarget.transition[ i ].fadePosition ) ;
				if( tFadePosition != tTarget.transition[ i ].fadePosition )
				{
					Undo.RecordObject( tTarget, "UIButton : Fade Position Change" ) ;	// アンドウバッファに登録
					tTarget.transition[ si ].fadePosition = tFadePosition ;
					EditorUtility.SetDirty( tTarget ) ;
				}*/

				Vector3 tFadeRotation = EditorGUILayout.Vector3Field( "   Fade Rotation",  tTarget.transition[ si ].fadeRotation ) ;
				if( tFadeRotation != tTarget.transition[ si ].fadeRotation )
				{
					Undo.RecordObject( tTarget, "UIButton : Fade Rotation Change" ) ;	// アンドウバッファに登録
					tTarget.transition[ si ].fadeRotation = tFadeRotation ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tFadeScale    = EditorGUILayout.Vector3Field( "   Fade Scale",      tTarget.transition[ si ].fadeScale   ) ;
				if( tFadeScale    != tTarget.transition[ si ].fadeScale    )
				{
					Undo.RecordObject( tTarget, "UIButton : Fade Scale    Change" ) ;	// アンドウバッファに登録
					tTarget.transition[ si ].fadeScale    = tFadeScale ;
					EditorUtility.SetDirty( tTarget ) ;
				}


				if( tTarget.transition[ si ].processType == UITransition.ProcessType.Ease )
				{
					// イーズタイプ
					UITransition.EaseType tFadeEaseType = ( UITransition.EaseType )EditorGUILayout.EnumPopup( "   Fade Ease Type",  tTarget.transition[ si ].fadeEaseType ) ;
					if( tFadeEaseType != tTarget.transition[ si ].fadeEaseType )
					{
						Undo.RecordObject( tTarget, "UIButton : Fade Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.transition[ si ].fadeEaseType = tFadeEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}

					DrawCurve( tTarget, tTarget.transition[ si ].fadeEaseType ) ;

					// デュアレーション
					float tFadeDuration = EditorGUILayout.FloatField( "   Fade Duration",  tTarget.transition[ si ].fadeDuration ) ;
					if( tFadeDuration != tTarget.transition[ si ].fadeDuration )
					{
						Undo.RecordObject( tTarget, "UIButton : Fade Duration Change" ) ;	// アンドウバッファに登録
						tTarget.transition[ si ].fadeDuration = tFadeDuration ;
					}
				}
				else
				if( tTarget.transition[ si ].processType == UITransition.ProcessType.AnimationCurve )
				{
					tTarget.transition[ si ].fadeAnimationCurve = EditorGUILayout.CurveField( "   Animation Curve", tTarget.transition[ si ].fadeAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;

					int l = tTarget.transition[ si ].fadeAnimationCurve.length ;
					Keyframe tKeyFrame = tTarget.transition[ si ].fadeAnimationCurve[ l - 1 ] ;	// 最終キー
					float tFadeDuration = tKeyFrame.time ;
					EditorGUILayout.FloatField( "   Fade Duration", tFadeDuration ) ;
				}

				//--------------------------------------------

				EditorGUILayout.Separator() ;	// 少し区切りスペース
		
				// フィニッシュの後にポーズするかどうか
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tPauseAfterFinished = EditorGUILayout.Toggle( tTarget.pauseAfterFinished, GUILayout.Width( 24f ) ) ;
					if( tPauseAfterFinished != tTarget.pauseAfterFinished )
					{
						Undo.RecordObject( tTarget, "UITransition : Pause After Finished Change" ) ;	// アンドウバッファに登録
						tTarget.pauseAfterFinished = tPauseAfterFinished ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Pause After Finished" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				// フィニッシュの後にポーズするかどうか
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tColorTransmission = EditorGUILayout.Toggle( tTarget.colorTransmission, GUILayout.Width( 24f ) ) ;
					if( tColorTransmission != tTarget.colorTransmission )
					{
						Undo.RecordObject( tTarget, "UITransition : Color Transmission Change" ) ;	// アンドウバッファに登録
						tTarget.colorTransmission = tColorTransmission ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Color Transmission" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了




				// ポーズ状態
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					bool tIsPausing = EditorGUILayout.Toggle( tTarget.isPauseing, GUILayout.Width( 24f ) ) ;
					if( tIsPausing != tTarget.isPauseing )
					{
						Undo.RecordObject( tTarget, "UITransition : IsPausing Change" ) ;	// アンドウバッファに登録
						tTarget.isPauseing = tIsPausing ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUILayout.Label( "Is Pausing" ) ;
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				//--------------------------------------------

				// ワイドモードを元に戻す
				EditorGUIUtility.wideMode = tWideMode ;
			}
		}

		// 曲線を描画する
		private void DrawCurve( UITransition tTarget, UITransition.EaseType tEaseType )
		{
			Rect tRect = GUILayoutUtility.GetRect( Screen.width - 160, 102f ) ;
		
			float x = 0 ;
			x = ( tRect.width - 102f ) * 0.5f ;
			if( x <  0 )
			{
				x  = 0 ;
			}
			tRect.x = x ;
			tRect.width = 102f ;
		
			EditorGUI.DrawRect( new Rect( tRect.x + 0, tRect.y + 0, tRect.width - 0, tRect.height - 0 ), new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ) ;
			EditorGUI.DrawRect( new Rect( tRect.x + 1, tRect.y + 1, tRect.width - 2, tRect.height - 2 ), new Color( 0.2f, 0.2f, 0.2f, 1.0f ) ) ;

			DrawLine(   0,  25, 99,  25, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  74, 99,  74, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(  50,  99, 50,   0, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  49, 99,  49, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;

/*			int px = 0, py = 0 ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px <  25 ; px ++  )
			{
				py = ( int )tTarget.GetValue(   0,  25, ( float )px * 0.04f, tEaseType ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 37 - oy ) / 1 ) + 0, px, ( ( 37 - py ) / 1 ) + 0, 0xFF00FF00, tRect.x + 1.0f, tRect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}*/

			int px = 0, py = 0 ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px < 100 ; px ++  )
			{
				py = ( int )tTarget.GetValue(   0,  50, ( float )px * 0.01f, tEaseType ) ;

				if( px == 0 )
				{
					ox = px ;
					oy = py ;
				}
				else
				{
					DrawLine( ox, ( ( 74 - oy ) / 1 ) + 0, px, ( ( 74 - py ) / 1 ) + 0, 0xFF00FF00, tRect.x + 1.0f, tRect.y + 1.0f ) ;

					ox = px ;
					oy = py ;
				}
			}



		}

		// 直線を描画する
		private void DrawLine( int x0, int y0, int x1, int y1, uint tColor, float tScreenX, float tScreenY )
		{
			int dx = x1 - x0 ;
			int dy = y1 - y0 ;

			int sx = 0 ;
			if( dx <  0 )
			{
				dx  = - dx ;
				sx  = -1 ;
			}
			else
			if( dx >  0 )
			{
				sx  =  1 ;
			}

			int sy = 0 ;
			if( dy <  0 )
			{
				dy  = - dy ;
				sy  = -1 ;
			}
			else
			if( dy >  0 )
			{
				sy  =  1 ;
			}

			dx ++ ;
			dy ++ ;

			Color32 tC = new Color32( ( byte )( ( tColor >> 16 ) & 0xFF ), ( byte )( ( tColor >>  8 ) & 0xFF ),  ( byte )( ( tColor >>   0 ) & 0xFF ), ( byte )( ( tColor >> 24 ) & 0xFF ) ) ;
			Rect tR = new Rect( 0, 0, 1, 1 ) ;

			int lx, ly ;
			int px, py ;
			int cx, cy ;

			px = x0 ;
			py = y0 ;

			if( dx == 1 && dy == 1 )
			{
				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx >  1 && dy == 1 )
			{
				if( x1 <  x0 )
				{
					px = x1 ;
				}

				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				tR.width = dx ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx == 1 && dy >  1 )
			{
				if( y1 <  y0 )
				{
					py = y1 ;
				}

				tR.x = ( float )px + tScreenX ;
				tR.y = ( float )py + tScreenY ;
				tR.height = dy ;
				EditorGUI.DrawRect( tR, tC ) ;
			}
			else
			if( dx >= dy )
			{
				cy = 0 ;
				for( lx  = 0 ; lx <  dx ; lx ++ )
				{
					tR.x = ( float )px + tScreenX ;
					tR.y = ( float )py + tScreenY ;
					EditorGUI.DrawRect( tR, tC ) ;

					cy = cy + dy ;
					if( cy >= dx )
					{
						cy = cy - dx ;
						py = py + sy ;
					}

					px = px + sx ;
				}
			}
			else
			{
				cx = 0 ;
				for( ly  = 0 ; ly <  dy ; ly ++ )
				{
					tR.x = ( float )px + tScreenX ;
					tR.y = ( float )py + tScreenY ;
					EditorGUI.DrawRect( tR, tC ) ;

					cx = cx + dx ;
					if( cx >= dy )
					{
						cx = cx - dy ;
						px = px + sx ;
					}

					py = py + sy ;
				}
			}
		}

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
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