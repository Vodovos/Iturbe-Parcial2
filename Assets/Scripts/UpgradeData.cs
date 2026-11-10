using UnityEngine;

public enum UpgradeType
{
    MoveSpeed,
    Damage,
    MaxHealth
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Roguelike/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Identificación")]
    public string upgradeName;

    [Header("Configuración")]
    public UpgradeType type;

    [Range(0.01f, 0.50f)]
    [Tooltip("Incremento porcentual por acumulación (0.05 = +5%)")]
    public float percentageIncrease = 0.05f;

    // Aplica el efecto directamente sobre el PlayerController
    public void ApplyUpgrade(PlayerController player)
    {
        switch (type)
        {
            case UpgradeType.MoveSpeed:
                player.AddSpeedMultiplier(percentageIncrease);
                break;
            case UpgradeType.Damage:
                player.AddDamageMultiplier(percentageIncrease);
                break;
            case UpgradeType.MaxHealth:
                player.AddHealthMultiplier(percentageIncrease);
                break;
        }
    }
}