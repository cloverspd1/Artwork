namespace BEL.ArtworkWorkflow.Models.ViewArtwork
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
    using BEL.ArtworkWorkflow.Models.ExistingArtwork;

    /// <summary>
    /// Existing Artwork Detail Section
    /// </summary>
    [DataContract, Serializable]
    public class ViewArtworkDetailSection : ISection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewArtworkDetailSection"/> class.
        /// </summary>
        public ViewArtworkDetailSection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewArtworkDetailSection"/> class.
        /// </summary>
        /// <param name="isSet">if set to <c>true</c> [is set].</param>
        public ViewArtworkDetailSection(bool isSet)
        {
            if (isSet)
            {
                this.ListDetails = new List<ListItemDetail>() { new ListItemDetail(ArtworkMasters.ITEMMASTER, true) };
                this.SectionName = ArtworkSectionName.ARTWORKDETAILSECTION;

                this.ExistingAttachment = new List<ITrans>();
                this.TempExistingAttachment = new List<ExistingArtworkAttachment>();

                this.MasterData = new List<IMaster>();
                this.MasterData.Add(new ExistingApproverMaster());
                this.MasterData.Add(new RoleWiseApproverMaster());
                this.MasterData.Add(new BusinessUnitMaster());
                this.MasterData.Add(new ProductCategoryMaster());
                this.MasterData.Add(new ArtworkTypeMaster());
            }
        }

        /// <summary>
        /// Is ABSQ User
        /// </summary>
        [DataMember, IsListColumn(false)]
        public bool IsABSQUser { get; set; }

        /// <summary>
        /// Is ABSQ User
        /// </summary>
        [DataMember, IsListColumn(false)]
        public bool IsNewArtwork { get; set; }

        /// <summary>
        /// ABSQ User
        /// </summary>
        [DataMember, IsListColumn(false)]
        public string ABSQUsers { get; set; }

        /// <summary>
        /// Gets or sets the master data.
        /// </summary>
        /// <value>
        /// The master data.
        /// </value>
        [DataMember, IsListColumn(false), ContainsMasterData(true)]
        public List<IMaster> MasterData { get; set; }

        /// <summary>
        /// Gets or sets the  Product Serial List.
        /// </summary>
        /// <value>
        /// The Product Serial List
        /// </value>
        [DataMember, IsListColumn(false), IsTran(true, ArtworkListNames.EXISTINGARTWORKATTACHMENTLIST, typeof(ExistingArtworkAttachment))]
        public List<ITrans> ExistingAttachment { get; set; }

        /// <summary>
        /// Gets or sets the  Product Serial List.
        /// </summary>
        /// <value>
        /// The Product Serial List
        /// </value>
        [DataMember, IsListColumn(false)]
        public List<ExistingArtworkAttachment> TempExistingAttachment { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember, IsListColumn(false)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        [DataMember, IsPerson(true, false), IsViewer]
        public string ProposedBy { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
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
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string FileNameList { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the Business Unit.
        /// </summary>
        /// <value>
        /// The Business Unit.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the domestic or imported.
        /// </summary>
        /// <value>
        /// The domestic or imported.
        /// </value>
        [DataMember, IsListColumn(true)]
        public string DomesticOrImported { get; set; }

        /// <summary>
        /// Gets or sets the Product Category.
        /// </summary>
        /// <value>
        /// The Product Category.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string ProductCategory { get; set; }

        /// <summary>
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(true), Required, FieldColumnName("Model"), MaxLength(255)]
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
        /// Gets or sets the Artwork Type.
        /// </summary>
        /// <value>
        /// The Artwork Type.
        /// </value>
        [DataMember, IsListColumn(true), Required]
        public string ArtworkType { get; set; }

        /// <summary>
        /// str Artwork TypeList
        /// </summary>
        [DataMember, IsListColumn(false), Required]
        public string StrArtworkTypeList { get; set; }
    }
}