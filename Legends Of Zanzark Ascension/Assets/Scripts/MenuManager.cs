using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    //Server connection details
    [SerializeField] TMP_InputField Username;
    [SerializeField] string ServerName;
    [SerializeField] TextMeshProUGUI ConnectedIndicator;
    [SerializeField] TextMeshProUGUI PlayerCount;

    //joining/hosting games buttons
    [SerializeField] Button RandomMatchMakingBut;
    [SerializeField] Button JoinHostBut;
    [SerializeField] Button JoinGameBut;
    [SerializeField] Button HostGameBut;

    //the server listing script
    [SerializeField] ServerListingManager slm;

    //the lobby panel
    [SerializeField] GameObject JoinPanel;

    //toggles for the player
    bool Connected = false;
    bool SearchingForGame = false;

    //input field for joining a server with a password
    [SerializeField] TMP_InputField PasswordInputJoin;

    //HostServerSettings
    int maxPlayers = 9;
    string password = "";

    //hosting a server input fields
    [SerializeField] TMP_InputField PasswordInputHost;
    [SerializeField] TMP_InputField ServerNameInputHost;

    //Sets the server name via input field
    public void SetServerName(TMP_InputField s)
    {
        ServerName = s.text;
    }
    //sets the server name via string
    public void SetServerName(string s)
    {
        ServerName = s;
    }
    //Set server password via input field
    public void SetServerPassword(TMP_InputField s)
    {
        if (s.text.Length > 1)
            password = s.text;
        else
            password = "";
    }
    //sets the server name via string
    public void SetServerPassword(string s)
    {
        password = s;
    }
    //tells the game the player is searching for a game
    public void MatchMakingOn()
    {
        SearchingForGame = true;
    }
    //tells the game the player is no longer searching for a game
    public void MatchMakingOff()
    {
        SearchingForGame = false;
    }
    //closes the application
    public void ExitGame()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        //connects to the photon servers
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    //when connected to the photon servers
    private void OnConnectedToMaster()
    {
        //join the lobby and allow the player interact with the game
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        JoinPanel.SetActive(true);
        Connected = true;
    }

    // Update is called once per frame
    void Update()
    {
        //if connected to the photon servers
        if (Connected)
        {
            //tell the player how many people are online and also that they themselves are connected to the servers
            ConnectedIndicator.text = "Connected";
            PlayerCount.text = PhotonNetwork.countOfPlayers.ToString();
        }

        //make sure there username is atleast 2 characters
        if (Username.text.Length > 2)
        {
            RandomMatchMakingBut.interactable = true;
            JoinHostBut.interactable = true;
        }
        else
        {
            RandomMatchMakingBut.interactable = false;
            JoinHostBut.interactable = false;
        }

        //make sure the server name is atleast 1 character
        if(ServerNameInputHost.text.Length > 1)
        {
            HostGameBut.interactable = true;
        }
        else
        {
            HostGameBut.interactable = false;
        }

        //when searching for a game
        if (SearchingForGame)
        {
            //set ur username
            PhotonNetwork.player.NickName = Username.text;

            //iterate through all the open rooms
            foreach(RoomInfo ri in PhotonNetwork.GetRoomList())
            {
                //if room is open and can be joined without a password
                if(ri.IsOpen && (int)ri.MaxPlayers > ri.PlayerCount && ri.CustomProperties["P"].ToString() == "")
                {
                    //join room
                    PhotonNetwork.player.NickName = Username.text;
                    PhotonNetwork.JoinRoom(ri.Name);
                    PhotonNetwork.LoadLevel(1);
                }
            }

            //if ur in a room go to the game scene
            if (PhotonNetwork.inRoom)
                PhotonNetwork.LoadLevel(1);
        }
    }

    public void JoinServer()
    {
        //sets the server data, password and server name
        SetServerPassword(PasswordInputJoin);
        SetServerName(slm.SelectedRoom.Name);

        //checks server password
        if(ServerName.Length > 1 && slm.SelectedRoom.CustomProperties["P"].ToString() == password)
        {
            //joins server
            PhotonNetwork.player.NickName = Username.text;
            PhotonNetwork.JoinRoom(ServerName);
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void CreateServer()
    {
        //sets the server data, password and server name
        SetServerName(ServerNameInputHost);
        SetServerPassword(PasswordInputHost);

        //if server name is longer than 1 character
        if (ServerName.Length > 1)
        {
            //set username
            PhotonNetwork.player.NickName = Username.text;

            //create room with data
            PhotonNetwork.CreateRoom
                (
                ServerName,
                new RoomOptions()
                {
                    MaxPlayers = (byte)this.maxPlayers,
                    IsVisible = true,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
                    {
                        { "P", password }
                    },
                    CustomRoomPropertiesForLobby = new string[] { "P" }
                },
                TypedLobby.Default
                );

            //load game scene
            PhotonNetwork.LoadLevel(1);
        }
    }

}