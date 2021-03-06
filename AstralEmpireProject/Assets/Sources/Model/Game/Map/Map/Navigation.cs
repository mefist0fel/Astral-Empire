﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.PathFind {
    public sealed class Navigation {
        private readonly PriorityQueueCustom<Coord, int> frontier = new PriorityQueueCustom<Coord, int>();
        private readonly Coord[,] cameFromCoord;
        private readonly int[,] distanceToId;

        private readonly int width;
        private readonly int height;
        private readonly Map map;

        public Navigation(Map contolMap) {
            map = contolMap;
            width = map.Width;
            height = map.Height;
            cameFromCoord = new Coord[width, height];
            distanceToId = new int[width, height];
            ClearDistances();
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
        // For debug purposes
        public int[,] GetDistanceMap() {
            return distanceToId;
        }

        public MarkersSet GetActionZone(Unit unit) {
            var moveMarkers = new MarkersSet();
            MarkMoveZoneRecursive(moveMarkers, unit.Coordinate, unit, unit.ActionPoints + map[unit.Coordinate].MoveCost);
            return moveMarkers;
        }

        public MarkersSet GetMoveZone(Unit unit) {
            var moveMarkers = new MarkersSet();
            MarkMoveZoneRecursive(moveMarkers, unit.Coordinate, unit, unit.ActionPoints + map[unit.Coordinate].MoveCost);
            return moveMarkers;
        }

        private void MarkMoveZoneRecursive(MarkersSet moveMarkers, Coord coord, Unit unit, int actionPoints) {
            var cell = map[coord];
            actionPoints -= cell.MoveCost;
            if (actionPoints < 0) // No action points
                return;
            if (!cell.CanMoveAcrossBy(unit.MoveTerrainMask)) // Can't move across cell type of terrain
                return;
            if (actionPoints <= moveMarkers[coord]) // cell already pacified
                return;
            if (cell.HasEnemyUnit(unit.Faction)) // enemy unit on cell
                return;
            if (cell.HasEnemyCity(unit.Faction)) // enemy city on cell
                return;
            moveMarkers[coord] = actionPoints;
            for (int i = 0; i < Neigbhors.Length; i++) {
                MarkMoveZoneRecursive(moveMarkers, coord + Neigbhors[i], unit, actionPoints);
            }
        }

        public List<Coord> TryFindPath(Coord from, Coord to, MarkersSet moveMarkers) {
            List<Coord> pathPoints = new List<Coord>();
            if (moveMarkers[to] >= 0) {
                pathPoints.Add(to);
                Coord current = to;
                const int maxSearchDeep = 100;
                int deep = 0;
                int nearestDistance;
                Coord neigbhor;
                Coord nearest = new Coord();
                while (deep < maxSearchDeep) {
                    nearestDistance = 0;
                    for (int i = 0; i < Neigbhors.Length; i++) {
                        neigbhor = current + Neigbhors[i];
                        if (nearestDistance < moveMarkers[neigbhor]) {
                            nearestDistance = moveMarkers[neigbhor];
                            nearest = neigbhor;
                        }
                    }
                    current = nearest;
                    pathPoints.Add(current);
                    if (current == from) {
                        break;
                    }
                    deep += 1;
                }
                if (pathPoints.Count >= 2) {
                    pathPoints.Reverse();
                    return pathPoints;
                }
            }
            Debug.LogError("Error find path by markers set");
            return new List<Coord>() { from, to };
        }

        public List<Coord> FindPathAStar(Coord startCoord, Coord endCoord, Unit unit) {
            frontier.Clear();
            frontier.Enqueue(startCoord, 0);
            cameFromCoord[startCoord.x, startCoord.y] = new Coord();
            Coord currentCoord = startCoord;
            ClearDistances();
            distanceToId[startCoord.x, startCoord.y] = 0;
            int i;
            int distance;
            Coord neigbhorCoord;
            Cell neigbhorCell;
            while (frontier.Count > 0) {
                currentCoord = frontier.Dequeue();
                if (currentCoord == endCoord)
                    break;

                for (i = 0; i < Neigbhors.Length; i++) {
                    neigbhorCoord = currentCoord + Neigbhors[i];
                    neigbhorCell = map[neigbhorCoord];
                    distance = distanceToId[currentCoord.x, currentCoord.y] + neigbhorCell.MoveCost;

                    if (distance < distanceToId[neigbhorCoord.x, neigbhorCoord.y] &&
                        neigbhorCell.CanMoveAcrossBy(unit.MoveTerrainMask) &&
                        (!neigbhorCell.HasEnemyCity(unit.Faction) || neigbhorCoord == endCoord) &&
                        (!neigbhorCell.HasEnemyUnit(unit.Faction) || neigbhorCoord == endCoord)) {
                        frontier.Enqueue(neigbhorCoord, (distance + HeuristicDistance(neigbhorCoord, endCoord)) * 2 + (neigbhorCell.HasUnit ? 1 : 0));
                        cameFromCoord[neigbhorCoord.x, neigbhorCoord.y] = currentCoord;
                        distanceToId[neigbhorCoord.x, neigbhorCoord.y] = distance;
                    }
                }
            }
            if (currentCoord == endCoord)
                return FindPathBackTrace(startCoord, endCoord);
            return new List<Coord>();
        }

        private void ClearDistances() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    distanceToId[x, y] = int.MaxValue;
                }
            }
        }

        private List<Coord> FindPathBackTrace(Coord startCoord, Coord endCoord) {
            var path = new List<Coord>();
            path.Add(endCoord);
            while (endCoord != startCoord) {
                endCoord = cameFromCoord[endCoord.x, endCoord.y];
                path.Add(endCoord);
            }
            path.Reverse();
            return path;
        }

        private int HeuristicDistance(Coord from, Coord to) {
            const float triangleHeight = 0.866f;
            var fromPosition = new Vector2(from.x + from.y * 0.5f, from.y * triangleHeight);
            var toPosition = new Vector2(to.x + to.y * 0.5f, to.y * triangleHeight);
            const float heuristicAttenuationFactor = 1.3f;
            return Mathf.CeilToInt(Vector2.Distance(fromPosition, toPosition) * heuristicAttenuationFactor);
        }
    }
}
