namespace Client.Runtime
{
    public sealed class JigsawPieceData
    {
        public JigsawBoardCell OriginalCell { get; private set; }

        public JigsawPieceData(JigsawBoardCell originalcell)
        {
            OriginalCell = originalcell;
        }
    }
}
