﻿namespace SpaiceGameTerminal
{
	using System;
	public static class RandomWrapper
	{
		//Function to get random number
		private static readonly Random random = new Random();
		private static readonly object syncLock = new object();
		public static int RandomNumber(int min, int max)
		{
			lock (syncLock)
			{ // synchronize
				return random.Next(min, max);
			}
		}
	}
}
