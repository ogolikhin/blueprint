using System;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    /// <summary>
    /// Action Parameter
    /// Provides named data for Trigger's Actions
    /// DataType defines the type of data:None/Number/Text/Date
    /// </summary>
    [XmlType("Parameter")]
    public class ActionParameter
    {
        #region Constructor
        public ActionParameter() { }
        #endregion

        #region Properties

        public string Name { get; set; }

        [XmlElement("Type")]
        public int DataType { get; set; }

        [XmlElement("Value")]
        public string ParamValue { get; set; }

        #endregion

        #region Methods

        public bool IsValid()
        {
            switch ((ActionDataTypes)DataType)
            {
                case ActionDataTypes.None:
                    return false;
                case ActionDataTypes.Text:
                    return true;
                case ActionDataTypes.Number:
                    int num = 0;
                    return int.TryParse(ParamValue, out num);
                case ActionDataTypes.Date:
                    DateTime dt = new DateTime();
                    return DateTime.TryParse(ParamValue, out dt);
                default:
                    return false;
            }
        }
        #endregion
    }
}