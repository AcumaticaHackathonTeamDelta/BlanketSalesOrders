using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.Common.Exceptions;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CT;

namespace PX.Objects.EP
{
	[TableDashboardType]
	public class EPDocumentRelease : PXGraph<EPDocumentRelease>
	{
		public PXCancel<EPExpenseClaim> Cancel;

		[PXFilterable]
		public PXProcessing<
			EPExpenseClaim, 
			Where<
				EPExpenseClaim.released, Equal<False>, 
				And<EPExpenseClaim.approved, Equal<True>>>> 
			EPDocumentList;

		public APSetupNoMigrationMode APSetup;

		public EPDocumentRelease()
		{
			APSetup accountsPayableSetup = APSetup.Current;

			EPDocumentList.SetProcessDelegate(ReleaseDoc);
			EPDocumentList.SetProcessCaption(Messages.Release);
			EPDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
			EPDocumentList.SetSelected<EPExpenseClaim.selected>();
		}

		public static void ReleaseDoc(EPExpenseClaim claim)
		{
			#region prepare required variables
			APInvoiceEntry docgraph = PXGraph.CreateInstance<APInvoiceEntry>();
			ExpenseClaimEntry expenseClaimGraph = PXGraph.CreateInstance<ExpenseClaimEntry>();


			var receipts = PXSelect<EPExpenseClaimDetails,
						Where<EPExpenseClaimDetails.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(expenseClaimGraph, claim.RefNbr).Select(
						result => (EPExpenseClaimDetails)result).GroupBy(
						item => Tuple.Create(
							item.TaxZoneID,
							item.TaxCalcMode
						)).ToDictionary(x => x.Key, group => group.ToList());
			if (receipts.Count() == 0)
			{
				receipts.Add(Tuple.Create(claim.TaxZoneID, claim.TaxCalcMode), new List<EPExpenseClaimDetails>());
			}
			EPSetup epsetup = PXSelectReadonly<EPSetup>.Select(docgraph);
			APSetup apsetup = PXSelectReadonly<APSetup>.Select(docgraph);
			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Required<EPExpenseClaim.employeeID>>>>.Select(docgraph, claim.EmployeeID);
			Location emplocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<EPExpenseClaim.employeeID>>, And<Location.locationID, Equal<Required<EPExpenseClaim.locationID>>>>>.Select(docgraph, claim.EmployeeID, claim.LocationID);

			List<APRegister> doclist = new List<APRegister>();
			expenseClaimGraph.SelectTimeStamp();
			#endregion

			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (var res in receipts)
				{
					docgraph.Clear(PXClearOption.ClearAll);
					docgraph.SelectTimeStamp();
					docgraph.vendor.Current = employee;
					docgraph.location.Current = emplocation;

					CurrencyInfo infoOriginal = PXSelect<CurrencyInfo,
						Where<CurrencyInfo.curyInfoID, Equal<Required<EPExpenseClaim.curyInfoID>>>>.Select(docgraph, claim.CuryInfoID);
					CurrencyInfo info = PXCache<CurrencyInfo>.CreateCopy(infoOriginal);

					info.CuryInfoID = null;
					info = docgraph.currencyinfo.Insert(info);
					#region CreateInoiceHeader
					APInvoice invoice = new APInvoice();


					bool reversedDocument = false;
					if (res.Value.Sum(_ => _.ClaimCuryTranAmtWithTaxes) < 0)
					{
						invoice.DocType = APInvoiceType.DebitAdj;
						reversedDocument = true;
					}
					else
					{
						invoice.DocType = APInvoiceType.Invoice;
					}
                    decimal signOperation;
                    if (reversedDocument)
                    {
                        signOperation = -1;
                    }
                    else
                    {
                        signOperation = 1;
                    }

                    invoice.CuryInfoID = info.CuryInfoID;

					invoice.Hold = true;
					invoice.Released = false;
					invoice.Printed = false;
					invoice.OpenDoc = true;

					invoice.DocDate = claim.DocDate;
					invoice.FinPeriodID = claim.FinPeriodID;
					invoice.InvoiceNbr = claim.RefNbr;
					invoice.DocDesc = claim.DocDesc;
					invoice.VendorID = claim.EmployeeID;
					invoice.CuryID = info.CuryID;
					invoice.VendorLocationID = claim.LocationID;
					invoice.APAccountID = emplocation != null ? emplocation.APAccountID : null;
					invoice.APSubID = emplocation != null ? emplocation.APSubID : null;
					invoice.TaxZoneID = res.Key.Item1;
					invoice.TaxCalcMode = res.Key.Item2;
					invoice.BranchID = claim.BranchID;
					invoice.OrigModule = BatchModule.EP;
					invoice.OrigRefNbr = claim.RefNbr;

					invoice = docgraph.Document.Insert(invoice);

					PXCache<CurrencyInfo>.RestoreCopy(info, infoOriginal);
					info.CuryInfoID = invoice.CuryInfoID;

					PXCache claimcache = docgraph.Caches[typeof(EPExpenseClaim)];
					PXCache claimdetailcache = docgraph.Caches[typeof(EPExpenseClaimDetails)];

					PXNoteAttribute.CopyNoteAndFiles(claimcache, claim, docgraph.Document.Cache, invoice, epsetup.GetCopyNoteSettings<PXModule.ap>());
					#endregion
					TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
					decimal? claimCuryTaxRoundDiff = 0m;
					decimal? claimTaxRoundDiff = 0m;
                    foreach (EPExpenseClaimDetails claimdetail in res.Value)
					{
						#region AddDetails

						decimal tipQty;
						if (reversedDocument == claimdetail.ClaimCuryTranAmtWithTaxes < 0)
						{
							tipQty = 1;
						}
						else
						{
							tipQty = -1;
						}
						Contract contract = PXSelect<Contract, Where<Contract.contractID, Equal<Required<EPExpenseClaimDetails.contractID>>>>.SelectSingleBound(docgraph, null, claimdetail.ContractID);

						if (claimdetail.TaskID != null)
						{
							PMTask task = PXSelect<PMTask, Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>.Select(expenseClaimGraph, claimdetail.TaskID);
							if (task != null && !(bool)task.VisibleInAP)
								throw new PXException(PM.Messages.TaskInvisibleInModule, task.TaskCD, BatchModule.AP);
						}

						APTran tran = new APTran();
						tran.InventoryID = claimdetail.InventoryID;
						tran.TranDesc = claimdetail.TranDesc;
						decimal unitCost;
						decimal amount;
						decimal taxableAmt;
						decimal taxAmt;
						if (EPClaimReceiptController.IsSameCury(expenseClaimGraph, claimdetail.CuryInfoID, claimdetail.ClaimCuryInfoID))
						{
							unitCost = claimdetail.CuryUnitCost ?? 0m;
							amount = claimdetail.CuryTaxableAmt ?? 0m;
							taxableAmt = claimdetail.CuryTaxableAmtFromTax ?? 0m;
							taxAmt = claimdetail.CuryTaxAmt ?? 0m;
						}
						else
						{

							if (claimdetail.CuryUnitCost == null || claimdetail.CuryUnitCost == 0m)
							{
								unitCost = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.UnitCost, out unitCost);
							}
							if (claimdetail.CuryTaxableAmt == null || claimdetail.CuryTaxableAmt == 0m)
							{
								amount = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxableAmt, out amount);
							}
							if (claimdetail.CuryTaxableAmtFromTax == null || claimdetail.CuryTaxableAmtFromTax == 0m)
							{
								taxableAmt = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxableAmtFromTax, out taxableAmt);
							}
							if (claimdetail.CuryTaxAmt == null || claimdetail.CuryTaxAmt == 0m)
							{
								taxAmt = 0m;
							}
							else
							{
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TaxAmt, out taxAmt);
							}


						}

