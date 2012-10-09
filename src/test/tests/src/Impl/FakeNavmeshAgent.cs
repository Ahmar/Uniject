using System;
using UnityEngine;

namespace Tests {
    public class FakeNavmeshAgent : Testable.TestableComponent, Testable.INavmeshAgent {
        public FakeNavmeshAgent (Testable.TestableGameObject obj) : base(obj) {
        }

        public void setDestination (UnityEngine.Vector3 dest) {
        }

        public void Stop() {
        }

        public void setSpeedMultiplier (float multiplier) {
        }
    
        public void onPlacedOnNavmesh () {
        }

        public ObstacleAvoidanceType obstacleAvoidanceType { get; set; }

        public float BaseOffset { get; set; }

        public bool Enabled { get; set; }
    
        public float radius { get; set; }
    }
}

