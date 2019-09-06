using System ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine ;


namespace HTS_Engine_API
{
	public class HTS_SStreamSet
	{
		private HTS_SStream[]	sstream ;			// state streams
		private int				nstream ;			// # of streams
		private int				nstate ;			// # of states
		private int[]			duration ;			// duration sequence
		private int				total_state ;		// total state
		private int				total_frame ;		// total frame

		//-----------------------------------------------------------

		public HTS_SStreamSet()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			this.nstream		= 0 ;
			this.nstate			= 0 ;
			this.sstream		= null ;
			this.duration		= null ;
			this.total_state	= 0 ;
			this.total_frame	= 0 ;
		}

		// HTS_SStreamSet_create: parse label and determine state duration
		public bool Create( HTS_ModelSet ms, HTS_Label label, bool phoneme_alignment_flag, double speed, double[] duration_iw, double[][] parameter_iw, double[][] gv_iw )
		{
			int i, j, k ;
			double temp ;
			int shift ;
			int state ;
			HTS_SStream sst ;
			double[] duration_mean, duration_vari ;
			double frame_length ;
			int next_time ;
			int next_state ;
			
			Debug.LogWarning( "======= SSS Create Start : " + label.GetSize() + " / " + ms.GetNumericOfVoice() ) ;


			if( label.GetSize() == 0 )
			{
				return false ;
			}
			
			// check interpolation weights
			for( i  = 0, temp = 0.0; i <  ms.GetNumericOfVoice() ; i ++ )
			{
				temp += duration_iw[ i ] ;
			}

			if( temp == 0.0 )
			{
				return false ;
			}
			else
			if( temp != 1.0 )
			{
				for( i  = 0 ; i <  ms.GetNumericOfVoice() ; i ++ )
				{
					if( duration_iw[ i ] != 0.0 )
					{
						duration_iw[ i ] /= temp ;
					}
				}
			}

			for( i  = 0 ; i <  ms.GetNumericOfStream() ; i ++ )
			{
				for( j  = 0, temp = 0.0 ; j <  ms.GetNumericOfVoice() ; j ++ )
				{
					temp += parameter_iw[ j ][ i ] ;
				}

				if( temp == 0.0 )
				{
					return false ;
				}
				else
				if( temp != 1.0 )
				{
					for( j  = 0 ; j <  ms.GetNumericOfVoice() ; j ++ )
					{
						if( parameter_iw[ j ][ i ] != 0.0 )
						{
							parameter_iw[ j ][ i ] /= temp ;
						}
					}
				}

				if( ms.UseGv( i ) == true )
				{
					for( j  = 0, temp = 0.0 ; j <  ms.GetNumericOfVoice() ; j ++ )
					{
						temp += gv_iw[ j ][ i ] ;
					}

					if( temp == 0.0 )
					{
						return false ;
					}
					else
					if( temp != 1.0 )
					{
						for( j  = 0 ; j <  ms.GetNumericOfVoice() ; j ++ )
						{
							if( gv_iw[ j ][ i ] != 0.0 )
							{
								gv_iw[ j ][ i ] /= temp ;
							}
						}
					}
				}
			}

			// initialize state sequence
			this.nstate			= ms.GetNumericOfState() ;
			this.nstream		= ms.GetNumericOfStream() ;
			this.total_frame	= 0;
			this.total_state	= label.GetSize() * this.nstate ;
			this.duration		= new int[ this.total_state ] ;
			this.sstream		= new HTS_SStream[ this.nstream ] ;
			
			for( i  = 0 ; i <  this.nstream ; i ++ )
			{
				this.sstream[ i ] = new HTS_SStream() ;

				sst = this.sstream[ i ] ;
				sst.vector_length = ms.GetVectorLength( i ) ;
				sst.mean = new double[ this.total_state ][] ;
				sst.vari = new double[ this.total_state ][] ;

				if( ms.IsMsd( i ) == true )
				{
					sst.msd = new double[ this.total_state ] ;
				}
				else
				{
					sst.msd = null ;
				}
				
				for( j  = 0 ; j < this.total_state ; j ++ )
				{
					sst.mean[ j ] = new double[ sst.vector_length * ms.GetWindowSize( i ) ] ;
					sst.vari[ j ] = new double[ sst.vector_length * ms.GetWindowSize( i ) ] ;
				}

				if( ms.UseGv( i ) == true )
				{
					sst.gv_switch = new bool[ this.total_state ] ;
					for( j  = 0 ; j <  this.total_state ; j ++ )
					{
						sst.gv_switch[ j ] = true ;
					}
				}
				else
				{
					sst.gv_switch = null ;
				}
			}
			
			// determine state duration
			duration_mean = new double[ this.total_state ] ;
			duration_vari = new double[ this.total_state ] ;

			for( i  = 0 ; i <  label.GetSize() ; i ++ )
			{
				ms.GetDuration( label.GetString( i ), duration_iw, duration_mean, i * this.nstate, duration_vari, i * this.nstate ) ;
			}

			if( phoneme_alignment_flag == true )
			{
				// use duration set by user
				next_time	= 0 ;
				next_state	= 0 ;
				state		= 0 ;

				for( i  = 0 ; i <  label.GetSize() ; i ++ )
				{
					temp = label.GetEndFrame( i ) ;

					if( temp >= 0 )
					{
						next_time += ( int )SetSpecifiedDuration( this.duration, next_state, duration_mean, next_state, duration_vari, next_state, state + this.nstate - next_state, temp - next_time ) ;
						next_state = state + this.nstate ;
					}
					else
					if( i + 1 == label.GetSize() )
					{
						Debug.LogError( "HTS_SStreamSet_create: The time of final label is not specified." ) ;
						SetDefaultDuration( this.duration, next_state, duration_mean, next_state, duration_vari, next_state, state + this.nstate - next_state ) ;
					}
					state += this.nstate ;
				}
			}
			else
			{
				// determine frame length
				if( speed != 1.0 )
				{
					temp = 0.0 ;
					for( i  = 0 ; i < this.total_state ; i ++ )
					{
						temp += duration_mean[ i ] ;
					}
					frame_length = temp / speed ;
					SetSpecifiedDuration( this.duration, 0, duration_mean, 0, duration_vari, 0, this.total_state, frame_length ) ;
				}
				else
				{
					SetDefaultDuration( this.duration, 0, duration_mean, 0, duration_vari, 0, this.total_state ) ;
				}
			}
			duration_mean = null ;
			duration_vari = null ;
			
			// get parameter
			for( i  = 0, state  = 0 ; i <  label.GetSize() ; i ++ )
			{
				for( j  = 2 ; j <= this.nstate + 1 ; j ++ )
				{
					this.total_frame += this.duration[ state ] ;
					for( k  = 0 ; k <  this.nstream ; k ++ )
					{
						sst = this.sstream[ k ] ;
						if( sst.msd != null )
						{
							ms.GetParameter( k, j, label.GetString( i ), parameter_iw, sst.mean[ state ], 0, sst.vari[ state ], 0, sst.msd, state ) ;
						}
						else
						{
							ms.GetParameter( k, j, label.GetString( i ), parameter_iw, sst.mean[ state ], 0, sst.vari[ state ], 0, null, 0 ) ;
						}
					}
					state ++ ;
				}
			}

			// copy dynamic window
			for( i  = 0 ; i <  this.nstream ; i ++ )
			{
				sst = this.sstream[ i ] ;
				sst.win_size				= ms.GetWindowSize( i ) ;
				sst.win_max_width			= ms.GetWindowMaxWidth( i ) ;
				sst.win_l_width				= new int[ sst.win_size ] ;
				sst.win_r_width				= new int[ sst.win_size ] ;
				sst.win_coefficient			= new double[ sst.win_size ][] ;
				sst.win_coefficient_offset	= new int[ sst.win_size ] ;

				for( j  = 0 ; j <  sst.win_size ; j ++ )
				{
					sst.win_l_width[ j ] = ms.GetWindowLeftWidth( i, j ) ;
					sst.win_r_width[ j ] = ms.GetWindowRightWidth( i, j ) ;

					if( sst.win_l_width[ j ] + sst.win_r_width[ j ] == 0 )
					{
						sst.win_coefficient[ j ] = new double[ -2 * sst.win_l_width[ j ] + 1 ] ;
					}
					else
					{
						sst.win_coefficient[ j ] = new double[ -2 * sst.win_l_width[ j ] ] ;
					}

					// オフセット操作をしている
//					sst.win_coefficient[ j ] -= sst.win_l_width[ j ] ;
					sst.win_coefficient_offset[ j ] -= sst.win_l_width[ j ] ;
					
					for( shift  = sst.win_l_width[ j ] ; shift <= sst.win_r_width[ j ] ; shift ++ )
					{
						// オフセット操作に対応
//						sst.win_coefficient[ j ][ shift ] = ms.GetWindowCoefficient( i, j, shift ) ;
						sst.win_coefficient[ j ][ sst.win_coefficient_offset[ j ] + shift ] = ms.GetWindowCoefficient( i, j, shift ) ;
					}
				}
			}

			// determine GV
			for( i  = 0 ; i <  this.nstream ; i ++ )
			{
				sst = this.sstream[ i ] ;

				if( ms.UseGv( i ) == true )
				{
					sst.gv_mean = new double[ sst.vector_length ] ;
					sst.gv_vari = new double[ sst.vector_length ] ;

					ms.GetGv( i, label.GetString( 0 ), gv_iw, sst.gv_mean, sst.gv_vari ) ;
				}
				else
				{
					sst.gv_mean = null ;
					sst.gv_vari = null ;
				}
			}

			for( i  = 0 ; i <  label.GetSize() ; i ++ )
			{
				if( ms.GetGvFlag( label.GetString( i ) ) == false )
				{
					for( j  = 0 ; j <  this.nstream ; j ++ )
					{
						if( ms.UseGv( j ) == true )
						{
							for( k  = 0 ; k <  this.nstate ; k ++ )
							{
								this.sstream[ j ].gv_switch[ i * this.nstate + k ] = false ;
							}
						}
					}
				}
			}

			//--------------------------------------------------------------------------

/*			Debug.LogWarning( "=====> SStream Check : " + this.nstream ) ;

			int p = 0, q ;
			for( p  = 0 ; p <  this.nstream ; p ++ )
			{
				Debug.LogWarning( "=====" + p ) ;
				Debug.LogWarning( "SS[" + p + "]00:" + this.sstream[ p ].vector_length ) ;
				Debug.LogWarning( "SS[" + p + "]01:" + this.sstream[ p ].mean[ 0 ][ 0 ] ) ;
				Debug.LogWarning( "SS[" + p + "]02:" + this.sstream[ p ].vari[ 0 ][ 0 ] ) ;
				if( this.sstream[ p ].msd == null )
				{
					Debug.LogWarning( "SS[" + p + "]03: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "SS[" + p + "]03:" + this.sstream[ p ].msd[ 0 ] ) ;
				}
				Debug.LogWarning( "SS[" + p + "]04:" + this.sstream[ p ].win_size ) ;
				Debug.LogWarning( "SS[" + p + "]05:" + this.sstream[ p ].win_l_width[ 0 ] ) ;
				Debug.LogWarning( "SS[" + p + "]06:" + this.sstream[ p ].win_r_width[ 0 ] ) ;

				Debug.LogWarning( "SS[" + p + "]07:" + this.sstream[ p ].win_coefficient[ 0 ][ this.sstream[ p ].win_coefficient_offset[ 0 ] + 0 ] ) ;

				Debug.LogWarning( "SS[" + p + "]08:" + this.sstream[ p ].win_max_width ) ;
				if( this.sstream[ p ].gv_mean == null )
				{
					Debug.LogWarning( "SS[" + p + "]09: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "SS[" + p + "]09:" + this.sstream[ p ].gv_mean[ 0 ] ) ;
				}
				if( this.sstream[ p ].gv_vari == null )
				{
					Debug.LogWarning( "SS[" + p + "]10: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "SS[" + p + "]10:" + this.sstream[ p ].gv_vari[ 0 ] ) ;
				}
				if( this.sstream[ p ].gv_switch == null )
				{
					Debug.LogWarning( "SS[" + p + "]11: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "SS[" + p + "]11:" + this.sstream[ p ].gv_switch[ 0 ] ) ;
				}
			}

			Debug.LogWarning( "=====" ) ;
			Debug.LogWarning( "total_frame : " + this.total_frame ) ;
			Debug.LogWarning( "total_state : " + this.total_state ) ;
			for( p  = this.total_state - 10 ; p <  this.total_state ; p ++ )
			{
				Debug.LogWarning( "duration[" + p +"] " + this.duration[ p ] ) ;
			}*/

			return true ;
		}


