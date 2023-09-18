using System.Text;

namespace YTGsr
{
    public static class Util
    {
        public static string GetHourFromDateTime(DateTime dateTime)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dateTime.Hour.ToString());
            stringBuilder.Append(":");
            stringBuilder.Append(dateTime.Minute.ToString("00"));

            return stringBuilder.ToString();
        }


    }
}
