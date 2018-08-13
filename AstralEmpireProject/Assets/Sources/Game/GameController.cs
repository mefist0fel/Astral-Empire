using System;
using Model;
using UnityEngine;

public class GameController : MonoBehaviour {
    [SerializeField]
    private int wight = 51;
    [SerializeField]
    private int height = 51;
    [SerializeField]
    private MapView mapView; // Set from editor
    [SerializeField]
    private PlayerTurnController playerController; // Set from editor

    private Game game;
    private CameraControlModel cameraModel = new CameraControlModel(OnFieldClick);

    private void Start () {
        var factions = new Faction[] {
            new Faction(playerController, 0, Color.blue, Color.white, "player 1"),
            new Faction(playerController, 0, Color.red, Color.black, "player 2")
        };

        var map = new Map(wight, height);
        game = new Game(map, factions);
        game.OnAddUnit += OnAddUnit;
        mapView.Init(map);
        CameraController.SetBorders(mapView.GetBorders(map));
    }

    private void OnAddUnit(Unit unit) {
    }

    private static void OnFieldClick(Vector2 pointerPosition) {
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
        cameraModel.Process();
    }
}
