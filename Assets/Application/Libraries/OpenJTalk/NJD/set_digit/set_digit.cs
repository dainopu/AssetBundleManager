using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text ;

namespace OJT
{
	public partial class NJD
	{
		public void SetDigit()
		{
			int i, j ;
			NJDNode s = null ;
			NJDNode e = null ;
			NJDNode node ;
			int find = 0 ;
			
			// convert digit sequence
			for( node  = this.head ; node != null ; node = node.next )
			{
				if( find == 0 && node.PosGroup1 == NJD_SET_DIGIT_KAZU )
				{
					find = 1 ;
				}
				if( GetDigit( node, 1 ) >= 0 )
				{
					if( s == null )
					{
						s = node ;
					}
					if( node == this.tail )
					{
						e = node ;
					}
				}
				else
				{
					if( s != null )
					{
						e = node.prev ;
					}
				}

				if( s != null && e != null )
				{
					ConvertDigitSequence( s, e ) ;
					s = e = null ;
				}
			}

			if( find == 0 )
			{
				return ;
			}

			this.remove_silent_node() ;

			if( this.head == null )
			{
				return ;
			}
			
			for( node = this.head.next ; node != null && node.next != null ; node = node.next )
			{
				if
				(
					node.Word != "*" &&
					node.prev.Word != "*" &&
					( node.Word == NJD_SET_DIGIT_TEN1 || node.Word == NJD_SET_DIGIT_TEN2 ) &&
					( node.prev.PosGroup1 == NJD_SET_DIGIT_KAZU ) &&
					( node.next.PosGroup1 == NJD_SET_DIGIT_KAZU )
				)
				{
					node.Load( NJD_SET_DIGIT_TEN_FEATURE ) ;
					node.ChainFlag = 1 ;

					if( node.prev.Word == NJD_SET_DIGIT_ZERO1 || node.prev.Word == NJD_SET_DIGIT_ZERO2 )
					{
						node.prev.Pron = NJD_SET_DIGIT_ZERO_BEFORE_DP ;
						node.prev.MoraSize = 2 ;
					}
					else
					if( node.prev.Word == NJD_SET_DIGIT_TWO )
					{
						node.prev.Pron = NJD_SET_DIGIT_TWO_BEFORE_DP ;
						node.prev.MoraSize = 2 ;
					}
					else
					if( node.prev.Word == NJD_SET_DIGIT_FIVE )
					{
						node.prev.Pron = NJD_SET_DIGIT_FIVE_BEFORE_DP ;
						node.prev.MoraSize = 2 ;
					}
				}
			}

			for( node = this.head.next ; node != null ; node = node.next )
			{
				if( node.prev.PosGroup1 == NJD_SET_DIGIT_KAZU )
				{
					if( node.PosGroup2 == NJD_SET_DIGIT_JOSUUSHI || node.PosGroup1 == NJD_SET_DIGIT_FUKUSHIKANOU )
					{
						// convert digit pron
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1b, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1b, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1c1, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1c1, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1c2, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1c2, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1d, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1d, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1e, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1e, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1f, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1f, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1g, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1g, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1h, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1h, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1i, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1i, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1j, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1j, node.prev ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class1k, node ) == 1 )
						{
							ConvertDigitPron( njd_set_digit_rule_conv_table1k, node.prev ) ;
						}

						// convert numerative pron
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class2b, node ) == 1 )
						{
							ConvertNumerativePron( njd_set_digit_rule_conv_table2b, node.prev, node ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class2c, node ) == 1 )
						{
							ConvertNumerativePron( njd_set_digit_rule_conv_table2c, node.prev, node ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class2d, node ) == 1 )
						{
							ConvertNumerativePron( njd_set_digit_rule_conv_table2d, node.prev, node ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class2e, node ) == 1 )
						{
							ConvertNumerativePron( njd_set_digit_rule_conv_table2e, node.prev, node ) ;
						}
						else
						if( SearchNumerativeClass( njd_set_digit_rule_numerative_class2f, node ) == 1 )
						{
							ConvertNumerativePron( njd_set_digit_rule_conv_table2f, node.prev, node ) ;
						}

