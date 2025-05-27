using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PtasieRadio
{
	public readonly struct SemaphoreLock(SemaphoreSlim semaphore) : IDisposable
	{
		public void Dispose() => semaphore.Release();
	}

	public static class SemaphoreLockEx
	{
		public static async Task<SemaphoreLock> Lock(this SemaphoreSlim semaphore)
		{
			await semaphore.WaitAsync();
			return new SemaphoreLock(semaphore);
		}
	}
}
