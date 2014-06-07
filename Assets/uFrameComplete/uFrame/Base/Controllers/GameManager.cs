using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// A singleton that manages our current Scene Manager and all the games types in the scene.
/// This component will persist through every scene
/// </summary>
public class GameManager : MonoBehaviour
{
    public Color _AmbientLight = new Color(0.2f, 0.2f, 0.2f, 1.0f);

    public float _FlareStrength = 1.0f;

    public bool _Fog;

    public Color _FogColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    public float _FogDensity = 0.01f;

    public FogMode _FogMode = FogMode.ExponentialSquared;

    public float _HaloStrength = 0.5f;

    public float _LinearFogEnd = 300.0f;

    public float _LinearFogStart = 0.0f;

    /// <summary>
    /// A level that displays a progress bar and message
    /// </summary>
    public string _LoadingLevel;

    public Material _SkyboxMaterial;

    /// <summary>
    /// Set this to the game that will load when the game starts
    /// </summary>
    public SceneManager _Start;

    public string _StartupScene;

    [HideInInspector]
    public string _ViewModelScriptsPath = "@ElementPath/";

    [HideInInspector]
    public string _ViewPrefabsPath = "@ElementPath/Resources/";

    [HideInInspector]
    public string _ViewsScriptsPath = "@ElementPath/";

    private static IGameContainer _container;

    private static LevelLoadViewModel _loadingViewModel;

    private List<SceneManager> _games = new List<SceneManager>();

    /// <summary>
    /// The current running game
    /// </summary>
    public static SceneManager ActiveSceneManager { get; private set; }

    public static IGameContainer Container
    {
        get
        {
            if (_container == null)
            {
                _container =new GameContainer();
                _container.RegisterInstance(Progress);
            }
            return _container;
        }
    }

    private static LevelLoadViewModel _progress;

    public static LevelLoadViewModel Progress
    {
        get
        {
            return _progress ?? (_progress = new LevelLoadViewModel());
        }
    }

    /// <summary>
    /// The current instance of GameManager
    /// </summary>
    public static GameManager Instance { get; set; }

    /// <summary>
    /// The view model that is used for loading a scene.  Bind to this to be notified of progress changes
    /// </summary>
    /// <value>The loading view model.</value>
    public static LevelLoadViewModel LoadingViewModel
    {
        get { return _loadingViewModel ?? (_loadingViewModel = new LevelLoadViewModel()); }
        set { _loadingViewModel = value; }
    }

    public static ISwitchLevelSettings SwitchLevelSettings { get; set; }
    public static void ProgressUpdated(string message, float progress)
    {
        Progress.Status = message;
        Progress.Progress = progress;
    }
    public Type ContainerType
    {
        get;
        set;
    }

    /// <summary>
    /// A list of all the game in the scene.
    /// Each game registers itself with this manager and is added to this list.
    /// </summary>
    public List<SceneManager> Games
    {
        get { return _games; }
        set { _games = value; }
    }

    public static Coroutine SwitchGame<T>(Action<T> setup, UpdateProgressDelegate progress = null) where T : SceneManager
    {
var currentSceneManager = GameManager.ActiveSceneManager;
        return SwitchGame(Instance.Games.OfType<T>().First());
    }

    /// <summary>
    /// This switches the game from one to the other invoking a sequence of actions
    /// SwitchGame
    ///     - Invoke the current controllers Unload() method.
    ///     - Set the CurrentController Property to the new game
    ///     - New Controller Load() method is invoked via StartCoroutine
    ///     - New Controller OnLoading() method is invoked
    ///     - After the Load() Coroutine method is complete it will invoke the ActiveGame Game's OnLoaded() method
    /// </summary>
    /// <typeparam name="TGame">The Scene Manager</typeparam>
    /// <param name="progress"></param>
    /// <param name="setup"></param>
    /// <param name="controller"></param>
    /// <returns></returns>
    public static Coroutine SwitchGame<TGame>(TGame controller, Action<TGame> setup = null, UpdateProgressDelegate progress = null) where TGame : SceneManager
    {
        if (controller == null)
        {
            throw new Exception(String.Format("{0} is null or was not found while trying to load the game. Have you loaded the correct scene? Try", typeof(TGame).Name));
        }

        if (ActiveSceneManager != null)
        {
            ActiveSceneManager.Unload();
            ActiveSceneManager.enabled = false;
            ActiveSceneManager.gameObject.SetActive(false);
        }

        ActiveSceneManager = controller;
        ActiveSceneManager.gameObject.SetActive(true);
        ActiveSceneManager.enabled = true;

        if (setup != null)
        {
            setup(controller);
        }
        ActiveSceneManager.OnLoading();
        return Instance.StartCoroutine(InitializeController(progress ?? DefaultUpdateProgress));
    }

    /// <summary>
    /// Loads the other levels asynchronously and then switches the
    /// game assuming that it will exist in the scene after loading is finished.
    /// </summary>
    /// <typeparam name="T">The type of game</typeparam>
    /// <returns></returns>
    public static void SwitchGameAndLevel<T>(SwitchLevelSettings<T> settings) where T : SceneManager
    {
        if (ActiveSceneManager != null)
        {
            ActiveSceneManager.Unload();
            ActiveSceneManager.enabled = false;
            ActiveSceneManager.gameObject.SetActive(false);
            ActiveSceneManager = null;
        }

        SwitchLevelSettings = settings;

        Application.LoadLevel(Instance._LoadingLevel);
    }

