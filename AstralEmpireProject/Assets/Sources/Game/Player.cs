using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System;

/// <summary>
/// Base player data
/// </summary>
namespace Model {
    [Serializable]
    public sealed class Player {

        private const string saveName = "save.json";
        private static string SavePath { get { return Path.Combine(Application.persistentDataPath, saveName); } }

        public static Player Loaded { get; private set; } // todo editor only

        [JsonProperty("items")]
        public Container Items = new Container();

        // Game state - current system, mission and node.
        [JsonProperty("current_system")]
        public string currentSystemId = null;
        [JsonProperty("current_planet")]
        public string currentPlanetId = null;
        [JsonProperty("system")]
        public string systemId = null;

        [JsonProperty("units")]
        public string[] UnitNames = null;
        private const string missionItemTag = "mission/";

        private const int maxAvailableSlots = 7;

        public void CommitSaveChanges() {
            Save();
        }

        public Player() {
            Loaded = this;
        }

        public static Player CreateNew() {
            var player = new Player();
            return player;
        }

        public static Player Load () {
            if (!File.Exists(SavePath)) {
                return null;
            }
            var player = JsonConvert.DeserializeObject<Player>(File.ReadAllText(SavePath));
            Loaded = player;
            return player;
        }

        public void Save() {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}