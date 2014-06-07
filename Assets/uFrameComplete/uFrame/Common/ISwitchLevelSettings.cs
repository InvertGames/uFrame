using System;

public interface ISwitchLevelSettings
{
    string[] Levels { get; set; }

    Action<LevelLoadProgress> ProgressUpdated { get; set; }

    Type StartControllerType { get; }

    void InvokeControllerSetup(SceneManager sceneManager);
}