using UnityEngine;

public class PlayerData
{
  public string Name;
  public int HighestLevel;
  public int Coins;
  
  public SettingData Settings;

  public static PlayerData FromJson(string json)
  {
    return JsonUtility.FromJson<PlayerData>(json);
  }

  public string ToJson(bool prettyPrint = false)
  {
    return JsonUtility.ToJson(this, prettyPrint);
  }
}