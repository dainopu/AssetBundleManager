using UnityEngine ;
using UnityEditor ;
using System ;
using System.IO ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// デフォルトフォント設定の操作ウィンドウ
	/// </summary>
	
	public class DefaultSettingsMenu : EditorWindow
	{
		[ MenuItem( "uGUIHelper/Tools/DefaultSettings" ) ]
		private static void OpenWindow()
		{
			EditorWindow.GetWindow<DefaultSettingsMenu>( false, "Default Settings", true ) ;
		}

		// レイアウトを描画する
		private void OnGUI()
		{
			string tPath = null ;
			string[] tId = AssetDatabase.FindAssets( "DefaultSettings" ) ;
			if( tId != null )
			{
				int i, l = tId.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tPath = AssetDatabase.GUIDToAssetPath( tId[ i ] ) ;

					if( Directory.Exists( tPath ) == true )
					{
						break ;
					}
				}

				if( i >= l )
				{
					tPath = null ;
				}
			}

			if( string.IsNullOrEmpty( tPath ) == true )
			{
				EditorGUILayout.HelpBox( "状態が異常です", MessageType.Warning ) ;
				return ;
			}

			tPath = tPath + "/Resources/uGUIHelper" ;
			if( Directory.Exists( tPath ) == false )
			{
				EditorGUILayout.HelpBox( "保存フォルダが存在しません", MessageType.Warning ) ;
				return ;
			}

			DefaultSettings tDS = null ;

			tPath = tPath + "/DefaultSettings.asset" ;
			if( File.Exists( tPath ) == false )
			{
				// ファイルが存在しない
				tDS= ScriptableObject.CreateInstance<DefaultSettings>() ;
				tDS.name = "DefaultSettings" ;
		
				AssetDatabase.CreateAsset( tDS, tPath ) ;
				AssetDatabase.Refresh() ;
			}
			else
			{
				// ファイルが存在する
				tDS = AssetDatabase.LoadAssetAtPath<DefaultSettings>( tPath ) ;
			}

			Selection.activeObject = tDS ;

			//----------------------------------------------------------

			bool tDirty = false ;

			// ボタンの下地
			Sprite tButtonFrame = EditorGUILayout.ObjectField( "Button", tDS.buttonFrame, typeof( Sprite ), false ) as Sprite ;
			if( tButtonFrame != tDS.buttonFrame )
			{
				tDS.buttonFrame		= tButtonFrame ;
				tDirty = true ;
			}

			// ボタン無効化時の色
			Color c ;
			c.r = tDS.buttonDisabledColor.r ;
			c.g = tDS.buttonDisabledColor.g ;
			c.b = tDS.buttonDisabledColor.b ;
			c.a = tDS.buttonDisabledColor.a ;
			tDS.buttonDisabledColor = EditorGUILayout.ColorField( "Button Disabled Color", tDS.buttonDisabledColor ) ;
			if( tDS.buttonDisabledColor.Equals( c ) == false )
			{
				tDirty = true ;
			}

			// ボタンラベルのフォントサイズ
			int tButtonLabelFontSize =  EditorGUILayout.IntField( "Button Label Font Size", tDS.buttonLabelFontSize ) ;
			if( tButtonLabelFontSize != tDS.buttonLabelFontSize )
			{
				tDS.buttonLabelFontSize	= tButtonLabelFontSize ;
				tDirty = true ;
			}

			// ボタンラベルの色
			c.r = tDS.buttonLabelColor.r ;
			c.g = tDS.buttonLabelColor.g ;
			c.b = tDS.buttonLabelColor.b ;
			c.a = tDS.buttonLabelColor.a ;
			tDS.buttonLabelColor = EditorGUILayout.ColorField( "Button Label Color", tDS.buttonLabelColor ) ;
			if( tDS.buttonLabelColor.Equals( c ) == false )
			{
				tDirty = true ;
			}

			bool tButtonLabelShadow = EditorGUILayout.Toggle( "Button Label Shadow", tDS.buttonLabelShadow ) ;
			if( tButtonLabelShadow != tDS.buttonLabelShadow )
			{
				tDS.buttonLabelShadow		= tButtonLabelShadow ;
				tDirty = true ;
			}

			bool tButtonLabelOutline = EditorGUILayout.Toggle( "Button Label Outline", tDS.buttonLabelOutline ) ;
			if( tButtonLabelOutline != tDS.buttonLabelOutline )
			{
				tDS.buttonLabelOutline		= tButtonLabelOutline ;
				tDirty = true ;
			}

			//----------------------------------

			// プログレスバーの下地
			Sprite tProgressbarFrame = EditorGUILayout.ObjectField( "ProgressbarFrame", tDS.progressbarFrame, typeof( Sprite ), false ) as Sprite ;
			if( tProgressbarFrame != tDS.progressbarFrame )
			{
				tDS.progressbarFrame	= tProgressbarFrame ;
				tDirty = true ;
			}

			// プログレスバーの前景
			Sprite tProgressbarThumb = EditorGUILayout.ObjectField( "ProgressbarThumb", tDS.progressbarThumb, typeof( Sprite ), false ) as Sprite ;
			if( tProgressbarThumb != tDS.progressbarThumb )
			{
				tDS.progressbarThumb	= tProgressbarThumb ;
				tDirty = true ;
			}

			//----------------------------------

			// テキストの色
			c.r = tDS.textColor.r ;
			c.g = tDS.textColor.g ;
			c.b = tDS.textColor.b ;
			c.a = tDS.textColor.a ;
			tDS.textColor = EditorGUILayout.ColorField( "Text Color", tDS.textColor ) ;
			if( tDS.textColor.Equals( c ) == false )
			{
				tDirty = true ;
			}

			// フォント
			Font tFont = EditorGUILayout.ObjectField( "Font", tDS.font, typeof( Font ), false ) as Font ;
			if( tFont != tDS.font )
			{
				tDS.font		= tFont ;
				tDirty = true ;
			}

			// フォントサイズ
			int tFontSize =  EditorGUILayout.IntField( "Font Size", tDS.fontSize ) ;
			if( tFontSize != tDS.fontSize )
			{
				tDS.fontSize	= tFontSize ;
				tDirty = true ;
			}

			//----------------------------------

			// インプットフィールド関係
			FontFilter tFontFilter = EditorGUILayout.ObjectField( "Font Filter", tDS.fontFilter, typeof( FontFilter ), false ) as FontFilter ;
			if( tFontFilter != tDS.fontFilter )
			{
				tDS.fontFilter = tFontFilter ;
				tDirty = true ;
			}

			string tFontAlternateCodeOld = "" + tDS.fontAlternateCode ;
			string tFontAlternateCodeNew = EditorGUILayout.TextField( "Font Alternate Code", tFontAlternateCodeOld ) ;
			if( string.IsNullOrEmpty( tFontAlternateCodeNew ) == false && ( tFontAlternateCodeNew[ 0 ] != tFontAlternateCodeOld[ 0 ] ) )
			{
				tDS.fontAlternateCode = tFontAlternateCodeNew[ 0 ] ;
				tDirty = true ;
			}

			//----------------------------------

			// 更新判定
			if( tDirty == true )
			{
				EditorUtility.SetDirty( tDS ) ; // 更新実行
//				AssetDatabase.Refresh() ;
			}
		}
	}
}

