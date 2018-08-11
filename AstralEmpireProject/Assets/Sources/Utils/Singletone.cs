using Newtonsoft.Json;
using UnityEngine;

public abstract class Singletone<T> where T: Singletone<T>, new() {
    private static T instance = null;

    protected static T GetInstance() {
        if (instance == null) {
            instance = new T();
        }
        return instance;
    }
}
