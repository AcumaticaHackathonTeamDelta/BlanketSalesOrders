﻿using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;

namespace PX.Objects.AR.BQL
{
	/// <summary>
	/// A predicate that returns <c>true</c> if and only if the document type
	/// represented by the generic type passed corresponds to one of the 
	/// Accounts Receivable self-voiding documents, which do not create a
	/// separate voiding document: instead, all their applications are reversed.
	/// </summary>
	public class IsSelfVoiding<TDocTypeField> : IBqlUnary
		where TDocTypeField : IBqlField
	{
		private static IBqlCreator Where => new Where<
			TDocTypeField, Equal<ARDocType.smallBalanceWO>,
			Or<TDocTypeField, Equal<ARDocType.smallCreditWO>>>();

		public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, BqlCommand.Selection selection)
			=> Where.Parse(graph, pars, tables, fields, sortColumns, text, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			=> Where.Verify(cache, item, pars, ref result, ref value);
	}
}
