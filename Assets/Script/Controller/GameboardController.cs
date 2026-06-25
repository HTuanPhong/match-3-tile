using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
      _view.CreateTileOnBoard(tile).Forget();
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
        _view.DarkenTile(tile).Forget();
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

    // Lighten revealed tiles in the background without waiting
    foreach (TileData revealedTile in receipt.RevealedTiles)
    {
      _view.LightenTile(revealedTile).Forget();
    }

    if (receipt.DidMergeOccur)
    {
      // SHIFT RIGHT (Make space)
      // The model already removed the 3 merged tiles. Any remaining tile with an index 
      // >= (RackInsertionIndex - 2) was sitting 3 spaces to the right before the merge happened.
      for (int i = 0; i < _model.TilesOnRack.Count; i++)
      {
        if (i >= receipt.RackInsertionIndex - 2)
        {
          _view.MoveTileToRack(_model.TilesOnRack[i], i + 3).Forget();
        }
      }

      // INSERT (Move the newly clicked tile into its slot)
      await _view.MoveTileToRack(tile, receipt.RackInsertionIndex);

      // MERGE (Pop the 3 matching tiles)
      await _view.MergeTile(receipt.MergedTiles);

      // SHIFT LEFT (Close the gap left by the destroyed tiles)
      UniTask lastSlideTask = UniTask.CompletedTask;
      for (int i = 0; i < _model.TilesOnRack.Count; i++)
      {
        if (i >= receipt.RackInsertionIndex - 2)
        {
          // Zero Allocation trick: Only await the very last task
          if (i == _model.TilesOnRack.Count - 1)
            lastSlideTask = _view.MoveTileToRack(_model.TilesOnRack[i], i);
          else
            _view.MoveTileToRack(_model.TilesOnRack[i], i).Forget();
        }
      }
      await lastSlideTask;

      // update score with tile so far model
    }
    else
    {
      // NO MERGE: Just shift the necessary tiles right and insert the new one.
      // The new tile is at RackInsertionIndex in the model, so we shift everything after it right by 1.
      for (int i = receipt.RackInsertionIndex + 1; i < _model.TilesOnRack.Count; i++)
      {
        _view.MoveTileToRack(_model.TilesOnRack[i], i).Forget();
      }

      // Wait for the new tile to slot into place
      await _view.MoveTileToRack(tile, receipt.RackInsertionIndex);
    }

    // End game states
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