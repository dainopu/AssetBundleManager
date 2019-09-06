using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace MecabForOpenJTalk.Classes
{
	// dictionary.h
	public class Token
	{
		public	ushort	lcAttr ;
		public	ushort	rcAttr ;
		public	ushort	posid ;
		public	short	wcost ;
		public	uint	feature ;
		public	uint	compound ;

		public void Open( byte[] b, ref int o )
		{
			lcAttr		= GetUshortL( b, ref o ) ;
			rcAttr		= GetUshortL( b, ref o ) ;
			posid		= GetUshortL( b, ref o ) ;
			wcost		= GetShortL( b, ref o ) ;
			feature		= GetUintL( b, ref o ) ;
			compound	= GetUintL( b, ref o ) ;
		}

		private short GetShortL( byte[] b, ref int p )
		{
			int i ;
			short v = 0 ;
			for( i  = 0 ; i <  2 ; i ++ )
			{
				v = ( short )( v | ( short )( b[ p + i ] << ( i * 8 ) ) ) ;
			}

			p = p + 2 ;

			return v ;
		}

		private ushort GetUshortL( byte[] b, ref int p )
		{
			int i ;
			ushort v = 0 ;
			for( i  = 0 ; i <  2 ; i ++ )
			{
				v = ( ushort )( v | ( ushort )( b[ p + i ] << ( i * 8 ) ) ) ;
			}

			p = p + 2 ;

			return v ;
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
