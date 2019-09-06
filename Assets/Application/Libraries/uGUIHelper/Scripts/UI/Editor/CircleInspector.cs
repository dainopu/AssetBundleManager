using UnityEngine ;
using UnityEditor ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// Circle のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( Circle ) ) ]
	public class CircleInspector : MaskableGraphicWrapperInspector
	{
		/// <summary>
		/// スンスペクター描画
		/// </summary>
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
//			DrawDefaultInspector() ;
		
			//--------------------------------------------
		
			// ターゲットのインスタンス
			Circle tTarget = target as Circle ;
		
			// Graphic の基本情報を描画する
			DrawBasis( tTarget ) ;

			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// スプライト
			Sprite tSprite = EditorGUILayout.ObjectField( "Sprite", tTarget.sprite, typeof( Sprite ), false ) as Sprite ;
			if( tSprite != tTarget.sprite )
			{
				Undo.RecordObject( tTarget, "Circle : Sprite Change" ) ;	// アンドウバッファに登録
				tTarget.sprite = tSprite ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 内側の色
			Color tInnerColor = new Color( tTarget.innerColor.r, tTarget.innerColor.g, tTarget.innerColor.b, tTarget.innerColor.a ) ;
			tInnerColor = EditorGUILayout.ColorField( "Inner Color", tInnerColor ) ;
			if( tInnerColor.r != tTarget.innerColor.r || tInnerColor.g != tTarget.innerColor.g || tInnerColor.b != tTarget.innerColor.b || tInnerColor.a != tTarget.innerColor.a )
			{
				Undo.RecordObject( tTarget, "Circle : Inner Color Change" ) ;	// アンドウバッファに登録
				tTarget.innerColor = tInnerColor ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 外側の色
			Color tOuterColor = new Color( tTarget.outerColor.r, tTarget.outerColor.g, tTarget.outerColor.b, tTarget.outerColor.a ) ;
			tOuterColor = EditorGUILayout.ColorField( "Outer Color", tOuterColor ) ;
			if( tOuterColor.r != tTarget.outerColor.r || tOuterColor.g != tTarget.outerColor.g || tOuterColor.b != tTarget.outerColor.b || tOuterColor.a != tTarget.outerColor.a )
			{
				Undo.RecordObject( tTarget, "Circle : Outer Color Change" ) ;	// アンドウバッファに登録
				tTarget.outerColor = tOuterColor ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 分割数
			int tSplit = EditorGUILayout.IntSlider( "Split", tTarget.split,   3, 360 ) ;
			if( tSplit != tTarget.split && tSplit >= 3 )
			{
				Undo.RecordObject( tTarget, "Circle : Split Change" ) ;	// アンドウバッファに登録
				tTarget.split = tSplit ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// 内側の塗りつぶしの有無
			bool tFillInner = EditorGUILayout.Toggle( "Fill Inner", tTarget.fillInner ) ;
			if( tFillInner != tTarget.fillInner )
			{
				Undo.RecordObject( tTarget, "Circle : Fill Inner Change" ) ;	// アンドウバッファに登録
				tTarget.fillInner = tFillInner ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.fillInner == false )
			{
				// 外周の線の太さ(塗りつぶし無し限定)
				float tLineWidth = EditorGUILayout.Slider( "Line Width", tTarget.lineWidth, 1, 20 ) ;
				if( tLineWidth != tTarget.lineWidth )
				{
					Undo.RecordObject( tTarget, "Circle : Line Width Change" ) ;	// アンドウバッファに登録
					tTarget.lineWidth = tLineWidth ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
			else
			{
				// テクスチャの張り方(塗りつぶし有り限定)
				Circle.DecalType tDecalType = ( Circle.DecalType )EditorGUILayout.EnumPopup( "Decal Type",  tTarget.decalType ) ;
				if( tDecalType != tTarget.decalType )
				{
					Undo.RecordObject( tTarget, "Circle : Decal Type Change" ) ;	// アンドウバッファに登録
					tTarget.decalType = tDecalType ;
					EditorUtility.SetDirty( tTarget ) ;
				}
			}
		}
	}
}
