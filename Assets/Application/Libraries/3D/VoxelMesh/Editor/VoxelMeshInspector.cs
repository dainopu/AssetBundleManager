using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

[ CustomEditor( typeof( VoxelMesh ) ) ]
public class VoxelMeshInspector : Editor
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
		VoxelMesh tTarget = target as VoxelMesh ;
		
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		// オフセット
		Vector3 tOffset = EditorGUILayout.Vector3Field( "Offset", tTarget.offset ) ;
		if( tOffset != tTarget.offset )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Offset Change" ) ;	// アンドウバッファに登録
			tTarget.offset = tOffset ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// ボクセルスケール
//		float tVoxelScale = EditorGUILayout.FloatField( "Voxel Scale", tTarget.voxelScale ) ;

		float tVoxelScale = EditorGUILayout.Slider( "Voxel Scale", tTarget.voxelScale, 0.1f, 2.0f ) ;
		if( tVoxelScale != tTarget.voxelScale && tVoxelScale >  0 )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Voxel Scale Change" ) ;	// アンドウバッファに登録
			tTarget.voxelScale = tVoxelScale ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// マテリアル
		Material tMaterial = EditorGUILayout.ObjectField( "Maretial", tTarget.material, typeof( Material ), false ) as Material ;
		if( tMaterial != tTarget.material )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Material Change" ) ;	// アンドウバッファに登録
			tTarget.material = tMaterial ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// テクスチャ
		Texture2D tTexture = EditorGUILayout.ObjectField( "Texture", tTarget.texture, typeof( Texture2D ), false ) as Texture2D ;
		if( tTexture != tTarget.texture )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Texture Change" ) ;	// アンドウバッファに登録
			tTarget.texture = tTexture ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// ボクセルテクスチャ
		Texture2D tVoxelTexture = EditorGUILayout.ObjectField( "Voxel Texture", tTarget.voxelTexture, typeof( Texture2D ), false ) as Texture2D ;

		bool tVoxelTextureReadable = tTarget.voxelTextureReadable ;
		if( tVoxelTexture != null )
		{
			// Read/Write 属性が有効になっているか確認する
//			EditorUtility.DisplayDialog( "Voxel Texture", GetMessage( "VoxelTexture_BadProperty" ), "Close" ) ;

			// オブジェクトのファイルパスを取得する
			string tPath = AssetDatabase.GetAssetPath( tVoxelTexture.GetInstanceID() ) ;
			
			// インポーターを取得る
			TextureImporter tTextureImporter = AssetImporter.GetAtPath( tPath ) as TextureImporter ;

			// インポーターから meta の設定情報を読み取る
			TextureImporterSettings tSettings = new TextureImporterSettings() ;
			tTextureImporter.ReadTextureSettings( tSettings ) ;

			if( tSettings.readable == false )
			{
				// 読み取り属性が有効になっていない
				EditorGUILayout.HelpBox( GetMessage( "VoxelTexture_BadProperty" ), MessageType.Warning, true ) ;
				
				tVoxelTextureReadable = false ;
			}
			else
			{
				// 読み取り属性が有効になっている
				tVoxelTextureReadable = true ;
			}
		}
		else
		{
			tVoxelTextureReadable = false ;	
		}

		if( tVoxelTextureReadable != tTarget.voxelTextureReadable )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Voxel Texture Readable Change" ) ;	// アンドウバッファに登録
			tTarget.voxelTextureReadable = tVoxelTextureReadable ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tVoxelTexture != tTarget.voxelTexture )
		{
			Undo.RecordObject( tTarget, "VoxelMesh : Voxel Texture Change" ) ;	// アンドウバッファに登録
			tTarget.voxelTexture = tVoxelTexture ;
			EditorUtility.SetDirty( tTarget ) ;
		}


	}

	//--------------------------------------------------------------------------

	private Dictionary<string,string> mJapanese_Message = new Dictionary<string, string>()
	{
		{ "VoxelTexture_BadProperty",   "テクスチャの [Read/Write] フラグを有効にしてください" },
	} ;
	private Dictionary<string,string> mEnglish_Message = new Dictionary<string, string>()
	{
		{ "VoxelTexture_BadProperty",   "Please enable [Read/Write] in texture properties" },
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
