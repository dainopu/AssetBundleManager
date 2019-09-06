using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text ;

namespace OJT
{
	public partial class NJD
	{
		public void SetUnvoicedVowel()
		{
			NJDNode node ;
			int index ;
			int len ;
			string buff ;
			string str ;
			
			// mora information for current, next, and next-next moras
			string mora1 = null, mora2 = null, mora3 = null ;
			NJDNode nlink1 = null, nlink2 = null, nlink3 = null ;
			int size1 = 0, size2 = 0, size3 = 0 ;
			int flag1 = -1, flag2 = -1, flag3 = -1 ;	// unknown:-1, voice:0, unvoiced:1
			int midx1 = 0, midx2 = 1, midx3 = 2 ;
			int atype1 = 0, atype2 = 0, atype3 = 0 ;
			
			for( node  = this.head ; node != null ; node = node.next )
			{
				buff = "" ;
				
				// get pronunciation
				str = node.Pron ;
				len = str.Length ;
				
				// parse pronunciation
				for( index  = 0 ; index <  len ; )
				{
					// get mora information
					if( mora1 == null )
					{
						GetMoraInformation( node, index, ref mora1, ref nlink1, ref flag1, ref size1, ref midx1, ref atype1 ) ;
					}

					if( mora1 == null )
					{
						Debug.LogError( "WARNING: set_unvoiced_vowel() in njd_set_unvoiced_vowel.c: Wrong pron." ) ;
						return ;
					}

					if( mora2 == null )
					{
						midx2 = midx1 + 1 ;
						atype2 = atype1 ;
						GetMoraInformation( node, index + size1, ref mora2, ref nlink2, ref flag2, ref size2, ref midx2, ref atype2 ) ;
					}

					if( mora3 == null )
					{
						midx3 = midx2 + 1 ;
						atype3 = atype2 ;
						GetMoraInformation( node, index + size1 + size2, ref mora3, ref nlink3, ref flag3, ref size3, ref midx3, ref atype3 ) ;
					}
					
					// rule 1: look-ahead for 'masu' and 'desu'
					if
					(
						mora2 != null && mora3 != null && nlink1 == nlink2 && nlink2 != nlink3 &&
						(
							mora1 == NJD_SET_UNVOICED_VOWEL_MA ||
							mora1 == NJD_SET_UNVOICED_VOWEL_DE
						) &&
						mora2 == NJD_SET_UNVOICED_VOWEL_SU &&
						(
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_DOUSHI		||
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_JODOUSHI	||
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_KANDOUSHI
						)
					)
					{
						if( nlink3.Pron == NJD_SET_UNVOICED_VOWEL_QUESTION || nlink3.Pron == NJD_SET_UNVOICED_VOWEL_CHOUON )
						{
							flag2 = 0 ;
						}
						else
						{
							flag2 = 1 ;
						}
					}
					
					// rule 2: look-ahead for 'shi'
					if
					(
						flag1 != 1 && flag2 == -1 && flag3 != 1 && mora2 != null &&
						nlink2.Pron == NJD_SET_UNVOICED_VOWEL_SHI &&
						(
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_DOUSHI		||
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_JODOUSHI	||
							nlink2.Pos == NJD_SET_UNVOICED_VOWEL_JOSHI
						)
					)
					{
						if( atype2 == midx2 + 1 )
						{
							// rule 4
							flag2 = 0 ;
						}
						else
						{
							// rule 5
							flag2 = ApplyUnvoiceRule( mora2, mora3 ) ;
						}

						if( flag2 == 1 )
						{
							if( flag1 == -1 )
							{
								flag1 = 0 ;
							}
							if( flag3 == -1 )
							{
								flag3 = 0 ;
							}
						}
					}
					
					// estimate unvoice
					if( flag1 == -1 )
					{
						if( nlink1.Pos == NJD_SET_UNVOICED_VOWEL_FILLER )
						{
							// rule 0
							flag1 = 0 ;
						}
						else
						if( flag2 == 1 )
						{
							// rule 3
							flag1 = 0 ;
						}
						else
						if( atype1 == midx1 + 1 )
						{
							// rule 4
							flag1 = 0 ;
						}
						else
						{
							// rule 5
							flag1 = ApplyUnvoiceRule( mora1, mora2 ) ;
						}
					}

					if( flag1 == 1 && flag2 == -1 )
					{
						flag2 = 0 ;
					}
					
					// store pronunciation

					buff += mora1 ;

					if( flag1 == 1 )
					{
						buff += NJD_SET_UNVOICED_VOWEL_QUOTATION ;
					}

					// prepare next step
					index += size1 ;
					
					mora1	= mora2 ;
					nlink1	= nlink2 ;
					size1	= size2 ;
					flag1	= flag2 ;
					midx1	= midx2 ;
					atype1	= atype2 ;

					mora2	= mora3 ;
					nlink2	= nlink3 ;
					size2	= size3 ;
					flag2	= flag3 ;
					midx2	= midx3 ;
					atype2	= atype3 ;

					mora3	= null ;
					nlink3	= null ;
					size3	= 0 ;
					flag3	= -1 ;
					midx3	= 0 ;
					atype3	= 0 ;
				}

				node.Pron = buff ;
			}
		}

