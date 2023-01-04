using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour, IPointerClickHandler
{
    public TerrainType Terrain;
    public List<TileManager> NeighboringTiles;

    public bool isDestroyed;
    public bool isLandMark;
    public bool canBeInfluenced;
    public bool Enraged;

    public Image BackgroundColour;
    public TextMeshProUGUI InfluenceValueTxt;
    public GameObject LandmarkMarker;

    public int InfluenceValueInt;

    public bool Influencable;

    public PlayerManager player;

    public StartLocation StartLocale;

    public string ControlledBy;

    public int InfluenceFromNeighbor = 0;

    public int LandMarkInfluence = 1;

    public int OgdarClanId = 0;

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

    private void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }

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

        LandmarkMarker.SetActive(isLandMark);

        InfluenceValueTxt.text = InfluenceValueInt.ToString();

        ValuesForEdditor.Clear();
        foreach (int i in CurrentInfluenceOnTile.Values)
        {
            ValuesForEdditor.Add(i);
        }

        foreach (string key in PreviousInfluenceOnTile.Keys)
        {
            if (NeighboringTiles.Find(neighbor => neighbor.CurrentOwnerCheck() == key && neighbor.isLandMark))
            {
                ControlledBy = CurrentOwnerCheck(InfluenceFromNeighbor);
            }
            else
            {
                ControlledBy = CurrentOwnerCheck(0);
            }
        }

        InfluenceCheck();

        if (isDestroyed)
        {
            BackgroundColour.color = Color.black;
        }

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

            Influencable = false;

        }
    }

    public string CurrentOwnerCheck(int modifier = 0, bool ChangeValues = false)
    {
        string HighestCult = "";
        int HighestNum = 0;

        foreach (string key in CurrentInfluenceOnTile.Keys)
        {
            int i = CurrentInfluenceOnTile[key] + modifier;

            if(key == "EmperorEternal" && Enraged)
            {
                i = i * 2;
            }

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

            player.SendData();
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

    public void EndTurn()
    {
        int mod = InfluenceFromNeighbor;
        
        //strong
        if(OgdarClanId == 4)
        {
            mod = mod + 1;
        }

        if (player.PlayingAs == PlayerManager.Cults.DarkPantheon && isLandMark)
        {
            mod= mod + 1;
        }

        if(player.PlayingAs == PlayerManager.Cults.Ancestor && Terrain == TerrainType.Mountain)
        {
            mod = mod + InfluenceValueInt;
        }

        if(player.PlayingAs == PlayerManager.Cults.HighPantheon && Terrain == TerrainType.Forrest)
        {
            mod = mod + InfluenceValueInt;
        }

        CurrentInfluenceOnTile[player.PlayingAsString] = InfluenceValueInt + mod;
        Dictionary<string, int> DataHolder = new Dictionary<string, int>();

        DataHolder = CurrentInfluenceOnTile;

        string HighestCult = "";

        foreach (string key in DataHolder.Keys)
        {

            if (NeighboringTiles.Find(neighbor => neighbor.CurrentOwnerCheck() == key && neighbor.isLandMark))
            {
                HighestCult = CurrentOwnerCheck(mod, true);
            }
            else
            {
                HighestCult = CurrentOwnerCheck(0, true);
            }
        }

        player.EndofTurnUpdate();
        GameStarted = true;

        PreviousInfluenceOnTile = DataHolder;

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
                foreach (TileManager ti in t.NeighboringTiles)
                {
                    if (ti.ControlledBy == player.PlayingAsString)
                    {
                        canBeInfluenced = true;
                    }
                }
            }

            if (t.ControlledBy == player.PlayingAsString)
            {
                canBeInfluenced = true;
            }
        }

        if (isDestroyed) canBeInfluenced = false;

        if (canBeInfluenced)
        {
            Influencable = true;
        }
        else
        {
            Influencable = false;
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
                c = Color.blue;
                break;
            case "HighPantheon":
                c = new Color(0.6392157f, 0.7960784f, 0.1843137f);
                break;
            case "DarkPantheon":
                c = new Color(0.0497508f, 0.245283f, 0.05721563f);
                break;
            case "Elemental":
                c = new Color(0.5283019f,0f, 0.201051f);
                break;
            case "BlackBlood":
                c = Color.red;
                break;
            case "Ogdarism":
                c = new Color(0.9339623f, 0.32505f, 0f);
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

    //on use abilities
    public void OnPointerClick(PointerEventData eventData)
    {

        if (GameManager.MakeExample)
        {
            if (ControlledBy == player.PlayingAsString)
            {
                isDestroyed = true;
                player.InfluenceAvailable = player.InfluenceAvailable + 10;
                player.gm.TurnOffAbilityUI();
                GameManager.MakeExample = false;
            }
        }
        else if (GameManager.PurgeNonBelievers)
        {
            Debug.Log("1");
            bool neighborcheck = false;
            foreach (TileManager t in NeighboringTiles)
            {
                if (t.ControlledBy == player.PlayingAsString)
                {
                    Debug.Log("2");
                    neighborcheck = true;
                }
            }
            if (neighborcheck)
            {
                Debug.Log("3");
                CurrentInfluenceOnTile[player.PlayingAsString] = 100;
                player.gm.TurnOffAbilityUI();
                GameManager.PurgeNonBelievers = false;
            }
        }
        else if (GameManager.DwarvenTunnels)
        {
            InfluenceValueInt++;
            player.gm.TurnOffAbilityUI();
            GameManager.DwarvenTunnels = false;
        }
        else if (GameManager.Erruption)
        {
            isDestroyed = true;
            player.gm.TurnOffAbilityUI();
            GameManager.Erruption = false;
        }
        else if (GameManager.RedRage)
        {
            Enraged = true;
            player.gm.TurnOffAbilityUI();
            GameManager.RedRage = false;
        }
        else if (isLandMark && player.PlayingAs == PlayerManager.Cults.Ogdarism && ControlledBy == player.PlayingAsString)
        {
            player.gm.OgdarOptions(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlusInfulence();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            MinusInfulence();
        }

    }
    
    public void onPointerEnter()
    {

        if (!isDestroyed)
        {
            player.gm.toolTipOn(PreviousInfluenceOnTile);

            string s = "";
            switch (Terrain)
            {
                case TerrainType.Mountain:
                    s = "Mountains";
                    break;
                case TerrainType.Forrest:
                    s = "Forrests";
                    break;
                case TerrainType.Plains:
                    s = "Plains";
                    break;
                case TerrainType.Wasteland:
                    s = "Wastelands";
                    break;
            }
            player.gm.toolTipOn(s);
        }
        else
            player.gm.toolTipOn("Destroyed");
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
