using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject upgradesPanel;
    public GameObject shopPanel;   // ← новое поле

    // Открыть/закрыть Upgrades
    public void ToggleUpgradesPanel()
    {
        bool show = !upgradesPanel.activeSelf;
        upgradesPanel.SetActive(show);

        // Если открыли Upgrades — закрываем Shop
        if (show && shopPanel != null)
            shopPanel.SetActive(false);
    }

    // Открыть/закрыть Shop
    public void ToggleShopPanel()
    {
        bool show = !shopPanel.activeSelf;
        shopPanel.SetActive(show);

        // Если открыли Shop — закрываем Upgrades
        if (show && upgradesPanel != null)
            upgradesPanel.SetActive(false);
    }

    // Закрыть всё (если понадобится)
    public void CloseAll()
    {
        if (upgradesPanel != null) upgradesPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
    }
}
