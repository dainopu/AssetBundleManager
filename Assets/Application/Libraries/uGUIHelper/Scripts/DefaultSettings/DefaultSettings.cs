using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

//#if TextMeshPro
using TMPro ;
//#endif

namespace uGUIHelper
{
	/// <summary>
	/// デフォルト設定情報
	/// </summary>
	[CreateAssetMenu( fileName = "DefaultSettings", menuName = "ScriptableObject/uGUIHelper/DefaultSettings" )]
	public class DefaultSettings : ScriptableObject
	{
		// ボタン関係
		public Sprite			buttonFrame = null ;
		public Color			buttonDisabledColor = new Color( 0.75f, 0.75f, 0.75f, 0.5f ) ;

		public int				buttonLabelFontSize = 0 ;
		public Color			buttonLabelColor = Color.black ;
		public bool				buttonLabelShadow = false ;
		public bool				buttonLabelOutline = false ;


		// プログレス関係
		public Sprite			progressbarFrame = null ;
		public Sprite			progressbarThumb = null ;

		// テキスト関係
		public Color			textColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f ) ;

		// フォント関係
		public Font				font = null ;

//#if TextMeshPro
		public TMP_FontAsset	fontAsset = null ;
		public Material			fontMaterial = null ;
//#endif
		public int				fontSize = 0 ;

		// インプットフィールド関係
		public FontFilter		fontFilter = null ;
		public char				fontAlternateCode = '？' ;
	}
}
