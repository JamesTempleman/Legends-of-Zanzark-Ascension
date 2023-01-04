using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public GameObject GameUI;

    public Text EndTxt;
    public GameObject EndPanel;

    public TextMeshProUGUI Loadtxt;
    public GameObject LoadingPanel;
    public int turn = 1;
    int TurnLoadingValue = 1;
    public int percentLoaded = 0;

    public GameObject AbilityInUseUI;

    public GameObject RedRageBut;
    public GameObject EruptionBut;
    public GameObject DwarvenTunnelsBut;
    public GameObject PurgeNonBelieversBut;
    public GameObject MakeExampleBut;
    public static bool PurgeNonBelievers = false;
    public static bool RedRage = false;
    public static bool Erruption = false;
    public static bool DwarvenTunnels = false; 
    public static bool MakeExample = false;

    public GameObject OgdarOptionsPanel;
    public TileManager OgdarLookingAt;

    public void OgdarOptions(TileManager t)
    {
        OgdarLookingAt = t;
        OgdarOptionsPanel.SetActive(true);
    }

    public void MagicPressed()
    {
        int Value = CyberMath.CyberMath.GetRandomInt(1, 100);

        if(Value > 51)
        {
            OgdarLookingAt.isLandMark = false;
            OgdarLookingAt.Terrain = TileManager.TerrainType.Wasteland;
            foreach(TileManager t in OgdarLookingAt.NeighboringTiles)
            {
                t.Terrain = TileManager.TerrainType.Wasteland;
            }
        }
        else
        {
            OgdarLookingAt.InfluenceValueInt = OgdarLookingAt.InfluenceValueInt + 20;
        }

        OgdarLookingAt.OgdarClanId = 3;
        OgdarLookingAt = null;
        OgdarOptionsPanel.SetActive(false);
    }

    public void StrongPressed()
    {
        foreach (TileManager t in OgdarLookingAt.NeighboringTiles)
        {
            if (!t.isLandMark)
            {
                t.OgdarClanId = 4;
            }
        }

        if (OgdarLookingAt.OgdarClanId != 4)
            OgdarLookingAt.OgdarClanId = 2;
     
        OgdarLookingAt = null;
        OgdarOptionsPanel.SetActive(false);
    }

    public void SmartPressed()
    {
        OgdarLookingAt.OgdarClanId = 1;
        OgdarLookingAt = null;
        OgdarOptionsPanel.SetActive(false);
    }

    public void toolTipOn(Dictionary<string, int> d)
    {
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        foreach (string key in d.Keys)
        {
            if (d[key] > 0) txt.text = txt.text + System.Environment.NewLine + key + ": " + d[key].ToString();
        }

        txt.gameObject.SetActive(true);
    }

    public void toolTipOn(string s)
    {
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        txt.text = txt.text + System.Environment.NewLine + s;

        txt.gameObject.SetActive(true);
    }

    public void toolTipOff()
    {
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        txt.text = "";
        ToolTip.SetActive(false);
    }

    public void RoomLeave()
    {
        PhotonNetwork.LeaveRoom();
    }

    // Start is called before the first frame update
    void Start()
    {
        toolTipOff();
        GamePhase = 0;
        Players = new List<PlayerManager>();
    }

    public void RedRagePressed()
    {
        RedRage = true;
    }

    public void ErruptionPressed()
    {
        Erruption = true;
    }

    public void TurnOffAbilityUI()
    {
        AbilityInUseUI.SetActive(false);
    }

    public void RedRageOn()
    {
        RedRageBut.SetActive(true);
    }

    public void EruptionOn()
    {
        EruptionBut.SetActive(true);
    }

    public void DwarvenTunnelsOn()
    {
        DwarvenTunnelsBut.SetActive(true);
    }

    public void DwarvenTunnelsPressed()
    {
        DwarvenTunnels = true;
    }
    public void PurgeNonBelieverOn()
    {
        PurgeNonBelieversBut.SetActive(true);
    }

    public void MakeExamplePressed()
    {
        MakeExample = true;
    }
    public void MakeExampleOn()
    {
        MakeExampleBut.SetActive(true);
    }

    public void PurgeNonBelieverPressed()
    {
        PurgeNonBelievers = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (LoadingPanel.activeInHierarchy)
        {
            Loadtxt.text = "Syncing data with other Players";

            bool Loaded = true;
            int TurnValue = 0;

            foreach (PlayerManager p in Players)
            {
                //loading Percent
                int value = p.LoadingStatus;
                int total = TurnLoadingValue + PhotonNetwork.room.PlayerCount;
                TurnValue = total;
                total = total * 531;
                percentLoaded = value/total;
                percentLoaded = percentLoaded * 100;

                Loadtxt.text = Loadtxt.text + System.Environment.NewLine + p.PlayingAsString + " " + percentLoaded.ToString() + "%";

                if(percentLoaded < 100)
                {
                    Loaded = false;
                }
            }

            if (Loaded)
            {
                LoadingPanel.SetActive(false);
                TurnLoadingValue = TurnValue;
                turn++;

                foreach(TileManager t in AllTiles)
                {
                    t.Enraged = false;
                }

                if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.EmperorEternal)
                    RedRageOn();
            }
        }
      
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

                    //on use abilities
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.EmperorEternal)
                    {
                        RedRageOn();
                    }
                    if(AllTiles[0].player.PlayingAs == PlayerManager.Cults.BlackBlood)
                    {
                        EruptionOn();
                    }
                    if(AllTiles[0].player.PlayingAs == PlayerManager.Cults.SaintHood)
                    {
                        PurgeNonBelieverOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.Ancestor)
                    {
                        DwarvenTunnelsOn();
                    }
                    if(AllTiles[0].player.PlayingAs == PlayerManager.Cults.HighPantheon)
                    {
                        MakeExampleOn();
                    }

                }
                else
                {
                    foreach (PlayerManager p in Players)
                    {
                        if (p.pv.owner == PhotonNetwork.player)
                        {
                            if (p.ControlledTiles.Count == 0 && p.InfluenceAvailable == 0)
                            {

                                EndTxt.text = p.PlayingAsString + " is out of the game";
                                EndPanel.SetActive(true);

                                if (!p.nextTurnReady)
                                {
                                    EndTurn();
                                }
                            }
                        }
                    }
                }
                GameUI.SetActive(true);

                break;

        }

        if (check && GamePhase > 0)
        {
            int ii = 0;
            foreach (PlayerManager p in Players)
            {
                if (p.isReady)
                {
                    ii++;
                }
            }
            if (PhotonNetwork.room != null)
                if (ii == PhotonNetwork.room.PlayerCount)
                {
                    check = false;
                    Players[0].pv.RPC("NextGamePhase", PhotonTargets.All, new object[] { });
                }
        }


        int i = 0;
        foreach (PlayerManager p in Players)
        {
            if (p.nextTurnReady)
            {
                i++;
            }
        }
        if (PhotonNetwork.room != null)
            if (i == PhotonNetwork.room.PlayerCount)
            {
                ready = true;
                foreach (PlayerManager p in Players)
                {
                    p.nextTurnReady = false;
                }
            }

        if (ready)
        {
            foreach (PlayerManager p in Players)
            {
                if (p.pv.owner == PhotonNetwork.player)
                {
                    LoadingPanel.SetActive(true);

                    p.pv.RPC("EndTurn", PhotonTargets.All);
                }
            }

            ready = false;
            WaitingForPlayers.SetActive(false);
        }

    }

    public void EndTurn()
    {
        foreach (PlayerManager p in Players)
        {
            if (p.pv.owner == PhotonNetwork.player)
            {
                p.pv.RPC("NextTurn", PhotonTargets.All, new object[] { });
                p.gm.WaitingForPlayers.SetActive(true);
            }
        }
    }

    public void NextGamePhase()
    {
        if(GamePhase < 2)
        GamePhase++;
    }
}
