using UnityEngine;

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
        
        [EasyButton(Name = "TestName")]
        public void LogErrorWithName()
        {
            Debug.LogError($"[EasyButton]log error with name");
        }

        [EasyButton]
        public static void StaticLogError()
        {
            Debug.LogError($"[EasyButton]static log error");
        }

        [EasyButton(3u, DefaultExpand = true)]
        public static uint StaticLogWithParam(uint a = 2)
        {
            Debug.LogError($"a={a}");
            return a;
        }

        [EasyButton(3, null, false, Order = 1)]
        public int LogWithParam(int a, Transform tf, bool c = true)
        {
            Debug.LogError($"a={a} transform={tf}");
            return a;
        }

        [EasyButton(PrintReturn = PrintLevel.Error)]
        public int LogErrorWithReturn()
        {
            return 0;
        }

        [EasyButton(EnableMode = EnableMode.PlayModeEnable)]
        public void TestPlayModeEnable()
        {
        }

        [EasyButton(EnableMode = EnableMode.EditModeEnable)]
        public void TestEditModeEnable()
        {
        }
        
        [EasyButton(".A", null, false, Order = 1, PrintReturn = PrintLevel.Error)]
        public Transform LogWithInstanceParam(int a, Transform tf, bool c = true)
        {
            Debug.LogError($"a={a} transform={tf}");
            return transform;
        }
    }
}