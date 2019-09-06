using System.Collections ;
using System.Collections.Generic ;
using System.Text ;
using UnityEngine;

namespace OJT
{
	public class JPCommonLabel
	{
		public	int							size ;
		public	string[]					feature ;
		public	JPCommonLabelBreathGroup	breath_head ;
		public	JPCommonLabelBreathGroup	breath_tail ;
		public	JPCommonLabelAccentPhrase	accent_head ;
		public	JPCommonLabelAccentPhrase	accent_tail ;
		public	JPCommonLabelWord			word_head ;
		public	JPCommonLabelWord			word_tail ;
		public	JPCommonLabelMora			mora_head ;
		public	JPCommonLabelMora			mora_tail ;
		public	JPCommonLabelPhoneme		phoneme_head ;
		public	JPCommonLabelPhoneme		phoneme_tail ;
		public	int							short_pause_flag ;

		//-----------------------------------------------------------

		private const int	MAX_S			=   19 ;
		private const int	MAX_M			=   49 ;
		private const int	MAX_L			=   99 ;
		private const int	MAX_LL			=  199 ;

		private const string JPCOMMON_MORA_UNVOICE			= "’"	;
		private const string JPCOMMON_MORA_LONG_VOWEL		= "ー"	;
		private const string JPCOMMON_MORA_SHORT_PAUSE		= "、"	;
		private const string JPCOMMON_MORA_QUESTION			= "？"	;
		private const string JPCOMMON_PHONEME_SHORT_PAUSE	= "pau"	;
		private const string JPCOMMON_PHONEME_SILENT		= "sil"	;
		private const string JPCOMMON_PHONEME_UNKNOWN		= "xx"	;
		private const string JPCOMMON_FLAG_QUESTION			= "1"	;

		//-----------------------------------------------------------

		public JPCommonLabel()
		{
			Initialize() ;
		}

		public void Initialize()
		{
			this.short_pause_flag	= 0 ;
			this.breath_head		= null ;
			this.breath_tail		= null ;
			this.accent_head		= null ;
			this.accent_tail		= null ;
			this.word_head			= null ;
			this.word_tail			= null ;
			this.mora_head			= null ;
			this.mora_tail			= null ;
			this.phoneme_head		= null ;
			this.phoneme_tail		= null ;
		}

		public void Clear()
		{
			JPCommonLabelPhoneme		p, pn ;
			JPCommonLabelMora			m, mn ;
			JPCommonLabelWord			w, wn ;
			JPCommonLabelAccentPhrase	a, an ;
			JPCommonLabelBreathGroup	b, bn ;

			for( p  = this.phoneme_head ; p != null ; p = pn )
			{
				pn = p.next ;
				p.Clear() ;
			}
			
			for( m  = this.mora_head ; m != null ; m = mn )
			{
				mn = m.next ;
				m.Clear() ;
			}
			
			for( w  = this.word_head ; w != null ; w = wn )
			{
				wn = w.next ;
				w.Clear() ;
			}
			
			for( a  = this.accent_head ; a != null ; a = an )
			{
				an = a.next ;
				a.Clear() ;
			}
			
			for( b  = this.breath_head ; b != null ; b = bn )
			{
				bn = b.next ;
				b.Clear() ;
			}

			this.size		= 0 ;
			this.feature	= null ;
		}

		//-----------------------------------------------------------

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


