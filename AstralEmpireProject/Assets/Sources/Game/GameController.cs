using System;
using Model;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public sealed class GameController : MonoBehaviour, Game.IGameController {
    [SerializeField]
    private int widht = 51;
    [SerializeField]
    private int height = 51;
    [SerializeField]
    private MapView mapView; // Set from editor
    [SerializeField]
    private PlayerTurnController playerController; // Set from editor

    private Game game;
    private Dictionary<Unit, UnitView> unitViews = new Dictionary<Unit, UnitView>();
    private Dictionary<City, CityView> cityViews = new Dictionary<City, CityView>();

    private void Start () {
        var factions = new Faction[] {
            new Faction(new EmptyFactionController(), Color.gray, Color.white, "Neutral"),
            new Faction(playerController, Color.blue, Color.white, "Blue player"),
            new Faction(playerController, Color.red, Color.black, "Red player")
        };

        var mapGenerator = new MapGenerator(widht, height);
        var map = new Map(mapGenerator.GenerateCells());
        map.OnAction += OnAddActionHandler;
        game = new Game(this, map, factions);
        mapView.Init(map, mapGenerator);
        CameraController.SetBorders(mapView.GetBorders(map));
        CreateDummyUnits(factions[1], 10);
        CreateDummyUnits(factions[2], 10);
        CreateCities(mapGenerator.CityCoords);
    }

    private void CreateCities(Coord[] cityCoords) {
        foreach (var coord in cityCoords) {
            game.CreateCity(coord);
        }
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

    private void CreateDummyUnits(Faction faction, int count) {
        for (int i = 0; i < count; i++) {
            var randomCoord = game.Map.GetRandomCoord(MoveType.Land, new Coord((int)(widht * 0.4f), (int)(height * 0.4f)), new Coord((int)(widht * 0.6f), (int)(height * 0.6f)));
            game.CreateUnit(faction, randomCoord);
        }
    }

    public void OnAddUnit(Unit unit) {
        unitViews.Add(unit, CreateUnitView(transform, mapView, unit, "test_unit"));
    }

    public void OnAddCity(City city) {
        cityViews.Add(city, CreateCityView(transform, mapView, city, "test_city"));
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

    private CityView CreateCityView(Transform parent, MapView mapView, City city, string prefabName) {
        var unitPrefab = Resources.Load<CityView>(prefabName);
        if (unitPrefab == null) {
            Debug.LogError("Can't load unit " + prefabName);
            return null;
        }
        CityView cityView = Instantiate(unitPrefab);
        cityView.gameObject.SetActive(true);
        cityView.transform.parent = parent;
        cityView.Init(city, mapView.CellCoordToPosition(city.Coordinate));
        return cityView;
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
