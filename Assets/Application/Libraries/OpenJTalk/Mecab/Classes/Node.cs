using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace MecabForOpenJTalk.Classes
{
	// mecab.h
	public class Node
	{
		public Node		prev ;
		public Node		next ;

		public Node		enext ;
		public Node		bnext ;

		public byte[]	surface_s ;	// オフセット byte[] -> int
		public int		surface_o ;
		public byte[]	feature_s ;	// オフセット btte[] -> int
		public int		feature_o ;

		public ushort	length ;
		public ushort	rlength ;

		public ushort	rcAttr ;
		public ushort	lcAttr ;

//		public ushort	posid ;
		public byte		char_type ;
		public byte		stat ;
		public byte		isbest ;

//		public float	alpha ;
//		public float	beta ;
//		public float	prob ;

		public short	wcost ;
		public long		cost ;
	}
}
