using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Text ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Window
	{
		public int			size ;					// # of windows (static + deltas)
		public int[]		l_width ;				// left width of windows
		public int[]		r_width ;				// right width of windows
		public double[][]	coefficient ;			// window coefficient		ポインタを動かしているので注意
		public int[]		coefficient_offset ;
		public int			max_width ;				// maximum width of windows

		//-----------------------------------------------------------

		public HTS_Window()
		{
			Initialize() ;
		}

		private void Initialize()
		{
			size		= 0 ;

			l_width		= null ;
			r_width		= null ;
			coefficient	= null ;

			max_width	= 0 ;
		}

		// HTS_Window_load: load dynamic windows
		public bool Load( HTS_File[] fp, int size )
		{
			int i, j ;
			int fsize, length ;
			string tToken ;
			bool result = true ;

			Initialize() ;

			// check
			if( fp == null || size == 0 )
			{
				return false ;
			}
			
			this.size			= size ;
			this.l_width		= new int[ this.size ] ;
			this.r_width		= new int[ this.size ] ;
			this.coefficient	= new double[ this.size ][] ;
			this.coefficient_offset = new int[ this.size ] ;	// オフセットを動かしているので注意
			
			// set delta coefficents
			for( i  = 0 ; i <  this.size ; i ++ )
			{
				tToken = fp[ i ].GetToken() ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					result = false ;
					fsize = 1 ;
				}
				else
				{
					int.TryParse( tToken, out fsize ) ;
					if( fsize == 0 )
					{
						result = false ;
						fsize = 1 ;
					}
				}

				// read coefficients
				this.coefficient[ i ] = new double[ fsize ] ;
				for( j  = 0 ; j <  fsize ; j ++ )
				{
					tToken = fp[ i ].GetToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						result = false ;
						this.coefficient[ i ][ j ] = 0.0 ;
					}
					else
					{
						double.TryParse( tToken, out this.coefficient[ i ][ j ] ) ;
					}
				}
				
				// set pointer
				length = fsize / 2 ;
//				this.coefficient[ i ] += length ;
				this.coefficient_offset[ i ] = length ;	// オフセットを動かしているので注意
				this.l_width[ i ] = -1 * ( int )length ;
				this.r_width[ i ] =      ( int )length ;
				if( fsize % 2 == 0 )
				{
					this.r_width[ i ] -- ;
				}
			}

			// calcurate max_width to determine size of band matrix
			this.max_width = 0 ;
			for( i  = 0 ; i <  this.size ; i ++ )
			{
				if( this.max_width <  Math.Abs( this.l_width[ i ] ) )
				{
					this.max_width  = Math.Abs( this.l_width[ i ] ) ;
				}
				if( this.max_width <  Math.Abs( this.r_width[ i ] ) )
				{
					this.max_width  = Math.Abs( this.r_width[ i ] ) ;
				}
			}
			
			if( result == false )
			{
				Initialize() ;
				return false ;
			}

			return true ;
		}
	}

}
