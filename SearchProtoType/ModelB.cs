﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchProtoType
{
    public class ModelB
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ForeignKey { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
