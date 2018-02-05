using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HtmlLibrary.RichText
{
    [TestClass]
    public class HtmlHelperTests
    {
        [TestMethod]
        public void ToPlainText_HtmlIsNull_DefaultOptions_NoNewlineReplacement_ThrowsException()
        {
            // Arrange

            // Act
            var actual = HtmlHelper.ToPlainText(null);

            // Assert
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsNoText_ReturnsEmptyString()
        {
            // Arrange
            var expected = string.Empty;

            // Act
            var actual = HtmlHelper.ToPlainText(null, newLineReplacement: StringHelper.SpaceConstant);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsEmptyText_ReturnsEmptyString()
        {
            // Arrange
            var expected = string.Empty;

            // Act
            var actual = HtmlHelper.ToPlainText(string.Empty, newLineReplacement: StringHelper.SpaceConstant);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsWhitespace_ReturnsEmptyString()
        {
            // Arrange
            var expected = " ";

            // Act
            var actual = HtmlHelper.ToPlainText(" ", newLineReplacement: StringHelper.SpaceConstant);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsParagraph_ReturnsCorrectResult()
        {
            // Arrange
            const string text =
                "<html>" +
                "	<head/>" +
                "	<body style=\"padding: 1px 0px 0px; font-size: 10.6700000762939px; line-height: 1.45000004768372\">" +
                "		<p style=\"margin: 0px; font-size: 10.6700000762939px; line-height: 1.45000004768372\">" +
                "			<span style=\"font-size: 10.6700000762939px; line-height: 1.45\">Test</span>" +
                "		</p>" +
                "	</body>" +
                "</html>";
            var expected = "Test";

            // Act
            var actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsPreTag_ReturnsCorrectResult()
        {
            // Arrange
            const string text =
                "<html>" +
                "	<head/>" +
                "	<body style=\"padding: 1px 0px 0px; font-size: 10.6700000762939px; line-height: 1.45000004768372\">" +
                "		<pre>    testing string<br>" +
                "test" +
                "		</pre>" +
                "	</body>" +
                "</html>";

            var expected = "    testing string\r\ntest";

            // Act
            var actual = HtmlHelper.ToPlainText(text);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsTable_ReturnsCorrectResult()
        {
            const string text =
                "<html>" +
                "   <head/>" +
                "   <body style=\"padding: 1px 0px 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "      <div style=\"font-size: 11px; line-height: 1.45000004768372\">" +
                "         <table style=\"border-collapse: collapse; font-size: 11px; width: 100%; line-height: 1.45000004768372\">" +
                "            <tbody style=\"vertical-align: middle; font-size: 11px; line-height: 1.45000004768372\">" +
                "               <tr style=\"font-size: 11px; line-height: 1.45000004768372\">" +
                "                  <td style=\"border-style: solid; border-width: thin; padding: 0px 7px; background-color: #9accff; font-size: 11px; height: 16px; line-height: 1.45000004768372; width: 206px\">" +
                "                     <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "                        <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Example</span>" +
                "                     </p>" +
                "                  </td>" +
                "                  <td style=\"border-style: solid; border-width: thin; padding: 0px 7px; background-color: #9accff; font-size: 11px; height: 16px; line-height: 1.45000004768372; width: 763px\">" +
                "                     <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "                        <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Description</span>" +
                "                     </p>" +
                "                  </td>" +
                "                  <td style=\"border-style: solid; border-width: thin; padding: 0px 7px; background-color: #9accff; font-size: 11px; height: 16px; line-height: 1.45000004768372; width: 860px\">" +
                "                     <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "                        <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Where / Instructions</span>" +
                "                     </p>" +
                "                  </td>" +
                "               </tr>" +
                "            </tbody>" +
                "         </table><p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "            <span style=\"font-size: 11px; line-height: 1.45000004768372\">&#x200b;</span>" +
                "         </p>" +
                "      </div>" +
                "   </body>" +
                "</html>";

            // Assert
            var expected = "Example \r\n\tDescription \r\n\tWhere / Instructions";
            var actual = HtmlHelper.ToPlainText(text);
            Assert.AreEqual(expected, actual);

            // Using TextOnly option
            expected = "Example Description Where / Instructions";
            actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsEmptyText_ReturnsCorrectResult()
        {
            var expected = string.Empty;
            var actual = HtmlHelper.ToPlainText(null, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);

            var text = "<html></html>";
            actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);

            text =
                "<html>" +
                "   <head/>" +
                "   <body style=\"padding: 1px 0px 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "      <div style=\"font-size: 11px; line-height: 1.45000004768372\">" +
                "         <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\"> </span>" +
                "         </p>" +
                "         <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\"> </span>" +
                "         </p>" +
                "         <p style=\"margin: 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\"> </span>" +
                "         </p>" +
                "      </div>" +
                "   </body>" +
                "</html>";

            actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsUnorderedList_ReturnsCorrectResult()
        {
            var text =
                "<html>" +
                "   <head/>" +
                "   <body style=\"padding: 1px 0px 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "      <ul>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">First Item</span>" +
                "         </li>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Second Item</span>" +
                "         </li>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Third Item</span>" +
                "         </li>" +
                "      </ul>" +
                "   </body>" +
                "</html>";

            var expected = "First Item Second Item Third Item";
            var actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);

            expected = "•\tFirst Item \r\n•\tSecond Item \r\n•\tThird Item";
            actual = HtmlHelper.ToPlainText(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsOrderedList_ReturnsCorrectResult()
        {
            var text =
                @"<html>" +
                "   <head/>" +
                "   <body style=\"padding: 1px 0px 0px; font-size: 11px; line-height: 1.45000004768372\">" +
                "      <ol>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">First Item</span>" +
                "         </li>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Second Item</span>" +
                "         </li>" +
                "         <li>" +
                "            <span style=\"font-size: 11px; font-weight: bold; line-height: 1.45000004768372\">Third Item</span>" +
                "         </li>" +
                "      </ol>" +
                "   </body>" +
                "</html>";

            var expected = "First Item Second Item Third Item";
            var actual = HtmlHelper.ToPlainText(text, PlainTextOptions.TextOnly);
            Assert.AreEqual(expected, actual);

            expected = "1.\tFirst Item \r\n2.\tSecond Item \r\n3.\tThird Item";
            actual = HtmlHelper.ToPlainText(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsLineBreakAndParagraphs_ReturnsCorrectResult()
        {
            // Arrange
            const string text =
                @"<html>
                    <head/>
                    <body><span><br></span>
                        <p>
                            Paragraph 1
                        </p>
                        <p>Paragraph 2</p>
                    </body>
                </html>";

            var expected = "\r\nParagraph 1 \r\nParagraph 2";

            // Act
            var actual = HtmlHelper.ToPlainText(text);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_ContainsParagraphInTableCell_ReturnsCorrectResult()
        {
            // Arrange
            const string text =
                @"<html>
                    <body>
                        <table>
                        <tr>
                            <td><p>One</p></td>
                            <td><p>Two</p></td>
                          </tr>
                          <tr>
                            <td><p>Three</p></td>
                            <td><p>Four</p></td>
                          </tr>
                          </table>
                    </body>
                    </html>";

            var expected = "One\tTwo\t\r\nThree\tFour";

            // Act
            var actual = HtmlHelper.ToPlainText(text);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToPlainText_NewlineReplacement_ReturnsCorrectResult()
        {
            // Arrange
            const string text =
                @"<html>
                    <body>
                        <table>
                        <tr>
                            <td><p>One</p></td>
                            <td><p>Two</p></td>
                          </tr>
                          <tr>
                            <td><p>Three</p></td>
                            <td><p>Four</p></td>
                          </tr>
                          </table>
                          <p><h2>This is a test</h2></p>
                    </body>
                    </html>";

            var expected = "One\tTwo\t Three\tFour\t This is a test ";

            // Act
            var actual = HtmlHelper.ToPlainText(text, newLineReplacement: StringHelper.SpaceConstant);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
