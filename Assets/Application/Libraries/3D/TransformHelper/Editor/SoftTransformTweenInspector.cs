using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace TransformHelper
{
	[ CustomEditor( typeof( SoftTransformTween ) ) ]
	public class SoftTransformTweenInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			SoftTransformTween tTarget = target as SoftTransformTween ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 識別子
			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity",  tTarget.identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity != tTarget.identity )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		
			// ディレイ
			float tDelay = EditorGUILayout.FloatField( "Delay",  tTarget.delay ) ;
			if( tDelay != tTarget.delay )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Delay Change" ) ;	// アンドウバッファに登録
				tTarget.delay = tDelay ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// デュアレーション
			float tDuration = EditorGUILayout.FloatField( "Duration",  tTarget.duration ) ;
			if( tDuration != tTarget.duration )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Duration Change" ) ;	// アンドウバッファに登録
				tTarget.duration = tDuration ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			//------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// ワイドモードを有効にする
			bool tWideMode = EditorGUIUtility.wideMode ;
			EditorGUIUtility.wideMode = true ;


			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.positionEnabled == false )
				{
					GUILayout.Label( "Position Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.positionFoldOut = EditorGUILayout.Foldout( tTarget.positionFoldOut, "Position Enabled" ) ;
				}

				bool tPositionEnabled = EditorGUILayout.Toggle( tTarget.positionEnabled, GUILayout.Width( 24f ) ) ;
				if( tPositionEnabled != tTarget.positionEnabled )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.positionEnabled = tPositionEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.positionEnabled == true && tTarget.positionFoldOut == true )
			{
				// ポジション

				Vector3 tPositionFrom = EditorGUILayout.Vector3Field( " From",  tTarget.positionFrom /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionFrom != tTarget.positionFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position From Change" ) ;	// アンドウバッファに登録
					tTarget.positionFrom = tPositionFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tPositionTo = EditorGUILayout.Vector3Field( " To",  tTarget.positionTo /*, GUILayout.MaxWidth( 100f ) */ ) ;
				if( tPositionTo != tTarget.positionTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position To Change" ) ;	// アンドウバッファに登録
					tTarget.positionTo = tPositionTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessType tPositionProcessType = ( SoftTransformTween.ProcessType )EditorGUILayout.EnumPopup( " Process Type",  tTarget.positionProcessType ) ;
				if( tPositionProcessType != tTarget.positionProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Position Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.positionProcessType = tPositionProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.positionProcessType == SoftTransformTween.ProcessType.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseType tPositionEaseType = ( SoftTransformTween.EaseType )EditorGUILayout.EnumPopup( " EaseType",  tTarget.positionEaseType ) ;
					if( tPositionEaseType != tTarget.positionEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Position Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.positionEaseType = tPositionEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.positionProcessType == SoftTransformTween.ProcessType.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.positionAnimationCurve.keys ) ;
					tTarget.positionAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.isChecker == true )
				{
					DrawCurve( tTarget, tTarget.checkFactor, tTarget.positionProcessType, tTarget.positionEaseType, tTarget.positionAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
				}
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.rotationEnabled == false )
				{
					GUILayout.Label( "Rotation Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.rotationFoldOut = EditorGUILayout.Foldout( tTarget.rotationFoldOut, "Rotation Enabled" ) ;
				}

				bool tRotationEnabled = EditorGUILayout.Toggle( tTarget.rotationEnabled, GUILayout.Width( 24f ) ) ;
				if( tRotationEnabled != tTarget.rotationEnabled )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.rotationEnabled = tRotationEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.rotationEnabled == true && tTarget.rotationFoldOut == true )
			{
				// ローテーション

				Vector3 tRotationFrom = EditorGUILayout.Vector3Field( " From",  tTarget.rotationFrom ) ;
				if( tRotationFrom != tTarget.rotationFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation From Change" ) ;	// アンドウバッファに登録
					tTarget.rotationFrom = tRotationFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tRotationTo = EditorGUILayout.Vector3Field( " To",  tTarget.rotationTo ) ;
				if( tRotationTo != tTarget.rotationTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation To Change" ) ;	// アンドウバッファに登録
					tTarget.rotationTo = tRotationTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessType tRotationProcessType = ( SoftTransformTween.ProcessType )EditorGUILayout.EnumPopup( " Process Type",  tTarget.rotationProcessType ) ;
				if( tRotationProcessType != tTarget.rotationProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.rotationProcessType = tRotationProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.rotationProcessType == SoftTransformTween.ProcessType.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseType tRotationEaseType = ( SoftTransformTween.EaseType )EditorGUILayout.EnumPopup( " EaseType",  tTarget.rotationEaseType ) ;
					if( tRotationEaseType != tTarget.rotationEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Rotation Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.rotationEaseType = tRotationEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.rotationProcessType == SoftTransformTween.ProcessType.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.rotationAnimationCurve.keys ) ;
					tTarget.rotationAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.isChecker == true )
				{
					DrawCurve( tTarget, tTarget.checkFactor, tTarget.rotationProcessType, tTarget.rotationEaseType, tTarget.rotationAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
				}
			}

			//--------------

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				if( tTarget.scaleEnabled == false )
				{
					GUILayout.Label( "Scale Enabled" /*, GUILayout.Width( 116f ) */ ) ;
				}
				else
				{
					tTarget.scaleFoldOut = EditorGUILayout.Foldout( tTarget.scaleFoldOut, "Scale Enabled" ) ;
				}

				bool tScaleEnabled = EditorGUILayout.Toggle( tTarget.scaleEnabled, GUILayout.Width( 24f ) ) ;
				if( tScaleEnabled != tTarget.scaleEnabled )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale Enabled Change" ) ;	// アンドウバッファに登録
					tTarget.scaleEnabled = tScaleEnabled ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tTarget.scaleEnabled == true && tTarget.scaleFoldOut == true )
			{
				// スケール

				Vector3 tScaleFrom = EditorGUILayout.Vector3Field( " From",  tTarget.scaleFrom ) ;
				if( tScaleFrom != tTarget.scaleFrom )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale From Change" ) ;	// アンドウバッファに登録
					tTarget.scaleFrom = tScaleFrom ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				Vector3 tScaleTo = EditorGUILayout.Vector3Field( " To",  tTarget.scaleTo ) ;
				if( tScaleTo != tTarget.scaleTo )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale To Change" ) ;	// アンドウバッファに登録
					tTarget.scaleTo = tScaleTo ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				// プロセスタイプ
				SoftTransformTween.ProcessType tScaleProcessType = ( SoftTransformTween.ProcessType )EditorGUILayout.EnumPopup( " Process Type",  tTarget.scaleProcessType ) ;
				if( tScaleProcessType != tTarget.scaleProcessType )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Scale Process Type Change" ) ;	// アンドウバッファに登録
					tTarget.scaleProcessType = tScaleProcessType ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.scaleProcessType == SoftTransformTween.ProcessType.Ease )
				{
					// イーズタイプ
					SoftTransformTween.EaseType tScaleEaseType = ( SoftTransformTween.EaseType )EditorGUILayout.EnumPopup( " EaseType",  tTarget.scaleEaseType ) ;
					if( tScaleEaseType != tTarget.scaleEaseType )
					{
						Undo.RecordObject( tTarget, "SoftTransformTween : Scale Ease Type Change" ) ;	// アンドウバッファに登録
						tTarget.scaleEaseType = tScaleEaseType ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				else
				if( tTarget.scaleProcessType == SoftTransformTween.ProcessType.AnimationCurve )
				{
					AnimationCurve tAnimationCurve = new AnimationCurve(  tTarget.scaleAnimationCurve.keys ) ;
					tTarget.scaleAnimationCurve = EditorGUILayout.CurveField( " Animation Curve", tAnimationCurve, GUILayout.Width( 170f ), GUILayout.Height( 52f ) ) ;
				}

				if( tTarget.isChecker == true )
				{
					DrawCurve( tTarget, tTarget.checkFactor, tTarget.scaleProcessType, tTarget.scaleEaseType, tTarget.scaleAnimationCurve ) ;
				}

				if( tTarget.GetComponent<Transform>() == null )
				{
					EditorGUILayout.HelpBox( GetMessage( "TransformNone" ), MessageType.Warning, true ) ;		
				}
			}

		
			// ワイドモードを元に戻す
			EditorGUIUtility.wideMode = tWideMode ;


			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//--------------------------------------------------------------------

			if( tTarget.enabled == true )
			{
				GUILayout.BeginHorizontal() ;	// 横並び
				{
					// チェック
					GUILayout.Label( "Checker (Editor Only)", GUILayout.Width( 150f ) ) ;

					bool tIsChecker = EditorGUILayout.Toggle( tTarget.isChecker ) ;
					if( tIsChecker != tTarget.isChecker )
					{
						if( tIsChecker == true )
						{
							SoftTransformTween[] tTweenList = tTarget.gameObject.GetComponents<SoftTransformTween>() ;
							if( tTweenList != null && tTweenList.Length >  0 )
							{
								for( int i  = 0 ; i <  tTweenList.Length ; i ++ )
								{
									if( tTweenList[ i ] != tTarget )
									{
										if( tTweenList[ i ].isChecker == true )
										{
											tTweenList[ i ].isChecker  = false ;
										}
									}
								}
							}
						}

						Undo.RecordObject( tTarget, "SoftTransformTween : Checker Change" ) ;	// アンドウバッファに登録
						tTarget.isChecker = tIsChecker ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTarget.isChecker == true )
				{
					GUILayout.BeginHorizontal() ;	// 横並び
					{
						float tCheckFactor = EditorGUILayout.Slider( tTarget.checkFactor, 0, 1 ) ;
						if( tCheckFactor != tTarget.checkFactor )
						{
							tTarget.checkFactor = tCheckFactor ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}

			//--------------------------------------------------------------------

			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			// バリュータイプ
			SoftTransformTween.ValueType tValueType = ( SoftTransformTween.ValueType )EditorGUILayout.EnumPopup( "ValueType",  tTarget.valueType ) ;
			if( tValueType != tTarget.valueType )
			{
				Undo.RecordObject( tTarget, "SoftTransformTween : Value Type Change" ) ;	// アンドウバッファに登録
				tTarget.valueType = tValueType ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// ループ
				GUILayout.Label( "Loop", GUILayout.Width( 116f ) ) ;

				bool tLoop = EditorGUILayout.Toggle( tTarget.loop ) ;
				if( tLoop != tTarget.loop )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Loop Change" ) ;	// アンドウバッファに登録
					tTarget.loop = tLoop ;
					EditorUtility.SetDirty( tTarget ) ;
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
						Undo.RecordObject( tTarget, "SoftTransformTween : Reverse Change" ) ;	// アンドウバッファに登録
						tTarget.reverse = tReverse ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}

			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// イグノアタイムスケール
				GUILayout.Label( "Ignore Time Scale", GUILayout.Width( 116f ) ) ;

				bool tIgnoreTimeScale = EditorGUILayout.Toggle( tTarget.ignoreTimeScale ) ;
				if( tIgnoreTimeScale != tTarget.ignoreTimeScale )
				{
					Undo.RecordObject( tTarget, "SoftTransformTween : Ignore Time Scale Change" ) ;	// アンドウバッファに登録
					tTarget.ignoreTimeScale = tIgnoreTimeScale ;
					EditorUtility.SetDirty( tTarget ) ;
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
					Undo.RecordObject( tTarget, "SoftTransformTween : Play On Awake Change" ) ;	// アンドウバッファに登録
					tTarget.playOnAwake = tPlayOnAwake ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
//				// プレイオンアウェイク
//				GUILayout.Label( "Interaction Disable In Playing", GUILayout.Width( 180f ) ) ;
//
//				bool tInteractionDisableInPlaying = EditorGUILayout.Toggle( tTarget.interactionDisableInPlaying ) ;
//				if( tInteractionDisableInPlaying != tTarget.interactionDisableInPlaying )
//				{
//					Undo.RecordObject( tTarget, "SoftTransformTween : Interaction Disable In Playing Change" ) ;	// アンドウバッファに登録
//					tTarget.interactionDisableInPlaying = tInteractionDisableInPlaying ;
//					EditorUtility.SetDirty( tTarget ) ;
//				}
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了

//			if( tTarget.interactionDisableInPlaying == true )
//			{
//			}

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

		// 曲線を描画する
		private void DrawCurve( SoftTransformTween tTarget, float tCheckFactor, SoftTransformTween.ProcessType tProcessType, SoftTransformTween.EaseType tEaseType, AnimationCurve tAnimationCurve )
		{
			Rect tRect = GUILayoutUtility.GetRect( Screen.width - 64, 102f ) ;
		
			float x = 0 ;
			x = ( tRect.width - 52f ) * 0.5f ;
			if( x <  0 )
			{
				x  = 0 ;
			}
			tRect.x = x ;
			tRect.width = 52f ;
		
			EditorGUI.DrawRect( new Rect( tRect.x + 0, tRect.y + 0, tRect.width - 0, tRect.height - 0 ), new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ) ;
			EditorGUI.DrawRect( new Rect( tRect.x + 1, tRect.y + 1, tRect.width - 2, tRect.height - 2 ), new Color( 0.2f, 0.2f, 0.2f, 1.0f ) ) ;

			DrawLine(   0,  25, 49,  25, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  74, 49,  74, 0xFF7F7F7F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(  25,  99, 25,   0, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;
			DrawLine(   0,  49, 49,  49, 0xFF4F4F4F, tRect.x + 1.0f, tRect.y + 1.0f ) ;

			int px = 0, py = 0 ;
			int ox = 0, oy = 0 ;
			for( px  =   0 ; px <  50 ; px ++  )
			{
				py = ( int )SoftTransformTween.GetValue(   0,  50, ( float )px * 0.02f, tProcessType, tEaseType, tAnimationCurve ) ;

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

			px = ( int )( ( 50.0f * tCheckFactor ) + 0.5f ) ;
			DrawLine( px, 99, px,  0, 0xFFFF0000, tRect.x + 1.0f, tRect.y + 1.0f ) ;
		}


	//	private AnimationCurve mCurve  = AnimationCurve.EaseInOut( 0, 0, 1, 1 ) ;

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

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "TransformNone",		"Transform クラスが必要です" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "TransformNone",		"'Transorm' is necessary." },
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
