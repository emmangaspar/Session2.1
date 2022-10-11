using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace Session2._1_Assignment
{
    [TestClass]
    public class HttpClientTests
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string UsersEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{UsersEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data

            // Create Json Object
            PetModel petModel = new PetModel()
            {
                Id = 1234,
                Name = "doggie",
                PhotoUrls = new string[] { "https://www.chewy.com/" },
                Category = new Category { Id = 1234, Name = "Dogs" },
                Tags = new Category[] { new Category { Id = 1234, Name = "Brown" } },
                Status = "available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petModel);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(UsersEndpoint), postRequest);

            #endregion

            #region get Name of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{petModel.Id}"));

            // Deserialize Content
            var listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var petData = listUserData.Name;

            #endregion

            #region send put request to update data

            // Update value of userData
            petModel = new PetModel()
            {
                Id = 4567,
                Name = "doggieupdated",
                PhotoUrls = new string[] { "https://www.chewy.com/" },
                Category = new Category { Id = 4567, Name = "Dogs" },
                Tags = new Category[] { new Category { Id = 4567, Name = "Brown" } },
                Status = "unavailable"
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petModel);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{UsersEndpoint}"), postRequest);

            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{petModel.Id}"));

            // Deserialize Content
            listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");
            Assert.AreEqual(petModel.Id, listUserData.Id, "Pet Id is successfully updated");
            Assert.AreEqual(petModel.Name, listUserData.Name, "Pet Name is successfully updated");
            Assert.AreNotEqual(petModel.Tags, listUserData.Tags, "Pet Tag is not successfully updated");
            Assert.AreEqual(petModel.Status, listUserData.Status, "Pet Status is successfully updated");


            #endregion

        }
    }
}