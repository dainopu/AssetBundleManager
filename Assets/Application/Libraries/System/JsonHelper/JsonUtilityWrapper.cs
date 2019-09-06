using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JsonHelper
{
	public class JsonUtilityWrapper
	{
		public static string ToJson( object tObject, bool tPrettyPrint = false, bool tReduce = false )
		{
			string tJsonText = JsonUtility.ToJson( tObject, tPrettyPrint ) ;

			if( tReduce == true )
			{
				JsonObject tJsonObject = new JsonObject( tJsonText ) ;
				tJsonText = tJsonObject.ToString( "\t", false ) ;
			}

			return tJsonText ;
		}
	}
}
