using Model;
using UnityEngine;

public sealed class PlayerTurnController : MonoBehaviour, Faction.IController {

    private void Start() {
        UILayer.Show<GameUI>();
    }

    public void OnChangeStatus() {}

    public void OnEndTurn() {}

    public void OnStartTurn(Faction faction) {}
	
	private void Update () { }
}
