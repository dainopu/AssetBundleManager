using System.Collections;
using System.Collections.Generic;
using System.Text ;

using UnityEngine ;


namespace OJT
{
	public partial class NJD
	{
		public void SetAccentType()
		{
			NJDNode node ;
			NJDNode top_node = null ;
			string rule = "" ;
			int add_type = 0 ;
			int mora_size = 0 ;

			if( this.head == null )
			{
				return ;
			}

			for( node  = this.head ; node != null ; node = node.next )
			{
				if( node.Word == null )
				{
					continue ;
				}

				if( node == this.head || node.ChainFlag != 1 )
				{
					// store the top node
					top_node = node ;
					mora_size = 0 ;
				}
				else
				if( node.prev != null && node.ChainFlag == 1 )
				{
					// get accent change type
					GetRule( node.ChainRule, node.prev.Pos, ref rule, ref add_type ) ;
					
					// change accent type
					if( rule == "*" )
					{
						// no chnage
					}
					else
					if( rule == "F1" )
					{
						// for ancillary word
					}
					else
					if( rule == "F2" )
					{
						if( top_node.Acc == 0 )
						{
							top_node.Acc = mora_size + add_type ;
						}
					}
					else
					if( rule == "F3" )
					{
						if( top_node.Acc != 0 )
						{
							top_node.Acc = mora_size + add_type ;
						}
					}
					else
					if( rule == "F4" )
					{
						top_node.Acc = mora_size + add_type ;
					}
					else
					if( rule == "F5" )
					{
						top_node.Acc = 0 ;
					}
					else
					if( rule == "C1" )
					{
						// for noun
						top_node.Acc = mora_size + node.Acc ;
					}
					else
					if( rule == "C2" )
					{
						top_node.Acc = mora_size + 1 ;
					}
					else
					if( rule == "C3" )
					{
						top_node.Acc = mora_size ;
					}
					else
					if( rule == "C4" )
					{
						top_node.Acc = 0 ;
					}
					else
					if( rule == "C5" )
					{
					}
					else
					if( rule == "P1" )
					{
						// for postfix
						if( node.Acc == 0 )
						{
							top_node.Acc = 0 ;
						}
						else
						{
							top_node.Acc = mora_size + node.Acc ;
						}
					}
					else
					if( rule == "P2" )
					{
						if( node.Acc == 0 )
						{
							top_node.Acc = mora_size + 1 ;
						}
						else
						{
							top_node.Acc = mora_size + node.Acc ;
						}
					}
					else
					if( rule == "P6" )
					{
						top_node.Acc = 0 ;
					}
					else
					if( rule == "P14" )
					{
						if( node.Acc != 0 )
						{
							top_node.Acc = mora_size + node.Acc ;
						}
					}
				}
				
				// change accent type for digit
				if
				(
					( node.prev				!= null						) &&
					( node.ChainFlag		== 1						) &&
					( node.prev.PosGroup1	== NJD_SET_ACCENT_TYPE_KAZU	) &&
					( node.PosGroup1		== NJD_SET_ACCENT_TYPE_KAZU	)
				)
				{
					if( node.Word == NJD_SET_ACCENT_TYPE_JYUU )
					{
						// 10^1
						if
						(
							node.prev.Word != null &&
							(
								node.prev.Word == NJD_SET_ACCENT_TYPE_SAN		||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_YON		||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_KYUU	||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_NAN		||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_SUU
							)
						)
						{
							node.prev.Acc = 1 ;
						}
						else
						{
							node.prev.Acc = 1 ;
						}
						
						if
						(
							node.prev.Word != null &&
							(
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_GO		||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_ROKU	||
								node.prev.Word ==	NJD_SET_ACCENT_TYPE_HACHI
							)
						)
						{
							if
							(
								node.next != null && node.next.Word != null &&
								(
									node.next.Word ==	NJD_SET_ACCENT_TYPE_ICHI	||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_NI		||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_SAN		||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_YON		||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_GO		||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_ROKU	||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_NANA	||
									node.next.Word ==	NJD_SET_ACCENT_TYPE_HACHI	||
									node.next.Word == NJD_SET_ACCENT_TYPE_KYUU
								)
							)
							{
								node.prev.Acc = 0 ;
							}
						}
					}
					else
					if( node.Word == NJD_SET_ACCENT_TYPE_HYAKU )
					{
						// 10^2
						if( node.prev.Word != null && node.prev.Word == NJD_SET_ACCENT_TYPE_NANA )
						{
							node.prev.Acc = 2 ;
						}
						else
						if
						(
							node.prev.Word != null &&
							(
								node.prev.Word == NJD_SET_ACCENT_TYPE_SAN	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_YON	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_KYUU	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_NAN
							)
						)
						{
							node.prev.Acc = 1 ;
						}
						else
						{
							node.prev.Acc = node.prev.MoraSize + node.MoraSize ;
						}
					}
					else
					if( node.Word == NJD_SET_ACCENT_TYPE_SEN )
					{
						// 10^3
						node.prev.Acc = node.prev.MoraSize + 1 ;
					}
					else
					if( node.Word == NJD_SET_ACCENT_TYPE_MAN )
					{
						// 10^4
						node.prev.Acc = node.prev.MoraSize + 1 ;
					}
					else
					if( node.Word == NJD_SET_ACCENT_TYPE_OKU )
					{
						// 10^8
						if
						(
							node.prev.Word != null &&
							(
								node.prev.Word == NJD_SET_ACCENT_TYPE_ICHI	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_ROKU	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_NANA	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_HACHI	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_IKU
							)
						)
						{
							node.prev.Acc = 2 ;
						}
						else
						{
							node.prev.Acc = 1 ;
						}
					}
					else
					if( node.Word == NJD_SET_ACCENT_TYPE_CHOU )
					{
						// 10^12
						if
						(
							node.prev.Word != null &&
							(
								node.prev.Word == NJD_SET_ACCENT_TYPE_ROKU	||
								node.prev.Word == NJD_SET_ACCENT_TYPE_NANA
							)
						)
						{
							node.prev.Acc = 2 ;
						}
						else
						{
							node.prev.Acc = 1 ;
						}
					}
				}
				
				if
				(
					node.Word			== NJD_SET_ACCENT_TYPE_JYUU	&&
					node.ChainFlag		!= 1						&&
					node.next			!= null						&&
					node.next.PosGroup1	== NJD_SET_ACCENT_TYPE_KAZU
				)
				{
					node.Acc = 0 ;
				}
				
				mora_size += node.MoraSize ;
			}
		}

