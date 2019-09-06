using UnityEngine ;
using System.Collections ;

using uGUIHelper ;

namespace uGUIHelper
{
	/// <summary>
	/// ＵＩの一部として表示したいカメラを簡単につくるためのコンポーネント（クリッピングは効かない事に注意）
	/// </summary>
	public class UISpace : UIView
	{
		[SerializeField][HideInInspector]
		private Camera m_TargetCamera = null ;
		public  Camera   targetCamera
		{
			get
			{
				return m_TargetCamera ;
			}
			set
			{
				m_TargetCamera = value ;
			}
		}
		
		/// <summary>
		/// カメラデプスを設定する
		/// </summary>
		/// <param name="tDepth"></param>
		/// <returns></returns>
		public bool SetCameraDepth( int tDepth )
		{
			if( targetCamera == null )
			{
				return false ;
			}

			targetCamera.depth = tDepth ;

			return true ;
		}

		/// <summary>
		/// カリングマスクを設定する
		/// </summary>
		/// <param name="tMask"></param>
		/// <returns></returns>
		public bool SetCullingMask( int tMask )
		{
			if( targetCamera == null )
			{
				return false ;
			}

			targetCamera.cullingMask = tMask ;

			return true ;
		}

		[SerializeField][HideInInspector]
		private bool	m_RenderTextureEnabled = false ;
		public  bool	  renderTextureEnabled
		{
			get
			{
				return m_RenderTextureEnabled ;
			}
			set
			{
				if( m_RenderTextureEnabled != value )
				{
					m_RenderTextureEnabled = value ;

					if( Application.isPlaying == true )
					{
						if( value == true )
						{
							CreateRenderTexture() ;
						}
						else
						{
							DeleteRenderTexture() ;
						}
					}
				}
			}
		}

		[SerializeField][HideInInspector]
		private RenderTexture	m_RenderTexture = null ;

		/// <summary>
		/// レンダーテクスチャのインスタンス
		/// </summary>
		public RenderTexture	texture
		{
			get
			{
				return m_RenderTexture ;
			}
		}

		[SerializeField][HideInInspector]
		private UIRawImage		m_RenderImage = null ;

		/// <summary>
		/// レンダーイメージのインスタンス
		/// </summary>
		public UIRawImage image
		{
			get
			{
				return m_RenderImage ;
			}
		}


		[SerializeField][HideInInspector]
		private Vector2			m_Size ;

		[SerializeField][HideInInspector]
		private bool			m_FlexibleFieldOfView = true ;
		public  bool			  flexibleFieldOfView
		{
			get
			{
				return m_FlexibleFieldOfView ;
			}
			set
			{
				m_FlexibleFieldOfView = value ;
			}
		}

		[SerializeField][HideInInspector]
		private float			m_BasisHeight = 0 ;
		public  float			  basisHeight
		{
			get
			{
				return m_BasisHeight ;
			}
			set
			{
				m_BasisHeight = value ;
			}
		}


		/// <summary>
		/// カメラの焦点の位置
		/// </summary>
		public Vector2	cameraOffset = Vector2.zero ;


		private float m_InitialFieldOfView = 0 ;

		//-----------------------------------------------------------

		// レンダーテクスチャが有効な場合のみ使用可能なパラメータ

		public bool	imageMask		= false ;
		public bool imageInversion	= false ;
		public bool imageShadow		= false ;
		public bool imageOutline	= false ;
		public bool imageGradient	= false ;

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			ResetRectTransform() ;

			// ひとまず自身の子としてカメラを生成しておく
			GameObject tCameraGameObject = new GameObject( "Camera" ) ;
			
			tCameraGameObject.transform.localPosition = Vector3.zero ;
			tCameraGameObject.transform.localRotation = Quaternion.identity ;
			tCameraGameObject.transform.localScale = Vector3.one ;
			tCameraGameObject.transform.SetParent( transform, false ) ;

			m_TargetCamera = tCameraGameObject.AddComponent<Camera>() ;
		}


//		override protected void OnAwake()
//		{
//		}

