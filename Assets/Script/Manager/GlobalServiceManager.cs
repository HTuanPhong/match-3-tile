using UnityEngine;
using System.IO;

public class GlobalServiceManager : MonoBehaviour
{
  public static GlobalServiceManager Instance { get; private set; } = null;
  public LocalIOService LocalIOService;
  public AudioService AudioService;
  [SerializeField] private AudioSource _musicSource;
  [SerializeField] private AudioSource _effectSource;
  private void Awake()
  {
    SingletonSetup();
    LocalIOService = new LocalIOService(Path.Combine(Application.dataPath, "IO"));
    AudioService = AudioService = new AudioService(_musicSource, _effectSource);

    // init
    var boardData = new GameboardData
    {
      MaxHeight = 7,
      MaxWidth = 7,
      RackSize = 7,
      TileToWin = 100,
      Tiles = new TileData[]
        {
        new() { Type = 4, X = 3.0f, Y = 6.5f, Z = 100 },
        new() { Type = 2, X = 0.5f, Y = 6.5f, Z = 100 },
        new() { Type = 2, X = 0.5f, Y = 5.5f, Z = 100 },
        new() { Type = 2, X = 1.5f, Y = 6.5f, Z = 100 },
        new() { Type = 2, X = 1.5f, Y = 5.5f, Z = 100 },
        new() { Type = 5, X = 0, Y = 7, Z = 3 },
        new() { Type = 6, X = 0, Y = 6, Z = 3 },
        new() { Type = 7, X = 0, Y = 5, Z = 3 },
        new() { Type = 8, X = 1, Y = 7, Z = 3 },
        new() { Type = 9, X = 1, Y = 6, Z = 3 },
        new() { Type = 10, X = 1, Y = 5, Z = 3 },
        new() { Type = 1, X = 2, Y = 7, Z = 3 },
        new() { Type = 1, X = 2, Y = 6, Z = 3 },
        new() { Type = 1, X = 2, Y = 5, Z = 3 },
        new() { Type = 1, X = 0, Y = 6, Z = 2 },
        new() { Type = 1, X = 0, Y = 5, Z = 2 },
        new() { Type = 1, X = 1, Y = 7, Z = 2 },
        new() { Type = 1, X = 1, Y = 6, Z = 2 },
        new() { Type = 1, X = 1, Y = 5, Z = 2 },
        new() { Type = 1, X = 2, Y = 7, Z = 2 },
        new() { Type = 1, X = 2, Y = 6, Z = 2 },
        new() { Type = 1, X = 2, Y = 5, Z = 2 },
        new() { Type = 1, X = 0, Y = 7, Z = 1 },
        new() { Type = 1, X = 0, Y = 6, Z = 1 },
        new() { Type = 1, X = 0, Y = 5, Z = 1 },
        new() { Type = 1, X = 1, Y = 7, Z = 1 },
        new() { Type = 1, X = 1, Y = 6, Z = 1 },
        new() { Type = 1, X = 1, Y = 5, Z = 1 },
        new() { Type = 1, X = 2, Y = 7, Z = 1 },
        new() { Type = 1, X = 2, Y = 6, Z = 1 },
        new() { Type = 1, X = 2, Y = 5, Z = 1 },
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
}

