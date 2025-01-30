Here is a GitHub-style README for the code you provided:

---

# CyberArk Credential Management Tool

This console application is designed to interact with CyberArk's REST API for the purpose of managing applications and secured items. It enables users to authenticate, retrieve applications or secured items, and fetch associated credentials using CyberArk's API endpoints.

## Features
- **Authentication:** Start and advance authentication to retrieve a bearer token.
- **Get Applications:** Retrieve a list of applications associated with the user.
- **Get Secured Items:** Retrieve a list of secured items associated with the user.
- **Retrieve Credentials:** Fetch credentials for a selected application or secured item.

## Requirements
- **.NET Core 3.1 or higher** for running the application.
- **CyberArk Tenant ID** (required for authentication).
- **Public and Private Keys** for encrypting and decrypting sensitive data.
- **CyberArk API Access**: API credentials and endpoints should be properly configured in the CyberArk system.

## Installation

1. Clone this repository to your local machine:
   ```bash
   git clone https://github.com/yourusername/CyberArk-Credential-Management.git
   cd CyberArk-Credential-Management
   ```

2. Ensure that you have the necessary keys (`pubkey.txt` and `privKey.txt`) stored in the appropriate paths (e.g., `C:/pubkey.txt` and `C:/privKey.txt`).

3. Restore any necessary NuGet packages:
   ```bash
   dotnet restore
   ```

4. Build the project:
   ```bash
   dotnet build
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

## Usage

### Step 1: Authenticate

- When prompted, enter your **Tenant ID** and **Username**.
- You will be asked for your **Password** to complete the authentication process. After successful authentication, a **Bearer Token** will be generated.

### Step 2: Choose Operation
You can choose from two operations:

1. **Get Applications**: Retrieve and display a list of available applications associated with your tenant.
   - For each application, you can retrieve associated credentials.

2. **Get Secured Items**: Retrieve and display a list of secured items associated with your tenant.
   - For each secured item, you can retrieve its credentials.

### Step 3: Select Item
- After selecting an application or secured item, the system will retrieve and display its credentials.

## API Endpoints

The following CyberArk API endpoints are used by the application:

1. **Start Authentication**
   - **Endpoint:** `/Security/StartAuthentication`
   - **Method:** `POST`
   - **Description:** Starts the authentication process by accepting the username and returning a session ID and mechanism ID.
   
2. **Advance Authentication**
   - **Endpoint:** `/Security/AdvanceAuthentication`
   - **Method:** `POST`
   - **Description:** Completes the authentication process by accepting a password and returning a bearer token.

3. **Get Applications**
   - **Endpoint:** `/UPRest/GetUPData?force=true`
   - **Method:** `POST`
   - **Description:** Retrieves a list of applications available to the authenticated user.

4. **Get Application Credentials**
   - **Endpoint:** `/UPRest/GetMCFA?appkey={appkey}`
   - **Method:** `POST`
   - **Description:** Retrieves credentials for a specific application identified by its `appkey`.

5. **Get Secured Items**
   - **Endpoint:** `/UPRest/GetSecuredItemsData`
   - **Method:** `POST`
   - **Description:** Retrieves a list of secured items associated with the authenticated user.

6. **Get Secured Item Credentials**
   - **Endpoint:** `/UPRest/GetCredsForSecuredItem?sItemKey={itemKey}`
   - **Method:** `POST`
   - **Description:** Retrieves credentials for a specific secured item identified by its `sItemKey`.

## Decryption

The application supports decrypting secured credentials using an AES decryption method. The `privKey.txt` file must contain the private key required for decrypting the credentials.

## Error Handling

The application includes basic error handling for HTTP request failures and authentication issues. In case of an error, the status code or exception message will be displayed to help diagnose the issue.

## Security Considerations

- **Credentials:** Be cautious when handling credentials. Ensure private keys and passwords are stored securely.
- **Decryption:** The application uses AES decryption. Ensure that your private key (`privKey.txt`) is handled securely and not exposed.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments
- CyberArk for providing the API documentation and support.
- Newtonsoft.Json for handling JSON serialization and deserialization.
