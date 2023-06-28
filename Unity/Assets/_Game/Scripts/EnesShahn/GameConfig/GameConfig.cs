using UnityEngine;

namespace EnesShahn.Configurations
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public bool EnableDebug = false;
        public string BaseUrl = "http://localhost:5259";

        #region GameConfig Related
        private static string ConfigPath = "Configurations/GameConfig";
        private static GameConfig s_defaultConfig;
        public static GameConfig DefaultConfig => s_defaultConfig ??= Resources.Load<GameConfig>(ConfigPath);
        #endregion
    }
}