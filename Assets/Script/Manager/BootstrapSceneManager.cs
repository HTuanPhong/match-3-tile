using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class BootstrapSceneManager : MonoBehaviour
{
  private void Start()
  {
#if UNITY_EDITOR
    // Fetch the actual saved scene path from EditorPrefs
    string previousScenePath = EditorPrefs.GetString("k_PreviousScene", "");
    string bootstrapScenePath = EditorBuildSettings.scenes[0].path;

    // If we came from a scene that is NOT the bootstrap scene, load it back
    if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != bootstrapScenePath)
    {
      // This loads the test scene instantly in the Editor without needing a build setup
      EditorSceneManager.LoadSceneInPlayMode(previousScenePath, new LoadSceneParameters(LoadSceneMode.Single));
      return;
    }
#endif

    SceneManager.LoadScene("Menu");
  }
}

