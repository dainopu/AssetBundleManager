using System.Collections;
using System.Collections.Generic;
using System.Text ;
using UnityEngine;


namespace OJT
{
	public partial class NJD
	{
		public NJDNode	head ;
		public NJDNode	tail ;

		public NJD()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			this.head	= null ;
			this.tail	= null ;
		}

		public void Refresh()
		{
		   this.Initialize() ;
		}

		public bool Analyze( string[] tFeature )
		{
			int i ;
			NJDNode tNode ;

			Debug.LogWarning( "要素数:" + tFeature.Length ) ;

			for( i  = 0 ; i <  tFeature.Length ; i ++ )
			{
				Debug.LogWarning( "要素[ " + i + " ] " + tFeature[ i ] ) ;
				
				tNode = new NJDNode() ;
				tNode.Load( tFeature[ i ] ) ;

				PushNode( tNode ) ;
			}

			Debug.Log( "====================================" ) ;

			SetPronunciation() ;
			SetDigit() ;
			SetAccentPhrase() ;
			SetAccentType() ;
			SetUnvoicedVowel() ;

			return true ;
		}

		private void PushNode( NJDNode node )
		{
			if( this.head == null )
			{
				this.head  = node ;
			}
			else
			{
				this.tail.next = node ;
				node.prev = this.tail ;
			}

			while( node.next != null )
			{
				node = node.next ;
			}

			this.tail = node ;
		}

		private NJDNode remove_node( NJDNode node )
		{
			NJDNode next ;

			if( node == this.head && node == this.tail )
			{
				this.head = null ;
				this.tail = null ;
				next = null ;
			}
			else
			if( node == this.head )
			{
				this.head = node.next ;
				this.head.prev = null ;
				next = this.head ;
			}
			else
			if( node == this.tail )
			{
				this.tail = node.prev ;
				this.tail.next = null ;
				next = null ;
			}
			else
			{
				node.prev.next = node.next ;
				node.next.prev = node.prev ;
				next = node.next ;
			}

			node.Initialize() ;

			return next ;
		}

		private void remove_silent_node()
		{
			NJDNode node ;
			
			for( node = this.head ; node != null ; )
			{
				if( node.Pron == "*" )
				{
					node = this.remove_node( node ) ;
				}
				else
				{
					node = node.next ;
				}
			}
		}


		//---------------------------------------------------------------------------

		private int StrTopCmp( string s, string p )
		{
			if( s.IndexOf( p ) >= 0 )
			{
				return p.Length ;
			}
			return -1 ;
		}

		private int StrTopCmp( string s, int o, string p )
		{
			int i = s.Substring( o ).IndexOf( p ) ;
			if( i == 0 )
			{
				// 先頭とマッチしなければだめ
				return p.Length ;
			}
			return -1 ;
		}

		private int Atoi( string tWord, int tDefaultValue = 0 )
		{
			int v = tDefaultValue ;
			if( int.TryParse( tWord, out v ) == false )
			{
				v = tDefaultValue ;
			}
			return v ;
		}
	}
}
