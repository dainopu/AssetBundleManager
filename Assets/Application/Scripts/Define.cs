using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace AssetableExperiment
{
	/// <summary>
	/// 共通定義クラス Version 2017/08/13 0
	/// </summary>
	public class Define
	{
		/// <summary>
		/// 暗号化キー
		/// </summary>
		public const string cryptoKey		= "lkirwf897+22#bbtrm8814z5qq=498j5" ;

		/// <summary>
		/// 暗号化ベクター
		/// </summary>
		public const string cryptoVector	= "741952hheeyy66#cs!9hjv887mxx7@8y" ;

		/// <summary>
		/// プレイヤーデータの保存フォルダ
		/// </summary>
		public const string folder			= "preference/" ;


		/// <summary>
		/// アセットバンドル用のプラッフォーム名を取得する
		/// </summary>
		public static string AssetBundlePlatformName
		{
			get
			{
				string platformName = "Windows" ;


#if UNITY_ANDROID
				platformName = "Android" ;
#elif UNITY_IOS || UNITY_IPHONE
				platformName = "iOS" ;
#endif

				return platformName ;
			}
		}

	}
}
