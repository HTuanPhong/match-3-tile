using UnityEngine;
public class GlobalModelManager : MonoBehaviour
{
  public static GlobalModelManager Instance { get; private set; } = null;
  public GameStateModel GameStateModel;
  // add more global model here if needed
  private void Awake()
  {
    SingletonSetup();
    GameStateModel = new GameStateModel();
  }

  private void SingletonSetup()
  {
    // singleton setup
    if (Instance != null)
    {
      Debug.LogError("Another BootstrapManager on " + gameObject.name);
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }
}

