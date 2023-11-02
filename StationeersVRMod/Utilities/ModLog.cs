namespace StationeersVR.Utilities
{
    internal class ModLog
    {
        public static void Error(string line)
        {
            if (ConfigFile.LogLevel >= 0)
            {
                UnityEngine.Debug.LogError("[StationeersVR]: " + line);
            }
        }

        public static void Error(System.Exception line)
        {
            UnityEngine.Debug.LogError("[StationeersVR]: Exception :");
            UnityEngine.Debug.LogException(line);
        }

        public static void Warning(string line)
        {
            if (ConfigFile.LogLevel >= 0)
            {
                UnityEngine.Debug.LogWarning("[StationeersVR]: " + line);
            }
        }

        public static void Info(string line)
        {
            if (ConfigFile.LogLevel >= 1)
            {
                UnityEngine.Debug.Log("[StationeersVR]: " + line);
            }
        }

        public static void Debug(string line)
        {
            if (ConfigFile.LogLevel >= 2)
            {
                UnityEngine.Debug.Log("[StationeersVR]: " + line);
            }
        }
    }
}
