﻿using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.GL;
using PX.Objects.IN;
using static PX.Objects.PM.PMAllocator;
using PX.Objects.EP;
using PX.Objects.AP;
using PX.Common.Parser;

namespace PX.Objects.PM
{
	public class PMBillEngine : PXGraph<PMBillEngine>, IRateTable
	{
		#region DAC Attributes Override
		[PXDBInt()]
		protected virtual void PMDetail_ContractItemID_CacheAttached(PXCache sender) { }
		[PXDBInt(IsKey = true)]
		protected virtual void PMDetail_TaskID_CacheAttached(PXCache sender) { }
		#endregion
		public PXSelect<PMProject> Project;
		public PXSelect<PMDetail> PMDetail;
		public PXSelect<ContractBillingSchedule> BillingSchedule;
		public PXSelect<PMTran> Transactions;
		public PXSelect<PMBillingRecord, Where<PMBillingRecord.projectID, Equal<Current<PMProject.contractID>>>> BillingRecord;
		protected Dictionary<int, Dictionary<int, List<PXResult<PMTran>>>> transactions; //Transactions stored by TaskID and then By AccountGroupID
		protected Dictionary<int, List<PMDetail>> recurrentItemsByTask;
		internal Dictionary<string, List<PMBillingRule>> billingRules;
		protected Dictionary<string, List<PMRateDefinition>> rateDefinitions; //Cached set of RateDefinitions;
		protected Dictionary<string, decimal?> ratios; //Cached set of billable ratios for fix price tasks.
		protected InvoicePersistingHandler invoicePersistingHandler;

		protected RateEngineV2 rateEngine;
		public PXSetup<Company> CompanySetup;

		public const string emptyInvoiceDescriptionKey = "EMPTY_INV_GROUP";

		public virtual bool IncludeTodaysTransactions
		{
			get
			{
				bool result = true;
				PMSetup setup = PXSelect<PMSetup>.Select(this);
				if (setup != null && setup.CutoffDate == PMCutOffDate.Excluded)
				{
					result = false;
				}

				return result;
			}
		}

		public PMBillEngine()
		{
			PXDBDefaultAttribute.SetDefaultForUpdate<ContractBillingSchedule.contractID>(BillingSchedule.Cache, null, false);
			PXDBDefaultAttribute.SetDefaultForUpdate<PMDetail.contractID>(PMDetail.Cache, null, false);
			transactions = new Dictionary<int, Dictionary<int, List<PXResult<PMTran>>>>();
			recurrentItemsByTask = new Dictionary<int, List<PMDetail>>();
			billingRules = new Dictionary<string, List<PMBillingRule>>();
			rateDefinitions = new Dictionary<string, List<PMRateDefinition>>();
			invoicePersistingHandler = new InvoicePersistingHandler();
		}
		
		protected ProformaEntry proformaEntry;
		public virtual ProformaEntry ProformaEntry
		{
			get
			{
				if (proformaEntry == null)
				{
					proformaEntry = PXGraph.CreateInstance<ProformaEntry>();
					proformaEntry.RecalculateAvalaraTaxesSync = true;
					proformaEntry.RowPersisted.AddHandler<PMProforma>(invoicePersistingHandler.OnProformaPersisted);
				}

				return proformaEntry;
			}
		}

		protected ARInvoiceEntry invoiceEntry;
		public virtual ARInvoiceEntry InvoiceEntry
		{
			get
			{
				if (invoiceEntry == null)
				{
					invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
					invoiceEntry.FieldVerifying.AddHandler<ARTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.
					invoiceEntry.RowPersisted.AddHandler<ARInvoice>(invoicePersistingHandler.OnInvoicePersisted);
				}

				return invoiceEntry;
			}
		}


