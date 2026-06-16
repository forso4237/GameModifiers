using System;
using System.Collections.Generic;
using Il2CppAssets.Scripts.Models;

namespace GameModifiers;

internal static class Effects
{
    public static void MultiplyBloonHealth(GameModel model, double f)
    {
        if (model.bloons == null) return;
        foreach (var bloon in model.bloons)
        {
            if (bloon == null) continue;
            var hp = (int) Math.Round(bloon.maxHealth * f);
            bloon.maxHealth = hp < 1 ? 1 : hp;
        }
    }

    public static void MultiplyBloonSpeed(GameModel model, double f)
    {
        if (model.bloons == null) return;
        foreach (var bloon in model.bloons)
        {
            if (bloon == null) continue;
            bloon.speed *= (float) f;
        }
    }

    public static void MultiplyAttackSpeed(GameModel model, double f)
    {
        if (f <= 0) return;
        foreach (var weapon in model.GetAllWeaponModels())
        {
            if (weapon == null) continue;
            weapon.rate /= (float) f;
        }
    }

    public static void MultiplyRange(GameModel model, double f)
    {
        if (model.towers != null)
        {
            foreach (var tower in model.towers)
            {
                if (tower == null) continue;
                tower.range *= (float) f;
            }
        }

        foreach (var attack in model.GetAllAttackModels())
        {
            if (attack == null) continue;
            attack.range *= (float) f;
        }
    }

    public static void MultiplyDamage(GameModel model, double f)
    {
        foreach (var projectile in model.GetAllProjectileModels())
        {
            if (projectile == null) continue;
            var damage = projectile.GetDamageModel();
            if (damage != null) damage.damage *= (float) f;
        }
    }

    public static void MultiplyPierce(GameModel model, double f)
    {
        foreach (var projectile in model.GetAllProjectileModels())
        {
            if (projectile == null) continue;
            if (projectile.pierce > 0) projectile.pierce *= (float) f;
            if (projectile.maxPierce > 0) projectile.maxPierce *= (float) f;
        }
    }

    private static readonly Dictionary<string, int> OriginalCounts = new();

    public static void SetBloonCount(GameModel model, double f)
    {
        var roundSet = model.roundSet;
        if (roundSet?.rounds == null) return;
        var setName = roundSet.name ?? "set";

        var roundIndex = 0;
        foreach (var round in roundSet.rounds)
        {
            if (round?.groups != null)
            {
                var groupIndex = 0;
                foreach (var group in round.groups)
                {
                    if (group == null) { groupIndex++; continue; }

                    var key = setName + "|" + roundIndex + "|" + groupIndex;
                    if (!OriginalCounts.TryGetValue(key, out var original))
                    {
                        original = group.count;
                        OriginalCounts[key] = original;
                    }

                    var c = (int) Math.Round(original * f);
                    group.count = c < 1 ? 1 : c;

                    groupIndex++;
                }
            }
            roundIndex++;
        }
    }
}
