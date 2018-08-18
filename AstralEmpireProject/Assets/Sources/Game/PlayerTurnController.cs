using System;
using Model;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

    private Dictionary<Unit, Coord> unitTargetPoints = new Dictionary<Unit, Coord>();
    private Unit selectedUnit = null;
    private Coord selectedCoord = new Coord();

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
            SelectUnit(cell.Unit);
        } else {
            if (cell.Unit == null) {
                if (selectedCoord == pointerCoord) {
                    if (unitTargetPoints.ContainsKey(selectedUnit)) {
                        unitTargetPoints[selectedUnit] = selectedCoord;
                    } else {
                        unitTargetPoints.Add(selectedUnit, selectedCoord);
                    }
                    MoveUnitToTarget(selectedUnit, selectedCoord);
                    SelectUnit(null);
                } else {
                    selectedCoord = pointerCoord;
                    var path = game.Map.FindPath(selectedUnit, pointerCoord);
                    var markersPositions = path.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
                    moveMarkersView.Show(markersPositions);
                }
            } else {
                SelectUnit(cell.Unit);
            }
        }
        ShowStatus(cell, pointerCoord);
    }

    private void MoveUnitToTarget(Unit unit, Coord target) {
        if (unit.Coordinate == target) {
            unitTargetPoints.Remove(unit);
            return;
        }
        var pathCoords = game.Map.FindPath(selectedUnit, target);
        if (pathCoords.Count == 0) {
            unitTargetPoints.Remove(unit);
            return;
        }
        var moveZone = game.Map.GetMoveZone(unit);
        var targetToNearTurn = FindFarestPointInPath(pathCoords, moveZone);
        game.Map.MoveUnit(unit.Coordinate, targetToNearTurn, moveZone);
    }

    private Coord FindFarestPointInPath(List<Coord> pathCoords, MarkersSet moveZone) {
        var pathCoord = pathCoords.First();
        Coord nextCoord;
        for (int i = 1; i < pathCoords.Count; i++) {
            nextCoord = pathCoords[i];
            if (moveZone[nextCoord] < 0)
                return pathCoord;
            if (!game.Map[nextCoord].HasUnit)
                pathCoord = nextCoord;
        }
        return pathCoord;
    }

    private void SelectUnit(Unit unit) {
        selectedUnit = unit;
        moveMarkersView.Hide();
        if (unit == null) {
            moveZoneMarkersView.Hide();
            return;
        }
        if (unit.Faction != currentFaction) {
            moveZoneMarkersView.Hide();
            return;
        }
        selectedCoord = unit.Coordinate;
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

    public void OnEndTurn() {
        SelectUnit(null);
    }

    public void OnStartTurn(Faction faction) {
        currentFaction = faction;
        gameUI.SetStatusText(faction.Name + " turn");
    }
	
	private void Update () {
        cameraModel.Process();
    }
}
