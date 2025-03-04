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

![](Documentation~/PackageManager_Dark.png#gh-dark-mode-only)
![](Documentation~/PackageManager_Light.png#gh-light-mode-only)

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
トラッキングする必要はありませんので、プロジェクトの.gitignoreファイル（もしくはお使いのVCSの無視設定）に追加することを推奨します。

```
/[Aa]ssets/[Aa]utopilot[Ss]tate.asset*
```

また、Automated QAパッケージの機能を使用すると、次のファイルが生成されます。
これらも同様に無視設定することを推奨します。

```
/[Aa]ssets/[Aa]utomated[Qq][Aa]*
/[Aa]ssets/[Rr]ecordings*
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

> [!WARNING]  
> 実行中に例外が発生した時点でオートパイロットを中断するために、[ErrorHandlerAgent](#ErrorHandlerAgent) の設定をお勧めします。

#### オートパイロット実行時間設定

<dl>
  <dt>実行時間</dt><dd>実行時間上限を秒で指定します。デフォルトは300秒で、0を指定すると無制限に動作します。
        この項目は、コマンドラインから上書きもできます（後述）。</dd>
  <dt>終了コード</dt><dd>オートパイロットの実行時間が満了したときに使用される終了コードを選択します</dd>
  <dt>カスタム終了コード</dt><dd>終了コードを整数値で指定します</dd>
  <dt>メッセージ</dt><dd>オートパイロットの実行時間が満了したとき、Reporterから送信されるメッセージ</dd>
</dl>

#### オートパイロット実行設定

<dl>
  <dt>擬似乱数シード</dt><dd>疑似乱数発生器に与えるシード値を固定したいときに指定します（省略可）。なお、これはオートパイロットの使用する疑似乱数発生器に関する設定であり、ゲーム本体の疑似乱数発生器シードを固定するにはゲームタイトル側での実装が必要です。
        この項目は、コマンドラインから上書きもできます（後述）。</dd>
  <dt>タイムスケール</dt><dd>Time.timeScaleを指定します。デフォルトは1.0。
        この項目は、コマンドラインから上書きもできます（後述）。</dd>
  <dt>出力ルートパス</dt><dd>Agent、Logger、および Reporter が出力するファイルのルートディレクトリパスを指定します。相対パスが指定されたときの起点は、エディターではプロジェクトルート、プレーヤーでは Application.persistentDataPath になります。
        この項目は、コマンドラインから上書きもできます（後述）。</dd>
  <dt>スクリーンショット出力パス</dt><dd>Agent が撮影するスクリーンショットの出力ディレクトリ パスを指定します。相対パスが指定されたとき、outputRootPath が起点となります。
        この項目は、コマンドラインから上書きもできます（後述）。</dd>
  <dt>スクリーンショットを消去</dt><dd>オートパイロットを起動するとき、screenshotsPath 下のスクリーンショットを消去します</dd>
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

実行したい設定ファイル（AutopilotSettings）をインスペクタで開き、**実行** ボタンをクリックすると、オートパイロットが起動します。
設定された実行時間が経過するか、**停止** ボタンクリックで停止します。

> [!TIP]  
> 編集モードからオートパイロットを起動したとき、オートパイロットが停止すると編集モードに戻ります。
> 再生モードの状態でオートパイロットを起動すると、オートパイロットを停止しても再生モードは継続されます。


### 2. コマンドラインからの起動

コマンドラインから起動する場合、以下の引数を指定します。

```bash
$(UNITY) \
  -projectPath $(PROJECT_HOME) \
  -batchmode \
  -executeMethod DeNA.Anjin.Editor.Commandline.Bootstrap \
  -AUTOPILOT_SETTINGS Assets/Path/To/AutopilotSettings.asset
```

なお、

- `UNITY`にはUnityエディタへのパス、`PROJECT_HOME`には自動実行対象プロジェクトのルートを指定します
- `-AUTOPILOT_SETTINGS` に続けて、実行したい設定ファイル（AutopilotSettings）のパスを指定します
- `-quit` は指定しないでください（Play modeに入らず終了してしまいます）
- `-nographics` は指定しないでください（GameViewウィンドウを表示できません）

また、以下の引数を追加することで一部の設定を上書きできます。
各引数の詳細は前述の「オートパイロット設定ファイル」の同名項目を参照してください。
いずれも、キーの先頭に`-`を付けて`-LIFESPAN_SEC 60`のように指定してください。

<dl>
  <dt>LIFESPAN_SEC</dt><dd>実行時間上限を秒で指定します</dd>
  <dt>RANDOM_SEED</dt><dd>疑似乱数発生器に与えるシード値を固定したいときに指定します</dd>
  <dt>TIME_SCALE</dt><dd>Time.timeScaleを指定します。デフォルトは1.0</dd>
  <dt>OUTPUT_ROOT_DIRECTORY_PATH</dt><dd>Agent、Logger、および Reporter が出力するファイルのルートディレクトリパスを指定します</dd>
  <dt>SCREENSHOTS_DIRECTORY_PATH</dt><dd>Agent が撮影するスクリーンショットの出力ディレクトリパスを指定します</dd>
</dl>

次の引数は、コマンドラインからUnityエディターおよびAnjinを起動する場合にのみ有効な引数です。

<dl>
  <dt>GAME_VIEW_WIDTH</dt><dd>GameView の幅を設定します。この引数は、コマンド ラインからエディターを起動する場合にのみ使用されます。デフォルト値は 640 です</dd>
  <dt>GAME_VIEW_HEIGHT</dt><dd>GameView の高さを設定します。この引数は、コマンド ラインからエディターを起動する場合にのみ使用されます。デフォルト値は 480 です</dd>
</dl>

> [!IMPORTANT]  
> バッチモードでも GameView ウィンドウが表示されます。
> Xvfbでも動作することを確認していますが、Unityの正式なサポートではないため、将来利用できなくなる恐れはあります。


### 3. Play Modeテスト内で実行

静的メソッド `Launcher.LaunchAutopilotAsync(string)` を使用することで、テストコード内でオートパイロットが動作します。
引数には `AutopilotSettings` ファイルパスを指定します。

```
[Test]
public async Task LaunchAutopilotInTest()
{
  // 最初のSceneをロード（"Scenes in Build" に含まれている必要があります）
  await SceneManager.LoadSceneAsync("Title");

  // オートパイロットを起動
  await Launcher.LaunchAutopilotAsync("Assets/Path/To/AutopilotSettings.asset");
}
```

> [!WARNING]  
> テストのデフォルトタイムアウトは3分です。オートパイロットの実行時間が3分を超える場合は `Timeout` 属性でタイムアウト時間を指定してください。

> [!WARNING]  
> テストをプレイヤーで実行するときは、必要な設定ファイルを `Resources` フォルダに置き、ビルドに含まれるようにしてください。テストのプレイヤービルドに処理を挟むには `IPrebuildSetup` および `IPostBuildCleanup` が利用できます。
> ファイルパスは `Resources` フォルダからの相対パスを拡張子（".asset"）なしで指定します。

> [!NOTE]  
> テストランナーに `LogException` または `LogError` 出力を検知されると、そのテストは失敗と判定されます。エラーハンドリングをAnjinで行なう前提で `LogAssert.ignoreFailingMessages` で抑止してもいいでしょう。

> [!NOTE]  
> 複数のシナリオを連続実行する場合、`AutopilotSettings.outputRootPath` に個別のパスを指定することで出力ファイルを保全できます。



## ビルトインAgent

以下のAgentが用意されています。これらをそのまま使用することも、ゲームタイトル固有のカスタムAgentを実装して使用することも可能です。


### UGUIMonkeyAgent

uGUIのコンポーネントをランダムに操作するAgentです。
実装にはオープンソースの [Monkey Test Helper](https://github.com/nowsprinting/test-helper.monkey) パッケージを使用しています。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>実行時間</dt><dd>ランダム操作の実行時間を秒で指定します。0を指定するとほぼ無制限（TimeSpan.MaxValue）に動作します。この設定でAgentが終了してもオートパイロットおよびアプリ自体は終了しません。次にSceneが切り替わるまでなにもしない状態になります</dd>
  <dt>操作間隔</dt><dd>ランダム操作間のウェイト間隔をミリ秒で指定します</dd>
  <dt>要素なしタイムアウト</dt><dd>指定した秒数の間、対話可能なUI/2D/3D要素が出現しなければ、オートパイロットの実行を中断します</dd>
  <dt>Gizmos を有効</dt><dd>もし有効ならモンキー操作中の GameView に Gizmo を表示します。もし無効なら GameView に Gizmo を表示しません</dd>
</dl>

***スクリーンショット設定:***

<dl>
  <dt>有効</dt><dd>スクリーンショット撮影を有効にします</dd>
  <dt>ファイル名</dt><dd><b>デフォルト値を使用: </b>スクリーンショットのファイル名のプレフィックスにデフォルト値を使用します。デフォルト値はAgentの名前です<br><b>プレフィックス: </b>スクリーンショットのファイル名のプレフィックスを指定します</dd>
  <dt>拡大係数</dt><dd>解像度をあげるための係数。ステレオキャプチャモードと同時には設定できません</dd>
  <dt>ステレオキャプチャモード</dt><dd>ステレオレンダリングが有効な場合にどちらのカメラを使用するかを指定できます。拡大係数と同時には設定できません</dd>
</dl>

***クリック＆ホールド オペレーター設定:***

<dl>
  <dt>クリック＆ホールド時間</dt><dd>クリック＆ホールドの継続時間をミリ秒で指定します</dd>
</dl>

***テキスト入力オペレーター設定:***

<dl>
  <dt>GameObjectの名前</dt><dd>対象のGameObject名を指定します。名前による特定が難しい場合は後述のInputFieldAnnotationも利用できます</dd>
  <dt>文字種</dt><dd>InputFieldに入力される文字種を指定します。選択肢は「印刷可能文字」「英数字」「数字」です</dd>
  <dt>最小文字列長</dt><dd>InputFieldに入力される文字数の最小値を指定します</dd>
  <dt>最大文字列長</dt><dd>InputFieldに入力される文字数の最大値を指定します</dd>
</dl>

> [!TIP]  
> `UGUIMonkeyAgent` によって操作されたくない `GameObject` がある場合、`TestHelper.Monkey.Annotations` アセンブリに含まれる `IgnoreAnnotation` コンポーネントをアタッチしておくことで操作を回避できます。
> 詳しくは後述の**Anjin Annotations**を参照してください。


### UGUIPlaybackAgent

[Automated QA](https://docs.unity3d.com/Packages/com.unity.automated-testing@latest)パッケージのRecorded Playback機能でレコーディングしたuGUI操作を再生するAgentです。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>操作を記録したJSONファイル</dt><dd>Automated QAパッケージのRecorded Playbackウィンドウで記録したJSONファイルを設定します</dd>
</dl>

Automated QAによる操作のレコーディングは、Unityエディターのメニューから
**Automated QA > Automated QA Hub > Recorded Playback**
で開くウィンドウから行ないます。
レコーディングファイル（.json）は Assets/Recordings/ フォルダ下に保存されますが、移動・リネームは自由です。

なお、Automated QAのRecorded Playback機能ではScene遷移をまたがって操作を記録ができますが、AnjinではSceneが切り替わったところでAgentも強制的に切り替わるため、再生も中断されてしまいます。
従って、レコーディングはScene単位に区切って行なうようご注意ください。

> [!IMPORTANT]  
> Automated QAパッケージは、再生に失敗（対象のボタンが見つからないなど）したときコンソールに `LogType.Error` を出力します。これを検知してオートパイロットを停止するには、次の設定が必要です。
> 1. [ErrorHandlerAgent](#ErrorHandlerAgent) を追加し、`Handle Error` を `Terminate Autopilot` に設定します
> 2. [ConsoleLogger](#ConsoleLogger) を追加し、`Filter LogType` に `Error` 以上を設定します

> [!IMPORTANT]  
> Automated QAパッケージはプレビュー段階のため、破壊的変更や、パッケージ自体の開発中止・廃止もありえる点、ご注意ください。


### DoNothingAgent

なにもしないAgentです。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>実行時間</dt><dd>なにもしない時間を秒で指定します。0を指定すると無制限になにもしません。この設定でAgentが終了してもオートパイロットおよびアプリ自体は終了しません。次にSceneが切り替わるまでなにもしない状態になります</dd>
</dl>


### TerminateAgent

オートパイロットの実行を停止します。テストシナリオの目標を達成した後などに実行されることを想定しています。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>終了コード</dt><dd>このAgentがオートパイロットを停止するときに使用される終了コードを選択します</dd>
  <dt>カスタム終了コード</dt><dd>終了コードを整数値で入力します</dd>
  <dt>メッセージ</dt><dd>オートパイロットを停止するとき、Reporterから送信されるメッセージ</dd>
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

1つの子Agentを登録し、それをオートパイロット実行期間を通じて1回だけ実行できるAgentです。
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

異常系のログメッセージを捕捉してオートパイロットの実行を停止するAgentです。

常に、他の（実際にゲーム操作を行なう）Agentと同時に起動しておく必要があります。
一般的には、`AutopilotSettings` の `Scene Crossing Agents` に追加します。
Sceneごとに設定を変えたい場合は、`ParallelCompositeAgent` と合わせて使用します。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>例外を検知</dt><dd>例外ログを検知したとき、オートパイロットを停止するか、レポート送信のみ行なうかを指定します。
        コマンドライン引数 <code>-HANDLE_EXCEPTION</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>エラーを検知</dt><dd>エラーログを検知したとき、オートパイロットを停止するか、レポート送信のみ行なうかを指定します。
        コマンドライン引数 <code>-HANDLE_ERROR</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>アサートを検知</dt><dd>アサートログを検知したとき、オートパイロットを停止するか、レポート送信のみ行なうかを指定します。
        コマンドライン引数 <code>-HANDLE_ASSERT</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>警告を検知</dt><dd>警告ログを検知したとき、オートパイロットを停止するか、レポート送信のみ行なうかを指定します。
        コマンドライン引数 <code>-HANDLE_WARNING</code> で上書きできますが、複数のErrorHandlerAgentを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>無視するメッセージ</dt><dd>指定された文字列を含むログメッセージは停止条件から無視されます。正規表現も使用できます。エスケープは単一のバックスラッシュ (`\`) です。</dd>
</dl>

> [!TIP]  
> 実行中に例外が発生した時点でオートパイロットを中断するために「例外を検知」を停止に設定することを推奨します。


### UGUIEmergencyExitAgent

Sceneに含まれる `EmergencyExitAnnotations` コンポーネントの出現を監視し、表示されたら即クリックするAgentです。
たとえば通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなる振る舞いが含まれるゲームで利用できます。

常に、他の（実際にゲーム操作を行なう）Agentと同時に起動しておく必要があります。
一般的には、`AutopilotSettings` の `Scene Crossing Agents` に追加します。
Sceneごとに設定を変えたい場合は、`ParallelCompositeAgent` と合わせて使用します。

このAgentのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>間隔</dt><dd>EmergencyExitAnnotationコンポーネントの出現を確認する間隔をミリ秒で指定します</dd>
  <dt>スクリーンショット</dt><dd>EmergencyExitボタンクリック時にスクリーンショットを撮影します</dd>
</dl>



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
  <dt>出力ファイルパス</dt><dd>ログファイルの出力先パスを指定します。相対パスが指定されたとき、<code>AutopilotSettings.outputRootPath</code>が起点となります。
        コマンドライン引数 <code>-FILE_LOGGER_OUTPUT_PATH</code> で上書きできますが、複数のFileLoggerを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>フィルタリングLogType</dt><dd>選択したLogType以上のログ出力のみを有効にします</dd>
  <dt>タイムスタンプを追加</dt><dd>ログエンティティにタイムスタンプを出力します</dd>
</dl>



## ビルトインReporter

以下のReporterタイプが用意されています。これらをそのまま使用することも、ゲームタイトル固有のカスタムReporterを実装して使用することも可能です。


### JUnitXmlReporter

JUnit XMLフォーマットのレポートファイルを出力するReporterです。
オートパイロット実行の成否は、Unityエディターの終了コードでなくこのファイルを見て判断するのが確実です。errors, failuresともに0件であれば正常終了と判断できます。

このReporterのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>出力ファイルパス</dt><dd>JUnit XML形式ファイルの出力先パスを指定します。相対パスが指定されたとき、<code>AutopilotSettings.outputRootPath</code>が起点となります。
        コマンドライン引数 <code>-JUNIT_REPORT_PATH</code> で上書きできますが、複数のJUnitXmlReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
</dl>


### SlackReporter

Slackにレポート送信するReporterです。

このReporterのインスタンス（.assetファイル）には以下を設定できます。

<dl>
  <dt>Slackトークン</dt><dd>Slack通知に使用するBotのOAuthトークン。省略時は送信されません。
        コマンドライン引数 <code>-SLACK_TOKEN</code> で上書きできますが、複数のSlackReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>Slackチャンネル</dt><dd>Slack通知を送るチャンネルの名前（例: "#test"）またはID（例: "C123456"）をカンマ区切りで指定します。省略時は送信されません。
        チャンネルにはBotを招待しておく必要があります。
        コマンドライン引数 <code>-SLACK_CHANNELS</code> で上書きできますが、複数のSlackReporterを定義しているとき、すべて同じ値で上書きされますので注意してください。</dd>
  <dt>メンション宛先ユーザーグループID</dt><dd>エラー終了時に送信する通知をメンションするユーザーグループのIDをカンマ区切りで指定します</dd>
  <dt>@hereをつける</dt><dd>エラー終了時に送信する通知に@hereを付けます</dd>
  <dt>リード文</dt><dd>エラー終了時に送信する通知のリード文。OSの通知に使用されます。"{message}" のようなプレースホルダーを指定できます</dd>
  <dt>メッセージ</dt><dd>エラー終了時に送信するメッセージ本文のテンプレート。"{message}" のようなプレースホルダーを指定できます</dd>
  <dt>色</dt><dd>エラー終了時に送信するメッセージのアタッチメントに指定する色（デフォルト：Slackの "danger" と同じ赤色）</dd>
  <dt>スクリーンショット</dt><dd>エラー終了時にスクリーンショットを撮影します（デフォルト: on）</dd>
  <dt>正常終了時にも送信</dt><dd>正常終了時にもレポートを送信します（デフォルト: off）</dd>
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
- "{mppm-tags}": [Multiplayer Play Mode](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@latest) パッケージのタグ



## ゲームタイトル固有の実装

ゲームタイトル固有のカスタムAgentや初期化処理を実装する場合、リリースビルドへの混入を避けるため、専用のアセンブリに分けることをおすすめします。
Assembly Definition File (asmdef) のAuto Referencedをoff、Define Constraintsに `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` を設定することで、リリースビルドからは除外できます。

このasmdef及び格納フォルダは、Projectウィンドウの任意の場所でコンテキストメニューを開き
**Create > Anjin > Game Title Specific Assembly Folder**
を選択することで生成できます。


### ゲームタイトル固有の初期化処理

ゲームタイトル固有の初期化処理が必要な場合、初期化を行なう静的メソッドに `InitializeOnLaunchAutopilot` 属性を付与してください。
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

このコンポーネントがアタッチされた`Button`が表示されると、`UGUIEmergencyExitAgent`はすぐにクリックを試みます。
通信エラーや日またぎで「タイトル画面に戻る」ボタンのような、テストシナリオ遂行上イレギュラーとなるボタンに付けることを想定しています。



## Multiplayer Play Mode パッケージと使用する

Unity 6以降で利用できる [Multiplayer Play Mode](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@latest)（以下MPPM）パッケージを使用することで、マルチプレイ対応ゲームの動作確認を簡単に行えるようになりました。

AnjinをMPPMパッケージと使用するときの注意点を以下に示します。
MPPMパッケージ v1.3.1で確認しています。


### Virtual Playersで実行する

[Virtual Players](https://docs-multiplayer.unity3d.com/mppm/current/virtual-players/) で実行する場合、編集モードからAutopilotSettingsファイルの **実行** ボタンをクリックすると、すべてのVirtual Playerでオートパイロットが起動します。

再生モードからの起動および、プレイヤーによって異なるAutopilotSettingsを使用することはできません。
プレイヤーごとに異なる振る舞いを指示するには、[タグ](https://docs-multiplayer.unity3d.com/mppm/current/player-tags/) によって分岐するカスタムAgentを実装する必要があります。

AutopilotSettingsの「出力ルートパス」に相対パスを指定したとき、起点は各Virtual Playerのディレクトリになります。
従って、FileLoggerやスクリーンショットはVirtual Playerごとに保全されます。

> [!CAUTION]  
> `UGUIPlaybackAgent` が依存しているAutomated QAパッケージは、出力ファイルを `Application.persistentDataPath` に書き出します。
> すべてのVirtual Playerが同じファイルに書き込もうとするため、コンソールにエラーが出力されます。

> [!WARNING]  
> Virtual PlayersにはAssets下のファイルがシンボリックリンクで共有されます。
> そのため、インスペクタで設定を変更したときは、再実行する前にメニューの **File > Save (command + S)** で保存する必要があります。


### Play Mode Scenariosで実行する

[Play Mode Scenarios](https://docs-multiplayer.unity3d.com/mppm/current/play-mode-scenario/play-mode-scenario-about/) で実行する場合、Anjinを組み込んだプレイヤービルドが必要になります。
ビルド方法については [プレイヤービルドでの実行](#プレイヤービルドでの実行実験的機能) を参照してください。

Play Mode Scenariosではプレイヤーごとにコマンドライン引数を指定して起動することができるため、異なるAutopilotSettingsファイルを使用できます。

> [!CAUTION]  
> `UGUIPlaybackAgent` が依存しているAutomated QAパッケージは、出力ファイルを `Application.persistentDataPath` に書き出します。
> すべてのVirtual Playerが同じファイルに書き込もうとするため、コンソールにエラーが出力されます。



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

### オートパイロットは停止しているのに AutopilotSettings の Run ボタンが表示されない

Anjinの実行状態を永続化している `AutopilotState.asset` が不正な状態になっている恐れがあります。
インスペクタで開いて **リセット** ボタンをクリックしてください。

それでも解決しない場合、 `AutopilotState.asset` を削除してみてください。


### プロジェクトを再生モードにすると勝手にオートパイロットが動いてしまう

Anjinの実行状態を永続化している `AutopilotState.asset` が不正な状態になっている恐れがあります。
インスペクタで開いて **リセット** ボタンをクリックしてください。

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


### 起動時に "settings has been obsolete" 警告が出る

たとえば次のような警告メッセージが出力されることがあります。

```
Slack settings in AutopilotSettings has been obsolete.
Now, automatically converted it to SlackReporter asset file. Check it out and commit it to your VCS.
```

これは、廃止された設定項目を新しい設定方法（上例では `SlackReporter` ）に自動変換したことを示しています。
設定ファイルが更新されていますので、確認してVCS（Gitなど）にコミットしてください。



## ライセンス

MIT License


## コントリビュート

IssueやPull requestを歓迎します。
Pull requestには `enhancement`, `bug`, `chore`, `documentation` といったラベルを付けてもらえるとありがたいです。
ブランチ名から自動的にラベルを付ける設定もあります。[PR Labeler settings](.github/pr-labeler.yml) を参照してください。


おおまかな機能追加の受け入れ方針は次のとおりです。

- すべてのビルトイン機能は、Unityエディタのインスペクタウィンドウで設定を完結して使用できること
- Autopilot本体への機能追加は極力避け、Agentなどによる拡張を検討する
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
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.4.1 or later
> - [Test Helper](https://github.com/nowsprinting/test-helper) package v1.1.1 or later

テスト専用のUnityプロジェクトを生成し、Unityバージョンを指定してテストを実行するには、次のコマンドを実行します。

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```

> [!WARNING]  
> テストを実行するには、**Project Settings > Player > Active Input Handling** を "Input Manager (Old)" または "Both" に設定する必要があります。


## リリースワークフロー

**Actions > Create release pull request > Run workflow** を実行し、作られたpull requestをデフォルトブランチにマージすることでリリース処理が実行されます。
（もしくは、デフォルトブランチのpackage.json内のバージョン番号を書き換えます）

リリース処理は、[Release](.github/workflows/release.yml)ワークフローで自動的に行われます。
tagが付与されると、OpenUPMがtagを収集して更新してくれます。

以下の操作は手動で行わないでください。

- リリースタグの作成
- ドラフトリリースの公開
