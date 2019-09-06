using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Label
	{
		private HTS_LabelString	head ;				// pointer to the head of label string
		private int				size ;				// # of label strings

		//-----------------------------------------------------------

		public HTS_Label()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			head		= null ;
			size		= 0 ;
		}
		
		// HTS_Label_load_from_strings: load label from strings
		public void LoadFromStrings( int tSamplingRate, int fperiod, string[] lines )
		{
			string tToken ;
			HTS_LabelString lstring = null ;
			double start, end ;
			int i, l ;

			double tRate = ( double )tSamplingRate / ( ( double )fperiod * 1e+7 ) ;
			
			if( this.head != null || this.size != 0 )
			{
				Debug.LogError( "HTS_Label_load_from_fp: label list is not initialized." ) ;
				return ;
			}

			// copy label
			l = lines.Length ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( IsGraph( lines[ i ][ 0 ] ) == false )
				{
					break ;
				}

				this.size ++ ;
				
				if( lstring != null )
				{
					lstring.next = new HTS_LabelString() ;
					lstring = lstring.next ;
				}
				else
				{
					// first time
					lstring = new HTS_LabelString() ;
					this.head = lstring ;
				}
				
				if( IsDigitString( lines[ i ] ) == true )
				{
					// has frame infomation
					tToken = GetTokenFromString( lines[ i ] ) ;
					double.TryParse( tToken, out start ) ;

					tToken = GetTokenFromString( lines[ i ] ) ;
					double.TryParse( tToken, out end ) ;

					tToken = GetTokenFromString( lines[ i ] ) ;
					
					lstring.name	= tToken ;
					lstring.start	= tRate * start ;
					lstring.end		= tRate * end ;
				}
				else
				{
					lstring.name	= lines[ i ] ;
					lstring.start	= -1.0 ;
					lstring.end		= -1.0 ;
				}

				lstring.next = null ;
			}

			CheckTime() ;
		}

		// HTS_Label_check_time: check label
		private void CheckTime()
		{
			HTS_LabelString lstring	= this.head ;
			HTS_LabelString next	= null ;
			
			if( lstring != null )
			{
				lstring.start = 0.0 ;
			}

			while( lstring != null )
			{
				next = lstring.next ;
				if( next == null )
				{
					break ;
				}

				if( lstring.end <  0.0 && next.start >= 0.0 )
				{
					lstring.end = next.start ;
				}
				else
				if( lstring.end >= 0.0 && next.start <  0.0 )
				{
					next.start = lstring.end ;
				}
				
				if( lstring.start <  0.0 )
				{
					lstring.start	= -1.0 ;
				}

				if( lstring.end <  0.0 )
				{
					lstring.end		= -1.0 ;
				}

				lstring = next ;
			}
		}

		// HTS_Label_get_size: get number of label string
		public int GetSize()
		{
			return this.size ;
		}

		// HTS_Label_get_string: get label string
		public string GetString( int index )
		{
			int i ;
			HTS_LabelString lstring = this.head ;
			
			for( i  = 0; i <  index && lstring != null ; i ++ )
			{
				lstring = lstring.next ;
			}

			if( lstring == null )
			{
				return null ;
			}

			return lstring.name ;
		}

		// HTS_Label_get_end_frame: get end frame
		public double GetEndFrame( int index )
		{
			int i ;
			HTS_LabelString lstring = this.head ;
			
			for( i  = 0 ; i <  index && lstring != null ; i ++ )
			{
				lstring = lstring.next ;
			}

			if( lstring == null )
			{
				return -1.0 ;
			}

			return lstring.end ;
		}

		//---------------------------------------------------------------------------

		private string GetTokenFromString( string tString )
		{
			int i = 0 ;
			char c ;
			
			c = tString[ i ++ ] ;

			List<char> tToken = new List<char>() ;

			// 0x0Dの可能性もある
			while( c != ' ' && c != 0x0A && c != '\t' )
			{
				tToken.Add( c ) ;

				if( i >= tString.Length )
				{
					break ;
				}

				c = tString[ i ++ ] ;
			}

			if( tToken.Count == 0 )
			{
				return "" ;
			}

			return new string( tToken.ToArray() ) ;
		}

		private bool IsGraph( int b )
		{
			if( b == ' ' )
			{
				return false ;
			}

			if( b >= 0x21 && b <= 0x7E )
			{
				return true ;
			}

			return false ;
		}

		private bool IsDigitString( string b )
		{
			if( b == null || b.Length == 0 )
			{
				return false ;
			}

			int i, l = b.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( b[ i ] <  '0' || b[ i ] >  '9' )
				{
					return false ;
				}
			}

			return true ;
		}
	}
}
