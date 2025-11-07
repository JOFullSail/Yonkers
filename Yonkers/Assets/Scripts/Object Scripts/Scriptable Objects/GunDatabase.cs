using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GunDatabase", menuName = "Scriptable Objects/GunDatabase")]
public class GunDatabase : ScriptableObject
{
    public List<GunStats> allGuns = new List<GunStats>();

    public GunStats GetGunByName(string name)
    {
        return allGuns.Find(g => g.name == name);
    }
}
