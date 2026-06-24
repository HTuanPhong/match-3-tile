using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class GameboardView : MonoBehaviour
{
  [SerializeField] private Camera _camera;
  [SerializeField] private TileView _tilePrefab;
  [SerializeField] private InputActionReference _pointAction;
  [SerializeField] private InputActionReference _clickAction;
  [SerializeField] private Transform[] _rackTransforms;
  private Dictionary<TileData, TileView> _tileMap;
  private TileView _chosenOne;

  public event Action<TileData> TileSelect;

  private void Awake()
  {

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
    RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
    if (hit.collider == null)
    {
      _chosenOne = null;
      return;
    }
    Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, hit.transform.position.z - _camera.transform.position.z);
    TileView tileView = hit.collider.GetComponent<TileView>();
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

  public void CreateTileOnBoard(TileData tile)
  {
    // todo use object pool
    TileView tileView = Instantiate(_tilePrefab, new Vector3(tile.X, tile.Y, -tile.Z + tile.Y / 10), _tilePrefab.transform.rotation);
    // todo use addressable
    tileView.IconRenderer.sprite = Resources.Load<Sprite>($"Images/Tiles/{tile.Type}");
    tileView.Tile = tile;
    _tileMap.Add(tile, tileView);
  }

  public void CreateTileOnRack(TileData tile, int index)
  {

  }

  public async Task ShakeTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    Transform tileTransform = tv.transform;

    if (DOTween.IsTweening(tileTransform)) return;

    // Juice: Shake position + slight rotation + a brief red error flash
    Sequence seq = DOTween.Sequence();

    _ = seq.Append(tileTransform.DOShakePosition(duration: 0.25f, strength: new Vector3(0.1f, 0f, 0f), vibrato: 20, randomness: 0))
       .Join(tileTransform.DOShakeRotation(duration: 0.25f, strength: new Vector3(0, 0, 10f), vibrato: 15)) // Wiggle
       .Join(tv.BaseRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.125f).SetLoops(2, LoopType.Yoyo)) // Flash red and back
       .Join(tv.IconRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.125f).SetLoops(2, LoopType.Yoyo));
    await seq.ToUniTask();
  }

  public void LightenTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    // Juice: Smooth fade instead of instant snap
    tv.BaseRenderer.DOColor(Color.white, 0.15f).SetEase(Ease.OutQuad);
    tv.IconRenderer.DOColor(Color.white, 0.15f).SetEase(Ease.OutQuad);
  }

  public void DarkenTile(TileData tile)
  {
    TileView tv = _tileMap[tile];
    Color darkColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    // Juice: Smooth fade to dark
    tv.BaseRenderer.DOColor(darkColor, 0.15f).SetEase(Ease.OutQuad);
    tv.IconRenderer.DOColor(darkColor, 0.15f).SetEase(Ease.OutQuad);
  }

  public async UniTask MoveTileToRack(TileData tile, int index)
  {
    TileView tv = _tileMap[tile];
    tv.transform.DOKill(); // Prevent overlap glitches

    Sequence seq = DOTween.Sequence();

    // Juice: Move it with the snap, but also add a tiny scale "pulse" as it flies
    _ = seq.Append(tv.transform.DOMove(_rackTransforms[index].position, 10f).SetEase(Ease.OutBack, 1.1f))
             .Join(tv.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.18f, 1, 0.5f));

    await seq.ToUniTask();
  }

  public async UniTask MergeTile(List<TileData> tiles)
  {
    List<UniTask> mergeTasks = new List<UniTask>();

    foreach (TileData tile in tiles)
    {
      TileView tv = _tileMap[tile];
      Transform t = tv.transform;
      t.DOKill();

      Sequence seq = DOTween.Sequence();

      // 1. Anticipation: Scale up slightly and flash bright white
      _ = seq.Append(t.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad))
         .Join(tv.BaseRenderer.DOColor(Color.white, 0.1f)) // Ensure it's fully bright
         .Join(tv.IconRenderer.DOColor(Color.white, 0.1f));

      // 2. The Pop: Spin, suck inward to scale 0, and float up slightly
      _ = seq.Append(t.DOScale(0f, 0.2f).SetEase(Ease.InBack))
         .Join(t.DORotate(new Vector3(0, 0, 180f), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.InQuad))
         .Join(t.DOMoveY(t.position.y + 0.5f, 0.2f).SetEase(Ease.InQuad)) // Float up
         .Join(tv.BaseRenderer.DOFade(0f, 0.15f)) // Fade out alpha
         .Join(tv.IconRenderer.DOFade(0f, 0.15f));

      // 3. Cleanup: Destroy the object once the sequence finishes
      _ = seq.OnComplete(() => Destroy(tv.gameObject));

      // Add to our tracker so we wait for all tiles simultaneously
      mergeTasks.Add(seq.ToUniTask());
    }

    // Wait until ALL tiles have finished their fancy destroy animations
    await UniTask.WhenAll(mergeTasks);
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