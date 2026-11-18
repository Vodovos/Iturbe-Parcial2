using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Widgets de Interfaz")]
    public GameObject battleMenuPanel;     // Panel con los 4 botones (Corte, Contundente, Perforante, Defender)
    public GameObject victoryPanel;        // Panel de fin de nivel con botón Reiniciar
    public GameObject defeatPanel;         // Panel de Game Over con botón Reiniciar

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        battleMenuPanel.SetActive(false);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
    }

    public void ShowBattleUI(bool show)
    {
        battleMenuPanel.SetActive(show);
    }

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);
    }

    public void ShowDefeatScreen()
    {
        defeatPanel.SetActive(true);
    }
}