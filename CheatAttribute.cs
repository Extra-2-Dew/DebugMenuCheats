using System;

namespace DebugMenuCheats
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CheatAttribute : Attribute
	{
		public string CommandName { get; }
		public string[] CommandAliases { get; }

		public CheatAttribute(string commandName, string[] commandAliases = null)
		{
			CommandName = commandName;
			CommandAliases = commandAliases;
		}
	}
}