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
    public GameObject Player_PREFAB;
    public GameObject MainCanvas;
    public GameObject MainCamera;
    public Text TimerText;
    public Text PingText;

    [Space]

    [Header("Respawn")]
    public GameObject[] spawnPoints;
    public Text RespawnText;
    public GameObject RespawnUI;
    private float TimerAmount = 3;
    private bool RespawnTimerActive = false;

    [HideInInspector] public GameObject localPlayer; // Set from playerhealth class
    [HideInInspector] private float maxPing;
    [HideInInspector] private float Timer;


    private void Awake()
    {
        Instance = this;
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

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
        int minutes = Mathf.FloorToInt(Timer / 60F);
        int seconds = Mathf.FloorToInt(Timer % 60F);
        TimerText.text = minutes.ToString ("00") + ":" + seconds.ToString ("00");
        
        if (PhotonNetwork.GetPing() > maxPing) maxPing = PhotonNetwork.GetPing();
        PingText.text = "Ping: " + PhotonNetwork.GetPing() + " (" + maxPing + ")";

        if (RespawnTimerActive)
        {
            StartRespawn();
        }
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
        RespawnText.text = "Respawn In: " + TimerAmount.ToString("F0");

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
        var index = Random.Range(0, spawnPoints.Length);
        localPlayer.transform.localPosition = new Vector2(spawnPoints[index].transform.position.x, spawnPoints[index].transform.position.y);
    }

    public void SpawnPlayer()
    {
        MainCamera.SetActive(false);
        var index = Random.Range(0, spawnPoints.Length);
        if (Player_PREFAB == null) Debug.Log("PlayerPrefab???");
        if (spawnPoints[index] == null) Debug.Log("index");

        PhotonNetwork.Instantiate(Player_PREFAB.name, spawnPoints[index].transform.position, Quaternion.identity, 0);
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
