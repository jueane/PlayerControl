using UnityEngine;
using System.Collections;

public class AnimatorControl : MonoBehaviour {

    public Animator _animator;
    private float Horizontal;
    public PlayerControl _player;
    // Use this for initialization
    void Start () {


    }
	
	// Update is called once per frame
	void Update () {


        Horizontal = _player.moveProc.horizontalInputSpeed;
        UpdateAnimation();
    }


    void UpdateAnimation()
    {
        _animator.SetBool("Ground", _player.groundDct.IsOnGround());
        _animator.SetBool("Raising", _player.state == RoleState.Raising);
        //_animator.SetBool("Falling", _player.state == RoleState.Falling);
        _animator.SetBool("Falling", (_player.state != RoleState.Raising && _player.groundDct.IsOnGround() == false));
        _animator.SetFloat("InputH", Mathf.Abs(Horizontal));
        _animator.SetFloat("DisToGround", _player.groundDct.disGround);
    }

    public void DoubleJumpEffect(Vector3 pos)
    {

    }
}
