using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove : MonoBehaviour
{
    public bool turn = false;
    

    List<Tile> selectableTiles = new List<Tile>();
    GameObject[] tiles;

    Stack<Tile> path = new Stack<Tile>();
    protected static Tile currentTile; // Has to be changed to local variable when game will be online


    public bool moving = false;
    public int move = 5;
    public float jumpHeight = 2;
    public float moveSpeed = 2;
    public float jumpVelocity = 4.5f;

    Vector3 velocity = new Vector3(); // How fast player moves from tile to tile
    Vector3 heading = new Vector3(); // Direction where player heading in

    float halfHeight = 0;

    bool fallingDown = false;
    bool jumpingUp = false;
    bool movingEdge = false;
    Vector3 jumpTarget;

    public bool stopCurrentTileChecking = false;

    public List<GameItem> items;
    public List<string> MyItems;


    GameItem currentItem;
    public int ChosenItemIndex = 0;
    public int SendingItemIndex = 0;
    public List<string> itemReceivers;
    public int receiverIndex = 0;

    public int Health = 5;

    public string currentItemName;

    public int crateItemIndex;
    public int putIntoCrateIndex;

    public GameObject Map;

    public bool isBot = false;
    public GameObject target;
    public Tile targetTile;

    public Tile actualTargetTile;

    // BOT GOAL
    public bool Attack = false;
    public bool Protect = false;
    public bool Survive = false;
    public bool Explore = false;

    // BOT SENSORS
    public bool isAttacked = false;
    public bool isDrowning = false;
    public bool isInTrap = false;

    // BOT CHOICE
    public bool iChose = false;

    // STOP CONDITION FOR CALLING FindSelectableTile() method.
    bool callFindSelectableTile;

    // Initialization at the beginning of the scene
    protected void Init()
    {
        Map = GameObject.FindGameObjectWithTag("Map");

        items = new List<GameItem>();
        MyItems = new List<string>();
        itemReceivers = new List<string>();
        tiles = GameObject.FindGameObjectsWithTag("Tile");

        halfHeight = GetComponent<Collider>().bounds.extents.y;

        // ADD DEFAULT SHOVEL FOR EVERY PLAYER
        Shovel shovel = new Shovel();
        shovel.Name = "Shovel";
        shovel.Attackable = true;
        items.Add(shovel);
        MyItems.Add("Shovel");
        currentItem = items[0];
        // ADD DEFAULT SHOVEL FOR EVERY PLAYER

        // We are passing this current TacticsMove object as a parameter. Adding current unit to the TurnManager
        TurnManager.AddUnit(this);
        
    }

    // Finding the tile among selectable tiles which is the closest one to the target tile. Written by Murad Shahsuvarov.
    Tile GetNearestWalkableTile(GameObject target)
    {
        Tile targetTile = GetTargetTile(target); // Tile of the nearest player
        Tile chosenTile = null;
        List<float> distances = new List<float>();

        foreach (Tile tile in selectableTiles)
        {
            if (tile.getType() == "Iceberg")
            {
                distances.Add(Vector3.Distance(tile.transform.position,targetTile.transform.position)); // We get distances between each tile and the target tile
            }
        }

        // Sort the distances
        distances.Sort();
        float minimumDistance = distances[0];

        foreach (Tile tile in selectableTiles)
        {
            if (tile.getType() == "Iceberg")
            {
                if (Vector3.Distance(tile.transform.position,targetTile.transform.position) == minimumDistance)
                {
                    chosenTile = tile;
                    break;
                }
            }
        }

        return chosenTile;
    }

    // Finding Iceberg End Tile
    protected Tile FindEndTile(Tile t) // t - tile of the target
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent; // stay next to parent
        while (next != null) // calc path going all the way back
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move)
        {
            return t.parent;
        }

        List<Tile> endTiles = new List<Tile>();
        List<Tile> endIcebergs = new List<Tile>();
        List<float> distances = new List<float>();
        Tile endTile = null;
        Tile tmp = null;
        for (int i = 0; i <= move; i++)
        {
            //  endTile = tempPath.Pop(); // Stay next to parent, grab that
            endTiles.Add(tempPath.Pop());
        }



        for (int i = 0; i < endTiles.Count; i++)
        {
            if (endTiles[i].getType() == "Iceberg")
            {
                Debug.Log($"ICEBERG TYPE: {endTiles[i].getType()}");
                endIcebergs.Add(endTiles[i]);
            }
        }



        for (int i = 0; i < endIcebergs.Count; i++)
        {
            distances.Add(Vector3.Distance(endIcebergs[i].transform.position, target.transform.position));
        }

        distances.Sort();

        float min = 0.0f;
        if (distances.Count != 0)
        {
            min = distances[0];
            for (int i = 0; i < endIcebergs.Count; i++)
            {
                if (Vector3.Distance(endIcebergs[i].transform.position, target.transform.position) == min)
                {
                    endTile = endIcebergs[i];
                    break;
                }
            }
        }
        else
        {
            TurnManager.EndTurn();
        }



        if (endTile.transform.position == target.transform.position)
        {
            TurnManager.EndTurn();
        }

        Debug.Log($"END TILE TYPE: {endTile.getType()}");
        return endTile;

    } 

    void CalculatePath()
    {
        Tile targetTile = null;
        if (Attack == true)
        {
            targetTile = GetTargetTile(target); // Tile of the nearest player
        } else if (Protect == true || Explore == true)
        {
            targetTile = target.GetComponent<Tile>();
        }

        FindPath(targetTile);
    }

    protected Tile FindLowestF(List<Tile> list)
    {
        Tile lowest = list[0];

        foreach (Tile t in list)
        {
            if (t.f < lowest.f) // if true, then t becomes next lowest
            {
                lowest = t;
            }
        }

        list.Remove(lowest);


        return lowest;
    }

    // Finding Any End Tile

        /*
    protected Tile FindEndTile(Tile t) // t - tile of the target
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent; // stay next to parent
        while (next != null) // calc path going all the way back
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move)
        {
            return t.parent;
        }

        Tile endTile = null;
        for (int i = 0; i <= move; i++)
        {
            endTile = tempPath.Pop(); // Stay next to parent, grab that
        }

        return endTile;

    } */

    protected void FindPath(Tile target)
    {
        Debug.Log($"FindPath Target {target.gameObject.GetComponent<Tile>().ID}");
        ComputeAdjacencyList(jumpHeight, target);

            GetCurrentTile();


        List<Tile> openList = new List<Tile>(); // Any tiles that have not been processed
        List<Tile> closedList = new List<Tile>(); // Any tiles that have been processed

        openList.Add(currentTile);
        //currentTile.parent = ??
        // Sqr.Magnitude can be used instead of Distance to make the algorithm faster
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        while (openList.Count > 0)
        {
            Tile t = FindLowestF(openList); // find one with the lowest F cost

                closedList.Add(t);
            

          
            if (t == target) // Our goal
            {
                  
                actualTargetTile = FindEndTile(t);
                MoveToTile(actualTargetTile);
                return;
            }
            else
            {
                Debug.Log($"{gameObject.name} , T = {t.name}  NOT EQUAL TO TARGET = {target.name} AZERBAIJAN");
            }

            foreach (Tile tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    //Do nothing, already processed
                }
                else if(openList.Contains(tile))
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                }else
                {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h;

                    openList.Add(tile); // Meaning it is processed , and potentially gonna be processed in the future
                }


            }


        }

        //todo - what do you do if there is no path to the target tile?
        Debug.Log("Path not found");



    }

    void FindFarestTarget()
    {
            List<Tile> selTiles = selectableTiles;

        //Find the farest tile relative among selectable ones
        List<float> distances = new List<float>();

        GameObject[] MyTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (var myTile in MyTiles)
        {
            foreach (var selTile in selTiles)
            {
                if (myTile.GetComponent<Tile>().ID == selTile.ID && myTile.gameObject.GetComponent<Tile>().getType() == "Iceberg"
                    && Vector3.Distance(myTile.transform.position,FindNearestPlayer().transform.position) != 0)
                {
                    distances.Add(Vector3.Distance(transform.position, myTile.transform.position));
                }
            }
        }

        if (distances.Count == 0)
        {
            TurnManager.EndTurn();
        }

        distances.Sort();
        float maxDist = distances[ distances.Count - 1];

        GameObject farest = null; // Eventual target

        foreach (GameObject myTile in MyTiles)
        {
            if (Vector3.Distance(transform.position, myTile.transform.position) == maxDist)
            {
                Debug.Log($"I FOUND THE FAREST {myTile.GetComponent<Tile>().ID}");
                farest = myTile;
                break;
            }
        }


            target = farest; // Then target is used to get the current location
    }

    void FindExplorableTile()
    {

        //Get all tiles
        GameObject[] tiles_gm = GameObject.FindGameObjectsWithTag("Tile");
        List<Tile> selectableTiles_0 = selectableTiles;
        List<int> selTilesIDs = new List<int>();
        foreach (Tile item in selectableTiles_0)
        {
            if (item.getType() == "Iceberg")
            {
                selTilesIDs.Add(item.GetComponent<Tile>().ID);
                Debug.Log($"SEL TYPE IS { item.GetComponent<Tile>().getType() }, SEL TYPE ID { item.GetComponent<Tile>().ID}");
            }
        }

        if (selTilesIDs.Count != 0)
        {
            int selTileID = UnityEngine.Random.Range(0, selTilesIDs.Count);
            foreach (GameObject tile_gm in tiles_gm)
            {
                if (tile_gm.GetComponent<Tile>().ID == selTileID)
                {
                    target = tile_gm;
                    Debug.Log($"{gameObject.name} EXPLORABLE TILE IS FOUND!!! TARGET ID: {target.GetComponent<Tile>().ID}," +
                        $"TARGET TYPE: {target.GetComponent<Tile>().getType()}");
                    break;
                }
            }

            }
        else
        {
            Debug.Log($"{gameObject.name}: Couldn't find tile to explore");
            TurnManager.EndTurn();
        }
    }

    // Finds our target
    void FindNearestTarget()
    {
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

            GameObject nearest = null; // Eventual target
            float distance = Mathf.Infinity; // Everything after this is smaller

            foreach (GameObject obj in targets)
            {
            if (obj.GetComponent<TacticsMove>().isDrowning == false)
            {
                float d = Vector3.Distance(transform.position, obj.transform.position);

                if (d < distance) // If true , then the object is closer
                {
                    distance = d;
                    nearest = obj;
                }
            }

            }

            target = nearest; // Then target is used to get the current location
    }

    // Finds our target
    GameObject FindNearestPlayer()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null; // Eventual target
        float distance = Mathf.Infinity; // Everything after this is smaller

        foreach (GameObject obj in targets)
        {
            if (obj.GetComponent<TacticsMove>().isDrowning == false)
            {
                float d = Vector3.Distance(transform.position, obj.transform.position);

                if (d < distance) // If true , then the object is closer
                {
                    distance = d;
                    nearest = obj;
                }
            }
        }

        target = nearest; // Then target is used to get the current location
        return target;
    }


    public bool updateTilesAllowed;

    public void Update()
    {

        if (currentTile != null)
        {
            if (updateTilesAllowed == true)
            {

                Debug.Log($"{gameObject.name}: updateTiles(), curTile = {currentTile.getType()}");
                updateTiles();


                    updateTilesAllowed = false;
            }
        }



        if (isBot == false)
        {
            

            RaycastHit hit;
            Debug.DrawRay(gameObject.transform.position, Vector3.forward * 2, Color.red);

            // If its not their turn , then it means that they are dissabled even though they are still in the scene
            if (!turn)
            {
                checkAliviness();
                return;
            }

            if (!moving)
            {
                if (callFindSelectableTile == true)
                {
                    FindSelectableTiles();
                    callFindSelectableTile = false;
                }
                CheckMouse();
            }
            else
            {

                Move();
            }
        }
        else if(isBot == true){


            // Choose random action
            int randomAction = UnityEngine.Random.Range(0,4);
            if (randomAction == 0 && iChose == false)
            {
                Debug.Log($"{gameObject.name}: Attack mod");
                  Attack = true;
                  Protect = false;
            //      Survive = false;
                  Explore = false;

                iChose = true;

            }
            else if (randomAction == 1 && iChose == false)
            {
                Debug.Log($"{gameObject.name}: Protect mod");
                Attack = false;
                Protect = true;
            //    Survive = false;
                Explore = false;

                iChose = true;
            }
            else if (randomAction == 2 && iChose == false)
            {
                Debug.Log($"{gameObject.name}: Explore mod");
                Attack = false;
                Protect = false;
            //    Survive = false;
                Explore = true;

                iChose = true;
            }

            RaycastHit hit;
            Debug.DrawRay(gameObject.transform.position, Vector3.forward * 2, Color.red);

            // If its not their turn , then it means that they are dissabled even though they are still in the scene
            if (!turn)
            {
                checkAliviness();
                return;
            }

            if (!moving && Attack == true && Explore == false && Survive == false && Protect == false)
            {
                // Find the nearest Player and attack him
                FindNearestTarget();
                CalculatePath();
                FindSelectableTiles();
                actualTargetTile.target = true; // Turn it green, we found target
            } else if (!moving && Attack == false && Explore == false && Survive == false && Protect == true)
            {
                // Find the furthest tile to step on
                Debug.Log($"DISTANCE BETWEEN {gameObject.name} and Player: " + Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position));
                if (Vector3.Distance(transform.position, FindNearestPlayer().transform.position) <= 4)
                {
                    FindSelectableTiles();
                    FindFarestTarget();
                    CalculatePath();
                    FindSelectableTiles();
                    actualTargetTile.target = true; // Turn it green, we found target
                    Debug.Log($"{gameObject.name}: I am running");
                }
                else
                {
                    Debug.Log($"{gameObject.name}: I am already far away");
                    TurnManager.EndTurn();
                }



            }
            else if (!moving && Attack == false && Explore == false && Survive == true && Protect == false)
            {
                // Try to get out from the water
                FindNearestTarget();
                CalculatePath();
                FindSelectableTiles();
                actualTargetTile.target = true; // Turn it green, we found target
            }
            else if (!moving && Attack == false && Explore == true && Survive == false && Protect == false)
            {
                GetCurrentTile();
                Debug.Log($"{gameObject.name}: Exploring...");
                // Try to find parts of the gun
                if (currentTile.getType() == "Iceberg")
                {
                    if (currentTile.snowLevel > 0)
                    {
                        Debug.Log($"{gameObject.name}: 1 Exploring...");
                        if (currentItemName == "Shovel")
                        {
                            UseItem();
                        }

                    }
                    else if (currentTile.snowLevel == 0)
                    {

                        if (currentTile.pickable)
                        {
                            Debug.Log($"{gameObject.name}: 3 Exploring... : ITEM NAME: {currentTile.itemName}");
                            pickItem();
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name}: 2 Exploring...");
                            Debug.Log($"{gameObject.name}: I am finding another tile to explore.");
                            FindSelectableTiles();
                            FindExplorableTile();
                            CalculatePath();
                            actualTargetTile.target = true; // Turn it green, we found target
                        }

                    }
                }else if (currentTile.getType() == "Ice")
                {

                        if (currentTile.pickable)
                        {
                            Debug.Log($"{gameObject.name}: 3 Exploring... : ITEM NAME: {currentTile.itemName}");
                            pickItem();
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name}: 2 Exploring...");
                            Debug.Log($"{gameObject.name}: I am finding another tile to explore.");
                            FindSelectableTiles();
                            FindExplorableTile();
                            CalculatePath();
                            actualTargetTile.target = true; // Turn it green, we found target
                        }

                }else if (currentTile.getType() == "Water")
                {
                        Debug.Log($"{gameObject.name}: 2 Exploring...");
                        Debug.Log($"{gameObject.name}: I am finding another tile to explore.");
                        FindSelectableTiles();
                        FindExplorableTile();
                        CalculatePath();
                        actualTargetTile.target = true; // Turn it green, we found target
                    
                }
               
            }
            else
            {
                Move();
            }
        }
        

        // If figure died then TODO:
        checkAliviness();

        if (currentItem != null)
        {
            currentItemName = currentItem.Name;
        }


    }

    public void GetCurrentTile()
    {

            currentTile = GetTargetTile(gameObject); // Get target tile for this unit
        
        if (!stopCurrentTileChecking)
        {
            if (currentTile.getType() == "Iceberg")
            {
                Debug.Log("Current tile: " + currentTile.getType());
            }
            else if (currentTile.getType() == "Ice")
            {
                Debug.Log("Current tile: " + currentTile.getType());
            }
            else if (currentTile.getType() == "Water")
            {
                Debug.Log("Current tile: " + currentTile.getType());
            }

            stopCurrentTileChecking = true;
        }

        currentTile.current = true;
    }

    public static bool jumped = false;
    public Tile GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile tile = null;

        if (jumped) // If player jumped from ice to ice
        {
            if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 4))
            {
                tile = hit.collider.GetComponent<Tile>();
            }
        }
        else // If player moves from iceberg to any tile
        {
            if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1))
            {
                tile = hit.collider.GetComponent<Tile>();
            }
        }
        
        return tile;
    }



    public void ComputeAdjacencyList(float jumpHeight, Tile target)
    {
        // If map is changing the size. Thus new and deleted tile are matched
        //tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles)
        {
            Tile t = tile.GetComponent<Tile>();
            t.FindNeighbors(jumpHeight, target);
        }
    }


    public void FindSelectableTiles()
    {

        ComputeAdjacencyList(jumpHeight, null);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        process.Enqueue(currentTile);
        currentTile.visited = true; // Never want to come back to visited tile
        //currentTile.parent = ?? leave as null

        // Will process all of the nodes which are less than move
        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);

            // Checks which tiles are needed to be selected
            if (currentTile.getType() == "Ice")
            {
                if (t.distance == 2 && (t.getType() == "Ice" || t.getType() == "Iceberg"))
                {
                    t.selectable = true;
                }
            }else if (currentTile.getType() == "Iceberg")
            {
                if (t.distance == 2 && (t.getType() == "Ice" || t.getType() == "Iceberg"))
                {

                    t.selectable = true;
                }else if (t.distance != 2 && (t.getType() == "Iceberg" && t.getType() != "Ice"))
                {
                    t.selectable = true;
                }
            }


            if (t.distance < move) //Didn't meet the edge yet
            {

                foreach (Tile tile in t.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = 1 + t.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }

    }

    
    public void MoveToTile(Tile tile)
    {


        path.Clear();
        tile.target = true;
        moving = true;

        // CHANGE COLOR OF THE TARGET TILE
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject t in tiles)
        {
            if (t.gameObject.GetComponent<Tile>().ID == tile.ID)
            {
                t.gameObject.GetComponent<Tile>().changeIsAllowed = true;
            }
        }

        Tile next = tile;
        // BFS Algorithm
        // After this stack will have all path in reverse order
        while (next != null) // Path finding
        {
            path.Push(next);
            next = next.parent;
        }
    }

    // Move unit from one tile to the next
    public void Move()
    {


        // If we have a path , then we move
        if (path.Count > 0)
        {
            TurnManager.choiceIsAllowed = false;
            Tile t = path.Peek(); // we don't remove untill we reach to it , but peek it
            Vector3 target = t.transform.position;

            // Calculate the unit's position on top of the target tile
            target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;
            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                Debug.Log($"{gameObject.name}: Vector3.Distance(transform.position, target) >= 0.05f");
                bool jump = transform.position.y != target.y;
                Debug.Log($"{gameObject.name}: jump = {jump}");
                if (jump)
                {
                    Jump(target);
                    this.gameObject.GetComponent<CentreHeightManager>().stopCentrilize = true;
                }
                else
                {
                    Debug.Log($"{gameObject.name}: I jumped");
                    this.gameObject.GetComponent<CentreHeightManager>().stopCentrilize = true;


                    CalculateHeading(target);
                    SetHorizontalVelocity();
                }


                if (jumped == false)
                {
                      // This part of the code is responsible for stucking in the air while jumping
                // Check what on which tile are you staying
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -Vector3.up, out hit, 2f))
                {
                        if ((hit.collider.GetComponent<Tile>().getType() == "Iceberg") || (hit.collider.GetComponent<Tile>().getType() == "Water"))
                        {
                            Debug.Log($"{gameObject.name}: 1 Walking...");
                            //Localmotion
                            transform.forward = heading;
                            transform.position += velocity * Time.deltaTime;
                        }
                }
                else
                {
                    Debug.Log($"{gameObject.name}: I am falling....");
                    RaycastHit hitWater;

                    if (Physics.Raycast(transform.position, -Vector3.up, out hitWater, 4f))
                    {
                        this.gameObject.GetComponent<CentreHeightManager>().stopCentrilize = false;
                        Debug.Log($"{gameObject.name}: I am hitting the {hitWater.collider.GetComponent<Tile>().getType()}");
                        if (hitWater.collider.GetComponent<Tile>().getType() == "Water")
                        {
                            Debug.Log($"{gameObject.name}: 2 Walking...");
                            //Localmotion
                            transform.forward = heading;
                            transform.position += velocity * Time.deltaTime;

                            Debug.Log($"{gameObject.name}: I fell into the water");
                            isDrowning = true; // To indicate that the figure is drawning
                            TurnManager.EndTurn();

                            if (MyItems.Contains("DivingSuit") == false)
                            {
                                Health = 0;
                            }
                        }

                    }

                }
                }
              


            }
            else
            {
                //Tile center reached
                transform.position = target;
                path.Pop();
                TurnManager.choiceIsAllowed = true;
            }

            if (jumped == true)
            {
                RemoveSelectableTiles();
                TurnManager.EndTurn();
            }

        }
        else // When we reach our target tile
        {
            // Decrement the turns left till blizzard
            Map.GetComponent<TurnManager>().TurnsLeft--;

            Debug.Log("TARGET TILE REACHED!!!");

            if (Attack == true && isBot == true)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.forward, out hit, 2))
                {
                    Debug.Log($"FOUND OBJECT {hit.collider.gameObject.name}");
                    if (hit.collider.gameObject.tag == "Player") // if you amra 1 tile away from the player 1
                    {
                        Debug.Log($"HITTING OBJECT {hit.collider.gameObject.name}");
                        hit.collider.GetComponent<PlayerMove>().Health--;
                    } 
                }else if (Physics.Raycast(transform.position, Vector3.back, out hit, 2))
                {
                    Debug.Log($"FOUND OBJECT {hit.collider.gameObject.name}");
                    if (hit.collider.gameObject.tag == "Player") // if you amra 1 tile away from the player 1
                    {
                        Debug.Log($"HITTING OBJECT {hit.collider.gameObject.name}");
                        hit.collider.GetComponent<PlayerMove>().Health--;
                    }
                }
                else if (Physics.Raycast(transform.position, Vector3.left, out hit, 2))
                {
                    Debug.Log($"FOUND OBJECT {hit.collider.gameObject.name}");
                    if (hit.collider.gameObject.tag == "Player") // if you amra 1 tile away from the player 1
                    {
                        Debug.Log($"HITTING OBJECT {hit.collider.gameObject.name}");
                        hit.collider.GetComponent<PlayerMove>().Health--;
                    }
                }
                else if (Physics.Raycast(transform.position, Vector3.right, out hit, 2))
                {
                    Debug.Log($"FOUND OBJECT {hit.collider.gameObject.name}");
                    if (hit.collider.gameObject.tag == "Player") // if you amra 1 tile away from the player 1
                    {
                        Debug.Log($"HITTING OBJECT {hit.collider.gameObject.name}");
                        hit.collider.GetComponent<PlayerMove>().Health--;
                    }
                }
            }
            RemoveSelectableTiles();
            moving = false;

            // To enable auto current tile type checking after passing the turn to another team
            stopCurrentTileChecking = false;
            // Ending the turn when movement end. It can be pasted to the action part, so considered as action.
            TurnManager.EndTurn();
        }
    }

    protected void RemoveSelectableTiles()
    {
        if(currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize(); // Should be unit vector with magnitude of 1 
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    void Jump(Vector3 target)
    {
        if (fallingDown)
        {
            FallDownward(target);
        }
        else if(jumpingUp)
        {
            JumpUpward(target);
        }else if (movingEdge)
        {
            MoveToEdge();
        } 
        else
        {
            PrepareJump(target);
        }
    }

    void PrepareJump(Vector3 target)
    {
        float targetY = target.y;

        target.y = transform.position.y;

        CalculateHeading(target);

        if (transform.position.y > targetY)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = true;

            jumpTarget = transform.position + (target - transform.position) / 2.0f;
        }
        else
        {
            fallingDown = false;
            jumpingUp = true;
            movingEdge = false;

            // Horizontal Velocity
            velocity = heading * moveSpeed / 3.0f;

            float difference = targetY - transform.position.y;

            // IF want to jump bigger adjust this , jumpVelocity is vertical velocity
            velocity.y = jumpVelocity * (0.5f + difference / 2.0f);

        }
    }

    void FallDownward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime;

        if (transform.position.y <= target.y)
        {
            fallingDown = false;
            jumpingUp = false;
            movingEdge = false;
            Vector3 p = transform.position; // X,Y,Z
            p.y = target.y;
            transform.position = p;

            velocity = new Vector3();
        }
    }

    void JumpUpward(Vector3 target)
    {
        velocity += Physics.gravity * Time.deltaTime; // acceleration

        if (transform.position.y > target.y)
        {
            jumpingUp = false;
            fallingDown = true;
        }
    }

    void MoveToEdge()
    {
        if (Vector3.Distance(transform.position, jumpTarget) >= 0.05f)
        {
            SetHorizontalVelocity();
        }
        else
        {
            movingEdge = false;
            fallingDown = true;

            velocity /= 5.0f; // slowing down
            velocity.y = 1.5f; // small little hop (small vertical velocity)

        }
    }

    public static Tile getCurrentTile()
    {
        return currentTile;
    }

    private void updateTiles()
    {
        // SET CHANGE IS ALLOWED OF ALL TILE TO TRUE
        GameObject[] changeableTiles = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject tiles in changeableTiles)
        {
            tiles.gameObject.GetComponent<Tile>().changeIsAllowed = true;
        }
    }

    public void BeginTurn()
    {
        callFindSelectableTile = true;
        updateTilesAllowed = true;
        turn = true;
    }

    public void EndTurn()
    {
        updateTilesAllowed = true;
        turn = false;
    }

    public void pickItem()
    {
        // If there is something on the tile , then pick it
        if (currentTile.pickable && currentTile.getType() == "Iceberg" && currentTile.snowLevel == 0)
        {
            Debug.Log($"Item {currentTile.itemName} is picked!!!!!");
            currentTile.item.ownerName = TurnManager.currentPlayerName;
            items.Add(currentTile.item);
            MyItems.Add(currentTile.item.Name);
            currentTile.item = new GameItem();
            currentTile.itemName = "";
            currentTile.pickable = false;
            currentItem = items[0];
        }else if (currentTile.pickable && currentTile.getType() == "Ice")
        {
            Debug.Log($"Item {currentTile.itemName} is picked!!!!!");
            currentTile.item.ownerName = TurnManager.currentPlayerName;
            items.Add(currentTile.item);
            MyItems.Add(currentTile.item.Name);
            currentTile.item = new GameItem();
            currentTile.itemName = "";
            currentTile.pickable = false;
            currentItem = items[0];
        }
    }


    public void SwitchItem()
    {
        if (items.Count != 0)
        {
            currentItem = items[ChosenItemIndex];
        }
    }

    public void UseItem()
    {
        // If item length is not null , then use the object
        if (items.Count != 0)
        {
            if (currentItem.Attackable)
            {
                //TODO
                //Choose target player
                if (currentItem.Name == "Axe")
                {
                    RaycastHit hit;
                    if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit, 2))
                    {
                        if (hit.collider.GetComponent<PlayerMove>().Name != null && hit.collider.GetComponent<PlayerMove>().tag != this.gameObject.tag)
                        {
                            Debug.Log("Axe is used");
                            hit.collider.GetComponent<PlayerMove>().Health--;
                        }
                    }
                    items.Remove(currentItem);
                    MyItems.Remove(currentItem.Name);
                }
                else if (currentItem.Name == "Arrow")
                {
                    RaycastHit hit;
                    if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit, 4))
                    {
                        if (hit.collider.GetComponent<PlayerMove>().Name != null && hit.collider.GetComponent<PlayerMove>().tag != this.gameObject.tag)
                        {
                            Debug.Log("Arrow is used");
                            hit.collider.GetComponent<PlayerMove>().Health--;
                        }
                    }
                    items.Remove(currentItem);
                    MyItems.Remove(currentItem.Name);
                }
                else if (currentItem.Name == "MedicalAid")
                {
                    //TODO: Increment health
                    Debug.Log("Medical Aid is used!!!!");
                    Health = Health + 2;
                    items.Remove(currentItem);
                    MyItems.Remove(currentItem.Name);
                }
                else if (currentItem.Name == "Shovel")
                {
                    if (currentTile.getType() == "Iceberg")
                    {
                        //TODO: Decrement snow level of the tile
                        if (currentTile.snowLevel > 0)
                        {
                            currentTile.snowLevel--;
                            TurnManager.EndTurn();
                        }
                        Debug.Log($"Shovel is used by {gameObject.name} !!!! SNOW LEVEL IS {currentTile.snowLevel}");
                    }
                }

                
                if (items.Count != 0)
                {
                    bool stopChoosing = false;
                    int i = 0;
                    while (stopChoosing != true)
                    {
                        if (items[i] != null)
                        {
                            currentItem = items[i];
                            stopChoosing = true;
                        }
                   
                    }
                }

                //Call attack method and pass him there
            }
            
        }
    }


    // Sends item to nearby figure
    public void FindReceivers()
    {
        RaycastHit hit;
        itemReceivers = new List<string>();
        if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit, 2)) // Forward
        {
            if (hit.collider.gameObject.tag == this.gameObject.tag)
            {
                if (!itemReceivers.Contains(hit.collider.gameObject.name))
                {
                    itemReceivers.Add(hit.collider.gameObject.name);
                }
            }
        }

        if (Physics.Raycast(gameObject.transform.position, Vector3.back, out hit, 2)) // Backward
        {
            if (hit.collider.gameObject.tag == this.gameObject.tag)
            {
                if (!itemReceivers.Contains(hit.collider.gameObject.name))
                {
                    itemReceivers.Add(hit.collider.gameObject.name);
                }
            }
        }

        if (Physics.Raycast(gameObject.transform.position, Vector3.right, out hit, 2)) // Right
        {
            if (hit.collider.gameObject.tag == this.gameObject.tag)
            {
                if (!itemReceivers.Contains(hit.collider.gameObject.name))
                {
                    itemReceivers.Add(hit.collider.gameObject.name);
                }
            }
        }

        if (Physics.Raycast(gameObject.transform.position, Vector3.left, out hit, 2)) // Left
        {
            if (hit.collider.gameObject.tag == this.gameObject.tag)
            {
                if (!itemReceivers.Contains(hit.collider.gameObject.name))
                {
                    itemReceivers.Add(hit.collider.gameObject.name);
                }
            }
        }
    }

    public void SendItem()
    {
        try
        {
            if (itemReceivers.Count != 0)
            {
                string receiverName = itemReceivers[receiverIndex];
                RaycastHit hit;
                bool ignore = false;
                if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit, 2) && ignore == false) // Forward
                {
                    sendWithHitCollider(hit, receiverName);
                    ignore = true;
                }

                if (Physics.Raycast(gameObject.transform.position, Vector3.back, out hit, 2) && ignore == false) // Baclward
                {
                    sendWithHitCollider(hit, receiverName);
                    ignore = true;
                }

                if (Physics.Raycast(gameObject.transform.position, Vector3.right, out hit, 2) && ignore == false) // Right
                {
                    sendWithHitCollider(hit, receiverName);
                    ignore = true;
                }

                if (Physics.Raycast(gameObject.transform.position, Vector3.left, out hit, 2) && ignore == false) // Left
                {
                    sendWithHitCollider(hit, receiverName);
                    ignore = true;
                }

                if (ignore == false)
                {
                    Debug.Log("Nothing was sent");
                }

            }
        }
        catch (Exception e)
        {
            Debug.Log("ERROR: " + e);
        }
        
    }

    void sendWithHitCollider(RaycastHit hit, string receiverName)
    {
        if (hit.collider.gameObject.name == receiverName)
        {
            hit.collider.GetComponent<PlayerMove>().items.Add(this.items[SendingItemIndex]);
            hit.collider.GetComponent<PlayerMove>().MyItems.Add(this.items[SendingItemIndex].Name);
            Debug.Log($"Object {this.items[SendingItemIndex].Name} is send from {this.gameObject.name} to {hit.collider.gameObject.name}");
            this.MyItems.Remove(this.items[SendingItemIndex].Name);
            this.items.RemoveAt(SendingItemIndex);
            foreach (var item in MyItems)
            {
                Debug.Log("ITEM: " + item);
            }
            TurnManager.EndTurn();
        }
    }

    public void showCrate()
    {
        // If there is something on the tile , then pick it
        if (currentTile.isBase)
        {
            for (int i = 0; i < currentTile.CrateItems.Count; i++)
            {
                Debug.Log($"Item {i}: {currentTile.CrateItems[i]}");
            }

        }
    }

    public void getFromCrate()
    {
        if (currentTile.isBase)
        {
            items.Add(currentTile.Crate[crateItemIndex]);
            MyItems.Add(currentTile.CrateItems[crateItemIndex]);
            Debug.Log($"{currentTile.CrateItems[crateItemIndex]} is added to {gameObject.name}");
            currentTile.Crate.RemoveAt(crateItemIndex);
            currentTile.CrateItems.RemoveAt(crateItemIndex);
        }
    }


    public void putIntoCrate()
    {
        // If there is something on the tile , then pick it
        if (currentTile.isBase == true)
        {
            currentTile.Crate.Add(items[putIntoCrateIndex]);
            currentTile.CrateItems.Add(MyItems[putIntoCrateIndex]);
            Debug.Log($"{MyItems[putIntoCrateIndex]} is put to {currentTile.gameObject.name}'s base");
            items.RemoveAt(putIntoCrateIndex);
            MyItems.RemoveAt(putIntoCrateIndex);
        }
    }

    public void Rescue()
    {
        if (MyItems.Contains("Rope"))
        {
            Debug.Log($"{gameObject.name}: Rescueing ");
            // FIND DROWNER
            List<Tile> oneWaySelTiles = new List<Tile>();
            GameObject[] tilesGO = GameObject.FindGameObjectsWithTag("Tile");
            List<Tile> tiles = new List<Tile>();
            foreach (var item in tilesGO)
            {
                tiles.Add(item.GetComponent<Tile>());
            }
            foreach (Tile tile in tiles)
            {
                if ((tile.distance == 0 || tile.distance == 1) && tile.getType() == "Water" && tile.StayedFigureTag == gameObject.tag)
                {
                    Debug.Log($"{gameObject.name}: Drowner tile ID is {tile.ID}");
                    Debug.Log($"{gameObject.name}: Drowner tile distance is {tile.distance}");
                    oneWaySelTiles.Add(tile);
                }
            }
            Debug.Log($"{gameObject.name}: oneWaySelTiles count = {oneWaySelTiles.Count}");
            if (oneWaySelTiles.Count != 0)
            {
                GameObject StayedFigureGO = null;
                Tile drownerTile = null;
                bool drownerIsFound = false;
                foreach (Tile tile in oneWaySelTiles)
                {
                    if (tile.StayedFigureTag == gameObject.tag)
                    {
                        drownerTile = tile;
                        StayedFigureGO = drownerTile.StayedFigureGO;
                        drownerIsFound = true;
                        Debug.Log($"{gameObject.name}: Drowner's name = {drownerTile.StayedFigureName}");
                        Debug.Log($"{gameObject.name}: Drowner's tag = {drownerTile.StayedFigureTag}");
                        break;
                    }
                }

                if (drownerIsFound == true)
                {
                    GameObject[] figures = GameObject.FindGameObjectsWithTag(gameObject.tag);
                    Debug.Log($"{gameObject.name}: Drowner is ready to be rescued...");
                    // Finding empty icebergs
                    List<int> icebergsIDs = new List<int>();
                    foreach (var iceberg in tilesGO)
                    {
                        if (iceberg.GetComponent<Tile>().getType() == "Iceberg"
                            && iceberg.GetComponent<Tile>().StayedFigureName == ""
                            && iceberg.GetComponent<Tile>().distance == 1)
                        {
                            icebergsIDs.Add(iceberg.GetComponent<Tile>().ID);
                            Debug.Log($"{gameObject.name}: Rescue Iceberg ID = {iceberg.GetComponent<Tile>().ID}");
                        }
                    }
                    // Choosing iceberg
                    int randomIcebergIndex = UnityEngine.Random.Range(0, icebergsIDs.Count);
                    int randomIcebergID = 0;
                    for (int i = 0; i < icebergsIDs.Count; i++)
                    {
                        if (i == randomIcebergIndex)
                        {
                            randomIcebergID = icebergsIDs[i];
                            break;
                        }
                    }
                    foreach (var iceberg in tilesGO)
                    {
                        if (iceberg.GetComponent<Tile>().ID == randomIcebergID)
                        {
                            // Put the drowner on that iceberg
                            StayedFigureGO.transform.position = new Vector3(iceberg.GetComponent<Tile>().transform.position.x, 2.4f, iceberg.GetComponent<Tile>().transform.position.z);
                            StayedFigureGO.GetComponent<TacticsMove>().isDrowning = false;
                            Debug.Log($"{gameObject.name}: {StayedFigureGO.name} is rescued!");
                        }
                    }

                    drownerIsFound = false;
                }
            }


            // Release rope from the items list at the end
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == "Rope")
                {
                    MyItems.Remove("Rope");
                    items.RemoveAt(i);
                    Debug.Log($"{gameObject.name}: Rope is used!");
                    break;
                }
            }
        }
        else
        {
            Debug.Log($"{gameObject.name}: There is no rope to rescue {gameObject.tag}!");
        }
    }

    void Die()
    {
        TurnManager.RemoveUnit(this);
        Destroy(this.gameObject);

        TurnManager.turnAllowed = false;
        TurnManager.turnTeam[TurnManager.currentPlayerIndex].EndTurn(); // Ends the turn of the current player
        TurnManager.currentPlayerIndex = 0;
    }

    void checkAliviness()
    {
        // If figure died then TODO:
        if (Health == 0) {
            Die();
        }
    }

    float yJumpPos = 0.0f;
    void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // Creates a 3D ray from the point on the screen where the mouse was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "Tile" && hit.collider.gameObject.GetComponent<Tile>().getType() == "Iceberg" && currentTile.getType() == "Iceberg")
                {

                    Tile t = hit.collider.GetComponent<Tile>();
                    Debug.Log($"TILE IS: {t.getType()}");
                    if (t.selectable) // If tile is selectable
                    {
                        TacticsMove.jumped = false;
                        //Move target
                        MoveToTile(t);
                    }
                }
                else if (hit.collider.tag == "Tile" && hit.collider.gameObject.GetComponent<Tile>().getType() == "Ice" && currentTile.getType() == "Ice")
                {

                    Tile t = hit.collider.GetComponent<Tile>();
                    targetTile = t;
                    Debug.Log($"TILE IS: {t.getType()}");
                    if (t.selectable) // If tile is selectable
                    {
                        //Jump to target
                        Debug.Log($"CUR X {TacticsMove.getCurrentTile().transform.position.x}");
                        Debug.Log($"CUR Z {hit.collider.transform.gameObject.transform.position.z}");
                        TacticsMove.jumped = true;
                        if ((TacticsMove.getCurrentTile().transform.position.x < hit.collider.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z)  // Jump to the right
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 0;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("RIGHT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x > hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z) // Jump to the left
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 180;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("LEFT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z < hit.collider.transform.gameObject.transform.position.z) // Jump down
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = -90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("UP");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z > hit.collider.transform.gameObject.transform.position.z) // Jump forward
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("DOWN");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }

                        TacticsMove.jumped = false;
                        TurnManager.EndTurn();
                    }
                }
                else if (hit.collider.tag == "Tile" && hit.collider.gameObject.GetComponent<Tile>().getType() == "Ice" && currentTile.getType() == "Iceberg")
                {
                    this.gameObject.GetComponent<CentreHeightManager>().stopCentrilize = false;
                    Tile t = hit.collider.GetComponent<Tile>();
                    targetTile = t;
                    Debug.Log($"TILE IS: {t.getType()}");
                    if (t.selectable) // If tile is selectable
                    {
                        //Jump to target
                        Debug.Log($"CUR X {TacticsMove.getCurrentTile().transform.position.x}");
                        Debug.Log($"CUR Z {hit.collider.transform.gameObject.transform.position.z}");
                        TacticsMove.jumped = true;
                        if ((TacticsMove.getCurrentTile().transform.position.x < hit.collider.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z)  // Jump to the right
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 0;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("RIGHT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x > hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z) // Jump to the left
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 180;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("LEFT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z < hit.collider.transform.gameObject.transform.position.z) // Jump down
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = -90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("UP");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z > hit.collider.transform.gameObject.transform.position.z) // Jump forward
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("DOWN");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }

                        TacticsMove.jumped = false;
                        TurnManager.EndTurn();
                    }
                }
                else if (hit.collider.tag == "Tile" && hit.collider.gameObject.GetComponent<Tile>().getType() == "Iceberg" && currentTile.getType() == "Ice")
                {

                    Tile t = hit.collider.GetComponent<Tile>();
                    targetTile = t;
                    Debug.Log($"TILE IS: {t.getType()}");
                    if (t.selectable) // If tile is selectable
                    {
                        //Jump to target
                        Debug.Log($"CUR X {TacticsMove.getCurrentTile().transform.position.x}");
                        Debug.Log($"CUR Z {hit.collider.transform.gameObject.transform.position.z}");
                        TacticsMove.jumped = true;
                        if ((TacticsMove.getCurrentTile().transform.position.x < hit.collider.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z)  // Jump to the right
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 0;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("RIGHT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x > hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z == hit.collider.transform.gameObject.transform.position.z) // Jump to the left
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 180;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("LEFT");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z < hit.collider.transform.gameObject.transform.position.z) // Jump down
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = -90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("UP");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }
                        else if ((TacticsMove.getCurrentTile().gameObject.transform.position.x == hit.collider.gameObject.transform.position.x) && TacticsMove.getCurrentTile().transform.gameObject.transform.position.z > hit.collider.transform.gameObject.transform.position.z) // Jump forward
                        {
                            Debug.Log($"Z1 = {TacticsMove.getCurrentTile().transform.gameObject.transform.position.z}, Z2 = {hit.collider.transform.gameObject.transform.position.z}");
                            yJumpPos = 90;
                            transform.rotation = Quaternion.Euler(0, yJumpPos, 0);
                            Debug.Log("DOWN");
                            GetComponent<ParabolaController>().FollowParabola(); // GameObject followes parabola
                            Map.GetComponent<TurnManager>().TurnsLeft--;
                        }

                        TacticsMove.jumped = false;
                        TurnManager.EndTurn();
                    }
                }
            }

        }
    }
}