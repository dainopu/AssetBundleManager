using UnityEngine ;
using System;
using System.Collections ;

// Last Update 2017/05/26

/// <summary>
/// 乱数生成のパッケージ
/// </summary>
namespace RandomHelper
{
	/// <summary>
	/// インスタンスを生成しなくても使えるランダムクラス
	/// </summary>
	public static class Random_XorShift
	{
		private static XorShift m_XorShift = new XorShift() ;

		static Random_XorShift()
		{
			// 初期化時に現在時刻を元にランダムな回数（最大60回）乱数を読み捨てる
			int i, l = DateTime.Now.Second ;
			for( i = 0; i < l ; i ++ )
			{
				m_XorShift.Get() ;
			}
		}

		public static ulong seed
		{
			get
			{
				return m_XorShift.seed ;
			}
			set
			{
				m_XorShift.seed = value ;
			}
		}

		public static ulong Get()
		{
			return m_XorShift.Get() ;
		}

		public static int Get( int tMax )
		{
			return m_XorShift.Get( tMax ) ;
		}

		public static int Get( int tMin, int tMax, bool tSwap = false )
		{
			return m_XorShift.Get( tMin, tMax, tSwap ) ;
		}

		public static float Get( float tMin, float tMax, bool tSwap = false )
		{
			return m_XorShift.Get( tMin, tMax, tSwap ) ;
		}
	}

	/// <summary>
	/// XorShift アルゴリズムの乱数生成クラス Version 2016/12/13 0
	/// </summary>
	public class XorShift
	{
		// 初期の根値
		private ulong m_RandomSeedX = 123456789L ;
		private ulong m_RandomSeedY = 362436069L ;
		private ulong m_RandomSeedZ = 521288629L ;
		private ulong m_RandomSeedW =  88675123L  ;

		/// <summary>
		/// 疑似乱数根
		/// </summary>
		public ulong seed
		{
			get
			{
				return m_RandomSeedX ;
			}
			set
			{
				m_RandomSeedX = ( ulong )value ;
				m_RandomSeedY = 362436069L ;
				m_RandomSeedZ = 521288629L ;
				m_RandomSeedW =  88675123L  ;
			}
		}

		/// <summary>
		/// 整数値の範囲で乱数値を取得する(xorshift)
		/// </summary>
		/// <returns></returns>
		public ulong Get()
		{
			ulong t = ( m_RandomSeedX ^ ( m_RandomSeedX << 11 ) ) ;

//			Debug.LogWarning( "RX:" + mRandomSeedX + " RY:" + mRandomSeedX + " RZ:" + mRandomSeedZ + " RW:" + mRandomSeedW ) ;

			m_RandomSeedX = m_RandomSeedY ;
			m_RandomSeedY = m_RandomSeedZ ;
			m_RandomSeedZ = m_RandomSeedW ;
			m_RandomSeedW = m_RandomSeedW ^ ( m_RandomSeedW >> 19 ) ^ ( t ^ ( t >> 8 ) )  ;

			return m_RandomSeedW ; 
		}

		/// <summary>
		/// ０から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="tMax"></param>
		/// <returns></returns>
		public int Get( int tMax )
		{
			if( tMax <  0 )
			{
				return 0 ; // 値が不正
			}

			return ( int )( Get() % ( ulong )( tMax + 1 ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の整数型乱数値を返す
		/// </summary>
		/// <param name="tMin">最小値</param>
		/// <param name="tMax">最大値</param>
		/// <returns></returns>
		public int Get( int tMin, int tMax, bool tSwap = false )
		{
			if( tMin >  tMax )
			{
				if( tSwap == true )
				{
					// 値を入れ替える
					int v = tMin ;
					tMin = tMax ;
					tMax = v ;
				}
				else
				{
					return 0 ;  // 値が不正
				}
			}

			return tMin + ( int )( Get() % ( ulong )( ( tMax - tMin ) + 1 ) ) ;
		}

		/// <summary>
		/// 最小値から最大値の範囲の小数型乱数を返す
		/// </summary>
		/// <param name="tMin">最小値</param>
		/// <param name="tMax">最大値</param>
		/// <returns></returns>
		public float Get( float tMin, float tMax, bool tSwap = false )
		{
			if( tMin >  tMax )
			{
				if( tSwap == true )
				{
					// 値を入れ替える
					float v = tMin ;
					tMin = tMax ;
					tMax = v ;
				}
				else
				{
					return 0f ;  // 値が不正
				}
			}

			ulong r = Get() ;
			float a = ( float )( r % ( 100000000L + 1L ) ) / ( float )100000000L ;
//			Debug.LogWarning( "r値:" + r + " a値:" + a ) ;

			return tMin + ( ( tMax - tMin ) * a ) ;
		}
	}
}
