using UnityEngine;

namespace SnoopyKnights.Rendering
{
    /// <summary>Ambient chimney smoke on hearth buildings for a lived-in look.</summary>
    public sealed class BuildingSmoke : MonoBehaviour
    {
        Vector2 chimney;
        float nextPuff;

        public void Init(Vector2 chimneyWorld)
        {
            chimney = chimneyWorld;
            nextPuff = Random.Range(0f, 1.5f); // desync chimneys from each other
        }

        void Update()
        {
            nextPuff -= Time.deltaTime;
            if (nextPuff > 0f) return;
            nextPuff = Random.Range(0.9f, 1.5f);
            // Mid-gray reads against both pale roofs and bright grass.
            FadeOutSprite.Spawn(
                chimney + Random.insideUnitCircle * 0.06f,
                SpriteFactory.SoftCircle,
                new Color(0.62f, 0.62f, 0.64f, 0.7f),
                Random.Range(0.24f, 0.38f),
                Random.Range(1.6f, 2.3f),
                new Vector2(Random.Range(-0.04f, 0.12f), 0.4f));
        }
    }
}
