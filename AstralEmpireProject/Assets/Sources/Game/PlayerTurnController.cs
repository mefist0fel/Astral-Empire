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
    private MarkersCollectionView fireSelector = null;
    [SerializeField]
    private MarkersCollectionView moveSelector = null;
    [SerializeField]
    private float markersDistance = 0.2f;

    private GameUI gameUI = null;
    private Faction currentFaction = null;
    private CameraControlModel cameraModel = null;
    private Game game = null;

    private Dictionary<Unit, Coord> unitTargetPoints = new Dictionary<Unit, Coord>();

    private readonly Coord Empty = new Coord(int.MinValue, int.MinValue);

    private Unit selectedUnit = null;
    private Coord selectedCoord;

    private Coord moveTargetCoord; // coord to move
    private Coord fireTargetCoord; // coord to fire
    private Coord turnTargetCoord; // coord to accept user choice

    private MarkersSet moveZone = new MarkersSet();
    private MarkersSet fireZone = new MarkersSet();

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
        SelectUnit(null);
    }

    private void OnDestroy() {
        gameUI.OnEndTurnClick -= OnEndTurnHandler;
    }

    private void OnEndTurnHandler() {
        game.EndTurn();
    }

    private void OnFieldClick(Vector2 pointerPosition) {
        var floorPosition = cameraController.Raycast(pointerPosition);
        var coord = mapView.CellPositionToCoord(floorPosition);
        OnCellSelect(coord);
    }

    private void OnCellSelect(Coord coord) {
        if (currentFaction == null)
            return;
        var cell = game.Map[coord];
        if (cell == null)
            return;
        ShowStatus(cell, coord);
        if (UnitAlreadySelected(cell, coord)) {
            SelectUnit(null);
            return;
        }
        if (selectedUnit != null && !cell.HasAlliedUnit(selectedUnit)) {
            if (IsPathAccepted(coord)) {
                MakeTurn();
            } else {
                GeneratePath(coord);
            }
        } else {
            SelectUnit(cell.Unit);
        }
    }

    private bool UnitAlreadySelected(Map.Cell cell, Coord coord) {
        return cell.GetUnit() == selectedUnit;
    }

    private void SelectUnit(Unit unit) {
        selectedUnit = unit;
        moveZone.Clear();
        fireZone.Clear();

        fireTargetCoord = Empty;
        moveTargetCoord = Empty;
        turnTargetCoord = Empty;

        moveMarkersView.Hide();
        fireMarkersView.Hide();
        moveZoneMarkersView.Hide();
        selectedMarkersView.Hide();

        moveSelector.Hide();
        fireSelector.Hide();
        if (unit == null) {
            gameUI.ShowUnit(null);
            return;
        }
        if (unit.Faction != currentFaction) {
            selectedUnit = null;
            gameUI.ShowUnit(null);
            return;
        }
        gameUI.ShowUnit(unit);
        selectedCoord = unit.Coordinate;
        selectedMarkersView.Show(mapView.CellCoordToPosition(selectedCoord));
        if (selectedUnit.ActionPoints > 0) {
            moveZone = game.Map.GetMoveZone(unit);
            var moveCoords = moveZone.GetCoordList();
            fireZone = game.Map.GetFireZoneForMoveZone(unit, moveZone);
            var fireCoords = fireZone.GetCoordList();
            moveZoneMarkersView.Show(CoordToPositions(moveCoords));
            fireMarkersView.Show(CoordToPositions(fireCoords));
        }
        if (unitTargetPoints.ContainsKey(selectedUnit)) {
            ShowCurrentPath(selectedUnit, unitTargetPoints[selectedUnit]);
        }
    }

    private bool IsPathAccepted(Coord coord) {
        return coord == turnTargetCoord;
    }

    private void GeneratePath(Coord coord) {
        turnTargetCoord = coord;
        var clickedUnit = game.Map[coord].GetUnit();
        if (clickedUnit != null && fireZone[coord] > 0) {
            // set fire and move markers on this turn
            if (!game.Map.IsCanFireFromCoord(selectedUnit, moveTargetCoord, coord)) {
                moveTargetCoord = game.Map.FindOptimalMovePoint(selectedUnit, moveZone, coord);
            }
            fireTargetCoord = coord;
            // ShowMoveTarget
            moveSelector.Show(mapView.CellCoordToPosition(moveTargetCoord));
            fireSelector.Show(mapView.CellCoordToPosition(fireTargetCoord));
            ShowCurrentPath(selectedUnit, moveTargetCoord);
            return;
        }
        if (moveZone[coord] > 0) {
            // set move marker only on this turn
            moveTargetCoord = coord;
            fireTargetCoord = Empty;
            moveSelector.Show(mapView.CellCoordToPosition(moveTargetCoord));
            fireSelector.Hide();
            ShowCurrentPath(selectedUnit, turnTargetCoord);
            return;
        }
        // find path other one turn
        var pathCoords = game.Map.FindPath(selectedUnit, coord);
        if (pathCoords.Count == 0) {
            moveSelector.Hide();
            fireSelector.Hide();
            return;
        }
        moveTargetCoord = FindFarestPointInPath(pathCoords, moveZone);
        fireTargetCoord = Empty;
        moveSelector.Show(new List<Vector3>() {
            mapView.CellCoordToPosition(moveTargetCoord),
            mapView.CellCoordToPosition(turnTargetCoord)});
        fireSelector.Hide();
        ShowCurrentPath(selectedUnit, turnTargetCoord);
    }

    private void MakeTurn() {
        moveSelector.Hide();
        fireSelector.Hide();
        game.Map.MoveUnit(selectedUnit.Coordinate, moveTargetCoord, moveZone);
        if (game.Map[fireTargetCoord].Unit != null && fireZone[fireTargetCoord] > 0) {
            game.Map.AttackUnit(selectedUnit, game.Map[fireTargetCoord].Unit);
        }
        // ShowSelectionMarker(selectedUnit.Coordinate);
        moveMarkersView.Hide();
        fireMarkersView.Hide();
        moveZoneMarkersView.Hide();
        if (selectedUnit.ActionPoints > 0) {
            SelectUnit(selectedUnit);
        } else {
            SelectUnit(null);
        }
    }

    // private void MoveUnitToTarget(Unit unit, Coord target) {
    //     if (unit.Coordinate == target) {
    //         unitTargetPoints.Remove(unit);
    //         return;
    //     }
    //     var pathCoords = game.Map.FindPath(selectedUnit, target);
    //     if (pathCoords.Count == 0) {
    //         unitTargetPoints.Remove(unit);
    //         return;
    //     }
    //     var moveZone = game.Map.GetMoveZone(unit);
    //     var targetToNearTurn = FindFarestPointInPath(pathCoords, moveZone);
    //     game.Map.MoveUnit(unit.Coordinate, targetToNearTurn, moveZone);
    // }

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

    private void ShowCurrentPath(Unit unit, Coord selectedCoord) {
        var path = game.Map.FindPath(unit, selectedCoord);
        var markersPositions = path.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
        var pathPoints = GetBesiePoints(markersPositions);
        moveMarkersView.Show(pathPoints);
    }


    // private void Old() {
    //
    //     if (selectedUnit == null) {
    //         // First click
    //         SelectPlayerUnit(cell.Unit);
    //     } else {
    //         if (selectedCoord != coord) {
    //             selectedCoord = coord;
    //             if (cell.Unit == null) {
    //                 ShowCurrentPath(selectedUnit, selectedCoord);
    //             } else {
    //                 if (cell.Unit == selectedUnit) { // deselect unit on second click
    //                     SelectUnit(null); // on click selected unit again - deselect
    //                 } else {
    //                     if (cell.Unit.Faction == selectedUnit.Faction) { // on click ally unit - select it
    //                         SelectPlayerUnit(cell.Unit);
    //                     } else { // on click enemy unit - move to it
    //                         ShowCurrentPath(selectedUnit, selectedCoord);
    //                     }
    //                 }
    //             }
    //         } else {
    //             if (cell.Unit == null) {
    //                 if (unitTargetPoints.ContainsKey(selectedUnit)) {
    //                     unitTargetPoints[selectedUnit] = selectedCoord;
    //                 } else {
    //                     unitTargetPoints.Add(selectedUnit, selectedCoord);
    //                 }
    //                 MoveUnitToTarget(selectedUnit, selectedCoord);
    //                 if (selectedUnit.ActionPoints > 0)
    //                     SelectUnit(selectedUnit);
    //                 else
    //                     SelectUnit(null);
    //             } else {
    //                 if (cell.Unit.Faction == selectedUnit.Faction) { // on click ally unit - select it
    //                     if (cell.Unit == selectedUnit) { // deselect unit on second click
    //                         SelectUnit(null);
    //                     } else {
    //                         SelectPlayerUnit(cell.Unit);
    //                     }
    //                 } else {
    //                     MoveUnitToTarget(selectedUnit, selectedCoord);
    //                     if (selectedUnit.ActionPoints > 0)
    //                         SelectUnit(selectedUnit);
    //                     else
    //                         SelectUnit(null);
    //                 }
    //             }
    //         }
    //     }
    //     ShowStatus(cell, coord);
    // }

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
                status += unit.Id + " hp: " + unit.HitPoints + "/" + unit.MaxHitPoints;
            } else {
                status += "enemy hp: " + unit.HitPoints + "/" + unit.MaxHitPoints;
            }
        }
        gameUI.SetStatusText(status);
    }

    public void OnChangeStatus() {}

    public void OnEndTurn() {
        currentFaction = null;
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
