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
		/// 定義データ読み込み
		/// </summary>
		/// <param name="pSettingFilePath">定義ファイルのパス</param>
		/// <remarks>
		/// 定義データを読み込みインスタンスを初期化する。
		/// </remarks>
		public void Load(string pSettingFilePath)
		{
			this.items.Clear();
			this.majorVer = 0;
			this.minorVer = 0;

			int dataCount = 0;

			using (StreamReader reader = new StreamReader(pSettingFilePath, Encoding.UTF8)) {
				while (!reader.EndOfStream) {
					string line = reader.ReadLine();
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

				reader.Close();
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
				// key
				result.SetUnicodeKey(MakeStringFromCodePoints(fields[0]));

				// JISX0213Levels
				if (fields.Length < 2) {
					break;
				}
				string strwk = fields[1].Trim();
				if (strwk.Length >= 1) {
					result.SetJISX0213Level((JISX0213Levels)int.Parse(strwk));
				}

				// NameTypes
				if (fields.Length < 3) {
					break;
				}
				strwk = fields[2].Trim();
				if (strwk.Length >= 1) {
					result.SetNameType((NameTypes)int.Parse(strwk));
				}

				// ETaxAvailable
				if (fields.Length < 4) {
					break;
				}
				strwk = fields[3].Trim();
				if (strwk.Length >= 1) {
					result.SetETaxAvailable(int.Parse(strwk) != 0);
				}

				// GakunenbetuKanji
				if (fields.Length < 5) {
					break;
				}
				strwk = fields[4].Trim();
				if (strwk.Length >= 1) {
					result.SetGakunenbetuKanji((GakunenbetuKanjis)int.Parse(strwk));
				}

				// NISAAvailable
				if (fields.Length < 6) {
					break;
				}
				strwk = fields[5].Trim();
				if (strwk.Length >= 1) {
					result.SetNISAAvailable(int.Parse(strwk) != 0);
				}

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
			foreach (InfomationRecord item in this.items.Values) {
				if (!jisx0213levelcount.ContainsKey(item.JISX0213Level)) {
					jisx0213levelcount[item.JISX0213Level] = 0;
				}
				jisx0213levelcount[item.JISX0213Level]++;

				if (!nametypecount.ContainsKey(item.NameType)) {
					nametypecount[item.NameType] = 0;
				}
				nametypecount[item.NameType]++;

				if (!etaxcount.ContainsKey(item.ETaxAvailable)) {
					etaxcount[item.ETaxAvailable] = 0;
				}
				etaxcount[item.ETaxAvailable]++;

				if (!gakunenbetukanjicount.ContainsKey(item.GakunenbetuKanji)) {
					gakunenbetukanjicount[item.GakunenbetuKanji] = 0;
				}
				gakunenbetukanjicount[item.GakunenbetuKanji]++;

				if (!nisacount.ContainsKey(item.NISAAvailable)) {
					nisacount[item.NISAAvailable] = 0;
				}
				nisacount[item.NISAAvailable]++;
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

			// end
			sb.AppendLine("DebugDump out --");

			return sb.ToString();
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
					if (charInfo.JISX0213Level != JISX0213Levels.None) {
						sb.AppendFormat(" {0}", charInfo.JISX0213Level);
					}

					if (charInfo.NameType != NameTypes.None) {
						sb.AppendFormat(" {0}", charInfo.NameType);
					}

					if (charInfo.ETaxAvailable) {
						sb.AppendFormat(" e-Tax");
					}

					if (charInfo.GakunenbetuKanji != GakunenbetuKanjis.None) {
						sb.AppendFormat(" {0}", charInfo.GakunenbetuKanji);
					}

					if (charInfo.NISAAvailable) {
						sb.AppendFormat(" NISA");
					}
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
		public InfomationRecord(string pKey, JISX0213Levels pJISX0213Level, NameTypes pNameType, bool pETax, GakunenbetuKanjis pGakunenbetuKanji, bool pNISA)
		{
			this.KeyUnicodeString = pKey;
			this.JISX0213Level = pJISX0213Level;
			this.NameType = pNameType;
			this.ETaxAvailable = pETax;
			this.GakunenbetuKanji = pGakunenbetuKanji;
			this.NISAAvailable = pNISA;
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
		}
	}
}
