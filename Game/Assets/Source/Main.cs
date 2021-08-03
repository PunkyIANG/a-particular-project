using UnityEngine;

namespace SomeProject
{
    public class Main
    {
        [RuntimeInitializeOnLoadMethod]
        private static void SetBuiltinCommands()
        {
            Generated.CommandsInitialization.InitializeBuiltinCommands();
        }
    }
}