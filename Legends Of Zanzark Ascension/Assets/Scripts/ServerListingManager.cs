using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerListingManager : MonoBehaviour
{
    //the server listing prefab
    public GameObject ServerListing;

    //list of all the server listings
    public List<GameObject> ServerListings;

    //selected room
    public RoomInfo SelectedRoom;

    //refresh
    bool Refresh = false;

    // Start is called before the first frame update
    void Awake()
    {
        //refresh all the listings
        SelectedRoom = new RoomInfo();
        ListingRefresh();
        ToggleRefresh();
    }

    //toggles the current state of the refresh variable
    void ToggleRefresh()
    {
        if (Refresh)
            Refresh = false;
        else
            Refresh = true;
    }

    //refreshes the current server list
    public void ListingRefresh()
    {
        //itereates through the current server listings destroying them
        for (int i = ServerListings.Count; i > 0; i--)
        {
            GameObject g = ServerListings[i-1];
            ServerListings.Remove(g);
            Destroy(g);
        }

        //creates new server listings 
        foreach(RoomInfo r in PhotonNetwork.GetRoomList())
        {
            Debug.Log(r.Name);
            GameObject g = GameObject.Instantiate(ServerListing);
            ServerListing sl = g.GetComponent<ServerListing>();
            sl.room = r;
            ServerListings.Add(g);
            g.transform.SetParent(this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //refreshes the listings if need be
        if(Refresh != this.gameObject.activeSelf)
        {
            Awake();
        }
    }
}
