using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesRegistrator : MonoBehaviour {
    [SerializeField]
    private State[] statesForRegistration = null;

	private void Awake () {
        if (statesForRegistration != null) {
            foreach (var state in statesForRegistration) {
                if (state != null) {
                    state.Register();
                }
            }
        }
	}
}
