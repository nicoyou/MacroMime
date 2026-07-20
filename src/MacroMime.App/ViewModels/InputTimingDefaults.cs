namespace MacroMime.App.ViewModels;

/// <summary>入力操作の待機時間の既定値 ( ms )</summary>
internal static class InputTimingDefaults {
	/// <summary>クリック・スクロール開始までの時間短縮の既定値</summary>
	public const int COOLDOWN_MS = 50;
	/// <summary>押してから離すまでの時間の既定値。クリック時間の短縮の既定値でもある</summary>
	public const int DURATION_MS = 20;
}
