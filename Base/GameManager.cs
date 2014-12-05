using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 	<para>The Game Manager has the longest life-cycle of all the uFrame classes, it will remain throughout the entire lifecycle of a game/application.  It uses
/// the Unity "DontDestroyOnLoad" functionality allowing it to persist from scene to scene.</para>
/// 	<para></para>
/// 	<para>The game managers main responsibility is to maintain the current Scene Manager and process the appropriate setup required by the rest of uFrame.  The
/// current GameManager instance can always be accessed from anywhere via its static GameManager.Instance property.</para>
/// 	<para></para>
/// 	<para>The game manager also has a static ActiveSceneManager property that can be used to access the current Scene's Manager class.</para>
/// </summary>
public class GameManager : MonoBehaviour, ICommandDispatcher
{
    /// <summary>
    /// Do not use async loading on "TransitionLevel"
    /// </summary>
    public bool _DontUseAsyncLoading = false;

    /// <summary>
    /// A level that displays a progress bar and message
    /// </summary>
    public string _LoadingLevel = "Loading";

    public bool _ShowLogs = false;

    /// <summary>
    /// Set this to the game that will load when the game starts
    /// </summary>
    public SceneManager _Start;

    public string _StartupScene;


    private static SceneManager _activeSceneManager;
    private static IGameContainer _container;
    private static LevelLoadViewModel _loadingViewModel;
    private static LevelLoadViewModel _progress;
    private static IViewResolver _viewResolver;
    private static ICommandDispatcher _commandDispatcher;
    private List<SceneManager> _sceneManagers = new List<SceneManager>();
    private SimpleSubject<CommandInfo> _commandsAsObservable;

    public void Reset()
    {
        if (this.gameObject.GetComponent<MainThreadDispatcher>() == null)
            this.gameObject.AddComponent<MainThreadDispatcher>();
    }
    /// <summary>
    /// The current running game
    /// </summary>
    public static SceneManager ActiveSceneManager
    {
        get { return _activeSceneManager ?? (_activeSceneManager = Instance._Start) ?? (_activeSceneManager = UnityEngine.Object.FindObjectOfType<SceneManager>()); }
        private set { _activeSceneManager = value; }
    }

    public static IGameContainer Container
    {
        get
        {
            if (_container == null)
            {
                _container = new GameContainer();
                _container.RegisterInstance(Progress);
                uFrameBootstraper.Configure(Instance, _container);
            }
            return _container;
        }
    }

    public static IViewResolver ViewResolver
    {
        get { return _viewResolver ?? (_viewResolver = Container.Resolve<IViewResolver>()); }
        set { _viewResolver = value; }
    }

    public static ICommandDispatcher CommandDispatcher
    {
        get { return _commandDispatcher ?? (_commandDispatcher = Container.Resolve<ICommandDispatcher>()); }
        set { _commandDispatcher = value; }
    }

    /// <summary>
    /// The current instance of GameManager
    /// </summary>
    public static GameManager Instance { get; set; }

    /// <summary>
    /// Is this a pro license?
    /// </summary>
    public static bool IsPro
    {
        get
        {
            return Application.HasProLicense();
        }
    }

    /// <summary>
    /// The view model that is used for loading a scene.  Bind to this to be notified of progress changes
    /// </summary>
    /// <value>The loading view model.</value>
    public static LevelLoadViewModel LoadingViewModel
    {
        get { return _loadingViewModel ?? (_loadingViewModel = new LevelLoadViewModel()); }
        set { _loadingViewModel = value; }
    }

    public static LevelLoadViewModel Progress
    {
        get
        {
            return _progress ?? (_progress = new LevelLoadViewModel());
        }
    }

    public static ISwitchLevelSettings SwitchLevelSettings { get; set; }

    public Type ContainerType
    {
        get;
        set;
    }

    /// <summary>
    /// A list of all the game in the scene.
    /// Each game registers itself with this manager and is added to this list.
    /// </summary>
    public List<SceneManager> SceneManagers
    {
        get { return _sceneManagers; }
        set { _sceneManagers = value; }
    }

