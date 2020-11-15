using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SendDataToServer : MonoBehaviour
{
    public HostSettings HostSettings;
    private static string token;
    private static string host;
    public void Start()
    {
        token = HostSettings.GameToken;
        host = HostSettings.Host;
    }

    public static IEnumerator SendPaymentInfo(PaymentResponse info)
    {
        var postForm = new PayData()
        {
            productName = "monetki",
            token = token,
            username = "Wolfman",
            description = info.additionalInfo,
            moneyAmount = info.amount
        };

        byte[] byteArray = Encoding.UTF8.GetBytes(JsonUtility.ToJson(postForm));

        using (UnityWebRequest request = new UnityWebRequest(Path.Combine(host, "payment")))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = "POST";
            request.uploadHandler = new UploadHandlerRaw(byteArray);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.downloadHandler.text);
                Debug.LogError("Unable to send the POST: " + request.error);
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + request.responseCode);
                string responseText = request.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);
                
            }
        }
    }
}

[Serializable]
public class PayData
{
    public string productId = null;
    public string productName = null;
    public string token;
    public string username;
    public string description;
    public string moneyAmount;
}
