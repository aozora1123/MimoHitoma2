using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MimoHitoma2.ViewModels
{
    public class UploadViewModel
    {
        public string imageTags { get; set; }

        public UploadViewModel()
        {
            this.imageTags = "";
        }
    }
}