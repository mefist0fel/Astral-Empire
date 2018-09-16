namespace Model {
    public sealed class AttackCityAction : Map.AbstractAction {
        public readonly Unit AttackerUnit;
        public readonly City City;
        public readonly int DamageToUnit;
        public readonly int DamageToCity;

        public AttackCityAction(Unit attacker, City city, int damageToUnit, int damageToCity) {
            AttackerUnit = attacker;
            City = city;
            DamageToUnit = damageToUnit;
            DamageToCity = damageToCity;
        }
    }
}