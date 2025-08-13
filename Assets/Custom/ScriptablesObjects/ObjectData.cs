using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Scriptable Objects/ObjectData")]
public class ObjectData : ScriptableObject
{
    public GameObject prefab;
    public int pointValue;
    public string objectName;
    internal Sprite icon;
    internal string displayName;
    internal string id;
}
