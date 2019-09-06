using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections.Generic ;

/// <summary>
/// シンプルスプライトパッカーパッケージ
/// </summary>
namespace SimpleSpritePacker
{
	/// <summary>
	/// スプライトパッカークラス(エディター用) Version 2017/08/18 0
	/// </summary>
	public class SimpleSpritePacker : EditorWindow
	{
		[ MenuItem( "Tools/Simple Sprite Packer" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleSpritePacker>( false, "Sprite Packer", true ) ;
		}

		// 要素がどのように変化するか
		public enum SpriteAction
		{
			None,		// 要素維持
			Add,		// 要素追加
			Update,		// 要素更新
			Delete,		// 要素削除
		} ;

		// 要素の情報
		public class SpriteElement
		{
			public Texture2D		texture ;	// 追加または更新が行われ場合の別テクスチャファイルのインスタンス
			public SpriteMetaData	metaData ;
			public SpriteAction		action ;
			public int				type ;      // 要素となる画像がテクスチャ・シングルタイプスプライト(0)なのかマルチプルタイプスプライト(1)なのか

			public TextureImporterCompression	format ;
			public TextureImporterNPOTScale		NPOTscale ;
		} ;

		private string m_AtlasPath = "Assets/" ;
		private string m_AtlasName = "A New Atlas" ;
		private string m_AtlasFullPath = "" ;

		private Dictionary<string,SpriteElement> m_SpriteElementHash = new Dictionary<string,SpriteElement>() ;
	
		private int	m_Padding = 2 ;

		private bool m_OutputSourceTexture = false ;
		private string m_OutputSourceTexturePath = "" ;

		private Vector2 m_ScrollPosition = Vector2.zero ;
	
		//----------------------------------------------------------

		// レイアウトを描画する
		private void OnGUI()
		{
			Texture2D tAtlas = null ;

			// 保存先のパスの設定
			GUILayout.BeginHorizontal() ;
			{
				// 保存パスを選択する
				if( GUILayout.Button( "Atlas Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
					{
						// ルート
						m_AtlasPath = "Assets/" ;
					}
					else
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						string tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( System.IO.Directory.Exists( tPath ) == true )
						{
							// フォルダを指定しています
						
							// ファイルかどうか判別するには System.IO.File.Exists
						
							// 有効なフォルダ
							tPath = tPath.Replace( "\\", "/" ) ;
							m_AtlasPath = tPath + "/" ;
						}
						else
						{
							// ファイルを指定しています
							tPath = tPath.Replace( "\\", "/" ) ;
							m_AtlasPath = tPath ;

							// 拡張子を見てアセットバンドルであればファイル名まで置き変える
							// ただしこれを読み出して含まれるファイルの解析などは行わない
							// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
						
							// 最後のフォルダ区切り位置を取得する
							int tIndex = m_AtlasPath.LastIndexOf( '/' ) ;
							if( tIndex >= 0 )
							{
								m_AtlasPath = m_AtlasPath.Substring( 0, tIndex ) + "/" ;
							}

							if( tPath.Length >  4 )
							{
								if( tPath.Substring( tPath.Length - 4, 4 ) == ".png" )
								{
									tPath = tPath.Substring( tIndex + 1, tPath.Length - ( tIndex + 1 ) ) ;
									m_AtlasName = tPath.Substring(  0, tPath.Length - 4 ) ;
								}
							}
						}
					}
				}
			
				// 保存パス
				m_AtlasPath = EditorGUILayout.TextField( m_AtlasPath ) ;
			
				// 名前
				m_AtlasName = EditorGUILayout.TextField( m_AtlasName ) ;
			}
			GUILayout.EndHorizontal() ;

			// 指定しているパスのファイルがアトラスならばインスタンスを取得する
			string tAtlasFullPath = m_AtlasPath + m_AtlasName + ".png" ;
			if( string.IsNullOrEmpty( tAtlasFullPath ) == false )
			{
				TextureImporter tTextureImporter = AssetImporter.GetAtPath( tAtlasFullPath ) as TextureImporter ;
				if( tTextureImporter != null && tTextureImporter.textureType == TextureImporterType.Sprite )
				{
					tAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>( tAtlasFullPath ) ;
				}
			}

			GUILayout.Space( 6f ) ;

			//-----------------------------------------------------
		
			// アトラステクスチャ情報の表示を行う
			if( tAtlas != null )
			{
				GUILayout.BeginHorizontal() ;
				{
					GUILayout.Label( "Path", GUILayout.Width( 36f ) ) ;
					GUI.color = Color.cyan ;
					GUILayout.Label( tAtlasFullPath ) ;
					GUI.color = Color.white ;
				}
				GUILayout.EndHorizontal() ;

				// アトラスが選択されている時のみ表示編集が可能となる
				GUILayout.BeginHorizontal() ;
				{
					if( GUILayout.Button( "Sprite", GUILayout.Width( 76f ) ) )
					{
						Selection.activeObject = tAtlas ;
					}
					GUILayout.Label( " " + tAtlas.width + "x" + tAtlas.height ) ;
				}
				GUILayout.EndHorizontal() ;
			}

			//------------------------------------------

			bool tClear = false ;
			if( m_AtlasFullPath != tAtlasFullPath )
			{
				m_AtlasFullPath  = tAtlasFullPath ;
				tClear  = true ;
			}

			// リストを更新する
			UpdateList( tAtlas, tClear ) ;

			//-------------------------------------------------

			bool tExecute  = false ;

			if( m_SpriteElementHash.Count >  0 )
			{
				// マルチタイプのスプライトで要素が１以上存在する

				GUILayout.BeginHorizontal() ;	// 横一列開始
				{
					GUILayout.Label( "Padding", GUILayout.Width( 76f ) ) ;
					m_Padding = Mathf.Clamp( EditorGUILayout.IntField( m_Padding, GUILayout.Width( 50f ) ),  0, 10 ) ;
				}
				GUILayout.EndHorizontal() ;		// 横一列終了


				GUILayout.BeginHorizontal() ;
				{
					if( tAtlas == null )
					{
						// 新規作成
						// 新規作成で生成可能
						GUI.backgroundColor = Color.green ;
						tExecute = GUILayout.Button( "Create" ) ;
						GUI.backgroundColor = Color.white ;
					}
					else
					{
						// 維持更新
						GUI.backgroundColor = Color.cyan ;
						tExecute = GUILayout.Button( "Update" ) ;		// 更新または追加
						GUI.backgroundColor = Color.white ;
					}
				}
				GUILayout.EndHorizontal() ;
				

				// テクスチャ分解出力
				if( tAtlas != null )
				{
					// テクスチャ分解出力パス
					GUILayout.BeginHorizontal() ;
					{
						EditorGUIUtility.labelWidth = 140f ;
						EditorGUIUtility.fieldWidth =  40f ;
						m_OutputSourceTexture = EditorGUILayout.Toggle( "Output Divided Sprite", m_OutputSourceTexture ) ;
						EditorGUIUtility.labelWidth =  80f ;
						EditorGUIUtility.fieldWidth =  50f ;
					}
					GUILayout.EndHorizontal() ;
			
					if( m_OutputSourceTexture == true )
					{
						// 保存先のパスの設定
						GUILayout.BeginHorizontal() ;
						{
							// 保存パスを選択する
							if( GUILayout.Button( "Output Path", GUILayout.Width( 100f ) ) == true )
							{
								if( Selection.objects != null && Selection.objects.Length == 0 && Selection.activeObject == null )
								{
									// ルート
									m_OutputSourceTexturePath = "Assets/" ;
								}
								else
								if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
								{
									string tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
									if( System.IO.Directory.Exists( tPath ) == true )
									{
										// フォルダを指定しています
								
										// ファイルかどうか判別するには System.IO.File.Exists
								
										// 有効なフォルダ
										tPath = tPath.Replace( "\\", "/" ) ;
										m_OutputSourceTexturePath = tPath + "/" ;
									}
									else
									{
										// ファイルを指定しています
										tPath = tPath.Replace( "\\", "/" ) ;
								
										// 拡張子を見てアセットバンドルであればファイル名まで置き変える
										// ただしこれを読み出して含まれるファイルの解析などは行わない
										// なぜなら違うプラットフォームの場合は読み出せずにエラーになってしまうから
								
										// 最後のフォルダ区切り位置を取得する
										int tIndex = tPath.LastIndexOf( '/' ) ;
								
										m_OutputSourceTexturePath = tPath.Substring( 0, tIndex ) + "/" ;
									}
								}
							}
					
							// 保存パス
							m_OutputSourceTexturePath = EditorGUILayout.TextField( m_OutputSourceTexturePath ) ;
					
							if( string.IsNullOrEmpty( m_OutputSourceTexturePath ) == false && tAtlas != null )
							{
								if( Directory.Exists( m_OutputSourceTexturePath ) == true )
								{
									if( GUILayout.Button( "Execute", GUILayout.Width( 80f ) ) == true )
									{
										OutputDividedSprite( tAtlas, m_OutputSourceTexturePath ) ;
									}
								}
							}
						}
						GUILayout.EndHorizontal() ;
					}
				}
			}

			//------------------------------------------
		
			// 素材情報のリストを表示する
			if( m_SpriteElementHash.Count >  0 )
			{
				EditorGUILayout.Separator() ;
			
				EditorGUILayout.LabelField( "Target (" + m_SpriteElementHash.Count + ")" ) ;

				GUILayout.BeginVertical() ;
				{
					m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
					{
						foreach( KeyValuePair<string,SpriteElement> tSpriteElementKeyValue in m_SpriteElementHash )
						{
							string tName = tSpriteElementKeyValue.Key ;
							SpriteElement tSpriteElement = tSpriteElementKeyValue.Value ;
						
							GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
							{
								GUILayout.Label( tName, GUILayout.Height( 20f ) ) ;
							
								switch( tSpriteElement.action )
								{
									// 通常状態
									case SpriteAction.None :
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											tSpriteElement.action = SpriteAction.Delete ;	// 破棄対象にする
										}
									break ;

									// 追加対象
									case SpriteAction.Add :
										GUI.backgroundColor = Color.green ;
										GUILayout.Box( "Add", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 更新対象
									case SpriteAction.Update :
										GUI.backgroundColor = Color.yellow ;
										GUILayout.Box( "Update", GUILayout.Width( 60f ) ) ;
										GUI.backgroundColor = Color.white ;
									break ;

									// 削除対象
									case SpriteAction.Delete :
										GUI.backgroundColor = Color.red ;
										if( GUILayout.Button( "Delete", GUILayout.Width( 60f ) ) )
										{
											tSpriteElement.action = SpriteAction.None ;
										}
										GUI.backgroundColor = Color.white ;
									break ;
								}
							}
							GUILayout.EndHorizontal() ;		// 横一列終了
						}
					}
					GUILayout.EndScrollView() ;
				}
				GUILayout.EndVertical() ;
			}
			else
			{
				EditorGUILayout.HelpBox( GetMessage( "SelectTexture" ), MessageType.Info ) ;
			}

			//-------------------------------------------

			if( tExecute == true )
			{
				// アトラスを生成する
				BuildAtlas( tAtlas ) ;
			}
		}

		private void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//---------------------------------------------------------------

		// 素材リストの情報を生成する
		private void UpdateList( Texture2D tAtlas, bool tClear )
		{
			if( tClear == true )
			{
				m_SpriteElementHash.Clear() ;
			}
		
			Dictionary<string,SpriteElement> tSpriteElementHash = new Dictionary<string,SpriteElement>() ;

			if( tAtlas != null )
			{
				// 既にある場合はアトラス内のスプライト情報を展開する
				string tAtlasPath = AssetDatabase.GetAssetPath( tAtlas.GetInstanceID() ) ;
			
				TextureImporter tTextureImporter = AssetImporter.GetAtPath( tAtlasPath ) as TextureImporter ;
			
				foreach( SpriteMetaData tSpriteMetaData in tTextureImporter.spritesheet )
				{
					SpriteElement tSpriteElement = null ;
				
					string tName = tSpriteMetaData.name ;

					if( m_SpriteElementHash.TryGetValue( tName, out tSpriteElement ) )
					{
						// 既にリストに登録済みの情報
						tSpriteElement.texture = null ;

						tSpriteElement.metaData = tSpriteMetaData ;

						if( tSpriteElement.action == SpriteAction.Delete )
						{
							tSpriteElement.action  = SpriteAction.Delete ;
						}
						else
						{
							tSpriteElement.action  = SpriteAction.None ;
						}
						tSpriteElement.type = 0 ;
					}
					else
					{
						// リストに存在しない情報
						tSpriteElement = new SpriteElement() ;
						tSpriteElement.texture = null ;

						tSpriteElement.metaData = tSpriteMetaData ;

						tSpriteElement.action = SpriteAction.None ;
						tSpriteElement.type = 0 ;
					}

					tSpriteElementHash.Add( tName, tSpriteElement ) ;
				}
			}

			// 選択中の素材を追加する
			foreach( UnityEngine.Object tObject in Selection.objects )
			{
				Texture2D tTexture = tObject as Texture2D ;
			
				if( tTexture == null || tTexture == tAtlas )
				{
					continue ;
				}

				//-----------------------------------------

				SpriteElement tSpriteElement = null ;
			
				string tName ;

				// 素材となる画像がスプライト（アトラス）かそれ以外（テクスチャ）かで処理が異なる
				string tTexturePath = AssetDatabase.GetAssetPath( tTexture.GetInstanceID() ) ;
				TextureImporter tTextureImporter = AssetImporter.GetAtPath( tTexturePath ) as TextureImporter ;
				if( tTextureImporter != null && tTextureImporter.textureType == TextureImporterType.Sprite )
				{
					// スプライト扱い
					if( tTextureImporter.spriteImportMode == SpriteImportMode.Single )
					{
						// シングルタイプ
						tName = tTexture.name ;

						if( tSpriteElementHash.TryGetValue( tName, out tSpriteElement ) )
						{
							// 既に存在するのでアップデートになる
							tSpriteElement.texture = tTexture ;
						
							tSpriteElement.metaData.border	= tTextureImporter.spriteBorder ;
							tSpriteElement.metaData.pivot	= tTextureImporter.spritePivot ;
						
							tSpriteElement.action = SpriteAction.Update ;
							tSpriteElement.type = 0 ;
						}
						else
						{
							// 存在しないため追加となる
							tSpriteElement = new SpriteElement() ;
							tSpriteElement.texture = tTexture ;
						
							tSpriteElement.metaData = new SpriteMetaData() ;
							tSpriteElement.metaData.name	= tName ;

							tSpriteElement.metaData.border	= tTextureImporter.spriteBorder ;
							tSpriteElement.metaData.pivot	= tTextureImporter.spritePivot ;

							tSpriteElement.action = SpriteAction.Add ;
							tSpriteElement.type = 0 ;

							tSpriteElementHash.Add( tName, tSpriteElement ) ;
						}
					}
					else
					if( tTextureImporter.spriteImportMode == SpriteImportMode.Multiple )
					{
						// マルチプルタイプ
						foreach( SpriteMetaData tSpriteMetaData in tTextureImporter.spritesheet )
						{
							tName = tSpriteMetaData.name ;
						
							if( tSpriteElementHash.TryGetValue( tName, out tSpriteElement ) )
							{
								// 既に存在するのでアップデートになる
								tSpriteElement.texture = tTexture ;
							
								tSpriteElement.metaData = tSpriteMetaData ;

								tSpriteElement.action = SpriteAction.Update ;
								tSpriteElement.type = 1 ;
							}
							else
							{
								// 存在しないため追加となる
								tSpriteElement = new SpriteElement() ;
								tSpriteElement.texture = tTexture ;
							
								tSpriteElement.metaData = tSpriteMetaData ;

								tSpriteElement.action = SpriteAction.Add ;
								tSpriteElement.type = 1 ;

								tSpriteElementHash.Add( tName, tSpriteElement ) ;
							}
						}
					}
				}
				else
				{
					// テクスチャ扱い
					tName = tTexture.name ;
				
					if( tSpriteElementHash.TryGetValue( tName, out tSpriteElement ) )
					{
						// 既に存在するのでアップデートになる
						tSpriteElement.texture = tTexture ;
					
						tSpriteElement.action = SpriteAction.Update ;
						tSpriteElement.type = 0 ;
					}
					else
					{
						// 存在しないため追加となる
						tSpriteElement = new SpriteElement() ;
						tSpriteElement.texture = tTexture ;
					
						tSpriteElement.metaData = new SpriteMetaData() ;
						tSpriteElement.metaData.name = tName ;
					
						tSpriteElement.action = SpriteAction.Add ;
						tSpriteElement.type = 0 ;

						tSpriteElementHash.Add( tName, tSpriteElement ) ;
					}
				}
			}

			m_SpriteElementHash = tSpriteElementHash ;
		}

		// アトラステクスチャの更新を実行する
		private void BuildAtlas( Texture2D tAtlas )
		{
			Texture2D tTexture = new Texture2D( 1, 1, TextureFormat.ARGB32, false ) ;
		
			List<SpriteMetaData> tSpriteSheetList = new List<SpriteMetaData>() ;
			List<Texture2D> tSpriteTextureList = new List<Texture2D>() ;
			List<string> tDeleteSpriteList = new List<string>() ;
		
			string tPath ;

			// 元の状態を保存する(バックアップが必要な項目)
			TextureImporterCompression	tAtlasFormat	= TextureImporterCompression.Uncompressed ;
			TextureImporterNPOTScale	tAtlasNPOtScale	= TextureImporterNPOTScale.None ;

			if( tAtlas != null )
			{
				// 既に作成済みのアトラスの更新
				tPath = AssetDatabase.GetAssetPath( tAtlas.GetInstanceID() ) ;
			
				SetTextureSetting( tPath, true, ref tAtlasFormat, ref tAtlasNPOtScale ) ;	// 書き込み属性を有効にする
			
				tAtlas = AssetDatabase.LoadAssetAtPath( tPath, typeof( Texture2D ) ) as Texture2D ;
			}
		
			foreach( KeyValuePair<string,SpriteElement> tSpriteElementKeyValue in m_SpriteElementHash )
			{
				string tName = tSpriteElementKeyValue.Key ;
				SpriteElement tSpriteElement = tSpriteElementKeyValue.Value ;

				Texture2D tElementTexture ;
				int x, y, w, h ;

				switch( tSpriteElement.action )
				{
					// アトラススプライトに内包される領域
					case SpriteAction.None :
						tSpriteSheetList.Add( tSpriteElement.metaData ) ;
					
						x = ( int )tSpriteElement.metaData.rect.x ;
						y = ( int )tSpriteElement.metaData.rect.y ;
						w = ( int )tSpriteElement.metaData.rect.width ;
						h = ( int )tSpriteElement.metaData.rect.height ;
					
						tElementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
						tElementTexture.SetPixels( tAtlas.GetPixels( x, y, w, h ) ) ;
						tElementTexture.Apply() ;
					
						tSpriteTextureList.Add( tElementTexture ) ;
					break ;

					// 新規追加
					case SpriteAction.Add :
						tSpriteSheetList.Add( tSpriteElement.metaData ) ;
					
						tPath = AssetDatabase.GetAssetPath( tSpriteElement.texture.GetInstanceID() ) ;
					
						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						SetTextureSetting( tPath, true, ref tSpriteElement.format, ref tSpriteElement.NPOTscale ) ;
					
						if( tSpriteElement.type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							tSpriteElement.texture = AssetDatabase.LoadAssetAtPath( tPath, typeof( Texture2D ) ) as Texture2D ;
							tSpriteTextureList.Add( tSpriteElement.texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )tSpriteElement.metaData.rect.x ;
							y = ( int )tSpriteElement.metaData.rect.y ;
							w = ( int )tSpriteElement.metaData.rect.width ;
							h = ( int )tSpriteElement.metaData.rect.height ;
						
							tElementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
							tElementTexture.SetPixels( tSpriteElement.texture.GetPixels( x, y, w, h ) ) ;
							tElementTexture.Apply() ;
					
							tSpriteTextureList.Add( tElementTexture ) ;
						}
					break ;

					// 領域更新
					case SpriteAction.Update :
						tSpriteSheetList.Add( tSpriteElement.metaData ) ;
				
						tPath = AssetDatabase.GetAssetPath( tSpriteElement.texture.GetInstanceID() ) ;

						// 読み込む前にフォーマットを強制的に ARGB32 NPOT にしてやる(アルファテクスチャなどだとオリジナルの状態が正しく読み込めない)
						SetTextureSetting( tPath, true, ref tSpriteElement.format, ref tSpriteElement.NPOTscale ) ;
					
						if( tSpriteElement.type == 0 )
						{
							// テクスチャまたはシングルスプライトタイプ
							tSpriteElement.texture = AssetDatabase.LoadAssetAtPath( tPath, typeof( Texture2D ) ) as Texture2D ;
							tSpriteTextureList.Add( tSpriteElement.texture ) ;
						}
						else
						{
							// マルチプルスプライトタイプ
							x = ( int )tSpriteElement.metaData.rect.x ;
							y = ( int )tSpriteElement.metaData.rect.y ;
							w = ( int )tSpriteElement.metaData.rect.width ;
							h = ( int )tSpriteElement.metaData.rect.height ;
						
							tElementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
							tElementTexture.SetPixels( tSpriteElement.texture.GetPixels( x, y, w, h ) ) ;
							tElementTexture.Apply() ;
						
							tSpriteTextureList.Add( tElementTexture ) ;
						}
					break ;

					case SpriteAction.Delete :
						tDeleteSpriteList.Add( tName ) ;
					break ;
				}

				tElementTexture = null ;
			}

			if( tSpriteSheetList.Count >  0 && tSpriteTextureList.Count >  0 )
			{
				int tMaxSize = Mathf.Min( SystemInfo.maxTextureSize, 2048 ) ;

				int tPadding = m_Padding ;
				if( tSpriteTextureList.Count == 1 )
				{
					// 要素が１つだけの場合はパディングは強制的に０にする
					tPadding = 0 ;
				}

				Rect[] tRectList = tTexture.PackTextures( tSpriteTextureList.ToArray(), tPadding, tMaxSize ) ;

				// 後始末
				foreach( KeyValuePair<string,SpriteElement> tSpriteElementKeyValue in m_SpriteElementHash )
				{
					SpriteElement tSpriteElement = tSpriteElementKeyValue.Value ;
				
					switch( tSpriteElement.action )
					{
						// 新規追加
						case SpriteAction.Add :
							tPath = AssetDatabase.GetAssetPath( tSpriteElement.texture.GetInstanceID() ) ;
							SetTextureSetting( tPath, false, ref tSpriteElement.format, ref tSpriteElement.NPOTscale ) ;
						
							tSpriteElement.texture = null ;
						
							tSpriteElement.action = SpriteAction.None ;
						break ;

						// 領域更新
						case SpriteAction.Update :
							tPath = AssetDatabase.GetAssetPath( tSpriteElement.texture.GetInstanceID() ) ;
							SetTextureSetting( tPath, false, ref tSpriteElement.format, ref tSpriteElement.NPOTscale ) ;
						
							tSpriteElement.texture = null ;
						
							tSpriteElement.action = SpriteAction.None ;
						break ;
					}
				}

				//---------------------------------------------------

				// 出来上がったテクスチャに無駄が無いか確認し無駄があれば除去する

				int i, l ;

				float tw = tTexture.width ;
				float th = tTexture.height ;

				l = tSpriteSheetList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					Rect tRect = tRectList[ i ] ;
					tRect.x      *= tw ;
					tRect.y      *= th ;
					tRect.width  *= tw ;
					tRect.height *= th ;
					tRectList[ i ] = tRect ;
				}

				// ソースのリージョンを検査して無駄が無いか確認する
				float xMin = tw ;
				float yMin = th ;
				float xMax = 0 ;
				float yMax = 0 ;
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( tRectList[ i ].xMin <  xMin )
					{
						xMin  = tRectList[ i ].xMin ;
					}
				
					if( tRectList[ i ].yMin <  yMin )
					{
						yMin  = tRectList[ i ].yMin ;
					}
				
					if( tRectList[ i ].xMax >  xMax )
					{
						xMax  = tRectList[ i ].xMax ;
					}
				
					if( tRectList[ i ].yMax >  yMax )
					{
						yMax  = tRectList[ i ].yMax ;
					}
				}

				xMin = xMin - tPadding ;
				if( xMin <  0 )
				{
					xMin  = 0 ;
				}
				yMin = yMin - tPadding ;
				if( yMin <  0 )
				{
					yMin  = 0 ;
				}
				xMax = xMax + tPadding ;
				if( xMax >  tw )
				{
					xMax  = tw ;
				}
				yMax = yMax + tPadding ;
				if( yMax >  th )
				{
					yMax  = th ;
				}
				int pw = ( int )( xMax - xMin ) ;
				int ph = ( int )( yMax - yMin ) ;

				int rw = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rw >= pw )
					{
						break ;
					}
					rw = rw << 1 ;
				}

