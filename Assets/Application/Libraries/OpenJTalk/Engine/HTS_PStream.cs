using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;

namespace HTS_Engine_API
{
	// HTS_PStream: individual PDF stream
	public class HTS_PStream
	{
		public int				vector_length ;				// vector length (static features only)
		public int				length ;					// stream length
		public int				width ;						// width of dynamic window
		public double[][]		par ;						// output parameter vector
		public HTS_SMatrices	sm = new HTS_SMatrices() ;	// matrices for parameter generation
		public int				win_size ;					// # of windows (static + deltas)
		public int[]			win_l_width ;				// left width of windows
		public int[]			win_r_width ;				// right width of windows
		public double[][]		win_coefficient ;			// window coefficients
		public int[]			win_coefficient_offset ;	// window coefficients
		public bool[]			msd_flag ;					// Boolean sequence for MSD
		public double[]			gv_mean ;					// mean vector of GV
		public double[]			gv_vari ;					// variance vector of GV
		public bool[]			gv_switch ;					// GV flag sequence
		public int				gv_length ;					// frame length for GV calculation

		//---------------------------------------------------------------------------

		private const double	STEPINIT			= 0.1 ;
		private const double	STEPDEC				= 0.5 ;
		private const double	STEPINC				= 1.2 ;
		private const double	W1					= 1.0 ;
		private const double	W2					= 1.0 ;
		private const double	GV_MAX_ITERATION	= 5 ;
		
		//---------------------------------------------------------------------------

		// HTS_PStream_mlpg: generate sequence of speech parameter vector maximizing its output probability for given pdf sequence
		public void Mlpg()
		{
			int m ;
			
			if( this.length == 0 )
			{
				return ;
			}
			
			for( m  = 0 ; m <  this.vector_length ; m ++ )
			{
				CalcWuwAndWum( m ) ;

				LdlFactorization() ;			// LDL factorization

				ForwardSubstitution() ;			// forward substitution

				BackwardSubstitution( m ) ;		// backward substitution

				if( this.gv_length >  0 )
				{
					GvParmgen( m ) ;
				}
			}
		}

		// HTS_PStream_calc_wuw_and_wum: calcurate W'U^{-1}W and W'U^{-1}M
		private void CalcWuwAndWum( int m )
		{
			int t, i, j ;
			int shift ;
			double wu ;
			
			for( t  = 0 ; t <  this.length ; t ++ )
			{
				// initialize
				this.sm.wum[ t ] = 0.0 ;
				for( i  = 0 ; i <  this.width ; i ++ )
				{
					this.sm.wuw[ t ][ i ] = 0.0 ;
				}
				
				// calc WUW & WUM
				for( i  = 0 ; i <  this.win_size ; i ++ )
				{
					for( shift  = this.win_l_width[ i ] ; shift <= this.win_r_width[ i ] ; shift ++ )
					{
						if( ( ( int )t + shift >= 0 ) && ( ( int ) t + shift <  this.length ) && ( this.win_coefficient[ i ][ this.win_coefficient_offset[ i ] - shift ] != 0.0 ) )
						{
							wu = this.win_coefficient[ i ][ this.win_coefficient_offset[ i ] - shift ] * this.sm.ivar[ t + shift ][ i * this.vector_length + m ] ;
							
							this.sm.wum[ t ] += wu * this.sm.mean[ t + shift ][ i * this.vector_length + m ] ;
							
							for( j  = 0 ; ( j <  this.width ) && ( t + j <  this.length ) ; j ++ )
							{
								if( ( ( int )j <= this.win_r_width[ i ] + shift ) && ( this.win_coefficient[ i ][ this.win_coefficient_offset[ i ] + j - shift ] != 0.0 ) )
								{
									this.sm.wuw[ t ][ j ] += wu * this.win_coefficient[ i ][ this.win_coefficient_offset[ i ] + j - shift ] ;
								}
							}
						}
					}
				}
			}
		}

		// HTS_PStream_ldl_factorization: Factorize W'*U^{-1}*W to L*D*L' (L: lower triangular, D: diagonal)
		private void LdlFactorization()
		{
			int t, i, j ;
			
			for( t  = 0; t <  this.length ; t ++ )
			{
				for( i  = 1 ; ( i <  this.width ) && ( t >= i ) ; i ++ )
				{
					this.sm.wuw[ t ][ 0 ] -= this.sm.wuw[ t - i ][ i ] * this.sm.wuw[ t - i ][ i ] * this.sm.wuw[ t - i ][ 0 ] ;
				}

				for( i  = 1 ; i <  this.width ; i ++ )
				{
					for( j  = 1 ; ( i + j <  this.width ) && ( t >= j ) ; j ++ )
					{
						this.sm.wuw[ t ][ i ] -= this.sm.wuw[ t - j ][ j ] * this.sm.wuw[ t - j ][ i + j ] * this.sm.wuw[ t - j ][ 0 ] ;
					}
					this.sm.wuw[ t ][ i ] /= this.sm.wuw[ t ][ 0 ] ;
				}
			}
		}


		// HTS_PStream_forward_substitution: forward subtitution for mlpg
		private void ForwardSubstitution()
		{
			int t, i ;
			
			for( t  = 0 ; t <  this.length ; t ++ )
			{
				this.sm.g[ t ] = this.sm.wum[ t ] ;
				for( i  = 1 ; ( i <  this.width ) && ( t >= i ) ; i ++ )
				{
					this.sm.g[ t ] -= this.sm.wuw[ t - i ][ i ] * this.sm.g[ t - i ] ;
				}
			}
		}


