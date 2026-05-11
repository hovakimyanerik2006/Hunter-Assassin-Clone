using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hunter Assassin-style contact: overlapping an <see cref="Enemy"/> kills that enemy.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerHitsEnemies : MonoBehaviour
{
    [SerializeField] private float touchRadiusBonus = 0.5f;
    [SerializeField] private float ignoreContactsForSecondsOnStart = 0.15f;

    private NavMeshAgent agent;
    private float ignoreContactsUntilTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ignoreContactsUntilTime = Time.time + Mathf.Max(0f, ignoreContactsForSecondsOnStart);
    }

    private void FixedUpdate()
    {
        if (Time.time < ignoreContactsUntilTime)
        {
            return;
        }

        float r = agent.radius + touchRadiusBonus;
        Collider[] hits = Physics.OverlapSphere(transform.position, r, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider c in hits)
        {
            if (c == null)
            {
                continue;
            }

            if (c.transform == transform || c.transform.IsChildOf(transform))
            {
                continue;
            }

            Enemy enemy = c.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.Kill();
            }
        }
    }
}
