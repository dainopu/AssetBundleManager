using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// ImageNumber のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( ImageNumber ) ) ]
	public class ImageNumberInspector : MaskableGraphicWrapperInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			ImageNumber tTarget = target as ImageNumber ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// アトラス
			DrawAtlas( tTarget ) ;

			// １文字あたりの大きさ
			Vector2 tCodeScale = EditorGUILayout.Vector2Field( "Code Scale", tTarget.codeScale ) ;
			if( tCodeScale != tTarget.codeScale && tCodeScale.x >  0 && tCodeScale.y >  0 )
			{
				Undo.RecordObject( tTarget, "ImageNumber : Code Scale Change" ) ;	// アンドウバッファに登録
				tTarget.codeScale = tCodeScale ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// 文字間隔
			float tCodeSpace = EditorGUILayout.FloatField( "Code Space", tTarget.codeSpace ) ;
			if( tCodeSpace != tTarget.codeSpace )
			{
				Undo.RecordObject( tTarget, "ImageNumber : Code Space Change" ) ;	// アンドウバッファに登録
				tTarget.codeSpace = tCodeSpace ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			// データ配列
			SerializedObject tSO = new SerializedObject( tTarget ) ;
			SerializedProperty tSP = tSO.FindProperty( "codeOffset" ) ;
			if( tSP != null )
			{
				EditorGUILayout.PropertyField( tSP, new GUIContent( "codeOffset" ), true ) ;
			}
			tSO.ApplyModifiedProperties() ;

			// コードアンカー
			TextAnchor tAlignment = ( TextAnchor )EditorGUILayout.EnumPopup( "Alignment",  tTarget.alignment ) ;
			if( tAlignment != tTarget.alignment )
			{
				Undo.RecordObject( tTarget, "ImageNumber : Alignement Change" ) ;	// アンドウバッファに登録
				tTarget.alignment = tAlignment ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}
			
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// 表示する値
			GUI.backgroundColor = new Color( 0.0f, 1.0f, 1.0f, 1.0f ) ;	// ＧＵＩの下地を灰にする
			int tValue = EditorGUILayout.IntField( "Value", tTarget.value, GUILayout.Width( 200f ) ) ;
			GUI.backgroundColor = Color.white ;
			if( tValue != tTarget.value )
			{
				// 変化があった場合のみ処理する
				Undo.RecordObject( tTarget, "ImageNumber : Value Change" ) ;	// アンドウバッファに登録
				tTarget.value = tValue ;
				EditorUtility.SetDirty( tTarget ) ;
//				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
			}

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			//-----------------------------------------------------
			
			EditorGUIUtility.labelWidth =  60f ;
			EditorGUIUtility.fieldWidth =  30f ;
		
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 縦方向揃え
				GUILayout.Label( "Digit", GUILayout.Width( 40.0f ) ) ;	// null でないなら 74
				int tDigitInteger = EditorGUILayout.IntField( tTarget.digitInteger, GUILayout.Width( 40f ) ) ;
				if( tDigitInteger != tTarget.digitInteger )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "ImageNumber : Digit Integer Change" ) ;	// アンドウバッファに登録
					tTarget.digitInteger = tDigitInteger ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			
				GUILayout.Label( ".", GUILayout.Width( 10.0f ) ) ;	// null でないなら 74
			
				int tDigitDecimal = EditorGUILayout.IntField( tTarget.digitDecimal, GUILayout.Width( 40f ) ) ;
				if( tDigitDecimal != tTarget.digitDecimal )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "ImageNumber : Digit Decimal Change" ) ;	// アンドウバッファに登録
					tTarget.digitDecimal = tDigitDecimal ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				// 適当なスペース
				GUILayout.Label( "", GUILayout.Width( 5f ) ) ;

				// カンマ
				GUILayout.Label( "Comma", GUILayout.Width( 50.0f ) ) ;	// null でないなら 74
				int tComma = EditorGUILayout.IntField( tTarget.comma, GUILayout.Width( 40f ) ) ;
				if( tComma != tTarget.comma )
				{
					// 変化があった場合のみ処理する
					Undo.RecordObject( tTarget, "ImageNumber : Comma Change" ) ;	// アンドウバッファに登録
					tTarget.comma = tComma ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
			EditorGUIUtility.labelWidth = 116f ;
			EditorGUIUtility.fieldWidth =  40f ;
		
			// 符号を表示するか否か
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPlusSign = EditorGUILayout.Toggle( tTarget.plusSign, GUILayout.Width( 16f ) ) ;
				if( tPlusSign != tTarget.plusSign )
				{
					Undo.RecordObject( tTarget, "ImageNumber : Plus Sign Change" ) ;	// アンドウバッファに登録
					tTarget.plusSign = tPlusSign ;
					EditorUtility.SetDirty( tTarget ) ;
//					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}
				GUILayout.Label( "Plus Sign", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// 符号を表示するか否か
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroSign = EditorGUILayout.Toggle( tTarget.zeroSign, GUILayout.Width( 16f ) ) ;
				if( tZeroSign != tTarget.zeroSign )
				{
					Undo.RecordObject( tTarget, "ImageNumber : Zero Sign Change" ) ;	// アンドウバッファに登録
					tTarget.zeroSign = tZeroSign ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Sign", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		
//			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;

			// ０埋め
			GUILayout.BeginHorizontal() ;	// 横並び
			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tZeroPadding = EditorGUILayout.Toggle( tTarget.zeroPadding, GUILayout.Width( 16f ) ) ;
				if( tZeroPadding != tTarget.zeroPadding )
				{
					Undo.RecordObject( tTarget, "ImageNumber : Zero Padding Change" ) ;	// アンドウバッファに登録
					tTarget.zeroPadding = tZeroPadding ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Zero Padding", GUILayout.Width( 80f ) ) ;
//			}
//			GUILayout.EndHorizontal() ;		// 横並び終了
		
			GUILayout.Label( "", GUILayout.Width( 10f ) ) ;
		
			// パーセント
//			GUILayout.BeginHorizontal() ;	// 横並び
//			{
				// 各種ＧＵＩのデフォルトリソースのロードを有効にするか
				bool tPercent = EditorGUILayout.Toggle( tTarget.percent, GUILayout.Width( 16f ) ) ;
				if( tPercent != tTarget.percent )
				{
					Undo.RecordObject( tTarget, "ImageNumber : Percent Change" ) ;	// アンドウバッファに登録
					tTarget.percent = tPercent ;
					EditorUtility.SetDirty( tTarget ) ;
				}
				GUILayout.Label( "Percent", GUILayout.Width( 80f ) ) ;
			}
			GUILayout.EndHorizontal() ;		// 横並び終了
		}


		// AtlasSprite の項目を描画する
		private void DrawAtlas( ImageNumber tTarget )
		{
			UIAtlasSprite tAtlasSprite ;
			
			Texture tAtlasTextureBase = null ;
			if( tTarget.atlasSprite != null )
			{
				tAtlasTextureBase = tTarget.atlasSprite.texture ;
			}

			Texture tAtlasTexture = EditorGUILayout.ObjectField( "Atlas Texture", tAtlasTextureBase, typeof( Texture ), false ) as Texture ;
			if( tAtlasTexture != tAtlasTextureBase )
			{
				Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録

				List<Sprite> tList = new List<Sprite>() ;

				if( tAtlasTexture != null )
				{
					string tPath = AssetDatabase.GetAssetPath( tAtlasTexture.GetInstanceID() ) ;

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

					tAtlasSprite = UIAtlasSprite.Create() ;
					tAtlasSprite.Set( tList.ToArray() ) ;
/*					tAtlasSprite.name = tAtlasTexture.name + "(Instance Only)" ;*/
					tTarget.atlasSprite = tAtlasSprite ;
				}
				else
				{
					// 存在しないのでクリアする
					tTarget.atlasSprite = null ;
				}

				EditorUtility.SetDirty( tTarget ) ;
			}
		}
	}
}
