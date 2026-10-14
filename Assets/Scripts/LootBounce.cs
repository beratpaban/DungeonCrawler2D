using System.Collections;
using UnityEngine;

public class LootBounce : MonoBehaviour
{
    [Header("Zıplama Parametreleri")]
    [Tooltip("Eşyanın havaya doğru yükseleceği tepe noktası")]
    [SerializeField] private float bounceHeight = 0.6f;

    [Tooltip("Zıplamanın toplam tamamlanma süresi (saniye)")]
    [SerializeField] private float duration = 0.35f;

    private void Start()
    {
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Vector3 startPos = transform.position;
        // Rastgele hafif sağa, sola, yukarı veya aşağı saçılma hedefi
        Vector3 targetPos = startPos + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.2f), 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Parabolik sinüs eğrisi: Başlangıçta 0, ortada 1, sonda tekrar 0 olur (Zıplama yayı)
            float height = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            transform.position = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * height;

            yield return null;
        }

        transform.position = targetPos;
    }
}