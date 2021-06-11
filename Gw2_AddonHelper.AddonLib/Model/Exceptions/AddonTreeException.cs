using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2_AddonHelper.AddonLib.Model.Exceptions
{
    public class AddonTreeException : Exception
    {
        public enum ExceptionType
        {
            ReferenceLoop
        }

        public ExceptionType Type { get; set; }

        public AddonTreeException(ExceptionType type)
        {
            Type = type;
        }
    }
}
