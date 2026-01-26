using UnityEngine;

namespace Client.Runtime
{
    public class JigsawPieceRendererData
    {
        public readonly Renderer Mesh;
        public readonly Vector3 TrayEulers;

        public JigsawPieceRendererData(Renderer mesh, Vector3 trayEulers)
        {
            Mesh = mesh;
            TrayEulers = trayEulers;
        }
    }
}