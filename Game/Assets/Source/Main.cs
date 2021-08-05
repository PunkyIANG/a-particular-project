using UnityEngine;

namespace SomeProject
{
    public class Main
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetBuiltinCommands()
        {
            Generated.CommandsInitialization.InitializeBuiltinCommands();
        }
    }
}