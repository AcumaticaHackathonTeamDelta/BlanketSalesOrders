using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.Common;

namespace PX.Objects.Delta
{
    [PXTable(typeof(SOLine.orderType), typeof(SOLine.orderNbr), typeof(SOLine.lineNbr), IsOptional = true)]
    [Serializable]
    public class SOLineDAExtension : PXCacheExtension<SOLine>
    {
        #region Selected
        public abstract class dAselected : PX.Data.IBqlField
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
        #region DABlanketOrderType
        [PXDBString(2, IsKey = true, IsFixed = true, InputMask = "")]
        [PXUIField(DisplayName = "DABlanket Order Type")]
        public virtual string DABlanketOrderType { get; set; }
        public abstract class dABlanketOrderType : IBqlField { }
        #endregion

        #region DABlanketOrderNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "DABlanket Order Nbr")]
        public virtual string DABlanketOrderNbr { get; set; }
        public abstract class dABlanketOrderNbr : IBqlField { }
        #endregion

        #region DABlanketLineNbr
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "DABlanket Line Nbr")]
        public virtual int? DABlanketLineNbr { get; set; }
        public abstract class dABlanketLineNbr : IBqlField { }
        #endregion

        #region DABlanketOrderQty
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity(typeof(SOLine.uOM), typeof(SOLineDAExtension.dABlanketBaseOrderQty))]
        [PXUIField(DisplayName = "Blanket Order Qty")]
        public virtual decimal? DABlanketOrderQty { get; set; }
        public abstract class dABlanketOrderQty : IBqlField { }
        #endregion

        #region DABlanketBaseOrderQty
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity()]
        [PXUIField(DisplayName = "Blanket Base Order Qty")]
        public virtual decimal? DABlanketBaseOrderQty { get; set; }
        public abstract class dABlanketBaseOrderQty : IBqlField { }
        #endregion
    }
}
