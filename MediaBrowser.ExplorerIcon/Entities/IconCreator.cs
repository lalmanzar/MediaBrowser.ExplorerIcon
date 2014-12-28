using System.Drawing;
using System.IO;

namespace MediaBrowser.ExplorerIcon.Entities
{
    public class IconCreator
    {
        public static void CreateIcon(Stream source, string destinationFile)
        {
            var originalImage = Image.FromStream(source);
            PngIconConverter.Convert(ConvertToIcon(originalImage), destinationFile, 256);
        }

        private static Image ConvertToIcon(Image newImage)
        {
            Image icon = new Bitmap(256, 256);
            var width = 256;
            var height = 256;
            if (newImage.Width > newImage.Height)
            {
                height = (int) (((float) newImage.Height)/newImage.Width*256);
            }
            else
            {
                width = (int) (((float) newImage.Width)/newImage.Height*256);
            }
            Image iconpng = new Bitmap(newImage, width, height);
            using (var gr = Graphics.FromImage(icon))
            {
                gr.DrawImage(iconpng, ((float) 256 - width)/2, ((float) 256 - height)/2);
            }
            return icon;
        }
    }
}