using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //The buttons you press to select culture
    public List<Button> CultureSelectionBut;
    //to show/hide the specifc cultures
    public List<GameObject> CultureSelection;

    //two lists the first contains the playermanagers for each player the second is used for when changes need to be made the original
    public static List<PlayerManager> Players;
    [SerializeField] List<PlayerManager> PlayersTemp;

    //int to show which phase the game is in (0: game start 1: culture selection 2: Gameplay loop)
    public int GamePhase;

    //Holds the different ui for each phase
    public GameObject[] GamePhases;

    //holds all the tiles in the game
    public List<TileManager> AllTiles;

    //used for making sure certain code in Update() only runs once
    bool check = true;
    bool check2 = true;

    //the tool tip text gameobject
    [SerializeField] GameObject ToolTip;

    //checks if the game should start the next turn
    bool NextTurn = false;

    //checks if everyone is ready to play
    bool ready = false;

    //waiting for people to end turn
    public GameObject WaitingForPlayers;

    //text objects the show the influence, tiles owned and the current turn for the user
    public TextMeshProUGUI InfluenceAvailableTxt;
    public TextMeshProUGUI TilesOwnedTxt;
    public TextMeshProUGUI TurnCounterTxt;

    //UI used when playing the game
    public GameObject GameUI;

    //used when the game ends for the player
    public Text EndTxt;
    public GameObject EndPanel;

    //used for notifying the player that the game is loading
    public TextMeshProUGUI Loadtxt;
    public GameObject LoadingPanel;

    //current turn
    public int turn = 1;
    
    //used for calculating how far along the loading is
    int TurnLoadingValue = 1;
    public int percentLoaded = 0;

    //shows that the player is using an ability
    public GameObject AbilityInUseUI;

    //ability buttons
    public GameObject RedRageBut;
    public GameObject EruptionBut;
    public GameObject DwarvenTunnelsBut;
    public GameObject PurgeNonBelieversBut;
    public GameObject MakeExampleBut;
    public GameObject DedicationBut;

    //toggles on the abilities
    public static bool PurgeNonBelievers = false;
    public static bool RedRage = false;
    public static bool Erruption = false;
    public static bool DwarvenTunnels = false;
    public static bool MakeExample = false;
    public static bool DedicationMode = false;

    //used for the Ogdarism ability
    public GameObject OgdarOptionsPanel;
    public TileManager OgdarLookingAt;

    //when using ogdar ability
    public void OgdarOptions(TileManager t)
    {
        //remembers this tile for later
        OgdarLookingAt = t;
        //shows ogdar panel
        OgdarOptionsPanel.SetActive(true);
    }

    //ogdar Magic 'uns ability used
    public void MagicPressed()
    {
        //random number between 1 and 100
        int Value = CyberMath.CyberMath.GetRandomInt(1, 100);

        //50% chance to take away land mark status and turn this tile and all neighbouring tiles into wasteland tiles
        if (Value > 51)
        {
            OgdarLookingAt.isLandMark = false;
            OgdarLookingAt.Terrain = TileManager.TerrainType.Wasteland;
            foreach (TileManager t in OgdarLookingAt.NeighboringTiles)
            {
                t.Terrain = TileManager.TerrainType.Wasteland;
            }
        }
        //50% chance to put 20 influence on the tile
        else
        {
            OgdarLookingAt.InfluenceValueInt = OgdarLookingAt.InfluenceValueInt + 20;
        }

        //tells the tile what type of tile it is
        OgdarLookingAt.OgdarClanId = 3;

        //forget what tile is selected
        OgdarLookingAt = null;
        //hide ogdar panel
        OgdarOptionsPanel.SetActive(false);
    }

    //Ogdar strong 'uns pressed
    public void StrongPressed()
    {
        //makes it so neighbouring tiles know they are being influenced by strong ogres
        foreach (TileManager t in OgdarLookingAt.NeighboringTiles)
        {
            if (!t.isLandMark)
            {
                t.OgdarClanId = 4;
            }
        }

        //checks if this tile is being influenced if not tell it to be strong home
        if (OgdarLookingAt.OgdarClanId != 4)
            OgdarLookingAt.OgdarClanId = 2;

        //forget what tile is selected
        OgdarLookingAt = null;
        //hide ogdar panel
        OgdarOptionsPanel.SetActive(false);
    }

    //Ogdar smart 'uns pressed
    public void SmartPressed()
    {
        //tells the tile that this is a smart tile
        OgdarLookingAt.OgdarClanId = 1;
        //forget what tile is selected
        OgdarLookingAt = null;
        //hide ogdar panel
        OgdarOptionsPanel.SetActive(false);
    }

    //turns tooltip on and shows whats in the paramater dictionary
    public void toolTipOn(Dictionary<string, int> d)
    {
        //finds the text componenet
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        //iterates through the dictionary
        foreach (string key in d.Keys)
        {
            //if the value is greater than 0 show it
            if (d[key] > 0) txt.text = txt.text + System.Environment.NewLine + key + ": " + d[key].ToString();
        }

        //shows tooltip
        txt.gameObject.SetActive(true);
    }

    //turns tooltip on and shows whats in the paramater string
    public void toolTipOn(string s)
    {
        //finds the text componenet
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        //put the text into the tool tip
        txt.text = txt.text + System.Environment.NewLine + s;

        //shows tooltip
        txt.gameObject.SetActive(true);
    }

    //hides tooltip
    public void toolTipOff()
    {
        //find text component
        TextMeshProUGUI txt = ToolTip.GetComponent<TextMeshProUGUI>();

        //clear and hide text
        txt.text = "";
        ToolTip.SetActive(false);
    }

    //leave the current room
    public void RoomLeave()
    {
        //set up/clear the playerstemp list
        PlayersTemp = new List<PlayerManager>();

        //players temp is the same as players
        PlayersTemp = Players;

        //get rid of the player leaving
        PlayersTemp.Remove(AllTiles[0].player);

        //tell everyone to clear there local players list
        foreach (PlayerManager p in PlayersTemp)
        {
            p.pv.RPC("ClearPlayers", PhotonTargets.All, new object[] { });
        }
        //tell everyone to update there local players list
        foreach (PlayerManager p in PlayersTemp)
        {
            p.pv.RPC("UpdatePlayers", PhotonTargets.All, new object[] { });
        }

        //leave room and go back to main menu
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    //toggles red rage on
    public void RedRagePressed()
    {
        RedRage = true;
    }

    //toggles erruption on
    public void ErruptionPressed()
    {
        Erruption = true;
    }

    //hides ability UI
    public void TurnOffAbilityUI()
    {
        AbilityInUseUI.SetActive(false);
    }

    //shows the red rage ability button
    public void RedRageOn()
    {
        RedRageBut.SetActive(true);
    }

    //shows the erruption ability button
    public void EruptionOn()
    {
        EruptionBut.SetActive(true);
    }

    //shows the dwarven tunnels ability button
    public void DwarvenTunnelsOn()
    {
        DwarvenTunnelsBut.SetActive(true);
    }

    //toggles dwarven tunnels on
    public void DwarvenTunnelsPressed()
    {
        DwarvenTunnels = true;
    }

    //toggles Purge the non believers on
    public void PurgeNonBelieverOn()
    {
        PurgeNonBelieversBut.SetActive(true);
    }


    //toggles Make Example of on
    public void MakeExamplePressed()
    {
        MakeExample = true;
    }

    //shows the  Make Example of ability button
    public void MakeExampleOn()
    {
        MakeExampleBut.SetActive(true);
    }

    //shows the Purge the non believers ability button
    public void PurgeNonBelieverPressed()
    {
        PurgeNonBelievers = true;
    }

    //toggles Ogdar dedication
    public void Dedication()
    {
        if (!DedicationMode)
        {
            AbilityInUseUI.SetActive(true);
            DedicationMode = true;
        }
        else if (DedicationMode)
        {
            AbilityInUseUI.SetActive(false);
            DedicationMode = false;
        }
    }

    //shows the ogdar dedication toggle
    public void DedicationOn()
    {
        DedicationBut.SetActive(true);
    }

    //shows the end screen
    void WinnerScreen()
    {
        EndPanel.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        //turns tooltips off
        toolTipOff();

        //sets game phase to 0
        GamePhase = 0;

        //sets up the players list
        Players = new List<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //if loading
        if (LoadingPanel.activeInHierarchy)
        {
            //tells the player whats going on
            Loadtxt.text = "Syncing data with other Players";

           
            bool Loaded = true;
            int TurnValue = 0;

            //iterates through the players
            foreach (PlayerManager p in Players)
            {
                //calculates loading Percent
                int value = p.LoadingStatus;
                int total = TurnLoadingValue + PhotonNetwork.room.PlayerCount;
                TurnValue = total;
                total = total * 531;
                percentLoaded = value / total;
                percentLoaded = percentLoaded * 100;

                //tells the player loading percentage
                Loadtxt.text = Loadtxt.text + System.Environment.NewLine + p.PlayingAsString + " " + percentLoaded.ToString() + "%";

                //checks if everyone is loaded
                if (percentLoaded < 100)
                {
                    Loaded = false;
                }
            }

            //if everyone is loaded
            if (Loaded)
            {
                //turn off loading ui
                LoadingPanel.SetActive(false);
                //next turn
                TurnLoadingValue = TurnValue;
                turn++;

                TurnCounterTxt.text = turn.ToString();

                //toggle off Red rage ability
                foreach (TileManager t in AllTiles)
                {
                    t.Enraged = false;
                }

                //allows Red rage to be pressed again for Emperor Eternal Players
                if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.EmperorEternal)
                    RedRageOn();
            }
        }

        //checks mouse position
        Vector3 tooltippos = Input.mousePosition;
        tooltippos.x++;
        tooltippos.y++;

        //sets the tool tip text to the mouse Position
        ToolTip.transform.position = tooltippos;

        //opens the right UI for the game phase
        GamePhases[GamePhase].SetActive(true);

        switch (GamePhase)
        {
            case 0:
                //turns off incorrect UI
                GamePhases[1].SetActive(false);
                GamePhases[2].SetActive(false);

                break;

            case 1:
                //turns off incorrect UI
                GamePhases[0].SetActive(false);
                GamePhases[2].SetActive(false);

                break;

            case 2:
                //turns off incorrect UI
                GamePhases[0].SetActive(false);
                GamePhases[1].SetActive(false);

                //make sure this runs only once
                if (check2)
                {
                    //sets up all the tiles
                    foreach (TileManager T in AllTiles)
                    {
                        T.OnGameStart();
                    }

                    //make sure this doesn't run twice
                    check2 = false;

                    //on use abilities
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.EmperorEternal)
                    {
                        RedRageOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.BlackBlood)
                    {
                        EruptionOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.SaintHood)
                    {
                        PurgeNonBelieverOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.Ancestor)
                    {
                        DwarvenTunnelsOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.HighPantheon)
                    {
                        MakeExampleOn();
                    }
                    if (AllTiles[0].player.PlayingAs == PlayerManager.Cults.Ogdarism)
                    {
                        DedicationOn();
                    }

                }
                else
                {
                    //the players playermanager
                    var p = AllTiles[0].player;

                    //checks if they lost
                    if (p.ControlledTiles.Count == 0)
                    {

                        //tell them they lost
                        EndTxt.text = p.PlayingAsString + " is out of the game";
                        EndPanel.SetActive(true);

                        //make it so the other players are not waiting on them
                        if (!p.nextTurnReady)
                        {
                            EndTurn();
                        }
                    }
                    //checks if they won
                    if (p.ControlledTiles.Count >= 60 || PhotonNetwork.room.PlayerCount == 1)
                    {
                        //tell them they won
                        WinnerScreen();
                    }

                }
                //show game ui
                GameUI.SetActive(true);

                break;

        }

        //if start of game
        if (check && GamePhase > 0)
        {
            //counts how many players are ready
            int ii = 0;
            foreach (PlayerManager p in Players)
            {
                if (p.isReady)
                {
                    ii++;
                }
            }
            //if in a room
            if (PhotonNetwork.room != null)
            {
                // everyone is ready go onto the next game phase
                if (ii == Players.Count && ii != 0)
                {
                    check = false;
                    Players[0].pv.RPC("NextGamePhase", PhotonTargets.All, new object[] { });
                }
            }
        }

        //check if everyone is ready
        int i = 0;
        foreach (PlayerManager p in Players)
        {
            if (p.nextTurnReady)
            {
                i++;
            }
        }
        //checks that your still in a room
        if (PhotonNetwork.room != null)
        {  
            //if everyone is ready next turn
            if (i == Players.Count && i != 0)
            {
                ready = true;
                foreach (PlayerManager p in Players)
                {
                    p.nextTurnReady = false;
                }
            }
        }

        //if everyone is ready to go
        if (ready)
        {
            //start the end turn sequence for each player
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

    //end turn button pressed
    public void EndTurn()
    {
        // tell everyone the end turn but has been pressed
        var p = AllTiles[0].player;
        p.pv.RPC("NextTurn", PhotonTargets.All, new object[] { });
        p.gm.WaitingForPlayers.SetActive(true);
    }

    //turn to the next phase
    public void NextGamePhase()
    {
        if (GamePhase < 2)
            GamePhase++;
    }

    //makes it so you can't be the same culture as someone else
    public void ToggleCultureSelection()
    {
        foreach (Button b in CultureSelectionBut)
        {
            b.interactable = true;
        }

        foreach (GameObject g in CultureSelection)
        {
            g.SetActive(false);
        }
    }
}
