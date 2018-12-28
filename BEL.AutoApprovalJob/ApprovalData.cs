using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEL.AutoApprovalJob
{
    public class ApprovalData
    {
        public string Approver { get; set; }

        public string ApproverName { get; set; }

        public int RequestID { get; set; }

        public bool IsAutoApproval { get; set; }

        public bool IsAutoRejection { get; set; }

        public string SectionName { get; set; }

        public string Levels { get; set; }
    }
}
