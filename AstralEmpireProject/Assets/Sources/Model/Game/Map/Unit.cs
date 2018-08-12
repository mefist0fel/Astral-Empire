using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    [Serializable]
    public sealed class Unit {
        [SerializeField]
        private int maxHitPoints = 10;
        [SerializeField]
        private int hitPoints = 10;
        public int HitPoints { get { return hitPoints; } }
        public int MaxHitPoints { get { return maxHitPoints; } }
        public string Model { get; private set; }
        public string Name { get; private set; }
        public Faction faction = null;

        public int moveDistance = 3;
        public Map.CellType[] moveTerrainMask = new Map.CellType[] {
            Map.CellType.Land
        };

        [SerializeField]
        private int damage = 6;
        public int maxFireRange = 1;
        public int minFireRange = 1;

        public bool canMove = false;
        public bool canFire = false;
        public List<Coord> movePath = new List<Coord>();
        private Map map = null;

        [SerializeField] private Coord coord = new Coord();

        public Unit() {// UnitData unitData) {
     //       hitPoints = unitData.HitPoints;
     //       maxHitPoints = unitData.HitPoints;
     //       Model = unitData.Model;
     //       Name = unitData.Name;
     //       moveDistance = unitData.MoveDistance;
     //       moveTerrainMask = GetMaskForType(unitData.MoveType);
     //       damage = unitData.Damage;
     //       maxFireRange = unitData.MaxFireRange;
     //       minFireRange = unitData.MinFireRange;
        }

      //  private Map.CellType[] GetMaskForType(UnitData.MovingType moveType) {
      //      switch (moveType) {
      //          default:
      //          case UnitData.MovingType.starship:
      //              return new Map.CellType[] { Map.CellType.Space };
      //          case UnitData.MovingType.fighter:
      //              return new Map.CellType[] { Map.CellType.Space, Map.CellType.Debris };
      //      }
      //  }
      //
        public bool IsAlive {
            get { return hitPoints > 0; }
        }

        public Coord Coordinate {
            get { return coord; }
        }

        public int ContourAttackDamage {
            get { return Mathf.CeilToInt(damage * 0.5f); }
        }

        public void OnStartTurn() {
            canMove = moveDistance > 0;
            canFire = true;
        }

        public void Init(Map controlMap, Faction team, Coord unitCoord) {
            faction = team;
            coord = unitCoord;
            this.map = controlMap;
            map[coord].unit = this; // add Set unit action
            Name = "Unit_" + team.Name;
        }

        public void MoveTo(Map map, Coord newCoord) {
            canMove = false;
            coord = newCoord;
        }

        public void AttackUnit(Unit enemy) {
            canFire = false;
            canMove = false;
            FireTo(enemy, damage);
            if (enemy.IsAlive) {
                if (enemy.IsCanCounterAttack(this)) {
                    enemy.FireTo(this, ContourAttackDamage);
                    if (!IsAlive) {
                        Death();
                    }
                }
            } else {
                enemy.Death();
            }
        }

        public void FireTo(Unit enemy, int damage) {
            enemy.OnHit(damage);
            map.AddAction(new AttackAction(this, enemy, damage));
        }

        public void OnHit(int damage) {
            hitPoints -= damage;
        }

        public void Death() {
            map.KillUnit(Coordinate);
            hitPoints = 0;
        }

        public bool IsCanMoveThrew(Map.CellType type) {
            if (moveTerrainMask != null) {
                foreach (var mask in moveTerrainMask) {
                    if (mask == type) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsCanCounterAttack(Unit unit) {
            var fireMarkers = map.GetFireZone(this);
            return fireMarkers[unit.Coordinate] > 0;
        }
    }
}
