using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;

namespace ServiceLibrary.Models
{
    [TestClass]
    public class ItemsRemovalParamsExtensionsTests
    {
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public void Validate_ItemsRemovalParamsIsNull_BadRequestException()
        {
            ItemsRemovalParams removalParams = null;
            removalParams.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public void Validate_ItemsRemovalParamsIsEmpty_BadRequestException()
        {
            var removalParams = new ItemsRemovalParams()
            {
                ItemIds = new List<int>(),
                SelectionType = SelectionType.Selected
            };
            removalParams.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public void Validate_ItemsRemovalParamsItemIdsIsNullSelectionTypeExcluded_BadRequestException()
        {
            var removalParams = new ItemsRemovalParams()
            {
                ItemIds = null,
                SelectionType = SelectionType.Excluded
            };
            removalParams.Validate();
        }

        [TestMethod]
        public void Validate_AllParametersAreValid_Success()
        {
            var removalParams = new ItemsRemovalParams()
            {
                ItemIds = new List<int> { 1, 2, 3 }
            };
            removalParams.Validate();
        }
    }
}
