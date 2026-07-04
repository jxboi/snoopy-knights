using UnityEditor;
using UnityEngine;

namespace SnoopyKnights.EditorTools
{
    /// <summary>
    /// Applies crisp pixel-art import settings to everything under
    /// Resources/Art (point filter, 16 px/unit, uncompressed). Building sprites
    /// get a bottom-center pivot so they can be anchored to their footprint and
    /// extend upward like top-down structures.
    /// </summary>
    public sealed class ArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Resources/Art/")) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)(path.Contains("/Art/buildings/")
                ? SpriteAlignment.BottomCenter
                : SpriteAlignment.Center);
            ti.SetTextureSettings(s);
        }
    }
}
