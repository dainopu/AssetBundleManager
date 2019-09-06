using UnityEngine;
using System.Collections;

/// <summary>
/// イーズの値を管理するパッケージ
/// </summary>
namespace EaseHelper
{
	/// <summary>
	/// イーズの種別
	/// </summary>
	public enum EaseType
	{
		easeInQuad,
		easeOutQuad,
		easeInOutQuad,
		easeInCubic,
		easeOutCubic,
		easeInOutCubic,
		easeInQuart,
		easeOutQuart,
		easeInOutQuart,
		easeInQuint,
		easeOutQuint,
		easeInOutQuint,
		easeInSine,
		easeOutSine,
		easeInOutSine,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		easeInCirc,
		easeOutCirc,
		easeInOutCirc,
		linear,
		spring,
		easeInBounce,
		easeOutBounce,
		easeInOutBounce,
		easeInBack,
		easeOutBack,
		easeInOutBack,
		easeInElastic,
		easeOutElastic,
		easeInOutElastic,
//		punch
	}

	/// <summary>
	/// イーズの値を管理するクラス Version 2016/10/14 0
	/// </summary>
	public class Ease
	{

		/// <summary>
		/// カーブの値を取得する
		/// </summary>
		/// <param name="tFactor"></param>
		/// <param name="tEaseType"></param>
		/// <returns></returns>
		public static float GetValue( float tFactor, EaseType tEaseType )
		{
			return GetValue( 0, 1, tFactor, tEaseType ) ;
		}


		public static Vector2 GetValue( Vector2 tStart, Vector2 tEnd, float tFactor, EaseType tEaseType )
		{
			float x = GetValue( tStart.x, tEnd.x, tFactor, tEaseType ) ;
			float y = GetValue( tStart.y, tEnd.y, tFactor, tEaseType ) ;

			return new Vector2( x, y ) ;
		}

		public static Vector3 GetValue( Vector3 tStart, Vector3 tEnd, float tFactor, EaseType tEaseType )
		{
			float x = GetValue( tStart.x, tEnd.x, tFactor, tEaseType ) ;
			float y = GetValue( tStart.y, tEnd.y, tFactor, tEaseType ) ;
			float z = GetValue( tStart.z, tEnd.z, tFactor, tEaseType ) ;

			return new Vector3( x, y, z ) ;
		}

