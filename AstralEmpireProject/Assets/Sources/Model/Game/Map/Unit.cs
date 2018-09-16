using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Model {
    public sealed class Unit {
        public sealed class Data {
            public static readonly MoveType[] DefaultMoveMask = new MoveType[] {
                MoveType.Land,
                MoveType.Rough
            };

            public readonly string Id;
            public readonly int HitPoints;
            public readonly int ActionPoints;
            public readonly MoveType[] MoveTerrainMask;
            public Data(string id, int hitPoints, int actionPoints = 3, MoveType[] moveTerrainMask = null) {
                Id = id;
                HitPoints = hitPoints;
                ActionPoints = actionPoints;
                MoveTerrainMask = moveTerrainMask ?? DefaultMoveMask;
            }
        }

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
        public MoveType[] MoveTerrainMask;

        public List<Coord> movePath = new List<Coord>();
        private Map map = null;

        public Unit(Data data) {
            Coordinate = new Coord();
            Id = data.Id;
            HitPoints = data.HitPoints;
            MaxHitPoints = data.HitPoints;
            ActionPoints = data.ActionPoints;
            MaxActionPoints = data.ActionPoints;
            MoveTerrainMask = data.MoveTerrainMask;
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

        public void SetPositionTo(Coord newCoord, int actionPoints = 0) {
            Coordinate = newCoord;
            ActionPoints -= actionPoints;
        }

        public void AttackUnit(Unit enemy) {
            ActionPoints = 0;
            int damageToEnemy = 0;
            int damageToMe = 0;
            CalculateCombat(enemy, out damageToMe, out damageToEnemy);
            this.OnHit(damageToMe);
            enemy.OnHit(damageToEnemy);
            map.AddAction(new AttackUnitAction(this, enemy, damageToMe, damageToEnemy));
            if (!IsAlive)
                Death();
            if (!enemy.IsAlive)
                enemy.Death();
        }

        public void AttackCity(City enemyCity) {
            ActionPoints = 0;
            int damageToCity = 0;
            int damageToMe = 0;
            CalculateCombat(enemyCity.Garrison, out damageToMe, out damageToCity);
            this.OnHit(damageToMe);
            enemyCity.Garrison.OnHit(damageToCity);
            map.AddAction(new AttackCityAction(this, enemyCity, damageToMe, damageToCity));
            if (!IsAlive)
                Death();
        }

        private void CalculateCombat(Unit enemy, out int damageToMe, out int damageToEnemy) {
            damageToEnemy = 0;
            damageToMe = 0;
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
