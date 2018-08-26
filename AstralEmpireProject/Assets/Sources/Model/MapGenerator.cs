using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model {
    public sealed class MapGenerator {
        private const int defaultSize = 21;
        private int widht;
        private int height;

        public MapGenerator(int widht = defaultSize, int height = defaultSize) {
            this.widht = widht;
            this.height = height;
        }

        public Cell[,] GenerateCells() {
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
    }
}
