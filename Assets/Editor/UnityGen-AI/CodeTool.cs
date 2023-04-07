using UnityEditor;
using UnityEngine;

public class CodeTool : EditorWindow
{
    private string responseText = "";

    public void ShowResponse(string response)
    {
        responseText = response;
        Repaint();
    }

    public void OnGUI()
    {
        // Display the response text in a text area
        EditorGUILayout.LabelField("Response:", EditorStyles.boldLabel);
        responseText = EditorGUILayout.TextArea(responseText, GUILayout.ExpandHeight(true));
    }
    
}