using UnityEngine;

[CreateAssetMenu(fileName = "BoggieFaceData", menuName = "Scriptable Objects/BoggieFaceData")]
public class BoggieFaceData : ScriptableObject
{
    public Material EyeBallMat, EyeMat;
    public Mesh FaceMesh;
}