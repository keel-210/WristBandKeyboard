using UnityEngine;
using System;
using WindowsInput;
using WindowsInput.Native;

public class InputTest : MonoBehaviour
{
    InputSimulator IS = new InputSimulator();
    [SerializeField] string inputText;
    float Timer;
    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > 3f)
        {
            VirtualKeyCode vk;
            bool isSuccess;
            if (isSuccess = System.Enum.TryParse(inputText, out vk))
                IS.Keyboard.KeyDown(vk);
            Debug.Log(isSuccess);
            Timer = 0;
        }
    }
}