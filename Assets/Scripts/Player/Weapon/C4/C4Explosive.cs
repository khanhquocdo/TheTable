using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý logic nổ và gây sát thương của C4
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class C4Explosive : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float maxDamage = 80f;
    [SerializeField] private float minDamage = 20f;
    [SerializeField] private float autoDetonateTime = -1f; // < 0 nghĩa là chỉ nổ khi kích hoạt
    [SerializeField] private float tankBonusMultiplier = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual & Audio")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private AudioClip explosionSFX;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private bool hasExploded = false;
    private Camera mainCamera;
    private Coroutine autoDetonateCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void OnEnable()
    {
        hasExploded = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (autoDetonateTime > 0f)
        {
            autoDetonateCoroutine = StartCoroutine(AutoDetonateCountdown());
        }

        // Đăng ký với detonator
        C4Detonator.Register(this);
    }

    void OnDisable()
    {
        if (autoDetonateCoroutine != null)
        {
            StopCoroutine(autoDetonateCoroutine);
            autoDetonateCoroutine = null;
        }

        // Hủy đăng ký với detonator
        C4Detonator.Unregister(this);
    }

    private IEnumerator AutoDetonateCountdown()
    {
        yield return new WaitForSeconds(autoDetonateTime);
        if (!hasExploded)
        {
            Detonate();
        }
    }

    public void Detonate()
    {
        if (hasExploded) return;

        hasExploded = true;
        Vector2 explosionPosition = transform.position;

        // Ẩn sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Dừng di chuyển
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        DealDamage(explosionPosition);
        SpawnExplosionVFX(explosionPosition);
        PlayExplosionSFX(explosionPosition);

        StartCoroutine(ReturnToPoolAfterDelay(0.5f));
    }

    private void DealDamage(Vector2 explosionPosition)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius, enemyLayer);

        foreach (Collider2D hitCollider in hitColliders)
        {
            float distance = Vector2.Distance(explosionPosition, hitCollider.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius);
            damageMultiplier = Mathf.Clamp01(damageMultiplier);

            float finalDamage = Mathf.Lerp(minDamage, maxDamage, damageMultiplier);

            // Bonus damage cho Tank / Vehicle
            if (hitCollider.GetComponent<TankEnemyController>() != null)
            {
                finalDamage *= tankBonusMultiplier;
            }

            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(finalDamage);
            }
        }
    }

    private void SpawnExplosionVFX(Vector2 position)
    {
        if (explosionVFXPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVFXPrefab, position, Quaternion.identity);
            // Prefab nên tự hủy sau khi phát xong
        }
    }

    private void PlayExplosionSFX(Vector2 position)
    {
        if (explosionSFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSFX, position);
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (C4Pool.Instance != null)
        {
            C4Pool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

