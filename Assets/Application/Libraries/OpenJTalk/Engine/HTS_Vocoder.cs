using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	// HTS_Vocoder: structure for setting of vocoder
	public class HTS_Vocoder
	{
		private bool		is_first ;
		private int			stage ;					// Gamma=-1/stage: if stage=0 then Gamma=0
		private double		gamma ;					// Gamma
		private bool		use_log_gain ;			// log gain flag (for LSP)
		private int			fprd ;					// frame shift
		private ulong		next ;					// temporary variable for random generator
		private bool		gauss ;					// flag to use Gaussian noise
		private double		rate ;					// sampling rate
		private double		pitch_of_curr_point ;	// used in excitation generation
		private double		pitch_counter ;			// used in excitation generation
		private double		pitch_inc_per_point ;	// used in excitation generation
		private double[]	excite_ring_buff ;		// used in excitation generation
		private int			excite_buff_size ;		// used in excitation generation
		private int			excite_buff_index ;		// used in excitation generation
		private byte		sw ;					// switch used in random generator
		private uint		x ;						// excitation signal
		private double[]	freqt_buff ;			// used in freqt
		private int			freqt_size ;			// buffer size for freqt
		private double[]	spectrum2en_buff ;		// used in spectrum2en
		private int			spectrum2en_size ;		// buffer size for spectrum2en
		private double		r1, r2, s ;				// used in random generator
		private double[]	postfilter_buff ;		// used in postfiltering
		private int			postfilter_size ;		// buffer size for postfiltering
//		private double[]	c, cc, cinc, d1 ;		// used in the MLSA/MGLSA filter
		private double[]	c ;						// used in the MLSA/MGLSA filter
		private int			cc, cinc, d1 ;			// オフセット補正
		private double[]	lsp2lpc_buff ;			// used in lsp2lpc
		private int			lsp2lpc_size ;			// buffer size of lsp2lpc
		private double[]	gc2gc_buff ;			// used in gc2gc
		private int			gc2gc_size ;			// buffer size for gc2gc

		//-----------------------------------------------------------

		private const int		SEED		= 1 ;

		private const uint		B0			= 0x00000001 ;
		private const uint		B28			= 0x10000000 ;
		private const uint		B31			= 0x80000000 ;
		private const uint		B31_		= 0x7fffffff ;

		private const bool		GAUSS		= true ;
		private const int		PADEORDER	= 5 ;			// pade order (for MLSA filter)
		private const int		IRLENG		= 576 ;			// length of impulse response

		private const double	LZERO		= ( -1.0e+10 ) ;		// ~log(0)

		private const double	MAX_F0		= 20000.0 ;
		private const double	MIN_F0		= 20.0 ;
		private const double	MAX_LF0		= 9.9034875525361280454891979401956 ;	// log(20000.0)
		private const double	MIN_LF0		= 2.9957322735539909934352235761425 ;	// log(20.0)
		private const double	HALF_TONE	= 0.05776226504666210911810267678818 ;	// log(2.0) / 12.0
		private const double	DB			= 0.11512925464970228420089957273422 ;	// log(10.0) / 20.0


		private const bool		NORMFLG1	= true ;
//		private const bool		NORMFLG2	= false ;
		private const bool		MULGFLG1	= true ;
//		private const bool		MULGFLG2	= false ;
		private const bool		NGAIN		= false ;


		private const double	CHECK_LSP_STABILITY_MIN		= 0.25 ;
		private const int		CHECK_LSP_STABILITY_NUM		= 4 ;

		private const double	PI							= 3.14159265358979323846 ;

		private const double	RANDMAX						= 32767 ;


		//-----------------------------------------------------------

		public HTS_Vocoder()
		{
			Initialize() ;
		}

		// HTS_Vocoder_clear: clear vocoder
		public void Initialize()
		{
			// free buffer
			this.freqt_buff  = null ;
			this.freqt_size = 0 ;

			this.gc2gc_buff  = null ;
			this.gc2gc_size = 0 ;

			this.lsp2lpc_buff  = null ;
			this.lsp2lpc_size = 0 ;
			
			this.postfilter_buff  = null ;
			this.postfilter_size = 0 ;

			this.spectrum2en_buff  = null ;
			this.spectrum2en_size = 0 ;

			this.c  = null ;

			this.excite_buff_size = 0 ;
			this.excite_buff_index = 0 ;

			this.excite_ring_buff  = null ;
		}

		// HTS_Vocoder_initialize: initialize vocoder
		public void Prepare( int m, int stage, bool use_log_gain, int rate, int fperiod )
		{
			// set parameter
			this.is_first	= true ;
			this.stage		= stage ;

			if( stage != 0 )
			{
				this.gamma = -1.0 / this.stage ;
			}
			else
			{
				this.gamma = 0.0 ;
			}

			this.use_log_gain			= use_log_gain ;
			this.fprd					= fperiod ;
			this.next					= SEED ;
			this.gauss					= GAUSS ;
			this.rate					= rate ;
			this.pitch_of_curr_point	= 0.0 ;
			this.pitch_counter			= 0.0 ;
			this.pitch_inc_per_point	= 0.0 ;
			this.excite_ring_buff		= null ;
			this.excite_buff_size		= 0 ;
			this.excite_buff_index		= 0 ;
			this.sw						= 0 ;
			this.x						= 0x55555555 ;

			// init buffer
			this.freqt_buff				= null ;
			this.freqt_size				= 0;
			this.gc2gc_buff				= null ;
			this.gc2gc_size				= 0 ;
			this.lsp2lpc_buff			= null ;
			this.lsp2lpc_size			= 0 ;
			this.postfilter_buff		= null ;
			this.postfilter_size		= 0 ;
			this.spectrum2en_buff		= null ;
			this.spectrum2en_size		= 0 ;

//			Debug.LogWarning( "======= Initialize GAUSS : " + GAUSS ) ;
//			Debug.LogWarning( "======= Initialize PADEORDER : " + PADEORDER ) ;
//			Debug.LogWarning( "======= Initialize IRLENG : " + IRLENG ) ;
			
			if( this.stage == 0 )
			{
				// for MCP
				this.c		= new double[ m * ( 3 + PADEORDER ) + 5 * PADEORDER + 6 ] ;

				// オフセット補正
//				this.cc		= this.c    + m + 1 ;
//				this.cinc	= this.cc   + m + 1 ;
//				this.d1		= this.cinc + m + 1 ;
				this.cc		=               m + 1 ;
				this.cinc	= this.cc     + m + 1 ;
				this.d1		= this.cinc   + m + 1 ;
			}
			else
			{
				// for LSP
				this.c		= new double[ ( m + 1 ) * ( this.stage + 3 ) ] ;

				// オフセット補正
//				this.cc		= this.c    + m + 1 ;
//				this.cinc	= this.cc   + m + 1 ;
//				this.d1		= this.cinc + m + 1 ;
				this.cc		=               m + 1 ;
				this.cinc	= this.cc     + m + 1 ;
				this.d1		= this.cinc   + m + 1 ;
			}
		}


		// HTS_Vocoder_initialize_excitation: initialize excitation
		private void InitializeExcitation( double pitch, int nlpf )
		{
			int i ;
			
			this.pitch_of_curr_point	= pitch ;
			this.pitch_counter			= pitch ;
			this.pitch_inc_per_point	= 0.0 ;

			if( nlpf >  0 )
			{
				this.excite_buff_size = nlpf ;
				this.excite_ring_buff = new double[ this.excite_buff_size ] ;

				for( i  = 0; i <  this.excite_buff_size ; i ++ )
				{
					this.excite_ring_buff[ i ] = 0.0 ;
				}
				
				this.excite_buff_index = 0 ;
			}
			else
			{
				this.excite_buff_size	= 0 ;
				this.excite_ring_buff	= null ;
				this.excite_buff_index	= 0 ;
			}
		}




		// HTS_Vocoder_synthesize: pulse/noise excitation and MLSA/MGLSA filster based waveform synthesis
		public void Synthesize( int m, double lf0, double[] spectrum, int nlpf, double[] lpf, double alpha, double beta, double volume, double[] rawdata, int rawdata_o )
		{
			double x ;
			int i, j ;
//			short xs ;
			int rawidx = 0 ;
			double p ;
			
			// lf0 -> pitch
			if( lf0 == LZERO )
			{
				p = 0.0 ;
			}
			else
			if( lf0 <= MIN_LF0 )
			{
				p = this.rate / MIN_F0 ;
			}
			else
			if( lf0 >= MAX_LF0 )
			{
				p = this.rate / MAX_F0 ;
			}
			else
			{
				p = this.rate / Math.Exp( lf0 ) ;
			}

			// first time
			if( this.is_first == true )
			{
				InitializeExcitation( p, nlpf ) ;
				
				if( this.stage == 0 )
				{
					// for MCP

					Mc2b( spectrum, 0, this.c, 0, m, alpha ) ;
				}
				else
				{
					// for LSP
					Movem( spectrum, 0, this.c, 0, m + 1 ) ;
					Lsp2mgc( this.c, 0, this.c, 0, m, alpha ) ;
					Mc2b( this.c, 0, this.c, 0, m, alpha ) ;
					Gnorm( this.c, 0, this.c, 0, m, this.gamma ) ;

					for( i  = 1 ; i <= m ; i ++ )
					{
						this.c[ i ] *= this.gamma ;
					}
				}

				this.is_first = false ;
			}

			StartExcitation( p ) ;

			if( this.stage == 0 )
			{
				// for MCP
				PostfilterMcp( spectrum, m, alpha, beta ) ;

				// オフセット操作
//				Mc2b( spectrum, this.cc, m, alpha ) ;
				Mc2b( spectrum, 0, this.c, this.cc, m, alpha ) ;

				for( i  = 0 ; i <= m ; i ++ )
				{
					// オフセット操作
//					this.cinc[ i ] = ( this.cc[ i ] - this.c[ i ] ) / this.fprd ;
					this.c[ this.cinc + i ] = (  this.c[ this.cc + i ] - this.c[ i ] ) / this.fprd ;
				}
			}
			else
			{
				// for LSP
				PostfilterLsp( spectrum, m, alpha, beta ) ;

				CheckLspStability( spectrum, m ) ;

				// オフセット操作
//				Lsp2mgc( spectrum, this.cc, m, alpha ) ;
				Lsp2mgc( spectrum, 0, this.c, this.cc, m, alpha ) ;

				// オフセット操作
//				Mc2b( this.cc, this.cc, m, alpha ) ;
				Mc2b( this.c, this.cc, this.c, this.cc, m, alpha ) ;

				// オフセット操作
//				Gnorm( this.cc, this.cc, m, this.gamma ) ;
				Gnorm( this.c, this.cc, this.c, this.cc, m, this.gamma ) ;

				for( i  = 1 ; i <= m ; i ++ )
				{
					// オフセット操作
//					this.cc[ i ] *= this.gamma ;
					this.c[ this.cc + i ] *= this.gamma ;
				}

				for( i  = 0; i <= m ; i ++ )
				{
					// オフセット操作
//					this.cinc[ i ] = ( this.cc[ i ] - this.c[ i ] ) / this.fprd ;
					this.c[	this.cinc + i ] = ( this.c[ this.cc + i ] - this.c[ i ] ) / this.fprd ;
				}
			}

			for( j  = 0 ; j <  this.fprd ; j ++ )
			{
//				if( HTS_Engine.GetDebugFlag( 0 ) == 1 )
//				{
//					if( j <  20 )
//					{
//						HTS_Engine.SetDebugFlag( 1, 1 ) ;
//					}
//					else
//					{
//						HTS_Engine.SetDebugFlag( 1, 0 ) ;
//					}
//				}

				x = GetExcitation( lpf ) ;

				if( this.stage == 0 )
				{
					// for MCP
					if( x != 0.0 )
					{
						x *= Math.Exp( this.c[ 0 ] ) ;
					}

					// オフセット操作
//					x = Mlsadf( x, this.c, m, alpha, PADEORDER, this.d1 ) ;
					x = Mlsadf( x, this.c, m, alpha, PADEORDER, this.c, this.d1 ) ;
				}
				else
				{
					// for LSP
					if( NGAIN == false )
					{
						x *= this.c[ 0 ] ;
					}
					
					// オフセット操作
//					x = Mglsadf( x, this.c, m, alpha, this.stage, this.d1 ) ;
					x = Mglsadf( x, this.c, m, alpha, this.stage, this.c, this.d1 ) ;
				}
				
//				if( HTS_Engine.GetDebugFlag( 1 ) == 1 )
//				{
//					Debug.LogWarning( "x[" + j + "] : " + x ) ;
//				}

				x *= volume ;
				
				// output
				if( rawdata != null )
				{
					rawdata[ rawdata_o + rawidx ++ ] = x ;
				}
								
				for( i  = 0 ; i <= m ; i ++ )
				{
					// オフセット操作
//					this.c[ i ] += this.cinc[ i ] ;
					this.c[ i ] += this.c[ this.cinc + i ] ;
				}
			}
			
			EndExcitation( p ) ;
			
			// オフセット操作
//			Movem( this.cc, this.c, m + 1 ) ;
			Movem( this.c, this.cc, this.c, 0, m + 1 ) ;
		}
		
		//---------------------------------------------------------------------------

		// HTS_mc2b: transform mel-cepstrum to MLSA digital fillter coefficients
		private void Mc2b( double[] mc, int mc_o, double[] b, int b_o, int m, double a )
		{
			if( mc != b || mc_o != b_o )
			{
				if( a != 0.0 )
				{
					b[ b_o + m ] = mc[ mc_o + m ] ;
					
					for( m -- ; m >= 0 ; m -- )
					{
						b[ b_o + m ] = mc[ mc_o + m ] - a * b[ b_o + m + 1 ] ;
					}
				}
				else
				{
					Movem( mc, mc_o, b, b_o, m + 1 ) ;
				}
			}
			else
			if( a != 0.0 )
			{
				for( m -- ; m >= 0 ; m -- )
				{
					b[ b_o + m ] -= a * b[ b_o + m + 1 ] ;
				}
			}
		}

		// HTS_movem: move memory
//		private void Movem( double *a, double *b, int nitem )
//		{
//			long i = ( long )nitem ;
//			
//			if( a >  b )
//			{
//				// b a
//				while( i -- )
//				{
//					*b++ = *a ++ ;
//				}
//			}
//			else
//			{
//				// a b
//				a += i ;
//				b += i ;
//				while( i -- )
//				{
//					*--b = *--a ;
//				}
//			}
//		}
		
		private void Movem( double[] a, int oa, double[] b, int ob, int nitem )
		{
			int i = ( int )nitem ;
			int p ;

			if( a != b )
			{
				Array.Copy( a, oa, b, ob, i ) ;
			}
			else
			{
				if( oa >  ob )
				{
					// b a
					for( p  = 0 ; p <  i ; p ++ )
					{
						b[ ob + p ] = a[ oa + p ] ;
					}
				}
				else
				{
					// a b
					for( p  = i - 1 ; p >= 0 ; p -- )
					{
						b[ ob + p ] = a[ oa + p ] ;
					}
				}
			}
		}
		
		// HTS_lsp2mgc: transform LSP to MGC
		private void Lsp2mgc( double[] lsp, int lsp_o, double[] mgc, int mgc_o, int m, double alpha )
		{
//			int i ;

			// lsp2lpc
			Lsp2lpc( lsp, lsp_o + 1, mgc, mgc_o, m ) ;

			if( this.use_log_gain == true )
			{
				mgc[ mgc_o + 0 ] = Math.Exp( lsp[ lsp_o + 0 ] ) ;
			}
			else
			{
				mgc[ mgc_o + 0 ] = lsp[ lsp_o + 0 ] ;
			}
			
			// mgc2mgc
			if( NORMFLG1 )
			{
				Ignorm( mgc, mgc_o, mgc, mgc_o, m, this.gamma ) ;
			}
//			else
//			if( MULGFLG1 )
//			{
//				mgc[ mgc_o + 0 ] = ( 1.0 - mgc[ mgc_o + 0 ] ) * ( ( double )this.stage ) ;
//			}

//			if( MULGFLG1 )
//			{
//				for( i  = m ; i >= 1 ; i -- )
//				{
//					mgc[ mgc_o + i ] *= -( ( double )this.stage ) ;
//				}
//			}
			
			Mgc2mgc( mgc, mgc_o, m, alpha, this.gamma, mgc, mgc_o, m, alpha, this.gamma ) ;
			
//			if( NORMFLG2 )
//			{
//				Gnorm( mgc, mgc_o, mgc, mgc_o, m, this.gamma ) ;
//			}
//			else
//			if( MULGFLG2 )
//			{
//				mgc[ mgc_o + 0 ] = mgc[ mgc_o + 0 ] * this.gamma + 1.0 ;
//			}

//			if( MULGFLG2 )
//			{
//				for( i  = m ; i >= 1 ; i -- )
//				{
//					mgc[ mgc_o + i ] *= this.gamma ;
//				}
//			}
		}
		
		// HTS_lsp2lpc: transform LSP to LPC
		private void Lsp2lpc( double[] lsp, int lsp_o, double[] a, int a_o, int m )
		{
			int i, k, mh1, mh2, flag_odd ;
			double xx, xf, xff;

			// オフセット操作
//			double[]	p, q ;
//			double[]	a0, a1, a2, b0, b1, b2 ;
			int			p, q ;
			int			a0, a1, a2, b0, b1, b2 ;


			flag_odd = 0 ;

			if( m % 2 == 0 )
			{
				mh1 = mh2 = m / 2 ;
			}
			else
			{
				mh1 = ( m + 1 ) / 2 ;
				mh2 = ( m - 1 ) / 2 ;
				flag_odd = 1 ;
			}

			if( m >  this.lsp2lpc_size )
			{
				if( this.lsp2lpc_buff != null )
				{
					this.lsp2lpc_buff  = null ;
				}
				
				this.lsp2lpc_buff = new double[ 5 * m + 6 ] ;
				this.lsp2lpc_size = m ;
			}
			
			// オフセット操作
//			p  = this.lsp2lpc_buff + m ;
//			q  = p  + mh1 ;
//			a0 = q  + mh2 ;
//			a1 = a0 + ( mh1 + 1 ) ;
//			a2 = a1 + ( mh1 + 1 ) ;
//			b0 = a2 + ( mh1 + 1 ) ;
//			b1 = b0 + ( mh2 + 1 ) ;
//			b2 = b1 + ( mh2 + 1 ) ;
			p  = m ;
			q  = p  + mh1 ;
			a0 = q  + mh2 ;
			a1 = a0 + ( mh1 + 1 ) ;
			a2 = a1 + ( mh1 + 1 ) ;
			b0 = a2 + ( mh1 + 1 ) ;
			b1 = b0 + ( mh2 + 1 ) ;
			b2 = b1 + ( mh2 + 1 ) ;
			
			
 			Movem( lsp, lsp_o, this.lsp2lpc_buff, 0, m ) ;

			for( i  = 0 ; i <  mh1 + 1 ; i ++ )
			{
				// オフセット操作
//				a0[ i ] = 0.0 ;
				this.lsp2lpc_buff[ a0 + i ] = 0.0 ;
			}

			for( i  = 0 ; i <  mh1 + 1 ; i ++ )
			{
				// オフセット操作}
//				a1[ i ] = 0.0 ;
				this.lsp2lpc_buff[ a1 + i ] = 0.0 ;
			}

			for( i  = 0 ; i <  mh1 + 1 ; i ++ )
			{
				// オフセット操作
//				a2[ i ] = 0.0 ;
				this.lsp2lpc_buff[ a2 + i ] = 0.0 ;				
			}

			for( i  = 0 ; i <  mh2 + 1 ; i ++ )
			{
				// オフセット操作
//				b0[ i ] = 0.0 ;
				this.lsp2lpc_buff[ b0 + i ] = 0.0 ;				
			}

			for( i  = 0 ; i <  mh2 + 1 ; i ++ )
			{
				// オフセット操作
//				b1[ i ] = 0.0 ;
				this.lsp2lpc_buff[ b1 + i ] = 0.0 ;				
			}

			for( i  = 0 ; i <  mh2 + 1 ; i ++ )
			{
				// オフセット操作
//				b2[ i ] = 0.0 ;
				this.lsp2lpc_buff[ b2 + i ] = 0.0 ;				
			}

			// lsp filter parameters
			for( i  = k  = 0 ; i <  mh1 ; i ++, k += 2 )
			{
				// オフセット操作
//				p[ i ] = -2.0 * Math.Cos( this.lsp2lpc_buff[ k ] ) ;
				this.lsp2lpc_buff[ p + i ] = -2.0 * Math.Cos( this.lsp2lpc_buff[ k ] ) ; ;				
			}

			for( i  = k  = 0 ; i <  mh2 ; i ++, k += 2 )
			{
				// オフセット操作
//				q[ i ] = -2.0 * Math.Cos( this.lsp2lpc_buff[ k + 1 ] ) ;
				this.lsp2lpc_buff[ q + i ] = -2.0 * Math.Cos( this.lsp2lpc_buff[ k + 1 ] ) ;
			}

			// impulse response of analysis filter
			xx = 1.0 ;
			xf = xff = 0.0 ;
			
			for( k  = 0 ; k <= m ; k ++ )
			{
				if( flag_odd >  0 )
				{
					// オフセット操作
//					a0[ 0 ] = xx ;
//					b0[ 0 ] = xx - xff ;
					this.lsp2lpc_buff[ a0 + 0 ] = xx ;
					this.lsp2lpc_buff[ b0 + 0 ] = xx - xff ;
					
					xff = xf ;
					xf  = xx ;
				}
				else
				{
					// オフセット操作
//					a0[ 0 ] = xx + xf ;
//					b0[ 0 ] = xx - xf ;
					this.lsp2lpc_buff[ a0 + 0 ] = xx + xf ;
					this.lsp2lpc_buff[ b0 + 0 ] = xx - xf ;
					
					xf = xx ;
				}
				
				for( i  = 0 ; i <  mh1 ; i ++ )
				{
					// オフセット操作
//					a0[ i + 1 ] = a0[ i ] + p[ i ] * a1[ i ] + a2[ i ] ;
//					a2[ i ] = a1[ i ] ;
//					a1[ i ] = a0[ i ] ;
					this.lsp2lpc_buff[ a0 + i + 1 ] = this.lsp2lpc_buff[ a0 + i ] + this.lsp2lpc_buff[ p + i ] * this.lsp2lpc_buff[ a1 + i ] + this.lsp2lpc_buff[ a2 + i ] ;
					this.lsp2lpc_buff[ a2 + i ] = this.lsp2lpc_buff[ a1 + i ] ;
					this.lsp2lpc_buff[ a1 + i ] = this.lsp2lpc_buff[ a0 + i ] ;
				}
				
				for( i  = 0 ; i <  mh2 ; i ++ )
				{
					// オフセット操作
//					b0[ i + 1 ] = b0[ i ] + q[ i ] * b1[ i ] + b2[ i ] ;
//					b2[ i ] = b1[ i ] ;
//					b1[ i ] = b0[ i ] ;
					this.lsp2lpc_buff[ b0 + i + 1 ] = this.lsp2lpc_buff[ b0 + i ] + this.lsp2lpc_buff[ q + i ] * this.lsp2lpc_buff[ b1 + i ] + this.lsp2lpc_buff[ b2 + i ] ;
					this.lsp2lpc_buff[ b2 + i ] = this.lsp2lpc_buff[ b1 + i ] ;
					this.lsp2lpc_buff[ b1 + i ] = this.lsp2lpc_buff[ b0 + i ] ;
				}
				
				if( k != 0 )
				{
					// オフセット操作
//					a[ k - 1 ] = -0.5 * ( a0[ mh1 ] + b0[ mh2 ] ) ;
					a[ a_o + k - 1 ] = -0.5 * ( this.lsp2lpc_buff[ a0 + mh1 ] + this.lsp2lpc_buff[ b0 + mh2 ] ) ;
				}
				
				xx = 0.0 ;
			}
			
			for( i  = m - 1 ; i >= 0 ; i -- )
			{
				a[ a_o + i + 1 ] = -a[ a_o + i ] ;
			}
			
			a[ a_o + 0 ] = 1.0 ;
		}
		
		// HTS_ignorm: inverse gain normalization
		private void Ignorm( double[] c1, int c1_o, double[] c2, int c2_o, int m, double g )
		{
			double k ;

			if( g != 0.0 )
			{
				k = Math.Pow( c1[ c1_o + 0 ], g ) ;
				for( ; m >= 1 ; m -- )
				{
					c2[ c2_o + m ] = k * c1[ c1_o + m ] ;
				}

				c2[ c2_o + 0 ] = ( k - 1.0 ) / g ;
			}
			else
			{
				Movem( c1, c1_o + 1, c2, c2_o + 1, m ) ;
				c2[ c2_o + 0 ] = Math.Log( c1[ c1_o + 0 ] ) ;
			}
		}
		
		// HTS_mgc2mgc: frequency and generalized cepstral transformation
		private void Mgc2mgc( double[] c1, int c1_o, int m1, double a1, double g1, double[] c2, int c2_o, int m2, double a2, double g2 )
		{
			double a ;
			
			if( a1 == a2 )
			{
				Gnorm( c1, c1_o, c1, c1_o, m1, g1 ) ;
				Gc2gc( c1, c1_o, m1, g1, c2, c2_o, m2, g2 ) ;
				Ignorm( c2, c2_o, c2, c2_o, m2, g2 ) ;
			}
			else
			{
				a = ( a2 - a1 ) / ( 1 - a1 * a2 ) ;
				Freqt( c1, c1_o, m1, c2, c2_o, m2, a ) ;
				Gnorm( c2, c2_o, c2, c2_o, m2, g1 ) ;
				Gc2gc( c2, c2_o, m2, g1, c2, c2_o, m2, g2 ) ;
				Ignorm( c2, c2_o, c2, c2_o, m2, g2 ) ;
			}
		}

		// HTS_gnorm: gain normalization
		private void Gnorm( double[] c1, int c1_o, double[] c2, int c2_o, int m, double g )
		{
			double k ;

			if( g != 0.0 )
			{
				k = 1.0 + g * c1[ c1_o + 0 ] ;
				for( ; m >= 1 ; m -- )
				{
					c2[ c2_o + m ] = c1[ c1_o + m ] / k ;
				}
				
				c2[ c2_o + 0 ] = Math.Pow( k, 1.0 / g ) ;
			}
			else
			{
				Movem( c1, c1_o + 1, c2, c2_o + 1, m ) ;
				c2[ c2_o + 0 ] = Math.Exp( c1[ c1_o + 0 ] ) ;
			}
		}
		
		// HTS_gc2gc: generalized cepstral transformation
		private void Gc2gc( double[] c1, int c1_o, int m1, double g1, double[] c2, int c2_o, int m2, double g2 )
		{
			int i, min, k, mk ;
			double ss1, ss2, cc ;
			
			if( m1 >  this.gc2gc_size )
			{
				if( this.gc2gc_buff != null )
				{
					this.gc2gc_buff  = null ;
				}
				
				this.gc2gc_buff = new double[ m1 + 1 ] ;
				this.gc2gc_size = m1 ;
			}
			
			Movem( c1, c1_o + 0, this.gc2gc_buff, 0, m1 + 1 ) ;
			
			c2[ c2_o + 0 ] = this.gc2gc_buff[ 0 ] ;

			for( i  = 1 ; i <= m2 ; i ++ )
			{
				ss1 = ss2 = 0.0 ;
				min = m1 < i ? m1 : i - 1 ;

				for( k  = 1 ; k <= min ; k ++ )
				{
					mk = i - k ;
					cc = this.gc2gc_buff[ k ] * c2[ c2_o + mk ] ;
					ss2 += k  * cc ;
					ss1 += mk * cc ;
				}
				
				if( i <= m1 )
				{
					c2[ c2_o + i ] = this.gc2gc_buff[ i ] + ( g2 * ss2 - g1 * ss1 ) / i ;
				}
				else
				{
					c2[ c2_o + i ] = ( g2 * ss2 - g1 * ss1 ) / i ;
				}
			}
		}
		
		// HTS_freqt: frequency transformation
		private void Freqt( double[] c1, int c1_o, int m1, double[] c2, int c2_o, int m2, double a )
		{
			int i, j ;
			double b = 1 - a * a ;

			// オフセット操作
//			double[] g ;
			int	g ;
			
			if( m2 >  this.freqt_size )
			{
				if( this.freqt_buff != null )
				{
					this.freqt_buff  = null ;
				}
				
				this.freqt_buff = new double[ m2 + m2 + 2 ] ;
				this.freqt_size = m2 ;
			}

			// オフセット操作
//			g = this.freqt_buff + this.freqt_size + 1 ;
			g = this.freqt_size + 1 ;

			for( i  = 0 ; i <  m2 + 1 ; i ++ )
			{
				// オフセット操作
//				g[ i ] = 0.0 ;
				this.freqt_buff[ g + i ] = 0.0 ;
			}
			
			for( i  = - m1 ; i <= 0 ; i ++ )
			{
				if( 0 <= m2 )
				{
					// オフセット操作
//					g[ 0 ] = c1[ -i ] + a * ( this.freqt_buff[ 0 ] = g[ 0 ] ) ;
					this.freqt_buff[ 0 ] = this.freqt_buff[ g + 0 ] ;
					this.freqt_buff[ g + 0 ] = c1[ c1_o - i ] + a * this.freqt_buff[ 0 ] ;
				}

				if( 1 <= m2 )
				{
					// オフセット操作
//					g[ 1 ] = b * this.freqt_buff[ 0 ] + a * ( this.freqt_buff[ 1 ] = g[ 1 ] ) ;
					this.freqt_buff[ 1 ] = this.freqt_buff[ g + 1 ] ;
					this.freqt_buff[ g + 1 ] = b * this.freqt_buff[ 0 ] + a * this.freqt_buff[ 1 ] ;
				}

				for( j  = 2 ; j <= m2 ; j ++ )
				{
					// オフセット操作
//					g[ j ] = this.freqt_buff[ j - 1 ] + a * ( ( this.freqt_buff[ j ] = g[ j ] ) - g[ j - 1 ] ) ;
					this.freqt_buff[ j ] = this.freqt_buff[ g + j ] ;
					this.freqt_buff[ g + j ] = this.freqt_buff[ j - 1 ] + a * ( ( this.freqt_buff[ j ] ) - this.freqt_buff[ g + j - 1 ] ) ;
				}
			}

			// オフセット操作
//			HTS_movem( g, c2, m2 + 1 ) ;
			Movem( this.freqt_buff, g, c2, c2_o, m2 + 1 ) ;
		}
		
		// HTS_Vocoder_postfilter_mcp: postfilter for MCP
		private void PostfilterMcp( double[] mcp, int m, double alpha, double beta )
		{
			double e1, e2 ;
			int k ;
			
			if( beta >  0.0 && m >  1 )
			{
				if( this.postfilter_size <  m )
				{
					if( this.postfilter_buff != null )
					{
						this.postfilter_buff  = null ;
					}
					
					this.postfilter_buff = new double[ m + 1 ] ;
					this.postfilter_size = m ;
				}
				
				Mc2b( mcp, 0, this.postfilter_buff, 0, m, alpha ) ;

				e1 = B2en( this.postfilter_buff, m, alpha ) ;

				this.postfilter_buff[ 1 ] -= beta * alpha * this.postfilter_buff[ 2 ] ;
				
				for( k  = 2 ; k <= m ; k ++ )
				{
					this.postfilter_buff[ k ] *= ( 1.0 + beta ) ;
				}
				
				e2 = B2en( this.postfilter_buff, m, alpha ) ;

				this.postfilter_buff[ 0 ] += Math.Log( e1 / e2 ) / 2 ;

				B2mc( this.postfilter_buff, mcp, m, alpha ) ;
			}
		}
		
		// HTS_b2en: calculate frame energy
		private double B2en( double[] b, int m, double a )
		{
			int i ;
			double en = 0.0 ;

			// オフセット操作
//			double[] cep ;
//			double[] ir ;
			int cep ;
			int ir ;

			if( this.spectrum2en_size <  m )
			{
				if( this.spectrum2en_buff != null )
				{
					this.spectrum2en_buff  = null ;
				}

				this.spectrum2en_buff = new double[ ( m + 1 ) + 2 * IRLENG ] ;
				this.spectrum2en_size = m ;
			}
			
			// オフセット操作
//			cep = this.spectrum2en_buff + m + 1 ;
			cep = m + 1 ;

			// オフセット操作
//			ir = cep + IRLENG ;
			ir = cep + IRLENG ;

			B2mc( b, this.spectrum2en_buff, m, a ) ;
			
			// オフセット操作(m=34 cep=35 IRLENG=576
//			Freqt( this.spectrum2en_buff, m, cep, IRLENG - 1, -a ) ;
			Freqt( this.spectrum2en_buff, 0, m, this.spectrum2en_buff, cep, IRLENG - 1, - a ) ;

			// オフセット操作
//			C2ir( cep, IRLENG, ir, IRLENG ) ;
			C2ir( this.spectrum2en_buff, cep, IRLENG, this.spectrum2en_buff, ir, IRLENG ) ;

			for( i  = 0 ; i <  IRLENG ; i ++ )
			{
				// オフセット操作
//				en += ir[ i ] * ir[ i ] ;
				en += this.spectrum2en_buff[ ir + i ] * this.spectrum2en_buff[ ir + i ] ;
			}

			return en ;
		}

		// HTS_b2bc: transform MLSA digital filter coefficients to mel-cepstrum
		private void B2mc( double[] b, double[] mc, int m, double a )
		{
			double d, o ;
			
			d = mc[ m ] = b[ m ] ;

			for( m -- ; m >= 0 ; m -- )
			{
				o = b[ m ] + a * d ;
				d = b[ m ] ;
				mc[ m ] = o ;
			}
		}

		// HTS_c2ir: The minimum phase impulse response is evaluated from the minimum phase cepstrum
		private void C2ir( double[] c, int c_o, int nc, double[] h, int h_o, int leng )
		{
			int n, k, upl ;
			double d ;
			
			h[ h_o + 0 ] = Math.Exp( c[ c_o + 0 ] ) ;

			for( n  = 1 ; n <  leng ; n ++ )
			{
				d = 0 ;
				upl = ( n >= nc ) ? nc - 1 : n ;

				for( k  = 1 ; k <= upl ; k ++ )
				{
					d += k * c[ c_o + k ] * h[ h_o + n - k ] ;
				}

				h[ h_o + n ] = d / n ;
			}
		}
		
		// HTS_Vocoder_postfilter_lsp: postfilter for LSP
		private void PostfilterLsp( double[] lsp, int m, double alpha, double beta )
		{
			double e1, e2 ;
			int i ;
			double d1, d2 ;

			if( beta >  0.0 && m >  1 )
			{
				if( this.postfilter_size <  m )
				{
					if( this.postfilter_buff != null )
					{
						this.postfilter_buff  = null ;
					}
					
					this.postfilter_buff = new double[ m + 1 ] ;
					this.postfilter_size = m ;
				}
				
				e1 = Lsp2en( lsp, 0, m, alpha ) ;
				
				// postfiltering
				for( i  = 0 ; i <= m ; i ++ )
				{
					if( i >  1 && i <  m )
					{
						d1 = beta * ( lsp[ i + 1 ] - lsp[ i ]     ) ;
						d2 = beta * ( lsp[ i ]     - lsp[ i - 1 ] ) ;
						this.postfilter_buff[ i ] = lsp[ i - 1 ] + d2 + ( d2 * d2 * ( ( lsp[ i + 1 ] - lsp[ i - 1 ] ) - ( d1 + d2 ) ) ) / ( ( d2 * d2 ) + ( d1 * d1 ) ) ;
					}
					else
					{
						this.postfilter_buff[ i ] = lsp[ i ] ;
					}
				}

				Movem( this.postfilter_buff, 0, lsp, 0, m + 1 ) ;
				
				e2 = Lsp2en( lsp, 0, m, alpha ) ;
				
				if( e1 != e2 )
				{
					if( this.use_log_gain == true )
					{
						lsp[ 0 ] += 0.5 * Math.Log( e1 / e2 ) ;
					}
					else
					{
						lsp[ 0 ] *= Math.Sqrt( e1 / e2 ) ;
					}
				}
			}
		}
		
		// HTS_lsp2en: calculate frame energy
		private double Lsp2en( double[] lsp, int lsp_o, int m, double alpha )
		{
			int i ;
			double en = 0.0 ;

			// オフセット操作
//			double[] buff ;
			int buff ;
			
			if( this.spectrum2en_size <  m )
			{
				if( this.spectrum2en_buff != null )
				{
					this.spectrum2en_buff  = null ;
				}
				
				this.spectrum2en_buff = new double[ m + 1 + IRLENG ] ;
				this.spectrum2en_size = m ;
			}

			// オフセット操作
//			buff = this.spectrum2en_buff + m + 1 ;
			buff = m + 1 ;
			
			// lsp2lpc
			// オフセット操作
//			Lsp2lpc( lsp + 1, this.spectrum2en_buff, m ) ;
			Lsp2lpc( lsp, lsp_o + 1, this.spectrum2en_buff, 0, m ) ;

			if( this.use_log_gain == true )
			{
				this.spectrum2en_buff[ 0 ] = Math.Exp( lsp[ lsp_o + 0 ] ) ;
			}
			else
			{
				this.spectrum2en_buff[ 0 ] = lsp[ lsp_o + 0 ] ;
			}
			
			// mgc2mgc
			if( NORMFLG1 )
			{
				Ignorm( this.spectrum2en_buff, 0, this.spectrum2en_buff, 0, m, this.gamma ) ;
			}
//			else
//			if( MULGFLG1 )
//			{
//				this.spectrum2en_buff[ 0 ] = ( 1.0 - this.spectrum2en_buff[ 0 ] ) * ( ( double )this.stage ) ;
//			}
			
			if( MULGFLG1 )
			{
				for( i  = 1 ; i <= m ; i ++ )
				{
					this.spectrum2en_buff[ i ] *= -( ( double )this.stage ) ;
				}
			}

			// オフセット操作
//			Mgc2mgc( this.spectrum2en_buff, m, alpha, this.gamma, buff, IRLENG - 1, 0.0, 1 ) ;
			Mgc2mgc( this.spectrum2en_buff, 0, m, alpha, this.gamma, this.spectrum2en_buff, buff, IRLENG - 1, 0.0, 1 ) ;
			
			for( i  = 0 ; i <  IRLENG ; i ++ )
			{
				// オフセット操作
//				en += buff[ i ] * buff[ i ] ;
				en += this.spectrum2en_buff[ buff + i ] * this.spectrum2en_buff[ buff + i ] ;
			}

			return en ;
		}
		
		// THS_check_lsp_stability: check LSP stability
		private void CheckLspStability( double[] lsp, int m )
		{
			int i, j ;
			double tmp ;
			double min = ( CHECK_LSP_STABILITY_MIN * PI ) / ( m + 1 ) ;
			bool find ;

			for( i  = 0 ; i <  CHECK_LSP_STABILITY_NUM ; i ++ )
			{
				find = false ;
				
				for( j  = 1 ; j <  m ; j ++ )
				{
					tmp = lsp[ j + 1 ] - lsp[ j ] ;
					if( tmp <  min )
					{
						lsp[ j     ] -= 0.5 * ( min - tmp ) ;
						lsp[ j + 1 ] += 0.5 * ( min - tmp ) ;
						find = true ;
					}
				}
				
				if( lsp[ 1 ] <  min )
				{
					lsp[ 1 ] = min ;
					find = true ;
				}
				
				if( lsp[ m ] >  PI - min )
				{
					lsp[ m ]  = PI - min ;
					find = true ;
				}
				
				if( find == false )
				{
					break ;
				}
			}
		}

		// HTS_Vocoder_get_excitation: get excitation of each sample
		private double GetExcitation( double[] lpf )
		{
			double x ;
			double noise, pulse = 0.0 ;
			
			if( this.excite_buff_size >  0 )
			{
				noise = WhiteNoise() ;
				pulse = 0.0 ;

				if( this.pitch_of_curr_point == 0.0 )
				{
					ExciteUnvoicedFrame( noise ) ;
				}
				else
				{
					this.pitch_counter += 1.0 ;
					if( this.pitch_counter >= this.pitch_of_curr_point )
					{
						pulse = Math.Sqrt( this.pitch_of_curr_point ) ;
						this.pitch_counter -= this.pitch_of_curr_point ;
					}

					ExciteVoicedFrame( noise, pulse, lpf ) ;
					this.pitch_of_curr_point += this.pitch_inc_per_point ;
				}

				x = this.excite_ring_buff[ this.excite_buff_index ] ;

				this.excite_ring_buff[ this.excite_buff_index ] = 0.0 ;
				this.excite_buff_index ++ ;

				if( this.excite_buff_index >= this.excite_buff_size )
				{
					this.excite_buff_index  = 0 ;
				}
			}
			else
			{
				if( this.pitch_of_curr_point == 0.0 )
				{
					x = WhiteNoise() ;
				}
				else
				{
					this.pitch_counter += 1.0 ;
					if( this.pitch_counter >= this.pitch_of_curr_point )
					{
						x = Math.Sqrt( this.pitch_of_curr_point ) ;
						this.pitch_counter -= this.pitch_of_curr_point ;
					}
					else
					{
						x = 0.0 ;
					}

					this.pitch_of_curr_point += this.pitch_inc_per_point ;
				}
			}
			
			return x ;
		}


		// HTS_white_noise: return white noise
		private double WhiteNoise()
		{
			if( this.gauss == true )
			{
				return ( double )Nrandom() ;
			}
			else
			{
				return ( double )Mseq() ;
			}
		}

		// HTS_nrandom: functions for gaussian random noise generation
		private double Nrandom()
		{
			if( this.sw == 0 )
			{
				this.sw = 1 ;
				do
				{
					this.r1 = 2 * Rnd( ref this.next ) - 1 ;
					this.r2 = 2 * Rnd( ref this.next ) - 1 ;
					this.s = this.r1 * this.r1 + this.r2 * this.r2 ;
				}
				while( this.s >  1 || this.s == 0 ) ;

				this.s = Math.Sqrt( -2 * Math.Log( this.s ) / this.s ) ;
				return ( this.r1 * this.s ) ;
			}
			else
			{
				this.sw = 0 ;
				return ( this.r2 * this.s ) ;
			}
		}

		// HTS_rnd: functions for random noise generation
		private double Rnd( ref ulong next )
		{
			double r ;
			
			next = next * 1103515245L + 12345 ;
			r = ( next / 65536L ) % 32768L ;
			
			return ( r / RANDMAX ) ;
		}

		// HTS_mceq: function for M-sequence random noise generation
		private int Mseq()
		{
			int x0, x28 ;
			
			this.x >>= 1 ;
			
			if( ( this.x & B0 ) != 0 )
			{
				x0 =  1 ;
			}
			else
			{
				x0 = -1 ;
			}
			
			if( ( this.x & B28 ) != 0 )
			{
				x28 =  1 ;
			}
			else
			{
				x28 = -1 ;
			}
			
			if( ( x0 + x28 ) != 0 )
			{
				this.x &= B31_ ;
			}
			else
			{
				this.x |= B31 ;
			}

		   return x0 ;
		}

		// HTS_Vocoder_excite_unvoiced_frame: ping noise to ring buffer
		private void ExciteUnvoicedFrame( double noise )
		{
			int center = ( this.excite_buff_size - 1 ) / 2 ;
			this.excite_ring_buff[ ( this.excite_buff_index + center ) % this.excite_buff_size ] += noise ;
		}
		
		// HTS_Vocoder_excite_vooiced_frame: ping noise and pulse to ring buffer
		private void ExciteVoicedFrame( double noise, double pulse, double[] lpf )
		{
			int i ;
			int center = ( this.excite_buff_size - 1 ) / 2 ;
			
			if( noise != 0.0 )
			{
				for( i  = 0 ; i <  this.excite_buff_size ; i ++ )
				{
					if( i == center )
					{
						this.excite_ring_buff[ ( this.excite_buff_index + i ) % this.excite_buff_size ] += noise * ( 1.0 - lpf[ i ] ) ;
					}
					else
					{
						this.excite_ring_buff[ ( this.excite_buff_index + i ) % this.excite_buff_size ] += noise * ( 0.0 - lpf[ i ] ) ;
					}
				}
			}

			if( pulse != 0.0 )
			{
				for( i  = 0 ; i <  this.excite_buff_size ; i ++ )
				{
					this.excite_ring_buff[ ( this.excite_buff_index + i ) % this.excite_buff_size ] += pulse * lpf[ i ] ;
				}
			}
		}

		private static double[] pade =
		{
		   1.00000000000,
		   1.00000000000,
		   0.00000000000,
		   1.00000000000,
		   0.00000000000,
		   0.00000000000,
		   1.00000000000,
		   0.00000000000,
		   0.00000000000,
		   0.00000000000,
		   1.00000000000,
		   0.49992730000,
		   0.10670050000,
		   0.01170221000,
		   0.00056562790,
		   1.00000000000,
		   0.49993910000,
		   0.11070980000,
		   0.01369984000,
		   0.00095648530,
		   0.00003041721
		} ;
		
		// HTS_mlsadf: functions for MLSA filter
		private double Mlsadf( double x, double[] b, int m, double a, int pd, double[] d, int d_o )
		{
			double aa = 1 - a * a ;

			// オフセット操作
//			double *ppade = &( pade[ pd * ( pd + 1 ) / 2 ] ) ;
			int ppade = pd * ( pd + 1 ) / 2 ;

			x = Mlsadf1( x, b, m, a, aa, pd, d, d_o,                  pade, ppade ) ;
			x = Mlsadf2( x, b, m, a, aa, pd, d, d_o + 2 * ( pd + 1 ), pade, ppade ) ;
			
			return x ;
		}
		
		// HTS_mlsadf1: sub functions for MLSA filter
		private double Mlsadf1( double x, double[] b, int m, double a, double aa, int pd, double[] d, int d_o, double[] ppade, int ppade_o )
		{
			double v, tOut = 0.0 ;

			// オフセット操作
//			double[] pt ;
			int	pt ;

			int i ;
			
			// オフセット操作
//			pt = &d[ pd + 1 ] ;
			pt = d_o + pd + 1 ;

			for( i  = pd ; i >= 1 ; i -- )
			{
				// オフセット操作
//				d[ i ] = aa * pt[ i - 1 ] + a * d[ i ] ;
				d[ d_o + i ] = aa * d[ pt + i - 1 ] + a * d[ d_o + i ] ;

				// オフセット操作
//				pt[ i ] = d[ i ] * b[ 1 ] ;
				d[ pt + i ] = d[ d_o + i ] * b[ 1 ] ;
				
				// オフセット操作
//				v = pt[ i ] * ppade[ i ] ;
				v = d[ pt + i ] * ppade[ ppade_o + i ] ;

				x += ( 1 & i ) != 0 ?  v : -v ;

				tOut += v ;
			}

			// オフセット操作
//			pt[ 0 ] = x ;
			d[ pt + 0 ] = x ;

			tOut += x ;
			
			return tOut ;
		}
		
		// HTS_mlsadf2: sub functions for MLSA filter
		private double Mlsadf2( double x, double[] b, int m, double a, double aa, int pd, double[] d, int d_o, double[] ppade, int ppade_o )
		{
			double v, tOut = 0.0 ;

			// オフセット操作
//			double[] pt ;
			int pt ;

			int i ;

			// オフセット操作
//			pt = &d[ pd * ( m + 2 ) ] ;
			pt = d_o + pd * ( m + 2 ) ;
			
			for( i  = pd ; i >= 1 ; i -- )
			{
				// オフセット操作
//				pt[ i ] = Mlsafir( pt[ i - 1 ], b, m, a, aa, &d[ ( i - 1 ) * ( m + 2 ) ] ) ;
				d[ pt + i ] = Mlsafir( d[ pt + i - 1 ], b, m, a, aa, d, d_o + ( i - 1 ) * ( m + 2 ) ) ;
				
				// オセット操作
//				v = pt[ i ] * ppade[ i ] ;
				v = d[ pt + i ] * ppade[ ppade_o + i ] ;

				x += ( 1 & i ) != 0 ?  v : -v;
				tOut += v ;
			}
			
			// オフセット操作
			d[ pt + 0 ] = x ;
			tOut += x ;
			
			return tOut ;
		}

		// HTS_mlsafir: sub functions for MLSA filter
		private double Mlsafir( double x, double[] b, int m, double a, double aa, double[] d, int d_o )
		{
			double y = 0.0 ;
			int i ;
			
			d[ d_o + 0 ] = x ;
			d[ d_o + 1 ] = aa * d[ d_o + 0 ] + a * d[ d_o + 1 ] ;
			
			for( i  = 2 ; i <= m ; i ++ )
			{
				d[ d_o + i ] += a * ( d[ d_o + i + 1 ] - d[ d_o + i - 1 ] ) ;
			}
			
			for( i  = 2 ; i <= m ; i ++ )
			{
				y += d[ d_o + i ] * b[ i ] ;
			}
			
			for( i  = m + 1 ; i >  1 ; i -- )
			{
				d[ d_o + i ] = d[ d_o + i - 1 ] ;
			}
			
			return y ;
		}

		// HTS_mglsadf: sub functions for MGLSA filter
		private double Mglsadf( double x, double[] b, int m, double a, int n, double[] d, int d_o )
		{
			int i ;
			
			for( i  = 0 ; i <  n ; i ++ )
			{
				// オフセット操作
//				x = Mglsadff( x, b, m, a, &d[ i * ( m + 1 ) ] ) ;
				x = Mglsadff( x, b, m, a, d, d_o + i * ( m + 1 ) ) ;
			}
			
			return x ;
		}

		// HTS_mglsadff: sub functions for MGLSA filter
		private double Mglsadff( double x, double[] b, int m, double a, double[] d, int d_o )
		{
			int i ;
			double y ;

			y = d[ d_o + 0 ] * b[ 1 ] ;

			for( i  = 1 ; i <  m ; i ++ )
			{
				d[ d_o + i ] += a * ( d[ d_o + i + 1 ] - d[ d_o + i - 1 ] ) ;
				y += d[ d_o + i ] * b[ i + 1 ] ;
			}
			x -= y ;
			
			for( i  = m ; i >  0 ; i -- )
			{
				d[ d_o + i ] = d[ d_o + i - 1 ] ;
			}
			
			d[ d_o + 0 ] = a * d[ d_o + 0 ] + ( 1 - a * a ) * x ;
			
			return x ;
		}


		// HTS_Vocoder_start_excitation: start excitation of each frame
		private void StartExcitation( double pitch )
		{
			if( this.pitch_of_curr_point != 0.0 && pitch != 0.0 )
			{
				this.pitch_inc_per_point = ( pitch - this.pitch_of_curr_point ) / this.fprd ;
			}
			else
			{
				this.pitch_inc_per_point	= 0.0 ;
				this.pitch_of_curr_point	= pitch ;
				this.pitch_counter			= pitch ;
			}
		}

		// HTS_Vocoder_end_excitation: end excitation of each frame
		private void EndExcitation( double pitch )
		{
			this.pitch_of_curr_point = pitch ;
		}
	}
}
