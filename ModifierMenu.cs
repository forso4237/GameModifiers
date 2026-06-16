using System;
using UnityEngine;
using UnityEngine.UI;
using Il2CppTMPro;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;

namespace GameModifiers;

internal static class ModifierMenu
{
    private const string ButtonName = "GameModifiersButton";
    private const string DropdownName = "GameModifiersDropdown";
    private const string CheckboxPrefix = "modcb_";
    private const float HoverDelay = 1f;

    private static readonly string[] ShowOnMenus =
    {
        SceneNames.DifficultySelectUI,
        SceneNames.ModeSelectUI
    };

    private static readonly Color GoodColor = new(0.55f, 1f, 0.55f);
    private static readonly Color BadColor = new(1f, 0.55f, 0.55f);
    private static readonly Color TipColor = new(1f, 0.95f, 0.7f);

    private static ModHelperButton? button;
    private static ModHelperText? buttonText;
    private static ModHelperPanel? dropdown;
    private static Animator? dropdownAnimator;
    private static ModHelperText? tooltipText;

    private static bool onScreen;
    private static bool expanded;
    private static bool buildFailed;

    private static string? hoveredId;
    private static float hoverStart;
    private static string? tooltipFor;

    public static void OnUpdate()
    {
        try
        {
            if (MenuManager.instance == null) { SetOnScreen(false); return; }

            var show = InGame.instance == null &&
                       Array.IndexOf(ShowOnMenus, MenuManager.instance.GetCurrentMenuName()) >= 0;

            if (show != onScreen) SetOnScreen(show);

            UpdateTooltip();
        }
        catch (Exception e)
        {
            ModHelper.Warning<GameModifiers>("Modifier menu error: " + e.Message);
        }
    }

    private static void SetOnScreen(bool show)
    {
        onScreen = show;

        if (show)
        {
            if (buildFailed) return;
            EnsureBuilt();
            if (button != null) button.SetActive(true);
            Collapse(true);
        }
        else
        {
            if (button != null) button.SetActive(false);
            if (dropdown != null) dropdown.SetActive(false);
            expanded = false;
            ClearHover();
        }
    }

    private static void EnsureBuilt()
    {
        var foreground = CommonForegroundScreen.instance;
        if (foreground == null) return;

        if (button != null && foreground.transform.Find(ButtonName) != null) return;

        try
        {
            Build(foreground.gameObject);
        }
        catch (Exception e)
        {
            buildFailed = true;
            ModHelper.Warning<GameModifiers>("Failed to build modifier menu: " + e.Message);
        }
    }

    private static void Build(GameObject screen)
    {
        button = screen.AddButton(new Info(ButtonName)
        {
            Anchor = new Vector2(0.5f, 1),
            Pivot = new Vector2(0.5f, 1),
            Y = -90, Width = 560, Height = 130
        }, VanillaSprites.GreenBtnLong, new Action(Toggle));
        buttonText = button.AddText(new Info("Text", 520, 110), "Show Modifiers", 52f);

        dropdown = screen.AddModHelperPanel(new Info(DropdownName)
        {
            Anchor = new Vector2(0.5f, 1),
            Pivot = new Vector2(0.5f, 1),
            Y = -230, Width = 1480, Height = 900
        }, VanillaSprites.MainBGPanelBlue, RectTransform.Axis.Vertical, 16, 50);

        dropdownAnimator = dropdown.AddComponent<Animator>();
        dropdownAnimator.runtimeAnimatorController = Animations.PopupAnim;
        dropdownAnimator.speed = 1.6f;

        dropdown.AddText(new Info("Title", 1380, 80), "Game Modifiers", 60f);
        dropdown.AddText(new Info("Subtitle", 1380, 46),
            "Tick any you like \u2014 green helps you, red makes it harder. They stack.", 30f);

        var columns = dropdown.AddPanel(new Info("Columns", 1380, 520), null,
            RectTransform.Axis.Horizontal, 80, 0);
        columns.LayoutGroup.childAlignment = TextAnchor.UpperCenter;

        var helpful = columns.AddPanel(new Info("Helpful", 640, 520), null,
            RectTransform.Axis.Vertical, 12, 0);
        helpful.LayoutGroup.childAlignment = TextAnchor.UpperLeft;
        SetColor(helpful.AddText(new Info("HelpfulHeader", 620, 60), "Helpful", 46f), GoodColor);

        var harmful = columns.AddPanel(new Info("Harmful", 640, 520), null,
            RectTransform.Axis.Vertical, 12, 0);
        harmful.LayoutGroup.childAlignment = TextAnchor.UpperLeft;
        SetColor(harmful.AddText(new Info("HarmfulHeader", 620, 60), "Harmful", 46f), BadColor);

        foreach (var modifier in GameModifiers.Modifiers)
        {
            AddRow(modifier.Good ? helpful : harmful, modifier);
        }

        tooltipText = dropdown.AddText(new Info("Tooltip", 1380, 80), "", 34f);
        SetColor(tooltipText, TipColor);

        dropdown.SetActive(false);
    }

