using System.Collections ;
using System.Collections.Generic ;
using System.Text ;
using UnityEngine ;

namespace OJT
{
	public class NJDNode
	{
		private const string m_NoData = "*" ;

		//-----------------------------------------------------------


		private	string	m_Word ;
		private	string	m_Pos ;
		private	string	m_PosGroup1 ;
		private string	m_PosGroup2 ;
		private	string	m_PosGroup3 ;
		private	string	m_CType ;			// conjugation type
		private	string	m_CForm ;			// conjugation form

		private	string	m_ChainRule ;
		private	int		m_ChainFlag ;

		private	string	m_Orig ;			// genkei
		private	string	m_Read ;			// yomi
		private	string	m_Pron ;			// hatsuon
		private	int		m_Acc ;			// accent
		private	int		m_MoraSize ;

		public	NJDNode prev ;
		public	NJDNode	next ;

		//---------------------------------------------------------------------------

		public NJDNode()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			m_Word			= null ;
			m_Pos			= null ;
			m_PosGroup1		= null ;
			m_PosGroup2		= null ;
			m_PosGroup3		= null ;
			m_CType			= null ;
			m_CForm			= null ;
			m_ChainRule		= null ;
			m_ChainFlag		= -1 ;

			m_Orig			= null ;
			m_Read			= null ;
			m_Pron			= null ;
			m_Acc			= 0 ;
			m_MoraSize		= 0 ;

			this.prev			= null ;
			this.next			= null ;
		}


		public void Load( string tString )
		{
			int i, j ;

			string	tOrig ;
			string	tRead ;
			string	tPron ;
			string	tAcc ;

			int count ;

			int tWordOffset ;
			int tOrigOffset ;
			int tReadOffset ;
			int tPronOffset ;
			int tAccOffset ;

			NJDNode prev = null ;
			
			//----------------------------------

			int tOffset = 0 ;
			string tToken ;

			//----------------------------------

			string tWord = GetTokenFromString( tString, ref tOffset, ',' ) ;


			m_Pos = GetTokenFromString( tString, ref tOffset, ',' ) ;
			m_PosGroup1 = GetTokenFromString( tString, ref tOffset, ',' ) ;
			m_PosGroup2 = GetTokenFromString( tString, ref tOffset, ',' ) ;
			m_PosGroup3 = GetTokenFromString( tString, ref tOffset, ',' ) ;

			m_CType = GetTokenFromString( tString, ref tOffset, ',' ) ;
			m_CForm = GetTokenFromString( tString, ref tOffset, ',' ) ;
			
			tOrig = GetTokenFromString( tString, ref tOffset, ',' ) ;
			tRead = GetTokenFromString( tString, ref tOffset, ',' ) ;
			tPron = GetTokenFromString( tString, ref tOffset, ',' ) ;
			tAcc = GetTokenFromString( tString, ref tOffset, ',' ) ;


			m_ChainRule = GetTokenFromString( tString, ref tOffset, ',' ) ;

			tToken = GetTokenFromString( tString, ref tOffset, ',' ) ;
			int tChainFlag = -1 ;	// デフォルトは -1
			if( int.TryParse( tToken, out tChainFlag ) == false )
			{
				tChainFlag = -1 ;
			}
			m_ChainFlag = tChainFlag ;

			// for symbol
			if( tAcc.IndexOf( "*" ) >= 0 || tAcc.IndexOf( "/" ) <  0 )
			{
				m_Word		= tWord ;
				m_Orig		= tOrig ;
				m_Read		= tRead ;
				m_Pron		= tPron ;
				m_Acc		= 0 ;
				m_MoraSize	= 0 ;
				return ;
			}
			
			for( i  = 0, count  = 0 ; i <  tAcc.Length ; i ++ )
			{
				if( tAcc[ i ] == '/' )
				{
					count ++ ;
				}
			}


			// for single word
			if( count == 1 )
			{
				m_Word = tWord ;
				m_Orig = tOrig ;
				m_Read = tRead ;
				m_Pron = tPron ;

				tAccOffset = 0 ;

				tToken = GetTokenFromString( tAcc, ref tAccOffset, '/' ) ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					j = 0 ;
					Debug.LogWarning( "WARNING: NJDNode_load() in njd_node.c: Accent is empty." ) ;
				}
				else
				{
					j = 0 ;
					int.TryParse( tToken, out j ) ;
				}
				m_Acc = j ;

				tToken = GetTokenFromString( tAcc, ref tAccOffset, ':' ) ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					j = 0 ;
					Debug.LogWarning( "WARNING: NJDNode_load() in njd_node.c: Mora size is empty." ) ;
				}
				else
				{
					j = 0 ;
					int.TryParse( tToken, out j ) ;
				}
				m_MoraSize = j ;

