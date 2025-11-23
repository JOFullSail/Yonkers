using UnityEngine;

[CreateAssetMenu(fileName = "DropTable", menuName = "Scriptable Objects/DropTable")]
public class DropTable : ScriptableObject
{
    [System.Serializable]
    public struct DropEntry
    {
        public GameObject dropPrefab;
        [Range(0, 100)]
        public float weight;
    }

    public DropEntry[] drops;
}
