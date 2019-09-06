using UnityEngine ;
using UnityEngine.UI ;
using System.Collections ;

namespace uGUIHelper
{
	[ RequireComponent(typeof(UnityEngine.Canvas))]
	[ RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
	[ RequireComponent(typeof(UnityEngine.UI.GraphicRaycaster))]
//	[ RequireComponent(typeof(GraphicRaycasterWrapper))]

	/// <summary>
	/// uGUI:Canvas クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UICanvas : UIView
	{
		public enum AutomaticRenderMode
		{
			None				= 0,
			ScreenSpaceOverlay	= 1,
			ScreenSpaceCamera	= 2,
			WorldSpace			= 3,
		}


		/// <summary>
		/// 実行時に簡易的にパラメータを設定する
		/// </summary>
		public AutomaticRenderMode	renderMode = AutomaticRenderMode.None ;

		/// <summary>
		/// 仮想解像度の横幅
		/// </summary>
		public float	width  = 960 ;

		/// <summary>
		/// 仮想解像度の縦幅
		/// </summary>
		public float	height = 540 ;

		/// <summary>
		/// 表示の優先順位値(大きい方が手前)
		/// </summary>
		public int		depth = 54 ;

		/// <summary>
		/// ターゲットカメラカメラ
		/// </summary>
		public Camera	renderCamera
		{
			get
			{
				return m_RenderCamera ;
			}
			set
			{
				m_RenderCamera = value ;
			}
		}

		[SerializeField]
		private Camera m_RenderCamera = null ;

		/// <summary>
		/// ＶＲモードで表示した際の基準位置からの前方への距離
		/// </summary>
		public float	vrDistance = 1.0f ;

		/// <summary>
		/// ＶＲモードで表示した際の縦の視野に対する大きさの比率
		/// </summary>
		public float	vrScale = 1.0f ;


		//-----------------------------------

		/// <summary>
		/// オーバーレイ表示を行うか
		/// </summary>
		public bool		isOverlay = false ;

		//-----------------------------------------------------
	
		/// <summary>
		/// 派生クラスの Awake
		/// </summary>
		override protected void OnAwake()
		{
			if( Application.isPlaying == true )
			{
				// 実行中のみイベントシステムを生成する
				UIEventSystem.Create() ;

				//---------------------------------------------------------

				// 実行時に簡易的に状態を設定する

				if( renderMode != AutomaticRenderMode.None )
				{
					SetRenderMode( renderMode ) ;
				}
			}
		}

		/// <summary>
		/// 表示モードを設定する
		/// </summary>
		/// <param name="tDisplayMode"></param>
		public void SetRenderMode( AutomaticRenderMode tRenderMode )
		{
			if( tRenderMode == AutomaticRenderMode.ScreenSpaceOverlay || tRenderMode == AutomaticRenderMode.ScreenSpaceCamera )
			{
				// ２Ｄモード
				if( tRenderMode == AutomaticRenderMode.ScreenSpaceOverlay )
				{
					Canvas tCanvas = _canvas ;
					if( tCanvas != null )
					{
						tCanvas.renderMode = RenderMode.ScreenSpaceOverlay ;
						tCanvas.sortingOrder = depth ;
					}
				}
				else
				if( tRenderMode == AutomaticRenderMode.ScreenSpaceCamera )
				{
					Canvas tCanvas = _canvas ;
					if( tCanvas != null )
					{
						tCanvas.renderMode = RenderMode.ScreenSpaceCamera ;
						Camera tCamera = tCanvas.worldCamera ;
						if( tCamera == null )
						{
							tCamera = m_RenderCamera ;
						}
						if( tCamera == null )
						{
							tCamera = GetComponentInChildren<Camera>() ;
						}
						if( tCamera != null )
						{
							tCanvas.worldCamera = tCamera ;
						}
						if( tCanvas.worldCamera != null )
						{
							tCanvas.worldCamera.gameObject.SetActive( true ) ;
							tCanvas.worldCamera.depth = depth ;
						}
					}
				}

				CanvasScaler tCanvasScaler = _canvasScaler ;
				if( tCanvasScaler != null )
				{
					tCanvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize ;
					tCanvasScaler.referenceResolution = new Vector2( width, height ) ;
//					tCanvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand ;
				}

				_x = 0 ;
				_y = 0 ;
				_z = 0 ;
			}
			else
			if( tRenderMode == AutomaticRenderMode.WorldSpace )
			{
				// ３Ｄモード
				Canvas tCanvas = _canvas ;
				if( tCanvas != null )
				{
					tCanvas.renderMode = RenderMode.WorldSpace ;
					Camera tCamera = tCanvas.worldCamera ;
					if( tCamera == null )
					{
						tCamera = GetComponentInChildren<Camera>() ;
						if( tCamera != null )
						{
							tCamera.gameObject.SetActive( false ) ;
						}
					}
					else
					{
						// カメラが無効でも設定されているとレイキャストに引っかからなくなるので null にする必要がある
						tCamera.gameObject.SetActive( false ) ;
//						tCamera.depth = depth ;
						tCanvas.worldCamera = null ;
					}
					tCanvas.sortingOrder = depth ;
				}

//				CanvasScaler tCanvasScaler = _canvasScaler ;
//				if( tCanvasScaler != null )
//				{
//					tCanvasScaler.dynamicPixelsPerUnit = 3 ;
//					tCanvasScaler.referencePixelsPerUnit = 1 ;
//				}

				_x = 0 ;
				_y = 0 ;
				_z = 0 ;

				_w = width ;
				_h = height ;
			}
		}

		//-----------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = null )
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				tCanvas = gameObject.AddComponent<Canvas>() ;
			}
			if( tCanvas == null )
			{
				// 異常
				return ;
			}
				
			tCanvas.renderMode = RenderMode.ScreenSpaceOverlay ;
			tCanvas.pixelPerfect = true ;
				
			tCanvas.sortingOrder = 0 ;	// 表示を強制的に更新するために初期化が必要
				
			ResetRectTransform() ;

			//------------------------------------------

			CanvasScaler tCanvasScaler = _canvasScaler ;
			if( tCanvasScaler == null )
			{
				tCanvasScaler = gameObject.AddComponent<CanvasScaler>() ;
			}
			if( tCanvasScaler == null )
			{
				// 異常
				return ;
			}

			tCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize ;
			tCanvasScaler.referenceResolution = new Vector2( 960, 640 ) ;
			tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
			tCanvasScaler.matchWidthOrHeight = 1.0f ;
			tCanvasScaler.referencePixelsPerUnit = 100.0f ;
			
			GraphicRaycasterWrapper tGraphicRaycaster = _graphicRaycaster ;
			if( tGraphicRaycaster == null )
			{
				tGraphicRaycaster = gameObject.AddComponent<GraphicRaycasterWrapper>() ;
			}
			if( tGraphicRaycaster == null )
			{
				// 異常
				return ;
			}
				
			tGraphicRaycaster.ignoreReversedGraphics = true ;
			tGraphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None ;
		}

