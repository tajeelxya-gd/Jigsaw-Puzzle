using UnityEngine;

namespace Client.Runtime
{
    public class JigsawPieceRendererData
    {
        public readonly Renderer Mesh;
        public readonly Renderer FlatMesh;

        public JigsawPieceRendererData(Renderer mesh, Renderer flatMesh)
        {
            Mesh = mesh;
            FlatMesh = flatMesh;
        }
    }
}