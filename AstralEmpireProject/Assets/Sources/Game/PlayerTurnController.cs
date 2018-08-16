using System;
using Model;
using UnityEngine;
using System.Linq;

public sealed class PlayerTurnController : MonoBehaviour, Faction.IController {
    [SerializeField]
    private MapView mapView = null;
    [SerializeField]
    private CameraController cameraController = null;
    [SerializeField]
    private MarkersCollectionView moveZoneMarkersView = null;
    [SerializeField]
    private MarkersCollectionView moveMarkersView = null;
    [SerializeField]
    private MarkersCollectionView fireMarkersView = null;

    private GameUI gameUI = null;
    private Faction currentFaction = null;
    private CameraControlModel cameraModel = null;
    private Game game = null;

    private Unit selectedUnit;

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
        if (selectedUnit == null) {
            if (cell.Unit != null && cell.Unit.Faction == currentFaction) {
                SelectUnit(cell.Unit);
            }
        } else {
            if (cell.Unit == null) {
                // var path2 = game.Map.FindPathDijkstra(selectedUnit.Coordinate, pointerCoord, selectedUnit.moveTerrainMask, selectedUnit.Faction);
                // var markers2Positions = path2.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
                // fireMarkersView.Show(markers2Positions);
                var path = game.Map.FindPath(selectedUnit.Coordinate, pointerCoord, selectedUnit.moveTerrainMask, selectedUnit.Faction);
                var markersPositions = path.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
                moveMarkersView.Show(markersPositions);
            } else {
                SelectUnit(cell.Unit);
            }
        }
        ShowStatus(cell, pointerCoord);
    }

    private void SelectUnit(Unit unit) {
        selectedUnit = unit;
        var moveMarkers = game.Map.GetMoveZone(unit);
        var markersPositions = moveMarkers.GetCoordList().Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
        moveZoneMarkersView.Show(markersPositions);
    }

    private void ShowStatus(Map.Cell cell, Coord coord) {
        var status = coord.ToString() + " " + cell.Type.ToString() + "\n";
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