		//-----------------------------------------------------
		
		/// <summary>
		/// 仮想解像度を設定する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void SetResolution( float w, float h )
		{
			CanvasScaler tCanvasScaler = _canvasScaler ;
			if( tCanvasScaler == null )
			{
				return ;
			}
		
			tCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize ;

			if( w <= 0 && h <= 0 )
			{
				w = Screen.width ;
				h = Screen.height ;
				tCanvasScaler.referenceResolution = new Vector2( w, h ) ;
				tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;
			}
			else
			if( w <= 0 && h >  0 )
			{
				w = h ;
				tCanvasScaler.referenceResolution = new Vector2( w, h ) ;
				tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
				tCanvasScaler.matchWidthOrHeight = 1.0f ;
			}
			else
			if( w >  0 && h <= 0 )
			{
				h = w ;
				tCanvasScaler.referenceResolution = new Vector2( w, h ) ;
				tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
				tCanvasScaler.matchWidthOrHeight = 0.0f ;
			}
			else
			{
				tCanvasScaler.referenceResolution = new Vector2( w, h ) ;
//				tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand ;
				tCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight ;
//				tCanvasScaler.matchWidthOrHeight = 1.0f ;
			}

			tCanvasScaler.referencePixelsPerUnit = 100.0f ;
		}
		
