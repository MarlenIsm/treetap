using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Currencies")]
    public int coins = 0;
    public int gems = 0;

    [Header("Tap Power")]
    public int clickLevel = 0;              // уровень прокачки клика
    public int baseClickUpgradeCost = 10;   // базовая цена апгрейда
    public int coinsPerTap = 1;             // сколько монет даёт один тап

    [Header("Auto Clicker")]
    public int autoClickLevel = 0;              // уровень автокликера
    public int autoClickPower = 0;              // сколько монет в секунду даёт автокликер
    public int baseAutoClickCost = 50;          // базовая цена автокликера
    public float autoClickCostMultiplier = 1.25f; // коэффициент роста цены автокликера

    [Header("Tree Income")]
    public int treeIncomeLevel = 0;                 // уровень Tree Income
    public int treeIncomePower = 0;                 // сколько монет в секунду даёт Tree Income
    public int baseTreeIncomeCost = 150;            // базовая цена Tree Income
    public float treeIncomeCostMultiplier = 1.28f;  // рост цены Tree Income

    [Header("Gem Tap Upgrade")]
    public int gemTapLevel = 0;                 // уровень апгрейда за гемы
    public int baseGemTapCost = 20;             // базовая цена в гемах
    public float gemTapBonusPerLevel = 0.5f;    // +50% к клику за уровень
    public float gemTapCostMultiplier = 1.25f;  // рост цены апгрейда за гемы

    [Header("Tap Boost (temporary)")]
    public bool tapBoostActive = false;        // активен ли временный буст
    public float tapBoostMultiplier = 2f;      // x2 к клику
    public float tapBoostDuration = 30f;       // длительность буста в секундах
    private float tapBoostTimeLeft = 0f;       // сколько осталось времени
    public int tapBoostGemCost = 100;          // цена буста в гемах
    public TextMeshProUGUI tapBoostTimerText;  // текст таймера (можно не заполнять, тогда таймер просто не отображается)

    [Header("Currency UI")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;

    [Header("Click Upgrade UI")]
    public TextMeshProUGUI clickUpgradeLevelText;
    public TextMeshProUGUI clickUpgradeCostText;

    [Header("Auto Clicker UI")]
    public TextMeshProUGUI autoClickLevelText;
    public TextMeshProUGUI autoClickCostText;

    [Header("Tree Income UI")]
    public TextMeshProUGUI treeIncomeLevelText;
    public TextMeshProUGUI treeIncomeCostText;

    [Header("Gem Tap Upgrade UI")]
    public TextMeshProUGUI gemTapLevelText;
    public TextMeshProUGUI gemTapCostText;

    void Start()
    {
        RecalculateTapPower();
        RecalculateTreeIncomePower();

        UpdateCoinsUI();
        UpdateGemsUI();
        UpdateClickUpgradeUI();
        UpdateAutoClickUI();
        UpdateTreeIncomeUI();
        UpdateGemTapUpgradeUI();

        // запуск тика пассивного дохода: раз в 1 секунду
        InvokeRepeating(nameof(PassiveIncomeTick), 1f, 1f);
    }

    void Update()
    {
        // тик временного буста
        if (tapBoostActive)
        {
            tapBoostTimeLeft -= Time.deltaTime;

            if (tapBoostTimerText != null)
            {
                int seconds = Mathf.CeilToInt(tapBoostTimeLeft);
                tapBoostTimerText.text = seconds > 0 ? $"x2 TAP: {seconds}s" : "";
            }

            if (tapBoostTimeLeft <= 0f)
            {
                tapBoostActive = false;
                tapBoostTimeLeft = 0f;

                if (tapBoostTimerText != null)
                    tapBoostTimerText.text = "";

                RecalculateTapPower(); // возвращаем силу клика без буста
            }
        }
    }

    // ================= ЛОГИКА ТАПОВ И ВАЛЮТ =================

    // выстрел при тапе по дереву
    public void AddCoin()
    {
        coins += coinsPerTap;
        UpdateCoinsUI();
    }

    public void AddGems(int amount)
    {
        gems += amount;
        UpdateGemsUI();
    }

    // ================= ПОКУПКИ АПГРЕЙДОВ =================

    // нажали BUY у апгрейда клика (за монеты)
    public void BuyClickUpgrade()
    {
        int cost = GetClickUpgradeCost();

        if (coins >= cost)
        {
            coins -= cost;
            clickLevel++;
            RecalculateTapPower();

            UpdateCoinsUI();
            UpdateClickUpgradeUI();
        }
        else
        {
            Debug.Log("Not enough coins for Click Upgrade");
        }
    }

    // нажали BUY у автокликера (за монеты)
    public void BuyAutoClickUpgrade()
    {
        int cost = GetAutoClickCost();

        if (coins >= cost)
        {
            coins -= cost;
            autoClickLevel++;

            // пока просто: 1 уровень = 1 монета в секунду
            autoClickPower = autoClickLevel;

            UpdateCoinsUI();
            UpdateAutoClickUI();
        }
        else
        {
            Debug.Log("Not enough coins for Auto Click Upgrade");
        }
    }

    // нажали BUY у Tree Income (за монеты)
    public void BuyTreeIncomeUpgrade()
    {
        int cost = GetTreeIncomeCost();

        if (coins >= cost)
        {
            coins -= cost;
            treeIncomeLevel++;

            RecalculateTreeIncomePower();

            UpdateCoinsUI();
            UpdateTreeIncomeUI();
        }
        else
        {
            Debug.Log("Not enough coins for Tree Income Upgrade");
        }
    }

    // нажали BUY у Gem Tap Upgrade (за гемы, перманентный буст)
    public void BuyGemTapUpgrade()
    {
        int cost = GetGemTapUpgradeCost();

        if (gems >= cost)
        {
            gems -= cost;
            gemTapLevel++;

            RecalculateTapPower();   // пересчитали силу клика с учётом бонуса
            UpdateGemsUI();
            UpdateGemTapUpgradeUI();
            UpdateClickUpgradeUI();  // чтобы игрок видел, что клик вырос
        }
        else
        {
            Debug.Log("Not enough gems for Gem Tap Upgrade");
        }
    }

    // нажали BUY у временного Tap Boost (за гемы, x2 на 30 секунд)
    public void BuyTapBoost()
    {
        if (gems < tapBoostGemCost)
        {
            Debug.Log("Not enough gems for Tap Boost");
            return;
        }

        gems -= tapBoostGemCost;
        UpdateGemsUI();

        tapBoostActive = true;
        tapBoostTimeLeft = tapBoostDuration;

        RecalculateTapPower(); // усиливаем клик с учётом буста

        Debug.Log($"Tap Boost activated: x{tapBoostMultiplier} for {tapBoostDuration} seconds");
    }

    // ================= ФОРМУЛЫ =================

    // стоимость апгрейда клика (монеты)
    int GetClickUpgradeCost()
    {
        float cost = baseClickUpgradeCost * Mathf.Pow(1.15f, clickLevel);
        return Mathf.RoundToInt(cost);
    }

    // стоимость автокликера (монеты)
    int GetAutoClickCost()
    {
        float cost = baseAutoClickCost * Mathf.Pow(autoClickCostMultiplier, autoClickLevel);
        return Mathf.RoundToInt(cost);
    }

    // стоимость Tree Income (монеты)
    int GetTreeIncomeCost()
    {
        float cost = baseTreeIncomeCost * Mathf.Pow(treeIncomeCostMultiplier, treeIncomeLevel);
        return Mathf.RoundToInt(cost);
    }

    // стоимость Gem Tap Upgrade (гемы)
    int GetGemTapUpgradeCost()
    {
        float cost = baseGemTapCost * Mathf.Pow(gemTapCostMultiplier, gemTapLevel);
        return Mathf.RoundToInt(cost);
    }

    // сила клика (с учётом апгрейда за гемы и временного буста)
    void RecalculateTapPower()
    {
        // базовый клик от обычного Click Power
        float baseTap = 1 + clickLevel;

        // множитель от Gem Tap Upgrade
        float gemMultiplier = 1f + gemTapLevel * gemTapBonusPerLevel; // lvl1 = x1.5, lvl2 = x2.0 и т.д.

        // множитель от временного Tap Boost
        float boostMultiplier = tapBoostActive ? tapBoostMultiplier : 1f;

        float result = baseTap * gemMultiplier * boostMultiplier;
        coinsPerTap = Mathf.RoundToInt(result);
        if (coinsPerTap < 1) coinsPerTap = 1;
    }

    // сила Tree Income
    void RecalculateTreeIncomePower()
    {
        // пока: 1 уровень = 5 монет в секунду
        treeIncomePower = treeIncomeLevel * 5;
    }

    // общий пассивный доход раз в секунду
    void PassiveIncomeTick()
    {
        int totalPassive = autoClickPower + treeIncomePower;
        if (totalPassive <= 0) return;

        coins += totalPassive;
        UpdateCoinsUI();
    }

    // ================= UI ОБНОВЛЕНИЕ =================

    void UpdateCoinsUI()
    {
        if (coinsText != null)
            coinsText.text = FormatNumber(coins);
    }

    void UpdateGemsUI()
    {
        if (gemsText != null)
            gemsText.text = FormatNumber(gems);
    }

    void UpdateClickUpgradeUI()
    {
        if (clickUpgradeLevelText != null)
            clickUpgradeLevelText.text = "Level: " + clickLevel;

        if (clickUpgradeCostText != null)
            clickUpgradeCostText.text = "Cost: " + FormatNumber(GetClickUpgradeCost());
    }

    void UpdateAutoClickUI()
    {
        if (autoClickLevelText != null)
            autoClickLevelText.text = "Level: " + autoClickLevel;

        if (autoClickCostText != null)
            autoClickCostText.text = "Cost: " + FormatNumber(GetAutoClickCost());
    }

    void UpdateTreeIncomeUI()
    {
        if (treeIncomeLevelText != null)
            treeIncomeLevelText.text = "Level: " + treeIncomeLevel;

        if (treeIncomeCostText != null)
            treeIncomeCostText.text = "Cost: " + FormatNumber(GetTreeIncomeCost());
    }

    void UpdateGemTapUpgradeUI()
    {
        if (gemTapLevelText != null)
            gemTapLevelText.text = "Level: " + gemTapLevel;

        if (gemTapCostText != null)
            gemTapCostText.text = "Cost: " + FormatNumber(GetGemTapUpgradeCost()) + "💎";
    }

    // ================= ФОРМАТИРОВАНИЕ ЧИСЕЛ =================
    public static string FormatNumber(long num)
    {
        if (num < 1000)
            return num.ToString(); // 0–999

        if (num < 1_000_000)
            return (num / 1000f).ToString("0.#") + "K"; // 1.2K, 350K

        if (num < 1_000_000_000)
            return (num / 1_000_000f).ToString("0.#") + "M"; // 1.5M

        if (num < 1_000_000_000_000)
            return (num / 1_000_000_000f).ToString("0.#") + "B"; // 2.3B

        return (num / 1_000_000_000_000f).ToString("0.#") + "T"; // 4.7T+
    }

    // ================= SHOP: GEM PACKS =================

    public void BuySmallGemPack()
    {
        AddGems(50);
        Debug.Log("Small Gem Pack bought: +50 gems");
    }

    public void BuyMediumGemPack()
    {
        AddGems(250);
        Debug.Log("Medium Gem Pack bought: +250 gems");
    }

    public void BuyLargeGemPack()
    {
        AddGems(1000);
        Debug.Log("Large Gem Pack bought: +1000 gems");
    }
}
