using UnityEngine;

/// <summary>
/// Component gắn vào Enemy prefab để track spawn point và return về pool khi chết
/// Component này sẽ được add tự động bởi EnemySpawnPoint khi spawn enemy
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemySpawnedHandler : MonoBehaviour
{
    private EnemySpawnPoint spawnPoint;
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    /// <summary>
    /// Gắn spawn point cho enemy này
    /// </summary>
    public void SetSpawnPoint(EnemySpawnPoint spawnPoint)
    {
        this.spawnPoint = spawnPoint;
        
        // Subscribe death event
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }
    }

    /// <summary>
    /// Xử lý khi enemy chết
    /// </summary>
    private void OnEnemyDeath()
    {
        if (spawnPoint != null)
        {
            spawnPoint.OnEnemyDeath(gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe event
        if (health != null)
        {
            health.OnDeath -= OnEnemyDeath;
        }
    }
}
