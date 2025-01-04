using Microsoft.Extensions.Logging;
using BudgetEase.Services;
using Blazored.Modal;
using Microsoft.JSInterop;

namespace BudgetEase
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();  // This already provides IJSRuntime

            // Register your services
            builder.Services.AddBlazoredModal();
            builder.Services.AddSingleton<UserServices>();
            builder.Services.AddSingleton<TransactionService>();
            builder.Services.AddSingleton<DebtService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
