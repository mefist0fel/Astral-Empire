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
            new Faction(playerController, Color.blue, Color.white, "Blue player"),
            new Faction(playerController, Color.red, Color.black, "Red player")
        };

        var mapGenerator = new MapGenerator(widht, height);
        var map = new Map(mapGenerator.GenerateCells());
        map.OnAction += OnAddActionHandler;
        game = new Game(this, map, factions);
        mapView.Init(map, mapGenerator);
        CameraController.SetBorders(mapView.GetBorders(map));
        // test params
        CreateDummyUnits(factions[0], 2);
        CreateDummyUnits(factions[1], 2);
        CreateCities(mapGenerator.CityCoords);
        var randomPlayerCity = Random.Range(0, game.Cities.Count);
        int randomEnemyCity;
        do {
            randomEnemyCity = Random.Range(0, game.Cities.Count);
        }
        while (randomEnemyCity == randomPlayerCity);
        game.Cities[randomPlayerCity].SetFaction(factions[0]);
        game.Cities[randomEnemyCity].SetFaction(factions[1]);
        SetCameraTo(game.Cities[randomPlayerCity]);
    }

    private void SetCameraTo(City city) {
        var startPosition = mapView.CellCoordToPosition(city.Coordinate);
        CameraController.SetPosition(startPosition);
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
            game.CreateUnit(new Unit.Data("infantry", 2, 3), randomCoord, faction);
            randomCoord = game.Map.GetRandomCoord(MoveType.Land, new Coord((int)(widht * 0.4f), (int)(height * 0.4f)), new Coord((int)(widht * 0.6f), (int)(height * 0.6f)));
            game.CreateUnit(new Unit.Data("heavy_infantry", 3, 4), randomCoord, faction);
            randomCoord = game.Map.GetRandomCoord(MoveType.Land, new Coord((int)(widht * 0.4f), (int)(height * 0.4f)), new Coord((int)(widht * 0.6f), (int)(height * 0.6f)));
            game.CreateUnit(new Unit.Data("AIV", 2, 3), randomCoord, faction);
            randomCoord = game.Map.GetRandomCoord(MoveType.Land, new Coord((int)(widht * 0.4f), (int)(height * 0.4f)), new Coord((int)(widht * 0.6f), (int)(height * 0.6f)));
            game.CreateUnit(new Unit.Data("tank", 2, 3), randomCoord, faction);
        }
    }

    public void OnAddUnit(Unit unit) {
        unitViews.Add(unit, CreateUnitView(transform, mapView, unit, unit.Id));
    }

    public void OnAddCity(City city) {
        cityViews.Add(city, CreateCityView(transform, mapView, city, "test_city"));
    }

    public void OnRemoveUnit(Unit unit) {
    }

    public void OnEndTurn(Faction faction) {
    }

    public void OnStartTurn(Faction faction) {
        // move camera to main city
        var factionCity = faction.Cities.FirstOrDefault();
        if (factionCity != null) {
            var capitalPosition = mapView.CellCoordToPosition(factionCity.Coordinate);
            CameraController.SetPosition(capitalPosition, true);
        }
    }

    public static UnitView CreateUnitView(Transform parent, MapView mapView, Unit unit, string prefabName) {
        var unitPrefab = Resources.Load<UnitView>("Units/" + prefabName);
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
