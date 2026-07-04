using UnityEngine;

namespace SnoopyKnights.Combat
{
    /// <summary>Anything that can be attacked: units and buildings.</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        Vector2 AimPoint { get; }
        void TakeDamage(int amount);
    }
}
