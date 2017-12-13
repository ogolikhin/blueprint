using System.Collections.Generic;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class TriggerConverterTests
    {
        [TestMethod]
        public void ToXmlModel_PropertyChangeAction_ChoiceProperty_DuplicateValidValue_ResultHasCorrectIds()
        {
            // Arrange
            const string propertyName = "Choice Property";
            var converter = new TriggerConverter();
            var trigger = new IeTrigger
            {
                Action = new IePropertyChangeAction
                {
                    PropertyName = propertyName,
                    ValidValues = new List<IeValidValue>
                    {
                        new IeValidValue { Id = 1, Value = "test" },
                        new IeValidValue { Id = 2, Value = "test" }
                    }
                }
            };
            var dataMaps = new WorkflowDataMaps();
            dataMaps.PropertyTypeMap.Add(propertyName, 1);
            dataMaps.ValidValuesById.Add(1, new Dictionary<int, string> { { 1, "test" }, { 2, "test" } });
            dataMaps.ValidValuesByValue.Add(1, new Dictionary<string, int> { { "test", 1 } });

            // Act
            var xmlModel = converter.ToXmlModel(new[] { trigger }, dataMaps);

            // Assert
            var xmlAction = (XmlPropertyChangeAction)xmlModel.Triggers[0].Action;
            Assert.AreEqual(2, xmlAction.ValidValues.Count);
            Assert.IsTrue(xmlAction.ValidValues.Contains(1));
            Assert.IsTrue(xmlAction.ValidValues.Contains(2));
        }
    }
}