		private void InsertPause()
		{
			// insert short pause
			if( this.short_pause_flag == 1 )
			{
				if( this.phoneme_tail != null )
				{
					if( this.phoneme_tail.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE )
					{
						Debug.LogError( "WARNING: JPCommonLabel_insert_word() in jpcommon_label.c: Short pause should not be chained." ) ;
						return ;
					}
					this.phoneme_tail.next = new JPCommonLabelPhoneme() ;
					this.phoneme_tail.next.Initialize( JPCOMMON_PHONEME_SHORT_PAUSE, this.phoneme_tail, null, null ) ;
					this.phoneme_tail = this.phoneme_tail.next ;
				}
				else
				{
					Debug.LogError( "WARNING: JPCommonLabel_insert_word() in jpcommon_label.c: First mora should not be short pause." ) ;
				}
				this.short_pause_flag = 0 ;
			}
		}

		
		public void Push( string pron, string pos, string ctype, string cform, int acc, int chain_flag )
		{
			int i ;
			int find ;
			int is_first_word = 1 ;
			
			if( pron == JPCOMMON_MORA_SHORT_PAUSE )
			{
				this.short_pause_flag = 1 ;
				return ;
			}
			
			// set emotion flag
			if( pron == JPCOMMON_MORA_QUESTION )
			{
				if( this.phoneme_tail != null )
				{
					if( this.phoneme_tail.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE )
					{
						if( this.phoneme_tail.prev.up.up.up.emotion == null )
						{
							this.phoneme_tail.prev.up.up.up.emotion = JPCOMMON_FLAG_QUESTION ;
						}
					}
					else
					{
						if( this.phoneme_tail.up.up.up.emotion == null )
						{
							this.phoneme_tail.up.up.up.emotion = JPCOMMON_FLAG_QUESTION ;
						}
					}
				}
				else
				{
					Debug.LogError( "WARNING: JPCommonLabel_push_word() in jpcommon_label.c: First mora should not be question flag." ) ;
				}
				this.short_pause_flag = 1 ;
				return ;
			}
			
			// analysis pron
			int pron_o = 0 ;
			while( pron_o <  pron.Length )
			{
				find = StrTopCmp( pron, pron_o, JPCOMMON_MORA_LONG_VOWEL ) ;
				if( find != -1 )
				{
					// for long vowel
					if( this.phoneme_tail != null && this.short_pause_flag == 0 )
					{
						this.InsertPause() ;
						this.phoneme_tail.next	= new JPCommonLabelPhoneme() ;
						this.mora_tail.next		= new JPCommonLabelMora() ;
						this.phoneme_tail.next.Initialize( this.phoneme_tail.phoneme, this.phoneme_tail, null, this.mora_tail.next ) ;
						this.mora_tail.next.Initialize( JPCOMMON_MORA_LONG_VOWEL, this.phoneme_tail.next, this.phoneme_tail.next, this.mora_tail, null, this.mora_tail.up ) ;
						this.phoneme_tail		= this.phoneme_tail.next ;
						this.mora_tail			= this.mora_tail.next ;
						this.word_tail.tail		= this.mora_tail ;
					}
					else
					{
						Debug.LogError( "WARNING: JPCommonLabel_push_word() in jpcommon_label.c: First mora should not be long vowel symbol." ) ;
					}
					pron_o += find ;
				}
				else
				{
					find = StrTopCmp( pron, pron_o, JPCOMMON_MORA_UNVOICE ) ;
					if( find != -1 )
					{
						// for unvoice
						if( this.phoneme_tail != null && is_first_word != 1 )
						{
							this.phoneme_tail.ConvertUnvoice() ;
						}
						else
						{
							Debug.LogError( "WARNING: JPCommonLabel_push_word() in jpcommon_label.c: First mora should not be unvoice flag." ) ;
						}
						pron_o += find ;
					}
					else
					{
						// for normal word
						for( i  = 0 ; jpcommon_mora_list[ i ] != null ; i += 3 )
						{
							find = StrTopCmp( pron, pron_o, jpcommon_mora_list[ i ] ) ;
							if( find != -1 )
							{
								break ;
							}
						}

						if( find != -1 )
						{
							if( this.phoneme_tail == null )
							{
								this.InsertPause() ;
								this.phoneme_tail	= new JPCommonLabelPhoneme() ;
								this.mora_tail		= new JPCommonLabelMora() ;
								this.word_tail		= new JPCommonLabelWord() ;
								this.phoneme_tail.Initialize( jpcommon_mora_list[ i + 1 ], null, null, this.mora_tail ) ;
								this.mora_tail.Initialize( jpcommon_mora_list[ i ], this.phoneme_tail, this.phoneme_tail, null, null, this.word_tail ) ;
								this.word_tail.Initialize( pron, pos, ctype, cform, this.mora_tail, this.mora_tail, null, null ) ;
								this.phoneme_head	= this.phoneme_tail ;
								this.mora_head		= this.mora_tail ;
								this.word_head		= this.word_tail ;
								is_first_word = 0 ;
							}
							else
							{
								if( is_first_word == 1 )
								{
									this.InsertPause() ;
									this.phoneme_tail.next	= new JPCommonLabelPhoneme() ;
									this.mora_tail.next		= new JPCommonLabelMora() ;
									this.word_tail.next		= new JPCommonLabelWord() ;
									this.phoneme_tail.next.Initialize( jpcommon_mora_list[ i + 1 ], this.phoneme_tail, null, this.mora_tail.next ) ;
									this.mora_tail.next.Initialize( jpcommon_mora_list[ i ], this.phoneme_tail.next, this.phoneme_tail.next, this.mora_tail, null, this.word_tail.next ) ;
									this.word_tail.next.Initialize( pron, pos, ctype, cform, this.mora_tail.next, this.mora_tail.next, this.word_tail, null ) ;
									this.phoneme_tail		= this.phoneme_tail.next ;
									this.mora_tail			= this.mora_tail.next ;
									this.word_tail			= this.word_tail.next ;
									is_first_word = 0 ;
								}
								else
								{
									this.InsertPause() ;
									this.phoneme_tail.next	= new JPCommonLabelPhoneme() ;
									this.mora_tail.next		= new JPCommonLabelMora() ;
									this.phoneme_tail.next.Initialize( jpcommon_mora_list[ i + 1 ], this.phoneme_tail, null, this.mora_tail.next ) ;
									this.mora_tail.next.Initialize( jpcommon_mora_list[ i ], this.phoneme_tail.next, this.phoneme_tail.next, this.mora_tail, null, this.mora_tail.up ) ;
									this.phoneme_tail		= this.phoneme_tail.next ;
									this.mora_tail			= this.mora_tail.next ;
									this.word_tail.tail		= this.mora_tail ;
								}
							}

							if( jpcommon_mora_list[ i + 2 ] != null )
							{
								this.InsertPause() ;
								this.phoneme_tail.next	= new JPCommonLabelPhoneme() ;
								this.phoneme_tail.next.Initialize( jpcommon_mora_list[ i + 2 ], this.phoneme_tail, null, this.mora_tail ) ;
								this.phoneme_tail		= this.phoneme_tail.next ;
								this.mora_tail.tail		= this.phoneme_tail ;
							}
							pron_o += find ;
						}
						else
						{
							Debug.LogError( "WARNING: JPCommonLabel_push_word() in jpcommon_label.c: " + pron + " is wrong mora list." ) ;
							break ;
						}
					}
				}
			}

			// check
			if( is_first_word == 1 )
			{
				return ;
			}

			if( this.phoneme_tail == null )
			{
				return ;
			}

			if( this.phoneme_tail.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE )
			{
				return ;
			}
			
			// make accent, phrase
			if( this.word_head == this.word_tail )
			{
				// first word
				this.accent_tail	= new JPCommonLabelAccentPhrase() ;
				this.breath_tail	= new JPCommonLabelBreathGroup() ;
				this.word_tail.up	= this.accent_tail ;
				this.accent_tail.Initialize( acc, null, this.word_tail, this.word_tail, null, null, this.breath_tail ) ;
				this.breath_tail.Initialize( this.accent_tail, this.accent_tail, null, null ) ;
				this.accent_head	= this.accent_tail ;
				this.breath_head	= this.breath_tail ;
			}
			else
			if( chain_flag == 1 )
			{
				// common accent phrase and common phrase
				this.word_tail.up		= this.accent_tail ;
				this.accent_tail.tail	= this.word_tail ;
			}
			else
			{
				if( this.word_tail.prev.tail.tail.next.phoneme != JPCOMMON_PHONEME_SHORT_PAUSE )
				{
					// different accent phrase && common phrase
					this.accent_tail.next	= new JPCommonLabelAccentPhrase() ;
					this.word_tail.up		= this.accent_tail.next ;
					this.accent_tail.next.Initialize( acc, null, this.word_tail, this.word_tail, this.accent_tail, null, this.breath_tail ) ;
					this.breath_tail.tail	= this.accent_tail.next ;
					this.accent_tail		= this.accent_tail.next ;
				}
				else
				{
					// different accent phrase && different phrase
					this.accent_tail.next	= new JPCommonLabelAccentPhrase() ;
					this.breath_tail.next	= new JPCommonLabelBreathGroup() ;
					this.word_tail.up		= this.accent_tail.next ;
					this.accent_tail.next.Initialize( acc, null, this.word_tail, this.word_tail, this.accent_tail, null, this.breath_tail.next ) ;
					this.breath_tail.next.Initialize( this.accent_tail.next, this.accent_tail.next, this.breath_tail, null ) ;
					this.accent_tail		= this.accent_tail.next ;
					this.breath_tail		= this.breath_tail.next ;
				}
			}
		}

