using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buy : MonoBehaviour
{
    public GameObject paymentCanvas;
    public Text amountText;
    public GameObject successCanvas;
    public int amount = 123;
    public RequestToRaif Requestor;

    private void OnEnable()
    {
        Requestor.OnPaymentSuccess += OnPaymentSuccess;
    }
    
    private void OnDisable()
    {
        Requestor.OnPaymentSuccess -= OnPaymentSuccess;
    }
    

    public void OnBtnBuyClick()
    {
        StartCoroutine(Requestor.RegisterQRCode(amount));
        if (Application.platform != RuntimePlatform.Android &&
            Application.platform != RuntimePlatform.IPhonePlayer)
        {
            amountText.text = $"Оплатить {amount} Р";
            paymentCanvas.SetActive(true);
        }
    }

    private void OnPaymentSuccess()
    {
        paymentCanvas.SetActive(false);
        StartCoroutine(ShowSuccessCanvas());
    }

    private IEnumerator ShowSuccessCanvas()
    {
        successCanvas.SetActive(true);
        yield return new WaitForSeconds(2);
        successCanvas.SetActive(false);
    }
}