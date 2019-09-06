using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	// HTS_Engine: Engine itself.
	public class HTS_Engine
	{
		public static int[] m_DebugFlags = new int[ 100 ] ;
		public static void SetDebugFlag( int tIndex, int tValue )
		{
			m_DebugFlags[ tIndex ] = tValue ;
		}
		public static int GetDebugFlag( int tIndex )
		{
			return m_DebugFlags[ tIndex ] ;
		}

		//---------------------------------------------------------------------------

		public const string	COPYRIGHT	= "The HMM-Based Speech Synthesis Engine \"hts_engine API\"\nVersion 1.10 (http://hts-engine.sourceforge.net/)\nCopyright (C) 2001-2015 Nagoya Institute of Technology\n              2001-2008 Tokyo Institute of Technology\nAll rights reserved.\n" ;

		private const double MAX_F0		= 20000.0								;
		private const double MIN_F0		= 20.0									;
		private const double MAX_LF0	= 9.9034875525361280454891979401956		;	// log(20000.0)
		private const double MIN_LF0	= 2.9957322735539909934352235761425		;	// log(20.0)
		private const double HALF_TONE	= 0.05776226504666210911810267678818	;   // log(2.0) / 12.0
		private const double DB			= 0.11512925464970228420089957273422	;	// log(10.0) / 20.0

		//-----------------------------------------------------------

		private HTS_Condition	m_Condition ;					// synthesis condition
		private HTS_ModelSet	m_ModelSet ;						// set of duration models, HMMs and GV models
		private HTS_Label		m_Label ;						// label
		private HTS_SStreamSet	m_SSS ;						// set of state streams
		private HTS_PStreamSet	m_PSS ;						// set of PDF streams
		private HTS_GStreamSet	m_GSS ;						// set of generated parameter streams

		//-----------------------------------------------------------

		public HTS_Engine()
		{
			m_Condition			= new HTS_Condition() ;
			m_ModelSet			= new HTS_ModelSet() ;
			m_Label				= new HTS_Label() ;
			m_SSS				= new HTS_SStreamSet() ;
			m_PSS				= new HTS_PStreamSet() ;
			m_GSS				= new HTS_GStreamSet() ;

			Initialize() ;
		}

		public void Initialize()
		{
			m_Condition.Initialize() ;
			m_ModelSet.Initialize() ;
			m_Label.Initialize() ;
			m_SSS.Initialize() ;
			m_PSS.Initialize() ;
			m_GSS.Initialize() ;
		}

		// 複数の音響モデルをロード出来るらしい
		public bool Load( string tVoiceFilePath )
		{
			return Load( new string[]{ tVoiceFilePath } ) ;
		}

		public bool Load( string[] tVoiceFilePaths )
		{
			int num_voices = tVoiceFilePaths.Length ;

			int i, j ;
			int nstream ;
			double average_weight ;
			string option, find ;
			
			// reset engine
			Initialize() ;

			// load voices
			if( m_ModelSet.Load( tVoiceFilePaths ) == false )
			{
				Debug.LogError( "ModelSet.Load Error" ) ;
				Initialize() ;
				return false ;
			}

			nstream = m_ModelSet.GetNumericOfStream() ;
			average_weight = 1.0 / num_voices ;
			
			// global
			m_Condition.sampling_frequency	= m_ModelSet.GetSamplingFrequency() ;
			m_Condition.fperiod				= m_ModelSet.GetFramePeriod() ;

			m_Condition.msd_threshold		= new double[ nstream ] ;

			for( i  = 0 ; i <  nstream ; i ++ )
			{
				m_Condition.msd_threshold[ i ] = 0.5 ;
			}

			m_Condition.gv_weight = new double[ nstream ] ;

			for( i  = 0 ; i <  nstream ; i ++ )
			{
				m_Condition.gv_weight[ i ] = 1.0 ;
			}
			
			// spectrum
			option = m_ModelSet.GetOption( 0 ) ;

			find = GetOptionValue( option, "GAMMA=" ) ;
			if( string.IsNullOrEmpty( find ) == false )
			{
				int.TryParse( find, out m_Condition.stage ) ;
			}

			find = GetOptionValue( option, "LN_GAIN=" ) ;
			if( string.IsNullOrEmpty( find ) == false )
			{
				int b ;
				int.TryParse( find, out b ) ;
				m_Condition.use_log_gain = ( b == 1 ) ? true : false ;
			}

			find = GetOptionValue( option, "ALPHA=" ) ;
			if( find != null )
			{
				double.TryParse( find, out m_Condition.alpha ) ;
			}

			// interpolation weights
			m_Condition.duration_iw = new double[ num_voices ] ;
			for( i  = 0 ; i <  num_voices ; i ++ )
			{
				m_Condition.duration_iw[ i ] = average_weight ;
			}
			
			m_Condition.parameter_iw =  new double[ num_voices ][] ;
			for( i  = 0 ; i <  num_voices ; i ++ )
			{
				m_Condition.parameter_iw[ i ] = new double[ nstream ] ;
				for( j  = 0 ; j <  nstream ; j ++ )
				{
					m_Condition.parameter_iw[ i ][ j ] = average_weight ;
				}
			}

			m_Condition.gv_iw = new double[ num_voices ][] ;
			for( i  = 0 ; i <  num_voices ; i ++ )
			{
				m_Condition.gv_iw[ i ] = new double[ nstream ] ;
				for( j  = 0 ; j <  nstream ; j ++ )
				{
					m_Condition.gv_iw[ i ][ j ] = average_weight ;
				}
			}

			return true ;
		}

		//---------------------------------------------------------------------------

		private string GetOptionValue( string tString, string tLabel )
		{
			int i = tString.IndexOf( tLabel ) ;
			if( i <  0 )
			{
				return null ;
			}

			int p = i + tLabel.Length ;
			return tString.Substring( p, tString.Length - p ) ;
		}

		//---------------------------------------------------------------------------

		// HTS_Engine_set_sampling_frequency: set sampling frequency
		public void SetSamplingFrequency( int i )
		{
			if( i <  1 )
			{
				i = 1 ;
			}
			
			m_Condition.sampling_frequency = i ;
		}

		// HTS_Engine_get_sampling_frequency: get sampling frequency
		public int GetSamplingFrequency()
		{
			return m_Condition.sampling_frequency ;
		}

		// HTS_Engine_set_fperiod: set frame period
		public void SetFperiod( int i )
		{
			if( i <  1 )
			{
				i  = 1 ;
			}
			m_Condition.fperiod = i ;
		}

		// HTS_Engine_get_fperiod: get frame period
		public int GetFperiod()
		{
			return m_Condition.fperiod ;
		}

		// HTS_Engine_set_volume: set volume in db
		public void SetVolume( double f )
		{
			m_Condition.volume = Math.Exp( f * DB ) ;
		}

		// HTS_Engine_get_volume: get volume in db
		public double GetVolume()
		{
			return Math.Log( m_Condition.volume ) / DB ;
		}

		// HTS_Egnine_set_msd_threshold: set MSD threshold
		public void SetMsdThreshold( int stream_index, double f )
		{
			if( f <  0.0 )
			{
				f  = 0.0 ;
			}

			if( f >  1.0 )
			{
				f  = 1.0 ;
			}
			
			m_Condition.msd_threshold[ stream_index ] = f ;
		}

		// HTS_Engine_get_msd_threshold: get MSD threshold
		public double GetMsdThreshold( int stream_index )
		{
			return m_Condition.msd_threshold[ stream_index ] ;
		}

		// HTS_Engine_set_gv_weight: set GV weight
		public void SetGvWeight( int stream_index, double f )
		{
			if( f <  0.0 )
			{
				f  = 0.0 ;
			}

			m_Condition.gv_weight[ stream_index ] = f ;
		}

		// HTS_Engine_get_gv_weight: get GV weight
		public double GetGvWeight( int stream_index )
		{
			return m_Condition.gv_weight[ stream_index ] ;
		}
		
		// HTS_Engine_set_speed: set speech speed
		public void SetSpeed( double f )
		{
			if( f <  1.0E-06 )
			{
				f  = 1.0E-06 ;
			}

			m_Condition.speed = f ;
		}

		// HTS_Engine_set_phoneme_alignment_flag: set flag for using phoneme alignment in label
		public void SetPhonemeAlignmentFlag( bool b )
		{
			m_Condition.phoneme_alignment_flag = b ;
		}

		// HTS_Engine_set_alpha: set alpha
		public void SetAlpha( double f )
		{
			if( f <  0.0 )
			{
				f  = 0.0 ;
			}

			if( f >  1.0 )
			{
				f  = 1.0 ;
			}

			m_Condition.alpha = f ;
		}

		// HTS_Engine_get_alpha: get alpha
		public double GetAlpha()
		{
			return m_Condition.alpha ;
		}

		// HTS_Engine_set_beta: set beta
		public void SetBeta( double f )
		{
			if( f <  0.0 )
			{
				f  = 0.0 ;
			}

			if( f >  1.0 )
			{
				f  = 1.0 ;
			}

			m_Condition.beta = f ;
		}

		// HTS_Engine_get_beta: get beta
		public double GetBeta()
		{
			return m_Condition.beta ;
		}

		// HTS_Engine_add_half_tone: add half tone
		public void AddHalfTone( double f )
		{
			m_Condition.additional_half_tone = f ;
		}

		// HTS_Engine_set_duration_interpolation_weight: set interpolation weight for duration
		public void SetDurationInterpolationWeight( int voice_index, double f )
		{
			m_Condition.duration_iw[ voice_index ] = f ;
		}

		// HTS_Engine_get_duration_interpolation_weight: get interpolation weight for duration
		public double GetDurationInterpolationWeight( int voice_index )
		{
			return m_Condition.duration_iw[ voice_index ] ;
		}

		// HTS_Engine_set_parameter_interpolation_weight: set interpolation weight for parameter
		public void SetParameterInterpolationWeight( int voice_index, int stream_index, double f )
		{
			m_Condition.parameter_iw[ voice_index ][ stream_index ] = f ;
		}

		// HTS_Engine_get_parameter_interpolation_weight: get interpolation weight for parameter
		public double GetParameterInterpolationWeight( int voice_index, int stream_index )
		{
			return m_Condition.parameter_iw[ voice_index ][ stream_index ] ;
		}

		// HTS_Engine_set_gv_interpolation_weight: set interpolation weight for GV
		public void SetGvInterpolationWeight( int voice_index, int stream_index, double f )
		{
			m_Condition.gv_iw[ voice_index ][ stream_index ] = f ;
		}

		// HTS_Engine_get_gv_interpolation_weight: get interpolation weight for GV
		public double GetGvInterpolationWeight( int voice_index, int stream_index )
		{
			return m_Condition.gv_iw[ voice_index ][ stream_index ] ;
		}

		// HTS_Engine_get_total_state: get total number of state
		public int GetTotalState()
		{
		   return m_SSS.GetTotalState() ;
		}

		// HTS_Engine_set_state_mean: set mean value of state
		public void SetStateMean( int stream_index, int state_index, int vector_index, double f )
		{
			m_SSS.SetMean( stream_index, state_index, vector_index, f ) ;
		}

		// HTS_Engine_get_state_mean: get mean value of state
		public double GetStateMean( int stream_index, int state_index, int vector_index )
		{
			return m_SSS.GetMean( stream_index, state_index, vector_index ) ;
		}

		// HTS_Engine_get_state_duration: get state duration
		public int GetStateDuration( int state_index )
		{
			return m_SSS.GetDuration( state_index ) ;
		}

		// HTS_Engine_get_nvoices: get number of voices
		public int GetNumericOfVoices()
		{
			return m_ModelSet.GetNumericOfVoice() ;
		}

		// HTS_Engine_get_nstream: get number of stream
		public int GetNumericOfStream()
		{
			return m_ModelSet.GetNumericOfStream() ;
		}

		// HTS_Engine_get_nstate: get number of state
		public int GetNumericOfState()
		{
			return m_ModelSet.GetNumericOfState() ;
		}

		// HTS_Engine_get_fullcontext_label_format: get full context label format
		public string GetFullcontextLabelFormat()
		{
			return m_ModelSet.GetFullcontextLabelFormat() ;
		}

		// HTS_Engine_get_fullcontext_label_version: get full context label version
		public string GetFullcontextLabelVersion()
		{
			return m_ModelSet.GetFullcontextLabelVersion() ;
		}

		// HTS_Engine_get_total_frame: get total number of frame
		public int GetTotalFrame()
		{
			return m_GSS.GetTotalFrame() ;
		}

		// HTS_Engine_get_nsamples: get number of samples
		public int GetTotalSample()
		{
			return m_GSS.GetTotalSample() ;
		}

		// HTS_Engine_get_generated_parameter: output generated parameter
		public double GetGeneratedParameter( int stream_index, int frame_index, int vector_index )
		{
			return m_GSS.GetParameter( stream_index, frame_index, vector_index ) ;
		}

		// HTS_Engine_get_generated_speech: output generated speech
		public double GetGeneratedSpeech( int index )
		{
			return m_GSS.GetSpeech( index ) ;
		}

		//---------------------------------------------------------------------------

		// HTS_Engine_synthesize_from_strings: synthesize speech from strings
		public bool SynthesizeFromStrings( string[] tLines )
		{
			Refresh() ;
			
			m_Label.LoadFromStrings( m_Condition.sampling_frequency, m_Condition.fperiod, tLines ) ;

			return Synthesize() ;
		}

		// HTS_Engine_synthesize: synthesize speech
		public bool Synthesize()
		{
			if( GenerateStateSequence() != true )
			{
				Refresh() ;
				return false ;
			}

			if( GenerateParameterSequence() != true )
			{
				Refresh() ;
				return false ;
			}

			if( GenerateSampleSequence() != true )
			{
				Refresh() ;
				return false ;
			}

			return true ;
		}

		// HTS_Engine_generate_state_sequence: genereate state sequence (1st synthesis step)
		private bool GenerateStateSequence()
		{
			int i, state_index, model_index ;
			double f ;
			
			if( m_SSS.Create
			(
				m_ModelSet,
				m_Label,
				m_Condition.phoneme_alignment_flag,
				m_Condition.speed,
				m_Condition.duration_iw,
				m_Condition.parameter_iw,
				m_Condition.gv_iw
			) != true )
			{
				Refresh() ;
				return false ;
			}

			if( m_Condition.additional_half_tone != 0.0 )
			{
				state_index = 0 ;
				model_index = 0 ;

				for( i  = 0 ; i <  GetTotalState() ; i ++ )
				{
					f = GetStateMean( 1, i, 0 ) ;
					f += m_Condition.additional_half_tone * HALF_TONE ;
					
					if( f <  MIN_LF0 )
					{
						f  = MIN_LF0 ;
					}
					else
					if( f >  MAX_LF0 )
					{
						f  = MAX_LF0 ;
					}

					SetStateMean( 1, i, 0, f ) ;
					state_index ++ ;

					if( state_index >= GetNumericOfState() )
					{
						state_index = 0 ;
						model_index ++ ;
					}
				}
		   }

			return true ;
		}

		// HTS_Engine_generate_parameter_sequence: generate parameter sequence (2nd synthesis step)
		private bool GenerateParameterSequence()
		{
			return m_PSS.Create
			(
				m_SSS,
				m_Condition.msd_threshold,
				m_Condition.gv_weight
			) ;
		}

		// HTS_Engine_generate_sample_sequence: generate sample sequence (3rd synthesis step)
		private bool GenerateSampleSequence()
		{
			return m_GSS.Create
			(
				m_PSS,
				m_Condition.stage,
				m_Condition.use_log_gain,
				m_Condition.sampling_frequency,
				m_Condition.fperiod,
				m_Condition.alpha,
				m_Condition.beta,
				m_Condition.volume
			) ;
		}

		// HTS_Engine_save_generated_speech: save generated speech
		public float[] GetWaveData()
		{
			int i, tTotalSample = m_GSS.GetTotalSample() ;
			if( tTotalSample <= 0 )
			{
				return null ;
			}

			double x ;

			float[] tData = new float[ tTotalSample ] ;

			for( i  = 0 ; i <  tTotalSample ; i ++ )
			{
				x = m_GSS.GetSpeech( i ) ;

				if( x >   32767.0 )
				{
					x =  32767 ;
				}
				else
				if( x <  -32768.0 )
				{
					x = -32768 ;
				}

				tData[ i ] = ( float )( x / 32768 ) ;
			}

			return tData ;
		}

		public int GetWaveData( float[] tBuffer, int tOffset, int tLength )
		{
			int i, tTotalSample = m_GSS.GetTotalSample() ;
			if( tTotalSample <= 0 )
			{
				return 0 ;
			}

			if( tBuffer == null || tBuffer.Length <= 0 )
			{
				return 0 ;
			}

			if( tOffset <  0 )
			{
				tOffset  = 0 ;
			}
			else
			if( tOffset >= tBuffer.Length )
			{
				return 0 ;
			}

			if( tLength <= 0 )
			{
				tLength  = tBuffer.Length ;
			}
			
			if( ( tOffset + tLength ) >  tBuffer.Length )
			{
				tLength  = tBuffer.Length - tOffset ;
			}
			
			if( tTotalSample <  tLength )
			{
				tLength = tTotalSample ;
			}

			//----------------------------------

			double x ;

			for( i  = 0 ; i <  tLength ; i ++ )
			{
				x = m_GSS.GetSpeech( i ) ;

				if( x >   32767.0 )
				{
					x =  32767 ;
				}
				else
				if( x <  -32768.0 )
				{
					x = -32768 ;
				}

				tBuffer[ tOffset + i ] = ( float )( x / 32768 ) ;
			}

			return tTotalSample ;
		}

		// HTS_Engine_save_riff: save RIFF format file
		public byte[] GetWaveFile()
		{
			int i, tTotalSample = m_GSS.GetTotalSample() ;
			if( tTotalSample <= 0 )
			{
				return null ;
			}

			double x ;
			short t ;
			
			List<byte> tData = new List<byte>() ;

			byte[] data_01_04	= { ( byte )'R', ( byte )'I', ( byte )'F', ( byte )'F' } ;
			int data_05_08		= m_GSS.GetTotalSample() * sizeof( short ) + 36 ;
			byte[] data_09_12	= { ( byte )'W', ( byte )'A', ( byte )'V', ( byte )'E' } ;
			byte[] data_13_16	= { ( byte )'f', ( byte )'m', ( byte )'t', ( byte )' ' } ;
			int data_17_20		= 16 ;
			short data_21_22	= 1 ;	// PCM
			short data_23_24	= 1 ;	// monoral
			int data_25_28		= m_Condition.sampling_frequency ;
			int data_29_32		= m_Condition.sampling_frequency * sizeof( short ) ;
			short data_33_34	= sizeof( short ) ;
			short data_35_36	= ( short )( sizeof( short ) * 8 ) ;
			byte[] data_37_40	= { ( byte )'d', ( byte )'a', ( byte )'t', ( byte )'a' } ;
			int data_41_44		= tTotalSample * sizeof( short ) ;
			
			// write header
			FwriteLittleEndian( data_01_04,	4,					ref tData ) ;
			FwriteLittleEndian( data_05_08,	sizeof( int ),		ref tData ) ;
			FwriteLittleEndian( data_09_12,	4,					ref tData ) ;
			FwriteLittleEndian( data_13_16,	4,					ref tData ) ;
			FwriteLittleEndian( data_17_20,	sizeof( int ),		ref tData ) ;
			FwriteLittleEndian( data_21_22,	sizeof( short ),	ref tData ) ;
			FwriteLittleEndian( data_23_24,	sizeof( short ),	ref tData ) ;
			FwriteLittleEndian( data_25_28,	sizeof( int ),		ref tData ) ;
			FwriteLittleEndian( data_29_32,	sizeof( int ),		ref tData ) ;
			FwriteLittleEndian( data_33_34,	sizeof( short ),	ref tData ) ;
			FwriteLittleEndian( data_35_36,	sizeof( short ),	ref tData ) ;
			FwriteLittleEndian( data_37_40,	4,					ref tData ) ;
			FwriteLittleEndian( data_41_44,	sizeof( int ),		ref tData ) ;

			Debug.LogWarning( "Total Sample Count:" + m_GSS.GetTotalSample() ) ;

			// write data
			for( i  = 0 ; i <  tTotalSample ; i ++ )
			{
				x = m_GSS.GetSpeech( i ) ;

				if( x >   32767.0 )
				{
					t =  32767 ;
				}
				else
				if( x <  -32768.0 )
				{
					t = -32768 ;
				}
				else
				{
					t = ( short )x ;
				}

				FwriteLittleEndian( t, sizeof( short ),	ref tData ) ;
			}

			return tData.ToArray() ;
		}

		private void FwriteLittleEndian( byte[] data, int count, ref List<byte> buffer )
		{
			int i, l = count ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				buffer.Add( data[ i ] ) ;
			}
		}

		private void FwriteLittleEndian( int data, int size, ref List<byte> buffer )
		{
			int i, l = size ;
			byte b ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				b = ( byte )( ( data >> ( i * 8 ) ) & 0xFF ) ;
				buffer.Add( b ) ;
			}
		}

		// HTS_Engine_refresh: free model per one time synthesis
		public void Refresh()
		{
			// free generated parameter stream set
			m_GSS.Initialize() ;
			
			// free parameter stream set
			m_PSS.Initialize() ;

			// free state stream set
			m_SSS.Initialize() ;

			// free label list
			m_Label.Initialize() ;
		}
	}
}