    /// <summary>
    /// 	<para>The load method is used internally in order to load and process any transition that has occured. This is currently invoked by the LevelLoaderView class
    /// which resides in the loading scene.</para>
    /// 	<innovasys:widget type="Tip Box" layout="block" xmlns:innovasys="http://www.innovasys.com/widgets">
    /// 		<innovasys:widgetproperty layout="block" name="Content">If there is a custom loading screen without this view, it should invoked this in it's bind
    ///     method.</innovasys:widgetproperty>
    /// 	</innovasys:widget>
    /// 	<para>1st - It will grab any instance of LoadingScreenObject and cache it so they can be destroyed after loading. So anything in a loading screen that should be
    /// destroyed should use this component on their game object.</para>
    /// 	<para>2nd - It will process and load each scene passed to any of the transition methods.</para>
    /// 	<para>3rd - Yield until the scene manager is loaded, then properly executes its "Load" method on it for any scene specific loading to occur while the loading
    /// screen is still visible.</para>
    /// 	<para>Final Step - Destroy all the loading screen objects</para>
    /// </summary>
    /// <remarks>The load method is called by the startup and is a coroutine.</remarks>
    /// <seealso cref="M:.GameManager.Start"></seealso>
    /// <seealso cref="M:.GameManager.RegisterSceneManager(SceneManager)"></seealso>
    /// <requirements>This must be called by a loading screen.</requirements>
    public static IEnumerator Load()
    {
        // Wait till all other things have processed in the scene before loading.
        yield return new WaitForEndOfFrame();
        Log("Load Begin");
        // Grab the game manager instance transform so we can ignore it
        var gameTransform = Instance.transform;
        // Grab the root objects in the scene to destroy them later
        var list = new List<LoadingScreenObject>();
        // The loading screen should be small
        foreach (LoadingScreenObject p in FindObjectsOfType(typeof(LoadingScreenObject)))
        {
            if (p == gameTransform)
                continue;
            list.Add(p);
        }
        // For progress tracking
        var progressValue = 0f;
        if (SceneManager.Settings.Levels != null)
        {
            var numberOfLevelsToLoad = (SceneManager.Settings.Levels == null ? 1 : SceneManager.Settings.Levels.Length) + 1;
            var progressFactor = numberOfLevelsToLoad;
            var progressBase = 0f;
            for (int index = 0; index < SceneManager.Settings.Levels.Length; index++)
            {
                var level = SceneManager.Settings.Levels[index];
                if (IsPro && Instance != null && !Instance._DontUseAsyncLoading)
                {
                    Log("Loading Level Additively Async: " + level);
                    AsyncOperation asyncOperation;
                    asyncOperation = Application.LoadLevelAdditiveAsync(level);
                    while (!asyncOperation.isDone)
                    {
                        progressValue = (asyncOperation.progress + progressBase) / progressFactor;
                        ProgressUpdated(String.Format("Loading '{0}'", level), progressValue);
                        yield return new WaitForSeconds(0.1f);
                        Log("Loaded Level Async: " + level);
                    }
                }
                else
                {
                    ProgressUpdated(String.Format("Loading '{0}'", level), numberOfLevelsToLoad);
                    Application.LoadLevelAdditive(level);
                    Log("Loaded Level Additevely: " + level);
                }

                progressBase += 1f;
            }
        }

        Action<string, float> progressUpdateWithFactor =
            delegate(string s, float f)
            {
                progressValue += (1f - progressValue) * f;
                ProgressUpdated(s, progressValue);
            };

        var sceneManager = Instance.SceneManagers.Find(p => p.GetType() == SceneManager.Settings.StartManagerType);
        while (sceneManager == null)
        {
            Log("Waiting on scene manager");
            yield return new WaitForSeconds(0.1f);
            sceneManager = Instance.SceneManagers.Find(p => p.GetType() == SceneManager.Settings.StartManagerType);
        }
        Log("Transitioning to scene manager " + sceneManager.GetType().Name);
        yield return Transition(sceneManager, null, new UpdateProgressDelegate(progressUpdateWithFactor));
        Log("Destroying LoadingLevelObject's");
        SwitchLevelSettings = null;
        foreach (var t in list)
        {
            Destroy(t.gameObject);
        }
        Log("Load complete");
        ProgressUpdated("Complete", 1f);
    }

    /// <summary>The log method is a wrapper around the UnityEngine.Debug.Log method that allows you to create debug messages with an on and off switch on the game manager's
    /// inspector.</summary>
    public static void Log(string message, UnityEngine.Object obj = null)
    {
        if (!Instance._ShowLogs) return;
        if (obj == null)
        {
            UnityEngine.Debug.Log(message);
        }
        else
        {
            UnityEngine.Debug.Log(message, obj);
        }
    }

    public static void ProgressUpdated(string message, float progress)
    {
        Progress.Status = message;
        Progress.Progress = progress;
    }

    /// <buildflag>Exclude from Online</buildflag>
    /// <buildflag>Exclude from Booklet</buildflag>
    [Obsolete("The method SwitchGame is obsolete.  Use Transition<T> or 'Save & Compile' your diagram.")]
    public static Coroutine SwitchGame<T>(Action<T> setup, UpdateProgressDelegate progress = null) where T : SceneManager
    {
        return null;
    }

