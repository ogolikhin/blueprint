using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;

namespace AdminStore.Helpers
{
    [TestClass]
    public class SearchFieldValidatorTest
    {
        [TestMethod]
        public void Validate_SearchStringIsCorrect_SuccessfulValidation()
        {
            //Arange
            var search = "hello";
            Exception exception = null;

            //Act
            try
            {
                SearchFieldValidator.Validate(search);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNull(exception);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public void Validate_SearchStringExceedsLimit_BadRequestError()
        {
            //Arange
            var search = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis feugiat,
                            purus id elementum tincidunt, urna urna gravida sem, id interdum dui diam vel nulla. Vivamus ultrices
                           orci metus, eu luctus nisi ultricies ac. Nulla facilisi. Nam sed turpis duis.";

            //Act
            SearchFieldValidator.Validate(search);

            //Assert
            //Exception
        }
    }
}