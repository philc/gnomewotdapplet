/// <remarks>
/// Author: Phil Crosby
/// </remarks>

using System.IO;
using System.Collections;
using System.Xml;
using System;
using System.Text;

namespace org.gnome.Applets.GnomeWotdApplet
{
	/// <summary>
	/// Class representing a dictionary full of dictionary Entries. 
	/// </summary>
	public class Dictionary : ICloneable
	{	
		IList entries = new ArrayList();
		Random random = new Random();
		
		// Constructors
		private Dictionary()
		{
		}
		
		/// <summary />Add an entry to the dictionary.
		/// <param />Entry to add to the dictoinary
		public void Add(Entry entry)
		{
			entries.Add(entry);
		}	
		/// <summary>Static factory method that builds a Dictionary from an 
		/// XML stream.
		/// </summary>
		/// <param name="reader" />XmlTextReader containing markup for a Dictionary.
		/// <returns />A Dictionary object constructed from the XML stream
		/// <remarks>
		/// An empty dictionary (dictionary with zero entries) is acceptable.
		/// </remarks>
		public static Dictionary FromXml(XmlTextReader reader)
		{
			Dictionary result = new Dictionary();
			while (reader.Read()){
				if (reader.NodeType==XmlNodeType.Element){
					if (reader.LocalName.Equals("Entry")){
						result.Add(Entry.FromXml(reader));
						}
				}
			}
			return result;
		}
		/// <summary>
		/// Create a Dictionary from a list of entries.
		/// </summary>
		/// <param name="entries" />A List of Entry objects.
		public static Dictionary FromEntries(IList entries)
		{
			if (entries==null)
				entries=new ArrayList();
			Dictionary result = new Dictionary();
			result.Entries=entries;			
			return result;
		}
		public Object Clone()
		{
			Dictionary result = new Dictionary();
			// Create a deep copy
			Entry [] newEntries = new Entry[this.entries.Count];
			for (int i=0; i< this.entries.Count; i++)
				newEntries[i] = (Entry)((Entry)this.entries[i]).Clone();
			result.entries = new ArrayList(newEntries);
			return result;
		}
		/// <summary>
		/// Writes this dictionary to an xml stream
		/// </summary>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Dictionary");
			foreach (Entry entry in this.entries)
				entry.WriteXml(writer);
			writer.WriteEndElement();
		}
		/// <summary>
		/// Saves this dictionary to the given location.
		/// </summary>
		/// <param name="path" />Path to save this dictionary to.
		public void Save(string path)
		{
			//TextWriter textWriter = new TextWriter(path);
			XmlTextWriter writer = new XmlTextWriter(new StreamWriter(path));
			writer.Formatting = Formatting.Indented;			
			this.WriteXml(writer);
			writer.Close();
		}
		
		
		/// <summary>
		/// Static factory method that builds a Dictionary
		/// from an XML file.
		/// </summary>
		/// <param name="path" />Path to file
		/// <returns>A Dictionary constructed from the XML file
		public static Dictionary FromFile(string path){
			Dictionary result=null;
			XmlTextReader reader = new XmlTextReader(path);
			reader.WhitespaceHandling=WhitespaceHandling.Significant;
			try{
				result = Dictionary.FromXml(reader);
			}finally{
				if (reader!=null)
					reader.Close();
			}
		
			return result;
		}
		/// <summary>
		/// Shuffles the current dictionary into a randomly ordered assortment of entries.
		/// </summary>
		public void Shuffle(){
			if (entries.Count<=2)
				return;
			Entry lastEntry = entries[entries.Count-1] as Entry;
			
			IList shuffledList=new ArrayList(entries.Count);
			while (entries.Count>0){
				int randomIndex=random.Next(entries.Count);
				shuffledList.Add(entries[randomIndex]);
				entries.RemoveAt(randomIndex);			
			}
			
			// Protect against the case where the last entry is now the first,
			// thus giving us two words in a row after a shuffle
			if (shuffledList[0]==lastEntry){			
				shuffledList[0]=shuffledList[shuffledList.Count-1];
				shuffledList[shuffledList.Count-1]=lastEntry;
			}
			
			entries=shuffledList;
			
		}
		
		/// <see cref="Object.ToString()">
		public override string ToString(){
			StringBuilder builder = new StringBuilder();
			foreach (Entry item in entries)
				builder.Append(item.ToString());
			return builder.ToString();
		}
		
		#region Properties
		/// <summary />Collection of entries.
		public IList Entries{
			get { return entries; }
			set { entries = value; }
		}	
		#endregion
	}
	
}//end namespace