using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("General")]
    public GameObject MainCamera;
    public Text TimerText;
    public Text EliminationsText;
    public Text PingText;

    [Header("Respawn")]
    public GameObject[] spawnPoints;
    public Text RespawnText;
    public GameObject RespawnUI;
    private float TimerAmount = 3;
    private bool RespawnTimerActive = false;

    [Header("Game Over")]
    public GameObject GameOverPanel;
    public GameObject WinnerCharacterSprite;
    public Text WinnerEliminationsText;
    public Text ResultText;

    [HideInInspector] public GameObject localPlayer; // Set from playerhealth class
    [HideInInspector] private float MatchLength;
    [HideInInspector] private float Timer;
    [HideInInspector] private Boolean endedGame = false;
    [HideInInspector] public string CharacterSelected;

    private void Awake()
    {
        Instance = this;
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        CharacterSelected = PlayerPrefs.GetString("CharacterSelected");
        MatchLength = Int32.Parse(PlayerPrefs.GetString("MatchLength"));

        PhotonNetwork.SendRate = 40; // Default is 20
        PhotonNetwork.SerializationRate = 30; // Default is 10
    }
    
    private void Start()
    {
        SpawnPlayer();
        Timer = 0;
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        int TimeLeft = (int) (MatchLength - Timer);
        //TimerText.text = Convert.ToString(MatchLength - Timer);
        int minutes = Mathf.FloorToInt(TimeLeft / 60F);
        int seconds = Mathf.FloorToInt(TimeLeft % 60F);
        TimerText.text = minutes.ToString ("0") + ":" + seconds.ToString ("00");

        if (TimeLeft <= 0 && !endedGame) 
        {
            EndGame();
            endedGame = true;
        }

        if (TimeLeft <= -10)
            OnClick_LoadLobby();

        EliminationsText.text = localPlayer.GetComponent<PlayerHealth>().Eliminations.ToString();

        PingText.text = "Ping: " + PhotonNetwork.GetPing();

        if (RespawnTimerActive) StartRespawn();
    }

    public void EndGame()
    {
        GameOverPanel.SetActive(true);
        ResultText.text = "Winner: " + GetWinner();
    }

    public string GetWinner()
    {
        int maxEliminations = 0;
        string maxName = "";

        var photonViews = UnityEngine.Object.FindObjectsOfType<PhotonView>();
        foreach (var player in photonViews)
        {
            int playerEliminations = player.GetComponent<PlayerHealth>().Eliminations;
            if (playerEliminations > maxEliminations)
            {
                maxName = player.Owner.NickName;
                maxEliminations = playerEliminations;
                WinnerCharacterSprite.GetComponent<Image>().sprite = player.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
            }
        }

        WinnerEliminationsText.text = maxEliminations.ToString();
        return maxName;
    }

    public void EnableRespawn()
    {
        TimerAmount = 5f;
        RespawnTimerActive = true;
        RespawnUI.SetActive(true);
    }

    private void StartRespawn()
    {
        TimerAmount -= Time.deltaTime;
        RespawnText.text = "Respawning In: " + TimerAmount.ToString("F0");

        if (TimerAmount <= 0)
        {
            RespawnLocation();
            localPlayer.GetComponent<PhotonView>().RPC("Revive", RpcTarget.AllBuffered);
            localPlayer.GetComponent<PlayerHealth>().EnableInput();
            RespawnUI.SetActive(false);
            RespawnTimerActive = false;
        }
    }

    public void RespawnLocation()
    {
        var index = UnityEngine.Random.Range(0, spawnPoints.Length);
        localPlayer.transform.localPosition = new Vector2(spawnPoints[index].transform.position.x, spawnPoints[index].transform.position.y);
    }

    public void SpawnPlayer()
    {
        MainCamera.SetActive(false);
        var index = UnityEngine.Random.Range(0, spawnPoints.Length);

        PhotonNetwork.Instantiate(CharacterSelected, spawnPoints[index].transform.position, Quaternion.identity, 0);
    }

    public void OnClick_LoadLobby()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("MainMenu");
    }
}
