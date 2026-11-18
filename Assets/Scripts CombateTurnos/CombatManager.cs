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

    // Referencias para la cámara dinámica
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

        // Guardar la posición de la cámara de exploración actual antes de moverla
        originalCamPos = mainCamTransform.position;
        originalCamRot = mainCamTransform.rotation;

        // Desactivar el script de movimiento del jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovementScript = playerObj.GetComponent<PlayerMovement>();
            if (playerMovementScript != null) playerMovementScript.enabled = false;

            // --- POSICIONAR LA CÁMARA PARA EL COMBATE ---
            // Colocar la cámara en un punto medio entre el jugador y el enemigo, elevada y de lado
            Vector3 midpoint = (playerObj.transform.position + enemy.transform.position) / 2f;
            mainCamTransform.position = midpoint + new Vector3(0f, 3f, -4f);

            // Hacer que la cámara mire hacia el punto medio (donde están ambos)
            mainCamTransform.LookAt(midpoint + Vector3.up * 1f);
        }

        // Mostrar UI estilo Pokémon
        UIManager.Instance.ShowBattleUI(true);

        currentState = CombatState.PlayerTurn;
    }

    // --- ACCIONES DEL JUGADOR ---
    public void PlayerAttack(DamageType attackType)
    {
        if (currentState != CombatState.PlayerTurn) return;

        int baseDamage = 12;
        int finalDamage = baseDamage;

        if (attackType == enemyWeakness)
        {
            finalDamage *= 2; // ¡Golpe crítico por debilidad!
            Debug.Log($"¡Debilidad acertada! Haces {finalDamage} de daño de {attackType}.");
        }
        else
        {
            Debug.Log($"Ataque de {attackType}: {finalDamage} de daño.");
        }

        enemyCurrentHP -= finalDamage;

        if (enemyCurrentHP <= 0)
        {
            EndCombat(true);
            return;
        }

        currentState = CombatState.EnemyTurn;
        StartCoroutine(EnemyTurnRoutine());
    }

    public void PlayerDefend()
    {
        if (currentState != CombatState.PlayerTurn) return;

        isDefending = true;
        Debug.Log("Te pones en guardia. El próximo daño se reduce a la mitad.");

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

        // Devolver la cámara a su posición y rotación original de exploración
        if (mainCamTransform != null)
        {
            mainCamTransform.position = originalCamPos;
            mainCamTransform.rotation = originalCamRot;
        }

        // Reactivar movimiento del player
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