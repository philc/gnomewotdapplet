/// <remarks>
/// Author: Phil Crosby
/// </remarks>

using NUnit.Framework;
using org.gnome.Applets.GnomeWotdApplet;
using System;
using System.Diagnostics;

namespace org.gnome.Applets.GnomeWotdApplet.Test
{
	/// <summary>
	/// FileWordGenerator test suite.
	/// </summary>
	[TestFixture]
	public class UrlWordGeneratorTest
	{	
		UrlWordGenerator tmp =null;
		int testCounter=0;
		
		#region Test UrlWordGenerator creation
		/// <summary>
		/// Test dictionary.com dictionary
		/// </summary>
		[Test]
		public void TestCreateDictionaryCom()
		{
			UrlWordGenerator gen = new UrlWordGenerator(UrlWordGenerator.DictionaryComUrl);
			gen.WordChangedEvent+=new WordChangedEventHandler(CreateHandler1);
			gen.MoveNext();
			// Ensure the current word is null, or we've already retrieved it
			// (i.e. testCounter>0)
			Assert.IsTrue(gen.CurrentWord==null || testCounter>0);
			
			while (testCounter<=0)
				System.Threading.Thread.Sleep(0);
				
			Assert.IsTrue(gen.CurrentWord!=null);
		}
		
		/// <summary /> Test bad urls
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void BadCreate1()
		{
			tmp=new UrlWordGenerator("bad url format");
		}
		/// <summary />Test unknown urls.
		[Test]
		[ExpectedException(typeof(ArgumentException))]		
		public void BadCreate2()
		{
			tmp = new UrlWordGenerator("http://notAValidSite.com");
		}
		private void CreateHandler1(object sender, WordChangedEventArgs e)
		{
			testCounter++;
			Debug.WriteLine("caught url word event, word is " + e.Word);
			Assert.IsTrue(e.Word!=null);
		}
		#endregion		
	}
}