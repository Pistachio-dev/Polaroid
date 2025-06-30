using Microsoft.Extensions.DependencyInjection;
using Polaroid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaroid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUiVisibilityManager(this IServiceCollection sc)
        {
            sc.AddSingleton<Plugin>();

            return sc;
        }
    }
}
