using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
public class AITool : EditorWindow 
{
    // Public variables
    [Tooltip("Prompt to start the AI with.")]
    public static string prompt;

    [Tooltip("Max tokens to generate. Must be between 1 and 2048.")]
    public static int maxTokens;

    [Tooltip("Temperature Controls the Variants. Must be between 0 and 1.")]
    public static float temperature;

    // Private variables
    private Vector2 responseScroll = Vector2.zero;
    private bool isProcessing = false;
    private string response;
    private string scriptName;
    private SmartAI ai;
    private CoderAI aiCode;
    private AIConfig config;
    private bool hasReceivedResponse = false;
    private Texture2D logo;

    // Create window
    [MenuItem("UniGen AI/Text AI")]
    public static void ShowWindow()
    {
        var window = GetWindow<AITool>("Text Tool");
        window.logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Logo.png");

        window.Reset();
        maxTokens = 100;
        temperature = 0.2f;
    }
     [MenuItem("UniGen AI/Code AI")]
    public static void ShowCodeWindow()
    {     
       var window = GetWindow<AITool>("Code Tool");
        window.logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Logo.png");
        
        window.Reset();
        maxTokens = 100;
        temperature = 0.2f;
    }
     
    // Reset prompt
    public void Reset()
    {
        prompt = "";
    }

    // Load AIConfig asset to get key
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
            aiCode = new CoderAI(config.apiKey);
        }
    }

    public void OnGUI()
    {
        if (logo != null && !hasReceivedResponse)
        {
             GUI.DrawTexture(new Rect(300, 300, 300, 220), logo);

            if (GUI.Button(new Rect(300, 250, 300, 150), "Click me", EditorStyles.boldLabel))
            {
                Application.OpenURL("shorturl.at/mwILZ");      

            }   
        }


        GUILayout.Label("AI Tool", EditorStyles.boldLabel);

        // Prompt input
        prompt = EditorGUILayout.TextField("Prompt:", prompt);

        // Max tokens input
        maxTokens = EditorGUILayout.IntField("Max Tokens:", maxTokens);

        // Temperature slider
        temperature = EditorGUILayout.Slider("Temperature:", temperature, 0f, 1f);

        // Send Request button
        if (GUILayout.Button("Send Request") && !isProcessing && !string.IsNullOrEmpty(prompt))
        {
            SendRequest();
        }

        // Output panel for response
       else if (isProcessing && !string.IsNullOrEmpty(response))
        {
            DrawOutputPanel();       
        }
        if (GUILayout.Button("Save Script"))
        {
           SaveScript();
         
        }
        

        else if (string.IsNullOrEmpty(prompt))
        {
            GUILayout.Label("Please enter a prompt.", EditorStyles.helpBox);
        }
    }

   // Send AI request
private void SendRequest()
{  
   if (EditorWindow.focusedWindow.titleContent.text == "Text Tool" && !hasReceivedResponse)
    {
            Debug.Log("Sending chat request...");
            ai.SendChatRequest(prompt, maxTokens, temperature, OnResponseReceived);
            isProcessing = true;
    }
    else if (EditorWindow.focusedWindow.titleContent.text == "Code Tool" && !hasReceivedResponse)
    {  
            Debug.Log("Sending code request...");
            aiCode.SendCodeRequest(prompt, maxTokens, temperature, OnResponseReceived);
            isProcessing = true;     
    }
}
private void SendFollowUp(){
     if (EditorWindow.focusedWindow.titleContent.text == "Text Tool" && hasReceivedResponse)
     {

            Debug.Log("Sending follow-up request...");
            ai.SendChatRequest(prompt, maxTokens, temperature, OnResponseReceived);
            isProcessing = true;
            hasReceivedResponse = false;      
    }
    else if (EditorWindow.focusedWindow.titleContent.text == "Code Tool" && hasReceivedResponse)
        {
            Debug.Log("Sending follow-up request...");
            aiCode.SendCodeRequest(prompt, maxTokens, temperature, OnResponseReceived);
            isProcessing = true;
            hasReceivedResponse = false;
        }
    
}
    
    // Show response in output panel
 private void OnResponseReceived(string resp)
    {    
    if (EditorWindow.focusedWindow.titleContent.text == "Text Tool")
    {  
        var textTool = EditorWindow.GetWindow<TextTool>();
        if (textTool != null)
        {
            textTool.ShowResponse(resp);
        }
    }
    else if (EditorWindow.focusedWindow.titleContent.text == "Code Tool")
    {
        var codeTool = EditorWindow.GetWindow<CodeTool>();
        if (codeTool != null)
        {
            codeTool.ShowResponse(resp);
        }
    }
          
        // Save response to file
        SaveResponse();
        ShowResponseGeneratedMessageBox();    

        Repaint();        
 }

    // Draw output panel for response
    private void DrawOutputPanel()
    {
        GUILayout.BeginArea(new Rect(10, 110, position.width - 20, 50000));
        GUILayout.Label("Response:", EditorStyles.boldLabel);
        responseScroll = GUILayout.BeginScrollView(responseScroll);
        GUILayout.Label(response, EditorStyles.textArea);
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Repaint the window to update the scroll view
        Repaint();
        EditorGUILayout.Space(300);
    }

    // Save generated script
    private void SaveScript()
    {
        scriptName = EditorUtility.SaveFilePanel
        ("Save C# Script", "", "MyScript", "cs");
            if (!string.IsNullOrEmpty(scriptName))
             {
                System.IO.File.WriteAllText(scriptName,GenerateScript());
                AssetDatabase.Refresh();
                }
}

    // "Adding Generated response to C# script" 
private string GenerateScript()
{
    string script = $@"using UnityEngine;
            public class MyScript : MonoBehaviour
        {{

        {response}
      
                }}";
                return script;
}


    // "Save AI response" to file
private void SaveResponse()
{
    string filePath = Application.dataPath + "/response.txt";
    if (File.Exists(filePath))
    {
        File.WriteAllText(filePath, response);
    }
    else
    {
        Debug.LogError("File does not exist: " + filePath);
    }
    AssetDatabase.Refresh();
}

    // Display message box to notify user that "response has been generated"
private void ShowResponseGeneratedMessageBox()
{
    EditorUtility.DisplayDialog("Response Received", "The response has been generated.", "OK");
}

}