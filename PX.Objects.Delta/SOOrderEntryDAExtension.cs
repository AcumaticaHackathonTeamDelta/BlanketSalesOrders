﻿using System;
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

            PXUIFieldAttribute.SetVisible<SOLineDAExtension.dABlanketOrderQty>(Base.Transactions.Cache, null, IsBlanketOrder);
        }

        protected virtual void SOLine_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (SOLine)e.Row;
            var oldRow = (SOLine)e.OldRow;
            if (row == null || oldRow == null)
            {
                return;
            }

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

            var parentBlanketLine = (BlanketSOLine)PXSelect<BlanketSOLine,
                Where<BlanketSOLine.orderType, Equal<Required<BlanketSOLine.orderType>>,
                    And<BlanketSOLine.orderNbr, Equal<Required<BlanketSOLine.orderNbr>>,
                        And<BlanketSOLine.lineNbr, Equal<Required<BlanketSOLine.lineNbr>>>>>>.Select(Base,
                rowExt.DABlanketOrderType, rowExt.DABlanketOrderNbr, rowExt.DABlanketLineNbr);

            if (parentBlanketLine == null)
            {
                return;
            }

            parentBlanketLine.DABlanketOrderQty += orderQtyChange * -1;
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

            var newRowExt = newRow.GetExtension<SOLineDAExtension>();
            if (newRowExt != null)
            {
                newRowExt.DABlanketOrderType = row.OrderType;
                newRowExt.DABlanketOrderNbr = row.OrderNbr;
                newRowExt.DABlanketLineNbr = row.LineNbr;
            }

            Base.Transactions.Update(newRow);
        }
    }
}