using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        public int AttackCount { get; private set; }
        public int Power { get; private set; }

        public Faction Faction = null;

        public int moveDistance = 3;
        public MoveType[] MoveTerrainMask = new MoveType[] {
            MoveType.Land,
            MoveType.Rough
        };

        public List<Coord> movePath = new List<Coord>();
        private Map map = null;

        public Unit(string id, int maxHitPoints, int maxActionPoints) {
            Coordinate = new Coord();
            HitPoints = maxHitPoints;
            MaxHitPoints = maxHitPoints;
            ActionPoints = maxActionPoints;
            MaxActionPoints = maxActionPoints;
            Id = id;
            AttackCount = 3;
            Power = 5;
        }

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
            ActionPoints = 0;
            var damageToEnemy = 0;
            var damageToMe = 0;
            for (int i = 0; i < AttackCount; i++) {
                // attack enemy
                if (this.HitEnemy(enemy))
                    damageToEnemy += 1;
                if (damageToEnemy >= enemy.HitPoints)
                    break;
                // back attack me
                if (enemy.HitEnemy(this))
                    damageToMe += 1;
                if (damageToMe >= this.HitPoints)
                    break;
            }
            this.OnHit(damageToMe);
            enemy.OnHit(damageToEnemy);
            map.AddAction(new AttackAction(this, enemy, damageToEnemy));
            map.AddAction(new AttackAction(enemy, this, damageToMe));
            if (!IsAlive)
                Death();
            if (!enemy.IsAlive)
                enemy.Death();
        }

        private bool HitEnemy(Unit enemy) {
            var hitChance = this.Power / (float)(this.Power + enemy.Power);
            return Random.Range(0f, 1f) < hitChance;
        }

        public void OnHit(int damage) {
            HitPoints -= damage;
            if (HitPoints < 0)
                HitPoints = 0;
        }

        public void Death() {
            HitPoints = 0;
            map.KillUnit(Coordinate);
        }

        public bool IsCanMoveThrew(MoveType type) {
            if (MoveTerrainMask != null) {
                foreach (var mask in MoveTerrainMask) {
                    if (mask == type) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
