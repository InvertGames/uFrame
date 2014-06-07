using System;
using UnityEngine;

public class SwitchLevelSettings<T> : ISwitchLevelSettings where T : SceneManager
{
    public string[] Levels
    {
        get;
        set;
    }

    public Action<LevelLoadProgress> ProgressUpdated { get; set; }

    public Action<T> Setup
    {
        get;
        set;
    }

    public Type StartControllerType
    {
        get
        {
            return typeof(T);
        }
    }

    public SwitchLevelSettings()
    {
    }

    public SwitchLevelSettings(Action<T> setup)
        : this(new string[] { }, setup)
    {
    }

    public SwitchLevelSettings(string[] levels, Action<T> setup)
    {
        Levels = levels;
        Setup = setup;
        ProgressUpdated = p => Debug.Log(string.Format("Loading Level Progress: ({0}%) {1}", p.Progress * 100f, setup));
    }

    public SwitchLevelSettings(Action<LevelLoadProgress> progressUpdated, string[] levels, Action<T> setup)
    {
        ProgressUpdated = progressUpdated;
        Levels = levels;
        Setup = setup;
    }

    public SwitchLevelSettings(Action<LevelLoadProgress> progressUpdated, string[] levels)
    {
        ProgressUpdated = progressUpdated;
        Levels = levels;
    }

    public SwitchLevelSettings(Action<LevelLoadProgress> progressUpdated)
    {
        ProgressUpdated = progressUpdated;
    }

    public void InvokeControllerSetup(SceneManager sceneManager)
    {
        if (Setup != null)
            Setup(sceneManager as T);
    }
}