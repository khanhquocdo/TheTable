using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho Enemy Bullet để tối ưu performance
/// </summary>
public class EnemyBulletPool : MonoBehaviour
{
    public static EnemyBulletPool Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;
    
    [Header("Layer Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private List<GameObject> allBullets = new List<GameObject>();
    private bool isInitialized = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        if (bulletPrefab != null && !isInitialized)
        {
            InitializePool();
        }
    }
    
    /// <summary>
    /// Khởi tạo pool với số lượng bullet ban đầu
    /// </summary>
    public void InitializePool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("EnemyBulletPool: Bullet Prefab chưa được gán!");
            return;
        }
        
        if (isInitialized)
        {
            Debug.LogWarning("EnemyBulletPool: Pool đã được khởi tạo rồi!");
            return;
        }
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet();
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Tạo một bullet mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);
        poolQueue.Enqueue(bullet);
        allBullets.Add(bullet);
        return bullet;
    }
    
    /// <summary>
    /// Spawn bullet từ pool
    /// </summary>
    public void SpawnBullet(Vector2 position, Vector2 direction, float speed, float damage, float lifetime, LayerMask playerLayerMask, LayerMask obstacleLayerMask)
    {
        GameObject bullet = GetBullet();
        if (bullet == null)
        {
            return;
        }
        
        bullet.transform.position = position;
        bullet.SetActive(true);
        
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, speed, damage, lifetime, playerLayerMask, obstacleLayerMask);
        }
    }
    
    /// <summary>
    /// Spawn bullet sử dụng layer mặc định từ pool (backward compatibility)
    /// </summary>
    public void SpawnBullet(Vector2 position, Vector2 direction, float speed, float damage, float lifetime)
    {
        SpawnBullet(position, direction, speed, damage, lifetime, playerLayer, obstacleLayer);
    }
    
    /// <summary>
    /// Lấy một bullet từ pool
    /// </summary>
    private GameObject GetBullet()
    {
        GameObject bullet;
        
        if (poolQueue.Count > 0)
        {
            bullet = poolQueue.Dequeue();
        }
        else if (allBullets.Count < maxPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            bullet = CreateNewBullet();
            poolQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng bullet cũ nhất
            bullet = allBullets[0];
            allBullets.RemoveAt(0);
            allBullets.Add(bullet);
        }
        
        return bullet;
    }
    
    /// <summary>
    /// Trả bullet về pool
    /// </summary>
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null)
        {
            return;
        }
        
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        bullet.transform.position = Vector3.zero;
        bullet.transform.rotation = Quaternion.identity;
        
        // Reset Rigidbody2D
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        if (!poolQueue.Contains(bullet))
        {
            poolQueue.Enqueue(bullet);
        }
    }
    
    /// <summary>
    /// Set prefab cho pool (có thể gọi từ code khác)
    /// </summary>
    public void SetPrefab(GameObject prefab)
    {
        if (isInitialized)
        {
            Debug.LogWarning("EnemyBulletPool: Không thể thay đổi prefab sau khi pool đã được khởi tạo!");
            return;
        }
        bulletPrefab = prefab;
    }
}

