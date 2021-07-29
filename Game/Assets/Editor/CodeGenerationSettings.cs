// The class needs to be in a separate file with a matching name. See:
// https://github.com/hextantstudios/com.hextantstudios.utilities/issues/2
using UnityEditor;
using UnityEngine;
using Hextant;
using Hextant.Editor;

namespace SomeProject.EditorExtensions
{
    [Settings(SettingsUsage.EditorUser, "Code Generation settings")]
    public sealed class CodeGenerationSettings : Settings<CodeGenerationSettings>
    {
        public bool RegenerateOnReload => _regenerateOnReload;
        [SerializeField, Tooltip("Whether to call Kari when the editor restarts")] 
        private bool _regenerateOnReload = true;

        [SettingsProvider]
        static SettingsProvider GetSettingsProvider() => instance.GetSettingsProvider();
    }
}