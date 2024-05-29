using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Skill", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    public enum Type
    {
        Woodcutting,
        Fishing,
        Mining,
        Melee,
        Cooking,
        Firemaking,
        Character,
        None,
        Ranged,
        Crafting,
    }

    [SerializeField] private Item xpItem;
    [SerializeField] private string nameKey;
    [SerializeField] private Sprite skillIcon;
    [SerializeField] private Type type;

    public ActionModifiers startingModifiers;
    public ActionModifiers modifiersPerLevel;
    public ActionModifiers maxLevelupModifier;

    public Attribute power;
    public Attribute powerMulti;
    public Attribute speedMulti;
    public Attribute critChance;
    public Attribute critDamage;
    public Attribute range;
    public Attribute accu;
    public Attribute bonusXp;

    public Item XpItem => xpItem;
    public string NameKey => nameKey;
    public Sprite SkillIcon => skillIcon;
    public Type SkillType => type;

    public ActionModifiers ModifiersForLevel(int level)
    {
        var ans = new ActionModifiers();
        ans.Combine(modifiersPerLevel);
        ans.Scale(level);

        foreach (var maxAttribute in maxLevelupModifier.AttributeChanges)
        {
            var attrChange = ans.GetAttributeChange(maxAttribute.attribute);
            if (attrChange.change > maxAttribute.change)
            {
                attrChange.change = maxAttribute.change;
            }
        }
        ans.Combine(startingModifiers);
        return ans;
    }
    
    public static List<PrettyModifierInfo> GetPrettyModifier(ActionModifiers actionModifiers)
    {
        var ans = new List<PrettyModifierInfo>();
        foreach (var attributeChange in actionModifiers.AttributeChanges)
        {
            var value = attributeChange.Value;
            ans.Add(new PrettyModifierInfo
            {
                Name = attributeChange.attribute.LocalizedName,
                FormattedValue = SkillExperienceTable.Format(value) + attributeChange.attribute.Suffix,
                Value = value,
            });
        }

        return ans;
    }
    
    public static List<PrettyModifierInfo> GetPrettyModifierComparison(ActionModifiers myModifiers, ActionModifiers otherModifiers)
    {
        var ans = new List<PrettyModifierInfo>();
        foreach (var attributeChange in myModifiers.AttributeChanges)
        {
            var otherChange = otherModifiers.GetAttributeChange(attributeChange.attribute);
            var value = attributeChange.Value;
            var delta = value;
            if (otherChange != null)
            {
                delta = value - otherChange.Value;
            }
            ans.Add(new PrettyModifierInfo
            {
                Name = attributeChange.attribute.LocalizedName,
                FormattedValue = SkillExperienceTable.Format(value) + attributeChange.attribute.Suffix,
                Delta = delta,
                DeltaFormatted = SkillExperienceTable.Format(delta) + attributeChange.attribute.Suffix,
                Value = value,
            });
        }
        foreach (var attributeChange in otherModifiers.AttributeChanges)
        {
            var myChange = myModifiers.GetAttributeChange(attributeChange.attribute);
            if (myChange == null)
            {
                ans.Add(new PrettyModifierInfo
                {
                    Name = attributeChange.attribute.LocalizedName,
                    FormattedValue = string.Empty,
                    Delta = -attributeChange.Value,
                    DeltaFormatted = SkillExperienceTable.Format(-attributeChange.Value) + attributeChange.attribute.Suffix,
                    Value = 0,
                });
            }
        }

        return ans;
    }

    public class PrettyModifierInfo
    {
        public string Name;
        public string FormattedValue;
        public string DeltaFormatted;
        public float Delta;
        public float Value;
    }
}