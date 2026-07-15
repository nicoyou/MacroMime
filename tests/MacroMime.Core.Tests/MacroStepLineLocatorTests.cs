using System.Text;
using System.Text.Json;
using MacroMime.Core.Models;
using MacroMime.Core.Persistence;

namespace MacroMime.Core.Tests;

/// <summary>MacroStepLineLocator の行番号算出のテスト</summary>
public class MacroStepLineLocatorTests {
	/// <summary>JSON 文字列から各ステップの開始行番号を求める</summary>
	/// <param name="json">対象の JSON 文字列</param>
	/// <returns>各ステップの開始行番号の一覧</returns>
	private static IReadOnlyList<int> Locate(string json)
		=> MacroStepLineLocator.LocateStepLines(Encoding.UTF8.GetBytes(json));

	/// <summary>手編集を想定した整形の JSON で各ステップの行番号が正しく求まる</summary>
	[Fact]
	public void LocateStepLines_HandEditedJson_ReturnsExpectedLines() {
		// 2 番目のステップは複数行に渡る ( 開始行を返すことを確認する )
		const string json = """
		{
		  "schemaVersion": 1,
		  "name": "日本語の名前",
		  "steps": [
		    { "$type": "mouseMove", "x": 1, "y": 2 },
		    { "$type": "keyDown", "virtualKey": 65,
		      "scanCode": 30 },
		    { "$type": "mouseUp", "button": "Left", "x": 3, "y": 4 }
		  ]
		}
		""";

		Assert.Equal([5, 6, 8], Locate(json));
	}

	/// <summary>1 行に全て書かれた JSON では全ステップが行 1 になる</summary>
	[Fact]
	public void LocateStepLines_SingleLineJson_ReturnsLineOne() {
		const string json =
			"""{"schemaVersion":1,"name":"x","steps":[{"$type":"mouseMove","x":1,"y":2},{"$type":"keyUp","virtualKey":65,"scanCode":30}]}""";

		Assert.Equal([1, 1], Locate(json));
	}

	/// <summary>CRLF と LF のどちらの改行でも同じ行番号になる</summary>
	[Fact]
	public void LocateStepLines_CrlfAndLf_ReturnSameLines() {
		var lfJson = "{\n\"steps\": [\n{ \"$type\": \"mouseMove\" },\n{ \"$type\": \"keyUp\" }\n]\n}";
		var crlfJson = lfJson.Replace("\n", "\r\n");

		Assert.Equal([3, 4], Locate(lfJson));
		Assert.Equal(Locate(lfJson), Locate(crlfJson));
	}

	/// <summary>steps プロパティが無い JSON では空の一覧が返る</summary>
	[Fact]
	public void LocateStepLines_MissingSteps_ReturnsEmpty() {
		Assert.Empty(Locate("""{ "schemaVersion": 1, "name": "x" }"""));
	}

	/// <summary>steps が空配列の JSON では空の一覧が返る</summary>
	[Fact]
	public void LocateStepLines_EmptySteps_ReturnsEmpty() {
		Assert.Empty(Locate("""{ "schemaVersion": 1, "name": "x", "steps": [] }"""));
	}

	/// <summary>ルート直下ではないネストした steps プロパティは無視される</summary>
	[Fact]
	public void LocateStepLines_NestedSteps_IsIgnored() {
		const string json = """
		{
		  "meta": { "steps": [ { "a": 1 } ] },
		  "steps": [
		    { "$type": "mouseMove", "x": 1, "y": 2 }
		  ]
		}
		""";

		Assert.Equal([4], Locate(json));
	}

	/// <summary>MacroJson.Default のシリアライズ出力に対して件数が一致し、各行がステップの開始行を指す</summary>
	[Fact]
	public void LocateStepLines_SerializedMacro_MatchesStepCountAndLines() {
		var macro = new Macro {
			name = "serialized",
			steps =
			[
				new MouseMoveStep { delayBeforeMs = 120, x = 960, y = 540 },
				new KeyDownStep { delayBeforeMs = 300, virtualKey = 65, scanCode = 30 },
				new KeyUpStep { delayBeforeMs = 60, virtualKey = 65, scanCode = 30 },
			],
		};
		var json = JsonSerializer.Serialize(macro, MacroJson.Default);

		var stepLines = Locate(json);

		Assert.Equal(macro.steps.Count, stepLines.Count);
		Assert.Equal(stepLines, stepLines.OrderBy(line => line));
		// WriteIndented では各ステップは "{" のみの行から始まる
		var lines = json.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
		Assert.All(stepLines, line => Assert.Equal("{", lines[line - 1].Trim()));
	}
}
