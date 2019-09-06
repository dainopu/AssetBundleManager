using UnityEngine ;
using System.Collections ;

[ RequireComponent( typeof( Camera ) ) ]
	
[ ExecuteInEditMode ]


/// <summary>
/// カメラの表示領域をクリッピングするコンポーネントクラス Version 2016/11/11 0
/// </summary>
public class CameraClipping : MonoBehaviour
{
	/// <summary>
	/// 画面の仮想解像度
	/// </summary>
	public Vector2 size = new Vector2( 270, 480 ) ;

	/// <summary>
	/// 画面に対する表示領域
	/// </summary>
	public Rect    view = new Rect(   0,   0, 270, 480 ) ;

	
	/// <summary>
	/// スクリーンに対する配置方法の定義
	/// </summary>
	public enum ScreenMatchMode
	{
		Expand = 0,
		Width  = 1,
		Height = 2,
	}

	/// <summary>
	/// スクリーンに対する配置方法
	/// </summary>
	public ScreenMatchMode screenMatchMode = ScreenMatchMode.Height ;

	// 対象のカメラのインスタンス
	private Camera m_Camera = null ;


	void Start()
	{
	}
	
	void Update()
	{
		if( m_Camera == null )
		{
			m_Camera = GetComponent<Camera>() ;
		}

		if( m_Camera != null && size.x >  0 && size.y >  0 )
		{
			UpdateCameraViewport( m_Camera, size.x, size.y, view.x, view.y, view.width, view.height ) ;
        }
	}

	// バトルスペースのアスペクト比を設定
	private void UpdateCameraViewport( Camera tCamera, float aw, float ah, float dx, float dy, float dw, float dh )
	{
		if( tCamera == null )
		{
			return ;
		}

		float tSW = Screen.width ;
		float tSH = Screen.height ;
		
		ScreenMatchMode tScreenMatchMode = screenMatchMode ;
			
		if( tCamera.targetTexture != null )
		{
			// バックバッファのサイズを基準にする必要がある
			tSW = tCamera.targetTexture.width ;
			tSH = tCamera.targetTexture.height ;
		}

		float sx, sy, sw, sh ;

		// 想定アスペクト比(仮想解像度)
		float tVW = aw ;
		float tVH = ah ;

		float vw, vh ;

		// 仮想解像度での表示領域
		float tRX = dx ;
		float tRY = dy ;
		float tRW = dw ;
		float tRH = dh ;

		float rx, ry, rw, rh ;

		sx = 0 ;
		sy = 0 ;
		sw = 1 ;
		sh = 1 ;

		rx = 0 ;
		ry = 0 ;
		rw = 1 ;
		rh = 1 ;

		if( ( tSH / tSW ) >= ( tVH / tVW ) )
		{
			// 縦の方が長い(横はいっぱい表示)

//				Debug.LogWarning( "縦の方が長い" ) ;

			vw = tVW ;
			vh = tVH ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

			if( tScreenMatchMode == ScreenMatchMode.Expand )
			{
				// 常にアスペクトを維持する(Expand)　縦に隙間が出来る
				sx = 0 ;
				sw = 1 ;
				sh = tSW * vh / vw ;
				sh = sh / tSH ;
				sy = ( 1.0f - sh ) * 0.5f ;
			}
			else
			if( tScreenMatchMode == ScreenMatchMode.Width )
			{
				// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が増加
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				tVH = tVW * tSH / tSW ;
			}
			else
			if( tScreenMatchMode == ScreenMatchMode.Height )
			{
				// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が減少
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				tVW = tVH * tSW / tSH ;

				// 解像度が減少した分表示位置を移動させる
				tRX = tRX - ( ( vw - tVW ) * 0.5f ) ;
			}
		}
		else
		{
			// 横の方が長い(縦はいっぱい表示)

//				Debug.LogWarning( "横の方が長い" ) ;

			vh = tVH ;
			vw = tVW ;	// 完全に 9 : 16 にすると、フェードなどで 1 ライン余計なものが見えてしまうので、少し正方形寄りにする。

			if( tScreenMatchMode == ScreenMatchMode.Expand )
			{
				// 常にアスペクトを維持する(Expand)　横に隙間が出来る
				sy = 0 ;
				sh = 1 ;
				sw = tSH * vw / vh ;
				sw = sw / tSW ;
				sx = ( 1.0f - sw ) * 0.5f ;
			}
			else
			if( tScreenMatchMode == ScreenMatchMode.Height )
			{
				// 縦の仮想解像度を維持する(Height)　全体表示　横方向の仮想解像度が増加
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				tVW = tVH * tSW / tSH ;
			}
			else
			if( tScreenMatchMode == ScreenMatchMode.Width )
			{
				// 横の仮想解像度を維持する(Width)　全体表示　縦方向の仮想解像度が減少
				sx = 0 ;
				sw = 1 ;
				sy = 0 ;
				sh = 1 ;

				tVH = tVW * tSH / tSW ;

				// 解像度が減少した分表示位置を移動させる
				tRY = tRY - ( ( vh - tVH ) * 0.5f ) ;
			}
		}

		//----------------------------------------------------------

		if( tRW >  0 )
		{
			rx = tRX / tVW ;
			rw = tRW / tVW ;
		}
		else
		{
			// ０以下ならフル指定
			rx = 0 ;
			rw = 1 ;
		}

		if( tRH >  0 )
		{
			ry = tRY / tVH ;
			rh = tRH / tVH ;
		}
		else
		{
			// ０以下ならフル指定
			ry = 0 ;
			rh = 1 ;
		}
			
		if( rx <  0 )
		{
			rw = rw + rx ;
			rx = 0 ;
		}
		if( rx >  0 && ( rx + rw ) >  1 )
		{
			rw = rw - ( ( rx + rw ) - 1 ) ;
		}
		if( rw >  1 )
		{
			rw  = 1 ;
		}
		if( rw == 0 )
		{
			rx = 0 ;
			rw = 1 ;
		}
			
		if( ry <  0 )
		{
			rh = rh + ry ;
			ry = 0 ;
		}
		if( ry >  0 && ( ry + rh ) >  1 )
		{
			rh = rh - ( ( ry + rh ) - 1 ) ;
		}
		if( rh >  1 )
		{
			rh  = 1 ;
		}
		if( rh == 0 )
		{
			ry = 0 ;
			rh = 1 ;
		}

//			Debug.LogWarning( "sx:" + sx + " sw:" + sw + " rx:" + rx + " rw:" + rw ) ;
//			Debug.LogWarning( "sy:" + sy + " sh:" + sh + " ry:" + ry + " rh:" + rh ) ;
		tCamera.rect = new Rect( sx + rx * sw, sy + ( sh - ( ( ry + rh ) * sh ) ), rw * sw, rh * sh ) ;
	}

}
