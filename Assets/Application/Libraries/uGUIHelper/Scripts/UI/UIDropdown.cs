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
	[ RequireComponent( typeof( UnityEngine.UI.Dropdown ) ) ]
	public class UIDropdown : UIImage
	{
		/// <summary>
		/// captionText(ショートカット)
		/// </summary>
		public UIText captionText
		{
			get
			{
				if( _dropdown == null )
				{
					return null ;
				}

				return _dropdown.captionText.GetComponent<UIText>() ;
			}
		}


		/// <summary>
		/// 各派生クラスでの初期化処理を行う（メニューまたは AddView から生成される場合のみ実行れる）
		/// </summary>
		/// <param name="tOption"></param>
		override protected void OnBuild( string tOption = "" )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				tDropdown = gameObject.AddComponent<Dropdown>() ;
			}
			if( tDropdown == null )
			{
				// 異常
				return ;
			}

			Image tImage = _image ;

			//------------------------------------------

			Vector2 tSize = GetCanvasSize() ;
			if( tSize.x >  0 && tSize.y >  0 )
			{
				SetSize( tSize.y * 0.28f, tSize.y * 0.05f ) ;
			}
				
			ColorBlock tColorBlock = tDropdown.colors ;
			tColorBlock.fadeDuration = 0.1f ;
			tDropdown.colors = tColorBlock ;
				
			tImage.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultButton" ) ;
			tImage.color = Color.white ;
			tImage.type = Image.Type.Sliced ;

			// 初期のオプションを追加する
			Dropdown.OptionData tDataA = new Dropdown.OptionData() ;
			tDataA.text = "Option A" ;
			tDropdown.options.Add( tDataA ) ;
			Dropdown.OptionData tDataB = new Dropdown.OptionData() ;
			tDataB.text = "Option B" ;
			tDropdown.options.Add( tDataB ) ;
			Dropdown.OptionData tDataC = new Dropdown.OptionData() ;
			tDataC.text = "Option C" ;
			tDropdown.options.Add( tDataC ) ;

			// Label
			UIText tLabel = AddView<UIText>( "Label", "FitOff" ) ;
			tLabel.text = tDropdown.options[ 0 ].text ;
			tLabel.SetAnchorToStretch() ;
			tLabel.SetMargin( 10, 25,  7,  6 ) ;
			tLabel._text.alignment = TextAnchor.MiddleLeft ;
