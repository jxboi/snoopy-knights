using System.IO;
using UnityEditor;
using UnityEngine;

namespace SnoopyKnights.EditorTools
{
    /// <summary>
    /// Applies crisp pixel-art import settings to everything under
    /// Resources/Art (point filter, 16 px/unit, uncompressed). Building sprites
    /// get a bottom-center pivot so they can be anchored to their footprint and
    /// extend upward like top-down structures.
    ///
    /// Animated units live one folder deeper — Resources/Art/units/&lt;folder&gt;/
    /// &lt;action&gt;.png — and stand on their feet (bottom-center). A png there
    /// whose name is NOT already numbered is treated as a horizontal strip of
    /// square frames and auto-sliced into &lt;action&gt;_0, _1, ... (see
    /// SpriteBank.Clip). Numbered frames are imported as-is.
    /// </summary>
    public sealed class ArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/Resources/Art/")) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.alphaIsTransparency = true;

            bool unitAnim = IsUnitAnim(path);
            ti.spriteImportMode = (unitAnim && !IsNumberedFrame(path))
                ? SpriteImportMode.Multiple  // a strip; sliced in OnPostprocessTexture
                : SpriteImportMode.Single;

            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            bool feet = path.Contains("/Art/buildings/") || unitAnim;
            s.spriteAlignment = (int)(feet ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            ti.SetTextureSettings(s);
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            string path = assetPath.Replace('\\', '/');
            if (!IsUnitAnim(path) || IsNumberedFrame(path)) return;

            var ti = (TextureImporter)assetImporter;
            if (ti.spriteImportMode != SpriteImportMode.Multiple) return;

            int frame = texture.height;
            if (frame <= 0) return;
            int count = Mathf.Max(1, texture.width / frame);
            string name = Path.GetFileNameWithoutExtension(assetPath);

            var meta = new SpriteMetaData[count];
            for (int i = 0; i < count; i++)
                meta[i] = new SpriteMetaData
                {
                    name = name + "_" + i,
                    rect = new Rect(i * frame, 0, frame, frame),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f)
                };
#pragma warning disable CS0618 // SpriteMetaData: still the supported path for postprocess slicing.
            ti.spritesheet = meta;
#pragma warning restore CS0618
        }

        /// <summary>units/&lt;folder&gt;/&lt;file&gt;.png — an animated unit clip (two levels deep).</summary>
        static bool IsUnitAnim(string path)
        {
            const string marker = "/Resources/Art/";
            int i = path.IndexOf(marker);
            if (i < 0) return false;
            string rel = path.Substring(i + marker.Length);
            var parts = rel.Split('/');
            return parts.Length == 3 && parts[0] == "units";
        }

        /// <summary>File ends in _&lt;digits&gt; — an individual numbered frame, not a strip.</summary>
        static bool IsNumberedFrame(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            int u = name.LastIndexOf('_');
            if (u < 0 || u == name.Length - 1) return false;
            for (int i = u + 1; i < name.Length; i++)
                if (!char.IsDigit(name[i])) return false;
            return true;
        }
    }
}
