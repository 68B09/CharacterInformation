using System;
using System.Text;
using CharacterInformations;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.InputEncoding = Encoding.Unicode;
			Console.OutputEncoding = Encoding.Unicode;

			CharacterInformation cInfo = new CharacterInformation();
			cInfo.Load(@"..\..\..\..\CharacterInformation.txt");

#if DEBUG
			Console.WriteLine(cInfo.DebugDump());
#endif

			while (true) {
				Console.WriteLine("Text[enter]");
				string text = Console.ReadLine();

				foreach (string chars in CharacterInformation.GetOneCharacterEnumerator(text)) {
					int ucode = char.ConvertToUtf32(chars, 0);
					Console.Write("[{0}](U+{1:X}):", chars, ucode);

					InfomationRecord info = cInfo[chars];
					if (info == null) {
						Console.Write(" no-data");
					} else {
						info.DebugDump();
					}

					Console.Write("\n");
				}

				Console.Write("\n");
			}
		}
	}
}
