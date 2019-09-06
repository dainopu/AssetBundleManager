using System ;
using UnityEngine ;

/// <summary>
/// ＭＯＥパッケージ
/// </summary>
namespace MultiplayerHelper
{
	/// <summary>
	/// 非同期処理待ち状態クラス
	/// </summary>
	public class MultiplayerRequest : CustomYieldInstruction
	{
		public MultiplayerRequest()
		{
		}

		public override bool keepWaiting
		{
			get
			{
				if( isDone == false && string.IsNullOrEmpty( error ) == true )
				{
					return true ;    // 継続
				}
				return false ;   // 終了
			}
		}

		/// <summary>
		/// 通信が終了したかどうか
		/// </summary>
		public bool isDone = false ;

		/// <summary>
		/// エラーが発生したかどうか
		/// </summary>
		public string error = "" ;
	}
}
