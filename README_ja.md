# Anjin

[![Meta file check](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml/badge.svg)](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml)
[![openupm](https://img.shields.io/npm/v/com.dena.anjin?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.dena.anjin/)

Unity製ゲームのオートパイロットツールです。
次の2つの要素で構成されています。

1. ゲーム中にロードされたSceneに応じて、対応するAgentを起動するディスパッチャ
2. Sceneに閉じた自動実行を実現するAgent

Agentとは、UI操作のプレイバックやモンキーテストなど、特定の操作を実行する小さく切り分けられたC#スクリプトです。
ビルトインで提供しているもののほか、ゲームタイトル固有のものを実装して使用できます。

Click [English](./README.md) for English page if you need.



## インストール方法

主に2通りの方法でインストールできます。

### Package Manager ウィンドウを使用する場合

1. Project Settings ウィンドウ（**Editor > Project Settings**）にある、**Package Manager** タブを開きます
2. **Scoped Registries** の下にある **+** ボタンをクリックし、次の項目を設定します（図 1）
   1. **Name:** `package.openupm.com`
   2. **URL:** `https://package.openupm.com`
   3. **Scope(s):** `com.dena`, `com.cysharp`, and `com.nowsprinting`
3. Package Managerウィンドウを開き（**Window > Package Manager**）、レジストリ選択ドロップダウンで **My Registries** を選択します（図 2）
4. `com.dena.anjin` パッケージの **Install** ボタンをクリックします

**図 1.** Project Settings ウィンドウの Package Manager タブ

![](Documentation~/ProjectSettings_Dark.png#gh-dark-mode-only)
![](Documentation~/ProjectSettings_Light.png#gh-light-mode-only)

**図 2.** Package Manager ウィンドウのレジストリ選択ドロップダウン

![](Documentation~/PackageManager_Dark.png/#gh-dark-mode-only)
![](Documentation~/PackageManager_Light.png/#gh-light-mode-only)

> [!NOTE]  
> scopesに `com.cysharp` と `com.nowsprinting` を忘れず追加してください。Anjin内で使用しています。

> [!NOTE]  
> Anjinパッケージ内のテストを実行する場合（package.jsonの `testables` に追加するとき）は、[Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) パッケージ v1.3以上が必要です。

### openupm-cli を使用する場合

[openupm-cli](https://github.com/openupm/openupm-cli) がインストールされている状態で、ターミナルから次のコマンドを実行します。

```bash
openupm add com.dena.anjin
```

Assembly Definition FileのDefine Constraintsに `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` が設定されていますので、原則リリースビルドからは除外されます。

### 推奨.gitignore

Anjinを起動すると、次のファイルが自動生成されます。
トラッキングする必要はありませんので、プロジェクトの.gitignoreファイルに追加することを推奨します。

```
/Assets/AutopilotState.asset*
```



## ゲームタイトルごとの設定

ゲームタイトルのプロジェクトにUPMパッケージをインストール後、以下の設定・実装を行います。

- AutopilotSettings.assetファイルを生成、設定
- 使用するAgentの.assetファイルを生成・設定
- 必要に応じて専用Agentの実装
- 必要に応じて初期化処理の実装

> [!NOTE]  
> ゲームタイトル固有のオートパイロット向けコードは、属するAssembly Definition FileのDefine Constraintsに `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` を設定することで、リリースビルドから除外できます


### オートパイロット設定ファイル（.asset）の生成

UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Autopilot Settings**
を選択すると生成できます。
ファイル名は任意で、プロジェクト内に複数作成して使い分けできます。

大きく3つの設定項目があります。

#### Agent割り当て設定

##### Scene Agent MappingとフォールバックAgent

Sceneごとに自動実行を行なうAgent設定ファイル（.asset）の対応付けを設定します。

リストになっていますので、`Scene` と `Agent` の組み合わせを設定してください。並び順は動作に影響しません。
リストに存在しないSceneでは、`Fallback Agent`に設定されたAgentが使用されます。

たとえば全てのSceneで `UGUIMonkeyAgent` が動くようにしたければ、`Scene Agent Maps` は空に、
`Fallback Agent` には `UGUIMonkeyAgent` の.assetファイルを設定します。

##### オブザーバーAgent

`Scene Agent Maps`と`Fallback Agent`とは独立して、常に並列に起動されるAgentを設定します。
v1.0.0時点では `EmergencyExitAgent` の使用を想定しています。
複数のAgentを並列起動する必要があるときは `ParallelCompositeAgent` を使用してください。

なお、ここに設定されたAgentは `DontDestroyOnLoad` **ではなく**、新しいSceneがロードされる度に破棄・生成される点に注意してください。

#### オートパイロット実行設定

この項目は、コマンドラインから上書きもできます（後述）。

<dl>
  <dt>Lifespan</dt><dd>実行時間上限を秒で指定します。デフォルトは300秒、0を指定すると無制限に動作します</dd>
  <dt>Random Seed</dt><dd>疑似乱数発生器に与えるシードを固定したいときに指定します（省略可）。なお、これはオートパイロットの使用する疑似乱数発生器に関する設定であり、ゲーム本体の疑似乱数発生器シードを固定するにはタイトル側での実装が必要です。</dd>
  <dt>Time Scale</dt><dd>Time.timeScaleを指定します。デフォルトは1.0</dd>
  <dt>JUnit Report Path</dt><dd>JUnit形式のレポートファイル出力パスを指定します（省略可）。オートパイロット実行の成否は、Unityエディターの終了コードでなくこのファイルを見て判断するのが確実です。errors, failuresともに0件であれば正常終了と判断できます。</dd>
  <dt>Logger</dt><dd>オートパイロットが使用するロガー指定します。省略時は <code>Debug.unityLogger</code> がデフォルトとして使用されます</dd>
  <dt>Reporter</dt><dd>対象のアプリケーションで発生したエラーを通知するレポータを指定します</dd>
</dl>

#### エラーハンドリング設定

異常系ログメッセージを捕捉してレポータで通知するフィルタを設定します。

<dl>
  <dt>handle Exception</dt><dd>例外を検知したらレポータで通知します</dd>
  <dt>handle Error</dt><dd>エラーを検知したらレポータで通知します</dd>
  <dt>handle Assert</dt><dd>アサート違反を検知したらレポータで通知します</dd>
  <dt>handle Warning</dt><dd>警告を検知したらレポータで通知します</dd>
  <dt>Ignore Messages</dt><dd>ここに設定した文字列を含むメッセージはレポータで通知しません</dd>
</dl>


### Agent設定ファイル（.asset）の生成

ビルトインのAgentを使用する場合でも、タイトル独自Agentを実装した場合でも、Unityエディタでそのインスタンス（.assetファイル）を生成する必要があります。

インスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Agent名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにAgent固有の設定項目が表示され、カスタマイズが可能です。
同じAgentでも設定の違うものを複数用意して、Sceneによって使い分けることができます。


### ロガー設定ファイル（.asset）の生成

ロガーインスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Logger名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにロガー固有の設定項目が表示され、カスタマイズが可能です。
同じロガーでも設定の違うものを複数用意して使い分けることができます。


### レポータ設定ファイル（.asset）の生成

レポータインスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Reporter名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにレポータ固有の設定項目が表示され、カスタマイズが可能です。
同じレポータでも設定の違うものを複数用意して使い分けることができます。



## 実行方法

次の2通りの方法でオートパイロットを実行できます。


### 1. Unityエディタ (GUI) の再生モードから実行

実行したい設定ファイル（AutopilotSettings）をインスペクタで開き、**実行**ボタンをクリックするとオートパイロットが有効な状態で再生モードに入ります。
設定された実行時間が経過するか、通常の再生モードと同じく再生ボタンクリックで停止します。


### 2. Play Modeテストから実行

非同期メソッド `LauncherFromTest.AutopilotAsync(string)` を使用することで、テストコード内でオートパイロットが動作します。
引数には `AutopilotSettings` ファイルパスを指定します。

```
[Test]
public async Task LaunchAutopilotFromTest()
{
  await LauncherFromTest.AutopilotAsync("Assets/Path/To/AutopilotSettings.asset");
}
```

> [!NOTE]  
> 実行中にエラーを検知すると `LogError` が出力されるため、そのテストは失敗と判定されます。

> [!WARNING]  
> テストのデフォルトタイムアウトは3分です。オートパイロットの実行時間が3分を超える場合は `Timeout` 属性でタイムアウト時間を指定してください。


### 3. コマンドラインから実行

コマンドラインから実行する場合、以下の引数を指定します。

```bash
$(UNITY) \
  -projectPath $(PROJECT_HOME) \
  -batchmode \
  -executeMethod DeNA.Anjin.Editor.Commandline.Bootstrap \
  -AUTOPILOT_SETTINGS Assets/Path/To/AutopilotSettings.asset
```

なお、

- `UNITY`にはUnityエディタへのパス、`PROJECT_HOME`には自動実行対象プロジェクトのルートを指定します
- `-AUTOPILOT_SETTINGS` には、実行したい設定ファイル（AutopilotSettings）のパスを指定します
- `-quit` は指定しないでください（Play modeに入らず終了してしまいます）
- `-nographics` は指定しないでください（GameViewウィンドウを表示できません）

また、以下の引数を追加することで一部の設定を上書きできます。
各引数の詳細は前述の「オートパイロット設定ファイル」の同名項目を参照してください。

<dl>
  <dt>LIFESPAN_SEC</dt><dd>実行時間上限を秒で指定します</dd>
  <dt>RANDOM_SEED</dt><dd>疑似乱数発生器に与えるシードを固定したいときに指定します</dd>
  <dt>TIME_SCALE</dt><dd>Time.timeScaleを指定します。デフォルトは1.0</dd>
  <dt>JUNIT_REPORT_PATH</dt><dd>JUnit形式のレポートファイル出力パスを指定します</dd>
  <dt>HANDLE_EXCEPTION</dt><dd>例外を検知したときに通知を行なうかを TRUE/ FALSEで上書きします</dd>
  <dt>HANDLE_ERROR</dt><dd>エラーを検知したときに通知を行なうかを TRUE/ FALSEで上書きします</dd>
  <dt>HANDLE_ASSERT</dt><dd>アサート違反を検知したときに通知を行なうかを TRUE/ FALSEで上書きします</dd>
  <dt>HANDLE_WARNING</dt><dd>警告を検知したときに通知を行なうかを TRUE/ FALSEで上書きします</dd>
</dl>

いずれも、キーの先頭に`-`を付けて`-LIFESPAN_SEC 60`のように指定してください。



## ビルトインAgent

以下のAgentが用意されています。これらをそのまま使用することも、プロジェクト独自のAgentを実装して使用することも可能です。


### UGUIMonkeyAgent

uGUIのコンポーネントをランダムに操作するAgentです。
実装にはオープンソースの[test-helper.monkey](https://github.com/nowsprinting/test-helper.monkey)パッケージを使用しています。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>実行時間</dt><dd>ランダム操作の実行時間を秒で指定します。0を指定するとほぼ無制限（TimeSpan.MaxValue）に動作します。この設定でAgentが終了してもオートパイロットおよびアプリ自体は終了しません。次にSceneが切り替わるまでなにもしない状態になります</dd>
  <dt>操作間隔</dt><dd>ランダム操作間のウェイト間隔をミリ秒で指定します</dd>
  <dt>コンポーネント探索時間</dt><dd>インタラクティブな要素を探索する秒数を指定します。探索時間がこれを超えるとエラーを発生させます</dd>
  <dt>タッチ&ホールド時間</dt><dd>タッチ&ホールドの継続時間をミリ秒で指定します</dd>
  <dt>Gizmo</dt><dd>もし有効ならモンキー操作中の GameView に Gizmo を表示します。もし無効なら GameView に Gizmo を表示しません</dd>
</dl>

#### *スクリーンショット設定:*

<dl>
  <dt>有効</dt><dd>スクリーンショット撮影を有効にします</dd>
  <dt>ディレクトリ</dt><dd><b>デフォルト値を使用: </b>スクリーンショットの保存先のディレクトリ名にデフォルト値を使用します。デフォルト値はコマンドライン引数 "-testHelperScreenshotDirectory" で指定します。コマンドライン引数も省略した場合は、Application.persistentDataPath + "/TestHelper/Screenshots/" が使用されます<br><b>パス: </b>スクリーンショットの保存先ディレクトリのパス</dd>
  <dt>ファイル名</dt><dd><b>デフォルト値を使用: </b>スクリーンショットのファイル名のプレフィックスにデフォルト値を使用します。デフォルト値はAgentの名前です<br><b>プレフィックス: </b>スクリーンショットのファイル名のプレフィックスを指定します</dd>
  <dt>拡大係数</dt><dd>解像度をあげるための係数。ステレオキャプチャモードと同時には設定できません</dd>
  <dt>ステレオキャプチャモード</dt><dd>ステレオレンダリングが有効な場合にどちらのカメラを使用するかを指定できます。拡大係数と同時には設定できません</dd>
</dl>

`UGUIMonkeyAgent` によって操作されたくない `GameObject` がある場合、
`TestHelper.Monkey.Annotations` アセンブリに含まれる `IgnoreAnnotation` コンポーネントをアタッチしておくことで操作を回避できます。
詳しくは後述の**Anjin Annotations**を参照してください。


### UGUIPlaybackAgent

[Automated QA](https://docs.unity3d.com/Packages/com.unity.automated-testing@latest)パッケージのRecorded Playback機能でレコーディングしたuGUI操作を再生するAgentです。

> [!NOTE]  
> Automated QAパッケージはプレビュー段階のため、破壊的変更や、パッケージ自体の開発中止・廃止もありえる点、ご注意ください。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Recorded Json</dt><dd>再生するレコーディングファイル (.json) を指定します</dd>
</dl>

Automated QAによる操作のレコーディングは、Unityエディターのメニューから
**Automated QA > Automated QA Hub > Recorded Playback**
で開くウィンドウから行ないます。
レコーディングファイル（.json）は Assets/Recordings/ フォルダ下に保存されますが、移動・リネームは自由です。

なお、Automated QAのRecorded Playback機能ではScene遷移をまたがって操作を記録ができますが、AnjinではSceneが切り替わったところでAgentも強制的に切り替わるため、再生も中断されてしまいます。
従って、レコーディングはScene単位に区切って行なうようご注意ください。

また、Automated QAによる操作のスクリーンショットを`Application.persistentDataPath/Anjin`下に保存しています。
各プラットフォームの`Application.persistentDataPath`はUnityマニュアルの
[Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
を参照してください。


### DoNothingAgent

なにもしないAgentです。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Lifespan Sec</dt><dd>なにもしない時間を秒で指定します。0を指定すると無制限になにもしません。この設定でAgentが終了してもオートパイロットおよびアプリ自体は終了しません。次にSceneが切り替わるまでなにもしない状態になります</dd>
</dl>


### ParallelCompositeAgent

複数のAgentを登録し、それを並列実行するAgentです。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agents</dt><dd>並列実行するAgentのリスト。CompositeAgentを指定して入れ子にすることも可能です</dd>
</dl>


### SerialCompositeAgent

複数のAgentを登録し、それを直列実行するAgentです。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agents</dt><dd>直列実行するAgentのリスト。CompositeAgentを指定して入れ子にすることも可能です</dd>
</dl>


### OneTimeAgent

1つの子Agentを登録し、それをAutopilot実行期間を通じて1回だけ実行できるAgentです。
2回目以降の実行はスキップされます。

たとえば、タイトル画面で初回だけ導線が異なる、ホーム画面で初回だけログインボーナス受け取りがあるといったゲームにおいて、 通信エラーなどによるタイトル画面戻しが発生してもそのまま実行するシナリオを構築できます。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agent</dt><dd>1回だけ実行できるAgent。CompositeAgentを指定して入れ子にすることも可能です</dd>
</dl>


### RepeatAgent

1つの子Agentを登録し、それを無限に繰り返し実行するAgentです。

SerialCompositeAgentと組み合わせることで、シナリオを何周もしたり、シナリオの最後のAgentだけを繰り返し実行することができます。
なお、有限回の繰り返しはサポートしません（SerialCompositeAgentで実現できるため）。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agent</dt><dd>繰り返し実行するAgent。CompositeAgentを指定して入れ子にすることも可能です</dd>
</dl>


### TimeBombAgent

内包するAgentが終了する前に解除メッセージを受信しないと失敗するAgent。

例えば、アウトゲームのチュートリアル（スマホゲームなどuGUIのタップ操作で完遂できるもの）を突破するには、次のように設定します。

1. `UGUIMonkeyAgent` を `Agent` に設定します。進行するボタン以外は操作できないはずです。`実行時間` は少し余裕をもって設定してください
2. チュートリアル完遂時にログに出力されるメッセージを `解除メッセージ` に設定します

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agent</dt><dd>実際に動作するAgent。このAgentが先に終了すると、TimeBombAgentは失敗します。</dd>
  <dt>解除メッセージ</dt><dd>このメッセージが先にログに出力されたら、TimeBombAgentは正常終了します。正規表現でも指定できます。</dd>
</dl>


### EmergencyExitAgent

`DeNA.Anjin.Annotations` アセンブリに含まれる `EmergencyExit` コンポーネントの出現を監視し、表示されたら即クリックするAgentです。
たとえば通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなる振る舞いが含まれるゲームで利用できます。

常に、他の（実際にゲーム操作を行なう）Agentと同時に起動しておく必要があります。
`ParallelCompositeAgent` でも実現できますが、AutopilotSettingsに `Observer Agent` として設定するほうが簡単です。



## ビルトイン ロガー

以下のロガータイプが用意されています。これらをそのまま使用することも、プロジェクト独自のロガーを実装して使用することも可能です。


### Composite Logger

複数のロガーを登録し、そのすべてにログ出力を委譲するロガーです。

このロガーのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Loggers</dt><dd>ログ出力を委譲するLoggerのリスト</dd>
</dl>


### Console Logger

ログをコンソールに出力するロガーです。

このロガーのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>フィルタリングLogType</dt><dd>選択したLogType以上のログ出力のみを有効にします</dd>
</dl>


### File Logger

ログを指定ファイルに出力するロガーです。

このロガーのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>出力ファイルパス</dt><dd>ログ出力ファイルのパス。プロジェクトルートからの相対パスまたは絶対パスを指定します。プレイヤー実行では相対パスの起点は <code>Application.persistentDataPath</code> になります。
        コマンドライン引数 <code>-FILE_LOGGER_OUTPUT_PATH</code> で上書きできますが、複数のFileLoggerを定義しているときに指定すると、すべての出力パスが上書きされますので注意してください。</dd>
  <dt>フィルタリングLogType</dt><dd>選択したLogType以上のログ出力のみを有効にします</dd>
  <dt>タイムスタンプを出力</dt><dd>ログエンティティにタイムスタンプを出力します</dd>
</dl>



## ビルトイン レポータ

以下のレポータタイプが用意されています。これらをそのまま使用することも、プロジェクト独自のレポータを実装して使用することも可能です。


### Composite Reporter

複数のレポータを登録し、そのすべてにレポート送信を委譲するレポータです。

このレポータのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Reporters</dt><dd>レポート送信を委譲するReporterのリスト</dd>
</dl>


### Slack Reporter

Slackにレポート送信するレポータです。

このレポータのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Slack Token</dt><dd>Slack通知に使用するWeb APIトークン（省略時は通知されない）。コマンドライン引数 <code>-SLACK_TOKEN</code> で上書きできます。</dd>
  <dt>Slack Channels</dt><dd>Slack通知を送るチャンネル（省略時は通知されない）。カンマ区切りで複数指定できます。コマンドライン引数 <code>-SLACK_CHANNELS</code> で上書きできます。</dd>
  <dt>Mention Sub Team IDs</dt><dd>Slack通知メッセージでメンションするチームのIDをカンマ区切りで指定します</dd>
  <dt>Add Here In Slack Message</dt><dd>Slack通知メッセージに@hereを付けます。デフォルトはoff</dd>
</dl>



## ゲームタイトル独自処理の実装

ゲームタイトル固有のAgent等を実装する場合、リリースビルドへの混入を避けるため、専用のアセンブリに分けることをおすすめします。
Assembly Definition File (asmdef) のAuto Referencedをoff、Define Constraintsに `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` を設定することで、リリースビルドからは除外できます。

このasmdef及び格納フォルダは、Projectウィンドウの任意の場所でコンテキストメニューを開き
**Create > Anjin > Title Own Assembly Folder**
を選択することで生成できます。


### タイトル独自Agent

タイトル独自のAgentは、`Anjin.Agents.AbstractAgent` を継承して作ります。
メソッド `UniTask Run(CancellationToken)` に、Agentが実行する処理を実装するだけです。

`AbstractAgent` に定義された以下のフィールドを利用できます。各インスタンスは `Run` メソッド呼び出し前に設定されています。

- `Logger` :  `UnityEngine.ILogger` の実装です。現時点ではコンソール出力ですが、ファイルロガーに差し替える予定があります。ログ出力にはこちらを使ってください
- `Random` :  `Anjin.Utilities.IRandom` の実装です。設定ファイルもしくは起動時引数で指定されたシードから作られています

なお、`[CreateAssetMenu]`アトリビュートを設定しておくとコンテキストメニューからインスタンス生成ができて便利です。


### タイトル独自事前処理

タイトル独自の初期化処理が必要な場合、初期化を行なう `public static` メソッドに `InitializeOnLaunchAutopilot` 属性を付与してください。
オートパイロットの起動処理の中でメソッドを呼び出します。

```csharp
[InitializeOnLaunchAutopilot]
public static void InitializeOnLaunchAutopilotMethod()
{
    // プロジェクト固有の初期化処理
}
```

非同期メソッドにも対応しています。

```csharp
[InitializeOnLaunchAutopilot]
public static async UniTask InitializeOnLaunchAutopilotMethod()
{
    // プロジェクト固有の初期化処理
}
```

なお、オートパイロットの起動処理は、`RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`（`RuntimeInitializeOnLoadMethod`のデフォルト）で実行しています。
また`RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)`で、Configurable Enter Play Modeのための初期化処理を実装しています。



## Anjin Annotations

Anjinの操作を制御するためのアノテーションを定義しています。
`DeNA.Anjin.Annotations` アセンブリをAssembly Definition Referencesに追加して使用してください。
仕組み上、リリースビルドに含まれることになりますのでご注意ください。

> [!NOTE]  
> アノテーションアセンブリをリリースビルドから除いても、アセットバンドルビルドされたSceneやPrefabにはアノテーションコンポーネントへのリンクが残ります。
> そのため、インスタンス化の際に警告ログが出力されてしまいます。
> これを回避するため、リリースビルドにアノテーションアセンブリを含めるようにしています。


### IgnoreAnnotation

このコンポーネントがアタッチされた`GameObject`は、`UGUIMonkeyAgent`によって操作されることを避けることができます。


### EmergencyExit

このコンポーネントがアタッチされた`Button`が表示されると、`EmergencyExitAgent`はすぐにクリックを試みます。
通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなるボタンに付けることを想定しています。



## トラブルシューティング

### プロジェクトを再生モードにすると勝手にオートパイロットが動いてしまう

Anjinの実行状態を永続化している `AutopilotState.asset` が不正な状態になっている恐れがあります。
インスペクタで開いて**Reset**ボタンをクリックしてください。

それでも解決しない場合、 `AutopilotState.asset` を削除してみてください。


### [CompilerError] Argument 1: Cannot convert to 'System.Threading.CancellationToken'

次のコンパイルエラーが発生するケースが報告されています。

```
[CompilerError] Argument 1:.
Cannot convert from 'DeNA.Anjin.Reporters.SlackReporter.CoroutineRunner' to 'System.Threading.CancellationToken'
Compiler Error at Library\PackageCache\com.dena.anjin@1.0.1\Runtime\Reporters\SlackReporter.cs:66 column 53
```

これは、プロジェクトにインストールされているUniTaskがv2.3.0未満のときに発生します。
この箇所で使用している `UniTask.WaitForEndOfFrame(MonoBehaviour)` は、UniTask v2.3.1で追加されたAPIです。

通常、インストール方法に従っていれば適正なバージョンのUniTaskがインストールされます。
しかし、プロジェクトに埋め込みパッケージとしてUniTaskが配置されていたりすればそちらが優先されてしまいます。
Unityエディターが生成するPackages/packages-lock.jsonの中身を確認するか、お使いのIDEのコード定義ジャンプ機能で `UniTask.WaitForEndOfFrame()` のソースファイルがどこにあるかを確認するなどして、古いUniTaskがインストールされている原因を突き止められます。



## ライセンス

MIT License


## コントリビュート

IssueやPull requestを歓迎します。

Pull requestには `enhancement`, `bug`, `chore`, `documentation` といったラベルを付けてもらえるとありがたいです。
ブランチ名から自動的にラベルを付ける設定もあります。[PR Labeler settings](.github/pr-labeler.yml) を参照してください。


## 開発方法

本リポジトリをUnityプロジェクトのサブモジュールとして Packages/ ディレクトリ下に置いてください。

ターミナルから次のコマンドを実行します。

```bash
git submodule add https://github.com/dena/Anjin.git Packages/com.dena.anjin
```

> [!WARNING]  
> Anjinパッケージ内のテストを実行するためには、次のパッケージのインストールが必要です。
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - [Test Helper](https://github.com/nowsprinting/test-helper) package v0.4.2 or later

テスト専用のUnityプロジェクトを生成し、Unityバージョンを指定してテストを実行するには、次のコマンドを実行します。

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```


## リリースワークフロー

**Actions > Create release pull request > Run workflow** を実行し、作られたpull requestをデフォルトブランチにマージすることでリリース処理が実行されます。
（もしくは、デフォルトブランチのpackage.json内のバージョン番号を書き換えます）

リリース処理は、[Release](.github/workflows/release.yml)ワークフローで自動的に行われます。
tagが付与されると、OpenUPMがtagを収集して更新してくれます。

以下の操作は手動で行わないでください。

- リリースタグの作成
- ドラフトリリースの公開
