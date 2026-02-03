using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public class Currency : EntityBase<CurrencyData, CurrencySavedData>
    {
        public Currency(string id) : base(id)
        {
            // Empty
        }

        protected override void OnInject(IResolver resolver) { }

        protected override void OnInit() { }

        protected override void OnReset() { }
    }
}