///<remarks>Author: Michael Quinn
///</remarks>

using Gtk;
using Gdk;

namespace org.gnome.Applets.GnomeWotdApplet {

	public class About
	{
		private static string version = "0.1.0";
		public static string Version {
			get {
				return version;
			}
		}
	
		private static string [] authors = null;
		public static string [] Authors {
			get {
				if (authors == null) {
					authors = new string [2];
	
					authors [0] = "Phil Crosby (reformist@philisoft.com)";
					authors [1] = "Michael Quinn (mikeq@wam.umd.edu)";
				}
				
				return authors;
			}
		}
	
		public static void ShowWindow ()
		{
			string [] documenters = new string [] {};
	
			Gnome.About about;
			about = new Gnome.About ("Gnome Word of the Day Applet", version,
						 "Copyright © 2004, Michael Quinn",
						 "Increase your vocabulary.  You might have one as good as Mike's one day.",
						 Authors, documenters,
						 null,
						 null);
	
			about.Run ();
		}
	}
}		