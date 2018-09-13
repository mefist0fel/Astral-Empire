using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class Projects {
        public readonly BuildUnitProject Infantry;
        public readonly BuildUnitProject HeavyInfantry;
        public readonly BuildUnitProject AIV;
        public readonly BuildUnitProject Tank;

        public Projects(Game game) {
            Infantry = new BuildUnitProject("infantry", 5, game, new Unit.Data("infantry", 2, 3));
            HeavyInfantry = new BuildUnitProject("heavy_infantry", 7, game, new Unit.Data("heavy_infantry", 3, 3));
            AIV = new BuildUnitProject("AIV", 10, game, new Unit.Data("AIV", 2, 3));
            Tank = new BuildUnitProject("tank", 20, game, new Unit.Data("tank", 2, 3));
        }        
	}
}
