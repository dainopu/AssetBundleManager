using System.Collections;
using System.Collections.Generic;
using System.Text ;

using UnityEngine ;

namespace OJT
{
	public partial class NJD
	{
		public void SetPronunciation()
		{
			NJDNode node ;
			string str ;
			int i, j = 0 ;
			int pos ;
			int len ;

			for( node = this.head ; node != null ; node = node.next )
			{
				if( node.MoraSize == 0 )
				{
					node.Read = null ;
					node.Pron = null ;

					// if the word is kana, set them as filler
					{
						// str は Shift-JIS のバイト配列
						str = node.Word ;
						len = str.Length ;


						for( pos  = 0 ; pos <  len ; )
						{
							for( i  = 0, j  = 0 ; njd_set_pronunciation_list[ i ] != null ; i += 3 )
							{
								j = StrTopCmp( str, pos, njd_set_pronunciation_list[ i ] ) ;
								if( j >  0 )
								{
									break ;
								}
							}

							if( j >  0 )
							{
								pos += j ;
								node.AddRead( njd_set_pronunciation_list[ i + 1 ] ) ;
								node.AddPron( njd_set_pronunciation_list[ i + 1 ] ) ;
								node.AddMoraSize( Atoi( njd_set_pronunciation_list[ i + 2 ] ) ) ;
							}
							else
							{
								pos ++ ;
							}
						}

						node.Pos = NJD_SET_PRONUNCIATION_FILLER ;
						node.PosGroup1 = null ;
						node.PosGroup2 = null ;
						node.PosGroup3 = null ;
					}

					// if known symbol, set the pronunciation
					if( node.Pron == "*" )
					{
						for( i  = 0 ; njd_set_pronunciation_symbol_list[ i ] != null ; i += 2 )
						{
							if( node.Word == njd_set_pronunciation_symbol_list[ i ] )
							{
								node.Read = njd_set_pronunciation_symbol_list[ i + 1 ] ;
								node.Pron = njd_set_pronunciation_symbol_list[ i + 1 ] ;
								break ;
							}
						}
					}

					// if the word is not kana, set pause symbol
					if( node.Pron == "*" )
					{
						node.Read	= NJD_SET_PRONUNCIATION_TOUTEN ;
						node.Pron	= NJD_SET_PRONUNCIATION_TOUTEN ;
						node.Pos	= NJD_SET_PRONUNCIATION_KIGOU ;
					}
				}
			}

			this.remove_silent_node() ;

			for( node  = this.head ; node != null ; node = node.next )
			{
				if
				(
					node.next				!= null &&
					node.next.Pron			== NJD_SET_PRONUNCIATION_U &&
					node.next.Pos			== NJD_SET_PRONUNCIATION_JODOUSHI &&
					( node.Pos == NJD_SET_PRONUNCIATION_DOUSHI || node.Pos == NJD_SET_PRONUNCIATION_JODOUSHI ) &&
					node.MoraSize >  0
				)
				{
					node.next.Pron = NJD_SET_PRONUNCIATION_CHOUON ;
				}

				if
				(
					node.next		!= null &&
					node.Pos		== NJD_SET_PRONUNCIATION_JODOUSHI &&
					node.next.Word	== NJD_SET_PRONUNCIATION_QUESTION
				)
				{
					if( node.Word == NJD_SET_PRONUNCIATION_DESU_STR )
					{
						node.Pron = NJD_SET_PRONUNCIATION_DESU_PRON ;
					}
					else
					if( node.Word == NJD_SET_PRONUNCIATION_MASU_STR )
					{
						node.Pron = NJD_SET_PRONUNCIATION_MASU_PRON ;
					}
				}
			}
		}
	}
}
