using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;  
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class MenuManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.1";

    #pragma warning disable 0649
    [SerializeField] private GameObject LobbyPanel;
    [SerializeField] private GameObject CharacterSelectPanel, ProfilePanel, ShopPanel;
    [SerializeField] private GameObject FriendsPanel, GameModePanel, SettingsPanel;
    [SerializeField] private GameObject LoadingPanel, WaitingPanel;
    [SerializeField] private GameObject AccountPanel, ProcessPanel, UsernamePanel;

    [Header("Other")]
    [SerializeField] private GameObject CreateAccountButton, LogInButton, SignInButton, SignUpButton;
    [SerializeField] private InputField UserNameInput, EmailInput, PasswordInput;
    [SerializeField] private Text profileText, gameModeText, gameModeTypeText;
    [SerializeField] private Text coinsText, bitsText;
    [SerializeField] private Text playersFoundText;
    #pragma warning restore 0649

    [HideInInspector] private bool Waiting = false;
    [HideInInspector] private bool HasSaveData = false;
    [HideInInspector] private static String key = "b14ca5898a4e4133bbce2ea2315a1916";  
    [HideInInspector] private FirebaseAuth auth;
    [HideInInspector] private FirebaseFirestore db;
    [HideInInspector] private CollectionReference usersRef;
    [HideInInspector] private CollectionReference friendsRef;
    [HideInInspector] private DocumentReference docRef;

    private void Awake()
    {
        DisableAllPanels();
        LoadingPanel.SetActive(true);

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Connecting to Photon...");
    }

    private void Update()
    {
        if (Waiting)
        {
            if (gameModeTypeText.text == "Casual")
            {
                if (gameModeText.text == "Duel")
                    WaitingRoom(2);
                else if (gameModeText.text == "TDM")
                {
                    playersFoundText.text = PhotonNetwork.PlayerList.Length + "/4 Players";
                    if (PhotonNetwork.PlayerList.Length == 4 && PhotonNetwork.IsMasterClient)
                    {
                        Waiting = false;
                        WaitingPanel.SetActive(false);
                        LoadArena("SecondLevel");
                    }            
                }
                else if (gameModeText.text == "FFA")
                {
                    playersFoundText.text = PhotonNetwork.PlayerList.Length + "/8 Players";
                    if (PhotonNetwork.PlayerList.Length == 8 && PhotonNetwork.IsMasterClient)
                    {
                        Waiting = false;
                        WaitingPanel.SetActive(false);
                        LoadArena("SecondLevel");
                    }            
                }
            }
            else if (gameModeText.text == "Training")
            {
                playersFoundText.text = PhotonNetwork.PlayerList.Length + "/0 Players";
                if (PhotonNetwork.PlayerList.Length == 1 && PhotonNetwork.IsMasterClient)
                {
                    Waiting = false;
                    WaitingPanel.SetActive(false);
                    LoadArena("TrainingLevel");
                }            
            }
        }
    }

    #region Photon Calls

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("ConnectedToMaster()");
        base.OnConnectedToMaster();
    }

    public override void OnJoinedLobby()
    {
        if (LoadLogin() == false)
        {
            Debug.Log("No previous save data found");
            LobbyPanel.SetActive(false);
            AccountPanel.SetActive(true);
            Debug.Log("JoinedLobby()");
            base.OnJoinedLobby();   
        }
        else HasSaveData = true;
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

    public void LoadArena(string LevelName)
    {
        PhotonNetwork.LoadLevel(LevelName);
        Debug.Log("Joined " + LevelName);
    }

    public void OnDisconnectedFromPhoton() 
    { 
        Debug.Log("Lost Connection to Photon"); 
    }

    #endregion

    #region UI

    public void DisableAllPanels()
    {
        LoadingPanel.SetActive(false);

        AccountPanel.SetActive(false);
        ProcessPanel.SetActive(false);

        LobbyPanel.SetActive(false);
        CharacterSelectPanel.SetActive(false);
        ProfilePanel.SetActive(false);
        ShopPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        GameModePanel.SetActive(false);
        SettingsPanel.SetActive(false);
        WaitingPanel.SetActive(false);
    }

    public void OnClick_LogIn()
    {
        ProcessPanel.SetActive(true);
        UsernamePanel.SetActive(false);
        SignInButton.SetActive(true);
    }

    public void OnClick_CreateAccount()
    {
        ProcessPanel.SetActive(true);
        UsernamePanel.SetActive(true);
        SignUpButton.SetActive(true);
    }

    /*public void OnChange_UserNameInput()
    {
        if (UserNameInput.text.Length >= 2)
            SignInButton.SetActive(true);
        else
            SignInButton.SetActive(false);
    }*/

    public void OnClick_SignUp()
    { 
        auth.CreateUserWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.Log("Firebase user created successfully: " + newUser.DisplayName + " | " + newUser.UserId);
            
            SetupProfileData();
        });

        SaveLogin();
    }

    public void OnClick_SignIn()
    {
        auth.SignInWithEmailAndPasswordAsync(EmailInput.text, PasswordInput.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                EmailInput.text, newUser.UserId);
        });

        usersRef = db.Collection("users");
        docRef = usersRef.Document(EmailInput.text);

        SaveLogin();
        LoadLobby();
    }

    public void OnClick_SignOut()
    {
        Debug.Log("SignOut()");
        auth.SignOut();

        File.Delete(Application.persistentDataPath + "/save.txt");

        PhotonNetwork.Disconnect();

        UserNameInput.text = "";
        EmailInput.text = "";
        PasswordInput.text = "";

        Awake();
    }
   
    public void SetupProfileData()
    {
        usersRef = db.Collection("users");
        docRef = usersRef.Document(EmailInput.text);
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "Username", UserNameInput.text },
            { "Email", EmailInput.text },
            { "Password", PasswordInput.text },
            { "UserId", auth.CurrentUser.UserId },
            { "Coins", 0},
            { "Bits", 0}
        };
        docRef.SetAsync(user).ContinueWithOnMainThread(task => {
            Debug.Log("Added data to the " + EmailInput.text + " document in the users collection.");
            
            PhotonNetwork.LocalPlayer.NickName = profileText.text;
            Debug.Log("Player name is: " + PhotonNetwork.LocalPlayer.NickName);

            LoadLobby();
        });
    }

    public void LoadLobby()
    {
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
        DocumentSnapshot snapshot = task.Result;
        if (snapshot.Exists) 
        {
            Dictionary<string, object> userData = snapshot.ToDictionary();
            profileText.text = (String) userData["Username"];
            PhotonNetwork.LocalPlayer.NickName = profileText.text;
            coinsText.text = "Coins: " + userData["Coins"];
            bitsText.text = "Bits: " + userData["Bits"];
            OnClick_ToLobby();
        } 
        else Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
        });
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

    public void OnClick_ToSettings()
    {
        LobbyPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void OnClick_SelectTraining() 
    { 
        gameModeText.text = "Training"; gameModeTypeText.text = "Practice";
        OnClick_ToLobby(); 
    }
    public void OnClick_SelectCasualDuel() 
    { 
        gameModeText.text = "Duel"; gameModeTypeText.text = "Casual";
        OnClick_ToLobby(); 
    } 
    public void OnClick_SelectCasualTDM() 
    { 
        gameModeText.text = "TDM"; gameModeTypeText.text = "Casual";
        OnClick_ToLobby(); 
    }
    public void OnClick_SelectCasualFFA() 
    { 
        gameModeText.text = "FFA"; gameModeTypeText.text = "Casual";
        OnClick_ToLobby(); 
    } 

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

    public void OnClick_LeaveRoom()
    {
        Debug.Log("LeaveRoom()");
        Waiting = false;
        PhotonNetwork.LeaveRoom();
        OnClick_ToLobby();
    }

    private void WaitingRoom(int maxPlayers)
    {
        playersFoundText.text = PhotonNetwork.PlayerList.Length + "/" + maxPlayers + " Players";
        if (PhotonNetwork.PlayerList.Length == maxPlayers && PhotonNetwork.IsMasterClient)
        {
            Waiting = false;
            WaitingPanel.SetActive(false);
            LoadArena("FirstLevel");
        }
    }

    private void CreateCustomRoom(int maxPlayers)
    {
        Debug.Log("CreateCustomRoom(" + maxPlayers + ")");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Convert.ToByte(maxPlayers);
        PhotonNetwork.CreateRoom(null, roomOptions, null, null);
    }

    private void SaveLogin()
    {
        if (!HasSaveData) 
        {
            Debug.Log("SaveLogin()");
            StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/save.txt", false);
            writer.WriteLine(EncryptString(key, EmailInput.text));
            writer.WriteLine(EncryptString(key, PasswordInput.text));
            writer.Close();
            HasSaveData = true;
        }
    }

    private bool LoadLogin()
    {     
        Debug.Log("LoadLogin()");
        
        try 
        {
            StreamReader reader = new StreamReader(Application.persistentDataPath + "/save.txt");
            EmailInput.text = DecryptString(key, reader.ReadLine());
            PasswordInput.text = DecryptString(key, reader.ReadLine());
            reader.Close();

            OnClick_SignIn();
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    private static string EncryptString(string key, string plainText)  
    {  
        byte[] iv = new byte[16];  
        byte[] array;  

        using (Aes aes = Aes.Create())  
        {  
            aes.Key = Encoding.UTF8.GetBytes(key);  
            aes.IV = iv;  

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);  

            using (MemoryStream memoryStream = new MemoryStream())  
            {  
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))  
                {  
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))  
                    {  
                        streamWriter.Write(plainText);  
                    }  

                    array = memoryStream.ToArray();  
                }  
            }  
        }  

        return Convert.ToBase64String(array);  
    }  

    private static string DecryptString(string key, string cipherText)  
    {  
        byte[] iv = new byte[16];  
        byte[] buffer = Convert.FromBase64String(cipherText);  

        using (Aes aes = Aes.Create())  
        {  
            aes.Key = Encoding.UTF8.GetBytes(key);  
            aes.IV = iv;  
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);  

            using (MemoryStream memoryStream = new MemoryStream(buffer))  
            {  
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))  
                {  
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))  
                    {  
                        return streamReader.ReadToEnd();  
                    }  
                }  
            }  
        }  
    } 

    #endregion
}
