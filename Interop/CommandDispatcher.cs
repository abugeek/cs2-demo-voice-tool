using System;
using System.Collections.Generic;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Interop.Handlers;
using DemoPulse.Models;
using DemoPulse.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DemoPulse.Interop
{
    public class CommandDispatcher
    {
        private readonly IUiMessenger _messenger;
        private readonly Dictionary<string, ICommandHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

        public CommandDispatcher(IEnumerable<ICommandHandler> handlers, IUiMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    Register(handler);
                }
            }
        }

        public static CommandDispatcher CreateDefault(IUiMessenger messenger, IDialogService dialogService, IDemoService demoService, AppSettings settings)
        {
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
            if (demoService == null) throw new ArgumentNullException(nameof(demoService));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            return new CommandDispatcher(new ICommandHandler[]
            {
                new GetSettingsHandler(messenger, settings),
                new SaveSettingsHandler(messenger, settings),
                new BrowseCs2FolderHandler(dialogService, messenger, settings),
                new GenerateVoiceCfgHandler(messenger, settings),
                new ExportVoiceCfgHandler(dialogService, settings),
                new SelectFileHandler(dialogService, demoService),
                new ParseDemoHandler(demoService),
                new LaunchCs2Handler(dialogService, settings, demoService),
                new RenameDemoHandler(messenger, dialogService),
                new OpenFolderHandler(dialogService)
            }, messenger);
        }

        public void Register(ICommandHandler handler)
        {
            _handlers[handler.CommandName] = handler;
        }

        public bool Dispatch(string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage)) return false;

            try
            {
                IpcRequest request;

                try
                {
                    // Attempt to parse structured JSON envelope
                    if (rawMessage.TrimStart().StartsWith("{"))
                    {
                        var parsed = JsonSerializer.Deserialize<IpcRequest>(rawMessage);
                        if (parsed != null && !string.IsNullOrWhiteSpace(parsed.Command))
                        {
                            request = parsed;
                        }
                        else
                        {
                            request = ParseLegacyStringMessage(rawMessage);
                        }
                    }
                    else
                    {
                        request = ParseLegacyStringMessage(rawMessage);
                    }
                }
                catch
                {
                    request = ParseLegacyStringMessage(rawMessage);
                }

                if (_handlers.TryGetValue(request.Command, out var handler))
                {
                    try
                    {
                        handler.Execute(request);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        string safeError = ex.Message.Replace("\"", "'").Replace("\n", " ").Replace("\r", "");
                        _messenger.SendResponse(new IpcResponse
                        {
                            Id = request.Id,
                            Type = "COMMAND_ERROR",
                            Success = false,
                            Error = $"Error executing command '{request.Command}': {safeError}"
                        });
                        return false;
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Command))
                {
                    _messenger.SendResponse(new IpcResponse
                    {
                        Id = request.Id,
                        Type = "COMMAND_NOT_FOUND",
                        Success = false,
                        Error = $"Command '{request.Command}' not recognized"
                    });
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CommandDispatcher Fatal] Dispatch error: {ex}");
                return false;
            }
        }

        private static IpcRequest ParseLegacyStringMessage(string rawMessage)
        {
            string commandName = rawMessage;
            string rawPayload = "";

            int colonIdx = rawMessage.IndexOf(':');
            if (colonIdx > 0)
            {
                commandName = rawMessage.Substring(0, colonIdx);
                rawPayload = rawMessage.Substring(colonIdx + 1);
            }

            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(rawPayload));
            return new IpcRequest
            {
                Command = commandName,
                Payload = doc.RootElement.Clone()
            };
        }
    }
}
