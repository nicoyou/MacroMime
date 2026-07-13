# CLAUDE.md - Unity プロジェクト共通ガイド

## 命名規則

### フィールド・プロパティ

- **プライベートフィールド**: camelCase (`power`, `speed`, `count`) — アンダースコアは付けない
- **バッキングフィールド**: `_level`, `_health`, `_instance` (アンダースコア + camelCase)
- **プロパティ**: camelCase (`level`, `health`, `instance`, `maximumHealth`)
- **SerializeField**: camelCase (`mainCamera`, `playerPrefab`)
- **定数**: UPPER_SNAKE_CASE (`FULL_CIRCLE_DEGREES`, `MINIMUM_SE_INTERVAL`)

```csharp
// Good ( プロパティが無いプライベートフィールドはアンダースコアなし )
private float power;

// Good ( 同名プロパティと衝突するためバッキングフィールドにアンダースコアを付ける )
private float _health;
public float health => _health;

// Bad ( プロパティが無いのにアンダースコアを付けている )
private float _power;
```

### 不要なバッキングフィールドの禁止

`_field` + 同名プロパティのペアは、単純なミラー ( = ゲッターで `_field` をそのまま返すだけ ) の場合は書かず、auto-property に置き換える。コード量が増えるだけで実利が無い。

ペアを使ってよいのは以下のケースに限る:

- `[SerializeField]` で Unity に保存する必要がある
- ゲッター本体で計算や変換を行う
- `readonly` な配列・コレクションを `IReadOnlyList` 等として公開する

それ以外 ( クラス内で書き換えるが外部からは読み取り専用にしたい、初期化後に変更しない、等 ) は `{ get; private set; }` / `{ get; init; }` / `{ get; }` で十分。

```csharp
// Good ( 外部からは読み取り専用、クラス内で書き換える )
public Item pendingItem { get; private set; }

// Good ( 初期化後は不変 )
public BeltLogic logic { get; init; } = new();

// Good ( SerializeField なのでペアが必要 )
[SerializeField, CanBeNull]
private Belt _downstream;
[CanBeNull]
public Belt downstream => _downstream;

// Good ( 配列を readonly で持ち、IReadOnlyList で公開 )
private readonly Item[] _cells = new Item[CELL_COUNT];
public IReadOnlyList<Item> cells => _cells;

// Bad ( 単純ミラー。auto-property で済むのにペアを書いている )
private Item _pendingItem;
public Item pendingItem => _pendingItem;
```

### 意味が変わったらリネームを優先する

変数・メソッド・クラス・ファイルなどの「指している実態」が、リファクタや機能追加によって元の名前と乖離した場合、その時点で **適切な意味の名前にリネームする** ことを優先する。「呼び出し箇所が多くて影響範囲が広い」「リネームコミットが膨らむ」といった理由でリネームを後回しにしない。

名前と実態がずれた状態を放置すると、コードを読む人が誤解する・新規追加が同じ命名規則に引きずられて悪化する、といった劣化が連鎖する。IDE のリネーム機能で機械的に置き換えられる以上、影響範囲は通常の障壁にならない。

例:

- もともと「最大値」を意味していた変数 `limit` が「現在の使用可能上限」に意味を変えたら `currentLimit` 等にリネームする
- メソッド `Save()` の中身が保存だけでなくバリデーションも含むようになったら `ValidateAndSave()` にリネーム、あるいは責務を分割する

リネームに伴う既存呼び出し箇所の修正は、機能追加・リファクタと同じコミットに含めて構わない。後続コミットへの先送りも避ける ( 中途半端な状態を main に残さない )。

### 略称の禁止

命名に略称は基本的に使用しない。省略せず意味が明確な名前を付ける。

```csharp
// Good
var position = transform.position;
var destination = target.position;

// Bad
var pos = transform.position;
var dest = target.position;
```

### 意味のないサフィックスの禁止

変数は情報を格納するものなので、`~Info` のような意味のないサフィックスは使用しない。

```csharp
// Good
var player = GetPlayer();
var item = GetItem();

// Bad
var playerInfo = GetPlayer();
var itemInfo = GetItem();
```

