﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U5ki.Infrastructure.Code.Base
{
    public interface IBaseFactory<T>
    {

        T ManufactureOne(BaseDto parameters);

    }
}
