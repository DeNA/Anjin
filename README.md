# Anjin

[![Meta file check](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml/badge.svg)](https://github.com/DeNA/Anjin/actions/workflows/metacheck.yml)
[![openupm](https://img.shields.io/npm/v/com.dena.anjin?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.dena.anjin/)

This is an autopilot tool for games made with Unity.
It consists of the following two elements.

1. A dispatcher that launches the corresponding Agent according to the Scene loaded in the game
2. An Agent that realizes auto-execution closed to the Scene

Agents are small, isolated C# scripts that perform specific operations, such as playback of UI operations or monkey tests.
In addition to those provided built-in, game title-specific ones can be implemented and used.

Click [日本語](./README_ja.md) for the Japanese page if you need.



## Installation

You can choose from two typical installation methods.

### Install via Package Manager window

1. Open the **Package Manager** tab in Project Settings window (**Editor > Project Settings**)
2. Click **+** button under the **Scoped Registries** and enter the following settings (figure 1.):
   1. **Name:** `package.openupm.com`
   2. **URL:** `https://package.openupm.com`
   3. **Scope(s):** `com.dena`, `com.cysharp`, and `com.nowsprinting`
3. Open the Package Manager window (**Window > Package Manager**) and select **My Registries** in registries drop-down list (figure 2.)
4. Click **Install** button on the `com.dena.anjin` package

**Figure 1.** Package Manager tab in Project Settings window.

![](Documentation~/ProjectSettings_Dark.png#gh-dark-mode-only)
![](Documentation~/ProjectSettings_Light.png#gh-light-mode-only)

**Figure 2.** Select registries drop-down list in Package Manager window.

![](Documentation~/PackageManager_Dark.png/#gh-dark-mode-only)
![](Documentation~/PackageManager_Light.png/#gh-light-mode-only)

> [!NOTE]  
> Do not forget to add `com.cysharp` and `com.nowsprinting` into scopes. These are used within Anjin.

> [!NOTE]  
> Required install [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3 or later for running tests (when adding to the `testables` in package.json).

### Install via OpenUPM-CLI

If you installed [openupm-cli](https://github.com/openupm/openupm-cli), run the command below:

```bash
openupm add com.dena.anjin
```

`UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` is set in the Define Constraints of the Assembly Definition File, So excluded from the release build in principle.


### Recommended .gitignore

The following files are automatically generated when you start Anjin.
There is no need to track it, so we recommend adding it to your project's .gitignore file.

```
/Assets/AutopilotState.asset*
```



## Settings for game title

After installing the UPM package in the game title project, configure and implement the following.

- Generate and configure the AutopilotSettings.asset file
- Generate and configure the .asset file for the Agent to be used
- Implement a custom Agent if necessary
- Implement initialization process as needed

> [!NOTE]  
> Set `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE` in the Define Constraints of the Assembly Definition File to which they belong to exclude them from release builds.


### Generate and configure the AutopilotSettings.asset file

Right-click in the Unity Editor's Project window to open the context menu, then select
**Create > Anjin > Autopilot Settings**
to create the file.
The file name is arbitrary, and multiple files can be created and used within a project.

There are three main settings.

#### Agent Assignment

##### Scene Agent Mapping and Fallback Agent

Set up a mapping of Agent configuration files (.asset) to be automatically executed for each Scene.

Set the combination of `Scene` and `Agent`.
The order does not affect the operation.
If a Scene does not exist in the list, the Agent set in `Fallback Agent` will be used.

For example, if you want `UGUIMonkeyAgent` to work for all Scenes, set `Scene Agent Maps` to empty, and set
Set the `Fallback Agent` to an `UGUIMonkeyAgent`'s .asset file.

##### Observer Agent

Set the Agents to always be launched in parallel, independent of `Scene Agent Maps` and `Fallback Agent`.
As of v1.0.0, `EmergencyExitAgent` is assumed to be used.
If you need to launch multiple Agents in parallel, use `ParallelCompositeAgent`.

Note that the Agents set here will be destroyed and created each time a new Scene is loaded.
It is **NOT** set to `DontDestroyOnLoad`.

#### Autopilot Run Settings

This item can also be overridden from the commandline (see below).

<dl>
  <dt>Lifespan</dt><dd>Specifies the execution time limit in seconds. Defaults to 300 seconds, 0 specifies unlimited operation</dd>
  <dt>Random Seed</dt><dd>Specify when you want to fix the seed given to the pseudo-random number generator (optional). This is a setting related to the pseudo-random number generator used by the autopilot. To fix the seed of the pseudo-random number generator in the game itself, it is necessary to implement this setting on the game title side. </dd>
  <dt>Time Scale</dt><dd>Time.timeScale. Default is 1.0</dd>
  <dt>JUnit Report Path</dt><dd>Specifies the JUnit format report file output path (optional). If there are zero errors and zero failures, the autopilot run is considered to have completed successfully. </dd>
  <dt>Logger</dt><dd>Logger used for this autopilot settings. If omitted, <code>Debug.unityLogger</code> will be used as default.</dd>
  <dt>Reporter</dt><dd>Reporter that called when some errors occurred in target application</dd>
</dl>

#### Error Handling Settings

Set up a filter to catch abnormal log messages and notify using Reporter.

<dl>
  <dt>Handle Exception</dt><dd>Report when exception is detected in log</dd>
  <dt>Handle Error</dt><dd>Report when error message is detected in log</dd>
  <dt>Handle Assert</dt><dd>Report when assert message is detected in log</dd>
  <dt>Handle Warning</dt><dd>Report when warning message is detected in log</dd>
  <dt>Ignore Messages</dt><dd>Log messages containing this string will not be report</dd>
</dl>


### Generate and configure the Agent setting file (.asset)

Whether you use the built-in Agent or implement custom Agent, you must create an instance (.asset file) of it in the Unity editor.

Instances are created by right-clicking in the Project window of the Unity editor to open the context menu, then selecting
**Create > Anjin > Agent type name**.

Select the generated file, Agent-specific settings are displayed in the inspector and can be customized.
You can prepare multiple Agents with different settings for the same Agent type and use them in different Scenes.


### Generate and configure the Logger setting file (.asset)

Logger instances are created by right-clicking in the Project window of the Unity editor to open the context menu, then selecting
**Create > Anjin > Logger type name**.

Select the generated file, Logger-specific settings are displayed in the inspector and can be customized.
You can prepare multiple Loggers with different settings for the same Logger type.


### Generate and configure the Reporter setting file (.asset)

Reporter instances are created by right-clicking in the Project window of the Unity editor to open the context menu, then selecting
**Create > Anjin > Reporter type name**.

Select the generated file, Reporter-specific settings are displayed in the inspector and can be customized.
You can prepare multiple Reporters with different settings for the same Reporter type.



## Run autopilot

Autopilot can be executed in two ways.


### 1. Run on play mode in Unity editor (GUI)

Open the AutopilotSettings file you wish to run in the inspector and click the **Run** button to enter play mode with Autopilot enabled.
After the set run time has elapsed, or as in normal play mode, clicking the Play button will stop the program.


### 2. Run from Play Mode test

Autopilot works within your test code using the async method `LauncherFromTest.AutopilotAsync(string)`.
Specify the `AutopilotSettings` file path as the argument.

```
[Test]
public async Task LaunchAutopilotFromTest()
{
  await LauncherFromTest.AutopilotAsync("Assets/Path/To/AutopilotSettings.asset");
}
```

> [!NOTE]  
> If an error is detected in running, it will be output to `LogError` and the test will fail.

> [!WARNING]  
> The default timeout for tests is 3 minutes. If the autopilot execution time exceeds 3 minutes, please specify the timeout time with the `Timeout` attribute.


### 3. Run from commandline

To execute from the commandline, specify the following arguments.

```bash
$(UNITY) \
  -projectPath $(PROJECT_HOME) \
  -batchmode \
  -executeMethod DeNA.Anjin.Editor.Commandline.Bootstrap \
  -AUTOPILOT_SETTINGS Assets/Path/To/AutopilotSettings.asset
```

- `UNITY` is the path to the Unity editor, and `PROJECT_HOME` is the root of the project to be autorun.
- `-AUTOPILOT_SETTINGS` is the path to the settings file (AutopilotSettings) you want to run.
- Do not specify `-quit` (it will exit without entering play mode).
- Do not specify `-nographics` (do not show GameView window).

In addition, some settings can be overridden by adding the following arguments.
For details on each argument, see the entry of the same name in the "Generate and configure the AutopilotSettings.asset file" mentioned above.

<dl>
  <dt>LIFESPAN_SEC</dt><dd>Specifies the execution time limit in seconds</dd>
  <dt>RANDOM_SEED</dt><dd>Specifies when you want to fix the seed given to the pseudo-random number generator</dd>
  <dt>TIME_SCALE</dt><dd>Specifies the Time.timeScale. Default is 1.0</dd>
  <dt>JUNIT_REPORT_PATH</dt><dd>Specifies the JUnit-style report file output path</dd>
  <dt>HANDLE_EXCEPTION</dt><dd>Overwrites whether to report when an exception occurs with TRUE/FALSE</dd>
  <dt>HANDLE_ERROR</dt><dd>Overwrites whether to report when an error message is detected with TRUE/FALSE</dd>
  <dt>HANDLE_ASSERT</dt><dd>Overwrites whether to report when an assert message is detected with TRUE/FALSE</dd>
  <dt>HANDLE_WARNING</dt><dd>Overwrites whether to report when an warning message is detected with TRUE/FALSE</dd>
</dl>

In both cases, the key should be prefixed with `-` and specified as `-LIFESPAN_SEC 60`.



## Built-in Agents

The following Agent types are provided. These can be used as they are, or project-specific Agents can be implemented and used.


### UGUIMonkeyAgent

This is an Agent that randomly manipulates uGUI components.
This agent implementation uses open source [test-helper.monkey](https://github.com/nowsprinting/test-helper.monkey) package.

An instance of this Agent (.asset file) can contain the following.

<dl>
  <dt>Lifespan Sec</dt><dd>Duration of random operation execution time in secounds. If 0 is specified, the operation is almost unlimited (TimeSpan.MaxValue). With this setting, neither Autopilot nor the app itself will exit when the Agent exits. It will not do anything until the next Scene switch</dd>
  <dt>Delay Millis</dt><dd>Wait interval [milliseconds] between random operations</dd>
  <dt>Secs Searching Components</dt><dd>Seconds to determine that an error has occurred when an object that can be interacted with does not exist</dd>
  <dt>Touch and Hold Millis</dt><dd>Delay time for touch-and-hold [ms]</dd>
  <dt>Enable Gizmos</dt><dd>Show Gizmos on GameView during running monkey test if true</dd>
</dl>

#### *Screenshot Options:*

<dl>
  <dt>Enabled</dt><dd>Whether screenshot is enabled or not</dd>
  <dt>Directory</dt><dd><b>Use Default: </b>Whether using a default directory path to save screenshots or specifying it manually. Default value is specified by command line argument "-testHelperScreenshotDirectory". If the command line argument is also omitted, Application.persistentDataPath + "/TestHelper/Screenshots/" is used.<br><b>Path: </b>Directory path to save screenshots</dd>
  <dt>Filename</dt><dd><b>Use Default: </b>Whether using a default prefix of screenshots filename or specifying it manually. Default value is agent name<br><b>Prefix: </b>Prefix of screenshots filename</dd>
  <dt>Super Size</dt><dd>The factor to increase resolution with. Neither this nor Stereo Capture Mode can be specified</dd>
  <dt>Stereo Capture Mode</dt><dd>The eye texture to capture when stereo rendering is enabled. Neither this nor Resolution Factor can be specified</dd>
</dl>

If you have a `GameObject` that you want to avoid manipulation by the `UGUIMonkeyAgent`,
attach the `IgnoreAnnotation` component in the `TestHelper.Monkey.Annotations` assembly.
See **Anjin Annotations** below for more information.


### UGUIPlaybackAgent

This is an Agent that playback uGUI operations with the Recorded Playback feature of the [Automated QA](https://docs.unity3d.com/Packages/com.unity.automated-testing@latest) package.

> [!NOTE]  
> The Automated QA package is in the preview stage. Please note that destructive changes may occur, and the package itself may be discontinued or withdrawn.

The following can be set in an instance (.asset file) of this Agent.

<dl>
  <dt>Recorded Json</dt><dd>Specify the recording file (.json) to play</dd>
</dl>

Use the Recorded Playback window for recording operations with Automated QA. The window is opened via the Unity editor menu
**Automated QA > Automated QA Hub > Recorded Playback**.
The recording file (.json) is saved under the Assets/Recordings/ folder and can be freely moved or renamed.

Note that the Recorded Playback function in Automated QA can record operations across Scene transitions, but in Anjin, when the Scene is switched, the Agent is also forcibly switched, so playback is also interrupted.
Therefore, please be careful to record in units of Scenes.

A screenshot of the operation by Automated QA is stored under `Application.persistentDataPath/Anjin`.
The `Application.persistentDataPath` for each platform can be found in the Unity manual at
[Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html).


### DoNothingAgent

An Agent that does nothing.

The following settings can be configured for this Agent instance (.asset file).

<dl>
  <dt>Lifespan Sec</dt><dd>Specifies the do nothing time in seconds. 0 means unlimited time to do nothing. If 0 is specified, an unlimited amount of time for no action is taken. It will not do anything until the next Scene is switched.</dd>
</dl>


### ParallelCompositeAgent

An Agent that executes multiple Agents in parallel.

The instance of this Agent (.asset file) can have the following settings.

<dl>
  <dt>Agents</dt><dd>A list of Agents to be executed in parallel, which can be nested by specifying a CompositeAgent</dd>
</dl>


### SerialCompositeAgent

An Agent that executes multiple Agents in series.

The instance of this Agent (.asset file) can have the following settings.

<dl>
  <dt>Agents</dt><dd>A list of Agents to be executed in series, which can be nested by specifying a CompositeAgent</dd>
</dl>


### OneTimeAgent

An Agent that can register one child Agent and execute it only once throughout the Autopilot execution period.
The second time executions will be skipped.

For example, in a game where the title scene leads to a different path only for the first time or where the user receives a login bonus only for the first time on the home scene, it is possible to construct a scenario in which the title screen is executed as is even if a communication error or the like causes a return to the title scene.

The Agent instance (.asset file) can contain the following settings.

<dl>
  <dt>Agent</dt><dd>Agent that can be executed only once, which can be nested by specifying a CompositeAgent</dd>
</dl>


### RepeatAgent

An Agent that executes one child Agent indefinitely and repeatedly.

Combined with SerialCompositeAgent, it can be used to repeat a scenario many times or to repeat only the last Agent in a scenario.
Note that finite iterations are not supported (to use SerialCompositeAgent).

This Agent instance (.asset file) can contain the following.

<dl>
  <dt>Agent</dt><dd>Agent to be executed repeatedly, which can be nested by specifying a CompositeAgent</dd>
</dl>


### TimeBombAgent

An Agent that will fail if a defuse message is not received before the inner agent exits.

For example, pass the out-game tutorial (Things that can be completed with tap operations on uGUI, such as smartphone games).

1. Set `UGUIMonkeyAgent` as the `Agent`. It should not be able to operate except for the advancing button. Set the `Lifespan Sec` with a little margin.
2. Set the log message to be output when the tutorial is completed as the `Defuse Message`.

This Agent instance (.asset file) can contain the following.

<dl>
  <dt>Agent</dt><dd>Working Agent. If this Agent exits first, the TimeBombAgent will fail.</dd>
  <dt>Defuse Message</dt><dd>Defuse the time bomb when this message comes to the log first. Can specify regex.</dd>
</dl>


### EmergencyExitAgent

An Agent that monitors the appearance of the `EmergencyExit` component in the `DeNA.Anjin.Annotations` assembly and clicks on it as soon as it appears.
This can be used in games that contain behavior that is irregular in the execution of the test scenario, for example, communication errors or "return to title screen" buttons that are triggered by a daybreak.

It should always be started at the same time as other Agents (that actually perform game operations).
This can be accomplished with `ParallelCompositeAgent`, but it is easier to set it up as an `ObserverAgent` in AutopilotSettings.



## Built-in Logger

The following Logger types are provided. These can be used as they are, or project-specific Loggers can be implemented and used.


### Composite Logger

A Logger that delegates to multiple loggers.

The instance of this Logger (.asset file) can have the following settings.

<dl>
  <dt>Loggers</dt><dd>A list of Logger to delegates</dd>
</dl>


### Console Logger

A Logger that outputs to a console.

The instance of this Logger (.asset file) can have the following settings.

<dl>
  <dt>Filter LogType</dt><dd>To selective enable debug log message</dd>
</dl>


### File Logger

A Logger that outputs to a specified file.

The instance of this Logger (.asset file) can have the following settings.

<dl>
  <dt>Output File Path</dt><dd>Log output file path. Specify relative path from project root or absolute path. When run on player, it will be the <code>Application.persistentDataPath</code>.
        This setting can be overwritten with the command line argument <code>-FILE_LOGGER_OUTPUT_PATH</code>, but if multiple File Loggers are defined, they will all be overwritten with the same path.</dd>
  <dt>Filter LogType</dt><dd>To selective enable debug log message</dd>
  <dt>Timestamp</dt><dd>Output timestamp to log entities</dd>
</dl>



## Built-in Reporter

The following Reporter types are provided. These can be used as they are, or project-specific Reporters can be implemented and used.


### Composite Reporter

A Reporter that delegates to multiple Reporters.

The instance of this Reporter (.asset file) can have the following settings.

<dl>
  <dt>Reporters</dt><dd>A list of Reporter to delegates</dd>
</dl>


### Slack Reporter

A Reporter that post report to Slack.

The instance of this Reporter (.asset file) can have the following settings.

<dl>
  <dt>Slack Token</dt><dd>Web API token used for Slack notifications. If omitted, no notifications will be sent.
        This setting can be overwritten with the command line argument <code>-SLACK_TOKEN</code>.</dd>
  <dt>Slack Channels</dt><dd>Channels to send Slack notifications. If omitted, not notified. Multiple channels can be specified by separating them with commas.
        This setting can be overwritten with the command line argument <code>-SLACK_CHANNELS</code>.</dd>
  <dt>Mention Sub Team IDs</dt><dd>Comma Separated Team IDs to Mention in Slack Notification Message</dd>
  <dt>Add Here In Slack Message</dt><dd>Add @here to Slack notification message. Default is off</dd>
</dl>



## Implementation of game title-specific code

Game title specific Agents and initialization code must be avoided in the release build.
Create a unique assembly and turn off "Auto Referenced", and set the "Define Constraints" to `UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE`.

This asmdef and its storage folder can be created by opening the context menu anywhere in the Project window and selecting
**Create > Anjin > Title Own Assembly Folder**.


### Custom Agent

A custom Agent is created by inheriting from `Anjin.Agents.AbstractAgent`.
Simply implement the process to be executed by the Agent in the method `UniTask Run(CancellationToken)`.

The following fields defined in `AbstractAgent` are available. Each instance is set before calling the `Run` method.

- `Logger`: implementation of `UnityEngine.ILogger`. Use this for log output. Currently, it is a console output, but there are plans to replace it with a file logger.
- `Random`: Implementation of `Anjin.Utilities.IRandom`. Which is created from a seed specified in a configuration file or a startup argument.

Note that it is convenient to set the `[CreateAssetMenu]` attribute to create an instance from the context menu.


### Game title-specific pre-processing

If your title requires its own initialization process, add the `InitializeOnLaunchAutopilot` attribute to the `public static` method that does the initialization.
An added method is called from the autopilot launch process.

```csharp
[InitializeOnLaunchAutopilot]
public static void InitializeOnLaunchAutopilotMethod()
{
    // initialize code for your game.
}
```

Async methods are also supported.

```csharp
[InitializeOnLaunchAutopilot]
public static async UniTask InitializeOnLaunchAutopilotMethod()
{
    // initialize code for your game.
}
```

Note that the autopilot launch process is performed with `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)` (default for `RuntimeInitializeOnLoadMethod`).
Also, `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)` implements the initialization process for Configurable Enter Play Mode.



## Anjin Annotations

Annotations are defined to control Anjin operations.
Use the `DeNA.Anjin.Annotations` assembly by adding it to the Assembly Definition References.
Please note that this will be included in the release build due to the way it works.

> [!NOTE]  
> Even if the annotations assembly is removed from the release build, the link to the annotation component will remain Scenes and Prefabs in the asset bundle built.
> Therefore, a warning log will be output during instantiate.
> To avoid this, annotations assembly are included in release builds.


### IgnoreAnnotation

The `GameObject` to which this component is attached avoids manipulation by the `UGUIMonkeyAgent`.


### EmergencyExit

When a `Button` to which this component is attached appears, the `EmergencyExitAgent` will immediately attempt to click on it.
It is intended to be attached to buttons that are irregular in the execution of the test scenario, such as the "return to title screen" button due to a communication error or a daybreak.



## Troubleshooting

### Autopilot runs on its own in play mode.

The `AutopilotState.asset` that persists in Anjin's execution state may be in an incorrect state.
Open it in the inspector and click the **Reset** button.

If this does not solve the problem, try deleting `AutopilotState.asset`.


### [CompilerError] Argument 1: Cannot convert to 'System.Threading.CancellationToken'

The following compilation error has been reported in some cases:

```
[CompilerError] Argument 1:.
Cannot convert from 'DeNA.Anjin.Reporters.SlackReporter.CoroutineRunner' to 'System.Threading.CancellationToken'
Compiler Error at Library\PackageCache\com.dena.anjin@1.0.1\Runtime\Reporters\SlackReporter.cs:66 column 53
```

This compile error occurs when using UniTask v2.3.0 or older.
The `UniTask.WaitForEndOfFrame(MonoBehaviour)` API was implemented in UniTask v2.3.1

Usually, the correct version of UniTask is installed if you follow the installation instructions.
However, if UniTask is placed as an embedded package in the project, it will be prioritized.
Find out why you have an old UniTask installed.
e.g., Check the contents of Packages/packages-lock.json generated by the Unity editor; use your IDE's code definition jump function to check where the source file of `UniTask.WaitForEndOfFrame()` is.



## License

MIT License


## How to contribute

Open an issue or create a pull request.

Be grateful if you could label the pull request as `enhancement`, `bug`, `chore`, and `documentation`. See [PR Labeler settings](.github/pr-labeler.yml) for automatically labeling from the branch name.


## How to development

Add this repository as a submodule to the Packages/ directory in your project.

Run the command below:

```bash
git submodule add https://github.com/dena/Anjin.git Packages/com.dena.anjin
```

> [!WARNING]  
> Required install packages for running tests (when adding to the `testables` in package.json), as follows:
> - [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest) package v1.3.4 or later
> - [Test Helper](https://github.com/nowsprinting/test-helper) package v0.4.2 or later

Generate a temporary project and run tests on each Unity version from the command line.

```bash
make create_project
UNITY_VERSION=2019.4.40f1 make -k test
```


## Release workflow

Run **Actions > Create release pull request > Run workflow** and merge created pull request.
(Or bump version in package.json on default branch)

Then, Will do the release process automatically by [Release](.github/workflows/release.yml) workflow.
And after tagged, OpenUPM retrieves the tag and updates it.

Do **NOT** manually operation the following operations:

- Create release tag
- Publish draft releases
