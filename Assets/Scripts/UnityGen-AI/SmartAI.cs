//  private string apiKey = ""; // 
// public const string endpointUrl = "https://api.openai.com/v1/completions";//"https://api.openai.com/v1/chat/completions"

using System.Collections;
using UnityEngine.Networking;
using UnityEngine;


public class SmartAI {
    private const string API_URL = "https://api.openai.com/v1/completions";
    private float timeBetweenRequests = 2f; // Time in seconds between requests
    private float lastRequestTime = 0f; // Time of the last request

    private readonly string apiKey ;
    public bool isProcessing;

    public SmartAI(string apiKey) {
        this.apiKey = apiKey;
    }

   public void SendChatRequest(string prompt, int maxTokens, float temperature, System.Action<string> onResponse) 
    {
         if (Time.time < lastRequestTime + timeBetweenRequests)
        {
            // Don't send another request until enough time has passed
            return;
        }

        isProcessing = true;
        string url = API_URL;
        var request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        string requestBody = "{\"model\":\"text-davinci-002\", \"prompt\":\"" + prompt + "\", \"max_tokens\":" + maxTokens + ", \"temperature\":" + temperature + "}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SendWebRequest();

        lastRequestTime = Time.time; // Update the time of the last request
        
        while (!request.isDone)
        {
            // Do nothing until the request has completed
        }
        if (request.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Error sending chat request: " + request.error);
                    onResponse("response error" + request.error);
        } else {
        // Response Filteration 
        string responseText = request.downloadHandler.text; // Fetch the response
        string filteredResponse = responseText.Replace("\\n", "\n").Replace("\\", ""); // Replace unwanted characters
        int startIndex = filteredResponse.IndexOf("text\":\"") + "text\":\"".Length;  
        int endIndex = filteredResponse.IndexOf("\",", startIndex);
        int length = endIndex - startIndex;
        string response = filteredResponse.Substring(startIndex, length);
        string[] lines = response.Split('\n'); // Split response into separate lines
        string processedResponse = "";
        foreach (string line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                processedResponse += line.Trim() + "\n"; // Add each non-empty line to the processed response
            }
        }
        Debug.Log("Test: " + processedResponse  );
        onResponse(processedResponse);

        }
        isProcessing = false;
    }
}


