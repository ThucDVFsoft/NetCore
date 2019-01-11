using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Models;
using Movies.API.Models.Pagination;
using Movies.API.Services;
using Newtonsoft.Json;

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
                var filters = ParseJSON(search);
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
        public List<Filter> ParseJSON(string json)
        {
            var filters = JsonConvert.DeserializeObject<List<Filter>>(json);
            return filters;
        }
    }
}
