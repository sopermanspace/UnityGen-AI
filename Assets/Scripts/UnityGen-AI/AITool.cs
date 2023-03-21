using UnityEngine;
using UnityEditor;

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
    }
 // this will create a new AIConfig.asset file in the Assets folder
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
     
 
        // Draw output field/Pannel for response
        if (isProcessing)
        {
            GUILayout.Label("Waiting for response...");
            Debug.Log(response+ "This is a Response from AI Tools");
       
        }
 

        else if (!string.IsNullOrEmpty(response)){

    
            // Draw output panel for response
            
            GUILayout.BeginArea(new Rect(10, 110, position.width - 20, 50000));
            GUILayout.Label("Response:", EditorStyles.boldLabel);
            responseScroll = GUILayout.BeginScrollView(responseScroll);
            GUILayout.Label(response, EditorStyles.textArea);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
  

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
    }

    private void OnResponseReceived(string resp)
    {    
        response = resp;
        isProcessing = false;
    

        // Append response to script as a comment
        string script = GenerateScript();
        script += "\n// Generated response: " + resp;
        string filePath = Application.dataPath + "/response.txt";
        System.IO.File.WriteAllText(filePath, response);
        AssetDatabase.Refresh();
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
