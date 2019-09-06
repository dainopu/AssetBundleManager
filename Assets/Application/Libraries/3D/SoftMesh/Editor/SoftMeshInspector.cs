using UnityEngine ;
using UnityEditor ;
using System.Collections ;

[ CustomEditor( typeof( SoftMesh ) ) ]
public class SoftMeshInspector : Editor
{
	private bool	m_UVA_Show = false ;
	private bool[]	m_UVI_Show = null ;

	/// <summary>
	/// スンスペクター描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		// とりあえずデフォルト
		DrawDefaultInspector() ;
		
		//--------------------------------------------
		
		// ターゲットのインスタンス
		SoftMesh tTarget = target as SoftMesh ;
		
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		// タイプ
		SoftMesh.ShapeType tShapeType = ( SoftMesh.ShapeType )EditorGUILayout.EnumPopup( "Shape Type",  tTarget.shapeType ) ;
		if( tShapeType != tTarget.shapeType )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Shape Type Change" ) ;	// アンドウバッファに登録
			tTarget.shapeType = tShapeType ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tTarget.shapeType == SoftMesh.ShapeType.Capsule || tTarget.shapeType == SoftMesh.ShapeType.Cylinder )
		{
			// カプセル・シリンダーの場合の方向
			SoftMesh.Direction tDirection = ( SoftMesh.Direction )EditorGUILayout.EnumPopup( "Direction",  tTarget.direction ) ;
			if( tDirection != tTarget.direction )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.direction = tDirection ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
		else
		if( tTarget.shapeType == SoftMesh.ShapeType.Plane )
		{
			// プレーンの場合の方向
			SoftMesh.PlaneDirection tPlaneDirection = ( SoftMesh.PlaneDirection )EditorGUILayout.EnumPopup( "Plane Direction",  tTarget.planeDirection ) ;
			if( tPlaneDirection != tTarget.planeDirection )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Plane Direction Change" ) ;	// アンドウバッファに登録
				tTarget.planeDirection = tPlaneDirection ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		if( tTarget.shapeType == SoftMesh.ShapeType.Cube || tTarget.shapeType == SoftMesh.ShapeType.Sphere || tTarget.shapeType == SoftMesh.ShapeType.Capsule || tTarget.shapeType == SoftMesh.ShapeType.Cylinder || tTarget.shapeType == SoftMesh.ShapeType.Plane )
		{
			// ３Ｄ系

			// オフセット
			Vector3 tOffset = EditorGUILayout.Vector3Field( "Offset", tTarget.offset ) ;
			if( tOffset != tTarget.offset )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				tTarget.offset = tOffset ;
				EditorUtility.SetDirty( tTarget ) ;
			}
	
			// サイズ
			Vector3 tSize = EditorGUILayout.Vector3Field( "Size", tTarget.size ) ;
			if( tSize != tTarget.size )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Size Change" ) ;	// アンドウバッファに登録
				tTarget.size = tSize ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
		else
		{
			// ２Ｄ系

			// オフセット
			Vector2 tOffset2D_Old = new Vector2( tTarget.offset.x, tTarget.offset.y ) ;
			Vector2 tOffset2D_New = EditorGUILayout.Vector2Field( "Offset", tOffset2D_Old) ;
			if( tOffset2D_New != tOffset2D_Old )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				tTarget.offset = new Vector3( tOffset2D_New.x, tOffset2D_New.y, tTarget.offset.z ) ;
				EditorUtility.SetDirty( tTarget ) ;
			}
	
			// サイズ
			Vector2 tSize2D_Old = new Vector2( tTarget.size.x, tTarget.size.y ) ;
			Vector2 tSize2D_New = EditorGUILayout.Vector2Field( "Size", tSize2D_Old ) ;
			if( tSize2D_New != tSize2D_Old )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Size Change" ) ;	// アンドウバッファに登録
				tTarget.size = new Vector2( tSize2D_New.x, tSize2D_New.y ) ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		// カラー
		Color tVertexColor = Color.white ;
		tVertexColor.r = tTarget.vertexColor.r ;
		tVertexColor.g = tTarget.vertexColor.g ;
		tVertexColor.b = tTarget.vertexColor.b ;
		tVertexColor.a = tTarget.vertexColor.a ;
		tVertexColor = EditorGUILayout.ColorField( "Vertex Color", tVertexColor ) ;
//		Debug.LogWarning( "O:" + tTarget.vertexColor + " N:" + tVertexColor ) ;
		if
		(
			tVertexColor.r != tTarget.vertexColor.r ||
			tVertexColor.g != tTarget.vertexColor.g ||
			tVertexColor.b != tTarget.vertexColor.b ||
			tVertexColor.a != tTarget.vertexColor.a
		)
		{
//			Debug.LogWarning( "変化した" ) ;
			Undo.RecordObject( tTarget, "SoftMesh : Vertex Color Change" ) ;	// アンドウバッファに登録
			tTarget.vertexColor = tVertexColor ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// ＵＶ
		if( tTarget.shapeType == SoftMesh.ShapeType.Cube || tTarget.shapeType == SoftMesh.ShapeType.Plane || tTarget.shapeType == SoftMesh.ShapeType.Box2D )
		{
			Rect[] tUVA = tTarget.uv ;
			if( tUVA != null && tUVA.Length >  0 )
			{
				m_UVA_Show = EditorGUILayout.Foldout( m_UVA_Show, "UV" ) ;
				if( m_UVA_Show == true )
				{
					Rect tUV_Old ;
					Rect tUV_New ;

					int i, l = tUVA.Length ;
					if( m_UVI_Show == null )
					{
						m_UVI_Show = new bool[ l ] ;
					}

					if( tTarget.shapeType == SoftMesh.ShapeType.Plane || tTarget.shapeType == SoftMesh.ShapeType.Box2D )
					{
						if( l >  1 )
						{
							l  = 1 ;
						}
					}

					for( i  = 0 ; i <  l ; i ++ )
					{
						tUV_Old = new Rect( tUVA[ i ].x, tUVA[ i ].y, tUVA[ i ].width, tUVA[ i ].height ) ;
						tUV_New = new Rect( tUVA[ i ].x, tUVA[ i ].y, tUVA[ i ].width, tUVA[ i ].height ) ;

						m_UVI_Show[ i ] = EditorGUILayout.Foldout( m_UVI_Show[ i ], " Index " + i ) ;
						if( m_UVI_Show[ i ] == true )
						{
							tUV_New = EditorGUILayout.RectField( tUV_New ) ;
							if( tUV_Old.Equals( tUV_New ) == false )
							{
								Undo.RecordObject( tTarget, "SoftMesh : UV Change" ) ;	// アンドウバッファに登録
								tTarget.SetUV( i, tUV_New ) ;
								EditorUtility.SetDirty( tTarget ) ;
							}
						}
					}
				}
			}
		}


		// スプリット
		int tSplit = EditorGUILayout.IntSlider( "Split", tTarget.split, 0, 5 ) ;
		if( tSplit != tTarget.split )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Split Change" ) ;	// アンドウバッファに登録
			tTarget.split = tSplit ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tTarget.shapeType == SoftMesh.ShapeType.Plane || tTarget.shapeType == SoftMesh.ShapeType.Box2D )
		{
			// タイリング
			bool tTiling = EditorGUILayout.Toggle( "Tiling", tTarget.tiling ) ;
			if( tTiling != tTarget.tiling )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Tiling Change" ) ;	// アンドウバッファに登録
				tTarget.tiling = tTiling ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		//-----------------------------------------------------------

		EditorGUILayout.Separator() ;   // 少し区切りスペース

		// マテリアル
		Material tMaterial = EditorGUILayout.ObjectField( "Maretial", tTarget.material, typeof( Material ), false ) as Material ;
		if( tMaterial != tTarget.material )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Material Change" ) ;	// アンドウバッファに登録
			tTarget.material = tMaterial ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// テクスチャ
		Texture2D tTexture = EditorGUILayout.ObjectField( "Texture", tTarget.texture, typeof( Texture2D ), false ) as Texture2D ;
		if( tTexture != tTarget.texture )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Texture Change" ) ;	// アンドウバッファに登録
			tTarget.texture = tTexture ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		// コライダー
		bool tIsCollider = EditorGUILayout.Toggle( "Collider", tTarget.isCollider ) ;
		if( tIsCollider != tTarget.isCollider )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Collider Change" ) ;	// アンドウバッファに登録
			tTarget.isCollider = tIsCollider ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tTarget.isCollider == true )
		{
			// コライダーの自動調整
			bool tColliderAdjustment = EditorGUILayout.Toggle( "Collider Adjustment", tTarget.colliderAdjustment ) ;
			if( tColliderAdjustment != tTarget.colliderAdjustment )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Collider Adjustment Change" ) ;	// アンドウバッファに登録
				tTarget.colliderAdjustment = tColliderAdjustment ;
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		// リジッドボディ
		bool tIsRigidbody = EditorGUILayout.Toggle( "Rigidbody", tTarget.isRigidbody ) ;
		if( tIsRigidbody != tTarget.isRigidbody )
		{
			Undo.RecordObject( tTarget, "SoftMesh : Rigidbody Change" ) ;	// アンドウバッファに登録
			tTarget.isRigidbody = tIsRigidbody ;
			EditorUtility.SetDirty( tTarget ) ;
		}
/*
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		GUI.backgroundColor = Color.cyan ;
		if( GUILayout.Button( "Refresh", GUILayout.Width( 140f ) ) == true )
		{
			tTarget.Refresh() ;
		}
		GUI.backgroundColor = Color.white ;
*/
	}
}
