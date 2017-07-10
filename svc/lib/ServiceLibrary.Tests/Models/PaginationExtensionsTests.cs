using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    [TestClass]
    public class PaginationExtensionsTests
    {
        [TestMethod]
        public void Validate_PaginationNotSpecified_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            Pagination pagination = null;

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.InvalidPagination, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public void Validate_OffsetIsNull_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            Pagination pagination = new Pagination();

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.IncorrectOffsetParameter, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public void Validate_OffsetIsNegative_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            var pagination = new Pagination { Offset = -1, Limit = 25 };

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.IncorrectOffsetParameter, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public void Validate_LimitIsNull_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            var pagination = new Pagination { Offset = 0, Limit = null };

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.IncorrectLimitParameter, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public void Validate_LimitIsNegative_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            var pagination = new Pagination { Offset = 0, Limit = -1 };

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.IncorrectLimitParameter, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }

        [TestMethod]
        public void Validate_LimitIsZero_BadRequestResult()
        {
            //arrange
            BadRequestException exception = null;
            var pagination = new Pagination { Offset = 0, Limit = 0 };

            //act
            try
            {
                pagination.Validate();
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(ErrorMessages.IncorrectLimitParameter, exception.Message);
            Assert.AreEqual(ErrorCodes.BadRequest, exception.ErrorCode);
        }
    }
}
