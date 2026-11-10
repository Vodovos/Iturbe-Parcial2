using System;
using UnityEngine;
using UnityEngine.ProBuilder;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("--- Estadísticas Base (Balanceables) ---")]
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float fireRate = 0.2f; // Segundos entre disparos automáticos

    [Header("--- Referencias de Disparo ---")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera mainCamera;

    [Header("--- Feedback de Daño (Opcional) ---")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    // Multiplicadores acumulativos modificados por ScriptableObjects
    public float SpeedMultiplier { get; private set; } = 1f;
    public float DamageMultiplier { get; private set; } = 1f;
    public float HealthMultiplier { get; private set; } = 1f;

    // Propiedades públicas para cálculo dinámico
    public float CurrentMaxHealth => baseMaxHealth * HealthMultiplier;
    public float CurrentDamage => baseDamage * DamageMultiplier;
    public float CurrentMoveSpeed => baseMoveSpeed * SpeedMultiplier;
    public float CurrentHealth { get; private set; }

    // Eventos para UI y sistemas externos
    public static event Action<float, float> OnHealthChanged; // (vidaActual, vidaMaxima)
    public static event Action OnPlayerDied;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 aimPoint;
    private float nextFireTime;
    private float lastDamageTime;
    private bool isDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (mainCamera == null) mainCamera = Camera.main;

        // Bloqueo de físicas en Y y rotaciones no deseadas
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
    }

    void Start()
    {
        CurrentHealth = CurrentMaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();
        HandleAiming();
        HandleAutoShooting();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Movimiento omnidireccional normalizado en el plano XZ
        rb.linearVelocity = moveInput * CurrentMoveSpeed;
    }

    // -------------------------------------------------------------
    // INPUT Y ROTACIÓN
    // -------------------------------------------------------------
    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(moveX, 0f, moveZ).normalized;
    }

    private void HandleAiming()
    {
        // Raycast desde la cámara hacia el plano horizontal a la altura del jugador
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            aimPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = new Vector3(aimPoint.x, transform.position.y, aimPoint.z) - transform.position;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    // -------------------------------------------------------------
    // DISPARO AUTOMÁTICO
    // -------------------------------------------------------------
    private void HandleAutoShooting()
    {
        // Dispara de forma continua y automática respetando la cadencia
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Calculamos la dirección desde el firePoint hacia el punto del mouse (aimPoint)
        Vector3 shootDirection = (aimPoint - firePoint.position);
        shootDirection.y = 0f; // Asegura que viaje plano en el plano horizontal

        Quaternion bulletRotation;
        if (shootDirection.sqrMagnitude > 0.001f)
        {
            bulletRotation = Quaternion.LookRotation(shootDirection);
        }
        else
        {
            bulletRotation = transform.rotation;
        }

        // Instancia la bala orientada al cursor
        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, bulletRotation);

        if (bulletObj.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.Initialize(CurrentDamage);
        }
    }

    // -------------------------------------------------------------
    // DAÑO Y SALUD
    // -------------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (isDead || Time.time < lastDamageTime + invulnerabilityDuration) return;

        lastDamageTime = Time.time;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector3.zero;
        OnPlayerDied?.Invoke();
        Debug.Log("Jugador derrotado.");

        // Desactiva el collider para que los enemigos no sigan chocando
        if (TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }

        // Oculta el modelo visual
        if (TryGetComponent<MeshRenderer>(out var mesh))
        {
            mesh.enabled = false;
        }

        // Si tenés objetos hijos con modelos, podés desactivar el GameObject directamente:
        // gameObject.SetActive(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Daño por contacto continuo con enemigos (físicas estándar)
        if (collision.gameObject.TryGetComponent<SimpleEnemyController>(out var enemy))
        {
            TakeDamage(enemy.ContactDamage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Daño por contacto si los colliders de los enemigos son triggers
        if (other.TryGetComponent<SimpleEnemyController>(out var enemy))
        {
            TakeDamage(enemy.ContactDamage);
        }
    }

    // -------------------------------------------------------------
    // MÉTODOS PARA EL SCRIPTABLE OBJECT DE MEJORAS
    // -------------------------------------------------------------
    public void AddSpeedMultiplier(float amount)
    {
        SpeedMultiplier += amount;
    }

    public void AddDamageMultiplier(float amount)
    {
        DamageMultiplier += amount;
    }

    public void AddHealthMultiplier(float amount)
    {
        HealthMultiplier += amount;
        CurrentHealth += baseMaxHealth * amount; // Cura proporcionalmente al ganar vida máxima
        OnHealthChanged?.Invoke(CurrentHealth, CurrentMaxHealth);
    }
}