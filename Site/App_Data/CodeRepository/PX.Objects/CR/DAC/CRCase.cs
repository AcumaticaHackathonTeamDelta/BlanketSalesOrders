using System;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.TM;

namespace PX.Objects.CR
{
	[Serializable]
	[PXPrimaryGraph(typeof(CRCaseMaint))]
	[PXCacheName(Messages.Case)]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
		Where<Contact.bAccountID, Equal<Optional<CRCase.customerID>>,
		   Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	public partial class CRCase : IBqlTable, IAssign, IPXSelectable
	{
		#region Selected
		public abstract class selected : IBqlField { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion

		#region CaseID
		public abstract class caseID : IBqlField { }

		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Case ID")]
		public virtual Int32? CaseID { get; set; }
		#endregion

		#region CaseCD
		public abstract class caseCD : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Case ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.caseNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search3<CRCase.caseCD,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRCase.customerID>>>, 
			OrderBy<Desc<CRCase.caseCD>>>),
			typeof(CRCase.caseCD),
			typeof(CRCase.subject),
			typeof(CRCase.status),
			typeof(CRCase.priority),
			typeof(CRCase.severity),
			typeof(CRCase.caseClassID),
			typeof(BAccount.acctName),
			Filterable = true)]
		[PXFieldDescription]
		public virtual String CaseCD { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : IBqlField { }

		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTimeUtc(InputMask = "g")]
		[PXUIField(DisplayName = "Date Reported", Enabled = false)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
				_timeResolution = null;
			}
		}
		#endregion

		#region CaseClassID
		public abstract class caseClassID : IBqlField { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(Search<CRSetup.defaultCaseClassID>))]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRCaseClass.caseClassID), 
			DescriptionField = typeof(CRCaseClass.description), 
			CacheGlobal = true)]
		[PXMassUpdatableField]
		public virtual String CaseClassID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : IBqlField { }

		[PXDBString(255, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Subject { get; set; }
		#endregion

		#region Description
		public abstract class description : IBqlField { }

		protected String _Description;
		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
				_plainText = null;
			}
		}
		#endregion

		#region DescriptionAsPlainText
		public abstract class descriptionAsPlainText : IBqlField { }

		private string _plainText;
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false)]
		public virtual String DescriptionAsPlainText
		{
			get
			{
				return _plainText ?? (_plainText = PX.Data.Search.SearchService.Html2PlainText(this.Description));
			}
		}
		#endregion

		#region CustomerID
		public abstract class customerID : IBqlField { }
		public class Value<Field>:IBqlOperand
			where Field : IBqlField
		{
			
		}
		[CustomerAndProspect(DisplayName = "Business Account")]
		[PXRestrictor(typeof(Where<Current<CRCaseClass.requireCustomer>, Equal<False>,
  		Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Switch<Case<Where<CRCase.caseClassID, IsNotNull, And<Selector<CRCase.caseClassID, CRCaseClass.requireCustomer>, Equal<True>,
                                         And<Current<CRCase.customerID>, IsNotNull, And<Selector<Current<CRCase.customerID>, BAccount.type>, Equal<BAccountType.prospectType>>>>>,
                                         Null>,
                             CRCase.customerID>))]
		[PXFormula(typeof(Switch<Case<Where<Current<CRCase.customerID>, IsNull, And<Current<CRCase.contractID>, IsNotNull>>, 
								IsNull<Selector<CRCase.contractID, Selector<ContractBillingSchedule.accountID, BAccount.acctCD>>,
											 Selector<CRCase.contractID, Selector<Contract.customerID, BAccount.acctCD>>>>, 
							 CRCase.customerID>))]
		[PXFormula(typeof(Switch<Case<Where<Current<CRCase.customerID>, IsNull, 
														 And<Current<CRCase.contactID>, IsNotNull, 
														 And<Selector<CRCase.contactID, Contact.bAccountID>, IsNotNull>>>, 
								Selector<CRCase.contactID, Selector<Contact.bAccountID, BAccount.acctCD>>>, 
							CRCase.customerID>))]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : IBqlField { }
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CRCase.customerID>>>), DescriptionField = typeof(Location.descr))]
		[PXFormula(typeof(Switch<
							Case<Where<Current<CRCase.locationID>, IsNull, And<Current<CRCase.contractID>, IsNotNull>>,
									IsNull<Selector<CRCase.contractID, Selector<ContractBillingSchedule.locationID, Location.locationCD>>,
												 Selector<CRCase.contractID, Selector<Contract.locationID, Location.locationCD>>>,
						  Case<Where<Current<CRCase.locationID>, IsNull, And<Current<CRCase.customerID>, IsNotNull>>,
									 Selector<CRCase.customerID, Selector<BAccount.defLocationID, Location.locationCD>>,
							Case<Where<Current<CRCase.customerID>, IsNull>, Null>>>,
						  CRCase.locationID>))]
        [PXFormula(typeof(Default<CRCase.customerID>))]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region ContractID
		public abstract class contractID : IBqlField { }
		[PXDBInt]
		[PXUIField(DisplayName = "Contract")]
		[PXSelector(typeof(Search2<Contract.contractID,
				LeftJoin<ContractBillingSchedule, On<Contract.contractID, Equal<ContractBillingSchedule.contractID>>>,
			Where<Contract.isTemplate, NotEqual<True>,
				And<Contract.baseType, Equal<Contract.ContractBaseType>,
				And<Where<Current<CRCase.customerID>, IsNull,
						Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
							And<Current<CRCase.locationID>, IsNull>>,
						Or2<Where<Contract.customerID, Equal<Current<CRCase.customerID>>,
							And<Contract.locationID, Equal<Current<CRCase.locationID>>>>,
						Or<Where<ContractBillingSchedule.accountID, Equal<Current<CRCase.customerID>>,
							And<ContractBillingSchedule.locationID, Equal<Current<CRCase.locationID>>>>>>>>>>>>,
			OrderBy<Desc<Contract.contractCD>>>),
			DescriptionField = typeof(Contract.description),
			SubstituteKey = typeof(Contract.contractCD), Filterable = true)]
		[PXRestrictor(typeof(Where<Contract.status, Equal<Contract.status.active>>), Messages.ContractIsNotActive)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, LessEqual<Contract.graceDate>, Or<Contract.expireDate, IsNull>>), Messages.ContractExpired)]
		[PXRestrictor(typeof(Where<Current<AccessInfo.businessDate>, GreaterEqual<Contract.startDate>>), Messages.ContractActivationDateInFuture, typeof(Contract.startDate))]		
		[PXFormula(typeof(Default<CRCase.customerID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ContractID { get; set; }

		#endregion

		#region ContactID
		public abstract class contactID : IBqlField { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search2<Contact.contactID,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true, DirtyRead = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<CRCase.customerID>))]
		[PXRestrictor(typeof(Where<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
				And<WhereEqualNotNull<BAccount.bAccountID, CRCase.customerID>>>), Messages.ContactBAccountDiff)]
		[PXRestrictor(typeof(Where<Contact.isActive, Equal<True>>), Messages.ContactInactive, typeof(Contact.displayName))]		
        [PXRestrictor(typeof(Where<Current<CRCaseClass.requireCustomer>, Equal<False>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.CustomerRequired, typeof(BAccount.acctCD))]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region MajorStatus
		public abstract class majorStatus : IBqlField { }

		[PXDBInt]
		[CRCaseMajorStatuses]
		[PXDefault(-1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(Visible = false, DisplayName = "Major Status")]        

        public virtual Int32? MajorStatus { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault(CRCaseStatusesAttribute._NEW, typeof(Search<CRCaseClass.status,
			Where<CRCaseClass.caseClassID, Equal<Current<CRCase.caseClassID>>>>))]
		[CRCaseStatuses]
		[PXUIField(DisplayName = "Status")]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXMassUpdatableField]
		public virtual String Status { get; set; }
		#endregion

		#region Released
		public abstract class released : IBqlField { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual Boolean? Released { get; set; }
		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[CRCaseResolutions]
		[PXUIField(DisplayName = "Reason")]
		[PXChildUpdatable]
		[PXMassUpdatableField]
        public virtual String Resolution { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : IBqlField { }

		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		[PXMassUpdatableField]
		public virtual Int32? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : IBqlField { }

		[PXDBGuid()]
		[PXOwnerSelector(typeof(CRCase.workgroupID))]
		[PXChildUpdatable(AutoRefresh = true, TextField = "AcctName", ShowHint = false)]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : IBqlField { }

		private DateTime? _assignDate;
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Assignment Date")]
		public virtual DateTime? AssignDate
		{
			get { return _assignDate ?? CreatedDateTime; }
			set { _assignDate  = value; }
		}

		#endregion

		#region Source
		public abstract class source : IBqlField { }

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[CaseSources]
		public virtual String Source { get; set; }
		#endregion

		#region Severity
		public abstract class severity : IBqlField { }

		[PXDBString(1, IsFixed = true)]
        [PXDefault(CRCaseSeverityAttribute._MEDIUM)]
		[PXUIField(DisplayName = "Severity")]
        [CRCaseSeverity()]
		public virtual String Severity { get; set; }
		#endregion

		#region Priority
		public abstract class priority : IBqlField { }

		[PXDBString(1, IsFixed = true)]
        [PXDefault(CRCasePriorityAttribute._MEDIUM)]
		[PXUIField(DisplayName = "Priority")]
        [CRCasePriority()]
		public virtual String Priority { get; set; }
		#endregion

		#region SLAETA
		public abstract class sLAETA : IBqlField { }

		[PXDBDate(PreserveTime = true, DisplayMask = "g")]
		[PXUIField(DisplayName = "SLA")]
		[PXFormula(typeof(Default<CRCase.contractID, CRCase.severity, CRCase.caseClassID>))]
		public virtual DateTime? SLAETA { get; set; }
		#endregion

		#region TimeEstimated
		public abstract class timeEstimated : IBqlField { }

		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Estimation")]
		public virtual Int32? TimeEstimated { get; set; }
		#endregion

		#region ETA
		public abstract class eTA : IBqlField { }

		[PXDate(DisplayMask = "g")]
		[PXUIField(DisplayName = "Promised")]
		public virtual DateTime? ETA
		{
			get
			{
				if (CreatedDateTime == null || TimeEstimated == null)
					return null;
				return ((DateTime)CreatedDateTime).AddMinutes((int)TimeEstimated);
			}
		}
		#endregion

		#region RemaininingDate
		public abstract class remaininingDate : IBqlField { }
		[PXDependsOnFields(typeof(eTA), typeof(createdDateTime), typeof(timeEstimated))]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Remaining")]
		public virtual Int32? RemaininingDate
		{
			get
			{
				var eta = ETA	;
				if (eta == null) return null;
				return ((DateTime)eta).Minute - PXTimeZoneInfo.Now.Minute;
			}
		}
		#endregion


		#region RemaininingDateMinutes
		public abstract class remaininingDateMinutes : IBqlField { }
		[PXInt]
		[PXUIField(DisplayName = "Remaining (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? RemaininingDateMinutes
		{
			get { return RemaininingDate; }
		}
		#endregion
		
		#region LastActivity
		public abstract class lastActivity : IBqlField { }
        [PXDBDatetimeScalar(typeof(Search<CRActivityStatistics.lastActivityDate, Where<CRActivityStatistics.noteID, Equal<noteID>>>), PreserveTime = true, UseTimeZone = true)]
		[PXDate(InputMask = "g")]
		[PXUIField(DisplayName = "Last Activity Date", Enabled = false)]
		public virtual DateTime? LastActivity { get; set; }
		#endregion

		#region LastModified
		public abstract class lastModified : IBqlField { }
		[PXDate(InputMask = "g")]
		[PXFormula(typeof(Switch<Case<Where<lastActivity, IsNotNull, And<lastModifiedDateTime, IsNull>>, lastActivity,
			Case<Where<lastModifiedDateTime, IsNotNull, And<lastActivity, IsNull>>, lastModifiedDateTime,
			Case<Where<lastActivity, Greater<lastModifiedDateTime>>, lastActivity>>>, lastModifiedDateTime>))]
		[PXUIField(DisplayName = "Last Modified", Enabled = false)]
		public virtual DateTime? LastModified { get; set; }
		#endregion
		
		#region InitResponse
		public abstract class initResponse : IBqlField { }

		[CRTimeSpanCalced(typeof(Minus1<
			Search<CRActivity.startDate,
				Where<CRActivity.refNoteID, Equal<CRCase.noteID>,
					And2<Where<CRActivity.isPrivate, IsNull, Or<CRActivity.isPrivate, Equal<False>>>,
					And<CRActivity.ownerID, IsNotNull,
					And2<Where<CRActivity.incoming, IsNull, Or<CRActivity.incoming, Equal<False>>>,
					And<Where<CRActivity.isExternal, IsNull, Or<CRActivity.isExternal, Equal<False>>>>>>>>,
				OrderBy<Asc<CRActivity.startDate>>>,
			CRCase.createdDateTime>))]
		[PXUIField(DisplayName = "Init. Response", Enabled = false)]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		public virtual Int32? InitResponse { get; set; }

		#endregion

		#region InitResponseMinutes

		public abstract class initResponseMinutes : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "Init. Response (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? InitResponseMinutes
		{
			get { return InitResponse; }
		}

		#endregion

		#region TimeSpent
		public abstract class timeSpent : IBqlField { }

		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)] //TODO: need autocalculate
		[PXUIField(DisplayName = "Time Spent", Enabled = false)]
		public virtual Int32? TimeSpent { get; set; }
		#endregion

		#region OvertimeSpent
		public abstract class overtimeSpent : IBqlField { }

		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)] //TODO: need autocalculate
		[PXUIField(DisplayName = "Overtime Spent", Enabled = false)]
		public virtual Int32? OvertimeSpent { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : IBqlField { }

		[PXDBBool]
		[PXUIField(DisplayName = "Billable", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Selector<CRCase.caseClassID, CRCaseClass.isBillable>))]
		public virtual Boolean? IsBillable { get; set; }
		#endregion

		#region ManualBillableTimes

		public abstract class manualBillableTimes : IBqlField { }

		[PXDBBool]
		[PXUIField(DisplayName = "Manual Override", FieldClass = "BILLABLE")]
		[PXFormula(typeof(Switch<Case<Where<Selector<CRCase.caseClassID, CRCaseClass.perItemBilling>, Equal<BillingTypeListAttribute.perActivity>>, False>, CRCase.manualBillableTimes>))]
		public virtual Boolean? ManualBillableTimes { get; set; }

		#endregion

		#region TimeBillable

		public abstract class timeBillable : IBqlField { }
		protected int? _timeBillable;

		[PXDBInt]
		[PXUIField(DisplayName = "Billable Time", Enabled = false, FieldClass = "BILLABLE")]
		public virtual Int32? TimeBillable
		{
			get { return _timeBillable; }
			set { _timeBillable = value; }
		}

		#endregion

		#region OvertimeBillable

		public abstract class overtimeBillable : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Billable Overtime", FieldClass = "BILLABLE")]
		public virtual Int32? OvertimeBillable { get; set; }

		#endregion

		#region ResolutionDate
		public abstract class resolutionDate : IBqlField { }

		protected DateTime? _ResolutionDate;
		[PXDBDate(PreserveTime = true, DisplayMask = "g")]
		[PXUIField(DisplayName = "Closing Date", Enabled = false)]
		public virtual DateTime? ResolutionDate
		{
			get
			{
				return this._ResolutionDate;
			}
			set
			{
				this._ResolutionDate = value;
				_timeResolution = null;
			}
		}
		#endregion

		#region TimeResolution
		public abstract class timeResolution : IBqlField { }

		private int? _timeResolution;

		[PXDependsOnFields(typeof(resolutionDate), typeof(createdDateTime))]
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Resolution Time", Enabled = false)]
		public virtual Int32? TimeResolution
		{
			get
			{
				var createdDateTime = CreatedDateTime;
				var resolutionDate = ResolutionDate;
				if (_timeResolution == null && createdDateTime != null && 
					(resolutionDate != null || 
						MajorStatus != CRCaseMajorStatusesAttribute._CLOSED && 
						MajorStatus != CRCaseMajorStatusesAttribute._RELEASED))
				{
					var end = resolutionDate.Return(_ => _.Value, PXTimeZoneInfo.Now);
					var start = createdDateTime.Return(_ => _.Value.Ticks, end.Ticks);
					_timeResolution = (int) TimeSpan.FromTicks(end.Ticks - start).TotalMinutes;
				}
				return _timeResolution;
			}
		}
		#endregion

		#region TimeResolutionMinutes
		public abstract class timeResolutionMinutes : IBqlField { }
		[PXInt]
		[PXUIField(DisplayName = "Resolution (minutes)", Enabled = false, Visible = false)]
		public virtual Int32? TimeResolutionMinutes
		{
			get { return TimeResolution; }
		}
		#endregion

		#region ARRefNbr
		public abstract class aRRefNbr : IBqlField { }

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<ARInvoice.refNbr>))]
		public virtual String ARRefNbr { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : IBqlField { }

		[CRAttributesField(typeof(CRCase.caseClassID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		public string ClassID
		{
			get { return CaseClassID; }
		}

		#region Date
		public abstract class date : IBqlField { }

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Billing Date")]
		public virtual DateTime? Date { get; set; }
		#endregion

        #region Age

        public abstract class age : IBqlField { }

        [PXTimeSpanLong(InputMask = "#### ds ## hrs ## mins")]
        [PXFormula(typeof(Sub<PXDateAndTimeAttribute.now, CRCase.createdDateTime>))]
        [PXUIField(DisplayName = "Age")]
        public virtual Int32? Age { get; set; }

        #endregion

        #region LastActivityAge

        public abstract class lastActivityAge : IBqlField { }

        [PXTimeSpanLong(InputMask = "#### ds ## hrs ## mins")]
        [PXFormula(typeof(Sub<PXDateAndTimeAttribute.now, CRCase.lastActivity>))]
        [PXUIField(DisplayName = "Last Activity Age")]
        public virtual Int32? LastActivityAge { get; set; }

        #endregion

		#region StatusDate
		public abstract class statusDate : IBqlField { }

		[PXDBLastChangeDateTime(typeof(CRCase.status))]
		public virtual DateTime? StatusDate { get; set; }
		#endregion

		#region StatusRevision
		public abstract class statusRevision : IBqlField { }

		[PXDBRevision(typeof(CRCase.status))]
		public virtual int? StatusRevision { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXSearchable(SM.SearchCategory.CR, Messages.CaseSearchTitle, new Type[] { typeof(CRCase.caseCD), typeof(CRCase.customerID), typeof(BAccount.acctName) },
			new Type[] { typeof(CRCase.contactID), typeof(Contact.firstName), typeof(Contact.lastName), typeof(Contact.eMail), 
				typeof(CRCase.ownerID), typeof(EP.EPEmployee.acctCD),typeof(EP.EPEmployee.acctName),typeof(CRCase.subject) },
			NumberFields = new Type[] { typeof(CRCase.caseCD) },
			 Line1Format = "{1}{3}{4}", Line1Fields = new Type[] { typeof(CRCase.caseClassID), typeof(CRCaseClass.description), typeof(CRCase.contactID), typeof(Contact.fullName), typeof(CRCase.status) },
			 Line2Format = "{0}", Line2Fields = new Type[] { typeof(CRCase.subject) }
		 )]
		[PXNote(
			DescriptionField = typeof(CRCase.caseCD),
			Selector = typeof(CRCase.caseCD),
			ShowInReferenceSelector = true)]
		public virtual Guid? NoteID { get; set; }
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


		[PXDBLastModifiedDateTimeUtc(InputMask = "g")]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
	}
}
