# Anjin

[![Meta file check](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml/badge.svg)](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml)
[![openupm](https://img.shields.io/npm/v/com.dena.anjin?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.dena.anjin/)

Click [English](./README.md) for English page if you need.

**Anjin**は、Unity製ゲーム向けのオートパイロット フレームワークです。
次の2つの要素で構成されています。

1. ゲーム中にロードされたSceneに応じて、対応するAgentを起動するディスパッチャ
2. Sceneに閉じた自動実行を実現するAgent

Agentとは、UI操作のプレイバックやモンキーテストなど、特定の操作を実行する小さく切り分けられたC#スクリプトです。
ビルトインで提供しているもののほか、ゲームタイトル固有のものを実装して使用できます。



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

ゲームタイトルのUnityプロジェクトにUPMパッケージをインストール後、以下の設定・実装を行います。

- AutopilotSettings.assetファイルを生成、設定
- 使用するAgentの.assetファイルを生成・設定
- 必要に応じてカスタムAgentの実装
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

##### Scene横断Agents

`Scene Agent Maps` および `Fallback Agent`とは独立して、Sceneを横断して実行されるAgentを設定します。

指定されたAgentの寿命はオートパイロット本体と同じになります（つまり、`DontDestroyOnLoad` を使用します）。
たとえば、`ErrorHandlerAgent` や `UGUIEmergencyExitAgent` などを指定します。

#### オートパイロット実行設定

この項目は、コマンドラインから上書きもできます（後述）。

<dl>
  <dt>Lifespan</dt><dd>実行時間上限を秒で指定します。デフォルトは300秒、0を指定すると無制限に動作します</dd>
  <dt>Random Seed</dt><dd>疑似乱数発生器に与えるシードを固定したいときに指定します（省略可）。なお、これはオートパイロットの使用する疑似乱数発生器に関する設定であり、ゲーム本体の疑似乱数発生器シードを固定するにはゲームタイトル側での実装が必要です。</dd>
  <dt>Time Scale</dt><dd>Time.timeScaleを指定します。デフォルトは1.0</dd>
  <dt>Loggers</dt><dd>オートパイロットが使用するLogger指定します。省略時は <code>Debug.unityLogger</code> がデフォルトとして使用されます</dd>
  <dt>Reporters</dt><dd>オートパイロット終了時に通知を行なうReporterを指定します</dd>
</dl>


### Agent設定ファイル（.asset）の生成

ビルトインのAgentを使用する場合でも、カスタムAgentを実装した場合でも、Unityエディタでそのインスタンス（.assetファイル）を生成する必要があります。

インスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Agent名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにAgent固有の設定項目が表示され、カスタマイズが可能です。
同じAgentでも設定の違うものを複数用意して、Sceneによって使い分けることができます。


### Logger設定ファイル（.asset）の生成

Loggerインスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Logger名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにLogger固有の設定項目が表示され、カスタマイズが可能です。
同じLoggerでも設定の違うものを複数用意して使い分けることができます。


### Reporter設定ファイル（.asset）の生成

Reporterインスタンスは、UnityエディタのProjectウィンドウで右クリックしてコンテキストメニューを開き、
**Create > Anjin > Reporter名**
を選択すると生成できます。ファイル名は任意です。

生成したファイルを選択すると、インスペクタにReporter固有の設定項目が表示され、カスタマイズが可能です。
同じReporterでも設定の違うものを複数用意して使い分けることができます。



## 実行方法

次の3通りの方法で、Unityエディタ内でオートパイロットを実行できます。


### 1. Unityエディタ（GUI）の再生モードで実行

実行したい設定ファイル（AutopilotSettings）をインスペクタで開き、**実行**ボタンをクリックするとオートパイロットが有効な状態で再生モードに入ります。
設定された実行時間が経過するか、通常の再生モードと同じく再生ボタンクリックで停止します。


### 2. コマンドラインから実行

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
</dl>

いずれも、キーの先頭に`-`を付けて`-LIFESPAN_SEC 60`のように指定してください。


### 3. Play Modeテスト内で実行

非同期メソッド `Launcher.LaunchAutopilotAsync(string)` を使用することで、テストコード内でオートパイロットが動作します。
引数には `AutopilotSettings` ファイルパスを指定します。

```
[Test]
public async Task LaunchAutopilotInTest()
{
  // 最初のSceneをロード
  await SceneManager.LoadSceneAsync(0);

  // オートパイロットを起動
  await Launcher.LaunchAutopilotAsync("Assets/Path/To/AutopilotSettings.asset");
}
```

> [!WARNING]  
> テストのデフォルトタイムアウトは3分です。オートパイロットの実行時間が3分を超える場合は `Timeout` 属性でタイムアウト時間を指定してください。

> [!WARNING]  
> テストをプレイヤーで実行するときは、必要な設定ファイルを `Resources` フォルダに置き、ビルドに含まれるようにしてください。テストのプレイヤービルドに処理を挟むには `IPrebuildSetup` および `IPostBuildCleanup` が利用できます。

> [!NOTE]  
> テストランナーに `LogException` または `LogError` 出力を検知されると、そのテストは失敗と判定されます。エラーハンドリングをAnjinで行なう前提で `LogAssert.ignoreFailingMessages` で抑止してもいいでしょう。



## ビルトインAgent

以下のAgentが用意されています。これらをそのまま使用することも、ゲームタイトル固有のカスタムAgentを実装して使用することも可能です。


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
  <dt>ディレクトリ</dt><dd><b>デフォルト値を使用: </b>スクリーンショットの保存先のディレクトリ名にデフォルト値を使用します。デフォルト値はコマンドライン引数 "-testHelperScreenshotDirectory" で指定します。コマンドライン引数も省略した場合は、`Application.persistentDataPath` + "/TestHelper/Screenshots/" が使用されます<br><b>パス: </b>スクリーンショットの保存先ディレクトリのパス</dd>
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
各プラットフォームの `Application.persistentDataPath` はUnityマニュアルの
[Scripting API: Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
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

たとえば、タイトル画面で初回だけ導線が異なる、ホーム画面で初回だけログインボーナス受け取りがあるといったゲームにおいて、 通信エラーなどによるタイトル画面戻しが発生してもそのまま実行するテストシナリオを構築できます。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Agent</dt><dd>1回だけ実行できるAgent。CompositeAgentを指定して入れ子にすることも可能です</dd>
</dl>


### RepeatAgent

1つの子Agentを登録し、それを無限に繰り返し実行するAgentです。

SerialCompositeAgentと組み合わせることで、一連の操作を何周もしたり、特定のAgentだけを繰り返し実行することができます。
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


### ErrorHandlerAgent

異常系ログメッセージを捕捉してオートパイロットの実行を停止するAgentです。

常に、他の（実際にゲーム操作を行なう）Agentと同時に起動しておく必要があります。
`ParallelCompositeAgent` でも実現できますが、AutopilotSettingsに `Scene Crossing Agents` として設定するほうが簡単です。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Handle Exception</dt><dd>例外ログを検知したとき、オートパイロットを停止します。
        コマンドライン引数 <code>-HANDLE_EXCEPTION</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Handle Error</dt><dd>エラーログを検知したとき、オートパイロットを停止します。
        コマンドライン引数 <code>-HANDLE_ERROR</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Handle Assert</dt><dd>アサートログを検知したとき、オートパイロットを停止します。
        コマンドライン引数 <code>-HANDLE_ASSERT</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Handle Warning</dt><dd>警告ログを検知したとき、オートパイロットを停止します。
        コマンドライン引数 <code>-HANDLE_WARNING</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Ignore Messages</dt><dd>指定された文字列を含むログメッセージは停止条件から無視されます。正規表現も使用できます。エスケープは単一のバックスラッシュ (`\`) です</dd>
</dl>


### EmergencyExitAgent

`DeNA.Anjin.Annotations` アセンブリに含まれる `EmergencyExitAnnotations` コンポーネントの出現を監視し、表示されたら即クリックするAgentです。
たとえば通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなる振る舞いが含まれるゲームで利用できます。

常に、他の（実際にゲーム操作を行なう）Agentと同時に起動しておく必要があります。
`ParallelCompositeAgent` でも実現できますが、AutopilotSettingsの `Scene Crossing Agents` に追加するほうが簡単です。



## ビルトインLogger

以下のLoggerタイプが用意されています。これらをそのまま使用することも、ゲームタイトル固有のカスタムLoggerを実装して使用することも可能です。


### ConsoleLogger

ログをコンソールに出力するLoggerです。

このLoggerのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>フィルタリングLogType</dt><dd>選択したLogType以上のログ出力のみを有効にします</dd>
</dl>


### FileLogger

ログを指定ファイルに出力するLoggerです。

このLoggerのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>出力ファイルパス</dt><dd>ログ出力ファイルのパス。プロジェクトルートからの相対パスまたは絶対パスを指定します。プレイヤー実行では相対パスの起点は <code>Application.persistentDataPath</code> になります。
        コマンドライン引数 <code>-FILE_LOGGER_OUTPUT_PATH</code> で上書きできますが、複数のFileLoggerを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>フィルタリングLogType</dt><dd>選択したLogType以上のログ出力のみを有効にします</dd>
  <dt>タイムスタンプを出力</dt><dd>ログエンティティにタイムスタンプを出力します</dd>
</dl>



## ビルトインReporter

以下のReporterタイプが用意されています。これらをそのまま使用することも、ゲームタイトル固有のカスタムReporterを実装して使用することも可能です。


### JUnitXmlReporter

JUnit XMLフォーマットのレポートファイルを出力するReporterです。
オートパイロット実行の成否は、Unityエディターの終了コードでなくこのファイルを見て判断するのが確実です。errors, failuresともに0件であれば正常終了と判断できます。

このReporterのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>出力ファイルパス</dt><dd>JUnit XMLレポートファイルの出力パス。プロジェクトルートからの相対パスまたは絶対パスを指定します。プレイヤー実行では相対パスの起点は <code>Application.persistentDataPath</code> になります。
        コマンドライン引数 <code>-JUNIT_REPORT_PATH</code> で上書きできますが、複数のJUnitXmlReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
</dl>


### SlackReporter

Slackにレポート送信するReporterです。

このReporterのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Slack Token</dt><dd>通知に使用するSlack BotのOAuthトークン（省略時は通知されない）。
        コマンドライン引数 <code>-SLACK_TOKEN</code> で上書きできますが、複数のSlackReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Slack Channels</dt><dd>通知を送るチャンネル（省略時は通知されない）。カンマ区切りで複数指定できます。
        なお、チャンネルにはBotを招待しておく必要があります。
        コマンドライン引数 <code>-SLACK_CHANNELS</code> で上書きできますが、複数のSlackReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Mention Sub Team IDs</dt><dd>エラー終了時に送信する通知をメンションするチームのIDをカンマ区切りで指定します</dd>
  <dt>Add @here</dt><dd>エラー終了時に送信する通知に@hereを付けます</dd>
  <dt>Lead Text</dt><dd>エラー終了時に送信する通知のリード文。OSの通知に使用されます。"{message}" のようなプレースホルダーを指定できます</dd>
  <dt>Message</dt><dd>エラー終了時に送信するメッセージ本文のテンプレート。"{message}" のようなプレースホルダーを指定できます</dd>
  <dt>Color</dt><dd>エラー終了時に送信するメッセージのアタッチメントに指定する色（デフォルト：Slackの "danger" と同じ赤色）</dd>
  <dt>Screenshot</dt><dd>エラー終了時にスクリーンショットを撮影します（デフォルト: on）</dd>
  <dt>Normally terminated report</dt><dd>正常終了時にもレポートを送信します（デフォルト: off）</dd>
</dl>

Slack Botは次のページで作成できます。  
[Slack API: Applications](https://api.slack.com/apps)

Slack Botには次の権限が必要です。

- chat:write
- files:write

リード及びメッセージ本文のテンプレートに記述できるプレースホルダーは次のとおりです。

- "{message}": 終了要因メッセージ（エラーログのメッセージなど）
- "{settings}": 実行中の AutopilotSettings 名
- "{env.KEY}": 環境変数



## ゲームタイトル固有の実装

ゲームタイトル固有のカスタムAgentや初期化処理を実装する場合、リリースビルドへの混入を避けるため、専用のアセンブリに分けることをおすすめします。
Assembly Definition File (asmdef) のAuto Referencedをoff、Define Constraintsに `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` を設定することで、リリースビルドからは除外できます。

このasmdef及び格納フォルダは、Projectウィンドウの任意の場所でコンテキストメニューを開き
**Create > Anjin > Game Title Specific Assembly Folder**
を選択することで生成できます。


### カスタムAgent

カスタムAgentは、`Anjin.Agents.AbstractAgent` を継承して作ります。
メソッド `UniTask Run(CancellationToken)` に、Agentが実行する処理を実装するだけです。

`AbstractAgent` に定義された以下のフィールドを利用できます。各インスタンスは `Run` メソッド呼び出し前に設定されています。

- `Logger` :  `UnityEngine.ILogger` の実装です。AutopilotSettingsで設定されたLoggerがセットされます。Agent内のログ出力にはこちらを使用してください
- `Random` :  `Anjin.Utilities.IRandom` の実装です。AutopilotSettingsもしくは起動時引数で指定されたシードから作られています

なお、`[CreateAssetMenu]`アトリビュートを設定しておくとコンテキストメニューからインスタンス生成ができて便利です。


### カスタムLogger

カスタムLoggerは、`Anjin.Logers.AbstractLoggerAsset` を継承して作ります。
`UnityEngine.ILogger` の実装を返すプロパティ `Logger { get; }` を実装する必要があります。

なお、`[CreateAssetMenu]`アトリビュートを設定しておくとコンテキストメニューからインスタンス生成ができて便利です。


### カスタムReporter

カスタムReporterは、`Anjin.Reporters.AbstractReporter` を継承して作ります。
メソッド `UniTask PostReportAsync()` を実装するだけです。

なお、`[CreateAssetMenu]`アトリビュートを設定しておくとコンテキストメニューからインスタンス生成ができて便利です。


### ゲームタイトル固有の初期化処理

ゲームタイトル固有の初期化処理が必要な場合、初期化を行なう `static` メソッドに `InitializeOnLaunchAutopilot` 属性を付与してください。
オートパイロットの起動処理の中でメソッドを呼び出します。

```csharp
[InitializeOnLaunchAutopilot]
public static void InitializeOnLaunchAutopilotMethod()
{
    // ゲームタイトル固有の初期化処理
}
```

非同期メソッドにも対応しています。

```csharp
[InitializeOnLaunchAutopilot]
private static async UniTask InitializeOnLaunchAutopilotMethodAsync()
{
    // ゲームタイトル固有の初期化処理
}
```

> [!NOTE]  
> 引数に呼び出し順序を指定できます。値の低いメソッドから順に呼び出されます。

> [!NOTE]  
> オートパイロットの起動処理は、`RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`（`RuntimeInitializeOnLoadMethod`のデフォルト）で実行しています。
> また`RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)`で、Configurable Enter Play Modeのための初期化処理を実装しています。



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


### EmergencyExitAnnotations

このコンポーネントがアタッチされた`Button`が表示されると、`EmergencyExitAgent`はすぐにクリックを試みます。
通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなるボタンに付けることを想定しています。



## プレイヤービルドでの実行［実験的機能］

Anjinをプレイヤービルドに含めることで、プレイヤー（実機）でもオートパイロットを実行できます。

> [!WARNING]  
> この機能は実験的機能です。
> また、Anjin本体はメモリアロケーションなどをあまり気にせず作られています。パフォーマンステストに使うときは注意してください。


### ビルド方法

#### 1. カスタムスクリプトシンボル `DENA_AUTOPILOT_ENABLE` および `COM_NOWSPRINTING_TEST_HELPER_ENABLE` を設定する

カスタムスクリプトシンボルを設定します。設定方法はUnityマニュアルの
[カスタム製スクリプトシンボル - Unity マニュアル](https://docs.unity3d.com/ja/current/Manual/CustomScriptingSymbols.html)
を参照してください。


#### 2. 設定ファイルをビルドに含める

必要な設定ファイルを `Resources` フォルダに置き、ビルドに含まれるようにしてください。
プレイヤービルドに処理を挟むには `IPreprocessBuildWithReport` および `IPostprocessBuildWithReport` が利用できます。


#### 3. iCloudバックアップ対象から外す（任意）

プレイヤー実行では、ビルトインのFile LoggerやAgentのスクリーンショットなどは `Application.persistentDataPath` 下に出力されます。
このパスはデフォルトでiCloudバックアップ対象のため、不要であれば除外します。

```csharp
UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
```


#### 4. デバッグメニューにオートパイロット起動ボタンを追加する（任意）

たとえば [UnityDebugSheet](https://github.com/Haruma-K/UnityDebugSheet) から起動するには次のようにします。

```csharp
AddButton("Launch autopilot", clicked: () =>
  {
    _drawerController.SetStateWithAnimation(DrawerState.Min); // 先にデバッグメニューを閉じる

#if UNITY_EDITOR
    const string Path = "Assets/Path/To/AutopilotSettings.asset";
#else
    const string Path = "Path/To/AutopilotSettings"; // Resourcesからの相対パスを指定。拡張子（.asset）は不要です
#endif
    Launcher.LaunchAutopilotAsync(Path).Forget();
});
```

> [!NOTE]  
> `Launcher.LaunchAutopilotAsync` は、引数に `AutopilotSettings` インスタンスを渡すオーバーロードも使用できます。


### 起動方法

デバッグメニューから起動するほか、コマンドライン引数で起動を指示できます。
コマンドラインから実行する場合、以下の引数を指定します。

```bash
$(ROM) -LAUNCH_AUTOPILOT_SETTINGS Path/To/AutopilotSettings
```

なお、

- `ROM` にはプレイヤービルド実行ファイルのパスを指定します
- `-LAUNCH_AUTOPILOT_SETTINGS` には、実行したい設定ファイル（AutopilotSettings）のパスを指定します。パスは Resources フォルダからの相対パスで、拡張子（.asset）は不要です

ほか、エディターでの実行と同じ引数を指定できます。

> [!NOTE]  
> プレイヤー実行では、ビルトインのFile LoggerやAgentのスクリーンショットなどは `Application.persistentDataPath` 下に出力されます。  
> 各プラットフォームのパスはUnityマニュアルの
>[Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html)
> を参照してください。

> [!NOTE]  
> Androidはコマンドライン引数の指定方法が異なります。Unityマニュアルの
> [Android プレイヤーのコマンドライン引数を指定する - Unity マニュアル](https://docs.unity3d.com/ja/current/Manual/android-custom-activity-command-line.html)
> を参照してください。



## トラブルシューティング

### プロジェクトを再生モードにすると勝手にオートパイロットが動いてしまう

Anjinの実行状態を永続化している `AutopilotState.asset` が不正な状態になっている恐れがあります。
インスペクタで開いて**Reset**ボタンをクリックしてください。

それでも解決しない場合、 `AutopilotState.asset` を削除してみてください。


### \[CompilerError\] Argument 1: Cannot convert to 'System.Threading.CancellationToken'

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


### 実行中に "settings has been obsolete" 警告が出る

たとえば次のような警告メッセージが出力されることがあります。

```
Slack settings in AutopilotSettings has been obsolete.
Please delete the value using Debug Mode in the Inspector window. And create a SlackReporter asset file.
```

すでに新しい設定方法（上例では `SlackReporter` ）に移行済みであっても、廃止されたフィールドに値が残っていることを警告されています。
設定ファイルをInspectorウィンドウで開き、**Debug Mode** に切り替えることで廃止されたフィールドを編集できます。

Inspectorウィンドウの操作については、Unityマニュアルの
[Inspector の使用 - Unity マニュアル](https://docs.unity3d.com/ja/current/Manual/InspectorOptions.html)
を参照してください。



## ライセンス

MIT License


## コントリビュート

IssueやPull requestを歓迎します。
Pull requestには `enhancement`, `bug`, `chore`, `documentation` といったラベルを付けてもらえるとありがたいです。
ブランチ名から自動的にラベルを付ける設定もあります。[PR Labeler settings](.github/pr-labeler.yml) を参照してください。


おおまかな機能追加の受け入れ方針は次のとおりです。

- すべてのビルトイン機能は、Unityエディタのインスペクタウィンドウで設定を完結して使用できること
- `Autopilot` 本体への機能追加は極力避け、Agentなどによる拡張を検討する
- 汎用的でないAgentの追加は控える。ブログやGistでの公開、もしくはSamplesに置くことを検討する


次の機能要望およびPull requestは、Anjinのコンセプトに反するためリジェクトされます。

- 複数のテストシナリオ（AutopilotSettings）を連続実行する機能。Play Modeテストから実行する機能を利用してください
- AutopilotSettingsに「開始Scene」を指定できる機能。どのSceneからでも動作するテストシナリオを組むことを推奨しています


## 開発方法

本リポジトリをUnityプロジェクトのサブモジュールとして Packages/ ディレクトリ下に置いてください。

ターミナルから次のコマンドを実行します。

```bash
git submodule add https://github.com/dena/Anjin.git Packages/com.dena.anjin
```

> [!WARNING]  
> Anjinパッケージ内のテストを実行するためには、次のパッケージのインストールが必要です。
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - [Test Helper](https://github.com/nowsprinting/test-helper) package v0.7.2 or later

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
