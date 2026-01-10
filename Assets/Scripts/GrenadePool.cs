using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho Grenade để tối ưu performance, tránh instantiate/destroy liên tục
/// </summary>
public class GrenadePool : MonoBehaviour
{
    public static GrenadePool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private List<GameObject> allGrenades = new List<GameObject>();
    private bool isInitialized = false;

    /// <summary>
    /// Set prefab cho pool (có thể gọi từ code khác)
    /// </summary>
    public void SetPrefab(GameObject prefab)
    {
        if (isInitialized)
        {
            Debug.LogWarning("GrenadePool: Không thể thay đổi prefab sau khi pool đã được khởi tạo!");
            return;
        }
        grenadePrefab = prefab;
    }

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
        // Chỉ khởi tạo pool nếu prefab đã được gán
        if (grenadePrefab != null && !isInitialized)
        {
            InitializePool();
        }
    }

    /// <summary>
    /// Khởi tạo pool với số lượng grenade ban đầu
    /// </summary>
    public void InitializePool()
    {
        if (grenadePrefab == null)
        {
            Debug.LogError("GrenadePool: Grenade Prefab chưa được gán!");
            return;
        }

        if (isInitialized)
        {
            Debug.LogWarning("GrenadePool: Pool đã được khởi tạo rồi!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewGrenade();
        }

        isInitialized = true;
    }

    /// <summary>
    /// Tạo một grenade mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, transform);
        grenade.SetActive(false);
        poolQueue.Enqueue(grenade);
        allGrenades.Add(grenade);
        return grenade;
    }

    /// <summary>
    /// Lấy một grenade từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetGrenade()
    {
        GameObject grenade;

        if (poolQueue.Count > 0)
        {
            grenade = poolQueue.Dequeue();
        }
        else if (allGrenades.Count < maxPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            grenade = CreateNewGrenade();
            poolQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng grenade cũ nhất
            grenade = allGrenades[0];
            allGrenades.RemoveAt(0);
            allGrenades.Add(grenade);
        }

        grenade.SetActive(true);
        return grenade;
    }

    /// <summary>
    /// Trả grenade về pool để tái sử dụng
    /// </summary>
    public void ReturnToPool(GameObject grenade)
    {
        if (grenade == null) return;

        grenade.SetActive(false);
        grenade.transform.SetParent(transform);
        grenade.transform.position = Vector3.zero;
        grenade.transform.rotation = Quaternion.identity;

        // Reset Rigidbody2D nếu có
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (!poolQueue.Contains(grenade))
        {
            poolQueue.Enqueue(grenade);
        }
    }

    /// <summary>
    /// Kiểm tra xem pool có đủ grenade không
    /// </summary>
    public bool HasAvailableGrenade()
    {
        return poolQueue.Count > 0 || allGrenades.Count < maxPoolSize;
    }
}

