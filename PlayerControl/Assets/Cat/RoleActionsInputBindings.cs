using UnityEngine;
using System.Collections;
using InControl;

public class RoleActionsInputBindings : PlayerActionSet
{
    public PlayerAction Stick1_Up;
    public PlayerAction Stick1_Down;
    public PlayerAction Stick1_Left;
	public PlayerAction Stick1_Right;
	public PlayerAction KeyA;
	public PlayerAction LeftBumper;
	public PlayerAction KeyX;
	public PlayerOneAxisAction Move;
    public PlayerAction Left_Trigger;

    // 控制相机移动
    public PlayerAction Stick2_Up;
    public PlayerAction Stick2_Down;
    public PlayerAction Stick2_Left;
    public PlayerAction Stick2_Right;
    public PlayerTwoAxisAction Camera_Move;

	public RoleActionsInputBindings()
    {
        Stick1_Up = CreatePlayerAction("Move Up");
        Stick1_Down = CreatePlayerAction("Move Down");
        Stick1_Left = CreatePlayerAction ("Move Left");
		Stick1_Right = CreatePlayerAction ("Move Right");
		KeyA = CreatePlayerAction ("Jump");
		LeftBumper = CreatePlayerAction ("Change World");
		KeyX = CreatePlayerAction ("Get Soul");
        Left_Trigger = CreatePlayerAction("Switch Preview");
        Move = CreateOneAxisPlayerAction (Stick1_Left,Stick1_Right);

        // 控制相机移动
        Stick2_Up = CreatePlayerAction("CameraMove Up");
        Stick2_Down = CreatePlayerAction("CameraMove Down");
        Stick2_Left = CreatePlayerAction("CameraMove Left");
        Stick2_Right = CreatePlayerAction("CameraMove Right");
        Camera_Move = CreateTwoAxisPlayerAction(Stick2_Left, Stick2_Right, Stick2_Up, Stick2_Down);
    }

	public  static RoleActionsInputBindings ActionsBindings()
	{
		var characterActions = new RoleActionsInputBindings ();

        characterActions.Stick1_Up.AddDefaultBinding(Key.W);
        characterActions.Stick1_Up.AddDefaultBinding(InputControlType.LeftStickUp);
        characterActions.Stick1_Up.AddDefaultBinding(InputControlType.DPadUp);

        //左右键被第二角色占用
        //characterActions.Left.AddDefaultBinding( Key.LeftArrow );
        characterActions.Stick1_Left.AddDefaultBinding(Key.A);
		characterActions.Stick1_Left.AddDefaultBinding( InputControlType.LeftStickLeft );
        characterActions.Stick1_Left.AddDefaultBinding(InputControlType.DPadLeft);

		//characterActions.Right.AddDefaultBinding( Key.RightArrow );
        characterActions.Stick1_Right.AddDefaultBinding(Key.D);
		characterActions.Stick1_Right.AddDefaultBinding( InputControlType.LeftStickRight );
        characterActions.Stick1_Right.AddDefaultBinding(InputControlType.DPadRight);

		characterActions.KeyA.AddDefaultBinding( Key.Space );
		characterActions.KeyA.AddDefaultBinding( InputControlType.Action1 );

		characterActions.LeftBumper.AddDefaultBinding( Key.J );
		characterActions.LeftBumper.AddDefaultBinding( InputControlType.LeftBumper );

		characterActions.KeyX.AddDefaultBinding( Key.X );
		characterActions.KeyX.AddDefaultBinding( InputControlType.Action3 );

        characterActions.Left_Trigger.AddDefaultBinding(Key.H);
        characterActions.Left_Trigger.AddDefaultBinding(InputControlType.LeftTrigger);

        // 相机移动按键绑定
        characterActions.Stick2_Up.AddDefaultBinding(Key.UpArrow);
        characterActions.Stick2_Up.AddDefaultBinding(InputControlType.RightStickUp);
        characterActions.Stick2_Down.AddDefaultBinding(Key.DownArrow);
        characterActions.Stick2_Down.AddDefaultBinding(InputControlType.RightStickDown);
        characterActions.Stick2_Left.AddDefaultBinding(Key.LeftArrow);
        characterActions.Stick2_Left.AddDefaultBinding(InputControlType.RightStickLeft);
        characterActions.Stick2_Right.AddDefaultBinding(Key.RightArrow);
        characterActions.Stick2_Right.AddDefaultBinding(InputControlType.RightStickRight);

        return characterActions;
	}
}
