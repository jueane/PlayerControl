using UnityEngine;
using System.Collections;

public class InputMgr : MonoBehaviour
{
    public static InputMgr Instance { get; private set; }

    public bool noGravity;
    public static PlayerControl _player;
    public bool inputAvailable;

    public RoleActionsInputBindings catInput;

    void Awake()
    {

    }

    public void Init()
    {
    }

    void Start()
    {
        Instance = this;
        noGravity = false;

        _player = GetComponent<PlayerControl>();


        catInput = RoleActionsInputBindings.ActionsBindings();
    }

    // Update is called once per frame
    void Update()
    {
        if (catInput == null)
        {
            catInput = RoleActionsInputBindings.ActionsBindings();
        }
        if (noGravity)
            return;

        if (inputAvailable)
        {
            //if (_player == null)
            //{
            //    _player = GetComponent<PlayerControl>();

            //} 

            if (Time.timeScale == 0)
            {
                return;
            }
            //平移
            //            _player.SetMove(Input.GetAxis("Horizontal"));
            _player.SetMove(catInput.Move.Value);
            //空格跳
            //                if (Input.GetKey(KeyCode.Space) || Input.GetButton("Jump"))
            if (catInput.KeyA.IsPressed)
            {
                _player.jumpProc.JumpOnGround();
            }


        }


    }

    void OnDisable()
    {
        catInput.Destroy();
    }
}