    /// <summary>
    /// Loads the other levels asynchronously and then switches the
    /// game assuming that it will exist in the scene after loading is finished.
    /// </summary>
    /// <typeparam name="T">The type of the Game to switch to</typeparam>
    /// <param name="setup">Setup the Game?</param>
    /// <param name="levels">Load these levels additively?</param>
    /// <returns></returns>
    public static void SwitchGameAndLevel<T>(Action<T> setup, params string[] levels) where T : SceneManager
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
        SwitchGameAndLevel(settings);
    }

    /// <summary>
    /// Adds a controler to the list of registered controllers.
    /// You shouldn't have to use this method directly.  It is used by a game to register itself.
    /// </summary>
    /// <param name="sceneManager">The game being added.</param>
    public virtual void AddGame(SceneManager sceneManager)
    {
        if (Games.Contains(sceneManager)) return;
        sceneManager.Container = Container;
        sceneManager.Setup();
        Games.Add(sceneManager);
        sceneManager.enabled = false;
        sceneManager.gameObject.SetActive(false);
    }

    public void ApplyRenderSettings()
    {
        RenderSettings.fog = _Fog;
        RenderSettings.fogColor = _FogColor;
        RenderSettings.fogMode = _FogMode;
        RenderSettings.fogDensity = _FogDensity;

        RenderSettings.fogStartDistance = _LinearFogStart;
        RenderSettings.fogEndDistance = _LinearFogEnd;

        RenderSettings.ambientLight = _AmbientLight;
        RenderSettings.skybox = _SkyboxMaterial;

        RenderSettings.haloStrength = _HaloStrength;

        RenderSettings.flareStrength = _FlareStrength;
    }

    public void OnEnable()
    {
    }

    public void Awake()
    {
        ApplyRenderSettings();

        if (Instance != null)
        {
            // If the instace already exist destroy this
            Instance._Start = _Start;
            Instance.Startup();
           
        }
        else
        {
            Instance = this;
            // Don't destory this through scene switching
            DontDestroyOnLoad(gameObject);
            Startup();

            //_Start.Container;
        }
    }

    public void Start()
    {
       

        if (Instance != null && Instance != this)
        {
            SwitchGame(_Start);
            DestroyImmediate(gameObject);
        }
        else
        {
            if (_Start != null)
                SwitchGame(_Start);
        }
    }

    public virtual void Startup()
    {
        var games = FindObjectsOfType<SceneManager>();
        foreach (var game in games)
        {
            AddGame(game);
        }
        if (_Start == null)
        {
            if (games.Length > 0)
            {
                _Start = games.First();
            }
            else
                Debug.LogWarning(
                    "There is not a start game assigned to GameManager. Set the start game in the GameManager inspector.");
        }
        ActiveSceneManager = _Start;
    }



    public string GetPath(string elementPath, string path)
    {
        return Regex.Replace(path, "@ElementPath", elementPath);
    }

    public void LoadRenderSettings()
    {
        _Fog = RenderSettings.fog;
        _FogColor = RenderSettings.fogColor;
        _FogMode = RenderSettings.fogMode;
        _FogDensity = RenderSettings.fogDensity;

        _LinearFogStart = RenderSettings.fogStartDistance;
        _LinearFogEnd = RenderSettings.fogEndDistance;

        _AmbientLight = RenderSettings.ambientLight;
        _SkyboxMaterial = RenderSettings.skybox;

        _HaloStrength = RenderSettings.haloStrength;

        _FlareStrength = RenderSettings.flareStrength;
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Removes the Scene Manager from this manager.  This will only happen if a Game is destroyed
    /// </summary>
    /// <param name="sceneManager"></param>
    public virtual void RemoveGame(SceneManager sceneManager)
    {
        Games.Remove(sceneManager);
    }

    protected static void DefaultUpdateProgress(string message, float progress)
    {
        Debug.Log(String.Format("Loading: {0}% - {1}", progress * 100f, message));
    }

    private static IEnumerator InitializeController(UpdateProgressDelegate progress)
    {
       // yield return new WaitForEndOfFrame();
        yield return Instance.StartCoroutine(ActiveSceneManager.Load(progress));
        ActiveSceneManager.OnLoaded();
    }

    public static bool IsPro
    {
        get
        {
            
            return Application.HasProLicense();
        }
    }

    public bool _DontUseAsyncLoading = false;

    public static IEnumerator Load()
    {
        
        // Wait till all other things have processed in the scene before loading.
        yield return new WaitForEndOfFrame();
        // Grab the game manager instance transform so we can ignore it
        var gameTransform = Instance.transform;
        // Grab the root objects in the scene to destroy them later
        var list = new List<Transform>();
        // The loading screen should be small
        foreach (Transform p in FindObjectsOfType(typeof(Transform)))
        {
            if (p == gameTransform)
                continue;

            if (p.parent == null)
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
                    AsyncOperation asyncOperation;
                    asyncOperation = Application.LoadLevelAdditiveAsync(level);
                    while (!asyncOperation.isDone)
                    {
                        progressValue = (asyncOperation.progress + progressBase) / progressFactor;
                        ProgressUpdated(String.Format("Loading '{0}'", level), progressValue);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    ProgressUpdated(String.Format("Loading '{0}'", level), numberOfLevelsToLoad);
                    Application.LoadLevelAdditive(level);
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

        // Now that the main level with the controller is loaded we can grab the Scene Manager
        var controller = Instance.Games.Find(p => p.GetType() == SceneManager.Settings.StartControllerType);
        while (controller == null)
        {
            yield return new WaitForSeconds(0.1f);
            controller = Instance.Games.Find(p => p.GetType() == SceneManager.Settings.StartControllerType);
        }
        yield return SwitchGame(controller, SceneManager.Settings.InvokeControllerSetup, new UpdateProgressDelegate(progressUpdateWithFactor));
        foreach (var t in list)
        {
            Destroy(t.gameObject);
        }
        ProgressUpdated("Complete", 1f);
    }
}