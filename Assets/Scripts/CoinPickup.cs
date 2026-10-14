using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Sandığın dibindeyken altın oyuncunun tam üstüne fırlarsa kaçırmamak için güvenlik kontrolü
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (isCollected) return;

        // Çarpan nesnede veya ebeveyninde PlayerInventory var mı?
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null)
        {
            isCollected = true;
            inventory.AddCoins(coinValue);
            Destroy(gameObject); // Altını sahneden sil
        }
    }
}