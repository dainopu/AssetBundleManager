using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using HTS_Engine_API ;

using MecabForOpenJTalk ;

namespace OJT
{
	public class OpenJTalk
	{
		private Mecab		m_Mecab		= new Mecab() ;
		private NJD			m_NJD		= new NJD() ;
		private JPCommon	m_JPCommon	= new JPCommon() ;
		private HTS_Engine	m_Engine	= new HTS_Engine() ;

		//-----------------------------------------------------------

		public OpenJTalk()
		{
			Initialize() ;
		}

		/// <summary>
		/// 初期化する
		/// </summary>
		public void Initialize()
		{
			m_Mecab.Initialize() ;
			m_NJD.Initialize() ;
			m_JPCommon.Initialize() ;
			m_Engine.Initialize() ;
		}

		/// <summary>
		/// データをロードする
		/// </summary>
		/// <param name="tDirectory"></param>
		/// <param name="tVoice"></param>
		/// <returns></returns>
		public bool Load( string tDictionaryDirectory, string tVoiceFilePath = null )
		{
			if( m_Mecab.Load( tDictionaryDirectory ) != true )
			{
				Initialize() ;
				return false ;
			}

			if( string.IsNullOrEmpty( tVoiceFilePath ) == true )
			{
				return true ;	// ボイスファイルはロードしない
			}

			return LoadVoice( tVoiceFilePath ) ;
		}