		override protected void OnStart()
		{
			if( Application.isPlaying == true )
			{
				if( m_RenderTextureEnabled == true )
				{
					CreateRenderTexture() ;
					m_Size = size ;
				}

				if( m_TargetCamera != null )
				{
					m_InitialFieldOfView = m_TargetCamera.fieldOfView ;
				}
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			//---------------------------------------------------------

			if( m_RenderTextureEnabled == true && m_Size != size )
			{
				ResizeRenderTexture() ;
				m_Size = size ;
			}

			if( Application.isPlaying == true )
			{
				if( m_TargetCamera != null )
				{
					if( m_FlexibleFieldOfView == true && m_BasisHeight >  0 )
					{
						// 画面の縦幅に応じて３Ｄカメラの画角を調整し３Ｄの見た目の大きさを画面の縦比に追従させる
						float tFoV = m_InitialFieldOfView * 0.5f ;
						float tDistance = ( m_BasisHeight * 0.5f ) / Mathf.Tan( 2.0f * Mathf.PI * tFoV / 360.0f ) ;
						float tHeight = _h * 0.5f ;
						float tTanV = tHeight / tDistance ;
						tFoV = ( 360.0f * Mathf.Atan( tTanV ) / ( 2.0f * Mathf.PI ) ) ;
						m_TargetCamera.fieldOfView = tFoV * 2.0f ;
					}
				}
			}
		}

		private Rect m_FullRendering = new Rect( 0, 0, 1, 1 ) ;

		override protected void OnLateUpdate()
		{
			base.OnLateUpdate() ;

			// RectTransform の位置に Camera を調整する
			if( m_TargetCamera != null )
			{
				if( m_RenderTextureEnabled == false || Application.isPlaying == false )
				{
					Rect r = rectInCanvas ;

					Vector2 c = GetCanvasSize() ;

					// 画面の左下基準
					r.x = r.x + ( c.x * 0.5f ) ;
					r.y = r.y + ( c.y * 0.5f ) ;

					float w = c.x ;
					float h = c.y ;

					r.x = r.x / w ;
					r.y = r.y / h ;
					r.width  = r.width  / w ;
					r.height = r.height / h ;

					m_TargetCamera.rect = r ;
				}
				else
				{
					m_TargetCamera.rect = m_FullRendering ;
				}

				if( m_TargetCamera != null )
				{
					if( cameraOffset.x == 0 && cameraOffset.y == 0 )
					{
						m_TargetCamera.ResetProjectionMatrix() ;
//						Debug.LogWarning( "元の行列:\n" + m_TargetCamera.projectionMatrix ) ;
					}
					else
					{
						float tSW, tSH ;
						if( m_RenderTexture == null )
						{
							tSW = Screen.width ;
							tSH = Screen.height ;
						}
						else
						{
							tSW = m_RenderTexture.width ;
							tSH = m_RenderTexture.height ;
						}

						m_TargetCamera.projectionMatrix = PerspectiveOffCenter( m_TargetCamera, tSW, tSH, cameraOffset ) ;
//						Debug.LogWarning( "今の行列:\n" + m_TargetCamera.projectionMatrix ) ;
					}
				}
			}
		}

		override protected void OnDestroy()
		{
			if( m_RenderTextureEnabled == true )
			{
				DeleteRenderTexture() ;
			}
		}

		private void CreateRenderTexture()
		{
			if( m_TargetCamera == null )
			{
				return ;
			}

			m_TargetCamera.clearFlags = CameraClearFlags.SolidColor ;

			if( m_RenderTexture == null && _w >  0 && _h >  0 )
			{
				m_RenderTexture = new RenderTexture( ( int )_w, ( int )_h, 24 ) ;
				m_RenderTexture.antiAliasing = 2 ;
				m_RenderTexture.depth = 24 ;	// 24以上にしないとステンシルがサポートされない事に注意する
			}
			if( m_RenderTexture == null )
			{
				return ;
			}

			if( m_RenderImage == null )
			{
				m_RenderImage = AddView<UIRawImage>() ;
			}
			m_RenderImage.SetAnchorToStretch() ;

			m_TargetCamera.targetTexture = m_RenderTexture ;
			m_RenderImage.texture = m_RenderTexture ;

			m_RenderImage.isMask		= imageMask ;
			m_RenderImage.isInversion	= imageInversion ;
			m_RenderImage.isShadow		= imageShadow ;
			m_RenderImage.isOutline		= imageOutline ;
			m_RenderImage.isGradient	= imageGradient ;
		}

		private void DeleteRenderTexture()
		{
			if( m_TargetCamera != null )
			{
				m_TargetCamera.targetTexture = null ;
			}

			if( m_RenderImage != null )
			{
				m_RenderImage.texture = null ;
				DestroyImmediate( m_RenderImage.gameObject ) ;
				m_RenderImage = null ;
			}

			if( m_RenderTexture != null )
			{
				DestroyImmediate( m_RenderTexture ) ;
				m_RenderTexture = null ;
			}

			if( m_TargetCamera != null )
			{
				m_TargetCamera.clearFlags = CameraClearFlags.Depth ;
			}
		}

		private void ResizeRenderTexture()
		{
			if( m_TargetCamera == null || m_RenderTexture == null || m_RenderImage == null )
			{
				return ;
			}

			m_TargetCamera.targetTexture = null ;

			m_RenderImage.texture = null ;

			DestroyImmediate( m_RenderTexture ) ;
			m_RenderTexture = null ;

			if( _w >  0 && _h >  0 )
			{
				m_RenderTexture = new RenderTexture( ( int )_w, ( int )_h, 24 ) ;
				m_RenderTexture.antiAliasing = 2 ;
				m_RenderTexture.depth = 24 ;	// 24以上にしないとステンシルがサポートされない事に注意する
			}
			if( m_RenderTexture == null )
			{
				if( m_RenderImage != null )
				{
					m_RenderImage.texture = null ;
					DestroyImmediate( m_RenderImage.gameObject ) ;
					m_RenderImage = null ;
				}

				return ;
			}

			m_TargetCamera.targetTexture = m_RenderTexture ;
			m_RenderImage.texture = m_RenderTexture ;
		}

		private Matrix4x4 PerspectiveOffCenter( Camera tCamera, float tSW, float tSH, Vector2 tCameraOffset )
		{
			float tXMin = ( tCamera.rect.xMin - 0.5f ) * 2.0f ;
			float tXMax = ( tCamera.rect.xMax - 0.5f ) * 2.0f ;
			float tYMin = ( tCamera.rect.yMin - 0.5f ) * 2.0f ;
			float tYMax = ( tCamera.rect.yMax - 0.5f ) * 2.0f ;

			float tFoV  = tCamera.fieldOfView ;
			float tNear	= tCamera.nearClipPlane ;
			float tFar	= tCamera.farClipPlane ;

			//----------------------------------------------------------

			float vw = tXMax - tXMin ;
			float vh = tYMax - tYMin ;

			if( vw <= 0 || vh <= 0 )
			{
				return tCamera.projectionMatrix ;	// ０はダメよ
			}

			float sw = tSW * vw ;
			float sh = tSH * vh ;

			float ah = Mathf.Tan( 2.0f * Mathf.PI * tFoV / 360.0f ) ;
			float aw = ah * sh / sw ;

			//----------------------------------------------------------

			float x = aw ;
			float y = ah ;

			float a = ( tXMax + tXMin ) / vw ;
			float b = ( tYMax + tYMin ) / vh ;

			float c = - ( tFar + tNear ) / ( tFar - tNear ) ;
			float d = - ( 2.0f * tFar * tNear ) / ( tFar - tNear ) ;
			float e = - 1.0f ;

			Matrix4x4 m = new Matrix4x4() ;
			m[ 0, 0 ] = x ;
			m[ 0, 1 ] = 0 ;
			m[ 0, 2 ] = a ;
			m[ 0, 3 ] = tCameraOffset.x ;
			m[ 1, 0 ] = 0 ;
			m[ 1, 1 ] = y ;
			m[ 1, 2 ] = b ;
			m[ 1, 3 ] = tCameraOffset.y ;
			m[ 2, 0 ] = 0 ;
			m[ 2, 1 ] = 0 ;
			m[ 2, 2 ] = c ;
			m[ 2, 3 ] = d ;
			m[ 3, 0 ] = 0 ;
			m[ 3, 1 ] = 0 ;
			m[ 3, 2 ] = e ;
			m[ 3, 3 ] = 0 ;

			//----------------------------------------------------------

			return m ;
		}
	}
}
