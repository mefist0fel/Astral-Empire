using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model {
    public enum CellType {
        None = 0,
        Ocean = 1,
        Sea = 2,
        Lake = 3,
        Desert = 4,
        Grass = 5,
        Forest = 6,
        Hills = 7,
        Mountains = 8
    }

    public sealed class MapGenerator {
        private const int defaultSize = 21;

        public readonly int Width;
        public readonly int Height;
        public readonly CellType[,] Cells;
        public readonly Vector2 size = new Vector2(1f, 0.866f);

        // -1: 1  ---  0: 1  ---  1: 1
        // 	 |    ‾-_   |          |
        // -1: 0  ---  0: 0  ---  1: 0
        //   |          |    ‾-_   |
        // -1:-1  ---  0:-1  ---  1:-1

        public MapGenerator(int widht = defaultSize, int height = defaultSize) {
            Width = widht;
            Height = height;
            Cells = GenerateCells(Width, Height);
        }

        public MoveType[,] GenerateCells() {
            var mapType = new MoveType[Width, Height];
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    mapType[i, j] = ToMoveType(Cells[i, j]);
                }
            }
            return mapType;
        }

        private MoveType ToMoveType(CellType type) {
            switch (type) {
                default:
                case CellType.None:
                    return MoveType.None;
                case CellType.Ocean:
                case CellType.Sea:
                case CellType.Lake:
                    return MoveType.Water;
                case CellType.Grass:
                case CellType.Desert:
                    return MoveType.Land;
                case CellType.Hills:
                case CellType.Forest:
                    return MoveType.Rough;
                case CellType.Mountains:
                    return MoveType.Impassable;
            }
        }

        private CellType[,] GenerateCells(int widht, int height) {
            var cells = new CellType[widht, height];
            AddDefaultOcean(cells);
            AddLandInBorders(cells);
            var centerPosition = ToPosition((Width - 1f) * 0.5f, (Height - 1f) * 0.5f);
            const int cityDistance = 6;
            var cityCoords = new List<Coord>();
            var cityX = Width / (float)cityDistance;
            var cityY = Height / (float)cityDistance;
            for (int i = 1; i < cityX - 1; i++) {
                for (int j = 1; j < cityY - 1; j++) {
                    if (i + j <= cityX * 0.5f || i + j >= cityX + cityY - cityX * 0.5f - 1)
                        continue;
                    var cityCoord = new Coord(i * cityDistance, j * cityDistance) + GetRandomCoordOffset(1);
                    cityCoords.Add(cityCoord);
                }
            }
            for (int i = 1; i < Width - 1; i++) {
                for (int j = 1; j < Height - 1; j++) {
                    if (i + j <= Width * 0.5f || i + j >= Width + Height - Width * 0.5f - 2) {
                        continue;
                    }
                    cells[i, j] = CellType.Grass;
                    if (Random.Range(0, 6) == 0)
                        cells[i, j] = CellType.Forest;
                    if (Random.Range(0, 8) == 0)
                        cells[i, j] = CellType.Hills;
                    if (Random.Range(0, 16) == 0)
                        cells[i, j] = CellType.Mountains;
                }
            }
            foreach (var coord in cityCoords) {
                cells[coord.x, coord.y] = CellType.Desert;
            }
            return cells;
        }

        private Coord GetRandomCoordOffset(int offset) {
            var randomX = Random.Range(-offset, offset + 1);
            var randomY = Random.Range(-offset, offset + 1);
            var randomZ = Random.Range(-offset, offset + 1);
            return new Coord( randomX + randomZ, randomY - randomZ);
        }

        private float ScaledNoise(Vector2 position, float scale) {
            return Mathf.PerlinNoise(position.x * scale, position.y * scale) * 0.5f;
        }

        private Vector2 ToPosition(float x, float y) {
            return new Vector2(x * size.x + y * size.x * 0.5f, y * size.y);
        }

        private void AddLandInBorders(CellType[,] cells) {
            for (int i = 2; i < Width - 2; i++) {
                for (int j = 2; j < Height - 2; j++) {
                    if (i + j - 1 <= Width * 0.5f || i + j + 1 >= Width + Height - Width * 0.5f - 2) {
                        continue;
                    }
                    cells[i, j] = CellType.Grass;
                }
            }
        }

        private void AddDefaultOcean(CellType[,] cells) {
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    bool isBorder = (
                        i == 0 || j == 0 ||
                        i == Width - 1 || j == Height - 1 ||
                        i + j <= Width * 0.5f || i + j >= Width + Height - Width * 0.5f - 2);
                    if (isBorder)
                        cells[i, j] = CellType.None;
                    else
                        cells[i, j] = CellType.Ocean;
                }
            }
        }
    }
}
