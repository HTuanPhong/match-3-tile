// FILE: Assets/Script/View/GameboardView.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;

public class GameboardView : MonoBehaviour
{
  [SerializeField] private Camera _camera;
  [SerializeField] private TileView _tilePrefab;
  [SerializeField] private ParticleSystem _particlePrefab;
  [SerializeField] private InputActionReference _pointAction;
  [SerializeField] private InputActionReference _clickAction;
  [SerializeField] private Transform _rackTransform;

  private Dictionary<TileData, TileView> _tileMap;
  private List<TileView> _tileRack;
  private TileView _chosenOne;
  private int _layerMask;
  private bool _inputLock;

  // The Lambda Queue Pipeline Blueprint
  private readonly Queue<Func<UniTask>> _animationQueue = new Queue<Func<UniTask>>();
  private bool _isProcessingQueue;

  public event Action<TileData> TileSelect;

  private void Awake()
  {
    _layerMask = LayerMask.GetMask("Tiles");
    _tileMap = new Dictionary<TileData, TileView>();
    _tileRack = new List<TileView>();
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
    if (_inputLock) return;
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
        TileSelect?.Invoke(tileView.Tile);
      }
      _chosenOne = null;
    }
  }

  private void EnqueueVisualStep(Func<UniTask> animTask)
  {
    _animationQueue.Enqueue(animTask);
    if (!_isProcessingQueue)
    {
      ProcessQueueAsync().Forget();
    }
  }

  private async UniTaskVoid ProcessQueueAsync()
  {
    _isProcessingQueue = true;
    while (_animationQueue.Count > 0)
    {
      Func<UniTask> visualStep = _animationQueue.Dequeue();
      await visualStep();
    }
    _isProcessingQueue = false;
  }

  // --- API Methods Called Directly By Controller Event Mapping ---

  public void OnTileShake(TileData tile)
  {
    ShakeTile(tile).Forget();
  }

  public void OnTileSelectFeedback(TileData tile)
  {
    PlayTileSelectSound();
  }

  public void OnTileLighten(TileData tile)
  {
    LightenTile(tile).Forget();
  }

  public void QueueTileMovement(TileData tile, TileData frontTile)
  {
    // Executes instantly, bypassing the animation queue
    MoveTileToRack(tile, frontTile).Forget();
  }

  public void QueueMatchClear(List<TileData> tiles)
  {
    // Puts the merge sequence into the queue to wait its turn
    EnqueueVisualStep(async () =>
    {
      PlayMergeSound();
      await MergeTile(tiles);
    });
  }

  public void QueueGameEnd(bool isWin)
  {
    EnqueueVisualStep(() =>
    {
      DisableBoard();
      if (isWin) WinPanel(); else LoosePanel();
      return UniTask.CompletedTask;
    });
  }

  // --- Base Intermediary Direct Layout Methods ---

  public void CreateBoard(IReadOnlyList<TileData> board, IReadOnlyList<TileData> rack, Func<TileData, bool> isTileOverlapped)
  {
    DisableBoard();
    PlayMusic();
    foreach (TileData tile in board) { CreateTileOnBoard(tile).Forget(); }
    foreach (TileData tile in rack) { CreateTileOnRack(tile); }
    foreach (TileData tile in board)
    {
      if (isTileOverlapped(tile)) DarkenTile(tile).Forget();
    }
    EnableBoard();
  }

  private UniTask CreateTileOnBoard(TileData tile)
  {
    TileView tileView = Instantiate(_tilePrefab, new Vector3(tile.X, tile.Y, -tile.Z + tile.Y / 10), _tilePrefab.transform.rotation);
    tileView.Tile = tile;
    _tileMap.Add(tile, tileView);
    string address = $"Assets/Images/Tiles/{tile.Type}.png";
    Addressables.LoadAssetAsync<Sprite>(address).Completed += (handle) =>
    {
      if (handle.Status == AsyncOperationStatus.Succeeded) tileView.IconRenderer.sprite = handle.Result;
    };
    return tileView.Spawn();
  }

  private void CreateTileOnRack(TileData tile) { }
  private UniTask ShakeTile(TileData tile) => _tileMap.TryGetValue(tile, out var tv) ? tv.Shake() : UniTask.CompletedTask;
  private UniTask LightenTile(TileData tile) => _tileMap.TryGetValue(tile, out var tv) ? tv.Lighten() : UniTask.CompletedTask;
  private UniTask DarkenTile(TileData tile) => _tileMap.TryGetValue(tile, out var tv) ? tv.Darken() : UniTask.CompletedTask;

  private UniTask MoveTileToRack(TileData tile, TileData backTile)
  {
    if (!_tileMap.TryGetValue(tile, out TileView tv)) return UniTask.CompletedTask;

    if (_tileRack.Count == 0)
    {
      _tileRack.Add(tv);
      return tv.MoveTo(_rackTransform.position);
    }

    if (backTile == null)
    {
      Vector3 pos = _rackTransform.position;
      pos.x += _tileRack.Count;
      _tileRack.Add(tv);
      return tv.MoveTo(pos);
    }

    for (int i = 0; i < _tileRack.Count; i++)
    {
      if (_tileRack[i].Tile == backTile)
      {
        List<UniTask> taskList = new List<UniTask>();

        // Shift all existing tiles to the right
        for (int j = i; j < _tileRack.Count; j++)
        {
          TileView other = _tileRack[j];
          Vector3 pos = _rackTransform.position;
          pos.x += j + 1; // Fixed mathematical offset
          taskList.Add(other.MoveTo(pos));
        }

        _tileRack.Insert(i, tv);

        // Slot the new tile exactly into its spot
        Vector3 targetPos = _rackTransform.position;
        targetPos.x += i;
        taskList.Add(tv.MoveTo(targetPos));

        return UniTask.WhenAll(taskList);
      }
    }
    return UniTask.CompletedTask;
  }

  private async UniTask MergeTile(List<TileData> tiles)
  {
    foreach (TileData tile in tiles)
    {
      if (_tileMap.TryGetValue(tile, out TileView tv))
      {
        await UniTask.WaitWhile(() => tv.Moving);
      }
    }

    List<UniTask> dieTasks = new List<UniTask>();
    List<ParticleSystem> a = new List<ParticleSystem>();
    // 1. Start the pop/die animations, but keep them in the rack logically for now
    foreach (TileData tile in tiles)
    {
      if (_tileMap.TryGetValue(tile, out TileView tv))
      {
        // todo pool this stuff
        a.Add(Instantiate(_particlePrefab, tv.transform.position, _particlePrefab.transform.rotation));
        dieTasks.Add(tv.Die());
      }
    }

    // 2. WAIT here for the matching tiles to finish dying
    await UniTask.WhenAll(dieTasks);

    // 3. NOW remove them from the visual rack
    foreach (TileData tile in tiles)
    {
      if (_tileMap.TryGetValue(tile, out TileView tv))
      {
        _tileRack.Remove(tv);
      }
    }

    // 4. Slide the remaining right-side tiles back left into the newly cleared gaps
    List<UniTask> slideTasks = new List<UniTask>();
    for (int i = 0; i < _tileRack.Count; i++)
    {
      Vector3 pos = _rackTransform.position;
      pos.x += i;

      if (_tileRack[i].transform.position != pos)
      {
        slideTasks.Add(_tileRack[i].MoveTo(pos));
      }
    }

    await UniTask.WhenAll(slideTasks);
    foreach (var item in a)
    {
      Destroy(item);
    }
  }

  private void DisableBoard() => _inputLock = true;
  private void EnableBoard() => _inputLock = false;
  private void LoosePanel() { }
  private void WinPanel() { }
  private void PlayTileSelectSound() => GlobalServiceManager.Instance.AudioService.PlayEffect("Assets/Audio/tap.ogg");
  private void PlayMergeSound() => GlobalServiceManager.Instance.AudioService.PlayEffect("Assets/Audio/match.ogg");
  private void PlayMusic() => GlobalServiceManager.Instance.AudioService.PlayEffect("Assets/Audio/bg_music.ogg");

}