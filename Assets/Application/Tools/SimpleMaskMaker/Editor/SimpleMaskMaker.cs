using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections.Generic ;

/// <summary>
/// アウトラインメーカーパッケージ
/// </summary>
namespace OutlineMaker
{
	/// <summary>
	/// スプライトパッカークラス(エディター用) Version 2018/02/13 0
	/// </summary>
	public class SimpleMaskMaker : EditorWindow
	{
		[ MenuItem( "Tools/Simple Mask Maker" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<SimpleMaskMaker>( false, "Mask Maker", true ) ;
		}

		//-------------------------------------------------------------------------------------------

		private string	m_OutputPath = "Assets/" ;

		private bool	m_DoNotOverwritre = true ;

		public enum Mode
		{
			Mask		= 0,
			Outline		= 1,
		}

		private Mode	m_Mode = Mode.Mask ;

		private	bool	m_InnerRemoval = false ;

		private Color	m_OutlineColor = Color.black ;

		private int		m_OutlineColorWidth = 0 ;

		private bool	m_OutlineSkew = false ;

		private bool	m_MaintainSize = false ;

		private int		m_OutlineAlphaWidth = 0 ;


		private Vector2 m_ScrollPosition = Vector2.zero ;
	
		//----------------------------------------------------------

		// レイアウトを描画する
		private void OnGUI()
		{
			// 保存先のパスの設定
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
						string tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
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

			// 上書するかしないか
			m_DoNotOverwritre = EditorGUILayout.Toggle( "Do Not Overwrite", m_DoNotOverwritre ) ;

			GUILayout.Space( 6f ) ;

			//----------------------------------------------------------

			m_Mode = ( Mode )EditorGUILayout.EnumPopup( "Mode", m_Mode ) ;
			if( m_Mode == Mode.Mask )
			{
				m_InnerRemoval = EditorGUILayout.Toggle( "Inner Removal", m_InnerRemoval ) ;
			}
			else
			{
				m_OutlineColor = EditorGUILayout.ColorField( "Outline Color", m_OutlineColor ) ;
			}

			// 以下共通設定
			m_OutlineColorWidth = EditorGUILayout.IntSlider( "Outline Color Width", m_OutlineColorWidth,   0, 256 ) ;
			if( m_OutlineColorWidth >  0 )
			{
				m_OutlineSkew = EditorGUILayout.Toggle( "Outline Skew", m_OutlineSkew ) ;

				m_MaintainSize = EditorGUILayout.Toggle( "Maintain Size", m_MaintainSize ) ;

				m_OutlineAlphaWidth = EditorGUILayout.IntSlider( "Outline Alpha Width", m_OutlineAlphaWidth,   0, m_OutlineColorWidth ) ;
			}

			//----------------------------------------------------------
			
			bool tExecute = false ;

			string[] tTargetList = GetTargetList() ;

			// 素材情報のリストを表示する
			if( tTargetList != null && tTargetList.Length >  0 )
			{
				EditorGUILayout.Separator() ;
			
				GUI.backgroundColor = Color.green ;
				tExecute = GUILayout.Button( "Create" ) ;
				GUI.backgroundColor = Color.white ;

				EditorGUILayout.Separator() ;

				EditorGUILayout.LabelField( "Target (" + tTargetList.Length + ")" ) ;

				GUILayout.BeginVertical() ;
				{
					m_ScrollPosition = GUILayout.BeginScrollView( m_ScrollPosition ) ;
					{
						int i, l = tTargetList.Length ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							GUILayout.BeginHorizontal( "TextArea", GUILayout.MinHeight( 20f ) ) ;	// 横一列開始
							{
								GUILayout.Label( tTargetList[ i ], GUILayout.Height( 20f ) ) ;
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
				MakeMaskAll( tTargetList ) ;
			}
		}

		private void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//-------------------------------------------------------------------------------------------

		private string[] GetTargetList()
		{
			List<string> tTargetList = new List<string>() ;

			// 選択中の素材を追加する
			foreach( UnityEngine.Object tObject in Selection.objects )
			{
				Texture2D tTexture = tObject as Texture2D ;
			
				if( tTexture == null )
				{
					continue ;
				}

				//-----------------------------------------

				// 素材となる画像がスプライト（アトラス）かそれ以外（テクスチャ）かで処理が異なる
				string tTexturePath = AssetDatabase.GetAssetPath( tTexture.GetInstanceID() ) ;
				TextureImporter tTextureImporter = AssetImporter.GetAtPath( tTexturePath ) as TextureImporter ;
				if( tTextureImporter != null && tTextureImporter.textureType == TextureImporterType.Sprite )
				{
					// スプライト扱い
					tTargetList.Add( tTexturePath ) ;
				}
				else
				{
					// テクスチャ扱い
					tTargetList.Add( tTexturePath ) ;
				}
			}

			if( tTargetList.Count == 0 )
			{
				return null ;
			}

			return tTargetList.ToArray() ;
		}

		//-------------------------------------------------------------------------------------------

		private void MakeMaskAll( string[] tTexturePath )
		{
			int i, l = tTexturePath.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				MakeMask( tTexturePath[ i ] ) ;
			}
		}

		private void MakeMask( string tInputPath )
		{
			TextureImporter tInputTextureImporter = AssetImporter.GetAtPath( tInputPath ) as TextureImporter ;

			bool tIsReadable = tInputTextureImporter.isReadable ;

			tInputTextureImporter.isReadable = true ;	// 書き込み属性を有効にする

			AssetDatabase.ImportAsset( tInputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			Texture2D tInputTexture = AssetDatabase.LoadAssetAtPath( tInputPath, typeof( Texture2D ) ) as Texture2D ;
			
			//----------------------------------------------------------

			int x, y, o ;

			int tSW = tInputTexture.width ;	// 横幅
			int tSH = tInputTexture.height ;	// 縦幅

			Color32[] tInputPixels = tInputTexture.GetPixels32() ;

			int tDX = 0 ;
			int tDY = 0 ;

			int tDW = 0 ;
			int tDH = 0 ;

			// アウトライン用に画像を大きくするか
			if( m_MaintainSize == false )
			{
				// 場合によっては拡張する

				int tXMin, tXMax ;
				int tYMin, tYMax ;

				tXMin = tSW ;
				tXMax =   0 ;
				tYMin = tSH ;
				tYMax =   0 ;

				for( y  = 0 ; y <  tSH ; y ++ )
				{
					for( x  = 0 ; x <  tSW ; x ++ )
					{
						o = y * tSW + x ;

						if( tInputPixels[ o ].a >  0 )
						{
							if( x <  tXMin )
							{
								tXMin  = x ;
							}
							if( x >  tXMax )
							{
								tXMax  = x ;
							}
							if( y <  tYMin )
							{
								tYMin  = y ;
							}
							if( y >  tYMax )
							{
								tYMax  = y ;
							}
						}
					}
				}

				if( tXMin <  m_OutlineColorWidth || ( tSW - 1 - tXMax ) <  m_OutlineColorWidth || tYMin <  m_OutlineColorWidth || ( tSH - 1 - tYMax ) <  m_OutlineColorWidth )
				{
					// スペースが足りない箇所が存在する

					tDX = m_OutlineColorWidth ;
					tDY = m_OutlineColorWidth ;

					tDW = tSW + m_OutlineColorWidth * 2 ;
					tDH = tSH + m_OutlineColorWidth * 2 ;
				}
				else
				{
					tDX =   0 ;
					tDY =   0 ;

					tDW = tSW ;
					tDH = tSH ;
				}
			}
			else
			{
				// 維持する
				tDX =   0 ;
				tDY =   0 ;

				tDW = tSW ;
				tDH = tSH ;
			}
			
			Texture2D tOutputTexture = new Texture2D( tDW, tDH, TextureFormat.ARGB32, false ) ;

			Color32[]	tOutputPixels	= new Color32[ tDW * tDH ] ;
			int[]		tCode			= new int[ tDW * tDH ] ;

			for( y  = 0 ; y <  tDH ; y ++ )
			{
				for( x  = 0 ; x <  tDW ; x ++ )
				{
					o = y * tDW + x ;
					tOutputPixels[ o ] = new Color32( 0, 0, 0, 0 ) ;
					tCode[ o ] = -1 ;
				}
			}
			
			//----------------------------------------------------------

			int tSO, tDO ;

			for( y  = 0 ; y <  tSH ; y ++ )
			{
				for( x  = 0 ; x <  tSW ; x ++ )
				{
					tSO = y * tSW + x ;
					tDO = ( tDY + y ) * tDW + ( tDX + x ) ;

					if( tInputPixels[ tSO ].a >  0 )
					{
						// 色が存在する箇所
						tCode[ tDO ] = 0 ;
					}
				}
			}

			if( m_OutlineColorWidth >  0 )
			{
				// アウトラインの箇所にチェックを入れる

				int tLevel ;

				for( tLevel  = 0 ; tLevel <  m_OutlineColorWidth ; tLevel ++ )
				{
					for( y  = 1 ; y <  ( tDH - 1 ) ; y ++ )
					{
						for( x  = 1 ; x <  ( tDW - 1 ) ; x ++ )
						{
							o = y * tDW + x ;

							if( tCode[ o ] <  0 )
							{
								if
								(
									Check( tCode, x,     y - 1, tLevel, tDW ) == true ||
									Check( tCode, x,     y + 1, tLevel, tDW ) == true ||
									Check( tCode, x - 1, y,     tLevel, tDW ) == true ||
									Check( tCode, x + 1, y,     tLevel, tDW ) == true
								)
								{
									// 上下左右のいずれかにドットがあれば有効
									tCode[ o ] = 1 + tLevel ;
								}
	
								if( m_OutlineSkew == true )
								{
									// 斜めも有効
									if
									(
										Check( tCode, x - 1, y - 1, tLevel, tDW ) == true ||
										Check( tCode, x + 1, y - 1, tLevel, tDW ) == true ||
										Check( tCode, x - 1, y + 1, tLevel, tDW ) == true ||
										Check( tCode, x + 1, y + 1, tLevel, tDW ) == true
									)
									{
										tCode[ o ] = 1 + tLevel ;
									}
								}
							}
						}
					}
				}
			}

			//----------------------------------------------------------

			// 中

			// 絵の部分(モードによって処理を変える)
			if( m_Mode == Mode.Mask )
			{
				// 白
				if( m_InnerRemoval == false )
				{
					for( y  = 0 ; y <  tSH ; y ++ )
					{
						for( x  = 0 ; x <  tSW ; x ++ )
						{
							tSO = y * tSW + x ;
							tDO = ( tDY + y ) * tDW + ( tDX + x ) ;
		
							if( tInputPixels[ tSO ].a >  0 )
							{
								// 色が存在する箇所
								tOutputPixels[ tDO ] = Color.white ;
							}
						}
					}
				}
			}
			else
			{
				// 絵
				for( y  = 0 ; y <  tSH ; y ++ )
				{
					for( x  = 0 ; x <  tSW ; x ++ )
					{
						tSO = y * tSW + x ;
						tDO = ( tDY + y ) * tDW + ( tDX + x ) ;
	
						if( tInputPixels[ tSO ].a >  0 )
						{
							// 色が存在する箇所
							tOutputPixels[ tDO ] = tInputPixels[ tSO ] ;
						}
					}
				}
			}

			//----------------------------------------------------------

			// 外

			if( m_OutlineColorWidth >  0 )
			{
				// コード値が１以上の箇所
				Color32 tColor ;

				if( m_Mode == Mode.Mask )
				{
					// 色
					tColor = Color.white ;
				}
				else
				{
					tColor = m_OutlineColor ;
				}

				int tOutlineAlphaWidth = m_OutlineAlphaWidth ;

				if( tOutlineAlphaWidth >  m_OutlineColorWidth )
				{
					tOutlineAlphaWidth  = m_OutlineColorWidth ;
				}

				Color32[] tColorTable = new Color32[ m_OutlineColorWidth ] ;

				int i, p ;
				for( i  = 0 ; i <  m_OutlineColorWidth ; i ++ )
				{
					tColorTable[ i ] = tColor ;
					if( ( tOutlineAlphaWidth + i ) <  m_OutlineColorWidth )
					{
//						tColorTable[ i ].a = ( byte )( ( tColorTable[ i ].a * 255 ) / 255 ) ;
					}
					else
					{
						// 半透明になっていく(0～tOutlineAlphaWidth-1)
						p = ( tOutlineAlphaWidth + i ) - m_OutlineColorWidth ;
						tColorTable[ i ].a = ( byte )( ( tColorTable[ i ].a * ( tOutlineAlphaWidth - p ) * ( 255 / ( tOutlineAlphaWidth + 1 ) ) ) / 255 ) ;
					}
				}

				for( y  = 0 ; y <  tDH ; y ++ )
				{
					for( x  = 0 ; x <  tDW ; x ++ )
					{
						o = y * tDW + x ;

						p = tCode[ o ] ;
						if( p >= 1 )
						{
							tOutputPixels[ o ] = tColorTable[ p - 1 ] ;
						}
					}
				}
			}

			//----------------------------------------------------------

			tOutputTexture.SetPixels32( tOutputPixels ) ;

			//----------------------------------------------------------
			
			string tOutputPath = "" ;
			if( string.IsNullOrEmpty( m_OutputPath ) == true )
			{
				// 上書
				tOutputPath = tInputPath ;
			}
			else
			{
				// 新規
				tOutputPath = m_OutputPath ;
				if( tOutputPath[ tOutputPath.Length - 1 ] != '/' )
				{
					tOutputPath = tOutputPath + "/" ;
				}

				string tFileName = "" ;

				int i = tInputPath.LastIndexOf( "/" ) ;
				if( i >= 0 )
				{
					i ++ ;
					tFileName = tInputPath.Substring( i, tInputPath.Length - i ) ;
				}
				else
				{
					tFileName = tInputPath ;
				}

				tOutputPath = tOutputPath + tFileName ;
			}

			if( tOutputPath == tInputPath )
			{
				// 出力パスが同一の場合の処理
				if( m_DoNotOverwritre == true )
				{
					// 強制的に別のパスにする
					int i = tOutputPath.LastIndexOf( "." ) ;
					if( i >= 0 )
					{
						tOutputPath = tOutputPath.Substring( 0, i ) ;
					}

					if( m_Mode == Mode.Mask )
					{
						tOutputPath = tOutputPath + "_Mask.png" ;
					}
					else
					{
						tOutputPath = tOutputPath + "_Outline.png" ;
					}
				}
			}


			// テクスチャをＰＮＧ画像として保存する
			byte[] tData = tOutputTexture.EncodeToPNG() ;
			System.IO.File.WriteAllBytes( tOutputPath, tData ) ;
			tData = null ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			if( tOutputPath != tInputPath )
			{
				// 新規のケース(保存後に元の読み込み属性を禁止にする

				TextureImporter tOutputTextureImporter = AssetImporter.GetAtPath( tOutputPath ) as TextureImporter ;

				tOutputTextureImporter.npotScale			= TextureImporterNPOTScale.None ;

				tOutputTextureImporter.textureType			= TextureImporterType.Sprite ;
				tOutputTextureImporter.spriteImportMode		= SpriteImportMode.Single ;

				tOutputTextureImporter.spriteBorder			= tInputTextureImporter.spriteBorder ;
				tOutputTextureImporter.spritePivot			= tInputTextureImporter.spritePivot ;

				tOutputTextureImporter.alphaIsTransparency	= tInputTextureImporter.alphaIsTransparency ;
				tOutputTextureImporter.mipmapEnabled		= tInputTextureImporter.mipmapEnabled ;
				tOutputTextureImporter.wrapMode				= tInputTextureImporter.wrapMode ;
				tOutputTextureImporter.filterMode			= tInputTextureImporter.filterMode ;
				tOutputTextureImporter.textureCompression	= tInputTextureImporter.textureCompression ;

				tOutputTextureImporter.isReadable			= false ;	// 読み込み禁止

				AssetDatabase.ImportAsset( tOutputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			if( tIsReadable != tInputTextureImporter.isReadable )
			{
				tInputTextureImporter.isReadable = tIsReadable ;	// 書き込み属性を無効にする

				AssetDatabase.ImportAsset( tInputPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			tOutputTexture	= null ;
			tInputTexture	= null ;

			AssetDatabase.SaveAssets() ;
			AssetDatabase.Refresh() ;

			Resources.UnloadUnusedAssets() ;
		}



		private bool Check( int[] tCode, int x, int y, int c, int tDW )
		{
			int o = y * tDW + x ;
			return tCode[ o ] == c ;
		}











		//--------------------------------------------------------------------------

		private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "アウトラインを追加したいテクスチャを選択してください(複数可)" },
		} ;
		private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
		{
			{ "SelectTexture",   "Please select one or more textures you want to add outline." },
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