		// HTS_PStream_backward_substitution: backward subtitution for mlpg
		private void BackwardSubstitution( int m )
		{
			int rev, t, i ;
			
			for( rev  = 0 ; rev <  this.length ; rev ++ )
			{
				t = this.length - 1 - rev ;

				this.par[ t ][ m ] = this.sm.g[ t ] / this.sm.wuw[ t ][ 0 ] ;

				for( i  = 1 ; ( i <  this.width ) && ( t + i <  this.length ) ; i ++ )
				{
					this.par[ t ][ m ] -= this.sm.wuw[ t ][ i ] * this.par[ t + i ][ m ] ;
				}
		   }
		}


		// HTS_PStream_gv_parmgen: function for mlpg using GV
		private void GvParmgen( int m )
		{
			int t, i ;
			double step = STEPINIT ;
			double prev = 0.0 ;
			double obj ;

			if( this.gv_length == 0 )
			{
				return ;
			}

			ConvGv( m ) ;

			if( GV_MAX_ITERATION >  0 )
			{
				CalcWuwAndWum( m ) ;

				for( i  = 1 ; i <= GV_MAX_ITERATION ; i ++ )
				{
					obj = CalcDerivative( m ) ;
					if( i >  1 )
					{
						if( obj >  prev )
						{
							step *= STEPDEC ;
						}
						if( obj <  prev )
						{
							step *= STEPINC ;
						}
					}

					for( t  = 0 ; t <  this.length ; t ++ )
					{
						if( this.gv_switch[ t ] == true )
						{
							this.par[ t ][ m ] += step * this.sm.g[ t ] ;
						}
					}
					prev = obj ;
				}
			}
		}

		// HTS_PStream_conv_gv: subfunction for mlpg using GV
		private void ConvGv( int m )
		{
			int t ;
			double ratio ;
			double mean = 0 ;
			double vari = 0 ;
			
			CalcGv( m, ref mean, ref vari ) ;

			ratio = Math.Sqrt( this.gv_mean[ m ] / vari ) ;

			for( t  = 0 ; t <  this.length ; t ++ )
			{
				if( this.gv_switch[ t ] == true )
				{
					this.par[ t ][ m ] = ratio * ( this.par[ t ][ m ] - mean ) + mean ;
				}
			}
		}

		// HTS_PStream_calc_gv: subfunction for mlpg using GV
		private void CalcGv( int m, ref double mean, ref double vari )
		{
			int t ;
			int c ;
			
			mean = 0.0 ;
			c = 0 ;

			for( t  = 0 ; t <  this.length ; t ++ )
			{
				if( this.gv_switch[ t ] == true )
				{
					mean += this.par[ t ][ m ] ;
					c ++ ;
				}
			}

			mean /= ( double )this.gv_length ;

			vari = 0.0 ;
			c = 0 ;

			for( t  = 0 ; t <  this.length ; t ++ )
			{
				if( this.gv_switch[ t ] == true )
				{
					vari += ( this.par[ t ][ m ] - mean ) * ( this.par[ t ][ m ] - mean ) ;
					c ++ ;
				}
			}

			vari /= ( double )this.gv_length ;
		}

		// HTS_PStream_calc_derivative: subfunction for mlpg using GV
		private double CalcDerivative( int m )
		{
			int t, i ;
			double mean = 0 ;
			double vari = 0 ;
			double dv ;
			double h ;
			double gvobj ;
			double hmmobj ;
			double w = 1.0 / ( this.win_size * this.length ) ;
			
			CalcGv( m, ref mean, ref vari ) ;
			gvobj	= -0.5 * W2 * vari * this.gv_vari[ m ] * ( vari - 2.0 * this.gv_mean[ m ] ) ;
			dv		= -2.0 * this.gv_vari[ m ] * ( vari - this.gv_mean[ m ] ) / this.length ;
			
			for( t  = 0 ; t <  this.length ; t ++ )
			{
				this.sm.g[ t ] = this.sm.wuw[ t ][ 0 ] * this.par[ t ][ m ] ;
				for( i  = 1 ; i <  this.width ; i ++ )
				{
					if( t + i <  this.length )
					{
						this.sm.g[ t ] += this.sm.wuw[ t ][ i ] * this.par[ t + i ][ m ] ;
					}
					if( t + 1 >  i )
					{
						this.sm.g[ t ] += this.sm.wuw[ t - i ][ i ] * this.par[ t - i ][ m ] ;
					}
				}
			}
			
			for( t  = 0, hmmobj  = 0.0 ; t <  this.length ; t ++ )
			{
				hmmobj += W1 * w * this.par[ t ][ m ] * ( this.sm.wum[ t ] - 0.5 * this.sm.g[ t ] ) ;
				h = -W1 * w * this.sm.wuw[ t ][ 1 - 1 ] - W2 * 2.0 / ( this.length * this.length ) * ( ( this.length - 1 ) * this.gv_vari[ m ] * ( vari - this.gv_mean[ m ] ) + 2.0 * this.gv_vari[ m ] * ( this.par[ t ][ m ] - mean ) * ( this.par[ t ][ m ] - mean ) ) ;

				if( this.gv_switch[ t ] == true )
				{
					this.sm.g[ t ] = 1.0 / h * ( W1 * w * ( - this.sm.g[ t ] + this.sm.wum[ t ] ) + W2 * dv * ( this.par[ t ][ m ] - mean ) ) ;
				}
				else
				{
					this.sm.g[ t ] = 1.0 / h * ( W1 * w * ( - this.sm.g[ t ] + this.sm.wum[ t ] ) ) ;
				}
			}
			
			return ( - ( hmmobj + gvobj ) ) ;
		}
	}
}
