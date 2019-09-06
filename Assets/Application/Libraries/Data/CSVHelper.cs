using UnityEngine ;
using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Reflection ;
using System.Text ;

/// <summary>
/// ＣＳＶヘルパーパッケージ
/// </summary>
namespace CSVHelper
{
	/// <summary>
	/// ＣＳＶデータ管理クラス	Version 2019/02/08 0
	/// </summary>
	
	[Serializable]
	public class CSVObject
	{
		[SerializeField]
		private CSVLineObject[] m_Value ;
	
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="tCount">行数</param>
		public CSVObject( int tCount = 0 )
		{
			if( tCount >= 0 )
			{
				m_Value = new CSVLineObject[ tCount ] ;
			}
		}
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="tCount0">行数</param>
		/// <param name="tCount1">列数</param>
		public CSVObject( int tCount0, int tCount1 )
		{
			if( tCount0 >= 0 )
			{
				m_Value = new CSVLineObject[ tCount0 ] ;

				int i ;

				for( i  = 0 ; i <  tCount0 ; i ++ )
				{
					m_Value[ i ] = new CSVLineObject( tCount1 ) ;
				}
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="tText">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="tReduce">行の最後の空白の要素を削除するかどうか(true=削除する・false=削除しない)</param>
		public CSVObject( string tText, int tTop = 0, bool tReduce = true )
		{
			SetData( tText, tTop, tReduce ) ;
		}
		
		/// <summary>
		/// １行分のデータを追加する
		/// </summary>
		/// <param name="tData"></param>
		public void AddLine( params System.Object[] tData )
		{
			if( tData == null || tData.Length == 0 )
			{
				return ;
			}

			//----------------------------------

			List<CSVLineObject> tList = new List<CSVLineObject>() ;

			if( m_Value != null && m_Value.Length >  0 )
			{
				tList.AddRange( m_Value ) ;
			}

			CSVLineObject tLine = new CSVLineObject( tData ) ;

			tList.Add( tLine ) ;

			m_Value = tList.ToArray() ;
		}

		/// <summary>
		/// インデクサ(各行に直接アクセス可能)
		/// </summary>
		/// <param name="tLine">行番号(0～)</param>
		/// <returns>ＣＳＶの行単位の管理オブジェクトのインスタンス</returns>
		public CSVLineObject this[ int tLine ]
		{
			get
			{
				if( m_Value == null || tLine <  0 || tLine >= m_Value.Length )
				{
					return new CSVLineObject() ;	// 空のオブジェクトを返す
				}

				return m_Value[ tLine ] ;
			}
		}

		/// <summary>
		/// ＣＳＶオブジェクトのＣＳＶテキスト化を行う
		/// </summary>
		/// <param name="tSplitCode"></param>
		/// <returns></returns>
		public string ToString( string tSplitCode = "\t" )
		{
			if( m_Value == null )
			{
				return "" ;
			}

			string tText = "" ;

			int i, l = m_Value.Length ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tText = tText + m_Value[ i ].ToString( tSplitCode ) ;

				if( i <  ( l - 1 ) )
				{
					tText = tText + "\n" ;
				}
			}

			return tText ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストの各要素ををパースして格納する
		/// </summary>
		/// <param name="tText">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="tReduce">行の最後の空白の要素を削除するかどうか(true=削除する・false=削除しない)</param>
		/// <returns>パース結果(true=成功・false=失敗)</returns>
		public bool SetData( string tText, int tTop = 0, bool tReduce = true )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				return false ;
			}
		
			// まずコメント部分を削る
		
			int o, p, L, i, k, r ;
			char c ;
		
			//-------------------------------------------------------------
		
			// ダブルクォート内にある改行・タブを置き換える
		
			L = tText.Length ;
		
			char[] ca = new char[ L ] ;
		
			// 改行を \n に統一する（" 内外関係無し）
			p = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = tText[ i ] ;
			
				if( c == 13 )
				{
					// CR
					if( ( i + 1 ) <  L )
					{
						if( tText[ i + 1 ] == 10 )
						{
							// 改行
							ca[ p ] = '\n' ;
							p ++ ;
						
							i ++ ;	// １文字スキップ
						}
					}
				}
				else
				{
					ca[ p ] = c ;
					p ++ ;
				}
			}
		
			tText = new string( ca, 0, p ) ;
		
			L = tText.Length ;
		
			// この段階で CR は存在しない
		
			// ダブルクォートの外のカンマまたはタブの数をカウントする
			int lc = 0 ;
		