		public void make()
		{
			int i, tmp1, tmp2, tmp3 ;
			string buff ;
			JPCommonLabelPhoneme		p ;
			JPCommonLabelWord			w ;
			JPCommonLabelAccentPhrase	a ;
			JPCommonLabelBreathGroup	b ;
			string[] phoneme_list ;
			int short_pause_flag ;
			
			// initialize
			for( p  = this.phoneme_head, this.size = 0 ; p != null ; p = p.next )
			{
				this.size ++ ;
			}
			
			if( this.size <  1 )
			{
				Debug.LogError( "WARNING: JPCommonLabel_make() in jcomon_label.c: No phoneme." ) ;
				return ;
			}
			
			this.size += 2 ;
			this.feature = new string[ this.size ] ;
			for( i  = 0 ; i <  this.size ; i ++ )
			{
				this.feature[ i ] = "" ;
			}
			
			// phoneme list
			phoneme_list = new string[ this.size + 4 ] ;

			phoneme_list[ 0 ]				= JPCOMMON_PHONEME_UNKNOWN ;
			phoneme_list[ 1 ]				= JPCOMMON_PHONEME_UNKNOWN ;
			phoneme_list[ 2 ]				= JPCOMMON_PHONEME_SILENT	;
			phoneme_list[ this.size + 1 ]	= JPCOMMON_PHONEME_SILENT	;
			phoneme_list[ this.size + 2 ]	= JPCOMMON_PHONEME_UNKNOWN	;
			phoneme_list[ this.size + 3 ]	= JPCOMMON_PHONEME_UNKNOWN	;
			
			for( i  = 3, p = this.phoneme_head ; p != null ; p = p.next )
			{
				phoneme_list[ i ++ ] = p.phoneme ;
			}
			
			for( i  = 0, p = this.phoneme_head ; i <  this.size ; i ++ )
			{
				if( p.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE )
				{
					short_pause_flag = 1 ;
				}
				else
				{
					short_pause_flag = 0 ;
				}
				
				// for phoneme
				this.feature[ i ] = SPrintf( "%s^%s-%s+%s=%s", phoneme_list[ i ], phoneme_list[ i + 1 ], phoneme_list[ i + 2 ], phoneme_list[ i + 3 ], phoneme_list[ i + 4 ] ) ;

				// for A:
				if( i == 0 || i == this.size - 1 || short_pause_flag == 1 )
				{
					buff = SPrintf( "/A:xx+xx+xx" ) ;
				}
				else
				{
					tmp1 = index_mora_in_accent_phrase( p.up ) ;
					tmp2 = p.up.up.up.accent == 0 ? count_mora_in_accent_phrase( p.up ) : p.up.up.up.accent ;

					buff = SPrintf( "/A:%d+%d+%d", limit( tmp1 - tmp2, - MAX_M, MAX_M ), limit( tmp1, 1, MAX_M ), limit( count_mora_in_accent_phrase( p.up ) - tmp1 + 1, 1, MAX_M ) ) ;
				}
				this.feature[ i ] += buff ;
				
				// for B:
				if( short_pause_flag == 1 )
				{
					w = p.prev.up.up ;
				}
				else
				if( p.up.up.prev == null )
				{
					w = null ;
				}
				else
				if( i == this.size - 1 )
				{
					w = p.up.up ;
				}
				else
				{
					w = p.up.up.prev ;
				}
				
				if( w == null )
				{
					buff = SPrintf( "/B:xx-xx_xx" ) ;
				}
				else
				{
					buff = SPrintf( "/B:%s-%s_%s", w.pos, w.ctype, w.cform ) ;
				}
				this.feature[ i ] += buff ;
				
				// for C:
				if( i == 0 || i == this.size - 1 || short_pause_flag != 0 )
				{
					buff = SPrintf( "/C:xx_xx+xx" ) ;
				}
				else
				{
					buff = SPrintf( "/C:%s_%s+%s", p.up.up.pos, p.up.up.ctype, p.up.up.cform ) ;
				}
				this.feature[ i ] += buff ;
				
				// for D:
				if( short_pause_flag == 1 )
				{
					w = p.next.up.up ;
				}
				else
				if( p.up.up.next == null )
				{
					w = null ;
				}
				else
				if( i == 0 )
				{
					w = p.up.up ;
				}
				else
				{
					w = p.up.up.next ;
				}
				
				if( w == null )
				{
					buff = SPrintf( "/D:xx+xx_xx" ) ;
				}
				else
				{
					buff = SPrintf( "/D:%s+%s_%s", w.pos, w.ctype, w.cform ) ;
				}
				this.feature[ i ] += buff ;

				// for E:
				if( short_pause_flag == 1 )
				{
					a = p.prev.up.up.up ;
				}
				else
				if( i == this.size - 1 )
				{
					a = p.up.up.up ;
				}
				else
				{
					a = p.up.up.up.prev ;
				}
				
				if( a == null )
				{
					buff = SPrintf( "/E:xx_xx!xx_xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"/E:%d_%d!%s_xx",
						limit( count_mora_in_accent_phrase( a.head.head ), 1, MAX_M ),
						limit( a.accent == 0 ? count_mora_in_accent_phrase( a.head.head ) : a.accent, 1, MAX_M ),
						a.emotion == null ? "0" : a.emotion
					) ;
				}
				this.feature[ i ] += buff ;

				if( i == 0 || i == this.size - 1 || short_pause_flag == 1 || a == null )
				{
					buff = SPrintf( "-xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"-%d",
						a.tail.tail.tail.next.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE ? 0 : 1
					) ;
				}
				this.feature[ i ] += buff ;

				// for F:
				if( i == 0 || i == this.size - 1 || short_pause_flag == 1 )
				{
					a = null ;
				}
				else
				{
					a = p.up.up.up ;
				}
				if( a == null )
				{
					buff = SPrintf( "/F:xx_xx#xx_xx@xx_xx|xx_xx" ) ;
				}
				else
				{
					tmp1 = index_accent_phrase_in_breath_group( a ) ;
					tmp2 = index_mora_in_breath_group( a.head.head ) ;

					buff = SPrintf
					(
						"/F:%d_%d#%s_xx@%d_%d|%d_%d",
						limit( count_mora_in_accent_phrase( a.head.head ), 1, MAX_M ),
						limit( a.accent == 0 ? count_mora_in_accent_phrase( a.head.head) : a.accent, 1, MAX_M ),
						a.emotion == null ? "0" : a.emotion,
						limit( tmp1, 1, MAX_M ),
						limit( count_accent_phrase_in_breath_group( a ) - tmp1 + 1, 1, MAX_M ),
						limit( tmp2, 1, MAX_L ),
						limit( count_mora_in_breath_group( a.head.head ) - tmp2 + 1, 1, MAX_L )
					) ;
				}
				this.feature[ i ] += buff ;

				// for G:
				if( short_pause_flag == 1 )
				{
					a = p.next.up.up.up ;
				}
				else
				if( i == 0 )
				{
					a = p.up.up.up ;
				}
				else
				{
					a = p.up.up.up.next ;
				}
				
				if( a == null )
				{
					buff = SPrintf( "/G:xx_xx%%xx_xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"/G:%d_%d%%%s_xx",
						limit( count_mora_in_accent_phrase( a.head.head ), 1, MAX_M ),
						limit( a.accent == 0 ? count_mora_in_accent_phrase( a.head.head ) : a.accent, 1, MAX_M ),
						a.emotion == null ? "0" : a.emotion
					) ;
				}
				this.feature[ i ] += buff ;

				if( i == 0 || i == this.size - 1 || short_pause_flag == 1 || a == null )
				{
					buff = SPrintf( "_xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"_%d",
						a.head.head.head.prev.phoneme == JPCOMMON_PHONEME_SHORT_PAUSE ? 0 : 1
					) ;
				}
				this.feature[ i ] += buff ;
				
				// for H:
				if( short_pause_flag == 1 )
				{
					b = p.prev.up.up.up.up ;
				}
				else
				if( i == this.size - 1 )
				{
					b = p.up.up.up.up ;
				}
				else
				{
					b = p.up.up.up.up.prev ;
				}

				if( b == null )
				{
					buff = SPrintf( "/H:xx_xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"/H:%d_%d",
						limit( count_accent_phrase_in_breath_group( b.head ), 1, MAX_M ),
						limit( count_mora_in_breath_group( b.head.head.head ), 1, MAX_L )
					) ;
				}
				this.feature[ i ] += buff ;
				
				// for I:
				if( i == 0 || i == this.size - 1 || short_pause_flag == 1 )
				{
					b = null ;
				}
				else
				{
					b = p.up.up.up.up ;
				}

				if( b == null )
				{
					buff = SPrintf( "/I:xx-xx@xx+xx&xx-xx|xx+xx" ) ;
				}
				else
				{
					tmp1 = index_breath_group_in_utterance( b ) ;
					tmp2 = index_accent_phrase_in_utterance( b.head ) ;
					tmp3 = index_mora_in_utterance( b.head.head.head ) ;
					
					buff = SPrintf
					(
						"/I:%d-%d@%d+%d&%d-%d|%d+%d",
						limit( count_accent_phrase_in_breath_group( b.head ), 1, MAX_M ),
						limit( count_mora_in_breath_group( b.head.head.head ), 1, MAX_L ),
						limit( tmp1, 1, MAX_S ),
						limit( count_breath_group_in_utterance( b ) - tmp1 + 1, 1, MAX_S ),
						limit( tmp2, 1, MAX_M ),
						limit( count_accent_phrase_in_utterance( b.head ) - tmp2 + 1, 1, MAX_M ),
						limit( tmp3, 1, MAX_LL ),
						limit( count_mora_in_utterance( b.head.head.head ) - tmp3 + 1, 1, MAX_LL )
					) ;
				}
				this.feature[ i ] += buff ;

				// for J:
				if( short_pause_flag == 1 )
				{
					b = p.next.up.up.up.up ;
				}
				else
				if( i == 0 )
				{
					b = p.up.up.up.up ;
				}
				else
				{
					b = p.up.up.up.up.next ;
				}
				
				if( b == null )
				{
					buff = SPrintf( "/J:xx_xx" ) ;
				}
				else
				{
					buff = SPrintf
					(
						"/J:%d_%d",
						limit( count_accent_phrase_in_breath_group( b.head ), 1, MAX_M ),
						limit( count_mora_in_breath_group( b.head.head.head ), 1, MAX_L )
					) ;
				}
				this.feature[ i ] += buff ;

				// for K:
				buff = SPrintf
				(
					"/K:%d+%d-%d",
					limit( count_breath_group_in_utterance( this.breath_head ), 1, MAX_S ),
					limit( count_accent_phrase_in_utterance( this.accent_head ), 1, MAX_M ),
					limit( count_mora_in_utterance( this.mora_head ), 1, MAX_LL )
				) ;
				this.feature[ i ] += buff ;
				
				if( 0 <  i && i <  this.size - 2 )
				{
					p = p.next ;
				}

	//			Debug.LogWarning( "[" + i + "] : " + BytesToString( this.feature[ i ] ) ) ;
			}
			
			// free
			phoneme_list = null ;
		}

