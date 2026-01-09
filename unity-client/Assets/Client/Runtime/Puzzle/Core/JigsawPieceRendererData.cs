using UnityEngine;

namespace Client.Runtime
{
    public class JigsawPieceRendererData
    {
        public readonly Renderer Mesh;
        public readonly Renderer FlatMesh;
        public readonly Texture2D Texture;

        public JigsawPieceRendererData(Renderer mesh, Renderer flatMesh, Texture2D texture)
        {
            Mesh = mesh;
            FlatMesh = flatMesh;
            Texture = texture;
        }
    }
}