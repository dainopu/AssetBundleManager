using System ;
using System.Text ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Misc
	{
		// HTS_pattern_match: pattern matching function
		public static bool PatternMatch( string tString, string tPattern )
		{
			int i ;
			int tMax = 0, tStar = 0, tQuestion = 0 ;

			for( i  = 0 ; i <  tPattern.Length ; i ++ )
			{
				switch( tPattern[ i ] )
				{
					case '*' :
						tStar ++ ;
					break ;
					case '?' :
						tQuestion ++ ;
						tMax ++ ;
					break ;
					default :
						tMax ++ ;
					break ;
				}
			}

			if( tStar == 2 && tQuestion == 0 && tPattern[ 0 ] == '*' && tPattern[ i - 1 ] == '*' )
			{
				// only string matching is required
				int l = i - 2 ;

				// 最初と最後の * 以外の部分を切り出す
				string tWord = tPattern.Substring( 1, l ) ;	

				int p = tString.IndexOf( tWord ) ;
				if( p >= 0 )
				{
					return true ;
				}
				else
				{
					return false ;
				}
			}
			else
			{
//				bool tResult = DpMatch( tString, 0, tPattern, 0, 0, tString.Length - tMax ) ;
				bool tResult = DpMatch( tString, tPattern ) ;

				return tResult ;
			}
		}

		private static bool DpMatch( string tMessage, string tPattern )
		{
			if( tPattern.IndexOf( "**" ) >= 0 )
			{
				Debug.LogError( "２つ以上 * が続いている : " + tPattern ) ;
				return false ;
			}

			bool tResult = DpMatch( tMessage, 0, tPattern, 0 ) ;

			return tResult ;
		}

		private static bool DpMatch( string tMessage, int tMessage_o, string tPattern, int tPattern_o )
		{
			int i, j, p, q ;
			bool a ;

			int ml = tMessage.Length ;
			int pl = tPattern.Length ;


			//----------------------------------------------------------

			a = false ;

			if( tPattern[ tPattern_o ] == '*' )
			{
				if( ( tPattern_o + 1 ) == pl )
				{
					return true ;
				}

				tPattern_o ++ ;

				a = true ;
			}

			// * でも ? でもない文字を探す
			q = 0 ;
			for( i  = tPattern_o ; i <  pl ; i ++ )
			{
				if( tPattern[ i ] != '?' )
				{
					break ;
				}
				q ++ ;
			}
				
			if( q >  0 )
			{
				if( i >= pl )
				{
					// 残りは全て ? なので後は文字数の確認のみ
					if( ( ml - tMessage_o ) == q )
					{
						// 文字数的にはＯＫなのでマッチ成功とみなす
						return true ;
					}
					else
					{
						// 文字数的にはＮＧなのでマッチ失敗とみなす
						return false ;
					}
				}

				if( ( ml - tMessage_o ) <= q )
				{
					// 文字数が足りない
					return false ;
				}
			}

			tPattern_o = i ;	// * でも ? でもない最初の文字の位置
			
			// 本格的にパターンマッチングを行う
			if( a == false )
			{
				// パターンの先頭に * なし
				i = tMessage_o + q ;

				p = 0 ;
				for( j  = tPattern_o ; j <  pl ; j ++ )
				{
					if( ( i + p ) >= ml )
					{
						// マッチ対象文字のオーバー
						// 最後に置いてはダメ j >= pl を満たさなくなる
						break ;
					}

					if( tPattern[ j ] == '*' )
					{
						// また * が出た
						if( DpMatch( tMessage, i + p, tPattern, j ) == true )
						{
							// マッチした(再帰的に以下全てマッチしたという事なので)
							return true ;
						}
						else
						{
							// マッチしない(結果的にマッチしないになる)
							break ;
						}
					}
					else
					if( tPattern[ j ] != '?' && tMessage[ i + p ] != tPattern[ j ] )
					{
						// マッチしない
						break ;
					}
					p ++ ;
				}

				if( ( i + p ) >= ml && j >= pl )
				{
					// 最後までマッチした
					return true ;
				}
			}
			else
			{
				// パターンの先頭に * あり

				for( i  = ( tMessage_o + q ) ; i <  ml ; i ++ )
				{
					p = 0 ;
					for( j  = tPattern_o ; j <  pl ; j ++ )
					{
						if( ( i + p ) >= ml )
						{
							// マッチ対象文字のオーバー
							// 最後に置いてはダメ j >= pl を満たさなくなる
							break ;
						}

						if( tPattern[ j ] == '*' )
						{
							// また * が出が出た
							if( DpMatch( tMessage, i + p, tPattern, j ) == true )
							{
								// マッチした(再帰的に以下全てマッチしたという事なので)
								return true ;
							}
							else
							{
								// マッチしない(位置をずらして再検査)
								break ;
							}
						}
						else
						if( tPattern[ j ] != '?' && tMessage[ i + p ] != tPattern[ j ] )
						{
							// マッチしない
							break ;
						}
						p ++ ;
					}

					if( ( i + p ) >= ml && j >= pl )
					{
						// 最後までマッチした
						return true ;
					}
				}
			}

			// マッチしない
			return false ;
		}

		// 後で再帰を使わないもっと高速なパターンマッチ処理に最適化する
/*		private static bool DpMatch( string tString, int tString_o, string tPattern, int tPattern_o, int tPosition, int tMax )
		{
			// オリジナルのプログラムは文字列の最後に終端記号(0)が存在するのが前提で組まれている事に注意する事
			if( tPosition >  tMax )
			{
				return false ;
			}

			if( tString_o >= tString.Length && tPattern_o >= tPattern.Length )
			{
				return true ;
			}

			if( tPattern_o <  tPattern.Length && tPattern[ tPattern_o ] == '*' )
			{
				if( DpMatch( tString, tString_o + 1, tPattern, tPattern_o, tPosition + 1, tMax ) == true )
				{
					return true ;
				}
				else
				{
					return DpMatch( tString, tString_o, tPattern, tPattern_o + 1, tPosition, tMax ) ;
				}
			}

			if( ( tString_o <  tString.Length && tPattern_o <  tPattern.Length && tString[ tString_o ] == tPattern[ tPattern_o ] )|| ( tPattern_o <  tPattern.Length && tPattern[ tPattern_o ] == '?' ) )
			{
				// どうやら ? は１文字だけなんでもＯＫらしい( *  は前後の複数文字がなんでもＯＫ)

				if( DpMatch( tString, tString_o + 1, tPattern, tPattern_o + 1, tPosition + 1, tMax + 1 ) == true )
				{
					return true ;
				}
			}
			
			return false ;
		}*/


	}
}
