[![NuGet](https://img.shields.io/nuget/v/SimpleCsv.svg)](https://www.nuget.org/packages/SimpleCsv/) [![NuGet](https://img.shields.io/nuget/dt/SimpleCsv.svg)](https://www.nuget.org/packages/SimpleCsv/) [![license](https://img.shields.io/github/license/unterrainerinformatik/SimpleCSV.svg?maxAge=2592000)](http://unlicense.org)  [![Twitter Follow](https://img.shields.io/twitter/follow/throbax.svg?style=social&label=Follow&maxAge=2592000)](https://twitter.com/throbax)  

# General

This section contains various useful projects that should help your development-process.  

This section of our GIT repositories is free. You may copy, use or rewrite every single one of its contained projects to your hearts content.  
In order to get help with basic GIT commands you may try [the GIT cheat-sheet][coding] on our [homepage][homepage].  

This repository located on our  [homepage][homepage] is private since this is the master- and release-branch. You may clone it, but it will be read-only.  
If you want to contribute to our repository (push, open pull requests), please use the copy on github located here: [the public github repository][github]  

# ![Icon](https://github.com/UnterrainerInformatik/SimpleCSV/raw/master/icon.png) SimpleCSV

A small but powerful library for writing and reading CSV files. Supports buffered stream-input, so you don't have to have all of it in RAM. 

This project helps you to read and write CSV (Comma Separated Values) files in your program.
Those files are legacy, but many developers still use them because of their readability and because every sheet-calculation program can perfectly edit them.
With the classes in this project you can read/write them via a stream, so that not every byte of the CSV has to be in memory beforehand and they provide nice and fluid interfaces for all functions, they are well tested (visit the project-page) and they are capable of reading and writing quoted values.
Give it a try.


> **If you like this repo, please don't forget to star it.**
> **Thank you.**



## Getting Started

### StringReader

```c#
StringReader stringReader = new StringReader("\"test\";test1;A 01;t;;");

CsvReader csvReader = new CsvReader(stringReader, ';', "\n", '\"');
List<string> row = csvReader.ReadRow();
```



### StringWriter

```c#
using(CsvWriter csvWriter = new CsvWriter(new StringBuilder(), ';', "\n", '\"'))
{
  csvWriter.Write("Great");
  csvWriter.Write("Totally");
  csvWriter.Write("This is a\nbreak");
  // The next two lines do the same as calling csvWriter.writeLine("");
  csvWriter.Write();
  csvWriter.WriteLine();

  csvWriter.Write().Write("Gr;eat").WriteLine("Totally");

  csvWriter.Write("Great").Write("ssfe\"s").Write("").Write("Totally");
}
```

For more and more elaborate examples, take a look at the test-project.



[homepage]: http://www.unterrainer.info
[coding]: http://www.unterrainer.info/Home/Coding
[github]: https://github.com/UnterrainerInformatik/SimpleCSV