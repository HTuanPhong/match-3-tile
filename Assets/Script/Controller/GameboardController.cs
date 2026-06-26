public class GameboardController
{
  private GameboardModel _model;
  private GameboardView _view;
  private GameboardLocalRepository _repo;

  public GameboardController(GameboardLocalRepository repo, GameboardView view)
  {
    _repo = repo;
    _view = view;
  }

  public void Start()
  {
    _model = _repo.LoadCurrent();
    // view -> model
    _view.TileSelect += _model.SelectTile;
    // model -> view
    _model.OnTileIllegalSelected += _view.OnTileShake;
    _model.OnTileRemovedFromBoard += _view.OnTileSelectFeedback;
    _model.OnTileRevealed += _view.OnTileLighten;
    _model.OnTileAddedToRack += _view.QueueTileMovement;
    _model.OnMatchCleared += _view.QueueMatchClear;
    _model.OnGameEnded += _view.QueueGameEnd;

    _view.CreateBoard(_model.TilesOnBoard, _model.TilesOnRack, _model.IsTileOverlapped);
  }
}