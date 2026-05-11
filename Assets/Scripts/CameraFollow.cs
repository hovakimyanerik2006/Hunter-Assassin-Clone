using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -7f);
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private Collider fieldBoundsCollider;
    [SerializeField] private Vector2 clampPadding = Vector2.zero;
    [SerializeField] private bool autoClampOrthographicSize = true;
    [SerializeField] private float desiredOrthographicSize = 17f;
    [SerializeField] private bool keepFixedRotation = true;

    private Camera cam;
    private Quaternion initialRotation;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        initialRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        desiredPosition = ClampToFieldBounds(desiredPosition);
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        if (keepFixedRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    private Vector3 ClampToFieldBounds(Vector3 position)
    {
        if (fieldBoundsCollider == null || cam == null)
        {
            return position;
        }

        Bounds bounds = fieldBoundsCollider.bounds;
        float paddedMinX = bounds.min.x + clampPadding.x;
        float paddedMaxX = bounds.max.x - clampPadding.x;
        float paddedMinZ = bounds.min.z + clampPadding.y;
        float paddedMaxZ = bounds.max.z - clampPadding.y;

        if (paddedMinX > paddedMaxX)
        {
            float centerX = bounds.center.x;
            paddedMinX = centerX;
            paddedMaxX = centerX;
        }

        if (paddedMinZ > paddedMaxZ)
        {
            float centerZ = bounds.center.z;
            paddedMinZ = centerZ;
            paddedMaxZ = centerZ;
        }

        if (cam.orthographic)
        {
            if (autoClampOrthographicSize)
            {
                float fitByDepth = Mathf.Max(0.01f, (paddedMaxZ - paddedMinZ) * 0.5f);
                float fitByWidth = Mathf.Max(0.01f, (paddedMaxX - paddedMinX) * 0.5f / Mathf.Max(0.01f, cam.aspect));
                float maxSafeSize = Mathf.Min(fitByDepth, fitByWidth);
                cam.orthographicSize = Mathf.Min(Mathf.Max(0.01f, desiredOrthographicSize), maxSafeSize);
            }

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float minCamX = paddedMinX + halfWidth;
            float maxCamX = paddedMaxX - halfWidth;
            float minCamZ = paddedMinZ + halfHeight;
            float maxCamZ = paddedMaxZ - halfHeight;

            if (minCamX > maxCamX)
            {
                position.x = bounds.center.x;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, minCamX, maxCamX);
            }

            if (minCamZ > maxCamZ)
            {
                position.z = bounds.center.z;
            }
            else
            {
                position.z = Mathf.Clamp(position.z, minCamZ, maxCamZ);
            }

            return position;
        }

        // Perspective fallback: clamp camera pivot position.
        position.x = Mathf.Clamp(position.x, paddedMinX, paddedMaxX);
        position.z = Mathf.Clamp(position.z, paddedMinZ, paddedMaxZ);
        return position;
    }



    
}