		// float の変化中の値を取得
		public static float GetValue( float tStart, float tEnd, float tFactor, EaseType tEaseType )
		{
			float tValue = 0 ;
			switch( tEaseType )
			{
				case EaseType.easeInQuad		: tValue = easeInQuad(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuad		: tValue = easeOutQuad(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuad		: tValue = easeInOutQuad(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInCubic		: tValue = easeInCubic(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutCubic		: tValue = easeOutCubic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutCubic	: tValue = easeInOutCubic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInQuart		: tValue = easeInQuart(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuart		: tValue = easeOutQuart(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuart	: tValue = easeInOutQuart(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInQuint		: tValue = easeInQuint(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutQuint		: tValue = easeOutQuint(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutQuint	: tValue = easeInOutQuint(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInSine		: tValue = easeInSine(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutSine		: tValue = easeOutSine(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutSine		: tValue = easeInOutSine(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInExpo		: tValue = easeInExpo(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutExpo		: tValue = easeOutExpo(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutExpo		: tValue = easeInOutExpo(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInCirc		: tValue = easeInCirc(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutCirc		: tValue = easeOutCirc(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutCirc		: tValue = easeInOutCirc(		tStart, tEnd, tFactor )	; break ;
				case EaseType.linear			: tValue = linear(				tStart, tEnd, tFactor )	; break ;
				case EaseType.spring			: tValue = spring(				tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInBounce		: tValue = easeInBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutBounce		: tValue = easeOutBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutBounce	: tValue = easeInOutBounce(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInBack		: tValue = easeInBack(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutBack		: tValue = easeOutBack(			tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutBack		: tValue = easeInOutBack(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInElastic		: tValue = easeInElastic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeOutElastic	: tValue = easeOutElastic(		tStart, tEnd, tFactor )	; break ;
				case EaseType.easeInOutElastic	: tValue = easeInOutElastic(	tStart, tEnd, tFactor )	; break ;
			}
			return tValue ;
		}

		//------------------------

		private static float easeInQuad( float start, float end, float value )
		{
			end -= start ;
			return end * value * value + start ;
		}

		private static float easeOutQuad( float start, float end, float value )
		{
			end -= start ;
			return - end * value * ( value - 2 ) + start ;
		}

		private static float easeInOutQuad( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value + start ;
			value -- ;
			return - end * 0.5f * ( value * ( value - 2 ) - 1 ) + start ;
		}

		private static float easeInCubic( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value + start ;
		}

		private static float easeOutCubic( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value + 1 ) + start ;
		}

		private static float easeInOutCubic( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value + 2 ) + start ;
		}

		private static float easeInQuart( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value + start ;
		}

		private static float easeOutQuart( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return - end * ( value * value * value * value - 1 ) + start ;
		}

		private static float easeInOutQuart( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value + start ;
			value -= 2 ;
			return - end * 0.5f * ( value * value * value * value - 2 ) + start ;
		}

		private static float easeInQuint( float start, float end, float value )
		{
			end -= start ;
			return end * value * value * value * value * value + start ;
		}
	
		private static float easeOutQuint( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * ( value * value * value * value * value + 1 ) + start ;
		}

		private static float easeInOutQuint( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return end * 0.5f * value * value * value * value * value + start ;
			value -= 2 ;
			return end * 0.5f * ( value * value * value * value * value + 2 ) + start ;
		}

		private static float easeInSine( float start, float end, float value )
		{
			end -= start ;
			return - end * Mathf.Cos( value * ( Mathf.PI * 0.5f ) ) + end + start ;
		}

		private static float easeOutSine( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Sin( value * ( Mathf.PI * 0.5f ) ) + start ;
		}

		private static float easeInOutSine( float start, float end, float value )
		{
			end -= start ;
			return - end * 0.5f * ( Mathf.Cos( Mathf.PI * value ) - 1 ) + start ;
		}

		private static float easeInExpo( float start, float end, float value )
		{
			end -= start ;
			return end * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
		}

		private static float easeOutExpo( float start, float end, float value )
		{
			end -= start ;
			return end * ( - Mathf.Pow( 2, - 10 * value ) + 1 ) + start ;
		}

		private static float easeInOutExpo( float start, float end, float value )
		{
			value /= 0.5f;
			end -= start ;
			if( value <  1 ) return end * 0.5f * Mathf.Pow( 2, 10 * ( value - 1 ) ) + start ;
			value -- ;
			return end * 0.5f * ( - Mathf.Pow( 2, - 10 * value ) + 2 ) + start ;
		}

		private static float easeInCirc( float start, float end, float value )
		{
			end -= start ;
			return - end * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
		}

		private static float easeOutCirc( float start, float end, float value )
		{
			value -- ;
			end -= start ;
			return end * Mathf.Sqrt( 1 - value * value ) + start ;
		}

		private static float easeInOutCirc( float start, float end, float value )
		{
			value /= 0.5f ;
			end -= start ;
			if( value <  1 ) return - end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) - 1 ) + start ;
			value -= 2 ;
			return end * 0.5f * ( Mathf.Sqrt( 1 - value * value ) + 1 ) + start ;
		}

		private static float linear( float start, float end, float value )
		{
			return Mathf.Lerp( start, end, value ) ;
		}
	
		private static float spring( float start, float end, float value )
		{
			value = Mathf.Clamp01( value ) ;
			value = ( Mathf.Sin( value * Mathf.PI * ( 0.2f + 2.5f * value * value * value ) ) * Mathf.Pow( 1f - value, 2.2f ) + value ) * ( 1f + ( 1.2f * ( 1f - value ) ) ) ;
			return start + ( end - start ) * value ;
		}

		private static float easeInBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			return end - easeOutBounce( 0, end, d - value ) + start ;
		}
	
		private static float easeOutBounce( float start, float end, float value )
		{
			value /= 1f ;
			end -= start ;
			if( value <  ( 1 / 2.75f ) )
			{
				return end * ( 7.5625f * value * value ) + start ;
			}
			else
			if( value <  ( 2 / 2.75f ) )
			{
				value -= ( 1.5f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .75f ) + start ;
			}
			else
			if( value <  (  2.5  / 2.75 ) )
			{
				value -= ( 2.25f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .9375f ) + start ;
			}
			else
			{
				value -= ( 2.625f / 2.75f ) ;
				return end * ( 7.5625f * ( value ) * value + .984375f ) + start ;
			}
		}

		private static float easeInOutBounce( float start, float end, float value )
		{
			end -= start ;
			float d = 1f ;
			if( value <  d * 0.5f ) return easeInBounce( 0, end, value * 2 ) * 0.5f + start ;
			else return easeOutBounce( 0, end, value * 2 - d ) * 0.5f + end * 0.5f + start ;
		}

		private static float easeInBack( float start, float end, float value )
		{
			end -= start ;
			value /= 1 ;
			float s = 1.70158f ;
			return end * ( value ) * value * ( ( s + 1 ) * value - s ) + start ;
		}

		private static float easeOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value = ( value ) - 1 ;
			return end * ( ( value ) * value * ( ( s + 1 ) * value + s ) + 1 ) + start ;
		}

		private static float easeInOutBack( float start, float end, float value )
		{
			float s = 1.70158f ;
			end -= start ;
			value /= 0.5f ;
			if( ( value ) <  1 )
			{
				s *= ( 1.525f ) ;
				return end * 0.5f * ( value * value * ( ( ( s ) + 1 ) * value - s ) ) + start ;
			}
			value -= 2 ;
			s *= ( 1.525f ) ;
			return end * 0.5f * ( ( value ) * value * ( ( ( s ) + 1 ) * value + s ) + 2 ) + start ;
		}

		private static float easeInElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return - ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
		}		

		private static float easeOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d ) == 1 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p * 0.25f ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			return ( a * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start ) ;
		}		

		private static float easeInOutElastic( float start, float end, float value )
		{
			end -= start ;
		
			float d = 1f ;
			float p = d * 0.3f ;
			float s = 0 ;
			float a = 0 ;
		
			if( value == 0 ) return start ;
		
			if( ( value /= d * 0.5f ) == 2 ) return start + end ;
		
			if( a == 0f || a <  Mathf.Abs( end ) )
			{
				a = end ;
				s = p / 4 ;
			}
			else
			{
				s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a ) ;
			}
		
			if( value <  1 ) return - 0.5f * ( a * Mathf.Pow( 2, 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) ) + start ;
			return a * Mathf.Pow( 2, - 10 * ( value -= 1 ) ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) * 0.5f + end + start ;
		}		

		private static float punch( float amplitude, float value )
		{
			float s = 9 ;
			if( value == 0 )
			{
				return 0 ;
			}
			else
			if( value == 1 )
			{
				return 0 ;
			}
			float period = 1 * 0.3f ;
			s = period / ( 2 * Mathf.PI ) * Mathf.Asin( 0 ) ;
			return ( amplitude * Mathf.Pow( 2, - 10 * value ) * Mathf.Sin( ( value * 1 - s ) * ( 2 * Mathf.PI ) / period ) ) ;
		}

		private static float clerp( float start, float end, float value )
		{
			float min = 0.0f ;
			float max = 360.0f ;
			float half = Mathf.Abs( ( max - min ) * 0.5f ) ;
			float retval = 0.0f ;
			float diff = 0.0f ;
			if( ( end - start ) <  - half )
			{
				diff =   ( ( max - start ) + end   ) * value ;
				retval = start + diff ;
			}
			else
			if( ( end - start ) >    half )
			{
				diff = - ( ( max - end   ) + start ) * value ;
				retval = start + diff ;
			}
			else retval = start + ( end - start ) * value ;
			return retval ;
		}
	}
}
