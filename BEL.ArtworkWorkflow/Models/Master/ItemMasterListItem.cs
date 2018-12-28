namespace BEL.ArtworkWorkflow.Models.Master
{
    using CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    /// <summary>
    /// Item Master ListItem
    /// </summary>
    [DataContract, Serializable]
    public class ItemMasterListItem : IMasterItem
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>  
        [FieldColumnName("ItemCode")]
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [FieldColumnName("ItemCode")]
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>  
        [FieldColumnName("ID")]
        [DataMember]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ItemCode")]
        public string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the Reference No.
        /// </summary>
        /// <value>
        /// The Reference No.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ReferenceNo")]
        public string ReferenceNo { get; set; }

        /// <summary>
        /// Gets or sets the Business Unit.
        /// </summary>
        /// <value>
        /// The Business Unit.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("BusinessUnit")]
        public string BusinessUnit { get; set; }

        /// <summary>
        /// Gets or sets the domestic or imported.
        /// </summary>
        /// <value>
        /// The domestic or imported.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("DomesticOrImported")]
        public string DomesticOrImported { get; set; }

        /// <summary>
        /// Gets or sets the Product Category.
        /// </summary>
        /// <value>
        /// The Product Category.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ProductCategory")]
        public string ProductCategory { get; set; }

        /// <summary>
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Model")]
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the Bar Code.
        /// </summary>
        /// <value>
        /// The Bar Code.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("BarCode")]
        public string BarCode { get; set; }

        /// <summary>
        /// Gets or sets the MRP.
        /// </summary>
        /// <value>
        /// The MRP.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("MRP")]
        public string MRP { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("NoofCartoninMaster")]
        public string NoofCartoninMaster { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("UnitCartonDimensionL")]
        public string UnitCartonDimensionL { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("UnitCartonDimensionW")]
        public string UnitCartonDimensionW { get; set; }

        /// <summary>
        /// Gets or sets the Unit Carton Dimension.
        /// </summary>
        /// <value>
        /// The Unit Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("UnitCartonDimensionH")]
        public string UnitCartonDimensionH { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("MasterCartondimensionL")]
        public string MasterCartondimensionL { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("MasterCartondimensionW")]
        public string MasterCartondimensionW { get; set; }

        /// <summary>
        /// Gets or sets the Master Carton Dimension.
        /// </summary>
        /// <value>
        /// The Master Carton Dimension.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("MasterCartondimensionH")]
        public string MasterCartondimensionH { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Code.
        /// </summary>
        /// <value>
        /// The Vendor Code.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("VendorCode")]
        public string VendorCode { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Address.
        /// </summary>
        /// <value>
        /// The Vendor Address.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("VendorAddress")]
        public string VendorAddress { get; set; }

        /// <summary>
        /// Gets or sets the Warranty.
        /// </summary>
        /// <value>
        /// The Warranty.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Warranty")]
        public string Warranty { get; set; }

        /// <summary>
        /// Gets or sets the Locking Date.
        /// </summary>
        /// <value>
        /// The Locking Date.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("LockingDate")]
        public DateTime? LockingDate { get; set; }

        /// <summary>
        /// Gets or sets the Voltage.
        /// </summary>
        /// <value>
        /// The Voltage.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Voltage")]
        public string Voltage { get; set; }

        /// <summary>
        /// Gets or sets the Power.
        /// </summary>
        /// <value>
        /// The Power.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Power")]
        public string Power { get; set; }

        /// <summary>
        /// Gets or sets the Net Weight.
        /// </summary>
        /// <value>
        /// The Net Weight.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("NetWeight")]
        public string NetWeight { get; set; }

        /// <summary>
        /// Gets or sets the Gross Weight.
        /// </summary>
        /// <value>
        /// The Gross Weight.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("GrossWeight")]
        public string GrossWeight { get; set; }

        /// <summary>
        /// Gets or sets the Product.
        /// </summary>
        /// <value>
        /// The Product.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Color")]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the Product.
        /// </summary>
        /// <value>
        /// The Product.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Product")]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the Product.
        /// </summary>
        /// <value>
        /// The Product.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("IsNewArtwork")]
        public bool IsNewArtwork { get; set; }
    }
}