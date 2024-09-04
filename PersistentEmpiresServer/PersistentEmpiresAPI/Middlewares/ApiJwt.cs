/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PersistentEmpiresAPI.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ApiJwt
    {
        private readonly RequestDelegate _next;
        // private string secretKey = "23681e93dd3399d83d498a1f081e3866fcaedf6a27d2ac53cce09be51f95d739fb90a24840d41825dbda5d13b795ad82778fa491ad555336352659061edb5151";
        public ApiJwt(RequestDelegate next)
        {
            _next = next;
        }



        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Headers["Authorization"];

            var validationParameters = new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = "Sample",
                ValidAudience = "Sample",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PersistentEmpiresAPISubModule.SecretKey)) // The same key as the one that generate the token
            };

            if (accessToken.Count == 0)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided.");
                return;
            }
            // Console.WriteLine("KEY IS "+GenerateToken());
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(accessToken, validationParameters, out var validToken);
            JwtSecurityToken validJwt = validToken as JwtSecurityToken;

            if (validJwt == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid apikey");
                return;
            }

            if (!validJwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("use HS256");
                return;
            }

            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiJwtExtensions
    {
        public static IApplicationBuilder UseApiJwt(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiJwt>();
        }
    }
}