		//---------------------------------------------------------------------------


		private void GetMoraInformation( NJDNode node, int index, ref string mora, ref NJDNode nlink, ref int flag, ref int size, ref int midx, ref int atype )
		{
			int i ;
			int matched_size ;
			string str = node.Pron ;
			int len = str.Length ;
			
			// find next word 
			if( index >= len )
			{
				if( node.next != null )
				{
					GetMoraInformation( node.next, index - len, ref mora, ref nlink, ref flag, ref size, ref midx, ref atype ) ;
				}
				else
				{
					mora	= null ;
					nlink	= null ;
					flag	= -1 ;
					size	= 0 ;
					midx	= 0 ;
					atype	= 0 ;
				}
				return ;
			}
			
			nlink = node ;
			
			// reset mora index and accent type for new word
			if( index == 0 && node.ChainFlag != 1 )
			{
				midx = 0 ;
				atype = node.Acc ;
			}
			
			// special symbol
			if( str == NJD_SET_UNVOICED_VOWEL_TOUTEN )
			{
				mora = NJD_SET_UNVOICED_VOWEL_TOUTEN ;
				flag = 0 ;
				size = NJD_SET_UNVOICED_VOWEL_TOUTEN.Length ;
				return ;
			}

			if( str == NJD_SET_UNVOICED_VOWEL_QUESTION )
			{
				mora = NJD_SET_UNVOICED_VOWEL_QUESTION ;
				flag = 0 ;
				size = NJD_SET_UNVOICED_VOWEL_QUESTION.Length ;
				return ;
			}
			
			// reset
			mora = null ;
			flag = -1 ;
			size = 0 ;
			
			// get mora
			for( i  = 0 ; njd_set_unvoiced_vowel_mora_list[ i ] != null ; i ++ )
			{
				matched_size = StrTopCmp( str, index, njd_set_unvoiced_vowel_mora_list[ i ] ) ;
				if( matched_size >  0 )
				{
					mora = njd_set_unvoiced_vowel_mora_list[ i ] ;
					size = matched_size ;
					break ;
				}
			}
			
			// get unvoice flag
			matched_size = StrTopCmp( str, index + size, NJD_SET_UNVOICED_VOWEL_QUOTATION ) ;
			if( matched_size >  0 )
			{
				flag = 1 ;
				size += matched_size ;
			}
		}

		private int ApplyUnvoiceRule( string current, string next )
		{
			int i, j ;
			
			if( next == null )
			{
				return 0 ;
			}
			
			for( i  = 0 ; njd_set_unvoiced_vowel_candidate_list1[ i ] != null ; i ++ )
			{
				if( current == njd_set_unvoiced_vowel_candidate_list1[ i ] )
				{
					for( j  = 0 ; njd_set_unvoiced_vowel_next_mora_list1[ j ] != null ; j ++ )
					{
						if( StrTopCmp( next, njd_set_unvoiced_vowel_next_mora_list1[ j ] ) >  0 )
						{
							return 1 ;
						}
					}
					return 0 ;
				}
			}

			for( i  = 0 ; njd_set_unvoiced_vowel_candidate_list2[ i ] != null ; i ++ )
			{
				if( current == njd_set_unvoiced_vowel_candidate_list2[ i ] )
				{
					for( j  = 0 ; njd_set_unvoiced_vowel_next_mora_list2[ j ] != null ; j ++ )
					{
						if( StrTopCmp( next, njd_set_unvoiced_vowel_next_mora_list2[ j ] ) >  0 )
						{
							return 1 ;
						}
					}
					return 0 ;
				}
			}

			for( i  = 0 ; njd_set_unvoiced_vowel_candidate_list3[ i ] != null ; i ++ )
			{
				if( current == njd_set_unvoiced_vowel_candidate_list3[ i ] )
				{
					for( j  = 0 ; njd_set_unvoiced_vowel_next_mora_list3[ j ] != null ; j ++ )
					{
						if( StrTopCmp( next, njd_set_unvoiced_vowel_next_mora_list3[ j ] ) >  0 )
						{
							return 1 ;
						}
					}
					return 0 ;
				}
			}

			return -1 ;	// unknown
		}

	}
}

