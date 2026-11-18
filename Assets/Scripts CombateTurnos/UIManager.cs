using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Paneles Principales")]
    public GameObject battleMenuPanel;     // Los 4 botones de abajo
    public GameObject hudCombateContainer; // El contenedor de las barras y el texto de turno
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Elementos de Combate")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;
    public TMP_Text turnIndicatorText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (battleMenuPanel != null) battleMenuPanel.SetActive(false);
        if (hudCombateContainer != null) hudCombateContainer.SetActive(false); // Apagados al iniciar
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    public void ShowBattleUI(bool show)
    {
        if (battleMenuPanel != null) battleMenuPanel.SetActive(show);
        if (hudCombateContainer != null) hudCombateContainer.SetActive(show); // Se encienden y apagan juntos
    }

    public void UpdateHealthBars(int playerHP, int playerMaxHP, int enemyHP, int enemyMaxHP)
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = (float)playerHP / playerMaxHP;
        }
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.value = (float)enemyHP / enemyMaxHP;
        }
    }

    public void SetTurnIndicator(string message)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = message;
        }
    }

    public void ShowVictoryScreen()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void ShowDefeatScreen()
    {
        if (defeatPanel != null) defeatPanel.SetActive(true);
    }
}