using Model;
using UnityEngine;

public sealed class CityView : MonoBehaviour {
    private City city;

    public void Init(City controlCity, Vector3 position) {
        city = controlCity;
        transform.localPosition = position;
    }

    private void Start () {}
	
	// private void Update () {}
}
