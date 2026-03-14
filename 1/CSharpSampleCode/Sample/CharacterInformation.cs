using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CharacterInformations
{
	/// <summary>
	/// JISX0213水準列挙型
	/// </summary>
	public enum JISX0213Levels : int
	{
		None = 0,
		Lv1 = 1,				// 第一水準
		Lv2 = 2,				// 第二水準
		Lv3 = 3,				// 第三水準
		Lv4 = 4,				// 第四水準
		Hikanji = 5,			// 非漢字
	}

	/// <summary>
	/// 常用・人名用漢字列挙型
	/// </summary>
	public enum NameTypes : int
	{
		None = 0,
		Jyouyou = 1,			// 常用漢字
		Jinmei = 2,				// 人名用漢字(常用漢字以外)
		JyouyouItaiji = 3,		// 常用漢字の異体字
		JinmeiHikanji = 4,		// 子の名に使える非漢字
	}

	/// <summary>
	/// 学年別漢字配当列挙型
	/// </summary>
	public enum GakunenbetuKanjis : int
	{
		None = 0,
		S1 = 1,					// 小学1年
		S2 = 2,					// 小学2年
		S3 = 3,					// 小学3年
		S4 = 4,					// 小学4年
		S5 = 5,					// 小学5年
		S6 = 6,					// 小学6年
	}

	/// <summary>
	/// 文字情報辞書クラス
	/// </summary>
	public class CharacterInformationDictionary : Dictionary<string, InfomationRecord>
	{
	}

	/// <summary>
	/// 文字情報クラス
	/// </summary>
	public class CharacterInformation
	{
		/// <summary>
		/// メジャーバージョン
		/// </summary>
		private int majorVer;

		/// <summary>
		/// メジャーバージョン取得
		/// </summary>
		public int MajorVer
		{
			get
			{
				return this.majorVer;
			}
		}

		/// <summary>
		/// マイナーバージョン
		/// </summary>
		private int minorVer;

		/// <summary>
		/// マイナーバージョン取得
		/// </summary>
		public int MinorVer
		{
			get
			{
				return this.minorVer;
			}
		}

		/// <summary>
		/// 文字情報辞書
		/// </summary>
		private CharacterInformationDictionary items;

		/// <summary>
		/// 文字情報辞書取得
		/// </summary>
		public CharacterInformationDictionary Items
		{
			get
			{
				return this.items;
			}
		}

		/// <summary>
		/// 情報レコード取得(String)
		/// </summary>
		/// <param name="pMoji">キー文字列</param>
		/// <returns>InfomationRecord or null</returns>
		/// <remarks>pMojiに該当する情報が見つからない場合はnullを返す。</remarks>
		public InfomationRecord this[string pMoji]
		{
			get
			{
				if (this.items.ContainsKey(pMoji)) {
					return this.items[pMoji];
				}
				return null;
			}
		}

		/// <summary>
		/// 情報レコード取得(Char)
		/// </summary>
		/// <param name="pChar">キー文字</param>
		/// <returns>InfomationRecord or null</returns>
		/// <remarks>pCharに該当する情報が見つからない場合はnullを返す。</remarks>
		public InfomationRecord this[char pChar]
		{
			get
			{
				string moji = pChar.ToString();
				if (this.items.ContainsKey(moji)) {
					return this.items[moji];
				}
				return null;
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CharacterInformation()
		{
			this.items = new CharacterInformationDictionary();
			this.majorVer = 0;
			this.minorVer = 0;
		}

		/// <summary>
		/// 定義データ読み込み(ファイルパス渡し)
		/// </summary>
		/// <param name="pSettingFilePath">定義ファイルのパス</param>
		/// <remarks>
		/// 定義データを読み込みインスタンスを初期化する。
		/// </remarks>
		public void Load(string pSettingFilePath)
		{
			using (StreamReader reader = new StreamReader(pSettingFilePath, Encoding.UTF8)) {
				this.Load(reader);
				reader.Close();
			}
		}

		/// <summary>
		/// 定義データ読み込み(TextReader渡し)
		/// </summary>
		/// <param name="pReader">TextReader</param>
		/// <remarks>
		/// 定義データを読み込みインスタンスを初期化する。
		/// </remarks>
		public void Load(TextReader pReader)
		{
			this.items.Clear();
			this.majorVer = 0;
			this.minorVer = 0;

			int dataCount = 0;

			while (true) {
				string line = pReader.ReadLine();
				if (line == null) {
					break;
				}
				if ((line.Length == 0) || (line[0] == '#')) {
					continue;
				}

				dataCount++;

				if (dataCount == 1) {
					string[] fields = line.Split('.');

					this.majorVer = int.Parse(fields[0]);
					if (majorVer != 1) {
						throw new InvalidDataException("認識できないデータ(バージョン)");
					}

					this.minorVer = int.Parse(fields[1]);

					continue;
				}

				InfomationRecord rec = this.MakeInformationRecord(line);
				this.items[rec.KeyUnicodeString] = rec;
			}
		}

		/// <summary>
		/// 情報レコード取得(String,Safe)
		/// </summary>
		/// <param name="pMoji">キー文字列</param>
		/// <returns>InfomationRecord</returns>
		/// <remarks>
		/// pMojiに該当する情報を返す。this[pMoji]と違いnullを返さない。
		/// 該当する情報が見つからない場合はキーにpMojiをセットしたInfomationRecordを返す。
		/// </remarks>
		public InfomationRecord GetAtSafe(string pMoji)
		{
			InfomationRecord result = this[pMoji];
			if (result != null) {
				return result;
			}
			return new InfomationRecord(pMoji);
		}

		/// <summary>
		/// 情報レコード取得(Char,Safe)
		/// </summary>
		/// <param name="pChar">キー文字</param>
		/// <returns>InfomationRecord</returns>
		/// <remarks>
		/// pCharに該当する情報を返す。this[pChar]と違いnullを返さない。
		/// 該当する情報が見つからない場合はキーにpCharをセットしたInfomationRecordを返す。
		/// </remarks>
		public InfomationRecord GetAtSafe(char pChar)
		{
			string moji = pChar.ToString();
			InfomationRecord result = this[moji];
			if (result != null) {
				return result;
			}
			return new InfomationRecord(moji);
		}

		/// <summary>
		/// 順次１文字返却
		/// </summary>
		/// <param name="pString">解析文字列</param>
		/// <returns>文字(サロゲートペアあり)</returns>
		/// <remarks>
		/// pStringから１文字を切り出して順次返却する。
		/// サロゲートペアの場合は上位・下位サロゲートを組み立てて返却する。
		/// よって、返却される文字のLengthは1もしくは2となる。
		/// </remarks>
		static public IEnumerable<string> GetOneCharacterEnumerator(string pString)
		{
			for (int i = 0; i < pString.Length; i++) {
				char c = pString[i];

				if (char.IsHighSurrogate(c) && ((i + 1) < pString.Length)) {
					char cc = pString[i + 1];
					if (char.IsLowSurrogate(cc)) {
						i++;
						yield return c.ToString() + cc.ToString();
						continue;
					}
				}

				yield return c.ToString();
			}
		}

		/// <summary>
		/// 情報レコード作成
		/// </summary>
		/// <param name="pRecord">レコード文字列</param>
		/// <returns>InfomationRecord</returns>
		private InfomationRecord MakeInformationRecord(string pRecord)
		{
			string[] fields = pRecord.Split(',');
			InfomationRecord result = new InfomationRecord();

			if (fields.Length < 1) {
				throw new InvalidDataException("キーとなるコードポイントが定義されていない");
			}

			do {
				int idx = 0;

				// key
				result.SetUnicodeKey(MakeStringFromCodePoints(fields[idx]));
				idx++;

				// JISX0213Levels
				if (fields.Length <= idx) {
					break;
				}
				string strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetJISX0213Level((JISX0213Levels)int.Parse(strwk));
				}
				idx++;

				// NameTypes
				if (fields.Length <= idx) {
					break;
				}
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetNameType((NameTypes)int.Parse(strwk));
				}
				idx++;

				// ETaxAvailable
				if (fields.Length <= idx) {
					break;
				}
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetETaxAvailable(int.Parse(strwk) != 0);
				}
				idx++;

				// GakunenbetuKanji
				if (fields.Length <= idx) {
					break;
				}
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetGakunenbetuKanji((GakunenbetuKanjis)int.Parse(strwk));
				}
				idx++;

				// NISAAvailable
				if (fields.Length <= idx) {
					break;
				}
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetNISAAvailable(int.Parse(strwk) != 0);
				}
				idx++;

				// EELTAXAvailable
				if (fields.Length <= idx) {
					break;
				}
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) {
					result.SetELTAXAvailable(int.Parse(strwk) != 0);
				}
				idx++;

				// 組番号 285 BASIC JAPANESE
				if (fields.Length <= idx) { break; }
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) { result.SetJA_BASIC_JAPANESE(int.Parse(strwk) != 0); }
				idx++;
				// 組番号 371 JIS2004 IDEOGRAPHICS EXTENSION
				if (fields.Length <= idx) { break; }
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) { result.SetJA_JIS2004_IDEOGRAPHICS_EXTENSION(int.Parse(strwk) != 0); }
				idx++;
				// 組番号 372 JAPANESE IDEOGRAPHICS SUPPLEMENT
				if (fields.Length <= idx) { break; }
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) { result.SetJA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT(int.Parse(strwk) != 0); }
				idx++;
				// 組番号 286 JAPANESE NON IDEOGRAPHICS EXTENSION
				if (fields.Length <= idx) { break; }
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) { result.SetJA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION(int.Parse(strwk) != 0); }
				idx++;
				// 組番号 287 COMMON JAPANESE
				if (fields.Length <= idx) { break; }
				strwk = fields[idx].Trim();
				if (strwk.Length >= 1) { result.SetJA_COMMON_JAPANESE(int.Parse(strwk) != 0); }
				idx++;
			} while (false);

			return result;
		}

		/// <summary>
		/// コードポイント文字列より文字列を作成
		/// </summary>
		/// <param name="pCodePoint">コードポイント文字列(ex.U+304B+3099)</param>
		/// <returns>string</returns>
		static public string MakeStringFromCodePoints(string pCodePoint)
		{
			List<byte> bytetbl = new List<byte>(4 * 2);
			foreach (string hexcode in pCodePoint.Split('+')) {
				if (Uri.IsHexDigit(hexcode[0])) {
					bytetbl.AddRange(BitConverter.GetBytes(Convert.ToInt32(hexcode, 16)));
				}
			}
			if (bytetbl.Count == 0) {
				throw new InvalidDataException("キーとなるコードポイントが定義されていない");
			}

			return Encoding.UTF32.GetString(bytetbl.ToArray());
		}

