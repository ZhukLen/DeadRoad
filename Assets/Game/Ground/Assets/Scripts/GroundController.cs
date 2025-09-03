using Game.Common.Scripts.Game;
using UnityEngine;

namespace Game.Ground.Assets.Scripts
{
    public class GroundController : DistanceTracker<GroundController>
    {
        [SerializeField] private Collider _collider;

        public Vector3 BoxSize => ((BoxCollider)_collider).size;
    }
}