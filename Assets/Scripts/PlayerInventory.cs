using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Toplanan Ganimetler")]
    public int coinCount = 0;
    public int keyCount = 0;

    // --- ALTIN FONKSİYONLARI ---
    public void AddCoins(int amount)
    {
        coinCount += amount;
        Debug.Log($"💰 Altın Toplandı! (+{amount}) | Toplam Altın: {coinCount}");
    }

    // --- ANAHTAR FONKSİYONLARI (İleriki Odalar İçin) ---
    public void AddKey()
    {
        keyCount++;
        Debug.Log($"🗝️ Altın Anahtar Alındı! | Toplam Anahtar: {keyCount}");
    }

    public bool UseKey()
    {
        if (keyCount > 0)
        {
            keyCount--;
            Debug.Log($"🔓 Anahtar Kullanıldı! | Kalan Anahtar: {keyCount}");
            return true;
        }

        Debug.Log("⛔ Kapıyı açmak için anahtarınız yok!");
        return false;
    }
}