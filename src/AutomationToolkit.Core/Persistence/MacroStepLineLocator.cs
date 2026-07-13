using System.Text.Json;

namespace AutomationToolkit.Core.Persistence;

/// <summary>マクロ JSON テキストから steps 配列の各要素の開始行番号を求める</summary>
public static class MacroStepLineLocator {
	/// <summary>steps 配列の各要素が始まる 1 始まりの行番号を求める</summary>
	/// <param name="utf8Json">マクロファイルの UTF-8 バイト列。BOM は含まないこと</param>
	/// <returns>各ステップの開始行番号の一覧。steps 配列が見つからない場合は空</returns>
	/// <exception cref="JsonException">JSON として不正な場合</exception>
	public static IReadOnlyList<int> LocateStepLines(ReadOnlySpan<byte> utf8Json) {
		var offsets = LocateStepOffsets(utf8Json);
		if (offsets.Count == 0) return [];

		// オフセットは昇順なので、1 パスの LF カウントで全オフセットを行番号へ変換する
		// LF は UTF-8 の継続バイトに現れないためバイト単位のカウントで正確に求まる
		var lines = new int[offsets.Count];
		var offsetIndex = 0;
		var line = 1;
		for (var byteIndex = 0; byteIndex < utf8Json.Length && offsetIndex < offsets.Count; byteIndex++) {
			while (offsetIndex < offsets.Count && offsets[offsetIndex] == byteIndex) {
				lines[offsetIndex] = line;
				offsetIndex++;
			}
			if (utf8Json[byteIndex] == (byte)'\n') line++;
		}
		return lines;
	}

	/// <summary>ルート直下の steps 配列の各要素の開始バイトオフセットを列挙する</summary>
	/// <param name="utf8Json">マクロファイルの UTF-8 バイト列</param>
	/// <returns>各ステップの開始バイトオフセットの昇順一覧</returns>
	private static List<long> LocateStepOffsets(ReadOnlySpan<byte> utf8Json) {
		var offsets = new List<long>();
		var reader = new Utf8JsonReader(utf8Json);
		while (reader.Read()) {
			// ネストした同名プロパティを誤検出しないよう、ルート直下の steps のみ対象にする
			if (reader.CurrentDepth != 1) continue;
			if (reader.TokenType != JsonTokenType.PropertyName) continue;
			if (reader.ValueTextEquals("steps"u8) == false) continue;

			if (reader.Read() == false || reader.TokenType != JsonTokenType.StartArray) return offsets;
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
				offsets.Add(reader.TokenStartIndex);
				reader.Skip();
			}
			return offsets;
		}
		return offsets;
	}
}
