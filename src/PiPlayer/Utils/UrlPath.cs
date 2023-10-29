using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using PiPlayer.Extensions;

namespace PiPlayer.Utils
{
    public static class UrlPath
    {
        public static string Combine(params string[] args)
        {
            if (args?.Any() != true)
            {
                throw new ArgumentNullException(nameof(args), "The join args can not null");
            }
            if (!args[0].StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !args[0].StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The first argument musta start with 'http://' or 'https://'");
            }
            if (!args[0].EndsWith('/'))
            {
                args[0] = args[0] + "/";
            }
            if (args.Length == 1)
            {
                return args[0];
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(args[0]);
            for (int i = 1; i < args.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(args[i]))
                    continue;
                sb.Append(args[i].Replace('\\', '/'));
                if (i != args.Length - 1)
                    sb.Append('/');
            }
            return sb.ToString();
        }

        public static string Combine(HttpRequest httpRequest, params string[] args)
        {
            List<string> argsList = new List<string>(args.Length + 1)
            {
                httpRequest.GetHostUri()
            };
            argsList.AddRange(args);
            return Combine(argsList.ToArray());
        }
    }
}
