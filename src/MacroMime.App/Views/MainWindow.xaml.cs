using System.Windows;
using MacroMime.App.ViewModels;

namespace MacroMime.App.Views;

/// <summary>マクロ一覧と録画・再生操作を提供するメインウィンドウ</summary>
public partial class MainWindow : Window {
	/// <summary>メイン画面のビューモデル</summary>
	private readonly MainViewModel viewModel;

	/// <summary>ビューモデルをバインドしてウィンドウを初期化する</summary>
	/// <param name="viewModel">メイン画面のビューモデル</param>
	public MainWindow(MainViewModel viewModel) {
		InitializeComponent();
		this.viewModel = viewModel;
		DataContext = viewModel;
		Title = AppInfo.TITLE;
		this.viewModel.StateChangedForTray += UpdateTitle;
	}

	/// <summary>動作状態に応じてウィンドウタイトルを更新する</summary>
	private void UpdateTitle() {
		Title = viewModel.State switch {
			AppState.Recording => $"[REC] {AppInfo.TITLE}",
			AppState.Playing => $"[再生中] {AppInfo.TITLE}",
			_ => AppInfo.TITLE,
		};
	}

	/// <summary>ウィンドウを閉じるときにバインディングを保存する</summary>
	/// <param name="e">クローズイベントの引数</param>
	protected override void OnClosed(EventArgs e) {
		viewModel.PersistBindings();
		base.OnClosed(e);
	}
}
