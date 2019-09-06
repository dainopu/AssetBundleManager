using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

namespace HTS_Engine_API
{
	public class HTS_Model
	{
		public int				vector_length ;			// vector length (static features only)
		public int				num_windows ;			// # of windows for delta
		public bool				is_msd ;				// flag for MSD

		//-----------------------------------

		private int				ntree ;					// # of trees
		private int[]			npdf ;					// # of PDFs at each tree
		private int				npdf_o ;
		private float[][][]		pdf ;					// PDFs
		private int				pdf_o ;					// オフセットのズレ
		private int[]			pdf_o_o ;				// オフセットのズレ
		 
		private HTS_Tree		tree ;					// pointer to the list of trees
		private HTS_Question	question ;				// pointer to the list of questions

		//-----------------------------------------------------------

		public HTS_Model()
		{
			Initialize() ;
		}

		private void Initialize()
		{
			this.vector_length	= 0 ;
			this.num_windows	= 0 ;
			this.is_msd			= false ;

			this.ntree			= 0 ;
			this.npdf			= null ;
			this.pdf			= null ;
			this.tree			= null ;
			this.question		= null ;
		}

		// HTS_Model_load: load pdf and tree
		public bool Load( HTS_File pdf, HTS_File tree, int vector_length, int num_windows, bool is_msd )
		{
			// check
			if( pdf == null || vector_length == 0 || num_windows == 0 )
			{
				return false ;
			}
			
			// reset
			Initialize() ;
			
			// load tree
			if( LoadTree( tree ) != true )
			{
				Initialize() ;
				return false ;
			}
			
			// load pdf
			if( LoadPdf( pdf, vector_length, num_windows, is_msd ) != true )
			{
				Initialize() ;
				return false ;
			}
			
			return true ;
		}

		// HTS_Model_load_tree: load trees
		private bool LoadTree( HTS_File fp )
		{
			string tToken ;

			HTS_Question question, last_question ;
			HTS_Tree tree, last_tree ;
			int state ;
			
			// check
			if( fp == null )
			{
				ntree = 1 ;
				return true ;
			}
			
			ntree			= 0 ;
			last_question	= null ;
			last_tree		= null ;

			while( fp.Eof() == false )
			{
				tToken = fp.GetPatternToken() ;

				// parse questions
				if( tToken == "QS" )
				{
					question = new HTS_Question() ;

					if( question.Load( fp ) == false )
					{
						Debug.LogError( "Question Load falied" ) ;
						question = null ;
						Initialize() ;
						return false ;
					}

					if( this.question != null )
					{
						last_question.next = question ;
					}
					else
					{
						this.question = question ;
					}

					question.next = null ;
					last_question = question ;
				}
				else
				{
					// tToken が "QE" の場合に更新される事は無いので排他で良い
					// parse trees
					state = GetStateNum( tToken ) ;
					if( state != 0 )
					{
						tree = new HTS_Tree() ;
						tree.state = state ;
						tree.ParsePattern( tToken ) ;
					
						if( tree.Load( fp, this.question ) == false )
						{
							Debug.LogError( "Tree Load falied" ) ;
							tree = null ;
							Initialize() ;
							return false ;
						}

						if( this.tree != null )
						{
							last_tree.next = tree ;
						}
						else
						{
							this.tree = tree ;
						}

						tree.next = null ;
						last_tree = tree ;
						this.ntree ++ ;
					}
					else
					{
						Debug.LogWarning( "Bad:" + tToken ) ;
					}
				}
			}

			// No Tree information in tree file
			if( this.tree == null )
			{
				this.ntree = 1 ;
			}
			
			return true ;
		}

