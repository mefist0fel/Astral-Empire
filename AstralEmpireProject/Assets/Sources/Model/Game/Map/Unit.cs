using System;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    [Serializable]
    public sealed class Unit {
        public int HitPoints { get; private set; }
        public int MaxHitPoints { get; private set; }
        public int ActionPoints { get; private set; }
        public int MaxActionPoints { get; private set; }
        public readonly int MinFireRange = 1;
        public readonly int MaxFireRange = 1;
        public string Id { get; private set; }
        public Coord Coordinate { get; private set; }
        public Faction Faction = null;

        public int moveDistance = 3;
        public Map.CellType[] MoveTerrainMask = new Map.CellType[] {
            Map.CellType.Land,
            Map.CellType.Rough
        };

        [SerializeField]
        private int damage = 6;

        public List<Coord> movePath = new List<Coord>();
        private Map map = null;

        public Unit(string id, int maxHitPoints, int maxActionPoints) {
            Coordinate = new Coord();
            HitPoints = maxHitPoints;
            MaxHitPoints = maxHitPoints;
            ActionPoints = maxActionPoints;
            MaxActionPoints = maxActionPoints;
            Id = id;
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
            get { return HitPoints > 0; }
        }

        public void OnStartTurn() {
            ActionPoints = MaxActionPoints;
        }

        public void Init(Map controlMap, Faction team, Coord unitCoord) {
            Faction = team;
            Coordinate = unitCoord;
            map = controlMap;
            map[Coordinate].Unit = this; // add Set unit action
        }

        public void MoveTo(Coord newCoord, int actionPoints) {
            Coordinate = newCoord;
            ActionPoints -= actionPoints;
        }

        public void AttackUnit(Unit enemy) {
           // canFire = false;
           // canMove = false;
            FireTo(enemy, damage);
            if (enemy.IsAlive) {
                if (enemy.IsCanCounterAttack(this)) {
                 //   enemy.FireTo(this, ContourAttackDamage);
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
            HitPoints -= damage;
        }

        public void Death() {
            map.KillUnit(Coordinate);
            HitPoints = 0;
        }

        public bool IsCanMoveThrew(Map.CellType type) {
            if (MoveTerrainMask != null) {
                foreach (var mask in MoveTerrainMask) {
                    if (mask == type) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsCanCounterAttack(Unit unit) {
            //  var fireMarkers = map.GetFireZone(this);
            return false;// fireMarkers[unit.Coordinate] > 0;
        }
    }
}
