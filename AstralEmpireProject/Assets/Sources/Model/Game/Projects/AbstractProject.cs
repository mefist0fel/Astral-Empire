using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model {
    public abstract class AbstractProject {
        public readonly string ID;
        public readonly int Cost;

        public AbstractProject(string id, int cost) {
            ID = id;
            Cost = cost;
        }

        public abstract void EndBuilding(City city);
	}
}
