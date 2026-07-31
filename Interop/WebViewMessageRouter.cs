using System;
using DemoPulse.Models;
using DemoPulse.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DemoPulse.Interop
{
    public class WebViewMessageRouter
    {
        private readonly AppSettings _settings;
        private readonly CommandDispatcher _dispatcher;

        public WebViewMessageRouter(CommandDispatcher dispatcher, AppSettings settings)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public static WebViewMessageRouter CreateDefault(IUiMessenger messenger, IDialogService dialogService, IDemoService demoService, AppSettings settings)
        {
            return new WebViewMessageRouter(CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings), settings);
        }

        public AppSettings Settings => _settings;

        public void HandleMessage(string msg)
        {
            try
            {
                _dispatcher.Dispatch(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebViewMessageRouter Error] Exception handling message: {ex.Message}");
            }
        }
    }
}
