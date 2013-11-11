/**
 * File: Program.cs
 *
 * Authors: Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2006.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Collections;
using Microsoft.Ink;
using ConverterXML;
using ConverterJnt;
using Fragmenter;

namespace JntToXml
{
	/// <summary>
	/// Convert Microsoft Journal files into an XML standard.
	/// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ArrayList argArray = new ArrayList(args);			
			int numArgs = argArray.Count;

			string[] files;
 
			if (numArgs == 0)
			{
				Console.WriteLine("*****************************************************************");
				Console.WriteLine("*** JntToXml.exe");
				Console.WriteLine("*** by Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.");
				Console.WriteLine("*** Harvey Mudd College, Claremont, CA 91711.");
				Console.WriteLine("*** Sketchers 2006.");
				Console.WriteLine("***");
				Console.WriteLine("*** Usage: JntToXml.exe (-c | -d directory | -r) (-f)");
				Console.WriteLine("*** Usage: JntToXml.exe input1.jnt [input2.jnt ...]");
				Console.WriteLine("***");
				Console.WriteLine("*** -c: convert all files in current directory");
				Console.WriteLine("*** -d directory: convert all files in the specified directory");
				Console.WriteLine("*** -r: recursively convert files from the current directory");
				Console.WriteLine("*** -f: fragment the strokes");
			
				return;
			}
			else if(argArray.Contains("-c")) // Convert everything in this directory
			{
				files = Directory.GetFiles(Directory.GetCurrentDirectory());
			}
			else if(argArray.Contains("-d")) // Convert everything in specified directory
			{
				if(argArray.IndexOf("-d") + 1  >= argArray.Count)	// Are we in range?
				{
					Console.Error.WriteLine("No directory specified.");
					return;
				}
				else if(!Directory.Exists((string)argArray[argArray.IndexOf("-d") + 1])) // Does dir exist?
				{
					Console.Error.WriteLine("Directory doesn't exist.");
					return;
				}
				else
					files = Directory.GetFiles((string)argArray[argArray.IndexOf("-d") + 1]);
			}
			else if(argArray.Contains("-r")) //Recursive from current dir
			{
				//Get recursive files
				ArrayList rFiles = new ArrayList();
				DirSearch(Directory.GetCurrentDirectory(), ref rFiles);
			
				//Get current dir files
				string [] currDir = Directory.GetFiles(Directory.GetCurrentDirectory());

				files = new string[rFiles.Count + currDir.Length];

				//populate both recursive and current into files
				int current;
				for(current = 0; current < currDir.Length; ++current)
					files[current] = currDir[current];

				foreach(string s in rFiles)
				{
					files[current++] = s;
				}
			}
			else //Convert only the specified files
			{
				files = args;
			}

			bool frag = false;
			if(argArray.Contains("-f"))
				frag = true;

			string subDir = "convertedJnt";
			Directory.CreateDirectory(subDir);
						
			ConverterJnt.ReadJnt ink;
			ConverterXML.MakeXML xml;
			int numPages;
			int i;
			string output;

			foreach (string input in files)
			{
				//The Microsoft Journal file must end with .jnt or .jtp
				if ( !input.ToLower().EndsWith(".jnt") && !input.ToLower().EndsWith(".jtp") )
				{
					//Console.Error.WriteLine("Unknown extension in " + input + ", must be .jnt or .jtp");
					continue;
				}				
				
				try
				{
					Console.Write("Trying " + input + " ");
				
					numPages = ReadJnt.NumberOfPages(input);
					
					for (i = 1; i <= numPages; ++i)
					{
						Console.Write(".");
						
						output = subDir + "\\" + Path.GetFileNameWithoutExtension(input) + "." + i.ToString();
						if (frag)
							output += ".fragged";
						output += ".xml";
						
						ink = new ConverterJnt.ReadJnt(input, i);

						Sketch.Sketch theSketch = ink.Sketch;
				
						if (frag)
							Fragment.fragmentSketch(ink.Sketch);

						xml = new ConverterXML.MakeXML(ink.Sketch);
						xml.WriteXML(output);
					}

					Console.WriteLine();
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.InnerException);
					Console.WriteLine(e.StackTrace);
					Console.ReadLine();
					continue;
				}				
			}
		}


		/// <summary>
		/// Perform a recursive directory search. http://support.microsoft.com/default.aspx?scid=kb;en-us;303974
		/// </summary>
		/// <param name="sDir">Directory to search recursively</param>
		/// <param name="rFiles">Array to add the files to</param>
		static void DirSearch(string sDir, ref ArrayList rFiles) 
		{
			try	
			{
				foreach (string d in Directory.GetDirectories(sDir)) 
				{
					foreach (string f in Directory.GetFiles(d, "*.*")) 
					{
						rFiles.Add(f);
					}
					DirSearch(d, ref rFiles);
				}
			}
			catch (System.Exception excpt) 
			{
				Console.WriteLine(excpt.Message);
			}
		}
	}
}