		// HTS_SStreamSet_get_nstream: get number of stream
		public int GetNstream()
		{
			return this.nstream ;
		}

		// HTS_SStreamSet_get_vector_length: get vector length
		public int GetVectorLength( int stream_index )
		{
			return this.sstream[ stream_index ].vector_length ;
		}



		// HTS_SStreamSet_is_msd: get MSD flag
		public bool IsMsd( int stream_index )
		{
			return this.sstream[ stream_index ].msd != null ? true : false ;
		}





		// HTS_SStreamSet_get_total_state: get total number of state
		public int GetTotalState()
		{
			return this.total_state ;
		}

		// HTS_SStreamSet_get_total_frame: get total number of frame
		public int GetTotalFrame()
		{
			return this.total_frame ;
		}

		// HTS_SStreamSet_get_msd: get MSD parameter
		public double GetMsd( int stream_index, int state_index )
		{
			return this.sstream[ stream_index ].msd[ state_index ] ;
		}

		// HTS_SStreamSet_window_size: get dynamic window size
		public int GetWindowSize( int stream_index )
		{
			return this.sstream[ stream_index ].win_size ;
		}

		// HTS_SStreamSet_get_window_left_width: get left width of dynamic window
		public int GetWindowLeftWidth( int stream_index, int window_index )
		{
			return this.sstream[ stream_index ].win_l_width[ window_index ] ;
		}

