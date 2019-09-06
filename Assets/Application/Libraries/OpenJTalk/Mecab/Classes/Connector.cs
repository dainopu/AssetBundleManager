using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using HTS_Engine_API ;


namespace MecabForOpenJTalk.Classes
{
	public class Connector : Common
	{
		private	Mmap<short>		m_CMmap = new Mmap<short>() ;

		// オフセット操作
		private int				m_MatrixOffset ;
			
		private ushort			m_LSize ;
		private ushort			m_RSize ;

		//---------------------------------------------------------------------------

		public bool Open( string tDirectory )
		{
			string tPath = Path.Combine( tDirectory, MATRIX_FILE ).Replace( "\\", "/" ) ;

			if( m_CMmap.Open( tPath ) == false )
			{
				Debug.LogWarning( "cannot open: " + tPath ) ;
				return false ;
			}

			byte[] tData = m_CMmap.Data ;
			if( tData == null )
			{
				Debug.LogWarning( "matrix is NULL" ) ;
				return false ;
			}

			if( m_CMmap.Size <  2 )
			{
				Debug.LogWarning( "file size is invalid: " + tPath ) ;
				return false ;
			}

			m_LSize = ( ushort )m_CMmap[ 0 ] ;
			m_RSize = ( ushort )m_CMmap[ 1 ] ;

			if( ( 2 + m_LSize * m_RSize ) != m_CMmap.Size )
			{
				Debug.LogWarning( "file size is invalid: " + tPath ) ;
				return false ;
			}

			// オフセット操作
			m_MatrixOffset = 2 ;

			return true ;
		}

		public void Close()
		{
			m_CMmap.Close() ;
		}

		public int LSize
		{
			get
			{
				return ( int )m_LSize ;
			}
		}

		public int RSize
		{
			get
			{
				return ( int )m_RSize ;
			}
		}

		public short GetMatrix( int tIndex )
		{
			return m_CMmap[ m_MatrixOffset + tIndex ] ;
		}

		public int GetCost( Node tLNode, Node tRNode )
		{
//				return matrix_[ lNode.rcAttr + lsize_ * rNode.lcAttr ] + rNode.wcost ;
			return m_CMmap[ m_MatrixOffset + tLNode.rcAttr + m_LSize * tRNode.lcAttr ] + tRNode.wcost ;
		}
	}
}
