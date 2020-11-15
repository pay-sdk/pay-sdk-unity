using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RequestToRaif : MonoBehaviour
{
    public HostSettings HostSettings;

    public QRCodeResponse qr;
    public PaymentResponse PaymentResponse;

    public RawImage qrCodeRawImage;

    private bool paymentSuccess = false;
    public Action OnPaymentSuccess;

    private void OnEnable()
    {
        OnPaymentSuccess += LogToOurServer;
    }

    private void OnDisable()
    {
        OnPaymentSuccess -= LogToOurServer;
    }

    public void LogToOurServer()
    {
        StartCoroutine(SendDataToServer.SendPaymentInfo(PaymentResponse));

    }

    public IEnumerator RegisterQRCode(int amount)
    {
        qrCodeRawImage.texture = null;
        var rand = Random.Range(50, 100000);
        var postForm = new RegisterPostData()
        {
            additionalInfo = "additionalInfo",
            amount = amount,
            currency = "RUB",
            createDate = "2019-07-22T09:14:38.107227+03:00",
            order = "1-42-333" + rand,
            qrType = "QRStatic",
            sbpMerchantId = "MA565775"
        };

        byte[] byteArray = Encoding.UTF8.GetBytes(JsonUtility.ToJson(postForm));

        using (UnityWebRequest request = new UnityWebRequest("https://test.ecom.raiffeisen.ru/api/sbp/v1/qr/register"))
        {
            qr = new QRCodeResponse();
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
                qr = JsonUtility.FromJson<QRCodeResponse>(responseText);
                if (qr.qrId != null)
                {
                    if (Application.platform == RuntimePlatform.Android ||
                        Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        Application.OpenURL(qr.payload);
                    }
                    else
                    {
                        yield return DownloadQR();
                    }
                }
                else
                {
                    StartCoroutine(RegisterQRCode(amount));
                }
            }
        }
    }

    public IEnumerator GetQR(string qrId)
    {
        using (UnityWebRequest request =
            new UnityWebRequest($"https://test.ecom.raiffeisen.ru/api/sbp/v1/qr/{qrId}/info"))
        {
            qr = new QRCodeResponse();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + HostSettings.BankKey);
            request.method = "GET";
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Unable to send the POST: " + request.error);
            }
            else
            {
                Debug.Log("Post request complete!" + " Response Code: " + request.responseCode);
                string responseText = request.downloadHandler.text;
                Debug.Log("Response Text:" + responseText);
                qr = JsonUtility.FromJson<QRCodeResponse>(responseText);
                if (qr.qrId != null)
                {
                    yield return DownloadQR();
                }
            }
        }
    }

    IEnumerator CheckPaymentUpdate(string qrId)
    {
        while (!paymentSuccess)
        {
            StartCoroutine(CheckPayment(qrId));
            
            yield return new WaitForSeconds(HostSettings.RequestPeriod);
        }
        paymentSuccess = false;
    }

    private IEnumerator CheckPayment(string qrId)
    {
        using (UnityWebRequest request =
            new UnityWebRequest($"https://test.ecom.raiffeisen.ru/api/sbp/v1/qr/{qrId}/payment-info"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + HostSettings.BankKey);
            request.method = "GET";
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Unable to send the POST: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text;

                PaymentResponse = JsonUtility.FromJson<PaymentResponse>(responseText);
                if (PaymentResponse.paymentStatus == "SUCCESS")
                {
                    Debug.Log("---------------Payment successful------------");
                    Debug.Log("Response Text:" + responseText);
                    paymentSuccess = true;
                    OnPaymentSuccess?.Invoke();
                }
                else
                {
                    paymentSuccess = false;
                    Debug.Log(PaymentResponse.paymentStatus);
                }
            }
        }
    }

    IEnumerator DownloadQR()
    {
        StopAllCoroutines();
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(qr.qrUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                qrCodeRawImage.texture = DownloadHandlerTexture.GetContent(uwr);
                StartCoroutine(CheckPaymentUpdate(qr.qrId));
            }
        }
    }
}

[Serializable]
public class RegisterPostData
{
    public string additionalInfo;
    public int amount;
    public string currency;
    public string createDate;
    public string order;
    public string qrType;
    public string sbpMerchantId;
}

[Serializable]
public class QRCodeResponse
{
    public string code;
    public string qrId = null;
    public string payload;
    public string qrUrl;
}

[Serializable]
public class PaymentResponse
{
    public string additionalInfo;
    public string amount = null;
    public string code;
    public string createDate;
    public string currency;
    public string merchantId;
    public string order;
    public string paymentStatus;
    public string qrId;
    public string sbpMerchantId;
    public string transactionDate;
    public string transactionId;
}