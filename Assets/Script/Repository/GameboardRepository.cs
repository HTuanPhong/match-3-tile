public class GameboardLocalRepository
{
  private LocalIOService _localIOService;
  private GameStateModel _gameStateModel;

  public GameboardLocalRepository(GameStateModel gameStateModel, LocalIOService localIOService)
  {
    _localIOService = localIOService;
    _gameStateModel = gameStateModel;
  }
  public GameboardModel LoadCurrent()
  {
    int currentLevel = _gameStateModel.CurrentLevel;
    string json = _localIOService.ReadJson($"levels/level_{currentLevel}.json");
    return new GameboardModel(GameboardData.FromJson(json));
  }

  public void Save(GameboardModel model)
  {

  }
}