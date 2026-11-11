using UnityEngine;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UpgradeCardUI cardSpeed;
    [SerializeField] private UpgradeCardUI cardDamage;
    [SerializeField] private UpgradeCardUI cardHealth;

    private GameRoundManager roundManager;

    void Awake()
    {
        roundManager = FindFirstObjectByType<GameRoundManager>();
        Close();
    }

    public void Open(UpgradeData speed, UpgradeData damage, UpgradeData health)
    {
        panelRoot.SetActive(true);

        cardSpeed.Setup(speed, OnUpgradeChosen);
        cardDamage.Setup(damage, OnUpgradeChosen);
        cardHealth.Setup(health, OnUpgradeChosen);
    }

    private void OnUpgradeChosen(UpgradeData chosen)
    {
        Close();
        roundManager.ApplySelectedUpgrade(chosen);
    }

    public void Close()
    {
        panelRoot.SetActive(false);
    }
}