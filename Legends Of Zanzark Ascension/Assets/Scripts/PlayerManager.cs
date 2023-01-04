using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IPunObservable
{

    public int LoadingStatus;

    public Cults PlayingAs;

    public string PlayingAsString;

    public int InfluenceAvailable;
    public List<TileManager> ControlledTiles;
    public PhotonView pv;
    public GameManager gm;

    public bool isReady = false;

    public List<TileManager> LocalTileData;

    public bool nextTurnReady = false;
    
    [PunRPC]
    public void NextGamePhase()
    {
       gm.NextGamePhase();
    }

    [PunRPC]
    void EndTurn()
    {
        foreach(TileManager t in gm.AllTiles)
        {
            t.EndTurn();
        }
    }


    [PunRPC]
    void NextTurn()
    {
        nextTurnReady = true;
    }

    void Start()
    {
        ControlledTiles = new List<TileManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameManager.Players.Add(this);
        pv = this.GetComponent<PhotonView>();
        LocalTileData = gm.AllTiles;
    }

    public void SendData()
    {
        foreach (TileManager t in LocalTileData)
        {
            if (t.CurrentInfluenceOnTile[PlayingAsString] > 0)
                pv.RPC("RecieveTileData", PhotonTargets.Others, new object[] { t.name, PlayingAsString, t.CurrentInfluenceOnTile[PlayingAsString] });
        }

        if (pv.owner == PhotonNetwork.player)
        {
            LoadingStatus++;
        }
    }

    [PunRPC]
    public void RecieveTileData(string TileName, string PlayingAs, int influence)
    {
        LocalTileData.Find(tile => tile.name == TileName).CurrentInfluenceOnTile[PlayingAs] = influence;

    }

    // Update is called once per frame
    void Update()
    {

        LocalTileData = gm.AllTiles;

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

        if (PhotonNetwork.player == pv.owner)
        {
            gm.InfluenceAvailableTxt.text = "Influence Available: " + InfluenceAvailable.ToString();
            gm.TilesOwnedTxt.text = "Controlled Tiles: " + ControlledTiles.Count.ToString();
        }

        
    }


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

        StartofTurnSequence();

    }

    public void StartofTurnSequence()
    {
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
        InfluenceAvailable = ControlledTiles.Count + (int)mod;

    }

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
