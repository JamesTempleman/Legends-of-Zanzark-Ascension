using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ServerListing : MonoBehaviour, IPointerClickHandler
{
    //the info of the room in question
    public RoomInfo room;

    //room data
    public string ServerName;
    public string PlayerCount;
    public int PlayerCountint;
    public int PlayerCountIntMax;
    public string PasswordRequired;
    public bool PasswordRequiredBool;

    //text objects so the player can see the room info
    public TextMeshProUGUI ServerNameTxt;
    public TextMeshProUGUI PlayerCountTxt;
    public TextMeshProUGUI PasswordRequiredTxt;

    //when clicking this listing
    public void OnPointerClick(PointerEventData eventData)
    {
        //tell the server listing manager that this has been selected
        GameObject.Find("Content").GetComponent<ServerListingManager>().SelectedRoom = room;
    }

    // Update is called once per frame
    void Update()
    {
        //make it so if selected you can't select this twice more of a visual to tell the player what they have chosen
        if(GameObject.Find("Content").GetComponent<ServerListingManager>().SelectedRoom == room)
        {
            this.GetComponent<Button>().interactable = false;
        }
        else
        {
            this.GetComponent<Button>().interactable = true;
        }

        //sets variables to the servers details
        ServerName = room.Name;
        PlayerCountint = room.PlayerCount;
        PlayerCountIntMax = room.MaxPlayers;

        //tell the player if a password is needed
        if (room.CustomProperties["P"] != null)
        {
            if (room.CustomProperties["P"].ToString().Length > 1)
                PasswordRequiredBool = true;
            else
                PasswordRequiredBool = false;
        }

        if (PasswordRequiredBool)
            PasswordRequired = "Needs Password";
        else
            PasswordRequired = "Open To Join";

        //how many players are on this server
        PlayerCount = PlayerCountint.ToString() + "/" + PlayerCountIntMax.ToString();

        //set the text variables so the player knowns this servers details
        ServerNameTxt.text = ServerName;
        PlayerCountTxt.text = PlayerCount;
        PasswordRequiredTxt.text = PasswordRequired;

    }
}
