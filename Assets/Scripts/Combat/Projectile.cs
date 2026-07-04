using SnoopyKnights.Rendering;
using UnityEngine;

namespace SnoopyKnights.Combat
{
    /// <summary>A homing arrow. If the target dies mid-flight it fizzles at the last position.</summary>
    public sealed class Projectile : MonoBehaviour
    {
        const float Speed = 9f;
        const float Lifetime = 3f;

        IDamageable target;
        Vector2 lastKnown;
        int damage;
        float life;

        public static void Spawn(Vector2 from, IDamageable target, int damage)
        {
            var go = new GameObject("Arrow");
            go.transform.position = from;
            var p = go.AddComponent<Projectile>();
            p.target = target;
            p.lastKnown = target.AimPoint;
            p.damage = damage;

            var sr = SpriteFactory.NewRenderer(go.transform, "Sprite", SpriteFactory.Square,
                new Color(0.95f, 0.93f, 0.8f), SortLayer.Projectile);
            sr.transform.localScale = new Vector3(0.3f, 0.08f, 1f);
        }

        void Update()
        {
            life += Time.deltaTime;
            if (life > Lifetime) { Destroy(gameObject); return; }

            if (target != null && !target.Equals(null) && target.IsAlive)
                lastKnown = target.AimPoint;

            Vector2 pos = transform.position;
            Vector2 dir = lastKnown - pos;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            Vector2 next = Vector2.MoveTowards(pos, lastKnown, Speed * Time.deltaTime);
            transform.position = next;

            if ((next - lastKnown).sqrMagnitude < 0.02f)
            {
                if (target != null && !target.Equals(null) && target.IsAlive)
                    target.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
