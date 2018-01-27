using System;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.Delta.DAC
{
    /// <summary>
    /// Delta extension table for sales order types
    /// </summary>
    [PXTable(typeof(SOOrderType.orderType), IsOptional = true)]
    [Serializable]
    public class SOOrderTypeDAExtension : PXCacheExtension<SOOrderType>
    {
        #region DAIsBlanketOrder
        /// <summary>
        /// Defines a sales order type as a blanket order
        /// </summary>
        public abstract class dAIsBlanketOrder : IBqlField
        {
        }
        protected bool? _DAIsBlanketOrder = false;
        /// <summary>
        /// Defines a sales order type as a blanket order
        /// </summary>
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Blanket Order Type")]
        public virtual bool? DAIsBlanketOrder
        {
            get
            {
                return _DAIsBlanketOrder;
            }
            set
            {
                _DAIsBlanketOrder = value;
            }
        }
        #endregion
    }

    /*
        CREATE TABLE [dbo].[SOOrderTypeDAExtension]
        (
        [CompanyID] [int] NOT NULL DEFAULT ((0)),
        [OrderType] [char] (2) NOT NULL,
        [DAIsBlanketOrder] [bit] NULL
        ) ON [PRIMARY]
        GO
        ALTER TABLE [dbo].[SOOrderTypeDAExtension] ADD CONSTRAINT [SOOrderTypeDAExtension_PK] PRIMARY KEY CLUSTERED ([CompanyID], [OrderType]) ON [PRIMARY]
        GO
     */
}