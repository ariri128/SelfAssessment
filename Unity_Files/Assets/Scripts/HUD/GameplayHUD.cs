using System.Collections.Generic;
using UnityEngine;

/*
   Runtime gameplay UI added purely for visual assistance
   Draws with OnGUI so it needs no Canvas/prefab setup, just like AssessmentHUD
*/
public class GameplayHUD : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Auto-filled from GetComponent<WeaponSwitcher>() on this object if left blank.")]
    public WeaponSwitcher weaponSwitcher;

    [Header("Target Destroyed")]
    [Tooltip("How many seconds the 'Target Destroyed' message stays on screen after a kill.")]
    public float targetDestroyedDisplayTime = 2f;

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
