using System;
using System.Diagnostics;

namespace x937
{
    public static class Translator
    {
        public static IX9Record Translate(X9Rec item)
        {
            IX9Record ret;
            var dataLength = 80;
            switch (item.RecType)
            {
                case "01": ret = new R01(); break;
                case "10": ret = new R10(); break;
                case "20": ret = new R20(); break;
                case "25": ret = new R25(); break;
                default: ret = new Unknown(); break;
            }

            var recLen = item.RecData.Length;
            if (!(ret is Unknown) && dataLength != recLen) throw new Exception($"Record defined as {item.RecType} requires a length of {dataLength} but was {recLen}");
            ret.SetData(item.RecData);
            if (ret.RecordType != item.RecType) throw new Exception("I didn't get what I expected");
            return ret;
        }
    }

    public interface IX9Record
    {
        void SetData(string data);
        string RecordType { get; }
    }

    public abstract class X9Record: IX9Record
    {
        protected string Data;
        public virtual void SetData(string data)
        {
            // I only want SetData being called once
            // It's no big deal if it's called again
            // but the code isn't going to deal with that
            if (!string.IsNullOrWhiteSpace(Data)) return;
            Data = data;
        }
        public string RecordType => Data.Substring(0, 2);
    }

    public class Unknown : X9Record
    {
        // no properties, because we don't know what we have...
    }

    public class R01 : X9Record
    {
        public override void SetData(string data)
        {
            base.SetData(data);
            Debug.WriteLine("R01 SetData() called");
            StandardLevel = Data.Substring(2, 2);
            TestFileIndicator = Data.Substring(4, 1);
            ImmediateDestinationRoutingNumber = Data.Substring(5, 9);
            ImmediateOriginRoutingNumber = Data.Substring(14, 9);
            FileCreationDate = Data.Substring(23, 8);
            FileCreationTime = Data.Substring(31, 4);
            ResendIndicator = Data.Substring(35, 1);
            ImmediateDestinationName = Data.Substring(36, 18);
            ImmediateOriginName = Data.Substring(54, 18);
            FileIdModifier = Data.Substring(72, 1);
            CountryCode = Data.Substring(73, 2);
            UserField = Data.Substring(75, 4);
            Reserved = Data.Substring(79, 1);
        }

        public string StandardLevel { get; set; }
        public string TestFileIndicator { get; set; }
        public string ImmediateDestinationRoutingNumber { get; set; }
        public string ImmediateOriginRoutingNumber { get; set; }
        public string FileCreationDate { get; set; }
        public string FileCreationTime { get; set; }
        public string ResendIndicator { get; set; }
        public string ImmediateDestinationName { get; set; }
        public string ImmediateOriginName { get; set; }
        public string FileIdModifier { get; set; }
        public string CountryCode { get; set; }
        public string UserField { get; set; }
        public string Reserved { get; set; }
    }

    public class R10: X9Record
    {
        public override void SetData(string data)
        {
            base.SetData(data);
            Debug.WriteLine("R10 SetData() called");
            CollectionTypeIndicator = Data.Substring(2, 2);
            DestinationRoutingNumber = Data.Substring(4, 9);
            ECEInstitutionRoutingNumber = Data.Substring(13, 9);
            CashLetterBusinessDate = Data.Substring(22, 8);
            CashLetterCreationDate = Data.Substring(30, 8);
            CashLetterCreationTime = Data.Substring(38, 4);
            CashLetterRecordTypeIndicator = Data.Substring(42, 1);
            CashLetterDocumentationTypeIndicator = Data.Substring(43, 1);
            CashLetterId = Data.Substring(44, 8);
            OriginatorContactName = Data.Substring(52, 14);
            OriginatorContactPhoneNumber = Data.Substring(66, 10);
            FedWorkType = Data.Substring(76, 1);
            UserField = Data.Substring(77, 2);
            User = Data.Substring(79, 1);
        }

