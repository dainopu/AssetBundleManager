using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommonLabelMora
	{
		public	string					mora ;
		public	JPCommonLabelPhoneme	head ;
		public	JPCommonLabelPhoneme	tail ;
		public	JPCommonLabelMora		prev ;
		public	JPCommonLabelMora		next ;
		public	JPCommonLabelWord		up ;

		public void Initialize( string mora, JPCommonLabelPhoneme head, JPCommonLabelPhoneme tail, JPCommonLabelMora prev, JPCommonLabelMora next, JPCommonLabelWord up )
		{
			this.mora	= mora ;
			this.head	= head ;
			this.tail	= tail ;
			this.prev	= prev ;
			this.next	= next ;
			this.up		= up ;
		}

		public void Clear()
		{
			this.mora = null ;
		}
	}
}
