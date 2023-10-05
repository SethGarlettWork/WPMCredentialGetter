
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

GetSecuredItems();

//string bearerToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjVBNjlCMTMzQkQ2QTBFNDcwMTRCOTNGQzAxN0Y0MTExQjQwQ0VDNjkiLCJ4NXQiOiJXbW14TTcxcURrY0JTNVA4QVg5QkViUU03R2siLCJhcHBfaWQiOiJfX2lkYXB0aXZlX2N5YnJfdXNlcl9vaWRjIn0.eyJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXRoLmdhcmxldHQud29ya0BjeWJlcmFyay5jb20uNjU3NiIsImZhbWlseV9uYW1lIjoiV29yayIsInRlbmFudF9zdWJkb21haW4iOiIiLCJ1bmlxdWVfbmFtZSI6InNldGguZ2FybGV0dC53b3JrQGN5YmVyYXJrLmNvbS42NTc2IiwiaWRhcHRpdmVfdGVuYW50X2lkIjoiQUFZNDU1MSIsInBpY3R1cmUiOiJodHRwczovL2FheTQ1NTEuaWQuY3liZXJhcmsuY2xvdWQvVXNlck1nbXQvR2V0VXNlclBpY3R1cmU_aWQ9NzY3ODRhZDItMjBlYi00ZGFlLWIwNDktNTMwYTdlNDgwZjdkIiwidGVuYW50X2lkIjoiIiwidXNlcl9yb2xlcyI6WyJTYWxlc2ZvcmNlIENoYXR0ZXIgVXNlcnMiLCJQVldBIFVzZXJzIiwiU2FsZXNmb3JjZSBBZG1pbiBVc2VycyIsIkVQTSBVc2VycyIsIlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRmxvd3MgQWRtaW4iLCJObyBNRkEiLCJTV1MgQWRtaW4iLCJBV1MgRnVsbCBDb250cm9sIiwiRXZlcnlib2R5IiwiTWVkaXVtIFJpc2sgVXNlcnMiXSwiaWF0IjoxNjk2NTIzNTkxLCJzdWIiOiI3Njc4NGFkMi0yMGViLTRkYWUtYjA0OS01MzBhN2U0ODBmN2QiLCJhdXRoX3RpbWUiOjE2OTY1MjM1OTEsImV4cCI6MTY5NjUyMzg5MSwidXNlcl91dWlkIjoiNzY3ODRhZDItMjBlYi00ZGFlLWIwNDktNTMwYTdlNDgwZjdkIiwic2NvcGUiOiJvcGVuaWQgYXBpIHByb2ZpbGUiLCJsYXN0X2xvZ2luIjoiMTY5NjUxMTU2MSIsImF1ZCI6Il9faWRhcHRpdmVfY3licl91c2VyX29pZGMiLCJhd3NfcmVnaW9uIjoiIiwic3ViZG9tYWluIjoiIiwiY3NyZl90b2tlbiI6IldmOVJJLTdGUVJha1NkTVBoSmNGMWVpUlhJdDNuLUZQczFpWWo5eXBuMEExIiwiaW50ZXJuYWxfc2Vzc2lvbl9pZCI6ImRkMGExNGUyLTE2MTktNDliYS1iMTVjLWQxOWQ5NjU2YmE0OCIsInBsYXRmb3JtX2RvbWFpbiI6IiIsImlzcyI6Imh0dHBzOi8vYWF5NDU1MS5pZC5jeWJlcmFyay5jbG91ZC9fX2lkYXB0aXZlX2N5YnJfdXNlcl9vaWRjLyIsImF0X2hhc2giOiJYR1piREMzcUtvMVdzSmR3QlZRVF9RIiwibmFtZSI6IlNldGggR2FybGV0dCBXb3JrIiwiZ2l2ZW5fbmFtZSI6IlNldGgiLCJFeHRlcm5hbFV1aWQiOiI3Njc4NGFkMi0yMGViLTRkYWUtYjA0OS01MzBhN2U0ODBmN2QifQ.bhww9aND8A3m3x0MyFKa57SBWMemAGOWksQRoBqNtk41IQPniI-rTmtPU8TL2U0rKzTzwmRTeP9BI77tZYpZHl_Yk2hnU_krmos2_fcCmsx2_tRy-Cw1FvfuJMAm7P9iyGYPFNFYdT5hxLMCrrZIuSpeO-LN0YOp68fsWlx2CoN-R1PZ9nVNMiC-H1hC8rp7ZnAkXzyUDkh5Cseva9eIGlowDO6pQ04DTLtRoE5eOh-lLmUGvkNR9e8deNJXzNT7C2zb7IFCn_fcvfS2cpnpbAS9BAsjQ7ObP_y75e0dTqxauw_Loj1Q3W5i6ZqGyLGwvkWZSdefYXIvUi83zH5OBA";


static async Task<StartAuthDTO> StartAuthentication()
{
    var startAuthDTO = new StartAuthDTO();
    Console.WriteLine("Please enter your username:");
    var userName = Console.ReadLine();


    using var httpClient = new HttpClient();
    try
    {
        string apiUrl = "https://aay4551.id.cyberark.cloud/Security/StartAuthentication"; // Replace with your API endpoint URL
                                                                                          // Serialize the payload to JSON
        var payload = new
        {
            TenantID = "aay4551",
            User = userName,
            Version = "1.0"
        };
        string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a 30-second timeout

        // Send the POST request with the payload
        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Handle a successful response here
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {responseContent}");

            // Deserialize the response content into your object (StartAuthDTO)
            startAuthDTO = Newtonsoft.Json.JsonConvert.DeserializeObject<StartAuthDTO>(responseContent);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP Request Error: {ex.Message}");
    }
    return startAuthDTO;
}
static async Task<string> GetBearerToken()
{
    var startAuthentication = await StartAuthentication();
    var bearerToken = "";

    Console.WriteLine("Please enter your password:");
    var password = Console.ReadLine();

    var payload = new
    {
        SessionId = startAuthentication.SessionId,
        MechanismId = startAuthentication.MechanismId,
        Action = "Answer",
        Answer = password
    };
    string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    using var httpClient = new HttpClient();
    try
    {
        string apiUrl = "https://aay4551.id.cyberark.cloud/Security/AdvanceAuthentication"; // Replace with your API endpoint URL

        // Send the POST request with the payload
        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            // Handle a successful response here
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {responseContent}");

            // Deserialize the response content into your object (StartAuthDTO)
            bearerToken = Newtonsoft.Json.JsonConvert.DeserializeObject<BearerTokenResult>(responseContent).BearerToken;

        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP Request Error: {ex.Message}");
    }
    return bearerToken;
}



static async Task GetSecuredItems()
{
    var bearerToken = await GetBearerToken();
    using var httpClient = new HttpClient();
    try
    {
        string apiUrl = "https://aay4551.id.cyberark.cloud/UPRest/GetSecuredItemsData"; // Replace with your API endpoint URL
                                                                                        // Serialize the payload to JSON
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        // Send the POST request with the payload
        HttpResponseMessage response = await httpClient.PostAsync(apiUrl, null);

        if (response.IsSuccessStatusCode)
        {
            // Handle a successful response here
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response: {responseContent}");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP Request Error: {ex.Message}");
    }
}

public class StartAuthDTO
{
    [JsonProperty("Result.SessionId")]
    public string SessionId { get; set; }
    [JsonProperty("Result.Challenges[0].Mechanisms[0].MechanismId")]
    public string MechanismId { get; set; }
}

public class BearerTokenResult
{
    [JsonProperty("Result.Token")]
    public string BearerToken { get; set; }
}