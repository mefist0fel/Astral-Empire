﻿using System;
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
            for (int i = 2; i < Width - 2; i++) {
                for (int j = 2; j < Height - 2; j++) {
                    if (i + j - 1 <= Width * 0.5f || i + j + 1 >= Width + Height - Width * 0.5f - 2) {
                        continue;
                    }
                    if (Random.Range(0, 6) == 0) {
                        cells[i, j] = CellType.Forest;
                    }
                    if (Random.Range(0, 8) == 0) {
                        cells[i, j] = CellType.Hills;
                    }
                    if (Random.Range(0, 16) == 0) {
                        cells[i, j] = CellType.Mountains;
                    }
                }
            }
            return cells;
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
