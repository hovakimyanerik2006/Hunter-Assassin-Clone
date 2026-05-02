using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Stops the player when <see cref="Die"/> is called. Scene reload is off by default; enable in Inspector if needed.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private bool reloadSceneOnDeath;
    [SerializeField] private float reloadDelaySeconds = 0.35f;

    private NavMeshAgent agent;
    private bool dead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Die()
    {
        if (dead)
        {
            return;
        }

        dead = true;
        agent.isStopped = true;
        agent.enabled = false;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        if (reloadSceneOnDeath)
        {
            StartCoroutine(ReloadAfterDelay());
        }
    }

    private IEnumerator ReloadAfterDelay()
    {
        if (reloadDelaySeconds > 0f)
        {
            yield return new WaitForSeconds(reloadDelaySeconds);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
