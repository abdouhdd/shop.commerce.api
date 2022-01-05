using System.IO;

namespace shop.commerce.api.services.Helpers
{
    public class HelperFile
    {
        public static string FullPathDirectoryImage(string directory, string slug)
        {
            return Path.Combine(directory, slug);
        }
        public static string FullPathImage(string directory, string filename)
        {
            return Path.Combine(directory, filename);
        }
        public static string SourceImage(string directory, string slug, string filename)
        {
            //string fullpath = Path.Combine("images", "products", "images", filename);
            //string fullpath = $"images/products/{slug}/{filename}";
            string fullpath = $"{slug}/{filename}";
            return fullpath;
        }
        //public static string SourceImage(string directory, string slug, string filename)
        //{
        //    string fullpath = Path.Combine(directory, slug, filename);
        //    if (!File.Exists(fullpath))
        //    {
        //        fullpath = Path.Combine(directory, "images", filename);
        //    }
        //    if (File.Exists(fullpath))
        //    {
        //        return $"data:image/jpeg;base64,{System.Convert.ToBase64String(File.ReadAllBytes(fullpath))}";
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}
    }
}
