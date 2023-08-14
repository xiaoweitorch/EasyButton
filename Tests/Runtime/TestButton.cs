﻿using UnityEngine;

namespace LW.Util.EasyButton.Tests
{
    public class TestButton : MonoBehaviour
    {
        public int A;
        
        [EasyButton]
        public void LogError()
        {
            Debug.LogError($"[EasyButton]log error");
        }
        
        [EasyButton("TestName")]
        public void LogErrorWithName()
        {
            Debug.LogError($"[EasyButton]log error with name");
        }

        [EasyButton]
        public static void StaticLogError()
        {
            Debug.LogError($"[EasyButton]static log error");
        }

        [EasyButton]
        public static int StaticLogWithParam(int a = 2)
        {
            Debug.LogError($"a={a}");
            return a;
        }

        [EasyButton]
        public int LogWithParam(int a, Transform tf, bool c = true)
        {
            Debug.LogError($"a={a} transform={tf}");
            return a;
        }

        // TODO: lw not support return value
        [EasyButton]
        public int LogErrorWithReturn()
        {
            return 0;
        }
    }
}