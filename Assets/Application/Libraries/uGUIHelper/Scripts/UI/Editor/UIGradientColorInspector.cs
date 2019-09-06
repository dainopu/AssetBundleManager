using UnityEngine ;
using UnityEditor ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// UIGradientColor のインスペクタークラス
	/// </summary>
	[ CustomEditor( typeof( UIGradient ) ) ]
	public class UIGradientInspector : Editor
	{
		// スンスペクター描画
		public override void OnInspectorGUI()
		{
			// とりあえずデフォルト
	//		DrawDefaultInspector() ;
		
			//--------------------------------------------

			// ターゲットのインスタンス
			UIGradient tTarget = target as UIGradient ;
		
			EditorGUILayout.Separator() ;   // 少し区切りスペース

			// バリュータイプ
			UIGradient.Geometory tGeometory = ( UIGradient.Geometory )EditorGUILayout.EnumPopup( "Geometory",  tTarget.geometory ) ;
			if( tGeometory != tTarget.geometory )
			{
				Undo.RecordObject( tTarget, "UIGradientColor : Geometory Change" ) ;	// アンドウバッファに登録
				tTarget.geometory = tGeometory ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			// バリュータイプ
			UIGradient.Direction tDirection = ( UIGradient.Direction )EditorGUILayout.EnumPopup( "Direction",  tTarget.direction ) ;
			if( tDirection != tTarget.direction )
			{
				Undo.RecordObject( tTarget, "UIGradientColor : Direction Change" ) ;	// アンドウバッファに登録
				tTarget.direction = tDirection ;
				tTarget.Refresh() ;
				EditorUtility.SetDirty( tTarget ) ;
			}

			if( tTarget.direction == UIGradient.Direction.Vertical || tTarget.direction == UIGradient.Direction.Both )
			{
				Color tTop = CloneColor( tTarget.top ) ;
				tTop = EditorGUILayout.ColorField( "Top", tTop ) ;
				if( CheckColor( tTop, tTarget.top ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Top Change" ) ;	// アンドウバッファに登録
					tTarget.top = tTop ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.geometory == UIGradient.Geometory.Image )
				{
					Color tMiddle = CloneColor( tTarget.middle ) ;
					tMiddle = EditorGUILayout.ColorField( "Middle", tMiddle ) ;
					if( CheckColor( tMiddle, tTarget.middle ) == false )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Middle Change" ) ;	// アンドウバッファに登録
						tTarget.middle = tMiddle ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

				Color tBottom = CloneColor( tTarget.bottom ) ;
				tBottom = EditorGUILayout.ColorField( "Bottom", tBottom ) ;
				if( CheckColor( tBottom, tTarget.bottom ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Bottom Change" ) ;	// アンドウバッファに登録
					tTarget.bottom = tBottom ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.geometory == UIGradient.Geometory.Image )
				{
					float tPivotMiddle = EditorGUILayout.Slider( "Pivot Middle", tTarget.pivotMiddle, 0, 1 ) ;
					if( tPivotMiddle != tTarget.pivotMiddle )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Pivot Middle Change" ) ;	// アンドウバッファに登録
						tTarget.pivotMiddle = tPivotMiddle ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
			}

			if( tTarget.direction == UIGradient.Direction.Horizontal || tTarget.direction == UIGradient.Direction.Both )
			{
				Color tLeft = CloneColor( tTarget.left ) ;
				tLeft = EditorGUILayout.ColorField( "Left", tLeft ) ;
				if( CheckColor( tLeft, tTarget.left ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Left Change" ) ;	// アンドウバッファに登録
					tTarget.left = tLeft ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.geometory == UIGradient.Geometory.Image )
				{
					Color tCenter = CloneColor( tTarget.center ) ;
					tCenter = EditorGUILayout.ColorField( "Center", tCenter ) ;
					if( CheckColor( tCenter, tTarget.center ) == false )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Center Change" ) ;	// アンドウバッファに登録
						tTarget.center = tCenter ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}

				Color tRight = CloneColor( tTarget.right ) ;
				tRight = EditorGUILayout.ColorField( "Right", tRight ) ;
				if( CheckColor( tRight, tTarget.right ) == false )
				{
					Undo.RecordObject( tTarget, "UIGradientColor : Right Change" ) ;	// アンドウバッファに登録
					tTarget.right = tRight ;
					tTarget.Refresh() ;
					EditorUtility.SetDirty( tTarget ) ;
				}

				if( tTarget.geometory == UIGradient.Geometory.Image )
				{
					float tPivotCenter = EditorGUILayout.Slider( "Pivot Center", tTarget.pivotCenter, 0, 1 ) ;
					if( tPivotCenter != tTarget.pivotCenter )
					{
						Undo.RecordObject( tTarget, "UIGradientColor : Pivot Center Change" ) ;	// アンドウバッファに登録
						tTarget.pivotCenter = tPivotCenter ;
						tTarget.Refresh() ;
						EditorUtility.SetDirty( tTarget ) ;
					}
				}
			}
		}

		private Color CloneColor( Color tColor )
		{
			Color tClone = new Color() ;
			tClone.r = tColor.r ;
			tClone.g = tColor.g ;
			tClone.b = tColor.b ;
			tClone.a = tColor.a ;

			return tClone ;
		}

		private bool CheckColor( Color c0, Color c1 )
		{
			if( c0.r != c1.r || c0.g != c1.g  || c0.b != c1.b || c0.a != c1.a )
			{
				return false ;
			}

			return true ;
		}
	}
}

