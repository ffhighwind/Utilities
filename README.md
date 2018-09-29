# Description:

This project contains utilities for general purpose use. This includes
reading/writing files, emails, and databases as well as converting between
different types, process generation, security, and localization.

# Installation:

This project requires [Visual Studio](https://visualstudio.microsoft.com/) to compile.
Once installed to install the required NuGet packages. Right click on "Utilities" in the
Solution Explorer and select "Manage NuGet Packages...". From here Visual Studio will
prompt you to install the missing packages.

You can add the library to another Visual Studio solution by adding it as a
reference. Right click on the Solution of your other project and
Add > Existing Project. Then right click on "References in the Solution Explorer
and add this as a project reference.

# NuGet Packages:

* [CsvHelper](https://joshclose.github.io/CsvHelper/)
   For reading and writing comma/tab separated value (csv, tsv) files.
* [Dapper](https://github.com/StackExchange/Dapper)
   For querying SQL Databases in a type-safe way.
* [EPPlus](https://github.com/JanKallman/EPPlus)
   For Excel (xlsx) file reading and writing via Excel.Spreadsheet and Excel.Worksheet.
* [Microsoft.Exchange.WebServices](https://github.com/sherlock1982/ews-managed-api)
   For Microsoft Outlook access methods via Email.EmailService.
* [Newtonsoft.Json](https://www.newtonsoft.com/json)
   For pretty printing via Extensions.ToString.
* [MSTest.TestAdapter](https://www.nuget.org/packages/MSTest.TestAdapter/)
   For unit testing.
* [MSTest.TestFramework](https://www.nuget.org/packages/MSTest.TestFramework/)
   For unit testing.
* [Costura.Fody](https://github.com/Fody/Costura)
   Not required but this will reduce the size of the output executable and eliminate the dll files.

# Credits:

* [File Decoding](https://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp) author: [Dan W](https://stackoverflow.com/users/848344/dan-w)
* [US Federal Holidays](https://stackoverflow.com/questions/3709584/business-holiday-date-handling) author: [Todes3ngel](https://stackoverflow.com/users/3889241/todes3ngel)
* [Encryption](https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file) author: Jon Galloway
* [Network Connection](https://gist.github.com/AlanBarber/92db36339a129b94b7dd) author: Alan Barber
* [Generic DbDataReader](https://github.com/mgravell/fast-member/blob/master/FastMember/ObjectReader.cs) author: [Marc Gravell, FastMember](https://github.com/mgravell/fast-member)

# License:

*MIT License*

Copyright (c) 2018 Wesley Hamilton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.