### メソッド・クラス

- **メソッド**: PascalCase (`Initialize`, `GetDescription`, `ApplyDamage`)
- **クラス**: PascalCase (`PlayerController`, `EnemyBase`)
- **enum 値**: PascalCase (`ItemType.Wood`, `GameState.Playing`)
- **テスト定数**: UPPER_SNAKE_CASE (`RANDOM_TEST_COUNT`, `DEFAULT_WITHIN_AMOUNT`)

## コーディング規約

### ブレーススタイル

K&R スタイル (開き中括弧を宣言行の末尾に配置)。ただし `else` / `else if` は閉じ括弧の次の行に記述する。

```csharp
public class PlayerController : MonoBehaviour {
    public void TakeDamage(float amount) {
        if (isInvincible) {
            return;
        }
        else {
            health -= amount;
        }
    }
}
```

ガード節など、本体が単一文の `if` は中括弧を省略して 1 行で書く。ただし単一文であっても本体が長く、1 行に収めるとかえって読みにくい場合は中括弧を残して複数行で書いてよい ( 絶対ではなく、行の長さの感覚値に依る )。

```csharp
// Good ( 短い単一文は 1 行 )
if (target == null) return;
if (skip) continue;

// Good ( 単一文だが本体が長いため複数行のままにする方が読みやすい )
if (call.arguments.Count != 1) {
    throw IrError(call.position, "mine は引数 1 つ ( 資源ノード名の文字列リテラル ) を要求します ( 実引数数: {0} )".Format(call.arguments.Count));
}

// Bad ( 短い単一文なのに中括弧を付けている )
if (target == null) {
    return;
}
```

### インデント

タブを使用。

### 属性の改行

フィールド・プロパティ・メソッドに付与する属性 ( `[SerializeField]`, `[NonNullable]`, `[CanBeNull]`, `[SerializeReference]` 等 ) は、宣言と同じ行に書かず必ず独立した行に記述する。複数の属性を付ける場合は、ブラケットを分けて並べるのではなくカンマ区切りで 1 つのブラケットにまとめる。

```csharp
// Good
[SerializeField]
private MoveDirection direction = MoveDirection.Right;

[SerializeField, NonNullable]
private GameObject playerPrefab;

// Bad
[SerializeField] private MoveDirection direction = MoveDirection.Right;
[SerializeField]
[NonNullable]
private GameObject playerPrefab;
```

### bool の否定

`!` ではなく `== false` を使用する。

```csharp
// Good
if (isAlive == false) { ... }
if (candidates.Any() == false) { ... }

// Bad
if (!isAlive) { ... }
```

### 空文字列

`""` ではなく `string.Empty` を使用する。空文字列との比較には `string.IsNullOrEmpty` を使用する。

```csharp
// Good
var text = string.Empty;
if (string.IsNullOrEmpty(text)) { ... }

// Bad
var text = "";
if (text == string.Empty) { ... }
```

### XML ドキュメント

原則として、全ての型・メソッド・フィールド・プロパティに日本語の `<summary>` タグを付ける ( アクセス修飾子に関わらず public / internal / protected / private いずれも必須 )。`<param>`, `<returns>`, `<remarks>` も適宜使用する。オーバーライドメソッドには `/// <inheritdoc/>` を使用する。

これら以外のメソッド・フィールドにサマリコメントが無い場合は、必ず追加する。既存コードを編集する際に欠落を見つけた場合も同様に補完する。

```csharp
/// <summary>体力を全回復する</summary>
public void RecoverFullHealth() {
    health = maximumHealth;
}
```

### var の使用

型が明らかな場合は `var` を積極的に使用する。

### LINQ

コレクション操作には LINQ を積極的に活用する (`Where`, `Select`, `FirstOrDefault`, `Sum`, `OrderBy`, `Any` など)。

### コメントの書式

半角文字と全角文字の間にはスペースを入れる。ただし `「」` のだけスペース不要。
記号は全て半角を使用する。

