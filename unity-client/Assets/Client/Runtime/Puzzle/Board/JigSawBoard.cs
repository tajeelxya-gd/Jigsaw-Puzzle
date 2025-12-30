using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class JigSawBoard : EntityBase<JigSawBoardData, JigSawBoardSavedData>
    {
        public JigSawBoard(string id) : base(id)
        {
            // Empty yet
        }

        protected override void OnInject(IResolver resolver)
        {
            // Empty yet
        }

        protected override void OnInit()
        {
            // Empty yet
        }

        protected override void OnReset()
        {
            // Empty yet
        }
    }
}