						tran.ManualPrice = true;
						tran.CuryUnitCost = unitCost;
						tran.Qty = claimdetail.Qty * signOperation;
						tran.UOM = claimdetail.UOM;
						tran.NonBillable = claimdetail.Billable != true;
						claimCuryTaxRoundDiff += (claimdetail.ClaimCuryTaxRoundDiff ?? 0m) * signOperation;
						claimTaxRoundDiff += (claimdetail.ClaimTaxRoundDiff ?? 0m) * signOperation;
						tran.Date = claimdetail.ExpenseDate;

						if (contract.BaseType == PMProject.ProjectBaseType.Project)
						{
							tran.ProjectID = claimdetail.ContractID;
						}
						else
						{
							tran.ProjectID = ProjectDefaultAttribute.NonProject();
						}

						tran.TaskID = claimdetail.TaskID;
						tran.CostCodeID = claimdetail.CostCodeID;
						tran.AccountID = claimdetail.ExpenseAccountID;
						tran.SubID = claimdetail.ExpenseSubID;
						tran.TaxCategoryID = claimdetail.TaxCategoryID;
						tran.BranchID = claimdetail.BranchID;
						tran = docgraph.Transactions.Insert(tran);
						tran.CuryLineAmt = amount * signOperation;
						tran.CuryTaxAmt = 0;
						tran.CuryTaxableAmt = taxableAmt * signOperation;
						tran.CuryTaxAmt = taxAmt * signOperation;
						tran = docgraph.Transactions.Update(tran);