				return ;
			}

			// parse chained word
			tWordOffset		= 0 ;
			tOrigOffset		= 0 ;
			tReadOffset		= 0 ;
			tPronOffset		= 0 ;
			tAccOffset		= 0 ;

			NJDNode node = this ;

			for( i  = 0 ; i <  count ; i ++ )
			{
				if( i >  0 )
				{
					node = new NJDNode() ;
					node.Copy( node, prev ) ;
					node.m_ChainFlag = 0 ;
					node.prev = prev ;
					prev.next = node ;
				}

				// orig
				tToken = GetTokenFromString( tOrig, ref tOrigOffset, ',' ) ;
				m_Orig = tToken ;

				// string
				if( i + 1 <  count )
				{
					node.m_Word = tToken ;
					tWordOffset += tToken.Length ;
				}
				else
				{
					node.m_Word = tWord.Substring( tWordOffset ) ;
				}

				// read
				tToken = GetTokenFromString( tRead, ref tReadOffset, ',' ) ;
				node.m_Read = tToken ;

				// pron
				tToken = GetTokenFromString( tPron, ref tPronOffset, ',' ) ;
				node.m_Pron = tToken ;

				// acc
				tToken = GetTokenFromString( tAcc, ref tAccOffset, '/' ) ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					j = 0 ;
					Debug.LogWarning( "WARNING: NJDNode_load() in njd_node.c: Accent is empty." ) ;
				}
				else
				{
					j = 0 ;
					int.TryParse( tToken, out j ) ;
				}
				node.m_Acc = j ;

				// mora size
				tToken = GetTokenFromString( tAcc, ref tAccOffset, ':' ) ;
				if( string.IsNullOrEmpty( tToken ) == true )
				{
					j = 0 ;
					Debug.LogWarning( "WARNING: NJDNode_load() in njd_node.c: Mora size is empty." ) ;
				}
				else
				{
					j = 0 ;
					int.TryParse( tToken, out j ) ;
				}
				node.m_MoraSize = j ;

				prev = node ;
			}
		}

		public static NJDNode Insert( NJDNode prev, NJDNode next, NJDNode node )
		{
			NJDNode tail ;

			if( prev == null || next == null )
			{
				Debug.LogWarning( "ERROR: NJDNode_insert() in njd_node.c: NJDNodes are not specified." ) ;
				return null ;
			}

			for( tail  = node ; tail.next != null ; tail = tail.next ) ;

			prev.next = node ;
			node.prev = prev ;
			next.prev = tail ;
			tail.next = next ;

			return tail ;
		}

		//---------------------------------------------------------------------------

		public string Word
		{
			set
			{
				m_Word = value ;
			}
			get
			{
				if( m_Word == null )
				{
					return m_NoData ;
				}
				return m_Word ;
			}
		}

		public string Pos
		{
			set
			{
				m_Pos = value ;
			}
			get
			{
				if( m_Pos == null )
				{
					return m_NoData ;
				}
				return m_Pos ;
			}
		}

		public string PosGroup1
		{
			set
			{
				m_PosGroup1 = value ;
			}
			get
			{
				if( m_PosGroup1 == null )
				{
					return m_NoData ;
				}
				return m_PosGroup1 ;
			}
		}

		public string PosGroup2
		{
			set
			{
				m_PosGroup2 = value ;
			}
			get
			{
				if( m_PosGroup2 == null )
				{
					return m_NoData ;
				}
				return m_PosGroup2 ;
			}
		}

		public string PosGroup3
		{
			set
			{
				m_PosGroup3 = value ;
			}
			get
			{
				if( m_PosGroup3 == null )
				{
					return m_NoData ;
				}
				return m_PosGroup3 ;
			}
		}

		public string CType
		{
			set
			{
				m_CType = value ;
			}
			get
			{
				if( m_CType == null )
				{
					return m_NoData ;
				}
				return m_CType ;
			}
		}

		public string CForm
		{
			set
			{
				m_CForm = value ;
			}
			get
			{
				if( m_CForm == null )
				{
					return m_NoData ;
				}
				return m_CForm ;
			}
		}

		public string ChainRule
		{
			set
			{
				m_ChainRule = value ;
			}
			get
			{
				if( m_ChainRule == null )
				{
					return m_NoData ;
				}
				return m_ChainRule ;
			}
		}

		public int ChainFlag
		{
			set
			{
				m_ChainFlag = value ;
			}
			get
			{
				return m_ChainFlag ;
			}
		}

		public string Orig
		{
			set
			{
				m_Orig = value ;
			}
			get
			{
				if( m_Orig == null )
				{
					return m_NoData ;
				}
				return m_Orig ;
			}
		}

		public string Read
		{
			set
			{
				m_Read = value ;
			}
			get
			{
				if( m_Read == null )
				{
					return m_NoData ;
				}
				return m_Read ;
			}
		}

		public string Pron
		{
			set
			{
				m_Pron = value ;
			}
			get
			{
				if( m_Pron == null )
				{
					return m_NoData ;
				}
				return m_Pron ;
			}
		}

		public int Acc
		{
			set
			{
				m_Acc = value ;
				if( m_Acc <  0 )
				{
					Debug.LogWarning( "WARNING: NJDNode_set_acc() in njd_node.c: Accent must be positive value." ) ;
					m_Acc = 0 ;
				}
			}
			get
			{
				return m_Acc ;
			}
		}

		public int MoraSize
		{
			set
			{
				m_MoraSize = value ;
				if( m_MoraSize <  0 )
				{
					Debug.LogWarning( "WARNING: NJDNode_set_mora_size() in njd_node.c: Mora size must be positive value." ) ;
					m_MoraSize = 0 ;
				}
			}
			get
			{
				return m_MoraSize ;
			}
		}
		
		public void AddRead( string str )
		{
			m_Read += str ;
		}

		public void AddPron( string str )
		{
			m_Pron += str ;
		}

		public void AddMoraSize( int size )
		{
			m_MoraSize += size ;
			if( m_MoraSize <  0 )
			{
				Debug.LogWarning( "WARNING: NJDNode_add_mora_size() in njd_node.c: Mora size must be positive value." ) ;
				m_MoraSize = 0 ;
		   }
		}
		
		private void Copy( NJDNode tNode1, NJDNode tNode2 )
		{
			tNode1.m_Word		= tNode2.m_Word ;
			tNode1.m_Pos		= tNode2.m_Pos ;
			tNode1.m_PosGroup1	= tNode2.m_PosGroup1 ;
			tNode1.m_PosGroup2	= tNode2.m_PosGroup2 ;
			tNode1.m_PosGroup3	= tNode2.m_PosGroup3 ;
			tNode1.m_CType		= tNode2.m_CType ;
			tNode1.m_CForm		= tNode2.m_CForm ;
			tNode1.m_ChainRule	= tNode2.m_ChainRule ;
			tNode1.m_ChainFlag	= tNode2.m_ChainFlag ;

			tNode1.m_Orig		= tNode2.m_Orig ;
			tNode1.m_Read		= tNode2.m_Read ;
			tNode1.m_Pron		= tNode2.m_Pron ;
			tNode1.m_Acc		= tNode2.m_Acc ;
			tNode1.m_MoraSize	= tNode2.m_MoraSize ;
		}

		//---------------------------------------------------------------------------

		// 文字列から区切り記号で区切られたトークンを取り出す
		private string GetTokenFromString( string tString, ref int rIndex, char tSearator )
		{
			char c ;
			int o, l ;
			
			if( rIndex >= tString.Length )
			{
				return "" ;
			}

			c = tString[ rIndex ] ;
			if( c == tSearator )
			{
				// 空文字
				rIndex ++ ;
				return "" ;
			}

			o = rIndex ;
			l = 0 ;

			for( ; rIndex <  tString.Length ; )
			{
				c = tString[ rIndex ] ;
				rIndex ++ ;

				if( c != tSearator )
				{
					l ++ ;
				}
				else
				{
					break ;
				}
			}

			if( l == 0 )
			{
				// 最初から区切り記号(基本的にありえない)
				return "" ;
			}

			return tString.Substring( o, l ) ;
		}
	}
}
