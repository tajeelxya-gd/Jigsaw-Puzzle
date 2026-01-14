using System;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public static class InputHandler
    {
        private static bool _init;

        public static bool _3DActive { get; private set; }

        public static void Init()
        {
            if (_init) return;
            UniWidgets.OnPop += SetActive;
            UniWidgets.OnPush += SetActive;
            SetActive(null);
            _init = true;
        }

        private static void SetActive(Type type) => _3DActive = UniWidgets.Peek() == null;
    }
}