						if ((claimdetail.CuryTipAmt ?? 0) != 0)
						{
							APTran tranTip = new APTran();
							if (epsetup.NonTaxableTipItem == null)
							{
								throw new PXException(Messages.TipItemIsNotDefined);
							}
							IN.InventoryItem tipItem = PXSelect<IN.InventoryItem,
								Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(docgraph, epsetup.NonTaxableTipItem);

							if (tipItem == null)
							{
								string fieldname = PXUIFieldAttribute.GetDisplayName<EPSetup.nonTaxableTipItem>(docgraph.Caches[typeof(EPSetup)]);
								throw new PXException(ErrorMessages.ValueDoesntExistOrNoRights, fieldname, epsetup.NonTaxableTipItem);
							}
							tranTip.InventoryID = epsetup.NonTaxableTipItem;
							tranTip.TranDesc = tipItem.Descr;
							if (EPClaimReceiptController.IsSameCury(expenseClaimGraph, claimdetail.CuryInfoID, claimdetail.ClaimCuryInfoID))
							{
								tranTip.CuryUnitCost = Math.Abs(claimdetail.CuryTipAmt ?? 0m);
								tranTip.CuryTranAmt = claimdetail.CuryTipAmt * signOperation;
							}
							else
							{
								decimal tipAmt;
								PXCurrencyAttribute.CuryConvCury<EPExpenseClaimDetails.claimCuryInfoID>(expenseClaimGraph.ExpenseClaimDetails.Cache, claimdetail, (decimal)claimdetail.TipAmt, out tipAmt);
								tranTip.CuryUnitCost = Math.Abs(tipAmt);
								tranTip.CuryTranAmt = tipAmt * signOperation;
							}
							tranTip.Qty = tipQty;
							tranTip.UOM = tipItem.BaseUnit;
							tranTip.NonBillable = claimdetail.Billable != true;
							tranTip.Date = claimdetail.ExpenseDate;

							tranTip.BranchID = claimdetail.BranchID;
							tranTip = docgraph.Transactions.Insert(tranTip);

							if (epsetup.UseReceiptAccountForTips == true)
							{
								tranTip.AccountID = claimdetail.ExpenseAccountID;
								tranTip.SubID = claimdetail.ExpenseSubID;
							}
							else
							{
								tranTip.AccountID = tipItem.COGSAcctID;
								Location companyloc = (Location)PXSelectJoin<Location,
																			InnerJoin<BAccountR, On<Location.bAccountID, Equal<BAccountR.bAccountID>,
																								And<Location.locationID, Equal<BAccountR.defLocationID>>>,
																			InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
																Where<GL.Branch.branchID, Equal<Current<APInvoice.branchID>>>>.Select(docgraph);
								PMTask task = PXSelect<PMTask,
														Where<PMTask.projectID, Equal<Required<PMTask.projectID>>,
														And<PMTask.taskID, Equal<Required<PMTask.taskID>>>>>.Select(docgraph, claimdetail.ContractID, claimdetail.TaskID);

								Location customerLocation = (Location)PXSelectorAttribute.Select<EPExpenseClaimDetails.customerLocationID>(claimdetailcache, claimdetail);

								int? employee_SubID = (int?)docgraph.Caches[typeof(EPEmployee)].GetValue<EPEmployee.expenseSubID>(employee);
								int? item_SubID = (int?)docgraph.Caches[typeof(IN.InventoryItem)].GetValue<IN.InventoryItem.cOGSSubID>(tipItem);
								int? company_SubID = (int?)docgraph.Caches[typeof(Location)].GetValue<Location.cMPExpenseSubID>(companyloc);
								int? project_SubID = (int?)docgraph.Caches[typeof(Contract)].GetValue<Contract.defaultSubID>(contract);
								int? task_SubID = (int?)docgraph.Caches[typeof(PMTask)].GetValue<PMTask.defaultSubID>(task);
								int? location_SubID = (int?)docgraph.Caches[typeof(Location)].GetValue<Location.cSalesSubID>(customerLocation);

								object value = SubAccountMaskAttribute.MakeSub<EPSetup.expenseSubMask>(docgraph, epsetup.ExpenseSubMask,
									new object[] { employee_SubID, item_SubID, company_SubID, project_SubID, task_SubID, location_SubID },
									new Type[] { typeof(EPEmployee.expenseSubID), typeof(IN.InventoryItem.cOGSSubID), typeof(Location.cMPExpenseSubID), typeof(Contract.defaultSubID), typeof(PMTask.defaultSubID), typeof(Location.cSalesSubID) });

								docgraph.Caches[typeof(APTran)].RaiseFieldUpdating<APTran.subID>(tranTip, ref value);
								tranTip.SubID = (int?)value;
							}
							tranTip = docgraph.Transactions.Update(tranTip);
							tranTip.TaxCategoryID = tipItem.TaxCategoryID;
							tranTip.ProjectID = tran.ProjectID;
							tranTip.TaskID = tran.TaskID;
							tranTip = AddTaxes(docgraph, invoice, signOperation, claimdetail, tranTip, true);
							tranTip = docgraph.Transactions.Update(tranTip);
						}