				int rh = 1 ;
				for( i  =  0 ; i <  16 ; i ++ )
				{
					if( rh >= ph )
					{
						break ;
					}
					rh = rh << 1 ;
				}

				if( rw <= ( int )( tw / 2 ) || rh <= ( int )( th / 2 ) )
				{
					// 無駄がある

	//				Debug.LogWarning( "無駄がある:" + rw + " / " + tw + " " + rh + " / " + th ) ;

					int rx = ( int )xMin ;
					int ry = ( int )yMin ;

	//				Debug.LogWarning( "取得範囲:" + rx + " " + ry + " " + rw + " " + rh ) ;

					Texture2D tReduceTexture = new Texture2D( rw, rh, TextureFormat.ARGB32, false ) ;
					tReduceTexture.SetPixels( tTexture.GetPixels( rx, ry, rw, rh ) ) ;
					tReduceTexture.Apply() ;
				
					tTexture = tReduceTexture ;
					tReduceTexture = null ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						Rect tRect = tRectList[ i ] ;
						tRect.x      -= rx ;
						tRect.y      -= ry ;
						tRectList[ i ] = tRect ;
					}
				}
			
				//---------------------------------------------------

				string tAtlasFullPath ;
				if( tAtlas == null )
				{
					// 生成
					tAtlasFullPath = m_AtlasFullPath ;
				}
				else
				{
					// 更新
					tAtlasFullPath = AssetDatabase.GetAssetPath( tAtlas.GetInstanceID() ) ;
				}

				// テクスチャをＰＮＧ画像として保存する
				byte[] tData = tTexture.EncodeToPNG() ;
				System.IO.File.WriteAllBytes( tAtlasFullPath, tData ) ;
				tData = null ;
			
				AssetDatabase.SaveAssets() ;
				if( tAtlas == null )
				{
					AssetDatabase.Refresh() ;
				}
			
				TextureImporter tTextureImporter = AssetImporter.GetAtPath( tAtlasFullPath ) as TextureImporter ;

				// 既存の場合は以前の設定を引き継がせる
				tTextureImporter.textureCompression	= tAtlasFormat ;
				tTextureImporter.npotScale			= tAtlasNPOtScale ;

				if( tAtlas == null )
				{
					// 新規作成の場合に設定する
					tTextureImporter.textureType		= TextureImporterType.Sprite ;
					tTextureImporter.spriteImportMode	= SpriteImportMode.Multiple ;
					tTextureImporter.mipmapEnabled		= false ;
				}
			
				for( i  = 0 ; i <  l ; i ++ )
				{
					SpriteMetaData tSpriteMetaData = tSpriteSheetList[ i ] ;

					tSpriteMetaData.rect = tRectList[ i ] ;

					tSpriteSheetList[ i ] = tSpriteMetaData ;
				}
			
				tTextureImporter.spritesheet = tSpriteSheetList.ToArray() ;
			
				tTextureImporter.isReadable = false ;	// 書き込み属性を無効にする

				AssetDatabase.ImportAsset( tAtlasFullPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			
				tAtlas = AssetDatabase.LoadAssetAtPath( tAtlasFullPath, typeof( Texture2D ) ) as Texture2D ;

				tSpriteSheetList.Clear() ;
				tSpriteTextureList.Clear() ;

				Resources.UnloadUnusedAssets() ;
			}
			else
			{
				// 要素が全て無くなるためファイル自体を削除する
				AssetDatabase.DeleteAsset( m_AtlasFullPath ) ;
				tAtlas = null ;
			}

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;
		
			foreach( string tName in tDeleteSpriteList )
			{
				m_SpriteElementHash.Remove( tName ) ;
			}
			UpdateList( tAtlas, true ) ;
		}

		// 個々の要素をシングルタイプスプライトして書き出す
		private void OutputDividedSprite( Texture2D tAtlas, string tFolderPath )
		{
			if( string.IsNullOrEmpty( tFolderPath ) == true )
			{
				return ;
			}

			if( Directory.Exists( tFolderPath ) == false )
			{
				return ;
			}
			
			int l = tFolderPath.Length ;
			if( tFolderPath[ l - 1 ] != '/' )
			{
				tFolderPath = tFolderPath + '/' ;
			}

			//----------------------------------------------------------

			// アトラススプライトを読み出し可能状態にする
			string tAtlasPath = AssetDatabase.GetAssetPath( tAtlas.GetInstanceID() ) ;

			// 元の状態を保存する
			TextureImporterCompression	tAtlasFormat	= TextureImporterCompression.Uncompressed ;
			TextureImporterNPOTScale	tAtlasNPOtScale	= TextureImporterNPOTScale.None ;

			SetTextureSetting( tAtlasPath, true, ref tAtlasFormat, ref tAtlasNPOtScale ) ;	// 読み込み属性を有効にする
		
			tAtlas = AssetDatabase.LoadAssetAtPath( tAtlasPath, typeof( Texture2D ) ) as Texture2D ;

			TextureImporter tAtlasTextureImporter = AssetImporter.GetAtPath( tAtlasPath ) as TextureImporter ;
			int x, y, w, h ;

			Texture2D tElementTexture ;
			string tElementPath ;
			TextureImporter tElementTextureImporter ;
		
			foreach( SpriteMetaData tSpriteMetaData in tAtlasTextureImporter.spritesheet )
			{
				x = ( int )tSpriteMetaData.rect.x ;
				y = ( int )tSpriteMetaData.rect.y ;
				w = ( int )tSpriteMetaData.rect.width ;
				h = ( int )tSpriteMetaData.rect.height ;
			
				tElementTexture = new Texture2D( w, h, TextureFormat.ARGB32, false ) ;
				tElementTexture.SetPixels( tAtlas.GetPixels( x, y, w, h ) ) ;
				tElementTexture.Apply() ;

				tElementPath = tFolderPath + tSpriteMetaData.name + ".png" ;

				// テクスチャをＰＮＧ画像として保存する
				byte[] tData = tElementTexture.EncodeToPNG() ;
				System.IO.File.WriteAllBytes( tElementPath, tData ) ;
				tData = null ;

				AssetDatabase.SaveAssets() ;
				AssetDatabase.Refresh() ;

				// 新規生成

				tElementTextureImporter = AssetImporter.GetAtPath( tElementPath ) as TextureImporter ;
			
				tElementTextureImporter.npotScale			= TextureImporterNPOTScale.None ;

				tElementTextureImporter.textureType			= TextureImporterType.Sprite ;
				tElementTextureImporter.spriteImportMode	= SpriteImportMode.Single ;

				tElementTextureImporter.spriteBorder		= tSpriteMetaData.border ;
				tElementTextureImporter.spritePivot			= tSpriteMetaData.pivot ;

				tElementTextureImporter.alphaIsTransparency	= tAtlasTextureImporter.alphaIsTransparency ;
				tElementTextureImporter.mipmapEnabled		= tAtlasTextureImporter.mipmapEnabled ;
				tElementTextureImporter.wrapMode			= tAtlasTextureImporter.wrapMode ;
				tElementTextureImporter.filterMode			= tAtlasTextureImporter.filterMode ;
				tElementTextureImporter.textureCompression	= tAtlasTextureImporter.textureCompression ;

				tElementTextureImporter.isReadable			= false ;	// 読み込み禁止

				AssetDatabase.ImportAsset( tElementPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;

				tElementTexture = null ;
			}

			SetTextureSetting( tAtlasPath, false, ref tAtlasFormat, ref tAtlasNPOtScale ) ;	// 読み込み属性を無効にする

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;
		}

		//------------------------------------------------------------

		// テクスチャの属性を設定する（読み出し・書き込み）
		private void SetTextureSetting( string tPath, bool tReadable, ref TextureImporterCompression rFormat, ref TextureImporterNPOTScale rNPOTScale )
		{
			TextureImporter tTextureImporter = AssetImporter.GetAtPath( tPath ) as TextureImporter ;
		
			if( tReadable == true )
			{
				// 読み込み許可
				rFormat		= tTextureImporter.textureCompression ;
				rNPOTScale	= tTextureImporter.npotScale ;

				// 強制的に ARGB32 NPOT に変える必要がある
				tTextureImporter.textureCompression	= TextureImporterCompression.Uncompressed ;
				tTextureImporter.npotScale			= TextureImporterNPOTScale.None ;
			}
			else
			{
				// 読み込み禁止
				tTextureImporter.textureCompression	= rFormat ;
				tTextureImporter.npotScale			= rNPOTScale ;
			}

			tTextureImporter.isReadable = tReadable ;
		
			AssetDatabase.ImportAsset( tPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
		}

		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "パック対象にしたいテクスチャを選択してください(複数可)" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "Please select one or more textures in the Project View window." },
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

