using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    public List<PlayerManager> Players;
    public Button[] SelectBut;

    public PlayerManager player;

    public Button ComfirmBut;

    public int SelectedCult;

    public GameObject CoverPanel;

    // Start is called before the first frame update
    void Start()
    {
        var p = PhotonNetwork.Instantiate("PlayerManager", new Vector3(0, 0, 0), new Quaternion(), 0);
        player = p.GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Players = GameManager.Players;
    }

    public void SelectCult(int CultId)
    {
        SelectedCult = CultId;

        foreach(Button b in SelectBut)
        {
            b.interactable = true;
        }

        ComfirmBut.gameObject.SetActive(true);
        player.PlayingAs = (PlayerManager.Cults)SelectedCult;

    }

    public void ComfirmChoice()
    {
        player.isReady = true;
        CoverPanel.SetActive(true);
    }

}
