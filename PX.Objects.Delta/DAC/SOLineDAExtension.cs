﻿using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.SO;

namespace PX.Objects.Delta
{
    [PXTable(typeof(SOLine.orderType), typeof(SOLine.orderNbr), typeof(SOLine.lineNbr), IsOptional = true)]
    [Serializable]
    public class SOLineDAExtension : PXCacheExtension<SOLine>
    {
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
