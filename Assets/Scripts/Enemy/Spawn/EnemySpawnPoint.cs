using System.Collections;
using UnityEngine;

/// <summary>
/// Component quản lý Spawn Point cho Enemy
/// Xử lý spawn, respawn với timer, và player proximity check
/// </summary>
public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab để spawn")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Tooltip("Thời gian chờ trước khi respawn enemy (giây)")]
    [SerializeField] private float respawnDelay = 5f;
    
    [Tooltip("Bán kính kiểm tra Player. Nếu Player trong vùng này, timer sẽ tạm dừng")]
    [SerializeField] private float blockRespawnRadius = 3f;
    
    [Header("Detection Settings")]
    [Tooltip("Layer của Player để phát hiện")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Debug")]
    [Tooltip("Hiển thị gizmos trong Scene view")]
    [SerializeField] private bool showGizmos = true;
    
    [Tooltip("Màu gizmo cho spawn point")]
    [SerializeField] private Color gizmoColor = Color.red;
    
    [Tooltip("Màu gizmo cho block radius")]
    [SerializeField] private Color blockRadiusColor = Color.yellow;

    private GameObject currentEnemy;
    private Coroutine respawnCoroutine;
    private float respawnTimer;
    private bool isRespawnPaused = false;

    void Start()
    {
        // Kiểm tra EnemyPool
        if (EnemyPool.Instance == null)
        {
            Debug.LogError($"EnemySpawnPoint [{gameObject.name}]: EnemyPool.Instance không tồn tại! Vui lòng tạo GameObject với EnemyPool component.");
            return;
        }

        // Kiểm tra enemy prefab
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawnPoint [{gameObject.name}]: Enemy Prefab chưa được gán!");
            return;
        }

        // Khởi tạo pool cho prefab này
        EnemyPool.Instance.InitializePool(enemyPrefab);

        // Spawn enemy ban đầu
        SpawnEnemy();
    }

    void Update()
    {
        // Kiểm tra player proximity khi đang đợi respawn
        if (respawnCoroutine != null && currentEnemy == null)
        {
            CheckPlayerProximity();
        }
    }

    /// <summary>
    /// Spawn enemy từ pool
    /// </summary>
    private void SpawnEnemy()
    {
        if (EnemyPool.Instance == null || enemyPrefab == null)
        {
            return;
        }

        // Lấy enemy từ pool
        GameObject enemy = EnemyPool.Instance.GetEnemy(enemyPrefab);
        if (enemy == null)
        {
            Debug.LogError($"EnemySpawnPoint [{gameObject.name}]: Không thể lấy enemy từ pool!");
            return;
        }

        // Đặt vị trí
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;
        enemy.transform.SetParent(null);

        // Gắn EnemySpawnedHandler nếu chưa có
        EnemySpawnedHandler handler = enemy.GetComponent<EnemySpawnedHandler>();
        if (handler == null)
        {
            handler = enemy.AddComponent<EnemySpawnedHandler>();
        }
        handler.SetSpawnPoint(this);

        currentEnemy = enemy;
    }

    /// <summary>
    /// Được gọi khi enemy chết
    /// </summary>
    public void OnEnemyDeath(GameObject deadEnemy)
    {
        if (deadEnemy != currentEnemy)
        {
            return;
        }

        // Return enemy về pool
        ReturnEnemyToPool();

        // Bắt đầu respawn timer
        StartRespawnTimer();
    }

    /// <summary>
    /// Trả enemy về pool
    /// </summary>
    private void ReturnEnemyToPool()
    {
        if (currentEnemy != null && EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemyToPool(currentEnemy, enemyPrefab);
            currentEnemy = null;
        }
    }

    /// <summary>
    /// Bắt đầu đếm thời gian respawn
    /// </summary>
    private void StartRespawnTimer()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnTimer = 0f;
        isRespawnPaused = false;
        respawnCoroutine = StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Coroutine xử lý respawn với player proximity check
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        while (respawnTimer < respawnDelay)
        {
            // Chỉ đếm timer nếu không bị pause
            if (!isRespawnPaused)
            {
                respawnTimer += Time.deltaTime;
            }

            yield return null;
        }

        // Kiểm tra player một lần nữa trước khi spawn
        if (!IsPlayerInBlockRadius())
        {
            SpawnEnemy();
        }
        else
        {
            // Nếu player vẫn ở gần, đợi thêm một chút
            yield return new WaitForSeconds(0.5f);
            StartRespawnTimer(); // Restart timer
            yield break;
        }

        respawnCoroutine = null;
    }

    /// <summary>
    /// Kiểm tra Player có trong block radius không
    /// </summary>
    private bool IsPlayerInBlockRadius()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, blockRespawnRadius, playerLayer);
        return playerCollider != null;
    }

    /// <summary>
    /// Kiểm tra và cập nhật trạng thái pause của respawn timer
    /// </summary>
    private void CheckPlayerProximity()
    {
        bool playerInRadius = IsPlayerInBlockRadius();

        if (playerInRadius && !isRespawnPaused)
        {
            // Player vừa vào vùng block, pause timer
            isRespawnPaused = true;
        }
        else if (!playerInRadius && isRespawnPaused)
        {
            // Player vừa rời khỏi vùng block, tiếp tục timer
            isRespawnPaused = false;
        }
    }

    /// <summary>
    /// Vẽ gizmos trong Scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Vẽ spawn point
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "sv_icon_dot3_pix16_gizmo", true);

        // Vẽ block radius
        Gizmos.color = blockRadiusColor;
        Gizmos.DrawWireSphere(transform.position, blockRespawnRadius);
    }

    /// <summary>
    /// Vẽ gizmos luôn (không chỉ khi selected)
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Vẽ spawn point (màu nhạt hơn)
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
