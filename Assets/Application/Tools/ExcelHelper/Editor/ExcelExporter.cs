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
	/// Excel Exporter Version 2017/05/26 0
	/// </summary>
	public class ExcelExporter : AssetPostprocessor
	{
		private const string m_SymbolSgheetName = "@Export" ;

		public static void OnPostprocessAllAssets( string[] tImportedAssets, string[] tDeletedAssets, string[] tMovedAssets, string[] tMovedFromAssetPaths )
		{
//			Debug.LogWarning( "何らかのファイルがインポートされた" ) ;

			foreach( string tImportedAsset in tImportedAssets )
			{
				if( tImportedAsset.EndsWith( ".xls" ) == true )
				{
					ReadXLS( tImportedAsset ) ;
				}
				else
				if( tImportedAsset.EndsWith( ".xlsx" ) == true )
				{
					ReadXLSX( tImportedAsset ) ;
				}
			}
		}

		// .xls タイプのファイルを開く
		private static void ReadXLS( string tPath )
		{
			int i = tPath.LastIndexOf( '/' ) + 1 ;
			if( tPath.IndexOf( "~$" ) == i )
			{
//				Debug.LogWarning( "バックアップファイルは対象外:" + tPath ) ;
				return ;
			}

//			Debug.LogWarning( "ＸＬＳが読み出される:" + tPath ) ;
			using( FileStream tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
//				Debug.Log( "ReadXLS:" + tPath ) ;
 
				ReadBook( tPath, new HSSFWorkbook( tFS ) ) ;
			}
		}

		// .xksx タイプのファイルを開く
		private static void ReadXLSX( string tPath )
		{
			int i = tPath.LastIndexOf( '/' ) + 1 ;
			if( tPath.IndexOf( "~$" ) == i )
			{
//				Debug.LogWarning( "バックアップファイルは対象外:" + tPath ) ;
				return ;
			}

//			Debug.LogWarning( "ＸＬＳＸが読み出される:" + tPath ) ;
			using( FileStream tFS = new FileStream( tPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
//				Debug.Log( "ReadXLSX:" + tPath ) ;

				ReadBook( tPath, new XSSFWorkbook( tFS ) ) ;
			}
		}

		//-------------------------------------------------------------------------------------

		// 各シートを展開・保存する
		private static void ReadBook( string tPath, IWorkbook tBook )
		{
			Dictionary<string,string> tOverwritePath = new Dictionary<string, string>() ;

			string tRootPath = GetPath( tPath, tBook, ref tOverwritePath ) ;

			if( string.IsNullOrEmpty( tRootPath ) == true )
			{
				return ;	// 必要なシートが存在しない
			}

			//----------------------------------------------------------

			string tSymbolSheetName = m_SymbolSgheetName.ToLower() ;

			//----------------------------------------------------------

			int tNumberOfSheets = tBook.NumberOfSheets ;
			
			ISheet tSheet = null ;
			
			string tName, tText ;

			int tTotalExecute = 0 ;

			int tSheetIndex, tFolderIndex ;
			string tFolderName ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				if( tSheet != null )
				{
					tName = tSheet.SheetName ;
					if( string.IsNullOrEmpty( tName ) == false )
					{
						if( tName[ 0 ] != '#' )
						{
							// 無効化シートではない

							tText = GetText( tSheet ) ;

							if( string.IsNullOrEmpty( tName ) == false && string.IsNullOrEmpty( tText ) == false && tName.ToLower() != tSymbolSheetName )
							{
								if( tOverwritePath.ContainsKey( tName ) == false || ( tOverwritePath.ContainsKey( tName ) == true && string.IsNullOrEmpty( tOverwritePath[ tName ] ) == true ) )
								{
									tPath = tRootPath + tName ;

									if( tName.IndexOf( '.' ) <  0 )
									{
										tPath = tPath + ".txt" ;
									}
								}
								else
								{
									tPath = tOverwritePath[ tName ] ;
									if( tPath.Length >  0 && tPath[ tPath.Length - 1 ] != '/' )
									{
										tPath = tRootPath + tPath ;
									}
								}

								// パスにフォルダが含まれているかチェックする
								tFolderIndex = tPath.LastIndexOf( '/' ) ;
								if( tFolderIndex >= 0 )
								{
									// フォルダが含まれている
									tFolderName = tPath.Substring( 0, tFolderIndex ) ;

									if( Directory.Exists( tFolderName ) == false )
									{
										// フォルダを生成する(多階層をまとめて生成出来る)
										Directory.CreateDirectory( tFolderName ) ;
									}
								}


								// 文字列をバイナリ化する(UTF-8N)
								byte[] tData = System.Text.Encoding.UTF8.GetBytes( tText ) ;

								if( tData != null && tData.Length >= 3 )
								{
									if( tData[ 0 ] == ( byte )0xEF && tData[ 1 ] == ( byte )0xBB && tData[ 2 ] == ( byte )0xBF )
									{
										// 先頭にＢＯＭが付与されていたら削る
										int l = tData.Length - 3 ;
										byte[] tWork = new byte[ l ] ;
										if( l  >  0 )
										{
											Array.Copy( tData, 3, tWork, 0, l ) ;
										}
										tData = tWork ;
									}
								}

								bool tExecute = true ;

								if( File.Exists( tPath ) == true )
								{
									// 既にファイルが存在する場合は読み出して比較する
									byte[] tWork = File.ReadAllBytes( tPath ) ;
									if( tWork != null && tData.Length == tWork.Length )
									{
										int i, l = tWork.Length ;
										for( i  = 0 ; i <  l ; i ++ )
										{
											if( tData[ i ] != tWork[ i ] )
											{
												break ;
											}
										}
										if( i >= l )
										{
											tExecute = false ;	// 完全に同一ファイルなので書き込みを行わない
										}
									}
								}

								if( tExecute == true )
								{
									File.WriteAllBytes( tPath, tData ) ;
									Debug.Log( "Sheet Exported : " + tName +  " --> " + tPath ) ;
									tTotalExecute ++ ;
								}
							}
						}
					}
				}
			}

			if( tTotalExecute >  0 )
			{
				AssetDatabase.SaveAssets() ;
				AssetDatabase.Refresh() ;

				Debug.Log( "Excel Exporter Finished ... Total number of files : " + tTotalExecute ) ;
			}
		}


		// 出力先情報を取得する
		private static string GetPath( string tPath, IWorkbook tBook, ref Dictionary<string,string> tOverwritePath )
		{
			string tSymbolSheetName = m_SymbolSgheetName.ToLower() ;
			string tSheetName = null ;

			//----------------------------------------------------------

			int tNumberOfSheets = tBook.NumberOfSheets ;
 
			ISheet tSheet = null ;
			
			int tSheetIndex ;
			for( tSheetIndex = 0 ; tSheetIndex <  tNumberOfSheets ; tSheetIndex ++ )
			{
				// シート情報を取得する
				tSheet = tBook.GetSheetAt( tSheetIndex ) ;
				
				tSheetName = "" ;
				if( string.IsNullOrEmpty( tSheet.SheetName ) == false )
				{
					tSheetName = tSheet.SheetName.ToLower() ;
				}

				if( tSheetName == tSymbolSheetName )
				{
					break ;
				}
			}

			if( tSheetIndex >= tNumberOfSheets )
			{
				return null ;	// 必要なシート情報が取得出来ず
			}

			// デフォルトの出力パス
			string tRootPath = tPath ;
			if( tRootPath.Length >  0 && tRootPath[ 0 ] == '/' )
			{
				// 先頭にスラッシュがあれば削除する
				tRootPath = tRootPath.Remove( 0, 1 ) ;
			}

			int tIndex = tRootPath.LastIndexOf( '/' ) ;
			if( tIndex >= 0 )
			{
				tRootPath = tRootPath.Substring( 0, tIndex + 1 ) ;
			}

			//-------------------------------------------------

			// そのシートの最終行
			int tLastRowNumber = tSheet.LastRowNum ;

			IRow tRow ;
			int tLastCellNumber ;
			ICell tCell ;

			if( tLastRowNumber >= 0 )
			{
				tRow = tSheet.GetRow( 0 ) ;
				if( tRow != null )
				{
					tLastCellNumber = tRow.LastCellNum ;
					if( tLastCellNumber >= 0 )
					{
						tCell = tRow.GetCell( 0 ) ;
						if( tCell != null )
						{
							// ルートパスの指定があった
							tRootPath = tCell.ToString() ;

							if( tRootPath.Length >  0 && tRootPath[ 0 ] == '/' )
							{
								// 先頭にスラッシュがあれば削除する
								tRootPath = tRootPath.Remove( 0, 1 ) ;
							}
						}
					}
				}
			}

			int tLength = tRootPath.Length ;
			if( tLength >  0 )
			{
				if( tRootPath[ tLength - 1 ] != '/' )
				{
					tRootPath = tRootPath + "/" ;
				}
			}
			else
			{
				tRootPath = "/" ;
			}

			// 相対パス指定の場合はソースファイルのパスに連結する
			if( tRootPath.IndexOf( "./" ) == 0 || tRootPath.IndexOf( "../" ) == 0 )
			{
				string tBasePath = tPath ;
				int p = tBasePath.LastIndexOf( '/' ) ;
				if( p >= 0 )
				{
					tBasePath = tBasePath.Substring( 0, p ) ;
				}

				int l = tRootPath.Length ;
				if( tRootPath.IndexOf( "./" ) == 0 )
				{
					tRootPath = tBasePath + tRootPath.Substring( 1, l - 1 ) ;
				}
				else
				if( tRootPath.IndexOf( "../" ) == 0 )
				{
					p = tBasePath.LastIndexOf( '/' ) ;
					if( p >= 0 )
					{
						tBasePath = tBasePath.Substring( 0, p ) ;
					}
					tRootPath = tBasePath + tRootPath.Substring( 2, l - 2 ) ;
				}
			}

			if( tLastRowNumber >= 1 )
			{
				// 特殊パス指定あり
				string tSpecificName, tSpecificPath ;

				int tRowIndex ;
				for( tRowIndex  = 1 ; tRowIndex <= tLastRowNumber ; tRowIndex ++ )
				{
					tRow = tSheet.GetRow( tRowIndex ) ;
					if( tRow != null )
					{
						tLastCellNumber = tRow.LastCellNum ;
						if( tLastCellNumber >= 1 )
						{
							tSpecificName = "" ;
							tCell = tRow.GetCell( 0 ) ;
							if( tCell != null )
							{
								tSpecificName = tCell.ToString() ;
							}

							tSpecificPath = "" ;
							tCell = tRow.GetCell( 1 ) ;
							if( tCell != null )
							{
								tSpecificPath = tCell.ToString() ;
								tLength = tSpecificPath.Length ;
								if( tLength >  0 && tSpecificPath[ tLength - 1 ] == '/' )
								{
									if( string.IsNullOrEmpty( tSpecificName ) == false )
									{
										tSpecificPath = tSpecificPath + tSpecificName ;
									}

									if( tSpecificName.IndexOf( '.' ) <  0 )
									{
										tSpecificPath = tSpecificPath + ".txt" ;

									}
								}
							}

							if( string.IsNullOrEmpty( tSpecificName ) == false && string.IsNullOrEmpty( tSpecificPath ) == false )
							{
								if( tOverwritePath.ContainsKey( tSpecificName ) == false )
								{
									tOverwritePath.Add( tSpecificName, tSpecificPath ) ;
								}
							}
						}
					}
				}
			}

			return tRootPath ;
		}

		// テキストを取得する
		private static string GetText( ISheet tSheet )
		{
			string tText = "" ;

			int tLastRowNumber = tSheet.LastRowNum ;

			IRow tRow ;
			int tLastCellNumber ;
			ICell tCell ;

			string tWord ;

			if( tLastRowNumber >= 1 )
			{
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
//									tWord = tWord.Replace( Environment.NewLine, "\\n" ) ;	// Environment.NewLine Windows 環境は 0x0D 0x0A Machintosh 環境は 0x0A だが、Machintosh 環境でも Excel は 0x0D 0x0A を出力する
									tWord = tWord.Replace( "\x0D", "" ) ;					// Machintosh 環境の対策
									tText = tText + tWord ;
								}

								if( tCellIndex <  tLastCellNumber )
								{
									tText = tText + "\t" ;
								}
							}
						}
					}

					if( tRowIndex <  tLastRowNumber )
					{
						tText = tText + Environment.NewLine ;	// 改行を環境ごとのものにする(C#での\nはLF=x0A)
//						tText = tText + "\x0A" ;	// 改行はＬＦ限定
					}
				}
			}

			return tText ;
		}
	}
}
