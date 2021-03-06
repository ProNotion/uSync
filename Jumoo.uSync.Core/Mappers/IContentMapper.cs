﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Jumoo.uSync.Core.Mappers
{
    public interface IContentMapper
    {
        string GetExportValue(PropertyType propType, string value);
        string GetImportValue(PropertyType propType, string content);
    }
}
