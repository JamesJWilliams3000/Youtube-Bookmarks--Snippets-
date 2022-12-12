using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YotubeAPIExtras
{
    public class Video
    {
        public Video(DateTime date, string title, string id)
        {
            Date = date;
            Title = title;
            VideoID = id;
        }

        public Video(string id)
        {
            VideoID = id;
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set
            {
                date = value;
                SetShortcutName();
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = RemoveInvalidChars(value);
                SetShortcutName();
            }
        }

        public string VideoID { get; set; }
        public string ShortcutName { get; set; }

        public string URL => "https://www.youtube.com/watch?v=" + VideoID;

        private void SetShortcutName()
        {
            ShortcutName = string.Format("[{0}] {1}.url", Date.ToString("yyyy-MM-dd"), Title);
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} - {2}", Date.ToString("yyyy/MM/dd"), Title, VideoID);
        }

        private string RemoveInvalidChars(string filename)
        {
            char[] spaces = { '\\', '*', '?', '"' };

            filename = filename.Replace("\u0026", "&").Replace(spaces, " ").Replace(":", " -").Replace("/", "or").Replace('<', '(').Replace('>', ')').Replace('|', '-');
            return filename;
        }
    }

    public static class ExtensionMethods
    {
        public static string Replace(this string s, char[] separators, string newVal)
        {
            string[] temp;

            temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return String.Join(newVal, temp);
        }
    }
}
