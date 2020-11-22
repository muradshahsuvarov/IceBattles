using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    // string is for tag, so we have 2 teams Player and NPC. List holds all the members of that particular team -------------------TEAMS and PLAYERS-------------------
    static Dictionary<string, List<TacticsMove>> units = new Dictionary<string, List<TacticsMove>>();


    // This the currently active team. Who ever's turn it is. ------------------TEAMS NAMES-------------------
    static Queue<string> turnKey = new Queue<string>(); // e.g Player,NPC,Venom


    // Queue for the current team for who's turn it is. ------------------------PLAYERS' ORDER IN TEAMS---------------
    // static Queue<TacticsMove> turnTeam = new Queue<TacticsMove>(); FOR QUEUE INSTEAD OF LIST
    public static List<TacticsMove> turnTeam = new List<TacticsMove>();


    public static bool turnAllowed = false;

    public static string currentPlayerName;
    public string cpm;
    public static int currentPlayerIndex;
    public int cpi;
    public static string currentPlayerTag;

 

    public static bool choiceIsAllowed = true;
    public bool choiceIsAllowedCheck;

    public bool stopShowingWinner = false;

    public bool stopBones = false;

    public int TurnsLeft { get; set; }

    public static int GetNumberOfTeams()
    {
        int i = 0;
        foreach (var item in turnKey)
        {
            i++;
        }
        return i;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // Check how many teams are in the game
        int teamsCount = units.Count;
        if (teamsCount == 1 && !stopShowingWinner)
        {
            // Find the winner
            StringBuilder winner = new StringBuilder(String.Empty);
            foreach (KeyValuePair<string,List<TacticsMove>> item in units)
            {
                winner.Clear();
                winner.Append("Winner is " + item.Key);
            }
            // Display the winner
            Debug.Log(winner);
            stopShowingWinner = true;
        }

        

        cpi = currentPlayerIndex;
        cpm = currentPlayerName;

        // For the first run
        if (turnAllowed == false)
        {
            StartFirstTurn();
            InitTeamTurnQueue();
            foreach (var item in turnTeam)
            {
                Debug.Log("Player: " + item);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Creates a 3D ray from the point on the screen where the mouse was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            choiceIsAllowedCheck = choiceIsAllowed;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Player" && turnKey.Peek().Contains("Player") && choiceIsAllowed == true) // If Player is Active and clicked object's tag is Player then do the following
                {
                    turnAllowed = false;
                    turnTeam[currentPlayerIndex].stopCurrentTileChecking = false; // To enable auto current tile type checking after passing the turn to another team
                    turnTeam[currentPlayerIndex].EndTurn(); // Chooses the next player
                    

                    currentPlayerName = hit.transform.gameObject.name; // Hit object's name
                    Debug.Log("Player:" + currentPlayerName + " is clicked!");
                    int i = 0;
                    while (turnTeam[i].gameObject.name != currentPlayerName && i < turnTeam.Count)
                    {
                        i++;
                    }
                    currentPlayerIndex = i; // Index of the clicked player 
                }
                else if (hit.collider.tag == "NPC" && turnKey.Peek().Contains("NPC") && choiceIsAllowed == true) // If NPC is Active and clicked object's tag is NPC then do the following
                {
                    turnAllowed = false;
                    turnTeam[currentPlayerIndex].stopCurrentTileChecking = false; // To enable auto current tile type checking after passing the turn to another team
                    turnTeam[currentPlayerIndex].EndTurn(); // Chooses the next player
                    
                    currentPlayerName = hit.transform.gameObject.name; // Hit object's name
                    Debug.Log("NPC:" + currentPlayerName + " is clicked!");
                    int i = 0;
                    while (turnTeam[i].gameObject.name != currentPlayerName && i < turnTeam.Count)
                    {
                        i++;
                    }
                    currentPlayerIndex = i;  // Index of the clicked player
                    Debug.Log("COUNT: " + turnTeam.Count);
                }
                else if (hit.collider.tag == "Venom" && turnKey.Peek().Contains("Venom") && choiceIsAllowed == true) // If NPC is Active and clicked object's tag is NPC then do the following
                {
                    turnAllowed = false;
                    turnTeam[currentPlayerIndex].stopCurrentTileChecking = false; // To enable auto current tile type checking after passing the turn to another team
                    turnTeam[currentPlayerIndex].EndTurn(); // Chooses the next player

                    currentPlayerName = hit.transform.gameObject.name; // Hit object's name
                    Debug.Log("NPC:" + currentPlayerName + " is clicked!");
                    int i = 0;
                    while (turnTeam[i].gameObject.name != currentPlayerName && i < turnTeam.Count)
                    {
                        i++;
                    }
                    currentPlayerIndex = i;  // Index of the clicked player

                }

               
            }

        }


    }

    // Initializes next team
    static void InitTeamTurnQueue()
    {
        
            List<TacticsMove> teamList = new List<TacticsMove>(); // Players in the team
            bool stop = false;


        string key = "";
        // Manages switching from one team to another in case of one team is empty , so list is empty
        foreach (string item in turnKey)
        {
           
                if (units[item].Count != 0 && stop == false)
                {
                    teamList = units[item];
                    stop = true;
                }
            else if(units[item].Count == 0) // If the team's memebers count is 0 , then delete them from units and turnKey
            {
                key = item; // Deleted team's key
                units.Remove(item);
                Debug.Log("ITEM " + item + " is deleted");

                //Delete this key whose players count is 0
                bool stopSearching = false;
                while (stopSearching != true)
                {
                    string dKey = turnKey.Dequeue();
                    if (dKey != key)
                    {
                        turnKey.Enqueue(dKey);
                    }
                    else
                    {
                        stopSearching = true;
                    }
                }

            }
        }


            turnTeam = new List<TacticsMove>();
        if (teamList.Count == 0)
        {
            Debug.Log("TEAM LIST IS NULL ");
        }
        foreach (var item in teamList)
        {
            Debug.Log("TEAM LIST: " + item);
        }
        /* FOR QUEUE INSTEAD OF LIST
        foreach (TacticsMove unit in teamList)
        {
            turnTeam.Enqueue(unit); // Filling the team with team members
        } */

            foreach (TacticsMove unit in teamList)
            {
                Debug.Log($"PLAYER: {unit.name} ADDED!");
                turnTeam.Add(unit); // Filling the team with team members
            }

        try
        {
            StartTurn();
        }
        catch (Exception e)
        {
            Debug.Log("ERROR(InitTeamTurnQueue): " + e);
        }
            
   
    }

    // Starts turn of the chosen team from InitTeamTurnQueue()
    public static void StartTurn()
    {
        /* FOR QUEUE INSTEAD OF LIST
        if (turnTeam.Count > 0)
        {
            turnTeam.Peek().BeginTurn(); // Chooses the next player
        } */
           
            turnAllowed = true;

        bool existDrowner = false;
        List<int> indexesOfDrowners = new List<int>();
        for (int i = 0; i < turnTeam.Count; i++)
        {
            if (turnTeam[i].isDrowning == true)
            {
                indexesOfDrowners.Add(i);
                existDrowner = true;
            }
        }

        if (existDrowner == true) // If there is a drowner in the team
        {
            bool findFigure = false;
            for (int i = 0; i < indexesOfDrowners.Count; i++)
            {
                if (currentPlayerIndex == indexesOfDrowners[i])
                {
                    findFigure = true;
                    break;
                }
            }

            if (findFigure == true)
            {
                for (int i = 0; i < turnTeam.Count; i++)
                {
                    if (turnTeam[i].isDrowning == false)
                    {
                        currentPlayerIndex = i;
                        break;
                    }
                }

                findFigure = false;
            }
            turnTeam[currentPlayerIndex].iChose = false;
            turnTeam[currentPlayerIndex].BeginTurn(); // Chooses the next player
        }
        else // Else there is not a drowner in the team
        {

            // FIX THIS IF AND ELSE LOGIC LATER ON , BECAUSE IT IS MESSY
            if (turnTeam.Count != 2) // 2 can be changed to the intial # of figures in the team 
            {
                currentPlayerIndex = 0;
                turnTeam[currentPlayerIndex].iChose = false;
                turnTeam[currentPlayerIndex].BeginTurn(); // Chooses the next player
            }
            else // If there is 2 players. 2 means all. Later can be changed to N - number
            {
                choiceIsAllowed = true;
                turnTeam[currentPlayerIndex].iChose = false;
                turnTeam[currentPlayerIndex].BeginTurn(); // Chooses the next player
            }
        }

        
      
    }

    public static void EndTurn()
    {
            
            turnAllowed = false;
            TacticsMove unit = turnTeam[currentPlayerIndex];
            unit.EndTurn();


            if (turnAllowed == true) // Next guy gets its turn
            {
                StartTurn();
            }
            else // Or load next team and the first guy gets its turn
            {
            
                // STEP 1: 1|2|3 -> 2|3 -> 2|3|1
                string team = turnKey.Dequeue();
                turnKey.Enqueue(team);
            try // HERE WE GET INDEX OUT OF RANGE IF WE WANT TO SWITCH TO THE TEAM WHICH WAS DELETED
            {
                InitTeamTurnQueue();
            }
            catch (Exception e)
            {
                Debug.Log("ERROR(EndTurn): " + e);
            }
        }
        
        
        
    }


    public static void AddUnit(TacticsMove unit)
    {
        List<TacticsMove> list;

        if (!units.ContainsKey(unit.tag)) // IF THE DICTIONARY DOESN'T HAVE THE TEAM WITH THE ENTERED TAG AS KEY , THEN THIS TEAM IS ADDED TO THE DICTIONARY
        {
            list = new List<TacticsMove>();
            units[unit.tag] = list;

            if (!turnKey.Contains(unit.tag)) // IF SUCH TEAM WITH THE TAG DOESN'T EXIST , THEN THE TEAM'S TAG IS ADDED 
            {
                turnKey.Enqueue(unit.tag);  // TAG OF THE PLAYER IS ADDED HERE
                
            }
        }
        else // IF THE PLAYER WITH THE SAME KEY IS ADDED THEN THE LIST IS COPIED AND AFTER ADDED TO THE QUEUE
        {
            list = units[unit.tag];
        }

        list.Add(unit);

    }

    public static void RemoveUnit(TacticsMove unit)
    {
        List<TacticsMove> list = units[unit.tag];
        list.Remove(unit);
        Debug.Log("Player: " +  unit.gameObject.name + " is deleted");
        units[unit.tag] = list;
        Debug.Log("New Team members");
        foreach (var item in units[unit.tag])
        {
            Debug.Log(item);
        }
        if (units[unit.tag].Count == 0)
        {
            //SWITCH TO ANOTHER TEAM
            // Ending the turn when movement end. It can be pasted to the action part, so considered as action.
         
            EndTurn();
        }
        
    }

    // Start the turn of randomly choisen team at the begging of the game
    void StartFirstTurn()
    {
        Debug.Log($"TEAMS SIZE: {units.Count}");
        System.Random rand = new System.Random();
        if (!stopBones)
        {
            // Gives the max number of players.
            // If team = 1 , so second team must be selected E.g NPC
            int team = rand.Next(1, units.Count+1);
            Debug.Log($"TEAM NUMBER: {team}");
            for (int i = 1; i <= team; i++) // Player,NPC,Venom -> NPC,Venom,Player -> NPC starts its turn
            {
                string tempTeam = turnKey.Dequeue();
                turnKey.Enqueue(tempTeam);
            }
            stopBones = true;
        }
    }

    public void pickItem()
    {
        // If there is something on the tile , then pick it
        turnTeam[currentPlayerIndex].pickItem();
    }

    public void SwitchItem()
    {
        turnTeam[currentPlayerIndex].SwitchItem();
    }

    public void UseItem()
    {
        turnTeam[currentPlayerIndex].UseItem();
    }

    public void FindReceivers()
    {
        turnTeam[currentPlayerIndex].FindReceivers();
    }

    public void SendItem()
    {
        turnTeam[currentPlayerIndex].SendItem();
    }

    public void putIntoCrate()
    {
        turnTeam[currentPlayerIndex].putIntoCrate();
    }

    public void getFromCrate()
    {
        turnTeam[currentPlayerIndex].getFromCrate();
    }

    public void ShowCrate()
    {
        turnTeam[currentPlayerIndex].showCrate();
    }

    public void Rescue()
    {
        turnTeam[currentPlayerIndex].Rescue();
    }

    public void Restart()
    {
        SceneManager.LoadScene("Antarctida 1");
    }
}
