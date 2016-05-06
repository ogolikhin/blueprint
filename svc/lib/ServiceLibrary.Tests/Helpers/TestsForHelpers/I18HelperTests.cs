using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Helpers.TestsForHelpers
{
    [TestClass]
    public class I18HelperTests
    {
        #region Private functions

        /// <summary>
        /// Asserts that the provided delegate throws an exception of type T.
        /// </summary>
        /// <typeparam name="T">The expected exception type.</typeparam>
        /// <param name="action">The delegate to test.</param>
        /// <param name="nameOfFunction">The name of the function being tested (to include in the assert message).</param>
        private static void AssertThrows<T>(Action action, string nameOfFunction) where T : Exception
        {
            try
            {
                // Act
                action();
            }
            // Assert
            catch (T)
            {
                // The test passed.
                return;
            }
            catch
            {
                Assert.Fail(nameOfFunction + " threw an exception other than ArgumentNullException!");
            }

            Assert.Fail(nameOfFunction + " didn't throw an exception when passing a null argument!");
        }

        #endregion Private functions

        [TestMethod]
        public void EndWithOrdinal_FirstArgumentNull_ThrowsArgumentNullException()
        {
            AssertThrows<ArgumentNullException>(() => I18NHelper.EndsWithOrdinal(null, "foo"), "I18NHelper.EndsWithOrdinal");
        }

        [TestMethod]
        public void EqualsOrdinalIgnoreCase_FirstArgumentNull_ThrowsArgumentNullException()
        {
            AssertThrows<ArgumentNullException>(() => I18NHelper.EndsWithOrdinal(null, "foo"), "I18NHelper.EndsWithOrdinal");
        }

        [TestMethod]
        public void ToInt32_NonNumericValue_ReturnsDefaultValue()
        {
            // Arrange
            const string s = "foo";
            const int defaultValue = 5;

            // Act
            int x = I18NHelper.ToInt32(s, defaultValue);

            // Assert
            Assert.AreEqual(x, 5, I18NHelper.FormatInvariant("I18NHelper.ToInt32 converted '{0}' into {1} instead of {2}!", s, x, defaultValue));
        }

        [TestMethod]
        public void ToStringInvariant_FirstArgumentNull_ThrowsArgumentNullException()
        {
            AssertThrows<ArgumentNullException>(() => I18NHelper.ToStringInvariant(null, "foo"), "I18NHelper.ToStringInvariant");
        }
    }
}
