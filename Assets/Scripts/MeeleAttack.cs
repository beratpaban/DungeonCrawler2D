using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D Zindan Oyunu - Yakın Dövüş Kılıç Savurma Mekaniği
/// OSTİM OSB Teknik Koleji - Oyun Programlama Dersi
/// </summary>
public class MeleeAttack : MonoBehaviour
{
    [Header("Girdi ve Referanslar")]
    [Tooltip("PlayerInputActions içindeki Player/Attack aksiyonu")]
    [SerializeField] private InputActionReference attackAction;

    [Tooltip("Karakterin merkezindeki silah döndürme menteşesi")]
    [SerializeField] private Transform weaponPivot;

    [Tooltip("Kılıcın görsel çizicisi (SpriteRenderer)")]
    [SerializeField] private SpriteRenderer swordRenderer;

    [Tooltip("Kılıcın temas ve hasar algılama kutusu (Is Trigger)")]
    [SerializeField] private Collider2D attackCollider;

    [Header("Saldırı Ayarları")]
    [Tooltip("Kılıcın savrulma süresi (saniye cinsinden - düşük değer şimşek gibi vurur)")]
    [SerializeField] private float attackDuration = 0.12f;

    [Tooltip("Kılıcın savrulurken tarayacağı açı genişliği (derece)")]
    [SerializeField] private float attackArc = 80f;

    [Tooltip("İki saldırı arasında geçmesi gereken minimum bekleme süresi (cooldown)")]
    [SerializeField] private float attackCooldown = 0.25f;

    private Camera mainCamera;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Oyun başlarken kılıcı ve temas kutusunu tamamen gizle
        if (swordRenderer != null) swordRenderer.enabled = false;
        if (attackCollider != null) attackCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (attackAction != null) attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction != null) attackAction.action.Disable();
    }

    private void Update()
    {
        // Sol tık basıldı mı ve bekleme süresi doldu mu?
        if (attackAction != null && attackAction.action.WasPressedThisFrame() && Time.time >= nextAttackTime)
        {
            if (!isAttacking)
            {
                StartCoroutine(PerformSlashRoutine());
            }
        }
    }

    /// <summary>
    /// Kılıcı saliseler içinde farenin baktığı yöne savuran Coroutine
    /// </summary>
    private IEnumerator PerformSlashRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // 1. Farenin ekrandaki piksel konumunu oku ve zindan dünyası metre koordinatına çevir
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        // 2. Karakterden fareye doğru uzanan yön vektörünü ve taban açısını hesapla
        Vector3 direction = (mouseWorld - weaponPivot.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 3. Kılıcı görünür yap ve temas kutusunu aç
        swordRenderer.enabled = true;
        attackCollider.enabled = true;

        // Kritik Düzeltme: Fare sol taraftayken kılıcın ters (baş aşağı) durmaması için dikey çevir (flipY)
        swordRenderer.flipY = (baseAngle > 90f || baseAngle < -90f);

        // 4. Savurma Açısı: Belirlenen yay genişliğine göre başlangıç ve bitiş açılarını hesapla
        Quaternion startRot = Quaternion.Euler(0, 0, baseAngle + (attackArc / 2));
        Quaternion endRot = Quaternion.Euler(0, 0, baseAngle - (attackArc / 2));

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackDuration;

            // Küresel enterpolasyon (Slerp) ile menteşeyi başlangıç açısından bitiş açısına pürüzsüzce döndür
            weaponPivot.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // 5. Vuruş bitti: Kılıcı ve temas kutusunu tekrar tamamen gizle
        swordRenderer.enabled = false;
        attackCollider.enabled = false;
        isAttacking = false;
    }
}