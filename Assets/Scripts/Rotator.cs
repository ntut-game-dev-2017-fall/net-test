using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Rotator : NetworkBehaviour
{
    public int id;
    private GameMgr _gm;

    private void Start()
    {
        if (isServer)
        {
            _gm = GameObject.Find("GameMgr").GetComponent<GameMgr>();
            _gm.Login(this);
        }
    }

    [ClientRpc]
    public void RpcSetPlayer(int id)
    {
        if (isClient)
        {
            this.id = id;
        }
    }

    // Before rendering each frame..
    void Update ()
    {
        if (isServer)
        {
            // Rotate the game object that this script is attached to by 15 in the X axis,
            // 30 in the Y axis and 45 in the Z axis, multiplied by deltaTime in order to make it per second
            // rather than per frame.
            transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
        }
    }


    [ClientRpc]
    public void RpcReceiveServerObj(Vector3 pos, Quaternion ro, bool act)
    {
        if (isClient)
        {
            this.transform.position = pos;
            this.transform.rotation = ro;
            this.gameObject.SetActive(act);
        }
    }

}