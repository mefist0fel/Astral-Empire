using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model {
    public enum MapEnum {
    }

    public sealed class MapGenerator {
        private const int defaultSize = 21;
        private int widht;
        private int height;

        public MapGenerator(int widht = defaultSize, int height = defaultSize) {
            this.widht = widht;
            this.height = height;
        }

        // -1: 1  ---  0: 1  ---  1: 1
        // 	 |    ‾-_   |          |
        // -1: 0  ---  0: 0  ---  1: 0
        //   |          |    ‾-_   |
        // -1:-1  ---  0:-1  ---  1:-1

        public MoveType[,] GenerateCells() {
            var mapType = new MoveType[widht, height];
            for (int i = 0; i < widht; i++) {
                for (int j = 0; j < height; j++) {
                    mapType[i, j] = MoveType.None;
                }
            }
            for (int i = 2; i < widht - 2; i++) {
                for (int j = 2; j < height - 2; j++) {
                    if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                        continue;
                    }
                    mapType[i, j] = MoveType.Land;
                }
            }
            for (int i = 2; i < widht - 2; i++) {
                for (int j = 2; j < height - 2; j++) {
                    if (i + j - 1 <= widht * 0.5f || i + j + 1 >= widht + height - widht * 0.5f - 2) {
                        continue;
                    }
                    mapType[i, j] = MoveType.Land;
                    if (Random.Range(0, 8) == 0) {
                        mapType[i, j] = MoveType.Rough;
                    }
                    if (Random.Range(0, 16) == 0) {
                        mapType[i, j] = MoveType.Mountains;
                    }
                }
            }
            return mapType;
        }
    }
}
