using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameMgr : NetworkBehaviour
{
    private Dictionary<int, PlayerController> allPlayer = new Dictionary<int, PlayerController>();

    private Dictionary<int, Rotator> allMonster = new Dictionary<int, Rotator>();

    private static int _playerIdx = 0;

    // Use this for initialization
    void Start ()
    {
    }

    public void Login(PlayerController player)
    {
        //GameObject.Find("NetworkManager").GetComponent<NetworkManager>().
        int playerID = ++_playerIdx;
        allPlayer.Add(playerID, player);
        player.playerID = playerID;
        player.RpcSetPlayer(playerID);
    }

    public void Login(Rotator monster)
    {
        int mID = allMonster.Count;
        allMonster.Add(mID, monster);
        monster.RpcSetPlayer(allPlayer.Count);
    }

    // Update is called once per frame
    void Update ()
    {
        if (isServer)
        {
            foreach (var p in allPlayer.Values)
            {
                if (p != null && p.transform != null)
                {
                    p.RpcReceiveServerObj(
                        p.transform.position,
                        p.transform.rotation,
                        p.GetComponent<Rigidbody>().rotation
                    );
                }
            }

            foreach (var m in allMonster.Values)
            {
                m.RpcReceiveServerObj(
                    m.transform.position,
                    m.transform.rotation,
                    m.gameObject.activeSelf
                );
            }
        }
    }

    public string AllPlayerCount
    {
        get
        {
            string ret = "";

            foreach (var p in allPlayer.Values)
            {
                ret += string.Format("Player{0} Count: {1}\r\n", p.playerID, p.Count);
            }

            return ret;
        }
    }

    public bool CheckWin()
    {
        int allCatch = 0;

        foreach (var p in allPlayer.Values)
            if (p != null) allCatch += p.Count;

        return allCatch >= allMonster.Count;
    }

    public void CheckAndSendWinMsg()
    {
        if (CheckWin())
        {
            int max = 0, maxID = 0;

            foreach (var p in allPlayer.Values)
                if (p.Count > max)
                {
                    max = p.Count;
                    maxID = p.playerID;
                }

            foreach (var p in allPlayer.Values)
                p.RpcShowWhoWin(maxID);
        }
    }
}
