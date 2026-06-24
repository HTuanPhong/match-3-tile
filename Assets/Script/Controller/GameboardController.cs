using System;
using Cysharp.Threading.Tasks;
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
    _view.TileSelect += (tile) => ProcessTileSelect(tile).Forget();
    CreateBoard();
  }

  private void CreateBoard()
  {
    foreach (TileData tile in _model.TilesOnBoard)
    {
      _view.CreateTileOnBoard(tile);
    }

    int i = 0;
    foreach (TileData tiles in _model.TilesOnRack)
    {
      _view.CreateTileOnRack(tiles, i);
      i++;
    }

    foreach (TileData tile in _model.TilesOnBoard)
    {
      if (_model.IsTileOverlapped(tile))
      {
        _view.DarkenTile(tile);
      }
    }
  }

  public async UniTask ProcessTileSelect(TileData tile)
  {
    TileSelectReceipt receipt = _model.SelectTile(tile);
    if (receipt.IsMoveLegal == false)
    {
      await _view.ShakeTile(tile);
      return;
    }
    foreach (TileData revealedTile in receipt.RevealedTiles)
    {
      _view.LightenTile(revealedTile);
    }
    await _view.MoveTileToRack(tile, receipt.RackInsertionIndex);
    if (receipt.DidMergeOccur)
    {
      await _view.MergeTile(receipt.MergedTiles);
      // update score with tile so far model
    }
    if (receipt.GameIsLost)
    {
      _view.DisableBoard();
      _view.LoosePanel();
    }
    else if (receipt.GameIsWon)
    {
      _view.DisableBoard();
      _view.WinPanel();
    }
  }
}