using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace SearchService.Helpers
{
    [TestClass]
    public class CriteriaValidatorTests
    {
        #region Full Text Search

        [TestMethod]
        public void Validate_FTSModelStateIsInvalid_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "abc",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, false, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryIsNull_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = null,
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryIsEmpty_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = string.Empty,
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryIsBlank_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "        ",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryIsLessThan3Chars_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = " ab   ",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryIsMoreThan250Chars_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryNoProjectProvided_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "12345",
                ProjectIds = new int[] { }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_FTSQueryMin3CharIsValid_Passes()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "123",
                ProjectIds = new[] { 1 }
            };
            Exception exception = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Exception should not have been thrown");
        }

        [TestMethod]
        public void Validate_FTSQueryMax250CharIsValid_Passes()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
                ProjectIds = new[] { 1 }
            };
            Exception exception = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Exception should not have been thrown");
        }

        [TestMethod]
        public void Validate_FTSQueryIsValid_Passes()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.FullTextSearch;
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "12345678901234567890123456789012345678901234567890",
                ProjectIds = new[] { 1 }
            };
            Exception exception = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, ServiceConstants.MinSearchQueryCharLimit);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Exception should not have been thrown");
        }

        #endregion

        #region Item Name Search

        [TestMethod]
        public void Validate_ItemNameModelStateIsInvalid_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "abc",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, false, searchCriteria, 1);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_ItemNameQueryIsNull_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = null,
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, 1);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_ItemNameQueryIsEmpty_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = string.Empty,
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, 1);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_ItemNameQueryIsBlank_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "        ",
                ProjectIds = new[] { 1 }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, 1);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_ItemNameQueryNoProjectProvided_ThrowsBadRequestException()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "12345",
                ProjectIds = new int[] { }
            };
            BadRequestException badRequestException = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, 1);
            }
            catch (BadRequestException bre)
            {
                badRequestException = bre;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad request exception should have been thrown");
            Assert.IsTrue(badRequestException.ErrorCode == ErrorCodes.IncorrectSearchCriteria);
        }

        [TestMethod]
        public void Validate_ItemNameQueryIsValid_Passes()
        {
            // Arrange
            var criteriaValidator = new CriteriaValidator();
            var searchOption = SearchOption.ItemName;
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "1",
                ProjectIds = new[] { 1 }
            };
            Exception exception = null;

            // Act
            try
            {
                criteriaValidator.Validate(searchOption, true, searchCriteria, 1);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Exception should not have been thrown");
        }

        #endregion

    }
}
