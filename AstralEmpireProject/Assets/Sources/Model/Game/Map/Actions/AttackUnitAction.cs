namespace Model {
    public sealed class AttackUnitAction : Map.AbstractAction {
        public readonly Unit AttackerUnit;
        public readonly Unit DefensiveUnit;
        public readonly int AttackerDamage;
        public readonly int DefenserDamage;

        public AttackUnitAction(Unit attacker, Unit defenser, int attackerDamage, int defensiveDamage) {
            AttackerUnit = attacker;
            DefensiveUnit = defenser;
            AttackerDamage = attackerDamage;
            DefenserDamage = defensiveDamage;
        }
    }
}