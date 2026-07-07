using System;
using System.IO;
using System.Collections.Generic;

namespace BucketSurvey.Api.Helpers;

public static class EmailBodyBuilder
{
    public static string GenerateEmailBody(string Template, Dictionary<string, string> PlaceHolderValues)
    {
        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{Template}.html");

        using (StreamReader SR = new StreamReader(templatePath))
        {
            var Body = SR.ReadToEnd();

            foreach (var item in PlaceHolderValues)
            {
                Body = Body.Replace(item.Key, item.Value);
            }

            return Body;
        }
    }
}