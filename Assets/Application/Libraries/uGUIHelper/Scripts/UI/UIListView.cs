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
	/// uGUI:ScrollRect クラスの機能拡張コンポーネントクラス(複合)
	/// </summary>
//	[RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
	[RequireComponent(typeof(ScrollRectWrapper))]
	public class UIListView : UIScrollView
	{
		/// <summary>
		/// Item(テンプレート)のインスタンス
		/// </summary>
		public UIView		item ;

		/// <summary>
		/// リストビュー表示に使用するアイテム数
		/// </summary>
		public int		workingItemCount
		{
			get
			{
				return m_WorkingItemCount ;
			}
			set
			{
				if( m_WorkingItemCount != value && value >=  4 )
				{
					m_WorkingItemCount  = value ;
				}
				m_ItemListDirty = true ;	// 設定したら値が同じであっても必ず更新する
			}
		}

		[SerializeField][HideInInspector]
		private int		m_WorkingItemCount = 10 ;

		/// <summary>
		/// リストビュー表示に使用する余剰展開分のマージン
		/// </summary>
		public	float	workingMargin
		{
			get
			{
				return m_WorkingMargin ;
			}
			set
			{
				if( m_WorkingMargin  != value && value >= 64 )
				{
					m_WorkingMargin  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private float	m_WorkingMargin = 128 ;


		/// <summary>
		/// スクロールは有限か無限か
		/// </summary>
		public bool		infinity
		{
			get
			{
				return m_Infinity ;
			}
			set
			{
				if( m_Infinity != value )
				{
					m_Infinity  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_Infinity = false ;	// 有限

		/// <summary>
		/// 有限の場合のアイテムの最大数
		/// </summary>
		public int		itemCount
		{
			get
			{
				return m_ItemCount ;
			}
			set
			{
				if( m_ItemCount != value )
				{
					m_ItemCount  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private int		m_ItemCount = 20 ;


		/// <summary>
		/// スナップの有無
		/// </summary>
		public bool		snap
		{
			get
			{
				return m_Snap ;
			}
			set
			{
				if( value != m_Snap )
				{
					m_Snap = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private bool	m_Snap = false ;

		/// <summary>
		/// スナップの実行判定速度
		/// </summary>
		public float	snapThreshold = 100.0f ;

		/// <summary>
		/// スナップの速度
		/// </summary>
		public float	snapTime = 0.25f ;

		public enum SnapAnchor
		{
			Haed = 0,
			Tail = 1,
		}

		/// <summary>
		/// スナップの位置
		/// </summary>
		public SnapAnchor snapAnchor
		{
			get
			{
				return m_SnapAnchor ;
			}
			set
			{
				if( m_SnapAnchor != value )
				{
					m_SnapAnchor  = value ;
					m_ItemListDirty = true ;
				}
			}
		}

		[SerializeField][HideInInspector]
		private SnapAnchor	m_SnapAnchor = SnapAnchor.Haed ;

		//------------------------------------------------

		// 内部処理変数系

		public class ItemData
		{
			public int						index ;
			public UIView					view ;
			public UnityEngine.Component	code ;
			public float					size ;

			public ItemData( int tIndex, UIView tView, UnityEngine.Component tCode, float tSize )
			{
				index	= tIndex ;
				view	= tView ;
				code	= tCode ;
				size	= tSize ;
			}
		}		

		private Type	m_TargetType = null ;
		
		// アイテムリスト
		private List<ItemData> m_ItemList = new List<ItemData>() ;

		private int		m_CurrentItemIndex = 0 ;
		
		private float	m_ItemHeadPosition = 0 ;
		private float	m_ItemTailPosition = 0 ;

		private bool	m_ItemListDirty = true ;

		private float	m_CanvasLength = 0 ;

		private float?	m_DirtyPosition		= null ;
		private int?	m_DirtyIndex		= null ;
		private int?	m_DirtyItemCount	= null ;

		//-------------------------------------------------------------------------------------

		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			ScrollRectWrapper tScrollRect = _scrollRect ;

			if( tScrollRect == null )
			{
				tScrollRect = gameObject.AddComponent<ScrollRectWrapper>() ;
			}
			if( tScrollRect == null )
			{
				// 異常
				return ;
			}
			
			Image tImage = _image ;

			//-------------------------------------

			BuildType tBuildType = BuildType.Unknown ;
			Direction tDirection = Direction.Unknown ;

			if( tOption.ToLower() == "h" )
			{
				tBuildType = BuildType.ListView ;
				tDirection = Direction.Horizontal ;
			}
			else
			if( tOption.ToLower() == "v" )
			{
				tBuildType = BuildType.ListView ;
				tDirection = Direction.Vertical ;
			}

			buildType = tBuildType ;	// 後から変更は出来ない

			// 基本的な大きさを設定
			float s = 100.0f ;
			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				if( tSize.x <= tSize.y )
				{
					s = tSize.x ;
				}
				else
				{
					s = tSize.y ;
				}
				s = s * 0.5f ;
			}
				
			ResetRectTransform() ;

			// 方向を設定
			if( tDirection == Direction.Horizontal )
			{
				tScrollRect.horizontal = true ;
				tScrollRect.vertical   = false ;
				SetSize( s, s * 0.75f ) ;
			}
			else
			if( tDirection == Direction.Vertical )
			{
				tScrollRect.horizontal = false ;
				tScrollRect.vertical   = true ;
				SetSize( s * 0.75f, s ) ;
			}

			m_WorkingMargin = GetCanvasLength() * 128f / 960f ;


			// Mask 等を設定する Viewport を設定(スクロールバーは表示したいので ScrollRect と Mask は別の階層に分ける)
			m_Viewport = AddView<UIImage>( "Viewport" ) ;
			m_Viewport.SetAnchorToStretch() ;
			m_Viewport.SetMargin( 0, 0, 0, 0 ) ;
			m_Viewport.SetPivot( 0, 1 ) ;
			m_Viewport._z = -1 ;
			tScrollRect.viewport = m_Viewport._rectTransform ;

			// マスクは CanvasRenderer と 何等かの表示を伴うコンポートと一緒でなければ使用できない
//			Mask tMask = m_Viewport.gameObject.AddComponent<Mask>() ;
//			tMask.showMaskGraphic = false ;
			m_Viewport.gameObject.AddComponent<RectMask2D>() ;
//			m_Viewport.color = new Color( 0, 0, 0, 0 ) ;
			m_Viewport._image.enabled = false ;

			if( isCanvasOverlay == true )
			{
				m_Viewport.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
			
			// Content を追加する
			UIView tContent = CreateContent( m_Viewport, tBuildType, tDirection ) ;
			if( tContent != null )
			{
				tScrollRect.content = tContent._rectTransform ;
				m_Content = tContent ;
				
				// Item(テンプレート)を追加する
				item = CreateTemplateItem( tContent, tBuildType, tDirection ) ;

				tContent._z = -1 ;
			}

			// 自身の Image
			tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			tImage.color = new Color32( 255, 255, 255,  63 ) ;
			tImage.type = Image.Type.Sliced ;

			if( isCanvasOverlay == true )
			{
				tImage.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}
		}

		// デフォルトの Content を生成する
		private UIView CreateContent( UIView tParent, BuildType tBuildType, Direction tDirection )
		{
			UIView tContent = null ;

			tContent = tParent.AddView<UIView>( "Content" ) ;

			if( tDirection == Direction.Horizontal )
			{
				// 横スクロール
				tContent.SetAnchorToLeftStretch() ;
				tContent.SetPivot( 0.0f, 0.5f ) ;
			}

			if( tDirection == Direction.Vertical  )
			{
				// 縦スクロール
				tContent.SetAnchorToStretchTop() ;
				tContent.SetPivot( 0.5f, 1.0f ) ;
			}

			return tContent ;
		}

		// テンプレートのアイテムを生成する
		private UIView CreateTemplateItem( UIView tParent, BuildType tBuildType, Direction tDirection )
		{
			UIImage tItem = tParent.AddView<UIImage>( "Item(Template)" ) ;

			// Image
			tItem.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIBlank" ) ;
			tItem.color = Color.white ;
			tItem.type = Image.Type.Sliced ;

			// 横スクロール
			if( tDirection == Direction.Horizontal )
			{
				tItem.SetAnchorToLeftStretch() ;
				tItem.SetPivot( 0.0f, 0.5f ) ;
				tItem._w = _w * 0.2f ;
			}

			// 縦スクロール
			if( tDirection == Direction.Vertical  )
			{
				tItem.SetAnchorToStretchTop() ;
				tItem.SetPivot( 0.5f, 1.0f ) ;
				tItem._h = _h * 0.2f ;
			}

			if( isCanvasOverlay == true )
			{
				tItem.material = Resources.Load<Material>( "uGUIHelper/Shaders/UI-Overlay-Default" ) ;
			}

			return tItem ;
		}

		protected override void OnAwake()
		{
			base.OnAwake() ;

			if( Application.isPlaying == true )
			{
				// テンプレートのアイテムを無効化する(Awakeのタイミングで行わないとまずい)
				if( item != null )
				{
					item.SetActive( false ) ;
				}
			}
		}


		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			// ContentSize を設定後に呼ぶこと
			base.OnStart() ;

			// イベントトリガーにトランジション用のコールバックを登録する
			if( Application.isPlaying == true )
			{
				// キャンバスの基本サイズを取得する
				m_CanvasLength = GetCanvasLength() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}
			}
		}

		/// <summary>
		/// 派生クラスの Update
		/// </summary>
		protected override void OnUpdate()
		{
			base.OnUpdate() ;

			if( Application.isPlaying == true )
			{
				// キャンバスの基本サイズを取得する
				m_CanvasLength = GetCanvasLength() ;

				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}
			}
		}

		// リストにアップデートをかける
		private void UpdateItemList()
		{
			if( m_DirtyIndex != null )
			{
				SetContentIndex( m_DirtyIndex.Value, m_DirtyItemCount.Value, true ) ;
			}
			else
			if( m_DirtyPosition != null )
			{
				SetContentPosition( m_DirtyPosition.Value, m_DirtyItemCount.Value, true ) ;
			}
			else
			{
				// リフレッシュだけは例外的に即時更新も可能にする
				SetContentPosition( contentPosition ) ;
			}
		}

		void LateUpdate()
		{
			if( Application.isPlaying == true )
			{
				ProcessSnap() ;

				ProcessItem() ;
			}
		}

		// スナップ処理中かどうか
		protected override bool IsSnapping()
		{
			if( m_Snap == false || m_Snap == true && m_Snapping <  0 )
			{
				return false ;
			}

			return true ;
		}

		/// <summary>
		/// 現在のコンテントの位置を取得する
		/// </summary>
		/// <returns></returns>
		public float GetContentPosition()
		{
			return contentPosition ;
		}

		/// <summary>
		/// 値で指定した位置に座標を設定する
		/// </summary>
		/// <param name="tPosition"></param>
		public float SetContentPosition( float tPosition, int tItemCount = -1, bool isUpdating = true )
		{
			if( isUpdating == false )
			{
				m_DirtyIndex		= null ;
				m_DirtyPosition		= tPosition ;
				m_DirtyItemCount	= tItemCount ;

				m_ItemListDirty		= true ;

				return 0 ;
			}

			m_DirtyIndex			= null ;
			m_DirtyPosition			= null ;
			m_DirtyItemCount		= null ;

			//----------------------------------------------------------

			float tBasePosition = tPosition ;

			if( m_Infinity == false && tBasePosition <  0 )
			{
				// 有限スクロールの場合は負値は指定できない(代わりに最後に移動する)
				tBasePosition = 0 ;
			}

			int tIndex = -1 ;

			int i, l ;

			// この座標がどこに含まれるか検査する
			if( tBasePosition >= 0 )
			{
				// 正値
				if( infinity == false )
				{
					// 有限
					if( tItemCount >= 0 )
					{
						l = tItemCount ;
					}
					else
					{
						l = m_ItemCount ;
					}
				}
				else
				{
					// 無限
					l =  65536 ;
				}

				float p0 = 0 ;
				float p1 ;

				if( m_Snap == true && m_SnapAnchor == SnapAnchor.Tail )
				{
					// 最後基準の場合は例外的な処理を行う
					tBasePosition = tBasePosition + viewSize ;

					for( i  = 0 ; i <  l ; i ++ )
					{
						p1 = p0 + OnItemUpdatedInner( i, null, null ) ;
	
						if( tBasePosition >= p0 && tBasePosition <  p1 )
						{
							// ここに確定
							tIndex = i ;
							break ;
						}

						p0 = p1 ;
					}

					if( i >= l )
					{
						// 該当無しの場合は最後とする
						tIndex = l - 1 ;
					}
				}
				else
				{
					for( i  = 0 ; i <  l ; i ++ )
					{
						p1 = p0 + OnItemUpdatedInner( i, null, null ) ;
	
						if( tBasePosition >= p0 && tBasePosition <  p1 )
						{
							// ここに確定
							tIndex = i ;
							break ;
						}

						p0 = p1 ;
					}

					if( i >= l )
					{
						// 該当無しの場合は最後とする
						tIndex = l - 1 ;
					}
				}
			}
			else
			{
				// 負値(無限限定)

				l = -65536 ;

				float p0 ;
				float p1 = 0 ;
				for( i  = -1 ; i >  ( l - 1 ) ; i -- )
				{
					p0 = p1 - OnItemUpdatedInner( i, null, null ) ;

					if( tBasePosition >= p0 && tBasePosition <  p1 )
					{
						// ここに確定
						tIndex = i ;
						break ;
					}

					p1 = p0 ;
				}
			}

			// 指定のインデックスに飛ばす
			SetContentIndex( tIndex, tItemCount ) ;

			if( infinity == false )
			{
				// 有限

				if( tPosition <  0 )
				{
					tPosition  = 0 ;
				}

				// 位置が最後を超えてしまう場合はバウンドしないようにきっちり最後に合わせる
				if( ( contentSize - tPosition ) <  viewSize )
				{
					tPosition = contentSize - viewSize ;
				}
			}

			contentPosition = tPosition ;

			m_Snapping = 0 ;	// スナップ処理中であれば一旦キャンセルする

			if( m_Snap == true )
			{
				float tSnapToPosition = GetSnapPosition() ;
				if( contentPosition == tSnapToPosition )
				{
					m_Snapping = -1 ;
				}
			}
			return contentSize ;
		}

		// 指定したインデックスを基準としてアイテム群を表示させる
		public float SetContentIndex( int tIndex, int tItemCount = -1, bool isUpdating = true )
		{
			if( isUpdating == false )
			{
				m_DirtyIndex		= tIndex ;
				m_DirtyPosition		= null ;
				m_DirtyItemCount	= tItemCount ;

				m_ItemListDirty		= true ;

				return 0 ;
			}

			m_DirtyIndex			= null ;
			m_DirtyPosition			= null ;
			m_DirtyItemCount		= null ;

			m_ItemListDirty  = false ;

			//----------------------------------------------------------

			// スクロール中であれば止める
			_scrollRect.velocity = Vector2.zero ;

			if( content == null || item == null )
			{
				return 0 ;
			}

//			Debug.LogWarning( "アイテムリストを更新:" + name ) ;
//			float tt = Time.realtimeSinceStartup ;

			if( tItemCount <  0 )
			{
				// アイテム数は継続する
				tItemCount  = m_ItemCount ;
			}
			else
			if( m_ItemCount != tItemCount )
			{
				// アイテム数も変化する
				m_ItemCount  = tItemCount ;
			}
			
			if( infinity == false )
			{
				// 有限スクロール
				if( tIndex <  0 || tIndex >= tItemCount )
				{
					tIndex  = tItemCount - 1 ;	// 最後の位置にする
				}
			}

			//------------------------------------------------------------------

			int i, l ;

/*			if( m_ItemList.Count >  0 )
			{
				// 既に展開済みのアイテムを全て削除する
				l = m_ItemList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					if( m_ItemList[ i ].view != null )
					{
						DestroyImmediate( m_ItemList[ i ].view.gameObject ) ;
					}
				}

				m_ItemList.Clear() ;
			}*/

			//----------------------------------------------------------

			// コンテントサイズは最初に設定しておく必要がある

			if( infinity == false )
			{
				// 有限

				// コンテントのサイズを計算する
				float tContentSize = 0 ;
				for( i  = 0 ; i < tItemCount ; i ++ )
				{
					// 生成コールバックの呼び出し
					tContentSize += OnItemUpdatedInner( i, null, null ) ;
				}

				if( tContentSize <  viewSize )
				{
					tContentSize  = viewSize ;
				}

				contentSize = tContentSize ;

				_scrollRect.movementType = ScrollRect.MovementType.Elastic ;		// バウンドする
			}
			else
			{
				// 無限

				// コンテントのサイズ
				contentSize = viewSize ;	// 無限スクロールの場合はバウンドしないのでこの値自体はどうでもよい
				
				// スクロールタイプを設定
				_scrollRect.movementType = ScrollRect.MovementType.Unrestricted ;	// バウンドしない
			}

			//------------------------------------------------------------------

			float tItemOffset = 0 ;
			float tSnapOffset = 0 ;	// スナップ有効且つアンカーが最後の場合のズレ補正値

			if( ( m_Infinity == false && contentSize >  viewSize ) || m_Infinity == true )
			{
				if( m_Snap == true && m_SnapAnchor == SnapAnchor.Tail )
				{
					// 最後基準である場合はインデックスは画面最後のものとみなしオフセット値の開始値もずれる

					if( m_Infinity == false )
					{
						// 有限
						l = 0 ;
					}
					else
					{
						// 無限
						l = -65536 ;
					}

					float tContentSize = 0 ;
					for( i  = tIndex ; i >= l ; i -- )
					{
						tContentSize += OnItemUpdatedInner( i, null, null ) ;
						if( tContentSize >= viewSize )
						{
							break ;	// 足りた
						}
					}

					if( tContentSize <  viewSize )
					{
						// 足りない(足りない状況が起こり得るのは有限の場合のみ
						for( i  = ( tIndex + 1 ) ; i <  m_ItemCount ; i ++ )
						{
							tContentSize += OnItemUpdatedInner( i, null, null ) ;
							if( tContentSize >=  viewSize )
							{
								break ;	// 足りた
							}
						}

						tIndex = 0 ;	// インデックスは０始まりで確定
					}
					else
					{
						tIndex = i ;
					}

					tSnapOffset = tContentSize - viewSize ;	// ズレ
				}

				// オフセット値を調整する
				if( tIndex >  0 )
				{
					// 正方向
					for( i  = 0 ; i <  tIndex ; i ++ )
					{
						// 幅だけ取得する
						tItemOffset += OnItemUpdatedInner( i, null, null ) ;
					}
				}
				else
				if( tIndex <  0 )
				{
					// 負方向
					for( i  = -1 ; i >  ( tIndex - 1 ) ; i -- )
					{
						// 幅だけ取得する
						tItemOffset -= OnItemUpdatedInner( i, null, null ) ;
					}
				}

				// 有限の場合は最後の方に行き過ぎると表示がおかしくなるのでスナップで限界までに留める
				if( infinity == false )
				{
					// 有限
					if( ( contentSize - tItemOffset ) <  viewSize )
					{
						// 下に空白が出来てしまうので余力が出来るまでインデックスを引き下げる
						for( i  = tIndex - 1 ; i >= 0 ; i -- )
						{
							tItemOffset -= OnItemUpdatedInner( i, null, null ) ;

							if( ( contentSize - tItemOffset ) >= viewSize )
							{
								break ;	// 余力が出来た
							}
						}

						tIndex = i ;
					}
				}
			}
			else
			{
				// contentSize が viewSize より小さい場合はインデックスは０に固定される
				tIndex = 0 ;
			}

			//----------------------------------------------------------

			m_CurrentItemIndex = tIndex ;
			contentPosition = tItemOffset + tSnapOffset ;
			m_Snapping = 0 ;	// スナップ処理中であれば一旦キャンセルする

			//---------------------------------------------

			m_ItemHeadPosition = tItemOffset ;

			UIView	tItemView ;
			UnityEngine.Component tItemCode ;
			float	tItemSize ;

			// アイテムを展開する
			l = m_WorkingItemCount ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tItemCode = null ;

				if( i >= m_ItemList.Count )
				{
					// 複製
					tItemView = GameObject.Instantiate( item ) as UIView ;

					// 親を設定
					tItemView.SetParent( content, false ) ;
				}
				else
				{
					// 継続
					tItemView = m_ItemList[ i ].view ;

					tItemCode = m_ItemList[ i ].code ;
				}

				if( infinity == true || ( infinity == false && ( tIndex + i ) <  tItemCount ) )
				{
					// 名前を設定
					tItemView.name = ( tIndex + i ).ToString() ;

					// アイテムをアクティブ化
					tItemView.SetActive( true ) ;

					// 位置を設定
					SetItemPosition( tItemView, tItemOffset ) ;

					if( tItemCode == null )
					{
						if( m_TargetType != null )
						{
							tItemCode = tItemView.gameObject.GetComponent( m_TargetType ) ;
						}
					}

					// 生成コールバックの呼び出し
					tItemSize = OnItemUpdatedInner( tIndex + i, tItemView.gameObject, tItemCode ) ;

					// 縦幅を設定
					SetItemSize( tItemView, tItemSize ) ;

					// オフセット増加
					tItemOffset += tItemSize ;

					// アイテムリストに追加
					if( i >= m_ItemList.Count )
					{
						m_ItemList.Add( new ItemData( tIndex + i, tItemView, tItemCode, tItemSize ) ) ;
					}
					else
					{
						m_ItemList[ i ].index	= tIndex + i ;
						m_ItemList[ i ].size	= tItemSize ;
					}
				}
				else
				{
					// 有限且つインデックスが最大アイテム数を超える場合はここにくる

					// 見えないダミーを登録しておく(必ず DisplayItemCount 分展開する必要がある)

					// 名前を設定
					tItemView.name = "hide" ;

					// アイテムを非アクティブ化
					tItemView.SetActive( false ) ;

					if( tItemCode == null )
					{
						if( m_TargetType != null )
						{
							tItemCode = tItemView.gameObject.GetComponent( m_TargetType ) ;
						}
					}

					// ダミーサイズ
					tItemSize = workingMargin ;

					// オフセット増加
					tItemOffset += tItemSize ;

					if( i >= m_ItemList.Count )
					{
						// アイテムリストに追加
						m_ItemList.Add( new ItemData( -1, tItemView, tItemCode, tItemSize ) ) ;
					}
					else
					{
						m_ItemList[ i ].index	= -1 ;
						m_ItemList[ i ].size	= tItemSize ;
					}
				}
			}

			m_ItemTailPosition = tItemOffset ;

//			Debug.LogWarning( "------>計測時間: [ " + ( Time.realtimeSinceStartup - tt ) + " ]" ) ;

			return contentSize ;
		}


		/// <summary>
		/// 展開中のアイテムを取得する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <returns></returns>
		public UnityEngine.Component GetWorkingItem( int tIndex )
		{
			if( m_ItemList == null || m_ItemList.Count == 0 || tIndex <  0 )
			{
				return null ;
			}

			//----------------------------------------------------------

			int i, l = m_ItemList.Count ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				if( m_ItemList[ i ].index == tIndex )
				{
					// 発見
					return m_ItemList[ i ].code ;
				}
			}

			return null ;
		}

		// インデックスをポジションに変換する
		private float ConvertIndexToPosition( int tIndex )
		{
			//----------------------------------------------------------

			// コンテントサイズは最初に設定しておく必要がある

			int i, l ;
			int tItemCount = itemCount ;
			float tContentSize = 0 ;
			
			if( infinity == false )
			{
				// 有限

				// コンテントのサイズを計算する
				for( i  = 0 ; i < tItemCount ; i ++ )
				{
					// 生成コールバックの呼び出し
					tContentSize += OnItemUpdatedInner( i, null, null ) ;
				}

				if( tContentSize <  viewSize )
				{
					tContentSize  = viewSize ;
				}
			}
			else
			{
				// 無限

				// コンテントのサイズ
				tContentSize = viewSize ;	// 無限スクロールの場合はバウンドしないのでこの値自体はどうでもよい
			}

			//------------------------------------------------------------------

			float tItemOffset = 0 ;
			float tSnapOffset = 0 ;	// スナップ有効且つアンカーが最後の場合のズレ補正値

			if( ( m_Infinity == false && tContentSize >  viewSize ) || m_Infinity == true )
			{
				if( m_Snap == true && m_SnapAnchor == SnapAnchor.Tail )
				{
					// 最後基準である場合はインデックスは画面最後のものとみなしオフセット値の開始値もずれる

					if( m_Infinity == false )
					{
						// 有限
						l = 0 ;
					}
					else
					{
						// 無限
						l = -65536 ;
					}

					tContentSize = 0 ;
					for( i  = tIndex ; i >= l ; i -- )
					{
						tContentSize += OnItemUpdatedInner( i, null, null ) ;
						if( tContentSize >= viewSize )
						{
							break ;	// 足りた
						}
					}

					if( tContentSize <  viewSize )
					{
						// 足りない(足りない状況が起こり得るのは有限の場合のみ
						for( i  = ( tIndex + 1 ) ; i <  m_ItemCount ; i ++ )
						{
							tContentSize += OnItemUpdatedInner( i, null, null ) ;
							if( tContentSize >=  viewSize )
							{
								break ;	// 足りた
							}
						}

						tIndex = 0 ;	// インデックスは０始まりで確定
					}
					else
					{
						tIndex = i ;
					}

					tSnapOffset = tContentSize - viewSize ;	// ズレ
				}

				// オフセット値を調整する
				if( tIndex >  0 )
				{
					// 正方向
					for( i  = 0 ; i <  tIndex ; i ++ )
					{
						// 幅だけ取得する
						tItemOffset += OnItemUpdatedInner( i, null, null ) ;
					}
				}
				else
				if( tIndex <  0 )
				{
					// 負方向
					for( i  = -1 ; i >  ( tIndex - 1 ) ; i -- )
					{
						// 幅だけ取得する
						tItemOffset -= OnItemUpdatedInner( i, null, null ) ;
					}
				}

				// 有限の場合は最後の方に行き過ぎると表示がおかしくなるのでスナップで限界までに留める
				if( infinity == false )
				{
					// 有限
					if( ( contentSize - tItemOffset ) <  viewSize )
					{
						// 下に空白が出来てしまうので余力が出来るまでインデックスを引き下げる
						for( i  = tIndex - 1 ; i >= 0 ; i -- )
						{
							tItemOffset -= OnItemUpdatedInner( i, null, null ) ;

							if( ( contentSize - tItemOffset ) >= viewSize )
							{
								break ;	// 余力が出来た
							}
						}
					}
				}
			}

			//----------------------------------------------------------

			return tItemOffset + tSnapOffset ;
		}


		private List<ItemData> m_ItemChecker = new List<ItemData>() ;

		// 更新する(Contentから呼び出してもらう)
		private void ProcessItem()
		{
			if( m_WorkingItemCount <= 0 || m_ItemCount <= 0 )
			{
				return ;
			}

			if( m_Infinity == false && contentSize <= viewSize )
			{
				return ;
			}

			//----------------------------------------

			ItemData tItem ;
			UIView tItemView ;
			UnityEngine.Component tItemCode ;
			float tItemSize ;

			//----------------------------------------------------------

			m_ItemChecker.Clear() ;
			bool tWorkingItemFew = false ;

			float tTailPosition = contentPosition + viewSize ;

			// インデックスは↓正に進む＝コンテントは↑負に進む(上のを下に持っていく)
			while( ( m_ItemTailPosition - tTailPosition ) <  m_WorkingMargin )
			{
				// 左か上に現在の位置よりも 128 以上移動している
				// 最初のアイテムを最後のアイテムに移動させる

				tItem = m_ItemList[ 0 ] ;
				m_ItemList.RemoveAt( 0 ) ;
				m_ItemList.Add( tItem ) ;

				m_ItemChecker.Add( tItem ) ;	// チェッカー

				tItemView = tItem.view ;
				tItemCode = tItem.code ;
				tItemSize = tItem.size ;

				m_ItemHeadPosition += tItemSize ;

				int tIndex = m_CurrentItemIndex + m_WorkingItemCount ;
				if( m_Infinity == false )
				{
					// 有限
					if( tIndex <  0 || tIndex >= m_ItemCount )
					{
						// リミットを超えた分は非アクティブにする
//						Debug.LogWarning( "↓リミット超えてます:" + tIndex ) ;
						tItemView.SetActive( false ) ;

						// アイテムの名前を更新
						tItemView.name = "hide" ;

						// ダミーのサイズ
						tItemSize = m_WorkingMargin ;	// 幅が不確定な値になってしまうのでマージン値をダミー値としてセットする
					}
					else
					{
						// 生成時のコールバック呼び出し
						tItemView.SetActive( true ) ;

						// アイテムの名前を更新
						tItemView.name = tIndex.ToString() ;

						// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
						tItemSize = OnItemUpdatedInner( tIndex, tItemView.gameObject, tItemCode ) ;
					}
				}
				else
				{
					// 無限

					// アイテムの名前を更新
					tItemView.name = tIndex.ToString() ;

					// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
					tItemSize = OnItemUpdatedInner( tIndex, tItemView.gameObject, tItemCode ) ;
				}

				tItem.size  = tItemSize ;	// 位置変動したアイテムの縦幅をきちんと更新する
				tItem.index = tIndex ;

//				Debug.LogWarning( "↓位置:" + tIndex + " " + m_ItemTailPosition ) ;
				SetItemPosition( tItemView, m_ItemTailPosition ) ;
				if( tItemSize >  0 )
				{
					SetItemSize( tItemView, tItemSize ) ;
				}
				m_ItemTailPosition += tItemSize ;	// 後に座標を更新する

				m_CurrentItemIndex ++ ;
			}
			
			//----------------------------------------------------------

			float tHeadPosition = contentPosition ;

			// インデックスは↑負に進む＝コンテントは↓正に進む((下のを上に持っていく)
			while( ( tHeadPosition - m_ItemHeadPosition ) <  m_WorkingMargin )
			{
				// 最後のアイテムを最初に付け直す
				int tLastIndex = workingItemCount - 1 ; 

				tItem = m_ItemList[ tLastIndex ] ;
				m_ItemList.RemoveAt( tLastIndex ) ;
				m_ItemList.Insert( 0, tItem ) ;

				if( m_ItemChecker.Contains( tItem ) == true )
				{
					tWorkingItemFew = true ;
				}

				tItemView = tItem.view ;
				tItemCode = tItem.code ;
				tItemSize = tItem.size ;

				m_ItemTailPosition -= tItemSize ;

				m_CurrentItemIndex -- ;

				int tIndex = m_CurrentItemIndex ;
//				Debug.LogWarning( "展開:" + tIndex ) ;
				if( m_Infinity == false )
				{
					// 有限

					if( tIndex <  0 || tIndex >= m_ItemCount )
					{
						// リミットを超えた分は非アクティブにする
						tItemView.SetActive( false ) ;
//						Debug.LogWarning( "↑リミット超えてます:" + tIndex ) ;

						// アイテムの名前を更新
						tItemView.name = "hide" ;

						// ダミーのサイズ
						tItemSize = m_WorkingMargin ;	// 幅が不確定な値になってしまうのでマージン値をダミー値としてセットする
					}
					else
					{
						// 生成時のコールバック呼び出し
						tItemView.SetActive( true ) ;

						// アイテムの名前を更新
						tItemView.name = tIndex.ToString() ;

						// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
						tItemSize = OnItemUpdatedInner( m_CurrentItemIndex, tItemView.gameObject, tItemCode ) ;
					}
				}
				else
				{
					// 無限

					// アイテムの名前を更新
					tItemView.name = tIndex.ToString() ;

					// 更新があった場合のコールバックを呼び出す(ゲームオブジェクト自体は破棄する事は無い)
					tItemSize = OnItemUpdatedInner( m_CurrentItemIndex, tItemView.gameObject, tItemCode ) ;
				}

				tItem.size  = tItemSize ;	// 位置変動したアイテムの縦幅をきちんと更新する
				tItem.index = tIndex ;

				m_ItemHeadPosition -= tItemSize ;	// 先に座標を更新する

//				Debug.LogWarning( "↑位置:" + tIndex + " " + m_ItemHeadPosition ) ;
				SetItemPosition( tItemView, m_ItemHeadPosition ) ;
				if( tItemSize >  0 )
				{
					SetItemSize( tItemView, tItemSize ) ;
				}
			}

			m_ItemChecker.Clear() ;

			if( tWorkingItemFew == true )
			{
				Debug.LogWarning( "[UIListView] : WorkingItem is few -> " + m_ItemList.Count ) ;
			}
		}

		//-----------------------------------------------------------

		private int   m_Snapping = 0 ;
		private float m_SnapFromPosition ;
		private float m_SnapToPosition ;
		private float m_SnapBaseTime ;

		// スナップを処理する
		private void ProcessSnap()
		{
			if( snap == false )
			{
				return ;
			}

			if( m_Infinity == false && contentSize <= viewSize )
			{
				// スナップ無効
				return ;
			}

//			Debug.LogWarning( "リミット:" +  ( m_CanvasLength * 100f / 960f ) ) ;


			float wheel = Input.GetAxis( "Mouse ScrollWheel" ) ;

			if( _scrollRect.velocity.magnitude >  ( snapThreshold * m_CanvasLength / 960f ) || _scrollRect.isDrag == true || wheel != 0 )
			{
				m_Snapping = 0 ;

				// スナップ処理は行わない
				return ;
			}




//			Debug.LogWarning( "スナップ実行:" + _scrollRect.velocity.magnitude + " " + _scrollRect.velocity + " " + _scrollRect.isDrag ) ;

			// 強制的にフリックによるスクロールを停止させる
			_scrollRect.velocity = Vector2.zero ;

			if( m_Snapping <  0 )
			{
				// スナップ処理は実行していないがホイール操作でベロシティ０で位置ずれが発生する可能性がある
				float snapFromPosition = contentPosition ;	// スナップ前の位置
				float snapToPosition = GetSnapPosition() ;	// スナップ後の位置

				if( snapFromPosition != snapToPosition )
				{
					// 位置ずれが発生している
					m_Snapping = 0 ;
				}
			}

			if( m_Snapping == 0 )
			{
				// スナップ初期化

				// 現在位置を保存

				m_SnapFromPosition = contentPosition ;	// スナップ前の位置
				m_SnapToPosition = GetSnapPosition() ;	// スナップ後の位置

				if( snapTime <= 0 )
				{
					// 一瞬でスナップを終了させる
					contentPosition = m_SnapToPosition ;
					m_Snapping = -1 ;	// スナップ終了
					return ;
				}

				m_SnapBaseTime = Time.realtimeSinceStartup ;

				m_Snapping = 1 ;
			}

			if( m_Snapping == 1 )
			{
				// スナップ処理中
				float tTime = Time.realtimeSinceStartup - m_SnapBaseTime ;

				float tFactor = tTime / snapTime ;
				if( tFactor >  1 )
				{
					tFactor  = 1 ;
				}

				float tDelta = m_SnapToPosition - m_SnapFromPosition ;
				tDelta = UITween.GetValue( 0, tDelta, tFactor, UITween.ProcessType.Ease, UITween.EaseType.easeOutQuad ) ;
				
				contentPosition = m_SnapFromPosition + tDelta ;
				if( tFactor >= 1 )
				{
					contentPosition = m_SnapToPosition ;
					m_Snapping = -1 ;	// スナップ終了
				}
			}
		}

		// 現在の位置に対するスナップ後の位置を取得する
		private float GetSnapPosition()
		{
			float tSnapToPosition = 0 ;

			float tSnapFromPosition = contentPosition ;

			int i ;

			if( m_SnapAnchor == SnapAnchor.Haed )
			{
				// スナップは最初基準

				float tSnapBasePosition = tSnapFromPosition ;

				if( infinity == false )
				{
					// バウンドの完全戻りを待たずにスナップ処理が走り始めるので tSnapBasePosition がバウンド中状態の座標で + viewSize すると contentSize をオーバーしてしまいガクガク状態のバグが発生するのできちんと最大値を contentSize にする
					if( tSnapBasePosition <  0 )
					{
						tSnapBasePosition  = 0 ;
					}
				}

				float tNearestPosition	= m_ItemHeadPosition ;
				float tPosition			= m_ItemHeadPosition ;

				float tMinimumDistance = Mathf.Abs( tPosition - tSnapBasePosition ) ;
				float tDistance ;
				for( i  = 0 ; i <  m_ItemList.Count ; i ++ )
				{
					tPosition += m_ItemList[ i ].size ;

					// ブレブレチェック
					if( infinity == false && ( contentSize - tPosition ) <  viewSize )
					{
						// １つ前の位置で決定
						break ;
					}

					tDistance = Mathf.Abs( tPosition - tSnapBasePosition ) ;
					if( tDistance <  tMinimumDistance )
					{
						// 更新
						tNearestPosition = tPosition ;
						tMinimumDistance = tDistance ;
					}
				}
				
				tSnapToPosition = tNearestPosition ;
			}
			else
			{
				// スナップは最後基準

				float tSnapBasePosition = tSnapFromPosition + viewSize ;

				if( infinity == false )
				{
					// バウンドの完全戻りを待たずにスナップ処理が走り始めるので tSnapBasePosition がバウンド中状態の座標で + viewSize すると contentSize をオーバーしてしまいガクガク状態のバグが発生するのできちんと最大値を contentSize にする
					if( tSnapBasePosition >  contentSize )
					{
						tSnapBasePosition  = contentSize ;
					}
				}

				float tNearestPosition	= m_ItemHeadPosition ;
				float tPosition			= m_ItemHeadPosition + m_ItemList[ 0 ].size ;

				float tMinimumDistance = Mathf.Abs( tPosition - tSnapBasePosition ) ;
				float tDistance ;
				for( i  = 1 ; i <  m_ItemList.Count ; i ++ )
				{
					tPosition += m_ItemList[ i ].size ;

					if( ( ( tPosition >= tSnapBasePosition ) || ( ( tPosition <  tSnapBasePosition ) && tPosition >= viewSize ) ) )
					{
						// 更新
						tDistance = Mathf.Abs( tPosition - tSnapBasePosition ) ;
						if( tDistance <  tMinimumDistance )
						{
							tNearestPosition = tPosition ;
							tMinimumDistance = tDistance ;
						}
					}
				}
					
				tSnapToPosition = tNearestPosition - viewSize ;
			}

			return tSnapToPosition ;
		}


		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 基本的な設定を一括して行うメソッド(余計なリストの作り直しを省くためのもの)
		/// </summary>
		/// <param name="tIndex"></param>
		/// <param name="tItemCount"></param>
		/// <param name="tOnItemUpdated"></param>
		/// <param name="tSnap"></param>
		public void Setup( int tIndex, int tItemCount, Func<string,UIListView,int,GameObject,float> tOnItemUpdated, bool tSnap = false, bool tInfinity = false )
		{
			onItemUpdatedAction	= tOnItemUpdated ;
			m_Snap				= tSnap ;
			m_Infinity			= tInfinity ;

			SetContentIndex( tIndex, tItemCount ) ;
		}

		/// <summary>
		/// 位置やアイテム数は変えずに表示を更新する
		/// </summary>
		public void Refresh( bool tNow = true )
		{
			if( tNow == true )
			{
				UpdateItemList() ;	// 強制更新
			}
			else
			{
				m_ItemListDirty = true ;	// Start か Update のタイミングで実行する
			}
		}

		//-------------------------------------------------------------------------------------------

		// アイテムの幅を取得する
		public float defaultItemSize
		{
			get
			{
				if( item != null )
				{
					// 横スクロール
					if( direction == Direction.Horizontal )
					{
						return  item._w ;
					}
	
					// 縦スクロール
					if( direction == Direction.Vertical )
					{
						return item._h ;
					}
				}
				
				return 0 ;
			}
		}

		// アイテムのポジションを設定する
		private void SetItemPosition( UIView tView, float tPosition )
		{
			// 横スクロール
			if( direction == Direction.Horizontal )
			{
				tView._x =   tPosition ;
			}

			// 縦スクロール
			if( direction == Direction.Vertical )
			{
				tView._y = - tPosition ;
			}
		}

		// アイテムのサイズを設定する
		private void SetItemSize( UIView tView, float tSize )
		{
			// 横スクロール
			if( direction == Direction.Horizontal )
			{
				tView._w =   tSize ;
			}

			// 縦スクロール
			if( direction == Direction.Vertical )
			{
				tView._h =   tSize ;
			}
		}

		/// <summary>
		/// 表示上の最初の位置にあるアイテムのインデックスを取得する
		/// </summary>
		/// <returns></returns>
		public int headItemIndex
		{
			get
			{
				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}

				int tIndex = m_CurrentItemIndex ;

				float p0 = m_ItemHeadPosition ;
				float p1 ;

				float tPosition = contentPosition ;
				int i, l = m_ItemList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					p1 = p0 + m_ItemList[ i ].size ;
					if( tPosition >= p0 && tPosition <  p1 )
					{
						// ここ決定
						return tIndex ;
					}

					p0 = p1 ;

					tIndex ++ ;
				}

				return -1 ;	// 該当無し
			}
		}

		/// <summary>
		/// 表示上の最後の位置にあるアイテムのインデックスを取得する
		/// </summary>
		/// <returns></returns>
		public int tailItemIndex
		{
			get
			{
				if( m_ItemListDirty == true )
				{
					// 変化があったので更新をかける
//					SetContentPosition( contentPosition ) ;
					UpdateItemList() ;
				}

				int tIndex = m_CurrentItemIndex ;

				float p0 = m_ItemHeadPosition ;
				float p1 ;

				float tPosition = contentPosition + viewSize - 1 ;
				int i, l = m_ItemList.Count ;

				for( i  = 0 ; i <  l ; i ++ )
				{
					p1 = p0 + m_ItemList[ i ].size ;
					if( tPosition >= p0 && tPosition <  p1 )
					{
						// ここ決定
						return tIndex ;
					}

					p0 = p1 ;

					tIndex ++ ;
				}

				return -1 ;	// 該当無し
			}
		}

		/// <summary>
		/// スクロールが可能かどうか
		/// </summary>
		public bool IsScrollable
		{
			get
			{
				if( m_Infinity == false && contentSize <= viewSize )
				{
					return false ;
				}

				return true ;
			}
		}

		//-----------------------------------------------------------
		
		// 内部リスナー
		private float OnItemUpdatedInner( int tIndex, GameObject tItem, UnityEngine.Component tItemCode )
		{
			float tItemSize = 0 ;

			if( onItemUpdatedAction != null || onItemUpdatedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( onItemUpdatedAction != null )
				{
					tItemSize = onItemUpdatedAction( identity, this, tIndex, tItem ) ;
				}

				if( onItemUpdatedDelegate != null )
				{
					tItemSize = onItemUpdatedDelegate( identity, this, tIndex, tItem ) ;
				}
			}

			if( onItemClassUpdatedAction != null || onItemClassUpdatedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( onItemClassUpdatedAction != null )
				{
					tItemSize = onItemClassUpdatedAction( identity, this, tIndex, tItemCode ) ;
				}

				if( onItemClassUpdatedDelegate != null )
				{
					tItemSize = onItemClassUpdatedDelegate( identity, this, tIndex, tItemCode ) ;
				}
			}

			if( tItemSize <= 0 )
			{
				return defaultItemSize ;
			}

			return tItemSize ;
		}
		
		//-----------------------------------------------------------
		// GameObject

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクション
		/// </summary>
		Func<string, UIListView, int, GameObject, float> onItemUpdatedAction ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated( Func<string, UIListView, int, GameObject, float> tOnItemUpdatedAction )
		{
			if( onItemUpdatedAction != tOnItemUpdatedAction )
			{
				onItemUpdatedAction  = tOnItemUpdatedAction ;
			}
			m_ItemListDirty = true ;
		}
	
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tIndex">アイテムのインデックス番号</param>
		/// <param name="tItem">アイテムのゲームオブジェクトのインスタンス</param>
		public delegate float OnItemUpdatedDelegate( string tIdentity, UIListView tView, int tIndex, GameObject tItem ) ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲート
		/// </summary>
		public OnItemUpdatedDelegate onItemUpdatedDelegate ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnItemUpdatedDelegate">デリゲートメソッド</param>
		public void AddOnItemUpdated( OnItemUpdatedDelegate tOnItemUpdatedDelegate )
		{
			onItemUpdatedDelegate += tOnItemUpdatedDelegate ;
			m_ItemListDirty = true ;
		}
		
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnItemUpdatedDelegate">デリゲートメソッド</param>
		public void RemoveOnItemUpdated( OnItemUpdatedDelegate tOnItemUpdatedDelegate )
		{
			onItemUpdatedDelegate -= tOnItemUpdatedDelegate ;
			m_ItemListDirty = true ;
		}

		//-----------------------------------------------------------
		// Class

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクション
		/// </summary>
		Func<string, UIListView, int, UnityEngine.Component, float> onItemClassUpdatedAction ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnItemUpdatedAction">アクションメソッド</param>
		public void SetOnItemUpdated<T>( Func<string, UIListView, int, UnityEngine.Component, float> tOnItemClassUpdatedAction ) where T : UnityEngine.Component
		{
			m_TargetType = typeof( T ) ;

			if( onItemClassUpdatedAction != tOnItemClassUpdatedAction )
			{
				onItemClassUpdatedAction  = tOnItemClassUpdatedAction ;
			}
			m_ItemListDirty = true ;
		}
	
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tIndex">アイテムのインデックス番号</param>
		/// <param name="tItem">アイテムのゲームオブジェクトのインスタンス</param>
		public delegate float OnItemClassUpdatedDelegate( string tIdentity, UIListView tView, int tIndex, UnityEngine.Component tItem ) ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲート
		/// </summary>
		public OnItemClassUpdatedDelegate onItemClassUpdatedDelegate ;

		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnItemUpdatedDelegate">デリゲートメソッド</param>
		public void AddOnItemUpdated<T>( OnItemClassUpdatedDelegate tOnItemClassUpdatedDelegate ) where T : UnityEngine.Component
		{
			m_TargetType = typeof( T ) ;

			onItemClassUpdatedDelegate += tOnItemClassUpdatedDelegate ;
			m_ItemListDirty = true ;
		}
		
		/// <summary>
		/// アイテムの更新が必要な際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnItemUpdatedDelegate">デリゲートメソッド</param>
		public void RemoveOnItemUpdated<T>( OnItemClassUpdatedDelegate tOnItemClassUpdatedDelegate ) where T : UnityEngine.Component
		{
			m_TargetType = typeof( T ) ;

			onItemClassUpdatedDelegate -= tOnItemClassUpdatedDelegate ;
			m_ItemListDirty = true ;
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// 指定のインデックスまで移動させる
		/// </summary>
		/// <param name="tContentPosition"></param>
		/// <param name="tDuration"></param>
		/// <returns></returns>
		public MovableState MoveToIndex( int tContentIndex, float tDuration )
		{
			if( tDuration <= 0 )
			{
				return null ;
			}

			float tContentPosition = ConvertIndexToPosition( tContentIndex ) ;

			MovableState tState = new MovableState() ;
			StartCoroutine( MoveToPosition_Private( tContentPosition, tDuration, tState ) ) ;
			return tState ;
		}
	}
}
