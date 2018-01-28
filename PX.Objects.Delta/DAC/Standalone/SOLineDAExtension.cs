using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.Delta.Standalone
{
    [Serializable]
    public class SOLineDAExtension : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.IBqlField
        {
        }
        protected String _OrderType;
        [PXDBString(2, IsKey = true, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
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
        protected String _OrderNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
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
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false)]
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

        #region DABlanketOrderType
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Blanket Order Type", Enabled = false)]
        public virtual string DABlanketOrderType { get; set; }
        public abstract class dABlanketOrderType : IBqlField { }
        #endregion

        #region DABlanketOrderNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Blanket Order Nbr", Enabled = false)]
        public virtual string DABlanketOrderNbr { get; set; }
        public abstract class dABlanketOrderNbr : IBqlField { }
        #endregion

        #region DABlanketLineNbr
        [PXDBInt]
        [PXUIField(DisplayName = "Blanket Line Nbr", Enabled = false)]
        public virtual int? DABlanketLineNbr { get; set; }
        public abstract class dABlanketLineNbr : IBqlField { }
        #endregion

        #region DABlanketOrderQty
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity(typeof(SOLine.uOM), typeof(SOLineDAExtension.dABlanketBaseOrderQty))]
        [PXUIField(DisplayName = "Qty On Orders", Enabled = false)]
        public virtual decimal? DABlanketOrderQty { get; set; }
        public abstract class dABlanketOrderQty : IBqlField { }
        #endregion

        #region DABlanketBaseOrderQty
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity()]
        [PXUIField(DisplayName = "Base Qty On Orders", Enabled = false)]
        public virtual decimal? DABlanketBaseOrderQty { get; set; }
        public abstract class dABlanketBaseOrderQty : IBqlField { }
        #endregion
    }
}
