using UnityEngine;

/// <summary>
/// Hostile unit. <see cref="PlayerHitsEnemies"/> destroys it on player contact.
/// </summary>
public class Enemy : MonoBehaviour
{
    public void Kill()
    {
        Destroy(gameObject);
    }
}
