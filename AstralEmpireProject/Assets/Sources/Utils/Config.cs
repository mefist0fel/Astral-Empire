using Newtonsoft.Json;
using UnityEngine;

public abstract class Config <T> where T : Config <T> {
    [JsonProperty("self")]
    public string Self { get; private set; }

    public static T LoadFromResources(string path) {
        var asset = Resources.Load<TextAsset>(path);
        if (asset == null) {
            Debug.LogError("No config named " + path + " in resources");
            return null;
        }
        T config = JsonConvert.DeserializeObject<T>(asset.text);
        config.Self = path;
        return config;
    }
}
