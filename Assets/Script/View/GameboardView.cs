using System.Collections.Generic;
using UnityEngine;

public class GameboardView : MonoBehaviour
{
  [SerializeField] private TileView _tilePrefab;

  private Dictionary<TileData, TileView> _tileMap;

  public void CreateTileOnBoard(TileData tile)
  {
    // todo use object pool
    TileView tileObject = Instantiate(_tilePrefab, new Vector3(tile.X, tile.Y, tile.Z), _tilePrefab.transform.rotation);
    // todo use addressable
    tileObject.IconRenderer.sprite = Resources.Load<Sprite>($"Images/Tiles/{tile.Type}");
  }

  public void CreateTileOnRack(TileData tile, int index)
  {
    
  }

  public void ShakeTile(TileData tile)
  {

  }

  public void LightenTile(TileData tile)
  {

  }

  public void DarkenTile(TileData tile)
  {

  }

  public void MoveTileToRack(TileData tile, int index)
  {

  }

  public void MergeTile(List<TileData> tiles)
  {

  }

  public void DisableBoard()
  {

  }

  public void EnableBoard()
  {

  }

  public void LoosePanel()
  {

  }

  public void WinPanel()
  {

  }
}