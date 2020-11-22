using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlizzardManager : MonoBehaviour
{

    public GameObject Map;
    public int TurnsLeft;

    // Start is called before the first frame update
    void Start()
    {
        ResetTurns();
    }

    void BlizzardOccurs()
    {
        Debug.Log("BLIZZARD OCCURS!!!");
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject item in tiles)
        {
            int blizzardAllowed = Random.RandomRange(0, 2);
            if (blizzardAllowed == 1) // Blizzard allowed
            {
                item.GetComponent<Tile>().snowLevel = item.GetComponent<Tile>().snowLevel + 2;
            }
        }

    }

    void ResetTurns()
    {
        // SETTING UP TURNS LEFT # TILL BLIZZARD OCCURS
        Map = GameObject.FindGameObjectWithTag("Map");
        Map.GetComponent<TurnManager>().TurnsLeft = 10;
        TurnsLeft = Map.GetComponent<TurnManager>().TurnsLeft;
    }

    // Update is called once per frame
    void Update()
    {
        TurnsLeft = Map.GetComponent<TurnManager>().TurnsLeft;
        // <= 0 , because if we have 2 fast turns which will decr. 2 then TurnsLeft has to consider -1 as a blizzard occurs
        if (TurnsLeft <= 0)
        {
            // Calling Blizzard
            BlizzardOccurs();
            // Reseting the # of TurnsLeft
            ResetTurns();
        }
    }
}
