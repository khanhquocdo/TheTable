using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Object Pool cho Enemy để tối ưu performance
/// Hỗ trợ nhiều loại enemy prefab khác nhau
/// </summary>
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [Header("Pool Settings")]
    [Tooltip("Số lượng enemy ban đầu cho mỗi prefab")]
    [SerializeField] private int initialPoolSizePerPrefab = 5;
    
    [Tooltip("Số lượng enemy tối đa cho mỗi prefab")]
    [SerializeField] private int maxPoolSizePerPrefab = 30;

    // Dictionary để lưu pool cho mỗi prefab
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, List<GameObject>> allEnemiesDictionary = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, bool> initializedPools = new Dictionary<GameObject, bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Khởi tạo pool cho một enemy prefab
    /// </summary>
    public void InitializePool(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: Enemy Prefab không được null!");
            return;
        }

        if (initializedPools.ContainsKey(enemyPrefab) && initializedPools[enemyPrefab])
        {
            Debug.LogWarning($"EnemyPool: Pool cho {enemyPrefab.name} đã được khởi tạo rồi!");
            return;
        }

        // Tạo queue và list nếu chưa có
        if (!poolDictionary.ContainsKey(enemyPrefab))
        {
            poolDictionary[enemyPrefab] = new Queue<GameObject>();
            allEnemiesDictionary[enemyPrefab] = new List<GameObject>();
        }

        // Tạo các enemy ban đầu
        for (int i = 0; i < initialPoolSizePerPrefab; i++)
        {
            CreateNewEnemy(enemyPrefab);
        }

        initializedPools[enemyPrefab] = true;
    }

    /// <summary>
    /// Tạo một enemy mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewEnemy(GameObject enemyPrefab)
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);
        enemy.SetActive(false);
        poolDictionary[enemyPrefab].Enqueue(enemy);
        allEnemiesDictionary[enemyPrefab].Add(enemy);
        return enemy;
    }

    /// <summary>
    /// Lấy một enemy từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: Enemy Prefab không được null!");
            return null;
        }

        // Đảm bảo pool đã được khởi tạo
        if (!initializedPools.ContainsKey(enemyPrefab) || !initializedPools[enemyPrefab])
        {
            InitializePool(enemyPrefab);
        }

        GameObject enemy;

        if (poolDictionary[enemyPrefab].Count > 0)
        {
            enemy = poolDictionary[enemyPrefab].Dequeue();
        }
        else if (allEnemiesDictionary[enemyPrefab].Count < maxPoolSizePerPrefab)
        {
            // Tạo mới nếu chưa đạt max size
            enemy = CreateNewEnemy(enemyPrefab);
            poolDictionary[enemyPrefab].Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng enemy cũ nhất
            enemy = allEnemiesDictionary[enemyPrefab][0];
            allEnemiesDictionary[enemyPrefab].RemoveAt(0);
            allEnemiesDictionary[enemyPrefab].Add(enemy);
            
            // Reset enemy trước khi sử dụng lại
            ResetEnemy(enemy);
        }

        enemy.SetActive(true);
        
        // Enable lại các controller có thể bị disable khi enemy chết
        EnableEnemyComponents(enemy);
        
        return enemy;
    }

    /// <summary>
    /// Trả enemy về pool để tái sử dụng
    /// </summary>
    public void ReturnEnemyToPool(GameObject enemy, GameObject enemyPrefab)
    {
        if (enemy == null || enemyPrefab == null)
        {
            return;
        }

        if (!poolDictionary.ContainsKey(enemyPrefab))
        {
            Debug.LogWarning($"EnemyPool: Không tìm thấy pool cho prefab {enemyPrefab.name}");
            return;
        }

        ResetEnemy(enemy);

        if (!poolDictionary[enemyPrefab].Contains(enemy))
        {
            poolDictionary[enemyPrefab].Enqueue(enemy);
        }
    }

    /// <summary>
    /// Reset enemy về trạng thái ban đầu
    /// </summary>
    private void ResetEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        enemy.transform.SetParent(transform);
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        // Reset Rigidbody2D nếu có
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset Health nếu có
        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
        }
    }

    /// <summary>
    /// Enable lại các component của enemy (controller, animator, etc.)
    /// Được gọi khi enemy được spawn từ pool
    /// </summary>
    private void EnableEnemyComponents(GameObject enemy)
    {
        // Enable các controller có thể bị disable khi enemy chết
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = true;
        }
        
        MeleeEnemyController meleeController = enemy.GetComponent<MeleeEnemyController>();
        if (meleeController != null)
        {
            meleeController.enabled = true;
        }
        
        TankEnemyController tankController = enemy.GetComponent<TankEnemyController>();
        if (tankController != null)
        {
            tankController.enabled = true;
        }
        
        // Enable Animator nếu có
        Animator animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
        }
        
        // Enable EnemyAnimator nếu có
        MonoBehaviour enemyAnimator = enemy.GetComponent("EnemyAnimator") as MonoBehaviour;
        if (enemyAnimator != null)
        {
            enemyAnimator.enabled = true;
        }
        
        // Enable MeleeEnemyAnimator nếu có
        MonoBehaviour meleeAnimator = enemy.GetComponent("MeleeEnemyAnimator") as MonoBehaviour;
        if (meleeAnimator != null)
        {
            meleeAnimator.enabled = true;
        }
    }

    /// <summary>
    /// Kiểm tra xem pool có đủ enemy không
    /// </summary>
    public bool HasAvailableEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return false;
        
        if (!poolDictionary.ContainsKey(enemyPrefab))
        {
            return true; // Có thể tạo mới
        }

        return poolDictionary[enemyPrefab].Count > 0 || 
               allEnemiesDictionary[enemyPrefab].Count < maxPoolSizePerPrefab;
    }
}
