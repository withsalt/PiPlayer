using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PiPlayer.Extensions
{
    public static class StringExtensions
    {
        public static StringBuilder AppendWithSpace(this StringBuilder sb, string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return sb;
            }
            if (sb.Length > 0 && sb[sb.Length - 1] != ' ')
            {
                sb.Append(' ');
            }
            sb.Append(val);
            return sb;
        }

    }
}
