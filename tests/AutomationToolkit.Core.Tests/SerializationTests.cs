using System.Text.Json;
using AutomationToolkit.Core.Models;
using AutomationToolkit.Core.Persistence;

namespace AutomationToolkit.Core.Tests;

/// <summary>マクロの JSON シリアライズ/デシリアライズのテスト</summary>
public class SerializationTests
{
    /// <summary>全ステップ種別を含むサンプルマクロを生成する</summary>
    /// <returns>サンプルマクロ</returns>
    private static Macro CreateSampleMacro() => new()
    {
        Name = "sample",
        CreatedUtc = new DateTimeOffset(2026, 7, 12, 9, 30, 0, TimeSpan.Zero),
        Steps =
        [
            new MouseMoveStep { DelayBeforeMs = 120, X = 960, Y = 540 },
            new MouseDownStep { DelayBeforeMs = 35, Button = MouseButton.Left, X = 960, Y = 540 },
            new MouseUpStep { DelayBeforeMs = 90, Button = MouseButton.Left, X = 960, Y = 540 },
            new KeyDownStep { DelayBeforeMs = 300, VirtualKey = 65, ScanCode = 30 },
            new KeyUpStep { DelayBeforeMs = 60, VirtualKey = 65, ScanCode = 30 },
            new MouseWheelStep { DelayBeforeMs = 200, X = 960, Y = 540, Delta = -120 },
        ],
    };

    /// <summary>シリアライズと逆変換で全ステップの内容が保持される</summary>
    [Fact]
    public void RoundTrip_PreservesAllSteps()
    {
        var macro = CreateSampleMacro();

        var json = JsonSerializer.Serialize(macro, MacroJson.Default);
        var restored = JsonSerializer.Deserialize<Macro>(json, MacroJson.Default);

        Assert.NotNull(restored);
        Assert.Equal(macro.SchemaVersion, restored.SchemaVersion);
        Assert.Equal(macro.Name, restored.Name);
        Assert.Equal(macro.Steps.Count, restored.Steps.Count);

        var move = Assert.IsType<MouseMoveStep>(restored.Steps[0]);
        Assert.Equal((120, 960, 540), (move.DelayBeforeMs, move.X, move.Y));

        var down = Assert.IsType<MouseDownStep>(restored.Steps[1]);
        Assert.Equal(MouseButton.Left, down.Button);

        var keyDown = Assert.IsType<KeyDownStep>(restored.Steps[3]);
        Assert.Equal((65, 30, false), ((int)keyDown.VirtualKey, (int)keyDown.ScanCode, keyDown.IsExtended));

        var wheel = Assert.IsType<MouseWheelStep>(restored.Steps[5]);
        Assert.Equal(-120, wheel.Delta);
        Assert.False(wheel.IsHorizontal);
    }

    /// <summary>期待した型判別子と camelCase のプロパティ名でシリアライズされる</summary>
    [Fact]
    public void Serialize_UsesExpectedTypeDiscriminatorsAndCamelCase()
    {
        var json = JsonSerializer.Serialize(CreateSampleMacro(), MacroJson.Default);

        Assert.Contains("\"$type\": \"mouseMove\"", json);
        Assert.Contains("\"$type\": \"mouseDown\"", json);
        Assert.Contains("\"$type\": \"keyDown\"", json);
        Assert.Contains("\"$type\": \"mouseWheel\"", json);
        Assert.Contains("\"button\": \"Left\"", json);
        Assert.Contains("\"delayBeforeMs\": 120", json);
        Assert.Contains("\"schemaVersion\": 1", json);
    }

    /// <summary>既定値のプロパティは JSON に出力されない</summary>
    [Fact]
    public void Serialize_OmitsDefaultValues()
    {
        var macro = new Macro
        {
            Name = "minimal",
            Steps = [new MouseMoveStep { X = 10, Y = 20 }], // DelayBeforeMs = 0
        };

        var json = JsonSerializer.Serialize(macro, MacroJson.Default);

        Assert.DoesNotContain("delayBeforeMs", json);
        Assert.DoesNotContain("isExtended", json);
    }

    /// <summary>手書き・手編集を想定した最小 JSON をデシリアライズできる</summary>
    [Fact]
    public void Deserialize_HandEditedJson_Works()
    {
        // ユーザーが手書き・手編集する想定の最小 JSON
        const string json = """
        {
          "schemaVersion": 1,
          "name": "hand-written",
          "steps": [
            { "$type": "mouseMove", "delayBeforeMs": 120, "x": 960, "y": 540 },
            { "$type": "mouseDown", "button": "Right", "x": 960, "y": 540 },
            { "$type": "keyDown", "virtualKey": 65, "scanCode": 30 }
          ]
        }
        """;

        var macro = JsonSerializer.Deserialize<Macro>(json, MacroJson.Default);

        Assert.NotNull(macro);
        Assert.Equal(3, macro.Steps.Count);
        Assert.Equal(MouseButton.Right, Assert.IsType<MouseDownStep>(macro.Steps[1]).Button);
        Assert.Equal(0, macro.Steps[1].DelayBeforeMs);
    }
}
