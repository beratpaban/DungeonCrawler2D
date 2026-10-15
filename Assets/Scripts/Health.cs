using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Can Ayarları")]
    [Tooltip("Varlığın sahip olabileceği en yüksek can puanı")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Hasar Tepkisi & I-Frames (Dokunulmazlık)")]
    [Tooltip("Hasar alındığında sprite'ın bürüneceği flaş rengi")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [Tooltip("Hasar aldıktan sonra kaç saniye dokunulmaz kalınacak")]
    [SerializeField] private float invulnerabilityDuration = 0.8f;
    private bool isInvulnerable = false;

    [Header("Ölüm & Ganimet (Yalnızca Düşmanlar İçin)")]
    [Tooltip("Öldüğünde fırlatılacak altın sikke prefabı")]
    [SerializeField] private GameObject lootPrefab;
    [Range(0, 100)][SerializeField] private int lootDropChance = 60; // %60 ihtimalle altın düşer

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    // IDamageable Sözleşmesi
    public void TakeDamage(int damageAmount)
    {
        // Dokunulmazlık süresindeyse veya zaten ölmüşse hasar alma
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth -= damageAmount;
        Debug.Log($"💥 {gameObject.name} Hasar Aldı! Kalan Can: {currentHealth}/{maxHealth}");

        // Görsel Hasar Flaşı (Kırmızı yanıp sönme)
        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFeedbackRoutine());
        }

        // Can bitti mi?
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"💚 {gameObject.name} İyileşti! Güncel Can: {currentHealth}/{maxHealth}");
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        isInvulnerable = true;

        // Anlık kırmızı flaş
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(0.12f);
        spriteRenderer.color = originalColor;

        // Dokunulmazlık süresince yarı-saydam yanıp sönme
        float elapsed = 0.12f;
        while (elapsed < invulnerabilityDuration)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        spriteRenderer.color = originalColor;
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log($"💀 {gameObject.name} Öldü!");

        // Oyuncu öldüyse bölümü 1 saniye sonra baştan başlat
        if (CompareTag("Player"))
        {
            Debug.LogWarning("☠️ OYUN BİTTİ (GAME OVER)! Şövalye düştü.");
            Invoke(nameof(RestartLevel), 1.0f);
            gameObject.SetActive(false);
            return;
        }

        // Düşman öldüyse: Altın fırlat ve sahneden sil
        if (lootPrefab != null && Random.Range(0, 100) < lootDropChance)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}