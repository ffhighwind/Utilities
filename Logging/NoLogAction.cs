using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logging
{
	internal class NoLogAction : LogAction
	{
		public NoLogAction()
		{
			Name = this.GetType().FullName;
		}

		public override void Log(LogState state) { }
	}
}