		/// <summary>
		/// ソーティングオーダーを設定する
		/// </summary>
		/// <param name="tOrder"></param>
		public void SetSortingOrder( int tOrder )
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				return ;
			}
		
			_canvas.sortingOrder = tOrder ;
		}

		/// <summary>
		/// キャンバスのカメラを取得する
		/// </summary>
		/// <param name="tCameraDepth"></param>
		public Camera GetCamera()
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				return null ;
			}

			return tCanvas.worldCamera ;
		}

		/// <summary>
		/// カメラをセットする
		/// </summary>
		/// <param name="tCamera"></param>
		/// <returns></returns>
		public bool SetCamera( Camera tCamera )
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				return false ;
			}

			tCanvas.worldCamera = tCamera ;

			return true ;
		}

		/// <summary>
		/// キャンバスのカメラ
		/// </summary>
		public Camera worldCamera
		{
			get
			{
				return GetCamera() ;
			}
			set
			{
				SetCamera( value ) ;
			}
		}
		
		/// <summary>
		/// キャンバスのカメラデプスを設定する
		/// </summary>
		/// <param name="tCameraDepth"></param>
		public void SetCameraDepth( float tCameraDepth )
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				return ;
			}

			if( _canvas.worldCamera == null )
			{
				return ;
			}

			_canvas.worldCamera.depth = tCameraDepth ;
		}

		/// <summary>
		/// キャンバスのカメラデプスを取得する
		/// </summary>
		/// <returns></returns>
		public float GetCameraDepth()
		{
			Canvas tCanvas = _canvas ;
			if( tCanvas == null )
			{
				return -1 ;
			}

			if( _canvas.worldCamera == null )
			{
				return -1 ;
			}

			return _canvas.worldCamera.depth ;
		}

		/// <summary>
		/// スクリーンマッチモードを設定する
		/// </summary>
		/// <param name="tMode"></param>
		/// <param name="tScale"></param>
		/// <returns></returns>
		public bool SetScreenMatchMode( CanvasScaler.ScreenMatchMode tMode, float tScale )
		{
			CanvasScaler tCanvasScaler = _canvasScaler ;
			if( tCanvasScaler == null )
			{
				return false ;
			}

			tCanvasScaler.screenMatchMode = tMode ;
			tCanvasScaler.matchWidthOrHeight = tScale ;
			
			return true ;
		}

		/// <summary>
		/// スクリーンに対する配置方法
		/// </summary>
		public enum ScreenMatchMode
		{
			Expand = 0,
			Width  = 1,
			Height = 2,
		}

		/// <summary>
		/// キャンバスの画面に対する表示領域を設定する
		/// </summary>
		/// <param name="aw">横方向の想定解像度</param>
		/// <param name="ah">縦方向の想定解像度</param>
		/// <param name="dx">表示領域の左上の座標Ｘ</param>
		/// <param name="dy">表示領域の左上の座標Ｙ</param>
		/// <param name="dw">表示領域の横幅の想定解像度</param>
		/// <param name="dh">表示領域の縦幅の想定解像度</param>
		public void SetViewport( float aw, float ah, float dx, float dy, float dw, float dh, ScreenMatchMode tScreenMatchMode )
		{
			Camera tCamera = worldCamera ;

			if( tCamera == null )
			{
				return ;
			}

			float tSW = Screen.width ;
			float tSH = Screen.height ;
		
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



		//-------------------------------------------------------------------------------------
	
		/// <summary>
		/// キャンバスを生成する
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas Create( float w = 0, float h = 0 )
		{
			return Create( null, w, h ) ;
		}
		
		/// <summary>
		/// キャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas Create( Transform tParent = null, float w = 0, float h = 0 )
		{
			GameObject tCanvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			if( tParent != null )
			{
				tCanvasGO.transform.SetParent( tParent, false ) ;
			}
		
			UICanvas tCanvas = tCanvasGO.AddComponent<UICanvas>() ;
			tCanvas.SetDefault() ;
			tCanvas.SetResolution( w, h ) ;
		
			tCanvasGO.transform.localPosition = new Vector3( 0, 0, -100 ) ;
			tCanvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			tCanvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			tCanvasGO.layer = 5 ;
		
			return tCanvas ;
		}
		
		/// <summary>
		/// 子にカメラを持つ形でキャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas CreateWithCamera( Transform tParent = null, float w = 0, float h = 0, int tDepth = 0 )
		{
			float rw = w ;
			float rh = h ;
			if( rw <= 0 && rh <= 0 )
			{
				rw = Screen.width ;
				rh = Screen.height ;
			}
			else
			if( rw <= 0 && rh >  0 )
			{
				rw = rh ;
			}
			else
			if( rw >  0 && rh <= 0 )
			{
				rh = rw ;
			}

			GameObject tCanvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			if( tParent != null )
			{
				tCanvasGO.transform.SetParent( tParent, false ) ;
			}

			UICanvas tCanvas = tCanvasGO.AddComponent<UICanvas>() ;
			tCanvas.SetDefault() ;
			tCanvas.SetResolution( w, h ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			tCanvasGO.layer = 5 ;
		
			//------------------------

			GameObject tCameraGO = new GameObject( "Camera" ) ;
			tCameraGO.transform.SetParent( tCanvasGO.transform, false ) ;
		
			tCameraGO.transform.localPosition = new Vector3( 0, 0, -100 ) ;
			tCameraGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			tCameraGO.transform.localScale = new Vector3( 1, 1, 1 ) ;
		
			Camera tCamera = tCameraGO.AddComponent<Camera>() ;
		
			tCamera.clearFlags = CameraClearFlags.SolidColor ;
			tCamera.backgroundColor = new Color( 0, 0, 0, 0 ) ;
		
			tCamera.orthographic = true ;
			tCamera.orthographicSize = rh * 0.5f ;
			tCamera.nearClipPlane = 0.1f ;
			tCamera.farClipPlane  = 20000.0f ;
		
			tCamera.cullingMask = 1 << 5 ;

			tCamera.depth = tDepth ;

			tCamera.stereoTargetEye = StereoTargetEyeMask.None ;

			//------------------------
		
			tCanvas._canvas.renderMode = RenderMode.ScreenSpaceCamera ;
			tCanvas._canvas.pixelPerfect = true ;
			tCanvas._canvas.worldCamera = tCamera ;
			tCanvas._canvas.planeDistance = 100 ;
		
			//----------------------------------------------------------

			tCanvasGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
			tCanvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			tCanvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			return tCanvas ;		
		}

		/// <summary>
		/// 親にカメラを持つ形でキャンバスを生成する
		/// </summary>
		/// <param name="tParent"></param>
		/// <param name="w"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		public static UICanvas CreateOnCamera( Transform tParent = null, float w = 0, float h = 0, int tDepth = 0 )
		{
			float rw = w ;
			float rh = h ;
			if( rw <= 0 && rh <= 0 )
			{
				rw = Screen.width ;
				rh = Screen.height ;
			}
			else
			if( rw <= 0 && rh >  0 )
			{
				rw = rh ;
			}
			else
			if( rw >  0 && rh <= 0 )
			{
				rh = rw ;
			}

			Camera tCamera = null ;
		
			if( tParent != null )
			{
				tCamera = tParent.GetComponent<Camera>() ;
			}

			if( tCamera == null )
			{
				GameObject tCameraGO = new GameObject( "Camera" ) ;
				if( tParent != null )
				{
					tCameraGO.transform.SetParent( tParent, false ) ;
				}
		
				tCameraGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
				tCameraGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
				tCameraGO.transform.localScale = new Vector3( 1, 1, 1 ) ;
		
				tCamera = tCameraGO.AddComponent<Camera>() ;
			
				tCamera.clearFlags = CameraClearFlags.SolidColor ;
				tCamera.backgroundColor = new Color( 0, 0, 0, 0 ) ;
		
				tCamera.orthographic = true ;
				tCamera.orthographicSize = rh * 0.5f ;
				tCamera.nearClipPlane = 0.1f ;
				tCamera.farClipPlane  = 20000.0f ;
		
				tCamera.cullingMask = 1 << 5 ;

				tCamera.depth = tDepth ;

				tCamera.stereoTargetEye = StereoTargetEyeMask.None ;
			}

			//------------------------
		
			GameObject tCanvasGO = new GameObject( "Canvas", typeof( RectTransform ) ) ;
			tCanvasGO.transform.SetParent( tCamera.transform, false ) ;
		
			UICanvas tCanvas = tCanvasGO.AddComponent<UICanvas>() ;
			tCanvas.SetDefault() ;
			tCanvas.SetResolution( w, h ) ;

			// 各 UIView は Canvas のターゲットレイヤーを元に自身のレイヤーを設定するので
			// Canvas だけは自身でレイヤーを設定しなければならない
			tCanvasGO.layer = 5 ;
		
			//------------------------

			tCanvas._canvas.renderMode = RenderMode.ScreenSpaceCamera ;
			tCanvas._canvas.pixelPerfect = true ;
			tCanvas._canvas.worldCamera = tCamera ;
			tCanvas._canvas.planeDistance = 100 ;
		
			//----------------------------------------------------------

			tCanvasGO.transform.localPosition = new Vector3( 0, 0, 0 ) ;
			tCanvasGO.transform.localEulerAngles = new Vector3( 0, 0, 0 ) ;
			tCanvasGO.transform.localScale = new Vector3( 1, 1, 1 ) ;

			return tCanvas ;
		
		}

		/// <summary>
		/// blocksRaycasts へのショートカットプロパティ
		/// </summary>
		public bool blocksRaycasts
		{
			get
			{
				CanvasGroup tCanvasGroup = _canvasGroup ;
				if( tCanvasGroup == null )
				{
					return false ;
				}

				return tCanvasGroup.blocksRaycasts ;
			}
			set
			{
				CanvasGroup tCanvasGroup = _canvasGroup ;
				if( tCanvasGroup == null )
				{
					return ;
				}

				tCanvasGroup.blocksRaycasts = value ;
			}
		}
	}
}

