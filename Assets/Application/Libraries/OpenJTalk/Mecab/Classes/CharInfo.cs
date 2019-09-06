using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using HTS_Engine_API ;


namespace MecabForOpenJTalk.Classes
{
	public class CharInfo
	{
		// オリジナルではビットを使っているので注意

		private	int	m_Type ;			// 18 bit
		public  int   Type
		{
			get
			{
				return m_Type ;
			}
		}

		private	int m_DefaultType ;		//  8 bit
		public  int   DefaultType
		{
			get
			{
				return m_DefaultType ;
			}
		}

		private	int m_Length ;			//  4 bit
		public  int   Length
		{
			get
			{
				return m_Length ;
			}
		}

		private int m_Group ;			//  1 bit
		public  int   Group
		{
			get
			{
				return m_Group ;
			}
		}

		private int m_Invoke ;			//  1 bit
		public  int   Invoke
		{
			get
			{
				return m_Invoke ;
			}
		}

		public CharInfo()
		{
			m_Type			= 0 ;
			m_DefaultType	= 0 ;
			m_Length		= 0 ;
			m_Group			= 0 ;
			m_Invoke		= 0 ;
		}

		public void Open( byte[] b, ref int o )
		{
			uint v = GetUintL( b, ref o ) ;

			m_Type			= ( int )( ( v >>  0 ) & 0x0003FFFF ) ;	// 18 bit
			m_DefaultType	= ( int )( ( v >> 18 ) & 0x000000FF ) ;	//  8 bit
			m_Length		= ( int )( ( v >> 26 ) & 0x0000000F ) ;	//  4 bit
			m_Group			= ( int )( ( v >> 30 ) & 0x00000001 ) ; //  1 bit
			m_Invoke		= ( int )( ( v >> 31 ) & 0x00000001 ) ; //  1 bit
		}

		public bool isKindOf( CharInfo c )
		{
			return ( ( m_Type & c.Type ) != 0 ) ;
		}

		private uint GetUintL( byte[] b, ref int p )
		{
			int i ;
			uint v = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				v = v | ( uint )( b[ p + i ] << ( i * 8 ) ) ;
			}

			p = p + 4 ;

			return v ;
		}

	}
}
