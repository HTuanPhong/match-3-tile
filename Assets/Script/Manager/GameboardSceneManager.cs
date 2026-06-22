using System;
using UnityEngine;

public class GameboardSceneManager : MonoBehaviour
{
  private GameboardModel _model;
  [SerializeField] private GameboardView _view;
  private GameboardController _controller;
  private void Start()
  {
    int currentLevel = GameManager.Instance.GameStateModel.CurrentLevel;
    string json = GameManager.Instance.LocalIOService.ReadJson($"levels/level_{currentLevel}.json");
    _model = new GameboardModel(GameboardData.FromJson(json));
    _controller = new GameboardController(_model, _view);
  }
}