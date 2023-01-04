using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    [SerializeField] TMP_InputField Username;
    [SerializeField] TMP_InputField ServerName;

    [SerializeField] GameObject JoinPanel;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        JoinPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void JoinOrCreateServer()
    {
        if(Username.text.Length > 1 && ServerName.text.Length > 1)
        {
            PhotonNetwork.player.NickName = Username.text;
            PhotonNetwork.JoinOrCreateRoom(ServerName.text, new RoomOptions(), TypedLobby.Default);
            PhotonNetwork.LoadLevel(1);
        }
    }


}
