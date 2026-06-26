using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[SelectionBase]
public class TileView : MonoBehaviour
{
  [field: SerializeField] public SpriteRenderer IconRenderer { get; private set; }
  [field: SerializeField] public SpriteRenderer BaseRenderer { get; private set; }
  public Color DarkColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  [HideInInspector] public TileData Tile;


  // ideally there should be a two channel state machine here but lazy.....
  public bool Moving { get; private set; }

  [Button()]
  public UniTask Shake()
  {
    DOTween.Kill(gameObject, true);
    // Juice: Shake position + slight rotation + a brief red error flash
    Sequence seq = DOTween.Sequence().SetTarget(gameObject);
    _ = seq.Append(transform.DOShakePosition(duration: 0.25f, strength: new Vector3(0.1f, 0f, 0f), vibrato: 20, randomness: 0))
       .Join(transform.DOShakeRotation(duration: 0.25f, strength: new Vector3(0, 0, 10f), vibrato: 15)) // Wiggle
       .Join(BaseRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.125f).SetLoops(2, LoopType.Yoyo)) // Flash red and back
       .Join(IconRenderer.DOColor(new Color(1f, 0.5f, 0.5f), 0.125f).SetLoops(2, LoopType.Yoyo));
    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }

  [Button()]
  public UniTask Lighten()
  {
    DOTween.Kill(gameObject, true);
    Sequence seq = DOTween.Sequence().SetTarget(gameObject);
    _ = seq.Join(BaseRenderer.DOColor(Color.white, 0.15f).SetEase(Ease.OutQuad))
           .Join(IconRenderer.DOColor(Color.white, 0.15f).SetEase(Ease.OutQuad));

    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }

  [Button()]
  public UniTask Darken()
  {
    DOTween.Kill(gameObject, true);
    Sequence seq = DOTween.Sequence().SetTarget(gameObject);
    _ = seq.Join(BaseRenderer.DOColor(DarkColor, 0.15f).SetEase(Ease.OutQuad))
           .Join(IconRenderer.DOColor(DarkColor, 0.15f).SetEase(Ease.OutQuad));

    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }

  public UniTask MoveTo(Vector3 newPos)
  {
    DOTween.Kill(gameObject, true);
    Sequence seq = DOTween.Sequence().SetTarget(gameObject);
    Moving = true;
    // Juice: Move it with the snap, but also add a tiny scale "pulse" as it flies
    _ = seq.Append(transform.DOMove(newPos, 0.2f).SetEase(Ease.OutBack, 1.1f))
             .Join(transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.18f, 1, 0.5f))
             .OnComplete(() => Moving = false);
    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }

#if UNITY_EDITOR
  private bool __moveFlipFlop = true;
  [Button()]
  public UniTask MoveTest()
  {
    Vector3 result = __moveFlipFlop ? new Vector3(2, 2) : new Vector3(4, 6);
    __moveFlipFlop = !__moveFlipFlop;
    return MoveTo(result);
  }
#endif

  [Button()]
  public UniTask Die()
  {
    DOTween.Kill(gameObject, true);
    Sequence seq = DOTween.Sequence();

    // Anticipation: Scale up slightly and flash bright white
    _ = seq.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad))
       .Join(BaseRenderer.DOColor(Color.white, 0.2f)) // Ensure it's fully bright
       .Join(IconRenderer.DOColor(Color.white, 0.2f));

    // The Pop: Spin, suck inward to scale 0
    _ = seq.Append(transform.DOScale(0f, 0.4f).SetEase(Ease.InBack))
       .Join(transform.DORotate(new Vector3(0, 0, 180f), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.InQuad))
       .Join(BaseRenderer.DOFade(0f, 0.3f)) // Fade out alpha
       .Join(IconRenderer.DOFade(0f, 0.3f));

    // 3. Cleanup: Destroy the object once the sequence finishes
    _ = seq.OnComplete(() => gameObject.SetActive(false));
    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }

  [Button()]
  public UniTask Spawn()
  {
    gameObject.SetActive(true);
    DOTween.Kill(gameObject, true);
    Sequence seq = DOTween.Sequence().SetTarget(gameObject);
    transform.localScale = Vector3.zero;
    transform.localRotation = Quaternion.identity;
    BaseRenderer.color = Color.white;
    IconRenderer.color = Color.white;
    // Juice: Pop open from scale 0 to 1 with an elastic spring effect
    _ = seq.Append(transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack, 1.2f))
           // Give it a slight quick spin while appearing
           .Join(transform.DORotate(new Vector3(0, 0, 15f), 0.15f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutQuad));

    return seq.ToUniTask(TweenCancelBehaviour.Complete);
  }
}