using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Text ;
using System.Collections ;
using System.Collections.Generic ;
using NPOI.HSSF.UserModel ;
using NPOI.XSSF.UserModel ;
using NPOI.SS.UserModel ;

namespace ExcelHelper
{
	/// <summary>
	/// エクセルの読み書き
	/// </summary>
	public class ExcelUtility
	{
		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイル内の指定のシートをＣＳＶテキストとして読み出す
		/// </summary>
		/// <param name="tPath">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="tSeetName">シート名</param>
		/// <param name="tSplitCode">区切り記号</param>
		/// <returns>ＣＳＶテキスト</returns>
		public static string LoadText( string tPath, string tSheetName, string tSplitCode = "" )
		{
			FileStream tFS = null ;
			IWorkbook tBook = null ;

			if( File.Exists( tPath ) ==  false )
			{
				return null ;
			}

			try
			{
				tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

				if( tPath.EndsWith( ".xls" ) == true )
				{
					tBook = new HSSFWorkbook( tFS ) ;
				}
				else
				if( tPath.EndsWith( ".xlsx" ) == true )
				{
					tBook = new XSSFWorkbook( tFS ) ;
				}
				else
				{
					throw new Exception( "Bad File Type" ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e.Message ) ;
				tBook = null ;
			}
			finally
			{
				if( tFS != null )
				{
					tFS.Close() ;
					tFS.Dispose() ;
					tFS = null ;
				}
			}

			if( tBook == null )
			{
				return null ;
			}

			//----------------------------------------------------------

			// シートの数を取得する
			int tNumberOfSheets = tBook.NumberOfSheets ;
 
			tSheetName = tSheetName.ToLower() ;

			ISheet tSheet = null ;
			
			int tSheetIndex ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				if( tSheet != null )
				{
					if( tSheet.SheetName.ToLower() == tSheetName )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( tSheetIndex >= tNumberOfSheets )
			{
				return null ;	// 指定の名前のシートが見つからない
			}

			return GetText( tSheet, tSplitCode ) ;
		}

		// テキストを取得する
		private static string GetText( ISheet tSheet, string tSplitCode )
		{
			if( string.IsNullOrEmpty( tSplitCode ) == true )
			{
				tSplitCode = "\t" ;
			}

			string tText = "" ;

			int tLastRowNumber = tSheet.LastRowNum ;
			if( tLastRowNumber <  0 )
			{
				return tText ;
			}

			IRow tRow ;
			int tLastCellNumber ;
			ICell tCell ;

			string tWord ;

			int tRowIndex, tCellIndex ;
			for( tRowIndex  = 0 ; tRowIndex <= tLastRowNumber ; tRowIndex ++ )
			{
				tRow = tSheet.GetRow( tRowIndex ) ;
				if( tRow != null )
				{
					tLastCellNumber = tRow.LastCellNum ;
					if( tLastCellNumber >= 0 )
					{
						for( tCellIndex  = 0 ; tCellIndex <= tLastCellNumber ; tCellIndex ++ )
						{
							tCell = tRow.GetCell( tCellIndex ) ;
							if( tCell != null )
							{
								tWord = tCell.ToString() ;
								tWord = tWord.Replace( "\x0A", "\\n" ) ;				// Environment.NewLine Windows 環境は 0x0D 0x0A Machintosh 環境は 0x0A だが、Machintosh 環境でも Excel は 0x0D 0x0A を出力する
//								tWord = tWord.Replace( Environment.NewLine, "\\n" ) ;	// Environment.NewLine Windows 環境は 0x0D 0x0A Machintosh 環境は 0x0A だが、Machintosh 環境でも Excel は 0x0D 0x0A を出力する
								tWord = tWord.Replace( "\x0D", "" ) ;					// Machintosh 環境の対策
								tText = tText + tWord ;
							}

							if( tCellIndex <  tLastCellNumber )
							{
								tText = tText + tSplitCode ;
							}
						}
					}
				}

				if( tRowIndex <  tLastRowNumber )
				{
					tText = tText + Environment.NewLine ;	// 改行を環境ごとのものにする(C#での\nはLF=x0A)
//					tText = tText + "\x0A" ;	// 改行はＬＦ限定
				}
			}

			return tText ;
		}

		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイル内の指定のシートを行列の配列として読み出す
		/// </summary>
		/// <param name="tPath">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="tSeetName">シート名</param>
		/// <returns>ＣＳＶテキスト</returns>
		public static System.Object[,] LoadMatrix( string tPath, string tSheetName )
		{
			FileStream tFS = null ;
			IWorkbook tBook = null ;

			if( File.Exists( tPath ) ==  false )
			{
				return null ;
			}

			try
			{
				tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

				if( tPath.EndsWith( ".xls" ) == true )
				{
					tBook = new HSSFWorkbook( tFS ) ;
				}
				else
				if( tPath.EndsWith( ".xlsx" ) == true )
				{
					tBook = new XSSFWorkbook( tFS ) ;
				}
				else
				{
					throw new Exception( "Bad File Type" ) ;
				}
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e.Message ) ;
				tBook = null ;
			}
			finally
			{
				if( tFS != null )
				{
					tFS.Close() ;
					tFS.Dispose() ;
					tFS = null ;
				}
			}

			if( tBook == null )
			{
				return null ;
			}

			//----------------------------------------------------------

			// シートの数を取得する
			int tNumberOfSheets = tBook.NumberOfSheets ;
 
			tSheetName = tSheetName.ToLower() ;

			ISheet tSheet = null ;
			
			int tSheetIndex ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				if( tSheet != null )
				{
					if( tSheet.SheetName.ToLower() == tSheetName )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( tSheetIndex >= tNumberOfSheets )
			{
				return null ;	// 指定の名前のシートが見つからない
			}

			return GetMatrix( tSheet ) ;
		}

		// 行列を取得する
		private static System.Object[,] GetMatrix( ISheet tSheet )
		{
			int tLastRowNumber = tSheet.LastRowNum ;
			if( tLastRowNumber <  0 )
			{
				return null ;
			}


			IRow tRow ;
			int tLastCellNumber ;
			ICell tCell ;

			int tRowIndex, tCellIndex ;

			// 最大の列数を取得する
			tLastCellNumber = -1 ;

			for( tRowIndex  = 0 ; tRowIndex <= tLastRowNumber ; tRowIndex ++ )
			{
				tRow = tSheet.GetRow( tRowIndex ) ;
				if( tRow != null )
				{
					if( tRow.LastCellNum >  tLastCellNumber )
					{
						tLastCellNumber  = tRow.LastCellNum ;
					}
				}
			}

			if( tLastCellNumber <  0 )
			{
				return null ;
			}

			System.Object[,] tMatrix = new System.Object[ tLastRowNumber + 1, tLastCellNumber + 1 ] ;

			for( tRowIndex  = 0 ; tRowIndex <= tLastRowNumber ; tRowIndex ++ )
			{
				tRow = tSheet.GetRow( tRowIndex ) ;
				if( tRow != null )
				{
					tLastCellNumber = tRow.LastCellNum ;
					if( tLastCellNumber >= 0 )
					{
						for( tCellIndex  = 0 ; tCellIndex <= tLastCellNumber ; tCellIndex ++ )
						{
							tCell = tRow.GetCell( tCellIndex ) ;
							if( tCell != null )
							{
								if( tCell.CellType == CellType.Blank )
								{
									tMatrix[ tRowIndex, tCellIndex ] = ( string)"" ;
								}
								else
								if( tCell.CellType == CellType.Boolean )
								{
									tMatrix[ tRowIndex, tCellIndex ] = tCell.BooleanCellValue ;
								}
								else
								if( tCell.CellType == CellType.Numeric )
								{
									tMatrix[ tRowIndex, tCellIndex ] = tCell.NumericCellValue ;
								}
								else
								{
									tMatrix[ tRowIndex, tCellIndex ] = tCell.StringCellValue ;
								}
							}
						}
					}
				}
			}

			return tMatrix  ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイルにテキスト形式の行列情報を書き込む
		/// </summary>
		/// <param name="tPath">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="tSheetName">シート名</param>
		/// <param name="tText">テキスト形式の行列情報</param>
		/// <param name="tCreate">ファイルまたはシートが存在しない場合は自動的に生成するかどうか(falseの場合は該当するファイルがシートが存在しない場合はエラーとなる)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveText( string tPath, string tSheetName, string tText, bool tCreate = true )
		{
			FileStream tFS = null ;
			IWorkbook tBook = null ;

			bool tResult = false ;

			//----------------------------------------------------------

			// ファイルの取得・生成を行う
			if( File.Exists( tPath ) == true )
			{
				// ファイルが存在する

				try
				{
					// 読み出す
					tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

					if( tPath.EndsWith( ".xls" ) == true )
					{
						tBook = new HSSFWorkbook( tFS ) ;
					}
					else
					if( tPath.EndsWith( ".xlsx" ) == true )
					{
						tBook = new XSSFWorkbook( tFS ) ;
					}
					else
					{
						throw new Exception( "Bad File Type" ) ;
					}
				}
				catch( Exception e )
				{
					Debug.LogWarning( "Error:" + e.Message ) ;
					tBook = null ;
				}
				finally
				{
					if( tFS != null )
					{
						tFS.Close() ;
						tFS.Dispose() ;
						tFS = null ;
					}
				}

				if( tBook == null )
				{
					return false ;	// ファイルフォーマットに異常がある
				}
			}
			else
			{
				// ファイルが存在しない
				if( tCreate == false )
				{
					// エラー
					return false ;
				}

				// 新たに生成する

				if( tPath.EndsWith( ".xls" ) == true )
				{
					tBook = new HSSFWorkbook() ;
				}
				else
				if( tPath.EndsWith( ".xlsx" ) == true )
				{
					tBook = new XSSFWorkbook() ;
				}
				else
				{
					return false ;	// フォーマットが不明
				}
			}
			
			//----------------------------------------------------------

			// シートの追加・更新を行う
			if( SetText( tBook, tSheetName, tText, tCreate ) == false )
			{
				return false ;	// 失敗
			}

			//----------------------------------------------------------

			// シートを追加・更新したブックを保存する
			tResult = true ;

			try
			{
				tFS = new FileStream( tPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite ) ;

				tBook.Write( tFS ) ;
			}
			catch( Exception e )
			{
				Debug.LogWarning( "Error:" + e .Message ) ;
				tResult = false ;
			}
			finally
			{
				if( tFS != null )
				{
					tFS.Close() ;
					tFS.Dispose() ;
					tFS = null ;
				}
			}

			AssetDatabase.Refresh() ;

			return tResult ;
		}

		// テキスト形式の行列情報を書き込む
		private static bool SetText( IWorkbook tBook, string tSheetName, string tText, bool tCreate )
		{
			// シート名の小文字大文字を無視するためあえてめんどくさい方法を使う

			// シートの数を取得する
			int tNumberOfSheets = tBook.NumberOfSheets ;
 
			string tSheetNameChecker = tSheetName.ToLower() ;

			ISheet tSheet = null ;
			
			int tSheetIndex ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				if( tSheet != null )
				{
					if( tSheet.SheetName.ToLower() == tSheetNameChecker )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( tSheetIndex >= tNumberOfSheets )
			{
				// シートは見つからなかった
				if( tCreate == false )
				{
					return false ;	// 失敗
				}

				// シートを追加する
				tSheet = tBook.CreateSheet( tSheetName ) ;
			}

			//----------------------------------------------------------

			// テキストを２次元配列に変換する
			string[][] tDimension = GetDimension( tText ) ;
			if( tDimension == null )
			{
				return false ;	// 失敗
			}


			int x, y ;
			IRow tRow ;
			ICell tCell ;

			for( y  = 0 ; y <  tDimension.Length ; y ++ )
			{
				tRow = tSheet.GetRow( y ) ;
				if( tRow == null )
				{
					// 行を新たに生成する
					tRow = tSheet.CreateRow( y ) ;
				}

				for( x  = 0 ; x <  tDimension[ y ].Length ; x ++ )
				{
					tCell = tRow.GetCell( x ) ;
					if( tCell == null )
					{
						// 列を新たに生成する
						tCell = tRow.CreateCell( x ) ;
					}

					if( string.IsNullOrEmpty( tDimension[ y ][ x ] ) == false )
					{
//						tCell.SetCellType( CellType.String ) ;
						tCell.SetCellValue( ( string )tDimension[ y ][ x ] ) ;
					}
					else
					{
//						tCell.SetCellType( CellType.Blank ) ;
						tCell.SetCellValue( "" ) ;
					}
				}

				// 余計な列を削る
				if( tRow.LastCellNum >  ( tDimension[ y ].Length - 1 ) )
				{
					for( x  = tDimension[ y ].Length ; x <= tRow.LastCellNum ; x ++ )
					{
						tCell = tRow.GetCell( x ) ;
						if( tCell != null )
						{
							tRow.RemoveCell( tCell ) ;
						}
					}
				}
			}

			// 余計な行を削る
			if( tSheet.LastRowNum >  ( tDimension.Length - 1 ) )
			{
				for( y  = tDimension.Length ; y <= tSheet.LastRowNum ; y ++ )
				{
					tRow =  tSheet.GetRow( y ) ;
					if( tRow != null )
					{
						tSheet.RemoveRow( tRow ) ;
					}
				}
			}

			return true ;
		}


		/// <summary>
		/// 指定のパスのＥｘｃｅｌファイルにオブジェクト配列形式の行列情報を書き込む
		/// </summary>
		/// <param name="tPath">Ｅｘｃｅｌファイルのパス</param>
		/// <param name="tSheetName">シート名</param>
		/// <param name="tMatrix">オブジェクト配列形式の行列情報</param>
		/// <param name="tCreate">ファイルまたはシートが存在しない場合は自動的に生成するかどうか(falseの場合は該当するファイルがシートが存在しない場合はエラーとなる)</param>
		/// <returns>結果(true=成功・false=失敗)</returns>
		public static bool SaveMatrix( string tPath, string tSheetName, System.Object[,] tMatrix, bool tCreate = true )
		{
			FileStream tFS = null ;
			IWorkbook tBook = null ;

			bool tResult = false ;

			//----------------------------------------------------------

			// ファイルの取得・生成を行う
			if( File.Exists( tPath ) == true )
			{
				// ファイルが存在する

				// 読み出す
				try
				{
					tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ;

					if( tPath.EndsWith( ".xls" ) == true )
					{
						tBook = new HSSFWorkbook( tFS ) ;
					}
					else
					if( tPath.EndsWith( ".xlsx" ) == true )
					{
						tBook = new XSSFWorkbook( tFS ) ;
					}
					else
					{
						throw new Exception( "Bad File Type" ) ;
					}
				}
				catch( Exception e )
				{
					Debug.LogError( "Error:" + e.Message ) ;
					tBook = null ;
				}
				finally
				{
					if( tFS != null )
					{
						tFS.Close() ;
						tFS.Dispose() ;
						tFS = null ;
					}
				}

				if( tBook == null )
				{
					return false ;	// ファイルフォーマットに異常がある
				}
			}
			else
			{
				// ファイルが存在しない
				if( tCreate == false )
				{
					// エラー
					return false ;
				}

				// 新たに生成する

				if( tPath.EndsWith( ".xls" ) == true )
				{
					tBook = new HSSFWorkbook() ;
				}
				else
				if( tPath.EndsWith( ".xlsx" ) == true )
				{
					tBook = new XSSFWorkbook() ;
				}
				else
				{
					return false ;	// フォーマットが不明
				}
			}
			
			//----------------------------------------------------------

			// シートの追加・更新を行う
			if( SetMatrix( tBook, tSheetName, tMatrix, tCreate ) == false )
			{
				return false ;	// 失敗
			}

			//----------------------------------------------------------

			// シートを追加・更新したブックを保存する
			tFS = null ;
			tResult = true ;

			try
			{
				tFS = new FileStream( tPath, FileMode.OpenOrCreate, FileAccess.Write ) ;
				tBook.Write( tFS ) ;
			}
			catch( Exception e )
			{
				Debug.LogError( "Error:" + e .Message ) ;
				tResult = false ;
			}
			finally
			{
				if( tFS != null )
				{
					tFS.Close() ;
					tFS.Dispose() ;
					tFS = null ;
				}
			}

			//----------------------------------------------------------

			if( tResult == true )
			{
				AssetDatabase.Refresh() ;
			}

			return tResult ;
		}

		// オブジェクト配列形式の行列情報を書き込む
		private static bool SetMatrix( IWorkbook tBook, string tSheetName, System.Object[,] tMatrix, bool tCreate )
		{
			// シート名の小文字大文字を無視するためあえてめんどくさい方法を使う

			// シートの数を取得する
			int tNumberOfSheets = tBook.NumberOfSheets ;
 
			string tSheetNameChecker = tSheetName.ToLower() ;

			ISheet tSheet = null ;
			
			int tSheetIndex ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				if( tSheet != null )
				{
					if( tSheet.SheetName.ToLower() == tSheetNameChecker )
					{
						// ターゲットシートを発見した
						break ;
					}
				}
			}

			if( tSheetIndex >= tNumberOfSheets )
			{
				// シートは見つからなかった
				if( tCreate == false )
				{
					return false ;	// 失敗
				}

				// シートを追加する
				tSheet = tBook.CreateSheet( tSheetName ) ;
			}

			//----------------------------------------------------------

			int x, y ;
			IRow tRow ;
			ICell tCell ;

			for( y  = 0 ; y <  tMatrix.GetLength( 0 ) ; y ++ )
			{
				tRow = tSheet.GetRow( y ) ;
				if( tRow == null )
				{
					// 行を新たに生成する
					tRow = tSheet.CreateRow( y ) ;
				}

				for( x  = 0 ; x <  tMatrix.GetLength( 1 ) ; x ++ )
				{
					tCell = tRow.GetCell( x ) ;
					if( tCell == null )
					{
						// 列を新たに生成する
						tCell = tRow.CreateCell( x ) ;
					}

					if( tMatrix[ y, x ] != null )
					{
						if( tMatrix[ y, x ] is Boolean )
						{
//							tCell.SetCellType( CellType.Boolean ) ;
							tCell.SetCellValue( ( bool )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is int )
						{
//							tCell.SetCellType( CellType.Formula ) ;
							tCell.SetCellValue( ( int )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is uint )
						{
//							tCell.SetCellType( CellType.Formula ) ;
							tCell.SetCellValue( ( uint )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is long )
						{
//							tCell.SetCellType( CellType.Numeric ) ;
							tCell.SetCellValue( ( long )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is ulong )
						{
//							tCell.SetCellType( CellType.Numeric ) ;
							tCell.SetCellValue( ( ulong )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is float )
						{
//							tCell.SetCellType( CellType.Numeric ) ;
							tCell.SetCellValue( ( float )tMatrix[ y, x ] ) ;
						}
						else
						if( tMatrix[ y, x ] is double )
						{
//							tCell.SetCellType( CellType.Numeric ) ;
							tCell.SetCellValue( ( double )tMatrix[ y, x ] ) ;
						}
						else
						{
//							tCell.SetCellType( CellType.String ) ;
							tCell.SetCellValue( ( string )tMatrix[ y, x ] ) ;
						}
					}
					else
					{
//						tCell.SetCellType( CellType.Blank ) ;
						tCell.SetCellValue( "" ) ;
					}
				}

				// 余計な列を削る
				if( tRow.LastCellNum >  ( tMatrix.GetLength( 1 ) - 1 ) )
				{
					for( x  = tMatrix.GetLength( 1 ) ; x <= tRow.LastCellNum ; x ++ )
					{
						tCell = tRow.GetCell( x ) ;
						if( tCell != null )
						{
							tRow.RemoveCell( tCell ) ;
						}
					}
				}
			}

			// 余計な行を削る
			if( tSheet.LastRowNum >  ( tMatrix.GetLength( 0 ) - 1 ) )
			{
				for( y  = tMatrix.GetLength( 0 ) ; y <= tSheet.LastRowNum ; y ++ )
				{
					tRow =  tSheet.GetRow( y ) ;
					if( tRow != null )
					{
						tSheet.RemoveRow( tRow ) ;
					}
				}
			}

			return true ;
		}
		
		//--------------------------------------------------------------------------------------------

		/// <summary>
		/// ＣＳＶテキストの各要素ををパースして格納する
		/// </summary>
		/// <param name="tText">ＣＳＶテキスト(区切りはカンマまたはタブ)</param>
		/// <returns>パース結果(true=成功・false=失敗)</returns>
		private static string[][] GetDimension( string tText )
		{
			if( string.IsNullOrEmpty( tText ) == true )
			{
				return null ;
			}
		
			// まずコメント部分を削る

			int o, p, L, i, j, k, m, r ;
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
		
			// 配列を確保する
			string[][] tValue = new string[ lc ][] ;

			//-----------------------------------------------------
		
			// 実際に値を取得する
			List<string> tWordList = new List<string>() ;
		
			lc = 0 ;
		
			c = ' ' ;
			o = 0 ;
		
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
						tValue[ lc ] = tWordList.ToArray() ;
					
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
						f = 1 ;	// 状態を反転させる
						d = true ;	// ダブルクォート系
						tEscape = false ;
					}
					else
					{
						// そのまま文字列
						o = i ;
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
									if( i >  o )
									{
										tWordList.Add( tText.Substring( o, i - o ) ) ;
									}
									else
									{
										tWordList.Add( "" ) ;
									}
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
									
										tValue[ lc ] = tWordList.ToArray() ;
									
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
							tValue[ lc ] = tWordList.ToArray() ;
						
							lc ++ ;
							r = 0 ;
						}
					}
				}
			}
		
		
			if( r == 1 )
			{
				// 中途半端に終わった
				if( d == false )
				{
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
			
				tValue[ lc ] = tWordList.ToArray() ;
			
				lc ++ ;
				r = 0 ;
			}

			//-------------------------------------------------------------

			// 最後の部分の完全に値が無い部分を切り捨てる

			p = lc ;
			for( i  = ( lc - 1 ) ; i >= 0 ; i -- )
			{
				m = tValue[ i ].Length ;
				for( j  = 0 ; j <  m ; j ++ )
				{
					if( string.IsNullOrEmpty( tValue[ i ][ j ] ) == false )
					{
						break ;
					}
				}
				
				if( j <  m )
				{
					// 現在の行まで残す
					p = i + 1 ;
					break ;
				}
			}
			
			
			if( p <  lc )
			{
				// 切り詰める
				string[][] tReduceValue = new string[ p ][] ;
				
				for( i  = 0 ; i <  p ; i ++ )
				{
					m = tValue[ i ].Length ;
					tReduceValue[ i ] = new string[ m ] ;
					for( j  = 0 ; j <  m ; j ++ )
					{
						tReduceValue[ i ][ j ] = tValue[ i ][ j ] ;
					}
				}
				
				tValue = tReduceValue ;
			}
		
			//-------------------------------------------------------------
		
			return tValue ;
		}
	}
}
