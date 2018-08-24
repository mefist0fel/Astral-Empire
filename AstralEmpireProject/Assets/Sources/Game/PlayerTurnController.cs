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
    [SerializeField]
    private MarkersCollectionView selectedMarkersView = null;
    [SerializeField]
    private float markersDistance = 0.2f;

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
        gameUI.OnCancelUnitClick += CancelUnitSelectionHandler;
        gameUI.OnEndTurnClick += OnEndTurnHandler;
    }

    private void CancelUnitSelectionHandler() {
        SelectPlayerUnit(null);
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
            SelectPlayerUnit(cell.Unit);
        } else {
            if (selectedCoord != pointerCoord) {
                selectedCoord = pointerCoord;
                if (cell.Unit == null) {
                    ShowCurrentPath(selectedUnit, selectedCoord);
                } else {
                    if (cell.Unit == selectedUnit) { // deselect unit on second click
                        SelectPlayerUnit(null); // on click selected unit again - deselect
                    } else {
                        if (cell.Unit.Faction == selectedUnit.Faction) { // on click ally unit - select it
                            SelectPlayerUnit(cell.Unit);
                        } else { // on click enemy unit - move to it
                            ShowCurrentPath(selectedUnit, selectedCoord);
                        }
                    }
                }
            } else {
                if (cell.Unit == null) {
                    if (unitTargetPoints.ContainsKey(selectedUnit)) {
                        unitTargetPoints[selectedUnit] = selectedCoord;
                    } else {
                        unitTargetPoints.Add(selectedUnit, selectedCoord);
                    }
                    MoveUnitToTarget(selectedUnit, selectedCoord);
                    if (selectedUnit.ActionPoints > 0)
                        SelectPlayerUnit(selectedUnit);
                    else
                        SelectPlayerUnit(null);
                } else {
                    if (cell.Unit.Faction == selectedUnit.Faction) { // on click ally unit - select it
                        if (cell.Unit == selectedUnit) { // deselect unit on second click
                            SelectPlayerUnit(null);
                        } else {
                            SelectPlayerUnit(cell.Unit);
                        }
                    } else {
                        MoveUnitToTarget(selectedUnit, selectedCoord);
                        if (selectedUnit.ActionPoints > 0)
                            SelectPlayerUnit(selectedUnit);
                        else
                            SelectPlayerUnit(null);
                    }
                }
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

    private void SelectPlayerUnit(Unit unit) {
        selectedUnit = unit;
        moveMarkersView.Hide();
        fireMarkersView.Hide();
        selectedMarkersView.Hide();
        if (unit == null) {
            gameUI.ShowUnit(null);
            moveZoneMarkersView.Hide();
            return;
        }
        if (unit.Faction != currentFaction) {
            gameUI.ShowUnit(null);
            moveZoneMarkersView.Hide();
            return;
        }
        gameUI.ShowUnit(unit);
        selectedCoord = unit.Coordinate;
        selectedMarkersView.Show(mapView.CellCoordToPosition(selectedCoord));
        if (selectedUnit.ActionPoints > 0) {
            var moveZone = game.Map.GetMoveZone(unit);
            var moveCoords = moveZone.GetCoordList();
            moveZoneMarkersView.Show(CoordToPositions(moveCoords));
            var fireZone = game.Map.GetFireZoneForMoveZone(unit, moveZone);
            var fireCoords = fireZone.GetCoordList();
            fireMarkersView.Show(CoordToPositions(fireCoords));
        } else {
            moveZoneMarkersView.Hide();
        }
        if (unitTargetPoints.ContainsKey(selectedUnit)) {
            ShowCurrentPath(selectedUnit, unitTargetPoints[selectedUnit]);
        }
    }

    private void ShowCurrentPath(Unit unit, Coord selectedCoord) {
        var path = game.Map.FindPath(unit, selectedCoord);
        var markersPositions = path.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
        var pathPoints = GetBesiePoints(markersPositions);
        moveMarkersView.Show(pathPoints);
    }

    private List<Vector3> GetBesiePoints(List<Vector3> markersPositions) {
        if (markersPositions == null || markersPositions.Count < 2)
            return null;
        var pathBesie = new BesieCurve(markersPositions.ToArray());
        var pointsCount = Mathf.CeilToInt(pathBesie.Lenght / markersDistance);
        var curvedPath = new List<Vector3>(pointsCount);
        for (int i = 0; i < pointsCount; i++) {
            curvedPath.Add(pathBesie.GetPositionOnCurve(i * markersDistance));
        }
        return curvedPath;
    }

    private List<Vector3> CoordToPositions(List<Coord> coordList) {
        return coordList.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
    }

    private void ShowStatus(Map.Cell cell, Coord coord) {
        var status = coord.ToString() + " " + cell.Type.ToString() + "\n";
        var unit = cell.Unit;
        if (unit != null) {
            if (unit.Faction == currentFaction) {
                status += unit.Id + " hp:" + unit.HitPoints;
            } else {
                status += "enemy hp:" + unit.HitPoints;
            }
        }
        gameUI.SetStatusText(status);
    }

    public void OnChangeStatus() {}

    public void OnEndTurn() {
        SelectPlayerUnit(null);
    }

    public void OnStartTurn(Faction faction) {
        currentFaction = faction;
        gameUI.SetStatusText(faction.Name + " turn");
    }
	
	private void Update () {
        cameraModel.Process();
    }
}
