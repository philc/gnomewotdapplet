/// <remarks>
/// Author: Phil Crosby
/// </remarks>
using org.gnome.Applets.GnomeWotdApplet;
using NUnit.Framework;

namespace org.gnome.Applets.GnomeWotdApplet.Test
{
	/// <summary>
	/// Test suite for Dictionary.
	/// </summary>
	[TestFixture]
	public class DictionaryTest
	{
	
		Dictionary dictZero;
		Dictionary dictOne;
		Dictionary dictTwo;
		Dictionary dictFive;
		
		[SetUp]
		public void Init()
		{
			dictZero = Dictionary.FromFile(TestStrings.dictZeroEntries);
			dictOne = Dictionary.FromFile(TestStrings.dictOneWord);
			dictTwo = Dictionary.FromFile(TestStrings.dictTwoWords);
			dictFive = Dictionary.FromFile(TestStrings.dictFiveWords);
		}		

		#region Verify dictionary contents
		/// <summary>
		/// Verify contents of loaded dictionarise.
		/// </summary>
		public void VerifyContents()
		{
			Assert.AreEqual(dictOne.Entries.Count,0);
			
			Assert.AreEqual(dictOne.Entries.Count,1);
						
			Assert.AreEqual(dictOne.Entries.Count,2);
			
			Assert.AreEqual(dictOne.Entries.Count,5);		
		}
		#endregion		
		
		#region Opening dictionaries

		/// <summary>
		/// Test loading a non-existent path.
		/// </summary>
		[Test]
		[ExpectedException(typeof(System.IO.FileNotFoundException))]
		public void TestBadOpen1()
		{			
			Dictionary dict = Dictionary.FromFile("nonexistantPath");
		}		
		/// <summary>
		/// Test loading an empty file (i.e. an invalid file, because
		/// there is no xml document root.
		/// </summary> 
		[Test]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void TestOpenEmpty()
		{
			
			Dictionary dict = Dictionary.FromFile(TestStrings.dictEmpty);
			
		}
		[Test]
		/// <summary />Open a corrupted XML document
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void TestOpenCorruptXml()
		{
			Dictionary dict = Dictionary.FromFile(TestStrings.dictCorruptXml);
		}
		#endregion

	}			
	
}