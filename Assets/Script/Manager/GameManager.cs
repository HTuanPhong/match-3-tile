using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; } = null;
  public GameStateModel GameStateModel;
  public LocalIOService LocalIOService;

  private void Awake()
  {
    SingletonSetup();
    GameStateModel = new GameStateModel();
    LocalIOService = new LocalIOService(Path.Combine(Application.dataPath, "IO"));

    // init
    var boardData = new GameboardData
    {
      MaxHeight = 7,
      MaxWidth = 7,
      RackSize = 7,
      TileToWin = 10,
      Tiles = new TileData[]
        {
        new() { Type = 2, X = 0.5f, Y = 6.5f, Z = 1 },
        new() { Type = 2, X = 0.5f, Y = 5.5f, Z = 1 },
        new() { Type = 2, X = 1.5f, Y = 6.5f, Z = 1 },
        new() { Type = 2, X = 1.5f, Y = 5.5f, Z = 1 },
        new() { Type = 1, X = 0, Y = 7, Z = 0 },
        new() { Type = 1, X = 0, Y = 6, Z = 0 },
        new() { Type = 1, X = 0, Y = 5, Z = 0 },
        new() { Type = 1, X = 1, Y = 7, Z = 0 },
        new() { Type = 1, X = 1, Y = 6, Z = 0 },
        new() { Type = 1, X = 1, Y = 5, Z = 0 },
        new() { Type = 1, X = 2, Y = 7, Z = 0 },
        new() { Type = 1, X = 2, Y = 6, Z = 0 },
        new() { Type = 1, X = 2, Y = 5, Z = 0 },

        }
    };
    LocalIOService.SaveJson("levels/level_0.json", boardData.ToJson(true));
    var playerData = new PlayerData
    {
      Coins = 1000,
      HighestLevel = 2,
      Name = "Nguyen Nguyen",
      Settings = new SettingData
      {
        MusicVolume = 40
      }
    };
    LocalIOService.SaveJson("players/player_0.json", playerData.ToJson(true));

    // scene change 
    Debug.Log("Game Initialized!");

    ExitBootstrapScene();
  }

  private void SingletonSetup()
  {
    // singleton setup
    if (Instance != null)
    {
      Debug.LogError("Another BootstrapManager on " + gameObject.name);
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  private void ExitBootstrapScene()
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

