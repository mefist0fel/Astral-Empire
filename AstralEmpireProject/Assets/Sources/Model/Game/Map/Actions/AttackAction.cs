using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class AttackAction : Map.Action {
        public readonly Unit AttackerUnit;
        public readonly Unit DefenciveUnit;
        public readonly int Damage;

        public AttackAction(Unit attacker, Unit defencive, int damage) {
            AttackerUnit = attacker;
            DefenciveUnit = defencive;
            Damage = damage;
        }
    }
}