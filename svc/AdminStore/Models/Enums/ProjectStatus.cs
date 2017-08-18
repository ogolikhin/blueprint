using System.ComponentModel.DataAnnotations;

namespace AdminStore.Models.Enums
{
    public enum ProjectStatus
    {
        /// <summary>
        /// Project is in normal state and can be opened
        /// </summary>
        Live,
        /// <summary>
        /// Project is currently being imported
        /// </summary>
        Importing,
        /// <summary>
        /// The import process is cancelling when Cancel button is clicked in import Wizard
        /// </summary>
        CancelingImport,
        /// <summary>
        /// The import failed during a recoverable error
        /// </summary>
        ImportFailed
    }
}