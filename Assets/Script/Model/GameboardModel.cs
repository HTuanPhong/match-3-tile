using System;
using System.Collections.Generic;
using UnityEngine;
public class GameboardModel
{
  public int TileSoFar { get; private set; }
  public GameboardData GameboardData { get; private set; }
  public List<TileData> TilesOnBoard { get; private set; }
  public List<TileData> TilesOnRack { get; private set; }

  public event Action<TileData> OnTileIllegalSelected;
  public event Action<TileData> OnTileRemovedFromBoard;
  public event Action<TileData> OnTileRevealed;
  public event Action<TileData, TileData> OnTileAddedToRack;
  public event Action<List<TileData>> OnMatchCleared;
  public event Action<bool> OnGameEnded; // true = Win, false = Loss

  public GameboardModel(GameboardData data)
  {
    TileSoFar = 0;
    GameboardData = data;
    TilesOnBoard = new List<TileData>(data.Tiles);
    TilesOnRack = new List<TileData>(data.RackSize);
  }

  public void SelectTile(TileData tile)
  {
    if (IsLost())
    {
      OnTileIllegalSelected?.Invoke(tile);
      return;
    }
    if (IsTileOverlapped(tile))
    {
      OnTileIllegalSelected?.Invoke(tile);
      return;
    }

    if (!TryRemoveFromBoard(tile))
    {
      OnTileIllegalSelected?.Invoke(tile);
      return;
    }

    OnTileRemovedFromBoard?.Invoke(tile);

    List<TileData> revealed = GetTilesRevealedBy(tile);
    foreach (TileData unblockedTile in revealed)
    {
      OnTileRevealed?.Invoke(unblockedTile);
    }

    OnTileAddedToRack?.Invoke(tile, AddToRack(tile));

    if (TryMerge(tile, out List<TileData> matchedTiles))
    {
      TileSoFar += matchedTiles.Count;
      OnMatchCleared?.Invoke(matchedTiles);
    }

    // 5. Evaluate boundaries state rules
    if (IsWon())
    {
      OnGameEnded?.Invoke(true);
    }
    else if (IsLost())
    {
      OnGameEnded?.Invoke(false);
    }
  }

  public bool IsTileOverlapped(TileData tile)
  {
    foreach (TileData other in TilesOnBoard)
    {
      if (other.Z <= tile.Z) continue;
      if (DoTilesIntersect(tile, other)) return true;
    }
    return false;
  }

  private bool DoTilesIntersect(TileData a, TileData b)
  {
    return b.X < (a.X + 1) &&
           (b.X + 1) > a.X &&
           b.Y < (a.Y + 1) &&
           (b.Y + 1) > a.Y;
  }


  private bool IsWon()
  {
    return TileSoFar >= GameboardData.TileToWin;
  }

  private bool IsLost()
  {
    return TilesOnRack.Count >= GameboardData.RackSize;
  }

  private bool TryRemoveFromBoard(TileData tile)
  {
    return TilesOnBoard.Remove(tile);
  }

  private List<TileData> GetTilesRevealedBy(TileData tile)
  {
    var revealedTiles = new List<TileData>();
    int underZ = tile.Z - 1;
    foreach (TileData other in TilesOnBoard)
    {
      if (other.Z >= tile.Z) continue;
      if (!IsTileOverlapped(other))
      {
        revealedTiles.Add(other);
      }
    }
    return revealedTiles;
  }

  // Returns the tile in back (the one immediately after the inserted tile). 
  // If there is no tile behind it, returns null.
  private TileData AddToRack(TileData tile)
  {
    for (int i = TilesOnRack.Count - 1; i >= 0; i--)
    {
      if (TilesOnRack[i].Type == tile.Type)
      {
        int insertIndex = i + 1;
        TilesOnRack.Insert(insertIndex, tile);

        if (insertIndex + 1 < TilesOnRack.Count)
        {
          return TilesOnRack[insertIndex + 1];
        }
        else
        {
          return null;
        }
      }
    }
    TilesOnRack.Add(tile);
    return null;
  }

  private bool TryMerge(TileData tile, out List<TileData> matchedTiles)
  {
    matchedTiles = null;

    // Stop the loop 2 elements early so i + 1 and i + 2 never overshoot the list
    for (int i = 0; i < TilesOnRack.Count - 2; i++)
    {
      if (TilesOnRack[i].Type == tile.Type
       && TilesOnRack[i + 1].Type == tile.Type
       && TilesOnRack[i + 2].Type == tile.Type)
      {
        matchedTiles = new List<TileData>
            {
                TilesOnRack[i],
                TilesOnRack[i + 1],
                TilesOnRack[i + 2]
            };

        TilesOnRack.RemoveRange(i, 3);
        return true;
      }
    }
    return false;
  }
}