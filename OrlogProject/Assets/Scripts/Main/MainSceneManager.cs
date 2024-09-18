using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviourPunCallbacks
{
    public GameStartPanel gameStartPanel;
    public GameObject loadingPanel;

    private void Awake()
    {
        loadingPanel.SetActive(true);
    }

    public void HostGame()
    {
        RoomOptions _roomOptions = new RoomOptions();
        _roomOptions.IsVisible = true;
        _roomOptions.IsOpen = true;
        _roomOptions.MaxPlayers = 2;
        
        PhotonNetwork.CreateRoom($"Room_{Random.Range(100000, 999999)}", _roomOptions);
        loadingPanel.SetActive(true);
    }

    public void JoinGame()
    {

    }

    public override void OnConnectedToMaster()
    {
        loadingPanel.SetActive(false);

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.LogError("Joined Lobby");

        gameStartPanel.OnNetworkConnectHandler();
    }

    public override void OnJoinedRoom()
    {
        Debug.LogError("OnJoinedRoom");

        PhotonNetwork.LoadLevel(1);
    }
}
