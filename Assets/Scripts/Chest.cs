using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Chest : MonoBehaviour, IDamageable
{
    [Header("Açılma Animasyon Kareleri (Kenney Tiles)")]
    [SerializeField] private Sprite closedSprite;      // tile_0089: Kapalı Sandık
    [SerializeField] private Sprite openingSprite;     // tile_0090: Kapağı Aralanmış Sandık
    [SerializeField] private Sprite openedSprite;      // tile_0091: Tam Açık Sandık
    [SerializeField] private float frameDuration = 0.12f;

    [Header("Mimic (Canavar Sandık) Şansı")]
    [SerializeField] private bool canBeMimic = true;
    [Range(0, 100)][SerializeField] private int mimicChance = 20; // %20 şansla Mimic
    [SerializeField] private Sprite mimicSprite;       // tile_0092: Canavar Sandık
    [SerializeField] private int mimicDamage = 1;

    [Header("Ödül Ayarları")]
    [SerializeField] private GameObject rewardPrefab;  // Coin_Gold prefabı
    [SerializeField] private int coinRewardCount = 3;  // Sandıktan saçılacak altın sayısı

    [Header("Etkileşim Ayarları")]
    [SerializeField] private float interactionDistance = 1.3f; // Sağ tık menzili

    private bool isOpened = false;
    private bool isMimicTriggered = false;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (closedSprite != null) spriteRenderer.sprite = closedSprite;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (isOpened || isMimicTriggered) return;

        // Oyuncu sandığa yeterince yakın mı?
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= interactionDistance)
        {
            bool rightClicked = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                rightClicked = true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(1))
                rightClicked = true;
#endif

            if (rightClicked)
            {
                Interact();
            }
        }
    }

    // Kılıçla vurulduğunda da açılsın
    public void TakeDamage(int damageAmount)
    {
        if (!isOpened && !isMimicTriggered)
        {
            Interact();
        }
    }

    private void Interact()
    {
        // 1. Şans Zarı: Mimic mi, Hazine Sandığı mı?
        if (canBeMimic && Random.Range(0, 100) < mimicChance)
        {
            TriggerMimic();
        }
        else
        {
            StartCoroutine(OpenAnimationRoutine());
        }
    }

    // --- NORMAL SANDIK: AŞAMA AŞAMA AÇILMA ANİMASYONU ---
    private IEnumerator OpenAnimationRoutine()
    {
        isOpened = true;

        // 1. Aşama: Kapak aralanır (tile_0090)
        if (openingSprite != null) spriteRenderer.sprite = openingSprite;
        yield return new WaitForSeconds(frameDuration);

        // 2. Aşama: Kapak sonuna kadar açılır (tile_0091)
        if (openedSprite != null) spriteRenderer.sprite = openedSprite;
        yield return new WaitForSeconds(frameDuration * 0.5f);

        // 3. Aşama: Altınları fırlat!
        SpawnRewards();
    }

    private void SpawnRewards()
    {
        if (rewardPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 0.2f;

        for (int i = 0; i < coinRewardCount; i++)
        {
            Instantiate(rewardPrefab, spawnPos, Quaternion.identity);
        }
    }

    // --- SÜRPRİZ: MIMIC CANAVAR SANDIK ---
    private void TriggerMimic()
    {
        isMimicTriggered = true;

        // Canavar görseline geç (tile_0092)
        if (mimicSprite != null) spriteRenderer.sprite = mimicSprite;

        Debug.LogWarning("⚠️ DİKKAT! Bu bir Mimic (Canavar Sandık)! Seni ısırdı!");
    }
}