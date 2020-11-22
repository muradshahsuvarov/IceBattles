using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    static int static_id = 0;
    public int ID = 0;

    public bool walkable = true;
    public bool current = false; // Where we are standing at
    public bool target = false; // Where we are moving to
    public bool selectable = false; // Where we want to move and is available

    
    public List<Tile> adjacencyList = new List<Tile>(); // No diagonals , so (UP,DOWN,LEFT,RIGHT)

    // Needed BFS (breedth first search)
    public bool visited = false;
    public Tile parent = null;
    public int distance = 0; // How far each tile from the start tile


    public string Type;
    MeshRenderer meshRenderer;
    public Texture albedoTexture;

    public GameItem item;
    public string itemName;


    public bool StopGeneratingItems = false;
    public bool StopGeneratingGunFlare = false;
    public bool StopGeneratingGunFrame = false;
    public bool StopGeneratingGunTrigger = false;
    public bool pickable = false;

    GunElementsGenerator geg;

    public int snowLevel;

    public bool isBase;
    public string baseName;
    public List<GameItem> Crate;
    public List<string> CrateItems;

    public bool BlizzardAllowed = false;

    //For A*
    public float f = 0; // f = g + h
    public float g = 0; // cost from the parent to the current tile
    public float h = 0; // cost from current tile to the destination

    public GameObject StayedFigureGO;
    public TacticsMove StayedFigure;
    public string StayedFigureName;
    public string StayedFigureTag;


    public Material groundMaterial;

    public bool changeIsAllowed = false;
    public void SetStayedFigure()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, 2))
        {
                StayedFigureGO = hit.collider.gameObject;
                StayedFigure = hit.collider.gameObject.GetComponent<TacticsMove>();
                StayedFigureName = hit.collider.gameObject.name;
                StayedFigureTag = hit.collider.gameObject.tag;
        }
        else
        {
                StayedFigureGO = null;
                StayedFigure = null;
                StayedFigureName = "";
                StayedFigureTag = "";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        groundMaterial = Resources.Load("Main Material", typeof(Material)) as Material;

        ID = static_id; // Assign ID to the tile
        static_id++;

        if (Type == "Iceberg")
        {
            snowLevel = 3;
        }


        geg = new GunElementsGenerator();
        geg = GameObject.FindGameObjectWithTag("GunGenerator").GetComponent<GunElementsGenerator>(); // References to the memory address of GunElementsGenerator object

        Crate = new List<GameItem>();
        CrateItems = new List<string>();

        // Generating Items on the tile
        item = new GameItem();
        if ((Type == "Iceberg" || Type == "Ice") && StopGeneratingItems == false)
        {
            int selectedNumber = Random.Range(1,11);

            if (selectedNumber == 1) // Axe
            {
                Axe axe = new Axe();
                axe.Name = "Axe";
                axe.Attackable = true;
                axe.Damage = 1;
                axe.DamageDistance = 1;
                if (isBase == true)
                {
                    Crate.Add(axe);
                    CrateItems.Add("axe");
                }
                else
                {
                    item = axe;
                }
                pickable = true;
            }else if (selectedNumber == 2) // Arrow
            {
                Arrow arrow = new Arrow();
                arrow.Name = "Arrow";
                arrow.Attackable = true;
                arrow.Damage = 1;
                arrow.DamageDistance = 4; 
                if (isBase == true)
                {
                    Crate.Add(arrow);
                    CrateItems.Add("arrow");
                }
                else
                {
                    item = arrow;
                }
                pickable = true;
            }
            else if (selectedNumber == 3) // Rope
            {
                Rope rope = new Rope();
                rope.Name = "Rope";
                rope.Attackable = false;
                if (isBase == true)
                {
                    Crate.Add(rope);
                    CrateItems.Add("rope");
                }
                else
                {
                    item = rope;
                }
                pickable = true;
            }
            else if (selectedNumber == 4) // Medical Aid
            {
                MedicalAid medAid = new MedicalAid();
                medAid.Name = "MedicalAid";
                medAid.Attackable = false;
                if (isBase == true)
                {
                    Crate.Add(medAid);
                    CrateItems.Add("medAid");
                }
                else
                {
                    item = medAid;
                }
                pickable = true;
            }
            else if (selectedNumber == 6) // Diving Suit
            {
                DivingSuit divSuit = new DivingSuit();
                divSuit.Name = "DivingSuit";
                divSuit.Attackable = false;
                if (isBase == true)
                {
                    Crate.Add(divSuit);
                    CrateItems.Add("divSuit");
                }
                else
                {
                    item = divSuit;
                }
                pickable = true;
            }
            else if (selectedNumber == 7 && isBase == false) // Gun Frame
            {
                if (geg.StopGeneratingGunFrame == false)
                {
                    GunFrame gunFrame = new GunFrame();
                    gunFrame.Name = "GunFrame";
                    gunFrame.Attackable = false;
                    item = gunFrame;
                    pickable = true;
                    geg.StopGeneratingGunFrame = true;
                }
            }
            else if (selectedNumber == 8 && isBase == false) // Gun Flare
            {
                if (geg.StopGeneratingGunFlare == false)
                {
                    GunFlare gunFlare = new GunFlare();
                    gunFlare.Name = "GunFlare";
                    gunFlare.Attackable = false;
                    item = gunFlare;
                    pickable = true;
                    geg.StopGeneratingGunFlare = true;
                }

            }
            else if (selectedNumber == 9 && isBase == false) // Gun Trigger
            {
                if (geg.StopGeneratingGunTrigger == false)
                {
                    GunTrigger gunTrigger = new GunTrigger();
                    gunTrigger.Name = "GunTrigger";
                    gunTrigger.Attackable = false;
                    item = gunTrigger;
                    pickable = true;
                    geg.StopGeneratingGunTrigger = true;
                }
            }

            itemName = item.Name;
            StopGeneratingItems = true;
        }

        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void RemoveItem()
    {
        item = new GameItem();
        item.Name = "";
        
    }

    // Update is called once per frame
    void Update()
    {
        if (changeIsAllowed == true)
        {
            if (current) // Current Player. Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_ground"));
                GetComponent<Renderer>().material.color = Color.magenta;
            }
            else if (target) // Clicked tile
            {
                GetComponent<Renderer>().material.color = Color.blue;

            }
            else if (selectable && Type == "Ice" && TacticsMove.getCurrentTile().getType() == "Ice") // Available tiles . Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_ground"));
                GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (selectable && Type == "Iceberg" && TacticsMove.getCurrentTile().getType() == "Ice") // Available tiles . Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_ground"));
                GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (selectable && Type == "Ice" && TacticsMove.getCurrentTile().getType() == "Iceberg") // Available tiles . Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_ground"));
                GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (selectable && Type == "Water") // Available tiles . Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_water"));
                // GetComponent<Renderer>().material.color = Color.gray;
            }
            else if (selectable && Type == "Iceberg" && TacticsMove.getCurrentTile().getType() == "Iceberg") // Available tiles . Assign white material and then change its color.
            {
                meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_ground"));
                GetComponent<Renderer>().material.color = Color.red;
            }
            else // Other tiles
            {
                GetComponent<Renderer>().material.color = Color.white;

                // Checks the types of tiles and based on re-assign the tile.
                if (this.getType() == "Water")
                {
                    meshRenderer.material.SetTexture("_MainTex", Resources.Load<Texture>("tile_water"));
                }

                if (this.getType() == "Iceberg")
                {
                    // Set material to ground
                    gameObject.GetComponent<Renderer>().material = groundMaterial;
                }

                if (this.getType() == "Ice")
                {
                    // Set material to ground
                    gameObject.GetComponent<Renderer>().material = groundMaterial;
                }



            }

            changeIsAllowed = false;
        }
  

        StopGeneratingGunFlare = geg.StopGeneratingGunFlare;
        StopGeneratingGunFrame = geg.StopGeneratingGunFrame;
        StopGeneratingGunTrigger = geg.StopGeneratingGunTrigger;

        if (snowLevel > 5)
        {
            snowLevel = 5;
        }

        SetStayedFigure();
    }

    public void Reset()
    {
        adjacencyList.Clear();

        current = false; // Where we are standing at
        target = false; // Where we are moving to
        selectable = false; // Where we want to move and is available

        // Needed BFS (breedth first search)
        visited = false;
        parent = null;
        distance = 0;

        f = g = h = 0;
    }

    // jumpHeight = how many tiles we can jump up
    public void FindNeighbors(float jumpHeight, Tile target)
    {
        Reset();

        CheckTile(Vector3.forward, jumpHeight, target);
        CheckTile(-Vector3.forward, jumpHeight, target);
        CheckTile(Vector3.right, jumpHeight, target);
        CheckTile(-Vector3.right, jumpHeight, target);
    }

    // If distance between current tile and another tile is 2 , then it is walkable
    // jump height is different for every player
    public void CheckTile(Vector3 direction, float jumpHeight, Tile target)
    {
        Vector3 halfExtends = new Vector3(0.25f, (1 + jumpHeight) / 2.0f, 0.25f);
        // OverlapBox returns array of colliders
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtends);

        foreach (Collider item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null && tile.walkable) // There is a tile
            {
                RaycastHit hit;

                // If something up then don't add it , otherwise add it
                if (!Physics.Raycast(tile.transform.position, Vector3.up, out hit, 1) || (tile == target))
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }

    public string getType() {

        return this.Type;

    }
}
