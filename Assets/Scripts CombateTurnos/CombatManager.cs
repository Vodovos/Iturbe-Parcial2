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

            // Posicionar la cámara en el punto medio entre el jugador y el enemigo
            Vector3 midpoint = (playerObj.transform.position + enemy.transform.position) / 2f;
            mainCamTransform.position = midpoint + new Vector3(0f, 3f, -4f);
            mainCamTransform.LookAt(midpoint + Vector3.up * 1f);
        }

        UIManager.Instance.ShowBattleUI(true);
        currentState = CombatState.PlayerTurn;
    }

    // --- ACCIÓN GENERAL DEL JUGADOR USANDO SCRIPTABLE OBJECTS ---
    public void ExecuteAction(CombatAction action)
    {
        if (currentState != CombatState.PlayerTurn) return;

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
                Debug.Log($"¡Debilidad acertada con {action.actionName}! Haces {finalDamage} de daño.");
            }
            else
            {
                Debug.Log($"Ataque de {action.actionName}: {finalDamage} de daño.");
            }

            enemyCurrentHP -= finalDamage;

            if (enemyCurrentHP <= 0)
            {
                EndCombat(true);
                return;
            }
        }

        currentState = CombatState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    // --- TURNO DEL ENEMIGO ---
    IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.2f);

        int enemyDamage = 15;

        if (isDefending)
        {
            enemyDamage /= 2;
            isDefending = false;
        }

        GameManager.Instance.currentHP -= enemyDamage;
        Debug.Log($"El enemigo te ataca e inflige {enemyDamage} de daño. Te queda {GameManager.Instance.currentHP} HP.");

        if (GameManager.Instance.currentHP <= 0)
        {
            UIManager.Instance.ShowBattleUI(false);
            GameManager.Instance.TriggerGameOver();
            yield break;
        }

        currentState = CombatState.PlayerTurn;
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