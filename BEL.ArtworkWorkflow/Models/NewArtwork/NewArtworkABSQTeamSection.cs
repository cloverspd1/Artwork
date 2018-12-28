namespace BEL.ArtworkWorkflow.Models.NewArtwork
{
    using BEL.CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;
    using BEL.ArtworkWorkflow.Models.Common;
    using System.ComponentModel.DataAnnotations;
    using BEL.ArtworkWorkflow.Models.Master;

    /// <summary>
    /// New Artwork ABSQ Team Section
    /// </summary>
    [DataContract, Serializable]
    public class NewArtworkABSQTeamSection : ISection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewArtworkABSQTeamSection"/> class.
        /// </summary>
        public NewArtworkABSQTeamSection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewArtworkABSQTeamSection"/> class.
        /// </summary>
        /// <param name="isSet">if set to <c>true</c> [is set].</param>
        public NewArtworkABSQTeamSection(bool isSet)
        {
            if (isSet)
            {
                this.ListDetails = new List<ListItemDetail>() { new ListItemDetail(ArtworkListNames.NEWARTWORKLIST, true) };
                this.SectionName = ArtworkSectionName.ABSQTEAMSECTION;
                this.ArtworkAttachmentList = new List<ITrans>();
                this.ArtworkAttachmentTempList = new List<NewArtworkAttachment>();
                this.ApproversList = new List<ApplicationStatus>();
                this.CurrentApprover = new ApplicationStatus();
            }
        }

        /// <summary>
        /// Gets or sets the master data.
        /// </summary>
        /// <value>
        /// The master data.
        /// </value>
        [DataMember, IsListColumn(false), ContainsMasterData(true)]
        public List<IMaster> MasterData { get; set; }

        /// <summary>
        /// Gets or sets the Vendor DCR.
        /// </summary>
        /// <value>
        /// The Vendor DCR
        /// </value>
        [DataMember, IsListColumn(false), IsTran(true, ArtworkListNames.NEWARTWORKATTACHMENTLIST, typeof(NewArtworkAttachment))]
        public List<ITrans> ArtworkAttachmentList { get; set; }

        /// <summary>
        /// Gets or sets the Vendor DCR.
        /// </summary>
        /// <value>
        /// The Vendor DCR
        /// </value>
        [DataMember, IsListColumn(false)]
        public List<NewArtworkAttachment> ArtworkAttachmentTempList { get; set; }

        ///// <summary>
        ///// Gets or sets the  Product Serial List.
        ///// </summary>
        ///// <value>
        ///// The Product Serial List
        ///// </value>
        ////[DataMember, IsListColumn(false), IsTran(true, ArtworkListNames.FEEDBACKPRODUCTSERIAL, typeof(ProductSerial))]
        ////public List<ITrans> ProductSerial { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember, IsListColumn(false)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        /// <value>
        /// The name of the section.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string SectionName { get; set; }

        /// <summary>
        /// Gets or sets the form belong to.
        /// </summary>
        /// <value>
        /// The form belong to.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string FormBelongTo { get; set; }

        /// <summary>
        /// Gets or sets the application belong to.
        /// </summary>
        /// <value>
        /// The application belong to.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string ApplicationBelongTo { get; set; }

        /// <summary>
        /// Gets or sets the list details.
        /// </summary>
        /// <value>
        /// The list details.
        /// </value>
        [DataMember, IsListColumn(false)]
        public List<ListItemDetail> ListDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember, IsListColumn(false)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the action status.
        /// </summary>
        /// <value>
        /// The action status.
        /// </value>
        [DataMember, IsListColumn(false)]
        public ButtonActionStatus ActionStatus { get; set; }

        /// <summary>
        /// Gets or sets the current approver.
        /// </summary>
        /// <value>
        /// The current approver.
        /// </value>
        [DataMember, IsListColumn(false), IsApproverDetails(true, ArtworkListNames.NEWARTWORKAPPAPPROVALMATRIX)]
        public ApplicationStatus CurrentApprover { get; set; }

        /// <summary>
        /// Gets or sets the approvers list.
        /// </summary>
        /// <value>
        /// The approvers list.
        /// </value>
        [DataMember, IsListColumn(false), IsApproverMatrixField(true, ArtworkListNames.NEWARTWORKAPPAPPROVALMATRIX)]
        public List<ApplicationStatus> ApproversList { get; set; }

        /// <summary>
        /// Gets or sets the button caption.
        /// </summary>
        /// <value>
        /// The button caption.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string ButtonCaption { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [DataMember, IsFile(true)]
        public List<FileDetails> Files { get; set; }

        /// <summary>
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember, FieldColumnName("FNLTechnicalSpecificationAttachm")]
        public string FNLTechnicalSpecificationAttachment { get; set; }

        /// <summary>
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember]
        public string FNLQAPAttachment { get; set; }

        /// <summary>
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(true), Required, FieldColumnName("Model"), RequiredOnDraft]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string OldModelName { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true)]
        public string ItemCode { get; set; }
    }
}