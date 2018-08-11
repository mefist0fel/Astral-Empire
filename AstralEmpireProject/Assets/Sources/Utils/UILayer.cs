using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UILayer : MonoBehaviour {
    public const string pathToUIPrefabs = "UI/";
    #region Static part
    static Dictionary<System.Type, UILayer> registeredUI = new Dictionary<System.Type, UILayer>();
    static List<UILayer> displayedUILayers = new List<UILayer>();

    public static bool isInitialComponentRegistered = false;
    static Transform mainCanvas;
    static Transform MainCanvas {
        get {
            if (mainCanvas == null) {
                Canvas findedCanvas = GameObject.FindObjectOfType<Canvas>();
                mainCanvas = findedCanvas.transform;
                UILayer layer = mainCanvas.GetComponent<UILayer>();
                if (layer == null) {
                    mainCanvas.gameObject.AddComponent<UILayer>().isNeedDestroyUIBetweenScenes = false;
                }
            }
            return mainCanvas;
        }
    }

    public bool IsShowed {
        get { return displayedUILayers.Contains(this); }
    }

    static void SortLayers() {
        displayedUILayers.Sort((a, b)  => {
            if (a.layer == b.layer)
                return 0;
            if (a.layer > b.layer)
                return 1;
            else
                return -1;
        });
        displayedUILayers.ForEach((layer) => { layer.transform.SetSiblingIndex(displayedUILayers.IndexOf(layer)); });
    }

    static T Create<T>() where T : UILayer {
        if (registeredUI.ContainsKey(typeof(T))) {
            return (T)registeredUI[typeof(T)];
        }

        GameObject UIprefab = Resources.Load<GameObject>(pathToUIPrefabs + typeof(T).Name);
        return (T)InitializeUIGameObject<T>(UIprefab);
    }

    public static T Show<T>(System.Action showingDelegate = null) where T : UILayer {
        HideAll();
        return ShowModal<T>(showingDelegate);
    }

    public static bool IsLayerShowed<T>() where T : UILayer {
        if (registeredUI.ContainsKey(typeof(T))) {
            return displayedUILayers.Contains(registeredUI[typeof(T)]);
        }
        return false;
    }

    public static T Get<T>() where T : UILayer {
        if (registeredUI.ContainsKey(typeof(T))) {
            UILayer layer = registeredUI[typeof(T)];
            if (displayedUILayers.Contains(layer))
                return (T)layer;
        }
        return null;
    }

    public static T ShowModal<T>(System.Action showingDelegate = null) where T : UILayer {
        if (registeredUI.ContainsKey(typeof(T))) {
            UILayer layer = registeredUI[typeof(T)];
            if (!displayedUILayers.Contains(layer)) {
                layer.gameObject.SetActive(true);
                displayedUILayers.Add(layer);
                layer.Show(() => {
                    if (showingDelegate != null) {
                        showingDelegate();
                    }
                    layer.OnShow();
                });
            } else {
                layer.Show(() => {
                    if (showingDelegate != null) {
                        showingDelegate();
                    }
                    layer.OnShow();
                });
            }
            SortLayers();
            return (T)layer;
        } else {
            Create<T>();
            return UILayer.ShowModal<T>(showingDelegate);
        }
    }

    public static void Hide<T>(System.Action onFinishHiding = null) where T : UILayer {
        if (registeredUI.ContainsKey(typeof(T))) {
            if (displayedUILayers.Contains(registeredUI[typeof(T)])) {
                UILayer layer = registeredUI[typeof(T)];
                layer.Hide(() => {
                    layer.gameObject.SetActive(false);
                    layer.OnHide();
                    layer.transform.SetAsLastSibling();
                    displayedUILayers.Remove(layer);
                    if (onFinishHiding != null) {
                        onFinishHiding();
                    }
                });
            }
        }
    }

    public static void HideAll() {
        foreach (UILayer layer in displayedUILayers) {
            if (layer != null) {
                layer.Hide(() => {
                    layer.gameObject.SetActive(false);
                    layer.OnHide();
                    layer.transform.SetAsLastSibling();
                });
            }
        }
        displayedUILayers.Clear();
    }

    static T InitializeUIGameObject<T>(GameObject UIprefab) where T : UILayer {
        if (!isInitialComponentRegistered) {
            RegisterAllChildUI();
        }
        if (registeredUI.ContainsKey(typeof(T))) {
            return (T)registeredUI[typeof(T)];
        }

        GameObject interfaceObject;
        T interfaceComponent;
        if (UIprefab == null) {
            interfaceObject = new GameObject();
            interfaceComponent = interfaceObject.AddComponent<T>();
        } else {
            interfaceObject = Instantiate(UIprefab);
            interfaceComponent = interfaceObject.GetComponent<T>();
        }

        interfaceObject.name = typeof(T).Name;
        interfaceObject.GetComponent<RectTransform>().SetParent(MainCanvas, false);

        registeredUI.Add(typeof(T), interfaceComponent);
        interfaceComponent.Init();
        interfaceObject.SetActive(false);

        return (T)interfaceComponent;
    }

    // TODO - refactor it -  create 2 functions - with and without scene load
    public static void ShowWithLoader<T, LoaderUI>(System.Action loadingDelegate = null, string sceneName = "")
        where T : UILayer
        where LoaderUI : UILayer {
        UILayer.ShowModal<LoaderUI>(() => {
            registeredUI[typeof(LoaderUI)].isNeedDestroyUIBetweenScenes = false;
            AsyncLoad<T>(() => {
                if (loadingDelegate != null) {
                    loadingDelegate();
                }
                registeredUI[typeof(LoaderUI)].isNeedDestroyUIBetweenScenes = true;
                Hide<LoaderUI>();
            }, sceneName);
        });
    }

    public static void AsyncLoad<T>(System.Action loadingDelegate = null, string sceneName = "") where T : UILayer {
        MainCanvas.GetComponent<UILayer>().StartCoroutine(LoadingUIEnumerator<T>(loadingDelegate, sceneName));
    }

    static IEnumerator LoadingUIEnumerator<T>(System.Action loadingDelegate = null, string sceneName = "") where T : UILayer {
        if (sceneName != "" && SceneManager.GetActiveScene().name != sceneName) {
            List<System.Type> deleteUI = new List<System.Type>();
            foreach (var currentUI in registeredUI) {
                if (currentUI.Value.isNeedDestroyUIBetweenScenes) {
                    deleteUI.Add(currentUI.Key);
                }
            }

            foreach (var deleteKey in deleteUI) {
                if (registeredUI.ContainsKey(deleteKey)) {
                    if (displayedUILayers.Contains(registeredUI[deleteKey])) {
                        displayedUILayers.Remove(registeredUI[deleteKey]);
                    }
                    DestroyImmediate(registeredUI[deleteKey].gameObject);
                    registeredUI.Remove(deleteKey);
                }
            }

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            yield return async;
        }

        //		if (registeredUI.ContainsKey (typeof(T)))
        //		{
        //			if (loadingDelegate != null)
        //			{
        //				loadingDelegate();
        //			}
        //			yield break;
        //		}
        //		
        //		ResourceRequest UIrequest = Resources.LoadAsync (pathToUIPrefabs + typeof(T).Name);
        //		yield return UIrequest;
        //		
        //		InitializeUIGameObject<T> ((GameObject)UIrequest.asset);
        UILayer.Show<T>(loadingDelegate);
    }


    public static void RegisterAllChildUI() {
        UILayer[] findedComponents = MainCanvas.GetComponentsInChildren<UILayer>();
        for (int i = 0; i < findedComponents.Length; i++) {
            if (!registeredUI.ContainsKey(findedComponents[i].GetType())) {
                registeredUI.Add(findedComponents[i].GetType(), findedComponents[i]);
            }
        }
        isInitialComponentRegistered = true;
    }

    public static void HideUndisplayed() {
        foreach (UILayer layer in registeredUI.Values) {
            if (!displayedUILayers.Contains(layer)) {
                layer.gameObject.SetActive(false);
            }
        }
    }

    public static void UpdateInterface() {
        foreach (var layer in registeredUI.Values) {
            layer.UpdateState();
        }
    }

    #endregion

    #region Instance part
    public int layer = 0;
    public bool isNeedDestroyUIBetweenScenes = true;

    protected void Register() {
        if (!registeredUI.ContainsKey(this.GetType())) {
            RegisterAllChildUI();
        }
    }

    protected virtual void Awake() {
        Register();
    }

    void Start() {
        Register();
    }

    protected virtual void Init() {
    }

    protected virtual void Show(System.Action onFinishShowing = null) {
        if (onFinishShowing != null) {
            onFinishShowing();
        }
    }

    protected virtual void Hide(System.Action onFinishHiding = null) {
        if (onFinishHiding != null) {
            onFinishHiding();
        }
    }

    protected virtual void OnShow() {
    }

    protected virtual void OnHide() {
    }

    protected virtual void OnDestroy() {
        registeredUI.Remove(this.GetType());
    }

    public virtual void UpdateState() {
    }
    #endregion
}
