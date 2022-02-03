using Gtk;
using GtkSharp;
using System;
using GConf;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace org.gnome.Applets.GnomeWotdApplet{

	///<remarks>
	///Testing class
	///</remarks>
	public class Driver{
		
		private static string dictionaryPath="../../test/dictionaries/01.xml";
		private static string twoDictionaryPath="../../test/dictionaries/oneWord.xml";
		private static string greDictionaryPath="../../data/dictionaries/gre.xml";
		
		///<summary />Driver entry point
		public static void Main(string [] args){			
			TextWriterTraceListener listener = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(listener);
			
			//RunWordGenerator();
			//RunWordGeneratorOnTimer();
			//RunUrlWordGenerator();
			//TestSaveDictionary();
			
			RunGui(args);			
		}
		
		
		///<summary />Runs entry code for the GUI
		private static void RunGui(string[] args){
			Gnome.Program program = new Gnome.Program("GWOTD", About.Version, Gnome.Modules.UI,args);
			
			//Cycle time of every 3 seconds
			//IWordGenerator generator = new FileWordGenerator(dictionaryPath,3);
			//DisplayWindow window = new DisplayWindow(program, generator);
			DisplayWindow window = new DisplayWindow(program);
			
			window.Show();		
			program.Run ();
		}
		
		// Tests opening and then saving a dictionary.
		private static void TestSaveDictionary()
		{
			string testingPath = "/home/reformist/d/tmp/dictionaryOutput.xml";
			Dictionary dictionary = Dictionary.FromFile(dictionaryPath);
			dictionary.Save(testingPath);
			Dictionary dictionary2 = Dictionary.FromFile(testingPath);						
		}
		
		private static void RunUrlWordGenerator(){
			UrlWordGenerator generator = new UrlWordGenerator();
			//UrlWordGenerator generator = new UrlWordGenerator(UrlWordGenerator.MerriamWebsterUrl);
			generator.WordChangedEvent+=new WordChangedEventHandler(HandleWordChangedEvent);
			generator.MoveNext();
			System.Console.WriteLine(generator.CurrentWord);
			System.Console.In.ReadLine();        
		}
		private static void RunWordGenerator(){
			//FileWordGenerator generator = new FileWordGenerator(twoDictionaryPath,1);
			FileWordGenerator generator = new FileWordGenerator(greDictionaryPath,1);
			generator.WordChangedEvent+= new WordChangedEventHandler(HandleWordChangedEvent);
			generator.MoveNext();
			System.Console.ReadLine();
		}
		private static void HandleWordChangedEvent(object sender, WordChangedEventArgs e){
			System.Console.WriteLine(e.Word + " error? " + e.IsError);			
		}
	}

}//end namespace
