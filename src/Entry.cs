/// <remarks>
/// Author: Phil Crosby
/// </remarks>
using System;
using System.IO;
using System.Xml;
using System.Collections;

namespace org.gnome.Applets.GnomeWotdApplet{

	/// <remarks>
	/// Class representing an entry into a dictionary, complete
	/// with the word and a collection of definitions.
	/// </remarks>
	public class Entry : IComparable, ICloneable{
		protected string word;		
		// Each entry can have multiple definitions, each with different articles
		IList definitions = new ArrayList();
		
		#region Constructors
		private Entry(){
		}
		#endregion
		
		/// <summary />Constructs a new Entry
		/// <param name="word" />The word associated with this Entry
		public Entry(string word){
			this.word=word;
		}
		
		/// <summary>
		/// Constructs a new Entry, with a single definition.
		/// </summary>
		/// <param name="word" />The word associated with this Entry
		/// <param name="definition />A definition associatd with this Entry
		public Entry(string word, Definition definition){
			this.word=word;
			definitions.Add(definition);
		}
		
		/// <summary>
		/// Compares two dictonary entries.
		/// </summary>
		/// <param name="other" /> A Entry to compare this instance to.
		public int CompareTo(Object other){
			Entry o = other as Entry;
			if (o==null)
				return 1;
			return (word.CompareTo(o.Word));
		}
		/// <summary>
		/// Creates a copy of this instance, including a cloned list of
		/// definitions.
		/// </summary>
		public Object Clone()
		{
			Entry entry = new Entry();
			entry.word=this.word;
			// Create a deep copy
			Definition [] newDefs = new Definition[this.definitions.Count];
			for (int i=0; i< this.definitions.Count; i++)
				newDefs[i] = (Definition)((Definition)this.definitions[i]).Clone();
			entry.definitions = new ArrayList(newDefs);
			return entry;		
		}
		
		/// <summary>
		/// Static factory method that builds an Entry object from an 
		/// XML stream.
		/// </summary>
		/// <param name="reader" />XmlTextReader containing markup for an Entry.
		/// <returns />An Entry object constructed from the XML stream.
		public static Entry FromXml(XmlTextReader reader){
			Entry result = new Entry();
			while (reader.Read()){
				if (reader.NodeType==XmlNodeType.Element){				
					if (reader.LocalName.Equals("Definition"))
						result.definitions.Add(Definition.FromXml(reader));
					else if (reader.LocalName.Equals("Word"))					
						result.Word=reader.ReadString();					
				}else if (reader.NodeType==XmlNodeType.EndElement)
					break;
			}
			if (result.definitions.Count<=0 || result.word.Length<=0)
				throw new XmlException("Error parsing Entry XML fragment.");
			return result;
		}
		/// <summary>
		/// Writes this Entry to an XML stream.
		/// </summary>
		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement("Entry");
			writer.WriteElementString("Word",this.word);
			foreach (Definition definition in this.definitions)
				definition.WriteXml(writer);
			writer.WriteEndElement();	
		}
		
		public override string ToString()
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			builder.Append("  " + word + "\n");
			foreach (Object def in definitions)
				builder.Append("\t" + def + "\n");
			return builder.ToString();
		}
		
		/// <summary>
		/// The string word of the dictionary entry.
		/// </summary>
		public string Word{
			get { return word; }
			set { word=value;}
		}
		/// <summary>
		/// Collection of definitions for this word.
		/// </summary>
		public IList Definitions{
			get { return definitions; }
		}		
	}
}