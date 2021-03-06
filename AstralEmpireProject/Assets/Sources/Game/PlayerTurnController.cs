﻿using System;
using Model;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

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

    private DebugAction debugAction = DebugAction.None;

    private enum DebugAction {
        None = 0,
        PutBlueInfantry,
        PutRedInfantry
    }

    public interface IControlAction {
        void OnSelectCell(Coord coord);
        bool IsEnded();
    }

    public void OnStartGame(Game game) {
        this.game = game;
    } 

    private void Start() {
        cameraModel = new CameraControlModel(OnFieldClick);
        gameUI = UILayer.Show<GameUI>();
        gameUI.OnCancelUnitClick += CancelUnitSelectionHandler;
        gameUI.OnEndTurnClick += OnEndTurnClickHandler;
    }

    private void CancelUnitSelectionHandler() {
        SelectUnit(null, null);
    }

    private void OnDestroy() {
        gameUI.OnEndTurnClick -= OnEndTurnClickHandler;
    }

    private void OnEndTurnClickHandler() {
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
        if (MakeDebugAction(coord)) {
            debugAction = DebugAction.None;
            return;
        }
        ShowStatus(cell, coord);
        if (UnitAlreadySelected(cell, coord)) {
            SelectUnit(null, cell.City);
            return;
        }
        if (selectedUnit != null && !cell.HasAlliedUnit(selectedUnit)) {
            if (IsPathAccepted(coord)) {
                MakeTurn();
            } else {
                GeneratePath(coord);
            }
        } else {
            SelectUnit(cell.Unit, cell.City);
        }
    }

    private bool UnitAlreadySelected(Cell cell, Coord coord) {
        return cell.GetUnit() == selectedUnit;
    }

    private void SelectUnit(Unit unit, City city) {
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
            if (city != null && city.Faction == currentFaction) {
                CityUI.ShowCityUI(city, game);
            }
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
        var clickedCity = game.Map[coord].City;
        if (clickedCity != null && fireZone[coord] > 0) {
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
        // find path for multiple turns
        moveTargetCoord = FindFarestPointInPath(pathCoords, moveZone);
        fireTargetCoord = Empty;
        moveSelector.Show(new List<Vector3>() {
            mapView.CellCoordToPosition(moveTargetCoord),
            mapView.CellCoordToPosition(turnTargetCoord)});
        fireSelector.Hide();
        ShowCurrentPath(selectedUnit, turnTargetCoord);
    }

    private void MakeTurn() {
        Debug.LogError("Make turn");
        moveSelector.Hide();
        fireSelector.Hide();
        game.Map.MoveUnit(selectedUnit.Coordinate, moveTargetCoord, moveZone);
        if (game.Map[fireTargetCoord].HasUnit && fireZone[fireTargetCoord] > 0) {
            game.Map.AttackUnit(selectedUnit, game.Map[fireTargetCoord].Unit);
        }
        if (game.Map[fireTargetCoord].HasCity && fireZone[fireTargetCoord] > 0) {
            game.Map.AttackCity(selectedUnit, game.Map[fireTargetCoord].City);
        }
        // ShowSelectionMarker(selectedUnit.Coordinate);
        moveMarkersView.Hide();
        fireMarkersView.Hide();
        moveZoneMarkersView.Hide();
        if (moveTargetCoord != turnTargetCoord) {
            AddToTargetPoint(selectedUnit, turnTargetCoord);
        } else {
            RemoveTargetPoint(selectedUnit);
        }
        if (selectedUnit.ActionPoints > 0) {
            SelectUnit(selectedUnit, null);
        } else {
            SelectUnit(null, null);
        }
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

    private void ShowCurrentPath(Unit unit, Coord selectedCoord) {
        var path = game.Map.FindPath(unit, selectedCoord);
        var markersPositions = path.Select((coord) => mapView.CellCoordToPosition(coord)).ToList();
        var pathPoints = GetBesiePoints(markersPositions);
        moveMarkersView.Show(pathPoints);
    }

    private void AddToTargetPoint(Unit unit, Coord coord) {
        if (unitTargetPoints.ContainsKey(unit)) {
            unitTargetPoints[unit] = coord;
        } else {
            unitTargetPoints.Add(unit, coord);
        }
    }

    private void RemoveTargetPoint(Unit unit) {
        if (unitTargetPoints.ContainsKey(unit))
            unitTargetPoints.Remove(unit);
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

    private void ShowStatus(Cell cell, Coord coord) {
        var status = new StringBuilder();
        status.AppendFormat("{0} {1}\n", cell.Type, coord);
        //var status = cell.Type.ToString() + coord.ToString() + "\n";
        var city = cell.City;
        if (city != null) {
            if (city.Faction == currentFaction) {
                status.AppendFormat("Allied city: \"{0}\"\n", city.Name);
            } else {
                if (city.Faction == game.NeutralFaction) {
                    status.AppendFormat("Neutral city: \"{0}\"\n", city.Name);
                } else {
                    status.AppendFormat("Enemy city: \"{0}\"\n", city.Name);
                }
            }
        }
        var unit = cell.Unit;
        if (unit != null) {
            if (unit.Faction == currentFaction) {
                status.AppendFormat("{0} hp:{1}/{2}\n", unit.Id, unit.HitPoints, unit.MaxHitPoints);
            } else {
                status.AppendFormat("Enemy hp:{1}/{2}\n", unit.Id, unit.HitPoints, unit.MaxHitPoints);
            }
        }
        gameUI.SetStatusText(status.ToString());
    }

    public void OnChangeStatus() {}

    public void OnEndTurn() {
        currentFaction = null;
        SelectUnit(null, null);
    }

    public void OnStartTurn(Faction faction) {
        currentFaction = faction;
        gameUI.SetStatusText(faction.Name + " turn");
    }

    private bool MakeDebugAction(Coord coord) {
        switch (debugAction) {
            default:
            case DebugAction.None:
                return false;
            case DebugAction.PutBlueInfantry:
                if (CreateUnit(coord, game.Projects.Infantry.UnitData, game.Factions[0]))
                    debugAction = DebugAction.None;
                break;
            case DebugAction.PutRedInfantry:
                if (CreateUnit(coord, game.Projects.Infantry.UnitData, game.Factions[1]))
                    debugAction = DebugAction.None;
                break;
        }
        return true;
    }

    private bool CreateUnit(Coord coord, Unit.Data unitData, Faction faction) {
        var cell = game.Map[coord];
        if (cell == null || cell.HasUnit)
            return false;
        game.CreateUnit(game.Projects.Infantry.UnitData, coord, faction);
        return true;
    }

    private void Update() {
        cameraModel.Process();
        if (Input.GetKeyUp(KeyCode.F1)) {
            debugAction = DebugAction.PutBlueInfantry;
            gameUI.SetStatusText(debugAction.ToString());
            SelectUnit(null, null);
        }
        if (Input.GetKeyUp(KeyCode.F2)) {
            debugAction = DebugAction.PutRedInfantry;
            gameUI.SetStatusText(debugAction.ToString());
            SelectUnit(null, null);
        }
    }
}