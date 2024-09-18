using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GodCardComponent
{
    public int index;
    public string name;
}

public class GodManager : MonoBehaviour
{
    public List<GodCardComponent> cardList;
}
