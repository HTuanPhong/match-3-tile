using System;
using UnityEngine;

public class GameboardSceneManager : MonoBehaviour
{
  private GameboardModel _model;
  [SerializeField] private GameboardView _view;
  private GameboardController _controller;
  private GameboardLocalRepository _localRepository;

  private void Awake()
  {
    _localRepository = new GameboardLocalRepository(
                            GlobalModelManager.Instance.GameStateModel,
                            GlobalServiceManager.Instance.LocalIOService);
    _controller = new GameboardController(_localRepository, _view);
  }

  private void Start()
  {
    _controller.Start();
  }
}