// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.RuleEngine.Common
{
    /// <summary>
    /// The AdditionInfo class.
    /// </summary>
    public class AdditionalInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entityId">The entity-id.</param>
        /// <param name="hasETag">Indicates whether the entity has an etag.</param>
        /// <param name="hasMediaTag">Indicates whether the entity has a media tag.</param>
        public AdditionalInfo(string entityId, string etag = null, string mediaEtag = null)
        {
            this.EntityId = entityId;
            this.ODataEtag = etag;
            this.ODataMediaEtag = mediaEtag;
        }

        /// <summary>
        /// Gets or private sets the entity-id.
        /// </summary>
        public string EntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the etag status.
        /// </summary>
        public string ODataEtag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or private sets the media tag status.
        /// </summary>
        public string ODataMediaEtag
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the current entity has an odata.etag or odata.mediaEtag annotation or not.
        /// </summary>
        public bool HasEtag
        {
            get
            {
                return !string.IsNullOrEmpty(this.ODataEtag) | !string.IsNullOrEmpty(this.ODataMediaEtag);
            }
        }
    }
}
