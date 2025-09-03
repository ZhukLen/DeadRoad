using UnityEngine;

namespace Game.Common.Scripts.Game
{
    [CreateAssetMenu(fileName = "New Base Game Settings", menuName = "Game Data/Base Game Settings")]
    public class BaseGameSettings : ScriptableObject
    {
        [Header("Level Settings")]
        [Tooltip("Sections Count")] public int RoadLength = 5;
        [Space (10)]

        [Header("Enemy Settings")]
        [Tooltip("Enemy Count Per Section")] public int EnemyCount = 15;
        public float EnemyHealth = 5;
        public float EnemySpeed = 2;
        public float EnemyDamage  = 1;
        public float EnemyDetectRange  = 25;
        [Space (10)]

        [Header("Car Settings")]
        public float CarHealth  = 10;
        public float CarSpeed  = 5;
        [Space (10)]

        [Header("Fire Settings")]
        public float FireRate = 3;
        public float BulletDamage = 5;
        public float BulletSpeed = 30;
    }
}
