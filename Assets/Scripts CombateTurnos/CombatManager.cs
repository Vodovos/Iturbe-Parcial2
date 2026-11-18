using System.Collections;
using UnityEngine;

public enum DamageType { Corte, Contundente, Perforante }
public enum CombatState { Exploration, PlayerTurn, EnemyTurn, Resolution }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Datos de Combate")]
    public CombatState currentState;
    public DamageType enemyWeakness;
    public int enemyMaxHP = 40;
    public int enemyCurrentHP;
    private bool isDefending = false;
    private OverworldEnemy currentEnemyRef;

    // Control de turnos extra por debilidad (no acumulativo)
    private bool hasExtraTurn = false;

    // Referencias de cámara y movimiento
    private Transform mainCamTransform;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private MonoBehaviour playerMovementScript;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
        currentState = CombatState.Exploration;
    }

    public void StartCombat(OverworldEnemy enemy)
    {
        currentEnemyRef = enemy;
        enemyCurrentHP = enemyMaxHP;
        hasExtraTurn = false;

        // Elegir debilidad aleatoria
        enemyWeakness = (DamageType)Random.Range(0, 3);
        Debug.Log($"¡Combate iniciado! El enemigo es débil a: {enemyWeakness}");

        originalCamPos = mainCamTransform.position;
        originalCamRot = mainCamTransform.rotation;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            if (playerMovementScript != null) playerMovementScript.enabled = false;

            Vector3 midpoint = (playerObj.transform.position + enemy.transform.position) / 2f;
            mainCamTransform.position = midpoint + new Vector3(0f, 3f, -4f);
            mainCamTransform.LookAt(midpoint + Vector3.up * 1f);
        }

        UIManager.Instance.ShowBattleUI(true);
        UpdateUIValues();
        SetPlayerTurn();
    }

    void SetPlayerTurn()
    {
        currentState = CombatState.PlayerTurn;
        UIManager.Instance.SetTurnIndicator("¡Tu Turno!");
    }

    void SetEnemyTurn()
    {
        currentState = CombatState.EnemyTurn;
        UIManager.Instance.SetTurnIndicator("Turno Enemigo...");
        StartCoroutine(EnemyTurnRoutine());
    }

    void UpdateUIValues()
    {
        UIManager.Instance.UpdateHealthBars(GameManager.Instance.currentHP, GameManager.Instance.maxHP, enemyCurrentHP, enemyMaxHP);
    }

    // --- ACCIÓN DEL JUGADOR CON DELAY ---
    public void ExecuteAction(CombatAction action)
    {
        if (currentState != CombatState.PlayerTurn) return;

        // Ocultar botones temporalmente mientras se ejecuta la acción
        
        StartCoroutine(PlayerActionRoutine(action));
    }

    IEnumerator PlayerActionRoutine(CombatAction action)
    {
        // Breve delay de ejecución del ataque
        yield return new WaitForSeconds(0.4f);

        bool hitWeakness = false;

        if (action.isDefense)
        {
            isDefending = true;
            Debug.Log("Te pones en guardia. El próximo daño se reduce a la mitad.");
        }
        else
        {
            int finalDamage = action.baseDamage;

            if (action.damageType == enemyWeakness)
            {
                finalDamage *= 2; // ¡Golpe crítico por debilidad!
                hitWeakness = true;
                Debug.Log($"¡Debilidad acertada con {action.actionName}! Haces {finalDamage} de daño.");
            }
            else
            {
                Debug.Log($"Ataque de {action.actionName}: {finalDamage} de daño.");
            }

            enemyCurrentHP -= finalDamage;
            UpdateUIValues();

            if (enemyCurrentHP <= 0)
            {
                EndCombat(true);
                yield break;
            }
        }

        // Pausa dramática post-impacto
        yield return new WaitForSeconds(0.8f);

        // --- MECÁNICA DE DEBILIDAD Y TURNO EXTRA (NO ACUMULATIVO) ---
        if (hitWeakness && !hasExtraTurn)
        {
            hasExtraTurn = true; // Activamos bandera para que no se encadene infinitamente
            UIManager.Instance.SetTurnIndicator("¡Debilitado! ¡Turno Extra!");
            Debug.Log("¡El enemigo perdió su turno por debilidad! Ganas un turno extra.");

            yield return new WaitForSeconds(1f);

            UIManager.Instance.ShowBattleUI(true);
            SetPlayerTurn(); // Repites tu turno de inmediato
        }
        else
        {
            // Si ya venía de un turno extra o no dio a la debilidad, se resetea y pasa al enemigo
            hasExtraTurn = false;
            SetEnemyTurn();
        }
    }

    // --- TURNO DEL ENEMIGO CON DELAY ---
    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.2f); // Delay antes de que el enemigo golpee

        int enemyDamage = 15;

        if (isDefending)
        {
            enemyDamage /= 2;
            isDefending = false;
        }

        GameManager.Instance.currentHP -= enemyDamage;
        UpdateUIValues();
        Debug.Log($"El enemigo te ataca e inflige {enemyDamage} de daño. Te queda {GameManager.Instance.currentHP} HP.");

        yield return new WaitForSeconds(0.8f); // Delay post-ataque del enemigo

        if (GameManager.Instance.currentHP <= 0)
        {
            UIManager.Instance.ShowBattleUI(false);
            GameManager.Instance.TriggerGameOver();
            yield break;
        }

        UIManager.Instance.ShowBattleUI(true);
        SetPlayerTurn();
    }

    void EndCombat(bool playerWon)
    {
        currentState = CombatState.Exploration;
        UIManager.Instance.ShowBattleUI(false);

        if (mainCamTransform != null)
        {
            mainCamTransform.position = originalCamPos;
            mainCamTransform.rotation = originalCamRot;
        }

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (playerWon)
        {
            currentEnemyRef.Defeated();
            GameManager.Instance.RegisterEnemyDefeated();
        }
    }
}