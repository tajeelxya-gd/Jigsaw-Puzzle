using UnityEngine;

namespace Client.Runtime
{
    public class JigsawPieceRendererData
    {
        public readonly Renderer Mesh;
        public readonly Vector3 TrayEulers;
        public readonly Vector3 IdleShadowOffset;
        public readonly Vector3 HoverShadowOffset;

        public JigsawPieceRendererData(Renderer mesh, Vector3 trayEulers, Vector3 idleShadowOffset, Vector3 hoverShadowOffset)
        {
            Mesh = mesh;
            TrayEulers = trayEulers;
            IdleShadowOffset = idleShadowOffset;
            HoverShadowOffset = hoverShadowOffset;
        }
    }
}