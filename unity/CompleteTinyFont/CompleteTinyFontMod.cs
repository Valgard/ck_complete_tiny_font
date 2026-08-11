using System.Linq;
using PugMod;
using UnityEngine;

namespace CompleteTinyFont
{
    /// <summary>
    /// Mod bootstrap. The only state this mod owns is the AssetBundle handle —
    /// the font atlas lives there. The actual work happens in
    /// <see cref="ThinTinyFontPatch"/>, driven by a Harmony postfix on
    /// <c>TextManager.Init2</c>.
    /// </summary>
    public sealed class CompleteTinyFontMod : IMod
    {
        public const string Name = "Complete Tiny Font";

        /// <summary>The mod's bundle; the atlas is loaded from it.</summary>
        public static AssetBundle AssetBundle { get; private set; }

        public void EarlyInit()
        {
            var modInfo = API.ModLoader.LoadedMods.FirstOrDefault(m => m.Handlers.Contains(this));
            if (modInfo != null && modInfo.AssetBundles != null && modInfo.AssetBundles.Count > 0)
                AssetBundle = modInfo.AssetBundles[0];
            else
                Debug.LogWarning($"[{Name}] AssetBundle not available — the font cannot be replaced");
        }

        public void Init()
        {
            // Late-arrival path: if TextManager.Init2 already ran before this mod
            // was initialised, the postfix never fired. TryApply is idempotent,
            // so calling it here costs nothing in the normal ordering.
            ThinTinyFontPatch.TryApply();
            Debug.Log($"[{Name}] loaded (font applied: {ThinTinyFontPatch.Applied}).");
        }

        public void ModObjectLoaded(Object obj) { }

        public void Shutdown() { }

        public void Update() { }
    }
}
