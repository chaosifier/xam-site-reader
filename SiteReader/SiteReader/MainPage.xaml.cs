using SiteReader.Helpers;
using SiteReader.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SiteReader
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public ReaderViewModel TheViewModel = new ReaderViewModel();
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = TheViewModel;

            MessagingCenter.Subscribe<string, string>("App", MessagingCenterHelpers.READ_REQUEST_MESSAGE, readNewUrl);
        }

        public void readNewUrl(string sender, string url)
        {
            TheViewModel.Url = url;
        }
    }
}
