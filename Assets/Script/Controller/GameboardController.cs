using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GameboardController
{
  private readonly GameboardModel _model;
  private readonly GameboardView _view;

  public GameboardController(GameboardModel model, GameboardView view)
  {
    _model = model;
    _view = view;
    CreateBoard();
  }

  public void CreateBoard()
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

  public void ProcessTileSelect(TileData tile)
  {
    TileSelectReceipt receipt = _model.SelectTile(tile);
    if (receipt.IsMoveLegal == false)
    {
      // play tile locked animation shake or smth
      return;
    }
    foreach (TileData revealedTile in receipt.RevealedTiles)
    {
      // light up the tile
    }
    // play tile to rack animation with receipt.RackInsertionIndex
    if (receipt.DidMergeOccur)
    {
      // merge destruction animation with receipt.MergedTiles
      // update score with tile so far model
    }
    if (receipt.GameIsLost)
    {
      // disable board
      // loose panel
    }
    else if (receipt.GameIsWon)
    {
      // disable board
      // win panel
    }
  }
}