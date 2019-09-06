using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

/// <summary>
/// フォルマントキャリブレーターパッケージ
/// </summary>
namespace FormantCalibrator
{
	/// <summary>
	/// ウェーブノートピッカークラス(エディター用) Version 2017/10/20 0
	/// </summary>
	public class FormantCalibrator : EditorWindow
	{
		[ MenuItem( "Tools/Formant Calibrator" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<FormantCalibrator>( false, "Formant Calibrator", true ) ;
		}

		//----------------------------------------------------------


		

		private string	m_WavePath = "" ;

		private int		m_Sex = -1 ;

		private int[,]	m_DefaultFormant = new int[ 2, 10 ]
		{
			{	// 男
				 760, 1170,		// あ
				 260, 2270,		// い
				 370, 1300,		// う
				 480, 1740,		// え
				 550,  840,		// お
			},
			{	// 女
				 880, 1380,		// あ
				 330, 2730,		// い
				 380, 1660,		// う
				 490, 2330,		// え
				 490,  940,		// お
			}
		} ;

		private	int		m_LpcOrder			=  64 ;	// 人物毎に変えるのが理想
		private	int		m_LpcWidth			= ( int )( 44100 * 40 / 1000 ) ;	// 人物毎に変えるのが理想
		private	float	m_MinimumVolume		= 1e-5f ;
		private float	m_Scope				= 30 ;

		//-----------------------------------

		private int[]	m_ActiveFormant = new int[ 10 ] ;

		protected float[] m_LpcWork1 = null ;
		protected float[] m_LpcWork2 = null ;
		protected float[] m_LpcWork  = null ;


		//----------------------------------------------------------

		// レイアウトを描画する
		private void OnGUI()
		{
			string tPath ;

			//----------------------------------------------------------

			// 保存先のパスの設定
			EditorGUILayout.HelpBox( "母音測定対象となるサウンドファイルを選択して下さい", MessageType.Info ) ;
			GUILayout.BeginHorizontal() ;
			{
				// 測定に使うウェーブパスを選択する
				if( GUILayout.Button( "Wave Path", GUILayout.Width( 80f ) ) == true )
				{
					if( Selection.objects != null && Selection.objects.Length == 1 && Selection.activeObject != null )
					{
						tPath = AssetDatabase.GetAssetPath( Selection.activeObject.GetInstanceID() ) ;
						if( System.IO.File.Exists( tPath ) == true )
						{
							// ファイルを指定しています
							m_WavePath = tPath ;
						}
					}
				}
			
				// 保存パス
				m_WavePath = EditorGUILayout.TextField( m_WavePath ) ;
			}
			GUILayout.EndHorizontal() ;

			GUILayout.Space( 16 ) ;

			//----------------------------------------------------------

			// 性別

			int tSex = m_Sex ;
			if( tSex <  0 )
			{
				tSex  = 0 ;
			}

			GUILayout.BeginHorizontal() ;
			{
				EditorGUILayout.LabelField( "性別", GUILayout.Width( 36f ) ) ;
	
				if( EditorGUILayout.ToggleLeft( "男", tSex == 0, GUILayout.Width( 40f ) ) == true )
				{
					tSex = 0 ;
				}
				if( EditorGUILayout.ToggleLeft( "女", tSex == 1, GUILayout.Width( 40f ) ) == true )
				{
					tSex = 1 ;
				}
			}
			GUILayout.EndHorizontal() ;
			
			//----------------------------------------------------------

			// 母音(フォルマント)

			string[] tVowel = { "あ", "い", "う", "え", "お" } ;
			int v ;

			if( tSex != m_Sex )
			{
				// フォルマカント変更(リセット)

				for( v  = 0 ; v <  5 ; v ++ )
				{
					m_ActiveFormant[ v * 2 + 0 ] = m_DefaultFormant[ tSex, v * 2 + 0 ] ;
					m_ActiveFormant[ v * 2 + 1 ] = m_DefaultFormant[ tSex, v * 2 + 1 ] ;
				}

				m_Sex = tSex ;
			}

			for( v  = 0 ; v <  5 ; v ++ )
			{
				GUILayout.BeginHorizontal() ;
				{
					EditorGUILayout.LabelField( "", GUILayout.Width( 16f ) ) ;
					EditorGUILayout.LabelField( tVowel[ v ], GUILayout.Width( 24f ) ) ;

					EditorGUILayout.LabelField( "F1", GUILayout.Width( 20f ) ) ;
					EditorGUILayout.IntField( m_ActiveFormant[ v * 2 + 0 ], GUILayout.Width( 48f ) ) ;

					EditorGUILayout.LabelField( "F2", GUILayout.Width( 20f ) ) ;
					EditorGUILayout.IntField( m_ActiveFormant[ v * 2 + 1 ], GUILayout.Width( 48f ) ) ;
				}
				GUILayout.EndHorizontal() ;
			}

			GUILayout.Space( 16 ) ;

			int tLpcOrder = EditorGUILayout.IntField( "LPC Order", m_LpcOrder, GUILayout.Width( 320f ) ) ;
			if( tLpcOrder >=  32 )
			{
				m_LpcOrder = tLpcOrder ;
			}
			int tLpcWidth = EditorGUILayout.IntField( "LPC Width", m_LpcWidth, GUILayout.Width( 320f ) ) ;
			if( tLpcWidth >= 256 )
			{
				m_LpcWidth = tLpcWidth ;
			}

			m_MinimumVolume = EditorGUILayout.FloatField( "Minimum Volume", m_MinimumVolume, GUILayout.Width( 320f ) ) ;

			m_Scope = EditorGUILayout.FloatField( "Scope", m_Scope, GUILayout.Width( 320f ) ) ;

			//----------------------------------------------------------

			bool tExecute = false ;
			if( string.IsNullOrEmpty( m_WavePath ) == false )
			{
				GUILayout.Space( 16 ) ;

				GUI.backgroundColor = Color.green ;
				tExecute = GUILayout.Button( "CALIBRATE" ) ;
				GUI.backgroundColor = Color.white ;
			}

			if( tExecute == true )
			{
				Calibrate( m_WavePath ) ;
			}
		}

		//---------------------------------------------------------------

		// 選択しているものが変化したら再描画する
		private void OnSelectionChange() 
		{
			Repaint() ;
		}
	
		//---------------------------------------------------------------------------

		// 母音を推定する
		private void Calibrate( string tWavePath )
		{
			bool tPreloadAudioData ;

			AudioImporter tAudioImporter = AssetImporter.GetAtPath( tWavePath ) as AudioImporter ;
			tPreloadAudioData = tAudioImporter.preloadAudioData ;
			if( tPreloadAudioData == true )
			{
				tAudioImporter.preloadAudioData = false ;
				AssetDatabase.ImportAsset( tWavePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			//----------------------------------------------------------

			AudioClip tAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>( tWavePath ) ;

			// オーディオクリップのサンプルデータを読み出せるようにする
			tAudioClip.LoadAudioData() ;

			//----------------------------------------------------------

			// 総サンプル数
			int tSampleCount = tAudioClip.samples ;

			m_LpcWork1 = new float[ m_LpcWidth * 1 ] ;
			m_LpcWork2 = new float[ m_LpcWidth * 2 ] ;
			m_LpcWork  = new float[ m_LpcWidth ] ;

			float tDF = ( float )tAudioClip.frequency / ( float )m_LpcWidth ;

			Vector2 tFormant ;
			float	tScore ;

			Vector2[]	tTotalFormant	= { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero } ;
			float[]		tTotalScore		= { 0, 0, 0, 0, 0 } ;
			int[]		tCount			= { 0, 0, 0, 0, 0 } ;

			int  p, i ;
			for( p  = 0 ; p <  tSampleCount ; p = p + m_LpcWidth )
			{
				// サンプルデータをコピーする
				if( tAudioClip.channels == 1 )
				{
					// モノラル推奨
					tAudioClip.GetData( m_LpcWork1, p * 1 ) ;
				}
				else
				if( tAudioClip.channels == 2 )
				{
					// ステレオ
					tAudioClip.GetData( m_LpcWork2, p * 2 ) ;
				
					for( i  = 0 ; i <  m_LpcWidth ; i ++ )
					{
						m_LpcWork1[ i ] = ( m_LpcWork2[ ( i << 1 ) + 0 ] + m_LpcWork2[ ( i << 1 ) + 1 ] ) * 0.5f ;	// 平均値
					}
				}

				int		tVowel  = GetVowel( m_LpcWork1, tDF, m_LpcOrder, out tFormant, out tScore ) ;
				float	tVolume = GetVolume( m_LpcWork1 ) ;
				if( tVowel >= 0 && tVolume >= m_MinimumVolume )
				{
					// 母音として認識した
					tTotalFormant[ tVowel ]	= tTotalFormant[ tVowel ]	+ tFormant ;
					tTotalScore[ tVowel ]	= tTotalScore[ tVowel ]		+ tScore ;
					tCount[ tVowel ] ++ ;	
				}
			}

			//-----------------------------------

			string[] tWord = { "あ", "い", "う", "え", "お" } ;

			int tTotalCount = 0 ;

			float tTotalMatch = 0 ;

			int v ;
			for( v  = 0 ; v <  5 ; v ++ )
			{
				Debug.LogWarning( "推定結果:" + tWord[ v ] ) ;
				Debug.LogWarning( "認識数:" + tCount[ v ] ) ;
				if( tCount[ v ] >  0 )
				{
					Debug.LogWarning( "平均　F1:" + ( tTotalFormant[ v ].x / tCount[ v ] ) + " F2:" + ( tTotalFormant[ v ].y / tCount[ v ] ) ) ;
					Debug.LogWarning( "平均  Score:" + ( tTotalScore[ v ] / tCount[ v ] ) ) ;

					tTotalCount = tTotalCount + tCount[ v ] ;

					tTotalMatch = tTotalMatch + ( tTotalScore[ v ] / tCount[ v ] ) ;
				}
			}

			Debug.LogWarning( "累計認識数:" + tTotalCount ) ;
			Debug.LogWarning( "合計平均スコア:" + tTotalMatch ) ;

			for( v  = 0 ; v <  5 ; v ++ )
			{
				if( tCount[ v ] >  0 )
				{
					m_ActiveFormant[ v * 2 + 0 ] = ( int )( tTotalFormant[ v ].x / tCount[ v ] ) ;
					m_ActiveFormant[ v * 2 + 1 ] = ( int )( tTotalFormant[ v ].y / tCount[ v ] ) ;
				}
			}

			//----------------------------------------------------------
			
			// オーディオクリップのサンプルデータを破棄する
			tAudioClip.UnloadAudioData() ;

			if( tPreloadAudioData == true )
			{
				// PreloadAudioData を変更していた場合は元に戻す
				tAudioImporter.preloadAudioData = true ;
				AssetDatabase.ImportAsset( tWavePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport ) ;
			}

			//----------------------------------------------------------
		}

		//---------------------------------------------------------------------------

		// 母音情報を取得する(区間の平均値)
		protected int GetVowel( float[] tLpcWork, float tDF, int tLpcOrder, out Vector2 oFormant, out float oScore )
		{
			oFormant.x = 0 ;
			oFormant.y = 0 ;

			oScore = 0 ;

			//----------------------------------------------------------

			// 解析のためにデータをコピーする
			System.Array.Copy( tLpcWork, 0, m_LpcWork, 0, tLpcWork.Length ) ;
		
			// フォルマントの位置を取得する
			Vector2 tFormantIndices = GetFormantIndices( m_LpcWork, tLpcOrder ) ;
		
			float f1 = tFormantIndices.x * tDF ;
			float f2 = tFormantIndices.y * tDF ;

			//-----------------------------------

			// 母音推定
			float[] tVowelCode = new float[ 5 ] ;

			int v ;
			float f1_Average, f2_Average ;
//			float f1_Min, f1_Max, , f2_Min, f2_Max ;

//			float tScope = m_Scope / 100.0f ;

			for( v  = 0 ; v <  5 ; v ++ )
			{
				f1_Average	= m_ActiveFormant[ v * 2 + 0 ] ;
//				f1_Min		= f1_Average * ( 1.0f - tScope );
//				f1_Max		= f1_Average * ( 1.0f + tScope );

				f2_Average	= m_ActiveFormant[ v * 2 + 1 ] ;
//				f2_Min		= f2_Average * 0.8f ;
//				f2_Max		= f2_Average * 1.2f ;

//				if( f1 >= f1_Min && f1 <= f1_Max && f2 >= f2_Min && f2 <= f2_Max )
//				{
					tVowelCode[ v ] = Mathf.Pow( f1 - f1_Average, 2.0f ) + Mathf.Pow( f2 - f2_Average, 2.0f ) ;
//				}
//				else
//				{
//					tVowelCode[ v ] = -1 ;
//				}
			}

			int   tIndex = -1 ;
			float tSmall = Mathf.Infinity ;
			
			for( v  = 0 ; v <  5 ; v ++ )
			{
				if( tVowelCode[ v ] >= 0 && tVowelCode[ v ] <  tSmall )
				{
					tSmall = tVowelCode[ v ] ;
					tIndex = v ;
				}
			}

			if( tIndex >= 0 )
			{
				// 母音として認識した
				oFormant.x = f1 ;
				oFormant.y = f2 ;

				oScore = tSmall ;
			}

			return tIndex ;
		}



		// 区間の特徴量を取得する
		private Vector2 GetFormantIndices( float[] tLpcWork, int tLpcOrder )
		{
			int i, j, k, n, l, N = tLpcWork.Length ;
		
			//----------------------------------------------------------
			// ハミング窓をかける

			for( i  = 1 ; i <  N - 1 ; i ++ )
			{
				// 予めデータはコピーされている必要がある
				tLpcWork[ i ] *= 0.54f - 0.46f * Mathf.Cos( 2.0f * Mathf.PI * i / ( N - 1 ) ) ;
			}
			tLpcWork[ 0 ] = tLpcWork[ N - 1 ] = 0.0f ;
			
			//----------------------------------------------------------
			// 0～1の範囲に正規化する

			float tMax = 0.0f, tMin = 0.0f ;
			for( i  = 0 ; i <  N ; ++ i )
			{
				if( tLpcWork[ i ] >  tMax )
				{
					tMax = tLpcWork[ i ] ;
				}
				if( tLpcWork[ i ] <  tMin )
				{
					tMin = tLpcWork[ i ] ;
				}
			}

			tMax = Mathf.Abs( tMax ) ;
			tMin = Mathf.Abs( tMin ) ;

			float tFactor = 1.0f ;

			if( tMax >  tMin )
			{
				tFactor = 1.0f / tMax ;
			}
			if( tMax <  tMin )
			{
				tFactor = 1.0f / tMin ;
			}

			for( i  = 0 ; i <  N ; ++ i )
			{
				tLpcWork[ i ] *= tFactor ;
			}

			//----------------------------------------------------------
			// ＬＰＣ

			// 自己相関関数
			float[] r = new float[ tLpcOrder + 1 ] ;
			for( l  = 0 ; l <  tLpcOrder + 1 ; l ++ )
			{
				r[ l ] = 0.0f ;
				for( n  = 0 ; n <  N - l ; n ++ )
				{
					r[ l ] += tLpcWork[ n ] * tLpcWork[ n + l ] ;
				}
			}
		
			// Levinson-Durbin のアルゴリズムで LPC 係数を計算
			float[] a = new float[ tLpcOrder + 1 ] ;
			float[] e = new float[ tLpcOrder + 1 ] ;
			for( i  = 0 ; i <  tLpcOrder + 1 ; ++ i )
			{
				a[ i ] = e[ i ] = 0.0f ;
			}
			a[ 0 ] =  e[ 0 ] = 1.0f ;
			a[ 1 ] = -r[ 1 ] / r[ 0 ] ;
			e[ 1 ] =  r[ 0 ] + r[ 1 ] * a[ 1 ] ;

			for( k  = 1 ; k <  tLpcOrder ; k ++ )
			{
				float tLambda = 0.0f ;
				for( j  = 0 ; j <  k + 1 ; j ++ )
				{
					tLambda -= a[ j ] * r[ k + 1 - j ] ;
				}
				tLambda /= e[ k ] ;
			
				float[] U = new float[ k + 2 ] ;
				float[] V = new float[ k + 2 ] ;
				U[ 0 ] = 1.0f ;
				V[ 0 ] = 0.0f ;

				for( i  = 1 ; i <  k + 1 ; i ++ )
				{
					U[ i ]         = a[ i ] ;
					V[ k + 1 - i ] = a[ i ] ;
				}

				U[ k + 1 ] = 0.0f ;
				V[ k + 1 ] = 1.0f ;
			
				for( i  = 0 ; i <  k + 2 ; i ++ )
				{
					a[ i ] = U[ i ] + tLambda * V[ i ] ;
				}
			
				e[ k + 1 ] = e[ k ] * ( 1.0f - tLambda * tLambda ) ;
			}

			//----------------------------------------------------------
			// デジタルフィルタ

			float[] H = new float[ N ] ;
			for( n  = 0 ; n <  N ; n ++ )
			{
				float tNumeratorRe   = 0.0f, tNumeratorIm   = 0.0f ;
				float tDenominatorRe = 0.0f, tDenominatorIm = 0.0f ;

				for( i  = 0 ; i <  tLpcOrder + 1 ; i ++ )
				{
					float re = Mathf.Cos( -2.0f * Mathf.PI * n * i / N ) ;
					float im = Mathf.Sin( -2.0f * Mathf.PI * n * i / N ) ;
					tNumeratorRe   += e[ tLpcOrder - i ] * re ;
					tNumeratorIm   += e[ tLpcOrder - i ] * im ;
					tDenominatorRe += a[ tLpcOrder - i ] * re ;
					tDenominatorIm += a[ tLpcOrder - i ] * im ;
				}

				float tNumerator   = Mathf.Sqrt( Mathf.Pow( tNumeratorRe,   2.0f ) + Mathf.Pow( tNumeratorIm,   2.0f ) ) ;
				float tDenominator = Mathf.Sqrt( Mathf.Pow( tDenominatorRe, 2.0f ) + Mathf.Pow( tDenominatorIm, 2.0f ) ) ;
				H[ n ] = tNumerator / tDenominator ;
			}
		
			//----------------------------------------------------------
			// フォルマント f1 f2 を決定

			bool tFoundFirst = false ;
			int f1 = 0, f2 = 0 ;
			for( i  = 1 ; i <  N - 1 ; i ++ )
			{
				if( H[ i ] >  H[ i - 1 ] && H[ i ] >  H[ i + 1 ] )
				{
					if( tFoundFirst == false )
					{
						f1 = i ;	// １番目のピーク
						tFoundFirst = true ;
					}
					else
					{
						f2 = i ;	// ２番目のピーク
						break ;
					}
				}
			}

			return new Vector2( f1, f2 ) ;
		}

		// 音量情報を取得する(区間の平均値)
		private float GetVolume( float[] tLpcWork )
		{
			int i, l = tLpcWork.Length ;

			float tVolume = 0.0f ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tVolume += tLpcWork[ i ] * tLpcWork[ i ] ;	// 振幅は－１～＋１であるため二乗する
			}
			tVolume /= l ;	// 区間内の要素数で割って平均値を出す

			return tVolume ;
		}


	}
}

