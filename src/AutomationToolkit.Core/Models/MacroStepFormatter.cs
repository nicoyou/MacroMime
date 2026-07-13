namespace AutomationToolkit.Core.Models;

/// <summary>マクロステップの表示用の要約文字列を組み立てる</summary>
public static class MacroStepFormatter {
	/// <summary>ステップの種類と主要パラメーターを要約した文字列を返す</summary>
	/// <remarks>種類名は JSON の $type 判別子と一致させ、ファイル内検索の手がかりにする</remarks>
	/// <param name="step">要約するステップ</param>
	/// <returns>ステップの要約文字列</returns>
	public static string Describe(MacroStep step) => step switch {
		KeyDownStep keyDown => $"keyDown {VirtualKeyNames.GetName(keyDown.virtualKey)}",
		KeyUpStep keyUp => $"keyUp {VirtualKeyNames.GetName(keyUp.virtualKey)}",
		MouseMoveStep move => $"mouseMove ({move.x}, {move.y})",
		MouseDownStep down => $"mouseDown {down.button} ({down.x}, {down.y})",
		MouseUpStep up => $"mouseUp {up.button} ({up.x}, {up.y})",
		MouseWheelStep wheel => wheel.isHorizontal ? $"mouseWheel 水平 {wheel.delta}" : $"mouseWheel {wheel.delta}",
		_ => step.GetType().Name,
	};
}
