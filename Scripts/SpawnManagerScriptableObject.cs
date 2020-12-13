using UnityEditor;
using UnityEngine;

namespace Assets.Cubeage.Scripts
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
    public class SpawnManagerScriptableObject : ScriptableObject
    {
        public string prefabName;

        public int numberOfPrefabsToCreate;
        public Vector3[] spawnPoints;
    }
}