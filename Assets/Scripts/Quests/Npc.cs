using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

[CreateAssetMenu(fileName ="Npc", menuName = "Quests/Npc")]
public class Npc : ScriptableObject 
{
    public Sprite sprite;
    public string npcNameKey;
    public List<string> barks;
}