						PXNoteAttribute.CopyNoteAndFiles(claimdetailcache, claimdetail, docgraph.Transactions.Cache, tran, epsetup.GetCopyNoteSettings<PXModule.ap>());
						claimdetail.Released = true;
						expenseClaimGraph.ExpenseClaimDetails.Update(claimdetail);
						#endregion


						tran = AddTaxes(docgraph, invoice, signOperation, claimdetail, tran, false);

					}

					#region legacy taxes
					foreach (EPTaxAggregate tax in PXSelectReadonly<EPTaxAggregate,
                        Where<EPTaxAggregate.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.Select(docgraph, claim.RefNbr))
                    {
                        #region Add taxes
                        APTaxTran new_aptax = docgraph.Taxes.Search<APTaxTran.taxID>(tax.TaxID);

                        if (new_aptax == null)
                        {
                            new_aptax = new APTaxTran();
                            new_aptax.TaxID = tax.TaxID;
                            new_aptax = docgraph.Taxes.Insert(new_aptax);
                            if (new_aptax != null)
                            {
                                new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
                                new_aptax.CuryTaxableAmt = 0m;
                                new_aptax.CuryTaxAmt = 0m;
                                new_aptax.CuryExpenseAmt = 0m;
                                new_aptax = docgraph.Taxes.Update(new_aptax);
                            }
                        }

                        if (new_aptax != null)
                        {
                            new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
                            new_aptax.TaxRate = tax.TaxRate;
                            new_aptax.CuryTaxableAmt = (new_aptax.CuryTaxableAmt ?? 0m) + tax.CuryTaxableAmt * signOperation;
                            new_aptax.CuryTaxAmt = (new_aptax.CuryTaxAmt ?? 0m) + tax.CuryTaxAmt * signOperation;
                            new_aptax.CuryExpenseAmt = (new_aptax.CuryExpenseAmt ?? 0m) + tax.CuryExpenseAmt * signOperation;
                            new_aptax = docgraph.Taxes.Update(new_aptax);
                        }
                        #endregion
                    }
                    #endregion

                    invoice.CuryOrigDocAmt = invoice.CuryDocBal;
					invoice.CuryTaxAmt = invoice.CuryTaxTotal;
					invoice.Hold = false;
					docgraph.Document.Update(invoice);
					invoice.CuryTaxRoundDiff = invoice.CuryRoundDiff = invoice.CuryRoundDiff = claimCuryTaxRoundDiff;
					invoice.TaxRoundDiff = invoice.RoundDiff = claimTaxRoundDiff;
                    bool inclusive = PXSelectJoin<APTaxTran, InnerJoin<Tax,
                              On<APTaxTran.taxID, Equal<Tax.taxID>>>,
                              Where<APTaxTran.refNbr, Equal<Required<APInvoice.refNbr>>,
                              And<APTaxTran.tranType, Equal<Required<APInvoice.docType>>,
                              And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>>>>>
                              .Select(docgraph, invoice.RefNbr, invoice.DocType).Count > 0;
                    if ((invoice.TaxCalcMode == TaxCalculationMode.Gross
                        && PXSelectJoin<APTaxTran, InnerJoin<Tax,
                              On<APTaxTran.taxID, Equal<Tax.taxID>>>,
                              Where<APTaxTran.refNbr, Equal<Required<APInvoice.refNbr>>,
                              And<APTaxTran.tranType, Equal<Required<APInvoice.docType>>,
                              And<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>>>>>
                              .Select(docgraph, invoice.RefNbr, invoice.DocType).Count > 0)
                              || inclusive)
                    {

                        decimal curyAdditionalDiff = -(invoice.CuryTaxRoundDiff ?? 0m) + (invoice.CuryTaxAmt ?? 0m) - (invoice.CuryDocBal ?? 0m);
                        decimal additionalDiff = -(invoice.TaxRoundDiff ?? 0m) + (invoice.TaxAmt ?? 0m) - (invoice.DocBal ?? 0m);
                        foreach (APTran line in docgraph.Transactions.Select())
                        {
                            curyAdditionalDiff += (line.CuryTaxableAmt ?? 0m) == 0m ? (line.CuryTranAmt ?? 0m) : (line.CuryTaxableAmt ?? 0m);
                            additionalDiff += (line.TaxableAmt ?? 0m) == 0m ? (line.TranAmt ?? 0m) : (line.TaxableAmt ?? 0m);
                        }

                        invoice.CuryTaxRoundDiff += curyAdditionalDiff;
                        invoice.TaxRoundDiff += additionalDiff;

                    }

					docgraph.Document.Update(invoice);
					docgraph.Save.Press();
					foreach (EPExpenseClaimDetails claimdetail in res.Value)
					{
						claimdetail.APDocType = invoice.DocType;
						claimdetail.APRefNbr = invoice.RefNbr;
						expenseClaimGraph.ExpenseClaimDetails.Update(claimdetail);
					}
					claim.Status = EPExpenseClaimStatus.ReleasedStatus;
					claim.Released = true;
					expenseClaimGraph.ExpenseClaim.Update(claim);

					#region EP History Update
					EPHistory hist = new EPHistory();
					hist.EmployeeID = invoice.VendorID;
					hist.FinPeriodID = invoice.FinPeriodID;
					hist = (EPHistory)expenseClaimGraph.Caches[typeof(EPHistory)].Insert(hist);

					hist.FinPtdClaimed += invoice.DocBal;
					hist.FinYtdClaimed += invoice.DocBal;
					if (invoice.FinPeriodID == invoice.TranPeriodID)
					{
						hist.TranPtdClaimed += invoice.DocBal;
						hist.TranYtdClaimed += invoice.DocBal;
					}
					else
					{
						EPHistory tranhist = new EPHistory();
						tranhist.EmployeeID = invoice.VendorID;
						tranhist.FinPeriodID = invoice.TranPeriodID;
						tranhist = (EPHistory)expenseClaimGraph.Caches[typeof(EPHistory)].Insert(tranhist);
						tranhist.TranPtdClaimed += invoice.DocBal;
						tranhist.TranYtdClaimed += invoice.DocBal;
					}
					expenseClaimGraph.Views.Caches.Add(typeof(EPHistory));
					#endregion

					expenseClaimGraph.Save.Press();

					doclist.Add(docgraph.Document.Current);
				}

				ts.Complete();
			}

			if ((bool)epsetup.AutomaticReleaseAP == true)
			{
				APDocumentRelease.ReleaseDoc(doclist, false);
			}
		}

		private static APTran AddTaxes(APInvoiceEntry docgraph, APInvoice invoice, decimal signOperation, EPExpenseClaimDetails claimdetail, APTran tran, bool isTipTran)
		{
			var cmd = new PXSelect<EPTaxTran,
				Where<EPTaxTran.claimDetailID, Equal<Required<EPTaxTran.claimDetailID>>>>(docgraph);
			if(isTipTran)
			{
				cmd.WhereAnd<Where<EPTaxTran.isTipTax, Equal<True>>>();
			}
			else
			{
				cmd.WhereAnd<Where<EPTaxTran.isTipTax, Equal<False>>>();
			}
			foreach (EPTaxTran tax in cmd.Select(claimdetail.ClaimDetailID))
			{
				#region Add taxes
				APTaxTran new_aptax = docgraph.Taxes.Search<APTaxTran.taxID>(tax.TaxID);

				if (new_aptax == null)
				{
					new_aptax = new APTaxTran();
					new_aptax.TaxID = tax.TaxID;
					TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);
					new_aptax = docgraph.Taxes.Insert(new_aptax);
					if (new_aptax != null)
					{
						new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
						new_aptax.CuryTaxableAmt = 0m;
						new_aptax.CuryTaxAmt = 0m;
						new_aptax.CuryExpenseAmt = 0m;
						new_aptax = docgraph.Taxes.Update(new_aptax);
					}
				}

				if (new_aptax != null)
				{
					new_aptax = PXCache<APTaxTran>.CreateCopy(new_aptax);
					new_aptax.TaxRate = tax.TaxRate;
					new_aptax.CuryTaxableAmt = (new_aptax.CuryTaxableAmt ?? 0m) + tax.ClaimCuryTaxableAmt * signOperation;
					new_aptax.CuryTaxAmt = (new_aptax.CuryTaxAmt ?? 0m) + tax.ClaimCuryTaxAmt * signOperation;
					new_aptax.CuryExpenseAmt = (new_aptax.CuryExpenseAmt ?? 0m) + tax.ClaimCuryExpenseAmt * signOperation;
					TaxAttribute.SetTaxCalc<APTran.taxCategoryID, APTaxAttribute>(docgraph.Transactions.Cache, null, TaxCalc.ManualCalc);
					new_aptax = docgraph.Taxes.Update(new_aptax);
					//On first inserting APTaxTran APTax line will be created automatically. 
					//However, new APTax will not be inserted on APTaxTran line update, even if we already have more lines.
					//So, we have to do it manually.
					APTax aptax = docgraph.Tax_Rows.Search<APTax.lineNbr, APTax.taxID>(tran.LineNbr, new_aptax.TaxID);
					if (aptax == null)
					{
						aptax = docgraph.Tax_Rows.Insert(new APTax()
						{
							LineNbr = tran.LineNbr,
							TaxID = new_aptax.TaxID,
							TaxRate = tax.TaxRate,
							CuryTaxableAmt = tax.ClaimCuryTaxableAmt * signOperation,
							CuryTaxAmt = tax.ClaimCuryTaxAmt * signOperation,
							CuryExpenseAmt = tax.ClaimCuryExpenseAmt * signOperation
						});
					}
					Tax taxRow = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(docgraph, new_aptax.TaxID);
					if ((taxRow.TaxCalcLevel == CSTaxCalcLevel.Inclusive ||
						(invoice.TaxCalcMode == TaxCalculationMode.Gross && taxRow.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt))
						&& (tran.CuryTaxableAmt == null || tran.CuryTaxableAmt == 0m))
					{
						tran.CuryTaxableAmt = tax.ClaimCuryTaxableAmt * signOperation;
						tran.CuryTaxAmt = tax.ClaimCuryTaxAmt * signOperation;
						tran = docgraph.Transactions.Update(tran);
					}
				}
				#endregion
			}

			return tran;
		}
	}
}