		// HTS_SStreamSet_get_winodow_right_width: get right width of dynamic window
		public int GetWindowRightWidth( int stream_index, int window_index )
		{
			return this.sstream[ stream_index ].win_r_width[ window_index ] ;
		}
		
		// HTS_SStreamSet_get_window_coefficient: get coefficient of dynamic window
		public double GetWindowCoefficient( int stream_index, int window_index, int coefficient_index )
		{
			// オフセットのズレに対応
			int offset = this.sstream[ stream_index ].win_coefficient_offset[ window_index ] ;
			return this.sstream[ stream_index ].win_coefficient[ window_index ][ offset + coefficient_index ] ;
		}

		// HTS_SStreamSet_get_window_max_width: get max width of dynamic window
		public int GetWindowMaxWidth( int stream_index )
		{
			return this.sstream[ stream_index ].win_max_width ;
		}



		// HTS_SStreamSet_use_gv: get GV flag
		public bool UseGv( int stream_index )
		{
			return this.sstream[ stream_index ].gv_mean != null ? true : false ;
		}

		// HTS_SStreamSet_get_duration: get state duration
		public int GetDuration( int state_index )
		{
			return this.duration[ state_index ] ;
		}



		// HTS_SStreamSet_get_mean: get mean parameter
		public double GetMean( int stream_index, int state_index, int vector_index )
		{
		   return this.sstream[ stream_index ].mean[ state_index ][ vector_index ] ;
		}