        public string CollectionTypeIndicator { get; set; }
        public string DestinationRoutingNumber { get; set; }
        public string ECEInstitutionRoutingNumber { get; set; }
        public string CashLetterBusinessDate { get; set; }
        public string CashLetterCreationDate { get; set; }
        public string CashLetterCreationTime { get; set; }
        public string CashLetterRecordTypeIndicator { get; set; }
        public string CashLetterDocumentationTypeIndicator { get; set; }
        public string CashLetterId { get; set; }
        public string OriginatorContactName { get; set; }
        public string OriginatorContactPhoneNumber { get; set; }
        public string FedWorkType { get; set; }
        public string UserField { get; set; }
        public string User { get; set; }
    }

    public class R20: X9Record
    {
        public override void SetData(string data)
        {
            base.SetData(data);
            Debug.WriteLine("R20 SetData() called");
            CollectionTypeIndicator = Data.Substring(2, 2);
            DestinationRoutingNumber = Data.Substring(4, 9);
            ECEInstitutionRoutingNumber = Data.Substring(13, 9);
            BatchBusinessDate = Data.Substring(22, 8);
            BatchCreationDate = Data.Substring(30, 8);
            BatchId = Data.Substring(38, 10);
            BatchSequenceNumber = Data.Substring(48, 4);
            CycleNumber = Data.Substring(52, 2);
            ReturnLocationRoutingNumber = Data.Substring(54, 9);
            UserField = Data.Substring(63, 5);
            Reserved = Data.Substring(68, 12);
        }

        public string CollectionTypeIndicator { get; set; }
        public string DestinationRoutingNumber { get; set; }
        public string ECEInstitutionRoutingNumber { get; set; }
        public string BatchBusinessDate { get; set; }
        public string BatchCreationDate { get; set; }
        public string BatchId { get; set; }
        public string BatchSequenceNumber { get; set; }
        public string CycleNumber { get; set; }
        public string ReturnLocationRoutingNumber { get; set; }
        public string UserField { get; set; }
        public string Reserved { get; set; }
    }

    public class R25: X9Record
    {
        public override void SetData(string data)
        {
            base.SetData(data);
            Debug.WriteLine("R25 SetData() called");
            AuxiliaryOnUs = Data.Substring(2, 15);
            ExternalProcessingCode = Data.Substring(17, 1);
            PayorBankRoutingNumber = Data.Substring(18, 8);
            PriorBankRoutingNumberCheckDigit = Data.Substring(26, 1);
            OnUs = Data.Substring(27, 20);
            ItemAmount = Data.Substring(47, 10);
            ECEInstitutionItemSequenceNumber = Data.Substring(57, 15);
            DocumentationTypeIndicator = Data.Substring(72, 1);
            ReturnAcceptanceIndicator = Data.Substring(73, 1);
            MICRValidIndicator = Data.Substring(74, 1);
            BOFDIndicator = Data.Substring(75, 1);
            CheckDetailRecordAddendumCount = Data.Substring(76, 2);
            CorrectionIndicator = Data.Substring(78, 1);
            ArchiveTypeIndicator = Data.Substring(79, 1);
        }

        public string AuxiliaryOnUs { get; set; }
        public string ExternalProcessingCode { get; set; }
        public string PayorBankRoutingNumber { get; set; }
        public string PriorBankRoutingNumberCheckDigit { get; set; }
        public string OnUs { get; set; }
        public string ItemAmount { get; set; }
        public string ECEInstitutionItemSequenceNumber { get; set; }
        public string DocumentationTypeIndicator { get; set; }
        public string ReturnAcceptanceIndicator { get; set; }
        public string MICRValidIndicator { get; set; }
        public string BOFDIndicator { get; set; }
        public string CheckDetailRecordAddendumCount { get; set; }
        public string CorrectionIndicator { get; set; }
        public string ArchiveTypeIndicator { get; set; }
    }
}