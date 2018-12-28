﻿namespace BEL.ArtworkWorkflow.Models.ExistingArtwork
{
    using BEL.CommonDataContract;
    using BEL.ArtworkWorkflow.Models.Common;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Existing Artwork Form
    /// </summary>
    [DataContract, Serializable]
    public class ExistingArtworkForm : IForm
    {
        /// <summary>
        /// Initializes a new instance of the Existing Artwork Form class.
        /// </summary>
        /// <param name="setSections">if set to <c>true</c> [set setSections].</param>
        public ExistingArtworkForm(bool setSections)
        {
            if (setSections)
            {
                this.ApprovalMatrixListName = ArtworkListNames.EXISTINGARTWORKAPPAPPROVALMATRIX;
                this.SectionsList = new List<ISection>();
                this.SectionsList.Add(new ExistingArtworkDetailSection(true));
                this.SectionsList.Add(new ExistingArtworkBUHeadApproverSection(true));                
                this.SectionsList.Add(new ExistingArtworkADBLevel1Section(true));
                this.SectionsList.Add(new ExistingArtworkSCMLevel1Section(true));
                this.SectionsList.Add(new ExistingArtworkQALevel1Section(true));
                this.SectionsList.Add(new ExistingArtworkMarketingLevel1Section(true));
                this.SectionsList.Add(new ExistingArtworkADBLevel2Section(true));
                this.SectionsList.Add(new ExistingArtworkSCMLevel2Section(true));
                this.SectionsList.Add(new ExistingArtworkQALevel2Section(true));
                this.SectionsList.Add(new ExistingArtworkMarketingLevel2Section(true));
                this.SectionsList.Add(new ExistingArtworkABSQTeamSection(true));
                this.SectionsList.Add(new ExistingArtworkABSQApproverSection(true));
                this.SectionsList.Add(new ApplicationStatusSection(true) { SectionName = SectionNameConstant.APPLICATIONSTATUS });
                this.SectionsList.Add(new ActivityLogSection(ArtworkListNames.EXISTINGARTWORKACTIVITYLOG));
                this.Buttons = new List<Button>();
                this.MainListName = ArtworkListNames.EXISTINGARTWORKLIST;
            }
        }

        /// <summary>
        /// Gets the name of the form.
        /// </summary>
        /// <value>
        /// The name of the form.
        /// </value>
        [DataMember]
        public string FormName
        {
            get { return ArtworkFormNames.EXISTINGARTWORKFORM; }
        }

        /// <summary>
        /// Gets or sets the form status.
        /// </summary>
        /// <value>
        /// The form status.
        /// </value>
        [DataMember]
        public string FormStatus { get; set; }

        /// <summary>
        /// Gets or sets the form approval level.
        /// </summary>
        /// <value>
        /// The form approval level.
        /// </value>
        [DataMember]
        public int FormApprovalLevel { get; set; }

        /// <summary>
        /// Gets or sets the total approval required.
        /// </summary>
        /// <value>
        /// The total approval required.
        /// </value>
        [DataMember]
        public int TotalApprovalRequired { get; set; }

        /// <summary>
        /// Gets or sets the sections list.
        /// </summary>
        /// <value>
        /// The sections list.
        /// </value>
        [DataMember]
        public List<ISection> SectionsList { get; set; }

        /// <summary>
        /// Gets or sets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        [DataMember]
        public List<Button> Buttons { get; set; }

        /// <summary>
        /// Gets or sets the name of the approval matrix list.
        /// </summary>
        /// <value>
        /// The name of the approval matrix list.
        /// </value>
        [DataMember]
        public string ApprovalMatrixListName { get; set; }

        /// <summary>
        /// Gets or sets the name of the main list.
        /// </summary>
        /// <value>
        /// The name of the main list.
        /// </value>
        [DataMember]
        public string MainListName { get; set; }
    }
}