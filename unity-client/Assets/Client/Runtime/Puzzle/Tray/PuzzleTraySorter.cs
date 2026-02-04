using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleTraySorter
    {
        List<JigsawPiece> Sort(IEnumerable<JigsawPiece> pieces, int difficulty);
    }

    public sealed class PuzzleTraySorter : IPuzzleTraySorter
    {
        public List<JigsawPiece> Sort(IEnumerable<JigsawPiece> pieces, int difficulty)
        {
            var edgePieces = new List<JigsawPiece>();
            var otherPieces = new List<JigsawPiece>();

            foreach (var p in pieces)
            {
                if (JigsawBoardCalculator.IsEdge(p.CorrectIdx)) edgePieces.Add(p);
                else otherPieces.Add(p);
            }

            // Ensure edges are initially sorted logic-wise if we want to pick "top-left" pieces for sorted parts
            edgePieces.Sort((a, b) => a.CorrectIdx.CompareTo(b.CorrectIdx));
            ShuffleList(otherPieces);

            // 1. Calculations for distribution ratios
            float frontEdgesRatio = 1.0f;
            if (difficulty > 3)
            {
                // Drops from 1.0 at diff 3 to 0.5 at diff 5, then reaching 0.0 at diff 7
                frontEdgesRatio = Mathf.Max(0f, 1.0f - (difficulty - 3) * 0.25f);
            }

            float sortingRatio = 0.0f;
            if (difficulty < 3)
            {
                // 1.0 at diff 0, 2/3 at diff 1, 1/3 at diff 2, 0 at diff 3
                sortingRatio = 1.0f - (difficulty / 3.0f);
            }

            float othersToFrontRatio = 0.0f;
            if (difficulty > 5)
            {
                // 0.0 at diff 5, reaching 1.0 at diff 10
                othersToFrontRatio = (difficulty - 5f) / 5.0f;
            }

            // 2. Fragment the lists
            int fEdgesCount = Mathf.RoundToInt(edgePieces.Count * frontEdgesRatio);
            int sEdgesCount = Mathf.RoundToInt(fEdgesCount * sortingRatio);

            var frontSortedEdges = edgePieces.GetRange(0, sEdgesCount);
            var frontRandomEdges = edgePieces.GetRange(sEdgesCount, fEdgesCount - sEdgesCount);
            var remainingEdges = edgePieces.GetRange(fEdgesCount, edgePieces.Count - fEdgesCount);

            int fOthersCount = Mathf.RoundToInt(otherPieces.Count * othersToFrontRatio);
            var frontOthers = otherPieces.GetRange(0, fOthersCount);
            var remainingOthers = otherPieces.GetRange(fOthersCount, otherPieces.Count - fOthersCount);

            // 3. Asset assembly
            var result = new List<JigsawPiece>();

            // Priority 1: Hider pieces (Difficulty 5+)
            result.AddRange(frontOthers);

            // Priority 2: Sorted edges
            result.AddRange(frontSortedEdges);

            // Priority 3: Random edges (Difficulty 0-10)
            ShuffleList(frontRandomEdges);
            result.AddRange(frontRandomEdges);

            // Priority 4: Background (Everything else shuffled together)
            var backPool = new List<JigsawPiece>(remainingEdges);
            backPool.AddRange(remainingOthers);
            ShuffleList(backPool);
            result.AddRange(backPool);

            return result;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}
