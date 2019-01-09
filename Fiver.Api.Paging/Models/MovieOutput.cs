using Movies.API.Models.Pagination;
using System.Collections.Generic;

namespace Movies.API.Models
{
    public class MovieOutput
    {
        public PagingHeader Paging { get; set; }
        public List<LinkInfo> Links { get; set; }
        public List<MovieDto> Items { get; set; }
    }
}
