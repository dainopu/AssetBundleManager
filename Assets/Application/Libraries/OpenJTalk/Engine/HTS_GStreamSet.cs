using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	// HTS_GStreamSet: set of generated parameter stream.
	public class HTS_GStreamSet
	{
		private int				m_NumericOfStream ;		// # of streams
		private int				m_TotalSample ;			// total sample
		private int				m_TotalFrame ;			// total frame
		private HTS_GStream[]	m_GStream ;				// generated parameter streams
		private double[]		m_GSpeech ;				// generated speech

		//-----------------------------------------------------------

		private const double	HTS_NODATA		= ( -1.0e+10 ) ;

		//-----------------------------------------------------------

		public HTS_GStreamSet()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_NumericOfStream	= 0 ;
			m_TotalFrame		= 0 ;
			m_TotalSample		= 0 ;
			m_GStream			= null ;
			m_GSpeech			= null ;
		}

		// HTS_GStreamSet_create: generate speech
		public bool Create( HTS_PStreamSet pss, int stage, bool use_log_gain, int sampling_rate, int tFramePeriod, double alpha, double beta, double volume )
		{
			int i, j, k ;
			int msd_frame ;
			int nlpf = 0 ;
			double[] lpf = null ;

			HTS_Vocoder tVocoder = new HTS_Vocoder() ;

			// check
			if( m_GStream != null || m_GSpeech != null )
			{
				Debug.LogError( "HTS_GStreamSet_create: HTS_GStreamSet is not initialized." ) ;
				return false ;
			}

			// initialize
			m_NumericOfStream	= pss.GetNumericOfStream() ;
			m_TotalFrame		= pss.GetTotalFrame() ;
			m_TotalSample		= tFramePeriod * m_TotalFrame ;
			m_GStream			= new HTS_GStream[ m_NumericOfStream ] ;

			for( i  = 0 ; i <  m_NumericOfStream ; i ++ )
			{
				m_GStream[ i ] = new HTS_GStream() ;

				m_GStream[ i ].vector_length	= pss.GetVectorLength( i ) ;
				m_GStream[ i ].par = new double[ m_TotalFrame ][] ;

				for( j  = 0 ; j <  m_TotalFrame ; j ++ )
				{
					m_GStream[ i ].par[ j ] = new double[ m_GStream[ i ].vector_length ] ;
				}
			}
			m_GSpeech = new double[ m_TotalSample ] ;
			
			// copy generated parameter
			for( i  = 0 ; i <  m_NumericOfStream ; i ++ )
			{
				if( pss.IsMsd( i ) == true )
				{ 
					// for MSD
					for( j  = 0, msd_frame  = 0 ; j <  m_TotalFrame ; j ++ )
					{
						if( pss.GetMsdFlag( i, j ) == true )
						{
							for( k  = 0 ; k <  m_GStream[ i ].vector_length ; k ++ )
							{
								m_GStream[ i ].par[ j ][ k ] = pss.GetParameter( i, msd_frame, k ) ;
							}
							
							msd_frame ++ ;
						}
						else
						{
							for( k  = 0 ; k <  m_GStream[ i ].vector_length ; k ++ )
							{
								m_GStream[ i ].par[ j ][ k ] = HTS_NODATA ;
							}
						}
					}
				}
				else
				{
					// for non MSD
					for( j  = 0 ; j <  m_TotalFrame ; j ++ )
					{
						for( k  = 0 ; k <  m_GStream[ i ].vector_length ; k ++ )
						{
							m_GStream[ i ].par[ j ][ k ] = pss.GetParameter( i, j, k ) ;
						}
					}
				}
			}

			// check
			if( m_NumericOfStream != 2 && m_NumericOfStream != 3 )
			{
				Debug.LogError( "HTS_GStreamSet_create: The number of streams should be 2 or 3." ) ;
				Initialize() ;
				return false ;
			}
			
			if( pss.GetVectorLength( 1 ) != 1 )
			{
				Debug.LogError( "HTS_GStreamSet_create: The size of lf0 static vector should be 1." ) ;
				Initialize() ;
				return false ;
			}
			
			if( m_NumericOfStream >= 3 && m_GStream[ 2 ].vector_length % 2 == 0 )
			{
				Debug.LogError( "HTS_GStreamSet_create: The number of low-pass filter coefficient should be odd numbers." ) ;
				Initialize() ;
				return false ;
			}

			Debug.LogWarning( "================= Vocoder !!! : " + m_TotalFrame ) ;

			// synthesize speech waveform
			tVocoder.Prepare( m_GStream[ 0 ].vector_length - 1, stage, use_log_gain, sampling_rate, tFramePeriod ) ;

			if( m_NumericOfStream >= 3 )
			{
				nlpf = m_GStream[ 2 ].vector_length ;
			}
			
			// トータルフレーム数分ループする
			for( i  = 0 ; i <  m_TotalFrame ; i ++ )
			{
				j = i * tFramePeriod ;
				if( m_NumericOfStream >= 3 )
				{
					// ０じゃなかったらヤバかった
//					lpf = this.gstream[ 2 ].par[ i ][ 0 ] ;
					lpf = m_GStream[ 2 ].par[ i ] ;
				}
				
				// オフセット補正あり
//				HTS_Vocoder_synthesize(&v, gss->gstream[0].vector_length - 1, gss->gstream[1].par[i][0], &gss->gstream[0].par[i][0], nlpf, lpf, alpha, beta, volume, &gss->gspeech[j], audio);

				if( i == 0 )
				{
					HTS_Engine.SetDebugFlag( 0, 1 ) ;
				}

				tVocoder.Synthesize( m_GStream[ 0 ].vector_length - 1, m_GStream[ 1 ].par[ i ][ 0 ], m_GStream[ 0 ].par[ i ], nlpf, lpf, alpha, beta, volume, m_GSpeech, j ) ;

				if( i == 0 )
				{
					HTS_Engine.SetDebugFlag( 0, 0 ) ;
				}
			}

			tVocoder.Initialize() ;

			return true ;
		}

		//---------------------------------------------------------------------------

		// HTS_GStreamSet_get_total_frame: get total number of frame
		public int GetTotalFrame()
		{
			return m_TotalFrame ;
		}

		// HTS_GStreamSet_get_total_nsamples: get total number of sample
		public int GetTotalSample()
		{
			return m_TotalSample ;
		}

		// HTS_GStreamSet_get_parameter: get generated parameter
		public double GetParameter( int tStreamIndex, int tFrameIndex, int tVectorIndex )
		{
			return m_GStream[ tStreamIndex ].par[ tFrameIndex ][ tVectorIndex ] ;
		}

		// HTS_GStreamSet_get_vector_length: get features length
		public int GetVectorLength( int tStreamIndex )
		{
			return m_GStream[ tStreamIndex ].vector_length ;
		}

		// HTS_GStreamSet_get_speech: get synthesized speech parameter
		public double GetSpeech( int tSampleIndex )
		{
			return m_GSpeech[ tSampleIndex ] ;
		}
	}
}

