using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public sealed class BuildUnitProject : AbstractProject {
        public readonly Unit.Data UnitData;

        public BuildUnitProject(string id, int cost, Unit.Data unitData) : base(id, cost) {
            UnitData = unitData;
        }

        public override void EndBuilding(City city) {
            // TODO buid unit implementation
        }
    }
}
