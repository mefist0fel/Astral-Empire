using System;
using Model;
using UnityEngine;

public sealed class PlayerTurnController : MonoBehaviour, Faction.IController {
    [SerializeField]
    private MapView mapView = null;
    [SerializeField]
    private CameraController cameraController = null;

    private GameUI gameUI = null;
    private Faction currentFaction = null;
    private CameraControlModel cameraModel = null;
    private Game game = null;

    public void Init(Game game) {
        this.game = game;
    } 

    private void Start() {
        cameraModel = new CameraControlModel(OnFieldClick);
        gameUI = UILayer.Show<GameUI>();
        gameUI.OnEndTurnClick += OnEndTurnHandler;
    }

    private void OnDestroy() {
        gameUI.OnEndTurnClick -= OnEndTurnHandler;
    }

    private void OnEndTurnHandler() {
        game.EndTurn();
    }

    private void OnFieldClick(Vector2 pointerPosition) {
        var floorPosition = cameraController.Raycast(pointerPosition);
        var pointerCoord = mapView.CellPositionToCoord(floorPosition);
        var cell = game.Map[pointerCoord];
        var status = pointerCoord.ToString() + " " + cell.Type.ToString() + "\n";
        var unit = cell.Unit;
        if (unit != null) {
            if (unit.Faction == currentFaction) {
                status += unit.Name + " hp:" + unit.HitPoints;
            } else {
                status += "enemy hp:" + unit.HitPoints;
            }
        }
        gameUI.SetStatusText(status);
    }

    public void OnChangeStatus() {}

    public void OnEndTurn() {}

    public void OnStartTurn(Faction faction) {
        currentFaction = faction;
        gameUI.SetStatusText(faction.Name + " turn");
    }
	
	private void Update () {
        cameraModel.Process();
    }
}
