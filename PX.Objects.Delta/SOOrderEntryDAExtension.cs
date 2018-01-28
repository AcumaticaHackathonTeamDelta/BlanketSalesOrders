using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    /// <summary>
    /// Sales Order Entry - Delta graph extension
    /// </summary>
    public class SOOrderEntryDAExtension : PXGraphExtension<SOOrderEntry>
    {
        /// <summary>
        /// Does the current order type indicate the order is a blanket order
        /// </summary>
        public bool IsBlanketOrder => GetCurrentORderTypeExtension?.DAIsBlanketOrder ?? false;

        protected virtual SOOrderTypeDAExtension GetCurrentORderTypeExtension => Base?.soordertype?.Current?.GetExtension<SOOrderTypeDAExtension>();

        public SOOrderEntryDAExtension()
        {
            PXUIFieldAttribute.SetEnabled<SOLineSplit.shipDate>(Base.splits.Cache, null, true);
        }
    }
}