//			tLabel._text.fontSize = 0 ;
			tLabel._text.color = new Color32(  50,  50,  50, 255 ) ;

			tDropdown.captionText = tLabel._text ;


			// Arrow
			UIImage tArrow = AddView<UIImage>( "Arrow" ) ;
			tArrow.sprite = Resources.Load<Sprite>( "uGUIHelper/Textures/UIDefaultArrowDown" ) ;
			tArrow.color = Color.white ;
			tArrow.type = Image.Type.Sliced ;
				
			tArrow.SetAnchorToRightMiddle() ;
			tArrow.SetPosition( -18,   0 ) ;
			float s = _h * 0.6f ;
			tArrow.SetSize( s, s ) ;


			// ScrollView
			UIScrollView tScrollView = AddView<UIScrollView>( "Template", "Dropdown" ) ;
			tScrollView.SetAnchorToStretchBottom() ;
			tScrollView.SetPosition(  0,  2 ) ;
			tScrollView.SetSize(   0, _h * 5 ) ;
			tScrollView.SetPivot( 0.5f, 1.0f ) ;
	//		tScrollView.SetColor( 0xFFFFFFFF ) ;
			tScrollView.isVerticalScrollber = true ;
			tScrollView._scrollRect.verticalScrollbarSpacing = -2 ;

			tScrollView.content._h = _h ;

			tDropdown.template = tScrollView._rectTransform ;

			// テンプレートアイテムを１つ追加する
			tScrollView.dropdownItem._h = _h ;

			// 最後に無効化
			tScrollView.SetActive( false ) ;
				
			ResetRectTransform() ;
		}

		/// <summary>
		/// 派生クラスの Start
		/// </summary>
		override protected void OnStart()
		{
			base.OnStart() ;

			// 注意:実行のみにしておかないと ExecuteInEditMode で何度も登録されてしまう
			if( Application.isPlaying == true )
			{
				// カスタムリスナー登録（Awake 起動毎に実行する必要がある）
				if( _dropdown != null )
				{
					_dropdown.onValueChanged.AddListener( OnValueChangedInner ) ;
				}
			}
		}

		//---------------------------------------------

		/// <summary>
		/// 状態が変化した際に呼び出されるアクション
		/// </summary>
		public Action<string, UIDropdown, int> onValueChangedAction ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートの定義
		/// </summary>
		/// <param name="tIdentity">ビューの識別名(未設定の場合はゲームオブジェクト名)</param>
		/// <param name="tView">ビューのインスタンス</param>
		/// <param name="tValue">変化後の値</param>
		public delegate void OnValueChangedDelegate( string tIdentity, UIDropdown tView, int tValue ) ;

		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲート
		/// </summary>
		public OnValueChangedDelegate onValueChangedDelegate ;

		/// <summary>
		/// 状態が変化した際に呼び出されるアクションを設定する
		/// </summary>
		/// <param name="tOnValueChangedAction">アクションメソッド</param>
		public void SetOnValueChanged( Action<string, UIDropdown, int> tOnValueChangedAction )
		{
			onValueChangedAction = tOnValueChangedAction ;
		}
	
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを追加する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void AddOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate += tOnValueChangedDelegate ;
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるデリゲートを削除する
		/// </summary>
		/// <param name="tOnValueChangedDelegate">デリゲートメソッド</param>
		public void RemoveOnValueChanged( OnValueChangedDelegate tOnValueChangedDelegate )
		{
			onValueChangedDelegate -= tOnValueChangedDelegate ;
		}
	
		// 内部リスナー登録
		private void OnValueChangedInner( int tValue )
		{
			if( onValueChangedAction != null || onValueChangedDelegate != null )
			{
				string identity = Identity ;
				if( string.IsNullOrEmpty( identity ) == true )
				{
					identity = name ;
				}

				if( onValueChangedAction != null )
				{
					onValueChangedAction( identity, this, tValue ) ;
				}

				if( onValueChangedDelegate != null )
				{
					onValueChangedDelegate( identity, this, tValue ) ;
				}
			}
		}
	
		//---------------------------------------------
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを追加する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void AddOnValueChangedListener( UnityEngine.Events.UnityAction<int> tOnValueChanged )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown != null )
			{
				tDropdown.onValueChanged.AddListener( tOnValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを削除する
		/// </summary>
		/// <param name="tOnValueChanged">リスナーメソッド</param>
		public void RemoveOnValueChangedListener( UnityEngine.Events.UnityAction<int> tOnValueChanged )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown != null )
			{
				tDropdown.onValueChanged.RemoveListener( tOnValueChanged ) ;
			}
		}
		
		/// <summary>
		/// 状態が変化した際に呼び出されるリスナーを全て削除する
		/// </summary>
		public void RemoveOnValueChangedAllListeners()
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown != null )
			{
				tDropdown.onValueChanged.RemoveAllListeners() ;
			}
		}

		//-------------------------------------------------------------------------------------------

		/// <summary>
		/// オプションデータをまとめて設定する
		/// </summary>
		/// <param name="tName"></param>
		public bool Set( string[] tName, int tInitailValue = -1 )
		{
			if( tName == null )
			{
				return false ;
			}

			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				return false ;
			}

			int tValue = tDropdown.value ;

			tDropdown.value = 0 ;
			tDropdown.ClearOptions() ;

			int i, l = tName.Length ;
			Dropdown.OptionData tData = null ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tData = new Dropdown.OptionData() ;
				tData.text = tName[ i ] ;
				tDropdown.options.Add( tData ) ;
			}

			tDropdown.captionText.text = tDropdown.options[ _dropdown.value ].text ;

			// デフォルトカーソル位置
			if( tInitailValue >= 0 )
			{
				tValue  = tInitailValue ;
			}

			value = tValue ;

			return true ;
		}

		/// <summary>
		/// オプションデータを追加する
		/// </summary>
		/// <param name="tName"></param>
		/// <returns></returns>
		public bool Add( string tName )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				return false ;
			}

			Dropdown.OptionData tData = new Dropdown.OptionData() ;
			tData.text = tName ;
			tDropdown.options.Add( tData ) ;

			return true ;
		}

		/// <summary>
		/// オプションデータを挿入する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <param name="tName"></param>
		/// <returns></returns>
		public bool Insert( int tIndex, string tName )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				return false ;
			}

			Dropdown.OptionData tData = new Dropdown.OptionData() ;
			tData.text = tName ;
			tDropdown.options.Insert( tIndex, tData ) ;

			return true ;
		}


		/// <summary>
		/// オプションデータを削除する
		/// </summary>
		/// <param name="tIndex"></param>
		/// <returns></returns>
		public bool RemoveAt( int tIndex )
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				return false ;
			}

			tDropdown.options.RemoveAt( tIndex ) ;

			return true ;
		}

		/// <summary>
		/// カーソル位置
		/// </summary>
		public int value
		{
			get
			{
				Dropdown tDropdown = _dropdown ;
				if( tDropdown == null )
				{
					return 0 ;
				}
				return tDropdown.value ;
			}
			set
			{
				Dropdown tDropdown = _dropdown ;
				if( tDropdown == null )
				{
					return ;
				}

				if( value >= 0 && value <  tDropdown.options.Count )
				{
					tDropdown.value = value ;
				}
			}
		}

		/// <summary>
		/// 現在のカーソル位置の項目名
		/// </summary>
		public string label
		{
			get
			{
				Dropdown tDropdown = _dropdown ;
				if( tDropdown == null )
				{
					return "" ;
				}

				if( tDropdown.options != null && tDropdown.value >= 0 && tDropdown.value <   tDropdown.options.Count )
				{
					return tDropdown.options[ tDropdown.value ].text ;
				}

				return "" ;
			}
			set
			{
				Dropdown tDropdown = _dropdown ;
				if( tDropdown == null )
				{
					return ;
				}

				if( tDropdown.options != null && tDropdown.value >= 0 && tDropdown.value <   tDropdown.options.Count )
				{
					tDropdown.options[ tDropdown.value ].text = value ;
				}
			}
		}

		/// <summary>
		/// 項目一覧を取得する
		/// </summary>
		/// <returns></returns>
		public string[] Get()
		{
			Dropdown tDropdown = _dropdown ;
			if( tDropdown == null )
			{
				return null ;
			}

			if( tDropdown.options == null || tDropdown.options.Count == 0 )
			{
				return null ;
			}

			int i, l = tDropdown.options.Count ;

			string[] tLabels = new string[ l ] ;

			for( i  = 0 ; i <  l ; i ++ )
			{
				tLabels[ i ] = tDropdown.options[ i ].text ;
			}

			return tLabels ;
		}

		/// <summary>
		/// インデクサ(項目一覧へのショートカットアクセス)
		/// </summary>
		public string this[ int tIndex ]
		{
			get
			{
				Dropdown tDropdown = _dropdown ;
				if( tDropdown == null )
				{
					return null ;
				}

				if( tDropdown.options != null && tDropdown.value >= 0 && tDropdown.value <   tDropdown.options.Count )
				{
					return tDropdown.options[ tIndex ].text ;
				}

				return null ;
			}
		}
	}
}
