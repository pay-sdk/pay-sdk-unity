using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendData : MonoBehaviour
{
    public RequestToRaif requester;
    public int amount = 0;
    
    public void OnAmountFieldChanged(string a)
    {
        amount = Int32.Parse(a);
    }
    
    public void OnRegisterCodeButtonClick()
    {
        StartCoroutine(requester.RegisterQRCode(amount));
    }
}
