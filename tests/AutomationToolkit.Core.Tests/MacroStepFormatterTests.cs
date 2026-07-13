using AutomationToolkit.Core.Models;

namespace AutomationToolkit.Core.Tests;

/// <summary>MacroStepFormatter の要約文字列のテスト</summary>
public class MacroStepFormatterTests {
	/// <summary>キー押下ステップの要約にキー名が含まれる</summary>
	[Fact]
	public void Describe_KeyDown_UsesKeyName() {
		Assert.Equal("keyDown A", MacroStepFormatter.Describe(new KeyDownStep { virtualKey = 0x41 }));
	}

	/// <summary>キー解放ステップの要約にキー名が含まれる</summary>
	[Fact]
	public void Describe_KeyUp_UsesKeyName() {
		Assert.Equal("keyUp F1", MacroStepFormatter.Describe(new KeyUpStep { virtualKey = 0x70 }));
	}

	/// <summary>カーソル移動ステップの要約に座標が含まれる</summary>
	[Fact]
	public void Describe_MouseMove_IncludesCoordinates() {
		Assert.Equal("mouseMove (960, 540)", MacroStepFormatter.Describe(new MouseMoveStep { x = 960, y = 540 }));
	}

	/// <summary>マウスボタン押下ステップの要約にボタンと座標が含まれる</summary>
	[Fact]
	public void Describe_MouseDown_IncludesButtonAndCoordinates() {
		Assert.Equal("mouseDown Left (10, 20)",
			MacroStepFormatter.Describe(new MouseDownStep { button = MouseButton.Left, x = 10, y = 20 }));
	}

	/// <summary>マウスボタン解放ステップの要約にボタンと座標が含まれる</summary>
	[Fact]
	public void Describe_MouseUp_IncludesButtonAndCoordinates() {
		Assert.Equal("mouseUp Right (10, 20)",
			MacroStepFormatter.Describe(new MouseUpStep { button = MouseButton.Right, x = 10, y = 20 }));
	}

	/// <summary>ホイールステップの要約に回転量が含まれ、水平ホイールは付記される</summary>
	[Fact]
	public void Describe_MouseWheel_IncludesDeltaAndHorizontal() {
		Assert.Equal("mouseWheel -120", MacroStepFormatter.Describe(new MouseWheelStep { delta = -120 }));
		Assert.Equal("mouseWheel 水平 240",
			MacroStepFormatter.Describe(new MouseWheelStep { delta = 240, isHorizontal = true }));
	}
}
