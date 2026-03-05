using System;
using UniTx.Runtime.Widgets;

namespace Client.Runtime
{
    public static class InputHandler
    {
        public static bool _3DActive { get; private set; }

        public static void SetActive(bool active) => _3DActive = active;
    }
}