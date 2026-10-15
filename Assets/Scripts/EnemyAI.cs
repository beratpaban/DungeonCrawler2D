using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Wandering, Chasing }

    [Header("Durum ve Hız Ayarları")]
    [Tooltip("Oyuncuyu kovalarkenki koşma hızı")]
    [SerializeField] private float chaseSpeed = 2.4f;

    [Tooltip("Kendi etrafında gezinirkenki devriye hızı")]
    [SerializeField] private float wanderSpeed = 1.8f;

    [Tooltip("Oyuncuyu fark etme radar menzili (metre cinsinden)")]
    [SerializeField] private float detectionRadius = 5.5f;

    [Tooltip("Oyuncunun dibine gelince durma mesafesi")]
    [SerializeField] private float stopDistance = 0.4f;

    [Header("Devriye (Yürü - Bekle - Yürü) Ayarları")]
    [Tooltip("Durup bekleme süresi (saniye)")]
    [SerializeField] private float idleDuration = 0.8f;

    [Tooltip("Rastgele yöne yürüme süresi (saniye)")]
    [SerializeField] private float walkDuration = 2.5f;

    [Tooltip("Doğduğu noktadan en fazla kaç metre uzaklaşabilir")]
    [SerializeField] private float wanderRadius = 4.5f;

    [Header("Duvar ve Engel Algılama")]
    [Tooltip("Düşmanın duvara takılmaması için algılayacağı katmanlar (Varsayılan: Default ve Walls)")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Düşmanın önündeki duvarı algılama mesafesi")]
    [SerializeField] private float checkDistance = 0.5f;

    [Header("Hayalet Süzülme Animasyonu (Floating)")]
    [Tooltip("Bu yaratık hayalet gibi aşağı-yukarı süzülsün mü?")]
    [SerializeField] private bool isGhost = false;

    [Tooltip("Süzülme dalgalanma hızı")]
    [SerializeField] private float floatFrequency = 4.0f;

    [Tooltip("Yukarı-aşağı süzülme dalga kuvveti")]
    [SerializeField] private float floatAmplitude = 0.8f;

    [Header("Saldırı")]
    [SerializeField] private int contactDamage = 1;

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private EnemyState currentState = EnemyState.Idle;
    private Vector2 spawnPosition;
    private Vector2 wanderDirection;
    private float stateTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Engel katmanı seçilmediyse Default ve Walls katmanlarını baz al
        if (obstacleLayer.value == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default", "Walls");
            if (obstacleLayer.value == 0) obstacleLayer = 1 << 0; // Default layer
        }

        EnterIdleState();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Radar Kontrolü: Oyuncu menzildeyse KOVALA!
        if (distanceToPlayer <= detectionRadius)
        {
            currentState = EnemyState.Chasing;
        }
        else if (currentState == EnemyState.Chasing)
        {
            // Oyuncu menzilden çıktıysa devriyeye dön
            EnterIdleState();
        }

        // 2. Devriye Zamanlayıcı Mantığı (Idle <-> Wandering döngüsü)
        if (currentState != EnemyState.Chasing)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                if (currentState == EnemyState.Idle)
                {
                    EnterWanderState();
                }
                else
                {
                    EnterIdleState();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = Vector2.zero;

        switch (currentState)
        {
            case EnemyState.Chasing:
                if (playerTransform != null)
                {
                    float distance = Vector2.Distance(transform.position, playerTransform.position);
                    if (distance > stopDistance)
                    {
                        Vector2 rawDir = ((Vector2)playerTransform.position - rb.position).normalized;
                        // Akıllı Engel Aşma: Önünde duvar varsa teğet yönü seç
                        Vector2 moveDir = GetSmartMoveDirection(rawDir);

                        targetVelocity = moveDir * chaseSpeed;
                        UpdateFacing(moveDir.x);
                    }
                }
                break;

            case EnemyState.Wandering:
                targetVelocity = wanderDirection * wanderSpeed;
                UpdateFacing(wanderDirection.x);
                break;

            case EnemyState.Idle:
                targetVelocity = Vector2.zero;
                break;
        }

        // 👻 Hayalet süzülme dalgası
        if (isGhost)
        {
            float sineOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            targetVelocity.y += sineOffset;
        }

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = targetVelocity;
#else
        rb.velocity = targetVelocity;
#endif
    }

    // 🧭 AKILLI ENGEL DOLANMA (WHISKER SENSOR):
    // Düşmanın kendi collider'ına ASLA çarpmaz; önündeki duvarı görünce sağa/sola teğet açılır.
    private Vector2 GetSmartMoveDirection(Vector2 desiredDir)
    {
        // 1. Düz rotada gerçek bir engel var mı?
        if (!IsBlockedByWall(desiredDir, checkDistance, out RaycastHit2D frontHit))
        {
            return desiredDir; // Önü açıksa dümdüz oyuncuya git
        }

        // 2. Önümüz tıkalı! 50 derece sağa ve 50 derece sola bak
        Vector2 leftWhisker = Rotate(desiredDir, 50f);
        Vector2 rightWhisker = Rotate(desiredDir, -50f);

        bool leftBlocked = IsBlockedByWall(leftWhisker, checkDistance * 0.9f, out _);
        bool rightBlocked = IsBlockedByWall(rightWhisker, checkDistance * 0.9f, out _);

        if (!leftBlocked && rightBlocked)
        {
            return leftWhisker; // Sol açık, oradan ak
        }
        else if (!rightBlocked && leftBlocked)
        {
            return rightWhisker; // Sağ açık, oradan ak
        }
        else if (!leftBlocked && !rightBlocked)
        {
            // İkisi de açıksa duvardan teğet kay (Wall Slide)
            Vector2 normal = frontHit.normal;
            Vector2 slide = desiredDir - Vector2.Dot(desiredDir, normal) * normal;
            return slide.sqrMagnitude > 0.05f ? slide.normalized : leftWhisker;
        }
        else
        {
            // İki bıyık da tıkalıysa (iç köşe): Duvarın normaline dik 90 derece kaç
            Vector2 perp = new Vector2(-frontHit.normal.y, frontHit.normal.x);
            return perp;
        }
    }

    // Kendi vücudunu, oyuncuyu veya diğer düşmanları DUVAR sanmasını %100 engelleyen güvenli kontrol:
    private bool IsBlockedByWall(Vector2 dir, float distance, out RaycastHit2D validHit)
    {
        validHit = default;
        // Kendi collider'ımızın yarıçapından biraz dışarıdan raycast at
        Vector2 startPos = rb.position + dir * 0.2f;
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, dir, distance, obstacleLayer);

        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (h.collider == myCollider || h.collider.gameObject == gameObject) continue; // Kendini yoksay!
            if (h.collider.isTrigger) continue; // Trigger tetikleyicileri yoksay!
            if (h.collider.CompareTag("Player") || h.collider.CompareTag("Enemy")) continue; // Karakterleri yoksay!

            validHit = h;
            return true; // Gerçek bir duvar bulduk!
        }

        return false;
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
    }

    private void EnterIdleState()
    {
        currentState = EnemyState.Idle;
        stateTimer = Random.Range(idleDuration * 0.7f, idleDuration * 1.3f);
    }

    private void EnterWanderState()
    {
        currentState = EnemyState.Wandering;
        stateTimer = Random.Range(walkDuration * 0.7f, walkDuration * 1.3f);

        // Doğduğu yerden çok uzaklaşmışsa doğduğu noktaya geri yönel
        if (Vector2.Distance(transform.position, spawnPosition) > wanderRadius)
        {
            wanderDirection = (spawnPosition - (Vector2)transform.position).normalized;
        }
        else
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }
    }

    private void UpdateFacing(float directionX)
    {
        if (directionX < -0.05f) spriteRenderer.flipX = true;   // Sola bak
        else if (directionX > 0.05f) spriteRenderer.flipX = false; // Sağa bak
    }

    // Devriye gezerken duvara çarparsa duvardan sekip yoluna devam etsin
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Wandering && !collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.contactCount > 0)
            {
                // Bilardo topu gibi duvardan sekip gezinmeye devam et
                wanderDirection = Vector2.Reflect(wanderDirection, collision.contacts[0].normal).normalized;
            }
            else
            {
                EnterWanderState();
            }
        }
    }

    // Temas Hasarı: Oyuncuya dokunursa hasar ver
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IDamageable playerDamage = collision.gameObject.GetComponent<IDamageable>();
            if (playerDamage != null)
            {
                playerDamage.TakeDamage(contactDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPosition : transform.position, wanderRadius);
    }
}