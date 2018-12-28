namespace BEL.ArtworkWorkflow.Models.Master
{
    using CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    /// <summary>
    /// Artwork Type Master ListItem
    /// </summary>
    [DataContract, Serializable]
    public class ItemMasterArtworkTypeMappingMasterListItem : IMasterItem
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>  
        [FieldColumnName("Title")]
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [FieldColumnName("Title")]
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Artwork Type
        /// </summary>
        [FieldColumnName("ArtworkType")]
        [DataMember]
        public string ArtworkType { get; set; }

        /// <summary>
        /// Artwork Type Code
        /// </summary>
        [FieldColumnName("ArtworkTypeCode")]
        [DataMember]
        public string ArtworkTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ItemCode")]
        public string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string FileNameList { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [DataMember, IsFile(true), FieldColumnName("Attachments")]
        public List<FileDetails> Files { get; set; }

        /// <summary>
        /// Gets or sets the Request By.
        /// </summary>
        /// <value>
        /// The Request By.
        /// </value>
        [DataMember, IsPerson(true, false), FieldColumnName("RequestBy")]
        public string RequestBy { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("CreatedDate")]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        [DataMember, FieldColumnName("RequestID", true, false, "ID")]
        public string RequestID { get; set; }

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
        /// Gets or sets a value indicating whether this instance is not applicable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not applicable; otherwise, <c>false</c>.
        /// </value>
        [DataMember, FieldColumnName("IsNotApplicable")]
        public bool IsNotApplicable { get; set; }
    }
}