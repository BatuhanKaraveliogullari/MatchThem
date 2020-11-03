using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ParticalManager : MonoBehaviour
{
    public GameObject clearFXPrefab;
    public GameObject breakFXPrefab;
    public GameObject doubleBreakFXPrefab;
    public GameObject bombFXPrefab;
    public void ClearPieceFXAt(int x, int y, int z = 0)
    {
        if(clearFXPrefab != null)
        {
            GameObject clearFX = Instantiate(clearFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;

            ParticalPlayer particalPlayer = clearFX.GetComponent<ParticalPlayer>();

            if(particalPlayer != null)
            {
                particalPlayer.Play();
            }
        }
    }

    public void BreakTileFXAt(int breakableValue, int x, int y, int z = 0)
    {
        GameObject breakFX = null;
        ParticalPlayer particalPlayer = null;

        if(breakableValue > 1)
        {
            if(doubleBreakFXPrefab != null)
            {
                breakFX = Instantiate(doubleBreakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            }
        }

        else
        {
            if(breakFXPrefab != null)
            {
                breakFX = Instantiate(breakFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
            }
        }

        if(breakFX != null)
        {
            particalPlayer = breakFX.GetComponent<ParticalPlayer>();

            if(particalPlayer != null)
            {
                particalPlayer.Play();
            }
        }
    }

    public void BombFXAt(int x, int y, int z= 0)
    {
        if(bombFXPrefab != null)
        {
            GameObject bombFX = Instantiate(bombFXPrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;

            ParticalPlayer particalPlayer = bombFX.GetComponent<ParticalPlayer>();

            if(particalPlayer != null)
            {
                particalPlayer.Play();
            }
        }
    }
}
