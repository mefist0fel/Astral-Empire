using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class BuildUnitProject : AbstractProject {
        public readonly Unit.Data UnitData;
        private readonly Game game;

        public BuildUnitProject(string id, int cost, Game controlGame, Unit.Data unitData) : base(id, cost) {
            UnitData = unitData;
            game = controlGame;
        }

        public override void EndBuilding(City city) {
            var coord = FindNearestEmptySpawnCoord(city);
            game.CreateUnit(UnitData, coord, city.Faction);
        }

        private Coord FindNearestEmptySpawnCoord(City city) {
            foreach (var nearestCoord in nearestSpawnPoints) {
                var spawnCoord = nearestCoord + city.Coordinate;
                if (CanSpawn(spawnCoord))
                    return spawnCoord;
            }
            Debug.LogError("Can't find empty place near " + city.Name + " - can't build unit " + ID);
            return city.Coordinate;
        }

        // -1: 1  ---  0: 1  ---  1: 1
        // 	 |    ‾-_   |          |
        // -1: 0  ---  0: 0  ---  1: 0
        //   |          |    ‾-_   |
        // -1:-1  ---  0:-1  ---  1:-1

        private static readonly Coord[] nearestSpawnPoints = new Coord[] {
            new Coord( 0, 0),
            new Coord( 1, 0),
            new Coord( 1,-1),
            new Coord( 0,-1),
            new Coord(-1, 0),
            new Coord(-1, 1),
            new Coord( 0, 1),
            new Coord( 2, 0),
            new Coord( 2, -2),
            new Coord( 2, -1),
            new Coord( 1, 1),
            new Coord( 0, 2),
            new Coord(-1, 2),
            new Coord(-2, 2),
            new Coord(-2, 1),
            new Coord(-2, 0),
            new Coord(-1, -1),
            new Coord( 0, -2),
            new Coord( 1, -2)
        };

        private bool CanSpawn(Coord coordinate) {
            var map = game.Map;
            return !map[coordinate].HasUnit && map[coordinate].CanMoveAcrossBy(UnitData.MoveTerrainMask);
        }
    }
}
