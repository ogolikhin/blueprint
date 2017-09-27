using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace BlueprintSys.RC.Services.MessageHandlers.Notifications
{
    public class LogoDataProvider
    {
        #region Fields, Properties and Constants

        private const string LandingPageCustomLogoKey = "LandingPageCustomLogo";
        private const string BasePath = "~/Images/";
        private const string BlueprintLogoFile = "Blueprint_logo_Transparent.png";

        private const int Width = 246;
        private const int Height = 79;

        // Cache the logo image.
        private static byte[] _logo;
        private static byte[] Logo
        {
            get { return _logo ?? (_logo = GetCustomLogo() ?? GetBlueprintLogo()); }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the custom logo.
        /// </summary>
        private static byte[] GetCustomLogo()
        {
            var value = ConfigurationManager.AppSettings[LandingPageCustomLogoKey];
            return GetLogoByImageName(value);
        }

        /// <summary>
        /// Gets Blueprint's logo.
        /// </summary>
        private static byte[] GetBlueprintLogo()
        {
            return GetLogoByImageName(BlueprintLogoFile);
        }

        /// <summary>
        /// Gets the logo. If the custom logo is not available, returns Blueprint's logo.
        /// </summary>
        public byte[] GetLogo()
        {
            return Logo;
        }

        #endregion

        #region Helpers

        private static byte[] GetLogoByImageName(string imageName)
        {
            if (String.IsNullOrWhiteSpace(imageName))
                return null;

            var isValid = imageName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
            if (!isValid)
                return null;

            var folderPath = Directory.GetCurrentDirectory();

            var imagePath = Path.Combine(folderPath, imageName);
            if (!File.Exists(imagePath))
                return null;

            return GetLogoByImagePath(imagePath);
        }

        private static byte[] GetLogoByImagePath(string imagePath)
        {
            var image = Image.FromFile(imagePath);
            image = FixImageSize(image);
            return ToByteArray(image);
        }

        private static byte[] ToByteArray(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static Image FixImageSize(Image image)
        {
            if (image.Width == Width && image.Height == Height)
                return image;

            return ResizeImage(Width, Height, image);
        }

        private static Image ResizeImage(int newWidth, int newHeight, Image imgPhoto)
        {
            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;

            var destX = 0;
            var destY = 0;
            float nPercent;
            var nPercentW = ((float)newWidth / sourceWidth);
            var nPercentH = ((float)newHeight / sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                //Uncomment the line below in order to center the logo.
                //destX = Convert.ToInt16((newWidth - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt16((newHeight - (sourceHeight * nPercent)) / 2);
            }

            var destWidth = (int)(sourceWidth * nPercent);
            var destHeight = (int)(sourceHeight * nPercent);


            var bmPhoto = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            var grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                              new Rectangle(destX, destY, destWidth, destHeight),
                              new Rectangle(0, 0, sourceWidth, sourceHeight),
                              GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();
            return bmPhoto;
        }

        #endregion
    }
}
