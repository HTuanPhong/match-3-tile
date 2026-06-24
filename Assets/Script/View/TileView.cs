using UnityEngine;

public class TileView : MonoBehaviour
{
  [field: SerializeField] public SpriteRenderer IconRenderer { get; private set; }
  [field: SerializeField] public SpriteRenderer BaseRenderer { get; private set; }
  [HideInInspector] public TileData Tile;
}