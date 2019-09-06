using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGridMap のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIComplexRectangle ) ) ]
	public class UIComplexRectangleInspector : UIViewInspector
	{
		private int m_SpriteIndex = 0 ;


		/// <summary>
		/// スンスペクター描画
		/// </summary>
		override protected void DrawInspectorGUI()
		{
			// ターゲットのインスタンス
			UIComplexRectangle tTarget = target as UIComplexRectangle ;
		
			EditorGUILayout.Separator() ;	// 少し区切りスペース
		
			//-------------------------------------------------------------------
			
			// アトラススプライトの表示
			DrawAtlasForComplexRectangle( tTarget ) ;

			// マテリアル選択
			DrawMaterial( tTarget ) ;

			//-------------------------------------------------------------------
			//-------------------------------------------------------------------
		
/*			EditorGUILayout.Separator() ;	// 少し区切りスペース
			
			bool tAutoSizeFitting = EditorGUILayout.Toggle( "Auto Size Fitting", tTarget.autoSizeFitting ) ;
			if( tAutoSizeFitting != tTarget.autoSizeFitting )
			{
				Undo.RecordObject( tTarget, "UIComplexRectangle : Auto Size Fitting Change" ) ;	// アンドウバッファに登録
				tTarget.autoSizeFitting = tAutoSizeFitting ;
				EditorUtility.SetDirty( tTarget ) ;
			}*/
		}


		// AtlasSprite の項目を描画する
		protected void DrawAtlasForComplexRectangle( UIView tView )
		{
			UIComplexRectangle tTarget = null ;
			if( tView is UIComplexRectangle )
			{
				tTarget = tView as UIComplexRectangle ;
			}
			else
			{
				return ;
			}

			Texture tAtlasTextureBase = null ;
			if( tTarget.atlasSprite != null )
			{
				tAtlasTextureBase = tTarget.atlasSprite.texture ;
			}

			bool tAtlasTextureRefresh = false ;

			Texture tAtlasTexture = EditorGUILayout.ObjectField( "Atlas Texture", tAtlasTextureBase, typeof( Texture ), false ) as Texture ;
			if( tAtlasTexture != tAtlasTextureBase )
			{
				Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録

				RefreshAtlasSpriteForComplexRectangle( tTarget, tAtlasTexture ) ;

				tTarget.texture = tTarget.atlasSprite.texture ;

				EditorUtility.SetDirty( tTarget ) ;

				tAtlasTextureRefresh = true ;
			}

			GUILayout.Label( "Atlas Path" ) ;
			GUILayout.BeginHorizontal() ;
			{
				GUI.color = Color.cyan ;
				GUILayout.Label( "Resources/" ) ;
				GUI.color = Color.white ;

				string tAtlasPath = EditorGUILayout.TextField( "", tTarget.atlasSprite.path ) ;
				if( tAtlasPath != tTarget.atlasSprite.path )
				{
					Undo.RecordObject( tTarget, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
					tTarget.atlasSprite.path = tAtlasPath ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				tAtlasPath = "" ;
				if( tTarget.atlasSprite.texture != null )
				{
					string tPath = AssetDatabase.GetAssetPath( tTarget.atlasSprite.texture.GetInstanceID() ) ;
					if( System.IO.File.Exists( tPath ) == true )
					{
						string c = "/Resources/" ;
						int p = tPath.IndexOf( c ) ;
						if( p >= 0 )
						{
							// 有効なパス
							tAtlasPath = tPath.Substring( p + c.Length, tPath.Length - ( p + c.Length ) ) ;
							p = tAtlasPath.IndexOf( "." ) ;
							if( p >= 0 )
							{
								tAtlasPath = tAtlasPath.Substring( 0, p ) ;
							}
						}
					}
				}

				if( string.IsNullOrEmpty( tAtlasPath ) == false )
				{
					if( tAtlasPath != tTarget.atlasSprite.path )
					{
						GUI.backgroundColor = Color.yellow ;
					}
					else
					{
						GUI.backgroundColor = Color.white ;
					}
					if( GUILayout.Button( "Set", GUILayout.Width( 50f ) ) == true || tAtlasTextureRefresh == true )
					{
						Undo.RecordObject( tTarget, "UIAtlasSprite Path : Change" ) ;	// アンドウバッファに登録
						tTarget.atlasSprite.path = tAtlasPath ;
						EditorUtility.SetDirty( tTarget ) ;
					}
					GUI.backgroundColor = Color.white ;
				}
			}
			GUILayout.EndHorizontal() ;

			//-----------------------------------------------------
			
			// 一覧から選択出来るようにする


//			if( tTarget.atlasSprite != null && tTarget.atlasSprite.texture != null )
//			{
//				GUI.backgroundColor = Color.yellow ;
//				if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
//				{
//					Undo.RecordObject( tTarget, "UIAtlasSprite Texture : Change" ) ;	// アンドウバッファに登録
//					RefreshAtlasSprite( tTarget, tTarget.atlasSprite.texture ) ;
//					EditorUtility.SetDirty( tTarget ) ;
//				}
//				GUI.backgroundColor = Color.white ;
//			}

			if( tTarget.atlasSprite != null )
			{
				EditorGUILayout.Separator() ;	// 少し区切りスペース

				if( GUILayout.Button( "Reload", GUILayout.Width( 50f ) ) == true || ( tTarget.atlasSprite.isAvailable == false && tTarget.atlasSprite.texture != null && Application.isPlaying == false ) )
				{
					// データに異常が発生しているので自動的に更新する
					Debug.LogWarning( "Atlas を自動的に更新:" + tTarget.atlasSprite.texture.name ) ;
					RefreshAtlasSpriteForComplexRectangle( tTarget, tTarget.atlasSprite.texture ) ;
					EditorUtility.SetDirty( tTarget ) ;
					UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() ) ;
				}

				string[] tName = tTarget.atlasSprite.GetNameList() ;
				if( tName != null && tName.Length >  0 )
				{
					// ソートする
					List<string> tSortName = new List<string>() ;

					int i, l = tName.Length ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						tSortName.Add( tName[ i ] ) ;
					}
					tSortName.Sort() ;
					tName = tSortName.ToArray() ;

					if( m_SpriteIndex >= tName.Length )
					{
						m_SpriteIndex  = tName.Length - 1 ;
					}

					// フレーム番号
					m_SpriteIndex = EditorGUILayout.Popup( "Selected Sprite", m_SpriteIndex, tName ) ;

					// 確認用
					EditorGUILayout.ObjectField( "", tTarget.atlasSprite[ tName[ m_SpriteIndex ] ], typeof( Sprite ), false, GUILayout.Width( 60f ), GUILayout.Height( 60f ) ) ;
				}
			}
		}

		private void RefreshAtlasSpriteForComplexRectangle( UIComplexRectangle tTarget, Texture tAtlasTexture )
		{
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

				if( tList.Count >  0 )
				{
					// 存在するので更新する

//					UIAtlasSprite tAtlasSprite = UIAtlasSprite.Create() ;
					tTarget.atlasSprite.Clear() ;
					tTarget.atlasSprite.Set( tList.ToArray() ) ;
//					tTarget.atlasSprite = tAtlasSprite ;
				}
				else
				{
					// 存在しないのでクリアする
					if(	tTarget.atlasSprite != null )
					{
						tTarget.atlasSprite.Clear() ;
					}
				}
			}
			else
			{
				if(	tTarget.atlasSprite != null )
				{
					tTarget.atlasSprite.Clear() ;
				}
			}
		}


	}
}

