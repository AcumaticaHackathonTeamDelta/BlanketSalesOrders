using System;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	/// <summary>
	/// Represents a tax detail of an Expense Receipt document. 
	/// The entities of this type are edited on the Expense Receipt
	/// (EP301020) form, which correspond to 
	/// the <see cref="ExpenseClaimDetailEntry"/> graph.
	/// </summary>
	/// <remarks>
	/// Tax details are aggregates combined by <see cref="TaxBaseAttribute"/> 
	/// descendants from the line-level <see cref="EPTax"/> records.
	/// </remarks>
	[Serializable]
	public partial class EPTaxTran : TaxDetail, IBqlTable
	{
		#region ClaimDetailID
		public abstract class claimDetailID : IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(EPExpenseClaimDetails.claimDetailID))]
		[PXParent(typeof(Select<EPExpenseClaimDetails,
								Where<EPExpenseClaimDetails.claimDetailID, Equal<Current<EPTaxTran.claimDetailID>>>>))]
		public virtual int? ClaimDetailID
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true)]
		[PXDBDefault(typeof(EPExpenseClaimDetails.refNbr), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region TaxID
		public abstract class taxID : IBqlField
		{
		}
		[PXDBString(Tax.taxID.Length, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Tax ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public override string TaxID
		{
			get
			{
				return _TaxID;
			}
			set
			{
				_TaxID = value;
			}
		}
		#endregion
		#region IsTipTax
		public abstract class isTipTax : IBqlField { }
		[PXDBBool(IsKey = true)]
		[PXDefault(false)]
		public virtual bool? IsTipTax
		{
			get;
			set;
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : IBqlField
		{
		}
		[PXDBLong]
		[CurrencyInfo(typeof(EPExpenseClaimDetails.curyInfoID))]
		public override long? CuryInfoID
		{
			get
			{
				return _CuryInfoID;
			}
			set
			{
				_CuryInfoID = value;
			}
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : IBqlField
		{
		}

		/// <summary>
		/// The tax rate of the relevant <see cref="Tax"/> record.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public override decimal? TaxRate
		{
			get
			{
				return _TaxRate;
			}
			set
			{
				_TaxRate = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : IBqlField
		{
		}
		[PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]

		[PXUnboundFormula(typeof(Switch<Case<Where<WhereExempt<taxID>>, curyTaxableAmt>, decimal0>), typeof(SumCalc<EPExpenseClaimDetails.curyVatExemptTotal>), CancelParentUpdate = true)]
		[PXUnboundFormula(typeof(Switch<Case<Where<WhereTaxable<taxID>>, curyTaxableAmt>, decimal0>), typeof(SumCalc<EPExpenseClaimDetails.curyVatTaxableTotal>), CancelParentUpdate = true)]
		public decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : IBqlField
		{
		}
		[PXDBBaseCury]
		public decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : IBqlField
		{
		}
		[PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : IBqlField
		{
		}
		[PXDBBaseCury]
		public decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : IBqlField
		{
		}
		[PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.expenseAmt))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public override decimal? CuryExpenseAmt
		{
			get;
			set;
		}
		#endregion
		#region ExpenseAmt
		public abstract class expenseAmt : IBqlField
		{
		}
		[PXDBBaseCury]
		public override decimal? ExpenseAmt
		{
			get;
			set;
		}
        #endregion

        #region ClaimCuryTaxableAmt
        public abstract class claimCuryTaxableAmt : IBqlField
        {
        }
        [PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxableAmt), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]

        public decimal? ClaimCuryTaxableAmt
        {
            get;
            set;
        }
        #endregion
        #region ClaimCuryTaxAmt
        public abstract class claimCuryTaxAmt : IBqlField
        {
        }
        [PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.taxAmt), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
        public decimal? ClaimCuryTaxAmt
        {
            get;
            set;
        }
        #endregion
        #region ClaimCuryExpenseAmt
        public abstract class claimCuryExpenseAmt : IBqlField
        {
        }
        [PXDBCurrency(typeof(EPTaxTran.curyInfoID), typeof(EPTaxTran.expenseAmt), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
        public decimal? ClaimCuryExpenseAmt
        {
            get;
            set;
        }
        #endregion

        public abstract class nonDeductibleTaxRate : IBqlField
		{
		}
	}
}
