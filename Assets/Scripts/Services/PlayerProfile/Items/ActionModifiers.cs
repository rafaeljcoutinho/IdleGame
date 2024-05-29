using System;
using System.Collections.Generic;

[Serializable]
public class ActionModifiers
{
    [Serializable]
    public class AttributeChange
    {
        public Attribute attribute;
        public float change;

        public float Value => change * (attribute.DisplayAsPercentage ? 100 : 1);
    }
    
    public List<AttributeChange> AttributeChanges;

    public ActionModifiers()
    {
        AttributeChanges = new List<AttributeChange>();
    }
    
    public AttributeChange GetAttributeChange(Attribute attr)
    {
        foreach (var attribute in AttributeChanges)
        {
            if (attribute.attribute == attr)
            {
                return attribute;
            }
        }

        return null;
    }

    public void Reset()
    {
        AttributeChanges.Clear();
    }

    public float GetValueForAttribute(Attribute attribute, float defa)
    {
        foreach (var attributeChange in AttributeChanges)
        {
            if (attributeChange.attribute == attribute)
            {
                return attributeChange.change;
            }
        }

        return defa;
    }

    public void Scale(float scaleFactor)
    {
        foreach (var attributeChange in AttributeChanges)
        {
            attributeChange.change *= scaleFactor;
        }
    }

    public void Combine(ActionModifiers other)
    {
        foreach (var otherAttributeChange in other.AttributeChanges)
        {
            var myAttribute = GetAttributeChange(otherAttributeChange.attribute);
            if (myAttribute == null)
            {
                AttributeChanges.Add(new AttributeChange
                {
                    attribute = otherAttributeChange.attribute,
                    change = otherAttributeChange.change,
                });
            }
            else
            {
                if (myAttribute.attribute.combineType == Attribute.CombineType.Additive)
                {
                    myAttribute.change += otherAttributeChange.change;
                }
                else
                {
                    myAttribute.change *= otherAttributeChange.change;
                }
            }
        }
    }
}