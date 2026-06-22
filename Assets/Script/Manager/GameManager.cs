using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;
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
        new() { Type = 1, X = 0, Y = 0, Z = 0 },
        new() { Type = 2, X = 1, Y = 0, Z = 0 },
        new() { Type = 3, X = 2, Y = 0, Z = 0 },
        new() { Type = 4, X = 0, Y = -1, Z = 0 },
        new() { Type = 5, X = 0, Y = -2, Z = 0 },
        new() { Type = 10, X = 8, Y = -13, Z = 0 },
        new() { Type = 9, X = 8, Y = -14, Z = 0 },
        new() { Type = 6, X = 8, Y = -15, Z = 0 },
        new() { Type = 7, X = 7, Y = -15, Z = 0 },
        new() { Type = 8, X = 6, Y = -15, Z = 0 },
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
      SceneManager.LoadScene(previousScenePath);
      return;
    }
#endif

    SceneManager.LoadScene("Menu");
  }
}

