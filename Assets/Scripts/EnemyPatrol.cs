using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Enemy))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float minIdleSeconds = 0.4f;
    [SerializeField] private float maxIdleSeconds = 1.6f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private bool keepFixedRotation = true;

    private NavMeshAgent agent;
    private Quaternion initialRotation;
    private int patrolIndex;
    private float idleUntil;
    private bool waitingAtPoint;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        initialRotation = transform.rotation;
        ApplyAgentSettings();
    }

    private void Start()
    {
        EnsureOnNavMesh();
        TrySetNextDestination();
    }

    private void Update()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            EnsureOnNavMesh();
            return;
        }

        if (keepFixedRotation)
        {
            transform.rotation = initialRotation;
        }

        if (waitingAtPoint)
        {
            if (Time.time >= idleUntil)
            {
                waitingAtPoint = false;
                agent.isStopped = false;
                TrySetNextDestination();
            }

            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        if (!agent.hasPath)
        {
            TrySetNextDestination();
            return;
        }

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            return;
        }

        waitingAtPoint = true;
        agent.isStopped = true;
        idleUntil = Time.time + Random.Range(minIdleSeconds, maxIdleSeconds);
    }

    private void TrySetNextDestination()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform pt = patrolPoints[patrolIndex];
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            if (pt != null && NavMesh.SamplePosition(pt.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            return;
        }

        for (int i = 0; i < 12; i++)
        {
            Vector3 offset = Random.insideUnitSphere * wanderRadius;
            offset.y = 0f;
            Vector3 candidate = transform.position + offset;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    private void EnsureOnNavMesh()
    {
        if (agent == null || !agent.enabled || agent.isOnNavMesh)
        {
            return;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void ApplyAgentSettings()
    {
        agent.speed = Mathf.Max(0.1f, moveSpeed);
        agent.acceleration = Mathf.Max(0.1f, acceleration);
        agent.updateRotation = false;
        agent.stoppingDistance = 0.15f;
    }

    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            ApplyAgentSettings();
        }
    }
}