    [Obsolete("The method SwitchGame is obsolete.  Use TransitionLevel<T> or 'Save & Compile' your diagram.")]
    public static Coroutine SwitchGameAndLevel<TGame>(TGame controller, Action<TGame> setup = null,
        UpdateProgressDelegate progress = null) where TGame : SceneManager
    {
        return null;
    }

    [Obsolete("The method SwitchGame is obsolete.  Use TransitionLevel<T> or 'Save & Compile' your diagram.")]
    public static void SwitchGameAndLevel<T>(SwitchLevelSettings<T> settings) where T : SceneManager
    {
    }

    [Obsolete("The method SwitchGame is obsolete.  Use TransitionLevel<T> or 'Save & Compile' your diagram.")]
    public static void SwitchGameAndLevel<T>(Action<T> setup, params string[] levels) where T : SceneManager
    {
    }

    /// <summary>This method will transition a scene manager from on to the other, or load the first scene manager upon load.</summary>
    public static Coroutine Transition<T>(Action<T> setup, UpdateProgressDelegate progress = null) where T : SceneManager
    {
        return Transition(Instance.SceneManagers.OfType<T>().First());
    }

    /// <summary>This method will transition a scene manager from on to the other, or load the first scene manager upon load.</summary>
    /// <typeparam name="TSceneManager"></typeparam>
    /// <param name="progress"></param>
    /// <param name="setup"></param>
    /// <param name="sceneManager"></param>
    /// <returns></returns>
    /// <example>
    /// 	<code title="Example" description="" lang="CS">
    /// GameManager.Transition&lt;SceneManagerB&gt;(sceneManagerB=&gt;{
    ///     sceneManagerB.SetSomethingBeforeItsLoaded();
    /// }, (message,progress)=&gt; { Debug.Log(progress + "% " + message);</code>
    /// </example>
    public static Coroutine Transition<TSceneManager>(TSceneManager sceneManager, Action<TSceneManager> setup = null, UpdateProgressDelegate progress = null) where TSceneManager : SceneManager
    {
        if (sceneManager == null)
        {
            throw new Exception(String.Format("{0} is null or was not found while trying to load the game. Have you loaded the correct scene? Try", typeof(TSceneManager).Name));
        }

        if (ActiveSceneManager != null)
        {
            ActiveSceneManager.Unload();
            ActiveSceneManager.enabled = false;
            ActiveSceneManager.gameObject.SetActive(false);
            Log("Deactivated old scene manager.");
        }

        ActiveSceneManager = sceneManager;
        ActiveSceneManager.gameObject.SetActive(true);
        ActiveSceneManager.enabled = true;
        Log("Scene Manager Enabled");
        if (setup != null)
        {
            setup(sceneManager);
        }
        ActiveSceneManager.OnLoading();
        Log("Beginning Scene Manager Loading");
        return Instance.StartCoroutine(LoadSceneManager(progress ?? DefaultUpdateProgress));
    }

    /// <summary>
    /// Transitions to another scene and loads additional scene if specified.
    /// game assuming that it will exist in the scene after loading is finished.
    /// </summary>
    /// <typeparam name="T">The SceneManager type that will exist in the first scene specified.</typeparam>
    public static void TransitionLevel<T>(SwitchLevelSettings<T> settings) where T : SceneManager
    {
        if (ActiveSceneManager != null)
        {
            ActiveSceneManager.PersistantViewModels.Clear();
            ActiveSceneManager.PersistantViews.Clear();
            ActiveSceneManager.Unload();
            ActiveSceneManager.enabled = false;
            ActiveSceneManager.gameObject.SetActive(false);
            ActiveSceneManager = null;
        }

        SwitchLevelSettings = settings;

        Application.LoadLevel(Instance._LoadingLevel);
    }

    /// <summary>
    /// Transitions to another scene and loads additional scene if specified.
    /// game assuming that it will exist in the scene after loading is finished.
    /// </summary>
    /// <typeparam name="T">The SceneManager type that will exist in the first scene specified.</typeparam>
    /// <param name="setup">Perform additonal setup when the scene has transitioned.</param>
    /// <param name="levels">The SceneManager type that will exist in the first scene specified.</param>
    public static void TransitionLevel<T>(Action<T> setup, params string[] levels) where T : SceneManager
    {
        if (levels.Length < 1)
        {
            throw new Exception("There must be at least one level name passed to the SwitchGameAndLevel method.");
        }
        var settings = new SwitchLevelSettings<T>()
        {
            Setup = setup,
            Levels = levels
        };
        TransitionLevel(settings);
    }

