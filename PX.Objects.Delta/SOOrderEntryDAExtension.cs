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
        //View for Add Blanket lines panel
        //public PXSelectJoin<SOLine,
        //    InnerJoin<SOOrderType, On<SOLine.orderType, Equal<SOOrderType.orderType>>>,
        //    Where<SOOrderTypeDAExtension.dAIsBlanketOrder, Equal<True>,
        //        And<SOLine.openQty, Greater<decimal0>>>,
        //    OrderBy<Asc<SOLine.orderType, Asc<SOLine.orderNbr, Asc<SOLine.sortOrder, Asc<SOLine.lineNbr>>>>>> blanketLinesSelected;

        public PXSelect<BlanketSOLine, Where<BlanketSOLine.openQty, Greater<decimal0>>> blanketLinesSelected;

        //public PXSelect<SOLine,
        //    Where<SOLineDAExtension.dABlanketOrderType, Equal<Optional<SOLineDAExtension.dABlanketOrderType>>,
        //        And<SOLineDAExtension.dABlanketOrderNbr, Equal<Optional<SOLineDAExtension.dABlanketOrderNbr>>,
        //            And<SOLineDAExtension.dABlanketLineNbr, Equal<Optional<SOLineDAExtension.dABlanketLineNbr>>>>>> blanketLinkedLines;

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

            // Need to disable/hide add blanket lines for blanket orders or non SO types...

            //hide custom open order qty column of soline extension
            PXUIFieldAttribute.SetVisible<SOLineDAExtension.dABlanketOrderQty>(Base.Transactions.Cache, null, IsBlanketOrder);

        }

        protected virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (SOLine)e.Row;
            var oldRow = (SOLine)e.OldRow;
            if (row == null || oldRow == null || !IsBlanketOrder)
            {
                return;
            }

            var rowExt = row.GetExtension<SOLineDAExtension>();

            var orderQtyChange = oldRow.OrderQty.GetValueOrDefault() - row.OrderQty.GetValueOrDefault();
            if (orderQtyChange != 0)
            {
                row.OpenQty = Math.Max(row.OrderQty.GetValueOrDefault() - rowExt.DABlanketOrderQty.GetValueOrDefault(), 0m);
            }
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
            //if (this.Document.Current != null &&
            //    this.Document.Current.Hold == true &&
            //    POOrderType.IsUseBlanket(this.Document.Current.OrderType))
            //{
            //    if (this.poLinesSelection.AskExt() == WebDialogResult.OK)
            //    {
            //        foreach (POLineS it in poLinesSelection.Cache.Updated)
            //        {
            //            if ((bool)it.Selected)
            //                this.AddPOLine(it, this.filter.Current.OrderType == POOrderType.Blanket);
            //            it.Selected = false;
            //        }
            //    }
            //    else
            //    {
            //        foreach (POLineS it in poLinesSelection.Cache.Updated)
            //            it.Selected = false;
            //    }
            //}

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
        }
    }
}