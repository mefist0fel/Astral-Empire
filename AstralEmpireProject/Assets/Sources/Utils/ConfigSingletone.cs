using Newtonsoft.Json;
using UnityEngine;

public abstract class ConfigSingletone<T> where T: ConfigSingletone<T> {
    private static T instance = null;

    protected static T GetInstance(string path) {
        if (instance == null) {
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null) {
                Debug.LogError("No quest named " + path + " in resources");                
            } else {
                instance = JsonConvert.DeserializeObject<T>(asset.text);
            }
        }
        return instance;
    }
}