			int f = 0 ;	// ダブルクォートの内側か外側か（デフォルトは外側）
			bool d = false ;
			bool tEscape = false ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = tText[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						lc ++ ;	// 行増加
						r = 0 ;
					}
					else
						if( c == '"' )
					{
						// ダブルクォートを発見した
						f = 1 ;	// 状態を反転させる
						d = true ;	// ダブルクォート系
						tEscape = false ;
					}
					else
					{
						// そのまま文字列
						f = 1 ;
						d = false ;	// 非ダブルクォート系
						tEscape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							tEscape = true ;
						}
						else
						{
							if( tEscape == false )
							{
								if( c == '"' )
								{
									// 終了
									f = 2 ;
								}
							}
							else
							{
								tEscape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							tEscape = true ;
						}
						else
						{
							if( tEscape == false )
							{
								if( c == '\t' || c == ',' || c == '\n' )
								{
									// 終了
									if( c == '\t' || c == ',' )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								tEscape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || c == ',' || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
			if( r == 1 )
			{
				// 中途半端に終わった
				lc ++ ;
				r = 0 ;
			}
		
			//--------------------------
		
			if( ( lc - tTop ) <= 0 )
			{
				// 要素が存在しない
				return false ;
			}
		
			// 配列を確保する
			m_Value = new CSVLineObject[ lc ] ;
			for( i  = 0 ; i <  lc ; i ++ )
			{
				m_Value[ i ] = new CSVLineObject() ;
			}

			//-----------------------------------------------------
		
			// 実際に値を取得する
			List<string> tWordList = new List<string>() ;
		
			lc = 0 ;
		
			c = ' ' ;
			o = -1 ;
		
			r = 0 ;
			for( i  = 0 ; i <  L ; i ++ )
			{
				c = tText[ i ] ;
			
				if( r == 0 )
				{
					// 行開始
					tWordList.Clear() ;
					r = 1 ;
					f = 0 ;
				}
			
				if( f == 0 )
				{
					// 外側
					if( c == ' ' )
					{
						// スペースはスルー
					}
					else
					if( c == '\n' )
					{
						m_Value[ lc ].SetValueArray( tWordList.ToArray() ) ;
					
						lc ++ ;	// 行増加
						r = 0 ;
					}	
					else
					if( c == '\t' || c == ',' )
					{
						// 区切り
						tWordList.Add( "" ) ;
					}
					else
					if( c == '"' )
					{
						// ダブルクォートを発見した
						o = i + 1 ;
						f = 1 ;		// 文字の内側状態に
						d = true ;	// ダブルクォート系
						tEscape = false ;
					}
					else
					{
						// そのまま文字列
						o = i ;
						f = 1 ;		// 文字の内側状態に
						d = false ;	// 非ダブルクォート系
						tEscape = false ;
					}
				}
				else
				if( f == 1 )
				{
					// 内側
					if( d == true )
					{
						// ダブルクォートの文字列
						if( c == '\\' )
						{
							tEscape = true ;
						}
						else
						{
							if( tEscape == false )
							{
								if( c == '"' )
								{
									// 終了
									if( i >  o )
									{
										tWordList.Add( tText.Substring( o, i - o ) ) ;
									}
									else
									{
										tWordList.Add( "" ) ;
									}
									f =  2 ;
								}
							}
							else
							{
								tEscape = false ;
							}
						}
					}
					else
					{
						// 非ダブルクォート系の文字
						if( c == '\\' )
						{
							tEscape = true ;
						}
						else
						{
							if( tEscape == false )
							{
								if( c == '\t' || c == ',' || c == '\n' )
								{
									// 終了
									if( i >  o )
									{
										// 末尾のスペースは削除する
										for( k  = i - 1 ; k >= o ; k -- )
										{
											if( tText[ k ] != ' ' )
											{
												break ;
											}
										}
										if( k >= o )
										{
											tWordList.Add( tText.Substring( o, k - o + 1 ) ) ;
										}
										else
										{
											tWordList.Add( "" ) ;
										}
									}
									else
									{
										if( c != '\n' )
										{
											tWordList.Add( "" ) ;
										}
									}
								
									if( c == '\t' || c == ',' )
									{
										f = 0 ;
									}
									else
									if( c == '\n' )
									{
										f = 0 ;
									
										m_Value[ lc ].SetValueArray( tWordList.ToArray() ) ;
									
										lc ++ ;
										r = 0 ;
									}
								}
							}
							else
							{
								tEscape = false ;
							}
						}
					}
				}
				else
				if( f == 2 )
				{
					if( c == '\t' || c == ',' || c == '\n' )
					{
						// 終わり
						f = 0 ;
					
						if( c == '\n' )
						{
							m_Value[ lc ].SetValueArray( tWordList.ToArray() ) ;
							o = -1 ;
						
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
		
			if( r == 1 )
			{
				if( f == 1 && d == false )
				{
					// 中途半端に終わった
					if( i >  o )
					{
						// 末尾のスペースは削除する
						for( k  = i - 1 ; k >= o ; k -- )
						{
							if( tText[ k ] != ' ' )
							{
								break ;
							}
						}
						if( k >= o )
						{
							tWordList.Add( tText.Substring( o, k - o + 1 ) ) ;
						}
						else
						{
							tWordList.Add( "" ) ;
						}
					}
					else
					{
						if( c != '\n' )
						{
							tWordList.Add( "" ) ;
						}
					}
				}
		
				m_Value[ lc ].SetValueArray( tWordList.ToArray() ) ;
			
				lc ++ ;
				r = 0 ;
			}
		
			//-------------------------------------------------------------
			
			if( tTop >  0 )
			{
				// 頭の部分を切り捨てる
				CSVLineObject[] tValue = new CSVLineObject[ lc - tTop ] ;
				
				for( i  = 0 ; i <  ( lc - tTop ) ; i ++ )
				{
					tValue[ i ] = m_Value[ tTop + i ] ;
				}
				
				m_Value = tValue ;
				lc = lc - tTop ;
			}

			if( tReduce == true )
			{
				// 空行を切り捨てる

				p = 0 ;
				for( i  = 0 ; i <  lc ; i ++ )
				{
					if( m_Value[ i ].Length >   0 )
					{
						p ++ ;
					}
				}

				if( p == 0 )
				{
					m_Value = null ;
					return true ;	// 基本ありえない
				}

				if( p <  lc )
				{
					// 空行が１行以上存在する
					CSVLineObject[] tValue = new CSVLineObject[ p ] ;
					p = 0 ;
					for( i  = 0 ; i <  lc ; i ++ )
					{
						if( m_Value[ i ].Length >  0 )
						{
							tValue[ p ] = m_Value[ i ] ;
							p ++ ;
						}
					}

					m_Value = tValue ;
				}
			}
		
			//-------------------------------------------------------------
		
			return true ;
		}

		/// <summary>
		/// 行数を取得する
		/// </summary>
		/// <returns>行数</returns>
		public int GetLength()
		{
			if( m_Value == null )
			{
				return 0 ;
			}
		
			return m_Value.Length ;
		}
	
		/// <summary>
		/// 行数
		/// </summary>
		public int Length
		{
			get
			{
				return GetLength() ;
			}
		}

		/// <summary>
		/// 最大の列数を取得する
		/// </summary>
		/// <returns></returns>
		public int GetMaximumLineLength()
		{
			if( m_Value == null )
			{
				return 0 ;
			}

			int i, l = m_Value.Length ;
			int k ;
			int m = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_Value[ i ] != null )
				{
					k = m_Value[ i ].GetLength() ;
					if( k >  m )
					{
						m  = k ;
					}
				}
			}

			return m ;
		}

		/// <summary>
		/// 最大の列数
		/// </summary>
		public int MaximumLineLength
		{
			get
			{
				return GetMaximumLineLength() ;
			}
		}
		

		/// <summary>
		/// 行列の配列の形で取得する
		/// </summary>
		/// <returns>行列の配列</returns>
		public string[,] GetMatrix()
		{
			if( m_Value == null )
			{
				return null ;
			}

			int l = m_Value.Length ;
			int m = GetMaximumLineLength() ;

			string[,] tMatrix = new string[ l, m ] ;
			if( l  == 0 || m == 0 )
			{
				return tMatrix ;
			}

			int x, y ;
			for( y  = 0 ; y <  l ; y ++ )
			{
				for( x  = 0 ; x <  m ; x ++ )
				{
					tMatrix[ y, x ] = m_Value[ y ][ x ] ;
				}
			}

			return tMatrix ;
		}

		// 行列の配列
		public string[,] Matrix
		{
			get
			{
				return GetMatrix() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶの行単位の管理クラス
		/// </summary>
		[Serializable]
		public class CSVLineObject
		{
			[SerializeField]
			private string[] m_Value = null ;

			public CSVLineObject(){}

			public CSVLineObject( params System.Object[] tData )
			{
				Set( tData ) ;
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="tCount">列数</param>
			public CSVLineObject( int tCount = 0 )
			{
				if( tCount >= 0 )
				{
					m_Value = new string[ tCount ] ;

					int i ;

					for( i  = 0 ; i <  tCount ; i ++ )
					{
						m_Value[ i ] = "" ;
					}
				}
			}

			/// <summary>
			/// データを設定する
			/// </summary>
			/// <param name="tData"></param>
			public void Set( params System.Object[] tData )
			{
				if( tData == null || tData.Length == 0 )
				{
					m_Value = null ;
					return ;
				}

				int i, l = tData.Length ;

				m_Value = new string[ l ] ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					m_Value[ i ] = tData[ i ].ToString() ;
				}
			}

			/// <summary>
			/// データを追加する
			/// </summary>
			/// <param name="tData"></param>
			public void Add( params System.Object[] tData )
			{
				if( tData == null || tData.Length == 0 )
				{
					return ;
				}

				//---------------------------------

				List<string> tList = new List<string>() ;

				if( m_Value != null && m_Value.Length >  0 )
				{
					tList.AddRange( m_Value ) ;
				}

				int i, l = tData.Length ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					tList.Add( tData[ i ].ToString() ) ;
				}

				m_Value = tList.ToArray() ;
			}

			/// <summary>
			/// インデクサ(各列に直接アクセス可能)
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>指定した列に格納された値(文字列で取得)</returns>
			public string this[ int tIndex ]
			{
				get
				{
					if( m_Value == null || tIndex <  0 || tIndex >= m_Value.Length )
					{
						return "" ;
					}

					return m_Value[ tIndex ] ;
				}
				set
				{
					if( m_Value == null || tIndex <  0 || tIndex >= m_Value.Length )
					{
						return ;
					}

					m_Value[ tIndex ] = value ;
				}
			}

			/// <summary>
			/// 行をテキスト化する
			/// </summary>
			/// <param name="tSplitCode">区切り記号の文字列</param>
			/// <returns>行の文字列</returns>
			public string ToString( string tSplitCode = "\t" )
			{
				if( m_Value == null )
				{
					return "" ;
				}

				string tText = "" ;

				int i, l = m_Value.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( string.IsNullOrEmpty( m_Value[ i ] ) == false )
					{
						tText = tText + m_Value[ i ]  ;
					}

					if( i <  ( l - 1 ) )
					{
						tText = tText + tSplitCode ;
					}
				}

				return tText ;
			}


			/// <summary>
			/// 列の全要素を設定する
			/// </summary>
			/// <param name="tArray">列の全要素が格納された文字列型の配列</param>
			public void SetValueArray( string[] tArray )
			{
				m_Value = tArray ;
			}

			/// <summary>
			/// 列の要素を設定する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <param name="tValue">列の値</param>
			public void SetValue( int tIndex, string tValue )
			{
				if( m_Value == null || tIndex <  0 || tIndex >= m_Value.Length )
				{
					return ;
				}

				m_Value[ tIndex ] = tValue ;
			}

			// 0xb や 0x から値を取得する
			private ulong GetSpecialValue( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				value = value.ToLower() ;

				int l = value.Length - 1 ;

				if( value.Length >  1 && value[ 0 ] == 'b' )
				{
					return GetSpecialValueFromB( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0b" ) == 0 )
				{
					return GetSpecialValueFromB( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'b' )
				{
					return GetSpecialValueFromB( value.Substring( 0, l ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ 0 ] == 'o' )
				{
					return GetSpecialValueFromO( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0o" ) == 0 )
				{
					return GetSpecialValueFromO( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'o' )
				{
					return GetSpecialValueFromO( value.Substring( 0, l ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ 0 ] == 'x' )
				{
					return GetSpecialValueFromH( value.Substring( 1, l ), out result ) ;
				}
				else
				if( value.Length >  2 && value.IndexOf( "0x" ) == 0 )
				{
					return GetSpecialValueFromH( value.Substring( 2, l - 1 ), out result ) ;
				}
				else
				if( value.Length >  1 && value[ l ] == 'x' )
				{
					return GetSpecialValueFromH( value.Substring( 0, l ), out result ) ;
				}

				// 不明
				return 0 ;
			}

			// ２進数
			private ulong GetSpecialValueFromB( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b =  2 ;
				char c ;

				int i, l = value.Length ;

				if( l >  64 )
				{
					l  = 64 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '1' )
					{
						v = v + ( ulong )( c - '0' ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a = a * b ;
				}

				result = true ;
				return v ;
			}

			// ８進数
			private ulong GetSpecialValueFromO( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b =  8 ;
				char c ;

				int i, l = value.Length ;

				if( l >  21 )
				{
					l  = 21 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '7' )
					{
						v = v + ( ulong )( c - '0' ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a = a * b ;
				}

				result = true ;
				return v ;
			}

			// 16進数
			private ulong GetSpecialValueFromH( string value, out bool result )
			{
				result = false ;
				if( string.IsNullOrEmpty( value ) == true )
				{
					return 0 ;
				}

				ulong v =  0 ;
				ulong a =  1 ;
				ulong b = 16 ;
				char c ;

				int i, l = value.Length ;

				if( l >  16 )
				{
					l  = 16 ;
				}

				for( i  = 0 ; i <  l ; i ++ )
				{
					c = value[ l - 1 - i ] ;
					if( c >= '0' && c <= '9' )
					{
						v = v + ( ulong )( c - '0' ) * a ;
					}
					else
					if( c >= 'a' && c <= 'f' )
					{
						v = v + ( ulong )( c - 'a' + 10 ) * a ;
					}
					else
					{
						return 0 ;	// エラー
					}

					a = a * b ;
				}

				result = true ;
				return v ;
			}

			/// <summary>
			/// 列の要素を取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(文字列型)</returns>
			public string GetValue( int tIndex )
			{
				if( m_Value == null || tIndex <  0 || tIndex >= m_Value.Length )
				{
					return "" ;
				}

				return m_Value[ tIndex ] ;
			}

			/// <summary>
			/// 列の要素をブール型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(ブール型)</returns>
			public bool GetBoolean( int tIndex ) 
			{
				if( m_Value == null )
				{
					return false ;
				}
				
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return false ;
				}
		
				string tValue = m_Value[ tIndex ] ;
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return false ;
				}
				
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( v != 0 ) ;
				}

				tValue = tValue.ToLower() ;
		
				if( tValue.Equals( "true" ) == true || tValue.Equals( "1" ) == true )
				{
					return true ;
				}
		
				return false ;
			}
			
			/// <summary>
			/// 列の要素を符号なし8bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public byte GetByte( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( byte )( v & 0xFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tByteValue = 0 ;
				if( double.TryParse( tValue, out tByteValue ) == false )
				{
					Debug.LogWarning( "Byte Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( byte )tByteValue ;
			}

			/// <summary>
			/// 列の要素を符号なし16bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public char GetChar( int tIndex )
			{
				if( m_Value == null )
				{
					return ( char )0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return ( char )0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return ( char )0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( char )( v & 0xFFFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tCharValue = 0 ;
				if( double.TryParse( tValue, out tCharValue ) == false )
				{
					Debug.LogWarning( "Char Parse Error : " + tValue ) ;
					return ( char )0 ;
				}
		
				return ( char )tCharValue ;
			}

			/// <summary>
			/// 列の要素を符号あり16bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public short GetShort( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( short )( v & 0xFFFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tShortValue = 0 ;
				if( double.TryParse( tValue, out tShortValue ) == false )
				{
					Debug.LogWarning( "Short Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( short )tShortValue ;
			}

			/// <summary>
			/// 列の要素を符号なし16bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public ushort GetUshort( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( ushort )( v & 0xFFFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tUshortValue = 0 ;
				if( double.TryParse( tValue, out tUshortValue ) == false )
				{
					Debug.LogWarning( "Ushort Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( ushort )tUshortValue ;
			}

			/// <summary>
			/// 列の要素を符号あり32bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり32bit整数型)</returns>
			public int GetInt( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( int )( v & 0xFFFFFFFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tIntValue = 0 ;
				if( double.TryParse( tValue, out tIntValue ) == false )
				{
					Debug.LogWarning( "Int Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( int )tIntValue ;
			}
	
			/// <summary>
			/// 列の要素を符号なし32bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号なし32bit整数型)</returns>
			public uint GetUint( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ;
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( uint )( v & 0xFFFFFFFF ) ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tUintValue = 0 ;
				if( double.TryParse( tValue, out tUintValue ) == false )
				{
					Debug.LogWarning( "Uint Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( uint )tUintValue ;
			}
	
			/// <summary>
			/// 列の要素を符号あり64bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号あり64bit整数型)</returns>
			public long GetLong( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( long )v ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tLongValue = 0 ;
				if( double.TryParse( tValue, out tLongValue ) == false )
				{
					Debug.LogWarning( "Long Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( long )tLongValue ;
			}
	
			/// <summary>
			/// 列の要素を符号なし64bit整数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(符号なし64bit整数型)</returns>
			public ulong GetUlong( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ; 
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return v ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tUlongValue = 0 ;
				if( double.TryParse( tValue, out tUlongValue ) == false )
				{
					Debug.LogWarning( "Long Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return ( ulong )tUlongValue ;
			}
			
			/// <summary>
			/// 列の要素を単精度浮動小数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(単精度浮動小数型)</returns>
			public float GetFloat( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ;
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( float )v ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				float tFloatValue = 0 ;
				if( float.TryParse( tValue, out tFloatValue ) == false )
				{
					Debug.LogWarning( "Float Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return tFloatValue ;
			}
	
			/// <summary>
			/// 列の要素を倍精度浮動小数型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(倍精度浮動小数型)</returns>
			public double GetDouble( int tIndex )
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return 0 ;
				}
		
				string tValue = m_Value[ tIndex ] ;
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return 0 ;
				}
		
				bool r ;
				ulong v = GetSpecialValue( tValue, out r ) ;
				if( r == true )
				{
					return ( double )v ;
				}

				if( tValue[ tValue.Length - 1 ] == 'f' || tValue[ tValue.Length - 1 ] == 'F' )
				{
					tValue = tValue.Substring( 0, tValue.Length - 1 ) ;
				}
		
				double tDoubleValue = 0 ;
				if( double.TryParse( tValue, out tDoubleValue ) == false )
				{
					Debug.LogWarning( "Double Parse Error : " + tValue ) ;
					return 0 ;
				}
		
				return tDoubleValue ;
			}
			
			/// <summary>
			/// 列の要素を文字列型の値として取得する
			/// </summary>
			/// <param name="tIndex">列番号(0～)</param>
			/// <returns>列の要素(文字列)</returns>
			public string GetString( int tIndex )
			{
				if( m_Value == null )
				{
					return "" ;
				}
				
				if( tIndex <  0 || tIndex >= m_Value.Length )
				{
					return "" ;
				}
		
				// \n \t とを改行コードとタブコードに直す
				string tValue = m_Value[ tIndex ] ;
				if( string.IsNullOrEmpty( tValue ) == true )
				{
					return "" ;
				}
		
				int i, l = tValue.Length ;
				char[] a = new char[ l ] ;
				char w ;
				int p = 0 ;
		
				for( i  = 0 ; i <  l ; i ++ )
				{
					w = tValue[ i ] ;
					if( w == '\\' && i <  ( l - 1 ) && tValue[ i + 1 ] == 'n' )
					{
						// 改行発見
						a[ p ] = '\n' ;
						p ++ ;
						i ++ ;
					}
					else
					if( w == '\\' && i <  ( l - 1 ) && tValue[ i + 1 ] == 't' )
					{
						// タブ発見
						a[ p ] = '\t' ;
						p ++ ;
						i ++ ;
					}
					else
					if( w == '\\' && i <  ( l - 1 ) && tValue[ i + 1 ] == '"' )
					{
						// ダブルクォート発見
						a[ p ] = '"' ;
						p ++ ;
						i ++ ;
					}
					else
					{
						a[ p ] = w ;
						p ++ ;
					}
				}
		
				return new string( a, 0, p ) ;
			}
			
			/// <summary>
			/// 列の要素数を取得する
			/// </summary>
			/// <returns>列の要素数</returns>
			public int GetLength()
			{
				if( m_Value == null )
				{
					return 0 ;
				}
		
				return m_Value.Length ;
			}

			/// <summary>
			/// 列の要素数
			/// </summary>
			public int Length
			{
				get
				{
					return GetLength() ;
				}
			}
		}

		//-----------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶオブジェクトからバイナリ配列化したテーブル情報を取得する
		/// </summary>
		/// <param name="tEncode">文字列のエンコードタイプ(デフォルトは unicode)</param>
		/// <returns>バイナリ配列化したテーブル情報</returns>
		public byte[] ToBinaryTable( string tEncode = "unicode" )
		{
			if( m_Value == null || ( m_Value.Length ) <  3 )
			{
				return null ;
			}

			int r = m_Value.Length ;
			int c = m_Value[ 0 ].Length ;	// カラム数

			if( c == 0 )
			{
				return null ;
			}

			//-----------------------------------

			tEncode = tEncode.ToLower() ;
			Encoding tUTF8 = Encoding.GetEncoding( "UTF-8" ) ;

			int x, y, l, n, p, t, i, v ;
			uint u ;
			long lv ;
			ulong lu ;
			float f ;
			double d ;
			string w ;
			byte[] k ;

			// まずは必要となる総配列サイズ量を計算する

			p = 0 ;

			// カラム数
			v = c ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				p ++ ;
				v = v >> 7 ;
				if( v == 0 )
				{
					break ;
				}
			}

			// レコード数
			v = r ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				p ++ ;
				v = v >> 7 ;
				if( v == 0 )
				{
					break ;
				}
			}

			// カラム名
			if( tEncode == "utf8" || tEncode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					n = tUTF8.GetByteCount( m_Value[  0 ][ x ] ) ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						p ++ ;
						v = v >> 7 ;
						if( v == 0 )
						{
							break ;
						}
					}
					p = p + n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					n = m_Value[ 0 ][ x ].Length ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						p ++ ;
						v = v >> 7 ;
						if( v == 0 )
						{
							break ;
						}
					}
					p = p + n * 2 ;
				}
			}

			// タイプ
			p = p + c ;

			int[] tType = new int[ c ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				w = m_Value[ 1 ][ x ].ToLower() ;

				if( w == "bool" || w == "boolean" )
				{
					tType[ x ] =  1 ;	// bool
				}
				else
				if( w == "byte" )
				{
					tType[ x ] =  2 ;	// byte
				}
				else
				if( w == "char" )
				{
					tType[ x ] =  3 ;	// char
				}
				else
				if( w == "short" || w == "int16" )
				{
					tType[ x ] =  4 ;	// short
				}
				else
				if( w == "ushort" )
				{
					tType[ x ] =  5 ;	// ushort
				}
				else
				if( w == "int" || w == "int32" )
				{
					tType[ x ] =  6 ;	// int
				}
				else
				if( w == "uint" )
				{
					tType[ x ] =  7 ;	// uint
				}
				else
				if( w == "long" || w == "int64" )
				{
					tType[ x ] =  8 ;	// long
				}
				else
				if( w == "ulong" )
				{
					tType[ x ] =  9 ;	// ulong
				}
				else
				if( w == "float" )
				{
					tType[ x ] = 10 ;	// float
				}
				else
				if( w == "double" )
				{
					tType[ x ] = 11 ;	// double
				}
				else
				if( w == "string" || w == "text" )
				{
					tType[ x ] = 12 ;	// string
				}
			}

			l = r - 2 ;

			// データ部
			for( x  = 0 ; x <  c ; x ++ )
			{
				t = tType[ x ] ;

				if( t ==  1 || t ==  2 )
				{
					// bool byte
					p = p + ( 1 * l ) ;
				}
				else
				if( t ==  3 || t ==  4 || t ==  5 )
				{
					// char short ushort
					p = p + ( 2 * l ) ;
				}
				else
				if( t ==  6 || t ==  7 || t == 10 )
				{
					// int uint float
					p = p + ( 4 * l ) ;
				}
				else
				if( t ==  8 || t ==  9 || t == 11 )
				{
					// long ulong
					p = p + ( 8 * l ) ;
				}
				else
				if( t == 12 )
				{
					// string

					if( tEncode == "utf8" || tEncode == "utf-8" )
					{
						// UTF-8
						for( y  = 2 ; y <  r ; y ++ )
						{
							n = tUTF8.GetByteCount( m_Value[ y ][ x ] ) ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								p ++ ;
								v = v >> 7 ;
								if( v == 0 )
								{
									break ;
								}
							}
							p = p + n ;
						}
					}
					else
					{
						// Unicode
						for( y  = 2 ; y <  r ; y ++ )
						{
							n = m_Value[ y ][ x ].Length ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								p ++ ;
								v = v >> 7 ;
								if( v == 0 )
								{
									break ;
								}
							}
							p = p + n * 2 ;
						}
					}
				}
			}

			// 必要サイズが決定しました
	//		Debug.LogWarning( "必要サイズ:" + p ) ;

			//-------------------------------------------------------------------------------------

			byte[] tData = new byte[ p ] ;
			p = 0 ;

			// データを格納していく

			// カラム数
			v = c ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				tData[ p ] = ( byte )( v & 0x7F ) ;
				v = v >> 7 ;

				if( v >  0 )
				{
					tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
					p ++ ;
				}
				else
				{
					p ++ ;
					break ;
				}
			}

			// レコード数
			v = r ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				tData[ p ] = ( byte )( v & 0x7F ) ;
				v = v >> 7 ;

				if( v >  0 )
				{
					tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
					p ++ ;
				}
				else
				{
					p ++ ;
					break ;
				}
			}

			// カラム名
			if( tEncode == "utf8" || tEncode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					k = tUTF8.GetBytes( m_Value[ 0 ][ x ] ) ;
					if( k != null )
					{
						n = k.Length ;
					}
					else
					{
						n = 0 ;
					}
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						tData[ p ] = ( byte )( v & 0x7F ) ;
						v = v >> 7 ;

						if( v >  0 )
						{
							tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
							p ++ ;
						}
						else
						{
							p ++ ;
							break ;
						}
					}

					Array.Copy( k, 0, tData, p, n ) ;
					p = p + n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					w = m_Value[ 0 ][ x ] ;

					n = w.Length ;
					v = n ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						tData[ p ] = ( byte )( v & 0x7F ) ;
						v = v >> 7 ;

						if( v >  0 )
						{
							tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
							p ++ ;
						}
						else
						{
							p ++ ;
							break ;
						}
					}

					for( i  = 0 ; i <  n ; i ++ )
					{
						tData[ p ] = ( byte )( w[ i ] & 0xFF ) ;
						p ++ ;
						tData[ p ] = ( byte )( ( w[ i ] >> 8 ) & 0xFF ) ;
						p ++ ;
					}
				}
			}

			// タイプ
			for( x  = 0 ; x <  c ; x ++ )
			{
				tData[ p ] = ( byte )tType[ x ] ;
				p ++ ;
			}

			// データ部
			for( y  = 2 ; y <  r ; y ++ )
			{
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = tType[ x ] ;
					w = m_Value[ y ][ x ] ;

					if( t ==  1 )
					{
						// bool
						w = w.ToLower() ;
						if( w == "true" || w != "0" )
						{
							tData[ p ] = 1 ;
						}
						else
						{
							tData[ p ] = 0 ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// bool byte
						tData[ p ] = ( byte )( byte.Parse( w ) ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						u = char.Parse( w ) ;
						tData[ p ] = ( byte )( u & 0xFF ) ;
						p ++ ;
						tData[ p ] = ( byte )( ( u >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  4 )
					{
						// short
						v = short.Parse( w ) ;
						tData[ p ] = ( byte )( v & 0xFF ) ;
						p ++ ;
						tData[ p ] = ( byte )( ( v >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						u = ushort.Parse( w ) ;
						tData[ p ] = ( byte )( u & 0xFF ) ;
						p ++ ;
						tData[ p ] = ( byte )( ( u >> 8 ) & 0xFF ) ;
						p ++ ;
					}
					else
					if( t ==  6 )
					{
						// int
						v = int.Parse( w ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							tData[ p ] = ( byte )( ( v >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  7 )
					{
						// uint
						u = uint.Parse( w ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							tData[ p ] = ( byte )( ( u >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  8 )
					{
						// long
						lv = long.Parse( w ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							tData[ p ] = ( byte )( ( lv >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t ==  9 )
					{
						// ulong
						lu = ulong.Parse( w ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							tData[ p ] = ( byte )( ( lu >> ( i * 8 ) ) & 0xFF ) ;
							p ++ ;
						}
					}
					else
					if( t == 10 )
					{
						// float
						f = float.Parse( w ) ;
						byte[] b = BitConverter.GetBytes( f ) ;
						for( i  = 0 ; i <  4 ; i ++ )
						{
							tData[ p ] = b [ i ] ;
							p ++ ;
						}
					}
					else
					if( t == 11 )
					{
						// double
						d = double.Parse( w ) ;
						byte[] b = BitConverter.GetBytes( d ) ;
						for( i  = 0 ; i <  8 ; i ++ )
						{
							tData[ p ] = b [ i ] ;
							p ++ ;
						}
					}
					else
					if( t == 12 )
					{
						// string
						if( tEncode == "utf8" || tEncode == "utf-8" )
						{
							// UTF-8
							k = tUTF8.GetBytes( w ) ;
							if( k != null )
							{
								n = k.Length ;
							}
							else
							{
								n = 0 ;
							}
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								tData[ p ] = ( byte )( v & 0x7F ) ;
								v = v >> 7 ;

								if( v >  0 )
								{
									tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
									p ++ ;
								}
								else
								{
									p ++ ;
									break ;
								}
							}

							Array.Copy( k, 0, tData, p, n ) ;
							p = p + n ;
						}
						else
						{
							// Unicode
							n = w.Length ;
							v = n ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								tData[ p ] = ( byte )( v & 0x7F ) ;
								v = v >> 7 ;

								if( v >  0 )
								{
									tData[ p ] = ( byte )( tData[ p ] | 0x80 ) ;
									p ++ ;
								}
								else
								{
									p ++ ;
									break ;
								}
							}

							for( i  = 0 ; i <  n ; i ++ )
							{
								tData[ p ] = ( byte )( w[ i ] & 0xFF ) ;
								p ++ ;
								tData[ p ] = ( byte )( ( w[ i ] >> 8 ) & 0xFF ) ;
								p ++ ;
							}
						}
					}
				}
			}

	//		Debug.LogWarning( "格納サイズ:" + p ) ;

			return tData ;
		}


		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストから任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="tText">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="tKey">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<T>( string tText, ref List<T> rList, int tTop = 0 ) where T : class, new()
		{
			List<Dictionary<string,System.Object>> tData = LoadFromText( tText, tTop ) ;
			if( tData == null )
			{
				// 失敗
				return false ;
			}
		
			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = tData.Count ;
			
			T tRecord ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tRecord = new T() ;
				Initialize<T>( ref tRecord, tData[ i ] ) ;
				
				rList.Add( tRecord ) ;
			}

			return true ;
		}

		// テキストから配列を生成する
		private static List<Dictionary<string,System.Object>> LoadFromText( string tText, int tTop )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				// 失敗
				return null ;
			}

			CSVObject tCSV = new CSVObject( tText, tTop ) ;
			if( tCSV.Length <= 2 )
			{
				return null ;
			}

			//------------------------------------------------------------------

			int r, c, m, l = tCSV.Length ;
			string tLabel ;
			string tType ;


			List<Dictionary<string,System.Object>> tData = new List<Dictionary<string, object>>() ;

			m = tCSV[ 0 ].Length ;
			for( r  = 2 ; r <  l ; r ++ )
			{
				Dictionary<string,System.Object> tLine = new Dictionary<string, object>() ;
				for( c  = 0 ; c <  m ; c ++ )
				{
					tLabel = tCSV[ 0 ].GetString( c ) ;
					if( string.IsNullOrEmpty( tLabel ) == false )
					{
						if( tLine.ContainsKey( tLabel ) == true )
						{
#if UNITY_EDITOR
							Debug.LogWarning( "カラム名が重複している:" + tLabel ) ;
#endif
						}

						tType  = tCSV[ 1 ].GetString( c ).ToLower() ;

						if( tType == "bool" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetBoolean( c ) ) ;
						}
						else
						if( tType == "byte" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetByte( c ) ) ;
						}
						else
						if( tType == "char" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetChar( c ) ) ;
						}
						else
						if( tType == "short" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetShort( c ) ) ;
						}
						else
						if( tType == "ushort" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUshort( c ) ) ;
						}
						else
						if( tType == "int" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetInt( c ) ) ;
						}
						else
						if( tType == "uint" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUint( c ) ) ;
						}
						else
						if( tType == "long" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetLong( c ) ) ;
						}
						else
						if( tType == "ulong" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUlong( c ) ) ;
						}
						else
						if( tType == "float" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetFloat( c ) ) ;
						}
						else
						if( tType == "double" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetDouble( c ) ) ;
						}
						else
						{
							tLine.Add( tLabel, tCSV[ r ].GetString( c ) ) ;
						}
					}
				}

				tData.Add( tLine ) ;
			}

			return tData ;
		}


		
		/// <summary>
		/// ＣＳＶテキストから任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="tText">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="tKey">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<K,T>( string tText, ref List<T> rList, ref Dictionary<K,T> rHash, string tKey = "id", int tTop = 0 ) where T : class, new()
		{
			Dictionary<K,Dictionary<string,System.Object>> tData = LoadFromText<K>( tText, tKey, tTop ) ;
			if( tData == null )
			{
				// 失敗
				return false ;
			}
		
			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			if( rHash == null )
			{
				rHash	= new Dictionary<K,T>() ;
			}
			else
			{
				rHash.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = tData.Count ;
			
			K[] tId = new K[ l ] ;
			tData.Keys.CopyTo( tId, 0 ) ;
		
			T tRecord ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( rHash.ContainsKey( tId[ i ] ) == false )
				{
					tRecord = new T() ;
					Initialize<T>( ref tRecord, tData[ tId[ i ] ] ) ;
				
					rList.Add( tRecord ) ;
					rHash.Add( tId[ i ], tRecord ) ;
				}
			}

			return true ;
		}


		// テキストから配列を生成する
		private static Dictionary<K,Dictionary<string,System.Object>> LoadFromText<K>( string tText, string tKey, int tTop )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				// 失敗
				return null ;
			}

			CSVObject tCSV = new CSVObject( tText, tTop ) ;
			if( tCSV.Length <= 2 )
			{
				return null ;
			}

			//------------------------------------------------------------------

			int r, c, m, l = tCSV.Length ;
			string tLabel ;
			string tType ;
			K id ;


			Dictionary<K,Dictionary<string,System.Object>> tData = new Dictionary<K, Dictionary<string, object>>() ;

			m = tCSV[ 0 ].Length ;
			for( r  = 2 ; r <  l ; r ++ )
			{
				Dictionary<string,System.Object> tLine = new Dictionary<string, object>() ;
				for( c  = 0 ; c <  m ; c ++ )
				{
					tLabel = tCSV[ 0 ].GetString( c ) ;
					if( string.IsNullOrEmpty( tLabel ) == false )
					{
						if( tLine.ContainsKey( tLabel ) == true )
						{
#if UNITY_EDITOR
							Debug.LogWarning( "カラム名が重複している:" + tLabel ) ;
#endif
						}

						tType  = tCSV[ 1 ].GetString( c ).ToLower() ;

						if( tType == "bool" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetBoolean( c ) ) ;
						}
						else
						if( tType == "byte" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetByte( c ) ) ;
						}
						else
						if( tType == "char" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetChar( c ) ) ;
						}
						else
						if( tType == "short" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetShort( c ) ) ;
						}
						else
						if( tType == "ushort" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUshort( c ) ) ;
						}
						else
						if( tType == "int" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetInt( c ) ) ;
						}
						else
						if( tType == "uint" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUint( c ) ) ;
						}
						else
						if( tType == "long" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetLong( c ) ) ;
						}
						else
						if( tType == "ulong" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetUlong( c ) ) ;
						}
						else
						if( tType == "float" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetFloat( c ) ) ;
						}
						else
						if( tType == "double" )
						{
							tLine.Add( tLabel, tCSV[ r ].GetDouble( c ) ) ;
						}
						else
						{
							tLine.Add( tLabel, tCSV[ r ].GetString( c ) ) ;
						}
					}
				}

				if( tLine.ContainsKey( tKey ) == true )
				{
					id = ( K )tLine[ tKey ] ;
	
					if( tData.ContainsKey( id ) == false )
					{
						tData.Add( id, tLine ) ;
					}
					else
					{
						Debug.LogWarning( "[CSVHelper] キー値が重複します: Key = ( " + tKey  + " ) Value = ( " + id + " )\nText = " + tText ) ;
					}
				}
				else
				{
					Debug.LogWarning( "[CSVHelper] キー値が存在しません: Key = " + tKey ) ;
				}
			}