    /// <summary>
    /// 	<para>The on awake method will determine if there is already an instance created, if it hasn't then it will become the instance used throughout the games life
    /// time. </para>
    /// 	<para>If the instances is already set, it will pass the current scene-manager to the currently running instance and the destroy itself.  The running instance
    /// will then begin the uFrame boot process for that scene manager.</para>
    /// </summary>
    public void Awake()
    {
        if (Instance != null)
        {
            // If the instace already exist destroy this
            Instance._Start = _Start;
            Instance.Startup();
        }
        else
        {
            Instance = this;
            if (this.gameObject.GetComponent<MainThreadDispatcher>() == null)
                this.gameObject.AddComponent<MainThreadDispatcher>();
            // Don't destory this through scene switching
            DontDestroyOnLoad(gameObject);
            Startup();

            //_Start.Container;
        }
    }

    /// <summary>
    /// When this is destroyed check if we are the "current instance" and set "Instance" to null.
    /// Note: This should really never happen.  But in some test cases is necessary.
    /// </summary>
    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>During the Game-Managers load method it waits for a scene manager to register itself.  A scenemanager's awake method will invoke this function so the game
    /// manager prepare it for loading.</summary>
    /// <param name="sceneManager">The scene manager to register.</param>
    public virtual void RegisterSceneManager(SceneManager sceneManager)
    {
        if (SceneManagers.Contains(sceneManager)) return;
        SceneManagers.Add(sceneManager);
        Log(string.Format("Scene Manager {0} was registered", sceneManager.gameObject.name));
        sceneManager.Container = Container;

        if (SwitchLevelSettings != null)
        {
            SwitchLevelSettings.InvokeControllerSetup(sceneManager);
        }
        Container.Inject(sceneManager);
        sceneManager.Setup();
        sceneManager.Initialize();
     
        sceneManager.enabled = false;
        sceneManager.gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks if the gamemanager has already been loaded.  If so it will copy necessary info and destroy
    /// itself.  This also calls "Transition" in order to load the "Start" scene manager of the scene.
    /// </summary>
    public void Start()
    {
        if (Instance != null && Instance != this)
        {
            Instance._Start = this._Start;
            //SwitchGame(_Start);
            DestroyImmediate(gameObject);
        }
        else
        {
            if (_Start != null)
                Transition(_Start);
        }
    }

    /// <summary>
    /// Startup will register every scenemanager in the scene. As well as set the "ActiveSceneManager' to the specified
    /// 'Start' scene manager specified in the inspector.
    /// </summary>
    /// <buildflag>Exclude from Online</buildflag>
    /// <buildflag>Exclude from Booklet</buildflag>
    protected virtual void Startup()
    {
        SceneManagers.Clear();
        var games = FindObjectsOfType<SceneManager>();
        foreach (var game in games)
        {
            RegisterSceneManager(game);
        }
        if (_Start == null)
        {
            Debug.LogError(
                "You need to set the scene manager option on the game manager for this scene. Double-Click to navigate there.", this.gameObject);
        }
        ActiveSceneManager = _Start;
    }

    /// <summary>
    /// Removes the Scene Manager from this manager.  This will only happen if a Game is destroyed
    /// </summary>
    /// <param name="sceneManager"></param>
    public virtual void UnRegisterSceneManager(SceneManager sceneManager)
    {
        SceneManagers.Remove(sceneManager);
    }

    /// <buildflag>Exclude from Online</buildflag>
    /// <buildflag>Exclude from Booklet</buildflag>
    protected static void DefaultUpdateProgress(string message, float progress)
    {
        Debug.Log(String.Format("Loading: {0}% - {1}", progress * 100f, message));
    }

    /// <summary>
    /// Begins the the "Load" coroutine of the scenemanger passing the appropriate progress delegate.
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    private static IEnumerator LoadSceneManager(UpdateProgressDelegate progress)
    {
        // yield return new WaitForEndOfFrame();

        yield return Instance.StartCoroutine(ActiveSceneManager.Load(progress));
        Log("Scene Manager Loading Complete");
        ActiveSceneManager.OnLoaded();
    }

    public void ExecuteCommand(IuFrameCommand command, object argument, bool isChained = false)
    {
        command.Execute(argument);
        if (_commandsAsObservable != null)
        {
            _commandsAsObservable.OnNext(new CommandInfo(command,argument,isChained));
        }
    }

    public IObservable<CommandInfo> CommandsAsObservable
    {
        get { return _commandsAsObservable ?? (_commandsAsObservable = new SimpleSubject<CommandInfo>()); }
        
    }

    public IDisposable Subscribe(IObserver<CommandInfo> observer)
    {
        return CommandsAsObservable.Subscribe(observer);
    }
}