		// HTS_SStreamSet_set_mean: set mean parameter
		public void SetMean( int stream_index, int state_index, int vector_index, double f )
		{
		   this.sstream[ stream_index ].mean[ state_index ][ vector_index ] = f ;
		}

		// HTS_SStreamSet_get_vari: get variance parameter
		public double GetVari( int stream_index, int state_index, int vector_index )
		{
			return this.sstream[ stream_index ].vari[ state_index ][ vector_index ] ;
		}
		
		// HTS_SStreamSet_set_vari: set variance parameter
		public void SetVari( int stream_index, int state_index, int vector_index, double f )
		{
			this.sstream[ stream_index ].vari[ state_index ][ vector_index ] = f ;
		}
		
		// HTS_SStreamSet_get_gv_mean: get GV mean parameter
		public double GetGvMean( int stream_index, int vector_index )
		{
			return this.sstream[ stream_index ].gv_mean[ vector_index ] ;
		}

		// HTS_SStreamSet_get_gv_mean: get GV variance parameter
		public double GetGvVari( int stream_index, int vector_index )
		{
			return this.sstream[ stream_index ].gv_vari[ vector_index ] ;
		}

		// HTS_SStreamSet_set_gv_switch: set GV switch
		public void SetGvSwitch( int stream_index, int state_index, bool i )
		{
			this.sstream[ stream_index ].gv_switch[ state_index ] = i ;
		}

