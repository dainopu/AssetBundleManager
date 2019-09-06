using UnityEngine ;
using UnityEngine.UI ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	/// <summary>
	/// 拡張:イメージナンバー
	/// </summary>
	public class ImageNumber : MaskableGraphicWrapper
	{
		[SerializeField][HideInInspector]
		private UIAtlasSprite m_AtlasSprite = null ;

		private Texture2D m_BlankTexture = null ;

		public  UIAtlasSprite  atlasSprite
		{
			get
			{
				return m_AtlasSprite ;
			}
			set
			{
				if( m_AtlasSprite != value )
				{
					m_AtlasSprite  = value ;

					Texture2D tTexture = null ;
					if( m_AtlasSprite != null )
					{
						tTexture = m_AtlasSprite.texture ;
					}

					// テクスチャを更新する
					if( tTexture != null )
					{
						_CanvasRenderer.SetTexture( tTexture ) ;
					}
					else
					{
						if( m_BlankTexture == null )
						{
							m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
						}
						_CanvasRenderer.SetTexture( m_BlankTexture ) ;
					}

					m_CodeSprite = null ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		
		[SerializeField][HideInInspector]
		private Vector2 m_CodeScale = Vector2.one ;

		/// <summary>
		/// 数値の大きさ
		/// </summary>
		public  Vector2  codeScale
		{
			get
			{
				return m_CodeScale ;
			}
			set
			{
				if( m_CodeScale != value )
				{
					m_CodeScale  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}


		
		[SerializeField][HideInInspector]
		private float m_CodeSpace = 0 ;

		/// <summary>
		/// 数値間の空き
		/// </summary>
		public  float  codeSpace
		{
			get
			{
				return m_CodeSpace ;
			}
			set
			{
				if( m_CodeSpace != value )
				{
					m_CodeSpace  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// 文字単位の操作用パラメータ
		/// </summary>
		public Vector3[] codeOffset = new Vector3[]
		{
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 ),
			new Vector3( 0, 0, 1 )
		} ;

		public void SetCodeOffset( Vector3[] tCodeOffset )
		{
			codeOffset = tCodeOffset ;
			UpdateSize() ;	// 実サイズだけ先に更新する
			SetVerticesDirty() ;
		}



		[SerializeField][HideInInspector]
		private TextAnchor m_Alignment = TextAnchor.MiddleLeft ;

		/// <summary>
		/// 寄せる位置
		/// </summary>
		public  TextAnchor  alignment
		{
			get
			{
				return m_Alignment ;
			}
			set
			{
				if( m_Alignment != value )
				{
					m_Alignment  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		// 整数部の桁数（０でまた指定桁より大きい場合は任意）
		[HideInInspector][SerializeField]
		protected int m_DigitInteger = 0 ;

		/// <summary>
		/// 整数部の桁数
		/// </summary>
		public    int  digitInteger
		{
			get
			{
				return m_DigitInteger ;
			}
			set
			{
				if( m_DigitInteger != value )
				{
					m_DigitInteger = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 整数部の桁数を設定する
		/// </summary>
		/// <param name="tDigitInteger">整数部の桁数</param>
		public void SetDigitInteger( int tDigitInteger )
		{
			digitInteger = tDigitInteger ;
		}
	
		// 小数部の桁数
		[HideInInspector][SerializeField]
		protected int m_DigitDecimal = 0 ;

		/// <summary>
		/// 小数部の桁数
		/// </summary>
		public    int  digitDecimal
		{
			get
			{
				return m_DigitDecimal ;
			}
			set
			{
				if( m_DigitDecimal != value )
				{
					m_DigitDecimal = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// 小数部の桁数を設定する
		/// </summary>
		/// <param name="tDigitDecimal">小数部の桁数</param>
		public void SetDigitDecimal( int tDigitDecimal )
		{
			digitDecimal = tDigitDecimal ;
		}

		/// <summary>
		/// 整数部と小数部の桁数を設定する
		/// </summary>
		/// <param name="tInteger">整数部の桁数</param>
		/// <param name="tDecimal">小数部の桁数</param>
		public void SetDigit( int tInteger, int tDecimal )
		{
			digitInteger = tInteger ;
			digitDecimal = tDecimal ;
		}

		// 指定の桁ごとにカンマを表示するか
		[HideInInspector][SerializeField]
		protected int m_Comma  = 0 ;

		/// <summary>
		/// カンマを挿入する桁数
		/// </summary>
		public    int  comma
		{
			get
			{
				return m_Comma ;
			}
			set
			{
				if( m_Comma != value )
				{
					m_Comma = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
		
		/// <summary>
		/// カンマを挿入する桁数を設定する
		/// </summary>
		/// <param name="tComma">カンマを挿入する桁数</param>
		public void SetComma( int tComma )
		{
			comma = tComma ;
		}
	
		// プラス符号を表示するか
		[HideInInspector][SerializeField]
		protected bool m_PlusSign  = false ;

		/// <summary>
		/// プラス符号を表示するかどうか
		/// </summary>
		public    bool  plusSign
		{
			get
			{
				return m_PlusSign ;
			}
			set
			{
				if( m_PlusSign != value )
				{
					m_PlusSign = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// プラス符号を表示するかどうかを設定する
		/// </summary>
		/// <param name="tPlusSign">表示状態(true=表示する・false=表示しない)</param>
		public void SetPlusSign( bool tPlusSign )
		{
			plusSign = tPlusSign ;
		}
		
		// ゼロ符号を表示するか
		[HideInInspector][SerializeField]
		protected bool m_ZeroSign  = false ;

		/// <summary>
		/// ゼロ符号を表示するかどうか
		/// </summary>
		public    bool  zeroSign
		{
			get
			{
				return m_ZeroSign ;
			}
			set
			{
				if( m_ZeroSign != value )
				{
					m_ZeroSign = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		/// <summary>
		/// ゼロ符号を表示するかどうかを設定する
		/// </summary>
		/// <param name="tZeroSign">表示状態(true=表示する・false=表示しない)</param>
		public void SetZeroSign( bool tZeroSign )
		{
			zeroSign = tZeroSign ;
		}

		// 桁が足りない場合は０かスペースか
		[HideInInspector][SerializeField]
		protected bool m_ZeroPadding = false ;

		/// <summary>
		/// ゼロ埋めを行うか
		/// </summary>
		public    bool  zeroPadding
		{
			get
			{
				return m_ZeroPadding ;
			}
			set
			{
				if( m_ZeroPadding != value )
				{
					m_ZeroPadding  = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}
	
		// パーセンテージを表示するか
		[HideInInspector][SerializeField]
		protected bool m_Percent  = false ;

		/// <summary>
		/// パーセンテージ記号を表示するかどうか
		/// </summary>
		public    bool  percent
		{
			get
			{
				return m_Percent ;
			}
			set
			{
				if( m_Percent != value )
				{
					m_Percent = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		/// <summary>
		/// パーセンテージ記号を表示するかどうかを設定する
		/// </summary>
		/// <param name="tPercent">表示状態(true=表示する・false=表示しない)</param>
		public void SetPercent( bool tPercent )
		{
			percent = tPercent ;
		}
	



		[SerializeField][HideInInspector]
		private int m_Value ;

		/// <summary>
		/// 値
		/// </summary>
		public  int  value
		{
			get
			{
				return m_Value ;
			}
			set
			{
				if( m_Value != value )
				{
					m_Value = value ;
					UpdateSize() ;	// 実サイズだけ先に更新する
					SetVerticesDirty() ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredWidth = 0 ;

		/// <summary>
		/// 実際の横幅
		/// </summary>
		public float   preferredWidth
		{
			get
			{
				return m_PreferredWidth ;
			}
		}

		[SerializeField][HideInInspector]
		private float m_PreferredHeight = 0 ;
		
		/// <summary>
		/// 実際の縦幅
		/// </summary>
		public  float  preferredHeight
		{
			get
			{
				return m_PreferredHeight ;
			}
		}

		//----------------------------------------------------------

		Dictionary<char,Sprite> m_CodeSprite = null ;

		private void UpdateCodeSprite()
		{
			if( m_AtlasSprite == null || m_AtlasSprite.texture == null )
			{
				return ;
			}

			string[] tList = m_AtlasSprite.GetNameList() ;
			if( tList == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				m_CodeSprite = new Dictionary<char,Sprite>() ;
			}
			else
			{
				m_CodeSprite.Clear() ;
			}
			
			int i, j, l ;
			char c ;
			for( i  = 0 ; i <= 9 ; i ++ )
			{
				c = ( char )( ( int )'0' + i ) ;
				for( j  = 0 ; j <  tList.Length ; j ++ )
				{
					if( tList[ j ].IndexOf( c ) >= 0 )
					{
						m_CodeSprite.Add( ( char )( '0' + i ), m_AtlasSprite[ tList[ j ] ] ) ;
						break ;
					}
				}
			}

			l = tList.Length ;
			string tName ;
			for( i  = 0 ; i <  l ; i ++ )
			{
				tName = tList[ i ].ToLower() ;
				if( tName.Contains( "dot" ) == true && m_CodeSprite.ContainsKey( '.' ) == false )
				{
					m_CodeSprite.Add( '.', m_AtlasSprite[ tList[ i ] ] ) ;
				}
				else
				if( tName.Contains( "comma" ) == true && m_CodeSprite.ContainsKey( ',' ) == false )
				{
					m_CodeSprite.Add( ',', m_AtlasSprite[ tList[ i ] ] ) ;
				}
				else
				if( tName.Contains( "plus" ) == true && m_CodeSprite.ContainsKey( '+' ) == false )
				{
					m_CodeSprite.Add( '+', m_AtlasSprite[ tList[ i ] ] ) ;
				}
				else
				if( tName.Contains( "minus" ) == true && m_CodeSprite.ContainsKey( '-' ) == false )
				{
					m_CodeSprite.Add( '-', m_AtlasSprite[ tList[ i ] ] ) ;
				}
				else
				if( tName.Contains( "percent" ) == true && m_CodeSprite.ContainsKey( '%' ) == false )
				{
					m_CodeSprite.Add( '%', m_AtlasSprite[ tList[ i ] ] ) ;
				}
				else
				if( tName.Contains( "space" ) == true && m_CodeSprite.ContainsKey( ' ' ) == false )
				{
					m_CodeSprite.Add( ' ', m_AtlasSprite[ tList[ i ] ] ) ;
				}
			}
		}

		protected override void Awake()
		{
			base.Awake() ;

			UpdateCodeSprite() ;
		}

		// メッシュ更新
		protected override void OnPopulateMesh( VertexHelper tHelper )
		{
			if( IsActive() == false )
			{
				return ;
			}

			tHelper.Clear() ;

			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------

			if( m_AtlasSprite == null || m_AtlasSprite.texture == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				UpdateCodeSprite() ;
			}

			if( m_CodeSprite == null || m_CodeSprite.Count == 0 )
			{
				return ;
			}

			//----------------------------------------------

			List<UIVertex>	aV = new List<UIVertex>() ;
			List<int>		aI = new List<int>() ;

			// 値を文字列化する
			string vs = GetNumberString() ;
			int i, l = vs.Length ;
			
			float w = size.x ;
			float h = size.y ;

			Vector2 tPivot = _RectTransform.pivot ;

			float xMin = - ( w * tPivot.x ) ;
			float xMax = w + xMin ;
			float xCenter = ( xMin + xMax ) * 0.5f ;

			float yMin = - ( h * tPivot.y ) ;
			float yMax = h + yMin ;
			float yCenter = ( yMin + yMax ) * 0.5f ;

			int tw = m_AtlasSprite.texture.width ;
			int th = m_AtlasSprite.texture.height ;

			Rect r ;
			float vw, vh ;
			float fw = 0 ;

			// まずはトータルの横幅を計算する

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					r = m_CodeSprite[ vs[ i ] ].textureRect ;
					
					vw = r.width * m_CodeScale.x ;
					fw = fw + vw ;

					if( i <  ( l - 1 ) )
					{
						fw = fw + codeSpace ;
					}
				}
			}

			float sx = 0, sy = 0 ;

			if( m_Alignment == TextAnchor.UpperLeft || m_Alignment == TextAnchor.MiddleLeft || m_Alignment == TextAnchor.LowerLeft )
			{
				// 横は左寄
				sx = xMin ;
			}
			else
			if( m_Alignment == TextAnchor.UpperRight || m_Alignment == TextAnchor.MiddleRight || m_Alignment == TextAnchor.LowerRight )
			{
				// 横は右寄
				sx = xMax - fw ;
			}
			else
			if( m_Alignment == TextAnchor.MiddleCenter || m_Alignment == TextAnchor.UpperCenter || m_Alignment == TextAnchor.LowerCenter )
			{
				// 横は中央
				sx = xCenter - ( fw * 0.5f ) ;
			}

			UIVertex vd ;

			int o = 0 ;
			float ox, oy, oa ;


			float fh = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					// 大元のサイズを取得する
					r = m_CodeSprite[ vs[ i ] ].textureRect ;

					vw = r.width  * m_CodeScale.x ;
					vh = r.height * m_CodeScale.y ;

					if( vh >  fh )
					{
						fh  = vh ;
					}

					ox = 0 ;
					oy = 0 ;
					oa = 1 ;
					if( codeOffset != null && i <  codeOffset.Length )
					{
						ox = codeOffset[ i ].x ;
						oy = codeOffset[ i ].y ;
						oa = codeOffset[ i ].z ;
					}

					sy = 0 ;
					if( m_Alignment == TextAnchor.UpperLeft || m_Alignment == TextAnchor.UpperCenter || m_Alignment == TextAnchor.UpperRight )
					{
						// 縦は上寄
						sy = yMax ;
					}
					else
					if( m_Alignment == TextAnchor.LowerLeft || m_Alignment == TextAnchor.LowerCenter || m_Alignment == TextAnchor.LowerRight )
					{
						// 縦は下寄
						sy = yMin + vh ;
					}
					else
					if( m_Alignment == TextAnchor.MiddleLeft || m_Alignment == TextAnchor.MiddleCenter || m_Alignment == TextAnchor.MiddleRight )
					{
						// 縦は中央
						sy = yCenter + ( vh * 0.5f ) ;
					}

					vd = new UIVertex() ;

					// 0
					vd.position = new Vector3( sx + ox, sy + oy, 0 ) ;
					vd.normal   = new Vector3( 0, 0, -1 ) ;
					vd.color    = new Color( 1, 1, 1, oa ) ;
					vd.uv0      = new Vector2( r.x / tw, ( r.y + r.height ) / th ) ;
					aV.Add( vd ) ;

					// 1
					vd.position = new Vector3( sx + vw + ox, sy + oy, 0 ) ;
					vd.normal   = new Vector3( 0, 0, -1 ) ;
					vd.color    = new Color( 1, 1, 1, oa ) ;
					vd.uv0      = new Vector2( ( r.x + r.width ) / tw, ( r.y + r.height ) / th ) ;
					aV.Add( vd ) ;

					// 2
					vd.position = new Vector3( sx + vw + ox, sy - vh + oy, 0 ) ;
					vd.normal   = new Vector3( 0, 0, -1 ) ;
					vd.color    = new Color( 1, 1, 1, oa ) ;
					vd.uv0      = new Vector2( ( r.x + r.width ) / tw, r.y / th ) ;
					aV.Add( vd ) ;

					// 3
					vd.position = new Vector3( sx + ox, sy - vh + oy, 0 ) ;
					vd.normal   = new Vector3( 0, 0, -1 ) ;
					vd.color    = new Color( 1, 1, 1, oa ) ;
					vd.uv0      = new Vector2( r.x / tw, r.y / th ) ;
					aV.Add( vd ) ;

					aI.Add( o + 0 ) ;
					aI.Add( o + 1 ) ;
					aI.Add( o + 3 ) ;

					aI.Add( o + 1 ) ;
					aI.Add( o + 2 ) ;
					aI.Add( o + 3 ) ;
				
					o = o + 4 ;
								
					sx = sx + vw ;
					if( i <  ( l - 1 ) )
					{
						sx = sx + codeSpace ;
					}
				}
			}

			// 実際のサイズを保存する
			m_PreferredWidth  = fw ;
			m_PreferredHeight = fh ; 

			if( aV.Count >  0 && ( aV.Count % 4 ) == 0 )
			{
				tHelper.AddUIVertexStream( aV, aI ) ;
			}
		}

		// マテリアル更新
		protected override void UpdateMaterial()
		{
			base.UpdateMaterial() ;

			// テクスチャを更新する
			Texture2D tTexture = null ;
			if( m_AtlasSprite != null )
			{
				tTexture = m_AtlasSprite.texture ;
			}

			if( tTexture != null )
			{
				_CanvasRenderer.SetTexture( tTexture ) ;
			}
			else
			{
				if( m_BlankTexture == null )
				{
					m_BlankTexture = Resources.Load<Texture2D>( "uGUIHelper/Textures/UIBlank" ) ;
				}
				_CanvasRenderer.SetTexture( m_BlankTexture ) ;
			}
		}

		//-------------------------------------------------------------------------------------------

		// メッシュ更新
		private void UpdateSize()
		{
			m_PreferredWidth  = 0 ;
			m_PreferredHeight = 0 ;

			//-----------------------------------------

			if( m_AtlasSprite == null || m_AtlasSprite.texture == null )
			{
				return ;
			}

			if( m_CodeSprite == null )
			{
				UpdateCodeSprite() ;
			}

			if( m_CodeSprite == null || m_CodeSprite.Count == 0 )
			{
				return ;
			}

			//----------------------------------------------

			// 値を文字列化する
			string vs = GetNumberString() ;
			int i, l = vs.Length ;
			
			Rect r ;
			float vw, vh ;
			float fw = 0 ;

			// まずはトータルの横幅を計算する

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					r = m_CodeSprite[ vs[ i ] ].textureRect ;
					
					vw = r.width * m_CodeScale.x ;
					fw = fw + vw ;

					if( i <  ( l - 1 ) )
					{
						fw = fw + codeSpace ;
					}
				}
			}
			
			float fh = 0 ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_CodeSprite.ContainsKey( vs[ i ] ) == true )
				{
					// 大元のサイズを取得する
					r = m_CodeSprite[ vs[ i ] ].textureRect ;

					vh = r.height * m_CodeScale.y ;

					if( vh >  fh )
					{
						fh  = vh ;
					}
				}
			}

			// 実際のサイズを保存する
			m_PreferredWidth  = fw ;
			m_PreferredHeight = fh ; 
		}

		// 文字列を更新する
		private string GetNumberString()
		{
			int i, l ;
			string s, ss ;
			double tValue = m_Value ;

			//-----------------------------------------------------
		
			// 一旦数値を文字列化する
			string t = "" ;
		
			if( tValue <  0 )
			{
				t = "-" ;
				tValue = - tValue ;
			}
			else
			if( tValue >  0 )
			{
				if( m_PlusSign == true )
				{
					t = "+" ;
				}
			}
			else
			{
				if( m_ZeroSign == true )
				{
					t = "±" ;	// +-
				}
			}
		
			// 整数部
			if( m_DigitInteger <= 0 )
			{
				s = ( ( int )tValue ).ToString() ;
			}
			else
			{
				if( m_ZeroPadding == false )
				{
					s = string.Format( "{0," + m_DigitInteger + "}", ( int )tValue ) ;
				}
				else
				{
					s = string.Format( "{0,0:d" + m_DigitInteger + "}", ( int )tValue ) ;
				}
			}
		
			// s が整数値部
			l = s.Length ;

			if( m_Comma >  0 && m_Comma <  l )
			{
				// 指定桁ごとにカンマを仕込む
				ss = "" ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					if( i >  0 && ( ( i % m_Comma ) == 0 ) && s[ l - 1 - i ] >= '0' && s[ l - 1 - i ] <= '9' )
					{
						ss = "," + ss ;
					}
				
					ss = s[ l - 1 - i ] + ss ;
				}
			}
			else
			{
				ss = s ;
			}

			t = t + ss ;

			// 小数部
			if( m_DigitDecimal >  0 )
			{
				t = t + "." ;
			
				s = tValue.ToString() ;
			
				for( i  = 0 ; i <  s.Length ; i ++ )
				{
					if( ( int )s[ i ] == ( int )'.' )
					{
						break ;
					}
				}
			
				if( i >= s.Length )
				{
					s = "" ;
					l = 0 ;
				}
				else
				{
					s = s.Substring( i + 1, s.Length - ( i + 1 ) ) ;
					if( s.Length >  m_DigitDecimal )
					{
						s = s.Substring( 0, m_DigitDecimal ) ;
					}
				
					l = s.Length ;
				}
			
				l = m_DigitDecimal - l ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					s = s + "0" ;
				}

				l = s.Length ;

				// s が小数値部
				if( m_Comma >  0 && m_Comma <  l )
				{
					// 指定桁ごとにカンマを仕込む
					ss = "" ;
					for( i  = 0 ; i <  l ; i ++ )
					{
						if( i >  0 && ( ( i % m_Comma ) == 0 ) && s[ l - 1 - i ] >= '0' && s[ l - 1 - i ] <= '9' )
						{
							ss = ss + "," ;
						}
					
						ss = ss + s[ i ] ;
//						ss = ss + s.Substring( i, 1 ) ;
					}
				}
				else
				{
					ss = s ;
				}

				t = t + ss ;
			}
		
			if( m_Percent == true )
			{
				t = t + "%" ;
			}

			return t ;
		}
	}
}
