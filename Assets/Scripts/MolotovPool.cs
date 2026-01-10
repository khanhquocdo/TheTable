using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool cho Molotov và FireArea để tối ưu performance, tránh instantiate/destroy liên tục
/// </summary>
public class MolotovPool : MonoBehaviour
{
    public static MolotovPool Instance { get; private set; }

    [Header("Molotov Pool Settings")]
    [SerializeField] private GameObject molotovPrefab;
    [SerializeField] private int initialMolotovPoolSize = 10;
    [SerializeField] private int maxMolotovPoolSize = 50;

    [Header("Fire Area Pool Settings")]
    [SerializeField] private GameObject fireAreaPrefab;
    [SerializeField] private int initialFireAreaPoolSize = 5;
    [SerializeField] private int maxFireAreaPoolSize = 20;

    private Queue<GameObject> molotovQueue = new Queue<GameObject>();
    private List<GameObject> allMolotovs = new List<GameObject>();
    private bool isMolotovPoolInitialized = false;

    private Queue<GameObject> fireAreaQueue = new Queue<GameObject>();
    private List<GameObject> allFireAreas = new List<GameObject>();
    private bool isFireAreaPoolInitialized = false;

    /// <summary>
    /// Set prefab cho pool (có thể gọi từ code khác)
    /// </summary>
    public void SetMolotovPrefab(GameObject prefab)
    {
        if (isMolotovPoolInitialized)
        {
            Debug.LogWarning("MolotovPool: Không thể thay đổi prefab sau khi pool đã được khởi tạo!");
            return;
        }
        molotovPrefab = prefab;
    }

    public void SetFireAreaPrefab(GameObject prefab)
    {
        if (isFireAreaPoolInitialized)
        {
            Debug.LogWarning("MolotovPool: Không thể thay đổi FireArea prefab sau khi pool đã được khởi tạo!");
            return;
        }
        fireAreaPrefab = prefab;
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
        if (molotovPrefab != null && !isMolotovPoolInitialized)
        {
            InitializeMolotovPool();
        }

        if (fireAreaPrefab != null && !isFireAreaPoolInitialized)
        {
            InitializeFireAreaPool();
        }
    }

    #region Molotov Pool

    /// <summary>
    /// Khởi tạo pool Molotov với số lượng ban đầu
    /// </summary>
    public void InitializeMolotovPool()
    {
        if (molotovPrefab == null)
        {
            Debug.LogError("MolotovPool: Molotov Prefab chưa được gán!");
            return;
        }

        if (isMolotovPoolInitialized)
        {
            Debug.LogWarning("MolotovPool: Molotov Pool đã được khởi tạo rồi!");
            return;
        }

        for (int i = 0; i < initialMolotovPoolSize; i++)
        {
            CreateNewMolotov();
        }

        isMolotovPoolInitialized = true;
    }

    /// <summary>
    /// Tạo một Molotov mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewMolotov()
    {
        GameObject molotov = Instantiate(molotovPrefab, transform);
        molotov.SetActive(false);
        molotovQueue.Enqueue(molotov);
        allMolotovs.Add(molotov);
        return molotov;
    }

    /// <summary>
    /// Lấy một Molotov từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetMolotov()
    {
        GameObject molotov;

        if (molotovQueue.Count > 0)
        {
            molotov = molotovQueue.Dequeue();
        }
        else if (allMolotovs.Count < maxMolotovPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            molotov = CreateNewMolotov();
            molotovQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng Molotov cũ nhất
            molotov = allMolotovs[0];
            allMolotovs.RemoveAt(0);
            allMolotovs.Add(molotov);
        }

        molotov.SetActive(true);
        return molotov;
    }

    /// <summary>
    /// Trả Molotov về pool để tái sử dụng
    /// </summary>
    public void ReturnToPool(GameObject molotov)
    {
        if (molotov == null) return;

        molotov.SetActive(false);
        molotov.transform.SetParent(transform);
        molotov.transform.position = Vector3.zero;
        molotov.transform.rotation = Quaternion.identity;

        // Reset Rigidbody2D nếu có
        Rigidbody2D rb = molotov.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (!molotovQueue.Contains(molotov))
        {
            molotovQueue.Enqueue(molotov);
        }
    }

    /// <summary>
    /// Kiểm tra xem pool có đủ Molotov không
    /// </summary>
    public bool HasAvailableMolotov()
    {
        return molotovQueue.Count > 0 || allMolotovs.Count < maxMolotovPoolSize;
    }

    #endregion

    #region Fire Area Pool

    /// <summary>
    /// Khởi tạo pool FireArea với số lượng ban đầu
    /// </summary>
    public void InitializeFireAreaPool()
    {
        if (fireAreaPrefab == null)
        {
            Debug.LogError("MolotovPool: FireArea Prefab chưa được gán!");
            return;
        }

        if (isFireAreaPoolInitialized)
        {
            Debug.LogWarning("MolotovPool: FireArea Pool đã được khởi tạo rồi!");
            return;
        }

        for (int i = 0; i < initialFireAreaPoolSize; i++)
        {
            CreateNewFireArea();
        }

        isFireAreaPoolInitialized = true;
    }

    /// <summary>
    /// Tạo một FireArea mới và thêm vào pool
    /// </summary>
    private GameObject CreateNewFireArea()
    {
        GameObject fireArea = Instantiate(fireAreaPrefab, transform);
        fireArea.SetActive(false);
        fireAreaQueue.Enqueue(fireArea);
        allFireAreas.Add(fireArea);
        return fireArea;
    }

    /// <summary>
    /// Lấy một FireArea từ pool (hoặc tạo mới nếu pool rỗng và chưa đạt max size)
    /// </summary>
    public GameObject GetFireArea()
    {
        GameObject fireArea;

        if (fireAreaQueue.Count > 0)
        {
            fireArea = fireAreaQueue.Dequeue();
        }
        else if (allFireAreas.Count < maxFireAreaPoolSize)
        {
            // Tạo mới nếu chưa đạt max size
            fireArea = CreateNewFireArea();
            fireAreaQueue.Dequeue(); // Lấy ra khỏi queue vừa tạo
        }
        else
        {
            // Nếu đạt max size, tái sử dụng FireArea cũ nhất
            fireArea = allFireAreas[0];
            allFireAreas.RemoveAt(0);
            allFireAreas.Add(fireArea);
        }

        // Đảm bảo GameObject active trước khi trả về
        fireArea.SetActive(true);
        return fireArea;
    }

    /// <summary>
    /// Trả FireArea về pool để tái sử dụng
    /// </summary>
    public void ReturnFireAreaToPool(GameObject fireArea)
    {
        if (fireArea == null) return;

        fireArea.SetActive(false);
        fireArea.transform.SetParent(transform);
        fireArea.transform.position = Vector3.zero;
        fireArea.transform.rotation = Quaternion.identity;

        if (!fireAreaQueue.Contains(fireArea))
        {
            fireAreaQueue.Enqueue(fireArea);
        }
    }

    /// <summary>
    /// Kiểm tra xem pool có đủ FireArea không
    /// </summary>
    public bool HasAvailableFireArea()
    {
        return fireAreaQueue.Count > 0 || allFireAreas.Count < maxFireAreaPoolSize;
    }

    #endregion
}

