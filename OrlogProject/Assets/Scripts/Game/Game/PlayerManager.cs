using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    static public PlayerManager instance;

    public int faction;
    [SerializeField]
    private Camera cam0, cam1;
    [ReadOnlyInspector]
    public Camera playerCam;

    [ReadOnlyInspector]
    public PlayerSet myDice;

    private void Awake()
    {
        instance = this;
    }

    void SetPlayerProperties(string _key, object _value)
    {
        Hashtable _properties = new Hashtable();
        _properties[_key] = _value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_properties);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (PhotonNetwork.IsMasterClient)
        {
            playerCam = cam0;
            cam1.gameObject.SetActive(false);
            cam0.gameObject.SetActive(true);

            SetPlayerProperties("Faction", 0);
            faction = 0;

            myDice = DiceController.instance.playerSetA;
        }
        else
        {
            playerCam = cam1;
            cam0.gameObject.SetActive(false);
            cam1.gameObject.SetActive(true);

            SetPlayerProperties("Faction", 1);
            faction = 1;

            myDice = DiceController.instance.playerSetB;
        }
    }
}
