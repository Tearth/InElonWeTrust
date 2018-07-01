using System;
using System.Collections.Generic;
using System.Text;

namespace InElonWeTrust.Core.Services.LinkShortener
{
    public class ShortcutResponse
    {
        public ShortcutResponseData Data { get; set; }
        public bool Success { get; set; }
        public int Status { get; set; }
    }
}
