using System.Collections;
using System.Collections.Generic;

namespace HTS_Engine_API
{
	public class HTS_Node
	{
		public int			index ;					// index of this node
		public int			pdf ;					// index of PDF for this node (leaf node only)
		public HTS_Node		yes ;					// pointer to its child node (yes)
		public HTS_Node		no ;					// pointer to its child node (no)
		public HTS_Node		next ;					// pointer to the next node
		public HTS_Question	quest ;					// question applied at this node

		//-----------------------------------------------------------

		public HTS_Node()
		{
			Initialize() ;
		}

		private void Initialize()
		{
			index	= 0 ;
			pdf		= 0 ;
			yes		= null ;
			no		= null ;
			next	= null ;
			quest	= null ;
		}

		// HTS_Node_find: find node for given number
		public static HTS_Node Find( HTS_Node node, int num )
		{
			for( ; node != null ; node = node.next )
			{
				if( node.index == num )
				{
					return node ;
				}
			}

			return null ;
		}
	}
}
