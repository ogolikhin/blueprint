using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusControl.Repositories
{
	public interface IStatusRepository
	{
		Task<bool> GetStatus();
	}
}
