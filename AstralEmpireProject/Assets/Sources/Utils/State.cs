using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base game state class - main scene control object.
/// There is only one state can be active in single time moment
/// States can be changed by call GameState.Show<CustomState>() or GameState.LoadScene<CustomState>(SceneName)
/// Each state have complex of lifecircle methods - you need override it to implement need behavior
/// </summary>
public class State : MonoBehaviour {
    #region Static part
    private static Dictionary<System.Type, State> registeredStates = new Dictionary<System.Type, State>();
    private static State currentState = null;

    public static State Current {
        get {
            return currentState;
        }
    }

    public static bool IsCurrentState<T>() where T : State {
        if (currentState != null && currentState is T) {
            return true;
        }
        return false;
    }
    // TODO make Show<T> ( params = null) with optional implement params
    // params int[] list // https://msdn.microsoft.com/en-us/library/ms228391(v=vs.90).aspx
    public static T Show<T>() where T : State {
        HideCurrentScene();
        currentState = registeredStates.ContainsKey(typeof(T)) ? registeredStates[typeof(T)] : Create<T>();
        currentState.gameObject.SetActive(true);
        currentState.OnStateEntry();
        return (T)currentState;
    }
    // TODO Async load level with interface layer Loader
    //	public static void LoadScene<T, LoaderUI>(string sceneName = "")
    //		where T : UILayer
    //		where LoaderUI : UILayer
    //	{
    //		UILayer.ShowModal<LoaderUI> (() =>
    //		{
    //			registeredUI[typeof(LoaderUI)].isNeedDestroyUIBetweenScenes = false;
    //			AsyncLoad<T> (() =>
    //			{
    //				if (loadingDelegate != null)
    //				{
    //					loadingDelegate();
    //				}
    //				registeredUI[typeof(LoaderUI)].isNeedDestroyUIBetweenScenes = true;
    //				Hide<LoaderUI>();
    //			}, sceneName);
    //		});
    //	}

    static T Create<T>() where T : State {
        T state = Utils.Create<T>(typeof(T).Name);
        if (!registeredStates.ContainsKey(typeof(T))) {
            registeredStates.Add(typeof(T), state);
        }
        return state;
    }

    static void HideCurrentScene() {
        if (currentState != null)  // hide old state
        {
            currentState.OnStateLeave();
            currentState.gameObject.SetActive(false);
        }
    }

    protected static void Register(State state) {
        if (!registeredStates.ContainsKey(state.GetType())) {
            registeredStates.Add(state.GetType(), state);
            state.OnStateRegistered();
            if (state.stateByDefault) {
                HideCurrentScene();
                currentState = state;
                currentState.OnStateEntry();
            } else {
                state.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region Instance part
    [SerializeField] bool stateByDefault = false; // show this state by default in this scene

    public bool IsShowed {
        get { return currentState == this; }
    }

    public void Register() {
        Register(this);
    }

    protected virtual void Awake() {
        Register();
    }

    void Start() {
        Register();
    }

    // Lifecircle
    // Init - once on create // can use Start
    // then call GameState.Show<>()
    // oldState StartHiding - use for state out animation, need call onFinishHiding then its done
    // oldState OnHide - use to correct destroying/disabling/stopping state objects
    // newState StartShowing - use for scene start animations

    protected virtual void OnStateRegistered() { }

    protected virtual void OnShowWithParams(object parameters) {  // use for showing/create and activate  state objects with params
    }

    protected virtual void OnStateEntry() {  // use for showing/create and activate  state objects
    }

    protected virtual void OnStateLeave() {  // use to correct destroying/disabling/stopping state objects
    }

    protected virtual void OnDestroy() {
        registeredStates.Remove(this.GetType());
    }

    public virtual void UpdateState() {
    }
    #endregion
}
