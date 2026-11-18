using UnityEngine;

public class OverworldEnemy : MonoBehaviour
{
    public int enemyID; // Asigna 1 al primero y 2 al segundo en el Inspector
    public float triggerRange = 3f;
    private Transform player;
    private bool triggered = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (triggered) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= triggerRange)
        {
            triggered = true;
            GameManager.Instance.activeEnemyID = enemyID;

            // Activar sistema de combate y cámaras
            CombatManager.Instance.StartCombat(this);
        }
    }

    public void Defeated()
    {
        gameObject.SetActive(false); // Desaparece del mapa
    }
}