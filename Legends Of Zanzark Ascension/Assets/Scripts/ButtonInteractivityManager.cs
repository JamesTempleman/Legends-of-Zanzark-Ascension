using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractivityManager : MonoBehaviour
{
    //the id of the cult this button belongs to
    public int CultID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //turns off button if anyone is playing as this cult
        if (GameManager.Players.FindAll(p => p.PlayingAs == (PlayerManager.Cults)CultID).Count > 0)
        {
            gameObject.GetComponent<Button>().interactable = false;
        }
        else
        {
            gameObject.GetComponent<Button>().interactable = true;
        }


    }
}
