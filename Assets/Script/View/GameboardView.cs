using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameboardView : MonoBehaviour
{
  [SerializeField] private Camera _camera;
  [SerializeField] private TileView _tilePrefab;
  [SerializeField] private InputActionReference _pointAction;
  [SerializeField] private InputActionReference _clickAction;
  [SerializeField] private Transform[] _rackTransforms;
  private Dictionary<TileData, TileView> _tileMap;
  private TileView _chosenOne;
  private int _layerMask;

  public event Action<TileData> TileSelect;

  private void Awake()
  {
    _layerMask = LayerMask.GetMask("Tiles");
    _tileMap = new Dictionary<TileData, TileView>();
  }

  private void OnEnable()
  {
    _pointAction.action.Enable();
    _clickAction.action.Enable();
    _clickAction.action.performed += OnClickChanged;
  }

  private void OnDisable()
  {
    _clickAction.action.performed -= OnClickChanged;
    _clickAction.action.Disable();
    _pointAction.action.Disable();
  }

  private void OnClickChanged(InputAction.CallbackContext context)
  {
    bool isPressed = context.ReadValueAsButton();
    Vector2 pos = _pointAction.action.ReadValue<Vector2>();
    Ray ray = _camera.ScreenPointToRay(pos);
    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, _layerMask);
    if (hit.collider == null)
    {
      _chosenOne = null;
      return;
    }
    TileView tileView = hit.collider.GetComponent<TileView>();
    if (tileView == null)
    {
      _chosenOne = null;
      return;
    }
    if (isPressed)
    {
      _chosenOne = tileView;
    }
    else
    {
      if (tileView == _chosenOne)
      {
        TileSelect.Invoke(tileView.Tile);
      }
      _chosenOne = null;
    }
  }

  public UniTask CreateTileOnBoard(TileData tile)
  {
    // todo use object pool
    TileView tileView = Instantiate(_tilePrefab, new Vector3(tile.X, tile.Y, -tile.Z + tile.Y / 10), _tilePrefab.transform.rotation);
    tileView.Tile = tile;
    _tileMap.Add(tile, tileView);
    string address = $"Assets/Images/Tiles/{tile.Type}.png";
    Addressables.LoadAssetAsync<Sprite>(address).Completed += (AsyncOperationHandle<Sprite> handle) =>
    {
      if (handle.Status == AsyncOperationStatus.Succeeded)
      {
        // Assign the loaded sprite to the IconRenderer
        tileView.IconRenderer.sprite = handle.Result;
      }
      else
      {
        Debug.LogError($"Failed to load Addressable sprite: {address}");
      }
    };
    return tileView.Spawn();
  }

  public void CreateTileOnRack(TileData tile, int index)
  {

  }

  public UniTask ShakeTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    return tv.Shake();
  }

  public UniTask LightenTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    return tv.Lighten();
  }

  public UniTask DarkenTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    return tv.Darken();
  }

  public UniTask MoveTileToRack(TileData tile, int index)
  {
    TileView tv = _tileMap[tile];
    return tv.MoveTo(_rackTransforms[index].position);
  }

  public UniTask MergeTile(List<TileData> tiles)
  {
    List<UniTask> mergeTasks = new List<UniTask>();

    foreach (TileData tile in tiles)
    {
      TileView tv = _tileMap[tile];
      // Add to our tracker so we wait for all tiles simultaneously
      mergeTasks.Add(tv.Die());
    }

    // Wait until ALL tiles have finished their fancy destroy animations
    return UniTask.WhenAll(mergeTasks);
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