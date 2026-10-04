using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAttack : MonoBehaviour
{
    [Header("Girdi ve Referanslar")]
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private SpriteRenderer swordRenderer;
    [SerializeField] private Collider2D attackCollider;

    [Header("Saldırı Ayarları")]
    [SerializeField] private float attackDuration = 0.12f; // Kılıcın savrulma hızı (saniye)
    [SerializeField] private float attackArc = 80f;        // Kaç derecelik yay çizeceği
    [SerializeField] private float attackCooldown = 0.25f; // İki vuruş arası bekleme

    private Camera mainCamera;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Oyun başlarken kılıcı ve çarpışma kutusunu tamamen gizle
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
        if (attackAction != null && attackAction.action.WasPressedThisFrame() && Time.time >= nextAttackTime)
        {
            if (!isAttacking) StartCoroutine(PerformSlashRoutine());
        }
    }

    private IEnumerator PerformSlashRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // 1. Tıklandığı anda farenin dünyadaki konumunu ve açısını bul
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector3 direction = (mouseWorld - weaponPivot.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Kılıcı görünür yap ve temas alanını aç
        swordRenderer.enabled = true;
        attackCollider.enabled = true;

        // Fare sol taraftaysa kılıcın ters durmaması için dikey çevir (flipY)
        swordRenderer.flipY = (baseAngle > 90f || baseAngle < -90f);

        // 3. Savurma açısı hesapla (Başlangıç ve Bitiş)
        Quaternion startRot = Quaternion.Euler(0, 0, baseAngle + (attackArc / 2));
        Quaternion endRot = Quaternion.Euler(0, 0, baseAngle - (attackArc / 2));

        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackDuration;
            weaponPivot.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // 4. Vuruş bitti: Kılıcı ve temas kutusunu tekrar tamamen gizle
        swordRenderer.enabled = false;
        attackCollider.enabled = false;
        isAttacking = false;
    }
}