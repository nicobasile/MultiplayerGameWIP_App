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
    [SerializeField] private GameObject LoadingPanel;

    [Header("Other")]
    [SerializeField] private GameObject CreateUserNameButton, StartMatchButton;
    [SerializeField] private InputField UserNameInput, JoinOrCreateRoomInput;
    [SerializeField] private Text lobbyLeaderStatus, profileText, gameModeText;
    #pragma warning restore 0649
    
    private void Awake()
    {
        DisableAllPanels();
        LoadingPanel.SetActive(true);
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon...");
    }

    private void Start()
    {
        StartMatchButton.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("Connected to Master");
    }

    public override void OnJoinedLobby()
    {
        LobbyPanel.SetActive(false);
        SignUpPanel.SetActive(true);
        Debug.Log("Joined Lobby");
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Lost Connection to Photon");
    }

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

    public void OnClick_ReadyUp()
    {
        LobbyPanel.SetActive(false);
        DirectConnectPanel.SetActive(true);

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

    public void OnClick_SelectRanked() { gameModeText.text = "Ranked"; OnClick_ToLobby(); }
    public void OnClick_SelectCasual() { gameModeText.text = "Casual"; OnClick_ToLobby(); }
    public void OnClick_SelectTraining() { gameModeText.text = "Training"; OnClick_ToLobby(); }

    public void OnClick_OnJoinRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(JoinOrCreateRoomInput.text, roomOptions, TypedLobby.Default);
        Debug.Log("Joining Room: " + JoinOrCreateRoomInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("FirstLevel");
        if (PhotonNetwork.IsMasterClient)
        {
            //StartMatchButton.SetActive(true);
            //lobbyLeaderStatus.text = "You are Lobby Leader";
        }
        else
        {
            //StartMatchButton.SetActive(true);
            //lobbyLeaderStatus.text = "Wait for Lobby Leader to Start The Game";
        }
    }

    public void LoadArena()
    {
        PhotonNetwork.LoadLevel("FirstLevel");
        Debug.Log("Joined FirstLevel");
    }

    #endregion
}
