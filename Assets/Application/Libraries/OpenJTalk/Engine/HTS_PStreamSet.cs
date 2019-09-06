using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	// HTS_PStreamSet: set of PDF streams.
	public class HTS_PStreamSet
	{
		private HTS_PStream[]	m_PStream ;			// PDF streams
		private int				m_NumericOfStream ;	// # of PDF streams
		private int				m_TotalFrame ;		// total frame

		//-----------------------------------------------------------

		public HTS_PStreamSet()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_PStream			= null ;
			m_NumericOfStream	= 0 ;
			m_TotalFrame		= 0 ;
		}

		// HTS_PStreamSet_create: parameter generation using GV weight
		public bool Create( HTS_SStreamSet tSSS, double[] msd_threshold, double[] gv_weight )
		{
			int i, j, k, l, m ;
			int shift ;
			int frame, msd_frame, state ;
			
			HTS_PStream tPS ;
			bool not_bound ;
			
			if( m_NumericOfStream != 0 )
			{
				Debug.LogError( "HTS_PstreamSet_create: HTS_PStreamSet should be clear." ) ;
				return false ;
			}
			
			// initialize
			m_NumericOfStream	= tSSS.GetNstream() ;
			m_PStream			= new HTS_PStream[ m_NumericOfStream ] ;
			m_TotalFrame		= tSSS.GetTotalFrame() ;

//			Debug.LogWarning( "======= PSS Create Start:" + m_NumericOfStream ) ;

			// create
			for( i  = 0 ; i <  m_NumericOfStream ; i ++ )
			{
				m_PStream[ i ] = new HTS_PStream() ;
				tPS = m_PStream[ i ] ;

				if( tSSS.IsMsd( i ) == true )
				{
					// for MSD
					
					tPS.length = 0 ;
					for( state  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
					{
						if( tSSS.GetMsd( i, state ) >  msd_threshold[ i ] )
						{
							tPS.length += tSSS.GetDuration( state ) ;
						}
					}

					tPS.msd_flag = new bool[ m_TotalFrame ] ;
					
					for( state  = 0, frame  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
					{
						if( tSSS.GetMsd( i, state ) >  msd_threshold[ i ] )
						{
							for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++ )
							{
								tPS.msd_flag[ frame ] = true ;
								frame ++ ;
							}
						}
						else
						{
							for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++ )
							{
								tPS.msd_flag[ frame ] = false ;
								frame ++ ;
							}
						}
					}
				}
				else
				{
					// for non MSD
					tPS.length		= m_TotalFrame ;
					tPS.msd_flag	= null ;
				}

				tPS.vector_length	= tSSS.GetVectorLength( i ) ;
				tPS.width			= tSSS.GetWindowMaxWidth( i ) * 2 + 1 ; // band width of R
				tPS.win_size		= tSSS.GetWindowSize( i ) ;

				if( tPS.length >  0 )
				{
					tPS.sm.mean	= AllocMatrix( tPS.length, tPS.vector_length * tPS.win_size ) ;
					tPS.sm.ivar	= AllocMatrix( tPS.length, tPS.vector_length * tPS.win_size ) ;
					tPS.sm.wum		= new double[ tPS.length ] ;
					tPS.sm.wuw		= AllocMatrix( tPS.length, tPS.width			) ;
					tPS.sm.g		= new double[ tPS.length ] ;
					tPS.par		= AllocMatrix( tPS.length, tPS.vector_length	) ;
				}

				// copy dynamic window
				tPS.win_l_width				= new int[ tPS.win_size ] ;
				tPS.win_r_width				= new int[ tPS.win_size ] ;
				tPS.win_coefficient			= new double[ tPS.win_size ][] ;
				tPS.win_coefficient_offset	= new int[ tPS.win_size ] ;

				for( j  = 0 ; j <  tPS.win_size ; j ++ )
				{
					tPS.win_l_width[ j ] = tSSS.GetWindowLeftWidth( i, j ) ;
					tPS.win_r_width[ j ] = tSSS.GetWindowRightWidth( i, j ) ;

					if( tPS.win_l_width[ j ] + tPS.win_r_width[ j ] == 0 )
					{
						tPS.win_coefficient[ j ] = new double[ -2 * tPS.win_l_width[ j ] + 1 ] ;
					}
					else
					{
						tPS.win_coefficient[ j ] = new double[ -2 * tPS.win_l_width[ j ] ] ;
					}

					// オフセットのズレに対応
//					tPS.win_coefficient[ j ] -= tPS.win_l_width[ j ] ;
					tPS.win_coefficient_offset[ j ] -= tPS.win_l_width[ j ] ;
					
					for( shift  = tPS.win_l_width[ j ] ; shift <= tPS.win_r_width[ j ] ; shift ++ )
					{
						// オフセットのズレに対応
//						tPS.win_coefficient[ j ][ shift ] = tSSS.GetWindowCoefficient( i, j, shift );
						tPS.win_coefficient[ j ][ tPS.win_coefficient_offset[ j ] + shift ] = tSSS.GetWindowCoefficient( i, j, shift );
					}
				}

				// copy GV
				if( tSSS.UseGv( i ) == true )
				{
					tPS.gv_mean = new double[ tPS.vector_length ] ;
					tPS.gv_vari = new double[ tPS.vector_length ] ;

					for( j  = 0 ; j <  tPS.vector_length ; j ++ )
					{
						tPS.gv_mean[ j ] = tSSS.GetGvMean( i, j ) * gv_weight[ i ] ;
						tPS.gv_vari[ j ] = tSSS.GetGvVari( i, j ) ;
					}
					
					tPS.gv_switch = new bool[ tPS.length ] ;

					if( tSSS.IsMsd( i ) == true )
					{   
						// for MSD
						for( state  = 0, frame  = 0, msd_frame  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
						{
							for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++, frame ++ )
							{
								if( tPS.msd_flag[ frame ] == true )
								{
									tPS.gv_switch[ msd_frame ++ ] = tSSS.GetGvSwitch( i, state ) ;
								}
							}
						}
					}
					else
					{
						// for non MSD
						for( state  = 0, frame  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
						{
							for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++ )
							{
								tPS.gv_switch[ frame ++ ] = tSSS.GetGvSwitch( i, state ) ;
							}
						}
					}

					for( j  = 0, tPS.gv_length  = 0 ; j <  tPS.length ; j ++ )
					{
						if( tPS.gv_switch[ j ] == true )
						{
							tPS.gv_length ++ ;
						}
					}
				}
				else
				{
					tPS.gv_switch	= null ;
					tPS.gv_length	= 0 ;
					tPS.gv_mean		= null ;
					tPS.gv_vari		= null ;
				}

				// copy pdfs
				if( tSSS.IsMsd( i ) == true )
				{
					// for MSD
					for( state  = 0, frame  = 0, msd_frame  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
					{
						for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++ )
						{
							if( tPS.msd_flag[ frame ] == true )
							{
								// check current frame is MSD boundary or not
								for( k  = 0 ; k <  tPS.win_size ; k ++ )
								{
									not_bound = true ;
									for( shift  = tPS.win_l_width[ k ]; shift <= tPS.win_r_width[ k ] ; shift ++ )
									{
										if( ( int )frame + shift <  0 || ( int )m_TotalFrame <= ( int )frame + shift || tPS.msd_flag[ frame + shift ] != true )
										{
											not_bound = false ;
											break;
										}
									}

									for( l  = 0 ; l <  tPS.vector_length ; l ++ )
									{
										m = tPS.vector_length * k + l ;
										tPS.sm.mean[ msd_frame ][ m ] = tSSS.GetMean( i, state, m ) ;

										if( not_bound || k == 0 )
										{
											tPS.sm.ivar[ msd_frame ][ m ] = Finv( tSSS.GetVari( i, state, m ) ) ;
										}
										else
										{
											tPS.sm.ivar[ msd_frame ][ m ] = 0.0 ;
										}
									}
								}
								msd_frame ++ ;
							}
							frame ++ ;
						}
					}
				}
				else
				{
					// for non MSD
					for( state  = 0, frame  = 0 ; state <  tSSS.GetTotalState() ; state ++ )
					{
						for( j  = 0 ; j <  tSSS.GetDuration( state ) ; j ++ )
						{
							for( k  = 0 ; k <  tPS.win_size ; k ++ )
							{
								not_bound = true ;
								for( shift  = tPS.win_l_width[ k ] ; shift <= tPS.win_r_width[ k ] ; shift ++ )
								{
									if( ( int )frame + shift <  0 || ( int )m_TotalFrame <= ( int ) frame + shift )
									{
										not_bound = false ;
										break ;
									}
								}
								
								for( l  = 0 ; l <  tPS.vector_length ; l ++ )
								{
									m = tPS.vector_length * k + l ;
									tPS.sm.mean[ frame ][ m ] = tSSS.GetMean( i, state, m ) ;
									if( not_bound || k == 0 )
									{
										tPS.sm.ivar[ frame ][ m ] = Finv( tSSS.GetVari( i, state, m ) ) ;
									}
									else
									{
										tPS.sm.ivar[ frame ][ m ] = 0.0 ;
									}
								}
							}
							frame ++ ;
						}
					}
				}
				
				// parameter generation
				tPS.Mlpg() ;
			}
			
			//--------------------------------------------------------------------------

/*			Debug.LogWarning( "=====> PStream Check : " + this.nstream ) ;
			Debug.LogWarning( "total_frame : " + this.total_frame ) ;

			int p, q ;
			for( p  = 0 ; p <  this.nstream ; p ++ )
			{
				Debug.LogWarning( "=====" + p ) ;
				Debug.LogWarning( "PS[" + p + "]00:" + this.pstream[ p ].vector_length ) ;
				Debug.LogWarning( "PS[" + p + "]01:" + this.pstream[ p ].length ) ;
				Debug.LogWarning( "PS[" + p + "]02:" + this.pstream[ p ].width ) ;
				Debug.LogWarning( "PS[" + p + "]03:" + this.pstream[ p ].win_size ) ;
				Debug.LogWarning( "PS[" + p + "]04:" + this.pstream[ p ].win_l_width[ 0 ] ) ;
				Debug.LogWarning( "PS[" + p + "]05:" + this.pstream[ p ].win_r_width[ 0 ] ) ;

				Debug.LogWarning( "PS[" + p + "]06:" + this.pstream[ p ].win_coefficient[ 0 ][ this.pstream[ p ].win_coefficient_offset[ 0 ] + 0 ] ) ;

				if( this.pstream[ p ].msd_flag == null )
				{
					Debug.LogWarning( "PS[" + p + "]07: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "PS[" + p + "]07:" + this.pstream[ p ].msd_flag[ 0 ] ) ;
				}
				if( this.pstream[ p ].gv_mean == null )
				{
					Debug.LogWarning( "PS[" + p + "]08: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "PS[" + p + "]08:" + this.pstream[ p ].gv_mean[ 0 ] ) ;
				}
				if( this.pstream[ p ].gv_vari == null )
				{
					Debug.LogWarning( "PS[" + p + "]09: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "PS[" + p + "]09:" + this.pstream[ p ].gv_vari[ 0 ] ) ;
				}
				if( this.pstream[ p ].gv_switch == null )
				{
					Debug.LogWarning( "PS[" + p + "]10: is null" ) ;
				}
				else
				{
					Debug.LogWarning( "PS[" + p + "]10:" + this.pstream[ p ].gv_switch[ 0 ] ) ;
				}
				Debug.LogWarning( "PS[" + p + "]11:" + this.pstream[ p ].gv_length ) ;

				for( q  = 0 ; q <  3 ; q ++ )
				{
					Debug.LogWarning( "PS[" + p + "]sm.mean[" + q + "]:" + this.pstream[ p ].sm.mean[ q ][ 0 ] ) ;
					Debug.LogWarning( "PS[" + p + "]sm.ivar[" + q + "]:" + this.pstream[ p ].sm.ivar[ q ][ 0 ] ) ;
					Debug.LogWarning( "PS[" + p + "]sm.g[" + q + "]:" + this.pstream[ p ].sm.g[ q ] ) ;
					Debug.LogWarning( "PS[" + p + "]sm.wuw[" + q + "]:" + this.pstream[ p ].sm.wuw[ q ][ 0 ] ) ;
					Debug.LogWarning( "PS[" + p + "]sm.wum[" + q + "]:" + this.pstream[ p ].sm.wum[ q ] ) ;
				}
			}
			p = 1 ;
			for( q  = 0 ; q < 32 ; q ++ )
			{
				Debug.LogWarning( "PS[" + p + "]par[" + q + "]:" + this.pstream[ p ].par[ q ][ 0 ] ) ;
			}*/

			return true ;
		}

		// HTS_PStreamSet_get_nstream: get number of stream
		public int GetNumericOfStream()
		{
			return m_NumericOfStream ;
		}

		// HTS_PStreamSet_get_vector_length: get feature length
		public int GetVectorLength( int tStreamIndex )
		{
			return m_PStream[ tStreamIndex ].vector_length ;
		}

		// HTS_PStreamSet_get_total_frame: get total number of frame
		public int GetTotalFrame()
		{
			return m_TotalFrame ;
		}
		
		// HTS_PStreamSet_get_parameter: get parameter
		public double GetParameter( int tStreamIndex, int tFrameIndex, int tVectorIndex )
		{
			return m_PStream[ tStreamIndex ].par[ tFrameIndex ][ tVectorIndex ] ;
		}
		
		// HTS_PStreamSet_get_parameter_vector: get parameter vecto
		public double[] GetParameterVector( int tStreamIndex, int tFrameIndex )
		{
			return m_PStream[ tStreamIndex ].par[ tFrameIndex ] ;
		}
		
		// HTS_PStreamSet_get_msd_flag: get generated MSD flag per frame
		public bool GetMsdFlag( int tStreamIndex, int tFrameIndex )
		{
			return m_PStream[ tStreamIndex ].msd_flag[ tFrameIndex ] ;
		}
		
		// HTS_PStreamSet_is_msd: get MSD flag
		public bool IsMsd( int tStreamIndex )
		{
			return m_PStream[ tStreamIndex ].msd_flag != null ? true : false ;
		}

		//---------------------------------------------------------------------------

		// HTS_alloc_matrix: allocate double matrix
		private double[][] AllocMatrix( int x, int y )
		{
			int i ;
			double[][] p ;
			
			if( x == 0 || y == 0 )
			{
				return null ;
			}
			
			p = new double[ x ][] ;
			
			for( i  = 0 ; i <  x ; i ++ )
			{
				p[ i ] = new double[ y ] ;
			}

			return p ;
		}

		private const double		INFTY			= ( ( double )1.0e+38 ) ;
		private const double		INFTY2			= ( ( double )1.0e+19 ) ;
		private const double		INVINF			= ( ( double )1.0e-38 ) ;
		private const double		INVINF2			= ( ( double )1.0e-19 ) ;

		// HTS_finv: calculate 1.0/variance function 
		private double Finv( double x )
		{
			if( x >=  INFTY2 )
			{
				return 0.0 ;
			}

			if( x <= -INFTY2 )
			{
				return 0.0 ;
			}

			if( x <=  INVINF2 && x >= 0 )
			{
				return  INFTY ;
			}

			if( x >= -INVINF2 && x <  0 )
			{
				return -INFTY ;
			}

			return ( 1.0 / x ) ;
		}
	}
}
