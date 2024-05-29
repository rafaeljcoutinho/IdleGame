using System;
using System.Collections.Generic;
using UnityEngine;

public class DropGeneratorService
{
    public Dictionary<Guid, long> GenerateDrops(Droptable baseDropTable, float quantity)
    {
        var profile = Services.Container.Resolve<InventoryService>().PlayerProfile;
        var randomDrops = new List<Droptable.Drop>(10);
        randomDrops.AddRange(baseDropTable.RandomDrops);
        foreach (var droptableUnlock in profile.GetAllUnlockedDrops(baseDropTable))
        {
            randomDrops.Add(droptableUnlock.ToDroptableDrop());
        }

        return GenerateDrop(quantity, randomDrops, baseDropTable.GuaranteedDrops);
    }

    public long GetAverageExpDrop(SkillData.Type skill, List<Droptable.Drop> guaranteedDrops)
    {
        var xpItem = Services.Container.Resolve<SkillService>().SkillDatabase.GetSkillData(skill).XpItem.Uuid;
        foreach (var drop in guaranteedDrops)
        {
            if (drop.item.Uuid == xpItem)
            {
                return drop.minAmount;
            }
        }
        return 0;
    }

    private Dictionary<Guid, long> GenerateDrop(float quantity, List<Droptable.Drop> randomDrops, List<Droptable.Drop> guaranteedDrops)
    {
        var ans = new Dictionary<Guid, long>(10);
        if (randomDrops.Count > 0)
        {
            for (var i = 0; i < quantity; i++)
            {
                ans = Utils.Combine(ans, GenerateRandomWeightDrop(1, randomDrops));
            }
            var remainderDrops = quantity - (long)quantity;
            if(!Mathf.Approximately(remainderDrops, 0)){
                GenerateRandomWeightDrop(remainderDrops, randomDrops);
            }
            
        }

        foreach (var drop in guaranteedDrops)
        {
            var generatedGuaranteedDrops = Utils.Combine(RollItems(drop), quantity);
            ans = Utils.Combine(ans, generatedGuaranteedDrops);
        }
       
        return ans;
    }
    
    Dictionary<Guid, long> GenerateRandomWeightDrop(float percentageMultiplier, List<Droptable.Drop> randomDrops)
    {
        var rand = UnityEngine.Random.value * 100;
        var sum = 0f;
        for (var i = 0; i < randomDrops.Count; i++)
        {
            sum += randomDrops[i].percentage * percentageMultiplier;
            if (rand <= sum)
            {
                return RollItems(randomDrops[i]);
            }
        }

        return new Dictionary<Guid, long>();
    }

    Dictionary<Guid, long> RollItems(Droptable.Drop drop)
    {
        var distributionFactor = 0f;
        if (drop.distribution != null)
        {
            distributionFactor = drop.distribution.Evaluate(UnityEngine.Random.value);   
        }
        var amount = Mathf.RoundToInt(Mathf.Lerp(drop.minAmount, drop.maxAmount, distributionFactor));

        if (drop.droptable == null)
        {
            return new Dictionary<Guid, long> { { drop.item.Uuid, Mathf.RoundToInt(amount) } };   
        }

        var ans = new Dictionary<Guid, long>();
        for (var k = 0; k < amount; k++)
        {
            ans = Utils.Combine(ans, GenerateDrop(1, drop.droptable.RandomDrops, drop.droptable.GuaranteedDrops));   
        }

        return ans;
    }
}
