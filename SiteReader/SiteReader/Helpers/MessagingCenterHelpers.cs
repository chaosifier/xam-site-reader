using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SiteReader.Helpers
{
    public static class MessagingCenterHelpers
    {
        public const string READ_REQUEST_MESSAGE = "read_request";
        public static void SendReadRequest(string url)
        {
            MessagingCenter.Send<string, string>("App", READ_REQUEST_MESSAGE, url);
        }
    }
}
