using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace MecabForOpenJTalk.Classes
{
	// N = Node P = Path
	public class Tokenizer : Common
	{	
		private	WordDictionary						m_UNKDictionary ;
		private List<WordDictionary>				m_Dictionaries ;

		private CharProperty						m_CharProperty ;

		private List<KeyValuePair<Token,int>>		m_UNKTokens ;


		private	byte[]								m_BOSFeature ;

		private uint								m_LSize = 0	;	// 最後に追加された辞書のもの
		private uint								m_RSize = 0 ;	// 最後に追加された辞書のもの

		private	CharInfo							m_Space ;

//		private	int									m_MaxGroupingSize ;

		private const int							kResultsSize = 512 ;

		private CharsetCode							m_CharsetCode ;
		public  CharsetCode GetCharsetCode()
		{
			return m_CharsetCode ;
		}

		//---------------------------------------------------------------------------

		public bool Open( string tDirectory )
		{
			Close() ;

			// UNKDictionary Open
			m_UNKDictionary = new WordDictionary() ;
			if( m_UNKDictionary.Open( Path.Combine( tDirectory, UNK_DIC_FILE ).Replace( "\\", "/" ) ) == false )
			{
				return false ;
			}

			//----------------------------------
			// SystemDictionary Open

			m_Dictionaries		= new List<WordDictionary>() ;

			WordDictionary tSystemDictionary = new WordDictionary() ;
			if( tSystemDictionary.Open( Path.Combine( tDirectory, SYS_DIC_FILE ).Replace( "\\", "/" ) ) == false )
			{
				return false ;
			}

			if( tSystemDictionary.Type != 0 )
			{
				return false ;
			}

			// 文字コード文字列から文字コード識別値を取得する
			m_CharsetCode = GetCharsetCode( tSystemDictionary.Charset ) ;

			// 辞書リストに追加する
			m_Dictionaries.Add( tSystemDictionary ) ;

			//----------------------------------

			// CharProperty Open
			m_CharProperty = new CharProperty( m_CharsetCode ) ;
			if( m_CharProperty.Open( tDirectory ) == false )
			{
				return false ;
			}

			//----------------------------------

			int tLast = m_Dictionaries.Count - 1 ;
			m_LSize	= ( uint )m_Dictionaries[ tLast ].LSize ;
			m_RSize	= ( uint )m_Dictionaries[ tLast ].RSize ;

			//----------------------------------------------------------
			// UNKToken Open

			m_UNKTokens  = new List<KeyValuePair<Token, int>>() ;

			for( int i  = 0 ; i < m_CharProperty.Size ; ++ i )
			{
				byte[] tKey = m_CharProperty.GetName( i ) ;

				DoubleArray.Word n = m_UNKDictionary.ExactMatchSearch( tKey ) ;

				if( n.value == -1 )
				{
					Debug.LogWarning( "cannot find UNK category: " + tKey ) ;
					return false ;
				}

				Token	tToken	= m_UNKDictionary.GetToken( n ) ;
				int		tSize	= m_UNKDictionary.GetSize( n ) ;

				m_UNKTokens.Add( new KeyValuePair<Token, int>( tToken, tSize ) ) ;
			}

			//----------------------------------------------------------

			m_Space = m_CharProperty.GetCharInfo( 0x20 ) ;  // ad-hoc

			m_BOSFeature = StringToBytes( m_CharsetCode, BOS_FEATURE ) ;
//			m_MaxGroupingSize = DEFAULT_MAX_GROUPING_SIZE ;

			return true ;
		}

		public void Close()
		{
			m_UNKTokens = null ;

			m_CharProperty = null ;

			m_Dictionaries = null ;
			m_UNKDictionary = null ;
		}

		//---------------------------------------------------------------------------

		public uint LSize
		{
			get
			{
				return m_LSize ;
			}
		}

		public uint RSize
		{
			get
			{
				return m_RSize ;
			}
		}


		public Node GetBOSNode()
		{
			Node tBOSNode = new Node() ;

			tBOSNode.surface_s = StringToBytes( m_CharsetCode, BOS_KEY ) ;  // dummy
			tBOSNode.surface_o = 0 ;
			tBOSNode.feature_s = m_BOSFeature ;
			tBOSNode.feature_o = 0 ;

			tBOSNode.isbest = 1 ;
			tBOSNode.stat = MECAB_BOS_NODE ;

			return tBOSNode ;
		}

		public Node GetEOSNode()
		{
			Node tEOSNode = GetBOSNode() ;  // same

			tEOSNode.stat = MECAB_EOS_NODE ;

			return tEOSNode ;
		}

		// オフセット操作
		public Node Lookup( byte[] tSentence, int tBegin, int tEnd )
		{
			CharInfo cinfo = new CharInfo() ;
			Node result_node = null ;
			int mblen = 0 ;
			int clen = 0 ;

			// 最大 65536 バイトに制限する
			tEnd = ( tEnd - tBegin ) >= 65535 ? tBegin + 65535 : tEnd ;

			// オフセット操作
			int begin2 = m_CharProperty.SeekToOtherType( tSentence, tBegin, tEnd, m_Space, ref cinfo, ref mblen, ref clen ) ;

			DoubleArray.Word[] daresults = new DoubleArray.Word[ kResultsSize ] ;
			for( int i  = 0 ; i <  daresults.Length ; i ++ )
			{
				daresults[ i ] = new DoubleArray.Word() ;
			}

			int results_size = kResultsSize ;

			for( int it_p = 0 ; it_p <  m_Dictionaries.Count ; ++ it_p )
			{
				WordDictionary it = m_Dictionaries[ it_p ] ;

				int n = it.CommonPrefixSearch( tSentence, begin2, tEnd - begin2, daresults, results_size ) ;

				for( int i  = 0 ; i <  n ; ++ i )
				{
					Token	tToken	= it.GetToken( daresults[ i ] ) ;
					int		tSize	= it.GetSize( daresults[ i ] ) ;
						
					for( int j  = 0 ; j <  tSize ; ++ j )
					{
						Node tNewNode = new Node() ;

						ReadNodeInfo( it, it.GetRelativeToken( tToken, j ), ref tNewNode ) ;

						tNewNode.length		= ( ushort )daresults[ i ].length ;
						tNewNode.rlength	= ( ushort )( begin2 - tBegin + tNewNode.length ) ;

						// オフセット操作
						tNewNode.surface_s	= tSentence ;
						tNewNode.surface_o	= begin2 ;


						tNewNode.stat		= MECAB_NOR_NODE ;
						tNewNode.char_type	= ( byte )cinfo.DefaultType ;

						tNewNode.bnext = result_node ;
						result_node = tNewNode ;
					}
				}
			}

			if( result_node != null && cinfo.Invoke == 0 )
			{
				return result_node ;
			}

			// オフセット操作
			int begin3 = begin2 + mblen ;

			// オフセット操作
			int group_begin3 = 0 ;

			if( begin3 >  tEnd )
			{
				//--------------------------------------------------------
				// ADDUNKNWON ;
				ADDUNKNWON( cinfo, tSentence, tBegin, begin2, begin3, ref result_node ) ;

				//--------------------------------------------------------

				if( result_node != null )
				{
					return result_node ;
				}
			}

			if( cinfo.Group != 0 )
			{
				int tmp = begin3 ;

				CharInfo fail = new CharInfo() ;
				begin3 = m_CharProperty.SeekToOtherType( tSentence, begin3, tEnd, cinfo, ref fail, ref mblen, ref clen ) ;
				if( clen <= DEFAULT_MAX_GROUPING_SIZE )
				{
//						ADDUNKNWON ;
					ADDUNKNWON( cinfo, tSentence, tBegin, begin2, begin3, ref result_node ) ;
				}
				group_begin3 = begin3 ;
				begin3 = tmp ;
			}

			for( int i  = 1 ; i <= cinfo.Length ; ++ i )
			{
				if( begin3 >  tEnd )
				{
					break ;
				}

				if( begin3 == group_begin3 )
				{
					continue ;
				}

				clen = i ;

//					ADDUNKNWON ;
				ADDUNKNWON( cinfo, tSentence, tBegin, begin2, begin3, ref result_node ) ;

				if( cinfo.isKindOf( m_CharProperty.GetCharInfo( tSentence, begin3, tEnd, ref mblen ) ) == false )
				{
					break ;
				}

				begin3 += mblen ;
			}

			if( result_node == null )
			{
//					ADDUNKNWON ;
				ADDUNKNWON( cinfo, tSentence, tBegin, begin2, begin3, ref result_node ) ;
			}

			return result_node ;
		}

		// マクロをメソッド化
		private Node ADDUNKNWON( CharInfo cinfo, byte[] tSentence, int begin1, int begin2, int begin3, ref Node rResultNode )
		{
			Token	tToken	= m_UNKTokens[ cinfo.DefaultType ].Key ;
			int		tSize	= m_UNKTokens[ cinfo.DefaultType ].Value ;

			for( int k  = 0 ; k <  tSize ; ++ k )
			{
				Node tNewNode = new Node() ;

				ReadNodeInfo( m_UNKDictionary, m_UNKDictionary.GetRelativeToken( tToken, k ), ref tNewNode ) ;

				tNewNode.char_type	= ( byte )cinfo.DefaultType ;
				tNewNode.surface_s	= tSentence ;
				tNewNode.surface_o	= begin2 ;
				tNewNode.length		= ( ushort )( begin3 - begin2 ) ;
				tNewNode.rlength	= ( ushort )( begin3 - begin1 ) ;
				tNewNode.stat		= MECAB_UNK_NODE ;
				tNewNode.bnext		= rResultNode ;

				rResultNode = tNewNode ;
			}

			return rResultNode ;
		}

		//----------------------------------------------------------

		private void ReadNodeInfo( WordDictionary tDictionary, Token tToken, ref Node tNode )
		{
			tNode.lcAttr	= tToken.lcAttr ;
			tNode.rcAttr	= tToken.rcAttr;
//			tNode.posid		= tToken.posid ;
			tNode.wcost		= tToken.wcost ;
			tNode.feature_s	= tDictionary.GetFeature( tToken, out tNode.feature_o ) ;

//			Debug.LogWarning( "feature_s設定[3]:" + tNode.feature_o ) ;
		}

		//-----------------------------------------------------------
	}
}