using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGame : MonoBehaviour,IPunObservable
{
    //player in lobby name prefab
    [SerializeField] GameObject NamePrefab;

    //list of the player name prefabs for each player in lobby
    public List<GameObject> PlayerNames;
    [SerializeField] GameObject NamePanel;
    [SerializeField] PhotonView pv;
    [SerializeField] GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        //sets up the list
        PlayerNames = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //if the playernames count is not null
        if (PlayerNames.Count != null)
        {   //connected to a room and the amount of player names is not equal to the amount of players in room
            if(PhotonNetwork.room != null)
            if (PlayerNames.Count != PhotonNetwork.room.PlayerCount)
            {
                //create all the player names and set them up
                foreach (GameObject g in PlayerNames)
                {
                    PlayerNames.Remove(g);
                    Destroy(g);
                }

                for (int i = PhotonNetwork.room.PlayerCount; i > 0; i--)
                {
                    var g = GameObject.Instantiate(NamePrefab);
                    g.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.playerList[i - 1].NickName;
                    g.transform.parent = NamePanel.transform;
                    PlayerNames.Add(g);
                }
            }
        }
    }

    //when start game is pushed
    public void ButPush()
    {
        pv.RPC("startGame", PhotonTargets.All);
    }

    //start everyones games
    [PunRPC]
    public void startGame()
    {
        gm.NextGamePhase();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
