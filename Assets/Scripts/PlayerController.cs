
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D Zindan Oyunu - Karakter Hareket Kontrolcüsü
/// OSTİM OSB Teknik Koleji - Oyun Programlama Dersi
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Parametreleri")]
    [Tooltip("Karakterin saniyedeki yürüme hızı (birim/saniye)")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Girdi Referansı (New Input System)")]
    [Tooltip("PlayerInputActions içindeki Player/Move aksiyonu")]
    [SerializeField] private InputActionReference moveAction;

    // Dahili Bileşen Referansları
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Klavyeden anlık okunan yön vektörü (X: -1 ile +1, Y: -1 ile +1)
    private Vector2 moveInput;

    private void Awake()
    {
        // Karakterin üzerindeki fizik ve görsel bileşenlerini hafızaya al
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Script aktif olduğunda klavye dinleyicisini aç
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        // Script veya karakter devre dışı kaldığında dinleyiciyi kapat (Hafıza sızıntısını önler)
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Update()
    {
        // 1. Klavyeden gelen WASD verisini her render karesinde oku
        if (moveAction != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        // 2. Görsel Çevirme: Karakter sola yürüyorsa sprite'ı yatayda ters çevir (flipX)
        if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;  // Sola bak
        }
        else if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false; // Sağa bak (Orijinal yön)
        }
    }

    private void FixedUpdate()
    {
        // 3. Çapraz Hızlanmayı Önleme (Pisagor Teoremi Düzeltmesi):
        // W (0,1) ve D (1,0) aynı anda basıldığında uzunluk 1.414 birim olur.
        // .normalized kullanarak uzunluğu daima 1 birime sabitleriz.
        Vector2 targetVelocity = moveInput.normalized * moveSpeed;

        // 4. Unity Sürüm Uyumluluğu:
        // Unity 6'da linearVelocity standardı gelmiştir; eski sürümlerde velocity kullanılır.
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = targetVelocity;
#else
        rb.velocity = targetVelocity;
#endif
    }
}