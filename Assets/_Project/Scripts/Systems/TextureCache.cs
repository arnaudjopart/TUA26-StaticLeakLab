using System.Collections.Generic;
using StaticLeakLab.Models;
using UnityEngine;

namespace StaticLeakLab.Systems
{
    /// <summary>
    /// HAZARD #7 — Static cache of runtime-created native assets (real memory leak).
    ///
    /// Textures created with "new Texture2D" are NOT owned by the scene, so they are not
    /// destroyed when Play mode ends. With Domain Reload enabled the dictionary disappears
    /// with the old AppDomain, the textures become unreferenced and the asset GC
    /// (Resources.UnloadUnusedAssets, run when the scene loads) frees them.
    /// With Domain Reload DISABLED, the static dictionary keeps a managed reference to
    /// every texture, so the asset GC can never release them. On top of that the cache
    /// key is the entity id, which keeps increasing across sessions (see IdGenerator),
    /// so the cache never hits: every Play session adds MaxEnemies x 256x256 RGBA32
    /// textures (~256 KB each) of native memory that is never released.
    ///
    /// Open the Memory Profiler (or watch "Texture memory" on the dashboard) across
    /// several Play sessions to see the leak.
    /// </summary>
    public static class TextureCache
    {
        public const int Size = 256;

        private static readonly Dictionary<int, Texture2D> s_Cache = new Dictionary<int, Texture2D>();

        public static int Count => s_Cache.Count;

        public static long ApproximateBytes => (long)s_Cache.Count * Size * Size * 4;

        public static Texture2D GetOrCreate(int entityId)
        {
            if (s_Cache.TryGetValue(entityId, out var cached) && cached != null)
                return cached;

            var tint = Color.HSVToRGB(GameRandom.Value(), 0.6f, 1f);
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = $"Procedural_{entityId}",
                filterMode = FilterMode.Point
            };

            var pixels = new Color32[Size * Size];
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var checker = ((x / 32) + (y / 32)) % 2 == 0;
                pixels[y * Size + x] = checker ? tint : Color.Lerp(tint, Color.black, 0.5f);
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            s_Cache[entityId] = texture;
            return texture;
        }
    }
}
