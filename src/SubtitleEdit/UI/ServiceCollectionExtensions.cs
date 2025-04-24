using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // collection.AddSingleton<IRepository, Repository>();
        // collection.AddTransient<BusinessService>();
        collection.AddTransient<MainViewModel>();
    }
}