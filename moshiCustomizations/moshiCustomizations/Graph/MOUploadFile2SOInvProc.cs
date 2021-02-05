using PX.Data;
using PX.Objects.SO;
using PX.SM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace moshiCustomizations.Graph
{
    public class MOUploadFile2SOInvProc : PXGraph<MOUploadFile2SOInvProc>
    {
        public PXCancel<SOInvoice> Cancel;
        public PXProcessingJoin<SOInvoice, LeftJoin<NoteDoc, On<NoteDoc.noteID, Equal<SOInvoice.noteID>>>, 
                                           Where<NotExists<Select<UploadFile, Where<UploadFile.fileID, Equal<NoteDoc.fileID>>>>>> InvoiceProc;

        public MOUploadFile2SOInvProc()
        {
            InvoiceProc.SetProcessCaption("Upload");
            InvoiceProc.SetProcessAllCaption("Upload All");
            InvoiceProc.SetProcessDelegate((PXProcessingBase<SOInvoice>.ProcessListDelegate) (invoices => MOUploadFile2SOInvProc.UploadFiles(invoices)));
        }

        public static void UploadFiles(List<SOInvoice> invoices)
        {
            List<string> dirtys = new List<string>();
            List<string> merges = new List<string>();

            MOUploadFileByInventProc.ReadListFiles(invoices.Select<PX.Objects.SO.SOInvoice, string>((Func<PX.Objects.SO.SOInvoice, string>) (invoice => invoice.RefNbr.Trim())).ToArray<string>(), ref dirtys, ref merges);
            try
            {
                UploadFileMaintenance instance1 = PXGraph.CreateInstance<UploadFileMaintenance>();

                for (int index1 = 0; index1 < invoices.Count; ++index1)
                {
                    for (int index2 = 0; index2 < merges.Count; ++index2)
                    {
                        string name = merges[index2];

                        if (name.StartsWith(invoices[index1].RefNbr.Trim()))
                        {
                            FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create("FTP://ftp.bpscm.com.tw/DownloadBackup/" + name);
                            ftpWebRequest.Method = "RETR";
                            ftpWebRequest.Credentials = (ICredentials) new NetworkCredential("27353977p".Normalize(), "b541273$P".Normalize());
                            using (Stream responseStream = ftpWebRequest.GetResponse().GetResponseStream())
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    responseStream.CopyTo((Stream) memoryStream);

                                    byte[] array = memoryStream.ToArray();

                                    PX.SM.FileInfo finfo = new PX.SM.FileInfo(name, (string) null, array);

                                    if (instance1.SaveFile(finfo))
                                    {
                                        Guid? guid = finfo.UID;
                                        if (guid.HasValue)
                                        {
                                            SOInvoiceEntry instance2 = PXGraph.CreateInstance<SOInvoiceEntry>();

                                            instance2.Document.Current = (PX.Objects.AR.ARInvoice) instance2.Document.Search<PX.Objects.AR.ARInvoice.refNbr>((object) invoices[index1].RefNbr, (object) invoices[index1].DocType);
                                            PXCache cache = instance2.Document.Cache;
                                            PX.Objects.AR.ARInvoice current = instance2.Document.Current;
                                            Guid[] guidArray = new Guid[1];
                                            guid = finfo.UID;
                                            guidArray[0] = guid.Value;
                                            PXNoteAttribute.SetFileNotes(cache, (object) current, guidArray);
                                            instance2.Save.Press();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    }
            }
            catch (WebException ex)
            {
                PXProcessing<SOInvoice>.SetError(((FtpWebResponse) ex.Response).StatusDescription);
            }
            catch (Exception ex)
            {
                PXProcessing<SOInvoice>.SetError(ex.Message);
            }
        }
    }
}
