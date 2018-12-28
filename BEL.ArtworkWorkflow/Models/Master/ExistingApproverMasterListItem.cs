namespace BEL.ArtworkWorkflow.Models.Master
{
    using CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    /// <summary>
    /// Department Master List Item
    /// </summary>
    [DataContract, Serializable]
    public class ExistingApproverMasterListItem : IMasterItem
    {
        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        [DataMember, FieldColumnName("Title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember, FieldColumnName("Title")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("Requester")]
        public string Requester { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("Requester")]
        public string RequesterName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("BUApprover")]
        public string BUApprover { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("BUApprover")]
        public string BUApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("SCMLevel1Approver")]
        public string SCMLevel1Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("SCMLevel1Approver")]
        public string SCMLevel1ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("QALevel1Approver")]
        public string QALevel1Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("QALevel1Approver")]
        public string QALevel1ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("ADBLevel1Approver")]
        public string ADBLevel1Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("ADBLevel1Approver")]
        public string ADBLevel1ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("MarketingLevel1Approver")]
        public string MarketingLevel1Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("MarketingLevel1Approver")]
        public string MarketingLevel1ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("QALevel2Approver")]
        public string QALevel2Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("QALevel2Approver")]
        public string QALevel2ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("SCMLevel2Approver")]
        public string SCMLevel2Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("SCMLevel2Approver")]
        public string SCMLevel2ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("ADBLevel2Approver")]
        public string ADBLevel2Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("ADBLevel2Approver")]
        public string ADBLevel2ApproverName { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember, FieldColumnName("BsinessUnit", true, false, "Code")]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the domestic or imported.
        /// </summary>
        /// <value>
        /// The domestic or imported.
        /// </value>
        [DataMember, FieldColumnName("DomesticOrImported")]
        public string DomesticOrImported { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true), FieldColumnName("MarketingLevel2Approver")]
        public string MarketingLevel2Approver { get; set; }

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// The UserName.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("MarketingLevel2Approver")]
        public string MarketingLevel2ApproverName { get; set; }
    }
}