using BepInEx.Configuration;
using System;
using UnityEngine;
namespace var_browser
{
    class Settings
    {
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                if (instance == null) instance = new Settings();
                return instance;
            }
        }

        public ConfigEntry<string> UIKey;
        public ConfigEntry<string> CustomSceneKey;
        public ConfigEntry<string> CategorySceneKey;
        public ConfigEntry<float> UIScale;
        public ConfigEntry<Vector2> UIPosition;
        public ConfigEntry<bool> MiniMode;

        internal static void Init(ConfigFile config)
        {
            Instance.Load(config);
        }
        private void Load(ConfigFile config)
        {
            UIKey = config.Bind<string>("UI", "UIKey", "Ctrl+Shift+V", "Shortcut key for Show/Hide Var Browser.");
            CustomSceneKey = config.Bind<string>("UI", "CustomSceneKey", "Ctrl+Shift+Alpha1", "Shortcut key for open Custom Scene.");
            CategorySceneKey = config.Bind<string>("UI", "CategorySceneKey", "Ctrl+Shift+Alpha2", "Shortcut key for open Category Scene.");
            UIScale = config.Bind<float>("UI", "Scale", 1, "Set UI Scale.");
            UIPosition = config.Bind<Vector2>("UI", "Position", Vector2.zero, "Set UI Position.");
            MiniMode = config.Bind<bool>("UI", "MiniMode", false, "Set Mini Mode.");
        }
    }
}
