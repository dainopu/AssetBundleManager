using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

[ CustomEditor( typeof( ComplexSoftMesh ) ) ]
public class ComplexSoftMeshInspector : Editor
{
	private bool		m_ShapeData_A_Show = false ;
	private bool[]		m_ShapeData_I_Show = null ;

	private bool[]		m_ShapeData_UVA_Show = null ;
	private bool[][]	m_ShapeData_UVI_Show = null ;

	private string m_AddShapeDataName = "" ;
	private int    m_RemoveShapeDataIndex = 0 ;
	private int    m_RemoveShapeDataIndexAnswer = -1 ;



	/// <summary>
	/// スンスペクター描画
	/// </summary>
	public override void OnInspectorGUI()
	{
		// とりあえずデフォルト
//		DrawDefaultInspector() ;
		
		//--------------------------------------------
		
		// ターゲットのインスタンス
		ComplexSoftMesh tTarget = target as ComplexSoftMesh ;
		
		EditorGUILayout.Separator() ;   // 少し区切りスペース

		int i, l ;

		//--------------------------------------------

		List<ComplexSoftMesh.ShapeData> tShapeDatas = tTarget.shapeData ;

		//----------------------------------------------------

		if( m_RemoveShapeDataIndexAnswer <  0 )
		{
			GUILayout.BeginHorizontal() ;	// 横並び開始
			{
				bool tAdd = false ;

				GUI.backgroundColor = Color.cyan ;
				if( GUILayout.Button( "Add Shape", GUILayout.Width( 140f ) ) == true )
				{
					tAdd = true ;
				}
				GUI.backgroundColor = Color.white ;

				GUI.backgroundColor = Color.cyan ;
				m_AddShapeDataName = EditorGUILayout.TextField( "", m_AddShapeDataName, GUILayout.Width( 120f ) ) ;
				GUI.backgroundColor = Color.white ;

				if( tAdd == true )
				{
					if( string.IsNullOrEmpty( m_AddShapeDataName ) == false )
					{
						// ShapeData を追加する
						tTarget.AddShapeData( m_AddShapeDataName ) ;
						tTarget.Refresh() ;
						if( tTarget.colliderAdjustment == true )
						{
							tTarget.AdjustCollider() ;
						}
					}
					else
					{
						EditorUtility.DisplayDialog( "Add ShapeData", GetMessage( "InputIdentity" ), "Close" ) ;
					}
				}
			}
			GUILayout.EndHorizontal() ;		// 横並び終了

			if( tShapeDatas != null && tShapeDatas.Count >  0 )
			{
				// １つ以上存在していればリストとして描画する
				string[] tShapeDataNameArray = GetShapeDataNames( tShapeDatas ) ;

				//---------------------------------------------------------

				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					bool tRemove = false ;
					GUI.backgroundColor = Color.red ;	// ボタンの下地を緑に
					if( GUILayout.Button( "Remove ShapeData", GUILayout.Width( 140f ) ) == true )
					{
						tRemove = true ;
					}
					GUI.backgroundColor = Color.white ;	// ボタンの下地を緑に

					if( m_RemoveShapeDataIndex >= tShapeDataNameArray.Length )
					{
						m_RemoveShapeDataIndex  = tShapeDataNameArray.Length - 1 ;
					}
					m_RemoveShapeDataIndex = EditorGUILayout.Popup( "", m_RemoveShapeDataIndex, tShapeDataNameArray, GUILayout.Width( 120f ) ) ;	// フィールド名有りタイプ
				
					if( tRemove == true )
					{
						// 削除する
						m_RemoveShapeDataIndexAnswer = m_RemoveShapeDataIndex ;
					}
				}
				GUILayout.EndHorizontal() ;		// 横並び終了
			}
		}
		else
		{
			if( tShapeDatas != null && tShapeDatas.Count >  0 )
			{
				// １つ以上存在していればリストとして描画する
				string[] tShapeDataNameArray = GetShapeDataNames( tShapeDatas ) ;

				//---------------------------------------------------------

				string tMessage = GetMessage( "RemoveTweenOK?" ).Replace( "%1", tShapeDataNameArray[ m_RemoveShapeDataIndexAnswer ] ) ;
				GUILayout.Label( tMessage ) ;
//				GUILayout.Label( "It does really may be to remove tween '" + tTweenIdentityArray[ mRemoveTweenIndexAnswer ] + "' ?" ) ;
				GUILayout.BeginHorizontal() ;	// 横並び開始
				{
					GUI.backgroundColor = Color.red ;
					if( GUILayout.Button( "OK", GUILayout.Width( 100f ) ) == true )
					{
						// 本当に削除する
						Undo.RecordObject( tTarget, "ComplexSoftMesh : ShapeData Remove" ) ;	// アンドウバッファに登録
						tTarget.RemoveShapeData( tShapeDatas[ m_RemoveShapeDataIndexAnswer ].name ) ;
						tTarget.Refresh() ;
						if( tTarget.colliderAdjustment == true )
						{
							tTarget.AdjustCollider() ;
						}
						EditorUtility.SetDirty( tTarget ) ;

						m_RemoveShapeDataIndexAnswer = -1 ;
					}
					GUI.backgroundColor = Color.white ;
					if( GUILayout.Button( "Cancel", GUILayout.Width( 100f ) ) == true )
					{
						m_RemoveShapeDataIndexAnswer = -1 ;
					}
				}
				GUILayout.EndHorizontal() ;     // 横並び終了
			}
		}

