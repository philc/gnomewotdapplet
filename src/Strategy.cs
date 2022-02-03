/// <remarks>
/// Author: Michael Quinn
/// </remarks>
using System;

namespace org.gnome.Applets.GnomeWotdApplet  
{
	public class Strategy  
	{
		static public IWordGenerator GenerateWordGenerator(Preferences preferences)
		{
			if (preferences.RetrieveFromUrl == true)
			{
				return new UrlWordGenerator(preferences.DictionaryUrl);
			}
			else
			{
				// Set up properties specific to file generators, e.g. cycle time
				// and visited words array.
				FileWordGenerator wordGenerator = new FileWordGenerator(preferences.DictionaryPath, 
																		preferences.CycleTime);				
				wordGenerator.VisitedWords=preferences.VisitedWords;
				Console.WriteLine("Generating File Generator");
				Console.WriteLine("   Path: {0}", preferences.DictionaryPath);
				Console.WriteLine("   cycleTime: {0}", preferences.CycleTime);
				return wordGenerator;
			}
		}
		
		static public IWordGenerator GetGenerator(IWordGenerator generator, Preferences preferences)  
		{
			IWordGenerator newGenerator = null;
			if (preferences.RetrieveFromUrl == true)
			{				
				newGenerator = new UrlWordGenerator(preferences.DictionaryUrl);
				Console.WriteLine("Creating URL Generator");
				Console.WriteLine("   URL: {0}", preferences.DictionaryUrl);
			}
			else // preferences.RetriveFromUrl == false
			{
				if (generator is FileWordGenerator)
				{
					FileWordGenerator fileGenerator = (FileWordGenerator) generator;
					
					// TODO: Should preferences visited words be cleared?  where does this happen?
					if (fileGenerator.DictionaryPath != preferences.DictionaryPath)
						fileGenerator.DictionaryPath = preferences.DictionaryPath;
						
					fileGenerator.CycleTime = preferences.CycleTime;

					newGenerator = fileGenerator;
					
					Console.WriteLine("Modifying File Generator");
					Console.WriteLine("   Path: {0}", preferences.DictionaryPath);
					Console.WriteLine("   cycleTime: {0}", preferences.CycleTime);				
				}
				else
				{
					newGenerator = new FileWordGenerator(preferences.DictionaryPath,
														 preferences.CycleTime);
					Console.WriteLine("Creating new File Generator");
					Console.WriteLine("   Path: {0}", preferences.DictionaryPath);
					Console.WriteLine("   cycleTime: {0}", preferences.CycleTime);
				}
			}
			return newGenerator;	
		}	
	}
}