using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Tree
	{
		public HTS_Pattern	head ;					// pointer to the head of pattern list for this tree
		public HTS_Tree		next ;					// pointer to next tree
		public HTS_Node		root ;					// root node of this tree
		public int			state ;					// state index of this tree
		
		//-----------------------------------------------------------

		public HTS_Tree()
		{
			Initialize() ;
		}
	
		private void Initialize()
		{
			this.head	= null ;
			this.next	= null ;
			this.root	= null ;
			this.state	= 0 ;
		}

		// HTS_Tree_load: load trees
		public bool Load( HTS_File fp, HTS_Question question )
		{
			string tToken ;
			
			HTS_Node node, last_node ;
			
			if( fp == null )
			{
				return false ;
			}

			tToken = fp.GetPatternToken() ;
			if( string.IsNullOrEmpty( tToken ) == true )
			{
				Initialize() ;
				return false ;
			}

			node = new HTS_Node() ;

			this.root = last_node = node ;
			
			if( tToken == "{" )
			{
				int v ;

				tToken = fp.GetPatternToken() ;
				while( string.IsNullOrEmpty( tToken ) == false && tToken != "}" )
				{
					int.TryParse( tToken, out v ) ;
					node = HTS_Node.Find( last_node, v ) ;
					if( node == null )
					{
						Debug.LogError( "HTS_Tree_load: Cannot find node " + v + " ." ) ;
						Initialize() ;
						return false ;
					}

					tToken = fp.GetPatternToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						Initialize() ;
						return false ;
					}

					node.quest = HTS_Question.Find( question, tToken ) ;
					if( node.quest == null )
					{
						Debug.LogError( "HTS_Tree_load: Cannot find question " + tToken + "." ) ;
						Initialize() ;
						return false ;
					}

					node.yes	= new HTS_Node() ;
					node.no		= new HTS_Node() ;
					
					tToken = fp.GetPatternToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						node.quest	= null ;
						node.yes	= null ;
						node.no		= null ;
						Initialize() ;
						return false ;
					}

					if( IsNum( tToken ) == true )
					{
						int.TryParse( tToken, out v ) ;
						node.no.index = v ;
					}
					else
					{
						node.no.pdf = Name2num( tToken ) ;
					}
					node.no.next = last_node ;
					last_node = node.no ;
					
					tToken = fp.GetPatternToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						node.quest	= null ;
						node.yes	= null ;
						node.no		= null ;
						Initialize() ;
						return false ;
					}

					if( IsNum( tToken ) == true )
					{
						int.TryParse( tToken, out v ) ;
						node.yes.index = v ;
					}
					else
					{
						node.yes.pdf = Name2num( tToken ) ;
					}
					node.yes.next = last_node ;
					last_node = node.yes ;

					// 最後
					tToken = fp.GetPatternToken() ;
				}
			}
			else
			{
				node.pdf = Name2num( tToken ) ;
			}
			
			return true ;
		}

		// HTS_Tree_parse_pattern: parse pattern specified for each tree
		public void ParsePattern( string tString )
		{
			// {xxx,yyy,zzz} または {(xxx,yyy,zzz)} というパターンから値を切り出す
			HTS_Pattern pattern, last_pattern ;
			
			this.head = null ;
			last_pattern = null ;

			int i, l = tString.Length ;
			int p, q ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tString[ i ] == '{' )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return ;
			}

			i ++ ;

			if( i >= l )
			{
				return ;
			}

			if( tString[ i ] == '(' )
			{
				i ++ ;
			}

			if( i >= l )
			{
				return ;
			}

			p = i ;

			for( i  = p ; i <  l ; i ++ )
			{
				if( tString[ i ] == '}' )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return ;
			}

			i -- ;

			if( tString[ i ] == ')' )
			{
				i -- ;
			}

			q = i ;

			//----------------------------------

			l = q - p + 1 ;	

			string s = tString.Substring( p, l ) ;

			s = s + "," ;

			l ++ ;

			p = 0 ;


			// parse pattern
			while( p <  l )
			{
				pattern = new HTS_Pattern() ;
				if( this.head != null )
				{
					last_pattern.next = pattern ;
				}
				else
				{
					this.head = pattern ;
				}

				for( i  = p ; i <  l ; i ++ )
				{
					if( s[ i ] == ',' )
					{
						break ;
					}
				}

				pattern.word = s.Substring( p, i - p ) ;
				p = i + 1 ;

				pattern.next = null ;
				last_pattern = pattern ;
			}
		}

		// HTS_Node_search: tree search
		public int SearchNode( string tString )
		{
			HTS_Node node = this.root ;
			
			while( node != null )
			{
				if( node.quest == null )
				{
					return node.pdf ;
				}

				if( node.quest.Match( tString ) == true )
				{
					if( node.yes.pdf >  0 )
					{
						return node.yes.pdf ;
					}
					node = node.yes ;
				}
				else
				{
					if( node.no.pdf >  0 )
					{
						return node.no.pdf ;
					}
					node = node.no ;
				}
			}
			
			Debug.LogError( "HTS_Tree_search_node: Cannot find node." ) ;
			return 1 ;
		}

		//---------------------------------------------------------------------------

		// HTS_is_num: check given buffer is number or not
		private bool IsNum( string s )
		{
			int i ;
			int length = s.Length ;
			
			for( i  = 0 ; i <  length ; i ++ )
			{
				if( !( ( s[ i ] >= '0' && s[ i ] <= '9' ) || ( s[ i ] == '-' ) ) )
				{
					return false ;
				}
			}
			
			return true ;
		}

		// HTS_name2num: convert name of node to number
		private int Name2num( string s )
		{
			int i ;
			
			for( i = s.Length - 1 ; '0' <= s[ i ] && s[ i ] <= '9'; i -- ) ;
			i ++ ;

			int v = 0 ;
			int.TryParse( s.Substring( i, s.Length - i ), out v ) ;

			return v ;
		}
	}
}

