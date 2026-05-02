using UnityEngine;

/// <summary>
/// Top-down friendly: when the player is within range on XZ, lerps the gate to an open pose.
/// Put this on the gate root; assign <see cref="movingPart"/> to the mesh that should move (often a child).
/// </summary>
public class ProximityGate : MonoBehaviour
{
    public enum OpenStyle
    {
        SlideLocal,
        SwingLocalY,
        ScaleToZero,


    }

    [SerializeField] private Transform movingPart;
    [SerializeField] private Transform player;
    [SerializeField] private Transform proximityPoint;
    [SerializeField] private float openDistance = 3f;
    [SerializeField] private float smoothSpeed = 6f;
    [SerializeField] private OpenStyle style = OpenStyle.ScaleToZero;
    [SerializeField] private Vector3 openSlideLocalOffset = new Vector3(0f, 0f, -2.5f);
    [SerializeField] private float openSwingLocalYDegrees = 90f;
    [SerializeField] private float minOpenScale = 0.02f;

    private Vector3 closedLocalPosition;
    private Quaternion closedLocalRotation;
    private Vector3 closedLocalScale;

    private void Awake()
    {
        if (movingPart == null)
        {
            movingPart = transform;
        }

        closedLocalPosition = movingPart.localPosition;
        closedLocalRotation = movingPart.localRotation;
        closedLocalScale = movingPart.localScale;

        if (player == null)
        {
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                player = pm.transform;
            }
        }

        if (proximityPoint == null)
        {
            proximityPoint = transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 anchor = proximityPoint.position;
        Vector2 a = new Vector2(anchor.x, anchor.z);
        Vector2 b = new Vector2(player.position.x, player.position.z);
        bool open = Vector2.Distance(a, b) <= openDistance;
        float t = Time.deltaTime * smoothSpeed;

        if (style == OpenStyle.SlideLocal)
        {
            Vector3 targetPos = open ? closedLocalPosition + openSlideLocalOffset : closedLocalPosition;
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, targetPos, t);
        }
        else if (style == OpenStyle.SwingLocalY)
        {
            Quaternion targetRot = open
                ? closedLocalRotation * Quaternion.Euler(0f, openSwingLocalYDegrees, 0f)
                : closedLocalRotation;
            movingPart.localRotation = Quaternion.Slerp(movingPart.localRotation, targetRot, t);
        }
        else
        {
            float clampedMin = Mathf.Clamp01(minOpenScale);
            Vector3 targetScale = open ? closedLocalScale * clampedMin : closedLocalScale;
            movingPart.localScale = Vector3.Lerp(movingPart.localScale, targetScale, t);
        }
    }
}
