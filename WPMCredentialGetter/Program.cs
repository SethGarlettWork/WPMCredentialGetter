using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using static Program;

class Program
{
    private static string _TenantId { get; set; } = "aay4551";
    private static string _CompoundedURL { get; set; } = $"https://{_TenantId}.id.cyberark.cloud";
    private static string _PublicKey { get; set; } = "-----BEGIN PUBLIC KEY-----MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhxvr6R7Ery9qOwYVHy01OeHD4vW5g5my7EVlz9M14umetopZ3hM9S6x6CST4Tp7OhNytGDvYIOyO48XCGrWwSGYbCfHw4A4a8B3PrvY3TGjsQXgLWxQHePd7ITPsZi4I35dLJFoLYq4X5lcvD3CJPllaziqRylgSfcDjGYBUpJp1v3NHz9A7ZUg2NUaBNrxxOq0PORDhnkfewVbEeg4vdFnubLhyk98L+kkkzrvHWBO4ChugNbbIQY9FM7hzWxHAtiV5GrBqYSh8iWkWO5felGIRlZ16ygyZ2OOgZ5GLGpPpWlaJIzot1liKWfRIBhU41XKqQ2ZfkDwf73oOwIDAQAB-----END PUBLIC KEY-----";
    static void Main()
    {
        GetSecuredItems();

        if (_SecuredItems.Count > 0)
        {
            int selectedIndex = 0;

            while (selectedIndex > -1)
            {
                for (int i = 0; i < _SecuredItems.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_SecuredItems[i].DisplayName}");
                }

                Console.Write("Enter the number of the item you want to select - Enter 0 to Exit: ");
                if (int.TryParse(Console.ReadLine(), out selectedIndex))
                {
                    selectedIndex--; // Adjust for 0-based index

                    if (selectedIndex < 0)
                    {
                        break;
                    }

                    var selectedItem = _SecuredItems[selectedIndex];
                    Console.WriteLine($"Get credentials for {selectedItem.DisplayName}");
                    var creds = GetSecuredItemCreds(selectedItem);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }
        }
        else
        {
            Console.WriteLine("No Results Found");
        }
    }
    private static List<SecuredItem> _SecuredItems { get; set; }
    static StartAuthDTO StartAuthentication()
    {
        var startAuthDTO = new StartAuthDTO();

        Console.WriteLine("Please enter your username:");
        var userName = Console.ReadLine();
        var client = new RestClient($"{_CompoundedURL}/Security/StartAuthentication");
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.Timeout = 30000; // 30 seconds timeout

        var payload = new
        {
            TenantID = _TenantId,
            User = userName,
            Version = "1.0"
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                //Console.WriteLine($"API Response: {responseContent}");
                JObject jsonObject = JObject.Parse(responseContent);
                startAuthDTO.SessionId = jsonObject.SelectToken("Result.SessionId").ToString();
                startAuthDTO.MechanismId = jsonObject.SelectToken("Result.Challenges[0].Mechanisms[0].MechanismId").ToString();
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        return startAuthDTO;
    }
    public static string _BearerToken { get; set; }
    static string GetBearerToken()
    {
        if (!string.IsNullOrEmpty(_BearerToken))
        {
            return _BearerToken;
        }

        var startAuthentication = StartAuthentication();

        Console.WriteLine("Please enter your password:");
        var password = Console.ReadLine();

        var client = new RestClient($"{_CompoundedURL}/Security/AdvanceAuthentication");
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");

        var payload = new
        {
            SessionId = startAuthentication.SessionId,
            MechanismId = startAuthentication.MechanismId,
            Action = "Answer",
            Answer = password
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                //Console.WriteLine($"API Response: {responseContent}");
                JObject jsonObject = JObject.Parse(responseContent);
                _BearerToken = jsonObject.SelectToken("Result.Token").ToString();
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        return _BearerToken;
    }
    static void GetSecuredItems()
    {
        var bearerToken = GetBearerToken();
        var client = new RestClient($"{_CompoundedURL}/UPRest/GetSecuredItemsData");
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {bearerToken}");

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                SecuredItemsResponse securedItemsResponse = JsonConvert.DeserializeObject<SecuredItemsResponse>(responseContent);
                _SecuredItems = new List<SecuredItem>();
                _SecuredItems = securedItemsResponse.Result.SecuredItems;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
    }
    static SelectedItemCreds GetSecuredItemCreds(SecuredItem securedItem)
    {
        var selectedItemCreds = new SelectedItemCreds();
        var bearerToken = GetBearerToken();
        var client = new RestClient($"{_CompoundedURL}/UPRest/GetCredsForSecuredItem?sItemKey=" + securedItem.ItemKey);
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {bearerToken}");

        var payload = new
        {
            publicKey = _PublicKey
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                Console.WriteLine($"API Response: {responseContent}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        return selectedItemCreds;
    }
    public class StartAuthDTO
    {
        [JsonProperty("Result.SessionId")]
        public string SessionId { get; set; }
        [JsonProperty("Result.Challenges[0].Mechanisms[0].MechanismId")]
        public string MechanismId { get; set; }
    }
    public class SecuredItemsResponse
    {
        public Result Result { get; set; }
    }
    public class Result
    {
        public List<SecuredItem> SecuredItems { get; set; }
    }
    public class SecuredItem
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ItemKey { get; set; }
        public string SecuredItemType { get; set; }
    }
    public class SelectedItemCreds
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Note { get; set; }
    }
}