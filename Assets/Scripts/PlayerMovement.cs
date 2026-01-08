using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 movement;
    public float deadZone = 0.1f;
    private Animator ani;
    public Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
    }

    void Update()
    {
        // Lấy input 8 hướng (WASD / Arrow)
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        // Chuẩn hóa vector để đi chéo không nhanh hơn
        movement = movementInput.normalized;
        float speed = movement.magnitude;
        ani.SetFloat("Speed", speed);

        CalculateAnimation();
    }

    void CalculateAnimation()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;
        direction.Normalize();

        if (direction.magnitude < deadZone)
        {
            return;
        }

        ani.SetFloat("LookX", direction.x);
        ani.SetFloat("LookY", direction.y);
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }
}
