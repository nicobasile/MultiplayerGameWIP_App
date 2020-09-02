using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";

    #pragma warning disable 0649
    [SerializeField] private GameObject SignUpPanel, LobbyPanel, DirectConnectPanel;
    [SerializeField] private GameObject CharacterSelectPanel, ProfilePanel, ShopPanel;
    [SerializeField] private GameObject FriendsPanel, GameModePanel, SettingsPanel;
    [SerializeField] private GameObject LoadingPanel, WaitingPanel;

    [Header("Other")]
    [SerializeField] private GameObject CreateUserNameButton;
    [SerializeField] private InputField UserNameInput, JoinOrCreateRoomInput;
    [SerializeField] private Text lobbyLeaderStatus, profileText, gameModeText;
    [SerializeField] private Text playersFoundText;
    #pragma warning restore 0649

    [HideInInspector] private bool Waiting = false;

    private void Awake()
    {
        DisableAllPanels();
        LoadingPanel.SetActive(true);
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Connecting to Photon...");
    }

    private void Update()
    {
        if (Waiting)
        {
            if (gameModeText.text == "Casual 1v1")
            {
                playersFoundText.text = PhotonNetwork.PlayerList.Length + "/2 Players";
                if (PhotonNetwork.PlayerList.Length == 2 && PhotonNetwork.IsMasterClient)
                {
                    Waiting = false;
                    WaitingPanel.SetActive(false);
                    LoadArena();
                }
            }
            else if (gameModeText.text == "Casual BR")
            {
                playersFoundText.text = PhotonNetwork.PlayerList.Length + "/4 Players";
                if (PhotonNetwork.PlayerList.Length == 4 && PhotonNetwork.IsMasterClient)
                {
                    Waiting = false;
                    WaitingPanel.SetActive(false);
                    LoadArena();
                }            
            }
            else if (gameModeText.text == "Training")
            {
                playersFoundText.text = PhotonNetwork.PlayerList.Length + "/0 Players";
                if (PhotonNetwork.PlayerList.Length == 1 && PhotonNetwork.IsMasterClient)
                {
                    Waiting = false;
                    WaitingPanel.SetActive(false);
                    LoadArena();
                }            
            }
        }
    }

    #region Photon Calls

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("OnConnectedToMaster()");
        base.OnConnectedToMaster();   
    }

    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(false);
        SignUpPanel.SetActive(true);
        Debug.Log("OnJoinedLobby()");
        base.OnJoinedLobby();   
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random rooms exist, creating room now...");
        if (gameModeText.text == "Casual 1v1")
            CreateCustomRoom(2);
        else if (gameModeText.text == "Casual BR")
            CreateCustomRoom(4);
        base.OnJoinRandomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        Waiting = true;      
        Debug.Log("Joined Room");
        base.OnJoinedRoom();   
    }

    public void LoadArena()
    {
        PhotonNetwork.LoadLevel("FirstLevel");
        Debug.Log("Joined FirstLevel");
    }

    public void OnDisconnectedFromPhoton() { Debug.Log("Lost Connection to Photon"); }

    #endregion

    #region UI

    public void DisableAllPanels()
    {
        LoadingPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        DirectConnectPanel.SetActive(false);
        CharacterSelectPanel.SetActive(false);
        ProfilePanel.SetActive(false);
        ShopPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        GameModePanel.SetActive(false);
        SettingsPanel.SetActive(false);
        WaitingPanel.SetActive(false);
    }

    public void OnChange_UserNameInput()
    {
        if (UserNameInput.text.Length >= 2)
            CreateUserNameButton.SetActive(true);
        else
            CreateUserNameButton.SetActive(false);
    }

    public void OnClick_CreateUserName()
    {
        PhotonNetwork.LocalPlayer.NickName = UserNameInput.text;

        SignUpPanel.SetActive(false);
        LobbyPanel.SetActive(true);

        profileText.text = PhotonNetwork.LocalPlayer.NickName;
        Debug.Log("Player name is: " + PhotonNetwork.LocalPlayer.NickName);
    }

    public void OnClick_ToLobby()
    {
        DisableAllPanels();
        LobbyPanel.SetActive(true);
    }

    public void OnClick_ToGameModeSelect()
    {
        LobbyPanel.SetActive(false);
        GameModePanel.SetActive(true);
    }

    public void OnClick_SelectCasual1v1() { gameModeText.text = "Casual 1v1"; OnClick_ToLobby(); }
    public void OnClick_SelectTraining() { gameModeText.text = "Training"; OnClick_ToLobby(); }
    public void OnClick_SelectCasualBR() { gameModeText.text = "Casual BR"; OnClick_ToLobby(); }

    public void OnClick_ReadyUp()
    {
        LobbyPanel.SetActive(false);
        WaitingPanel.SetActive(true);

        if (gameModeText.text == "Casual 1v1")
        {
            playersFoundText.text = "0/2 Players";
            PhotonNetwork.JoinRandomRoom(null, 2, MatchmakingMode.FillRoom, null, null);
        }
        else if (gameModeText.text == "Casual BR")
        {
            playersFoundText.text = "0/4 Players";
            PhotonNetwork.JoinRandomRoom(null, 4, MatchmakingMode.FillRoom, null, null);
        }
        else if (gameModeText.text == "Training")
        {
            playersFoundText.text = "0/0 Players";
            CreateCustomRoom(8);
        }
    }

    private void CreateCustomRoom(int maxPlayers)
    {
        Debug.Log("CreateCustomRoom(" + maxPlayers + ")");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Convert.ToByte(maxPlayers);
        PhotonNetwork.CreateRoom(null, roomOptions, null, null);
    }

    #endregion
}
