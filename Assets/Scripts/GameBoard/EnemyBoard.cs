using System.Collections;
using System.Collections.Generic;
using TCGSim;
using TCGSim.CardScripts;
using UnityEngine;

public class EnemyBoard : Board
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Init(string boardName, ServerCon serverCon, string gameCustomID)
    {
        base.Init(boardName, serverCon, gameCustomID);
        if (serverCon == null)
        {
            Debug.LogError("ServerCon prefab NULL after Init!", this);
        }
        Debug.Log(boardName);
    }
}