		// HTS_Model_load_pdf: load pdfs
		private bool LoadPdf( HTS_File fp, int vector_length, int num_windows, bool is_msd )
		{
			int[] i = { 0 } ;
			int	j, k ;
			bool result = true ;
			int len ;
			
			// check
			if( fp == null || this.ntree <= 0 )
			{
				Debug.LogError( "HTS_Model_load_pdf: File for pdfs is not specified." ) ;
				return false ;
			}
			
			// read MSD flag
			this.vector_length	= vector_length ;
			this.num_windows	= num_windows ;
			this.is_msd			= is_msd ;
			this.npdf			= new int[ this.ntree ] ;
//			this.npdf			-= 2 ;	// オフセットのズレが生じる模様
			this.npdf_o			= -2 ;
			
			// read the number of pdfs
			for( j  = 2 ; j <= this.ntree + 1 ; j ++ )
			{
				if( fp.ReadLittleEndian( i, sizeof( int ), 1 ) != 1 )
				{
					result = false ;
					break ;
				}
//				this.npdf[ j ] = i ;

				this.npdf[ this.npdf_o + j ] = i[ 0 ] ;	// オフセットのズレが生じる模様
			}
			
			for( j  = 2 ; j <= this.ntree + 1 ; j ++ )
			{
//				if( this.npdf[ j ] <= 0 )
				if( this.npdf[ this.npdf_o + j ] <= 0 )		// オフセットのズレが生じる模様
				{
					Debug.LogError( "HTS_Model_load_pdf: # of pdfs at " + j + "-th state should be positive." ) ;
					result = false ;
					break ;
				}
			}

			if( result == false )
			{
//				this.npdf += 2 ;	// オフセットのズレを元に戻している
				this.npdf = null ;
				Initialize() ;
				return false ;
			}

			this.pdf = new float[ this.ntree ][][] ;
//			this.pdf -= 2 ;			// オフセットのズレが生じる模様
			this.pdf_o = -2 ;
			this.pdf_o_o = new int[ this.ntree ] ;
			
			// read means and variances
			if( is_msd == true )
			{
				// for MSD
				len = this.vector_length * this.num_windows * 2 + 1 ;
			}
			else
			{
				len = this.vector_length * this.num_windows * 2 ;
			}

			for( j  = 2 ; j <= this.ntree + 1 ; j ++ )
			{
//				this.pdf[ j ] = new float[ this.npdf[ j ] ][] ;
				this.pdf[ this.pdf_o + j ] = new float[ this.npdf[ this.npdf_o + j ] ][] ;	// オフセットのズレが生じる模様

//				this.pdf[ j ] -- ;	// オフセットのズレが生じる模様
				this.pdf_o_o[ this.pdf_o + j ] = -1 ; 
				
//				for( k  = 1 ; k <= this.npdf[ j ] ; k ++ )
				for( k  = 1 ; k <= this.npdf[ this.npdf_o + j ] ; k ++ )		// オフセットのズレが生じる模様
				{
//					this.pdf[ j ][ k ] = new float[ len ] ;
					this.pdf[ this.pdf_o + j ][ this.pdf_o_o[ this.pdf_o + j ] + k ] = new float[ len ] ;		// オフセットのズレが生じる模様
					if( fp.ReadLittleEndian( this.pdf[ this.pdf_o + j ][ this.pdf_o_o[ this.pdf_o + j ] + k ], sizeof( float ), len ) != len )
					{
						result = false ;
					}
				}
			}

			if( result == false )
			{
				Initialize() ;
				return false ;
			}

			return true ;
		}


		// HTS_get_state_num: return the number of state
		public int GetStateNum( string tString )
		{
			if( string.IsNullOrEmpty( tString ) == true )
			{
				return 0 ;
			}

			int i, l = tString.Length ;

			int p ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( tString[ i ] == '[' )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return 0 ;
			}

			p = i + 1 ;

			for( i  = p ; i <  l ; i ++ )
			{
				if( tString[ i ] == ']' )
				{
					break ;
				}
			}

			if( i >= l )
			{
				return 0 ;
			}


			int tStateNumber = 0 ;
			int.TryParse( tString.Substring( p, i - p ), out tStateNumber ) ;
			return tStateNumber ;
		}

		// HTS_Model_get_index: get index of tree and PDF
		public void GetIndex( int state_index, string tString, ref int tree_index, ref int pdf_index )
		{
			HTS_Tree tree ;
			HTS_Pattern pattern ;
			bool find ;
			
			tree_index	= 2 ;
			pdf_index	= 1 ;
			
			if( this.tree == null )
			{
				return ;
			}
			
			find = false ;
			for( tree = this.tree ; tree != null ; tree = tree.next )
			{
				if( tree.state == state_index )
				{
					pattern = tree.head ;
					if( pattern == null )
					{
						find = true ;
					}

					for( ; pattern != null ; pattern = pattern.next )
					{
						if( HTS_Misc.PatternMatch( tString, pattern.word ) == true )
						{
							find = true ;
							break ;
						}
					}

					if( find == true )
					{
						break ;
					}
				}
				tree_index ++ ;
			}

			if( tree != null )
			{
				pdf_index = tree.SearchNode( tString ) ;
			}
			else
			{
				pdf_index = this.tree.SearchNode( tString ) ;
			}
		}

		// HTS_Model_add_parameter: get parameter using interpolation weight
		public void AddParameter( int state_index, string tString, double[] mean, int om, double[] vari, int ov, double[] msd, int o_msd, double weight )
		{
			int i ;
			int tree_index = 0, pdf_index = 0 ;
			int len = this.vector_length * this.num_windows ;
			
			GetIndex( state_index, tString, ref tree_index, ref pdf_index ) ;

			for( i  = 0 ; i <  len ; i ++ )
			{
				// オフセットのズレを考慮する必要がある
				mean[ om + i ] += weight * this.pdf[ this.pdf_o + tree_index ][ this.pdf_o_o[ this.pdf_o + tree_index ] + pdf_index ][ i       ] ;
				vari[ ov + i ] += weight * this.pdf[ this.pdf_o + tree_index ][ this.pdf_o_o[ this.pdf_o + tree_index ] + pdf_index ][ i + len ] ;
			}

			if( msd != null && this.is_msd == true )
			{
				// オフセットのズレを考慮する必要がある
				msd[ o_msd ] += weight * this.pdf[ this.pdf_o + tree_index ][ this.pdf_o_o[ this.pdf_o + tree_index ] + pdf_index ][ len + len ] ;
			}
		}
	}
}
