using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;

using System.Collections;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

    // Create public variables for player speed, and for the Text UI game objects
    // [SyncVar]
    public float speed;
    public Text countText;
    public Text winText;
    public Text playerText;

    // Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
    // [SyncVar]
    public Rigidbody rb;
    [SyncVar (hook = "SetCount")]
    private int _count;

    [SyncVar]
    private Color _myColor;

    [SyncVar]
    public int playerID = -1;

    private GameMgr _gm;

    // At the start of the game..
    void Start ()
    {
        InitVar();

        if (isServer)
        {
            _gm.Login(this);
            SetCount(0);
        }
        else
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }

    private void InitVar()
    {
        if (isServer)
        {
            _gm = GameObject.Find("GameMgr").GetComponent<GameMgr>();
            _myColor = Random.ColorHSV();
            GetComponent<Renderer>().material.color = _myColor;
            RpcSetColor(_myColor);
        }

        rb = GetComponent<Rigidbody>();
        // Assign the Rigidbody component to our private rb variable
        countText = GameObject.Find("Count Text").GetComponent<Text>();
        winText = GameObject.Find("Win Text").GetComponent<Text>();
        playerText = GameObject.Find("PlayerText").GetComponent<Text>();
        // Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
        winText.text = "";

        if (isLocalPlayer)
        {
            playerText.text = "Player " + playerID.ToString();
        }
    }

    [ClientRpc]
    void RpcSetColor(Color c)
    {
        this._myColor = c;
        GetComponent<Renderer>().material.color = c;
    }

    [ClientRpc]
    public void RpcSetPlayer(int id)
    {
        if (isLocalPlayer)
        {
            InitVar();
            playerID = id;
            GameObject.Find("Main Camera").GetComponent<CameraController>().Attach(this.gameObject);
        }
    }

    private void OnGUI()
    {
        SetCountText();
    }

    // Each physics step..
    void FixedUpdate ()
    {
        if (isLocalPlayer)
        {
            // Set some local float variables equal to the value of our Horizontal and Vertical Inputs
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            // Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical) * speed;
            CmdSetPlayerForce(movement);
        }
    }

    // When this game object intersects a collider with 'is trigger' checked,
    // store a reference to that collider in a variable named 'other'..
    void OnTriggerEnter(Collider other)
    {
        if (isServer)
        {
            // ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
            if (other.gameObject.CompareTag("Pick Up"))
            {
                // Make the other game object (the pick up) inactive, to make it disappear
                other.gameObject.SetActive(false);
                // Add one to the score variable 'count'
                SetCount(_count + 1);
                _gm.CheckAndSendWinMsg();
            }
        }
    }


    // Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
    void SetCountText()
    {
        if (isServer)
        {
            countText.text = _gm.AllPlayerCount;
        }

        if (isLocalPlayer)
        {
            countText.text = "Count: " + _count.ToString();
        }
    }


    [ClientRpc]
    public void RpcShowWhoWin(int pid)
    {
        winText.text = string.Format("Player {0} Win!!", pid);
    }

    public int Count
    {
        get
        {
            return this._count;
        }
    }

    private void SetCount(int c)
    {
        this._count = c;
    }


    [ClientRpc]
    public void RpcReceiveServerObj(Vector3 pos, Quaternion r, Quaternion rq)
    {
        if (isClient && playerID != -1)
        {
            this.transform.position = pos;
            this.transform.rotation = r;
            rb.rotation = rq;
            // this.gameObject.SetActive(svrRotator.gameObject.activeSelf);
        }
    }



    [Command]
    public void CmdSetPlayerForce(Vector3 v)
    {
        rb.AddForce(v);
    }
}