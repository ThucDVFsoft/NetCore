using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.API.Models
{
    public class Filter
    {
        public string Attribute { get; set; } = "";
        public string Operator { get; set; } = "";

        private object values;
        //public object Values
        //{
        //    get
        //    {
        //        return values;
        //    }
        //    set
        //    {
        //        if (value is string)
        //        {
        //            values = new List<string>() { value as string };
        //        }
        //        else if (value is List<object>)
        //        {
        //            values = (value as List<object>).Select(s => s as string).ToList();
        //        }
        //    }
        //}
        public List<string> Values { get; set; }
    }
}
