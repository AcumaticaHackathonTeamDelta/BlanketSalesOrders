using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    [PXProjection(typeof(Select2<SOLine,
        InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOLine.orderType>>,
        InnerJoin<SOOrderTypeOperation,
              On<SOOrderTypeOperation.orderType, Equal<SOLine.orderType>,
                And<SOOrderTypeOperation.operation, Equal<SOLine.operation>>>>>,
        Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>,
        And<SOOrderTypeDAExtension.dAIsBlanketOrder, Equal<True>>>>), new Type[] { typeof(SOLine) }, Persistent = false)]
    [Serializable]
    public class BlanketSOLine : IBqlTable, IItemPlanMaster, ISortOrder
    {
        #region Selected
        public abstract class selected : PX.Data.IBqlField
        {
        }
        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.IBqlField
        {
        }
        protected string _OrderType;
        [PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(SOLine.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region OrderNbr
        public abstract class orderNbr : PX.Data.IBqlField
        {
        }
        protected string _OrderNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(SOLine.orderNbr))]
        //[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<BlanketSOLine.orderType>>, And<SOOrder.orderNbr, Equal<Current<BlanketSOLine.orderNbr>>>>>))]
        public virtual String OrderNbr
        {
            get
            {
                return this._OrderNbr;
            }
            set
            {
                this._OrderNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.IBqlField
        {
        }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(SOLine.lineNbr))]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.IBqlField
        {
        }
        protected Int32? _SortOrder;
        [PXDBInt(BqlField = typeof(SOLine.sortOrder))]
        public virtual Int32? SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                this._SortOrder = value;
            }
        }
        #endregion
        #region LineType
        public abstract class lineType : PX.Data.IBqlField
        {
        }
        protected String _LineType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(SOLine.lineType))]
        public virtual String LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region Operation
        public abstract class operation : PX.Data.IBqlField
        {
        }
        protected String _Operation;
        [PXDBString(1, IsFixed = true, InputMask = ">a", BqlField = typeof(SOLine.operation))]
        [PXUIField(DisplayName = "Operation")]
        [SOOperation.List]
        public virtual String Operation
        {
            get
            {
                return this._Operation;
            }
            set
            {
                this._Operation = value;
            }
        }
        #endregion
        #region ShipComplete
        public abstract class shipComplete : PX.Data.IBqlField
        {
        }
        protected String _ShipComplete;
        [PXDBString(1, IsFixed = true, BqlField = typeof(SOLine.shipComplete))]
        public virtual String ShipComplete
        {
            get
            {
                return this._ShipComplete;
            }
            set
            {
                this._ShipComplete = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.IBqlField
        {
        }
        protected Boolean? _Completed;
        [PXDBBool(BqlField = typeof(SOLine.completed))]
        public virtual Boolean? Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.IBqlField
        {
        }
        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(SOLine.inventoryID))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.IBqlField
        {
        }
        protected Int32? _SubItemID;
        [PXDBInt(BqlField = typeof(SOLine.subItemID))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.IBqlField
        {
        }
        protected Int32? _SiteID;
        [PXDBInt(BqlField = typeof(SOLine.siteID))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region SalesAcctID
        public abstract class salesAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _SalesAcctID;
        [PXDBInt(BqlField = typeof(SOLine.salesAcctID))]
        public virtual Int32? SalesAcctID
        {
            get
            {
                return this._SalesAcctID;
            }
            set
            {
                this._SalesAcctID = value;
            }
        }
        #endregion
        #region SalesSubID
        public abstract class salesSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _SalesSubID;
        [PXDBInt(BqlField = typeof(SOLine.salesSubID))]
        public virtual Int32? SalesSubID
        {
            get
            {
                return this._SalesSubID;
            }
            set
            {
                this._SalesSubID = value;
            }
        }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.IBqlField
        {
        }
        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(SOLine.tranDesc))]
        public virtual String TranDesc
        {
            get
            {
                return this._TranDesc;
            }
            set
            {
                this._TranDesc = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.IBqlField
        {
        }
        protected String _UOM;
        [INUnit(typeof(BlanketSOLine.inventoryID), BqlField = typeof(BlanketSOLine.uOM))]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region OrderQty
        public abstract class orderQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _OrderQty;
        [PXDBDecimal(6, BqlField = typeof(SOLine.orderQty))]
        public virtual Decimal? OrderQty
        {
            get
            {
                return this._OrderQty;
            }
            set
            {
                this._OrderQty = value;
            }
        }
        #endregion
        #region OpenQty
        public abstract class openQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _OpenQty;
        [PXDBDecimal(6, BqlField = typeof(SOLine.openQty))]
        public virtual Decimal? OpenQty
        {
            get
            {
                return this._OpenQty;
            }
            set
            {
                this._OpenQty = value;
            }
        }
        #endregion
        #region BaseShippedQty
        public abstract class baseShippedQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _BaseShippedQty;
        [PXDBBaseQuantity(typeof(BlanketSOLine.uOM), typeof(BlanketSOLine.shippedQty), BqlField = typeof(SOLine.baseShippedQty))]
        public virtual Decimal? BaseShippedQty
        {
            get
            {
                return this._BaseShippedQty;
            }
            set
            {
                this._BaseShippedQty = value;
            }
        }
        #endregion
        #region ShippedQty
        public abstract class shippedQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _ShippedQty;
        [PXDBDecimal(6, BqlField = typeof(SOLine.shippedQty))]
        public virtual Decimal? ShippedQty
        {
            get
            {
                return this._ShippedQty;
            }
            set
            {
                this._ShippedQty = value;
            }
        }
        #endregion
        #region BilledQty
        public abstract class billedQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _BilledQty;
        [PXDBDecimal(6, BqlField = typeof(SOLine.billedQty))]
        public virtual Decimal? BilledQty
        {
            get
            {
                return this._BilledQty;
            }
            set
            {
                this._BilledQty = value;
            }
        }
        #endregion
        #region BaseBilledQty
        public abstract class baseBilledQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _BaseBilledQty;
        [PXDBBaseQuantity(typeof(BlanketSOLine.uOM), typeof(BlanketSOLine.billedQty), BqlField = typeof(SOLine.baseBilledQty))]
        public virtual Decimal? BaseBilledQty
        {
            get
            {
                return this._BaseBilledQty;
            }
            set
            {
                this._BaseBilledQty = value;
            }
        }
        #endregion
        #region UnbilledQty
        public abstract class unbilledQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _UnbilledQty;
        [PXDBQuantity(BqlField = typeof(SOLine.unbilledQty))]
        [PXFormula(null, typeof(SumCalc<SOOrder.unbilledOrderQty>))]
        public virtual Decimal? UnbilledQty
        {
            get
            {
                return this._UnbilledQty;
            }
            set
            {
                this._UnbilledQty = value;
            }
        }
        #endregion
        #region BaseUnbilledQty
        public abstract class baseUnbilledQty : PX.Data.IBqlField
        {
        }
        protected Decimal? _BaseUnbilledQty;
        [PXDBBaseQuantity(typeof(BlanketSOLine.uOM), typeof(BlanketSOLine.unbilledQty), BqlField = typeof(SOLine.baseUnbilledQty))]
        public virtual Decimal? BaseUnbilledQty
        {
            get
            {
                return this._BaseUnbilledQty;
            }
            set
            {
                this._BaseUnbilledQty = value;
            }
        }
        #endregion
        #region CompleteQtyMin
        public abstract class completeQtyMin : PX.Data.IBqlField
        {
        }
        protected Decimal? _CompleteQtyMin;
        [PXDBDecimal(2, MinValue = 0.0, MaxValue = 99.0, BqlField = typeof(SOLine.completeQtyMin))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CompleteQtyMin
        {
            get
            {
                return this._CompleteQtyMin;
            }
            set
            {
                this._CompleteQtyMin = value;
            }
        }
        #endregion
        #region CompleteQtyMax
        public abstract class completeQtyMax : PX.Data.IBqlField
        {
        }
        protected Decimal? _CompleteQtyMax;
        [PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0, BqlField = typeof(SOLine.completeQtyMax))]
        [PXDefault(TypeCode.Decimal, "100.0")]
        public virtual Decimal? CompleteQtyMax
        {
            get
            {
                return this._CompleteQtyMax;
            }
            set
            {
                this._CompleteQtyMax = value;
            }
        }
        #endregion
        #region ShipDate
        public abstract class shipDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _ShipDate;
        [PXDBDate(BqlField = typeof(SOLine.shipDate))]
        public virtual DateTime? ShipDate
        {
            get
            {
                return this._ShipDate;
            }
            set
            {
                this._ShipDate = value;
            }
        }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.IBqlField
        {
        }
        protected Int64? _CuryInfoID;
        [PXDBLong(BqlField = typeof(SOLine.curyInfoID))]
        [CurrencyInfo()]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region CuryUnitPrice
        public abstract class curyUnitPrice : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryUnitPrice;
        [PXDBDecimal(6, BqlField = typeof(SOLine.curyUnitPrice))]
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return this._CuryUnitPrice;
            }
            set
            {
                this._CuryUnitPrice = value;
            }
        }
        #endregion
        #region DiscPct
        public abstract class discPct : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscPct;
        [PXDBDecimal(6, BqlField = typeof(SOLine.discPct))]
        public virtual Decimal? DiscPct
        {
            get
            {
                return this._DiscPct;
            }
            set
            {
                this._DiscPct = value;
            }
        }
        #endregion
        #region CuryUnbilledAmt
        public abstract class curyUnbilledAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryUnbilledAmt;
        [PXDBCurrency(typeof(BlanketSOLine.curyInfoID), typeof(BlanketSOLine.unbilledAmt), BqlField = typeof(SOLine.curyUnbilledAmt))]
        [PXFormula(typeof(Mult<Mult<BlanketSOLine.unbilledQty, BlanketSOLine.curyUnitPrice>, Sub<decimal1, Div<BlanketSOLine.discPct, decimal100>>>))]
        public virtual Decimal? CuryUnbilledAmt
        {
            get
            {
                return this._CuryUnbilledAmt;
            }
            set
            {
                this._CuryUnbilledAmt = value;
            }
        }
        #endregion
        #region UnbilledAmt
        public abstract class unbilledAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _UnbilledAmt;
        [PXDBDecimal(4, BqlField = typeof(SOLine.unbilledAmt))]
        public virtual Decimal? UnbilledAmt
        {
            get
            {
                return this._UnbilledAmt;
            }
            set
            {
                this._UnbilledAmt = value;
            }
        }
        #endregion
        #region GroupDiscountRate
        public abstract class groupDiscountRate : PX.Data.IBqlField
        {
        }
        protected Decimal? _GroupDiscountRate;
        [PXDBDecimal(6, BqlField = typeof(SOLine.groupDiscountRate))]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? GroupDiscountRate
        {
            get
            {
                return this._GroupDiscountRate;
            }
            set
            {
                this._GroupDiscountRate = value;
            }
        }
        #endregion
        #region DocumentDiscountRate
        public abstract class documentDiscountRate : PX.Data.IBqlField
        {
        }
        protected Decimal? _DocumentDiscountRate;
        [PXDBDecimal(6, BqlField = typeof(SOLine.documentDiscountRate))]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? DocumentDiscountRate
        {
            get
            {
                return this._DocumentDiscountRate;
            }
            set
            {
                this._DocumentDiscountRate = value;
            }
        }
        #endregion
        #region TaxCategoryID
        public abstract class taxCategoryID : PX.Data.IBqlField
        {
        }
        protected String _TaxCategoryID;
        [PXDBString(10, IsUnicode = true, BqlField = typeof(SOLine.taxCategoryID))]
        [SOUnbilledTax2(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran))]
        public virtual String TaxCategoryID
        {
            get
            {
                return this._TaxCategoryID;
            }
            set
            {
                this._TaxCategoryID = value;
            }
        }
        #endregion
        #region PlanType
        public abstract class planType : PX.Data.IBqlField
        {
        }
        protected String _PlanType;
        [PXDBString(2, IsFixed = true, BqlField = typeof(SOOrderTypeOperation.orderPlanType))]
        public virtual String PlanType
        {
            get
            {
                return this._PlanType;
            }
            set
            {
                this._PlanType = value;
            }
        }
        #endregion
        #region RequireLocation
        public abstract class requireLocation : PX.Data.IBqlField
        {
        }
        protected Boolean? _RequireLocation;
        [PXDBBool(BqlField = typeof(SOOrderType.requireLocation))]
        public virtual Boolean? RequireLocation
        {
            get
            {
                return this._RequireLocation;
            }
            set
            {
                this._RequireLocation = value;
            }
        }
        #endregion
        #region POSource
        public abstract class pOSource : PX.Data.IBqlField
        {
        }
        protected string _POSource;
        [PXDBString(BqlField = typeof(SOLine.pOSource))]
        public virtual string POSource
        {
            get
            {
                return this._POSource;
            }
            set
            {
                this._POSource = value;
            }
        }
        #endregion

        #region LastModifiedByID
        public abstract class lastModifiedByID : IBqlField { }

        [PXDBLastModifiedByID(BqlField = typeof(SOLine.lastModifiedByID))]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : IBqlField { }

        [PXDBLastModifiedByScreenID(BqlField = typeof(SOLine.lastModifiedByScreenID))]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : IBqlField { }

        [PXDBLastModifiedDateTime(BqlField = typeof(SOLine.lastModifiedDateTime))]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : IBqlField { }

        [PXDBTimestamp(BqlField = typeof(SOLine.Tstamp), RecordComesFirst = true)]
        public virtual byte[] tstamp { get; set; }
        #endregion
    }
}