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
    [Header("Panels")]
    [SerializeField] private GameObject LobbyPanel;
    [SerializeField] private GameObject CharacterSelectPanel, ProfilePanel, ShopPanel;
    [SerializeField] private GameObject FriendsPanel, GameModePanel, SettingsPanel;
    [SerializeField] private GameObject LoadingPanel, WaitingPanel, NotImplementedPanel;
    [SerializeField] private GameObject AccountPanel, ProcessPanel, UsernamePanel;
    [SerializeField] private GameObject AboutPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject CreateAccountButton;
    [SerializeField] private GameObject LogInButton, SignInButton, SignUpButton;
    [SerializeField] private GameObject CharacterSpriteButton, GameModeButton;

    [Header("Input Fields")]
    [SerializeField] private InputField UserNameInput;
    [SerializeField] private InputField EmailInput, PasswordInput;
    
    [Header("Text Fields")]
    [SerializeField] private Text profileText;
    [SerializeField] private Text gameModeText, gameModeTypeText, coinsText, bitsText;
    [SerializeField] private Text playersFoundText, characterNameText;

    [Header("Other")]
    [SerializeField] private Sprite purpleGame;
    [SerializeField] private Sprite yellowGame, greenGame, blueGame;
    [SerializeField] private Sprite SluggerSprite, ShinobiSprite, ZealotSprite;
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
                if (gameModeText.text == "Solo")
                    WaitingRoom(2);
                else if (gameModeText.text == "Quad")
                    WaitingRoom(4);
                else if (gameModeText.text == "Training")
                    WaitingRoom(1);
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
        PlayerPrefs.SetString("CharacterSelected", "Slugger");
        if (LoadLogin() == false)
        {
            Debug.Log("No previous save data found");
            LobbyPanel.SetActive(false);
            AccountPanel.SetActive(true);
            Debug.Log("JoinedLobby()");
            base.OnJoinedLobby();   
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random rooms exist, creating room now...");
        if (gameModeText.text == "Solo")
            CreateCustomRoom(2);
        else if (gameModeText.text == "Quad")
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
        AboutPanel.SetActive(false);
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
        HasSaveData = false;

        PhotonNetwork.Disconnect();

        UserNameInput.text = "";
        EmailInput.text = "";
        PasswordInput.text = "";

        Awake();
    }
   
    public void OnClick_ToLobby()
    {
        DisableAllPanels();
        LobbyPanel.SetActive(true);
    }

    public void OnClick_ToCharacterSelect()
    {
        LobbyPanel.SetActive(false);
        CharacterSelectPanel.SetActive(true); 
    }

    public void OnClick_ToGameModeSelect()
    {
        LobbyPanel.SetActive(false);
        GameModePanel.SetActive(true);
    }

    public void OnClick_ToSettings()
    {
        SettingsPanel.SetActive(true);
    }

    public void OnClick_ToAbout()
    {
        SettingsPanel.SetActive(false);
        AboutPanel.SetActive(true);
    }

    public void OnClick_SelectTraining() 
    { 
        gameModeText.text = "Training";
        gameModeTypeText.text = "Practice";
        GameModeButton.GetComponent<Image>().sprite = yellowGame;
        OnClick_ToLobby(); 
    }
    public void OnClick_SelectCasualSolo() 
    { 
        gameModeText.text = "Solo";
        gameModeTypeText.text = "Casual";
        GameModeButton.GetComponent<Image>().sprite = blueGame;
        OnClick_ToLobby(); 
    } 
    public void OnClick_SelectCasualQuad() 
    { 
        gameModeText.text = "Quad"; 
        gameModeTypeText.text = "Casual";
        GameModeButton.GetComponent<Image>().sprite = greenGame;
        OnClick_ToLobby(); 
    }

    public void OnClick_SelectSlugger()
    {
        characterNameText.text = "Slugger";
        PlayerPrefs.SetString("CharacterSelected", "Slugger");
        CharacterSpriteButton.GetComponent<Image>().sprite = SluggerSprite;
        CharacterSpriteButton.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(100, 100);
        OnClick_ToLobby(); 
    }
    public void OnClick_SelectShinobi()
    {
        characterNameText.text = "Shinobi";
                PlayerPrefs.SetString("CharacterSelected", "Shinobi");
        CharacterSpriteButton.GetComponent<Image>().sprite = ShinobiSprite;
        CharacterSpriteButton.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(110, 100);
        OnClick_ToLobby(); 
    }
    public void OnClick_SelectZealot()
    {
        characterNameText.text = "Zealot";
        PlayerPrefs.SetString("CharacterSelected", "Zealot");
        CharacterSpriteButton.GetComponent<Image>().sprite = ZealotSprite;
        CharacterSpriteButton.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(100, 100);
        OnClick_ToLobby(); 
    }

    public void OnClick_ReadyUp()
    {
        LobbyPanel.SetActive(false);
        WaitingPanel.SetActive(true);

        if (gameModeText.text == "Solo")
        {
            PlayerPrefs.SetString("MatchLength", "180");
            playersFoundText.text = "0/2 Players";
            PhotonNetwork.JoinRandomRoom(null, 2, MatchmakingMode.FillRoom, null, null);
        }
        else if (gameModeText.text == "Quad")
        {
            PlayerPrefs.SetString("MatchLength", "300");
            playersFoundText.text = "0/4 Players";
            PhotonNetwork.JoinRandomRoom(null, 4, MatchmakingMode.FillRoom, null, null);
        }
        else if (gameModeText.text == "Training")
        {
            PlayerPrefs.SetString("MatchLength", "480");
            playersFoundText.text = "0/1 Players";
            CreateCustomRoom(1);
        }
    }

    public void OnClick_LeaveRoom()
    {
        Debug.Log("LeaveRoom()");
        Waiting = false;
        PhotonNetwork.LeaveRoom();
        OnClick_ToLobby();
    }

    public void OnClick_NotImplemented()
    {
        StartCoroutine(Wait(3));
    }
    IEnumerator Wait(int time)
    {
        NotImplementedPanel.SetActive(true);
        yield return new WaitForSeconds(time);
        NotImplementedPanel.SetActive(false);
    }

    private void WaitingRoom(int maxPlayers)
    {
        playersFoundText.text = PhotonNetwork.PlayerList.Length + "/" + maxPlayers + " Players";
        if (PhotonNetwork.PlayerList.Length == maxPlayers && PhotonNetwork.IsMasterClient)
        {
            Waiting = false;
            WaitingPanel.SetActive(false);
            if (maxPlayers == 1)
            {
                LoadArena("TrainingLevel");
            }
            else if (maxPlayers == 2)
            {
                LoadArena("SoloLevel");
            }
            else
            {
                LoadArena("QuadLevel");
            }
        }
    }

    private void CreateCustomRoom(int maxPlayers)
    {
        Debug.Log("CreateCustomRoom(" + maxPlayers + ")");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Convert.ToByte(maxPlayers);
        PhotonNetwork.CreateRoom(null, roomOptions, null, null);
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
            { "Coins", 100 },
            { "Bits", 10 }
            /*{ "Statistics", new Dictionary<string, object>
                {
                    { "Kills", 0 },
                    { "Deaths", 0 },
                    { "Duel Games", 0 },
                    { "Duel Wins", 0 },
                    { "TDM Games", 0 },
                    { "TDM Wins", 0 },
                    { "FFA Games", 0 },
                    { "FFA Wins", 0 }
                }
            }*/
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
            coinsText.text = "" + userData["Coins"];
            bitsText.text = "" + userData["Bits"];
            OnClick_ToLobby();
        } 
        else Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
        });
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

            if (EmailInput.text == "" || PasswordInput.text == "") 
                return false;

            OnClick_SignIn();
            return true;
        }
        catch (FileNotFoundException)
        {
            HasSaveData = false;
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
        byte[] buffer;
        try 
        { 
            buffer = Convert.FromBase64String(cipherText); 
        } 
        catch (ArgumentNullException e)
        {
            return "";
        }

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
