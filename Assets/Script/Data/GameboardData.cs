
using System;
using UnityEngine;

[Serializable]
public class GameboardData
{
  public int MaxWidth;
  public int MaxHeight;
  public int TileToWin;
  public int RackSize;
  public TileData[] Tiles;

  public static GameboardData FromJson(string json)
  {
    return JsonUtility.FromJson<GameboardData>(json);
  }

  public string ToJson(bool prettyPrint = false)
  {
    return JsonUtility.ToJson(this, prettyPrint);
  }
}