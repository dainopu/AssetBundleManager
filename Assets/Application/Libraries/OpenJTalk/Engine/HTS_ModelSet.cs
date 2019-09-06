using System ;
using System.Collections;
using System.Collections.Generic;
using System.Text ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_ModelSet
	{
		private string			m_HtsVoiceVersion ;			// version of HTS voice format
		private int				m_SamplingFrequency ;		// sampling frequency
		private int				m_FramePeriod ;				// frame period
		private int				m_NumericOfVoice ;			// # of HTS voices
		private int				m_NumericOfState ;			// # of HMM states
		private int				m_NumericOfStream ;			// # of streams
		private string			m_StreamType ;				// stream type
		private string			m_FullcontextLabelFormat ;	// fullcontext label format
		private string			m_FullcontextLabelVersion ;	// version of fullcontext label
		private HTS_Question	m_GvOffContext ;			// GV switch
		private string[]		m_Option ;					// options for each stream

		private HTS_Model[]		m_Duration ;				// duration PDFs and trees
		private HTS_Window[]	m_Window ;				// window coefficients for delta
		private HTS_Model[][]	m_Stream ;				// parameter PDFs and trees
		private HTS_Model[][]	m_Gv ;					// GV PDFs and trees

		//-----------------------------------------------------------

		public HTS_ModelSet()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_HtsVoiceVersion			= null ;
			m_SamplingFrequency			= 0 ;
			m_FramePeriod				= 0 ;
			m_NumericOfVoice			= 0 ;
			m_NumericOfState			= 0 ;
			m_NumericOfStream			= 0 ;
			m_StreamType				= null ;
			m_FullcontextLabelFormat	= null ;
			m_FullcontextLabelVersion	= null ;
			m_GvOffContext				= null ;
			m_Option					= null ;

			m_Duration					= null ;
			m_Window					= null ;
			m_Stream					= null ;
			m_Gv						= null ;
		}

		//-----------------------------------------------------------

		// 複数の音響モデルを同時にロード出来る
		public bool Load( string[] voices )
		{
			int tNumericOfVoice = voices.Length ;

			int i, j, k, s, e ;
			bool error		= false ;
			HTS_File fp		= null ;
			int matched_size ;
			
			string[]	stream_type_list = null ;
			
			int[]	vector_length	= null ;
			bool[]	is_msd			= null ;
			int[]	num_windows		= null ;
			bool[]	use_gv			= null ;
			
			string	gv_off_context	= null ;
			
			// temporary values
			string		tHtsVoiceVersion ;
			int			temp_sampling_frequency ;
			int			temp_frame_period ;
			int			temp_num_states ;
			int			temp_num_streams ;
			string		temp_stream_type ;
			string		temp_fullcontext_format ;
			string		temp_fullcontext_version ;
			
			string		temp_gv_off_context ;
			
			int[]		temp_vector_length ;
			bool[]		temp_is_msd ;
			int[]		temp_num_windows ;
			bool[]		temp_use_gv ;
			string[]	temp_option ;
			
			string		temp_duration_pdf ;
			string		temp_duration_tree ;
			string[][]	temp_stream_win ;
			string[]	temp_stream_pdf ;
			string[]	temp_stream_tree ;
			string[]	temp_gv_pdf ;
			string[]	temp_gv_tree ;
			
			int start_of_data ;
			HTS_File	pdf_fp				= null ;
			HTS_File	tree_fp				= null ;
			HTS_File[]	win_fp				= null ;
			HTS_File	gv_off_context_fp	= null ;
			
			Initialize() ;
			
			if( voices == null || tNumericOfVoice <  1 )
			{
				return false ;
			}
			
			m_NumericOfVoice = tNumericOfVoice ;
			
			for( i  = 0 ; i <  m_NumericOfVoice && error == false ; i ++ )
			{
				// open file
				fp = HTS_File.Open( voices[ i ] ) ;
				if( fp == null )
				{
					error = true ;
					break ;
				}

				// reset GLOBAL options
				tHtsVoiceVersion			= null ;
				temp_sampling_frequency		= 0	 ;
				temp_frame_period			= 0 ;
				temp_num_states				= 0 ;
				temp_num_streams			= 0 ;
				temp_stream_type			= null ;
				temp_fullcontext_format		= null ;
				temp_fullcontext_version	= null ;
				temp_gv_off_context			= null ;

				// 0x0D かもしれない
				string tToken = fp.GetTokenFromFpWithSeparator( 0x0A ) ;
				string tValue ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					error = true ;
					break ;					
				}


				// load GLOBAL options
				if( tToken != "[GLOBAL]" )
				{
					error = true ;
					break ;
				}

				while( true )
				{
					// 0x0D かもしれない
					tToken = fp.GetTokenFromFpWithSeparator( 0x0A ) ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						error = true ;
						break ;
					}

					if( tToken == "[STREAM]" )
					{
						break ;
					}
					else
					if( MatchHeadString( tToken, "HTS_VOICE_VERSION:", out tValue ) == true )
					{
						tHtsVoiceVersion = tValue ;
					}
					else
					if( MatchHeadString( tToken, "SAMPLING_FREQUENCY:", out tValue ) == true )
					{
						int.TryParse( tValue, out temp_sampling_frequency ) ;
					}
					else
					if( MatchHeadString( tToken, "FRAME_PERIOD:", out tValue ) == true )
					{
						int.TryParse( tValue, out temp_frame_period ) ;
					}
					else
					if( MatchHeadString( tToken, "NUM_STATES:", out tValue ) == true )
					{
						int.TryParse( tValue, out temp_num_states ) ;
					}
					else
					if( MatchHeadString( tToken, "NUM_STREAMS:", out tValue ) == true )
					{
						int.TryParse( tValue, out temp_num_streams ) ;
					}
					else
					if( MatchHeadString( tToken, "STREAM_TYPE:", out tValue ) == true )
					{
						temp_stream_type = tValue ;
					}
					else
					if( MatchHeadString( tToken, "FULLCONTEXT_FORMAT:", out tValue ) == true )
					{
						temp_fullcontext_format = tValue ;
					}
					else
					if( MatchHeadString( tToken, "FULLCONTEXT_VERSION:", out tValue ) == true )
					{
						temp_fullcontext_version = tValue ;
					}
					else
					if( MatchHeadString( tToken, "GV_OFF_CONTEXT:", out tValue ) == true )
					{
						temp_gv_off_context = tValue ;
					}
					else
					if( MatchHeadString( tToken, "COMMENT:", out tValue ) == true )
					{
						if( string.IsNullOrEmpty( tValue ) == false )
						{
							Debug.Log( "COMMENT:" + tValue ) ;
						}
					}
					else
					{
						Debug.LogError( "HTS_ModelSet_load: Unknown option " + tToken + "." ) ;
					}
				}

				// check GLOBAL options
				if( i == 0 )
				{
					// 最初の音響モデルの共通パラメータ部分を全音響モデルの正式な共通パラメータとする
					m_HtsVoiceVersion			= tHtsVoiceVersion ;
					m_SamplingFrequency			= temp_sampling_frequency ;
					m_FramePeriod				= temp_frame_period ;
					m_NumericOfState			= temp_num_states ;
					m_NumericOfStream			= temp_num_streams ;
					m_StreamType				= temp_stream_type ;
					m_FullcontextLabelFormat	= temp_fullcontext_format ;
					m_FullcontextLabelVersion	= temp_fullcontext_version ;

					gv_off_context				= temp_gv_off_context ;
				}
				else
				{
					// ２つ目以降の音響モデル(共通パラメータに違いがあれば不正なものとする)
					if( m_HtsVoiceVersion != tHtsVoiceVersion )
					{
						error = true ;
					}
					if( m_SamplingFrequency != temp_sampling_frequency )
					{
						error = true ;
					}
					if( m_FramePeriod != temp_frame_period )
					{
						error = true ;
					}
					if( m_NumericOfState != temp_num_states )
					{
						error = true ;
					}
					if( m_NumericOfStream != temp_num_streams )
					{
						error = true ;
					}
					if( m_StreamType != temp_stream_type )
					{
						error = true ;
					}
					if( m_FullcontextLabelFormat != temp_fullcontext_format )
					{
						error = true ;
					}
					if( m_FullcontextLabelVersion != temp_fullcontext_version )
					{
						error = true ;
					}
					if( gv_off_context != temp_gv_off_context )
					{
						error = true ;
					}

					tHtsVoiceVersion  = null ;
					temp_stream_type  = null ;
					temp_fullcontext_format  = null ;
					temp_fullcontext_version  = null ;
					temp_gv_off_context  = null ;
				}

				// find stream names
				if( i == 0 )
				{
					// 最初の音響モデルのみチェックする
					stream_type_list = m_StreamType.Split( ',' ) ;
					if( stream_type_list == null || stream_type_list.Length != m_NumericOfStream )
					{
						error = true ;
					}
				}

				if( error != false )
				{
					if( fp != null )
					{
						fp.Close() ;
						fp = null ;
					}
					break ;
				}
			
				// 全音響モデルで共通のパラメータ(num_streams)を使って作業領域を確保する

				// reset STREAM options
				temp_vector_length	= new    int[ m_NumericOfStream ] ;
				temp_is_msd			= new   bool[ m_NumericOfStream ] ;
				temp_num_windows	= new    int[ m_NumericOfStream ] ;
				temp_use_gv			= new   bool[ m_NumericOfStream ] ;
				temp_option			= new string[ m_NumericOfStream ] ;

				// load STREAM options
				while( true )
				{
					// 0x0D の可能性あり
					tToken = fp.GetTokenFromFpWithSeparator( 0x0A ) ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						error = true ;
						break ;
					}

					if( tToken == "[POSITION]" )
					{
						break ;
					}
					else
					if( MatchHeadString( tToken, "VECTOR_LENGTH[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									int.TryParse( tValue, out temp_vector_length[ j ] ) ;
									break;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "IS_MSD[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_is_msd[ j ] = ( tValue == "1" ) ? true : false ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "NUM_WINDOWS[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									int.TryParse( tValue, out temp_num_windows[ j ] ) ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "USE_GV[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_use_gv[ j ] = ( tValue == "1" ) ? true : false ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "OPTION[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_option[ j ] = tValue ;
									break ;
								}
							}
						}
					}
					else
					{
						Debug.LogError( "HTS_ModelSet_load: Unknown option " + tToken ) ;
					}
				}
				
				// check STREAM options
				if( i == 0 )
				{
					// 最初の音響モデル
					vector_length	= temp_vector_length ;
					is_msd			= temp_is_msd ;
					num_windows		= temp_num_windows ;
					use_gv			= temp_use_gv ;
					m_Option		= temp_option ;
				}
				else
				{
					// ２つ目以降の音響モデル(値が最初の音響モデルと同じになっているかチェックする)
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( vector_length[ j ] != temp_vector_length[ j ] )
						{
							error = true ;
						}
					}
				
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( is_msd[ j ] != is_msd[ j ] )
						{
							error = true ;
						}
					}
				
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( num_windows[ j ] != temp_num_windows[ j ] )
						{
						   error = true ;
						}
					}
				
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( use_gv[ j ] != temp_use_gv[ j ] )
						{
							error = true ;
						}
					}
				
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( m_Option[ j ] != temp_option[ j ] )
						{
							error = true ;
						}
					}
				
					temp_vector_length	= null ;
					temp_is_msd			= null ;
					temp_num_windows	= null ;
					temp_use_gv			= null ;

					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						if( temp_option[ j ] != null )
						{
							temp_option[ j ]  = null ;
						}
					}
					temp_option = null ;
				}
				if( error != false )
				{
					if( fp != null )
					{
						fp.Close() ;
						fp = null ;
					}
					break ;
				}

				// reset POSITION
				temp_duration_pdf	= null ;
				temp_duration_tree	= null ;
				temp_stream_win		= new string[ m_NumericOfStream ][] ;
				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					temp_stream_win[ j ] = new string[ num_windows[ j ] ] ;
				}
				temp_stream_pdf		= new string[ m_NumericOfStream ] ;
				temp_stream_tree	= new string[ m_NumericOfStream ] ;
				temp_gv_pdf			= new string[ m_NumericOfStream ] ;
				temp_gv_tree		= new string[ m_NumericOfStream ] ;

				// load POSITION
				while( true )
				{
					tToken = fp.GetTokenFromFpWithSeparator( 0x0A ) ;

					if( string.IsNullOrEmpty( tToken ) == true )
					{
						error = true ;
						break ;
					}

					if(	tToken == "[DATA]" )
					{
						break ;
					}
					else
					if( MatchHeadString( tToken, "DURATION_PDF:", out tValue ) == true )
					{
						temp_duration_pdf = tValue ;
					}
					else
					if( MatchHeadString( tToken, "DURATION_TREE:", out tValue ) == true )
					{
						temp_duration_tree = tValue ;
					}
					else
					if( MatchHeadString( tToken, "STREAM_WIN[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									string[] tValues = tValue.Split( ',' ) ;
									if( tValues != null && tValues.Length == num_windows[ j ] )
									{
										for( k  = 0 ; k <  num_windows[ j ] ; k ++ )
										{
											temp_stream_win[ j ][ k ] = tValues[ k ] ;
										}
									}
									else
									{
										error = true ;
									}
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "STREAM_PDF[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_stream_pdf[ j ] = tValue ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "STREAM_TREE[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_stream_tree[ j ] = tValue ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "GV_PDF[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_gv_pdf[ j ] = tValue ;
									break ;
								}
							}
						}
					}
					else
					if( MatchHeadString( tToken, "GV_TREE[", out tValue ) == true )
					{
						string tLabel ;
						if( GetLabelAndValue( tValue, out tLabel, out tValue ) == true )
						{
							for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
							{
								if( stream_type_list[ j ] == tLabel )
								{
									temp_gv_tree[ j ] = tValue ;
									break ;
								}
							}
						}
					}
					else
					{
						Debug.LogError( "HTS_ModelSet_load: Unknown option " + tToken ) ;
					}
				}

				// check POSITION
				if( temp_duration_pdf == null )
				{
					error = true ;
				}

				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					for( k  = 0 ; k <  num_windows[ j ] ; k ++ )
					{
						if( temp_stream_win[ j ][ k ] == null )
						{
							error = true ;
						}
					}
				}
				
				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					if( temp_stream_pdf[ j ] == null )
					{
						error = true ;
					}
				}
				
				// prepare memory
				if( i == 0 )
				{
					// 最初の音響モデル
					m_Duration = new HTS_Model[ m_NumericOfVoice ] ;
					for( j  = 0 ; j <  m_NumericOfVoice ; j ++ )
					{
						m_Duration[ j ] = new HTS_Model() ;
					}
					
					m_Window = new HTS_Window[ m_NumericOfStream ] ;
					for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
					{
						m_Window[ j ] = new HTS_Window() ;
					}

					m_Stream = new HTS_Model[ m_NumericOfVoice ][] ;
					for( j  = 0 ; j <  m_NumericOfVoice ; j ++ )
					{
						m_Stream[ j ] = new HTS_Model[ m_NumericOfStream ] ;
						for( k  = 0 ; k <  m_NumericOfStream ; k ++ )
						{
							m_Stream[ j ][ k ] = new HTS_Model() ;
						}
					}

					m_Gv = new  HTS_Model[ m_NumericOfVoice ][] ;
					for( j  = 0 ; j <  m_NumericOfVoice ; j ++ )
					{
						m_Gv[ j ] = new HTS_Model[ m_NumericOfStream ] ;
						for( k  = 0 ; k <  m_NumericOfStream ; k ++ )
						{
							m_Gv[ j ][ k ] = new HTS_Model() ;
						}
					}
				}
				start_of_data = fp.Tell() ;

				// load duration
				pdf_fp			= null ;
				tree_fp			= null ;

				if( GetDuration( temp_duration_pdf, out s, out e ) == true )
				{
					fp.Seek( s, HTS_File.SEEK_CUR ) ;
					pdf_fp = HTS_File.Create( fp, e - s + 1 ) ;
					fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
				}

				if( GetDuration( temp_duration_tree, out s, out e ) == true )
				{
					fp.Seek( s, HTS_File.SEEK_CUR ) ;
					tree_fp = HTS_File.Create( fp, e - s + 1 ) ;
					fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
				}

				// ここの音響モデルのパラメータを展開する
				if( m_Duration[ i ].Load( pdf_fp, tree_fp, m_NumericOfState, 1, false ) != true )
				{
					error = true ;
				}

				pdf_fp.Close() ;
				tree_fp.Close() ;

				// load windows(窓は複数の音響モデルで共有？)
				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					win_fp = new HTS_File[ num_windows[ j ] ] ;
					for( k  = 0 ; k <  num_windows[ j ] ; k ++ )
					{
						matched_size = 0 ;
						if( GetTokenFromStringWithSeparator( temp_stream_win[ j ][ k ], ref matched_size, out tToken, '-' ) == true )
						{
							int.TryParse( tToken, out s ) ;
							tToken = temp_stream_win[ j ][ k ].Substring( matched_size ) ;
							int.TryParse( tToken, out e ) ;

							fp.Seek( s, HTS_File.SEEK_CUR ) ;
							win_fp[ k ] = HTS_File.Create( fp, e - s + 1 ) ;
							fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
						}
					}

					if( m_Window[ j ].Load( win_fp, num_windows[ j ] ) == false )
					{
						Debug.LogError( "Model.Load Error : Window" ) ;
						error = true ;
					}