						// modify accent phrase
						node.prev.ChainFlag = 0 ;
						node.ChainFlag = 1 ;
					}
				}
			}

			for( node  = this.head.next ; node != null ; node = node.next )
			{
				if( node.prev.PosGroup1 == NJD_SET_DIGIT_KAZU )
				{
					if( node.PosGroup1 == NJD_SET_DIGIT_KAZU && node.prev.Word != null && node.Word != null )
					{
						// modify accent phrase
						find = 0 ;
						for( i  = 0 ; njd_set_digit_rule_numeral_list4[ i ] != null ; i ++ )
						{
							if( node.prev.Word == njd_set_digit_rule_numeral_list4[ i ] )
							{
								for( j  = 0 ; njd_set_digit_rule_numeral_list5[ j ] != null ; j ++ )
								{
									if( node.Word == njd_set_digit_rule_numeral_list5[ j ] )
									{
										node.prev.ChainFlag = 0 ;
										node.ChainFlag = 1 ;
										find = 1 ;
										break ;
									}
								}
								break ;
							}
						}
						
						if( find == 0 )
						{
							for( i  = 0 ; njd_set_digit_rule_numeral_list5[ i ] != null ; i ++ )
							{
								if( node.prev.Word == njd_set_digit_rule_numeral_list5[ i ] )
								{
									for( j  = 0 ; njd_set_digit_rule_numeral_list4[ j ] != null ; j ++ )
									{
										if( node.Word == njd_set_digit_rule_numeral_list4[ j ] )
										{
											node.ChainFlag = 0 ;
											break ;
										}
									}
									break ;
								}
							}
						}
					}

					if( SearchNumerativeClass( njd_set_digit_rule_numeral_list8, node ) == 1 )
					{
						ConvertDigitPron( njd_set_digit_rule_numeral_list9, node.prev ) ;
					}

					if( SearchNumerativeClass( njd_set_digit_rule_numeral_list10, node ) == 1 )
					{
						ConvertDigitPron( njd_set_digit_rule_numeral_list11, node.prev ) ;
					}

					if( SearchNumerativeClass( njd_set_digit_rule_numeral_list6, node ) == 1 )
					{
						ConvertNumerativePron( njd_set_digit_rule_numeral_list7, node.prev, node ) ;
					}
				}
			}

			for( node  = this.head ; node != null ; node = node.next )
			{
				if
				(
					node.next != null &&
					node.next.Word != "*" &&
					( node.PosGroup1 == NJD_SET_DIGIT_KAZU ) &&
					( node.prev == null || node.prev.PosGroup1 != NJD_SET_DIGIT_KAZU ) &&
					( node.next.PosGroup2 == NJD_SET_DIGIT_JOSUUSHI || node.next.PosGroup1 == NJD_SET_DIGIT_FUKUSHIKANOU )
				)
				{
					// convert class3
					for( i  = 0 ; njd_set_digit_rule_numerative_class3[ i ] != null ; i += 2 )
					{
						if( node.next.Word == njd_set_digit_rule_numerative_class3[ i ] && node.next.Read == njd_set_digit_rule_numerative_class3[ i + 1 ] )
						{
							for( j  = 0 ; njd_set_digit_rule_conv_table3[ j ] != null ; j += 4 )
							{
								if( node.Word == njd_set_digit_rule_conv_table3[ j ] )
								{
									node.Read = njd_set_digit_rule_conv_table3[ j + 1 ] ;
									node.Pron = njd_set_digit_rule_conv_table3[ j + 1 ] ;
									node.Acc = Atoi( njd_set_digit_rule_conv_table3[ j + 2 ] ) ;
									node.MoraSize = Atoi( njd_set_digit_rule_conv_table3[ j + 3 ] ) ;
									break ;
								}
							}
							break ;
						}
					}

					// person
					if( node.next.Word == NJD_SET_DIGIT_NIN )
					{
						for( i  = 0 ; njd_set_digit_rule_conv_table4[ i ] != null ; i += 2 )
						{
							if( node.Word == njd_set_digit_rule_conv_table4[ i ] )
							{
								node.Load( njd_set_digit_rule_conv_table4[ i + 1 ] ) ;
								node.next.Pron = null ;
								break ;
							}
						}
					}

					// the day of month
					if( node.next.Word == NJD_SET_DIGIT_NICHI && node.Word != "*" )
					{
						if( node.prev != null && node.prev.Word.IndexOf( NJD_SET_DIGIT_GATSU ) == 0 && node.Word == NJD_SET_DIGIT_ONE )
						{
							node.Load( NJD_SET_DIGIT_TSUITACHI ) ;
							node.next.Pron = null ;
						}
						else
						{
							for( i  = 0 ; njd_set_digit_rule_conv_table5[ i ] != null ; i += 2 )
							{
								if( node.Word == njd_set_digit_rule_conv_table5[ i ] )
								{
									node.Load( njd_set_digit_rule_conv_table5[ i + 1 ] ) ;
									node.next.Pron = null ;
									break ;
								}
							}
						}
					}
					else
					if( node.next.Word == NJD_SET_DIGIT_NICHIKAN )
					{
						for( i  = 0 ; njd_set_digit_rule_conv_table6[ i ] != null ; i += 2 )
						{
							if( node.Word == njd_set_digit_rule_conv_table6[ i ] )
							{
								node.Load( njd_set_digit_rule_conv_table6[ i + 1 ] ) ;
								node.next.Pron = null ;
								break ;
							}
						}
					}
				}
			}

			for( node  = this.head ; node != null ; node = node.next )
			{
				if( ( node.prev == null || node.prev.PosGroup1 != NJD_SET_DIGIT_KAZU ) && node.next != null && node.next.next != null )
				{
					if( node.Word == NJD_SET_DIGIT_TEN && node.next.Word == NJD_SET_DIGIT_FOUR )
					{
						if( node.next.next.Word == NJD_SET_DIGIT_NICHI )
						{
							node.Load( NJD_SET_DIGIT_JUYOKKA ) ;
							node.next.Pron = null ;
							node.next.next.Pron = null ;
						}
						else
						if( node.next.next.Word == NJD_SET_DIGIT_NICHIKAN )
						{
							node.Load( NJD_SET_DIGIT_JUYOKKAKAN ) ;
							node.next.Pron = null ;
							node.next.next.Pron = null ;
						}
					}
					else
					if( node.Word == NJD_SET_DIGIT_TWO && node.next.Word == NJD_SET_DIGIT_TEN )
					{
						if( node.next.next.Word == NJD_SET_DIGIT_NICHI )
						{
							node.Load( NJD_SET_DITIT_HATSUKA ) ;
							node.next.Pron = null ;
							node.next.next.Pron = null ;
						}
						else
						if( node.next.next.Word == NJD_SET_DIGIT_NICHIKAN )
						{
							node.Load( NJD_SET_DIGIT_HATSUKAKAN ) ;
							node.next.Pron = null ;
							node.next.next.Pron = null ;
						}
						else
						if( node.next.next.Word == NJD_SET_DIGIT_FOUR && node.next.next.next != null )
						{
							if( node.next.next.next.Word == NJD_SET_DIGIT_NICHI )
							{
								node.Load( NJD_SET_DIGIT_NIJU ) ;
								node.next.Load( NJD_SET_DITIT_YOKKA ) ;
								node.next.next.Pron = null ;
								node.next.next.next.Pron = null ;
							}
							else
							if( node.next.next.next.Word == NJD_SET_DIGIT_NICHIKAN )
							{
								node.Load( NJD_SET_DIGIT_NIJU ) ;
								node.next.Load( NJD_SET_DIGIT_YOKKAKAN ) ;
								node.next.next.Pron = null ;
								node.next.next.next.Pron = null ;
							}
						}
					}
				}
			}

			this.remove_silent_node() ;
			if( this.head == null )
			{
				return ;
			}
		}

		//---------------------------------------------------------------------------

		private int GetDigit( NJDNode node, int convert_flag )
		{
			int i ;	

			if( node.Word == "*" )
			{
				return -1 ;
			}
			
			if( node.PosGroup1 == NJD_SET_DIGIT_KAZU )
			{
				for( i  = 0 ; njd_set_digit_rule_numeral_list1[ i ] != null ; i += 3 )
				{
					if( njd_set_digit_rule_numeral_list1[ i ] == node.Word )
					{
						if( convert_flag == 1 )
						{
							node.Word = njd_set_digit_rule_numeral_list1[ i + 2 ] ;
							node.Orig = njd_set_digit_rule_numeral_list1[ i + 2 ] ;
						}

						return Atoi( njd_set_digit_rule_numeral_list1[ i + 1 ] ) ;
					}
				}
			}
			
			return -1 ;
		}


		private int GetDigitSequenceScore( NJDNode start, NJDNode end )
		{
			string buff_pos_group1	= null ;
			string buff_pos_group2	= null ;
			string buff_word		= null ;
			int score = 0 ;
			
			if( start.prev != null )
			{
				buff_pos_group1 = start.prev.PosGroup1 ;
				buff_pos_group2 = start.prev.PosGroup2 ;
				buff_word		= start.prev.Word ;

				if( buff_pos_group1 == NJD_SET_DIGIT_SUUSETSUZOKU )    // prev pos_group1
				{
					score += 2 ;
				}

				if( buff_pos_group2 == NJD_SET_DIGIT_JOSUUSHI || buff_pos_group1 == NJD_SET_DIGIT_FUKUSHIKANOU )   // prev pos_group1 and pos_group2
				{
					score += 1 ;
				}
				
				if( buff_word != null )
				{
					if( buff_word == NJD_SET_DIGIT_TEN1 || buff_word == NJD_SET_DIGIT_TEN2 )
					{
						// prev string
						if( start.prev.prev == null || start.prev.prev.PosGroup1 != NJD_SET_DIGIT_KAZU )
						{
							score += 0 ;
						}
						else
						{
							score -= 5 ;
						}
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN1 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN2 )
					{
						score -= 2;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN3 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN4 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN5 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_KAKKO1 )
					{
						if( start.prev.prev == null || start.prev.prev.PosGroup1 != NJD_SET_DIGIT_KAZU )
						{
							score += 0 ;
						}
						else
						{
							score -= 2 ;
						}
					}
					else
					if( buff_word == NJD_SET_DIGIT_KAKKO2 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_BANGOU )
					{
						score -= 2 ;
					}
				}

				if( start.prev.prev != null )
				{
					buff_word = start.prev.prev.Word ;	// prev prev string
					if( buff_word == NJD_SET_DIGIT_BANGOU )
					{
						score -= 2 ;
					}
				}
			}

			if( end.next != null )
			{
				buff_pos_group1	= end.next.PosGroup1 ;
				buff_pos_group2	= end.next.PosGroup2 ;	// next pos_group2
				buff_word		= end.next.Word ;		// next string
				if( buff_pos_group2 == NJD_SET_DIGIT_JOSUUSHI || buff_pos_group1 == NJD_SET_DIGIT_FUKUSHIKANOU )
				{
					score += 2 ;
				}
				if( buff_word != null )
				{
					if( buff_word == NJD_SET_DIGIT_HAIHUN1 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN2 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN3 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN4 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_HAIHUN5 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_KAKKO1 )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_KAKKO2 )
					{
						if( end.next.next == null || end.next.next.PosGroup1 != NJD_SET_DIGIT_KAZU )
						{
							score += 0 ;
						}
						else
						{
							score -= 2 ;
						}
					}
					else
					if( buff_word == NJD_SET_DIGIT_BANGOU )
					{
						score -= 2 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_TEN1 )
					{
						score += 4 ;
					}
					else
					if( buff_word == NJD_SET_DIGIT_TEN2 )
					{
						score += 4 ;
					}
				}
			}

			return score ;
		}

		private void ConvertDigitSequence( NJDNode start, NJDNode end )
		{
			NJDNode node ;
			NJDNode newnode ;
			int digit ;
			int place = 0 ;
			int index ;
			int size = 0 ;
			int have = 0 ;
			
			for( node  = start ; node != end.next ; node = node.next )
			{
				size ++ ;
			}

			if( size <= 1 )
			{
				return ;
			}
			
			if( GetDigitSequenceScore( start, end ) <  0 )
			{
				for( node  = start, size = 0 ; node != end.next ; node = node.next )
				{
					if( node.Word == NJD_SET_DIGIT_ZERO1 || node.Word == NJD_SET_DIGIT_ZERO2 )
					{
						node.Pron = NJD_SET_DIGIT_ZERO_AFTER_DP ;
						node.MoraSize = 2 ;
					}
					else
					if( node.Word == NJD_SET_DIGIT_TWO )
					{
						node.Pron = NJD_SET_DIGIT_TWO_AFTER_DP ;
						node.MoraSize = 2 ;
					}
					else
					if( node.Word == NJD_SET_DIGIT_FIVE )
					{
						node.Pron = NJD_SET_DIGIT_FIVE_AFTER_DP ;
						node.MoraSize = 2 ;
					}
					node.ChainRule = null ;
					if( size % 2 == 0 )
					{
						node.ChainFlag = 0 ;
					}
					else
					{
						node.ChainFlag = 1 ;
						node.prev.Acc = 3 ;
					}
					size ++ ;
				}
				return ;
			}
			
			index = size % 4 ;
			if( index == 0 )
			{
				index  = 4 ;
			}

			if( size >  index )
			{
				place = ( size - index ) / 4 ;
			}
			index -- ;
			if( place >  17 )
			{
				return ;
			}
			
			for( node  = start ; node != end.next ; node = node.next )
			{
				digit = GetDigit( node, 0 ) ;
				if( index == 0 )
				{
					if( digit == 0 )
					{
						node.Pron = null ;
						node.Acc = 0 ;
						node.MoraSize = 0 ;
					}
					else
					{
						have = 1 ;
					}
					if( have == 1 )
					{
						if( place >  0 )
						{
							newnode = new NJDNode() ;
							newnode.Load( njd_set_digit_rule_numeral_list3[ place ] ) ;
							node = NJDNode.Insert( node, node.next, newnode ) ;
						}
						have = 0 ;
					}
					place -- ;
				}
				else
				{
					if( digit <= 1 )
					{
						node.Pron = null ;
						node.Acc = 0 ;
						node.MoraSize = 0 ;
					}
					if( digit >  0 )
					{
						newnode = new NJDNode() ;
						newnode.Load( njd_set_digit_rule_numeral_list2[ index ] ) ;
						node = NJDNode.Insert( node, node.next, newnode ) ;
						have = 1 ;
					}
				}
				index -- ;
				if( index <  0 )
				{
					index = 4 - 1 ;
				}
			}
		}

		private int SearchNumerativeClass( string[] list, NJDNode node )
		{
			int i ;
			string str = node.Word ;
			
			if( str == "*" )
			{
				return 0 ;
			}

			for( i  = 0 ; list[ i ] != null ; i ++ )
			{
				if( list[ i ] == str )
				{
					return 1 ;
				}
			}

			return 0 ;
		}

		private void ConvertDigitPron( string[] list, NJDNode node )
		{
			int i ;
			string str = node.Word ;
			
			if( str == "*" )
			{
				return ;
			}
			
			for( i  = 0 ; list[ i ] != null ; i += 4 )
			{
				if( list[ i ] == str )
				{
					node.Pron =list[ i + 1 ] ;
					node.Acc = Atoi( list[ i + 2 ] ) ;
					node.MoraSize = Atoi( list[ i + 3 ] ) ;
					return ;
				}
			}
		}

		private void ConvertNumerativePron( string[] list, NJDNode node1, NJDNode node2 )
		{
			int i, j ;
			int type = 0 ;
			string str = node1.Word ;
			string buff ;
			
			if( str == "*" )
			{
				return ;
			}
			
			for( i  = 0 ; list[ i ] != null ; i += 2 )
			{
				if( list[ i ] == str )
				{
					type = Atoi( list[ i + 1 ] ) ;
					break ;
				}
			}

			if( type == 1 )
			{
				for( i  = 0 ; njd_set_digit_rule_voiced_sound_symbol_list[ i ] != null ; i += 2 )
				{
					str = node2.Pron ;

					j = StrTopCmp( str, njd_set_digit_rule_voiced_sound_symbol_list[ i ] ) ;
					if( j >= 0 )
					{
						buff = njd_set_digit_rule_voiced_sound_symbol_list[ i + 1 ] ;
						buff += str.Substring( j ) ;
						node2.Pron = buff ;
						break ;
					}
				}
			}
			else
			if( type == 2 )
			{
				for( i  = 0 ; njd_set_digit_rule_semivoiced_sound_symbol_list[ i ] != null ; i += 2 )
				{
					str = node2.Pron ;
					j = StrTopCmp( str, njd_set_digit_rule_semivoiced_sound_symbol_list[ i ] ) ;
					if( j >= 0 )
					{
						buff = njd_set_digit_rule_semivoiced_sound_symbol_list[ i + 1 ] ;
						buff += str.Substring( j ) ;
						node2.Pron = buff ;
						break ;
					}
				}
			}
		}
	}
}
