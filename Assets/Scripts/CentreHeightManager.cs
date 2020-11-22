using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentreHeightManager : MonoBehaviour
{
    public bool stopCentrilize = false;

    // Ceentralized tile coordinates
    public Vector3 centrTile;
    public float tileX;
    public float tileY;
    public float tileZ;

    // Distance between tile and the figure
    public float distance;

    void Start()
    {
        stopCentrilize = true;
        centrTile = new Vector3();
    }

    void Update()
    {
        Tile tile = null;
        RaycastHit hit;


        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 10))
        {

                tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                gameObject.GetComponent<PlayerMove>().StayedTile = tile.getType();
            }

            if (gameObject.GetComponent<PlayerMove>().StayedTile == "Ice" && stopCentrilize == false)
            {
                transform.position = new Vector3(tile.transform.position.x, 1.5f, transform.position.z);
                tileX = tile.transform.position.x;
                tileY = 1.5f;
                tileZ = transform.position.z;
                centrTile = new Vector3(tileX, tileY, tileZ);


                distance = Vector3.Distance(gameObject.transform.position, this.gameObject.GetComponent<TacticsMove>().targetTile.transform.position);
                
            }
            else if (gameObject.GetComponent<PlayerMove>().StayedTile == "Iceberg" && stopCentrilize == false)
            {
                transform.position = new Vector3(tile.transform.position.x, 2.4f, transform.position.z);
                tileX = tile.transform.position.x;
                tileY = 2.4f;
                tileZ = transform.position.z;
                centrTile = new Vector3(tileX, tileY, tileZ);


                distance = Vector3.Distance(gameObject.transform.position, this.gameObject.GetComponent<TacticsMove>().targetTile.transform.position);

            }
            else if (gameObject.GetComponent<PlayerMove>().StayedTile == "Water" && stopCentrilize == false)
            {
                transform.position = new Vector3(tile.transform.position.x, 1.4f, tile.transform.position.z);
                tileX = tile.transform.position.x;
                tileY = 1.4f;
                tileZ = transform.position.z;
                centrTile = new Vector3(tileX, tileY, tileZ);

            }

        }
    }
}
