using ExactOnline.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ExactOnline.Client.Sdk;
using ExactOnline.Client.Sdk.Controllers;
using System.Web.Configuration;
using Web.Repository;
using Web.Models;

namespace Web.Service
{
    public interface IExactClientService
    {
        void initial();
        bool CreateDocument();
    }

    public class ExactClientService : IExactClientService
    {
        private ExactOnlineClient _client;
        private readonly ITokenRepository _tokenRepository;
        private string apiEndPoint = WebConfigurationManager.AppSettings["ExactOnlineEndpoint"];

        public ExactClientService(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;

        }

        public void initial()
        {
            if (_client == null)
            {
                _client = new ExactOnlineClient(apiEndPoint, _tokenRepository.GetAccessToken);
            }
        }

        public bool CreateDocument()
        {
            Document document = new Document
            {
                Subject = "User Acceptance Test Document",
                Body = "User Acceptance Test Document",
                Category = GetCategoryId(),
                Type = 55, //Miscellaneous
                DocumentDate = DateTime.Now.Date
            };

            bool success = _client.For<Document>().Insert(ref document);
            //documentId = document.ID;
            return success;
        }

        private Guid GetCategoryId()
        {
            var categories = _client.For<DocumentCategory>().Where("Description+eq+'General'").Get();
            var category = categories.First().ID;
            return category;
        }
    }

}