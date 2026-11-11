using TMPro;
using UnityEngine;

public class RoundHUD : MonoBehaviour
{
    [Header("--- Referencias UI ---")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("--- Gestor de Rondas ---")]
    [SerializeField] private GameRoundManager roundManager;

    void Awake()
    {
        if (roundManager == null)
        {
            roundManager = FindFirstObjectByType<GameRoundManager>();
        }
    }

    void Update()
    {
        if (roundManager == null) return;

        if (roundManager.IsRoundActive)
        {
            // Muestra: "OLEADA 1 / 10" (asumiendo 10 rondas del array)
            if (roundText != null)
            {
                roundText.text = $"OLEADA {roundManager.CurrentRoundIndex + 1}";
            }

            // Muestra el tiempo restante redondeado hacia arriba: "15s" o formato "00:15"
            if (timerText != null)
            {
                int secondsRemaining = Mathf.CeilToInt(roundManager.TimeRemaining);
                timerText.text = $"{secondsRemaining}s";
            }
        }
    }
}