```csharp
// Good
// コメント ( null の場合は ) コメント
// コメント「する」
// HP が 0 以下の場合はダメージを無効化する

// Bad
// コメント(nullの場合は)コメント
// コメント 「する」
// HPが0以下の場合はダメージを無効化する
```

### 文末の句点

サマリ ( `<summary>` 等 ) やコメントの文末に句点 `。` は基本的に付けない。句点を付けてよいのは、1 行の中に複数の文が並んでいる場合に限る ( 文の区切りとして必要なため )。コメントが複数行に渡っていても、1 行につき一文が縦に並んでいるだけで内容が連続していなければ各行とも句点は基本的に不要。

```csharp
// Good ( 一文の説明なので句点なし )
// HP が 0 以下の場合はダメージを無効化する

/// <summary>体力を全回復する</summary>

// Good ( 複数行だが 1 行一文なので句点なし )
// 攻撃対象を探索する
// 見つからなければ待機状態に戻る

// Good ( 1 行に複数文あるので区切りの句点を付ける )
// HP が 0 以下ならダメージを無効化する。無敵状態でも同様に無効化する

// Bad ( 一文だけなのに句点を付けている )
// HP が 0 以下の場合はダメージを無効化する。
```

### コメント内の具体的な数値

コメントにコード中のパラメーター値をそのまま書かない。値を変更した時にコメントとの不整合が起きるため、パラメーターがなくても伝わる場合は意味だけを記述する。

```csharp
// Good
color.a *= 0.3f; // 薄くする

// Bad
color.a *= 0.3f; // 30% 薄くする
```

### 不要な補足括弧

サマリやコメント内の `( ... )` による補足は、以下のいずれかに当てはまる場合は書かない。括弧での補足が多すぎると本筋が読みにくくなるため、本当に理解の助けになるものだけを残す。

- **実装の詳細を列挙していて、処理が変わるとずれるもの**: コードの具体的な構成要素や手順をそのまま書いた補足は、実装を変えるとコメントと不整合になるため書かない
- **直前の表現と同義で言い換えているだけのもの**: 補足を消しても同じ意味が伝わるなら冗長なだけなので書かない
- **コードを見れば自明な簡単な処理を説明しているだけのもの**: 処理が複雑でなく、ぱっと見て分かる内容を改めて補足しているだけなら書かない

逆に、補足が無いと意図・前提・制約が伝わらない場合 ( なぜそうするのか、どういう状態を指すのか等 ) は残す。

既存コードを編集する際にこれらの不要な補足を見つけた場合は、その場で削除する。

```csharp
// Bad ( 実装の構成要素を列挙していて、内容が変わるとずれる )
/// <summary>1 行分の表示要素 ( アイコン + 名称 + 個数 ) を生成する</summary>

// Good
/// <summary>1 行分の表示要素を生成する</summary>

// Bad ( 「有効である」と同義で言い換えているだけ )
/// <summary>現在の状態でこの項目が有効である ( 操作してよい ) か判定する</summary>

// Good
/// <summary>現在の状態でこの項目が有効であるか判定する</summary>

// Bad ( 簡単な処理をぱっと見たまま説明していて冗長 )
/// <summary>要素を横並びに配置する ( 区切り文字で連結する )</summary>

// Good
/// <summary>要素を横並びに配置する</summary>
```

### 括弧

全角括弧 `（）` は使用しない。半角括弧を使用し、括弧の前後と内側にスペースを入れる。

```csharp
// Good
// xxxxx ( あああ ) xxxxx

// Bad
// xxxxx（あああ）xxxxx
// xxxxx(あああ)xxxxx
// xxxxx (あああ) xxxxx
```

### #region の禁止

`#region` / `#endregion` は使用しない。既存処理を移植する場合も `#region` は削除する。

### クラスメンバの並び順

クラス内のメンバは原則として以下の順序で記述する。

