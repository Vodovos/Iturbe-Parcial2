using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Ajustes de Bala (Editables)")]
    [Tooltip("Velocidad constante de desplazamiento")]
    [SerializeField] private float speed = 18f;

    [Tooltip("Tiempo en segundos antes de autodestruirse si no impacta")]
    [SerializeField] private float lifetime = 7f;

    private float damage;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Se destruye automáticamente al agotarse el lifetime
        Destroy(gameObject, lifetime);
    }

    // Inicializa el daño inyectado por el Player y fija la velocidad
    public void Initialize(float damageAmount)
    {
        damage = damageAmount;
        rb.linearVelocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Al chocar con el enemigo, le hace daño y se autodestruye
        if (other.TryGetComponent<SimpleEnemyController>(out var enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            // Opcional: destruir si choca contra paredes/límites del escenario
            Destroy(gameObject);
        }
    }
}