//					for( k  = 0 ; k <  num_windows[ j ] ; k ++ )
//					{
//						if( win_fp[ k ] != null )
//						{
//							win_fp[ k ].Close() ;
//						}
//					}

					win_fp = null ;
				}

				// load streams
				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					pdf_fp			= null ;
					tree_fp			= null ;

					matched_size	= 0 ;
					if( GetTokenFromStringWithSeparator( temp_stream_pdf[ j ], ref matched_size, out tToken, '-' ) == true )
					{
						int.TryParse( tToken, out s ) ;
						tToken = temp_stream_pdf[ j ].Substring( matched_size ) ;
						int.TryParse( tToken, out e ) ;

						fp.Seek( s, HTS_File.SEEK_CUR ) ;
						pdf_fp = HTS_File.Create( fp, e - s + 1 ) ;
						fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
					}

					matched_size = 0 ;
					if( GetTokenFromStringWithSeparator( temp_stream_tree[ j ], ref matched_size, out tToken, '-') == true )
					{
						int.TryParse( tToken, out s ) ;
						tToken = temp_stream_tree[ j ].Substring( matched_size ) ;
						int.TryParse( tToken, out e ) ;

						fp.Seek( s, HTS_File.SEEK_CUR ) ;
						tree_fp = HTS_File.Create( fp, e - s + 1 ) ;
						fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
					}

					// i は音響モデルインデックス番号
					if( m_Stream[ i ][ j ].Load( pdf_fp, tree_fp, vector_length[ j ], num_windows[ j ], is_msd[ j ] ) != true )
					{
						Debug.LogError( "Model.Load Error" ) ;
						error = true ;
					}

					if( pdf_fp != null )
					{
						pdf_fp.Close() ;
					}
					if( tree_fp != null )
					{
						tree_fp.Close() ;
					}

					// 複数の音響モデルのロードループ閉じ
				}

				// load GVs
				for( j  = 0 ; j <  m_NumericOfStream ; j ++ )
				{
					pdf_fp			= null ;
					tree_fp			= null ;

					matched_size	= 0 ;
					if( GetTokenFromStringWithSeparator( temp_gv_pdf[ j ], ref matched_size, out tToken, '-' ) == true )
					{
						int.TryParse( tToken, out s ) ;
						tToken = temp_gv_pdf[ j ].Substring( matched_size ) ;
						int.TryParse( tToken, out e ) ;

						fp.Seek( s, HTS_File.SEEK_CUR ) ;
						pdf_fp = HTS_File.Create( fp, e - s + 1 ) ;
						fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
					}

					matched_size = 0 ;
					if( GetTokenFromStringWithSeparator( temp_gv_tree[ j ], ref matched_size, out tToken, '-' ) == true )
					{
						int.TryParse( tToken, out s ) ;
						tToken = temp_gv_tree[ j ].Substring( matched_size ) ;
						int.TryParse( tToken, out e ) ;

						fp.Seek( s, HTS_File.SEEK_CUR ) ;
						tree_fp = HTS_File.Create( fp, e - s + 1 ) ;
						fp.Seek( start_of_data, HTS_File.SEEK_SET ) ;
					}

					if( use_gv[ j ] == true )
					{
						if( m_Gv[ i ][ j ].Load( pdf_fp, tree_fp, vector_length[ j ], 1, false ) != true )
						{
							Debug.LogError( "Model.Load Error" ) ;
							error = true ;
						}
					}

					if( pdf_fp != null )
					{
						pdf_fp.Close() ;
					}
					if( tree_fp != null )
					{
						tree_fp.Close() ;
					}
				}
				
				// free
				temp_duration_pdf	= null ;
				temp_duration_tree	= null ;
				temp_stream_win		= null ;
				temp_stream_pdf		= null ;
				temp_stream_tree	= null ;
				temp_gv_pdf			= null ;
				temp_gv_tree		= null ;

				// fclose
				if( fp != null )
				{
					fp.Close() ;
					fp = null ;
				}

				if( error != false )
				{
					Debug.LogError( "Error" ) ;
					break ;
				}
			}

			if( gv_off_context != null )
			{
//				sprintf( buff1, "GV-Off { %s }", gv_off_context ) ;
				string ts = "GV-Off { " + gv_off_context + " }" ;
//				int tl = tb.Length ;
//				if( tl >  ( buff1.Length - 1 ) )
//				{
//					tl  = ( buff1.Length - 1 ) ;
//				}
//				Array.Copy( tb, 0, buff1, 0, tl ) ;
//				buff1[ tl ] = 0 ;
				
//				gv_off_context_fp = HTS_File.OpenFromData( buff1, HTS_Misc.Strlen( buff1 ) + 1 ) ;
				gv_off_context_fp = HTS_File.Create( ts ) ;

				m_GvOffContext = new HTS_Question() ;
				if( m_GvOffContext.Load( gv_off_context_fp ) == false )
				{
					Debug.LogError( "gv_off_context.Load Error : " + ts ) ;
				}

				gv_off_context_fp.Close() ;
				gv_off_context = null ;
			}

			stream_type_list	= null ;
			vector_length		= null ;
			is_msd				= null ;
			num_windows			= null ;
			use_gv				= null ;
			
			return !error ;
		}

		// HTS_ModelSet_get_nstream: get number of stream
		public int GetNumericOfStream()
		{
			return m_NumericOfStream ;
		}

		// HTS_ModelSet_get_sampling_frequency: get sampling frequency of HTS voices
		public int GetSamplingFrequency()
		{
			return m_SamplingFrequency ;
		}

		// HTS_ModelSet_get_fperiod: get frame period of HTS voices
		public int GetFramePeriod()
		{
			return m_FramePeriod ;
		}

		// HTS_ModelSet_get_fperiod: get stream option
		public string GetOption( int tStreamIndex )
		{
			return m_Option[ tStreamIndex ] ;
		}

		// HTS_ModelSet_get_nvoices: get number of stream
		public int GetNumericOfVoice()
		{
		   return m_NumericOfVoice ;
		}

		// HTS_ModelSet_get_nstate: get number of state
		public int GetNumericOfState()
		{
		   return m_NumericOfState ;
		}

		// HTS_Engine_get_fullcontext_label_format: get full-context label format
		public string GetFullcontextLabelFormat()
		{
			return m_FullcontextLabelFormat ;
		}

		// HTS_Engine_get_fullcontext_label_version: get full-context label version
		public string GetFullcontextLabelVersion()
		{
		   return m_FullcontextLabelVersion ;
		}

		// HTS_ModelSet_use_gv: get GV flag
		public bool UseGv( int stream_index )
		{
			if( m_Gv[ 0 ][ stream_index ].vector_length != 0 )
			{
				return true ;
			}
			else
			{
				return false ;
			}
		}

		// HTS_ModelSet_get_vector_length: get vector length
		public int GetVectorLength( int stream_index )
		{
			return m_Stream[ 0 ][ stream_index ].vector_length ;
		}

		// HTS_ModelSet_is_msd: get MSD flag
		public bool IsMsd( int stream_index )
		{
			return m_Stream[ 0 ][ stream_index ].is_msd ;
		}

		// HTS_ModelSet_get_window_size: get dynamic window size
		public int GetWindowSize( int stream_index )
		{
			return m_Window[ stream_index ].size ;
		}

		// HTS_ModelSet_get_window_left_width: get left width of dynamic window
		public int GetWindowLeftWidth( int stream_index, int window_index )
		{
			return m_Window[ stream_index ].l_width[ window_index ] ;
		}

		// HTS_ModelSet_get_window_right_width: get right width of dynamic window
		public int GetWindowRightWidth( int stream_index, int window_index )
		{
			return m_Window[ stream_index ].r_width[ window_index ] ;
		}

		// HTS_ModelSet_get_window_coefficient: get coefficient of dynamic window
		public double GetWindowCoefficient( int stream_index, int window_index, int coefficient_index )
		{
			// オフセットのズレに対応
			int tOffset = m_Window[ stream_index ].coefficient_offset[ window_index ] ;
			return m_Window[ stream_index ].coefficient[ window_index ][ tOffset + coefficient_index ] ;
		}

		// HTS_ModelSet_get_window_max_width: get max width of dynamic window
		public int GetWindowMaxWidth( int stream_index )
		{
			return m_Window[ stream_index ].max_width ;
		}

		// HTS_ModelSet_get_gv: get GV using interpolation weight
		public void GetGv( int stream_index, string tString, double[][] iw, double[] mean, double[] vari )
		{
			int i ;
			int len = m_Stream[ 0 ][ stream_index ].vector_length ;
			
			for( i  = 0 ; i <  len ; i ++ )
			{
				mean[ i ] = 0.0 ;
				vari[ i ] = 0.0 ;
			}
			
			for( i  = 0 ; i <  m_NumericOfVoice ; i ++ )
			{
				if( iw[ i ][ stream_index ] != 0.0 )
				{
					m_Gv[ i ][ stream_index ].AddParameter( 2, tString, mean, 0, vari, 0, null, 0, iw[ i ][ stream_index ] ) ;
				}
			}
		}

		// HTS_ModelSet_get_gv_flag: get GV flag
		public bool GetGvFlag( string tString )
		{
			if( m_GvOffContext == null )
			{
				return true ;
			}
			else
			if( m_GvOffContext.Match( tString ) == true )
			{
				return false ;
			}
			else
			{
				return true ;
			}
		}

		// HTS_ModelSet_get_duration: get duration using interpolation weight
		public void GetDuration( string tString, double[] iw, double[] mean, int om, double[] vari, int ov )
		{
			int i ;
			int len = m_NumericOfState ;
			
			for( i  = 0 ; i <  len ; i ++ )
			{
				mean[ om + i ] = 0.0 ;
				vari[ ov + i ] = 0.0 ;
			}

			for( i  = 0 ; i <  m_NumericOfVoice ; i ++ )
			{
				if( iw[ i ] != 0.0 )
				{
					m_Duration[ i ].AddParameter( 2, tString, mean, om, vari, ov, null, 0, iw[ i ] ) ;
				}
			}
		}

		// HTS_ModelSet_get_duration_index: get duration PDF & tree index
