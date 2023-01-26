using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TileManager : MonoBehaviour, IPointerClickHandler
{
    //the type of tile this is and all the neighbouring tiles
    public TerrainType Terrain;
    public List<TileManager> NeighboringTiles;

    //current tile status in relation to the player
    public bool isDestroyed;
    public bool isLandMark;
    public bool canBeInfluenced;
    public bool Enraged;

    //visual elements of the tile
    public Image BackgroundColour;
    public TextMeshProUGUI InfluenceValueTxt;
    public GameObject LandmarkMarker;

    //influence placed on tile
    public int InfluenceValueInt;

    //if can be influenced
    public bool Influencable;

    //the player
    public PlayerManager player;

    //who starts at this tile
    public StartLocation StartLocale;

    //who controls this tile
    public string ControlledBy;

    //how much influence is the neighbours putting on this tile
    public int InfluenceFromNeighbor = 0;

    //if this is a landmark how much influence will it give off
    public int LandMarkInfluence = 1;

    //what ogdar clan is this tile aligned with
    public int OgdarClanId = 0;

    //dictionary containing all current influence on tile
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

    //dictionary containing all influence on tile from the previous turn
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

    //the current influence on the tile for debugging
    [SerializeField] List<int> ValuesForEdditor = new List<int>();

    //has the game started
    bool GameStarted = false;

    //an enum for start locations
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
        //set up the buttons
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
    }

    // Start is called before the first frame update
    public void OnGameStart()
    {
        //sets the local player
        foreach (PlayerManager p in GameManager.Players)
        {
            if (p.pv.owner == PhotonNetwork.player)
            {
                player = p;
            }
        }

        //if ur starting here put influence here
        if((int)StartLocale == (int)player.PlayingAs)
        {
            InfluenceValueInt = 1;
            LandmarkMarker.GetComponent<TextMeshProUGUI>().text = "Religious Capital";
        }
        else
        {
            InfluenceValueInt = 0;
        }

        //start the end turn and then the start of turn sequence
        EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        //if the game hasn't started don't do anything
        if (!GameStarted)
            return;

        //if its al landmark turn on the marker
        LandmarkMarker.SetActive(isLandMark);

        //how much infulence you got
        InfluenceValueTxt.text = InfluenceValueInt.ToString();

        //debugging stuff for the editor
        ValuesForEdditor.Clear();
        foreach (int i in CurrentInfluenceOnTile.Values)
        {
            ValuesForEdditor.Add(i);
        }

        //checks current owner
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

        //checks if this tile can be influenced
        InfluenceCheck();

        //is this tile destroyed if so show it
        if (isDestroyed)
        {
            BackgroundColour.color = Color.black;
        }

        //influence neighbours if your a landmark
        if (isLandMark && ControlledBy == player.PlayingAsString)
        {
            foreach (TileManager t in NeighboringTiles)
            {
                t.InfluenceFromNeighbor = LandMarkInfluence;
            }
        }

        //add this to the controlled by list if your control it
        if (ControlledBy == player.PlayingAsString)
        {
            if (!player.ControlledTiles.Find(t => t.name == this.name))
                player.ControlledTiles.Add(this);
        }

        //if your ending your turn stop the player from making decisions
        if (player.nextTurnReady)
        {

            Influencable = false;

        }
    }

    //checks who owns this tile based on current data
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

        //if changing values sync this data with everyone
        if (ChangeValues)
        {
            ControlledBy = HighestCult;

            player.SendData();
        }

        return HighestCult;

    }

    //checks who owns this tile based on all previous data
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
        //if this is destoryed tell everyone about it
        if (isDestroyed)
        {
            player.pv.RPC("DestroyTile", PhotonTargets.All, new object[] { this.name });
        }

        //gain influence based on neigbours
        int mod = InfluenceFromNeighbor;
        
        //ogdar clan strong buff
        if(OgdarClanId == 4)
        {
            mod = mod + 1;
        }

        //all the mods you get from playing different cults
        if (player.PlayingAs == PlayerManager.Cults.DarkPantheon && isLandMark && ControlledBy == player.PlayingAsString)
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

        if (Enraged && player.PlayingAs == PlayerManager.Cults.EmperorEternal)
        {
            mod = InfluenceValueInt + mod;
        }
        
        //sets the current influence on this tile
        CurrentInfluenceOnTile[player.PlayingAsString] = InfluenceValueInt + mod;

        //a data holder just to go through all the data within the currentinfluenceontile list without editing it
        Dictionary<string, int> DataHolder = new Dictionary<string, int>();

        DataHolder = CurrentInfluenceOnTile;

        string HighestCult = "";

        foreach (string key in DataHolder.Keys)
        {
            //checks the current owner of this tile
            if (NeighboringTiles.Find(neighbor => neighbor.CurrentOwnerCheck() == key && neighbor.isLandMark))
            {
                HighestCult = CurrentOwnerCheck(mod, true);
            }
            else
            {
                HighestCult = CurrentOwnerCheck(0, true);
            }
        }

        //calls the end of turn sequence from the playermanager
        player.EndofTurnUpdate();
        GameStarted = true;

        //sets the previous influence on tile
        PreviousInfluenceOnTile = DataHolder;

        //add this tile to what you control if you control it
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
        //set up can be influenced
        canBeInfluenced = false;


        //if you control this tile it can be influenced
        if (ControlledBy == player.PlayingAsString)
        {
            canBeInfluenced = true;
        }

        //checks neighbouring tiles if they can be influenced
        foreach (TileManager t in NeighboringTiles)
        {
            //if playing as the Elemental Spirits you get an extra tile in distance for what you can influenc
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

            //if you control a neighbouring tile this can be influenced
            if (t.ControlledBy == player.PlayingAsString)
            {
                canBeInfluenced = true;
            }
        }

        //if this tile is destroyed it cannot be influenced
        if (isDestroyed) canBeInfluenced = false;

        //can this tile be influenced
        if (canBeInfluenced)
        {
            Influencable = true;
        }
        else
        {
            Influencable = false;
        }
        //set up a colour variable
        Color c = Color.white;

        //who controls this tile based on last turn
       ControlledBy = PrevOwnerCheck();

        //change tile colour based on who owns this tile or if it can be influenced
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

    //the type of tile this is
    public enum TerrainType
    {
        Mountain,
        Forrest,
        Plains,
        Wasteland
    }

    //destroy this tile
    public void DestroyTile()
    {
        isDestroyed = true;
    }

    //on use abilities
    public void OnPointerClick(PointerEventData eventData)
    {
        //if MakeExample is toggled on
        if (GameManager.MakeExample)
        {
            //if you control the tile
            if (ControlledBy == player.PlayingAsString)
            {
                //destroy a tile and give the player influence
                isDestroyed = true;
                player.gm.TurnOffAbilityUI();
                GameManager.MakeExample = false;
                player.InfluenceAvailable = player.InfluenceAvailable + 10;
            }
        }
        //if PurgeNonBelievers is toggled on
        else if (GameManager.PurgeNonBelievers)
        {
            //if neighbouring a controlled area
            bool neighborcheck = false;
            foreach (TileManager t in NeighboringTiles)
            {
                if (t.ControlledBy == player.PlayingAsString)
                {
                    neighborcheck = true;
                }
            }
            if (neighborcheck)
            {
                //gain control of this tile
                CurrentInfluenceOnTile[player.PlayingAsString] = 100;
                player.gm.TurnOffAbilityUI();
                GameManager.PurgeNonBelievers = false;
            }
        }
        //if DwarvenTunnels is toggled on
        else if (GameManager.DwarvenTunnels)
        {
            //put an influence point on the tile
            InfluenceValueInt++;
            player.gm.TurnOffAbilityUI();
            GameManager.DwarvenTunnels = false;
        }
        //if Erruption is toggled on
        else if (GameManager.Erruption)
        {
            //give this tile the destroyed status
            isDestroyed = true;
            player.gm.TurnOffAbilityUI();
            GameManager.Erruption = false;
        }
        //if RedRage is toggled on
        else if (GameManager.RedRage)
        {
            //give this tile the enraged status
            Enraged = true;
            player.gm.TurnOffAbilityUI();
            GameManager.RedRage = false;
        }
        //if DedicationMode is toggled on and this is a landmark that is owned by the player and they are playing ogdarism
        else if (isLandMark && player.PlayingAs == PlayerManager.Cults.Ogdarism && ControlledBy == player.PlayingAsString && GameManager.DedicationMode)
        {
            //turn on the ogdar dedication panel
            player.gm.OgdarOptions(this);
        }
        //if no abilities are being used and this place can have influence moved about
        else if (canBeInfluenced && Influencable)
        {
            //add influence
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                PlusInfulence();
            }
            //take away influence
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                MinusInfulence();
            }
        }
    }
    
    public void onPointerEnter()
    {
        //if is not destroyed
        if (!isDestroyed)
        {
            //turn tool tip on with all the previous turns data
            player.gm.toolTipOn(PreviousInfluenceOnTile);

            //set up a string
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
            //send the tooltip the terrain type in string form
            player.gm.toolTipOn(s);
        }
        //else send the text "Destroyed"
        else
            player.gm.toolTipOn("Destroyed");
    }

    //when the mouse leaves the tile
    public void onPointerExit()
    {
        //turn the tooltip off
        player.gm.toolTipOff();

    }

    //add influence to this tile
    public void PlusInfulence()
    {
        if (player.InfluenceAvailable > 0)
        {
            InfluenceValueInt++;
            player.InfluenceAvailable--;
        }
    }

    //take away influence on this tile
    public void MinusInfulence()
    {
        if (InfluenceValueInt > 0)
        {
            InfluenceValueInt--;
            player.InfluenceAvailable++;
        }
    }
}
