using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ExtendedStorage.Utils
{
    public static class ImageUtility
    {
        /// <summary>
        /// Try to get an ImageSource from EStorageFile
        /// </summary>
        /// <param name="source">The source EStorageFile</param>
        /// <returns>Returns an ImageSource if success, else null.</returns>
        public static async Task<ImageSource> TryGetImage(this EStorageFile source)
        {
            if (source == null) return null;

            try
            {
                using (Stream stream = source.OpenAsStream(FileAccess.Read))
                {
                    BitmapImage result = new BitmapImage();
                    await result.SetSourceAsync(stream.AsRandomAccessStream());
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
