using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommonNode
	{
		public	string	Pron ;			// pronunciation
		public	string	Pos ;			// part of speech
		public	string	CType ;			// conjugation type
		public	string	CForm ;			// conjugation form
		public	int		Acc ;			// accent type
		public	int		ChainFlag ;		// chain flag

		public	JPCommonNode prev ;
		public	JPCommonNode next ;
		

		public JPCommonNode()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			this.Pron		= null ;
			this.Pos		= null ;
			this.CType		= null ;
			this.CForm		= null ;
			this.Acc		= 0 ;
			this.ChainFlag	= -1 ;

			this.prev		= null ;
			this.next		= null ;
		}

		public void Clear()
		{
			Initialize() ;
		}
	}

}
