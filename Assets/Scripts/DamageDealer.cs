using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Tooltip("Kılıcın tek vuruşta vereceği hasar puanı")]
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Kılıç oyuncunun kendisine asla hasar vermesin!
        if (other.CompareTag("Player")) return;

        // 2. Çarptığımız nesnede IDamageable sözleşmesi var mı?
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}