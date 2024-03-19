using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCSSStuff.Components.LocalStorage
{
    internal class LocalStorageService
    {
        private const string StorageNotAvailableMessage = "Unable to access the browser storage. This is most likely due to the browser settings.";

        private readonly IJSRuntime _jSRuntime;
        private readonly IJSInProcessRuntime? _jSInProcessRuntime;

        public LocalStorageService(IJSRuntime jSRuntime)
        {
            _jSRuntime = jSRuntime;
            _jSInProcessRuntime = jSRuntime as IJSInProcessRuntime;
        }

        // Get and set item methods - these are the only ones we're using at this point
        public async ValueTask<string?> GetItemAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _jSRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
            }
            catch (Exception exception)
            {
                if (IsStorageDisabledException(exception))
                {
                    throw new BrowserStorageDisabledException(StorageNotAvailableMessage, exception);
                }

                throw;
            }
        }

        public async ValueTask SetItemAsync(string key, string data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _jSRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, data);
            }
            catch (Exception exception)
            {
                if (IsStorageDisabledException(exception))
                {
                    throw new BrowserStorageDisabledException(StorageNotAvailableMessage, exception);
                }

                throw;
            }
        }


        public string GetItem(string key)
        {
            CheckForInProcessRuntime();
            try
            {
                return _jSInProcessRuntime.Invoke<string>("localStorage.getItem", key);
            }
            catch (Exception exception)
            {
                if (IsStorageDisabledException(exception))
                {
                    throw new BrowserStorageDisabledException(StorageNotAvailableMessage, exception);
                }

                throw;
            }
        }

        public void SetItem(string key, string data)
        {
            CheckForInProcessRuntime();
            try
            {
                _jSInProcessRuntime.InvokeVoid("localStorage.setItem", key, data);
            }
            catch (Exception exception)
            {
                if (IsStorageDisabledException(exception))
                {
                    throw new BrowserStorageDisabledException(StorageNotAvailableMessage, exception);
                }

                throw;
            }
        }

        [MemberNotNull(nameof(_jSInProcessRuntime))]
        private void CheckForInProcessRuntime()
        {
            if (_jSInProcessRuntime == null)
                throw new InvalidOperationException("IJSInProcessRuntime not available");
        }

        private static bool IsStorageDisabledException(Exception exception)
            => exception.Message.Contains("Failed to read the 'localStorage' property from 'Window'");
    }

    internal class BrowserStorageDisabledException : Exception
    {
        public BrowserStorageDisabledException()
        {
        }

        public BrowserStorageDisabledException(string message) : base(message)
        {
        }

        public BrowserStorageDisabledException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
