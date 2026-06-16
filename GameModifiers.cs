global using BTD_Mod_Helper.Extensions;
using System.Collections.Generic;
using MelonLoader;
using BTD_Mod_Helper;
using GameModifiers;
using Il2CppAssets.Scripts.Models;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[assembly: MelonInfo(typeof(GameModifiers.GameModifiers), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace GameModifiers;

public class GameModifiers : BloonsTD6Mod
{
    public static readonly HashSet<string> Enabled = new();

    public static readonly List<Modifier> Modifiers = new()
    {
        new Modifier("brittle", "Brittle Bloons", "Bloons have half health.",
            true, m => Effects.MultiplyBloonHealth(m, 0.5)),
        new Modifier("sharp", "Sharp Shots", "Towers deal double damage.",
            true, m => Effects.MultiplyDamage(m, 2.0)),
        new Modifier("pierce", "Piercing Power", "Each shot pops twice as many bloons.",
            true, m => Effects.MultiplyPierce(m, 2.0)),
        new Modifier("eagle", "Eagle Eye", "Towers have 50% more range.",
            true, m => Effects.MultiplyRange(m, 1.5)),
        new Modifier("rapid", "Rapid Fire", "Towers attack 50% faster.",
            true, m => Effects.MultiplyAttackSpeed(m, 1.5)),

        new Modifier("iron", "Iron Bloons", "Bloons have double health.",
            false, m => Effects.MultiplyBloonHealth(m, 2.0)),
        new Modifier("fast", "Need for Speed", "Bloons move 50% faster.",
            false, m => Effects.MultiplyBloonSpeed(m, 1.5)),
        new Modifier("swarm", "Swarm", "50% more bloons every round.",
            false, m => Effects.SetBloonCount(m, 1.5)),
        new Modifier("sluggish", "Sluggish Towers", "Towers attack 30% slower.",
            false, m => Effects.MultiplyAttackSpeed(m, 0.7)),
        new Modifier("tunnel", "Tunnel Vision", "Towers have 25% less range.",
            false, m => Effects.MultiplyRange(m, 0.75)),
    };

    public override void OnApplicationStart()
    {
        ModHelper.Msg<GameModifiers>(
            "GameModifiers loaded! Choose modifiers on the difficulty/mode select screen.");
    }

    public override void OnNewGameModel(GameModel result)
    {
        Effects.SetBloonCount(result, 1.0);

        var active = new List<string>();
        foreach (var modifier in Modifiers)
        {
            if (!Enabled.Contains(modifier.Id)) continue;
            modifier.TryApply(result);
            active.Add(modifier.Name);
        }

        if (active.Count > 0)
        {
            ModHelper.Msg<GameModifiers>("Active modifiers: " + string.Join(", ", active));
        }
    }

    public override void OnUpdate()
    {
        ModifierMenu.OnUpdate();
    }

    public override void OnPointerEnterSelectable(Selectable button, PointerEventData eventData)
    {
        ModifierMenu.OnPointerEnter(button);
    }

    public override void OnPointerExitSelectable(Selectable button, PointerEventData eventData)
    {
        ModifierMenu.OnPointerExit(button);
    }
}
