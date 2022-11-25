using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public enum Scene
{
    WIN_SCENE,
    MAIN_GAME,
    GAME_OVER,
}*/

/*public class MapBoundaries
{
    public static readonly float xRange = 49.5f;
    public static readonly float yRange = 49.5f;
}*/
/*public static class ScenesExtensions
{
    public static string ToString(this Scene scene)
    {
        switch (scene)
        {
            case Scene.WIN_SCENE:
                return "WinScene";

            case Scene.MAIN_GAME:
                return "MainGame";

            case Scene.GAME_OVER:
                return "GameOver";

            default:
                return "Unknown";
        }
    }
}*/

public class PlayerInput
{
    public static bool HasPressedJumpKey()
    {
        return (Input.GetKeyDown(KeyCode.Space));
    }
    public static bool HasHeldJumpKey()
    {
        return (Input.GetKey(KeyCode.Space));
    }

    public static bool HasReleasedJumpKey()
    {
        return (Input.GetKeyUp(KeyCode.Space));
    }

    public static bool HasPressedAttackKey()
    {
        return (Input.GetKeyDown(KeyCode.B));
    }

    /*public static bool HasPressedDown()
    {
        return (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.L));
    }*/

    /*public static bool HasPressedLeft()
    {
        return (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.K));
    }*/

    /*public static bool HasPressedRight()
    {
        return (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Semicolon));
    }*/
    public static bool IsPressingUp()
    {
        return (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.O));
    }
    public static bool IsPressingDown()
    {
        return (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.L));
    }
    public static bool IsPressingLeft()
    {
        return (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.K));
    }

    public static bool IsPressingRight()
    {
        return (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Semicolon));
    }

    /*public static bool IsPressingUp()
    {
        return (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.O));
    }*/

    /*public static bool IsPressingDown()
    {
        return (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.L));
    }*/

    /*public static bool HasPressedEnter()
    {
        return (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
    }*/

    public static bool HasPressedResetKey()
    {
        return (Input.GetKeyDown(KeyCode.R));
    }

    /*public static bool HasPressedAttackKey()
    {
        return (Input.GetMouseButtonDown(0));
    }*/

    public static bool HasPressedAbsorbPlacementKey()
    {
        return (Input.GetMouseButtonDown(0));
    }
    /*public static bool HasPressedPlacementKey()
    {
        return (Input.GetMouseButtonDown(1));
    }*/
    /*public static bool HasPressedShootingKey()
    {
        return (Input.GetMouseButtonDown(0));
    }*/

    public static bool HasPressedEscapeKey()
    {
        return (Input.GetKeyDown(KeyCode.Escape));
    }
    /*public static bool HasSlappedCard()
    {
        return (Input.GetKeyDown(KeyCode.H));
    }*/
}
