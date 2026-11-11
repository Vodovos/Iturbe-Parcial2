using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private Button selectButton;

    private UpgradeData currentData;
    private Action<UpgradeData> onSelectedCallback;

    public void Setup(UpgradeData data, Action<UpgradeData> onSelected)
    {
        currentData = data;
        onSelectedCallback = onSelected;

        if (titleText != null)
            titleText.text = data.upgradeName;

        if (percentageText != null)
            percentageText.text = $"+{Mathf.RoundToInt(data.percentageIncrease * 100)}%";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onSelectedCallback?.Invoke(currentData);
    }
}