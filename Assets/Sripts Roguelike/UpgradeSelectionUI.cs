using System.Collections.Generic;
using UnityEngine;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private UpgradeCardUI[] cardSlots = new UpgradeCardUI[3];

    private GameRoundManager roundManager;

    void Awake()
    {
        roundManager = FindFirstObjectByType<GameRoundManager>();
        Close();
    }

    public void Open(List<UpgradeData> randomUpgrades)
    {
        panelRoot.SetActive(true);

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i < randomUpgrades.Count && randomUpgrades[i] != null)
            {
                cardSlots[i].gameObject.SetActive(true);
                cardSlots[i].Setup(randomUpgrades[i], OnUpgradeChosen);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
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