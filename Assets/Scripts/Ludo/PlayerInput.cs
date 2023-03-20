using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Button {Up = 0, Down = 1, Left = 2, Right = 3, A = 4, B = 5, Start = 6, Select = 7}
public class PlayerInput
{
    private static Timer[] inputTimers;       // For use with double taps, cooldown refactoring, and combos
    private static bool[] doubleTappedButtons;
    private const float DOUBLE_TAP_WINDOW       = .25F;

    private static Dictionary<Button, KeyCode[]> keyboardInputMap = new Dictionary<Button, KeyCode[]>(){
        {
            Button.Up, new[]{
            KeyCode.UpArrow, KeyCode.W
        }},
        {
            Button.Down, new[]{
            KeyCode.DownArrow, KeyCode.S
        }},
        {
            Button.Left, new[]{
            KeyCode.LeftArrow, KeyCode.A
        }},
        {
            Button.Right, new[]{
            KeyCode.RightArrow, KeyCode.D
        }},
        {
            Button.A, new[]{
            KeyCode.C, KeyCode.K, KeyCode.P
        }},
        {
            Button.B, new[]{
            KeyCode.Z, KeyCode.J, KeyCode.I
        }},
        {
            Button.Start, new[]{
            KeyCode.Return, KeyCode.Escape
        }},
        {
            Button.Select, new[]{
            KeyCode.R, KeyCode.LeftShift, KeyCode.RightShift
        }},
    };

    public static bool Pressed(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKeyDown(key))  return true;
        return false;
    }
    public static bool Held(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKey(key))  return true;
        return false;
    }

    public static bool Released(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKeyUp(key))  return true;
        return false;
    }

    public static void Init()
    {
        inputTimers = new Timer[8];
        doubleTappedButtons = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            inputTimers[i] = null;
            doubleTappedButtons[i] = false;
        }
    }

    public static bool DidDoubleTap(Button button) => doubleTappedButtons[(int)button];

    public static void Update()
    {
        // update button timers
        for (int i = 0; i < 8; i++)
        {
            Button button = (Button) i;
            doubleTappedButtons[i] = false;

            if (PlayerInput.Pressed(button))
            {                    
                if (inputTimers[i] != null &&  inputTimers[i].active)
                {
                    if (inputTimers[(int)button].elapsed < DOUBLE_TAP_WINDOW)
                    {
                        doubleTappedButtons[i] = true;
                    }
                    
                    inputTimers[i].Cancel();
                }

                inputTimers[i] = Timer.Set(1);
            }
        }
    }
}
