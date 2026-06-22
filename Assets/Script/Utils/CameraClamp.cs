using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraClamp : MonoBehaviour
{
    public float Width;
    public float Height;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        float targetSizeForHeight = Height / 2;

        float targetSizeForWidth = Width / (_camera.aspect * 2);

        _camera.orthographicSize = Mathf.Max(targetSizeForHeight, targetSizeForWidth);
    }
}
