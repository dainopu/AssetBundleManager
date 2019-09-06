using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OJT
{
	public class JPCommonLabelPhoneme
	{
		public	string					phoneme ;
		public	JPCommonLabelPhoneme	prev ;
		public	JPCommonLabelPhoneme	next ;
		public	JPCommonLabelMora		up ;

		public void Initialize( string phoneme, JPCommonLabelPhoneme prev, JPCommonLabelPhoneme next, JPCommonLabelMora up )
		{
			this.phoneme	= phoneme ;
			this.prev		= prev ;
			this.next		= next ;
			this.up			= up ;
		}

		public void Clear()
		{
			this.phoneme = null ;
		}

		public void ConvertUnvoice()
		{
			int i ;
			
			for( i  = 0 ; jpcommon_unvoice_list[ i ] != null ; i += 2 )
			{
				if( jpcommon_unvoice_list[ i ] == this.phoneme )
				{
					this.phoneme = jpcommon_unvoice_list[ i + 1 ] ;
					return ;
				}
			}
			
			Debug.LogError( "WARNING: JPCommonLabelPhoneme_convert_unvoice() in jpcommon_label.c: " + this.phoneme + " cannot be unvoiced." ) ;
		}

		//---------------------------------------------------------------------------

		private static string[] jpcommon_unvoice_list =
		{
			"a", "A",
			"i", "I",
			"u", "U",
			"e", "E",
			"o", "O",
			null, null
		} ;
	}


}
