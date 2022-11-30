using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static List<PlayerManager> Players;

    public int GamePhase;

    public GameObject[] GamePhases;

    public List<TileManager> AllTiles;

    bool check = true;
    bool check2 = true;

    [SerializeField] GameObject ToolTip;

    bool NextTurn = false;

    bool ready = false;

    public GameObject WaitingForPlayers;
    public TextMeshProUGUI InfluenceAvailableTxt;
    public TextMeshProUGUI TilesOwnedTxt;

    public void toolTipOn(Dictionary<string, int> d)
    {
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        txt.text = "";

        foreach(string key in d.Keys)
        {
            if(d[key]>0) txt.text = txt.text + System.Environment.NewLine + key + ": " + d[key].ToString();
        }

        txt.gameObject.SetActive(true);
    }

    public void toolTipOff()
    {
        ToolTip.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        toolTipOff();
        GamePhase = 0;
        Players = new List<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tooltippos = Input.mousePosition;
        tooltippos.x++; 
        tooltippos.y++;

        ToolTip.transform.position = tooltippos;

        GamePhases[GamePhase].SetActive(true);

        switch (GamePhase)
        {
            case 0:

                GamePhases[1].SetActive(false);
                GamePhases[2].SetActive(false);

                break;

            case 1:

                GamePhases[0].SetActive(false);
                GamePhases[2].SetActive(false);

                break;

            case 2:

                GamePhases[0].SetActive(false);
                GamePhases[1].SetActive(false);

                if (check2)
                {
                    foreach (TileManager T in AllTiles)
                    {
                        T.OnGameStart();
                    }
                    check2 = false;
                }

                break;

        }

        if (check && GamePhase > 0)
        {
            int ii = 0;
            foreach(PlayerManager p in Players)
            {
                if (p.isReady)
                {
                    ii++;
                }
            } 
            if(PhotonNetwork.room!=null)
            if (ii == PhotonNetwork.room.PlayerCount)
            {
                check = false;
                NextGamePhase();
            }
        }

        int i = 0;
        foreach(PlayerManager p in Players)
        {
            if (p.nextTurnReady)
            {
                i++;
            }
        }
        if(PhotonNetwork.room != null)
        if(i == PhotonNetwork.room.PlayerCount)
        {
            ready = true;
            foreach (PlayerManager p in Players)
            {
                p.nextTurnReady = false;
            }
        }

        if (ready)
        {
            foreach (TileManager t in AllTiles)
            {
                t.pv.RPC("EndTurn", PhotonTargets.All);
            }
            ready = false;
            WaitingForPlayers.SetActive(false);
        }

    }

    public void EndTurn()
    {
        AllTiles[0].player.SendData();
    }

    public void NextGamePhase()
    {
        GamePhase++;
    }
}
