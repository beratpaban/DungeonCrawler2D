using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Girdi Referansı")]
    [SerializeField] private InputActionReference moveAction;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    private void Update()
    {
        // 1. Klavyeden gelen WASD verisini oku (-1 ile +1 arasında değer döner)
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        // 2. Karakter sola yürüyorsa görseli sola çevir, sağa yürüyorsa düzelt
        if (moveInput.x < 0) spriteRenderer.flipX = true;
        else if (moveInput.x > 0) spriteRenderer.flipX = false;
    }

    private void FixedUpdate()
    {
        // 3. Çapraz yürürken karakterin iki kat hızlanmasını .normalized ile önlüyoruz
        Vector2 targetVelocity = moveInput.normalized * moveSpeed;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = targetVelocity;
#else
        rb.velocity = targetVelocity;
#endif
    }
}