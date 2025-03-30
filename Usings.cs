global using System;
global using System.Collections.Generic;

global using System.IdentityModel.Tokens.Jwt;

global using System.Linq;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global  using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Azure.Cosmos;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.IdentityModel.Protocols;
global using Microsoft.IdentityModel.Protocols.OpenIdConnect;
global using Microsoft.IdentityModel.Tokens;
global using Azure;
global using Azure.AI.OpenAI;
global using Azure.Identity;
global using Azure.Security.KeyVault.Secrets;
global using ChatApp.Models;
global using ChatApp.Services;

global using Internal;
global using Newtonsoft.Json;
global using OpenAI.Chat;
global using OpenAIChatMessage =
    OpenAI.Chat.ChatMessage; // since our model names overlap, we need to alias

global using Microsoft.AspNetCore.Authentication.OpenIdConnect;
global using Microsoft.Extensions.Configuration;
global using System.Text;