			return tData ;
		}

		//-----------------------------------------------------------

		/// <summary>
		/// バイナリ配列化したテーブル情報から任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="tBinary">バイナリ配列化したテーブル情報</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="tKey">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<T>( byte[] tBinary, ref List<T> rList ) where T : class, new()
		{
			// ロードする
			List<Dictionary<string,System.Object>> tData = LoadFromBinary( tBinary ) ;
			if( tData == null )
			{
				// 失敗
				return false ;
			}

			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = tData.Count ;

			T tRecord ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tRecord = new T() ;
				Initialize<T>( ref tRecord, tData[ i ] ) ;
					
				rList.Add( tRecord ) ;
			}

			return true ;
		}

		// バイナリ型ＣＳＶからデータ配列を生成する
		private static List<Dictionary<string,System.Object>> LoadFromBinary( byte[] tBinary, string tEncode = "unicode" )
		{
			if( tBinary == null )
			{
				return null ;
			}

			tEncode = tEncode.ToLower() ;
			Encoding tUTF8 = Encoding.GetEncoding( "UTF-8" ) ;

			byte b, t ;
			int i, p, x, y, n ;
			char[] w ;

			p = 0 ;

			// カラム数を取得
			int c = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = tBinary[ p ] ;
				p ++ ;

				c = c | ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// レコード数を取得
			int r = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = tBinary[ p ] ;
				p ++ ;

				r = r | ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// ラベル名を取得
			string[] tLabel = new string[ c ] ;

			if( tEncode == "utf8" || tEncode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = tBinary[ p ] ;
						p ++ ;

						n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					// Unicode 想定
					tLabel[ x ] = tUTF8.GetString( tBinary, p, n ) ;
					p = p + n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = tBinary[ p ] ;
						p ++ ;

						n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					w = new char[ n ] ;
					for( i  = 0 ; i <  n ; i ++ )
					{
						w[ i ] = ( char )( ( tBinary[ p + 1 ] << 8 ) | tBinary[ p ] ) ;
						p = p + 2 ;
					}
					tLabel[ x ] = new string( w ) ;

	//					Debug.LogWarning( "ラベル名:" + tLabel[ x ] ) ;
				}
			}


			// タイプを取得
			byte[] tType = new byte[ x ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				tType[ x ] = tBinary[ p ] ;
				p ++ ;
			}


			List<Dictionary<string,System.Object>> tData = new List<Dictionary<string, object>>() ;

			for( y  = 2 ; y <  r ; y ++ )
			{
				Dictionary<string,System.Object> tLine = new Dictionary<string, object>() ;
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = tType[ x ] ;

					if( t ==  1 )
					{
						// bool
						if( tBinary[ p ] == 0 )
						{
							tLine.Add( tLabel[ x ], false ) ;
						}
						else
						{
							tLine.Add( tLabel[ x ], true ) ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// byte
						tLine.Add( tLabel[ x ], tBinary[ p ] ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						tLine.Add( tLabel[ x ], ( char )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  4 )
					{
						// short
						tLine.Add( tLabel[ x ], ( short )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						tLine.Add( tLabel[ x ], ( ushort )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  6 )
					{
						// int
						tLine.Add( tLabel[ x ], ( int )( ( tBinary[ p + 3 ] << 24 ) |  ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 4 ;
					}
					else
					if( t ==  7 )
					{
						// uint
						tLine.Add( tLabel[ x ], ( uint )( ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 4 ;
					}
					else
					if( t ==  8 )
					{
						// long
						tLine.Add( tLabel[ x ], ( long )( ( tBinary[ p + 7 ] << 56 ) | ( tBinary[ p + 6 ] << 48 ) | ( tBinary[ p + 5 ] << 40 ) | ( tBinary[ p + 4 ] << 32 ) | ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 8 ;
					}
					else
					if( t ==  9 )
					{
						// ulong
						tLine.Add( tLabel[ x ], ( ulong )( ( tBinary[ p + 7 ] << 56 ) | ( tBinary[ p + 6 ] << 48 ) | ( tBinary[ p + 5 ] << 40 ) | ( tBinary[ p + 4 ] << 32 ) | ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 8 ;
					}
					else
					if( t == 10 )
					{
						// float
						tLine.Add( tLabel[ x ], BitConverter.ToSingle( tBinary, p ) ) ;
						p = p + 4 ;
					}
					else
					if( t == 11 )
					{
						// double
						tLine.Add( tLabel[ x ], BitConverter.ToDouble( tBinary, p ) ) ;
						p = p + 8 ;
					}
					else
					if( t == 12 )
					{
						// string
						if( tEncode == "utf8" || tEncode == "utf-8" )
						{
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = tBinary[ p ] ;
								p ++ ;

								n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							tLine.Add( tLabel[ x ], tUTF8.GetString( tBinary, p, n ) ) ;
							p = p + n ;
						}
						else
						{
							// Unicode
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = tBinary[ p ] ;
								p ++ ;

								n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							// Unicode 想定
							w = new char[ n ] ;
							for( i  = 0 ; i <  n ; i ++ )
							{
								w[ i ] = ( char )( ( tBinary[ p + 1 ] << 8 ) | tBinary[ p ] ) ;
								p = p + 2 ;
							}
							tLine.Add( tLabel[ x ], new string( w ) ) ;
						}
					}
				}

				tData.Add( tLine ) ;
			}

			return tData ;
		}
		//-----------------------------------------------------------

		/// <summary>
		/// バイナリ配列化したテーブル情報から任意クラスのリストとハッシュオブジェクトを生成する(必ず０行目を識別名・１行目を型とし、識別名id・型intを含めること)
		/// </summary>
		/// <typeparam name="K">キー値の型</typeparam>
		/// <typeparam name="T">任意クラスの型</typeparam>
		/// <param name="tBinary">バイナリ配列化したテーブル情報</param>
		/// <param name="rList">(出力)リストオブジェクト</param>
		/// <param name="rHash">(出力)ハッシュオブジェクト</param>
		/// <param name="tKey">キー値の名前</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool Load<K,T>( byte[] tBinary, ref List<T> rList, ref Dictionary<K,T> rHash, string tKey = "id" ) where T : class, new()
		{
			// ロードする
			Dictionary<K,Dictionary<string,System.Object>> tData = LoadFromBinary<K>( tBinary, tKey ) ;
			if( tData == null )
			{
				// 失敗
				return false ;
			}

			//------------------------------------------------------------------

			if( rList == null )
			{
				rList	= new List<T>() ;
			}
			else
			{
				rList.Clear() ;
			}

			if( rHash == null )
			{
				rHash	= new Dictionary<K,T>() ;
			}
			else
			{
				rHash.Clear() ;
			}

			// リフレクションで値を格納する
			int i, l = tData.Count ;

			K[] tId = new K[ l ] ;
			tData.Keys.CopyTo( tId, 0 ) ;
		
			T tRecord ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				if( rHash.ContainsKey( tId[ i ] ) == false )
				{
					tRecord = new T() ;
					Initialize<T>( ref tRecord, tData[ tId[ i ] ] ) ;
					
					rList.Add( tRecord ) ;
					rHash.Add( tId[ i ], tRecord ) ;
				}
			}

			return true ;
		}

		// バイナリ型ＣＳＶからデータ配列を生成する
		private static Dictionary<K,Dictionary<string,System.Object>> LoadFromBinary<K>( byte[] tBinary, string tKey, string tEncode = "unicode" )
		{
			if( tBinary == null )
			{
				return null ;
			}

			tEncode = tEncode.ToLower() ;
			Encoding tUTF8 = Encoding.GetEncoding( "UTF-8" ) ;

			byte b, t ;
			int i, p, x, y, n ;
			char[] w ;

			p = 0 ;

			// カラム数を取得
			int c = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = tBinary[ p ] ;
				p ++ ;

				c = c | ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// レコード数を取得
			int r = 0 ;
			for( i  = 0 ; i <  4 ; i ++ )
			{
				b = tBinary[ p ] ;
				p ++ ;

				r = r | ( ( b & 0x7F ) << ( 7 * i ) ) ;
				if( ( b & 0x80 ) == 0 )
				{
					break ;
				}
			}

			// ラベル名を取得
			string[] tLabel = new string[ c ] ;

			if( tEncode == "utf8" || tEncode == "utf-8" )
			{
				// UTF-8
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = tBinary[ p ] ;
						p ++ ;

						n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					// Unicode 想定
					tLabel[ x ] = tUTF8.GetString( tBinary, p, n ) ;
					p = p + n ;
				}
			}
			else
			{
				// Unicode
				for( x  = 0 ; x <  c ; x ++ )
				{
					// 文字数
					n = 0 ;
					for( i  = 0 ; i <  4 ; i ++ )
					{
						b = tBinary[ p ] ;
						p ++ ;

						n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
						if( ( b & 0x80 ) == 0 )
						{
							break ;
						}
					}

					w = new char[ n ] ;
					for( i  = 0 ; i <  n ; i ++ )
					{
						w[ i ] = ( char )( ( tBinary[ p + 1 ] << 8 ) | tBinary[ p ] ) ;
						p = p + 2 ;
					}
					tLabel[ x ] = new string( w ) ;

	//					Debug.LogWarning( "ラベル名:" + tLabel[ x ] ) ;
				}
			}


			// タイプを取得
			byte[] tType = new byte[ x ] ;
			for( x  = 0 ; x <  c ; x ++ )
			{
				tType[ x ] = tBinary[ p ] ;
				p ++ ;
			}

			K id ;


			Dictionary<K,Dictionary<string,System.Object>> tData = new Dictionary<K, Dictionary<string, object>>() ;

			for( y  = 2 ; y <  r ; y ++ )
			{
				Dictionary<string,System.Object> tLine = new Dictionary<string, object>() ;
				for( x  = 0 ; x <  c ; x ++ )
				{
					t = tType[ x ] ;

					if( t ==  1 )
					{
						// bool
						if( tBinary[ p ] == 0 )
						{
							tLine.Add( tLabel[ x ], false ) ;
						}
						else
						{
							tLine.Add( tLabel[ x ], true ) ;
						}
						p ++ ;
					}
					else
					if( t ==  2 )
					{
						// byte
						tLine.Add( tLabel[ x ], tBinary[ p ] ) ;
						p ++ ;
					}
					else
					if( t ==  3 )
					{
						// char
						tLine.Add( tLabel[ x ], ( char )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  4 )
					{
						// short
						tLine.Add( tLabel[ x ], ( short )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  5 )
					{
						// ushort
						tLine.Add( tLabel[ x ], ( ushort )( ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 2 ;
					}
					else
					if( t ==  6 )
					{
						// int
						tLine.Add( tLabel[ x ], ( int )( ( tBinary[ p + 3 ] << 24 ) |  ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 4 ;
					}
					else
					if( t ==  7 )
					{
						// uint
						tLine.Add( tLabel[ x ], ( uint )( ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 4 ;
					}
					else
					if( t ==  8 )
					{
						// long
						tLine.Add( tLabel[ x ], ( long )( ( tBinary[ p + 7 ] << 56 ) | ( tBinary[ p + 6 ] << 48 ) | ( tBinary[ p + 5 ] << 40 ) | ( tBinary[ p + 4 ] << 32 ) | ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 8 ;
					}
					else
					if( t ==  9 )
					{
						// ulong
						tLine.Add( tLabel[ x ], ( ulong )( ( tBinary[ p + 7 ] << 56 ) | ( tBinary[ p + 6 ] << 48 ) | ( tBinary[ p + 5 ] << 40 ) | ( tBinary[ p + 4 ] << 32 ) | ( tBinary[ p + 3 ] << 24 ) | ( tBinary[ p + 2 ] << 16 ) | ( tBinary[ p + 1 ] <<  8 ) | tBinary[ p ] ) ) ;
						p = p + 8 ;
					}
					else
					if( t == 10 )
					{
						// float
						tLine.Add( tLabel[ x ], BitConverter.ToSingle( tBinary, p ) ) ;
						p = p + 4 ;
					}
					else
					if( t == 11 )
					{
						// double
						tLine.Add( tLabel[ x ], BitConverter.ToDouble( tBinary, p ) ) ;
						p = p + 8 ;
					}
					else
					if( t == 12 )
					{
						// string
						if( tEncode == "utf8" || tEncode == "utf-8" )
						{
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = tBinary[ p ] ;
								p ++ ;

								n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							tLine.Add( tLabel[ x ], tUTF8.GetString( tBinary, p, n ) ) ;
							p = p + n ;
						}
						else
						{
							// Unicode
							n = 0 ;
							for( i  = 0 ; i <  4 ; i ++ )
							{
								b = tBinary[ p ] ;
								p ++ ;

								n = n | ( ( b & 0x7F ) << ( 7 * i ) ) ;
								if( ( b & 0x80 ) == 0 )
								{
									break ;
								}
							}

							// Unicode 想定
							w = new char[ n ] ;
							for( i  = 0 ; i <  n ; i ++ )
							{
								w[ i ] = ( char )( ( tBinary[ p + 1 ] << 8 ) | tBinary[ p ] ) ;
								p = p + 2 ;
							}
							tLine.Add( tLabel[ x ], new string( w ) ) ;
						}
					}
				}

				if( tLine.ContainsKey( tKey ) == true )
				{
					id = ( K )tLine[ tKey ] ;

					if( tData.ContainsKey( id ) == false )
					{
						tData.Add( id, tLine ) ;
					}
					else
					{
						Debug.LogWarning( "キー名が重複します:" + id ) ;
					}
				}
				else
				{
					Debug.LogWarning( "キー名が存在しません:" + tKey ) ;
				}
			}

			return tData ;
		}

		// リフレクションで値を格納する
		private static  bool Initialize<T>( ref T tTarget, Dictionary<string,System.Object> o )
		{
			if( o == null )
			{
				return false ;	// オブジェクトが空であれば展開しない
			}

			//--------------------------

			System.Object tThisObject = ( System.Object )tTarget ;	// 自身のオブジェクト
			System.Type   tThisType   = tTarget.GetType() ;		// 自身のタイプ（クラス）
		
			// メンバ情報を取得する
			MemberInfo[] tMemberInfoArray = tThisType.GetMembers
			(
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.Instance |
				// BindingFlags.Static |
				BindingFlags.DeclaredOnly |
				BindingFlags.FlattenHierarchy			// 継承元のメンバも含める
			) ;

			BindingFlags flags = ( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ;

			// メンバごとに処理を行う
			foreach( MemberInfo m in tMemberInfoArray )
			{
				if( m.MemberType == MemberTypes.Field )
				{
					//---------------------------
				
					// このメンバの型を取得する
					System.Type tFieldType = tThisType.GetField( m.Name, flags ).FieldType ;
				
					// フィールドタイプ（メンバの型）の種類に応じて値を格納する
					if( tFieldType.IsArray == false )
					{
						// スカラー型
					
						if( o.ContainsKey( m.Name ) == true )
						{
							if( tFieldType == typeof( bool ) )
							{
								// ブーリアン型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( bool )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( byte ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( byte )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( char ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( char )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( short ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( short )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( ushort ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( ushort )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( int ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( int )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( uint ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( uint )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( long ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( long )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( ulong ) )
							{
								// インテジャー型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( ulong )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( float ) )
							{
								// フロート型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( float )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( double ) )
							{
								// ダブル型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( double )o[ m.Name ] ) ;
							}
							else
							if( tFieldType == typeof( string ) )
							{
								// ストリング型のスカラー値
								tThisType.GetField( m.Name, flags ).SetValue( tThisObject, ( string )o[ m.Name ] ) ;
							}
						}
					}
					else
					{
						// アレイ型

						// １レコードのキーを全て取得する
						int i, l = o.Count ;
		
						string[] tKeyAll = new string[ l ] ;
		
						o.Keys.CopyTo( tKeyAll, 0 ) ;

						List<string> tArrayKey = new List<string>() ;

						// キーの配列名を持つものを挙列する
						int p, s, e, c ;
						for( i  = 0 ; i <  l ; i ++ )
						{
							if( tKeyAll[ i ].IndexOf( m.Name ) >= 0 )
							{
								// 配列名をキー名の一部にものものを発見した
								tArrayKey.Add( tKeyAll[ i ] ) ;
							}
						}

						if( tArrayKey.Count >  0 )
						{
							// １つ以上あり

							string k ;
							l = tArrayKey.Count ;
							int[] tIndex = new int[ l ] ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								tIndex[ i ] = -1 ;
							}

							c = 0 ;
							for( i  = 0 ; i <  l ; i ++ )
							{
								k = tArrayKey[ i ].Replace( " ", "" ) ;

								// 配列の添字で最も大きい値を取得する
								s = k.IndexOf( "[" ) ;
								e = k.IndexOf( "]" ) ;
								if( s >= 0 && e >= 0 )
								{
									k = k.Substring( s + 1, ( e - s - 1 ) ) ;
									if( k.Length >  0 )
									{
										p = int.Parse( k ) ;
										tIndex[ i ] = p ;
										if( p >  c )
										{
											c  = p ;
										}
									}
								}
							}


							// アレイの型を取得する
							System.Type tElementType = tFieldType.GetElementType() ;
					
							l = c + 1 ;
						
							System.Array tArray = Array.CreateInstance( tElementType, l ) ;
					
							// 要素の値数分ループ
							for( i  = 0 ; i <  tArrayKey.Count ; i ++ )
							{
								if( tIndex[ i ] >= 0 )
								{
									// エレメントタイプ（アレイの型）の種類に応じて値を格納する
									if( tElementType == typeof( bool ) ) 
									{
										tArray.SetValue( ( bool    )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( byte ) )
									{
										tArray.SetValue( ( byte    )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( char ) )
									{
										tArray.SetValue( ( char    )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( short ) )
									{
										tArray.SetValue( ( short   )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( ushort ) )
									{
										tArray.SetValue( ( ushort  )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( int ) )
									{
										tArray.SetValue( ( int    )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( uint ) )
									{
										tArray.SetValue( ( uint   )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( long ) )
									{
										tArray.SetValue( ( long   )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( ulong ) )
									{
										tArray.SetValue( ( ulong  )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( float ) )
									{
										tArray.SetValue( ( float  )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( double ) )
									{
										tArray.SetValue( ( double )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
									else
									if( tElementType == typeof( string ) )
									{
										tArray.SetValue( ( string )o[ tArrayKey[ i ] ], tIndex[ i ] ) ;
									}
								}
							}

							// 最後に配列全体をメンバにセットする
							tThisType.GetField( m.Name, flags ).SetValue( tThisObject, tArray ) ;
						}
					}
				}
			}

			return true ;
		}
	}
}
