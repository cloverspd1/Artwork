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
    /// New Artwork Detail Section Detail Section
    /// </summary>
    [DataContract, Serializable]
    public class NewArtworkDetailSection : ISection
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="NewArtworkDetailSection"/> class.
        /// </summary>
        public NewArtworkDetailSection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewArtworkDetailSection"/> class.
        /// </summary>
        /// <param name="isSet">if set to <c>true</c> [is set].</param>
        public NewArtworkDetailSection(bool isSet)
        {
            if (isSet)
            {
                this.ListDetails = new List<ListItemDetail>() { new ListItemDetail(ArtworkListNames.NEWARTWORKLIST, true) };
                this.SectionName = ArtworkSectionName.ARTWORKDETAILSECTION;

                this.ApproversList = new List<ApplicationStatus>();
                this.CurrentApprover = new ApplicationStatus();

                this.MasterData = new List<IMaster>();
                this.MasterData.Add(new ApproverMaster());
                this.MasterData.Add(new RoleWiseApproverMaster());
                this.MasterData.Add(new BusinessUnitMaster());
                this.MasterData.Add(new ProductCategoryMaster());
                this.MasterData.Add(new ArtworkTypeMaster());
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
        /// Gets or sets the Proposed By.
        /// </summary>
        /// <value>
        /// The Proposed By.
        /// </value>
        [DataMember, IsPerson(true, false), IsViewer]
        public string ProposedBy { get; set; }

        /// <summary>
        /// Gets or sets the Proposed By Name.
        /// </summary>
        /// <value>
        /// The Proposed By Name.
        /// </value>
        [DataMember, IsPerson(true, false, true), FieldColumnName("ProposedBy"), IsViewer]
        public string ProposedByName { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember]
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the workflow status.
        /// </summary>
        /// <value>
        /// The workflow status.
        /// </value>
        [DataMember]
        public string WorkflowStatus { get; set; }

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
        [DataMember]
        public string FNLSamplePhoto { get; set; }

        /// <summary>
        /// Old REFERENCE ID
        /// </summary>
        [DataMember, IsListColumn(false)]
        public int OldRequestId { get; set; }

        /// <summary>
        /// Gets or sets the Old Reference No.
        /// </summary>
        /// <value>
        /// The Old Reference No.
        /// </value>
        [DataMember]
        public string OldReferenceNo { get; set; }

        /// <summary>
        /// Gets or sets the Old Artwork Created Date.
        /// </summary>
        /// <value>
        /// Old Artwork Created Date
        /// </value>
        [DataMember]
        public DateTime? OldArtworkCreatedDate { get; set; }

        /// <summary>
        /// Artwork Rejected Date
        /// </summary>
        [DataMember]
        public DateTime? OldArtworkRejectedDate { get; set; }

        /// <summary>
        /// Artwork Rejected Comment
        /// </summary>
        [DataMember]
        public string OldArtworkRejectedComment { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        /// <value>
        /// The month.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string Month
        {
            get
            {
                if (this.RequestDate.HasValue)
                {
                    return Convert.ToDateTime(this.RequestDate).ToString("MMMM");
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string Year
        {
            get
            {
                if (this.RequestDate.HasValue)
                {
                    return Convert.ToDateTime(this.RequestDate).Year.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), Required, RequiredOnDraft]
        public string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the Reference No.
        /// </summary>
        /// <value>
        /// The Reference No.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Gets or sets the Business Unit.
        /// </summary>
        /// <value>
        /// The Business Unit.
        /// </value>
        [DataMember, IsListColumn(true), Required, RequiredOnDraft]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the Product Category.
        /// </summary>
        /// <value>
        /// The Product Category.
        /// </value>
        [DataMember, IsListColumn(true), Required, RequiredOnDraft]
        public string ProductCategory { get; set; }

        /// <summary>
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(true), Required, FieldColumnName("Model"), RequiredOnDraft]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the Color.
        /// </summary>
        /// <value>
        /// The Color.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the Bar Code.
        /// </summary>
        /// <value>
        /// The Bar Code.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string BarCode { get; set; }

        /// <summary>
        /// Gets or sets the MRP.
        /// </summary>
        /// <value>
        /// The MRP.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string MRP { get; set; }

        /////// <summary>
        /////// Gets or sets the Unit Carton Dimension.
        /////// </summary>
        /////// <value>
        /////// The Unit Carton Dimension.
        /////// </value>
        ////[DataMember, IsListColumn(true), Required]
        ////public string UnitCartonDimension { get; set; }

        /////// <summary>
        /////// Gets or sets the Master Carton Dimension.
        /////// </summary>
        /////// <value>
        /////// The Master Carton Dimension.
        /////// </value>
        ////[DataMember, IsListColumn(true), Required]
        ////public string MasterCartondimension { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string UnitCartonDimensionL { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string UnitCartonDimensionW { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string UnitCartonDimensionH { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string MasterCartondimensionL { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string MasterCartondimensionW { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string MasterCartondimensionH { get; set; }

        /// <summary>
        /// Gets or sets the No of Carton in Master.
        /// </summary>
        /// <value>
        /// The No of Carton in Master.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string NoofCartoninMaster { get; set; }

        /// <summary>
        /// Gets or sets the Voltage.
        /// </summary>
        /// <value>
        /// The Voltage.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string Voltage { get; set; }

        /// <summary>
        /// Gets or sets the Power.
        /// </summary>
        /// <value>
        /// The Power.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string Power { get; set; }

        /// <summary>
        /// Gets or sets the Net Weight.
        /// </summary>
        /// <value>
        /// The Net Weight.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string NetWeight { get; set; }

        /// <summary>
        /// Gets or sets the Gross Weight.
        /// </summary>
        /// <value>
        /// The Gross Weight.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string GrossWeight { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Code.
        /// </summary>
        /// <value>
        /// The Vendor Code.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string VendorCode { get; set; }

        /// <summary>
        /// Gets or sets the Product.
        /// </summary>
        /// <value>
        /// The Product.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Address.
        /// </summary>
        /// <value>
        /// The Vendor Address.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string VendorAddress { get; set; }

        /// <summary>
        /// Gets or sets the Warranty.
        /// </summary>
        /// <value>
        /// The Warranty.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string Warranty { get; set; }

        /// <summary>
        /// Gets or sets the Product Feature.
        /// </summary>
        /// <value>
        /// The Product Feature.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string ProductFeature { get; set; }

        /// <summary>
        /// Gets or sets the Artwork Type.
        /// </summary>
        /// <value>
        /// The Artwork Type.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string ArtworkType { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember, IsPerson(true, true, false), IsViewer]
        public string ABSAdmin { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember, IsPerson(true, true, true), FieldColumnName("ABSAdmin"), IsViewer]
        public string ABSAdminName { get; set; }

        /////// <summary>
        /////// Gets or sets the request date.
        /////// </summary>
        /////// <value>
        /////// The request date.
        /////// </value>
        ////[DataMember, IsPerson(true, true, false), IsViewer]
        ////public string CC { get; set; }

        /////// <summary>
        /////// Gets or sets the request date.
        /////// </summary>
        /////// <value>
        /////// The request date.
        /////// </value>
        ////[DataMember, IsPerson(true, true, true), FieldColumnName("CC"), IsViewer]
        ////public string CCName { get; set; }

        /////// <summary>
        /////// Gets or sets the request date.
        /////// </summary>
        /////// <value>
        /////// The request date.
        /////// </value>
        ////[DataMember, IsPerson(true, true, false), IsViewer]
        ////public string Legal { get; set; }

        /////// <summary>
        /////// Gets or sets the request date.
        /////// </summary>
        /////// <value>
        /////// The request date.
        /////// </value>
        ////[DataMember, IsPerson(true, true, true), FieldColumnName("Legal"), IsViewer]
        ////public string LegalName { get; set; }

        /// <summary>
        /// Gets or sets the domestic or imported.
        /// </summary>
        /// <value>
        /// The domestic or imported.
        /// </value>
        [DataMember, Required]
        public string DomesticOrImported { get; set; }
    }
}