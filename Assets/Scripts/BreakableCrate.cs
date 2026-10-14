using UnityEngine;

public class BreakableCrate : MonoBehaviour, IDamageable
{
    [Header("Görsel Parçalanma")]
    [Tooltip("Kutu kırıldığında patlayacak talaş parçacık prefabı")]
    [SerializeField] private GameObject breakEffectPrefab;

    [Header("Şanslı Ganimet Tablosu (Toplam %100)")]
    [Range(0, 100)][SerializeField] private int coinDropChance = 60; // %60 ihtimalle Altın çıkar
    // Kalan %40 ihtimalde kutu boş çıkar

    [Header("Düşecek Prefab")]
    [SerializeField] private GameObject coinPrefab;

    public void TakeDamage(int damageAmount)
    {
        // 1. Talaş patlama efektini kutunun konumunda oluştur
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Şans zarını at (0 ile 100 arası)
        int roll = Random.Range(0, 100);

        if (roll < coinDropChance)
        {
            if (coinPrefab != null)
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
            }
        }

        // 3. Kutuyu sahneden tamamen yok et
        Destroy(gameObject);
    }
}