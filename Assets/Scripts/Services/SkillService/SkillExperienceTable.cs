using System;
using System.Collections.Generic;
using System.Linq;

public class SkillExperienceTable
{
    private List<long> ExperienceRequiredToLevel;
    private List<long> TotalExperience;

    private static long CalculateExperienceRequired(float level)
    {
        double baseExp = 13034430d / (Math.Pow(Math.Log(200 + 5.2), 33));
        double bi = Math.Log(level + 41.6);
        return (long) (Math.Pow(bi, 33) * baseExp + 1);
    }

    static readonly string[] suffixes = { "", "K", "M", "B", "T", "Q" };
    private static readonly string G3Format = "G3";
    public static string Format(float number)
    {
        int suffixIndex = 0;

        while (Math.Abs(number) >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            number /= 1000;
            suffixIndex++;
        }
        var formattedNumber = number.ToString(G3Format);
        return formattedNumber + suffixes[suffixIndex];
    }
    
    public static string Format(long number)
    {
        return Format(Convert.ToSingle(number));
    }

    public int GetLevel(long experience)
    {
        return (int) BinarySearch(experience, 0, TotalExperience.Count - 1);
    }

    long BinarySearch(long experience, int left, int right)
    {
        if (left > right)
        {
            return left;
        }

        var mid = left + (right - left) / 2;

        if (TotalExperience[mid] == experience)
        {
            return mid + 1;
        }

        if (TotalExperience[mid] < experience)
        {
            return BinarySearch(experience, mid + 1, right);
        }

        return BinarySearch(experience, left, mid - 1);
    }

    public long GetCurrentExperienceInLevel(long experience)
    {
        for (var i = 1; i < TotalExperience.Count; i++)
        {
            if (TotalExperience[i] > experience)
                return experience - TotalExperience[i-1];
        }

        return experience - TotalExperience[^1];
    }

    public long GetExperienceToNextLevel(int currentLevel)
    {
        return ExperienceRequiredToLevel[currentLevel];
    }

    public SkillExperienceTable()
    {
        int maxLevel = 500;
        ExperienceRequiredToLevel = new long[maxLevel].ToList();
        TotalExperience = new long[maxLevel].ToList();
        for (var i = 1; i < maxLevel; i++)
        {
            ExperienceRequiredToLevel[i] = CalculateExperienceRequired(i);
            TotalExperience[i] = TotalExperience[i-1] + ExperienceRequiredToLevel[i];
        }
    }
}