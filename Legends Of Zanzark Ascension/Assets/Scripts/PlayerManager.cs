using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IPunObservable
{
    //this players current loading status
    public int LoadingStatus;

    //what they are playing as
    public Cults PlayingAs;
    public string PlayingAsString;

    //influence available and all currently controlled tiles
    public int InfluenceAvailable;
    public List<TileManager> ControlledTiles;

    //the photonview attached to this object
    public PhotonView pv;

    //the gamemanager
    public GameManager gm;

    //if the player is ready
    public bool isReady = false;

    //this players local tile data
    public List<TileManager> LocalTileData;

    //if has ended there turn
    public bool nextTurnReady = false;
    
    //if someone has destroyed this tile
    [PunRPC]
    void DestroyTile(string s)
    {
        //destroy this tile
        LocalTileData.Find(tile => tile.name == s).isDestroyed = true;
    }

    //go to the next game phase
    [PunRPC]
    public void NextGamePhase()
    {
       gm.NextGamePhase();
    }

    //end ur turn can be called by anyone
    [PunRPC]
    void EndTurn()
    {
        //tell ur tiles that ur ending ur turn
        foreach(TileManager t in gm.AllTiles)
        {
            t.EndTurn();
        }
    }

    //ready up for next turn can be called by anyone
    [PunRPC]
    void NextTurn()
    {
        nextTurnReady = true;
    }

    //on start
    void Start()
    {
        //set up some basic stuff for later
        ControlledTiles = new List<TileManager>(); 
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameManager.Players.Add(this);
        pv = this.GetComponent<PhotonView>();
        LocalTileData = gm.AllTiles;
    }

    //clear your local players list can be called by anyone
    [PunRPC]
    void ClearPlayers()
    {
        GameManager.Players.Clear();
    }

    //update ur player list by adding your self can be called by anyone
    [PunRPC]
    void UpdatePlayers()
    {
        GameManager.Players.Add(this);
    }

    //send all your tile data
    public void SendData()
    {
        foreach (TileManager t in LocalTileData)
        {
            if (t.CurrentInfluenceOnTile[PlayingAsString] > 0)
                pv.RPC("RecieveTileData", PhotonTargets.Others, new object[] { t.name, PlayingAsString, t.CurrentInfluenceOnTile[PlayingAsString], t.isLandMark, t.isDestroyed });
        }

        if (pv.owner == PhotonNetwork.player)
        {
            LoadingStatus++;
        }
    }

    //recieve tile data from others
    [PunRPC]
    public void RecieveTileData(string TileName, string PlayingAs, int influence, bool landmark, bool destroyed)
    {
        LocalTileData.Find(tile => tile.name == TileName).CurrentInfluenceOnTile[PlayingAs] = influence;
        
        if (destroyed)
            LocalTileData.Find(tile => tile.name == TileName).isDestroyed = destroyed;
        if (!landmark)
            LocalTileData.Find(tile => tile.name == TileName).isLandMark = landmark;
    }

    // Update is called once per frame
    void Update()
    {
        //yoour local tile data is the same as the data in the gamemanager
        LocalTileData = gm.AllTiles;

        //set the PlayasString variable
        switch (PlayingAs)
        {
            case Cults.None:
                PlayingAsString = "None";
                break;
            case Cults.SaintHood:
                PlayingAsString = "SaintHood";
                break;
            case Cults.Ancestor:
                PlayingAsString = "Ancestor";
                break;
            case Cults.HighPantheon:
                PlayingAsString = "HighPantheon";
                break;
            case Cults.DarkPantheon:
                PlayingAsString = "DarkPantheon";
                break;
            case Cults.Elemental:
                PlayingAsString = "Elemental";
                break;
            case Cults.BlackBlood:
                PlayingAsString = "BlackBlood";
                break;
            case Cults.Ogdarism:
                PlayingAsString = "Ogdarism";
                break;
            case Cults.LegionsofAboracrom:
                PlayingAsString = "LegionsofAboracrom";
                break;
            case Cults.EmperorEternal:
                PlayingAsString = "EmperorEternal";
                break;
        }

        //if u are this player
        if (PhotonNetwork.player == pv.owner)
        {
            //for resource bar
            gm.InfluenceAvailableTxt.text =  InfluenceAvailable.ToString();
            gm.TilesOwnedTxt.text =  ControlledTiles.Count.ToString();
        }

        
    }

    //all playable cultures in enum form
    public enum Cults
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

    //updates what each player has control of at the very end of the turn
    public void EndofTurnUpdate()
    {
        ControlledTiles.Clear();

        foreach(TileManager t in gm.AllTiles)
        {
            if(t.ControlledBy == PlayingAsString)
            {
                ControlledTiles.Add(t);
            }
        }

        //starts the next turn
        StartofTurnSequence();

    }

    //start the next turn
    public void StartofTurnSequence()
    {
        //any modifications that the faction has to what influence u start with each turn
        float mod = 0;

        //bonus influence for Legions of Aboracrom if there are many points on location
        if (PlayingAs == Cults.LegionsofAboracrom)
        {
            foreach (TileManager t in ControlledTiles)
            {
                if (t.InfluenceValueInt > 2)
                {
                    mod = mod + (t.InfluenceValueInt % 3);
                }
            }
        }
        //if a tile is a certain ogdar clan gain more points
        if (PlayingAs == Cults.Ogdarism)
        {
            foreach (TileManager t in ControlledTiles)
            {
                if (t.OgdarClanId == 1)
                {
                    mod = mod + 1;
                }
            }
        }

        //set ur influence points
        InfluenceAvailable = ControlledTiles.Count + (int)mod;

    }

    //sync up some data across the network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(LoadingStatus);

            stream.SendNext(isReady);
            stream.SendNext(PlayingAs);

            stream.SendNext(InfluenceAvailable);

        }
        if (stream.isReading)
        {
            LoadingStatus = (int)stream.ReceiveNext();

            isReady = (bool)stream.ReceiveNext();
            PlayingAs = (Cults)stream.ReceiveNext();

            InfluenceAvailable = (int)stream.ReceiveNext();
        }
    }
}