		private int limit( int in_, int min, int max )
		{
			if( in_ <= min )
			{
				return min ;
			}
			if( in_ >= max )
			{
				return max ;
			}
			return in_ ;
		}

		private int index_mora_in_accent_phrase( JPCommonLabelMora m )
		{
			int i;
			JPCommonLabelMora index ;
			
			for( i  = 0, index = m.up.up.head.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == m )
				{
					break ;
				}
			}
			return i ;
		}

		private int count_mora_in_accent_phrase( JPCommonLabelMora m )
		{
			int i ;
			JPCommonLabelMora index ;
			
			for( i  = 0, index  = m.up.up.head.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == m.up.up.tail.tail )
				{
					break ;
				}
			}
			return i ;
		}
		
		private int index_accent_phrase_in_breath_group( JPCommonLabelAccentPhrase a )
		{
			int i ;
			JPCommonLabelAccentPhrase index ;
			
			for( i  = 0, index  = a.up.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == a )
				{
					break ;
				}
			}
			return i ;
		}
		
		private int count_accent_phrase_in_breath_group( JPCommonLabelAccentPhrase a )
		{
			int i ;
			JPCommonLabelAccentPhrase index ;
			
			for( i  = 0, index  = a.up.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == a.up.tail )
				{
					break ;
				}
			}
			return i ;
		}
		
		private int index_mora_in_breath_group( JPCommonLabelMora m )
		{
			int i ;
			JPCommonLabelMora index ;
			
			for( i  = 0, index  = m.up.up.up.head.head.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == m )
				{
					break ;
				}
			}
			return i ;
		}
		
		private int count_mora_in_breath_group( JPCommonLabelMora m )
		{
			int i ;
			JPCommonLabelMora index ;
			
			for( i  = 0, index  = m.up.up.up.head.head.head ; index != null ; index = index.next )
			{
				i ++ ;
				if( index == m.up.up.up.tail.tail.tail )
				{
					break ;
				}
			}
			return i ;
		}
		
		private int index_breath_group_in_utterance( JPCommonLabelBreathGroup b )
		{
			int i ;
			JPCommonLabelBreathGroup  index ;
			
			for( i  = 0, index  = b ; index != null ; index = index.prev )
			{
				i ++ ;
			}
			return i ;
		}
		
		private int count_breath_group_in_utterance( JPCommonLabelBreathGroup b )
		{
			int i ;
			JPCommonLabelBreathGroup index ;
			
			for( i  = 0, index = b.next ; index != null ; index = index.next )
			{
				i ++ ;
			}
			return index_breath_group_in_utterance( b ) + i ;
		}
		
		private int index_accent_phrase_in_utterance( JPCommonLabelAccentPhrase a )
		{
			int i ;
			JPCommonLabelAccentPhrase index ;
			
			for( i  = 0, index  = a ; index != null ; index = index.prev )
			{
				i ++ ;
			}
			return i ;
		}
		
		private int count_accent_phrase_in_utterance( JPCommonLabelAccentPhrase a )
		{
			int i ;
			JPCommonLabelAccentPhrase index ;
			
			for( i  = 0, index  = a.next ; index != null ; index = index.next )
			{
				i ++ ;
			}
			return index_accent_phrase_in_utterance( a ) + i ;
		}
		
		private int index_mora_in_utterance( JPCommonLabelMora m )
		{
			int i ;
			JPCommonLabelMora index ;
			
			for( i  = 0, index  = m ; index != null ; index = index.prev )
			{
				i ++ ;
			}
			return i ;
		}
		
		private int count_mora_in_utterance( JPCommonLabelMora m )
		{
			int i ;
			JPCommonLabelMora index ;
			
			for( i  = 0, index  = m.next ; index != null ; index = index.next )
			{
				i ++ ;
			}
			return index_mora_in_utterance( m ) + i ;
		}

		private string SPrintf( string tFormat, params object[] tData )
		{
			int i, l = tFormat.Length ;

			string s = "" ;
			int o = 0 ;
			bool f = false ;
			char c ;
			int p = 0 ;
			object v ;
			bool a ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( f == false )
				{
					// スイッチが入っていない
					if( tFormat[ i ] == '%' )
					{
						f = true ;	// スイッチオン

						if( i >  o )
						{
							// ここまでをコピーする
							s = s + tFormat.Substring( o, i - o ) ;
							o = i + 1 ;	// 次の位置へ
						}
					}
				}
				else
				{
					// スイッチが入っている
					if( tFormat[ i ] == '%' )
					{
						// % 記号表示
						s = s + "%" ;
						o = i + 1 ;	// 次の位置へ
					}
					else
					{
						c = tFormat[ i ] ;
						if( c == 'd' )
						{
							// 整数値
							a = true ;
							if( tData != null && p <  tData.Length )
							{
								v = tData[ p ] ;
								if( v is byte	){ s = s + ( byte	)v ; }else
								if( v is char	){ s = s + ( char	)v ; }else
								if( v is short	){ s = s + ( short	)v ; }else
								if( v is ushort	){ s = s + ( ushort	)v ; }else
								if( v is int	){ s = s + ( int	)v ; }else
								if( v is uint	){ s = s + ( uint	)v ; }else
								if( v is long	){ s = s + ( long	)v ; }else
								if( v is ulong	){ s = s + ( ulong	)v ; }else
								if( v is float	){ s = s + ( int	)v ; }else
								if( v is double	){ s = s + ( long	)v ; }else
								{
									a = false ;
								}

								p ++ ;
							}
							else
							{
								a = false ;
							}
							
							if( a == false )
							{
								s = s + "%d" ;
							}
						}
						else
						if( c == 'f' )
						{
							// 小数値
							a = true ;
							if( tData != null && p <  tData.Length )
							{
								v = tData[ p ] ;
								if( v is byte	){ s = s + ( byte	)v + ".0" ; }else
								if( v is char	){ s = s + ( char	)v + ".0" ; }else
								if( v is short	){ s = s + ( short	)v + ".0" ; }else
								if( v is ushort	){ s = s + ( ushort	)v + ".0" ; }else
								if( v is int	){ s = s + ( int	)v + ".0" ; }else
								if( v is uint	){ s = s + ( uint	)v + ".0" ; }else
								if( v is long	){ s = s + ( long	)v + ".0" ; }else
								if( v is ulong	){ s = s + ( ulong	)v + ".0" ; }else
								if( v is float	){ s = s + ( float	)v ; }else
								if( v is double	){ s = s + ( double	)v ; }else
								{
									a = false ;
								}

								p ++ ;
							}
							else
							{
								a = false ;
							}
							
							if( a == false )
							{
								s = s + "%f" ;
							}
						}
						else
						if( c == 's' )
						{
							// 文字列
							a = true ;
							if( tData != null && p <  tData.Length )
							{
								v = tData[ p ] ;
								if( v is char[]	){ s = s + new string( ( char[]	)v ) ; }else
								if( v is string	){ s = s + ( string	)v ; }else
								{
									a = false ;
								}

								p ++ ;
							}
							else
							{
								a = false ;
							}
							
							if( a == false )
							{
								s = s + "%s" ;
							}
						}

						o = i + 1 ;	// 次の位置へ
					}

					f = false ;	// スイッチオフ
				}
			}
			
			if( i >  o )
			{
				// 最後をコピーする
				s = s + tFormat.Substring( o, i - o ) ;
			}

			return s ;
		}

		//---------------------------------------------------------------------------

		public int GetSize()
		{
		   return this.size ;
		}

		public string[] GetFeature()
		{
			return this.feature ;
		}

		//---------------------------------------------------------------------------

		private static string[] jpcommon_mora_list =
		{
			"ヴョ", "by", "o",
			"ヴュ", "by", "u",
			"ヴャ", "by", "a",
			"ヴォ", "v", "o",
			"ヴェ", "v", "e",
			"ヴィ", "v", "i",
			"ヴァ", "v", "a",
			"ヴ", "v", "u",
			"ン", "N", null,
			"ヲ", "o", null,
			"ヱ", "e", null,
			"ヰ", "i", null,
			"ワ", "w", "a",
			"ヮ", "w", "a",
			"ロ", "r", "o",
			"レ", "r", "e",
			"ル", "r", "u",
			"リョ", "ry", "o",
			"リュ", "ry", "u",
			"リャ", "ry", "a",
			"リェ", "ry", "e",
			"リ", "r", "i",
			"ラ", "r", "a",
			"ヨ", "y", "o",
			"ョ", "y", "o",
			"ユ", "y", "u",
			"ュ", "y", "u",
			"ヤ", "y", "a",
			"ャ", "y", "a",
			"モ", "m", "o",
			"メ", "m", "e",
			"ム", "m", "u",
			"ミョ", "my", "o",
			"ミュ", "my", "u",
			"ミャ", "my", "a",
			"ミェ", "my", "e",
			"ミ", "m", "i",
			"マ", "m", "a",
			"ポ", "p", "o",
			"ボ", "b", "o",
			"ホ", "h", "o",
			"ペ", "p", "e",
			"ベ", "b", "e",
			"ヘ", "h", "e",
			"プ", "p", "u",
			"ブ", "b", "u",
			"フォ", "f", "o",
			"フェ", "f", "e",
			"フィ", "f", "i",
			"ファ", "f", "a",
			"フ", "f", "u",
			"ピョ", "py", "o",
			"ピュ", "py", "u",
			"ピャ", "py", "a",
			"ピェ", "py", "e",
			"ピ", "p", "i",
			"ビョ", "by", "o",
			"ビュ", "by", "u",
			"ビャ", "by", "a",
			"ビェ", "by", "e",
			"ビ", "b", "i",
			"ヒョ", "hy", "o",
			"ヒュ", "hy", "u",
			"ヒャ", "hy", "a",
			"ヒェ", "hy", "e",
			"ヒ", "h", "i",
			"パ", "p", "a",
			"バ", "b", "a",
			"ハ", "h", "a",
			"ノ", "n", "o",
			"ネ", "n", "e",
			"ヌ", "n", "u",
			"ニョ", "ny", "o",
			"ニュ", "ny", "u",
			"ニャ", "ny", "a",
			"ニェ", "ny", "e",
			"ニ", "n", "i",
			"ナ", "n", "a",
			"ドゥ", "d", "u",
			"ド", "d", "o",
			"トゥ", "t", "u",
			"ト", "t", "o",
			"デョ", "dy", "o",
			"デュ", "dy", "u",
			"デャ", "dy", "a",
			"ディ", "d", "i",
			"デ", "d", "e",
			"テョ", "ty", "o",
			"テュ", "ty", "u",
			"テャ", "ty", "a",
			"ティ", "t", "i",
			"テ", "t", "e",
			"ヅ", "z", "u",
			"ツォ", "ts", "o",
			"ツェ", "ts", "e",
			"ツィ", "ts", "i",
			"ツァ", "ts", "a",
			"ツ", "ts", "u",
			"ッ", "cl", null,
			"ヂ", "j", "i",
			"チョ", "ch", "o",
			"チュ", "ch", "u",
			"チャ", "ch", "a",
			"チェ", "ch", "e",
			"チ", "ch", "i",
			"ダ", "d", "a",
			"タ", "t", "a",
			"ゾ", "z", "o",
			"ソ", "s", "o",
			"ゼ", "z", "e",
			"セ", "s", "e",
			"ズィ", "z", "i",
			"ズ", "z", "u",
			"スィ", "s", "i",
			"ス", "s", "u",
			"ジョ", "j", "o",
			"ジュ", "j", "u",
			"ジャ", "j", "a",
			"ジェ", "j", "e",
			"ジ", "j", "i",
			"ショ", "sh", "o",
			"シュ", "sh", "u",
			"シャ", "sh", "a",
			"シェ", "sh", "e",
			"シ", "sh", "i",
			"ザ", "z", "a",
			"サ", "s", "a",
			"ゴ", "g", "o",
			"コ", "k", "o",
			"ゲ", "g", "e",
			"ケ", "k", "e",
			"ヶ", "k", "e",
			"グヮ", "gw", "a",
			"グ", "g", "u",
			"クヮ", "kw", "a",
			"ク", "k", "u",
			"ギョ", "gy", "o",
			"ギュ", "gy", "u",
			"ギャ", "gy", "a",
			"ギェ", "gy", "e",
			"ギ", "g", "i",
			"キョ", "ky", "o",
			"キュ", "ky", "u",
			"キャ", "ky", "a",
			"キェ", "ky", "e",
			"キ", "k", "i",
			"ガ", "g", "a",
			"カ", "k", "a",
			"オ", "o", null,
			"ォ", "o", null,
			"エ", "e", null,
			"ェ", "e", null,
			"ウォ", "w", "o",
			"ウェ", "w", "e",
			"ウィ", "w", "i",
			"ウ", "u", null,
			"ゥ", "u", null,
			"イェ", "y", "e",
			"イ", "i", null,
			"ィ", "i", null,
			"ア", "a", null,
			"ァ", "a", null,
			null, null, null
		} ;

	}
}
