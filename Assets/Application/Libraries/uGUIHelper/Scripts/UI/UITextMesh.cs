//#if TextMeshPro

using System ;
using System.Collections ;
using System.Collections.Generic ;

using UnityEngine ;
using UnityEngine.UI ;

using TMPro ;

namespace uGUIHelper
{
	[RequireComponent( typeof( TextMeshProUGUI ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UITextMesh : UIView
	{
		/// <summary>
		/// RectTransform のサイズを自動的に文字列のサイズに合わせるかどうか
		/// </summary>
		public bool autoSizeFitting = true ;

		/// <summary>
		/// 文字列自体のサイズ
		/// </summary>
		public Vector2 textSize
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return Vector2.zero ;
				}
				return new Vector2( tTextMesh.preferredWidth, tTextMesh.preferredHeight ) ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int fontSize
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return 0 ;
				}
				return ( int )tTextMesh.fontSize ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.fontSize != value )
				{
					tTextMesh.fontSize = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// テキスト(ショートカット)
		/// </summary>
		public string text
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return null ;
				}
				return tTextMesh.text ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.text != value )
				{
					tTextMesh.text = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public TMP_FontAsset font
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return null ;
				}
				return tTextMesh.font ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.font != value )
				{
					tTextMesh.font = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// カラー(ショートカット)
		/// </summary>
		public Color color
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return Color.white ;
				}
				return tTextMesh.color ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.color.r != value.r || tTextMesh.color.g != value.g || tTextMesh.color.b != value.b || tTextMesh.color.a != value.a )
				{
					tTextMesh.color = value ;
				}
			}
		}
		
		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyles fontStyle
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return FontStyles.Normal ;
				}
				return tTextMesh.fontStyle ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.fontStyle != value )
				{
					tTextMesh.fontStyle = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// リッチテキスト(ショートカット)
		/// </summary>
		public bool supportRichText
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return false ;
				}
				return tTextMesh.richText ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.richText != value )
				{
					tTextMesh.richText = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAlignmentOptions alignment
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return TextAlignmentOptions.Center ;
				}
				return tTextMesh.alignment ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.alignment != value )
				{
					tTextMesh.alignment = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// リサイズ(ショートカット)
		/// </summary>
		public bool resizeTextForBestFit
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return false ;
				}
				return tTextMesh.enableAutoSizing ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.enableAutoSizing != value )
				{
					tTextMesh.enableAutoSizing = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  bool horizontalOverflow
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return false ;
				}
				return ( ! tTextMesh.enableWordWrapping ) ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.enableWordWrapping != ( ! value ) )
				{
					tTextMesh.enableWordWrapping = ( ! value ) ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  TextOverflowModes verticalOverflow
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return TextOverflowModes.Overflow ;
				}
				return tTextMesh.overflowMode ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.overflowMode != value )
				{
					tTextMesh.overflowMode = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}
				}
			}
		}

		/// <summary>
		/// 改行時の下方向への移動量係数(ショートカット)
		/// </summary>
		public  float lineSpacing
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return 1.0f ;
				}
				return tTextMesh.lineSpacing ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}

				if( tTextMesh.lineSpacing != value )
				{
					tTextMesh.lineSpacing = value ;

					if( autoSizeFitting == true )
					{
						Resize() ;
					}

					// 加工用に頂点情報をバックアップする
//					SetTextInfo() ;
				}
			}
		}

		/// <summary>
		/// レイキャストターゲット(ショートカット)
		/// </summary>
		public bool raycastTarget
		{
			get
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return false ;
				}
				return tTextMesh.raycastTarget ;
			}
			set
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				if( tTextMesh == null )
				{
					return ;
				}
				tTextMesh.raycastTarget = value ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 装飾の一部を変更するか
		/// </summary>
		public bool	isCustomized = false ;

		/// <summary>
		/// アウトラインの色(実行時にのみ反映)
		/// </summary>
		public Color	outlineColor = Color.black ;

		private Material	m_BasisMaterial = null ;
		private Material	m_CloneMaterial = null ;

		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params uint[] tCode )
		{
			if( tCode == null || tCode.Length == 0 )
			{
				return ;
			}

			int i, l = tCode.Length ;

			Color[] tColor = new Color[ l ] ;
			uint c ;
			byte r, g, b ,a ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				c = tCode[ i ] ;

				r = ( byte )( ( c >> 16 ) & 0xFF ) ;
				g = ( byte )( ( c >>  8 ) & 0xFF ) ;
				b = ( byte )(   c         & 0xFF ) ;
				a = ( byte )( ( c >> 24 ) & 0xFF ) ;

				tColor[ i ] = new Color32(  r, g, b, a ) ;
			}

			SetGradientColor( tColor ) ;
		}
		
		/// <summary>
		/// グラデーションカラー(ショートカット)
		/// </summary>
		public void SetGradientColor( params Color[] tColor )
		{
			TextMeshProUGUI tTextMesh = _textMesh ;
			if( tTextMesh == null )
			{
				return ;
			}

			if( tColor == null || tColor.Length == 0 )
			{
				return ;
			}

			int l = tColor.Length ;

			if( l == 1 )
			{
				// 単色

				VertexGradient vg =	new VertexGradient() ;
					
				vg.topLeft		= tColor[ 0 ] ;
				vg.topRight		= tColor[ 0 ] ;
				vg.bottomLeft	= tColor[ 0 ] ;
				vg.bottomRight	= tColor[ 0 ] ;

				tTextMesh.colorGradient = vg ;
			}
			else
			if( l == 2 || l == 3 )
			{
				// 上下
				VertexGradient vg =	new VertexGradient() ;
					
				vg.topLeft		= tColor[ 0 ] ;
				vg.topRight		= tColor[ 0 ] ;
				vg.bottomLeft	= tColor[ 1 ] ;
				vg.bottomRight	= tColor[ 1 ] ;

				tTextMesh.colorGradient = vg ;
			}
			else
			if( l >= 4 )
			{
				// 全体
				VertexGradient vg =	new VertexGradient() ;
					
				vg.topLeft		= tColor[ 0 ] ;
				vg.topRight		= tColor[ 1 ] ;
				vg.bottomLeft	= tColor[ 2 ] ;
				vg.bottomRight	= tColor[ 3 ] ;

				tTextMesh.colorGradient = vg ;
			}
		}

		//-----------------------------------------------------------


		//--------------------------------------------------
	
		// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		override protected void OnBuild( string tOption = "" )
		{
			TextMeshProUGUI tTextMesh = _textMesh ;
			if( tTextMesh == null )
			{
				tTextMesh = gameObject.AddComponent<TextMeshProUGUI>() ;
			}
			if( tTextMesh == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	tDefaultTextColor = Color.white ;

			TMP_FontAsset	tDefaultFontAsset = null ;
			Material		tDefaultFontMaterial = null ;
			int				tDefaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultTextColor		= tDS.textColor ;

					tDefaultFontAsset		= tDS.fontAsset ;
					tDefaultFontMaterial	= tDS.fontMaterial ; 
					tDefaultFontSize		= tDS.fontSize ;
				}
			}
			
