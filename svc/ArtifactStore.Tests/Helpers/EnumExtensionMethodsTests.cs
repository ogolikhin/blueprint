using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class EnumExtensionMethodsTests
    {
        [TestMethod]
        public void ToPredefinedType_AllTypes()
        {
            // Arrange
            var intTypes = new List<int>
            {
                0x1000 | 6, // PredefinedType.Folder
                0x1000 | 8, // PredefinedType.Actor
                0x1000 | 14, //PredefinedType.Document
                0x1000 | 16, // PredefinedType.DomainDiagram
                0x1000 | 12, // PredefinedType.GenericDiagram
                0x1000 | 3, // PredefinedType.Glossary
                0x1000 | 18, // PredefinedType.Process
                0x1000 | 15, // PredefinedType.Storyboard
                0x1000 | 5, // PredefinedType.Requirement
                0x1000 | 11, // PredefinedType.UiMockup
                0x1000 | 9, // PredefinedType.UseCase
                0x1000 | 17, // PredefinedType.UseCaseDiagram
                0x1000 | 0x100 | 1, // PredefinedType.BaselineReviewFolder
                0x1000 | 0x100 | 2, // PredefinedType.Baleline
                0x1000 | 0x100 | 3, // PredefinedType.Review
                0x1000 | 0x200 | 1, // PredefinedType.CollectionFolder
                0x1000 | 0x200 | 2, // PredefinedType.Collection
                0,
                0x1000 | 19,
                0x1000 | 0x100 | 4,
                0x1000 | 0x200 | 3
            };

            var expected = new List<PredefinedType>
            {
                PredefinedType.Folder,
                PredefinedType.Actor, 
                PredefinedType.Document,
                PredefinedType.DomainDiagram,
                PredefinedType.GenericDiagram,
                PredefinedType.Glossary,
                PredefinedType.Process,
                PredefinedType.Storyboard,
                PredefinedType.Requirement,
                PredefinedType.UiMockup,
                PredefinedType.UseCase,
                PredefinedType.UseCaseDiagram,
                PredefinedType.BaselineReviewFolder,
                PredefinedType.Baleline,
                PredefinedType.Review,
                PredefinedType.CollectionFolder,
                PredefinedType.Collection,
                PredefinedType.Unknown,
                PredefinedType.Unknown,
                PredefinedType.Unknown,
                PredefinedType.Unknown
            };
            // Act
            var actual = intTypes.Select(t => t.ToPredefinedType()).ToList();

            // Assert
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
