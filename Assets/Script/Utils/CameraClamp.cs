using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraClamp : MonoBehaviour
{
    public float Width;
    public float Height;
    private Camera _camera;

    private float _lastAspect;


    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (_camera.aspect != _lastAspect)
        {
            _lastAspect = _camera.aspect;

            float targetSizeForHeight = Height / 2;

            float targetSizeForWidth = Width / (_camera.aspect * 2);

            _camera.orthographicSize = Mathf.Max(targetSizeForHeight, targetSizeForWidth);
        }
    }
}
