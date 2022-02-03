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
	public class FileWordGeneratorTest
	{
		// Paths		
		FileWordGenerator genOne;
		FileWordGenerator genTwo;
		FileWordGenerator genThree;
		FileWordGenerator genFive;
		
		FileWordGenerator tmp;
		private int testCounter=0;
		
		/// <summary />Test dictionaries that should load without fail.
		[SetUp]
		public void Init()
		{
			// Set up some test dictionaries
			genOne = new FileWordGenerator(TestStrings.dictOneWord);
			genTwo = new FileWordGenerator(TestStrings.dictTwoWords);
			genThree = new FileWordGenerator(TestStrings.dictThreeWords);
			genFive = new FileWordGenerator(TestStrings.dictFiveWords);
		}
		#region Stress Test
		private Entry stressCurrentWord=null;
		// <summary>
		// Test moving through three entries 50 times, make sure no word
		// is repeated twice in the same order
		// </summary>
		[Test]		
		public void TestStress1()
		{
			genThree.WordChangedEvent+=new WordChangedEventHandler(StressHandler1);
			int runCount=50;
			for (int i=0; i<runCount; i++)
			{
				genThree.MoveNext();				
			} 
			Assert.AreEqual(testCounter, runCount);
			
		}
		private void StressHandler1(object sender, WordChangedEventArgs e)
		{
			testCounter++;
			Assert.IsTrue(e.Word != stressCurrentWord);
		}
		#endregion
		
		#region Test Properties
		/// <summary />Set Dictionary property with zero-sized property
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestProperties()
		{
			genOne.Dictionary = Dictionary.FromFile(TestStrings.dictZeroEntries);
			
		}
		#endregion
		
		#region Test Events		
		// These tests take awhile (minimum cycle time is one second)
		// so use sparingly.
		/// <summary>
		/// Ensure two word events are fired for two-word dictionary
		/// </summary>
		[Test]
		public void TestEvents1()	
		{
			// Create one second word generator
			FileWordGenerator gen = new FileWordGenerator(TestStrings.dictTwoWords,1);
			gen.WordChangedEvent+= new WordChangedEventHandler(this.WordChangedHandler1);
			// Make sure our event method get's called twice.
			gen.MoveNext();
			while (testCounter<2)
			{
				System.Threading.Thread.Sleep(0);
			}				
		}
		private void WordChangedHandler1(object sender, WordChangedEventArgs e)
		{
			testCounter++;
			Assert.IsTrue(e.Word != null);
		}
		#endregion
		
		#region Test MoveNext
		/// <summary>
		/// Test MoveNext'ing through 5 entries
		/// </summary>		
		[Test]
		public void TestMoveNextFive()
		{
			genFive.MoveNext();
			Entry initialWord = genFive.CurrentWord;
			// Move through the dictionary, ensure no repeats..
			for (int i=0; i<4; i++){
				genFive.MoveNext();
				Assert.IsTrue(initialWord!=genFive.CurrentWord);
			}
			// Make sure it hits our initial word once
			bool found=false;
			for (int i=0; i<5; i++){	
				genFive.MoveNext();				
				Assert.IsTrue(!(found && initialWord==genFive.CurrentWord));
				if (initialWord==genFive.CurrentWord)
					found=true;
			}
			Assert.IsTrue(found);
		}
		#endregion
		
		#region Test loading dictionaries
		///<summary />Load from non-existant path
		[Test]
		[ExpectedException(typeof(System.IO.FileNotFoundException))]
		public void TestBadOpen1()
		{
			tmp = new FileWordGenerator("nonExistantPath");					
		}
		///<summary />Load with invalid timer
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestBadOpen2()
		{
			tmp = new FileWordGenerator("bleh", -5);
		}
		///<summary />Test zero-entry dictionary
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestBadOpen3()
		{
			tmp = new FileWordGenerator(TestStrings.dictZeroEntries);
		}
		#endregion
	}
}