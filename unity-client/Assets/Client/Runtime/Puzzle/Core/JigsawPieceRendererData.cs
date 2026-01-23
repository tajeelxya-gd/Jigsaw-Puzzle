using UnityEngine;

namespace Client.Runtime
{
    public class JigsawPieceRendererData
    {
        public readonly Renderer Mesh;

        public JigsawPieceRendererData(Renderer mesh)
        {
            Mesh = mesh;
        }
    }
}