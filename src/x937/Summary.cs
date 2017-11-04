using System;
using System.Diagnostics;
using x937.ICL;

namespace x937
{
    public class Summary
    {
        private static ICLFileSummary _objFileSummary;
        private static ICLCashLetterSummary _objCashLetterSummary;
        private static ICLCreditSummary _objCreditSummary;

        private static ICLFileDetail _ifd;
        private static ICLCashLetterDetail _icld;
        private static ICLCreditDetail _icrd;

        public Summary()
        {
            SetObjects();
        }

        //public void SetObjectsNewForm()
        //{
        //    //Debug.WriteLine("SetObjectsNewForm");
        //    _ifd = new ICLFileDetail();
        //    _icld = new ICLCashLetterDetail();
        //    _icrd = new ICLCreditDetail();
        //    _objFileSummary = new ICLFileSummary();
        //    _objCashLetterSummary = new ICLCashLetterSummary();
        //    _objCreditSummary = new ICLCreditSummary();
        //}

        public void SetObjects()
        {
            //Debug.WriteLine("SetObjects");
            _ifd = _ifd ?? new ICLFileDetail();
            _icld = _icld ?? new ICLCashLetterDetail();
            _icrd = _icrd ?? new ICLCreditDetail();
            _objFileSummary = _objFileSummary ?? new ICLFileSummary();
            _objCashLetterSummary = _objCashLetterSummary ?? new ICLCashLetterSummary();
            _objCreditSummary = _objCreditSummary ?? new ICLCreditSummary();
        }

        private void CashLetterEnd()
        {
            _objCashLetterSummary.Add(_icld);
            _icld = new ICLCashLetterDetail();
        }

        private void BundleEnd()
        {
            if (_icrd != null && _icrd.CreditAmount > 0)
            {
                _objCreditSummary.Add(_icrd);
                _icrd = new ICLCreditDetail();
            }
        }

        public void FileSummary(string recType, string recData)
        {
            if (recType == "01")
            {
                _ifd.BankName = recData.Substring(36, 18);
                _ifd.FileCreationDate = recData.Substring(23, 8);
                _ifd.FileCreationTime = recData.Substring(31, 4);

            }
            if (recType == "99")
            {
                _ifd.TotalFileAmount = Convert.ToInt64(recData.Substring(24, 16));
                _ifd.TotalNumberofRecords = Convert.ToInt32(recData.Substring(8, 8));
            }
        }

        public void CashLetterSummary(string recType, string recData)
        {
            if (recType == "10" || recType == "90")
            {
                if (recType == "10")
                {
                    _icld.CashLetterPosition = Convert.ToInt32(recData.Substring(44, 8));
                }
                if (recType == "90")
                {
                    _icld.CashLetterAmount = Convert.ToInt64(recData.Substring(16, 14));
                    CashLetterEnd();
                }
            }
        }

        public void BundleSummary(string recType, string recData)
        {
            if (recType == "20" || recType == "70")
            {
                if (recType == "20")
                {
                }
                if (recType == "70")
                {
                    BundleEnd();
                }
            }
        }

        public void CreditSummary(string recType, string recData)
        {
            if (recType == "61")
            {
                _icrd.CreditAmount = Convert.ToInt64(recData.Substring(48, 10));
                _icrd.PostingAccRT = recData.Substring(19, 9);
                _icrd.PostingAccBankOnUs = recData.Substring(28, 20);
            }
        }

        public void CreateNodeValues()
        {
            Debug.WriteLine("CreateNodeValues");
            Console.WriteLine("**SUMMARY**");
            var tvSum = new TreeNode<string>(_ifd.BankName.Trim() + " - ICL File Summary");
            var total = tvSum.AddChild("File Total Amount ($) - " + _ifd.TotalFileAmount);
            total.AddChild("File Creation Date - " + _ifd.FileCreationDate);
            total.AddChild("Total No of Records - " + _ifd.TotalNumberofRecords);
            total.AddChild("Total No of Cash Letter - " + _objCashLetterSummary.Count);
            var cl = tvSum.AddChild("Total Cash Letter Amount ($) - " + _objCashLetterSummary.TotalCashLetterAmount);
            foreach (var icd in _objCashLetterSummary)
            {
                cl.AddChild("Cash Letter " + icd.CashLetterPosition + " Amount ($) - " + icd.CashLetterAmount);
            }
            if (_objCreditSummary != null && _objCreditSummary.Count > 0)
            {
                cl.AddChild("Total No of Credit Records - " + _objCreditSummary.Count);
                var credit = tvSum.AddChild("Total Credit Amount ($) - " + _objCreditSummary.TotalCreditRecordAmount);
                foreach (ICLCreditDetail crd in _objCreditSummary)
                {
                    credit.AddChild($"Posting Bank On-Us {crd.PostingAccBankOnUs.Trim()} Amount ($)- {crd.CreditAmount}");
                }
            }

            foreach (var node in tvSum)
            {
                var indent = Utils.CreateIndent(node.Level+1);
                Console.WriteLine($"{indent}{node.Data}");
            }
        }
    }
}
