using UnityEngine ;
using UnityEngine.UI ;
using System ;
using System.Collections ;
using System.Collections.Generic ;

namespace uGUIHelper
{
	[RequireComponent( typeof( RichText ) ) ]

	/// <summary>
	/// uGUI:Text クラスの機能拡張コンポーネントクラス
	/// </summary>
	public class UIRichText : UIView
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return Vector2.zero ;
				}
				return new Vector2( tRichText.preferredWidth, tRichText.preferredHeight ) ;
			}
		}

		/// <summary>
		/// 文字の最大縦幅
		/// </summary>
		public float fullHeight
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.preferredFullHeight ;
			}
		}

		/// <summary>
		/// フォントサイズ(ショートカット)
		/// </summary>
		public int fontSize
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.fontSize ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.fontSize = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return null ;
				}
				return tRichText.text ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.text = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// フォント(ショートカット)
		/// </summary>
		public Font font
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return null ;
				}
				return tRichText.font ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.font = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return Color.white ;
				}
				return tRichText.color ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.color = value ;
			}
		}
		
		/// <summary>
		/// マテリアル(ショートカット)
		/// </summary>
		public Material material
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return null ;
				}
				return tRichText.material ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.material = value ;
			}
		}

		/// <summary>
		/// フォントスタイル(ショートカット)
		/// </summary>
		public FontStyle fontStyle
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return FontStyle.Normal ;
				}
				return tRichText.fontStyle ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.fontStyle = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.supportRichText ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.supportRichText = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
		
		/// <summary>
		/// アライメント(ショートカット)
		/// </summary>
		public TextAnchor alignment
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return TextAnchor.MiddleCenter ;
				}
				return tRichText.alignment ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.alignment = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.resizeTextForBestFit ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.resizeTextForBestFit = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 横方向の表示モード(ショートカット)
		/// </summary>
		public  HorizontalWrapMode horizontalOverflow
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return HorizontalWrapMode.Overflow ;
				}
				return tRichText.horizontalOverflow ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.horizontalOverflow = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}
	
		/// <summary>
		/// 縦方向の表示モード(ショートカット)
		/// </summary>
		public  VerticalWrapMode verticalOverflow
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return VerticalWrapMode.Overflow ;
				}
				return tRichText.verticalOverflow ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.verticalOverflow = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 1.0f ;
				}
				return tRichText.lineSpacing ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.lineSpacing = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
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
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.raycastTarget ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.raycastTarget = value ;
			}
		}


		/// <summary>
		/// 上スペース係数(ショートカット)
		/// </summary>
		public  float topMaginSpacing
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.topMarginSpacing ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.topMarginSpacing= value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// 下スペース係数(ショートカット)
		/// </summary>
		public  float bottomMaginSpacing
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.bottomMarginSpacing ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.bottomMarginSpacing= value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

