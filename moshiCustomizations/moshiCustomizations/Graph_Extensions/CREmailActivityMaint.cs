using System;
using System.Collections;
using PX.SM;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.SO;

namespace PX.Objects.CR
{
    public class CREmailActivityMaint_Extension : PXGraphExtension<CREmailActivityMaint>
    {
        #region Delegate Action
        public delegate IEnumerable sendDelegate(PXAdapter adapter);
        [PXOverride]
        public IEnumerable send(PXAdapter adapter, sendDelegate baseMethod)
        {
            baseMethod(adapter);

            var row = Base.CurrentMessage.Current;

            // Only commercial invoice (sales) report needs to run update script.
            if (row != null && row.Subject.Contains("Moshi Commercial") && IsCommInvSaleRpt(row.NoteID) == true)
            {
                PXUpdate<Set<SOShipmentExt.usrSentCustomer, Required<SOShipmentExt.usrSentCustomer>>,
                         SOShipment,
                         Where<SOShipment.noteID, Equal<Required<SOShipment.noteID>>>>.Update(Base, true, row.RefNoteID);
            }

            return adapter.Get();
        }
        #endregion

        #region Method
        /// <summary>
        /// Use activity NoteID to search the primary screen for uploading file from specific report screen ID.
        /// </summary>
        /// <param name="noteID"></param>
        /// <returns></returns>
        protected bool IsCommInvSaleRpt(Guid? noteID)
        {
            return SelectFrom<UploadFile>.InnerJoin<NoteDoc>.On<NoteDoc.fileID.IsEqual<UploadFile.fileID>>
                                         .Where<NoteDoc.noteID.IsEqual<@P.AsGuid>
                                                .And<UploadFile.primaryScreenID.IsEqual<@P.AsString>>>.View
                                         .SelectSingleBound(Base, null, noteID, SOShipmentEntry_Extension.CommInvRpt2).Count > 0;
        }
        #endregion
    }
}   