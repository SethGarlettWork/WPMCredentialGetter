
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

Console.WriteLine("Please enter your username:");
var userName = Console.ReadLine();
Console.WriteLine("Please enter your password:");
var password = Console.ReadLine();

string apiUrl = "https://aay4551.id.cyberark.cloud/UPRest/GetSecuredItemsData"; // Replace with your API endpoint URL
string bearerToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjVBNjlCMTMzQkQ2QTBFNDcwMTRCOTNGQzAxN0Y0MTExQjQwQ0VDNjkiLCJ4NXQiOiJXbW14TTcxcURrY0JTNVA4QVg5QkViUU03R2siLCJhcHBfaWQiOiJfX2lkYXB0aXZlX2N5YnJfdXNlcl9vaWRjIn0.eyJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXRoLmdhcmxldHQud29ya0BjeWJlcmFyay5jb20uNjU3NiIsImZhbWlseV9uYW1lIjoiV29yayIsInRlbmFudF9zdWJkb21haW4iOiIiLCJ1bmlxdWVfbmFtZSI6InNldGguZ2FybGV0dC53b3JrQGN5YmVyYXJrLmNvbS42NTc2IiwiaWRhcHRpdmVfdGVuYW50X2lkIjoiQUFZNDU1MSIsInBpY3R1cmUiOiJodHRwczovL2FheTQ1NTEuaWQuY3liZXJhcmsuY2xvdWQvVXNlck1nbXQvR2V0VXNlclBpY3R1cmU_aWQ9NzY3ODRhZDItMjBlYi00ZGFlLWIwNDktNTMwYTdlNDgwZjdkIiwidGVuYW50X2lkIjoiIiwidXNlcl9yb2xlcyI6WyJTYWxlc2ZvcmNlIENoYXR0ZXIgVXNlcnMiLCJQVldBIFVzZXJzIiwiU2FsZXNmb3JjZSBBZG1pbiBVc2VycyIsIkVQTSBVc2VycyIsIlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRmxvd3MgQWRtaW4iLCJObyBNRkEiLCJTV1MgQWRtaW4iLCJBV1MgRnVsbCBDb250cm9sIiwiRXZlcnlib2R5IiwiTG93IFJpc2sgVXNlcnMiXSwiaWF0IjoxNjk2NTE0MzcyLCJzdWIiOiI3Njc4NGFkMi0yMGViLTRkYWUtYjA0OS01MzBhN2U0ODBmN2QiLCJhdXRoX3RpbWUiOjE2OTY1MTQzNzIsImV4cCI6MTY5NjUxNDY3MiwidXNlcl91dWlkIjoiNzY3ODRhZDItMjBlYi00ZGFlLWIwNDktNTMwYTdlNDgwZjdkIiwic2NvcGUiOiJvcGVuaWQgYXBpIHByb2ZpbGUiLCJsYXN0X2xvZ2luIjoiMTY5NjUxMTU2MSIsImF1ZCI6Il9faWRhcHRpdmVfY3licl91c2VyX29pZGMiLCJhd3NfcmVnaW9uIjoiIiwic3ViZG9tYWluIjoiIiwiY3NyZl90b2tlbiI6ImNHbmxGRmpjYmtHajFrSEh0dnM2Z3dPZjV1TGlCbFN1Z2dweGFPZEJVYmsxIiwiaW50ZXJuYWxfc2Vzc2lvbl9pZCI6IjNmYjI0Y2YyLTc5NjItNDQxMi1iNjI4LTlkNmU2N2U5ZmNjNCIsInBsYXRmb3JtX2RvbWFpbiI6IiIsImlzcyI6Imh0dHBzOi8vYWF5NDU1MS5pZC5jeWJlcmFyay5jbG91ZC9fX2lkYXB0aXZlX2N5YnJfdXNlcl9vaWRjLyIsImF0X2hhc2giOiJsU0lzX0Q4Y1huem1pQm50WVFaOElBIiwibmFtZSI6IlNldGggR2FybGV0dCBXb3JrIiwiZ2l2ZW5fbmFtZSI6IlNldGgiLCJFeHRlcm5hbFV1aWQiOiI3Njc4NGFkMi0yMGViLTRkYWUtYjA0OS01MzBhN2U0ODBmN2QifQ.M6LvlC65e-rsoiqlnIcJBb4iLaBM4_NZY2MG5ZKtJMpDAs3d2qLjvqgPtngkOsKQBrj4VX9uOARJQ81FSv3VEF2LPMnU6rByrjBdUCA9HwOM-mRBc_YHFy8OgeVkSR8MbQpJcIP9EhT4blgbh0tOnGNhGb7TaCu0yTH9hqmhSs0FQVSLBI579TWyno-OZ8I1P7azHByc9KsPjMmjgy5qDOVePIbh-4qU4oqwibbnyKzcMnCWX5gqEF3E2otnbFNkCvGJhKG1pAmSF5u5W0fsKAimB1cstHj4nj7rrThitUqfyL6rpcxxhWd1cUstflReHEBVJiY0MIoaZXDKKO18hQ"; // Replace with your Bearer token

using var httpClient = new HttpClient();

// Set the Bearer token in the request headers
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

// Create a JSON payload if needed
var payload = new
{
    Key1 = "Value1",
    Key2 = "Value2"
};

try
{
    // Serialize the payload to JSON
    string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

    // Create a StringContent with the JSON payload
    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    // Send the POST request with the payload
    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

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