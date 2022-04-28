using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> buildings = new List<GameObject>();
    public Text text;
    public Transform playerCameraTr;

    private readonly WaitForSeconds waitSec = new WaitForSeconds(1f);
    private int numOfBuildings;
    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("BUILDING");
        foreach(var item in temp)
        {
            buildings.Add(item);
        }
        numOfBuildings = buildings.Count;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "remainBuildings: " + buildings.Count.ToString() + " / " + numOfBuildings.ToString();
        text.transform.LookAt(playerCameraTr);
    }

    public IEnumerator OnBuildingDestroy()
    {
        yield return waitSec;
        for(int i = 0; i < buildings.Count; i++)
        {
            if (!buildings[i].activeSelf)
            {
                buildings.RemoveAt(i);
                break;
            }
        }
    }
}