1. 定数 ( `const`, `static readonly` 等 )
2. SerializeField ( インスペクタに公開するフィールド )
3. フィールド
4. プロパティ ( `_` 付きのバッキングフィールドが必要な場合は、対応するフィールドと上下にペアで並べる )
5. ゲッタープロパティ ( `_xxx` をそのまま返すだけのものではなく、バッキングフィールドのペアが存在せず計算結果や式本体で値を返すもの )
6. Unity ライフサイクルメソッド ( `Awake`, `Start`, `Update` 等 )
7. メソッド
8. コールバックメソッド ( `OnXxx` 系 )

バッキングフィールドとプロパティのペアは、上記の「フィールド」と「プロパティ」の境界をまたいでも構わないので、必ず隣接させて記述する ( `_` 付きフィールドを上、プロパティを下に置く )。

```csharp
public class PlayerController : MonoBehaviour {
    private const float MAXIMUM_SPEED = 10f;

    [SerializeField]
    private GameObject playerPrefab;

    private int level;
    private float power;

    private float _health;
    public float health => _health;

    public float healthRatio => _health / MAXIMUM_SPEED;

    private void Awake() { ... }

    private void Update() { ... }

    public void ApplyDamage(float amount) { ... }

    private void OnDamaged() { ... }
}
```

### 同じ分類のメンバ間の空行

「クラスメンバの並び順」で挙げた分類 ( 定数・フィールド・プロパティ等 ) が同じメンバ同士は、要素ごとに空行を空けず詰めて記述する。サマリコメント付きでも同様に詰める。宣言が長く途中で改行されるプロパティも対象に含む。空行は分類の切り替わりにのみ入れる。

要素ごとに空行を入れるとファイルが縦に間延びし、分類の境界 ( どこまでが定数でどこからがフィールドか ) も分かりにくくなる。

メソッドはこの規約の対象外で、各メソッド間に空行を入れる。

```csharp
// Good ( 同じ分類は詰め、分類の切り替わりだけ空行 )
/// <summary>攻撃間隔の最小秒数</summary>
private const float MINIMUM_ATTACK_INTERVAL = 0.5f;
/// <summary>一度に攻撃できる最大対象数</summary>
private const int MAXIMUM_TARGET_COUNT = 3;

/// <summary>現在の攻撃対象 ( 未選択の場合は null )</summary>
private Enemy target;
/// <summary>前回攻撃した時刻</summary>
private float lastAttackTime;

/// <summary>攻撃対象が選択されているかどうか</summary>
public bool hasTarget => target != null;

// Bad ( 同じ分類なのに要素ごとに空行を空けている )
/// <summary>攻撃間隔の最小秒数</summary>
private const float MINIMUM_ATTACK_INTERVAL = 0.5f;

/// <summary>一度に攻撃できる最大対象数</summary>
private const int MAXIMUM_TARGET_COUNT = 3;

/// <summary>現在の攻撃対象 ( 未選択の場合は null )</summary>
private Enemy target;

/// <summary>前回攻撃した時刻</summary>
private float lastAttackTime;
```

## C# 言語機能

### record 型

不変データの定義に `record` を使用:

```csharp
public record Damage(float value, bool isCritical = false);
public record StatusEffect(StatusType type, float duration, float magnitude);
public record ItemData(ItemType itemType, string name, int count);
```

### パターンマッチング

`switch` 式、`is` / `is not` パターンを積極的に使用:

```csharp
return state switch {
    GameState.Playing => HandlePlaying(),
    GameState.Paused => HandlePaused(),
    _ => throw new ArgumentException("未定義の状態が指定されました"),
};
```

### Null 安全

- `[CanBeNull]` (JetBrains) でNull許容を明示
- `[NonNullable]` カスタム属性で SerializeField の必須を明示
- `?.` (null 条件演算子) と `??` / `??=` (null 合体演算子) を使用

## 注意事項

- UI テキスト、コメント、ゲームメッセージは全て **日本語**
- 例外メッセージも日本語 (`throw new Exception("未定義の状態が指定されました")`)
- `== false` を統一的に使用する (プロジェクト全体のスタイル)
- `record` 型の `with` 式でイミュータブルな更新を行う
- `[NonNullable]` と `[NonSerialized]` 属性を適切に使い分ける
- `.meta` ファイルは作成しない (Unity が自動生成する)
