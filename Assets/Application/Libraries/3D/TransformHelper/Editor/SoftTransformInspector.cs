using UnityEngine ;
using UnityEngine.UI ;
using UnityEditor ;
using System.Collections.Generic ;

namespace TransformHelper
{
	[ CustomEditor( typeof( SoftTransform ) ) ]
	public class SoftTransformInspector : Editor
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			SoftTransform tTarget = target as SoftTransform ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-----------------------
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  40f ;

			GUI.backgroundColor = Color.cyan ;
			string tIdentity = EditorGUILayout.TextField( "Identity",  tTarget.identity ) ;
			GUI.backgroundColor = Color.white ;
			if( tIdentity !=  tTarget.identity )
			{
				Undo.RecordObject( tTarget, "SoftTransform : Identity Change" ) ;	// アンドウバッファに登録
				tTarget.identity = tIdentity ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;

			//------------------------------------------
		
			// Tween の追加と削除
			DrawTween( tTarget ) ;

			//------------------------------------------
		
			DrawInspectorGUI() ;
		}
	
		// 派生クラスの個々のＧＵＩを描画する
		virtual protected void DrawInspectorGUI(){}

		//-------------------------------------------------------------------------------------------

		// Tween の追加と削除
		private string m_AddTweenIdentity = "" ;
		private int    m_RemoveTweenIndex = 0 ;
		private int    m_RemoveTweenIndexAnswer = -1 ;

		protected void DrawTween( SoftTransform tTarget )
		{
			EditorGUILayout.Separator() ;	// 少し区切りスペース

			// 存在している Tween コンポーネントを取得する
			SoftTransformTween[] tTweenArray = tTarget.GetComponents<SoftTransformTween>() ;

			// １つ以上存在していればリストとして描画する
			int i, l = tTweenArray.Length, j, c ;
			string tIdentity ;
			string[] tTweenIdentityArray = new string[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tTweenIdentityArray[ i ] = tTweenArray[ i ].identity ;
			}
			for( i  = 0 ; i <  l ; i ++ )
			{
				// 既に同じ名前が存在する場合は番号を振る
				tIdentity = tTweenIdentityArray[ i ] ;

				c = 0 ;
				for( j  = i + 1 ; j <  l ; j ++ )
				{
					if( tTweenIdentityArray[ j ] == tIdentity )
					{
						// 同じ名前を発見した
						c ++ ;
						tTweenIdentityArray[ j ] = tTweenIdentityArray[ j ] + "(" + c + ")" ;
					}
				}
			}

			//----------------------------------------------------

			if( m_RemoveTweenIndexAnswer <  0 )
			{
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool tAdd = false ;

					GUI.backgroundColor = Color.cyan ;
					if( GUILayout.Button( "Add Tween", GUILayout.Width( 140f ) ) == true )
					{
						tAdd = true ;
					}
					GUI.backgroundColor = Color.white ;

					GUI.backgroundColor = Color.cyan ;
					m_AddTweenIdentity = EditorGUILayout.TextField( "", m_AddTweenIdentity, GUILayout.Width( 120f ) ) ;
					GUI.backgroundColor = Color.white ;

					if( tAdd == true )
					{
						if( string.IsNullOrEmpty( m_AddTweenIdentity ) == false )
						{
							// Tween を追加する
							SoftTransformTween tTween = tTarget.AddComponent<SoftTransformTween>() ;
							tTween.identity = m_AddTweenIdentity ;
	
							SoftTransformTween[] tTweenList = tTarget.gameObject.GetComponents<SoftTransformTween>() ;
							if( tTweenList != null && tTweenList.Length >  0 )
							{
								for( i  = 0 ; i <  tTweenList.Length ; i ++ )
								{
									if( tTweenList[ i ] != tTween )
									{
										break ;
									}
								}
								if( i <  tTweenList.Length )
								{
									// 既にトゥイーンコンポーネントがアタッチされているので enable と PlayOnAwake を false にする
									tTween.enabled = false ;
									tTween.playOnAwake = false ;
								}
							}
						}
						else
						{
							EditorUtility.DisplayDialog( "Add Tween", GetMessage( "InputIdentity" ), "Close" ) ;
						}
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了

				if( tTweenArray != null && tTweenArray.Length >  0 )
				{
					GUILayout.BeginHorizontal() ;	// 横並び開始
					{
						bool tRemove = false ;
						GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
						if( GUILayout.Button( "Remove Tween", GUILayout.Width( 140f ) ) == true )
						{
							tRemove = true ;
						}
						GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

						if( m_RemoveTweenIndex >= tTweenIdentityArray.Length )
						{
							m_RemoveTweenIndex  = tTweenIdentityArray.Length - 1 ;
						}
						m_RemoveTweenIndex = EditorGUILayout.Popup( "", m_RemoveTweenIndex, tTweenIdentityArray, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
				
						if( tRemove == true )
						{
							// 削除する
							m_RemoveTweenIndexAnswer = m_RemoveTweenIndex ;
						}
					}
					GUILayout.EndHorizontal() ;		// 横並び終了
				}
			}
			else
			{
				string tMessage = GetMessage( "RemoveTweenOK?" ).Replace( "%1", tTweenIdentityArray[ m_RemoveTweenIndexAnswer ] ) ;
				GUILayout.Label( tMessage ) ;
	//			GUILayout.Label( "It does really may be to remove tween '" + tTweenIdentityArray[ mRemoveTweenIndexAnswer ] + "' ?" ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( tTarget, "UIView : Tween Remove" ) ;	// アンドウバッファに登録
						tTarget.removeTweenIdentity = tTweenArray[ m_RemoveTweenIndexAnswer ].identity ;
						tTarget.removeTweenInstance = tTweenArray[ m_RemoveTweenIndexAnswer ].GetInstanceID() ;
						EditorUtility.SetDirty( tTarget ) ;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;

						m_RemoveTweenIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						m_RemoveTweenIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "Tween [ %1 ] を削除してもよろしいですか？" },
			{ "RemoveFlipperOK?", "Flipper [ %1 ] を削除してもよろしいですか？" },
			{ "EventTriggerNone", "EventTrigger クラスが必要です" },
			{ "InputIdentity",   "識別子を入力してください" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "RemoveTweenOK?",   "It does really may be to remove tween %1 ?" },
			{ "RemoveFlipperOK?", "It does really may be to remove flipper %1 ?" },
			{ "EventTriggerNone", "'EventTrigger' is necessary." },
			{ "InputIdentity",   "Input identity !" },
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
