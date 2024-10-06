XML to CSV Converter

This .NET Core 8 console application converts a directory of XML files with identical structure into a single CSV file. The application dynamically determines the CSV columns based on the first XML file's structure.

Features

Dynamic column generation: The CSV header and columns are created automatically by analyzing the structure of the first XML file in the input directory.

Handles nested lists: The application can process XML files containing nested lists, creating a separate CSV row for each item in the deeply nested lists.

Tag-based analysis: The converter analyzes XML tags to identify lists and nested objects for accurate data extraction.

Error handling: Basic error handling is implemented to ensure smooth execution and informative error messages.

Getting Started

Clone the repository:

git clone https://github.com/your-username/your-repo-name.git
content_copy
Use code with caution.
Bash

Build the project:

cd your-repo-name
dotnet build
content_copy
Use code with caution.
Bash

Run the application:

dotnet run "<path_to_xml_directory>" "<output_csv_file_path>"
content_copy
Use code with caution.
Bash

Replace <path_to_xml_directory> with the absolute path to the directory containing your XML files.

Replace <output_csv_file_path> with the desired path and filename for the output CSV file.

Example:

dotnet run "C:\Users\JohnDoe\Documents\XMLFiles" "C:\Users\JohnDoe\Documents\output.csv"
content_copy
Use code with caution.
Bash
Input/Output Example

Input XML (example):

<?xml version="1.0"?>
<Root>
  <Title>secondDoc</Title>
  <customers>
    <groupNumber>4</groupNumber>
    <groupId>4</groupId>
    <customer>
      <name>Charter Group</name>
      <address>
        <addres>
          <street>100 Main</street>
          <city>Framingham</city>
          <state>MA</state>
          <zip>01701</zip>
        </addres>
      </address>
      <id>
        <value>1</value>
      </id>
    </customer>
  </customers>
</Root>
content_copy
Use code with caution.
Xml

Output CSV:

Title,groupNumber,groupId,name,street,city,state,zip,value
secondDoc,4,4,Charter Group,100 Main,Framingham,MA,01701,1
content_copy
Use code with caution.
Notes

All XML files in the input directory must have the same structure.

The first XML file in the directory is used to determine the CSV structure.

The application assumes that all elements within a list have the same structure.

Future Enhancements

Implement more robust error handling and validation for different XML structures.

Add support for specifying custom delimiters and text qualifiers for the output CSV file.

Provide an option to specify the XML node to start extracting data from, allowing for greater flexibility.

Create a user interface for easier interaction with the application.
