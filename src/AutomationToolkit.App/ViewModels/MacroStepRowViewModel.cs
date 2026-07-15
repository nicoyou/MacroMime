using CommunityToolkit.Mvvm.ComponentModel;

namespace AutomationToolkit.App.ViewModels;

/// <summary>ステップ一覧の 1 行分のビューモデル</summary>
public sealed partial class MacroStepRowViewModel : ObservableObject {
	/// <summary>実行前の待機ミリ秒数</summary>
	[ObservableProperty]
	private int delayBeforeMs;

	/// <summary>待機時間が編集されたときに変更前の値と共に通知するイベント</summary>
	public event Action<MacroStepRowViewModel, int>? DelayEdited;

	/// <summary>1 始まりのステップ番号</summary>
	public int index { get; }
	/// <summary>ステップの要約文字列</summary>
	public string description { get; }

	/// <summary>1 行分の状態を組み立てる</summary>
	/// <param name="index">1 始まりのステップ番号</param>
	/// <param name="delayBeforeMs">実行前の待機ミリ秒数</param>
	/// <param name="description">ステップの要約文字列</param>
	public MacroStepRowViewModel(int index, int delayBeforeMs, string description) {
		this.index = index;
		this.delayBeforeMs = delayBeforeMs;
		this.description = description;
	}

	/// <summary>待機時間の変更時に変更前の値と共にイベントを通知する</summary>
	/// <param name="oldValue">変更前の待機ミリ秒数</param>
	/// <param name="newValue">変更後の待機ミリ秒数</param>
	partial void OnDelayBeforeMsChanged(int oldValue, int newValue) => DelayEdited?.Invoke(this, oldValue);
}
