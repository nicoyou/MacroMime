using MacroMime.Core.Models;
using MacroMime.Core.Playback;

namespace MacroMime.Core.Tests;

/// <summary>MacroPlayer の進行通知イベントのテスト</summary>
public class MacroPlayerProgressTests {
	/// <summary>入力を送出せず呼び出し履歴だけ記録するフェイク</summary>
	/// <param name="log">呼び出し履歴の記録先</param>
	private sealed class FakeInputSynthesizer(List<string> log) : IInputSynthesizer {
		/// <inheritdoc/>
		public void RefreshVirtualScreenMetrics() { }

		/// <inheritdoc/>
		public void Send(MacroStep step) => log.Add($"send {((MouseMoveStep)step).x}");

		/// <inheritdoc/>
		public void SendKey(ushort virtualKey, ushort scanCode, bool isExtended, bool down) { }

		/// <inheritdoc/>
		public void SendMouseButtonUp(MouseButton button) { }
	}

	/// <summary>x 座標をステップ番号に揃えたテスト用マクロを生成する</summary>
	/// <param name="stepCount">ステップ数</param>
	/// <returns>テスト用マクロ</returns>
	private static Macro CreateMacro(int stepCount) => new() {
		name = "progress-test",
		steps = [.. Enumerable.Range(0, stepCount).Select(i => new MouseMoveStep { x = i, y = 0 })],
	};

	/// <summary>各ステップの送出前に進行通知が発生し、ループを跨いで順序が正しい</summary>
	[Fact]
	public async Task PlayAsync_FiresProgressBeforeEachSend() {
		var log = new List<string>();
		var player = new MacroPlayer(new FakeInputSynthesizer(log));
		var progressRecords = new List<PlaybackProgress>();
		player.ProgressChanged += (_, progress) => {
			log.Add($"progress {progress.loopIndex}-{progress.stepIndex}");
			progressRecords.Add(progress);
		};

		await player.PlayAsync(CreateMacro(3), new PlaybackOptions { loopCount = 2 }, CancellationToken.None);

		Assert.Equal([
			"progress 0-0", "send 0", "progress 0-1", "send 1", "progress 0-2", "send 2",
			"progress 1-0", "send 0", "progress 1-1", "send 1", "progress 1-2", "send 2",
		], log);
		Assert.All(progressRecords, progress => Assert.Equal((3, 2), (progress.totalSteps, progress.loopCount)));
	}

	/// <summary>待機中にキャンセルすると例外なく完了し、最後の通知は待機中だったステップを指す</summary>
	[Fact]
	public async Task PlayAsync_CanceledDuringDelay_LastProgressPointsToWaitingStep() {
		var log = new List<string>();
		var player = new MacroPlayer(new FakeInputSynthesizer(log));
		using var cts = new CancellationTokenSource();
		var progressRecords = new List<PlaybackProgress>();
		player.ProgressChanged += (_, progress) => {
			progressRecords.Add(progress);
			// 2 番目のステップの待機に入る前にキャンセルする
			if (progress.stepIndex == 1) cts.Cancel();
		};

		var macro = new Macro {
			name = "cancel-test",
			steps =
			[
				new MouseMoveStep { x = 0, y = 0 },
				new MouseMoveStep { x = 1, y = 0, delayBeforeMs = 60000 },
			],
		};
		await player.PlayAsync(macro, new PlaybackOptions(), cts.Token);

		Assert.Equal(1, progressRecords[^1].stepIndex);
		Assert.DoesNotContain("send 1", log);
		Assert.False(player.isPlaying);
	}
}
