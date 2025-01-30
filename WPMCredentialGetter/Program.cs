using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
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

        Console.WriteLine("Select 1 for App Creds, 2 for Secured Items:");
        var answer = Console.ReadLine();
        
        if(answer == "1")
        {
            GetApplications();
            SetApplications();
        }
        else
        {
            GetSecuredItems();
            SetSecuredItems();
        }
    }
    private static void SetSecuredItems()
    {
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
    private static void SetApplications()
    {
        if (_Apps.Count > 0)
        {
            int selectedIndex = 0;

            while (selectedIndex > -1)
            {
                for (int i = 0; i < _Apps.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_Apps[i].DisplayName}");
                }

                Console.Write("Enter the number of the item you want to select - Enter 0 to Exit: ");
                if (int.TryParse(Console.ReadLine(), out selectedIndex))
                {
                    selectedIndex--; // Adjust for 0-based index

                    if (selectedIndex < 0)
                    {
                        break;
                    }

                    var selectedItem = _Apps[selectedIndex];
                    Console.WriteLine($"Get credentials for {selectedItem.DisplayName}");
                    var creds = GetAppCreds(selectedItem.AppKey);
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
    static List<App> _Apps { get; set; }
    static void GetApplications()
    {
        var bearerToken = GetBearerToken();
        var client = new RestClient($"{_CompoundedURL}/UPRest/GetUPData?force=true");
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {bearerToken}");

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                Root root = JsonConvert.DeserializeObject<Root>(responseContent);
                _Apps = new List<App>();
                _Apps = root.Result.Apps;
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
    static SelectedItemCreds GetAppCreds(string appkey)
    {
        var selectedItemCreds = new SelectedItemCreds();
        var bearerToken = GetBearerToken();
        var client = new RestClient($"{_CompoundedURL}/UPRest/GetMCFA?appkey=" + appkey);
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {bearerToken}");

        var payload = new { };
        string jsonPayload = JsonConvert.SerializeObject(payload);
        request.AddParameter("application/json", jsonPayload, ParameterType.RequestBody);

        try
        {
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseContent = response.Content;
                AppCredReturn obj = JsonConvert.DeserializeObject<AppCredReturn>(responseContent) ?? throw new NullReferenceException();
                if(obj.Success)
                {
                    Console.WriteLine($"Your UserName is: {obj.Result.u}");
                    Console.WriteLine($"Your Password is: {obj.Result.p}");
                }
                else
                {
                    Console.WriteLine("No Credentials Found");
                }
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
        //var client = new RestClient($"{_CompoundedURL}/UPRest/GetMCFA?appkey=" + "@/home/5ae4b83d-7d83-4c9d-b9b7-c526fe78b424/apps/UPS"); //securedItem.ItemKey);
        var client = new RestClient($"{_CompoundedURL}/UPRest/GetCredsForSecuredItem?sItemKey=" + securedItem.ItemKey);
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Authorization", $"Bearer {bearerToken}");

        var payload = new
        {
            //publicKey = _PublicKey
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
                CustomObject obj = JsonConvert.DeserializeObject<CustomObject>(responseContent) ?? throw new NullReferenceException();
                Console.WriteLine($"Your Information is: {obj.EncryptedSecuredItemResult.skey}");
                //Console.WriteLine($"Your decrypted inforamtion is: {DecryptSKey(obj.EncryptedSecuredItemResult.skey, obj.EncryptedSecuredItemResult.iv)}");
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
    public static string DecryptSKey(string skey, string iv)
    {
        try
        {
            // Convert the base64 encoded skey string to a byte array
            byte[] encryptedBytes = Convert.FromBase64String(skey);

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                var privKey = File.ReadAllText("C:/privKey.txt");

                aesAlg.Key = Encoding.UTF8.GetBytes(privKey); // Set the decryption key
                // Initialize the IV (Initialization Vector) - you should have this information
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams to process the decryption
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream and return as a string
                            string decryptedText = srDecrypt.ReadToEnd();
                            return decryptedText;
                        }
                    }
                }
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("Decryption error: " + ex.Message);
            return null;
        }
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
    public class App
    {
        public string CBEScript { get; set; }
        public bool Intranet { get; set; }
        public bool PasswordIsSet { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string DirectoryId { get; set; }
        public bool CanDeleteApp { get; set; }
        public string DisplayName { get; set; }
        public string RegistrationMessage { get; set; }
        public string AppTypeDisplayName { get; set; }
        public bool IsPromptForUsername { get; set; }
        public bool Automatic { get; set; }
        public bool IsSwsEnabled { get; set; }
        public string NonLocalizedAdminTag { get; set; }
        public string AdminTag { get; set; }
        public string UserNameStrategy { get; set; }
        public string Description { get; set; }
        public string ParentDisplayName { get; set; }
        public bool IsShareable { get; set; }
        public bool FormFillingEnabled { get; set; }
        public string SharedBy { get; set; }
        public bool UseLoginPw { get; set; }
        public bool IsCredsAccessible { get; set; }
        public string WebAppLoginType { get; set; }
        public bool Personal { get; set; }
        public bool Shortcut { get; set; }
        public string Username { get; set; }
        public string AppType { get; set; }
        public string Url { get; set; }
        public string AuthChallengeDefinitionFlowId { get; set; }
        public string Icon { get; set; }
        public bool CanViewCreds { get; set; }
        public string _RowKey { get; set; }
        public string AppKey { get; set; }
        public bool CanSaveCreds { get; set; }
        public bool BypassLoginMFA { get; set; }
        public bool CertBasedAuthEnabled { get; set; }
        public string WebAppType { get; set; }
        public int Rank { get; set; }
        public bool IsTransferred { get; set; }
        public string TemplateName { get; set; }
        public string Notes { get; set; }
        public string AuthChallengeDefinitionId { get; set; }
        public string WebUPAppType { get; set; }
        public string RegistrationLinkMessage { get; set; }
        public object SharedUntil { get; set; }
        public bool IsScaEnabled { get; set; }
        public bool DerivedCredsSupported { get; set; }
        public bool UsernameRO { get; set; }
        public bool IsMFAEnabled { get; set; }
        public string HostNameSuffix { get; set; }
    }
    public class AppResult
    {
        public List<App> Apps { get; set; }
        public object DefaultTag { get; set; }
        public List<object> Tags { get; set; }
        public bool AllowPasswordView { get; set; }
        public string PreferredTenantCname { get; set; }
    }
    public class Root
    {
        public bool Success { get; set; }
        public AppResult Result { get; set; }
        public object Message { get; set; }
        public object MessageID { get; set; }
        public object Exception { get; set; }
        public object ErrorID { get; set; }
        public object ErrorCode { get; set; }
        public bool IsSoftError { get; set; }
        public object InnerExceptions { get; set; }
    }
    public class AppCredReturn
    {
        public bool Success { get; set; }
        public AppCredResult Result { get; set; }
        public string Message { get; set; }
        public string MessageID { get; set; }
        public string Exception { get; set; }
        public string ErrorID { get; set; }
        public string ErrorCode { get; set; }
        public bool IsSoftError { get; set; }
        public object InnerExceptions { get; set; }
    }
    public class AppCredResult
    {
        public string p { get; set; }
        public string u { get; set; }
    }
}
