using System.Collections.Generic;
using UnityEngine;

// GameplayHUD.cs
// Runtime gameplay UI - separate from AssessmentHUD, which only handles the required P/O
// toggles. Draws with OnGUI so it needs no Canvas/prefab setup, matching the rest of the
// starter code's HUD approach. Three pieces, positioned so none of them can ever overlap:
//
//  - TOP-RIGHT (always visible): the weapon toggle list (1-4), tight-packed with no gap
//    between entries. Whichever weapon is currently equipped gets a red "(equipped)" tag as
//    an extra line right under its entry - that's the only entry that gets a second line.
//  - BOTTOM-CENTER (only while relevant): "Press [F] to collect" - shown while the player is
//    standing in a collectible's trigger radius and it hasn't been picked up yet. Disappears
//    the instant they leave range or collect it.
//  - TOP-LEFT (flashes briefly): "Target Destroyed" - shown for a couple seconds whenever a
//    WanderingAI's health hits 0. Opposite corner from the weapon list (top-right) and
//    opposite side from the collect prompt (bottom-center), so it can never overlap either.
//
// SETUP: put this on whichever object AssessmentHUD is on (Player or a GameManager object -
// either works). `weaponSwitcher` auto-fills via GetComponent, falling back to a scene-wide
// FindObjectOfType<WeaponSwitcher>() if it isn't on the same object - so it doesn't matter
// whether this and WeaponSwitcher share a GameObject or not. You can also just drag the
// Player object into the `weaponSwitcher` field by hand if you'd rather wire it explicitly.
public class GameplayHUD : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Auto-filled from GetComponent<WeaponSwitcher>() on this object if left blank.")]
    public WeaponSwitcher weaponSwitcher;

    [Header("Target Destroyed")]
    [Tooltip("How many seconds the 'Target Destroyed' message stays on screen after a kill.")]
    public float targetDestroyedDisplayTime = 2f;

    // (number key, weapon mode, display label) - drives both the panel text and which entry
    // gets the red "(equipped)" tag. Numbers here match the 1/2/3/4 binds in WeaponSwitcher.Update.
    static readonly (int key, WeaponMode mode, string label)[] WeaponEntries =
    {
        (1, WeaponMode.Hitscan, "Trace Fire"),
        (2, WeaponMode.Linear, "Linear Projectile"),
        (3, WeaponMode.Arched, "Arched Projectile"),
        (4, WeaponMode.Homing, "Homing Missile"),
    };

    readonly HashSet<CollectibleItem> collectiblesInRange = new HashSet<CollectibleItem>();
    float targetDestroyedTimer;

    GUIStyle weaponNameStyle;
    GUIStyle equippedStyle;
    GUIStyle promptStyle;
    GUIStyle promptShadowStyle;
    GUIStyle targetDestroyedStyle;
    GUIStyle targetDestroyedShadowStyle;

    void Awake()
    {
        // Falls back to a scene-wide search if this isn't sitting on the same object as
        // WeaponSwitcher (e.g. GameplayHUD lives on a GameManager object instead of the
        // Player) - so it works either way without forcing a particular object layout.
        if (!weaponSwitcher) weaponSwitcher = GetComponent<WeaponSwitcher>();
        if (!weaponSwitcher) weaponSwitcher = FindObjectOfType<WeaponSwitcher>();
    }

    void OnEnable()
    {
        CollectibleItem.AnyArmed += HandleCollectibleArmed;
        CollectibleItem.AnyDisarmed += HandleCollectibleDisarmed;
        WanderingAI.AnyTargetDestroyed += HandleTargetDestroyed;
    }

    void OnDisable()
    {
        CollectibleItem.AnyArmed -= HandleCollectibleArmed;
        CollectibleItem.AnyDisarmed -= HandleCollectibleDisarmed;
        WanderingAI.AnyTargetDestroyed -= HandleTargetDestroyed;
    }

    void Update()
    {
        if (targetDestroyedTimer > 0f)
        {
            targetDestroyedTimer -= Time.deltaTime;
        }
    }

    void HandleCollectibleArmed(CollectibleItem item) => collectiblesInRange.Add(item);

    void HandleCollectibleDisarmed(CollectibleItem item) => collectiblesInRange.Remove(item);

    // Any WanderingAI dying (re)starts the display timer, so if targets die close together the
    // message just stays up rather than flickering off and back on.
    void HandleTargetDestroyed(WanderingAI ai) => targetDestroyedTimer = targetDestroyedDisplayTime;

    void OnGUI()
    {
        BuildStyles();
        DrawWeaponPanel();
        DrawCollectPrompt();
        DrawTargetDestroyed();
    }

    void BuildStyles()
    {
        // Styles are built lazily inside OnGUI because GUI.skin isn't valid until the first
        // OnGUI call - same reasoning as AssessmentHUD.
        if (weaponNameStyle != null) return;

        weaponNameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.UpperRight,
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };
        weaponNameStyle.normal.textColor = Color.white;

        equippedStyle = new GUIStyle(weaponNameStyle) { fontStyle = FontStyle.Bold };
        equippedStyle.normal.textColor = new Color(0.95f, 0.15f, 0.15f);

        promptStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        promptStyle.normal.textColor = Color.white;

        // Cheap drop-shadow (a second label offset by a couple px underneath) so the prompt
        // and destroyed text stay readable over any background, not just dark ones.
        promptShadowStyle = new GUIStyle(promptStyle);
        promptShadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.75f);

        targetDestroyedStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.UpperLeft,
            fontStyle = FontStyle.Bold
        };
        targetDestroyedStyle.normal.textColor = new Color(0.95f, 0.15f, 0.15f);

        targetDestroyedShadowStyle = new GUIStyle(targetDestroyedStyle);
        targetDestroyedShadowStyle.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
    }

    void DrawWeaponPanel()
    {
        const float panelWidth = 260f;
        const float lineHeight = 18f;
        const float margin = 20f;

        WeaponMode? current = weaponSwitcher ? weaponSwitcher.currentMode : (WeaponMode?)null;

        float x = Screen.width - panelWidth - margin;
        float y = margin;

        foreach (var entry in WeaponEntries)
        {
            float damage = weaponSwitcher ? weaponSwitcher.GetWeaponDamage(entry.mode) : 0f;
            string line = $"{entry.key} - {entry.label} (Dmg {damage:0})";

            GUI.Label(new Rect(x, y, panelWidth, lineHeight), line, weaponNameStyle);
            y += lineHeight;

            // Only the currently-equipped entry gets the extra "(equipped)" line - every other
            // entry just runs straight into the next one, no reserved blank gap.
            if (current.HasValue && entry.mode == current.Value)
            {
                GUI.Label(new Rect(x, y, panelWidth, lineHeight), "(equipped)", equippedStyle);
                y += lineHeight;
            }
        }
    }

    void DrawCollectPrompt()
    {
        if (collectiblesInRange.Count == 0) return;

        const float width = 420f;
        const float height = 32f;
        const float bottomMargin = 28f;

        Rect rect = new Rect((Screen.width - width) / 2f, Screen.height - height - bottomMargin, width, height);
        Rect shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);

        GUI.Label(shadowRect, "Press [F] to collect", promptShadowStyle);
        GUI.Label(rect, "Press [F] to collect", promptStyle);
    }

    void DrawTargetDestroyed()
    {
        if (targetDestroyedTimer <= 0f) return;

        const float width = 320f;
        const float height = 36f;
        const float margin = 20f;

        Rect rect = new Rect(margin, margin, width, height);
        Rect shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);

        GUI.Label(shadowRect, "Target Destroyed", targetDestroyedShadowStyle);
        GUI.Label(rect, "Target Destroyed", targetDestroyedStyle);
    }
}
