using System.Collections.Generic;

public struct TileSelectReceipt
{
  public bool IsMoveLegal;
  public List<TileData> RevealedTiles;
  public int RackInsertionIndex;
  public bool DidMergeOccur;
  public List<TileData> MergedTiles;
  public bool GameIsWon;
  public bool GameIsLost;
}

public class GameboardModel
{
  public int TileSoFar { get; private set; }
  public GameboardData GameboardData { get; private set; }
  public List<TileData> TilesOnBoard { get; private set; }
  public List<TileData> TilesOnRack { get; private set; }

  public GameboardModel(GameboardData data)
  {
    TileSoFar = 0;
    GameboardData = data;
    TilesOnBoard = new List<TileData>(data.Tiles);
    TilesOnRack = new List<TileData>();
  }

  public TileSelectReceipt SelectTile(TileData tile)
  {
    var receipt = new TileSelectReceipt();
    if (IsTileOverlapped(tile))
    {
      receipt.IsMoveLegal = false;
      return receipt;
    }
    if (!TryRemoveFromBoard(tile))
    {
      receipt.IsMoveLegal = false;
      return receipt;
    }
    receipt.IsMoveLegal = true;
    receipt.RevealedTiles = GetTilesOverlappedBy(tile);
    receipt.RackInsertionIndex = AddToRack(tile);
    receipt.DidMergeOccur = TryMerge(tile, out List<TileData> matchedTiles);
    receipt.MergedTiles = matchedTiles;
    receipt.GameIsLost = IsLost();
    receipt.GameIsWon = IsWon();
    return receipt;
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

  private List<TileData> GetTilesOverlappedBy(TileData tile)
  {
    var overlappedTiles = new List<TileData>();
    int underZ = tile.Z - 1;
    foreach (TileData other in TilesOnBoard)
    {
      if (other.Z != underZ) continue;
      if (DoTilesIntersect(tile, other))
      {
        overlappedTiles.Add(other);
      }
    }
    return overlappedTiles;
  }

  // return insertion index
  private int AddToRack(TileData tile)
  {
    for (int i = TilesOnRack.Count - 1; i >= 0; i--)
    {
      if (TilesOnRack[i].Type == tile.Type)
      {
        TilesOnRack.Insert(i + 1, tile);
        return i + 1;
      }
    }

    TilesOnRack.Add(tile);
    return TilesOnRack.Count - 1;
  }

  private bool TryMerge(TileData tile, out List<TileData> matchedTiles)
  {
    matchedTiles = null;
    for (int i = 0; i < TilesOnRack.Count; i++)
    {
      if (TilesOnRack[i].Type == tile.Type
       && TilesOnRack[i + 1].Type == tile.Type
       && TilesOnRack[i + 2].Type == tile.Type)
      {
        matchedTiles = new List<TileData>
        {
            TilesOnRack[i],
            TilesOnRack[i+1],
            TilesOnRack[i+2]
        };
        TilesOnRack.RemoveRange(i, 3);
        return true;
      }
    }
    return false;
  }
}