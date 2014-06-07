using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate void UpdateProgressDelegate(string message, float progress);

/// <summary>
/// A [u]Frame built-in controller to manage loading a level via GameManager
/// Add this in a level-loading scene along with LevelLoadViewModel and a LevelLoaderView.
/// </summary>
public class LevelLoaderController : Controller
{
    /// <summary>
    /// The settings at which the level will be loaded
    /// </summary>
    /// <value>The settings.</value>
    public static ISwitchLevelSettings Settings
    {
        get
        {
            return GameManager.SwitchLevelSettings;
        }
    }

    protected LevelLoadViewModel Progress
    {
        get
        {
            return GameManager.LoadingViewModel;
        }
    }

    public void ProgressUpdated(string message, float progress)
    {
        Progress.Status = message;
        Progress.Progress = progress;
    }

    protected IEnumerator Load()
    {
        // Wait till all other things have processed in the scene before loading.
        yield return new WaitForEndOfFrame();
        // Grab the game manager instance transform so we can ignore it
        var gameTransform = GameManager.Instance.transform;
        // Grab the root objects in the scene to destroy them later
        var list = new List<Transform>();

        foreach (Transform p in Object.FindObjectsOfType(typeof(Transform)))
        {
            if (p == gameTransform)
                continue;

            if (p.parent == null)
                list.Add(p);
        }

        //var rootObjects = list.ToArray();

        // For progress tracking
        var progressValue = 0f;
        if (Settings.Levels != null)
        {
            var numberOfLevelsToLoad = (Settings.Levels == null ? 1 : Settings.Levels.Length) + 1;
            var progressFactor = numberOfLevelsToLoad;
            var progressBase = 0f;
            for (int index = 0; index < Settings.Levels.Length; index++)
            {
                var level = Settings.Levels[index];
                AsyncOperation asyncOperation;
                asyncOperation = Application.LoadLevelAdditiveAsync(level);
                while (!asyncOperation.isDone)
                {
                    progressValue = (asyncOperation.progress + progressBase) / progressFactor;
                    ProgressUpdated(string.Format("Loading '{0}'", level), progressValue);
                    yield return new WaitForSeconds(0.1f);
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

        // Now that the main level with the controller is loaded we can grab the controller
        var controller = GameManager.Instance.Games.Find(p => p.GetType() == Settings.StartControllerType);

        yield return GameManager.SwitchGame(controller, Settings.InvokeControllerSetup, new UpdateProgressDelegate(progressUpdateWithFactor));

        ProgressUpdated("Complete", 1f);

        //// Now destroy the loading scene
        //foreach (var rootObject in rootObjects)
        //{
        //    Object.Destroy(rootObject.gameObject);
        //}
    }

    public override void WireCommands(ViewModel viewModel)
    {
        throw new NotImplementedException();
    }

    public override ViewModel CreateEmpty()
    {
        throw new NotImplementedException();
    }

    public override ViewModel Create(System.Action<ViewModel> preInitializer = null)
    {
        throw new NotImplementedException();
    }

    public override void Initialize(ViewModel viewModel)
    {
        throw new NotImplementedException();
    }

    //public override void Initialize(ViewModel vm)
    //{
        
    //}
}