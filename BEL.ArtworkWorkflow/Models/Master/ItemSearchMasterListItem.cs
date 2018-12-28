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
    public class ItemSearchMasterListItem : IMasterItem
    {
        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ItemCode")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Item Code
        /// </summary>
        /// <value>
        /// The Item Code
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("ItemCode")]
        public string ItemCode { get; set; }

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
        /// Gets or sets the Model.
        /// </summary>
        /// <value>
        /// The Model.
        /// </value>
        [DataMember, IsListColumn(true), FieldColumnName("Model")]
        public string ModelName { get; set; }
    }
}