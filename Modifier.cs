using System;
using BTD_Mod_Helper;
using Il2CppAssets.Scripts.Models;

namespace GameModifiers;

public class Modifier
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public bool Good { get; }

    private readonly Action<GameModel> apply;

    public Modifier(string id, string name, string description, bool good, Action<GameModel> apply)
    {
        Id = id;
        Name = name;
        Description = description;
        Good = good;
        this.apply = apply;
    }

    public void TryApply(GameModel model)
    {
        try
        {
            apply(model);
        }
        catch (Exception e)
        {
            ModHelper.Warning<GameModifiers>($"Modifier '{Name}' failed: {e.Message}");
        }
    }
}
