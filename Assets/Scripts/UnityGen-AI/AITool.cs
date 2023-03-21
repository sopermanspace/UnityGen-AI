using UnityEngine;
using UnityEditor;
using System.IO;

public class AITool : EditorWindow 
{
    [Tooltip("Prompt to start the AI with.")]
    public static string prompt;
    [Tooltip("Max tokens to generate. Must be between 1 and 2048.")]
    public static int maxTokens = 100;
    [Tooltip("Temperature Controls the Variants. Must be between 0 and 1.")]
    public static float temperature = 0.2f;
    public static string response;
    private Vector2 responseScroll = Vector2.zero;
    public  SmartAI ai;
    private bool isProcessing;
    string scriptName;
    public AIConfig config;


    [MenuItem("UniGen AI/AI Tool")]
    public static void ShowWindow()
    {
       var window = GetWindow<AITool>("Code Tool");
        window.Reset();
    }

       public void Reset()
    {
        prompt = "";
        // response = "";
    }

    public void OnEnable()
    {
        config = AssetDatabase.LoadAssetAtPath<AIConfig>("Assets/AIConfig.asset");

        if (config == null)
        {
            Debug.LogError("No AIConfig found at Assets/AIConfig.asset. Please create one using the 'Create AI Config' option in the Project window.");
        }
        else
        {
            ai = new SmartAI(config.apiKey);
        }
    }

    private void OnResponseReceived(string resp){    
        response = resp;
        isProcessing = false;
        

   // Append response to script as a comment
    string script = GenerateScript();
    script += "\n// Generated response: " + resp;
    string filePath = Application.dataPath + "/response.txt"; // This will Crete A Text File In Assets Folder

    if (File.Exists(filePath))
    {
        File.WriteAllText(filePath, response);
    }
    else
    {
        Debug.LogError("File does not exist: " + filePath);
    }

    AssetDatabase.Refresh();


    // Display message Box to show response has been generated
    EditorUtility.DisplayDialog("Response Received", "The response has been generated.", "OK");
  
    }

    public void OnGUI()
    {
        GUILayout.Label("AI Tool", EditorStyles.boldLabel);

        prompt = EditorGUILayout.TextField("Prompt:", prompt); 
        maxTokens = EditorGUILayout.IntField("Max Tokens:", maxTokens);
        temperature = EditorGUILayout.Slider("Temperature:", temperature, 0f, 1f);

          // Draw Send Request button
        if (GUILayout.Button("Send Request") && !isProcessing && !string.IsNullOrEmpty(prompt))
        {
            Debug.Log("Sending chat request...");
            ai.SendChatRequest(prompt, maxTokens, temperature, OnResponseReceived);
            isProcessing = true;
        }    
     
 
         if (!isProcessing && !string.IsNullOrEmpty(response)){

            // Draw output panel for response
            
            GUILayout.BeginArea(new Rect(10, 110, position.width - 20, 50000));
            GUILayout.Label("Response:", EditorStyles.boldLabel);
            responseScroll = GUILayout.BeginScrollView(responseScroll);
            GUILayout.Label(response, EditorStyles.textArea);
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // Repaint the window to update the scroll view
             Repaint();
            EditorGUILayout.Space(300);

            // Draw Save Script button
              if (GUILayout.Button("Save Script"))
            {
            scriptName = EditorUtility.SaveFilePanel("Save C# Script", "", "MyScript", "cs");
            if (!string.IsNullOrEmpty(scriptName))
            {
                System.IO.File.WriteAllText(scriptName, GenerateScript());
            }
          }
        }  
        else if (string.IsNullOrEmpty(prompt)){
         GUILayout.Label("Please enter a prompt.", EditorStyles.helpBox);
       }
    }

    private string GenerateScript()
    {
        string script = @"using UnityEngine;
public class MyScript : MonoBehaviour
{
    private void Start()
    {
        // Insert code here
     
    }
}";
        return script;
    }
}
