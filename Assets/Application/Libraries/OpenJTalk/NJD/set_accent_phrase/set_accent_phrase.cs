using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text ;


namespace OJT
{
	public partial class NJD
	{
		public void SetAccentPhrase()
		{
			NJDNode node ;

			if( this.head == null )
			{
				return ;
			}
			
			for( node  = this.head.next ; node != null ; node = node.next )
			{
				if( node.ChainFlag <  0 )
				{
					// Rule 01
					node.ChainFlag = 1 ;

					// Rule 02
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
						{
							node.ChainFlag = 1 ;
						}
					}

					// Rule 03
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_KEIYOUSHI )
					{
						if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
						{
							node.ChainFlag = 0 ;
						}
					}

					// Rule 04
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						if( node.prev.PosGroup1 == NJD_SET_ACCENT_PHRASE_KEIYOUDOUSHI_GOKAN )
						{
							if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
							{
								node.ChainFlag = 0 ;
							}
						}
					}

					// Rule 05
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_DOUSHI )
					{
						if( node.Pos == NJD_SET_ACCENT_PHRASE_KEIYOUSHI )
						{
							node.ChainFlag = 0 ;
						}
						else
						if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
						{
							node.ChainFlag = 0 ;
						}
					}

					// Rule 06
					if
					(
						( node.Pos		== NJD_SET_ACCENT_PHRASE_FUKUSHI		) ||
						( node.prev.Pos	== NJD_SET_ACCENT_PHRASE_FUKUSHI		) ||
						( node.Pos		== NJD_SET_ACCENT_PHRASE_SETSUZOKUSHI	) ||
						( node.prev.Pos	== NJD_SET_ACCENT_PHRASE_SETSUZOKUSHI	) ||
						( node.Pos		== NJD_SET_ACCENT_PHRASE_RENTAISHI		) ||
						( node.Pos		== NJD_SET_ACCENT_PHRASE_RENTAISHI		)
					)
					{
						node.ChainFlag = 0 ;
					}

					// Rule 07
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						if( node.prev.PosGroup1 == NJD_SET_ACCENT_PHRASE_FUKUSHI_KANOU )
						{
							node.ChainFlag = 0 ;
						}
					}
					if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						if( node.PosGroup1 == NJD_SET_ACCENT_PHRASE_FUKUSHI_KANOU )
						{
							node.ChainFlag = 0 ;
						}
					}
					
					// Rule 08
					if( node.Pos == NJD_SET_ACCENT_PHRASE_JODOUSHI )
					{
						node.ChainFlag = 1 ;
					}
					if( node.Pos == NJD_SET_ACCENT_PHRASE_JOSHI )
					{
						node.ChainFlag = 1 ;
					}

					// Rule 09
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_JODOUSHI )
					{
						if( ( node.Pos != NJD_SET_ACCENT_PHRASE_JODOUSHI ) && ( node.Pos != NJD_SET_ACCENT_PHRASE_JOSHI ) )
						{
							node.ChainFlag = 0 ;
						}
					}
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_JOSHI )
					{
						if( ( node.Pos != NJD_SET_ACCENT_PHRASE_JODOUSHI ) && ( node.Pos != NJD_SET_ACCENT_PHRASE_JOSHI ) )
						{
							node.ChainFlag = 0 ;
						}
					}

					// Rule 10
					if( node.prev.PosGroup1 == NJD_SET_ACCENT_PHRASE_SETSUBI )
					{
						if( node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
						{
							node.ChainFlag = 0 ;
						}
					}

					// Rule 11
					if( node.Pos == NJD_SET_ACCENT_PHRASE_KEIYOUSHI )
					{
						if( node.PosGroup1 == NJD_SET_ACCENT_PHRASE_HIJIRITSU )
						{
							if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_DOUSHI )
							{
								if( StrTopCmp( node.prev.CForm, NJD_SET_ACCENT_PHRASE_RENYOU ) != -1 )
								{
									node.ChainFlag = 1 ;
								}
							}
							else
							if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_KEIYOUSHI )
							{
								if( StrTopCmp( node.prev.CForm, NJD_SET_ACCENT_PHRASE_RENYOU ) != -1 )
								{
									node.ChainFlag = 1 ;
								}
							}
							else
							if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_JOSHI )
							{
								if( node.prev.PosGroup1 == NJD_SET_ACCENT_PHRASE_SETSUZOKUJOSHI )
								{
									if( node.prev.Word == NJD_SET_ACCENT_PHRASE_TE )
									{
										node.ChainFlag = 1 ;
									}
									else
									if( node.prev.Word == NJD_SET_ACCENT_PHRASE_DE )
									{
										node.ChainFlag = 1 ;
									}
								}
							}
						}
					}

					// Rule 12
					if( node.Pos == NJD_SET_ACCENT_PHRASE_DOUSHI )
					{
						if( node.PosGroup1 == NJD_SET_ACCENT_PHRASE_HIJIRITSU )
						{
							if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_DOUSHI )
							{
								if( StrTopCmp( node.prev.CForm,  NJD_SET_ACCENT_PHRASE_RENYOU ) != -1 )
								{
									node.ChainFlag = 1 ;
								}
							}
							else
							if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
							{
								if( node.prev.PosGroup1 == NJD_SET_ACCENT_PHRASE_SAHEN_SETSUZOKU )
								{
									node.ChainFlag = 1 ;
								}
							}
						}
					}

					// Rule 13
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						if
						(
							( node.Pos			== NJD_SET_ACCENT_PHRASE_DOUSHI				) ||
							( node.Pos			== NJD_SET_ACCENT_PHRASE_KEIYOUSHI			) ||
							( node.PosGroup1	== NJD_SET_ACCENT_PHRASE_KEIYOUDOUSHI_GOKAN	)
						)
						{
							node.ChainFlag = 0 ;
						}
					}

					// Rule 14
					if( node.Pos == NJD_SET_ACCENT_PHRASE_KIGOU || node.prev.Pos == NJD_SET_ACCENT_PHRASE_KIGOU )
					{
						node.ChainFlag = 0 ;
					}

					// Rule 15
					if( node.Pos == NJD_SET_ACCENT_PHRASE_SETTOUSHI )
					{
						node.ChainFlag = 0 ;
					}

					// Rule 16
					if( node.prev.PosGroup3 == NJD_SET_ACCENT_PHRASE_SEI && node.Pos == NJD_SET_ACCENT_PHRASE_MEISHI )
					{
						node.ChainFlag = 0 ;
					}

					// Rule 17
					if( node.prev.Pos == NJD_SET_ACCENT_PHRASE_MEISHI && node.PosGroup3 == NJD_SET_ACCENT_PHRASE_MEI )
					{
						node.ChainFlag = 0 ;
					}

					// Rule 18
					if( node.PosGroup1 == NJD_SET_ACCENT_PHRASE_SETSUBI )
					{
						node.ChainFlag = 1 ;
					}
				}
			}
		}
	}
}

