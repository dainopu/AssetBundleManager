using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text ;

using HTS_Engine_API ;


namespace OJT
{
	public partial class NJD
	{
		private const string NJD_SET_UNVOICED_VOWEL_FILLER		= "フィラー" ;
		private const string NJD_SET_UNVOICED_VOWEL_DOUSHI		= "動詞" ;
		private const string NJD_SET_UNVOICED_VOWEL_JODOUSHI	= "助動詞" ;
		private const string NJD_SET_UNVOICED_VOWEL_JOSHI		= "助詞" ;
		private const string NJD_SET_UNVOICED_VOWEL_KANDOUSHI	= "感動詞" ;
		private const string NJD_SET_UNVOICED_VOWEL_TOUTEN		= "、" ;
		private const string NJD_SET_UNVOICED_VOWEL_QUESTION	= "？" ;
		private const string NJD_SET_UNVOICED_VOWEL_QUOTATION	= "’" ;
		private const string NJD_SET_UNVOICED_VOWEL_SHI			= "シ" ;
		private const string NJD_SET_UNVOICED_VOWEL_MA			= "マ" ;
		private const string NJD_SET_UNVOICED_VOWEL_DE			= "デ" ;
		private const string NJD_SET_UNVOICED_VOWEL_CHOUON		= "ー" ;
		private const string NJD_SET_UNVOICED_VOWEL_SU			= "ス" ;


		private static string[] njd_set_unvoiced_vowel_candidate_list1 =
		{
			"スィ",	// s i
			"ス",	// s u
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_next_mora_list1 =
		{
			"カ",	// k ky
			"キ",
			"ク",
			"ケ",
			"コ",
			"タ",	// t ty ch ts
			"チ",
			"ツ",
			"テ",
			"ト",
			"ハ",	// h f hy
			"ヒ",
			"フ",
			"ヘ",
			"ホ",
			"パ",	// p py
			"ピ",
			"プ",
			"ペ",
			"ポ",
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_candidate_list2 =
		{
			"フィ",	// f i
			"ヒ",	// h i
			"フ",	// f u
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_next_mora_list2 =
		{
			"カ",	// k ky
			"キ",
			"ク",
			"ケ",
			"コ",
			"サ",	// s sh
			"シ",
			"ス",
			"セ",
			"ソ",
			"タ",	// t ty ch ts
			"チ",
			"ツ",
			"テ",
			"ト",
			"パ",	// p py
			"ピ",
			"プ",
			"ペ",
			"ポ",
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_candidate_list3 =
		{
			"キュ",	// ky u
			"シュ",	// sh u
			"チュ",	// ch u
			"ツィ",	// ts i
			"ヒュ",	// hy u
			"ピュ",	// py u
			"テュ",	// ty u
			"トゥ",	// t u
			"ティ",	// t i
			"キ",	// k i
			"ク",	// k u
			"シ",	// sh i
			"チ",	// ch i
			"ツ",	// ts u
			"ピ",	// p i
			"プ",	// p u
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_next_mora_list3 =
		{
			"カ",	// k ky
			"キ",
			"ク",
			"ケ",
			"コ",
			"サ",	// s sh
			"シ",
			"ス",
			"セ",
			"ソ",
			"タ",	// t ty ch ts
			"チ",
			"ツ",
			"テ",
			"ト",
			"ハ",	// h f hy
			"ヒ",
			"フ",
			"ヘ",
			"ホ",
			"パ",	// p py
			"ピ",
			"プ",
			"ペ",
			"ポ",
			null
		} ;

		private static string[] njd_set_unvoiced_vowel_mora_list =
		{
			"ヴョ",
			"ヴュ",
			"ヴャ",
			"ヴォ",
			"ヴェ",
			"ヴィ",
			"ヴァ",
			"ヴ",
			"ン",
			"ヲ",
			"ヱ",
			"ヰ",
			"ワ",
			"ロ",
			"レ",
			"ル",
			"リョ",
			"リュ",
			"リャ",
			"リェ",
			"リ",
			"ラ",
			"ヨ",
			"ョ",
			"ユ",
			"ュ",
			"ヤ",
			"ャ",
			"モ",
			"メ",
			"ム",
			"ミョ",
			"ミュ",
			"ミャ",
			"ミェ",
			"ミ",
			"マ",
			"ポ",
			"ボ",
			"ホ",
			"ペ",
			"ベ",
			"ヘ",
			"プ",
			"ブ",
			"フォ",
			"フェ",
			"フィ",
			"ファ",
			"フ",
			"ピョ",
			"ピュ",
			"ピャ",
			"ピェ",
			"ピ",
			"ビョ",
			"ビュ",
			"ビャ",
			"ビェ",
			"ビ",
			"ヒョ",
			"ヒュ",
			"ヒャ",
			"ヒェ",
			"ヒ",
			"パ",
			"バ",
			"ハ",
			"ノ",
			"ネ",
			"ヌ",
			"ニョ",
			"ニュ",
			"ニャ",
			"ニェ",
			"ニ",
			"ナ",
			"ドゥ",
			"ド",
			"トゥ",
			"ト",
			"デョ",
			"デュ",
			"デャ",
			"ディ",
			"デ",
			"テョ",
			"テュ",
			"テャ",
			"ティ",
			"テ",
			"ヅ",
			"ツォ",
			"ツェ",
			"ツィ",
			"ツァ",
			"ツ",
			"ッ",
			"ヂ",
			"チョ",
			"チュ",
			"チャ",
			"チェ",
			"チ",
			"ダ",
			"タ",
			"ゾ",
			"ソ",
			"ゼ",
			"セ",
			"ズィ",
			"ズ",
			"スィ",
			"ス",
			"ジョ",
			"ジュ",
			"ジャ",
			"ジェ",
			"ジ",
			"ショ",
			"シュ",
			"シャ",
			"シェ",
			"シ",
			"ザ",
			"サ",
			"ゴ",
			"コ",
			"ゲ",
			"ケ",
			"グ",
			"ク",
			"ギョ",
			"ギュ",
			"ギャ",
			"ギェ",
			"ギ",
			"キョ",
			"キュ",
			"キャ",
			"キェ",
			"キ",
			"ガ",
			"カ",
			"オ",
			"ォ",
			"エ",
			"ェ",
			"ウォ",
			"ウェ",
			"ウィ",
			"ウ",
			"ゥ",
			"イェ",
			"イ",
			"ィ",
			"ア",
			"ァ",
			"ー",
			null
		} ;

	}
}

