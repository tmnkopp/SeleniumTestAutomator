using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Net;

namespace CyberBalance.CS.Core
{
    public interface IFeedProvider<T>
    {
        T GetFeed { get; }
        IFeedProvider<T> SetDeserializer(Func<string, T> Deserializer);
    }
    public class FeedProvider<T> : IFeedProvider<T>
    {
        private readonly string feedUri;
        public FeedProvider(string Uri)
        {
            this.feedUri = Uri;
        }
        #region PROPS  
        private T _feed;
        public T GetFeed
        {
            get
            {
                if (_feed == null) _feed = LoadFeed();
                return _feed;
            }
        }
        #endregion

        #region METHODS

        private T LoadFeed()
        {
            if (string.IsNullOrWhiteSpace(feedUri))
                throw new Exception($"Feed url cannot be null");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
 
            try
            {
                using (var webClient = new WebClient())
                {
                    var jsonData = webClient.DownloadString(feedUri);
                    return this.Deserializer(jsonData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Feed Load Error {feedUri} {ex.Message} {ex.InnerException}");
            } 
            return default(T);
        }
        private Func<string, T> Deserializer { get; set; } = (json) =>
        {
            return JsonConvert.DeserializeObject<T>(json);
        };
        public IFeedProvider<T> SetDeserializer(Func<string, T> Deserializer)
        {
            this.Deserializer = Deserializer;
            return this;
        }
        #endregion
    }

}
