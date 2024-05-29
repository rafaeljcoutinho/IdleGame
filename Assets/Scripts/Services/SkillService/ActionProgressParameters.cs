using System;

[Serializable]
public class ActionProgressParameters
{
    public int BasePower;
    public float ActionFreqHz;
    public float CritChance;
    public float SuccessChance;
    public float CrititalDamage;
    public float Range;

    public ActionProgressParameters Diff(ActionProgressParameters a, ActionProgressParameters b)
    {
        return new ActionProgressParameters
        {
            BasePower = a.BasePower - b.BasePower,
            ActionFreqHz = a.ActionFreqHz - b.ActionFreqHz,
            CritChance = a.CritChance - b.CritChance,
            SuccessChance = a.SuccessChance - b.SuccessChance,
            CrititalDamage = a.CrititalDamage - b.CrititalDamage,
            Range = a.Range - b.Range,
        };
    }
}