﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Capgemini.StudioReceptionist.ServiceConsumer.ACS;
using Capgemini.StudioReceptionist.BL.WebApi.Controllers;
using Capgemini.StudioReceptionist.Entities;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Capgemini.StudioReceptionist.BL.WebApi.Controllers
{
    public class ACSController : ApiController
    {
        SPOController spo;
        //private string imageString = string.Empty;
        private AzureCognitiveServicesServiceConsumer.WrapperPerson acs = new AzureCognitiveServicesServiceConsumer.WrapperPerson();
        public ACSController()
        {
            spo = new SPOController();
        }

        [HttpPost]
        [Route("api/ACS/DetectAndIdentifyFace")]
        public HttpResponseMessage InitialRequest(HttpRequestMessage request)
        {
            //Parse Json
            string response = string.Empty;
            string body = request.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<dynamic>(body);
            string imageString = json.image;

            string personId = IdentifyPerson(imageString);

            //If unable to identify person.
            if (string.IsNullOrEmpty(personId))
            {
                response = "{\"checkedIn\":\"false\", \"registered\":\"false\"}";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

            //Fetch guest data from SPO
            Guest guest = spo.FetchUserData(personId);

            if (spo.GuestCheckedIn(guest.Email))
            {
                response = "{\"checkedIn\":\"true\", \"registered\":\"true\",\"email\":\"" + guest.Email + "\"}";
            return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            else
            {
                response = "{\"checkedIn\":\"false\", \"registered\":\"true\", " +
                    "\"email\":\"" + guest.Email + "\"," +
                    "\"firstName\":\"" + guest.FirstName + "\"," +
                    "\"lastName\":\"" + guest.LastName + "\"," +
                    "\"company\":\"" + guest.Company + "\"" +
                    "}";
                return Request.CreateResponse(HttpStatusCode.OK, response); 
            }
        }

        [HttpPost]
        [Route("api/ACS/RegisterRequest/AddPerson")]
        public HttpResponseMessage RegisterRequest(HttpRequestMessage request)
        {
            string body = request.Content.ReadAsStringAsync().Result;
            Guest guest = JsonConvert.DeserializeObject<Guest>(body);
            //Create new person in person group in Azure.
            string personId = CreatePerson(guest);
            //Save the user to SPO with the relevant faceId.
            spo.SaveUser(guest, personId);

            string response = "{\"personId\":\"" + personId + "\"}"; 
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [Route("api/ACS/RegisterRequest/AddFaceToPerson")]
        public HttpResponseMessage RegisterRequestAddFaceToPerson(HttpRequestMessage request)
        {
            string body = request.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<dynamic>(body);
            string personId = json.personId;
            string image = json.image;
            //Add a face to the newly created person in Azure.
            string persistedFaceId = AddFaceToPerson(personId, image);

            //Train the PersonGroup to be able to recognize the person.
            string trainingStatus = TrainPersonGroup(); //TODO, use the response to determine if the training started. 

            //TODO, see if the training has ended/was successfull, as the TrainPersonGroup is async and is not waited for.

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [HttpPost]
        [Route("api/ACS/CheckIn")]
        public HttpResponseMessage CheckIn(HttpRequestMessage request)
        {
            string body = request.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<dynamic>(body);
            
            //Request contains email and host.
            string email = json.email;
            string host = json.personToVisit;

            spo.CreateLogEntry(email, host);

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [HttpPost]
        [Route("api/ACS/CheckOut")]
        public HttpResponseMessage CheckOut(HttpRequestMessage request)
        {
            string body = request.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<dynamic>(body);

            string email = json.email;

            //Find current Log by email and check out.
            spo.CheckOutLogEntry(email);

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        public string DetectFace(string imageString)
        {
            
            Task<string> response = acs.AzureDetectFace(imageString);
            response.Wait();
            return response.Result;
        }

        public string IdentifyPerson(string imageString)
        {
            Task<string> response = acs.AzureIdentifyPerson(DetectFace(imageString));
            response.Wait();
            return response.Result;
        }

        public string CreatePerson(Guest guest)
        {
            //Creating person with email adress as Id in Azure.
            Task<string> personId = acs.AzureCreatePersonInGroup(guest.Email);
            personId.Wait();
            return personId.Result;
        }

        public string TrainPersonGroup()
        {
            //Creating person with email adress as Id in Azure.
            Task<string> response = acs.AzureTrainPersonGroup();
            response.Wait();
            return response.Result;
        }

        public string AddFaceToPerson(string personId, string image)
        {
            Task<string> response = acs.AzureAddFaceForPerson(personId, image);
            try
            {
                response.Wait();
            }
            catch (Exception ex)
            {
                Debug.Write("Inner exception: "+ex.InnerException.Message, "Exception: "+ex.Message);
                throw;
            }
            return response.Result;
        }
    }
}
