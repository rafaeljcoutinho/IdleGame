using System.Collections.Generic;

public class LocalizationService
{
    public string LocalizeText(string key, params object[] args)
    {
        if (TempLocalizations.Texts.ContainsKey(key))
            return string.Format(TempLocalizations.Texts[key]);
        return string.Format(key, args);
    }
}

public static class TempLocalizations
{
    public static Dictionary<string, string> Texts = new () 
    {
        {"quests.portal_request.title", "A portal to a world of Adventures!"},
        
        {"quests.portal_request.dialog.0", "Well, well, well, look who it is!"},
        {"quests.portal_request.dialog.1", "Another adventurer stuck in an island seeking fame and fortune, eh?"},
        {"quests.portal_request.dialog.2", "Im stuck here? Shit..."},
        {"quests.portal_request.dialog.3", "Lucky for you, I'm building a portal to get us out of here..."},
        {"quests.portal_request.dialog.4", "I just need some materials, but im too old to get them myself. Can you manage that?"},
        {"quests.portal_request.dialog.5", "A portal? Like, a magic door thingy? Cool! What do you need, mysterious old man?"},
        {"quests.portal_request.dialog.6", "I require 10 tree logs and 10 chicken feathers. Can you manage that?"},
        {"quests.portal_request.dialog.7", "Its dangerous to chop a tree with bare hands. Take these!"},
        {"quests.portal_request.dialog.8", "I'm on it! Do I get a prize? Like a shiny thing?"},
        {"quests.portal_request.dialog.9", "Yes, yes, a reward awaits you. Now go, time is of the essence!"},
        {"quests.portal_request.dialog.10","Logs and bones, coming right up! Bye-bye, portal dude!"},
        
        {"ncp.mysterious_old_man.name", "Mysterious old man"},
    };
}

/*
 *
NPC: Well, well, well, look who it is! Another adventurer seeking fame and fortune, eh?
NPC: You've come to the right place! Our little village is in desperate need of some woodcutting expertise. And lucky for us, you seem like the perfect candidate!
Player: I am?
NPC: Of course! I mean, you do have a pulse, right? And two hands? That's all it takes to be a master woodcutter!
Player: Alright...
NPC: Fantastic! Here's an axe and some words of wisdom: chop wood, get logs. Bring 'em back to me and I'll reward you handsomely.
Player: Oh boy, I can't wait.
NPC: Hey! Woodcutting is a noble profession. It's the backbone of our economy!
Player: Sure, sure. I'll get right on it.
NPC: Bring 20 logs to me and I'll reward you handsomely. Who knows? You might also find something valuable out there.
*/