using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Question
	{
		private	string			m_Word ;		// name of this question
		private	HTS_Pattern		m_Head ;					// pointer to the head of pattern list
		public	HTS_Question	next ;					// pointer to the next question

		//-----------------------------------------------------------

		public HTS_Question()
		{
			Initialize() ;
		}

		private void Initialize()
		{
			m_Word	= null ;
			m_Head	= null ;
			next	= null ;
		}
		
		// HTS_Question_load: Load questions from file
		public bool Load( HTS_File fp )
		{
			if( fp == null )
			{
				return false ;
			}
			
			string tToken ;

			HTS_Pattern pattern, last_pattern ;
			
			Initialize() ;

			// get question name
			m_Word = fp.GetPatternToken() ;

			if( string.IsNullOrEmpty( m_Word ) == true )
			{
				return false ;
			}
			
			// get pattern list
			tToken = fp.GetPatternToken() ;
			if( string.IsNullOrEmpty( tToken ) == true )
			{
				Initialize() ;
				return false ;
			}
			
			last_pattern = null ;
			if( tToken == "{" )
			{
				while( true )
				{
					tToken = fp.GetPatternToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						Initialize() ;
						return false ;
					}

					pattern = new HTS_Pattern() ;
					if( m_Head != null )
					{
						last_pattern.next = pattern ;
					}
					else
					{
						// first time
						m_Head = pattern ;
					}

					pattern.word = tToken ;
					pattern.next = null ;

					tToken = fp.GetPatternToken() ;
					if( string.IsNullOrEmpty( tToken ) == true )
					{
						Initialize() ;
						return false ;
					}

					if( tToken == "}" )
					{
						break ;
					}

					last_pattern = pattern ;
				}
			}

			return true ;
		}

		// HTS_Question_find: find question from question list
		public static HTS_Question Find( HTS_Question question, string tString )
		{
			for( ; question != null ; question = question.next )
			{
				if( tString == question.m_Word )
				{
					return question ;
				}
			}

			return null ;
		}

		// HTS_Question_match: check given string match given question
		public bool Match( string tString )
		{
			HTS_Pattern tPattern ;
			
			for( tPattern = m_Head ; tPattern != null ; tPattern = tPattern.next )
			{
				if( HTS_Misc.PatternMatch( tString, tPattern.word ) == true )
				{
					return true ;
				}
			}
			
			return false ;
		}
	}
}
