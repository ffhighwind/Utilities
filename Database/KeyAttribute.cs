﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	public class KeyAttribute : Attribute
	{
		public KeyAttribute(bool required = false)
		{
			Required = required;
		}

		public bool Required { get; private set; }
	}
}
