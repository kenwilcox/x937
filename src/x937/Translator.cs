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
                case "26": ret = new R26(); break;
                case "50": ret = new R50(); break;
                case "52": dataLength = 117; ret = new R52(); break;
                case "61": ret = new R61(); break;
                case "70": ret = new R70(); break;
                default: ret = new Unknown(); break;
            }

            var recLen = item.RecData.Length;
            if (!(ret is Unknown) && dataLength != recLen) throw new Exception($"Record defined as {item.RecType} requires a length of {dataLength} but was {recLen}");
            ret.SetData(item.RecData, item.ImageData);
            if (ret.RecordType != item.RecType) throw new Exception("I didn't get what I expected");
            return ret;
        }
    }

    public interface IX9Record
    {
        void SetData(string data, byte[] optional = null);
        string RecordType { get; }
    }

    public abstract class X9Record: IX9Record
    {
        protected string Data;
        public virtual void SetData(string data, byte[] optional = null)
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
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
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
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
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
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
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
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
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

    public class R26: X9Record
    {
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
            Debug.WriteLine("R26 SetData() called");
            CheckDetailAddendumARecordNumber = Data.Substring(2, 1);
            BOFDRoutingNumber = Data.Substring(3, 9);
            BOFDBusinessDate = Data.Substring(12, 8);
            BOFDItemSequenceNumber = Data.Substring(20, 15);
            BOFDDepositAccountNumber = Data.Substring(35, 18);
            BOFDDepositBranch = Data.Substring(53, 5);
            PayeeName = Data.Substring(58, 15);
            TruncationIndicator = Data.Substring(73, 1);
            BOFDConversionIndicator = Data.Substring(74, 1);
            BOFDCorrectionIndicator = Data.Substring(75, 1);
            UserField = Data.Substring(76, 1);
            Reserved = Data.Substring(77, 3);
        }

        public string CheckDetailAddendumARecordNumber { get; set; }
        public string BOFDRoutingNumber { get; set; }
        public string BOFDBusinessDate { get; set; }
        public string BOFDItemSequenceNumber { get; set; }
        public string BOFDDepositAccountNumber { get; set; }
        public string BOFDDepositBranch { get; set; }
        public string PayeeName { get; set; }
        public string TruncationIndicator { get; set; }
        public string BOFDConversionIndicator { get; set; }
        public string BOFDCorrectionIndicator { get; set; }
        public string UserField { get; set; }
        public string Reserved { get; set; }
    }

    public class R50: X9Record
    {
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
            Debug.WriteLine("R50 SetData() called");
            ImageIndicator = Data.Substring(2, 1);
            ImageCreatorRoutingNumber = Data.Substring(3, 9);
            ImageCreatorDate = Data.Substring(12, 8);
            ImageViewFormatIndicator = Data.Substring(20, 2);
            ImageViewCompressionAlgorithmIdentifier = Data.Substring(22, 2);
            ImageViewDataSize = Data.Substring(24, 7);
            ViewSideIndicator = Data.Substring(31, 1);
            ViewDescriptor = Data.Substring(32, 2);
            DigitalSignatureIndicator = Data.Substring(34, 1);
            DigitalSignatureMethod = Data.Substring(35, 2);
            SecurityKeySize = Data.Substring(37, 5);
            StartOfProtectedData = Data.Substring(42, 7);
            LengthOfProtectedData = Data.Substring(49, 7);
            ImageRecreateIndicator = Data.Substring(56, 1);
            UserField = Data.Substring(57, 8);
            Reserved = Data.Substring(65, 15);
        }

        public string ImageIndicator { get; set; }
        public string ImageCreatorRoutingNumber { get; set; }
        public string ImageCreatorDate { get; set; }
        public string ImageViewFormatIndicator { get; set; }
        public string ImageViewCompressionAlgorithmIdentifier { get; set; }
        public string ImageViewDataSize { get; set; }
        public string ViewSideIndicator { get; set; }
        public string ViewDescriptor { get; set; }
        public string DigitalSignatureIndicator { get; set; }
        public string DigitalSignatureMethod { get; set; }
        public string SecurityKeySize { get; set; }
        public string StartOfProtectedData { get; set; }
        public string LengthOfProtectedData { get; set; }
        public string ImageRecreateIndicator { get; set; }
        public string UserField { get; set; }
        public string Reserved { get; set; }
    }

    public class R52: X9Record
    {
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
            Debug.WriteLine("R52 SetData() called");
            ECEInstitutionRoutingNumber = Data.Substring(2, 9);
            BatchBusinessDate = Data.Substring(11, 8);
            CycleNumber = Data.Substring(19, 2);
            ECEInstitutionItemSequenceNumber = Data.Substring(21, 15);
            SecurityOriginatorName = Data.Substring(36, 16);
            SecurityAuthenticator = Data.Substring(52, 16);
            SecurityKeyName = Data.Substring(68, 16);
            ClippingOrigin = Data.Substring(84, 1);
            ClippingCoordinateH1 = Data.Substring(85, 4);
            ClippingCoordinateH2 = Data.Substring(89, 4);
            ClippingCoordinateV1 = Data.Substring(93, 4);
            ClippingCoordinateV2 = Data.Substring(97, 4);
            LengthOfImageReferenceKey = Data.Substring(101, 4);
            LengthOfDigitalSignature = Data.Substring(105, 5);
            LengthOfImageData = Data.Substring(110, 7);
            ImageData = optional;
        }

        public string ECEInstitutionRoutingNumber { get; set; }
        public string BatchBusinessDate { get; set; }
        public string CycleNumber { get; set; }
        public string ECEInstitutionItemSequenceNumber { get; set; }
        public string SecurityOriginatorName { get; set; }
        public string SecurityAuthenticator { get; set; }
        public string SecurityKeyName { get; set; }
        public string ClippingOrigin { get; set; }
        public string ClippingCoordinateH1 { get; set; }
        public string ClippingCoordinateH2 { get; set; }
        public string ClippingCoordinateV1 { get; set; }
        public string ClippingCoordinateV2 { get; set; }
        public string LengthOfImageReferenceKey { get; set; }
        public string LengthOfDigitalSignature { get; set; }
        public string LengthOfImageData { get; set; }
        public byte[] ImageData { get; set; }
    }

    public class R61: X9Record
    {
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
            Debug.WriteLine("R61 SetData() called");
            AuxiliaryOnUs = Data.Substring(2, 15);
            ExternalProcessingCode = Data.Substring(17, 1);
            PayorBankRoutingNumber = Data.Substring(18, 9);
            CreditAccountNumberOnUs = Data.Substring(27, 20);
            ItemAccount = Data.Substring(47, 10);
            ECEInstitutionItemNumber = Data.Substring(57, 15);
            DocumentationTypeIndicator = Data.Substring(72, 1);
            TypeOfAccountCode = Data.Substring(73, 1);
            SourceOfWorkCode = Data.Substring(74, 1);
            WorkType = Data.Substring(75, 1);
            DebitCreditIndicator = Data.Substring(76, 1);
            Reserved = Data.Substring(77, 3);
        }

        public string AuxiliaryOnUs { get; set; }
        public string ExternalProcessingCode { get; set; }
        public string PayorBankRoutingNumber { get; set; }
        public string CreditAccountNumberOnUs { get; set; }
        public string ItemAccount { get; set; }
        public string ECEInstitutionItemNumber { get; set; }
        public string DocumentationTypeIndicator { get; set; }
        public string TypeOfAccountCode { get; set; }
        public string SourceOfWorkCode { get; set; }
        public string WorkType { get; set; }
        public string DebitCreditIndicator { get; set; }
        public string Reserved { get; set; }
    }

    public class R70: X9Record
    {
        public override void SetData(string data, byte[] optional = null)
        {
            base.SetData(data, optional);
            Debug.WriteLine("R70 SetData() called");
            ItemsWithinBatchCount = Data.Substring(2, 4);
            BatchTotalAmount = Data.Substring(6, 12);
            MICRValidTotalAmount = Data.Substring(18, 12);
            ImagesWithinBatchCount = Data.Substring(30, 5);
            UserField = Data.Substring(35, 20);
            Reserved = Data.Substring(55, 25);
        }

        public string ItemsWithinBatchCount { get; set; }
        public string BatchTotalAmount { get; set; }
        public string MICRValidTotalAmount { get; set; }
        public string ImagesWithinBatchCount { get; set; }
        public string UserField { get; set; }
        public string Reserved { get; set; }
    }
}