using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Models;
using Movies.API.Models.Pagination;
using Movies.API.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Movies.API.Controllers
{
    [Route("movies")]
    public class MoviesController : Controller
    {
        private readonly IMovieRepository service;
        private readonly IUrlHelper urlHelper;

        public MoviesController(IMovieRepository service, IUrlHelper urlHelper)
        {
            this.service = service;
            this.urlHelper = urlHelper;
        }

        //[HttpGet(Name = "GetMovies")]
        //public IActionResult Get(PagingParams pagingParams)
        //{
        //    var model = service.GetMovies(pagingParams);

        //    Response.Headers.Add("X-Pagination", model.Header.ToJson()); // X-Pagination →{"totalItems":21,"pageNumber":1,"pageSize":10,"totalPages":3}

        //    var output = new MovieOutput
        //    {
        //        Paging = model.Header,      // Information about totalItems, current pageNumber, pageSize, totalPages                  
        //        Links = GetLinks(model),    // 3 links: Previous<-> Current<-> Next
        //        Items = Mapper.Map<List<MovieDto>>(model.List), // Items of current page
        //    };
        //    return Ok(output);
        //}

        //[HttpGet(Name = "GetMovies")]
        //public IActionResult Get(PagingParams pagingParams, string searchStr)
        //{
        //    var model = service.GetMovies(pagingParams, searchStr);

        //    Response.Headers.Add("X-Pagination", model.Header.ToJson()); // X-Pagination →{"totalItems":21,"pageNumber":1,"pageSize":10,"totalPages":3}

        //    var output = new MovieOutput
        //    {
        //        Paging = model.Header,      // Information about totalItems, current pageNumber, pageSize, totalPages                  
        //        Links = GetLinks(model),    // 3 links: Previous<-> Current<-> Next
        //        Items = Mapper.Map<List<MovieDto>>(model.List), // Items of current page
        //    };
        //    return Ok(output);
        //}


        #region Filter
#if false
        [HttpGet(Name = "GetMovies")]
        public IActionResult Get(PagingParams pagingParams, string title = null, string releaseYear = null, string summary = null)
        {
            PagedList<Movie> model = null;
            model = service.GetMovies(pagingParams, title, releaseYear, summary);

            Response.Headers.Add("X-Pagination", model.Header.ToJson()); // X-Pagination →{"totalItems":21,"pageNumber":1,"pageSize":10,"totalPages":3}

            var output = new MovieOutput
            {
                Paging = model.Header,      // Information about totalItems, current pageNumber, pageSize, totalPages                  
                Links = GetLinks(model),    // 3 links: Previous<-> Current<-> Next
                Items = Mapper.Map<List<MovieDto>>(model.List), // Items of current page
            };
            return Ok(output);
        }
#else
        [HttpGet(Name = "GetMovies")]
        public IActionResult Get(PagingParams pagingParams, string search)
        {
            PagedList<Movie> model = null;
            if (string.IsNullOrEmpty(search))
            {
                model = service.GetMovies(pagingParams);
            }
            else
            {
                var jsonDict = ParseJSON(search);
                var filters = JsonToFilterModel(jsonDict);

                model = service.GetMovies(pagingParams, filters);
            }

            Response.Headers.Add("X-Pagination", model.Header.ToJson()); // X-Pagination →{"totalItems":21,"pageNumber":1,"pageSize":10,"totalPages":3}

            var output = new MovieOutput
            {
                Paging = model.Header,      // Information about totalItems, current pageNumber, pageSize, totalPages                  
                Links = GetLinks(model),    // 3 links: Previous<-> Current<-> Next
                Items = Mapper.Map<List<MovieDto>>(model.List), // Items of current page
            };
            return Ok(output);
        }
#endif
#endregion

#region " Links "
        private List<LinkInfo> GetLinks(PagedList<Movie> list)
        {
            var links = new List<LinkInfo>();

            if (list.HasPreviousPage)
                links.Add(CreateLink("GetMovies", list.PreviousPageNumber, list.PageSize, "previousPage", "GET"));

            links.Add(CreateLink("GetMovies", list.PageNumber, list.PageSize, "self", "GET"));

            if (list.HasNextPage)
                links.Add(CreateLink("GetMovies", list.NextPageNumber, list.PageSize, "nextPage", "GET"));

            return links;
        }

        private LinkInfo CreateLink(string routeName, int pageNumber, int pageSize, string rel, string method)
        {
            return new LinkInfo
            {
                Href = urlHelper.Link(routeName,
                            new { PageNumber = pageNumber, PageSize = pageSize }),
                Rel = rel,
                Method = method
            };
        }

#endregion

#region " JSON to FilterModel "
        public Dictionary<string, object> ParseJSON(string json)
        {
            int end;
            return ParseJSON(json, 0, out end);
        }
        private Dictionary<string, object> ParseJSON(string json, int start, out int end)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            bool escbegin = false;
            bool escend = false;
            bool inquotes = false;
            string key = null;
            int cend;
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> child = null;
            List<object> arraylist = null;
            Regex regex = new Regex(@"\\u([0-9a-z]{4})", RegexOptions.IgnoreCase);
            int autoKey = 0;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\\') escbegin = !escbegin;
                if (!escbegin)
                {
                    if (c == '"')
                    {
                        inquotes = !inquotes;
                        if (!inquotes && arraylist != null)
                        {
                            arraylist.Add(DecodeString(regex, sb.ToString()));
                            sb.Length = 0;
                        }
                        continue;
                    }
                    if (!inquotes)
                    {
                        switch (c)
                        {
                            case '{':
                                if (i != start)
                                {
                                    child = ParseJSON(json, i, out cend);
                                    if (arraylist != null) arraylist.Add(child);
                                    else
                                    {
                                        dict.Add(key, child);
                                        key = null;
                                    }
                                    i = cend;
                                }
                                continue;
                            case '}':
                                end = i;
                                if (key != null)
                                {
                                    if (arraylist != null) dict.Add(key, arraylist);
                                    else dict.Add(key, DecodeString(regex, sb.ToString()));
                                }
                                return dict;
                            case '[':
                                arraylist = new List<object>();
                                continue;
                            case ']':
                                if (key == null)
                                {
                                    key = "array" + autoKey.ToString();
                                    autoKey++;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                dict.Add(key, arraylist);
                                arraylist = null;
                                key = null;
                                continue;
                            case ',':
                                if (arraylist == null && key != null)
                                {
                                    dict.Add(key, DecodeString(regex, sb.ToString()));
                                    key = null;
                                    sb.Length = 0;
                                }
                                if (arraylist != null && sb.Length > 0)
                                {
                                    arraylist.Add(sb.ToString());
                                    sb.Length = 0;
                                }
                                continue;
                            case ':':
                                key = DecodeString(regex, sb.ToString());
                                sb.Length = 0;
                                continue;
                        }
                    }
                }
                sb.Append(c);
                if (escend) escbegin = false;
                if (escbegin) escend = true;
                else escend = false;
            }
            end = json.Length - 1;
            return dict; //theoretically shouldn't ever get here
        }
        private string DecodeString(Regex regex, string str)
        {
            return Regex.Unescape(regex.Replace(str, match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber))));
        }
        private List<Filter> JsonToFilterModel(Dictionary<string, object> dict)
        {
            if (dict == null) return new List<Filter>();

            List<Filter> filters = new List<Filter>();
            foreach (string key1 in dict.Keys)
            {
                Filter filter = new Filter();
                filter.Attribute = key1;
                var contents = (dict[key1] as List<object>);
                foreach (var item in contents)
                {
                    if (item is Dictionary<string, object>)
                    {
                        var itemDict = item as Dictionary<string, object>;
                        foreach (var key2 in itemDict.Keys)
                        {
                            if (key2 == "operator")
                            {
                                filter.Operator = itemDict["operator"].ToString();
                            }
                            else if (key2 == "value")
                            {
                                filter.Values = itemDict["value"];
                            }
                        }
                    }
                }

                filters.Add(filter);
            }
            return filters;
        }
#endregion
    }
}
