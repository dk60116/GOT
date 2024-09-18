using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManger : MonoBehaviourPunCallbacks
{
    public int playerFactioin;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnected()
    {
        base.OnConnected();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.LogError("OnMaster");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        RoomOptions _newRoomOption = new RoomOptions();
        _newRoomOption.MaxPlayers = 2;

        PhotonNetwork.JoinOrCreateRoom("ABC", _newRoomOption, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.LogError("Joined");
    }
}
