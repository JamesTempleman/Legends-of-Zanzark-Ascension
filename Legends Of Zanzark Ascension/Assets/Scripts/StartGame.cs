using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartGame : MonoBehaviour,IPunObservable
{
    [SerializeField] GameObject NamePrefab;
    public List<GameObject> PlayerNames;
    [SerializeField] GameObject NamePanel;
    [SerializeField] PhotonView pv;
    [SerializeField] GameManager gm;


    // Start is called before the first frame update
    void Start()
    {
        PlayerNames = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

        if (PlayerNames.Count != null)
        {
            if(PhotonNetwork.room != null)
            if (PlayerNames.Count != PhotonNetwork.room.PlayerCount)
            {
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

    public void ButPush()
    {
        pv.RPC("startGame", PhotonTargets.All);
    }

    [PunRPC]
    public void startGame()
    {
        gm.NextGamePhase();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
