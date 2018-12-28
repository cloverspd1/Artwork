namespace BEL.ArtworkWorkflow.Models.ExistingArtwork
{
    using BEL.CommonDataContract;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    /// <summary>
    /// New Artwork Attachment
    /// </summary>
    [DataContract, Serializable]
    public class ExistingArtworkAttachment : ITrans
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DataMember, IsListColumn(false)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        [DataMember, FieldColumnName("RequestID", true, false, "ID")]
        public int RequestID { get; set; }

        /// <summary>
        /// Gets or sets the Request By.
        /// </summary>
        /// <value>
        /// The Request By.
        /// </value>
        [DataMember, IsPerson(true, false), IsViewer]
        public string RequestBy { get; set; }

        /// <summary>
        /// Gets or sets the Request Date.
        /// </summary>
        /// <value>
        /// The Request Date.
        /// </value>
        [DataMember]
        public DateTime? RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        [DataMember, IsListColumn(false)]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the item action.
        /// </summary>
        /// <value>
        /// The item action.
        /// </value>
        [DataMember, IsListColumn(false)]
        public ItemActionStatus ItemAction { get; set; }

        /// <summary>
        /// Gets or sets the Artwork Type.
        /// </summary>
        /// <value>
        /// The Status.
        /// </value>
        [DataMember]
        public string ArtworkType { get; set; }

        /// <summary>
        /// Artwork Type Code
        /// </summary>
        [DataMember]
        public string ArtworkTypeCode { get; set; }

        /////// <summary>
        /////// Gets or sets the file name list.
        /////// </summary>
        /////// <value>
        /////// The file name list.
        /////// </value>
        ////[DataMember, IsListColumn(false)]
        ////public bool IsNotApplicable { get; set; }

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
        [DataMember, IsListColumn(false)]
        public string FileNameList { get; set; }

        /// <summary>
        /// Gets or sets the file name list.
        /// </summary>
        /// <value>
        /// The file name list.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("FileName")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        /// <value>
        /// The Status.
        /// </value>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the Item Code.
        /// </summary>
        /// <value>
        /// The Item Code.
        /// </value>
        [DataMember, IsListColumn(false)]
        public string ItemCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not applicable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not applicable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsNotApplicable { get; set; }
    }
}