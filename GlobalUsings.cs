

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Cors;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using System.Reflection;
global using JwtBearerDefaults = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults;
global using System.Text;



global using FluentValidation;
global using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
global using MapsterMapper;
global using Mapster;



global using AI_genda_API.Entities;
global using AI_genda_API.Contracts.Authentication;
global using System.ComponentModel.DataAnnotations;
global using  AI_genda_API.Authentication;
global using  Microsoft.Extensions.Options;
global using AppContext = AI_genda_API.Presistience.AppContext;
global using AI_genda_API.Abstractions;
global using Task = AI_genda_API.Entities.Task;