/*		/// <summary>
		/// カーソル位置(ショートカット)
		/// </summary>
		public  Vector2 cursorPosition
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return Vector2.zero ;
				}
				return tRichText.cursorPosition ;
			}
		}*/

		/// <summary>
		/// 上のマージンが必要かどうか(ショートカット)
		/// </summary>
		public  bool isNeedTopMagin
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.isNeedTopMargin ;
			}
		}

		/// <summary>
		/// 下のマージンが必要かどうか(ショートカット)
		/// </summary>
		public  bool isNeedBottomMagin
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.isNeedBottomMargin ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// ビューのコントロールの有効状態
		/// </summary>
		public bool viewControllEnabled
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.viewControllEnabled ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.viewControllEnabled = value ;
			}
		}
		
		/// <summary>
		/// 表示文字数(ショートカット)
		/// </summary>
		public  int lengthOfView
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.lengthOfView ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.lengthOfView = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// 表示開始行数(ショートカット)
		/// </summary>
		public  int startLineOfView
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.startLineOfView ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.startLineOfView = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// 表示終了行数(ショートカット)
		/// </summary>
		public  int endLineOfView
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.endLineOfView ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.endLineOfView = value ;

				if( autoSizeFitting == true )
				{
					Resize() ;
				}
			}
		}

		/// <summary>
		/// フェード開始オフセット(ショートカット)
		/// </summary>
		public  int startOffsetOfFade
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.startOffsetOfFade ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.startOffsetOfFade = value ;
			}
		}

		/// <summary>
		/// フェード終了オフセット(ショートカット)
		/// </summary>
		public  int endOffsetOfFade
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.endOffsetOfFade ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.endOffsetOfFade = value ;
			}
		}
		
		/// <summary>
		/// 表示文字比率(ショートカット)
		/// </summary>
		public  float ratioOfFade
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.ratioOfFade ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.ratioOfFade = value ;
			}
		}

		/// <summary>
		/// 透過対象文字数(ショートカット)
		/// </summary>
		public  int widthOfFade
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.widthOfFade ;
			}
			set
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return ;
				}
				tRichText.widthOfFade = value ;
			}
		}
		
		/// <summary>
		/// ルビが使用されているかどうか(ショートカット)
		/// </summary>
		public bool isRubyUsed
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return false ;
				}
				return tRichText.isRubyUsed ;
			}
		}
		
		//--------------------------------------------------

		public Shadow	shadow
		{
			get
			{
				return _shadow ;
			}
		}

		public Outline	outline
		{
			get
			{
				return _outline ;
			}
		}

		public UIGradient gradient
		{
			get
			{
				return _gradient ;
			}
		}

		//-----------------------------------------------------------
		
		/// <summary>
		/// 最大文字数(ショートカット)
		/// </summary>
		public  int length
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.length ;
			}
		}

		/// <summary>
		/// 行数を取得する(ショートカット)
		/// </summary>
		public int line
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return 0 ;
				}
				return tRichText.line ;
			}
		}

		/// <summary>
		/// 指定したラインの開始時の描画対象となる文字数を取得する(ショートカット)
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetStartOffsetOfLine( int tLine )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				return -1 ;
			}
			return tRichText.GetEndOffsetOfLine( tLine ) ;
		}

		/// <summary>
		/// 指定したラインの終了時の描画対象となる文字数を取得する(ショートカット)
		/// </summary>
		/// <param name="tLine"></param>
		/// <returns></returns>
		public int GetEndOffsetOfLine( int tLine )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				return -1 ;
			}
			return tRichText.GetEndOffsetOfLine( tLine ) ;
		}

		/// <summary>
		/// 最初の表示文字数を取得する
		/// </summary>
		public int startOffsetOfView
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return -1 ;
				}
				return tRichText.startOffsetOfView ;
			}
		}

		/// <summary>
		/// 最後の表示文字数を取得する
		/// </summary>
		public int endOffsetOfView
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return -1 ;
				}
				return tRichText.endOffsetOfView ;
			}
		}


		/// <summary>
		/// 拡張タグイベント
		/// </summary>
		public RichText.ExtraTagEvent[] extraTagEvent
		{
			get
			{
				RichText tRichText = _richText ;
				if( tRichText == null )
				{
					return null ;
				}
				return tRichText.extraTagEvent ;
			}
		}

		/// <summary>
		/// 拡張タグイベントを取得する
		/// </summary>
		public RichText.ExtraTagEvent[] GetExtraTagEvent( params string[] tFilter)
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				return null ;
			}
			return tRichText.GetExtraTagEvent( tFilter ) ;
		}

		/// <summary>
		/// 指定した文字数のキャレットの座標を指定する
		/// </summary>
		/// <param name="tLength"></param>
		/// <returns></returns>
		public Vector2 GetCaretPosition( int tLength = -1 )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				return Vector2.zero ;
			}
			return tRichText.GetCaretPosition( tLength ) ;
		}

		//-------------------------------------------------------------------------------------------

		private bool m_Playing = false ;
		private bool m_Pausing = false ;
		private bool m_EventPausing = false ;
		private bool m_Break = false ;

		private IEnumerator	m_Callback = null ;
		private IEnumerator m_Coroutine = null ;

		/// <summary>
		/// 拡張タグ名を設定する
		/// </summary>
		/// <param name="tExtraTagName"></param>
		public void SetExtraTagName( params string[] tExtraTagName )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				return ;
			}

			tRichText.SetExtraTagName( tExtraTagName ) ;
		}
		
		/// <summary>
		/// 文章をアニメーション表示させる
		/// </summary>
		/// <param name="tStartLine">開始行</param>
		/// <param name="tEndLine">終了行</param>
		/// <param name="tCodeTime">１文字あたりの表示時間(秒)</param>
		public void Play( int tStartLine = -1, int tEndLine = -1, float tCodeTime = 0.1f, bool tIsAbsoluteTime = false )
		{
			m_Coroutine = Play_Coroutine<System.Object>( tStartLine, tEndLine, tCodeTime, tIsAbsoluteTime, null, null, null ) ;
			StartCoroutine( m_Coroutine ) ;
		}
		
		/// <summary>
		/// 文章をアニメーション表示させる
		/// </summary>
		/// <param name="tStartLine">開始行</param>
		/// <param name="tEndLine">終了行</param>
		/// <param name="tCodeTime">１文字あたりの表示時間(秒)</param>
		/// <param name="tOnEvent">独自イベント発生時のコールバック</param>
		public void Play<T>( int tStartLine = -1, int tEndLine = -1, float tCodeTime = 0.1f, bool tIsAbsoluteTime = false, Func<RichText.ExtraTagEvent, T, IEnumerator> tOnEvent = null, T tObject = null, params string[] tFilter ) where T : class 
		{
			m_Coroutine = Play_Coroutine( tStartLine, tEndLine, tCodeTime, tIsAbsoluteTime, tOnEvent, tObject, tFilter ) ;
			StartCoroutine( m_Coroutine ) ;
		}
		
		// 文章をアニメーション表示させる
		private IEnumerator Play_Coroutine<T>( int tStartLine, int tEndLine, float tCodeTime, bool tIsAbsoluteTime, Func<RichText.ExtraTagEvent, T, IEnumerator> tOnEvent, T tObject, params string[] tFilter )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				yield break ;
			}

			int tLine = line ;

			if( tStartLine <  0 )
			{
				tStartLine   = 0 ;
			}
			if( tStartLine >  ( tLine - 1 ) )
			{
				tStartLine  = ( tLine - 1 ) ;
			}

			if( tEndLine <  0 )
			{
				tEndLine  = tLine ;
			}
			if( tEndLine >  tLine )
			{
				tEndLine  = tLine ;
			}

			if( tEndLine >= 0 && tEndLine <  ( tStartLine + 1 ) )
			{
				tEndLine  = ( tStartLine + 1 ) ;
			}

			if( tCodeTime <= 0 )
			{
				tCodeTime  = 0.05f ;
			}

			//----------------------------------------------------------

			m_Playing = true ;
			m_Pausing = false ;
			m_EventPausing = false ;
			m_Break = false ;

			tRichText.viewControllEnabled	= true ;
			tRichText.lengthOfView			= -1 ;
			tRichText.startLineOfView		= tStartLine ;
			tRichText.endLineOfView			= tEndLine ;

			List<RichText.ExtraTagEvent> tEvent = new List<RichText.ExtraTagEvent>() ;

			RichText.ExtraTagEvent[] tEventTemporary = tRichText.GetExtraTagEvent( tFilter ) ;

			bool f = false ;
			if( tEvent != null )
			{
				int i, l = tEventTemporary.Length ;
				for( i  = 0 ; i <  l ; i ++ )
				{
					tEvent.Add( tEventTemporary[ i ] ) ;
				}

				l = tEvent.Count ;
				if( l >  0 )
				{
					if( tEvent[ l - 1 ].offset >=  tRichText.endOffsetOfView )
					{
						// 一番最後にスベントが存在する
						f = true ;
					}
				}
			}

			if( f == false )
			{
				// 最後の終端を追加する
				tEvent.Add( new RichText.ExtraTagEvent( tRichText.endOffsetOfView, null, null ) ) ;
			}

			//----------------------------------------------------------

			// タイマー開始
			StartTimer( tIsAbsoluteTime ) ;

			float tTime, t ;

			int o = tRichText.startOffsetOfView ;

			// イベントでループ
			int e ;
			for( e  = 0 ; e <  tEvent.Count ; e ++ )
			{
				//--------------------------------------------------------/

				// １イベントあたりの文字列の表示時間
				tTime = ( tEvent[ e ].offset - o ) * tCodeTime ;

				tRichText.ratioOfFade = 0 ;
				tRichText.startOffsetOfFade = o ;
				tRichText.endOffsetOfFade = tEvent[ e ].offset ;

				// １イベントの文字列の表示ループ
				t = 0 ;
				while( t <  tTime )
				{
					if( m_Pausing == true )
					{
						// 先にポーズ中かのチェックを入れておかないと１フレーム消費して表示が激遅なる
						yield return new WaitWhile( () => m_Pausing == true ) ;
					}

					if( m_Break == true )
					{
						// 強制終了
						tRichText.ratioOfFade		= 1 ;
						tRichText.startOffsetOfFade	= tRichText.startOffsetOfView ;
						tRichText.endOffsetOfFade	= tRichText.endOffsetOfView ;

						m_Break = false ;
						m_Playing = false ;

						yield break ;
					}

					t = t + GetDeltaTime() ;
					if( t >  tTime )
					{
						t  = tTime ;
					}

					tRichText.ratioOfFade = t / tTime ;

					yield return null ;
				}

				if( string.IsNullOrEmpty( tEvent[ e ].tagName ) == false )
				{
					if( tOnEvent != null )
					{
						m_EventPausing = true ;

						// イベント用のコールバックを呼び出す
						m_Callback = tOnEvent( tEvent[ e ], tObject ) ;
						yield return StartCoroutine( m_Callback  ) ;

						m_EventPausing = false ;
					}
				}

				o = tEvent[ e ].offset ;
			}

			m_Break = false ;
			m_Playing = false ;

			m_Coroutine = null ;
		}
		
		private bool m_IsAbsoluteTime = false ;

		// タイマーを開始する
		private void StartTimer( bool tIsAbsoluteTime )
		{
			m_IsAbsoluteTime = tIsAbsoluteTime ;
		}

		// 経過時間を取得する
		private float GetDeltaTime()
		{
			if( m_IsAbsoluteTime == false )
			{
				return Time.deltaTime ;
			}
			else
			{
				return Time.unscaledDeltaTime ;
			}
		}

		//-----------------------------------------------------------

		/// <summary>
		/// 再生中かどうか
		/// </summary>
		public bool isPlaying
		{
			get
			{
				return m_Playing ;
			}
		}

		/// <summary>
		/// 一時停止中かどうか
		/// </summary>
		public bool isPausing
		{
			get
			{
				if( m_Pausing == true || m_EventPausing == true )
				{
					return true ;
				}

				return false ;
			}
		}



		/// <summary>
		/// 文章アニメーションを一時停止する
		/// </summary>
		public void Pause()
		{
			m_Pausing = true ;
		}

		/// <summary>
		/// ポーズを解除する
		/// </summary>
		public void Unpause()
		{
			m_Pausing = false ;
		}


		/// <summary>
		/// 文章アニメーションを強制終了する
		/// </summary>
		public void Stop()
		{
			if( m_Coroutine != null )
			{
				StopCoroutine( m_Coroutine ) ;
				m_Coroutine = null ;
			}

			m_Playing = false ;
			m_Pausing = false ;
			m_Break   = false ;
		}


		/// <summary>
		/// アニメーション中のメッセージを全て表示する
		/// </summary>
		public void Finish()
		{
			if( m_Playing == true )
			{
//				if( m_Callback != null )
//				{
//					StopCoroutine( m_Callback ) ;
//					m_Callback = null ;
//				}

				m_Break = true ;
			}
		}

		//--------------------------------------------------
	
		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			RichText tRichText = _richText ;
			if( tRichText == null )
			{
				tRichText = gameObject.AddComponent<RichText>() ;
			}
			if( tRichText == null )
			{
				// 異常
				return ;
			}

			//----------------------------

			Color	tDefaultTextColor = Color.white ;

			Font	tDefaultFont = null ;
			int		tDefaultFontSize = 0 ;

