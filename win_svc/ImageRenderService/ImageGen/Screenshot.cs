﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageRenderService.ImageGen
{
    public class Screenshot : IScreenshot
    {
        public Screenshot(Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException();
            }
            Image = image;
        }

        public void Save(Stream stream, ImageFormat format)
        {
            Image.Save(stream, format);
        }
        public Bitmap Image { get; set; }
        public virtual int Width => Image.Width;
        public virtual int Height => Image.Height;
        public void Dispose()
        {
            Image.Dispose();
        }
    }
}