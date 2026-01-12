using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho Tank Projectile để tối ưu performance
/// </summary>
public class TankProjectilePool : MonoBehaviour
{
    public static TankProjectilePool Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
    
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private List<GameObject> allProjectiles = new List<GameObject>();
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
        if (projectilePrefab != null && !isInitialized)
        {
            InitializePool();
        }
    }
    
    /// <summary>
    /// Khởi tạo pool với số lượng projectile ban đầu
    /// </summary>
    public void InitializePool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("TankProjectilePool: Projectile Prefab chưa được gán!");
            return;
        }
        
        if (isInitialized)
        {
            Debug.LogWarning("TankProjectilePool: Pool đã được khởi tạo rồi!");
            return;
        }
        
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Tạo một projectile mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform);
        projectile.SetActive(false);
        poolQueue.Enqueue(projectile);
        allProjectiles.Add(projectile);
        return projectile;
    }
    
    /// <summary>
    /// Lấy một projectile từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetProjectile()
    {
        GameObject projectile;
        
        if (poolQueue.Count > 0)
        {
            projectile = poolQueue.Dequeue();
        }
        else if (allProjectiles.Count < maxPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            projectile = CreateNewProjectile();
            poolQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng projectile cũ nhất
            projectile = allProjectiles[0];
            allProjectiles.RemoveAt(0);
            allProjectiles.Add(projectile);
        }
        
        projectile.SetActive(true);
        return projectile;
    }
    
    /// <summary>
    /// Trả projectile về pool
    /// </summary>
    public void ReturnProjectile(GameObject projectile)
    {
        if (projectile == null)
        {
            return;
        }
        
        projectile.SetActive(false);
        projectile.transform.SetParent(transform);
        projectile.transform.position = Vector3.zero;
        projectile.transform.rotation = Quaternion.identity;
        
        // Reset Rigidbody2D
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        if (!poolQueue.Contains(projectile))
        {
            poolQueue.Enqueue(projectile);
        }
    }
    
    /// <summary>
    /// Set prefab cho pool (có thể gọi từ code khác)
    /// </summary>
    public void SetPrefab(GameObject prefab)
    {
        if (isInitialized)
        {
            Debug.LogWarning("TankProjectilePool: Không thể thay đổi prefab sau khi pool đã được khởi tạo!");
            return;
        }
        projectilePrefab = prefab;
    }
}
