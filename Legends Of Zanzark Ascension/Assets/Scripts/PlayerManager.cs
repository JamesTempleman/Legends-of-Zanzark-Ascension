using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IPunObservable
{
    public Cults PlayingAs;

    public string PlayingAsString;

    public int InfluenceAvailable;
    public List<TileManager> ControlledTiles;
    public PhotonView pv;
    public GameManager gm;

    public bool isReady = false;

    public List<TileManager> LocalTileData;

    public bool nextTurnReady = false;

    void Start()
    {
        ControlledTiles = new List<TileManager>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        GameManager.Players.Add(this);
        pv = this.GetComponent<PhotonView>();
        LocalTileData = gm.AllTiles;
    }

    public void SendData(bool b = true)
    {
        foreach (TileManager t in LocalTileData)
        {
            pv.RPC("RecieveTileData", PhotonTargets.Others, new object[] { t.name, PlayingAsString, t.CurrentInfluenceOnTile[PlayingAsString] });
        }
        nextTurnReady = b;
        gm.WaitingForPlayers.SetActive(b);
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

    [PunRPC]
    public void RecieveTileData(string TileName, string PlayingAs, int influence)
    {
        LocalTileData.Find(tile => tile.name == TileName).CurrentInfluenceOnTile[PlayingAs] = influence;
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
            Debug.Log("Abora");
            foreach (TileManager t in ControlledTiles)
            {
                if (t.InfluenceValueInt > 2)
                {
                    mod = mod + (t.InfluenceValueInt % 3);
                }
            }
            Debug.Log(mod.ToString());
        }
            InfluenceAvailable = ControlledTiles.Count + (int)mod;

        Debug.Log(InfluenceAvailable.ToString());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(nextTurnReady);
            stream.SendNext(isReady);
            stream.SendNext(PlayingAs);

            stream.SendNext(InfluenceAvailable);

        }
        if (stream.isReading)
        {
            nextTurnReady = (bool)stream.ReceiveNext();
            isReady = (bool)stream.ReceiveNext();
            PlayingAs = (Cults)stream.ReceiveNext();

            InfluenceAvailable = (int)stream.ReceiveNext();

        }
    }
}