    private static void AddRow(ModHelperPanel column, Modifier modifier)
    {
        var m = modifier;

        var row = column.AddPanel(new Info("row_" + m.Id, 620, 84), null,
            RectTransform.Axis.Horizontal, 18, 0);
        row.LayoutGroup.childAlignment = TextAnchor.MiddleLeft;

        row.AddCheckbox(new Info(CheckboxPrefix + m.Id, 68, 68),
            GameModifiers.Enabled.Contains(m.Id),
            VanillaSprites.BlueInsertPanelRound, new Action<bool>(on =>
            {
                if (on) GameModifiers.Enabled.Add(m.Id);
                else GameModifiers.Enabled.Remove(m.Id);
                if (MenuManager.instance != null)
                {
                    MenuManager.instance.buttonClick3Sound.Play("ClickSounds");
                }
            }));

        SetColor(row.AddText(new Info("label", 520, 68), m.Name, 38f, TextAlignmentOptions.Left),
            m.Good ? GoodColor : BadColor);
    }

    private static void Toggle()
    {
        if (expanded) Collapse(false); else Expand();
        if (MenuManager.instance != null)
        {
            MenuManager.instance.buttonClick2Sound.Play("ClickSounds");
        }
    }

    private static void Expand()
    {
        if (dropdown == null) return;
        expanded = true;
        dropdown.SetActive(true);
        if (dropdownAnimator != null) dropdownAnimator.Play("PopupScaleIn");
        if (buttonText != null) buttonText.SetText("Hide Modifiers");
    }

    private static void Collapse(bool instant)
    {
        expanded = false;
        ClearHover();
        if (buttonText != null) buttonText.SetText("Show Modifiers");
        if (dropdown == null) return;

        if (instant)
        {
            dropdown.SetActive(false);
            return;
        }

        if (dropdownAnimator != null) dropdownAnimator.Play("PopupScaleOut");
        TaskScheduler.ScheduleTask(new Action(() =>
        {
            if (!expanded && dropdown != null) dropdown.SetActive(false);
        }), ScheduleType.WaitForFrames, 12);
    }

    public static void OnPointerEnter(Selectable selectable)
    {
        try
        {
            var name = selectable.gameObject.name;
            if (name != null && name.StartsWith(CheckboxPrefix))
            {
                hoveredId = name.Substring(CheckboxPrefix.Length);
                hoverStart = Time.time;
            }
        }
        catch { }
    }

    public static void OnPointerExit(Selectable selectable)
    {
        try
        {
            var name = selectable.gameObject.name;
            if (name != null && name.StartsWith(CheckboxPrefix)) ClearHover();
        }
        catch { }
    }

    private static void UpdateTooltip()
    {
        if (!expanded || tooltipText == null) return;

        if (hoveredId == null)
        {
            if (tooltipFor != null) { tooltipText.SetText(""); tooltipFor = null; }
            return;
        }

        if (tooltipFor == hoveredId) return;
        if (Time.time - hoverStart < HoverDelay) return;

        var mod = GameModifiers.Modifiers.Find(x => x.Id == hoveredId);
        if (mod != null)
        {
            tooltipText.SetText(mod.Name + ":  " + mod.Description);
            tooltipFor = hoveredId;
        }
    }

    private static void ClearHover()
    {
        hoveredId = null;
        tooltipFor = null;
        if (tooltipText != null) tooltipText.SetText("");
    }

    private static void SetColor(ModHelperText text, Color color)
    {
        try { text.Text.color = color; }
        catch { }
    }
}