		// HTS_SStreamSet_get_gv_switch: get GV switch
		public bool GetGvSwitch( int stream_index, int state_index )
		{
		   return this.sstream[ stream_index ].gv_switch[ state_index ] ;
		}







		// HTS_set_default_duration: set default duration from state duration probability distribution
		private double SetDefaultDuration( int[] duration, int od, double[] mean, int om, double[] vari, int ov, int size )
		{
			int i ;
			double temp ;
			int sum = 0 ;
			
			for( i  = 0 ; i <  size ; i ++ )
			{
				temp = mean[ om + i] + 0.5 ;
				if( temp <  1.0 )
				{
					duration[ od + i ] = 1;
				}
				else
				{
					duration[ od + i ] = ( int )temp ;
				}
				sum += duration[ od + i ] ;
			}
			
			return ( double )sum ;
		}

		// HTS_set_specified_duration: set duration from state duration probability distribution and specified frame length
		private double SetSpecifiedDuration( int[] duration, int od, double[] mean, int om, double[] vari, int ov, int size, double frame_length )
		{
			int i ;
			int j ;
			double temp1, temp2 ;
			double rho = 0.0 ;
			int sum = 0 ;
			int target_length ;
			
			// get the target frame length
			if( frame_length + 0.5 <  1.0 )
			{
				target_length = 1 ;
			}
			else
			{
				target_length = ( int )( frame_length + 0.5 ) ;
			}
			
			// check the specified duration
			if( target_length <= size )
			{
				if( target_length <  size )
				{
					Debug.LogError( "HTS_set_specified_duration: Specified frame length is too short." ) ;
				}

				for( i  = 0 ; i <  size ; i ++ )
				{
					duration[ od + i ] = 1 ;
				}

				return ( double )size ;
			}
			
			// RHO calculation
			temp1 = 0.0 ;
			temp2 = 0.0 ;
			for( i  = 0 ; i <  size ; i ++ )
			{
				temp1 += mean[ om + i ] ;
				temp2 += vari[ ov + i ] ;
			}
			rho = ( ( double )target_length - temp1 ) / temp2 ;
			
			// first estimation
			for( i  = 0 ; i <  size ; i ++ )
			{
				temp1 = mean[ om + i] + rho * vari[ ov + i ] + 0.5 ;
				if( temp1 <  1.0 )
				{
					duration[ od + i ] = 1 ;
				}
				else
				{
					duration[ od + i ] = ( int )temp1 ;
				}
				sum += duration[ od + i ] ;
			}
			
			// loop estimation
			while( target_length != sum )
			{
				// sarch flexible state and modify its duration
				if( target_length >  sum )
				{
					j = -1 ;
					for( i  = 0 ; i <  size ; i ++ )
					{
						temp2 = Math.Abs( rho - ( ( double )duration[ od + i ] + 1 - mean[ om + i ] ) / vari[ ov + i ] ) ;
						if( j <  0 || temp1 >  temp2 )
						{
							j = i ;
							temp1 = temp2 ;
						}
					}
					sum ++ ;
					duration[ od + j ] ++ ;
				}
				else
				{
					j = -1 ;
					for( i  = 0 ; i <  size ; i ++ )
					{
						if( duration[ od + i ] >  1 )
						{
							temp2 = Math.Abs( rho - ( ( double )duration[ od + i ] - 1 - mean[ om + i ] ) / vari[ ov + i ] ) ;
							if( j <  0 || temp1 > temp2 )
							{
								j = i ;
								temp1 = temp2 ;
							}
						}
					}
					sum -- ;
					duration[ od + j ] -- ;
				}
			}

			return ( double )target_length ;
		}
	}
}
