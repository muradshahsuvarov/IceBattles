using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMove : TacticsMove
{

    public static string currentTeamName;
    public string StayedTile;
    public string Name;

    GameItem currentItem;


    // Start is called before the first frame update
    void Start()
    {
        Name = this.gameObject.name;
        isBot = true;
        Init();
    }

}
