public class PlayerSkillInfo
{
    public int Level;
    public long XpInLevel;
    public long TotalXp;
    public long TotalXpToNextLevel;
    public double XpInLevelNormalized => XpInLevel / (double)TotalXpToNextLevel;
}