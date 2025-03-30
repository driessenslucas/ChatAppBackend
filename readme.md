# Chat Application with Azure OpenAI and Cosmos DB

This project is a chat application that leverages Azure OpenAI for intelligent responses and Azure Cosmos DB for persistent chat session storage. It provides a RESTful API for interacting with the chat functionality, including message history and user session management.

## Key Takeaways and Things I Learned

- **Azure OpenAI Integration:** I gained practical experience integrating Azure OpenAI into a .NET Core application, learning how to send and receive chat messages and manage AI responses.
- **Secure Secret Management with Azure Key Vault:** I learned how to securely store and retrieve sensitive information like API keys and connection strings using Azure Key Vault, ensuring the application's security.
- **User Authentication and Authorization with Azure AD B2C:** I explored and implemented user management and authentication using Azure AD B2C, understanding how to secure API endpoints and manage user identities.
- **Cosmos DB for NoSQL Data Storage:** I learned how to use Cosmos DB to store and retrieve chat session data, exploring its scalability and NoSQL capabilities.
- **API Development with ASP.NET Core:** I gained practical experience in designing and implementing a RESTful API using ASP.NET Core, including handling requests, responses, and error management.
- **Asynchronous Programming:** I developed a deeper understanding of asynchronous programming in .NET, and how to properly use async and await to prevent deadlocks.
- **Dependency Injection:** I learned how to properly use dependency injection to make my code more testable and maintainable.
- **Configuration Management:** I learned how to properly manage configuration using the options pattern.


## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Setup and Configuration](#setup-and-configuration)
  - [Prerequisites](#prerequisites)
  - [Azure Resource Setup](#azure-resource-setup)
  - [Configuration](#configuration)
  - [Build and Run](#build-and-run)
- [API Documentation](#api-documentation)
- [Deployment (Optional)](#deployment-optional)
- [Contributing](#contributing)
- [License](#license)

## Project Overview

This application allows users to engage in chat conversations with an AI assistant powered by Azure OpenAI. It stores chat sessions and message history in Azure Cosmos DB, enabling users to retrieve and continue past conversations. The API provides endpoints for sending messages, retrieving chat history, and managing user sessions.

## Architecture

```mermaid
graph LR
    A[Client (Optional)] --> B(API - ChatController);
    B --> C(OpenAIService);
    C --> D[Azure OpenAI];
    B --> E(CosmosDbService);
    E --> F[Azure Cosmos DB];
    B --> G(KeyVaultService);
    G --> H[Azure Key Vault];
```

* **Client (Optional):** A front-end application that interacts with the API.
* **API (ChatController):** ASP.NET Core Web API that handles user requests and orchestrates interactions with other services.
* **OpenAIService:** Manages communication with Azure OpenAI, sending user messages and receiving AI responses.
* **CosmosDbService:** Handles data persistence, storing and retrieving chat sessions from Azure Cosmos DB.
* **KeyVaultService:** Securely retrieves API keys and connection strings from Azure Key Vault.
* **Azure OpenAI:** Provides the AI chat capabilities.
* **Azure Cosmos DB:** Stores chat sessions and message history.
* **Azure Key Vault:** Stores sensitive application secrets.

## Technologies Used

* .NET Core 6+
* ASP.NET Core Web API
* Azure OpenAI SDK
* Azure Cosmos DB SDK
* Azure Key Vault SDK
* C#
* Swashbuckle/Swagger (for API documentation)

## Setup and Configuration

### Prerequisites

* .NET SDK 6+
* Visual Studio/VS Code
* Azure CLI (optional, for Azure resource creation)

### Azure Resource Setup

1.  Create an Azure account if you don't have one.
2.  Create an Azure OpenAI resource.
3.  Create an Azure Cosmos DB account and database/container.
4.  Create an Azure Key Vault resource.
5.  Store the OpenAI API key and Cosmos DB connection string in Key Vault.
6.  Create an Azure AD B2C tenant (for user authentication, see [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/overview) and [Azure AD B2C sample web api](https://learn.microsoft.com/en-us/azure/active-directory-b2c/configure-authentication-sample-web-app-with-api?tabs=visual-studio)). 

### Configuration

1.  Clone the repository.
2.  Create an `appsettings.json` file in the project directory.
3.  Add the following configuration settings:

```json
{
  "Azure": {
    "OpenAIEndpoint": "",
    "KeyVaultUri": "",
    "OpenAIKeySecretName": "",
    "CosmosDbConnectionString": ""
  },
  "AzureB2C": {
    "TenantName": "",
    "PolicyName": "",
    "ClientId": "",
    "Authority": "",
    "Domain": "",
    "SignUpSignInPolicyId": "",
    "Instance": ""
  },
  "CosmosDb":{
    "DatabaseName": "",
    "ContainerName": ""
  },
}
```

4. Ensure that the secrets referenced in the connection strings are located in the Azure KeyVault you created.
