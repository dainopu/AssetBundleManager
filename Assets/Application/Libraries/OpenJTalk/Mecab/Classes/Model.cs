using System ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

using HTS_Engine_API ;


namespace MecabForOpenJTalk.Classes
{
	public class Model : Common
	{
		private Tokenizer	m_Tokenizer	= null ;
		private Connector	m_Connector	= null ;
		
		//---------------------------------------------------------------------------

		private void Initialize()
		{
			m_Tokenizer = null ;
			m_Connector = null ;
		}
		
		/// <summary>
		/// 展開
		/// </summary>
		/// <param name="tDirectory"></param>
		/// <returns></returns>
		public bool Open( string tDirectory )
		{
			m_Tokenizer = new Tokenizer() ;
			if( m_Tokenizer.Open( tDirectory ) == false )
			{
				// 成功すれば辞書は最低１つは登録されているので辞書の存在のチェックは不要
				Initialize() ;
				return false ;
			}

			m_Connector = new Connector() ;
			if( m_Connector.Open( tDirectory ) == false )
			{
				Initialize() ;
				return false ;
			}

			//----------------------------------

			if( m_Tokenizer.LSize != m_Connector.LSize || m_Tokenizer.RSize != m_Connector.RSize )
			{
				Initialize() ;
				return false ;
			}

			//---------------------------------------------------------

			return true ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 解析
		/// </summary>
		/// <param name="lattice"></param>
		/// <returns></returns>
		public string[] Analyze( string tText )
		{
			if( m_Tokenizer == null || m_Connector == null )
			{
				return null ;
			}

			//----------------------------------------------------------

			// 文字コードに応じて文字列をバイト配列に変換する
			CharsetCode tCode = m_Tokenizer.GetCharsetCode() ;

			byte[] tSentence = StringToBytes( tCode, tText ) ;	// 終端記号が必要
			int tSize = GetLength( tSentence ) ;

			Node[]	tBeginNodes	= new Node[ tSize + 4 ] ;
			Node[]	tEndNodes	= new Node[ tSize + 4 ] ;

			//----------------------------------------------------------

			if( Process( tSentence, tBeginNodes, tEndNodes ) == false )
			{
				return null ;
			}

			if( BuildBestLattice( tEndNodes, tSize ) == false )
			{
				return null ;
			}

			//----------------------------------------------------------

			tSize = 0 ;
			for( Node tNode = tEndNodes[ 0 ] ; tNode != null ; tNode = tNode.next )
			{
				if( tNode.stat != MECAB_BOS_NODE && tNode.stat != MECAB_EOS_NODE )
				{
					tSize ++ ;
				}
			}
			
			if( tSize == 0 )
			{
				return null ;
			}

			string[] tFeature = new string[ tSize ] ;

			tSize = 0 ;
			for( Node tNode  = tEndNodes[ 0 ] ; tNode != null ; tNode = tNode.next )
			{
				if( tNode.stat != MECAB_BOS_NODE && tNode.stat != MECAB_EOS_NODE )
				{
					tFeature[ tSize ] =
						BytesToString( tCode, tNode.surface_s, tNode.surface_o, tNode.length ) +
						"," + 
						BytesToString( tCode, tNode.feature_s, tNode.feature_o ) ;

//					Debug.LogWarning( "--- feature : " + tSize + " = " + tFeature[ tSize ] ) ;

					tSize ++ ;
				}
			}

			return tFeature ;
		}

		// 解析処理を実行する
		private bool Process( byte[] tSentence, Node[] tBeginNodes, Node[] tEndNodes )
		{
			// 文字コードが異なるバイト配列化されているので文字数は配列数ではなくなっている
			int tSize = GetLength( tSentence ) ;

			// ポインタ→オフセット
			int		tBegin	= 0 ;	// 文字列
			int		tEnd	= tSize ;			// 文字列の最後の位置(終端記号の場所を指しているはず)

			Node tBOSNode = m_Tokenizer.GetBOSNode() ;

			// オフセット操作
			tBOSNode.surface_s = tSentence ;
			tBOSNode.surface_o = 0 ;

			tEndNodes[ 0 ] = tBOSNode ;

			for( int tPosition  = 0 ; tPosition <  tSize ; ++ tPosition )
			{
				if( tEndNodes[ tPosition ] != null )
				{
					// オフセット操作
					Node tRNode = m_Tokenizer.Lookup( tSentence, tBegin + tPosition, tEnd /*, tLattice*/ ) ;

					tBeginNodes[ tPosition ] = tRNode ;
						
					if( Connect( tPosition, tRNode, tBeginNodes, tEndNodes, m_Connector ) == false )
					{
						Debug.LogError( "too long sentence." ) ;
						return false ;
					}
				}
			}

			Node tEOSNode = m_Tokenizer.GetEOSNode() ;

			// オフセット操作
			tEOSNode.surface_s = tSentence ;
			tEOSNode.surface_o = tSize ;

			tBeginNodes[ tSize ] = tEOSNode ;

			for( long tPosition = tSize ; tPosition >= 0 ; -- tPosition )
			{
				if( tEndNodes[ ( int )tPosition ] != null )
				{
					if( Connect( ( int )tPosition, tEOSNode, tBeginNodes, tEndNodes, m_Connector ) == false )
					{
						Debug.LogError( "too long sentence." ) ;
						return false ;
					}
					break ;
				}
			}

			tEndNodes[ 0 ] = tBOSNode ;
			tBeginNodes[ tSize ] = tEOSNode ;

			return true ;
		}

		private bool Connect( int tPosition, Node tRNode, Node[] tBeginNodes, Node[] tEndNodes, Connector tConnector )
		{
			for( ; tRNode != null ; tRNode = tRNode.bnext )
			{
				long tBestCost = 2147483647 ;
				Node tBestNode = null ;

				for( Node tLNode  = tEndNodes[ tPosition ] ; tLNode != null ; tLNode = tLNode.enext )
				{
					int tLCost = tConnector.GetCost( tLNode, tRNode ) ;  // local cost
					long tCost = tLNode.cost + tLCost ;

					if( tCost <  tBestCost )
					{
						tBestNode  = tLNode ;
						tBestCost  = tCost ;
					}
				}

				// overflow check 2003/03/09
				if( tBestNode == null )
				{
					return false ;
				}

				tRNode.prev	= tBestNode ;
				tRNode.next	= null ;
				tRNode.cost	= tBestCost ;
				int x = tRNode.rlength + tPosition ;
				tRNode.enext = tEndNodes[ x ] ;
				tEndNodes[ x ] = tRNode ;
			}
				
			return true ;
		}

		// static
		private bool BuildBestLattice( Node[] tEndNodes, int tSize )
		{
			Node tNode = tEndNodes[ tSize ] ;

			for( Node tPrevNode = null ; tNode.prev != null ; )
			{
				tNode.isbest	= 1 ;

				tPrevNode		= tNode.prev ;
				tPrevNode.next	= tNode ;

				tNode			= tPrevNode ;
			}

			return true ;
		}

	}
}
