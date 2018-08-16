using System.Collections.Generic;
using UnityEngine;

namespace Model.PathFind {
    public sealed class Navigation {
        public interface IPathResolver {
            bool CanMoveThrough(Coord coord);
        }

        private readonly PriorityQueueCustom<Coord, int> frontier = new PriorityQueueCustom<Coord, int>();
        private readonly Coord[,] cameFromId;
        private readonly int[,] distanceToId;

        private readonly int width;
        private readonly int height;

        public Navigation(int mapWidth, int mapHeight) {
            width = mapWidth;
            height = mapHeight;
            cameFromId = new Coord[width, height];
            distanceToId = new int[width, height];
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

        public MarkersSet GetMoveZone(Coord coordinate, IPathResolver pathResolver, int distance) {
            var moveMarkers = new MarkersSet();
            MarkMoveZoneRecursive(moveMarkers, coordinate, pathResolver, distance);
            return moveMarkers;
        }

        private void MarkMoveZoneRecursive(MarkersSet moveMarkers, Coord startPoint, IPathResolver pathResolver, int distance) {
            if (distance <= 0)
                return;
            if (!pathResolver.CanMoveThrough(startPoint))
                return;
            if (distance <= moveMarkers[startPoint])
                return;
            moveMarkers[startPoint] = distance;
            for (int i = 0; i < Neigbhors.Length; i++) {
                MarkMoveZoneRecursive(moveMarkers, startPoint + Neigbhors[i], pathResolver, distance - 1);
            }
        }

        public List<Coord> TryFindPath(Coord from, Coord to, MarkersSet moveMarkers) {
            List<Coord> pathPoints = new List<Coord>();
            if (moveMarkers[to] > 0) {
                pathPoints.Add(to);
                Coord current = to;
                const int maxSearchDeep = 100;
                int deep = 0;
                while (deep < maxSearchDeep) {
                    for (int i = 0; i < Neigbhors.Length; i++) {
                        if (moveMarkers[current + Neigbhors[i]] > moveMarkers[current]) {
                            current = current + Neigbhors[i];
                            pathPoints.Add(current);
                            break;
                        }
                    }
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

        public List<Coord> FindPathDijkstra(Coord startCoord, Coord endCoord, IPathResolver pathResolver) {
            frontier.Clear();
            frontier.Enqueue(startCoord, 0);
            cameFromId[startCoord.x, startCoord.y] = new Coord();
            Coord currentId = startCoord;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    distanceToId[x, y] = int.MaxValue;
                }
            }
            distanceToId[startCoord.x, startCoord.y] = 0;
            int i;
            int distance;
            Coord neigbhor;
            while (frontier.Count > 0) {
                currentId = frontier.Dequeue();

                if (currentId == endCoord) {
                    break;
                }
                for (i = 0; i < Neigbhors.Length; i++) {
                    distance = distanceToId[currentId.x, currentId.y] + 1;
                    neigbhor = currentId + Neigbhors[i];
                    if (distance < distanceToId[neigbhor.x, neigbhor.y] && pathResolver.CanMoveThrough(neigbhor)) {
                        frontier.Enqueue(neigbhor, distance);
                        cameFromId[neigbhor.x, neigbhor.y] = currentId;
                        distanceToId[neigbhor.x, neigbhor.y] = distance;
                    }
                }
            }
            var path = new List<Coord>();
            if (currentId == endCoord) {
                path.Add(currentId);
                while (currentId != startCoord) {
                    currentId = cameFromId[currentId.x, currentId.y];
                    path.Add(currentId);
                }
                path.Reverse();
            }
            return path;
        }

        public List<Coord> FindPathAStar(Coord startCoord, Coord endCoord, IPathResolver pathResolver) {
            frontier.Clear();
            frontier.Enqueue(startCoord, 0);
            cameFromId[startCoord.x, startCoord.y] = new Coord();
            Coord currentId = startCoord;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    distanceToId[x, y] = int.MaxValue;
                }
            }
            distanceToId[startCoord.x, startCoord.y] = 0;
            int i;
            int distance;
            Coord neigbhor;
            while (frontier.Count > 0) {
                currentId = frontier.Dequeue();

                if (currentId == endCoord) {
                    break;
                }
                for (i = 0; i < Neigbhors.Length; i++) {
                    distance = distanceToId[currentId.x, currentId.y] + 1;
                    neigbhor = currentId + Neigbhors[i];
                    if (distance < distanceToId[neigbhor.x, neigbhor.y] && pathResolver.CanMoveThrough(neigbhor)) {
                        frontier.Enqueue(neigbhor, distance + HeuristicDistance(neigbhor, endCoord));
                        cameFromId[neigbhor.x, neigbhor.y] = currentId;
                        distanceToId[neigbhor.x, neigbhor.y] = distance;
                    }
                }
            }
            var path = new List<Coord>();
            if (currentId == endCoord) {
                path.Add(currentId);
                while (currentId != startCoord) {
                    currentId = cameFromId[currentId.x, currentId.y];
                    path.Add(currentId);
                }
                path.Reverse();
            }
            return path;
        }

        private int HeuristicDistance(Coord from, Coord to) {
            var fromPosition = new Vector2(from.x + from.y * 0.5f, from.y);
            var toPosition = new Vector2(to.x + to.y * 0.5f, to.y);
            return Mathf.CeilToInt(Vector2.Distance(fromPosition, toPosition));
        }
    }
}
