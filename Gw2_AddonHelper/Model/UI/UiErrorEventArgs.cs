using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.Model.UI
{
    public class UiErrorEventArgs : EventArgs
    {
        private Exception _exception;
        private string _message;

        public UiErrorEventArgs(string message)
        {
            _message = message;
        }

        public UiErrorEventArgs(Exception exception, string message) : this(message)
        {
            _exception = exception;
        }

        public Exception Exception
        {
            get => _exception; 
            set => _exception = value; 
        }

        public string Message
        {
            get => _message; 
            set => _message = value; 
        }
    }
}
