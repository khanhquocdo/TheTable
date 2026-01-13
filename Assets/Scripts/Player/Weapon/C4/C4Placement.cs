using UnityEngine;

/// <summary>
/// Xử lý đặt/néo C4: ném tầm ngắn, bám enemy hoặc dính trên ground/wall
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class C4Placement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float throwSpeed = 10f;

    [Header("Layer Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isAttached = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        if (col != null)
        {
            // Dùng trigger để tránh truyền lực va chạm làm văng target
            col.isTrigger = true;
        }
    }

    void OnEnable()
    {
        isAttached = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = true;
        }
    }

    /// <summary>
    /// Ném hoặc đặt C4
    /// </summary>
    public void Place(Vector2 direction, Vector2 startPosition)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        if (rb == null) return;

        // Nếu không có hướng, coi như đặt xuống chân player
        if (direction.sqrMagnitude < 0.01f)
        {
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.velocity = direction.normalized * throwSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isAttached) return;

        int otherLayer = collision.gameObject.layer;

        bool hitEnemy = (enemyLayer.value & (1 << otherLayer)) != 0;
        bool hitGround = (groundLayer.value & (1 << otherLayer)) != 0;

        if (hitEnemy)
        {
            AttachToTarget(collision.transform);
        }
        else if (hitGround)
        {
            StickToSurface();
        }
        else
        {
            // Nếu không thuộc layer nào, vẫn dính lại tại chỗ va chạm
            StickToSurface();
        }
    }

    private void AttachToTarget(Transform target)
    {
        isAttached = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = false; // Tắt collider để tránh va chạm tiếp
        }

        transform.SetParent(target);
        transform.position = target.position;
    }

    private void StickToSurface()
    {
        isAttached = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = false;
        }

        transform.SetParent(null);
    }
}

