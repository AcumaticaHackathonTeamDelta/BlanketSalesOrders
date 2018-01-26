using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	[Serializable]
	[PXCacheName(Messages.CATax)]
	public partial class CATax : TaxDetail, IBqlTable, ITranTax
	{
		#region AdjTranType
		public abstract class adjTranType : IBqlField
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(CAAdj.adjTranType))]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string AdjTranType
		{
			get;
			set;
		}
		#endregion
		#region AdjRefNbr
		public abstract class adjRefNbr : IBqlField
		{
		}

		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(CAAdj.adjRefNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string AdjRefNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : IBqlField
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXParent(typeof(Select<CASplit, Where<CASplit.adjTranType, Equal<Current<CATax.adjTranType>>, And<CASplit.adjRefNbr, Equal<Current<CATax.adjRefNbr>>, And<CASplit.lineNbr, Equal<Current<CATax.lineNbr>>>>>>))]
		public virtual int? LineNbr
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
		[PXDefault]
		[PXUIField(DisplayName = "Tax ID")]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public override string TaxID
		{
			get
			{
				return this._TaxID;
			}

			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : IBqlField
		{
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : IBqlField
		{
		}
		[PXDBLong]
		[CurrencyInfo(typeof(CAAdj.curyInfoID))]
		public override long? CuryInfoID
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
		#region CuryOrigTaxableAmt
		public abstract class curyOrigTaxableAmt : IBqlField
		{
		}

		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.origTaxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryOrigTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigTaxableAmt
		public abstract class origTaxableAmt : IBqlField
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : IBqlField
		{
		}

		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : IBqlField
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : IBqlField
		{
		}

		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : IBqlField
		{
		}

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryExpenseAmt
		public abstract class curyExpenseAmt : IBqlField
		{
		}
		[PXDBCurrency(typeof(CATax.curyInfoID), typeof(CATax.expenseAmt))]
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
		#endregion
		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion


		#region ITranTax

		public string TranType
		{
			get { return AdjTranType; }
			set { AdjTranType = value; }
		}

		public string RefNbr
		{
			get { return AdjRefNbr; }
			set { AdjRefNbr = value; }
		}

		#endregion
	}
}
