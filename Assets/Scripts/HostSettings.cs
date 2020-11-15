using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HostSettings", menuName = "HackApi/HostSettings", order = 1)]
public class HostSettings : ScriptableObject
{
    public string Host;
    public string BankKey;
    public int RequestPeriod = 1;
    public string GameToken;
}