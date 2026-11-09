using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleEnemyController : MonoBehaviour
{
    [Header("Ajustes de Enemigo")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float maxHealth = 30f;

    public float ContactDamage => contactDamage;
    private float currentHealth;
    private Transform playerTransform;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Movimiento en XZ directo al jugador
        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        rb.linearVelocity = direction * speed;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
}