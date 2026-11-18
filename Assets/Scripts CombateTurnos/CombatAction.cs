using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatAction", menuName = "Combat/ActionData")]
public class CombatAction : ScriptableObject
{
    public string actionName;       // Nombre de la acción (Ej: "Espada de Corte")
    public DamageType damageType;   // Corte, Contundente o Perforante
    public int baseDamage;          // Daño base que inflige
    public bool isDefense;          // Verdadero si es el movimiento de bloqueo/defensa
}