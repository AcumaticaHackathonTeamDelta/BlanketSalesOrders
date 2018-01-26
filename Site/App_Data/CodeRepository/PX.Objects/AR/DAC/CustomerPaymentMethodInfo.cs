namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.CA;

	/// <summary>
	/// A service projection DAC that is used to display customer payment method
	/// settings on the Customers (AR303000) form. The DAC is designed so that the
	/// <see cref="CustomerPaymentMethod">customer payment method</see> settings are
	/// displayed if they exist; <see cref="PaymentMethod">generic payment method</see>
	/// settings are displayed otherwise.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.CustomerPaymentMethodInfo)]
	[PXProjection(
		typeof(Select2<PMInstance, LeftJoin<PaymentMethod, On<PMInstance.pMInstanceID, Equal<PaymentMethod.pMInstanceID>>,
				LeftJoin<CustomerPaymentMethod, On<PMInstance.pMInstanceID, Equal<CustomerPaymentMethod.pMInstanceID>>,
					LeftJoin<PaymentMethodActive, On<PaymentMethod.paymentMethodID, Equal<PaymentMethodActive.paymentMethodID>,
						Or<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethodActive.paymentMethodID>>>>>>,
			Where2<Where<PaymentMethod.aRIsOnePerCustomer, Equal<True>, Or<PaymentMethod.aRIsOnePerCustomer, IsNull>>,
				And2<Where<PaymentMethod.useForAR, Equal<True>, Or<PaymentMethod.useForAR, IsNull>>,
					And<Where<PaymentMethod.pMInstanceID, IsNotNull, Or<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>))]
	public partial class CustomerPaymentMethodInfo : PX.Data.IBqlTable
	{
		#region BAccountID

		public abstract class bAccountID : PX.Data.IBqlField
		{
		}

		[PXDBInt(BqlField = typeof(CustomerPaymentMethod.bAccountID))]
		public virtual Int32? BAccountID { get; set; }

		#endregion

		#region IsDefault

		public abstract class isDefault : PX.Data.IBqlField
		{
		}

		[PXDBBool()]
		[PXUIField(DisplayName = "Is Default")]
		public virtual Boolean? IsDefault { get; set; }

		#endregion

		#region PaymentMethodID

		public abstract class paymentMethodID : PX.Data.IBqlField
		{
		}

		[PXString(10, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Payment Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(
				Switch
				<Case<Where<PaymentMethod.paymentMethodID, IsNotNull>, PaymentMethod.paymentMethodID>,
					CustomerPaymentMethod.paymentMethodID>), typeof(string))]
		public virtual String PaymentMethodID { get; set; }

		#endregion

		#region PMInstanceID

		public abstract class pMInstanceID : PX.Data.IBqlField
		{
		}

		[PXDBInt(IsKey = true, BqlField = typeof(PMInstance.pMInstanceID))]
		public virtual Int32? PMInstanceID { get; set; }

		#endregion

		#region CashAccountID

		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}

		[PXDBInt(BqlField = typeof(CustomerPaymentMethod.cashAccountID))]
		[PXSelector(typeof(CashAccount.cashAccountID), SubstituteKey = typeof(CashAccount.cashAccountCD))]
		[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Int32? CashAccountID { get; set; }

		#endregion

		#region Descr

		public abstract class descr : PX.Data.IBqlField
		{
		}

		protected String _Descr;

		[PXDBLocalizableString(255, IsUnicode = true, NonDB = true, BqlField = typeof(PaymentMethod.descr))]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(Switch<Case<Where<PaymentMethod.descr, IsNotNull>, PaymentMethod.descr>, CustomerPaymentMethod.descr>),
			typeof(string))]
		public virtual String Descr { get; set; }

		#endregion

		#region IsActive

		public abstract class isActive : PX.Data.IBqlField
		{
		}

		[PXBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(
				Switch<
					Case<Where<PaymentMethodActive.isActive, Equal<True>>,
						Switch<
							Case<Where<CustomerPaymentMethod.isActive, IsNotNull>, CustomerPaymentMethod.isActive>, PaymentMethod.isActive>>,
					Null>
			),
			typeof(bool))]
		public virtual Boolean? IsActive { get; set; }

		#endregion

		#region ARIsOnePerCustomer

		public abstract class aRIsOnePerCustomer : PX.Data.IBqlField
		{
		}

		[PXDBBool(BqlField = typeof(PaymentMethod.aRIsOnePerCustomer))]
		[PXDefault(false)]
		public virtual Boolean? ARIsOnePerCustomer { get; set; }

		#endregion

		#region IsCustomerPaymentMethod

		public abstract class isCustomerPaymentMethod : PX.Data.IBqlField
		{
		}

		[PXBool()]
		[PXDBCalced(typeof(Switch<Case<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>, True>, False>), typeof(bool))]
		[PXUIField(DisplayName = "Override", Enabled = false)]
		public virtual bool? IsCustomerPaymentMethod { get; set; }

		#endregion
	}
}