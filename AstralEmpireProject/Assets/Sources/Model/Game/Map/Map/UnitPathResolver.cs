using System.Linq;

namespace Model.PathFind {
    public sealed class UnitPathResolver : Navigation.IPathResolver {
        private readonly Map.CellType[] accessibilityMask;
        private readonly Faction faction;
        private readonly Map map;

        public UnitPathResolver(Map controlMap, Map.CellType[] accessibilityMask, Faction faction) {
            this.map = controlMap;
            this.accessibilityMask = accessibilityMask;
            this.faction = faction;
        }

        public bool CanMoveThrough(Coord coord) {
            var cell = map[coord];
            if (!accessibilityMask.Contains(cell.Type))
                return false;
            if (cell.Unit != null && faction != cell.Unit.Faction)
                return false;
            return true;
        }
    }
}
