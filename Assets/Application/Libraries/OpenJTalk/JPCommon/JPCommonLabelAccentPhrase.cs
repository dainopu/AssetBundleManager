

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommonLabelAccentPhrase
	{
		public	int							accent ;
		public	string						emotion ;
		public	JPCommonLabelWord			head ;
		public	JPCommonLabelWord			tail ;
		public	JPCommonLabelAccentPhrase	prev ;
		public	JPCommonLabelAccentPhrase	next ;
		public	JPCommonLabelBreathGroup	up ;


		public void Initialize( int acc, string emotion, JPCommonLabelWord head, JPCommonLabelWord tail, JPCommonLabelAccentPhrase prev, JPCommonLabelAccentPhrase next, JPCommonLabelBreathGroup up )
		{
			this.accent = acc ;

			if( emotion != null )
			{
				this.emotion = emotion ;
			}
			else
			{
				this.emotion = null ;
			}
			
			this.head	= head ;
			this.tail	= tail ;
			this.prev	= prev ;
			this.next	= next ;
			this.up		= up ;
		}

		public void Clear()
		{
			this.emotion  = null ;
		}
	}
}

