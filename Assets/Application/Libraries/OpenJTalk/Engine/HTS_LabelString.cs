using System ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_LabelString
	{
		public HTS_LabelString	next ;				// pointer to next label string
		public string			name ;				// label string
		public double			start ;				// start frame specified in the given label
		public double			end ;				// end frame specified in the given label
	}
}
