using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	[Serializable]
	[PXProjection(typeof(Select2<PaymentMethodAccount,
			InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<PaymentMethodAccount.cashAccountID>>>>),
		new Type[] { typeof(PaymentMethodAccount) })]
	[PXCacheName(Messages.PaymentMethodAccount)]
	public partial class PaymentMethodAccount : IBqlTable
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : IBqlField
		{
		}

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		[PXParent(typeof(Select<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Current<PaymentMethodAccount.paymentMethodID>>>>))]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		[PXUIField(DisplayName = "Payment Method")]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : IBqlField
		{
		}

		[CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, IsKey = true, DescriptionField = typeof(CashAccount.descr))]
		[PXDefault]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : IBqlField
		{
		}

		[Branch(BqlField = typeof(CashAccount.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region UseForAP
		public abstract class useForAP : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AP")]
		public virtual bool? UseForAP
		{
			get;
			set;
		}
		#endregion
		#region APIsDefault
		public abstract class aPIsDefault : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AP Default")]
		public virtual bool? APIsDefault
		{
			get;
			set;
		}
		#endregion
		#region APAutoNextNbr
		public abstract class aPAutoNextNbr : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AP - Suggest Next Number")]
		public virtual bool? APAutoNextNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbr
		public abstract class aPLastRefNbr : IBqlField
		{
		}

		[PXDBString(40, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "AP Last Reference Number")]
		public virtual string APLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbrIsNull
		protected bool? _APLastRefNbrIsNull = false;
		public virtual bool? APLastRefNbrIsNull
		{
			get
			{
				return this._APLastRefNbrIsNull;
			}
			set
			{
				this._APLastRefNbrIsNull = value;
			}
		}
		#endregion
		#region APBatchLastRefNbr
		public abstract class aPBatchLastRefNbr : IBqlField
		{
		}

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Last Reference Number")]
		public virtual string APBatchLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region UseForAR
		public abstract class useForAR : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Use in AR")]
		public virtual bool? UseForAR
		{
			get;
			set;
		}
		#endregion
		#region ARIsDefault
		public abstract class aRIsDefault : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR Default")]
		public virtual bool? ARIsDefault
		{
			get;
			set;
		}
		#endregion
		#region ARIsDefaultForRefund
		public abstract class aRIsDefaultForRefund : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR Default For Refund")]
		public virtual bool? ARIsDefaultForRefund
		{
			get;
			set;
		}
		#endregion
		#region ARAutoNextNbr
		public abstract class aRAutoNextNbr : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "AR - Suggest Next Number")]
		public virtual bool? ARAutoNextNbr
		{
			get;
			set;
		}
		#endregion
		#region ARLastRefNbr
		public abstract class aRLastRefNbr : IBqlField
		{
		}

		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Last Reference Number")]
		public virtual string ARLastRefNbr
		{
			get;
			set;
		}
		#endregion
		#region APLastRefNbrIsNull
		protected bool? _ARLastRefNbrIsNull = false;
		public virtual bool? ARLastRefNbrIsNull
		{
			get
			{
				return this._ARLastRefNbrIsNull;
			}
			set
			{
				this._ARLastRefNbrIsNull = value;
			}
		}
		#endregion
	}


	public class PaymentMethodAccountUsage
	{
		public const string UseForAP = "P";
		public const string UseForAR = "R";

		public class useForAP : Constant<string>
		{
			public useForAP() : base(UseForAP) { }
		}

		public class useForAR : Constant<string>
		{
			public useForAR() : base(UseForAR) { }
		}
	}
}
