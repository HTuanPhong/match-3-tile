using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// This class auto-loads a bootstrap screen (first scene in Build Settings) while working in the Editor.
/// </summary>
[InitializeOnLoad]
public static class SceneBootstrapper
{
  // This gets the bootstrap scene, which must be first scene in Build Settings
  private static string BootstrapScene
  {
    get => EditorBuildSettings.scenes[0].path;
  }

  // This string is the scene name where we entered Play mode
  private static string PreviousScene
  {
    get => EditorPrefs.GetString("k_PreviousScene");
    set => EditorPrefs.SetString("k_PreviousScene", value);
  }

  // A static constructor runs with InitializeOnLoad attribute
  static SceneBootstrapper()
  {
    // Listen for the Editor changing play states
    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
  }

  // This runs when the Editor changes play state (e.g. entering Play mode, exiting Play mode)
  private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
  {
    switch (playModeStateChange)
    {
      // This loads bootstrap scene when entering Play mode
      case PlayModeStateChange.ExitingEditMode:

        PreviousScene = EditorSceneManager.GetActiveScene().path;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && IsSceneInBuildSettings(BootstrapScene))
        {
          EditorSceneManager.OpenScene(BootstrapScene);
        }
        break;

      // This restores the PreviousScene when exiting Play mode
      case PlayModeStateChange.EnteredEditMode:

        if (!string.IsNullOrEmpty(PreviousScene))
        {
          EditorSceneManager.OpenScene(PreviousScene);
        }
        break;
    }
  }

  // Is a scenePath a valid scene in the File > Build Settings?
  private static bool IsSceneInBuildSettings(string scenePath)
  {
    if (string.IsNullOrEmpty(scenePath))
      return false;

    foreach (var scene in EditorBuildSettings.scenes)
    {
      if (scene.path == scenePath)
      {
        return true;
      }
    }

    return false;
  }

}