		//----------------------------------------------------------


		if( tShapeDatas != null && tShapeDatas.Count >  0 )
		{
			m_ShapeData_A_Show = EditorGUILayout.Foldout( m_ShapeData_A_Show, "ShapeData" ) ;
			if( m_ShapeData_A_Show == true )
			{
				CreateFoldFlag( tShapeDatas ) ;

				l = tShapeDatas.Count ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					// それぞれのシェイプデータを表示する
					m_ShapeData_I_Show[ i ] = EditorGUILayout.Foldout( m_ShapeData_I_Show[ i ], tShapeDatas[ i ].name ) ;
					if( m_ShapeData_I_Show[ i ] == true )
					{
						DrawShapeData( tTarget, tShapeDatas[ i ], i ) ;
					}
				}
			}
		}

		//-----------------------------------------------------------

		EditorGUILayout.Separator() ;   // 少し区切りスペース

		EditorGUILayout.Vector3Field( "Total Offset", tTarget.totalOffset ) ;
		EditorGUILayout.Vector3Field( "Total Size", tTarget.totalSize ) ;


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

	//--------------------------------------------------------------------------

	private string[] GetShapeDataNames( List<ComplexSoftMesh.ShapeData> tShapeDatas )
	{
		if( tShapeDatas == null || tShapeDatas.Count == 0 )
		{
			return null ;
		}

		int i, l, j, c ;

		// １つ以上存在していればリストとして描画する
		l = tShapeDatas.Count ;
		string tName ;
		string[] tShapeDataNameArray = new string[ l ] ;
		for( i  = 0 ; i <  l ; i ++ )
		{
			tShapeDataNameArray[ i ] = tShapeDatas[ i ].name ;
		}
		for( i  = 0 ; i <  l ; i ++ )
		{
			// 既に同じ名前が存在する場合は番号を振る
			tName = tShapeDataNameArray[ i ] ;

			c = 0 ;
			for( j  = i + 1 ; j <  l ; j ++ )
			{
				if( tShapeDataNameArray[ j ] == tName )
				{
					// 同じ名前を発見した
					c ++ ;
					tShapeDataNameArray[ j ] = tShapeDataNameArray[ j ] + "(" + c + ")" ;
				}
			}
		}
		return tShapeDataNameArray ;
	}

	private void CreateFoldFlag( List<ComplexSoftMesh.ShapeData> tShapeDatas )
	{
		int i, l = tShapeDatas.Count ;

		//-----------------------------------

		if( m_ShapeData_I_Show == null )
		{
			m_ShapeData_I_Show = new bool[ l ] ;
		}
		else
		if( m_ShapeData_I_Show.Length != l )
		{
			int n = m_ShapeData_I_Show.Length ;

			int c ;
			if( l >= n )
			{
				c  = n ;
			}
			else
			{
				c = l ;
			}

			bool[] tShapeData_I_Show = new bool[ l ] ;
			for( i  = 0 ; i <  c ; i ++ )
			{
				tShapeData_I_Show[ i ] = m_ShapeData_I_Show[ i ] ;
			}
			m_ShapeData_I_Show = tShapeData_I_Show ;
		}

		//-----------------------------------

		if( m_ShapeData_UVA_Show == null )
		{
			m_ShapeData_UVA_Show = new bool[ l ] ;
		}
		else
		if( m_ShapeData_UVA_Show.Length != l )
		{
			int n = m_ShapeData_UVA_Show.Length ;

			int c ;
			if( l >= n )
			{
				c  = n ;
			}
			else
			{
				c = l ;
			}

			bool[] tShapeData_UVA_Show = new bool[ l ] ;
			for( i  = 0 ; i <  c ; i ++ )
			{
				tShapeData_UVA_Show[ i ] = m_ShapeData_UVA_Show[ i ] ;
			}
			m_ShapeData_UVA_Show = tShapeData_UVA_Show ;
		}

		//-----------------------------------

		if( m_ShapeData_UVI_Show == null )
		{
			m_ShapeData_UVI_Show = new bool[ l ][] ;
		}
		else
		if( m_ShapeData_UVI_Show.Length != l )
		{
			int n = m_ShapeData_UVI_Show.Length ;

			int c ;
			if( l >= n )
			{
				c  = n ;
			}
			else
			{
				c = l ;
			}

			bool[][] tShapeData_UVI_Show = new bool[ l ][] ;
			for( i  = 0 ; i <  c ; i ++ )
			{
				tShapeData_UVI_Show[ i ] = m_ShapeData_UVI_Show[ i ] ;
			}
			m_ShapeData_UVI_Show = tShapeData_UVI_Show ;
		}
	}



	private void DrawShapeData( ComplexSoftMesh tTarget, ComplexSoftMesh.ShapeData tShapeData, int tIndex )
	{
		// ビジブル
		bool tVisible = EditorGUILayout.Toggle( "Visible", tShapeData.visible ) ;
		if( tVisible != tShapeData.visible )
		{
			Undo.RecordObject( tTarget, "ComplexSoftMesh : Visible Change" ) ;	// アンドウバッファに登録
			tShapeData.visible = tVisible ;
			tTarget.Refresh() ;
			if( tTarget.colliderAdjustment == true )
			{
				tTarget.AdjustCollider() ;
			}
			EditorUtility.SetDirty( tTarget ) ;
		}

		// タイプ
		ComplexSoftMesh.ShapeType tShapeType = ( ComplexSoftMesh.ShapeType )EditorGUILayout.EnumPopup( "Shape Type",  tShapeData.shapeType ) ;
		if( tShapeType != tShapeData.shapeType )
		{
			Undo.RecordObject( tTarget, "ComplexSoftMesh : Shape Type Change" ) ;	// アンドウバッファに登録
			tShapeData.shapeType = tShapeType ;
			tTarget.Refresh() ;
			if( tTarget.colliderAdjustment == true )
			{
				tTarget.AdjustCollider() ;
			}
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Capsule || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Cylinder )
		{
			// カプセル・シリンダーの場合の方向
			ComplexSoftMesh.Direction tDirection = ( ComplexSoftMesh.Direction )EditorGUILayout.EnumPopup( "Direction",  tShapeData.direction ) ;
			if( tDirection != tShapeData.direction )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Direction Change" ) ;	// アンドウバッファに登録
				tShapeData.direction = tDirection ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
		else
		if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Plane )
		{
			// プレーンの場合の方向
			ComplexSoftMesh.PlaneDirection tPlaneDirection = ( ComplexSoftMesh.PlaneDirection )EditorGUILayout.EnumPopup( "Plane Direction",  tShapeData.planeDirection ) ;
			if( tPlaneDirection != tShapeData.planeDirection )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Plane Direction Change" ) ;	// アンドウバッファに登録
				tShapeData.planeDirection = tPlaneDirection ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Cube || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Sphere || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Capsule || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Cylinder || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Plane )
		{
			// ３Ｄ系

			// オフセット
			Vector3 tOffset = EditorGUILayout.Vector3Field( "Offset", tShapeData.offset ) ;
			if( tOffset != tShapeData.offset )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				tShapeData.offset = tOffset ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
	
			// サイズ
			Vector3 tSize = EditorGUILayout.Vector3Field( "Size", tShapeData.size ) ;
			if( tSize != tShapeData.size )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Size Change" ) ;	// アンドウバッファに登録
				tShapeData.size = tSize ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
		}
		else
		{
			// ２Ｄ系

			// オフセット
			Vector2 tOffset2D_Old = new Vector2( tShapeData.offset.x, tShapeData.offset.y ) ;
			Vector2 tOffset2D_New = EditorGUILayout.Vector2Field( "Offset", tOffset2D_Old) ;
			if( tOffset2D_New != tOffset2D_Old )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Offset Change" ) ;	// アンドウバッファに登録
				tShapeData.offset = new Vector3( tOffset2D_New.x, tOffset2D_New.y, tShapeData.offset.z ) ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
	
			// サイズ
			Vector2 tSize2D_Old = new Vector2( tShapeData.size.x, tShapeData.size.y ) ;
			Vector2 tSize2D_New = EditorGUILayout.Vector2Field( "Size", tSize2D_Old ) ;
			if( tSize2D_New != tSize2D_Old )
			{
				Undo.RecordObject( tTarget, "SoftMesh : Size Change" ) ;	// アンドウバッファに登録
				tShapeData.size = new Vector2( tSize2D_New.x, tSize2D_New.y ) ;
				tTarget.Refresh() ;
				if( tTarget.colliderAdjustment == true )
				{
					tTarget.AdjustCollider() ;
				}
				EditorUtility.SetDirty( tTarget ) ;
			}
		}

		// カラー
		Color tVertexColor = Color.white ;
		tVertexColor.r = tShapeData.vertexColor.r ;
		tVertexColor.g = tShapeData.vertexColor.g ;
		tVertexColor.b = tShapeData.vertexColor.b ;
		tVertexColor.a = tShapeData.vertexColor.a ;
		tVertexColor = EditorGUILayout.ColorField( "Vertex Color", tVertexColor ) ;
//		Debug.LogWarning( "O:" + tTarget.vertexColor + " N:" + tVertexColor ) ;
		if
		(
			tVertexColor.r != tShapeData.vertexColor.r ||
			tVertexColor.g != tShapeData.vertexColor.g ||
			tVertexColor.b != tShapeData.vertexColor.b ||
			tVertexColor.a != tShapeData.vertexColor.a
		)
		{
//			Debug.LogWarning( "変化した" ) ;
			Undo.RecordObject( tTarget, "ComplexSoftMesh : Vertex Color Change" ) ;	// アンドウバッファに登録
			tShapeData.vertexColor = tVertexColor ;
			tTarget.Refresh() ;
			EditorUtility.SetDirty( tTarget ) ;
		}


		// ＵＶ
		if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Cube || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Plane || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Box2D )
		{
			Rect[] tUVA = tShapeData.uv ;
			if( tUVA != null && tUVA.Length >  0 )
			{
				m_ShapeData_UVA_Show[ tIndex ] = EditorGUILayout.Foldout( m_ShapeData_UVA_Show[ tIndex ], "UV" ) ;
				if( m_ShapeData_UVA_Show[ tIndex ] == true )
				{
					Rect tUV_Old ;
					Rect tUV_New ;

					int i, l = tUVA.Length ;
					if( m_ShapeData_UVI_Show[ tIndex ] == null )
					{
						m_ShapeData_UVI_Show[ tIndex ] = new bool[ l ] ;
					}

					if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Plane || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Box2D )
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

						m_ShapeData_UVI_Show[ tIndex ][ i ] = EditorGUILayout.Foldout( m_ShapeData_UVI_Show[ tIndex ][ i ], " Index " + i ) ;
						if( m_ShapeData_UVI_Show[ tIndex ][ i ] == true )
						{
							tUV_New = EditorGUILayout.RectField( tUV_New ) ;
							if( tUV_Old.Equals( tUV_New ) == false )
							{
								Undo.RecordObject( tTarget, "ComplexSoftMesh : UV Change" ) ;	// アンドウバッファに登録
								tShapeData.SetUV( i, tUV_New ) ;
								tTarget.Refresh() ;
								EditorUtility.SetDirty( tTarget ) ;
							}
						}
					}
				}
			}
		}
		
		// スプリット
		int tSplit = EditorGUILayout.IntSlider( "Split", tShapeData.split, 0, 5 ) ;
		if( tSplit != tShapeData.split )
		{
			Undo.RecordObject( tTarget, "ComplexSoftMesh : Split Change" ) ;	// アンドウバッファに登録
			tShapeData.split = tSplit ;
			tTarget.Refresh() ;
			EditorUtility.SetDirty( tTarget ) ;
		}

		if( tShapeData.shapeType == ComplexSoftMesh.ShapeType.Plane || tShapeData.shapeType == ComplexSoftMesh.ShapeType.Box2D )
		{
			// タイリング
			bool tTiling = EditorGUILayout.Toggle( "Tiling", tShapeData.tiling ) ;
			if( tTiling != tShapeData.tiling )
			{
				Undo.RecordObject( tTarget, "ComplexSoftMesh : Tiling Change" ) ;	// アンドウバッファに登録
				tShapeData.tiling = tTiling ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}
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
