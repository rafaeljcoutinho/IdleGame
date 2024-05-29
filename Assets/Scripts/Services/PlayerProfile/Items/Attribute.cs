using UnityEngine;

[CreateAssetMenu(fileName = "Attribute", menuName = "Skill/Attribute")]
public class Attribute : ScriptableObject
{
    public enum CombineType
    {
        Additive,
        Multiplicative,
    }

    public string LocalizedName;
    public string Suffix;
    public CombineType combineType;
    public bool DisplayAsPercentage;
}