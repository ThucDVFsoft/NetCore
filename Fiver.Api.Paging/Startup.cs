using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Movies.API.Models;
using Movies.API.Services;

namespace Movies.API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                           .ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddScoped<IMovieRepository, MovieRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Movie, MovieDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.ReleaseYear, opt => opt.MapFrom(src => src.ReleaseYear))
                    .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summary))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