		private char GetTokenFromString( string tString, ref int rIndex, out string oToken )
		{
			char c = ( char )0 ;
			int o, l ;
			
			oToken = "" ;

			if( rIndex >= tString.Length )
			{
				return ( char )0 ;
			}

			c = tString[ rIndex ] ;
			if( c == '%' || c == '@' || c == '/' )
			{
				// 空文字
				rIndex ++ ;
				return c ;
			}

			o = rIndex ;
			l = 0 ;
			c = ( char )0 ;

			for( ; rIndex <  tString.Length ; )
			{
				c = tString[ rIndex ] ;
				rIndex ++ ;

				if( c != '%' && c != '@' && c != '/' )
				{
					l ++ ;
				}
				else
				{
					break ;
				}
			}

			if( rIndex >= tString.Length )
			{
				c = ( char )0 ;	// 終端へ到達(重要)
			}

			if( l == 0 )
			{
				// 最初から区切り記号(基本的にありえない)
				return c ;
			}

			oToken = tString.Substring( o, l ) ;

			return c ;
		}

		private void GetRule( string input_rule, string prev_pos, ref string rRule, ref int add_type )
		{
			int index = 0 ;
			string buff ;
			char c = ' ' ;
			
			if( input_rule != null )
			{
				while( c != 0 )
				{
					c = GetTokenFromString( input_rule, ref index, out buff ) ;

					if( ( c == '%' && prev_pos.IndexOf( buff ) >= 0 ) || c == '@' || c == '/' || c == 0 )
					{
						// find
						if( c == '%' )
						{
							c = GetTokenFromString( input_rule, ref index, out rRule ) ;
						}
						else
						{
							rRule = buff ;
						}

						if( c == '@' )
						{
							c = GetTokenFromString( input_rule, ref index, out buff ) ;
							add_type = Atoi( buff ) ;
						}
						else
						{
							add_type = 0 ;
						}
						return ;
					}
					else
					{
						// skip
						while( c == '%' || c == '@' )
						{
							c = GetTokenFromString( input_rule, ref index, out buff ) ;
						}
					}
				}
			}
			
			// not found
			rRule = "*" ;
			add_type = 0 ;
		}		
	}
}
