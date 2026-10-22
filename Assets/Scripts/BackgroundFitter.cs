using UnityEngine;

/// <summary>
/// Scales a background sprite up so it always covers the whole camera
/// view, whatever the screen's aspect ratio. It never shrinks the sprite
/// below the scale it was given in the scene.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    // The camera whose view must be covered. Uses the main camera if left empty.
    public Camera targetCamera;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    // Runs every frame so resizing the window is handled too.
    void LateUpdate()
    {
        if (targetCamera == null || spriteRenderer.sprite == null) return;

        // Half the size of what the camera can see, in world units.
        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;

        // Account for the background being off-center from the camera.
        Vector2 offset = transform.position - targetCamera.transform.position;
        float neededWidth = 2f * (halfWidth + Mathf.Abs(offset.x));
        float neededHeight = 2f * (halfHeight + Mathf.Abs(offset.y));

        // Unscaled sprite size, in world units.
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float coverScale = Mathf.Max(neededWidth / spriteSize.x, neededHeight / spriteSize.y);

        float scale = Mathf.Max(coverScale, baseScale.x);
        transform.localScale = new Vector3(scale, scale, baseScale.z);
    }
}
