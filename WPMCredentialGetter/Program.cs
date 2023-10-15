using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using static Program;

class Program
{
    private static string _TenantId { get; set; } = "";
    private static string _PublicKey { get; set; } = File.ReadAllText(@"C:\pubkey.txt");
    private static string _CompoundedURL { get; set; } = "";
    static void Main()
    {
        Console.WriteLine("What is your tenant id?");
        _TenantId = Console.ReadLine() ?? throw new NullReferenceException();
        _CompoundedURL = $"https://{_TenantId}.id.cyberark.cloud";
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
        request.Timeout = 30000;

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
        //var client = new RestClient($"{_CompoundedURL}/UPRest/GetUPData?force=true");
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
        //var client = new RestClient($"{_CompoundedURL}/UPRest/GetMCFA?sItemKey=" + securedItem.ItemKey);
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
                //Console.WriteLine($"API Response: {responseContent}");
                CustomObject obj = JsonConvert.DeserializeObject<CustomObject>(responseContent);
                Console.WriteLine($"Your Information is: {obj.EncryptedSecuredItemResult.skey}");
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
    public class CustomObject
    {
        public bool success { get; set; }
        [JsonProperty("Result")]
        public EncryptedSecuredItemResult EncryptedSecuredItemResult { get; set; }
        public object Message { get; set; }
        public object MessageID { get; set; }
        public object Exception { get; set; }
        public object ErrorID { get; set; }
        public object ErrorCode { get; set; }
        public bool IsSoftError { get; set; }
        public object InnerExceptions { get; set; }
    }
    public class EncryptedSecuredItemResult
    {
        public string skey { get; set; }
        public object p { get; set; }
        public List<Ce> ce { get; set; }
        public string ne { get; set; }
        public string e { get; set; }
        public string iv { get; set; }
        public object n { get; set; }
        public string u { get; set; }
    }
    public class Ce
    {
        public object cfv { get; set; }
        public string cfv_e { get; set; }
        public string cfk { get; set; }
        public bool cfh { get; set; }
    }
}