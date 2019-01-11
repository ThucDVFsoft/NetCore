using Movies.API.Models;
using Movies.API.Models.Pagination;
using System.Collections.Generic;

namespace Movies.API.Services
{
    public interface IMovieRepository
    {
        // Paging Only
        PagedList<Movie> GetMovies(PagingParams pagingParams);

        // Paging + Searching
        PagedList<Movie> GetMovies(PagingParams pagingParams, string search);

        // Paging + Filtering
        PagedList<Movie> GetMovies(PagingParams pagingParams, string title = null, string releaseYear = null, string summary = null);
        PagedList<Movie> GetMovies(PagingParams pagingParams, IEnumerable<Filter> filters);
    }
}