#if DEBUG
		public string DebugDump()
		{
			StringBuilder sb = new StringBuilder();

			// header
			sb.AppendLine("DebugDump in ---");
			sb.AppendFormat("DataVer:{0}.{1}\n", this.majorVer, this.minorVer);
			sb.AppendFormat("Records:{0}\n", this.items.Count);
			sb.AppendLine("");

			SortedDictionary<JISX0213Levels, int> jisx0213levelcount = new SortedDictionary<JISX0213Levels, int>();
			SortedDictionary<NameTypes, int> nametypecount = new SortedDictionary<NameTypes, int>();
			SortedDictionary<bool, int> etaxcount = new SortedDictionary<bool, int>();
			SortedDictionary<GakunenbetuKanjis, int> gakunenbetukanjicount = new SortedDictionary<GakunenbetuKanjis, int>();
			SortedDictionary<bool, int> nisacount = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> eltaxcount = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> jaBJ = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> jaJIE = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> jaJIS = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> jaJNIE = new SortedDictionary<bool, int>();
			SortedDictionary<bool, int> jaCJ = new SortedDictionary<bool, int>();
			foreach (InfomationRecord item in this.items.Values) {
				jisx0213levelcount.Increment(item.JISX0213Level);
				nametypecount.Increment(item.NameType);
				etaxcount.Increment(item.ETaxAvailable);
				gakunenbetukanjicount.Increment(item.GakunenbetuKanji);
				nisacount.Increment(item.NISAAvailable);
				eltaxcount.Increment(item.ELTAXAvailable);
				jaBJ.Increment(item.JA_BASIC_JAPANESE);
				jaJIE.Increment(item.JA_JIS2004_IDEOGRAPHICS_EXTENSION);
				jaJIS.Increment(item.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT);
				jaJNIE.Increment(item.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION);
				jaCJ.Increment(item.JA_COMMON_JAPANESE);
			}

			// JIS X 0213
			sb.AppendLine("JISX0213Levels");
			int total = 0, totalwithoutnone = 0;
			foreach (KeyValuePair<JISX0213Levels, int> item in jisx0213levelcount) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
				if (item.Key != JISX0213Levels.None) {
					totalwithoutnone += item.Value;
				}
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0} (without None:{1})\n", total, totalwithoutnone);
			sb.AppendLine("");

			// 常用・人名用漢字
			sb.AppendLine("NameTypes");
			total = totalwithoutnone = 0;
			foreach (KeyValuePair<NameTypes, int> item in nametypecount) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
				if (item.Key != NameTypes.None) {
					totalwithoutnone += item.Value;
				}
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0} (without None:{1})\n", total, totalwithoutnone);
			sb.AppendLine("");

			// e-Tax
			sb.AppendLine("e-Tax");
			total = totalwithoutnone = 0;
			foreach (KeyValuePair<bool, int> item in etaxcount) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
				if (item.Key != false) {
					totalwithoutnone += item.Value;
				}
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0} (without False:{1})\n", total, totalwithoutnone);
			sb.AppendLine("");

			// 学年別漢字配当
			sb.AppendLine("GakunenbetuKanji");
			total = totalwithoutnone = 0;
			foreach (KeyValuePair<GakunenbetuKanjis, int> item in gakunenbetukanjicount) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
				if (item.Key != GakunenbetuKanjis.None) {
					totalwithoutnone += item.Value;
				}
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0} (without None:{1})\n", total, totalwithoutnone);
			sb.AppendLine("");

			// NISA
			sb.AppendLine("NISA");
			total = totalwithoutnone = 0;
			foreach (KeyValuePair<bool, int> item in nisacount) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
				if (item.Key != false) {
					totalwithoutnone += item.Value;
				}
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0} (without False:{1})\n", total, totalwithoutnone);
			sb.AppendLine("");

			// eLTAX
			Dump(sb, "eLTAX", eltaxcount);

			// JA 
			Dump(sb, "JA BASIC_JAPANESE", jaBJ);
			Dump(sb, "JA JIS2004_IDEOGRAPHICS_EXTENSION", jaJIE);
			Dump(sb, "JA JAPANESE_IDEOGRAPHICS_SUPPLEMENT", jaJIS);
			Dump(sb, "JA JAPANESE_NON_IDEOGRAPHICS_EXTENSION", jaJNIE);
			Dump(sb, "JA COMMON_JAPANESE", jaCJ);

			// end
			sb.AppendLine("DebugDump out --");

			return sb.ToString();
		}

		public void Dump(StringBuilder sb, string pTitle, IDictionary<bool, int> pDic)
		{
			sb.AppendLine(pTitle);
			int total = 0;
			foreach (KeyValuePair<bool, int> item in pDic) {
				sb.AppendFormat(" {0}:{1}\n", item.Key.ToString(), item.Value);
				total += item.Value;
			}
			sb.AppendLine(" -----");
			sb.AppendFormat(" Total:{0}\n", total);
			sb.AppendLine("");
		}


		static public string DebugStringsDump(CharacterInformation pCharInfo, string pString)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(pString);

			UTF32Encoding utf32withoutbom = new UTF32Encoding(false, false);

			List<string> characterList = new List<string>(GetOneCharacterEnumerator(pString));

			for (int i = 0; i < characterList.Count; i++) {
				string moji = characterList[i];
				sb.AppendFormat("[{0}]:{1}", i + 1, moji);

				byte[] bytetbl = utf32withoutbom.GetBytes(moji);
				for (int j = 0; j < bytetbl.Length; j += 4) {
					if (j == 0) {
						sb.Append("(U+");
					} else {
						sb.Append("+");
					}
					sb.AppendFormat("{0:X}", BitConverter.ToInt32(bytetbl, j));
				}
				sb.Append(")");

				InfomationRecord charInfo = pCharInfo[characterList[i]];
				if (charInfo != null) {
					if (charInfo.JISX0213Level != JISX0213Levels.None) { sb.AppendFormat(" {0}", charInfo.JISX0213Level); }
					if (charInfo.NameType != NameTypes.None) { sb.AppendFormat(" {0}", charInfo.NameType); }
					if (charInfo.ETaxAvailable) { sb.AppendFormat(" e-Tax"); }
					if (charInfo.GakunenbetuKanji != GakunenbetuKanjis.None) { sb.AppendFormat(" {0}", charInfo.GakunenbetuKanji); }
					if (charInfo.NISAAvailable) { sb.AppendFormat(" NISA"); }
					if (charInfo.ELTAXAvailable) { sb.AppendFormat(" eLTAX"); }
					if (charInfo.JA_BASIC_JAPANESE) { sb.AppendFormat(" JA_BASIC_JAPANESE"); }
					if (charInfo.JA_JIS2004_IDEOGRAPHICS_EXTENSION) { sb.AppendFormat(" JA_JIS2004_IDEOGRAPHICS_EXTENSION"); }
					if (charInfo.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT) { sb.AppendFormat(" JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT"); }
					if (charInfo.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION) { sb.AppendFormat(" JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION"); }
					if (charInfo.JA_COMMON_JAPANESE) { sb.AppendFormat(" JA_COMMON_JAPANESE"); }
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
#endif
	}

	/// <summary>
	/// 情報レコードクラス
	/// </summary>
	public class InfomationRecord
	{
		/// <summary>
		/// 文字列キー取得
		/// </summary>
		public string KeyUnicodeString { get; protected set; }

		/// <summary>
		/// JISX0213水準種類取得
		/// </summary>
		public JISX0213Levels JISX0213Level { get; protected set; }

		/// <summary>
		/// 常用・人名用漢字種類取得
		/// </summary>
		public NameTypes NameType { get; protected set; }

		/// <summary>
		/// e-Taxで利用可能
		/// </summary>
		public bool ETaxAvailable { get; protected set; }

		/// <summary>
		/// 学年別漢字配当取得
		/// </summary>
		public GakunenbetuKanjis GakunenbetuKanji { get; protected set; }

		/// <summary>
		/// NISAで利用可能
		/// </summary>
		public bool NISAAvailable { get; protected set; }

		/// <summary>
		/// eLTAXで利用可能
		/// </summary>
		public bool ELTAXAvailable { get; protected set; }

		/// <summary>
		/// 組番号 285 BASIC JAPANESE
		/// </summary>
		public bool JA_BASIC_JAPANESE { get; protected set; }

		/// <summary>
		/// 組番号 371 JIS2004 IDEOGRAPHICS EXTENSION
		/// </summary>
		public bool JA_JIS2004_IDEOGRAPHICS_EXTENSION { get; protected set; }

		/// <summary>
		/// 組番号 372 JAPANESE IDEOGRAPHICS SUPPLEMENT
		/// </summary>
		public bool JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT { get; protected set; }

		/// <summary>
		/// 組番号 286 JAPANESE NON IDEOGRAPHICS EXTENSION
		/// </summary>
		public bool JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION { get; protected set; }

		/// <summary>
		/// 組番号 287 COMMON JAPANESE
		/// </summary>
		public bool JA_COMMON_JAPANESE { get; protected set; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public InfomationRecord()
		{
			this.KeyUnicodeString = "";
			this.JISX0213Level = JISX0213Levels.None;
			this.NameType = NameTypes.None;
			this.ETaxAvailable = false;
			this.GakunenbetuKanji = GakunenbetuKanjis.None;
			this.NISAAvailable = false;
			this.ELTAXAvailable = false;
			this.JA_BASIC_JAPANESE = false;
			this.JA_JIS2004_IDEOGRAPHICS_EXTENSION = false;
			this.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT = false;
			this.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION = false;
			this.JA_COMMON_JAPANESE = false;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="pKey">文字列キー</param>
		public InfomationRecord(string pKey)
			: this()
		{
			this.KeyUnicodeString = pKey;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="pKey">文字列キー</param>
		/// <param name="pJISX0213Level">JISX0213Levels</param>
		/// <param name="pNameType">NameTypes</param>
		/// <param name="pETax">true=e-Taxで利用可能</param>
		/// <param name="pGakunenbetuKanji">GakunenbetuKanjis</param>
		/// <param name="pNISA">true=NISAで利用可能</param>
		/// <param name="pELTAX">true=eLTAXで利用可能</param>
		/// <param name="pJABJ">true=組番号 285 BASIC JAPANESE</param>
		/// <param name="pJAJIE">true=組番号 371 JIS2004 IDEOGRAPHICS EXTENSION</param>
		/// <param name="pJAJIS">true=組番号 372 JAPANESE IDEOGRAPHICS SUPPLEMENT</param>
		/// <param name="pJAJNIE">true=組番号 286 JAPANESE NON IDEOGRAPHICS EXTENSION</param>
		/// <param name="pJACJ">true=組番号 287 COMMON JAPANESE</param>
		public InfomationRecord(string pKey, JISX0213Levels pJISX0213Level, NameTypes pNameType, bool pETax, GakunenbetuKanjis pGakunenbetuKanji, bool pNISA, bool pELTAX, bool pJABJ, bool pJAJIE, bool pJAJIS, bool pJAJNIE, bool pJACJ)
		{
			this.KeyUnicodeString = pKey;
			this.JISX0213Level = pJISX0213Level;
			this.NameType = pNameType;
			this.ETaxAvailable = pETax;
			this.GakunenbetuKanji = pGakunenbetuKanji;
			this.NISAAvailable = pNISA;
			this.ELTAXAvailable = pELTAX;
			this.JA_BASIC_JAPANESE = pJABJ;
			this.JA_JIS2004_IDEOGRAPHICS_EXTENSION = pJAJIE;
			this.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT = pJAJIS;
			this.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION = pJAJNIE;
			this.JA_COMMON_JAPANESE = pJACJ;
		}

		/// <summary>
		/// 文字列キー設定
		/// </summary>
		/// <param name="pKey">文字列キー</param>
		public void SetUnicodeKey(string pKey)
		{
			this.KeyUnicodeString = pKey;
		}

		/// <summary>
		/// JISX0213水準種類設定
		/// </summary>
		/// <param name="pLevel">JISX0213Levels</param>
		public void SetJISX0213Level(JISX0213Levels pLevel)
		{
			this.JISX0213Level = pLevel;
		}

		/// <summary>
		/// 常用・人名用漢字種類設定
		/// </summary>
		/// <param name="pType">NameTypes</param>
		public void SetNameType(NameTypes pType)
		{
			this.NameType = pType;
		}

		/// <summary>
		/// e-Tax利用可能フラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetETaxAvailable(bool pAvailable)
		{
			this.ETaxAvailable = pAvailable;
		}

		/// <summary>
		/// 学年別漢字配当設定
		/// </summary>
		/// <param name="pGakunen">GakunenbetuKanjis</param>
		public void SetGakunenbetuKanji(GakunenbetuKanjis pGakunen)
		{
			this.GakunenbetuKanji = pGakunen;
		}

		/// <summary>
		/// NISA利用可能フラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetNISAAvailable(bool pAvailable)
		{
			this.NISAAvailable = pAvailable;
		}

		/// <summary>
		/// eLTAX利用可能フラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetELTAXAvailable(bool pAvailable)
		{
			this.ELTAXAvailable = pAvailable;
		}

		/// <summary>
		/// 組番号 285 BASIC JAPANESEフラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetJA_BASIC_JAPANESE(bool pAvailable)
		{
			this.JA_BASIC_JAPANESE = pAvailable;
		}

		/// <summary>
		/// 組番号 371 JIS2004 IDEOGRAPHICS EXTENSIONフラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetJA_JIS2004_IDEOGRAPHICS_EXTENSION(bool pAvailable)
		{
			this.JA_JIS2004_IDEOGRAPHICS_EXTENSION = pAvailable;
		}

		/// <summary>
		/// 組番号 372 JAPANESE IDEOGRAPHICS SUPPLEMENTフラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetJA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT(bool pAvailable)
		{
			this.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT = pAvailable;
		}

		/// <summary>
		/// 組番号 286 JAPANESE NON IDEOGRAPHICS EXTENSIONフラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetJA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION(bool pAvailable)
		{
			this.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION = pAvailable;
		}

		/// <summary>
		/// 組番号 287 COMMON JAPANESEフラグ設定
		/// </summary>
		/// <param name="pAvailable">true=利用可能</param>
		public void SetJA_COMMON_JAPANESE(bool pAvailable)
		{
			this.JA_COMMON_JAPANESE = pAvailable;
		}

		/// <summary>
		/// デバッグダンプ
		/// </summary>
		[Conditional("DEBUG")]
		public void DebugDump()
		{
			Console.Write("JISX0213:{0}", this.JISX0213Level.ToString());
			Console.Write(" NameType:{0}", this.NameType.ToString());
			Console.Write(" e-Tax:{0}", this.ETaxAvailable.ToString());
			Console.Write(" Gakunen:{0}", this.GakunenbetuKanji.ToString());
			Console.Write(" NISA:{0}", this.NISAAvailable.ToString());
			Console.Write(" eLTAX:{0}", this.ELTAXAvailable.ToString());
			Console.Write(" JA_BJ:{0}", this.JA_BASIC_JAPANESE.ToString());
			Console.Write(" JA_JIE:{0}", this.JA_JIS2004_IDEOGRAPHICS_EXTENSION.ToString());
			Console.Write(" JA_JIS:{0}", this.JA_JAPANESE_IDEOGRAPHICS_SUPPLEMENT.ToString());
			Console.Write(" JA_JNIE:{0}", this.JA_JAPANESE_NON_IDEOGRAPHICS_EXTENSION.ToString());
			Console.Write(" JA_CJ:{0}", this.JA_COMMON_JAPANESE.ToString());
		}
	}

	static public class Extends
	{
		public static void Increment<TKey>(this SortedDictionary<TKey, int> dict, TKey key)
		{
			int value;

			if (dict.TryGetValue(key, out value)) {
				dict[key] = value + 1;
			} else {
				dict[key] = 1;
			}
		}
	}
}
