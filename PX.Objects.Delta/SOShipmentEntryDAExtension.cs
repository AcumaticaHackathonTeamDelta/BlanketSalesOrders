using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    public class SOShipmentEntryDAExtension : PXGraphExtension<SOShipmentEntry>
    {
        protected virtual void SOLine2_BaseShippedQty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);
            var row = (SOLine2)e.Row;
            if (row == null)
            {
                return;
            }

            var oldValue = (decimal?)e.OldValue ?? 0m;
            var shipQtyChange = row.BaseShippedQty.GetValueOrDefault() - oldValue;

            UpdateParentBlanketOrderLine(row, shipQtyChange);
        }

        public virtual void UpdateParentBlanketOrderLine(SOLine2 childOrderLine, decimal shipQtyChange)
        {
            if (childOrderLine == null || shipQtyChange == 0m)
            {
                return;
            }

            var parentBlanketLine = (SOLine2)PXSelectJoin<SOLine2,
                InnerJoin<Standalone.SOLineDAExtension,
                    On<SOLine2.orderType, Equal<Standalone.SOLineDAExtension.dABlanketOrderType>,
                        And<SOLine2.orderNbr, Equal<Standalone.SOLineDAExtension.dABlanketOrderNbr>,
                            And<SOLine2.lineNbr, Equal<Standalone.SOLineDAExtension.dABlanketLineNbr>>>>>,
                Where<Standalone.SOLineDAExtension.orderType, Equal<Required<Standalone.SOLineDAExtension.orderType>>,
                    And<Standalone.SOLineDAExtension.orderNbr, Equal<Required<Standalone.SOLineDAExtension.orderNbr>>,
                        And<Standalone.SOLineDAExtension.lineNbr, Equal<Required<Standalone.SOLineDAExtension.lineNbr>>>>>
            >.Select(Base, childOrderLine.OrderType, childOrderLine.OrderNbr, childOrderLine.LineNbr);

            if (parentBlanketLine == null)
            {
                return;
            }

            parentBlanketLine.BaseShippedQty += shipQtyChange;
            if (parentBlanketLine.BaseShippedQty.GetValueOrDefault() < 0m)
            {
                parentBlanketLine.BaseShippedQty = 0m;
            }
            Base.soline.Update(parentBlanketLine);
        }
    }
}