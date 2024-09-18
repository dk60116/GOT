using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartPanel : BasicPanel
{
    [SerializeField]
    private Button hostGameBtn, joinGameBtn;

    void Awake()
    {
        hostGameBtn.interactable = false;
        joinGameBtn.interactable = false;
    }

    public void OnNetworkConnectHandler()
    {
        hostGameBtn.interactable = true;
        joinGameBtn.interactable = true;
    }
}
