using System;
using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    /// <summary>
    /// Sales Order Entry - Delta graph extension
    /// </summary>
    public class SOOrderEntryDAExtension : PXGraphExtension<SOOrderEntry>
    {
        public PXSelect<BlanketSOLine, 
            Where<BlanketSOLine.openQty, Greater<decimal0>,
            And<BlanketSOLine.customerID, Equal<Current<SOOrder.customerID>>>>> blanketLinesSelected;

        // Required to extend the shipments tab - queried results for blanket orders
        public PXSelectJoin<SOOrderShipment,
            LeftJoin<SOShipment, On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
                And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>,
            Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
                And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>> shipmentlist;

        /// <summary>
        /// Does the current order type indicate the order is a blanket order
        /// </summary>
        public bool IsBlanketOrder => GetCurrentORderTypeExtension?.DAIsBlanketOrder ?? false;

        //TODO: indicator for sales order type (excluding transfers, quotes, etc.)
        //public bool IsSalesOrderType => Base?.soordertype?.Current?.Behavior;

        protected virtual SOOrderTypeDAExtension GetCurrentORderTypeExtension => Base?.soordertype?.Current?.GetExtension<SOOrderTypeDAExtension>();

        public override void Initialize()
        {
            base.Initialize();

            PXUIFieldAttribute.SetEnabled<SOLineSplit.shipDate>(Base.splits.Cache, null, true);
            PXUIFieldAttribute.SetEnabled<SOLineDAExtension.dABlanketOrderQty>(Base.Transactions.Cache, null, false);
        }

        public virtual void SOOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(cache, e);
            AddBlanketOrderLineAction.SetEnabled(IsBlanketOrder);
            PXUIFieldAttribute.SetVisible<SOLineDAExtension.dABlanketOrderQty>(Base.Transactions.Cache, null, IsBlanketOrder);
        }

        protected virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            var oldRow = (SOLine)e.OldRow;
            if (row == null || oldRow == null)
            {
                return;
            }

            //TODO: use base qty in case order UOM diff from blanket

            var rowExt = row.GetExtension<SOLineDAExtension>();
            var orderQtyChange = oldRow.OrderQty.GetValueOrDefault() - row.OrderQty.GetValueOrDefault();
            if (orderQtyChange == 0)
            {
                return;
            }

            if (IsBlanketOrder)
            {
                row.OpenQty = Math.Max(row.OrderQty.GetValueOrDefault() - rowExt.DABlanketOrderQty.GetValueOrDefault(), 0m);
                return;
            }

            UpdateParentBlanketOrderLine(row, orderQtyChange * -1);
        }

        protected virtual void SOLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            del?.Invoke(sender, e);

            var row = (SOLine)e.Row;
            if (row == null || IsBlanketOrder)
            {
                return;
            }

            UpdateParentBlanketOrderLine(row, row.OrderQty.GetValueOrDefault() * -1);
        }

        /// <summary>
        /// Update the parent blanket order line to reflect the child line qty changing
        /// </summary>
        /// <param name="childOrderLine">child row of blanket order to update</param>
        /// <param name="orderQtyChange">qty changing (+/-)</param>
        protected virtual void UpdateParentBlanketOrderLine(SOLine childOrderLine, decimal orderQtyChange)
        {
            if (childOrderLine == null || orderQtyChange == 0m)
            {
                return;
            }

            var rowExt = childOrderLine.GetExtension<SOLineDAExtension>();
            var parentBlanketLine = (BlanketSOLine)PXSelect<BlanketSOLine,
                Where<BlanketSOLine.orderType, Equal<Required<BlanketSOLine.orderType>>,
                    And<BlanketSOLine.orderNbr, Equal<Required<BlanketSOLine.orderNbr>>,
                        And<BlanketSOLine.lineNbr, Equal<Required<BlanketSOLine.lineNbr>>>>>>.Select(Base,
                rowExt.DABlanketOrderType, rowExt.DABlanketOrderNbr, rowExt.DABlanketLineNbr);

            if (parentBlanketLine == null)
            {
                return;
            }

            parentBlanketLine.DABlanketOrderQty += orderQtyChange;
            if (parentBlanketLine.DABlanketOrderQty.GetValueOrDefault() < 0m)
            {
                parentBlanketLine.DABlanketOrderQty = 0m;
            }
            parentBlanketLine.OpenQty = Math.Max(parentBlanketLine.OrderQty.GetValueOrDefault() - parentBlanketLine.DABlanketOrderQty.GetValueOrDefault(), 0m);
            blanketLinesSelected.Update(parentBlanketLine);
        }

        /// <summary>
        /// ACtion to add blanket order lines to a base order
        /// </summary>
        public PXAction<SOOrder> AddBlanketOrderLineAction;

        [PXUIField(DisplayName = "Add Blanket Order Line", MapEnableRights = PXCacheRights.Select,
            MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton]
        public virtual IEnumerable addBlanketOrderLineAction(PXAdapter adapter)
        {
            if (IsBlanketOrder)
            {
                return adapter.Get();
            }

            if(blanketLinesSelected.AskExt() != WebDialogResult.OK)
            {
                foreach (BlanketSOLine row in blanketLinesSelected.Cache.Updated)
                {
                    row.Selected = false;
                }
                return adapter.Get();
            }

            foreach (BlanketSOLine row in blanketLinesSelected.Cache.Updated)
            {
                AddSOLine(row);
                row.Selected = false;
            }

            return adapter.Get();
        }

        private void AddSOLine(BlanketSOLine row)
        {
            // Add the lines to the order...
            if (row?.OrderNbr == null)
            {
                return;
            }

            var newRow = Base.Transactions.Insert(new SOLine());
            newRow.InventoryID = row.InventoryID;
            newRow.SubItemID = row.SubItemID;
            newRow.SiteID = row.SiteID;
            newRow.LocationID = row.LocationID;
            newRow.OrderQty = row.OpenQty;
            newRow.TranDesc = row.TranDesc;

            var newRowExt = newRow.GetExtension<SOLineDAExtension>();
            if (newRowExt != null)
            {
                newRowExt.DABlanketOrderType = row.OrderType;
                newRowExt.DABlanketOrderNbr = row.OrderNbr;
                newRowExt.DABlanketLineNbr = row.LineNbr;
            }

            Base.Transactions.Update(newRow);
        }

        public IEnumerable Shipmentlist()
        {
            if (IsBlanketOrder)
            {
                // list out shipments for the blanket order
                foreach (var shipline in PXSelectJoinGroupBy<SOLine, InnerJoin<SOShipLine,
                                            On<SOLine.orderType, Equal<SOShipLine.origOrderType>,
                                                And<SOLine.orderNbr, Equal<SOShipLine.origOrderNbr>,
                                                And<SOLine.lineNbr, Equal<SOShipLine.origLineNbr>>>>>,
                                        Where<SOLineDAExtension.dABlanketOrderType, Equal<Current<SOOrder.orderType>>,
                                            And<SOLineDAExtension.dABlanketOrderNbr, Equal<Current<SOOrder.orderNbr>>>>,
                                        Aggregate<GroupBy<SOLine.orderType, GroupBy<SOLine.orderNbr>>>>.Select(Base))
                {
                    var soline = (SOLine)shipline;
                    foreach (var line in PXSelectJoin<SOOrderShipment,
                                            LeftJoin<SOShipment, On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
                                                And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>,
                                            Where<SOOrderShipment.orderType, Equal<Required<SOOrder.orderType>>,
                                                And<SOOrderShipment.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(Base, soline.OrderType, soline.OrderNbr))
                    {
                        yield return line;
                    }
                }
            }
            else
            {
                // when not blanket order
                foreach (var line in PXSelectJoin<SOOrderShipment,
                                        LeftJoin<SOShipment, On<SOShipment.shipmentNbr, Equal<SOOrderShipment.shipmentNbr>,
                                            And<SOShipment.shipmentType, Equal<SOOrderShipment.shipmentType>>>>,
                                        Where<SOOrderShipment.orderType, Equal<Current<SOOrder.orderType>>,
                                            And<SOOrderShipment.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.Select(Base))
                {
                    yield return line;
                }
            }
        }

    }
}