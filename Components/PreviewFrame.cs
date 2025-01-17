﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WebMConverter
{
    public partial class PreviewFrame : UserControl
    {
        uint framenumber;
        FFMSSharp.Frame frame;
        int cachedframenumber;
        RotateFlipType rotateFlipType;

        [DefaultValue(0)]
        public int Frame
        {
            get { return (int)framenumber; }
            set { framenumber = (uint)value; GeneratePreview(); }
        }

        [DefaultValue(RotateFlipType.RotateNoneFlipNone)]
        public RotateFlipType RotateFlip
        {
            get { return rotateFlipType; }
            set { rotateFlipType = value; GeneratePreview(); }
        }

        public PreviewFrame()
        {
            if (Program.VideoSource != null)
            {
                // Prepare our "list" of accepted pixel formats
                List<int> pixelformat = new List<int>();
                pixelformat.Add(FFMSSharp.FFMS2.GetPixelFormat("bgra"));

                var infoframe = Program.VideoSource.GetFrame((int)framenumber);
                Program.VideoSource.SetOutputFormat(pixelformat, infoframe.EncodedResolution.Width, infoframe.EncodedResolution.Height, FFMSSharp.Resizer.BilinearFast);
            }

            cachedframenumber = -1;

            InitializeComponent();
        }

        public void GeneratePreview()
        {
            GeneratePreview(false);
        }

        public void GeneratePreview(bool force)
        {
            if (Program.VideoSource == null)
                return;

            if (force)
                cachedframenumber = -1;

            // Load the frame, if we haven't already
            if (cachedframenumber != framenumber)
            {
                cachedframenumber = (int)framenumber;
                frame = Program.VideoSource.GetFrame(cachedframenumber);
            }

            // Calculate width and height
            int width, height;
            float scale;
            scale = Math.Min((float)Size.Width / frame.EncodedResolution.Width, (float)Size.Height / frame.EncodedResolution.Height);
            width = (int)(frame.EncodedResolution.Width * scale);
            height = (int)(frame.EncodedResolution.Height * scale);

            // https://stackoverflow.com/a/24199315/174466
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            if (frame.EncodedResolution.Width > 2000)
                destImage.SetResolution(frame.EncodedResolution.Width / (float)2, frame.EncodedResolution.Height / (float)2);
            else
                destImage.SetResolution(frame.EncodedResolution.Width, frame.EncodedResolution.Height);

            using (var graphics = Graphics.FromImage(destImage))
            {
                if (frame.EncodedResolution.Width > 2000)
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.Bilinear;
                    graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                }
                else
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }


                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(frame.Bitmap, destRect, 0, 0, frame.EncodedResolution.Width, frame.EncodedResolution.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            destImage.RotateFlip(rotateFlipType);
            Picture.BackgroundImage = destImage;
            Picture.ClientSize = new Size(width, height);
            Picture.Refresh();

            // Center the pictureBox in our control
            if (width == Width || width - 1 == Width || width + 1 == Width) // this looks weird but keep in mind we're dealing with an ex float here
            {
                Padding = new Padding(0, (Height - height) / 2, 0, 0);
            }
            else
            {
                Padding = new Padding((Width - width) / 2, 0, 0, 0);
            }
        }

        void pictureBoxFrame_SizeChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        public string SavePreview(string directory, string name)
        {
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            Encoder myEncoder = Encoder.Quality;
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            string filename = $"{directory}//{name}-{framenumber}.jpg";
            frame.Bitmap.Save(filename, myImageCodecInfo, myEncoderParameters);
            return filename;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
