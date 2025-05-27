using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PtasieRadio
{
	public static class NullableEx
	{
		public static B Map<A, B>(this A a, Func<A, B> map) => map(a);
	}
}
