using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    [PXTable(typeof(SOLine.orderType), typeof(SOLine.orderNbr), typeof(SOLine.lineNbr), IsOptional = true)]
    [Serializable]
    public class SOLineDAExtension : PXCacheExtension<SOLine>
    {
        //#region DASelected
        //public abstract class dASelected : PX.Data.IBqlField
        //{
        //}
        //protected bool? _DASelected = false;
        //[PXBool]
        //[PXDefault(false)]
        //[PXUIField(DisplayName = "Selected")]
        //public virtual bool? DASelected
        //{
        //    get
        //    {
        //        return _DASelected;
        //    }
        //    set
        //    {
        //        _DASelected = value;
        //    }
        //}
        //#endregion
        #region DABlanketOrderType
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Blanket Order Type")]
        public virtual string DABlanketOrderType { get; set; }
        public abstract class dABlanketOrderType : IBqlField { }
        #endregion

        #region DABlanketOrderNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Blanket Order Nbr")]
        public virtual string DABlanketOrderNbr { get; set; }
        public abstract class dABlanketOrderNbr : IBqlField { }
        #endregion

        #region DABlanketLineNbr
        [PXDBInt]
        [PXUIField(DisplayName = "Blanket Line Nbr")]
        public virtual int? DABlanketLineNbr { get; set; }
        public abstract class dABlanketLineNbr : IBqlField { }
        #endregion

        #region DABlanketOrderQty
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBQuantity(typeof(SOLine.uOM), typeof(SOLineDAExtension.dABlanketBaseOrderQty))]
        [PXUIField(DisplayName = "Qty On Open Orders")]
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
