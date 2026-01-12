using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho C4 để tối ưu performance, tránh instantiate/destroy liên tục
/// </summary>
public class C4Pool : MonoBehaviour
{
    public static C4Pool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject c4Prefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private List<GameObject> allC4s = new List<GameObject>();
    private bool isInitialized = false;

    /// <summary>
    /// Set prefab cho pool (có thể gọi từ code khác)
    /// </summary>
    public void SetPrefab(GameObject prefab)
    {
        if (isInitialized)
        {
            Debug.LogWarning("C4Pool: Không thể thay đổi prefab sau khi pool đã được khởi tạo!");
            return;
        }
        c4Prefab = prefab;
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
        if (c4Prefab != null && !isInitialized)
        {
            InitializePool();
        }
    }

    /// <summary>
    /// Khởi tạo pool với số lượng C4 ban đầu
    /// </summary>
    public void InitializePool()
    {
        if (c4Prefab == null)
        {
            Debug.LogError("C4Pool: C4 Prefab chưa được gán!");
            return;
        }

        if (isInitialized)
        {
            Debug.LogWarning("C4Pool: Pool đã được khởi tạo rồi!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewC4();
        }

        isInitialized = true;
    }

    /// <summary>
    /// Tạo một C4 mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewC4()
    {
        GameObject c4 = Instantiate(c4Prefab, transform);
        c4.SetActive(false);
        poolQueue.Enqueue(c4);
        allC4s.Add(c4);
        return c4;
    }

    /// <summary>
    /// Lấy một C4 từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetC4()
    {
        GameObject c4;

        if (poolQueue.Count > 0)
        {
            c4 = poolQueue.Dequeue();
        }
        else if (allC4s.Count < maxPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            c4 = CreateNewC4();
            poolQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng C4 cũ nhất
            c4 = allC4s[0];
            allC4s.RemoveAt(0);
            allC4s.Add(c4);
        }

        c4.SetActive(true);
        return c4;
    }

    /// <summary>
    /// Trả C4 về pool để tái sử dụng
    /// </summary>
    public void ReturnToPool(GameObject c4)
    {
        if (c4 == null) return;

        c4.SetActive(false);
        c4.transform.SetParent(transform);
        c4.transform.position = Vector3.zero;
        c4.transform.rotation = Quaternion.identity;

        // Reset Rigidbody2D nếu có
        Rigidbody2D rb = c4.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (!poolQueue.Contains(c4))
        {
            poolQueue.Enqueue(c4);
        }
    }

    /// <summary>
    /// Kiểm tra xem pool có đủ C4 không
    /// </summary>
    public bool HasAvailableC4()
    {
        return poolQueue.Count > 0 || allC4s.Count < maxPoolSize;
    }
}

