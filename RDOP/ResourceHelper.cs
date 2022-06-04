using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RDOP
{
	public class ResourceHelper
	{
		public static async Task<List<string>?> Read(string name)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			string path = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

			await using Stream? stream = assembly.GetManifestResourceStream(path);
			
			if (stream != null)
			{
				List<string>? lines = new List<string>();
				using StreamReader reader = new StreamReader(stream);

				while (!reader.EndOfStream)
				{
					if (await reader.ReadLineAsync() is { } line)
					{
						lines.Add(line);
					}
				}
				
				return lines;
			}

			return null;
		}
	}
}