		protected RegisterEntry pmRegisterEntry;
		public virtual RegisterEntry PMEntry
		{
			get
			{
				if (pmRegisterEntry == null)
				{
					pmRegisterEntry = PXGraph.CreateInstance<RegisterEntry>();
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Project can be completed.
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });//Task can be completed.
					pmRegisterEntry.FieldVerifying.AddHandler<PMTran.inventoryID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });

				}

				return pmRegisterEntry;
			}
		}

		public virtual BillingResult Bill(int? projectID, DateTime? invoiceDate, string finPeriod)
		{
			this.Clear();

			PMProject project = SelectProjectByID(projectID);
			ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(this, project.ContractID);

			Customer customer = null;
			if (project.CustomerID != null)
			{
				customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);
			}

			if (customer == null)
				throw new PXException(Messages.NoCustomer);

			var dates = GetBillingAndCutOffDates(schedule, invoiceDate);
			DateTime billingDate = dates.Item1;
			DateTime cuttoffDate = dates.Item2;
			
			List<PMTask> tasks = SelectBillableTasks(project);
			
			HashSet<string> distinctRateTables = new HashSet<string>();
			foreach (PMTask task in tasks)
			{
				if (!string.IsNullOrEmpty(task.RateTableID))
					distinctRateTables.Add(task.RateTableID);
			}

			PreSelectRecurrentItems(projectID);
			PreSelectTasksTransactions(projectID, tasks, cuttoffDate); //billingRules dictionary also filled.

			HashSet<string> distinctRateTypes = new HashSet<string>();
			foreach (List<PMBillingRule> ruleList in billingRules.Values)
			{
				foreach (PMBillingRule rule in ruleList)
				{
					if (!string.IsNullOrEmpty(rule.RateTypeID))
						distinctRateTypes.Add(rule.RateTypeID);
				}
			}

			rateEngine = CreateRateEngineV2(distinctRateTables.ToList(), distinctRateTypes.ToList());

			Dictionary<string, List<BillingData>> list = new Dictionary<string, List<BillingData>>();
			List<PMTran> reversalTrans = new List<PMTran>();

			foreach (PMTask task in tasks)
			{
				string key = GenerateProformaTag(project, task);
								
				List<BillingData> taskBillData;
				if (!list.TryGetValue(key, out taskBillData))
				{
					taskBillData = new List<BillingData>();
					list.Add(key, taskBillData);
				}
				taskBillData.AddRange(BillTask(project, customer, task, billingDate));								
				reversalTrans.AddRange(ReverseWipTask(task, billingDate));
			}
			
			//Regroup by Invoices:
			Dictionary<string, List<BillingData>> invoices = new Dictionary<string, List<BillingData>>();
			
			Dictionary<string, string> invoiceDescriptions = new Dictionary<string, string>();

			foreach (KeyValuePair<string, List<BillingData>> kv in list)
			{				
				foreach (BillingData data in kv.Value)
				{
					string invoiceKey = GetInvoiceKey(kv.Key, data.Rule);

					if (invoices.ContainsKey(invoiceKey))
					{
						invoices[invoiceKey].Add(data);
					}
					else
					{
						invoices.Add(invoiceKey, new List<BillingData>(new BillingData[] { data }));
						
						if (!string.IsNullOrEmpty(data.Rule.InvoiceFormula))
						{
							try
							{
								PMTran container = new PMTran();
								container.ProjectID = data.Tran.ProjectID;
								container.TaskID = data.Tran.TaskID;
								container.Description = data.Rule.Description;
								container.AccountGroupID = data.Tran.AccountGroupID ?? data.Rule.AccountGroupID;
								container.InventoryID = data.Tran.InventoryID;
								container.BAccountID = project.CustomerID;
								container.CostCodeID = data.Tran.CostCodeID;
								container.ResourceID = data.Tran.ResourceID;
								container.Date = billingDate;
																
								PMDataNavigator navigator = new PMDataNavigator(this, new List<PMTran>(new PMTran[1] { container }));
								ExpressionNode descNode = PMExpressionParser.Parse(this, data.Rule.InvoiceFormula);
								descNode.Bind(navigator);
								object val = descNode.Eval(container);
								if (val != null)
								{
									invoiceDescriptions.Add(invoiceKey, val.ToString());
								}
							}
							catch (Exception ex)
							{
								throw new PXException(Messages.FailedToCalcInvDescFormula_Billing, data.Rule.BillingID, data.Rule.DescriptionFormula, ex.Message);
							}
						}
					}

					//Reverse On Billing:
					if (project.CreateProforma != true)
					{
						//For Proforma Reversal is done during released process.
						foreach (PMTran original in data.Transactions)
						{
							if (original != null && original.Reverse == PMReverse.OnBilling)
							{
								foreach (PMTran tran in ReverseTran(original))
								{
									tran.Date = billingDate;
									tran.FinPeriodID = null;
									tran.TranDate = null;
									tran.TranPeriodID = null;
									tran.OrigProjectID = null;
									tran.OrigTaskID = null;
									tran.OrigAccountGroupID = null;
									reversalTrans.Add(tran);
								}
							}
						}
					}
				}
			}

			//Schedule update:
			schedule.NextDate = GetNextBillingDate(this, schedule, schedule.NextDate);
			schedule.LastDate = this.Accessinfo.BusinessDate;
			BillingSchedule.Update(schedule);


			//PMDetail update:
			foreach (PMTask task in tasks)
			{
				List<PMDetail> details;
				if (recurrentItemsByTask.TryGetValue(task.TaskID.Value, out details))
				{
					foreach (PMDetail detail in details)
					{
						if ( string.Equals(detail.ResetUsage, ResetUsageOption.OnBilling, StringComparison.InvariantCultureIgnoreCase) )
						{
							detail.Used = 0;
							PMDetail.Update(detail);
						}
					}
				}
			}

			BillingResult result = new BillingResult();
			PMRegister reversalDoc = null;

			if (invoices.Count > 0)
			{
				List<BillingData> billedData = null;

				project.BillingLineCntr = project.BillingLineCntr.GetValueOrDefault() + 1;
				Project.Update(project);

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					bool newProformaAdded = false;
					foreach (KeyValuePair<string, List<BillingData>> kv in invoices)
					{
						if (project.CreateProforma == true)
							ProformaEntry.Clear();
						else
							InvoiceEntry.Clear();
												
						string docDesc = null;
						invoiceDescriptions.TryGetValue(kv.Key, out docDesc);
												
						ARInvoice invoice = null;
						if (project.CreateProforma == true)
						{
							InsertNewProformaDocument(finPeriod, customer, project, billingDate, docDesc);
							newProformaAdded = true;
							result.Proformas.Add(ProformaEntry.Document.Current);
						}
						else
						{
							string docType = GetDocType(kv.Value);
							
							invoice = InsertNewInvoiceDocument(finPeriod, docType, customer, project, billingDate, docDesc);
						}

						List<BillingData> unbilled = new List<BillingData>(kv.Value);

						if (project.CreateProforma == true)
						{
							InsertTransactionsInProforma(unbilled);
						}
						else
						{
							int mult = 1;
							if (invoice.DocType == ARDocType.CreditMemo)
								mult = -1;

							InsertTransactionsInInvoice(unbilled, mult);
							ARInvoice oldInvoice = (ARInvoice) InvoiceEntry.Document.Cache.CreateCopy(invoice);

							invoice.CuryOrigDocAmt = invoice.CuryDocBal;
							InvoiceEntry.Document.Cache.RaiseRowUpdated(invoice, oldInvoice);
							InvoiceEntry.Document.Cache.SetValue<ARInvoice.curyOrigDocAmt>(invoice, invoice.CuryDocBal);

							if (project.AutomaticReleaseAR == true)
								InvoiceEntry.Document.Cache.SetValueExt<ARInvoice.hold>(invoice, false);

							result.Invoices.Add(InvoiceEntry.Document.Current);
						}

						billedData = kv.Value;

						foreach (BillingData data in billedData)
						{
							foreach (PMTran orig in data.Transactions)
							{
								orig.Billed = true;
								orig.BilledDate = invoiceDate;
								Transactions.Update(orig);
								PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, orig);
							}
						}

						if (newProformaAdded || project.CreateProforma != true)
						{
							PMBillingRecord billingRecord = (PMBillingRecord)BillingRecord.Cache.CreateInstance();
							billingRecord.ProjectID = project.ContractID;
							billingRecord.RecordID = project.BillingLineCntr;
							billingRecord.BillingTag = kv.Key;
							billingRecord.Date = billingDate;
							BillingRecord.Insert(billingRecord);
						}

						invoicePersistingHandler.SetData(billedData, BillingRecord.Current);
						if (project.CreateProforma == true)
						{							
							ProformaEntry.autoApplyPrepayments.Press();
							ProformaEntry.Save.Press();
						}
						else
							InvoiceEntry.Save.Press();
					}

					Actions.PressSave();

					if (reversalTrans.Count > 0)
					{
						PMEntry.Clear();

						reversalDoc = (PMRegister)PMEntry.Document.Cache.Insert();
						reversalDoc.OrigDocType = PMOrigDocType.AllocationReversal;
						reversalDoc.Description = PXMessages.LocalizeNoPrefix(Messages.AllocationReversalBilling);
						PMEntry.Document.Current = reversalDoc;

						foreach (PMTran tran in reversalTrans)
						{
							PMEntry.Transactions.Insert(tran);
						}
						PMEntry.Save.Press();
					}

					ts.Complete();
				}

			}
			else
			{
				this.Persist(typeof(ContractBillingSchedule), PXDBOperation.Update);
				this.Persist(typeof(Contract), PXDBOperation.Update);
			}

			AutoReleaseCreatedDocuments(project, result, reversalDoc);

			return result;
		}
				
		public virtual List<PMTask> SelectBillableTasks(PMProject project)
		{
			List<PMTask> tasks = new List<PMTask>();
			PXSelectBase<PMTask> selectTasks = new PXSelect<PMTask,
				Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
				And<PMTask.billingID, IsNotNull,
				And<PMTask.isActive, Equal<True>>>>>(this);

			
			foreach (PMTask task in selectTasks.Select(project.ContractID))
			{
				if ((task.BillingOption == PMBillingOption.OnTaskCompletion && task.IsCompleted == true) ||
					  (task.BillingOption == PMBillingOption.OnProjectCompetion && project.IsCompleted == true) ||
					  task.BillingOption == PMBillingOption.OnBilling)
				{
					tasks.Add(task);
				}
			}

			return tasks;
		}

		public virtual string GetInvoiceKey(string proformaTag, PMBillingRule rule)
		{
			string invoiceKey = string.Format("{0}-{1}", proformaTag, rule == null || string.IsNullOrEmpty(rule.InvoiceGroup) ? emptyInvoiceDescriptionKey : rule.InvoiceGroup);
			return invoiceKey; 
		}

		protected virtual PMProject SelectProjectByID(int? projectID)
		{
			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, projectID);
			return project;
		}

		public virtual void AutoReleaseCreatedDocuments(PMProject project, BillingResult result, PMRegister reversalDoc)
		{
			if (project.AutomaticReleaseAR == true && result.Invoices.Count > 0)
			{
				try
				{
					ARDocumentRelease.ReleaseDoc(result.Invoices, false);
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.AutoReleaseARFailed, ex);
				}

				if (reversalDoc != null)
				{
					try
					{
						RegisterRelease.Release(reversalDoc);
					}
					catch (Exception ex)
					{
						throw new PXException(Messages.AutoReleaseOfReversalFailed, ex);
					}
				}

			}
		}

		public virtual Tuple<DateTime, DateTime> GetBillingAndCutOffDates(ContractBillingSchedule schedule, DateTime? invoiceDate)
		{
			DateTime billingDate;
			DateTime cuttoffDate;

			if (invoiceDate == null)
			{
				if (schedule.Type == BillingType.OnDemand)
				{
					billingDate = Accessinfo.BusinessDate ?? DateTime.Now;
					cuttoffDate = billingDate.AddDays(1);//ondemand always includes all transactions including the current day.
				}
				else
				{
					billingDate = schedule.NextDate.Value;
					cuttoffDate = billingDate.AddDays(IncludeTodaysTransactions ? 1 : 0);
				}
			}
			else
			{
				billingDate = invoiceDate.Value;
				cuttoffDate = billingDate.AddDays(IncludeTodaysTransactions ? 1 : 0);
			}

			return new Tuple<DateTime, DateTime>(billingDate, cuttoffDate);
		}

		public virtual string GenerateProformaTag(PMProject project, PMTask task)
		{
			string key = "P";
			if (task.BillSeparately == true)
			{
				key = "T:" + task.TaskID.ToString();
			}
			else if (task.LocationID != null && task.LocationID != project.LocationID)
			{
				key = "L:" + task.LocationID.ToString();
			}

			return key;
		}

		public virtual void InsertTransactionsInProforma(List<BillingData> unbilled)
		{
			foreach (BillingData data in unbilled)
			{
				PMProformaLine newTran = InsertTransaction(data.Tran, data.SubCD, data.Note, data.Files);
				if (newTran != null)
				{
					foreach (PMTran original in data.Transactions)
					{
                        original.ProformaRefNbr = newTran.RefNbr;
                        original.ProformaLineNbr = newTran.LineNbr;
					}
				}
			}
		}

		public virtual void InsertTransactionsInInvoice(List<BillingData> unbilled, int mult)
		{
			foreach (BillingData data in unbilled)
			{
				ARTran tran = new ARTran();
				tran.BranchID = data.Tran.BranchID;
				tran.InventoryID = data.Tran.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID ? null : data.Tran.InventoryID;
				tran.TranDesc = data.Tran.Description;
				tran.UOM = data.Tran.UOM;
				tran.Qty = data.Tran.Qty * mult;
				tran.ExtPrice = data.Tran.Amount * mult;
				tran.TranAmt = data.Tran.Amount * mult;
				tran.UnitPrice = data.Tran.UnitPrice;
				tran.ProjectID = data.Tran.ProjectID;
				tran.TaskID = data.Tran.TaskID;
				tran.CostCodeID = data.Tran.CostCodeID;
				tran.Date = data.Tran.Date;
				tran.AccountID = data.Tran.AccountID;
				tran.SubID = data.Tran.SubID;
				tran.TaxCategoryID = data.Tran.TaxCategoryID;
				if (data.Tran.IsPrepayment == true && !string.IsNullOrEmpty(data.Tran.DefCode))
				{
					tran.DeferredCode = data.Tran.DefCode;
				}

				decimal curyamount;
				PXDBCurrencyAttribute.CuryConvCury(InvoiceEntry.currencyinfo.Cache, InvoiceEntry.currencyinfo.Current, tran.UnitPrice.GetValueOrDefault(), out curyamount);
				tran.CuryUnitPrice = curyamount;
				PXDBCurrencyAttribute.CuryConvCury(InvoiceEntry.currencyinfo.Cache, InvoiceEntry.currencyinfo.Current, tran.ExtPrice.GetValueOrDefault(), out curyamount);
				tran.CuryExtPrice = curyamount;
				tran.CuryTranAmt = curyamount;
				tran.FreezeManualDisc = true;
				tran.ManualPrice = true;
				tran.CuryInfoID = InvoiceEntry.currencyinfo.Current.CuryInfoID;

				ARTran newTran = InsertTransaction(tran, data.SubCD, data.Note, data.Files);
				if (data.Tran.IsPrepayment != true && data.Tran.AccountGroupID != null)
					InvoiceEntry.SubtractAmountToInvoice(newTran, newTran.TranAmt.GetValueOrDefault(), data.Tran.AccountGroupID);

				foreach (PMTran original in data.Transactions)
				{
					original.RefLineNbr = newTran.LineNbr;
				}
			}
		}

		/// <summary>
		/// Inserts new AR transaction into current ARInvoice document
		/// </summary>
		/// <param name="tran">Transaction</param>
		/// <param name="subCD">override Subaccount </param>
		/// <param name="note">Note text</param>
		/// <param name="files">Attached files</param>
		public virtual ARTran InsertTransaction(ARTran tran, string subCD, string note, Guid[] files)
		{
			ARTran newTran = (ARTran)InvoiceEntry.Caches[typeof(ARTran)].Insert(tran);
						
			if (tran.AccountID != null && tran.AccountID != newTran.AccountID)
			{
				newTran.AccountID = tran.AccountID;
				newTran = InvoiceEntry.Transactions.Update(newTran);
			}

			if (subCD != null)
				InvoiceEntry.Transactions.SetValueExt<ARTran.subID>(newTran, subCD);

			if (note != null)
				PXNoteAttribute.SetNote(InvoiceEntry.Transactions.Cache, newTran, note);
			if (files != null && files.Length > 0)
				PXNoteAttribute.SetFileNotes(InvoiceEntry.Transactions.Cache, newTran, files);

			return newTran;
		}

		public virtual PMProformaLine InsertTransaction(PMProformaLine tran, string subCD, string note, Guid[] files)
		{
			PMProformaLine newTran = null;
			decimal curyamount;
			PXDBCurrencyAttribute.CuryConvCury(ProformaEntry.currencyinfo.Cache, ProformaEntry.currencyinfo.Current, tran.Amount.GetValueOrDefault(), out curyamount);
			tran.CuryAmount = curyamount;
			decimal curybillableamount;
			PXDBCurrencyAttribute.CuryConvCury(ProformaEntry.currencyinfo.Cache, ProformaEntry.currencyinfo.Current, tran.BillableAmount.GetValueOrDefault(), out curybillableamount);
			tran.CuryBillableAmount = curybillableamount;
			decimal curyunitprice;
			PXDBCurrencyAttribute.CuryConvCury(ProformaEntry.currencyinfo.Cache, ProformaEntry.currencyinfo.Current, tran.UnitPrice.GetValueOrDefault(), out curyunitprice);
			tran.CuryUnitPrice = curyunitprice;

			if (tran.Type == PMProformaLineType.Transaction )
			{
				newTran = ProformaEntry.TransactionLines.Insert( (PMProformaTransactLine) tran);
				if (newTran.CuryAmount != curyamount)
				{
					ProformaEntry.TransactionLines.SetValueExt<PMProformaTransactLine.curyAmount>((PMProformaTransactLine)newTran, curyamount);
				}
				
				if (subCD != null)
					ProformaEntry.Caches[typeof(PMProformaTransactLine)].SetValueExt<PMProformaTransactLine.subID>(newTran, subCD);

				if (note != null)
					PXNoteAttribute.SetNote(ProformaEntry.TransactionLines.Cache, newTran, note);
				if (files != null && files.Length > 0)
					PXNoteAttribute.SetFileNotes(ProformaEntry.TransactionLines.Cache, newTran, files);
			}
			else
			{
				if (ProformaEntry.Document.Cache.GetStatus(ProformaEntry.Document.Current) == PXEntryStatus.Inserted)
				{
					//progressive transactions are never appended.

					newTran = ProformaEntry.ProgressiveLines.Insert((PMProformaProgressLine)tran);
					if (tran.IsPrepayment != true)
						ProformaEntry.SubtractAmountToInvoice(newTran, newTran.Amount.GetValueOrDefault());

					if (subCD != null)
						ProformaEntry.Caches[typeof(PMProformaProgressLine)].SetValueExt<PMProformaProgressLine.subID>(newTran, subCD);

					if (note != null)
						PXNoteAttribute.SetNote(ProformaEntry.ProgressiveLines.Cache, newTran, note);
					if (files != null && files.Length > 0)
						PXNoteAttribute.SetFileNotes(ProformaEntry.ProgressiveLines.Cache, newTran, files);
				}
			}
			

			return newTran;
		}

		public virtual PMProforma InsertNewProformaDocument(string finPeriod, Customer customer, PMProject project, DateTime billingDate, string docDesc)
		{
			PMProforma doc = new PMProforma();
			doc.ProjectID = project.ContractID;
			doc.CustomerID = customer.BAccountID;
			doc.LocationID = project.LocationID ?? customer.DefLocationID;
			doc.InvoiceDate = billingDate;
			doc.Description = docDesc;
			doc.FinPeriodID = finPeriod;
			doc.BillAddressID = project.BillAddressID;
			doc.BillContactID = project.BillContactID;
			
			if (project.DefaultBranchID != null)
				doc.BranchID = project.DefaultBranchID;

			doc = ProformaEntry.Document.Insert(doc);

			ProformaEntry.Document.Cache.RaiseFieldUpdated<PMProforma.termsID>(doc, null);

			return doc;
		}

		public virtual ARInvoice InsertNewInvoiceDocument(string finPeriod, string docType, Customer customer, PMProject project,
			DateTime billingDate, string docDesc)
		{
			ARInvoice invoice = (ARInvoice)invoiceEntry.Document.Cache.CreateInstance();
			invoice.DocType = docType;
			invoice.CustomerID = customer.BAccountID;
			invoice.DocDate = billingDate;
			invoice.DocDesc = docDesc;
			invoice.CampaignID = project.CampaignID;
			if (project.DefaultBranchID != null)
				invoice.BranchID = project.DefaultBranchID;
			invoice = invoiceEntry.Document.Insert(invoice);
			if (!string.IsNullOrEmpty(finPeriod))
			{
				invoiceEntry.Document.Cache.SetValue<ARInvoice.finPeriodID>(invoice, finPeriod);
			}
			if (project.LocationID != null)
				invoiceEntry.Document.SetValueExt<ARInvoice.customerLocationID>(invoice, project.LocationID);//SetValueExt needed for correct TaxZone defaulting.
			invoice.ProjectID = project.ContractID;
		
			PMAddress addressPM = (PMAddress)PXSelect<PMAddress, Where<PMAddress.addressID, Equal<Required<PMProject.billAddressID>>>>.Select(this, project.BillAddressID);
			if (addressPM != null && addressPM.IsDefaultAddress != true)
			{
				ARAddress addressAR = invoiceEntry.Billing_Address.Current;

				addressAR.BAccountAddressID = addressPM.BAccountAddressID;
				addressAR.BAccountID = addressPM.BAccountID;
				addressAR.RevisionID = addressPM.RevisionID;
				addressAR.IsDefaultAddress = addressPM.IsDefaultAddress;
				addressAR.AddressLine1 = addressPM.AddressLine1;
				addressAR.AddressLine2 = addressPM.AddressLine2;
				addressAR.AddressLine3 = addressPM.AddressLine3;
				addressAR.City = addressPM.City;
				addressAR.State = addressPM.State;
				addressAR.PostalCode = addressPM.PostalCode;
				addressAR.CountryID = addressPM.CountryID;
				addressAR.IsValidated = addressPM.IsValidated;
			}

			PMContact contactPM = (PMContact)PXSelect<PMContact, Where<PMContact.contactID, Equal<Required<PMProject.billContactID>>>>.Select(this, project.BillContactID);

			if (contactPM != null && contactPM.IsDefaultContact != true)
			{
				ARContact contactAR = invoiceEntry.Billing_Contact.Current;

				contactAR.BAccountContactID = contactPM.BAccountContactID;
				contactAR.BAccountID = contactPM.BAccountID;
				contactAR.RevisionID = contactPM.RevisionID;
				contactAR.IsDefaultContact = contactPM.IsDefaultContact;
				contactAR.FullName = contactPM.FullName;
				contactAR.Salutation = contactPM.Salutation;
				contactAR.Title = contactPM.Title;
				contactAR.Phone1 = contactPM.Phone1;
				contactAR.Phone1Type = contactPM.Phone1Type;
				contactAR.Phone2 = contactPM.Phone2;
				contactAR.Phone2Type = contactPM.Phone2Type;
				contactAR.Phone3 = contactPM.Phone3;
				contactAR.Phone3Type = contactPM.Phone3Type;
				contactAR.Fax = contactPM.Fax;
				contactAR.FaxType = contactPM.FaxType;
				contactAR.Email = contactPM.Email;
			}

			CurrencyInfoAttribute.SetDefaults<ARInvoice.curyInfoID>(invoiceEntry.Document.Cache, invoice);
			return invoice;
		}

		public virtual string GetDocType(List<BillingData> value)
		{
			decimal amount = 0;
			foreach (BillingData data in value)
			{
				amount += data.Tran.Amount.GetValueOrDefault();
			}

			if (amount >= 0)
				return ARDocType.Invoice;
			else
				return ARDocType.CreditMemo;
		}

		public virtual bool IsNonGL(PMTran tran)
		{
			if (tran.IsNonGL == true)
				return true;

			if (tran.AccountID == null && tran.OffsetAccountID == null)
				return true;

			return false;
		}

		public virtual List<BillingData> BillTask(PMProject project, Customer customer, PMTask task, DateTime billingDate)
		{
			List<BillingData> list = new List<BillingData>();
			Dictionary<int, decimal> availableQty = new Dictionary<int, decimal>();//shared accross all billing rules.
			Dictionary<int, PMDetail> billingItems = new Dictionary<int, PMDetail>();//shared accross all billing rules.

			List<PMDetail> recurrentItems;
			List<PMBillingRule> rulesList;

			if (billingRules.TryGetValue(task.BillingID, out rulesList))
			{
				bool prepaymentAdded = false;
				foreach (PMBillingRule rule in rulesList)
				{

					if (!prepaymentAdded)
					{
						list.AddRange(BillPrepayment(project, customer, task, rule, billingDate));
						prepaymentAdded = true;
					}

					if (rule.Type == PMBillingType.Transaction)
					{
						list.AddRange(BillTask(project, customer, task, rule, billingDate, availableQty, billingItems));
					}
					else
						list.AddRange(BillFixPriceTask(project, customer, task, rule, billingDate));
				}
			}

			if (list.Count != 0 && recurrentItemsByTask.TryGetValue(task.TaskID.Value, out recurrentItems))
			{
				PMBillingRule rule = list.First<BillingData>().Rule;

				list.AddRange(BillRecurrentItems(recurrentItems, task, rule, billingDate, out availableQty, out billingItems));
			}
			else if (recurrentItemsByTask.TryGetValue(task.TaskID.Value, out recurrentItems))
			{
				PMBillingRule rule = rulesList.First<PMBillingRule>();

				list.AddRange(BillRecurrentItems(recurrentItems, task, rule, billingDate, out availableQty, out billingItems));
			}

			return list;
		}
				
		public virtual List<PMTran> ReverseWipTask(PMTask task, DateTime billingDate)
		{
			List<PMTran> list = new List<PMTran>();

			//usage:            
			PXSelectBase<PMTran> select = new PXSelect<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.taskID, Equal<Required<PMTran.taskID>>,
				And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
				And<PMTran.date, Less<Required<PMTran.date>>,
				And<PMTran.billed, Equal<False>,
				And<PMTran.released, Equal<True>,
				And<PMTran.reversed, Equal<False>>>>>>>>>(this);

			DateTime cuttofDate = billingDate;//all transactions  excluding the current day.
			ContractBillingSchedule schedule = PXSelect<ContractBillingSchedule>.Search<ContractBillingSchedule.contractID>(this, task.ProjectID);
			if (schedule != null && schedule.Type == BillingType.OnDemand)
			{
				cuttofDate = billingDate.AddDays(1);//ondemand always includes all transactions including the current day.
			}
			else
			{
				if (IncludeTodaysTransactions)
				{
					cuttofDate = billingDate.AddDays(1);
				}
			}

			foreach (PMTran tran in select.Select(task.ProjectID, task.TaskID, task.WipAccountGroupID, cuttofDate))
			{
				foreach (PMTran reversal in ReverseTran(tran))
				{
					reversal.Date = billingDate;
					reversal.FinPeriodID = null;
					reversal.TranDate = null;
					reversal.TranPeriodID = null;
					reversal.OrigProjectID = null;
					reversal.OrigTaskID = null;
					reversal.OrigAccountGroupID = null;

					list.Add(reversal);
				}
								
				tran.Billed = true;
				tran.BilledDate = billingDate;
				Transactions.Update(tran);

				PM.RegisterReleaseProcess.SubtractFromUnbilledSummary(this, tran);
			}

			return list;
		}

		public virtual List<BillingData> BillRecurrentItems(List<PMDetail> recurrentItems, PMTask task, PMBillingRule rule, DateTime billingDate, out Dictionary<int, decimal> availableQty, out Dictionary<int, PMDetail> billingItems)
		{
			List<BillingData> list = new List<BillingData>();
			availableQty = new Dictionary<int, decimal>();
			billingItems = new Dictionary<int, PMDetail>();

			if (task.IsCompleted == true)
				return list;


			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, task.ProjectID);
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, project.CustomerID);

			foreach (PMDetail billing in recurrentItems)
			{
				if (billing.InventoryID != null)
				{
					billingItems.Add(billing.InventoryID.Value, billing);

					if (billing.Included > 0)
					{
						if (billing.ResetUsage == ResetUsageOption.OnBilling)
						{
							availableQty.Add(billing.InventoryID.Value, billing.Included.Value);
						}
						else
						{
							decimal qtyLeft = billing.Included.Value - billing.LastBilledQty ?? 0;

							if (qtyLeft > 0)
							{
								availableQty.Add(billing.InventoryID.Value, qtyLeft);
							}
						}
					}
				}

				bool bill = false;
				if (billing.ResetUsage == ResetUsageOption.OnBilling)
				{
					bill = true;
				}
				else
				{
					if (billing.LastBilledDate == null)
						bill = true;
				}

				if (bill)
				{
					PMProformaTransactLine line = new PMProformaTransactLine();
					line.Type = PMProformaLineType.Transaction;
					line.InventoryID = billing.InventoryID;
					line.Description = billing.Description;
					line.BillableQty = billing.Included;
					line.Qty = line.BillableQty;
					line.UOM = billing.UOM;
					line.BillableAmount = billing.ItemFee;
					line.Amount = line.BillableAmount;
					line.ProjectID = task.ProjectID;
					line.TaskID = task.TaskID;
									

					string subCD = null;
					#region Set Account and Subaccount
					if (billing.AccountSource != PMAccountSource.None)
					{

						if (billing.AccountSource == PMAccountSource.RecurringBillingItem)
						{
							if (billing.AccountID != null)
							{
								line.AccountID = billing.AccountID;
							}
							else if (billing.InventoryID != null)
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.BillingRuleAccountIsNotConfiguredForBillingRecurent, item.InventoryCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.Project)
						{
							if (project.DefaultAccountID != null)
							{
								line.AccountID = project.DefaultAccountID;
							}
							else if (billing.InventoryID != null)
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.ProjectAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, project.ContractCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.Task)
						{
							if (task.DefaultAccountID != null)
							{
								line.AccountID = task.DefaultAccountID;
							}
							else if (billing.InventoryID != null)
							{
								InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
								throw new PXException(Messages.TaskAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, project.ContractCD, task.TaskCD);
							}
						}
						else if (billing.AccountSource == PMAccountSource.InventoryItem)
						{
							InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);

							if (item != null)
							{
								if (item.SalesAcctID != null)
								{
									line.AccountID = item.SalesAcctID;
								}
								else
								{
									throw new PXException(Messages.InventoryAccountIsNotConfiguredForBillingRecurent, item.InventoryCD);
								}
							}
						}
						else if (billing.AccountSource == PMAccountSource.Customer && customer != null)
						{
							CR.Location customerLoc = PXSelect<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(this, customer.BAccountID, customer.DefLocationID);
							if (customerLoc != null)
							{
								if (customerLoc.CSalesAcctID != null)
								{
									line.AccountID = customerLoc.CSalesAcctID;
								}
								else
								{
									InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
									throw new PXException(Messages.CustomerAccountIsNotConfiguredForBillingRecurent, item.InventoryCD, customer.AcctCD);
								}
							}
						}

						if (line.AccountID == null && !string.IsNullOrEmpty(billing.SubMask) && billing.InventoryID != null)
						{
							InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, billing.InventoryID);
							throw new PXException(Messages.SubAccountCannotBeComposed, item.InventoryCD);
						}
						else if (line.AccountID != null && !string.IsNullOrEmpty(billing.SubMask))
						{
							subCD = PMRecurentBillSubAccountMaskAttribute.MakeSub<PMBillingRule.subMask>(this, billing.SubMask,
								new object[] { billing.SubID, project.DefaultSubID, task.DefaultSubID },
								new Type[] { typeof(PMBillingRule.subID), typeof(PMProject.defaultSubID), typeof(PMTask.defaultSubID) });
						}
					}

					#endregion

					list.Add(new BillingData(line, rule, (PMTran)null, subCD, null, null));
					
					billing.LastBilledDate = billingDate;
					PMDetail.Update(billing);
				}
			}


			return list;
		}
		public virtual List<BillingData> BillTask(PMProject project, Customer customer, PMTask task, PMBillingRule rule, DateTime billingDate, Dictionary<int, decimal> availableQty, Dictionary<int, PMDetail> billingItems)
		{
			List<BillingData> list = new List<BillingData>();
			
			int mult = 1;
			PMAccountGroup ag = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, rule.AccountGroupID);
			if (ag == null)
			{
				throw new PXException(Messages.AccountGroupInBillingRuleNotFound, rule.BillingID, rule.AccountGroupID);
			}
			if (ag.Type == GL.AccountType.Liability || ag.Type == GL.AccountType.Income)
			{
				mult = -1;
			}

			List<PMTran> billingBase = SelectBillingBase(task.ProjectID, task.TaskID, rule.AccountGroupID, rule.IncludeNonBillable == true);
			Transform(billingBase, rule, task);

			var selectBAccount = new PXSelect<CR.BAccountR, Where<CR.BAccountR.bAccountID, Equal<Required<CR.BAccountR.bAccountID>>>>(this);
			
			if (rule.FullDetail)
			{
				foreach(PMTran tran in billingBase )
				{
					PMProformaTransactLine line = new PMProformaTransactLine();
					line.Type = PMProformaLineType.Transaction;
					line.BranchID = CalculateTargetBranchID(rule, project, task, tran, customer, tran.BranchID);
					line.InventoryID = tran.InventoryID;
					line.Description = tran.InvoicedDescription;
					line.UOM = tran.UOM;
					line.BillableQty = tran.InvoicedQty * mult;
					line.BillableAmount = tran.InvoicedAmount * mult;
					line.Qty = line.BillableQty;
					line.Amount = tran.Billable == true ? line.BillableAmount : 0 ;

					if (line.Qty != 0)
					{
						line.UnitPrice = line.Amount / line.Qty;
					}
					else
						line.UnitPrice = 0;
					line.ProjectID = task.ProjectID;
					line.TaskID = task.TaskID;
					line.Date = tran.Date;
					line.CostCodeID = tran.CostCodeID;
					line.ResourceID = tran.ResourceID;
										
					if (tran.BAccountID != null) // possible performance hit. TO REVIEW
					{
						CR.BAccountR ba = selectBAccount.Select(tran.BAccountID);
						if (ba != null && (ba.Type == CR.BAccountType.VendorType || ba.Type == CR.BAccountType.CombinedType))
						{
							line.VendorID = tran.BAccountID;
						}
					}

					line.AccountID = CalculateTargetSalesAccountID(rule, project, task, tran, line, customer);

					string subCD = CalculateTargetSalesSubaccountCD(rule, project, task, tran.ResourceID, tran.SubID, line.InventoryID, customer);
					
					string note = PXNoteAttribute.GetNote(Transactions.Cache, tran);
					Guid[] files = PXNoteAttribute.GetFileNotes(Transactions.Cache, tran);

					list.Add(new BillingData(line, rule, tran, subCD, note, files.ToArray()));

					if (billingItems.ContainsKey(tran.InventoryID.Value))
					{
						if (availableQty.ContainsKey(tran.InventoryID.Value))
						{
							decimal available = availableQty[tran.InventoryID.Value];

							if (tran.InvoicedQty <= available)
							{
								//Transaction is already payed for as a post payment included. Thus it should be free.
								using (new PXLocaleScope(customer.LocaleName))
									line.Description = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixIncludedUsage) + " " + tran.InvoicedDescription;
								availableQty[tran.InventoryID.Value] -= line.Qty.Value;//decrease available qty
								line.UnitPrice = 0;
								line.Amount = 0;
								line.BillableAmount = 0;
								line.Option = ARTran.pMDeltaOption.CompleteLine;
							}
							else
							{
								using (new PXLocaleScope(customer.LocaleName))
									line.Description = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixOverused) + " " + tran.InvoicedDescription;
								if (line.Qty != 0)
									line.BillableAmount = tran.InvoicedAmount * (line.Qty - available) / line.Qty;
								line.BillableQty = line.Qty - available;
								line.Amount = line.BillableAmount;
								line.Qty = line.BillableQty;
								line.Option = ARTran.pMDeltaOption.CompleteLine;

								availableQty[tran.InventoryID.Value] = 0;//all available qty was used.
							}
						}
					}
				}
			}
			else
			{
				Grouping grouping = CreateGrouping(rule);
				foreach (Group group in grouping.BreakIntoGroups(billingBase))
				{
					decimal sumBillableQty = 0;
					decimal sumAmt = 0;
					List<Guid> files = new List<Guid>();

					foreach (PMTran tr in group.List)
					{
						sumBillableQty += tr.InvoicedQty.GetValueOrDefault();
						sumAmt += tr.Billable == true ? tr.InvoicedAmount.GetValueOrDefault() : 0;
						files.AddRange(PXNoteAttribute.GetFileNotes(Transactions.Cache, tr));
					}

					if (group.List.Count > 0)
					{
						PMProformaTransactLine line = new PMProformaTransactLine();
						line.Type = PMProformaLineType.Transaction;
						line.BranchID =  CalculateTargetBranchID(rule, project, task, group.List[0], customer, group.List[0].BranchID);
						line.InventoryID = group.List[0].InventoryID;
						line.Description = group.List[0].InvoicedDescription;
						line.UOM = group.List[0].UOM;
						line.BillableQty = sumBillableQty * mult;
						line.BillableAmount = sumAmt * mult;
						line.Qty = line.BillableQty;
						line.Amount = line.BillableAmount;

						if (line.Qty != 0)
						{
							line.UnitPrice = line.Amount / line.Qty;
						}
						else
							line.UnitPrice = 0;
						line.ProjectID = task.ProjectID;
						line.TaskID = task.TaskID;
						line.Date = group.List[0].Date;
						line.CostCodeID = group.List[0].CostCodeID;
						line.ResourceID = group.List[0].ResourceID;
						if (group.List[0].BAccountID != null) // possible performance hit. TO REVIEW
						{
							CR.BAccountR ba = selectBAccount.Select(group.List[0].BAccountID);
							if (ba != null && (ba.Type == CR.BAccountType.VendorType || ba.Type == CR.BAccountType.CombinedType))
							{
								line.VendorID = group.List[0].BAccountID;
							}
						}
						line.AccountID = CalculateTargetSalesAccountID(rule, project, task, null, line, customer);

						string subCD = CalculateTargetSalesSubaccountCD(rule, project, task, line.ResourceID, group.List[0].SubID, line.InventoryID, customer);

						list.Add(new BillingData(line, rule, group.List, subCD, null, files.ToArray()));

						if (billingItems.ContainsKey(group.List[0].InventoryID.Value))
						{
							if (availableQty.ContainsKey(group.List[0].InventoryID.Value))
							{
								decimal available = availableQty[group.List[0].InventoryID.Value];

								if (group.List[0].InvoicedQty <= available)
								{
									//Transaction is already payed for as a post payment included. Thus it should be free.
									using (new PXLocaleScope(customer.LocaleName))
										line.Description = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixIncludedUsage) + " " + group.List[0].InvoicedDescription;
									availableQty[group.List[0].InventoryID.Value] -= line.Qty.Value;//decrease available qty
									line.UnitPrice = 0;
									line.Amount = 0;
									line.BillableAmount = 0;
									line.Option = ARTran.pMDeltaOption.CompleteLine;
								}
								else
								{
									using (new PXLocaleScope(customer.LocaleName))
										line.Description = PXMessages.LocalizeNoPrefix(CT.Messages.PrefixOverused) + " " + group.List[0].InvoicedDescription;
									if (line.Qty != 0)
										line.BillableAmount = group.List[0].InvoicedAmount * (line.Qty - available) / line.Qty;
									line.BillableQty = line.Qty - available;
									line.Amount = line.BillableAmount;
									line.Qty = line.BillableQty;
									line.Option = ARTran.pMDeltaOption.CompleteLine;

									availableQty[group.List[0].InventoryID.Value] = 0;//all available qty was used.
								}
							}
						}

						if (group.HasMixedInventory)
						{
							line.InventoryID = PMInventorySelectorAttribute.EmptyInventoryID; //mixed inventory in components
						}
						if (group.HasMixedUOM)
						{
							line.Qty = 0;
							line.BillableQty = 0;
							line.UOM = null;
							line.UnitPrice = 0;
						}
						if (group.HasMixedBAccount)
						{
							line.VendorID = null;
						}
					}
				}
			}
			
			return list;

		}

		public virtual List<BillingData> BillFixPriceTask(PMProject project, Customer customer, PMTask task, PMBillingRule rule, DateTime billingDate)
		{
			InitializeRatios(task.ProjectID);

			List<BillingData> list = new List<BillingData>();
			var select = new PXSelectJoin<PMRevenueBudget, 
				InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMRevenueBudget.accountGroupID>>>,
				Where<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>,
				And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);

			if (CostCodeAttribute.UseCostCode())
			{
				select.OrderByNew<OrderBy<Asc<PMRevenueBudget.inventoryID>>>();
			}
			
			foreach ( PXResult<PMRevenueBudget, PMAccountGroup> res in select.Select(task.TaskID))
			{
				PMRevenueBudget line = (PMRevenueBudget)res;
				PMAccountGroup ag = (PMAccountGroup)res;

				decimal unbilledAmount;

				string key = string.Format("{0}.{1}", line.ProjectTaskID, line.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID));
				decimal? ratio = null;
				if (ratios.TryGetValue(key, out ratio) && ratio != null)//autocalculated ratio.
				{
					unbilledAmount = line.RevisedAmount.GetValueOrDefault() * ratio.Value;
				}
				else
				{
					unbilledAmount = line.AmountToInvoice.GetValueOrDefault();
				}
				
				if (unbilledAmount != 0 || rule.IncludeZeroAmount == true)
				{
					string description = null;
					if (!string.IsNullOrEmpty(rule.DescriptionFormula))
					{
						PMTran container = new PMTran();
						container.ProjectID = line.ProjectID;
						container.TaskID = line.ProjectTaskID;
						container.Description = line.Description;
						container.AccountGroupID = line.AccountGroupID;
						container.InventoryID = line.InventoryID;
						container.CostCodeID = line.CostCodeID;
						container.UOM = line.UOM;
						container.Amount = unbilledAmount;

						decimal? qty = null;
						decimal? amt = null;

						PMDataNavigator navigator = new PMDataNavigator(this, new List<PMTran>(new PMTran[1] { container }));
						CalculateFormulas(navigator, rule, container, out qty, out amt, out description);
					}
					else
					{
						description = line.Description;
					}

					PMProformaProgressLine proformaLine = new PMProformaProgressLine();
					proformaLine.Type = PMProformaLineType.Progressive;
					proformaLine.Description = description;
					proformaLine.BillableAmount = unbilledAmount;
					proformaLine.BillableQty = 0;
					proformaLine.Amount = proformaLine.BillableAmount;
					proformaLine.Qty = proformaLine.BillableQty;
					proformaLine.UOM = line.UOM;
					proformaLine.ProjectID = line.ProjectID;
					proformaLine.TaskID = line.ProjectTaskID;
					proformaLine.AccountGroupID = line.AccountGroupID;
					proformaLine.CostCodeID = line.CostCodeID;
					proformaLine.InventoryID = line.InventoryID;
					proformaLine.TaxCategoryID = line.TaxCategoryID;
					proformaLine.AccountID = CalculateTargetSalesAccountID(rule, project, task, null, proformaLine, customer);
					proformaLine.BranchID = CalculateTargetBranchID(rule, project, task, null, customer, null);
										
					string subCD = CalculateTargetSalesSubaccountCD(rule, project, task, null, null, line.InventoryID, customer);

					string note = PXNoteAttribute.GetNote(Caches[typeof(PMRevenueBudget)], task);
					Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(PMRevenueBudget)], task);
					list.Add(new BillingData(proformaLine, rule, (PMTran)null, subCD, note, files));
				}

			}

			return list;
		}

		public virtual List<BillingData> BillPrepayment(PMProject project, Customer customer, PMTask task, PMBillingRule rule, DateTime billingDate)
		{
			List<BillingData> list = new List<BillingData>();
			var select = new PXSelectJoin<PMRevenueBudget,
				InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<PMRevenueBudget.accountGroupID>>>,
				Where<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>,
				And<PMRevenueBudget.type, Equal<GL.AccountType.income>,
				And<PMRevenueBudget.prepaymentAmount, Greater<PMRevenueBudget.prepaymentInvoiced>>>>>(this);

			if (CostCodeAttribute.UseCostCode())
			{
				select.OrderByNew<OrderBy<Asc<PMRevenueBudget.inventoryID>>>();
			}

			foreach (PXResult<PMRevenueBudget, PMAccountGroup> res in select.Select(task.TaskID))
			{
				PMRevenueBudget line = (PMRevenueBudget)res;
				PMAccountGroup ag = (PMAccountGroup)res;

				PMProformaLine proformaLine;
				if (rule.Type == PMBillingType.Transaction)
				{
					proformaLine = new PMProformaTransactLine();
					proformaLine.Type = PMProformaLineType.Transaction;
				}
				else
				{
					proformaLine = new PMProformaProgressLine();
					proformaLine.Type = PMProformaLineType.Progressive;
				}
				proformaLine.IsPrepayment = true;
				proformaLine.Description = Messages.Prepayment;
				proformaLine.BillableAmount = line.PrepaymentAmount.Value - line.PrepaymentInvoiced.Value;
				proformaLine.BillableQty = 0;
				proformaLine.Amount = proformaLine.BillableAmount;
				proformaLine.Qty = proformaLine.BillableQty;
				proformaLine.UOM = line.UOM;
				proformaLine.ProjectID = line.ProjectID;
				proformaLine.TaskID = line.ProjectTaskID;
				proformaLine.AccountGroupID = line.AccountGroupID;
				proformaLine.CostCodeID = line.CostCodeID;
				proformaLine.InventoryID = line.InventoryID;
				proformaLine.TaxCategoryID = line.TaxCategoryID;
				proformaLine.DefCode = project.PrepaymentDefCode;
				proformaLine.BranchID = CalculateTargetBranchID(rule, project, task, null, customer, null);

				string subCD = CalculateTargetSalesSubaccountCD(rule, project, task, null, null, line.InventoryID, customer);

				string note = PXNoteAttribute.GetNote(Caches[typeof(PMRevenueBudget)], task);
				Guid[] files = PXNoteAttribute.GetFileNotes(Caches[typeof(PMRevenueBudget)], task);
				BillingData data = new BillingData(proformaLine, rule, (PMTran)null, subCD, note, files);
				list.Add(data);

			}

			return list;
		}

		public virtual int? CalculateTargetBranchID(PMBillingRule rule, PMProject project, PMTask task, PMTran tran, Customer customer, int? defaultvalue)
		{
			int? result = defaultvalue;

			if (rule.BranchSource == PMAccountSource.Project && project.DefaultBranchID != null)
			{
				result = project.DefaultBranchID;
			}
			else if (rule.BranchSource == PMAccountSource.Task && task.DefaultBranchID != null)
			{
				result = task.DefaultBranchID;
			}
			else if (rule.BranchSource == PMAccountSource.BillingRule && rule.TargetBranchID != null)
			{
				result = rule.TargetBranchID;
			}
			else if (rule.BranchSource == PMAccountSource.Employee && tran != null && tran.ResourceID != null)
			{
				EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, tran.ResourceID);
				if (emp != null)
				{
					Branch branch = PXSelect<Branch, Where<Branch.bAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>.Select(this, emp.ParentBAccountID);
					if (branch != null)
					{
						result = branch.BranchID;
					}
				}
			}
			else if (rule.BranchSource == PMAccountSource.Customer)
			{
				CR.Location defLocation = PXSelect<CR.Location, Where<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>.Select(this, customer.DefLocationID);
				if (defLocation != null && defLocation.CBranchID != null)
				{
					result = defLocation.CBranchID;
				}
			}

			return result;
		}

		public virtual int? CalculateTargetSalesAccountID(PMBillingRule rule, PMProject project, PMTask task, PMTran tran, PMProformaLine line, Customer customer)
		{
			int? result = null;

			if (rule.AccountSource == PMAccountSource.BillingRule)
			{
				if (rule.AccountID != null)
				{
					result = rule.AccountID;
				}
				else
				{
					throw new PXException(Messages.BillingRuleAccountIsNotConfiguredForBilling, rule.BillingID);
				}
			}
			else if (rule.AccountSource == PMAccountSource.Project)
			{
				if (project.DefaultAccountID != null)
				{
					result = project.DefaultAccountID;
				}
				else
				{
					throw new PXException(Messages.ProjectAccountIsNotConfiguredForBilling, rule.BillingID, project.ContractCD);
				}
			}
			else if (rule.AccountSource == PMAccountSource.Task)
			{
				if (task.DefaultAccountID != null)
				{
					result = task.DefaultAccountID;
				}
				else
				{
					throw new PXException(Messages.TaskAccountIsNotConfiguredForBilling, rule.BillingID, project.ContractCD, task.TaskCD);
				}
			}
			else if (rule.AccountSource == PMAccountSource.InventoryItem)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, line.InventoryID);

				if (item != null && item.ItemStatus != IN.InventoryItemStatus.Unknown)
				{
					if (item.SalesAcctID != null)
					{
						result = item.SalesAcctID;
					}
					else
					{
						throw new PXException(Messages.InventoryAccountIsNotConfiguredForBilling, rule.BillingID, item.InventoryCD);
					}
				}
			}
			else if (rule.AccountSource == PMAccountSource.Customer && customer != null)
			{
				CR.Location customerLoc = PXSelect<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(this, customer.BAccountID, customer.DefLocationID);
				if (customerLoc != null)
				{
					if (customerLoc.CSalesAcctID != null)
					{
						result = customerLoc.CSalesAcctID;
					}
					else
					{
						throw new PXException(Messages.CustomerAccountIsNotConfiguredForBilling, rule.BillingID, customer.AcctCD);
					}
				}
			}
			else if (rule.AccountSource == PMAccountSource.Employee)
			{
				EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, line.ResourceID);

				if (emp != null)
				{
					if (emp.SalesAcctID != null)
					{
						result = emp.SalesAcctID;
					}
					else
					{
						throw new PXException(Messages.EmployeeAccountIsNotConfiguredForBilling, rule.BillingID, emp.AcctCD);
					}
				}
			}
			else if (rule.AccountSource == PMAccountSource.AccountGroup)
			{
				PMAccountGroup accountGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, line.AccountGroupID);

				if (accountGroup != null)
				{
					if (accountGroup.AccountID != null)
					{
						result = accountGroup.AccountID;
					}
					else
					{
						throw new PXException(Messages.DefaultAccountIsNotConfiguredForBilling, rule.BillingID, accountGroup.GroupCD);
					}
				}
			}
			else if (rule.AccountSource == PMAccountSource.None && tran != null)
			{
					result = tran.AccountID;
			}

				return result;
		}

		public virtual string CalculateTargetSalesSubaccountCD(PMBillingRule rule, PMProject project, PMTask task, int? employeeID, int? sourceSubID, int? inventoryID, Customer customer)
		{
			string result = null;
			string subMusk = rule.SubMask;

			int? employeeSubID = null;
			int? inventorySubID = null;
			int? customerSubID = null;

			if (employeeID != null && !string.IsNullOrEmpty(rule.SubMask) && rule.SubMask.Contains(AcctSubDefault.Employee))
			{
				EP.EPEmployee emp = PXSelect<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Required<EP.EPEmployee.bAccountID>>>>.Select(this, employeeID);

				if (emp != null)
				{
					employeeSubID = emp.SalesSubID;
				}
			}

			if (inventoryID != null && !string.IsNullOrEmpty(rule.SubMask) && rule.SubMask.Contains(AcctSubDefault.Inventory) && PMInventorySelectorAttribute.EmptyInventoryID != inventoryID)
			{
				InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, inventoryID);

				if (item != null)
				{
					inventorySubID = item.SalesSubID;
				}
			}
			else if (PMInventorySelectorAttribute.EmptyInventoryID == inventoryID)
			{
				CR.Location customerLoc = PXSelect<CR.Location, Where<CR.Location.bAccountID, Equal<Required<CR.Location.bAccountID>>, And<CR.Location.locationID, Equal<Required<CR.Location.locationID>>>>>.Select(this, customer.BAccountID, customer.DefLocationID);

				if (customerLoc != null)
				{
					customerSubID = customerLoc.CSalesSubID;
				}
				subMusk = subMusk.Replace('I', 'C');
			}

			result = PMBillSubAccountMaskAttribute.MakeSub<PMBillingRule.subMask>(this, subMusk,
			new object[] { rule.SubID, project.DefaultSubID, task.DefaultSubID, employeeSubID, inventorySubID, customerSubID, sourceSubID },
			new Type[] { typeof(PMBillingRule.subID), typeof(PMProject.defaultSubID), typeof(PMTask.defaultSubID), typeof(EP.EPEmployee.salesSubID), typeof(InventoryItem.salesSubID), typeof(CR.Location.cSalesSubID), typeof(PMProformaLine.subID) });


			return result;
		}

		public virtual void Transform(List<PMTran> billingBase, PMBillingRule rule, PMTask task)
		{
			foreach(PMTran tran in billingBase)
			{
				tran.Rate = GetRate(rule, tran, task.RateTableID);

				decimal? qty = null;
				decimal? amt = null;
				string desc = null;

				PMDataNavigator navigator = new PMDataNavigator(this, new List<PMTran>(new PMTran[1] { tran }));
				CalculateFormulas(navigator, rule, tran, out qty,  out amt, out desc);
				tran.InvoicedQty = qty;
				tran.InvoicedAmount = amt;
				tran.InvoicedDescription = desc;
			}
		}

		protected virtual void CalculateFormulas(PMDataNavigator navigator, PMBillingRule rule, PMTran tran, out decimal? qty, out decimal? amt, out string description)
		{
			qty = null;
			amt = null;
			description = null;

			if (!string.IsNullOrEmpty(rule.QtyFormula) && tran.RemainderOfTranID == null)
			{
				try
				{
					ExpressionNode qtyNode = PMExpressionParser.Parse(this, rule.QtyFormula);
					qtyNode.Bind(navigator);
					object val = qtyNode.Eval(tran);
					if (val != null)
					{
						qty = Convert.ToDecimal(val);
					}
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcQtyFormula_Billing, rule.BillingID, rule.QtyFormula, ex.Message);
				}
			}
			else
			{
				qty = tran.Qty;
			}
						
			if (!string.IsNullOrEmpty(rule.AmountFormula) && tran.RemainderOfTranID == null)
			{
				try
				{
					ExpressionNode amtNode = PMExpressionParser.Parse(this, rule.AmountFormula);
					amtNode.Bind(navigator);
					object val = amtNode.Eval(tran);
					if (val != null)
					{
						amt = Convert.ToDecimal(val);
					}
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcAmtFormula_Billing, rule.BillingID, rule.AmountFormula, ex.Message);
				}
			}
			else
			{
				amt = tran.Amount;
			}

			if (!string.IsNullOrEmpty(rule.DescriptionFormula))
			{
				try
				{
					ExpressionNode descNode = PMExpressionParser.Parse(this, rule.DescriptionFormula);
					descNode.Bind(navigator);
					object val = descNode.Eval(tran);
					if (val != null)
						description = val.ToString();
				}
				catch (Exception ex)
				{
					throw new PXException(Messages.FailedToCalcDescFormula_Billing, rule.BillingID, rule.DescriptionFormula, ex.Message);
				}
			}
			else
			{
				description = tran.Description;
			}

		}

		public virtual object Evaluate(PMObjectType objectName, string fieldName, string attribute, PMTran row)
		{
			switch (objectName)
			{
				case PMObjectType.PMTran:
					return ConvertFromExtValue(this.Caches[typeof(PMTran)].GetValueExt(row, fieldName));
				case PMObjectType.PMBudget:
					PMBudget budget = PXSelect<PMBudget, Where<PMBudget.projectID, Equal<Required<PMBudget.projectID>>, 
						And<PMBudget.projectTaskID, Equal<Required<PMBudget.projectTaskID>>,
						And<PMBudget.accountGroupID, Equal<Required<PMBudget.accountGroupID>>,
						And<PMBudget.inventoryID, Equal<Required<PMBudget.inventoryID>>,
						And<PMBudget.costCodeID, Equal<Required<PMBudget.costCodeID>>>>>>>>.Select(this, row.ProjectID, row.TaskID, row.AccountGroupID, row.InventoryID, row.CostCodeID);
					if(budget != null)
					{
						return ConvertFromExtValue(this.Caches[typeof(PMBudget)].GetValueExt(budget, fieldName));
					}
					break;
				case PMObjectType.PMProject:
					PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(this, row.ProjectID);
					if (project != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, project.NoteID);
						}
						else
						{
							return ConvertFromExtValue(this.Caches[typeof(PMProject)].GetValueExt(project, fieldName));
						}
					}
					break;
				case PMObjectType.PMTask:
					PMTask task = PXSelect<PMTask, Where<PMTask.projectID, Equal<Required<PMTask.projectID>>, And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(this, row.ProjectID, row.TaskID);
					if (task != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, task.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(PMTask)].GetValueExt(task, fieldName));
					}
					break;
				case PMObjectType.PMAccountGroup:
					PMAccountGroup accGroup = PXSelect<PMAccountGroup, Where<PMAccountGroup.groupID, Equal<Required<PMAccountGroup.groupID>>>>.Select(this, row.AccountGroupID);
					if (accGroup != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, accGroup.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(PMAccountGroup)].GetValueExt(accGroup, fieldName));
					}
					break;
				case PMObjectType.EPEmployee:
					EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(this, row.ResourceID);
					if (employee != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, employee.NoteID);

						}
						else
							return ConvertFromExtValue(this.Caches[typeof(EPEmployee)].GetValueExt(employee, fieldName));
					}
					break;
				case PMObjectType.Customer:
					Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, row.BAccountID);
					if (customer != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, customer.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(Customer)].GetValueExt(customer, fieldName));
					}
					break;
				case PMObjectType.Vendor:
					VendorR vendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Required<VendorR.bAccountID>>>>.Select(this, row.BAccountID);
					if (vendor != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, vendor.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(VendorR)].GetValueExt(vendor, fieldName));
					}
					break;
				case PMObjectType.InventoryItem:
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(this, row.InventoryID);
					if (item != null)
					{
						if (attribute != null)
						{
							return EvaluateAttribute(attribute, item.NoteID);
						}
						else
							return ConvertFromExtValue(this.Caches[typeof(InventoryItem)].GetValueExt(item, fieldName));
					}
					break;
				default:
					break;
			}

			return null;
		}

		public virtual decimal? GetPrice(PMTran tran)
		{
			decimal? result = null;

			if (tran.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
			{
				string customerPriceClass = ARPriceClass.EmptyPriceClass;

				PMTask projectTask = (PMTask)PXSelectorAttribute.Select(Caches[typeof(PMTran)], tran, "TaskID");
				CR.Location c = (CR.Location)PXSelectorAttribute.Select(Caches[typeof(PMTask)], projectTask, "LocationID");
				if (c != null && !string.IsNullOrEmpty(c.CPriceClassID))
					customerPriceClass = c.CPriceClassID;

				CM.CurrencyInfo dummy = new CM.CurrencyInfo();
				dummy.CuryID = CompanySetup.Current.BaseCuryID;//Accessinfo.BaseCuryID;
				dummy.BaseCuryID = CompanySetup.Current.BaseCuryID;//Accessinfo.BaseCuryID;
				dummy.CuryRate = 1;

				result = ARSalesPriceMaint.CalculateSalesPrice(Caches[typeof(PMTran)], customerPriceClass, projectTask.CustomerID, tran.InventoryID, dummy, tran.Qty, tran.UOM, tran.Date.Value, true);
			}

			return result;
		}

		protected virtual object ConvertFromExtValue(object extValue)
		{
			PXFieldState fs = extValue as PXFieldState;
			if (fs != null)
				return fs.Value;
			else
			{
				return extValue;
			}
		}

		protected virtual object EvaluateAttribute(string attribute, Guid? refNoteID)
		{
			PXResultset<CSAnswers> res = PXSelectJoin<CSAnswers,
				InnerJoin<CSAttribute, On<CSAttribute.attributeID, Equal<CSAnswers.attributeID>>>,
				Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>,
					And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>>>>.Select(this, refNoteID, attribute);

			CSAnswers ans = null;
			CSAttribute attr = null;
			if (res.Count > 0)
			{
				ans = (CSAnswers)res[0][0];
				attr = (CSAttribute)res[0][1];
			}

			if (ans == null || ans.AttributeID == null)
			{
				//answer not found. if attribute exists return the default value.
				attr = PXSelect<CSAttribute, Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>.Select(this, attribute);

				if (attr != null && attr.ControlType == CSAttribute.CheckBox)
				{
					return false;
				}
			}

			if (ans != null)
			{
				if (ans.Value != null)
					return ans.Value;
				else
				{
					if (attr != null && attr.ControlType == CSAttribute.CheckBox)
					{
						return false;
					}
				}
			}

			return string.Empty;
		}
				
		public virtual bool PreSelectTasksTransactions(int? projectID, List<PMTask> tasks, DateTime? cuttoffDate)
		{
			transactions.Clear();

			if (tasks.Count == 0) return false;

			HashSet<int> distinctAccountGroups = new HashSet<int>();
			
			foreach (PMTask task in tasks)
			{
				transactions.Add(task.TaskID.Value, new Dictionary<int, List<PXResult<PMTran>>>());

				List<PMBillingRule> rulesList;
				if (!billingRules.TryGetValue(task.BillingID, out rulesList))
				{
					PXSelectBase<PMBillingRule> billingRuleSelect = new PXSelect<PMBillingRule, Where<PMBillingRule.billingID, Equal<Required<PMBillingRule.billingID>>, And<PMBillingRule.isActive, Equal<True>>>>(this);
					rulesList = new List<PMBillingRule>();
					foreach (PMBillingRule rule in billingRuleSelect.Select(task.BillingID))
					{
						rulesList.Add(rule);
					}
					billingRules.Add(task.BillingID, rulesList);
				}
				
				foreach (PMBillingRule rule in rulesList)
				{
					if (rule.Type == PMBillingType.Transaction)
						distinctAccountGroups.Add(rule.AccountGroupID.Value);
				}
			}
			
			foreach (int distinctAccountGroup in distinctAccountGroups)
			{
				foreach (PXResult<PMTran> tran in GetTranFromDatabase(projectID, distinctAccountGroup, cuttoffDate))
				{
					Dictionary<int, List<PXResult<PMTran>>> transactionsByAccountGroup;

					if (transactions.TryGetValue(((PMTran)tran).TaskID.Value, out transactionsByAccountGroup))
					{
						List<PXResult<PMTran>> trans;
						if (!transactionsByAccountGroup.TryGetValue(distinctAccountGroup, out trans))
						{
							trans = new List<PXResult<PMTran>>();
							transactionsByAccountGroup.Add(distinctAccountGroup, trans);
						}

						trans.Add(tran);
					}
				}
			}

			return true;
		}

		public virtual void PreSelectRecurrentItems(int? projectID)
		{
			recurrentItemsByTask.Clear();

			PXSelectBase<PMDetail> selectBilling = new PXSelect<PMDetail,
			Where<PMDetail.contractID, Equal<Required<PMDetail.contractID>>>>(this);

			foreach (PMDetail billing in selectBilling.Select(projectID))
			{
				List<PMDetail> recurrentItems;

				if (!recurrentItemsByTask.TryGetValue(billing.TaskID.Value, out recurrentItems))
				{
					recurrentItems = new List<PMDetail>();
					recurrentItemsByTask.Add(billing.TaskID.Value, recurrentItems);
				}

				recurrentItems.Add(billing);
			}
		}

		public virtual List<PMTran> SelectBillingBase(int? projectID, int? taskID, int? accountGroupID, bool includeNonBillable)
		{
			List<PMTran> list = new List<PMTran>();

			//get from memory (pre-selected/cached)
			Dictionary<int, List<PXResult<PMTran>>> transactionsByTask;
			if (transactions.TryGetValue(taskID.Value, out transactionsByTask))
			{
				List<PXResult<PMTran>> source;
				if (transactionsByTask.TryGetValue(accountGroupID.Value, out source))
				{
					foreach (PMTran tran in source)
					{
						if (includeNonBillable == false && tran.Billable != true)
							continue;

						list.Add(tran);
					}
				}
			}

			return list;
		}

		public virtual PXResultset<PMTran> GetTranFromDatabase(int? projectID, int groupID, DateTime? cuttofDate)
		{
			PXSelectBase<PMTran> selectTrans = new PXSelectReadonly<PMTran,
					Where<PMTran.billed, Equal<False>,
					And<PMTran.released, Equal<True>,
					And<PMTran.reversed, Equal<False>,
					And<PMTran.accountGroupID, Equal<Required<PMTran.accountGroupID>>,
					And<PMTran.projectID, Equal<Required<PMTran.projectID>>,
					And<PMTran.date, Less<Required<PMTran.date>>>>>>>>>(this);
			
			return selectTrans.Select(groupID, projectID, cuttofDate) ;
		}

		public virtual IList<PMTran> ReverseTran(PMTran tran)
		{
			List<PMTran> list = new List<PMTran>();

			if (IsNonGL(tran))
			{
				list.AddRange(ReverseTranNonGL(tran));
			}
			else
			{
				list.Add(ReverseTranGL(tran));
			}

			return list;
		}

		public virtual PMTran ReverseTranGL(PMTran tran)
		{
			Account offsetAccount = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(this, tran.OffsetAccountID);

			if (offsetAccount != null && offsetAccount.AccountGroupID != null)
			{
				//Debit-Credit reversal
				PMTran rvrs = PXCache<PMTran>.CreateCopy(tran);
				rvrs.TranID = null;
				rvrs.TranType = null;
				rvrs.RefNbr = null;
				rvrs.RefLineNbr = null;
				rvrs.BatchNbr = null;
				rvrs.TranDate = null;
				rvrs.TranPeriodID = null;
				rvrs.Released = null;
				rvrs.AccountID = tran.OffsetAccountID;
				rvrs.SubID = tran.OffsetSubID;
				rvrs.OffsetAccountID = tran.AccountID;
				rvrs.OffsetSubID = tran.SubID;
				rvrs.AccountGroupID = offsetAccount.AccountGroupID;
				return rvrs;
			}
			else
			{
				//-ve reversal
				PMTran rvrs = PXCache<PMTran>.CreateCopy(tran);
				rvrs.TranID = null;
				rvrs.TranType = null;
				rvrs.RefNbr = null;
				rvrs.RefLineNbr = null;
				rvrs.BatchNbr = null;
				rvrs.TranDate = null;
				rvrs.TranPeriodID = null;
				rvrs.Released = null;
				rvrs.Amount *= -1;
				rvrs.Qty *= -1;
				rvrs.BillableQty *= -1;
				return rvrs;
			}
		}

		public virtual IList<PMTran> ReverseTranNonGL(PMTran tran)
		{
			List<PMTran> list = new List<PMTran>();

			//debit:
			PMTran debit = new PMTran();
			debit.AccountGroupID = tran.AccountGroupID;
			debit.OffsetAccountGroupID = tran.OffsetAccountGroupID;
			debit.ProjectID = tran.ProjectID;
			debit.Date = tran.Date;
			debit.FinPeriodID = tran.FinPeriodID;
			debit.TaskID = tran.TaskID;
			debit.InventoryID = tran.InventoryID;
			debit.Description = tran.Description;
			debit.UOM = tran.UOM;
			debit.Qty = -tran.Qty;
			debit.Billable = tran.Billable;
			debit.BillableQty = -tran.BillableQty;
			debit.Amount = -tran.Amount;
			debit.Allocated = true;
			debit.Billed = true;
			debit.IsNonGL = true;
			list.Add(debit);

			//credit:
			if (tran.OffsetAccountGroupID != null)
			{
				PMTran credit = new PMTran();
				credit.AccountGroupID = tran.OffsetAccountGroupID;
				credit.ProjectID = tran.ProjectID;
				credit.TaskID = tran.TaskID;
				credit.InventoryID = tran.InventoryID;
				credit.Description = tran.Description;
				credit.Date = tran.Date;
				credit.FinPeriodID = tran.FinPeriodID;
				credit.UOM = tran.UOM;
				credit.Qty = tran.Qty;
				credit.Billable = tran.Billable;
				credit.BillableQty = tran.BillableQty;
				credit.Amount = tran.Amount;
				credit.Allocated = true;
				credit.Billed = true;
				credit.IsNonGL = true;
				list.Add(credit);
			}

			return list;
		}

		public static DateTime? GetNextBillingDate(PXGraph graph, ContractBillingSchedule schedule, DateTime? date)
		{
			if (date == null)
				return null;

			switch (schedule.Type)
			{
				case BillingType.Annual:
					return date.Value.AddYears(1);
				case BillingType.Monthly:
                    return date.Value.AddMonths(1);
				case BillingType.Weekly:
                    return date.Value.AddDays(7);
				case BillingType.Quarterly:
                    return date.Value.AddMonths(3);
                case BillingType.OnDemand:
			        return null;
				default:
					throw new ArgumentException(Messages.InvalidScheduleType, "schedule");
			}
		}

		public virtual RateEngineV2 CreateRateEngineV2(IList<string> rateTables, IList<string> rateTypes)
		{
			return new RateEngineV2(this, rateTables, rateTypes);
		}

		/// <summary>
		/// Returns RateDefinitions from Cached rateDefinitions collection or from database if not found.
		/// </summary>
		public virtual IList<PMRateDefinition> GetRateDefinitions(string rateTable)
		{
			List<PMRateDefinition> result;
			if (!rateDefinitions.TryGetValue(rateTable, out result))
			{
				PXSelectBase<PMRateDefinition> select = new PXSelect<PMRateDefinition,
			   Where<PMRateDefinition.rateTableID, Equal<Required<PMRateDefinition.rateTableID>>>,
			   OrderBy<Asc<PMRateDefinition.rateTypeID, Asc<PMRateDefinition.sequence>>>>(this);

				result = new List<PMRateDefinition>(select.Select(rateTable).RowCast<PMRateDefinition>());
				rateDefinitions.Add(rateTable, result);
			}

			return result;
		}

		protected virtual decimal? GetRate(PMBillingRule rule, PMTran tran, string rateTableID)
		{
			if (string.IsNullOrEmpty(rule.RateTypeID))
			{
				switch (rule.NoRateOption)
				{
					case PMNoRateOption.SetZero:
						return 0;
					case PMNoRateOption.RaiseError:
						throw new PXException(Messages.RateTypeNotDefinedForBilling, rule.BillingID, rule.StepID);
					case PMNoRateOption.DontAllocate:
						return null;
					default:
						return 1;
				}
			}

			decimal? rate = null;
			string trace = null;

			if (!string.IsNullOrEmpty(rateTableID))
			{
				rate = rateEngine.GetRate(rateTableID, rule.RateTypeID, tran);
				trace = rateEngine.GetTrace(tran);
			}

			if (rate != null)
				return rate;
			else
			{
				switch (rule.NoRateOption)
				{
					case PMNoRateOption.SetZero:
						return 0;
					case PMNoRateOption.RaiseError:
						PXTrace.WriteInformation(trace);
						PXTrace.WriteError(Messages.RateNotDefinedForStep, rule.BillingID, rule.StepID);
						throw new PXException(Messages.RateNotDefinedForStep, rule.BillingID, rule.StepID);
					case PMNoRateOption.DontAllocate:
						PXTrace.WriteInformation(trace);
						return null;
					default:
						return 1;
				}
			}

		}

		public virtual void InitializeRatios(int? projectID)
		{
			if (ratios == null)
			{
				var selectCostBudgetWithProduction = new PXSelectGroupBy<PMCostBudget, Where<PMCostBudget.isProduction, Equal<True>,
						And<PMCostBudget.type, Equal<GL.AccountType.expense>,
						And<PMCostBudget.projectID, Equal<Required<PMCostBudget.projectID>>>>>,
						Aggregate<GroupBy<PMCostBudget.projectID,
						GroupBy<PMCostBudget.revenueTaskID,
						GroupBy<PMCostBudget.revenueInventoryID,
						Sum<PMCostBudget.revisedAmount,
						Sum<PMCostBudget.actualAmount>>>>>>>(this);

				ratios = new Dictionary<string, decimal?>();

				foreach (PMCostBudget total in selectCostBudgetWithProduction.Select(projectID))
				{
					string key = string.Format("{0}.{1}", total.RevenueTaskID, total.RevenueInventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID));
					decimal? ratio = null;

					if (total.RevisedAmount.GetValueOrDefault() != 0)
					{
						ratio = decimal.Round(total.ActualAmount.GetValueOrDefault() / total.RevisedAmount.GetValueOrDefault(), 2);
					}

					ratios.Add(key, ratio);
				}
			}
		}

		public virtual Grouping CreateGrouping(PMBillingRule rule)
		{
			PMTranComparer comparer = new PMTranComparer(rule.GroupByItem, rule.GroupByVendor, rule.GroupByDate, rule.GroupByEmployee, false);

			return new Grouping(comparer);
		}

		public class BillingData
		{
			public PMProformaLine Tran { get; private set; }
			public string SubCD { get; private set; }
			public PMBillingRule Rule { get; private set; }
			public List<PMTran> Transactions { get; private set; }
			public string Note { get; private set; }
			public Guid[] Files { get; private set; }

			public BillingData(PMProformaLine tran, PMBillingRule rule, PMTran pmTran, string subCD, string note, Guid[] files)
			{
				this.Transactions = new List<PMTran>();
				if (pmTran != null)
					this.Transactions.Add(pmTran);
				this.Tran = tran;
				this.Rule = rule;
				this.SubCD = subCD;
				this.Note = note;
				this.Files = files;
			}

			public BillingData(PMProformaLine tran, PMBillingRule rule, List<PMTran> transactions, string subCD, string note, Guid[] files)
			{
				this.Transactions = transactions;
				this.Tran = tran;
				this.Rule = rule;
				this.SubCD = subCD;
				this.Note = note;
				this.Files = files;
			}
		}

		public class BillingResult
		{
			/// <summary>
			/// Created and Modified proformas during billing
			/// </summary>
			public List<PMProforma> Proformas { get; }

			/// <summary>
			/// Created Invoices during billing
			/// </summary>
			public List<ARRegister> Invoices { get; }

			public bool IsEmpty
			{
				get { return Proformas.Count + Invoices.Count == 0; }
			}

			public bool IsSingle
			{
				get { return Proformas.Count + Invoices.Count == 1; }
			}


			public BillingResult()
			{
				Proformas = new List<PMProforma>();
				Invoices = new List<ARRegister>(); 
			}
		}

		public class InvoicePersistingHandler
		{
			List<BillingData> billedData;
			PMBillingRecord billingRecord;

			public void SetData(List<BillingData> billedData, PMBillingRecord billingRecord)
			{
				this.billedData = billedData;
				this.billingRecord = billingRecord;
			}

			public virtual void OnProformaPersisted(PXCache sender, PXRowPersistedEventArgs e)
			{
				if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Insert)
				{
					foreach (BillingData data in billedData)
					{
						foreach (PMTran orig in data.Transactions)
						{
							orig.ProformaRefNbr = ((PMProforma)e.Row).RefNbr;
						}
					}

					if (billingRecord != null)
						billingRecord.ProformaRefNbr = ((PMProforma)e.Row).RefNbr;
				}
			}

			public virtual void OnInvoicePersisted(PXCache sender, PXRowPersistedEventArgs e)
			{
				if (e.TranStatus == PXTranStatus.Open && e.Operation == PXDBOperation.Insert)
				{
					foreach (BillingData data in billedData)
					{
						foreach (PMTran orig in data.Transactions)
						{
							orig.ARTranType = ((ARInvoice)e.Row).DocType;
							orig.ARRefNbr = ((ARInvoice)e.Row).RefNbr;
						}
					}

					if (billingRecord != null)
					{
						billingRecord.ARDocType = ((ARInvoice)e.Row).DocType;
						billingRecord.ARRefNbr = ((ARInvoice)e.Row).RefNbr;
					}
				}
			}
		}
	}

	
}
