using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask clickMask = ~0;
    [SerializeField] private float maxRayDistance = 500f;
    [SerializeField] private Collider fieldBoundsCollider;
    [SerializeField] private float navMeshSampleDistance = 1.5f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float moveAcceleration = 20f;
    [SerializeField] private bool requireCompletePath = true;
    [SerializeField] private bool keepFixedRotation = true;

    private NavMeshAgent agent;
    private Quaternion initialRotation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCamera ??= Camera.main;

        initialRotation = transform.rotation;
        ApplyAgentSettings();
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryMoveToMouseClick(Mouse.current.position.ReadValue());
        }
    }

    private void LateUpdate()
    {
        if (keepFixedRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    private void TryMoveToMouseClick(Vector2 mouseScreenPosition)
    {
        if (mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, clickMask, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
        {
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (fieldBoundsCollider != null && !IsInsideFieldBounds(hit.point))
            {
                continue;
            }

            if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                continue;
            }

            if (requireCompletePath && !HasCompletePath(navHit.position))
            {
                continue;
            }

            agent.SetDestination(navHit.position);
            return;
        }
    }

    private bool IsInsideFieldBounds(Vector3 worldPoint)
    {
        Bounds bounds = fieldBoundsCollider.bounds;
        Vector3 pointOnBoundsY = new Vector3(worldPoint.x, bounds.center.y, worldPoint.z);
        return bounds.Contains(pointOnBoundsY);
    }

    private bool HasCompletePath(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(destination, path))
        {
            return false;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void ApplyAgentSettings()
    {
        agent.speed = Mathf.Max(0.1f, moveSpeed);
        agent.acceleration = Mathf.Max(0.1f, moveAcceleration);
        agent.updateRotation = false;
    }

    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
        ApplyAgentSettings();
    }
}
