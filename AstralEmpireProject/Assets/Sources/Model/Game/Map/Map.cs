using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random; // TODO remove me
using System.Linq;
using Model.PathFind;

namespace Model {
    /// <summary>
    /// Map. Base class to store cell structure and manage paht finding and fire range
    /// </summary>
    public sealed class Map {
        public abstract class AbstractAction { }

        private const int defaultSize = 21;
        private readonly int width;
        private readonly int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Cell[,] cells = null;
        public List<AbstractAction> actions = new List<AbstractAction>();
        public readonly Navigation Navigation;
        public event System.Action<AbstractAction> OnAction;

        public enum CellType {
            None = 0, // nothing - end of map move zone
            Land = 1, // usual units
            Water = 2, // ships
            Rough = 3, // forest and hills
            Mountains = 4 // No ships can move - large objects
        }

        public sealed class Cell {
            public Unit Unit = null;
            public CellType Type = CellType.None;
            public int MoveCost = 1;

            public Unit GetUnit() {
                if (HasUnit)
                    return Unit;
                return null;
            }

            public bool HasUnit {
                get { return (Unit != null && Unit.IsAlive); }
            }

            public bool HasEnemyUnit(Unit unit) {
                return (HasUnit && Unit.Faction != unit.Faction);
            }

            public bool CanMoveAcrossBy(Unit unit) {
                for (int i = 0; i < unit.MoveTerrainMask.Length; i++)
                    if (unit.MoveTerrainMask[i] == Type)
                        return true;
                return false;
            }
        }

        public Cell this[int x, int y] {
            get {
                if (x >= 0 && y >= 0 && x < width && y < height) {
                    return cells[x, y];
                }
                return new Cell();
            }
            set {
                if (x >= 0 && y >= 0 && x < width && y < height) {
                    cells[x, y] = value;
                }
            }
        }

        public Cell this[Coord coord] {
            get { return this[coord.x, coord.y]; }
            set { this[coord.x, coord.y] = value; }
        }

        public void MoveUnit(Coord from, Coord to, MarkersSet moveMarkers) {
            if (from == to) {
                return;
            }
            if (this[from].Unit != null && this[to].Unit == null) {
                var path = TryFindPath(from, to, moveMarkers); // TODO find separately for unit
                var actionPoints = this[from].Unit.ActionPoints - moveMarkers[to];
                this[to].Unit = this[from].Unit;
                this[from].Unit = null;
                this[to].Unit.MoveTo(to, actionPoints);
                AddAction(new MoveAction(this[to].Unit, from, to, actionPoints, path));
            }
#if UNITY_EDITOR
            else {
                Debug.LogError("move unit error - unit not found or place is not empty");
            }
#endif
        }

        public void KillUnit(Coord coord) {
            if (this[coord].Unit != null) {
                AddAction(new DeathAction(this[coord].Unit, coord));
                this[coord].Unit = null;
            }
#if UNITY_EDITOR
        else {
                Debug.LogError("try kill unit - but cell is empty");
            }
#endif
        }
        // TODO remove
        public Coord GetRandomCoord(CellType startType = CellType.Land, Coord from = new Coord(), Coord to = new Coord()) {
            if (from == Coord.Zero) {
                from = new Coord(1, 1);
            }
            if (to == Coord.Zero) {
                to = new Coord(height - 1, height - 1);
            }
            for (int i = 0; i < 50; i++) {
                Coord coord = new Coord(Random.Range(from.x, to.x), Random.Range(from.y, to.y));
                if (this[coord].Unit == null && this[coord].Type == startType) {
                    return coord;
                }
            }
            return new Coord();
        }

        // -1: 1  --  0: 1  --  1: 1
        // 	 |         |         |
        // -1: 0  --  0: 0  --  1: 0 
        //   |         |         |
        // -1:-1  --  0:-1  --  1:-1

        public static readonly Coord[] Neigbhors = new Coord[] {
            new Coord( 1, 0),
            new Coord( 1,-1),
            new Coord( 0,-1),
            new Coord(-1, 0),
            new Coord(-1, 1),
            new Coord( 0, 1),
        };