		/// <summary>
		/// ボイスデータをロードする
		/// </summary>
		/// <param name="tVoice"></param>
		/// <returns></returns>
		public bool LoadVoice( string tVoiceFilePath )
		{
			if( m_Engine.Load( tVoiceFilePath ) != true )
			{
				m_Engine.Initialize() ;
				return false ;
			}

			if( m_Engine.GetFullcontextLabelFormat() != "HTS_TTS_JPN" )
			{
				m_Engine.Initialize() ;
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// 生成する
		/// </summary>
		/// <param name="tText"></param>
		/// <param name="wavfp"></param>
		/// <param name="logfp"></param>
		/// <returns></returns>
		public bool Synthesis( string tText )
		{
			Refresh() ;

			// Mecab で解析を行う
			string[] tFeature = m_Mecab.Analyze( tText ) ;
			if( tFeature == null )
			{
				return false ;
			}

			// NJD で解析を行う
			if( m_NJD.Analyze( tFeature ) == false )
			{
				Refresh() ;
				return false ;
			}

			// JPCommon で解析を行う
			tFeature = m_JPCommon.Analyze( m_NJD ) ;
			if( tFeature == null || tFeature.Length <= 2 )
			{
				// 失敗
				Refresh() ;
				return false ;
			}

			// HTS_Enigne で音声を生成する
			return m_Engine.SynthesizeFromStrings( tFeature ) ;
		}

		/// <summary>
		/// 生成データをリフレッシュする
		/// </summary>
		public void Refresh()
		{
			m_NJD.Refresh() ;
			m_JPCommon.Refresh() ;
			m_Engine.Refresh() ;
		}
		
		/// <summary>
		/// 生成データを取得する
		/// </summary>
		/// <returns></returns>
		public float[] GetWaveData()
		{
			return m_Engine.GetWaveData() ;
		}

		public int GetWaveData( float[] tBuffer, int tOffset = 0, int tLength = 0 )
		{
			return m_Engine.GetWaveData( tBuffer, tOffset, tLength ) ;
		}


		/// <summary>
		/// 生成ファイルを取得する
		/// </summary>
		/// <returns></returns>
		public byte[] GetWaveFile()
		{
			return m_Engine.GetWaveFile() ;
		}



		//-----------------------------------------------------------

		public void SetSamplingFrequency( int i )
		{
			m_Engine.SetSamplingFrequency( i ) ;
		}

		public int GetSamplingFrequency()
		{
			return m_Engine.GetSamplingFrequency() ;
		}

		public void SetFperiod( int i )
		{
		   m_Engine.SetFperiod( i ) ;
		}

		public void SetAlpha( double f )
		{
			m_Engine.SetAlpha( f ) ;
		}
		
		public void SetBeta( double f )
		{
			m_Engine.SetBeta( f ) ;
		}
		
		public void SetSpeed( double f )
		{
			m_Engine.SetSpeed( f ) ;
		}
		
		public void AddHalfTone( double f )
		{
			m_Engine.AddHalfTone( f ) ;
		}
		
		public void SetMsdThreshold( int i, double f )
		{
			m_Engine.SetMsdThreshold( i, f ) ;
		}
		
		public void SetGvWeight( int i, double f )
		{
			m_Engine.SetGvWeight( i, f ) ;
		}
		
		public void SetVolume( double f )
		{
			m_Engine.SetVolume( f ) ;
		}
		
		//---------------------------------------------------------------------------

		public static bool Run( string tCommand )
		{
			if( string.IsNullOrEmpty( tCommand ) == true )
			{
				return false ;
			}

			tCommand = tCommand.Replace( "  ", " " ) ;
			tCommand = tCommand.Replace( "  ", " " ) ;

			string[] tArgv = tCommand.Split( ' ' ) ;

			//----------------------------------------------------------

			int i ;
			
			// 辞書フォルダ
			string tDictionaryDirectory = null ;
			
			// 音声ファイル
			string tVoiceFilePath = null ;
			
			// テキストファイルパス
			string tTextFilePath = null ;
			
			// 出力ファイルパス
			string tWavaFilePath = "Voice.wav" ;

			//----------------------------------

			int tArgc = tArgv.Length ;

			// ヘルプ
			for( i  = 0 ; i <  tArgc ; i ++ )
			{
				if( tArgv[ i ].Length >= 2 && tArgv[ i ][ 0 ] == '-' && tArgv[ i ][ 1 ] == 'h' )
				{
					return true ;
				}
			}

			// 辞書フォルダ
			for( i  = 0 ; i <  tArgc ; i ++ )
			{
				if(  tArgv[ i ].Length >= 2 && tArgv[ i ][ 0 ] == '-' && tArgv[ i ][ 1 ] == 'x' && ( i + 1 ) <  tArgc )
				{
					tDictionaryDirectory = tArgv[ i + 1 ] ;
					break ;
				}
			}

			if( string.IsNullOrEmpty( tDictionaryDirectory ) == true )
			{
				Debug.LogError( "Error: Dictionary must be specified." ) ;
				return false ;
			}
			
			// 音声ファイル
			for( i  = 0 ; i <  tArgc ; i ++ )
			{
				if( tArgv[ i ].Length >= 2 && tArgv[ i ][ 0 ] == '-' && tArgv[ i ][ 1 ] == 'm' )
				{
					tVoiceFilePath = tArgv[ i + 1 ] ;
					break ;
				}
			}

			if( string.IsNullOrEmpty( tVoiceFilePath ) == true )
			{
				Debug.LogError( "Error: HTS voice must be specified." ) ;
				return false ;
			}
			
			//------------------------------------------------------------------

			// 必要なデータをロードしている

			// Open JTalk
			OpenJTalk tOpenJTalk = new OpenJTalk() ;

			// load dictionary and HTS voice
			if( tOpenJTalk.Load( tDictionaryDirectory, tVoiceFilePath ) != true )
			{
				Debug.LogError( "Error: Dictionary or HTS voice cannot be loaded." ) ;
				tOpenJTalk.Initialize() ;
				return false ;
			}
			
			// get options
			for( i  = 0 ; i <  tArgc ; i ++ )
			{
				if( string.IsNullOrEmpty( tArgv[ i ] ) == false && tArgv[ i ].Length >= 2 && tArgv[ i ][ 0 ] == '-' )
				{
					switch( tArgv[ i ][ 1 ] )
					{
						case 'o':
							if( tArgv[ i ].Length >= 3 )
							{	 
								switch( tArgv[ i ][ 2 ] )
								{
									case 'w' :
										tWavaFilePath = tArgv[ i + 1 ] ;
									break ;

									default :
										Debug.LogError( "Error: Invalid option '-o" + tArgv[ i ].Substring( 2, 1 ) + "'." ) ;
										Application.Quit() ;
									break ;
								}
							}
							i ++ ;	// パラメータスキップ
						break ;

						case 'h' :
						break ;

						case 'x' :
							i ++ ;				// パラメータスキップ
						break;

						case 'm':
							i ++ ;				// パラメータスキップ
						break ;

						case 's' :
							tOpenJTalk.SetSamplingFrequency( int.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;

						case 'p' :
							tOpenJTalk.SetFperiod( int.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;

						case 'a' :
							tOpenJTalk.SetAlpha( float.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;
						
						case 'b' :
							tOpenJTalk.SetBeta( float.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;
						
						case 'r' :
							tOpenJTalk.SetSpeed( float.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;
						
						case 'f' :
							if( tArgv[ i ].Length >= 3 )
							{
								switch( tArgv[ i ][ 2 ] )
								{
									case 'm' :
										tOpenJTalk.AddHalfTone( float.Parse( tArgv[ i + 1 ] ) ) ;
									break ;
	
									default :
										Debug.LogError( "Error: Invalid option '-f" +  tArgv[ i ].Substring( 2, 1 ) + "'." ) ;
										Application.Quit() ;
									break ;
								}
							}
							i ++ ;				// パラメータスキップ
						break;

						case 'u' :
							tOpenJTalk.SetMsdThreshold( 1, float.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;
						
						case 'j' :
							if( tArgv[ i ].Length >= 3 )
							{	 
								switch( tArgv[ i ][ 2 ] )
								{
									case 'm' :
										tOpenJTalk.SetGvWeight( 0, float.Parse( tArgv[ i + 1 ] ) ) ;
									break;

									case 'f' :
									case 'p' :
										tOpenJTalk.SetGvWeight( 1, float.Parse( tArgv[ i + 1 ] ) ) ;
									break;

									default :
										Debug.LogError( "Error: Invalid option '-j{" + tArgv[ i ].Substring( 2, 1 ) + "}'." ) ;
										Application.Quit() ;
									break ;
								}
							}
							i ++ ;				// パラメータスキップ
						break ;
						
						case 'g' :
							tOpenJTalk.SetVolume( float.Parse( tArgv[ i + 1 ] ) ) ;
							i ++ ;				// パラメータスキップ
						break ;

						default :
							Debug.LogError( "Error: Invalid option '-{" + tArgv[ i ].Substring( 2, 1 ) + "}'." ) ;
							Application.Quit() ;
						break ;
					}
				}
				else
				{
					tTextFilePath = tArgv[ i ] ;
				}
			}

			//--------------------------------------------------------------------------

			// synthesize
			string tText = OpenJTalk_StorageAccessor.LoadText( tTextFilePath ) ;

			Debug.LogWarning( "文字列:" + tText ) ;

			if( tOpenJTalk.Synthesis( tText ) != true )
			{
				Debug.LogError( "Error: waveform cannot be synthesized." ) ;
				tOpenJTalk.Initialize() ;
				Application.Quit() ;
			}
			

			byte[] tData = tOpenJTalk.GetWaveFile() ;
			if( tData != null && tData.Length >  0 )
			{
				Debug.LogWarning( "ファイルサイズ:" + tData.Length ) ;
				OpenJTalk_StorageAccessor.Save( tWavaFilePath, tData ) ;
			}
			
			Debug.LogWarning( "[成功] --> " + tWavaFilePath ) ;
			return true ;
		}
	}
}
