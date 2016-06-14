using CustomAttributes;
using Helper;
using Model;
using NUnit.Framework;
using TestCommon;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.FileStore)]
    public class NonAsciiFilenames : TestBase
    {
        private const string ARABIC_CHARS = "غ	ظ	ض	ذ	خ	ث	ت	ش	ر	ق	ص	ف	ع	س	ن	م	ل	ك	ي	ط	ح	ز	و	ه	د	ج	ب	ا";
        private const string FRENCH_CHARS = "ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿ";
        private const string GERMAN_CHARS = "ÄÖÜäöüß";
        private const string JAPANESE_HIRAGANA_CHARS = "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよわん";
        private const string JAPANESE_KANJI_CHARS = "亜哀愛悪握圧扱安案暗以衣位囲医依委威胃為尉異移偉意違維慰遺緯域育";
        private const string KOREAN_HANGUL_CHARS = "ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎㅏㅓㅗㅜㅡㅣㅑㅕㅛㅠㄲㄸㅃㅆㅉㄳㄵㄶㄺㄻㄼㄽㄾㄿㅀㅄㅐㅒㅔㅖㅢㅘㅙㅚㅝㅞㅟ";
        private const string RUSSIAN_CHARS = "ЗэыяёюиЛДПБГЧЙЖШЮЦЩФЁЪ";
        private const string SIMPLIFIED_CHINESE_CHARS = "安吧爸八百北不大岛的弟地东都对多儿二方港哥个关贵国过海好很会家见叫姐京九可老李零六吗妈么没美妹们明名哪那南你您朋七起千去人认日三上谁什生师识十是四他她台天湾万王我五西息系先香想小谢姓休学也一亿英友月再张这中字";
        private const string SPANISH_CHARS = "áéíóúñÑüÜ¿¡ÁÉÍÓÚç";
        private const string SPECIAL_CHARS = "©®™~!@#$%^&*()_¢€£¤¥¦§¨ª«»¬¯°±¹²³´µ¶·¸º¼½¾¿";
        private const string TRADITIONAL_CHINESE_CHARS = "電買開東車紅馬無鳥熱時語假罐佛德拜黑冰兔妒壤每步";

        private IUser _user;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken);
        }

        [TestFixtureTearDown]
        public void ClassTearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Post tests

        [TestCase(nameof(ARABIC_CHARS), ARABIC_CHARS)]
        [TestCase(nameof(FRENCH_CHARS), FRENCH_CHARS)]
        [TestCase(nameof(GERMAN_CHARS), GERMAN_CHARS)]
        [TestCase(nameof(JAPANESE_HIRAGANA_CHARS), JAPANESE_HIRAGANA_CHARS)]
        [TestCase(nameof(JAPANESE_KANJI_CHARS), JAPANESE_KANJI_CHARS)]
        [TestCase(nameof(KOREAN_HANGUL_CHARS), KOREAN_HANGUL_CHARS)]
        [TestCase(nameof(RUSSIAN_CHARS), RUSSIAN_CHARS)]
        [TestCase(nameof(SIMPLIFIED_CHINESE_CHARS), SIMPLIFIED_CHINESE_CHARS)]
        [TestCase(nameof(SPANISH_CHARS), SPANISH_CHARS)]
        [TestCase(nameof(SPECIAL_CHARS), SPECIAL_CHARS)]
        [TestCase(nameof(TRADITIONAL_CHINESE_CHARS), TRADITIONAL_CHINESE_CHARS)]
        [TestRail(134136)]
        [Description("POST a file with a non-ASCII filename to FileStore (using Multipart-mime), then GET the file and compare against what we sent.")]
        public void PostFileWithMultiPartMime_VerifyFileExists(string charSet, string fakeFileName)
        {
            PostFile_VerifyFileExists(charSet, fakeFileName, useMultiPartMime: true);
        }

        [TestCase(nameof(ARABIC_CHARS), ARABIC_CHARS)]
        [TestCase(nameof(FRENCH_CHARS), FRENCH_CHARS)]
        [TestCase(nameof(GERMAN_CHARS), GERMAN_CHARS)]
        [TestCase(nameof(JAPANESE_HIRAGANA_CHARS), JAPANESE_HIRAGANA_CHARS)]
        [TestCase(nameof(JAPANESE_KANJI_CHARS), JAPANESE_KANJI_CHARS)]
        [TestCase(nameof(KOREAN_HANGUL_CHARS), KOREAN_HANGUL_CHARS)]
        [TestCase(nameof(RUSSIAN_CHARS), RUSSIAN_CHARS)]
        [TestCase(nameof(SIMPLIFIED_CHINESE_CHARS), SIMPLIFIED_CHINESE_CHARS)]
        [TestCase(nameof(SPANISH_CHARS), SPANISH_CHARS)]
        [TestCase(nameof(SPECIAL_CHARS), SPECIAL_CHARS)]
        [TestCase(nameof(TRADITIONAL_CHINESE_CHARS), TRADITIONAL_CHINESE_CHARS)]
        [TestRail(134137)]
        [Description("POST a file with a non-ASCII filename to FileStore (not using Multipart-mime), then GET the file and compare against what we sent.")]
        public void PostFileWithoutMultiPartMime_VerifyFileExists(string charSet, string fakeFileName)
        {
            PostFile_VerifyFileExists(charSet, fakeFileName, useMultiPartMime: false);
        }

        /// <summary>
        /// Posts a file to FileStore using the specified filename, then gets the file and verifies it matches with what we sent.
        /// </summary>
        /// <param name="charSet">The name of the character set in the filename.</param>
        /// <param name="fakeFileName">The filename to use.</param>
        /// <param name="useMultiPartMime">Specifies whether or not to use Multipart-mime.</param>
        private void PostFile_VerifyFileExists(string charSet, string fakeFileName, bool useMultiPartMime)
        {
            uint fileSize = 0;
            const string fileType = "text/plain";

            // Setup: Create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);
            IFile storedFile = null;

            // Execute: Post the file to Filestore.
            Assert.DoesNotThrow(() =>
            {
                storedFile = Helper.FileStore.PostFile(file, _user, useMultiPartMime: useMultiPartMime);
            }, "FileStore POST failed for file with {0} characters ({1} multipart-mime).", charSet, (useMultiPartMime ? "with" : "without"));

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        #endregion Post tests

        #region Put tests

        [TestCase(nameof(ARABIC_CHARS), ARABIC_CHARS)]
        [TestCase(nameof(FRENCH_CHARS), FRENCH_CHARS)]
        [TestCase(nameof(GERMAN_CHARS), GERMAN_CHARS)]
        [TestCase(nameof(JAPANESE_HIRAGANA_CHARS), JAPANESE_HIRAGANA_CHARS)]
        [TestCase(nameof(JAPANESE_KANJI_CHARS), JAPANESE_KANJI_CHARS)]
        [TestCase(nameof(KOREAN_HANGUL_CHARS), KOREAN_HANGUL_CHARS)]
        [TestCase(nameof(RUSSIAN_CHARS), RUSSIAN_CHARS)]
        [TestCase(nameof(SIMPLIFIED_CHINESE_CHARS), SIMPLIFIED_CHINESE_CHARS)]
        [TestCase(nameof(SPANISH_CHARS), SPANISH_CHARS)]
        [TestCase(nameof(SPECIAL_CHARS), SPECIAL_CHARS)]
        [TestCase(nameof(TRADITIONAL_CHINESE_CHARS), TRADITIONAL_CHINESE_CHARS)]
        [TestRail(134138)]
        [Description("POST and then PUT a file with a non-ASCII filename to FileStore (using Multipart-mime), then GET the file and compare against what we sent.")]
        public void PutFileWithMultiPartMime_VerifyFileExists(string charSet, string fakeFileName)
        {
            PutFile_VerifyFileExists(charSet, fakeFileName, useMultiPartMime: true);
        }

        [TestCase(nameof(ARABIC_CHARS), ARABIC_CHARS)]
        [TestCase(nameof(FRENCH_CHARS), FRENCH_CHARS)]
        [TestCase(nameof(GERMAN_CHARS), GERMAN_CHARS)]
        [TestCase(nameof(JAPANESE_HIRAGANA_CHARS), JAPANESE_HIRAGANA_CHARS)]
        [TestCase(nameof(JAPANESE_KANJI_CHARS), JAPANESE_KANJI_CHARS)]
        [TestCase(nameof(KOREAN_HANGUL_CHARS), KOREAN_HANGUL_CHARS)]
        [TestCase(nameof(RUSSIAN_CHARS), RUSSIAN_CHARS)]
        [TestCase(nameof(SIMPLIFIED_CHINESE_CHARS), SIMPLIFIED_CHINESE_CHARS)]
        [TestCase(nameof(SPANISH_CHARS), SPANISH_CHARS)]
        [TestCase(nameof(SPECIAL_CHARS), SPECIAL_CHARS)]
        [TestCase(nameof(TRADITIONAL_CHINESE_CHARS), TRADITIONAL_CHINESE_CHARS)]
        [TestRail(134139)]
        [Description("POST and then PUT a file with a non-ASCII filename to FileStore (not using Multipart-mime), then GET the file and compare against what we sent.")]
        public void PutFileWithoutMultiPartMime_VerifyFileExists(string charSet, string fakeFileName)
        {
            PutFile_VerifyFileExists(charSet, fakeFileName, useMultiPartMime: false);
        }

        /// <summary>
        /// Posts and Puts a file to FileStore using the specified filename, then gets the file and verifies it matches with what we sent.
        /// </summary>
        /// <param name="charSet">The name of the character set in the filename.</param>
        /// <param name="fakeFileName">The filename to use.</param>
        /// <param name="useMultiPartMime">Specifies whether or not to use Multipart-mime.</param>
        private void PutFile_VerifyFileExists(string charSet, string fakeFileName, bool useMultiPartMime)
        {
            uint fileSize = 0;
            const string fileType = "text/plain";

            // Setup: Create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fakeFileName, fileType);
            IFile storedFile = null;

            // Post the file to Filestore.
            Assert.DoesNotThrow(() =>
            {
                storedFile = Helper.FileStore.PostFile(file, _user, useMultiPartMime: useMultiPartMime);
            }, "FileStore POST failed for file with {0} characters ({1} multipart-mime).", charSet, (useMultiPartMime ? "with" : "without"));

            // Execute: Put the file chunk to FileStore.
            Assert.DoesNotThrow(() =>
            {
                storedFile = Helper.FileStore.PutFile(file, new byte[] { }, _user, useMultiPartMime);
            }, "FileStore PUT failed for file with {0} characters ({1} multipart-mime).", charSet, (useMultiPartMime ? "with" : "without"));

            FileStoreTestHelper.AssertFilesAreIdentical(file, storedFile, compareIds: false);

            // Verify: that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = Helper.FileStore.GetFile(storedFile.Id, _user);

            FileStoreTestHelper.AssertFilesAreIdentical(storedFile, returnedFile);
        }

        #endregion Put tests
    }
}