        public Map(int mapWidth = defaultSize, int mapHeight = defaultSize) {
            width = mapWidth;
            height = mapHeight;
            cells = GenerateCells(width, height);
            Navigation = new Navigation(this);
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
                    cells[i, j].Type = CellType.Land;
                }
            }
            for (int i = 2; i < widht - 2; i++) {
                for (int j = 2; j < height - 2; j++) {
                    if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                        continue;
                    }
                    cells[i, j].Type = CellType.Land;
                    if (Random.Range(0, 8) == 0) {
                        cells[i, j].Type = CellType.Rough;
                        cells[i, j].MoveCost = 2;
                    }
                    if (Random.Range(0, 16) == 0) {
                        cells[i, j].Type = CellType.Mountains;
                    }
                }
            }
            return cells;
        }

        public void AddAction(AbstractAction action) {
            actions.Add(action);
            OnAction.InvokeSafe(action);
        }

        public void AttackUnit(Unit unit, Unit attackedUnit) {
            unit.AttackUnit(attackedUnit);
        }

        public MarkersSet GetMoveZone(Unit unit) {
            return Navigation.GetMoveZone(unit);
        }

        public List<Coord> TryFindPath(Coord from, Coord to, MarkersSet moveMarkers) {
            return Navigation.TryFindPath(from, to, moveMarkers);
        }

        public List<Coord> FindPath(Unit unit, Coord endCoord) {
            return Navigation.FindPathAStar(unit.Coordinate, endCoord, unit);
        }

        // public MarkersSet GetFireZone(Unit unit) {
        //     var fireMarkers = new MarkersSet();
        //     MarkFireZoneRecursive(fireMarkers, unit.Coordinate, unit.maxFireRange + 1);
        //     ClearNearFireZoneRecursive(fireMarkers, unit.Coordinate, unit.minFireRange);
        //     MarkEnemyUnitsOnFireZone(fireMarkers, unit);
        //     return fireMarkers;
        // }

        //   private void MarkFireZoneRecursive(MarkersSet markers, Coord startPoint, int fireDistance) { // todo change
        //       if (fireDistance <= markers[startPoint])
        //           return;
        //       markers[startPoint] = fireDistance;
        //       for (int i = 0; i < Neigbhors.Length; i++) {
        //           MarkFireZoneRecursive(markers, startPoint + Neigbhors[i], fireDistance - 1);
        //       }
        //   }
        //
        //   private void ClearNearFireZoneRecursive(MarkersSet markers, Coord startPoint, int clearDistance) { // todo change
        //       if (markers[startPoint] == 0)
        //           return;
        //       if (clearDistance <= 0)
        //           return;
        //       markers[startPoint] = 0;
        //       for (int i = 0; i < Neigbhors.Length; i++) {
        //           ClearNearFireZoneRecursive(markers, startPoint + Neigbhors[i], clearDistance - 1);
        //       }
        //   }

        //  private void MarkEnemyUnitsOnFireZone(MarkersSet fireMarkers, Unit unit) {
        //      List<Coord> fireZoneMarkers = fireMarkers.GetCoordList();
        //      foreach (var markerCoord in fireZoneMarkers) {
        //          bool isEnemyUnit = (this[markerCoord].Unit != null && this[markerCoord].Unit.IsAlive && this[markerCoord].Unit.Faction != unit.Faction);
        //          fireMarkers[markerCoord] = isEnemyUnit ? 1 : 0;
        //      }
        //  }

        public MarkersSet GetFireZoneForMoveZone(Unit unit, MarkersSet moveZone = null) {
            MarkersSet fireMarkers = new MarkersSet();
            if (moveZone == null) {
                moveZone = GetMoveZone(unit);
            }
            MarkersSet fullFireZone = new MarkersSet();
            fullFireZone.Add(GetFireZoneFromCoord(unit.Coordinate, unit));
            var moveCoords = moveZone.GetCoordList();
            int availableActionPoints;
            foreach (var coord in moveCoords) {
                if (this[coord].Unit != null)
                    continue;
                availableActionPoints = moveZone[coord];
                if (availableActionPoints > 0)
                    fullFireZone.Add(GetFireZoneFromCoord(coord, unit));
            }
            return fullFireZone;
        }

        private MarkersSet GetFireZoneFromCoord(Coord coord, Unit unit) {
            MarkersSet fireMarkers = new MarkersSet();
            for (int i = -unit.MaxFireRange; i <= unit.MaxFireRange; i++) {
                for (int j = -unit.MaxFireRange; j <= unit.MaxFireRange; j++) {
                    var distance = CubeDistance(i, j);
                    var distanceIsActual = unit.MinFireRange <= distance && distance <= unit.MaxFireRange;
                    var currentCoord = new Coord(i, j) + coord;
                    var hasEnemyUnit = this[currentCoord].HasEnemyUnit(unit);
                    if (distanceIsActual && hasEnemyUnit)
                        fireMarkers[currentCoord] = 1;
                }
            }
            return fireMarkers;
        }

        private int CubeDistance(int x, int y) {
            return Math.Max(
                Mathf.Abs(-x - y),
                Math.Max(
                    Mathf.Abs(x),
                    Mathf.Abs(y)));
        }

        public bool IsCanFireFromCoord(Unit unit, Coord coord, Coord fireCoord) {
            var delta = coord - fireCoord;
            var distance = CubeDistance(delta.x, delta.y);
            return unit.MinFireRange <= distance && distance <= unit.MaxFireRange;
        }

        public Coord FindOptimalMovePoint(Unit unit, MarkersSet moveMarkers, Coord fireCoord) {
            var moveCoords = moveMarkers.GetCoordList();
            int bestMarker = 0;
            Coord nearestCoord = Coord.Zero;
            foreach (var moveCoord in moveCoords) {
                if (moveMarkers[moveCoord] > bestMarker && (this[moveCoord].Unit == unit || this[moveCoord].Unit == null)) {
                    if (IsCanFireFromCoord(unit, moveCoord, fireCoord)) {
                        nearestCoord = moveCoord;
                        bestMarker = moveMarkers[moveCoord];
                    }
                }
            }
            if (nearestCoord == Coord.Zero) {
                Debug.LogError("Can't find nearest fire point");
            }
            return nearestCoord;
        }

        //  internal Coord FindOptimalMovePoint(Unit unit, MarkersSet moveMarkers, Coord fireCoord) {
        //      // TODO change to IEnumerator
        //      var moveCoords = moveMarkers.GetCoordList();
        //      int bestMarker = 0;
        //      Coord nearestCoord = Coord.Zero;
        //      foreach (var moveCoord in moveCoords) {
        //          if (moveMarkers[moveCoord] > bestMarker && (this[moveCoord].Unit == unit || this[moveCoord].Unit == null)) {
        //              var fireMarkers = new MarkersSet();
        //              MarkFireZoneRecursive(fireMarkers, moveCoord, unit.maxFireRange + 1);
        //              ClearNearFireZoneRecursive(fireMarkers, moveCoord, unit.minFireRange);
        //              if (fireMarkers[fireCoord] > 0) {
        //                  nearestCoord = moveCoord;
        //                  bestMarker = moveMarkers[moveCoord];
        //              }
        //          }
        //      }
        //      if (nearestCoord == Coord.Zero) {
        //          Debug.LogError("Can't find nearest fire point");
        //      }
        //      return nearestCoord;
        //  }
        //
    }
}