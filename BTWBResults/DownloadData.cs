using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BTWBResults
{
    public class DownloadData
    {
        private string address = @"https://beyondthewhiteboard.com/";
        private string FileName;

        public DownloadData(string filePath)
        {
            FileName = System.IO.Path.Combine(filePath, "BTWB_data.csv");
        }

        public bool Login(string username, string pass)
        {
            var cookieJar = new CookieContainer();
            CookieAwareWebClient client = new CookieAwareWebClient(cookieJar);

            // the website sets some cookie that is needed for login, and as well the 'authenticity_token' is always different
            string response = client.DownloadString(new Uri(address + "signin"));
            // parse the 'authenticity_token' and cookie is auto handled by the cookieContainer
            string token = Regex.Match(response, "authenticity_token.+?value=\"(.+?)\"").Groups[1].Value;
            string encodedToken = System.Web.HttpUtility.UrlEncode(token);
            string postData =
                string.Format("utf8=%E2%9C%93&authenticity_token={0}&login={1}&password={2}&commit=Sign+In", encodedToken, username, pass);
            //WebClient.UploadValues is equivalent of Http url-encode type post
            client.Method = "POST";
            response = client.UploadString(new Uri(address + "session"), postData);

            try
            {
                client.DownloadFile(new Uri(address + "members/164332/workout_sessions.csv"), FileName);
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("401"))
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
    }

}
