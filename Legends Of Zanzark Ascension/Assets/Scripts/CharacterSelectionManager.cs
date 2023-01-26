using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    //list of players
    public List<PlayerManager> Players;

    //all the selection buttons for the cultures
    public Button[] SelectBut;

    // the player
    public PlayerManager player;

    //lock in your choice
    public Button ComfirmBut;

    //the id of the selected culture
    public int SelectedCult;

    // Start is called before the first frame update
    void Start()
    {
        //make player managers
        var p = PhotonNetwork.Instantiate("PlayerManager", new Vector3(0, 0, 0), new Quaternion(), 0);
        player = p.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //gets there list of players from the gamemanager
        Players = GameManager.Players;
    }

    public void SelectCult(int CultId)
    {
        //sets the selected cult by id
        SelectedCult = CultId;

        //makes sure other people can select there culture but not this one
        foreach(Button b in SelectBut)
        {
            b.interactable = true;
        }

        //gives option for player to lock in there choice
        ComfirmBut.gameObject.SetActive(true);

        //sets the cult the player is playing as
        player.PlayingAs = (PlayerManager.Cults)SelectedCult;

    }

    //confirm the choice
    public void ComfirmChoice()
    {
        player.isReady = true;
    }

}
