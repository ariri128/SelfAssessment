using UnityEngine;

/*
   Required Toggles:
     'P' toggles name + brand on screen
     'O' toggles the objectives list on screen
   Uses OnGUI for speed meaning no Canvas/prefab setup is needed
*/
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

    public float referenceScreenHeight = 1080f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) showPlayerInfo = !showPlayerInfo;
        if (Input.GetKeyDown(KeyCode.O)) showObjectives = !showObjectives;
    }
    void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = BuildFixedColorStyle(32, Color.white);
        bodyStyle = BuildFixedColorStyle(23, Color.yellow);
    }
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

        float uiScale = Screen.height / referenceScreenHeight;
        Matrix4x4 originalMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(uiScale, uiScale, 1f));

        float y = 20f;
        if (showPlayerInfo)
        {
            float h = titleStyle.CalcHeight(new GUIContent(playerInfoText), LabelWidth);
            GUI.Label(new Rect(LabelX, y, LabelWidth, h), playerInfoText, titleStyle);
            y += h + 8f;
        }
        if (showObjectives)
        {
            float h = bodyStyle.CalcHeight(new GUIContent(objectivesText), LabelWidth);
            GUI.Label(new Rect(LabelX, y, LabelWidth, h), objectivesText, bodyStyle);
        }

        GUI.matrix = originalMatrix;
    }
}
