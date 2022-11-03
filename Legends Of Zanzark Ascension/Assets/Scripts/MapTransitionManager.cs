using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransitionManager : MonoBehaviour
{
    [SerializeField] List<GameObject> Maps;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void TransitionMap(int mapID)
    {
        foreach(GameObject g in Maps)
        {
            g.SetActive(false);
        }

        Maps[mapID].SetActive(true);
    }

}
