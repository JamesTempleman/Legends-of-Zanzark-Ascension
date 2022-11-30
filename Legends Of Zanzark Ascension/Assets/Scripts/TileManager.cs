using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileManager : MonoBehaviour, IPunObservable
{
    public TerrainType Terrain;
    public List<TileManager> NeighboringTiles;

    public bool isDestroyed;
    public bool isLandMark;
    public bool canBeInfluenced;

    public Image BackgroundColour;
    public TextMeshProUGUI InfluenceValueTxt;
    public GameObject LandmarkMarker;
    public Button[] InfluenceButts;

    public int InfluenceValueInt;


    public PlayerManager player;

    public StartLocation StartLocale;

    public string ControlledBy;

    public int InfluenceFromNeighbor = 0;

    public int LandMarkInfluence = 1;

    public Dictionary<string, int> CurrentInfluenceOnTile = new Dictionary<string, int>()
    {
        {"SaintHood", 0 },
        {"Ancestor", 0 },
        {"HighPantheon", 0 },
        {"DarkPantheon", 0 },
        {"Elemental", 0 },
        {"BlackBlood", 0 },
        {"Ogdarism", 0 },
        {"LegionsofAboracrom", 0 },
        {"EmperorEternal", 0 },

    };

    public Dictionary<string, int> PreviousInfluenceOnTile = new Dictionary<string, int>()
    {
        {"SaintHood", 0 },
        {"Ancestor", 0 },
        {"HighPantheon", 0 },
        {"DarkPantheon", 0 },
        {"Elemental", 0 },
        {"BlackBlood", 0 },
        {"Ogdarism", 0 },
        {"LegionsofAboracrom", 0 },
        {"EmperorEternal", 0 },

    };

    [SerializeField] List<int> ValuesForEdditor = new List<int>();

    bool GameStarted = false;

    public enum StartLocation
    {
        None,
        SaintHood,
        Ancestor,
        HighPantheon,
        DarkPantheon,
        Elemental,
        BlackBlood,
        Ogdarism,
        LegionsofAboracrom,
        EmperorEternal
    }

    public PhotonView pv;


    // Start is called before the first frame update
    public void OnGameStart()
    {

        foreach (PlayerManager p in GameManager.Players)
        {
            if (p.pv.owner == PhotonNetwork.player)
            {
                player = p;
            }
        }

        if (isLandMark) 
        { 
            LandmarkMarker.SetActive(true); 
        }

        if((int)StartLocale == (int)player.PlayingAs)
        {
            InfluenceValueInt = 1;
            LandmarkMarker.GetComponent<TextMeshProUGUI>().text = "Religious Capital";
        }
        else
        {
            InfluenceValueInt = 0;
        }

        EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameStarted)
            return;

        InfluenceValueTxt.text = InfluenceValueInt.ToString();

        ValuesForEdditor.Clear();
        foreach (int i in CurrentInfluenceOnTile.Values)
        {
            ValuesForEdditor.Add(i);
        }

        InfluenceCheck();

        if (isLandMark && ControlledBy == player.PlayingAsString)
        {
            foreach (TileManager t in NeighboringTiles)
            {
                t.InfluenceFromNeighbor = LandMarkInfluence;
            }
        }

        if (ControlledBy == player.PlayingAsString)
        {
            if (!player.ControlledTiles.Find(t => t.name == this.name))
                player.ControlledTiles.Add(this);
        }

        if (player.nextTurnReady)
        {
            foreach(Button b in InfluenceButts)
            {
                b.interactable = false;
            }
        }
    }

    public string CurrentOwnerCheck(int modifier = 0, bool ChangeValues = false)
    {
        string HighestCult = "";
        int HighestNum = 0;

        foreach (string key in CurrentInfluenceOnTile.Keys)
        {
            int i = CurrentInfluenceOnTile[key] + modifier;

            if (i > HighestNum)
            {
                HighestCult = key;
                HighestNum = i;
                
            }
        }
        

        foreach (string key in CurrentInfluenceOnTile.Keys)
        {
            if (CurrentInfluenceOnTile[key] == HighestNum)
            {
                if(HighestCult != key)
                {
                    HighestCult = "None";
                }
            }
        }
        if (PreviousInfluenceOnTile["SaintHood"] == HighestNum)
        {
            HighestCult = "SaintHood";
        }

        if (HighestNum == 0)
        {
            HighestCult = "None";
        }

        if (ChangeValues)
        {
            ControlledBy = HighestCult;

            player.SendData(false);
        }

        return HighestCult;

    }
    public string PrevOwnerCheck(int modifier = 0)
    {
        string HighestCult = "";
        int HighestNum = 0;

        foreach (string key in PreviousInfluenceOnTile.Keys)
        {
            int i = PreviousInfluenceOnTile[key] + modifier;

            if (i > HighestNum)
            {
                HighestCult = key;
                HighestNum = i;

            }
        }

        foreach (string key in PreviousInfluenceOnTile.Keys)
        {
            if (PreviousInfluenceOnTile[key] == HighestNum)
            {
               if (HighestCult != key)
                {
                    HighestCult = "None";
                }
            }
        }

        if(PreviousInfluenceOnTile["SaintHood"] == HighestNum)
        {
            HighestCult = "SaintHood";
        }

        if (HighestNum == 0)
        {
            HighestCult = "None";
        }

        return HighestCult;

    }

    [PunRPC]
    public void EndTurn()
    {
        CurrentInfluenceOnTile[player.PlayingAsString] = InfluenceValueInt + InfluenceFromNeighbor;

        Dictionary<string, int> DataHolder = new Dictionary<string, int>();

        DataHolder = CurrentInfluenceOnTile;

        string HighestCult = "";

        foreach (string key in DataHolder.Keys)
        {

            if (NeighboringTiles.Find(neighbor => neighbor.CurrentOwnerCheck() == key && neighbor.isLandMark))
            {
                HighestCult = CurrentOwnerCheck(InfluenceFromNeighbor, true);
            }
            else
            {
                HighestCult = CurrentOwnerCheck(0, true);
            }
        }

        player.EndofTurnUpdate();
        GameStarted = true;

        PreviousInfluenceOnTile = DataHolder;

        if((int)StartLocale == (int)player.PlayingAs && ControlledBy != player.PlayingAsString)
        {
            //Kick from game
            //Player looses game
        }

        foreach(PlayerManager p in GameManager.Players)
        {
            if(ControlledBy == p.PlayingAsString)
            {
                if(!p.ControlledTiles.Find(tile => tile.name == this.name))
                p.ControlledTiles.Add(this);
            }
        }

    }

    public void InfluenceCheck()
    {
        canBeInfluenced = false;

        if (ControlledBy == player.PlayingAsString)
        {
            canBeInfluenced = true;
        }

        foreach (TileManager t in NeighboringTiles)
        {
            if (player.PlayingAs == PlayerManager.Cults.Elemental)
            {
                foreach (TileManager ti in NeighboringTiles)
                {
                    if (ti.ControlledBy == player.PlayingAsString)
                    {
                        canBeInfluenced = true;
                    }
                }
            }

            else if (t.ControlledBy == player.PlayingAsString)
            {
                canBeInfluenced = true;
            }
        }

        if (isDestroyed) canBeInfluenced = false;

        if (canBeInfluenced)
        {
            foreach (Button b in InfluenceButts)
            {
                b.interactable = true;
            }
        }
        else
        {
            foreach (Button b in InfluenceButts)
            {
                b.interactable = false;
            }
        }
        Color c = Color.white;

       ControlledBy = PrevOwnerCheck();

        //figure out definitive colours for each later
        switch (ControlledBy)
        {
            case "None":
                if (canBeInfluenced)
                    c = Color.green;
                break;
            case "SaintHood":
                c = Color.cyan;
                break;
            case "Ancestor":
                break;
            case "HighPantheon":
                break;
            case "DarkPantheon":
                break;
            case "Elemental":
                break;
            case "BlackBlood":
                c = Color.red;
                break;
            case "Ogdarism":
                break;
            case "LegionsofAboracrom":
                c = Color.magenta;
                break;
            case "EmperorEternal":
                c = Color.yellow;
                break;
        }

        BackgroundColour.color = c;
    }

    public enum TerrainType
    {
        Mountain,
        Forrest,
        Plains,
        Wasteland
    }

    public enum ControlState
    {
        Controlled,
        Contested,
        UnControlled
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public void onPointerEnter()
    {
        player.gm.toolTipOn(PreviousInfluenceOnTile);

    }

    public void onPointerExit()
    {

        player.gm.toolTipOff();

    }

    public void PlusInfulence()
    {
        if (player.InfluenceAvailable > 0)
        {
            InfluenceValueInt++;
            player.InfluenceAvailable--;
        }

    }

    public void MinusInfulence()
    {
        if (InfluenceValueInt > 0)
        {
            InfluenceValueInt--;
            player.InfluenceAvailable++;
        }

    }

}
