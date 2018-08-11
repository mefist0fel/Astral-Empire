using UnityEngine;

public static class ComponentExtention {

    public static void SetActive(this Component component, bool show = true) {
        if (component != null) {
            component.gameObject.SetActive(show);
        }
    }
}
