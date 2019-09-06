using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommonLabelBreathGroup
	{
		public	JPCommonLabelAccentPhrase	head ;
		public	JPCommonLabelAccentPhrase	tail ;
		public	JPCommonLabelBreathGroup	prev ;
		public	JPCommonLabelBreathGroup	next ;

		public void Initialize( JPCommonLabelAccentPhrase head, JPCommonLabelAccentPhrase tail, JPCommonLabelBreathGroup prev, JPCommonLabelBreathGroup next )
		{
			this.head	= head ;
			this.tail	= tail ;
			this.prev	= prev ;
			this.next	= next ;
		}

		public void Clear()
		{
		}
	}
}
