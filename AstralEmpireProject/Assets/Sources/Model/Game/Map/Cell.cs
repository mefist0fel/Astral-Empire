﻿using System;

namespace Model {
    public enum MoveType {
        None = 0,      // nothing - end of map move zone
        Water = 1,     // ships
        Land = 2,      // usual units
        Rough = 3,     // forest and hills
        Impassable = 4  // No ships can move - large objects
    }

    /// <summary>
    /// Map cell
    /// </summary>
    public sealed class Cell {
        public Unit Unit = null;
        public City City = null;
        public MoveType Type = MoveType.None;
        public int MoveCost = 1;

        public Cell(MoveType moveType = MoveType.None) {
            Type = moveType;
            MoveCost = GetMoveCost(Type);
        }

        private static int GetMoveCost(MoveType type) {
            switch (type) {
                default:
                    return 1;
                case MoveType.Rough:
                case MoveType.Impassable:
                    return 2;
            }
        }

        public Unit GetUnit() {
            if (HasUnit)
                return Unit;
            return null;
        }

        public bool HasUnit {
            get { return (Unit != null && Unit.IsAlive); }
        }

        public bool HasAlliedUnit(Unit unit) {
            return (HasUnit && Unit.Faction == unit.Faction);
        }

        public bool HasEnemyUnit(Faction faction) {
            return (HasUnit && Unit.Faction != faction);
        }

        public bool HasCity {
            get { return (City != null); }
        }

        public bool HasEnemyCity(Faction faction) {
            return (HasCity && City.Faction != faction);
        }

        public bool CanMoveAcrossBy(MoveType[] moveTerrainMask) {
            for (int i = 0; i < moveTerrainMask.Length; i++)
                if (moveTerrainMask[i] == Type)
                    return true;
            return false;
        }
    }
}
