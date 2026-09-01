using UnityEngine;

// AssessmentHUD.cs
// REQUIRED regardless of which tests you chose:
//   'P' toggles name + brand on screen
//   'O' toggles the objectives list on screen
// Uses OnGUI for speed (no Canvas/prefab setup needed) - perfectly fine for a dev-build
// assessment submission. Attach to any always-active GameObject (e.g. the player or a
// dedicated "GameManager" object).
public class AssessmentHUD : MonoBehaviour
{
    [TextArea] public string playerInfoText = "First Last - Programmer";
    [TextArea]
    public string objectivesText =
        "Weapon switching (trace fire / linear projectile / arched projectile) | " +
        "Homing missile with lock-on tracking a moving AI | " +
        "Save & load (location, orientation, progress, collectibles)";

    bool showPlayerInfo = false;
    bool showObjectives = false;

    GUIStyle titleStyle;
    GUIStyle bodyStyle;

    const float LabelX = 20f;
    const float LabelWidth = 900f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) showPlayerInfo = !showPlayerInfo;
        if (Input.GetKeyDown(KeyCode.O)) showObjectives = !showObjectives;
    }

    void EnsureStyles()
    {
        // Styles are built lazily because GUI.skin isn't valid until the first OnGUI call.
        if (titleStyle != null) return;

        titleStyle = BuildFixedColorStyle(22, Color.white);
        bodyStyle = BuildFixedColorStyle(16, Color.yellow);
    }

    // Unity's default label style uses a DIFFERENT text color for hover/active/focused than
    // for normal - so a plain GUI.Label can visibly change color just because the mouse
    // happens to be over its screen region, even though it isn't interactive. Pinning every
    // state to the same color stops that.
    GUIStyle BuildFixedColorStyle(int fontSize, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true };
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
        return style;
    }

    void OnGUI()
    {
        EnsureStyles();
        float y = 20f;

        if (showPlayerInfo)
        {
            // CalcHeight measures how tall the wrapped text actually needs at this width,
            // instead of guessing a fixed Rect height that clips longer text.
            float h = titleStyle.CalcHeight(new GUIContent(playerInfoText), LabelWidth);
            GUI.Label(new Rect(LabelX, y, LabelWidth, h), playerInfoText, titleStyle);
            y += h + 8f;
        }

        if (showObjectives)
        {
            float h = bodyStyle.CalcHeight(new GUIContent(objectivesText), LabelWidth);
            GUI.Label(new Rect(LabelX, y, LabelWidth, h), objectivesText, bodyStyle);
        }
    }
}
