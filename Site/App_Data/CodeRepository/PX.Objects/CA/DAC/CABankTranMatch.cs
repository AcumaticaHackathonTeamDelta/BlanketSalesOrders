﻿using System;
using PX.Data;
using PX.Objects.CA.BankStatementProtoHelpers;

namespace PX.Objects.CA
{
	[Serializable]
    [PXCacheName(Messages.BankTranMatch)]
	public partial class CABankTranMatch : IBqlTable
	{
		#region TranID
		public abstract class tranID : IBqlField
		{
		}

		[PXDBInt(IsKey = true)]
		public virtual int? TranID
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : IBqlField
		{
		}

		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[CABankTranType.List]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region CATranID
		public abstract class cATranID : IBqlField
		{
		}

		[PXDBLong]
		public virtual long? CATranID
		{
			get;
			set;
		}
		#endregion
		#region DocModule
		public abstract class docModule : IBqlField
		{
		}

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR }, new string[] { GL.Messages.ModuleAP, GL.Messages.ModuleAR })]
		public virtual string DocModule
		{
			get;
			set;
		}
		#endregion
		#region DocType
		public abstract class docType : IBqlField
		{
		}

		[PXDBString(3, IsFixed = true, InputMask = "")]
		public virtual string DocType
		{
			get;
			set;
		}
		#endregion
		#region DocRefNbr
		public abstract class docRefNbr : IBqlField
		{
		}

		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		public virtual string DocRefNbr
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : IBqlField
		{
		}

		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region CuryAmt
		public abstract class curyAmt : IBqlField
		{
		}

		[PXDBDecimal]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryAmt
		{
			get;
			set;
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
		public static void Redirect(PXGraph graph, CABankTranMatch match)
		{
			if (match.DocModule == GL.BatchModule.AP && match.DocType == CATranType.CABatch && match.DocRefNbr != null)
			{
				CABatchEntry docGraph = PXGraph.CreateInstance<CABatchEntry>();
				docGraph.Clear();
				docGraph.Document.Current = PXSelect<CABatch, Where<CABatch.batchNbr, Equal<Required<CATran.origRefNbr>>>>.Select(docGraph, match.DocRefNbr);
				throw new PXRedirectRequiredException(docGraph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			else if (match.CATranID != null)
			{
				CATran catran = PXSelect<CATran, Where<CATran.tranID, Equal<Required<CABankTranMatch.cATranID>>>>.Select(graph, match.CATranID);
				CATran.Redirect(null, catran);
			}
			else if (match.DocModule != null && match.DocType != null && match.DocRefNbr != null)
			{
				if (match.DocModule == GL.BatchModule.AP)
				{
					AP.APInvoiceEntry docGraph = PXGraph.CreateInstance<AP.APInvoiceEntry>();
					docGraph.Clear();
					AP.APInvoice invoice = PXSelect<AP.APInvoice, Where<AP.APInvoice.refNbr, Equal<Required<CABankTranMatch.docRefNbr>>, And<AP.APInvoice.docType, Equal<Required<CABankTranMatch.docType>>>>>.Select(docGraph, match.DocRefNbr, match.DocType);
					docGraph.Document.Current = invoice;
					throw new PXRedirectRequiredException(docGraph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
				else if (match.DocModule == GL.BatchModule.AR)
				{
					AR.ARInvoiceEntry docGraph = PXGraph.CreateInstance<AR.ARInvoiceEntry>();
					docGraph.Clear();
					AR.ARInvoice invoice = PXSelect<AR.ARInvoice, Where<AR.ARInvoice.refNbr, Equal<Required<CABankTranMatch.docRefNbr>>, And<AR.ARInvoice.docType, Equal<Required<CABankTranMatch.docType>>>>>.Select(docGraph, match.DocRefNbr, match.DocType);
					docGraph.Document.Current = invoice;
					throw new PXRedirectRequiredException(docGraph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
		}

		public void Copy(CABankTranDocRef docRef)
		{
			CATranID = docRef.CATranID;
			DocModule = docRef.DocModule;
			DocType = docRef.DocType;
			DocRefNbr = docRef.DocRefNbr;
			ReferenceID = docRef.ReferenceID;
		}
	}

	public partial class CABankTranMatch2 : CABankTranMatch
	{
		#region TranID
		public new abstract class tranID : IBqlField
		{
		}
		#endregion
		#region TranType
		public new abstract class tranType : IBqlField
		{
		}
		#endregion
		#region CATranID
		public new abstract class cATranID : IBqlField
		{
		}
		#endregion
		#region DocModule
		public new abstract class docModule : IBqlField
		{
		}
		#endregion
		#region DocType
		public new abstract class docType : IBqlField
		{
		}
		#endregion
		#region DocRefNbr
		public new abstract class docRefNbr : IBqlField
		{
		}
		#endregion
	}
}