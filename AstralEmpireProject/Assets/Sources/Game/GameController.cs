using System;
using Model;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour, Game.IGameController {
    [SerializeField]
    private int wight = 51;
    [SerializeField]
    private int height = 51;
    [SerializeField]
    private MapView mapView; // Set from editor
    [SerializeField]
    private PlayerTurnController playerController; // Set from editor

    private Game game;
    private Dictionary<Unit, UnitView> unitViews = new Dictionary<Unit, UnitView>();

    private void Start () {
        var factions = new Faction[] {
            new Faction(playerController, 0, Color.blue, Color.white, "Blue player"),
            new Faction(playerController, 1, Color.red, Color.black, "Red player")
        };

        var map = new Map(GenerateCells(wight, height));
        map.OnAction += OnAddActionHandler;
        game = new Game(this, map, factions);
        mapView.Init(map);
        playerController.Init(game);
        CameraController.SetBorders(mapView.GetBorders(map));
        CreateDummyUnits(10);
    }

    private Cell[,] GenerateCells(int widht, int height) {
        var cells = new Cell[widht, height];
        for (int i = 0; i < widht; i++) {
            for (int j = 0; j < height; j++) {
                cells[i, j] = new Cell();
            }
        }
        for (int i = 2; i < widht - 2; i++) {
            for (int j = 2; j < height - 2; j++) {
                if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                    continue;
                }
                cells[i, j].Type = MoveType.Land;
            }
        }
        for (int i = 2; i < widht - 2; i++) {
            for (int j = 2; j < height - 2; j++) {
                if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                    continue;
                }
                cells[i, j].Type = MoveType.Land;
                if (Random.Range(0, 8) == 0) {
                    cells[i, j].Type = MoveType.Rough;
                    cells[i, j].MoveCost = 2;
                }
                if (Random.Range(0, 16) == 0) {
                    cells[i, j].Type = MoveType.Mountains;
                }
            }
        }
        return cells;
    }

    private void OnAddActionHandler(Map.AbstractAction action) {
        if (action is MoveAction) {
            var moveAction = action as MoveAction;
            unitViews[moveAction.Unit].MoveTo(moveAction.Path.Select(coord => mapView.CellCoordToPosition(coord)).ToArray());
        }
        if (action is AttackAction) {
            var attackAction = action as AttackAction;
            unitViews[attackAction.AttackerUnit].Attack(unitViews[attackAction.DefenciveUnit], attackAction.Damage);
        }
        if (action is DeathAction) {
            var deathAction = action as DeathAction;
            unitViews[deathAction.Unit].Death();
            unitViews.Remove(deathAction.Unit);
        }
    }

    private void CreateDummyUnits(int count) {
        foreach (var faction in game.Factions) {
            for (int i = 0; i < count; i++) {
                var randomCoord = game.Map.GetRandomCoord(MoveType.Land, new Coord((int)(wight * 0.4f), (int)(height * 0.4f)), new Coord((int)(wight * 0.6f), (int)(height * 0.6f)));
                game.CreateUnit(faction, randomCoord);
            }
        }
    }

    public void OnAddUnit(Unit unit) {
        unitViews.Add(unit, CreateUnitView(transform, mapView, unit, "test_unit"));
    }

    public void OnRemoveUnit(Unit unit) {
    }

    public void OnEndTurn(Faction faction) {
    }

    public void OnStartTurn(Faction faction) {
    }

    public static UnitView CreateUnitView(Transform parent, MapView mapView, Unit unit, string prefabName) {
        var unitPrefab = Resources.Load<UnitView>(prefabName);
        if (unitPrefab == null) {
            Debug.LogError("Can't load unit " + prefabName);
            return null;
        }
        UnitView unitView = Instantiate(unitPrefab);
        unitView.gameObject.SetActive(true);
        unitView.transform.parent = parent;
        unitView.Init(unit, mapView.CellCoordToPosition(unit.Coordinate));
        return unitView;
    }

    private void Update () {
        if (Input.GetKeyDown(KeyCode.F1)) {
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            Time.timeScale = (Time.timeScale == 1f) ? 10f : 1f;
        }
        if (Input.GetKeyDown(KeyCode.F3)) {
        }
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
    }
}
