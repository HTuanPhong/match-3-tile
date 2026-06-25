using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private Camera _mainCamera;
    private SpriteRenderer _spriteRenderer;

    // These store the previous frame's data to detect changes
    private float _lastOrthoSize;
    private float _lastAspect;

    private void Start()
    {
        _mainCamera = Camera.main;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Do an initial fit right when the game starts
        FitToScreen();
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        // Check if the camera's zoom (ortho size) or screen shape (aspect) changed
        if (_mainCamera.orthographicSize != _lastOrthoSize || _mainCamera.aspect != _lastAspect)
        {
            FitToScreen();
        }
    }

    private void FitToScreen()
    {
        if (_spriteRenderer.sprite == null || _mainCamera == null || !_mainCamera.orthographic) return;

        // Save the new camera values so we stop checking until they change again
        _lastOrthoSize = _mainCamera.orthographicSize;
        _lastAspect = _mainCamera.aspect;

        // Get the Camera's current width and height in world space
        float screenHeight = _mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * _mainCamera.aspect;

        // Get the Sprite's raw, unscaled width and height
        float spriteHeight = _spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = _spriteRenderer.sprite.bounds.size.x;

        // Calculate the scale required to fit
        float scaleY = screenHeight / spriteHeight;
        float scaleX = screenWidth / spriteWidth;

        // Apply the Envelope scale (cover the whole screen without stretching or black bars)
        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}