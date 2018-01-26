using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.FA
{
	[Serializable]
	[PXCacheName(Messages.FATransaction)]
	public partial class FATran : PX.Data.IBqlTable, IFALocation
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(FARegister.refNbr), DefaultForUpdate = false)]
		[PXParent(typeof(Select<FARegister, Where<FARegister.refNbr, Equal<Current<FATran.refNbr>>>>))]
		[PXUIField(DisplayName = "Reference Number", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXSelector(typeof(FARegister.refNbr))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(FARegister.lineCntr))]
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
		#region AssetID
		public abstract class assetID : PX.Data.IBqlField
		{
		}
		protected Int32? _AssetID;
		[PXDBInt]
		[PXDBLiteDefault(typeof(FixedAsset.assetID))]
		[PXSelector(typeof(Search2<FixedAsset.assetID, LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
			LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FADetails.assetID>,
										And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>,
			LeftJoin<Branch, On<Branch.branchID, Equal<FALocationHistory.locationID>>,
			LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<FALocationHistory.employeeID>>>>>>,
			Where<FixedAsset.recordType, Equal<FARecordType.assetType>>>),
			typeof(FixedAsset.assetCD),
			typeof(FixedAsset.description),
			typeof(FixedAsset.classID),
			typeof(FixedAsset.usefulLife),
			typeof(FixedAsset.assetTypeID),
			typeof(FADetails.status),
			typeof(Branch.branchCD),
			typeof(EPEmployee.acctName),
			typeof(FALocationHistory.department),
			Filterable = true,
			SubstituteKey = typeof(FixedAsset.assetCD), 
			DescriptionField = typeof(FixedAsset.description))]
		[PXUIField(DisplayName = "Asset", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region BookID
		public abstract class bookID : IBqlField
		{
		}
		protected Int32? _BookID;
		[PXDBInt]
		[PXDefault]
		[PXSelector(typeof(Search2<FABook.bookID, 
							InnerJoin<FABookBalance, On<FABookBalance.bookID, Equal<FABook.bookID>>>,
							Where<FABookBalance.assetID, Equal<Current<assetID>>>>), 
			SubstituteKey = typeof(FABook.bookCode),
			DescriptionField = typeof(FABook.description))]
		[PXUIField(DisplayName = "Book", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? BookID
		{
			get
			{
				return _BookID;
			}
			set
			{
				_BookID = value;
			}
		}
		#endregion
		#region ReceiptDate
		public abstract class receiptDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _ReceiptDate;
		[PXDate]
		[PXUIField(DisplayName = "Receipt Date")]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region DeprFromDate
		public abstract class deprFromDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DeprFromDate;
		[PXDate]
		[PXDefault(typeof(FATran.receiptDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Placed-in-Service Date")]
		public virtual DateTime? DeprFromDate
		{
			get
			{
				return this._DeprFromDate;
			}
			set
			{
				this._DeprFromDate = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(FARegister.docDate))]
		[PXUIField(DisplayName = "Tran. Date")]
		public virtual DateTime? TranDate
		{
			get
			{
				return this._TranDate;
			}
			set
			{
				this._TranDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.IBqlField
		{
		}
		protected String _FinPeriodID;
		[PXUIField(DisplayName = "Tran. Period")]
		[FABookPeriodID(typeof(FATran.bookID), typeof(FATran.tranDate))]
		[PXDefault()]
		public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.IBqlField
		{
		}
		protected String _TranPeriodID;
		[FABookPeriodID(typeof(FATran.bookID))]
		[PXFormula(typeof(RowExt<FATran.finPeriodID>))]
		public virtual String TranPeriodID
		{
			get
			{
				return this._TranPeriodID;
			}
			set
			{
				this._TranPeriodID = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
			#region List
			public class CustomListAttribute : PXStringListAttribute
			{
				public string[] AllowedValues
				{
					get
					{
						return _AllowedValues;
					}
				}

				public string[] AllowedLabels
				{
					get
					{
						return _AllowedLabels;
					}
				}

				public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
					: base(AllowedValues, AllowedLabels)
				{
				}
			}

			public class ListAttribute : CustomListAttribute
			{
				public ListAttribute()
					: base(
					new string[] 
						{
							PurchasingPlus, 
							PurchasingMinus, 
							DepreciationPlus, 
							DepreciationMinus, 
							CalculatedPlus, 
							CalculatedMinus, 
							SalePlus, 
							SaleMinus, 
							TransferPurchasing, 
							TransferDepreciation, 
							ReconciliationPlus, 
							ReconciliationMinus, 
							PurchasingDisposal, 
							PurchasingReversal, 
							AdjustingDeprPlus,
							AdjustingDeprMinus,
						},
					new string[]
						{
							Messages.PurchasingPlus, 
							Messages.PurchasingMinus, 
							Messages.DepreciationPlus, 
							Messages.DepreciationMinus, 
							Messages.CalculatedPlus, 
							Messages.CalculatedMinus, 
							Messages.SalePlus, 
							Messages.SaleMinus, 
							Messages.TransferPurchasing,
							Messages.TransferDepreciation, 
							Messages.ReconciliationPlus, 
							Messages.ReconciliationMinus, 
							Messages.PurchasingDisposal, 
							Messages.PurchasingReversal,
							Messages.AdjustingDeprPlus,
							Messages.AdjustingDeprMinus,
						}) { }
			}

			public class AdjustmentListAttribute : CustomListAttribute
			{
				public AdjustmentListAttribute()
					: base(
					new string[] { PurchasingPlus, DepreciationPlus },
					new string[] { Messages.PurchasingPlus, Messages.DepreciationPlus }) { }
			}

			public class NonDepreciableListAttribute : CustomListAttribute
			{
				public NonDepreciableListAttribute()
					: base(
					new string[] { PurchasingPlus },
					new string[] { Messages.PurchasingPlus }) { }
			}

			public const string PurchasingPlus = "P+";
			public const string PurchasingMinus = "P-";
			public const string DepreciationPlus = "D+";
			public const string DepreciationMinus = "D-";
			public const string CalculatedPlus = "C+";
			public const string CalculatedMinus = "C-";
			public const string SalePlus = "S+";
			public const string SaleMinus = "S-";
			public const string TransferPurchasing = "TP";
			public const string TransferDepreciation = "TD";
			public const string ReconciliationPlus = "R+";
			public const string ReconciliationMinus = "R-";
			public const string PurchasingDisposal = "PD";
			public const string PurchasingReversal = "PR";
			public const string AdjustingDeprPlus = "A+";
			public const string AdjustingDeprMinus = "A-";


			public class purchasingPlus : Constant<string>
			{
				public purchasingPlus() : base(PurchasingPlus) { ;}
			}
			public class purchasingMinus : Constant<string>
			{
				public purchasingMinus() : base(PurchasingMinus) { ;}
			}
			public class depreciationPlus : Constant<string>
			{
				public depreciationPlus() : base(DepreciationPlus) { ;}
			}
			public class depreciationMinus : Constant<string>
			{
				public depreciationMinus() : base(DepreciationMinus) { ;}
			}
			public class calculatedPlus : Constant<string>
			{
				public calculatedPlus() : base(CalculatedPlus) { ;}
			}
			public class calculatedMinus : Constant<string>
			{
				public calculatedMinus() : base(CalculatedMinus) { ;}
			}
			public class salePlus : Constant<string>
			{
				public salePlus() : base(SalePlus) { ;}
			}
			public class saleMinus : Constant<string>
			{
				public saleMinus() : base(SaleMinus) { ;}
			}
			public class transferPurchasing : Constant<string>
			{
				public transferPurchasing() : base(TransferPurchasing) { }
			}
			public class transferDepreciation : Constant<string>
			{
				public transferDepreciation() : base(TransferDepreciation) { }
			}
			public class reconcilliationPlus : Constant<string>
			{
				public reconcilliationPlus() : base(ReconciliationPlus) { }
			}
			public class reconcilliationMinus : Constant<string>
			{
				public reconcilliationMinus() : base(ReconciliationMinus) { }
			}
			public class purchasingDisposal : Constant<string>
			{
				public purchasingDisposal() : base(PurchasingDisposal) { }
			}
			public class purchasingReversal : Constant<string>
			{
				public purchasingReversal() : base(PurchasingReversal) { }
			}
			public class adjustingDeprPlus : Constant<string>
			{
				public adjustingDeprPlus() : base(AdjustingDeprPlus) { }
			}
			public class adjustingDeprMinus : Constant<string>
			{
				public adjustingDeprMinus() : base(AdjustingDeprMinus) { }
			}
			#endregion
		}
		protected String _TranType;
		[PXDBString(2, IsFixed = true)]
		[tranType.List]
		[PXUIField(DisplayName = "Transaction Type", Visibility = PXUIVisibility.Visible)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region DebitAccountID
		public abstract class debitAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _DebitAccountID;
		[Account(DisplayName = "Debit Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description))]
		public virtual Int32? DebitAccountID
		{
			get
			{
				return this._DebitAccountID;
			}
			set
			{
				this._DebitAccountID = value;
			}
		}
		#endregion
		#region DebitSubID
		public abstract class debitSubID: IBqlField {}
		protected int? _DebitSubID;
		[SubAccount(typeof(FATran.debitAccountID), DisplayName = "Debit Subaccount", Visibility = PXUIVisibility.Visible)]
		public virtual int? DebitSubID
		{
			get
			{
				return this._DebitSubID;
			}
			set
			{
				this._DebitSubID = value;
			}
		}
		#endregion
		#region CreditAccountID
		public abstract class creditAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CreditAccountID;
		[Account(DisplayName = "Credit Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description))]
		public virtual Int32? CreditAccountID
		{
			get
			{
				return this._CreditAccountID;
			}
			set
			{
				this._CreditAccountID = value;
			}
		}
		#endregion
		#region CreditSubID
		public abstract class creditSubID: IBqlField {}
		protected int? _CreditSubID;
		[SubAccount(typeof(FATran.creditAccountID), DisplayName = "Credit Subaccount", Visibility = PXUIVisibility.Visible)]
		public virtual int? CreditSubID
		{
			get
			{
				return this._CreditSubID;
			}
			set
			{
				this._CreditSubID = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : IBqlField
		{
		}
		protected decimal? _TranAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "Transaction Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(AddCalc<FAAccrualTran.closedAmt>))]
		[PXFormula(null, typeof(SumCalc<FARegister.tranAmt>))]
		public virtual decimal? TranAmt
		{
			get
			{
				return _TranAmt;
			}
			set
			{
				_TranAmt = value;
			}
		}
		#endregion
		#region RGOLAmt
		public abstract class rGOLAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _RGOLAmt;
		[PXDBBaseCury]
		[PXUIField(DisplayName = "RGOL Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? RGOLAmt
		{
			get
			{
				return this._RGOLAmt;
			}
			set
			{
				this._RGOLAmt = value;
			}
		}
		#endregion
		#region MethodDesc
		public abstract class methodDesc : PX.Data.IBqlField
		{
		}
		protected String _MethodDesc;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Method", Enabled = false)]
		public virtual String MethodDesc
		{
			get
			{
				return this._MethodDesc;
			}
			set
			{
				this._MethodDesc = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Description")]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.IBqlField
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleFA>>>))]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.IBqlField
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region Posted
		public abstract class posted : PX.Data.IBqlField
		{
		}
		protected Boolean? _Posted;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Posted
		{
			get
			{
				return this._Posted;
			}
			set
			{
				this._Posted = value;
			}
		}
		#endregion
		#region Origin
		public abstract class origin : IBqlField
		{
		}
		protected String _Origin;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(FARegister.origin))]
		[PXUIField(Visible = false)]
		public virtual String Origin
		{
			get
			{
				return _Origin;
			}
			set
			{
				_Origin = value;
			}
		}
		#endregion

		#region ClassID
		public abstract class classID : IBqlField
		{
		}
		protected Int32? _ClassID;
		[PXInt]
		public virtual Int32? ClassID
		{
			get
			{
				return _ClassID;
			}
			set
			{
				_ClassID = value;
			}
		}
		#endregion
		#region TargetAssetID
		public abstract class targetAssetID : IBqlField
		{
		}
		protected Int32? _TargetAssetID;
		[PXInt]
		public virtual Int32? TargetAssetID
		{
			get
			{
				return _TargetAssetID;
			}
			set
			{
				_TargetAssetID = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch(typeof(Search<FixedAsset.branchID, Where<FixedAsset.assetID, Equal<Current<FATran.assetID>>>>), IsDetail = false)]
		public virtual Int32? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.IBqlField
		{
		}
		protected int? _EmployeeID;
		[PXInt]
		public virtual int? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
		#endregion
		#region Department
		public abstract class department : IBqlField
		{
		}
		protected String _Department;
		[PXString(10, IsUnicode = true)]
		public virtual String Department
		{
			get
			{
				return _Department;
			}
			set
			{
				_Department = value;
			}
		}
		#endregion
		#region NewAsset
		public abstract class newAsset : IBqlField
		{
		}
		protected Boolean? _NewAsset = true;
		[PXBool]
		[PXUIField(DisplayName = "New Asset")]
		public virtual Boolean? NewAsset
		{
			get
			{
				return _NewAsset;
			}
			set
			{
				_NewAsset = value;
			}
		}
		#endregion
		#region Component
		public abstract class component : IBqlField
		{
		}
		protected Boolean? _Component = false;
		[PXBool]
		[PXUIField(DisplayName = "Component")]
		public virtual Boolean? Component
		{
			get
			{
				return _Component;
			}
			set
			{
				_Component = value;
			}
		}
		#endregion

		#region GLTranID
		public abstract class gLtranID : IBqlField
		{
		}
		protected int? _GLTranID;
		[PXDBInt]
		[PXParent(typeof(Select<FAAccrualTran, Where<FAAccrualTran.tranID, Equal<Current<gLtranID>>, And<Current<origin>, NotEqual<FARegister.origin.reversal>>>>))]
		public virtual int? GLTranID
		{
			get
			{
				return _GLTranID;
			}
			set
			{
				_GLTranID = value;
			}
		}
		#endregion
		#region TranID
		public abstract class tranID : PX.Data.IBqlField
		{
		}
		protected Int32? _TranID;
		[PXDBIdentity()]
		public virtual Int32? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region SrcBranchID
		public abstract class srcBranchID : IBqlField
		{
		}
		protected Int32? _SrcBranchID;
		[PXDBInt]
		public virtual Int32? SrcBranchID
		{
			get
			{
				return _SrcBranchID;
			}
			set
			{
				_SrcBranchID = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : IBqlField
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "1.0")]
		[PXUIField(DisplayName = "Quantity")]
		[PXFormula(null, typeof(AddCalc<FAAccrualTran.closedQty>))]
		public virtual Decimal? Qty
		{
			get
			{
				return _Qty;
			}
			set
			{
				_Qty = value;
			}
		}
		#endregion
		#region AssetCD
		public abstract class assetCD : IBqlField
		{
		}
		protected String _AssetCD;
		[PXString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Asset ID")]
		public virtual string AssetCD
		{
			get
			{
				return _AssetCD;
			}
			set
			{
				_AssetCD = value;
			}
		}
		#endregion

		#region IsOriginReversal
		public virtual bool IsOriginReversal
		{
			get
			{
				return Origin == FARegister.origin.Reversal;
			}
		}
		#endregion
		#region IsTransfer
		public virtual bool IsTransfer
		{
			get
			{
				return TranType == FATran.tranType.TransferDepreciation || TranType == FATran.tranType.TransferPurchasing;
			}
		}
		#endregion
		#region ReclassificationOnDebitProhibited
		public abstract class reclassificationOnDebitProhibited : PX.Data.IBqlField
		{
		}
		protected Boolean? _ReclassificationOnDebitProhibited;

		[PXDBBool()]
		[PXUIField(DisplayName = "Reclassification on debit is prohibited", Visibility = PXUIVisibility.Invisible)]
		public virtual Boolean? ReclassificationOnDebitProhibited
		{
			get
			{
				return this._ReclassificationOnDebitProhibited;
			}
			set
			{
				this._ReclassificationOnDebitProhibited = value;
			}
		}
		#endregion
		#region ReclassificationOnCreditProhibited
		public abstract class reclassificationOnCreditProhibited : PX.Data.IBqlField
		{
		}
		protected Boolean? _ReclassificationOnCreditProhibited;

		[PXDBBool()]
		[PXUIField(DisplayName = "Reclassification on credit prohibited", Visibility = PXUIVisibility.Invisible)]
		public virtual Boolean? ReclassificationOnCreditProhibited
		{
			get
			{
				return this._ReclassificationOnCreditProhibited;
			}
			set
			{
				this._ReclassificationOnCreditProhibited = value;
			}
		}
		#endregion


		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion

		public string GetKeyImage()
		{
			return string.Format("{0}:{1}, {2}:{3}", typeof(FATran.refNbr).Name, RefNbr, typeof(FATran.lineNbr).Name, LineNbr);
		}

		public override string ToString()
		{
			return string.Format("{0}[{1}]", EntityHelper.GetFriendlyEntityName(typeof(FATran)), GetKeyImage());
		}
	}
}