/*		public void GetDurationIndex( int voice_index, string tString, ref int tree_index, ref int pdf_index )
		{
			this.duration[ voice_index ].GetIndex( 2, tString, ref tree_index, ref pdf_index ) ;
		}*/

		// HTS_ModelSet_get_parameter_index: get paramter PDF & tree index
		public void GetParameterIndex( int voice_index, int stream_index, int state_index, string tString, ref int tree_index, ref int pdf_index )
		{
			m_Stream[ voice_index ][ stream_index ].GetIndex( state_index, tString, ref tree_index, ref pdf_index ) ;
		}

		// HTS_ModelSet_get_parameter: get parameter using interpolation weight
		public void GetParameter( int stream_index, int state_index, string tString, double[][] iw, double[] mean, int om, double[] vari, int ov, double[] msd, int o_msd )
		{
			int i ;
			int len = m_Stream[ 0 ][ stream_index ].vector_length * m_Stream[ 0 ][ stream_index ].num_windows ;
			
			for( i  = 0 ; i <  len ; i ++ )
			{
				mean[ om + i ] = 0.0 ;
				vari[ ov + i ] = 0.0 ;
			}

			if( msd != null )
			{
				msd[ o_msd ] = 0.0 ;
			}

			for( i  = 0 ; i <  m_NumericOfVoice ; i ++ )
			{
				if( iw[ i ][ stream_index ] != 0.0 )
				{
					m_Stream[ i ][ stream_index ].AddParameter( state_index, tString, mean, om, vari, ov, msd, o_msd, iw[ i ][ stream_index ] ) ;
				}
			}
		}

		//---------------------------------------------------------------------------

		private bool MatchHeadString( string tToken, string tWord, out string oValue )
		{
			oValue = "" ;
			if( tToken.Contains( tWord ) == true )
			{
				oValue = tToken.Replace( tWord, "" ) ;
				return true ;
			}

			return false ;
		}

		private bool GetLabelAndValue( string tString, out string tLabel, out string tValue )
		{
			tLabel = null ;
			tValue = null ;

			int p = tString.IndexOf( "]" ) ;
			if( p <  0 )
			{
				return false ;
			}

			tLabel = tString.Substring( 0, p ) ;

			p ++ ;
			tString = tString.Substring( p, tString.Length - p ) ;
			if( tString.Length >  0 && tString[ 0 ] == ':' )
			{
				tValue = tString.Substring( 1, tString.Length - 1 ) ; 
				return true ;
			}

			return false ;
		}

		private bool GetDuration( string tString, out int s, out int e )
		{
			s = 0 ;
			e = 0 ;

			if( tString.IndexOf( "-" ) <  0 )
			{
				return false ;
			}

			string[] v = tString.Split( '-' ) ;
			if( v == null || v.Length != 2 )
			{
				return false ;
			}

			int.TryParse( v[ 0 ], out s ) ;
			int.TryParse( v[ 1 ], out e ) ;

			return true ;
		}

		// HTS_get_token_from_string_with_separator: get token from string with specified separator
		private bool GetTokenFromStringWithSeparator( string tString, ref int rIndex, out string tToken, char tSeparator )
		{
			tToken = "" ;

			char c ;
			
			if( string.IsNullOrEmpty( tString ) == true )
			{
				return false ;
			}
			
			if( rIndex >= tString.Length )
			{
				return false ;
			}

			c = tString[ rIndex ] ;

			// 区切り記号じゃない位置まで移動
			while( c == tSeparator )
			{
				if( rIndex >= tString.Length )
				{
					return false ;
				}

				rIndex ++ ;
				c = tString[ rIndex ] ;
			}

			List<char> tWord = new List<char>() ;

			while( c != tSeparator )
			{
				tWord.Add( c ) ;

				rIndex ++ ;

				if( rIndex >= tString.Length )
				{
					break ;
				}

				c = tString[ rIndex ] ;
			}

			if( rIndex <  tString.Length )
			{
				rIndex ++ ;	// 区切り記号の次の位置まで移動させておく
			}
			
			if( tWord.Count >  0 )
			{
				tToken = new string( tWord.ToArray() ) ;

				return true ;
			}
			else
			{
				return false ;
			}
		}
	}
}

