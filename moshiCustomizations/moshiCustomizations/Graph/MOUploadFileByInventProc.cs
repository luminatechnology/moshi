using PX.Data;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace moshiCustomizations.Graph
{
    public class MOUploadFileByInventProc : PXGraph<MOUploadFileByInventProc>
    {
        public PXCancel<InventoryItem> Cancel;
        public PXProcessingJoin<InventoryItem, LeftJoin<NoteDoc, On<NoteDoc.noteID, Equal<InventoryItem.noteID>>>,
                                               Where<NotExists<Select<UploadFile, Where<UploadFile.fileID, Equal<NoteDoc.fileID>>>>>> InventoryProc;


        public const string Uplaod = "Upload";
        public const string UplAll = "Upload All";
        public const string FTPPath = "FTP://ftp.bpscm.com.tw/DownloadBackup/";
        public const string FTPAcct = "27353977p";
        public const string FTPPwrd = "b541273$P";

        public MOUploadFileByInventProc()
        {
            InventoryProc.SetProcessCaption("Upload");
            InventoryProc.SetProcessAllCaption("Upload All");
            InventoryProc.SetProcessDelegate((PXProcessingBase<InventoryItem>.ProcessListDelegate)(inventories => MOUploadFileByInventProc.UploadFiles(inventories)));
        }

        public static void UploadFiles(List<InventoryItem> inventories)
        {
            List<string> dirtys = new List<string>();
            List<string> merges = new List<string>();

            MOUploadFileByInventProc.ReadListFiles(inventories.Select<InventoryItem, string>((Func<InventoryItem, string>)(item => item.InventoryCD.Trim())).ToArray<string>(), ref dirtys, ref merges);

            try
            {
                UploadFileMaintenance instance1 = PXGraph.CreateInstance<UploadFileMaintenance>();
                for (int index1 = 0; index1 < inventories.Count; ++index1)
                {
                    for (int index2 = 0; index2 < merges.Count; ++index2)
                    {
                        string str = merges[index2];
                        if (str.StartsWith(inventories[index1].InventoryCD.Trim()))
                        {
                            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create("FTP://ftp.bpscm.com.tw/DownloadBackup/" + str);
                            ftpWebRequest.Method = "RETR";
                            ftpWebRequest.Credentials = (ICredentials)new NetworkCredential("27353977p".Normalize(), "b541273$P".Normalize());

                            using (Stream responseStream = ftpWebRequest.GetResponse().GetResponseStream())
                            {
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    responseStream.CopyTo((Stream)memoryStream);

                                    byte[] array = memoryStream.ToArray();

                                    PX.SM.FileInfo finfo = new PX.SM.FileInfo(string.Format("Stock Items ({0}){1}{2}", inventories[index1].InventoryCD, "\\", str), null, array);

                                    instance1.Files.Current.PrimaryScreenID = "IN202500";

                                    if (instance1.SaveFile(finfo))
                                    {
                                        Guid? guid = finfo.UID;
                                        if (guid.HasValue)
                                        {
                                            InventoryItemMaint instance2 = PXGraph.CreateInstance<InventoryItemMaint>();
                                            instance2.Item.Current = (InventoryItem)instance2.Item.Search<InventoryItem.inventoryID>((object)inventories[index1].InventoryID);
                                            instance2.Item.Cache.SetValue<InventoryItem.imageUrl>((object)instance2.Item.Current, (object)instance1.Files.Current.Name);
                                            PXCache cache = instance2.Item.Cache;
                                            InventoryItem current = instance2.Item.Current;
                                            Guid[] guidArray = new Guid[1];
                                            guid = finfo.UID;
                                            guidArray[0] = guid.Value;
                                            PXNoteAttribute.SetFileNotes(cache, (object)current, guidArray);
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
                PXProcessing<InventoryItem>.SetError(((FtpWebResponse)ex.Response).StatusDescription);
            }
            catch (Exception ex)
            {
                PXProcessing<InventoryItem>.SetError(ex.Message);
            }
        }

        public static void ReadListFiles(string[] strings, ref List<string> dirtys, ref List<string> merges)
        {
            StreamReader streamReader = (StreamReader)null;
            try
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create("FTP://ftp.bpscm.com.tw/DownloadBackup/");
                    ftpWebRequest.Method = "NLST";
                    ftpWebRequest.Credentials = (ICredentials)new NetworkCredential("27353977p".Normalize(), "b541273$P".Normalize());
                    streamReader = new StreamReader(ftpWebRequest.GetResponse().GetResponseStream());
                    dirtys = ((IEnumerable<string>)streamReader.ReadToEnd().Split(new string[1]
                    {
            "\r\n"
                    }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
                    dirtys = dirtys.FindAll((Predicate<string>)(x => x.StartsWith(strings[i])));
                    for (int index = 0; index < dirtys.Count; ++index)
                        merges.Add(dirtys[index]);
                }
            }
            catch (WebException ex)
            {
                PXProcessing<InventoryItem>.SetError(ex.Message);
                throw;
            }
            finally
            {
                streamReader.Dispose();
            }
        }
    }
}