#endif

			tTextMesh.color = tDefaultTextColor ;

			if( tDefaultFontAsset == null )
			{
				tTextMesh.font = Resources.Load<TMP_FontAsset>( "Fonts & Materials/LiberationSans SDF" ) ;
			}
			else
			{
				tTextMesh.font = tDefaultFontAsset ;
			}

			if( tDefaultFontMaterial != null )
			{
				tTextMesh.fontMaterial = tDefaultFontMaterial ; 
			}

			if( tDefaultFontSize <= 0 )
			{
				tTextMesh.fontSize = 32 ;
			}
			else
			{
				tTextMesh.fontSize = tDefaultFontSize ;
			}

			tTextMesh.alignment = TextAlignmentOptions.TopLeft ;

			tTextMesh.enableWordWrapping = false ;
			tTextMesh.overflowMode = TextOverflowModes.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			tTextMesh.raycastTarget = false ;

		}

/*		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;

				if( textMesh != null )
				{
					textMesh.RegisterDirtyVerticesCallback( OnDirtyVertices ) ;
				}
			}
		}*/


		override protected void OnStart()
		{
			base.OnStart() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;

				if( textMesh != null )
				{
					if( isCustomized == true )
					{
						if( textMesh.material != null )
						{
							m_BasisMaterial = textMesh.fontMaterial ;
							m_CloneMaterial = GameObject.Instantiate<Material>( textMesh.fontMaterial ) ;
							textMesh.fontMaterial = m_CloneMaterial ;

							UpdateCustom( textMesh ) ;
						}
					}
				}
			}
		}

		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				TextMeshProUGUI textMesh = _textMesh ;
				if( textMesh != null )
				{
					if( isCustomized == true )
					{
						if( textMesh.material != null )
						{
							UpdateCustom( textMesh ) ;
						}
					}
				}
			}
		}

		private void UpdateCustom( TextMeshProUGUI textMesh )
		{
			if( m_CloneMaterial != null )
			{
				if( m_CloneMaterial.HasProperty( "_OutlineColor" ) == true )
				{
					m_CloneMaterial.SetColor(	"_OutlineColor",	outlineColor ) ;

					textMesh.SetMaterialDirty() ;
				}
			}
		}


		override protected void OnDestroy()
		{
			base.OnDestroy() ;

			if( m_CloneMaterial != null )
			{
				TextMeshProUGUI tTextMesh = _textMesh ;
				tTextMesh.fontMaterial = m_BasisMaterial ;

				DestroyImmediate( m_CloneMaterial ) ;
				m_CloneMaterial = null ;

				m_BasisMaterial = null ;
			}
		}

		override protected void OnLateUpdate()
		{
//			ProcessVertexModifier() ;
			
			if( autoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
			TextMeshProUGUI t = _textMesh ;
			RectTransform r = _rectTransform ;
			if( r != null && t != null )
			{
				Vector2 tSize = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.enableWordWrapping == false )
					{
						tSize.x = t.preferredWidth ;
					}
				}

				if( r.anchorMin.y == r.anchorMax.y )
				{
					if( t.overflowMode == TextOverflowModes.Overflow )
					{
						tSize.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = tSize ;
			}
		}


		//---------------------------------------------------------------------------

		// ルビフリ方法
		// http://baba-s.hatenablog.com/entry/2019/01/10/122500

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color32[] colors, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || colors == null || colors.Length == 0 )
			{
				return false ;
			}

			textMesh.ForceMeshUpdate() ;

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;
			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}

			//--------------

			if( length <= 0 )
			{
				length  = colors.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= colors.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  colors.Length )
			{
				length = colors.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex = 0 ;

			Color32[] baseColors ;

			Color32 replaceColor ;

			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					if( textInfo.characterInfo[ p ].isVisible == true )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex	= textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						replaceColor = new Color32
						(
							colors[ p ].r,
							colors[ p ].g,
							colors[ p ].b,
							colors[ p ].a
						) ;

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			//----------------------------------------------------------
			
			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Colors32 ) ;

			return true ;
		}

		/// <summary>
		/// 文字単位で色を設定する
		/// </summary>
		/// <param name="colors"></param>
		public bool SetColors( Color[] colors )
		{
			if( colors == null || colors.Length == 0 )
			{
				return false ;
			}

			int i, l = colors.Length ;
			Color32[] colors32 = new Color32[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				colors32[ i ] = colors[ i ] ;
			}

			return SetColors( colors32 ) ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// 文字の表示状態を変更する情報体
		/// </summary>
		public class Modifier
		{
			public Vector2	position = Vector2.zero ;
			public float	rotation = 0 ;
			public Vector2	scale = Vector2.one ;

			public float	gamma = 1 ;
			public float	alpha = 1 ;
		}

		/// <summary>
		/// 頂点を変化させる
		/// </summary>
		/// <param name="modifiers"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public bool SetModifiers( Modifier[] modifiers, int offset = 0, int length = 0 )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || modifiers == null || modifiers.Length == 0 )
			{
				return false ;
			}

			// メッシュ情報を一新する
			textMesh.ForceMeshUpdate() ;

			//----------------------------------------------------------

			TMP_TextInfo textInfo = textMesh.textInfo ;

			if( textInfo == null || textInfo.characterCount == 0 )
			{
				// 文字が存在しない
				return  true ;
			}
			
			//--------------

			if( length <= 0 )
			{
				length  = modifiers.Length ;
			}

			if( offset <  0 )
			{
				length += offset ;
				offset  = 0 ;
			}

			if( length <= 0 || offset >= modifiers.Length || offset >= textInfo.characterCount )
			{
				// 結果として変化は無い
				return true ;
			}

			if( ( offset + length ) >  modifiers.Length )
			{
				length = modifiers.Length - offset ;
			}

			if( ( offset + length ) >  textInfo.characterCount )
			{
				length = textInfo.characterCount - offset ;
			}

			//----------------------------------------------------------

			int vertexIndex ;
			int materialIndex = 0 ;

			Vector3[] baseVertices ;
			Color32[] baseColors ;

			Modifier modifier ;

			Vector3[]	replaceVertices ;
			Vector2		center ;

			Color32		replaceColor ;

			float dx, dy, vx, vy ;
			float cv, sv ;
			
			int i, j, p ;

			for( i  = 0 ; i <  length ; i ++ )
			{
				p = offset + i ;

				if( textInfo.characterInfo[ p ].isVisible == true )
				{
					modifier = modifiers[ p ] ;

					if( modifier != null )
					{
						vertexIndex		= textInfo.characterInfo[ p ].vertexIndex ;

						materialIndex = textInfo.characterInfo[ p ].materialReferenceIndex ;

						baseVertices	= textInfo.meshInfo[ materialIndex ].vertices ;
						baseColors		= textInfo.meshInfo[ materialIndex ].colors32 ;

						// Vertices
						replaceVertices = new Vector3[ 4 ] ;
						center = Vector2.zero ;
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Vertex
							replaceVertices[ j ] = new Vector3
							(
								baseVertices[ vertexIndex + j ].x,
								baseVertices[ vertexIndex + j ].y,
								baseVertices[ vertexIndex + j ].z
							) ;

							center.x += replaceVertices[ j ].x ;
							center.y += replaceVertices[ j ].y ;
						}

						center.x = center.x / 4f ; 
						center.y = center.y / 4f ;

						// 回転→拡縮→移動
						cv = Mathf.Cos( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						sv = Mathf.Sin( 2.0f * Mathf.PI * modifier.rotation / 360f ) ;
						
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// 回転
							dx = replaceVertices[ j ].x - center.x ;
							dy = replaceVertices[ j ].y - center.y ;

							vx = ( dx * cv ) - ( dy * sv ) ;
							vy = ( dx * sv ) + ( dy * cv ) ;

							// 拡縮
							vx *= modifier.scale.x ;
							vy *= modifier.scale.y ;

							// 移動
							vx += modifier.position.x ;
							vy += modifier.position.y ;

							replaceVertices[ j ].x = vx + center.x ;
							replaceVertices[ j ].y = vy + center.y ;
						}

						for( j  = 0 ; j <  4 ; j ++ )
						{
							baseVertices[ vertexIndex + j ] = replaceVertices[ j ] ;
						}

						// Color
						for( j  = 0 ; j <  4 ; j ++ )
						{
							// Color
							replaceColor = new Color32
							(
								baseColors[ vertexIndex + j ].r,
								baseColors[ vertexIndex + j ].g,
								baseColors[ vertexIndex + j ].b,
								baseColors[ vertexIndex + j ].a
							) ;

							if( modifier.gamma != 1 )
							{
								replaceColor.r = ( byte )Math.Round( ( float )replaceColor.r * modifier.gamma ) ;
								replaceColor.g = ( byte )Math.Round( ( float )replaceColor.g * modifier.gamma ) ;
								replaceColor.b = ( byte )Math.Round( ( float )replaceColor.b * modifier.gamma ) ;
							}

							if( modifier.alpha != 1 )
							{
								replaceColor.a = ( byte )Math.Round( ( float )replaceColor.a * modifier.alpha ) ;
							}

							baseColors[ vertexIndex + j ] = replaceColor ;
						}
					}
				}
			}

			textMesh.UpdateVertexData( TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32 ) ;

			//----------------------------------------------------------

			return true ;
		}

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// １文字ずつ表示する
		/// </summary>
		/// <param name="message"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		public UIView.MovableState Play( string message, float duration = 0.1f, int fadeWidth = 2, Func<bool> onFinished = null )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			if( textMesh == null || string.IsNullOrEmpty( message ) == true )
			{
				return null ;
			}

			if( gameObject.activeSelf == false )
			{
				gameObject.SetActive( true ) ;
			}

			if( gameObject.activeInHierarchy == false )
			{
				// 親以上がアクティブになっていないので再生できない
				Debug.LogWarning( "Parent is not active" ) ;
				return null ;
			}

			UIView.MovableState state = new MovableState() ;
			StartCoroutine( Play_Private( message, duration, fadeWidth, onFinished, state ) ) ;
			return state ;
		}

		private IEnumerator Play_Private( string message, float duration, int fadeWidth, Func<bool> onFinished, UIView.MovableState state )
		{
			TextMeshProUGUI textMesh = _textMesh ;

			textMesh.text = message ;

			int i, j, l = message.Length ;
			float t, f ;

			Modifier[] m = new Modifier[ l ] ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				m[ i ] = new Modifier() ;
				m[ i ].alpha = 0 ;
			}

			SetModifiers( m ) ;

			//----------------------------------

			if( duration <= 0 )
			{
				duration  = 0.05f ;
			}

			if( fadeWidth <= 0 )
			{
				// アルファフェードインを使わずに１文字ずつ表示する(時間は0.1)
				t = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					while( t <  duration )
					{
						t += Time.unscaledDeltaTime ;

						if( t >= duration )
						{
							m[ i ].alpha = 1 ;
						}

						SetModifiers( m, i, l - i ) ;

						if( t >= duration )
						{
							t -= duration ;
							break ;
						}

						if( onFinished != null )
						{
							if( onFinished() == true )
							{
								break ;	// 強制終了
							}
						}

						yield return null ;
					}
				}
			}
			else
			{
				// アルファフェードインを使い１文字ずつ表示する

				float wait = duration * fadeWidth ;

				t = 0 ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					while( t <  wait )
					{
						t += Time.unscaledDeltaTime ;

						for( j  = 0 ; j <  fadeWidth ; j ++ )
						{
							f = t - ( duration * j ) ;

							if( f >= wait )
							{
								f  = wait ;
							}
							if( f <  0 )
							{
								f  = 0 ;
							}

							if( ( ( i + j ) <  l ) && f >  0 )
							{
								m[ i + j ].alpha = ( f / wait ) ;
							}
						}
	
						SetModifiers( m, i, l - i ) ;

						if( t >= wait )
						{
							t -= duration ;

							break ;
						}

						if( onFinished != null )
						{
							if( onFinished() == true )
							{
								break ;	// 強制終了
							}
						}
						
						yield return null ;
					}
				}
			}

			// 元の状態に戻す
			textMesh.ForceMeshUpdate() ;

			state.isDone = true ;
		}
	}
}


//#endif