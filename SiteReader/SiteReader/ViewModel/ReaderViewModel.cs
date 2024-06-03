using HtmlAgilityPack;
using Plugin.TextToSpeech;
using Plugin.TextToSpeech.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace SiteReader.ViewModel
{
    public class ReaderViewModel : ObservableBase
    {
        private CancellationTokenSource _cTokenSrc;
        public ReaderViewModel()
        {
            this.PropertyChanged += ReaderViewModel_PropertyChanged;

            ToggleRead = new Command(() =>
            {
                Reading = !Reading;
            });
        }

        private async void ReaderViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Url))
            {
                await loadDataAsync();
            }
            else if (e.PropertyName == nameof(Reading))
            {
                if (Reading)
                {
                    _cTokenSrc = new CancellationTokenSource();
                    await read(Title);
                    await Task.Delay(1000);
                    await read(TextToRead);
                }
                else
                {
                    _cTokenSrc?.Cancel();
                }
            }
        }

        public ICommand ToggleRead { get; set; }

        private string _url;
        public string Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        private string _title = "Share news to read.";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private string _textToRead;
        public string TextToRead
        {
            get { return _textToRead; }
            set { SetProperty(ref _textToRead, value); }
        }

        private bool _reading;
        public bool Reading
        {
            get { return _reading; }
            set { SetProperty(ref _reading, value); }
        }

        private bool _ready;
        public bool Ready
        {
            get { return _ready; }
            set { SetProperty(ref _ready, value); }
        }

        private float _pitch = 1;
        public float Pitch
        {
            get { return _pitch; }
            set { SetProperty(ref _pitch, value); }
        }

        private float _speed = 1;
        public float Speed
        {
            get { return _speed; }
            set { SetProperty(ref _speed, value); }
        }

        private float _volume = 1;
        public float Volume
        {
            get { return _volume; }
            set { SetProperty(ref _volume, value); }
        }

        private async Task loadDataAsync()
        {
            this.Ready = false;

            // stop reading
            Reading = false;

            if (string.IsNullOrWhiteSpace(Url))
            {
                Title = string.Empty;
                TextToRead = string.Empty;
                Status = "Invaild url.";
                return;
            }

            try
            {
                // fetch data
                Status = "Fetching data...";
                var htmlData = await getHtmlAsync(Url);

                // extract text
                Status = "Extracting text...";
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlData);
                Title = getNewsTitle(htmlDoc);
                TextToRead = getNewsText(htmlDoc.DocumentNode, _url.ToLowerInvariant());

                Ready = true;

                // read
                Status = "Reading...";
                Reading = true;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
            }
        }

        private async Task read(string text)
        {
            try
            {
                var locales = await CrossTextToSpeech.Current.GetInstalledLanguages();
                var items = locales.Select(a => a.ToString()).ToArray();
                var nepLocal = locales.Where(l => l.ToString() == "ne-NP").FirstOrDefault();

                await CrossTextToSpeech.Current.Speak(text,
                        pitch: Pitch,
                        speakRate: Speed,
                        volume: Volume,
                        crossLocale: nepLocal,
                        cancelToken: _cTokenSrc.Token);
            }
            catch (Exception ex)
            {
                Reading = false;
                Status = ex.Message;
            }
        }

        private string getNewsTitle(HtmlDocument html)
        {
            return html.DocumentNode.SelectSingleNode("//head/title").InnerText;
        }

        private string getNewsText(HtmlNode doc, string url)
        {
            StringBuilder newsSb = new StringBuilder();
            if (url.Contains("onlinekhabar.com"))
            {
                var contentNodes = doc.SelectNodes("//div[contains(@class, 'main__read--content')]");

                foreach (var node in contentNodes)
                {
                    newsSb.Append(getNewsText(node, string.Empty));
                    newsSb.Append(Environment.NewLine);
                }
            }
            else
            {
                foreach (HtmlNode node in doc.SelectNodes("//p"))
                {
                    newsSb.Append(node.InnerText);
                    newsSb.Append(Environment.NewLine);
                }
            }

            return newsSb.ToString();
        }

        private async Task<string> getHtmlAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var data = await client.GetStringAsync(url);

                return data;
            }
        }
    }
}