#if UNITY_EDITOR

			if( Application.isPlaying == false )
			{
				// メニューから操作した場合のみ自動設定を行う
				DefaultSettings tDS = Resources.Load<DefaultSettings>( "uGUIHelper/DefaultSettings" ) ;
				if( tDS != null )
				{
					tDefaultTextColor	= tDS.textColor ;

					tDefaultFont		= tDS.font ;
					tDefaultFontSize	= tDS.fontSize ;
				}
			}
			
#endif

			tRichText.color = tDefaultTextColor ;

			if( tDefaultFont == null )
			{
				tRichText.font = Resources.GetBuiltinResource( typeof( Font ), "Arial.ttf" ) as Font ;
			}
			else
			{
				tRichText.font = tDefaultFont ;
			}

			if( tDefaultFontSize <= 0 )
			{
				tRichText.fontSize = 32 ;
			}
			else
			{
				tRichText.fontSize = tDefaultFontSize ;
			}

			tRichText.alignment = TextAnchor.MiddleLeft ;

			tRichText.horizontalOverflow = HorizontalWrapMode.Overflow ;
			tRichText.verticalOverflow   = VerticalWrapMode.Overflow ;
			
			ResetRectTransform() ;

			//----------------------------------

			if( isCanvasOverlay == true )
			{
				tRichText.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
		}

		override protected void OnLateUpdate()
		{
			if( autoSizeFitting == true )
			{
				Resize() ;
			}
		}

		private void Resize()
		{
//			Debug.LogWarning( "おい" ) ;
			RichText t = _richText ;
			RectTransform r = _rectTransform ;
			if( r != null && t != null )
			{
				Vector2 tSize = r.sizeDelta ;

				if( r.anchorMin.x == r.anchorMax.x )
				{
					if( t.horizontalOverflow == HorizontalWrapMode.Overflow )
					{
						tSize.x = t.preferredWidth ;
					}
				}

//				Debug.LogWarning( "きたよ:" + r.anchorMin.y + " " + r.anchorMax.y ) ;
				if( r.anchorMin.y == r.anchorMax.y )
				{
//					Debug.LogWarning( "きました:" + name ) ;
					if( t.verticalOverflow == VerticalWrapMode.Overflow )
					{
						tSize.y = t.preferredHeight ;
					}
				}

				r.sizeDelta = tSize ;
			}
		}
	}
}

